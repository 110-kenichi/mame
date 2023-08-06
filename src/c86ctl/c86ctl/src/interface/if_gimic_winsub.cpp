/***
	c86ctl
	gimic コントロール WinUSB版
	
	Copyright (c) 2009-2013, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com

	note: honet.kk
	WinUSBドライバによるバルクダンプ転送方式。

	HID版では1フレーム(1ms)に1回の転送しか行えなかったが、
	WinUSB版では十数回（理論最大値）まで
	1ms内に転送を行えるため、転送速度が速くなっている。

	ただし、USB転送速度よりもチップ書き込み速度のほうが遅いので
	その制限によってある程度の速度で頭打ちとなる。

	また、電文及び時間分解能等の仕組みはHID版と同じで有るため、
	USB伝送速度以外の違いはHID版とWinUSB版の間には無い。
	
	-- HID版のコード流用するためにこのような仕様にしてしまったが、
	-- 今となってはsetPLL等のコントロール系はEP0(ControlTransfer)を
	-- 使うべきであったと若干後悔している・・・。

	使用するにはgimicのファーム変更（デスクリプタ変更）と
	winusbドライバのインストールが必要。
*/


#include "stdafx.h"
#include "if_gimic_winusb.h"
#include "../c700/SpcControlDevice.h"

#ifdef SUPPORT_WINUSB

#include <setupapi.h>
#include <initguid.h>
#include <algorithm>
#include "chip/chip.h"
#include "chip/opm.h"
#include "chip/opna.h"
#include "chip/opn3l.h"
#include "chip/opl3.h"
#include "chip/spc700.h"

using namespace c86ctl;

/*----------------------------------------------------------------------------
	追加ライブラリ
----------------------------------------------------------------------------*/
#pragma comment(lib, "setupapi.lib")
#pragma comment(lib, "winusb.lib")


/*----------------------------------------------------------------------------
	パラメタ
----------------------------------------------------------------------------*/
#define PIPE_BUFFER_SIZE 64


/*----------------------------------------------------------------------------
	コンストラクタ
----------------------------------------------------------------------------*/
GimicWinUSB::GimicWinUSB()
	: hDev(0), hWinUsb(0), chip(0), chiptype(CHIP_UNKNOWN), cps(0), cal(0), calcount(0), delay(0),
	  inPipeId(0), outPipeId(0), inPipeMaxPktSize(0), outPipeMaxPktSize(0),
	mWriteBufferPtr(0), mReadBufferReadPtr(0), mReadBufferWritePtr(0),
	spcControlDevice(NULL)
{
	rbuff.alloc( 128 );
	dqueue.alloc(1024*16);
	::InitializeCriticalSection(&csection);

	::QueryPerformanceFrequency(&freq);
	freq.QuadPart/=1000; // 1ms

}

/*----------------------------------------------------------------------------
	デストラクタ
----------------------------------------------------------------------------*/
GimicWinUSB::~GimicWinUSB(void)
{
	::DeleteCriticalSection(&csection);
	CloseHandle(hDev);
	WinUsb_Free(hWinUsb);
	hDev = NULL;
	hWinUsb = NULL;
	if( chip )
		delete chip;
}

/*----------------------------------------------------------------------------
	Device Open
----------------------------------------------------------------------------*/
bool GimicWinUSB::OpenDevice(std::basic_string<TCHAR> devpath)
{
	HANDLE hNewDev = CreateFile(
		devpath.c_str(),
		GENERIC_READ|GENERIC_WRITE,
		0 /*FILE_SHARE_READ|FILE_SHARE_WRITE*/,
		NULL,
		OPEN_EXISTING,
		FILE_FLAG_OVERLAPPED, //FILE_FLAG_NO_BUFFERING,
		NULL);

	if(hNewDev == INVALID_HANDLE_VALUE)
		return false;

	HANDLE hNewWinUsb = NULL;
	if (!WinUsb_Initialize(hNewDev, &hNewWinUsb)){
		//DWORD err = GetLastError();
		CloseHandle(hNewDev);
		return false;
	}
	
	// エンドポイント情報取得
	USB_INTERFACE_DESCRIPTOR desc;
	if (!WinUsb_QueryInterfaceSettings(hNewWinUsb, 0, &desc)){
		WinUsb_Free(hNewWinUsb);
		CloseHandle(hNewDev);
		return false;
	}

	for ( int i=0; i<desc.bNumEndpoints; i++ ){
		WINUSB_PIPE_INFORMATION pipeInfo;
		if (WinUsb_QueryPipe(hNewWinUsb, 0, (UCHAR)i, &pipeInfo)){
			if( pipeInfo.PipeType == UsbdPipeTypeBulk &&
				USB_ENDPOINT_DIRECTION_OUT(pipeInfo.PipeId) ){
				outPipeId = pipeInfo.PipeId;
				outPipeMaxPktSize = pipeInfo.MaximumPacketSize;
			}else if ( pipeInfo.PipeType == UsbdPipeTypeBulk &&
				USB_ENDPOINT_DIRECTION_IN(pipeInfo.PipeId) ){
				inPipeId = pipeInfo.PipeId;
				inPipeMaxPktSize = pipeInfo.MaximumPacketSize;
			}
		}
	}

	// タイムアウト設定
	ULONG timeout = 500; //ms
	::WinUsb_SetPipePolicy( hNewWinUsb, outPipeId, PIPE_TRANSFER_TIMEOUT, sizeof(ULONG), &timeout );
	::WinUsb_SetPipePolicy( hNewWinUsb, inPipeId, PIPE_TRANSFER_TIMEOUT, sizeof(ULONG), &timeout );

	// ここでハンドル更新
	hDev = hNewDev;
	hWinUsb = hNewWinUsb;
	devPath = devpath;

	WinUsb_FlushPipe(hWinUsb, outPipeId);
	WinUsb_FlushPipe(hWinUsb, inPipeId);

	// Module情報取得 -----------
	// NOTE: (*chip) はキャッシュされているところが有るので、
	//       一度作ったら削除してはいけない。
	Devinfo info;
	getModuleInfo(&info);

	if( !memcmp( info.Devname, "GMC-OPN3L", 9 ) ){
		if( chiptype == 0 && chip == 0 ){
			chiptype = CHIP_OPN3L;
			chip = new COPN3L(this);
		}else if( chiptype != CHIP_OPN3L ){
			goto MODULE_CHANGED;
		}
	}else if( !memcmp( info.Devname, "GMC-OPM", 7 ) ){
		if( chiptype == 0 && chip == 0 ){
			chiptype = CHIP_OPM;
			chip = new COPM(this);
		}else if( chiptype != CHIP_OPM ){
			goto MODULE_CHANGED;
		}
	}else if( !memcmp( info.Devname, "GMC-OPNA", 8 ) ){
		if( chiptype == 0 && chip == 0 ){
			chiptype = CHIP_OPNA;
			chip = new COPNA(this);
		}else if( chiptype != CHIP_OPNA ){
			goto MODULE_CHANGED;
		}
	}else if( !memcmp( info.Devname, "GMC-OPL3", 8 ) ){
		if( chiptype == 0 && chip == 0 ){
			chiptype = CHIP_OPL3;
			chip = new COPL3();
		}else if( chiptype != CHIP_OPL3 ){
			goto MODULE_CHANGED;
		}
	}else if( !memcmp( info.Devname, "GMC-OPLMN", 9 ) ){
		if( chiptype == 0 && chip == 0 ){
			chiptype = CHIP_OPN3L;
			chip = new COPN3L(this);
		}else if( chiptype != CHIP_OPN3L ){
			goto MODULE_CHANGED;
		}
	}else if( !memcmp( info.Devname, "GMC-SPC", 7 ) ){
		if (chiptype == 0 && chip == 0) {
			chiptype = CHIP_SPC;
			chip = new CSPC700();
			spcControlDevice = new SpcControlDevice(this);
			return true;
		}
		else if (chiptype != CHIP_SPC) {
			goto MODULE_CHANGED;
		}
	}
	
	// 値をキャッシュさせるためのダミー呼び出し
	UCHAR vol;
	getSSGVolume(&vol);
	UINT clock;
	getPLLClock(&clock);
	
	return true;


MODULE_CHANGED:
	CloseHandle(hDev);
	WinUsb_Free(hWinUsb);
	hDev = NULL;
	hWinUsb = NULL;
	delete spcControlDevice;
	spcControlDevice = NULL;
	return false;
}

/*----------------------------------------------------------------------------
	factory
----------------------------------------------------------------------------*/
int GimicWinUSB::UpdateInstances( withlock< std::vector< std::shared_ptr<GimicIF> > > &gimics)
{
	gimics.lock();
	std::for_each( gimics.begin(), gimics.end(), []( std::shared_ptr<GimicIF> x ){ x->checkConnection(); } );

	BOOL bResult = TRUE;
	
	HDEVINFO devinf = INVALID_HANDLE_VALUE;
	SP_DEVICE_INTERFACE_DATA spid;
	PSP_DEVICE_INTERFACE_DETAIL_DATA fc_data = NULL;
	

	devinf = SetupDiGetClassDevs(
		(LPGUID)&GUID_DEVINTERFACE_WINUSBTESTTARGET,
		NULL,
		0,
		DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

	if( devinf ){
		for ( int i=0; ;i++ ){
			ZeroMemory(&spid, sizeof(spid));
			spid.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
			if ( !SetupDiEnumDeviceInterfaces(devinf, NULL,
				  (LPGUID) &GUID_DEVINTERFACE_WINUSBTESTTARGET, i, &spid) ){
				break;
			}

			unsigned long sz;
			std::basic_string<TCHAR> devpath;

			// 必要なバッファサイズ取得
			bResult = SetupDiGetDeviceInterfaceDetail(devinf, &spid, NULL, 0, &sz, NULL);
			PSP_INTERFACE_DEVICE_DETAIL_DATA dev_det = (PSP_INTERFACE_DEVICE_DETAIL_DATA)(malloc(sz));
			if(dev_det == NULL)
				break;
			dev_det->cbSize = sizeof(SP_INTERFACE_DEVICE_DETAIL_DATA);

			// デバイスノード取得
			if (!SetupDiGetDeviceInterfaceDetail(devinf, &spid, dev_det, sz, &sz, NULL)){
				free(dev_det);
				break;
			}

			devpath = dev_det->DevicePath;
			free(dev_det);
			dev_det = NULL;

			// 既にインスタンスがあるかどうか検索
			auto it = std::find_if( gimics.begin(), gimics.end(),
				[devpath](std::shared_ptr<GimicIF> x) -> bool {
					GimicWinUSB *gdev = dynamic_cast<GimicWinUSB*>(x.get());
					if(!gdev) return false;
					if(gdev->devPath != devpath ) return false;
					return true;
				}
			);
			
			if( it == gimics.end() ){
				GimicWinUSB *gimicDev = new GimicWinUSB();
				if( gimicDev ){
					if( gimicDev->OpenDevice(devpath) ){
						gimics.push_back( GimicIFPtr(gimicDev) );
					}else{
						delete gimicDev;
					}
				}
			}
			else if (!(*it)->isValid()){
				GimicWinUSB *gimicDev = dynamic_cast<GimicWinUSB*>(it->get());
				if( !gimicDev->OpenDevice(devpath) ){
					// OpenDeviceが失敗した場合は音源モジュールが前回接続時と
					// 異なっているため、別インスタンスを生成する
					GimicWinUSB *gimicDev = new GimicWinUSB();
					if( gimicDev ){
						if( gimicDev->OpenDevice(devpath) ){
							gimics.push_back( GimicIFPtr(gimicDev) );
						}else{
							delete gimicDev;
						}
					}
				}
			}
		}
		
		SetupDiDestroyDeviceInfoList(devinf);
	}

	gimics.unlock();
	return 0;
}


/*----------------------------------------------------------------------------
	internal.
----------------------------------------------------------------------------*/
int GimicWinUSB::sendMsg( MSG *data )
{
	UCHAR buff[66];
	int ret = C86CTL_ERR_UNKNOWN;

	UINT sz = data->len;
	if( 0<sz ){
		memcpy( &buff[0], &data->dat[0], sz );
		if( sz<64 )
			memset( &buff[sz], 0xff, 64-sz );

		::EnterCriticalSection(&csection);
		ret = devWrite(buff, sz + 1);
		::LeaveCriticalSection(&csection);
	}

	return ret;
}

int GimicWinUSB::sendMsg2(UCHAR* data, DWORD sz)
{
	::EnterCriticalSection(&csection);
	int ret = devWrite(data, sz);
	::LeaveCriticalSection(&csection);

	return ret;
}

int GimicWinUSB::transaction( MSG *txdata, uint8_t *rxdata, uint32_t rxsz )
{
	UCHAR buff[66];
	DWORD len = 0;
	int ret = C86CTL_ERR_UNKNOWN;

	::EnterCriticalSection(&csection);
	{
		UINT sz = txdata->len;
		if( 0<sz ){
			memcpy( &buff[0], &txdata->dat[0], sz );
			if( sz<64 )
				memset( &buff[sz], 0xff, 64-sz );

			ret = devWrite(buff, 65);
		}

		if( C86CTL_ERR_NONE==ret ){
			len = 0;
			ret = devRead(buff);
			if( C86CTL_ERR_NONE == ret )
				memcpy( rxdata, &buff[0], rxsz );
		}
	}
	::LeaveCriticalSection(&csection);

	return ret;
}



int GimicWinUSB::devRead( LPVOID data )
{
	if( !hDev || !hWinUsb )
		return C86CTL_ERR_NODEVICE;
	
	DWORD rlen;
	BOOL ret = WinUsb_ReadPipe( hWinUsb, inPipeId, (UCHAR*)data, 64, &rlen, 0);
	
	if(ret == FALSE){
		WinUsb_Free(hWinUsb);
		hWinUsb = NULL;
		CloseHandle(hDev);
		hDev = NULL;
		return C86CTL_ERR_UNKNOWN;
	}
	return C86CTL_ERR_NONE;
}

int GimicWinUSB::devWrite( LPCVOID data, DWORD sz)
{
	if( !hDev || !hWinUsb )
		return C86CTL_ERR_NODEVICE;
	
	DWORD wlen;
	BOOL ret = WinUsb_WritePipe( hWinUsb, outPipeId, (UCHAR*)data, sz, &wlen, 0);
	
	if(ret == FALSE || sz != wlen){
		WinUsb_Free(hWinUsb);
		hWinUsb = NULL;
		CloseHandle(hDev);
		hDev = NULL;
		return C86CTL_ERR_UNKNOWN;
	}
	return C86CTL_ERR_NONE;
}

/*----------------------------------------------------------------------------
	実装
---------------------------------------------------------------------------*/
int GimicWinUSB::reset(void)
{
	int ret;
	
	// ディレイキューの廃棄
	dqueue.flush();

	if (chiptype == CHIP_SPC)
	{
		ret = spcControlDevice->Init();
	}
	else
	{
		// リセットコマンド送信
		MSG d = { 2, { 0xfd, 0x82, 0 } };
		ret = sendMsg(&d);
	}
	if( C86CTL_ERR_NONE == ret ){
		// 各ステータス値リセット
		//   マスクの適用をreset内でする（送信処理が発生する）ので
		//   リセット後に処理しないとダメ。
		if( chip )
			chip->reset();
	}

	return ret;
}

int GimicWinUSB::isValid(void)
{
	return (hDev == 0) ? FALSE : TRUE;
}

int GimicWinUSB::setSSGVolume(UCHAR vol)
{
	if( chiptype != CHIP_OPNA )
		return C86CTL_ERR_UNSUPPORTED;

	gimicParam.ssgVol = vol;
	MSG d = { 3, { 0xfd, 0x84, vol } };
	return sendMsg( &d );
}

int GimicWinUSB::getSSGVolume(UCHAR *vol)
{
	if( chiptype != CHIP_OPNA )
		return C86CTL_ERR_UNSUPPORTED;
	if( !vol )
		return C86CTL_ERR_INVALID_PARAM;

	MSG d = { 2, { 0xfd, 0x86 } };
	int ret = transaction( &d, (uint8_t*)vol, 1 );
	
	if( C86CTL_ERR_NONE == ret )
		gimicParam.ssgVol = *vol;
	
	return ret;
}

int GimicWinUSB::setPLLClock(UINT clock)
{
	if( chiptype != CHIP_OPNA && chiptype != CHIP_OPM && chiptype != CHIP_OPL3  )
		return C86CTL_ERR_UNSUPPORTED;

	gimicParam.clock = clock;
	MSG d = { 6, { 0xfd, 0x83, clock&0xff, (clock>>8)&0xff, (clock>>16)&0xff, (clock>>24)&0xff, 0 } };
	int ret = sendMsg( &d );

	if( ret == C86CTL_ERR_NONE ){
		if( chip )
			chip->setMasterClock(clock);
	}
	return ret;
}

int GimicWinUSB::getPLLClock(UINT *clock)
{
	if( chiptype != CHIP_OPNA && chiptype != CHIP_OPM && chiptype != CHIP_OPL3 )
		return C86CTL_ERR_UNSUPPORTED;

	if( !clock )
		return C86CTL_ERR_INVALID_PARAM;

	MSG d = { 2, { 0xfd, 0x85 } };
	int ret = transaction( &d, (uint8_t*)clock, 4 );

	if( ret == C86CTL_ERR_NONE ){
		if( gimicParam.clock != *clock ){
			gimicParam.clock = *clock;
			if( chip )
				chip->setMasterClock(*clock);
		}
	}
	return ret;
}

int GimicWinUSB::getMBInfo( struct Devinfo *info )
{
	int ret;
	
	if( !info )
		return C86CTL_ERR_INVALID_PARAM;

	MSG d = { 3, { 0xfd, 0x91, 0xff } };
	if( C86CTL_ERR_NONE == (ret = transaction( &d, (uint8_t*)info, 32 )) ){
		char *p = &info->Devname[15];
		while( *p==0 || *p==-1 ) *p--=0;
		p = &info->Serial[14];
		while(*p==0||*p==-1) *p--=0;
	}
	return ret;
}

int GimicWinUSB::getModuleInfo( struct Devinfo *info )
{
	int ret;
	
	if( !info )
		return C86CTL_ERR_INVALID_PARAM;

	MSG d = { 3, { 0xfd, 0x91, 0 } };
	if( C86CTL_ERR_NONE == (ret = transaction( &d, (uint8_t*)info, 32 )) ){
		char *p = &info->Devname[15];
		while(*p==0||*p==-1) *p--=0;
		p = &info->Serial[14];
		while(*p==0||*p==-1) *p--=0;
	}
	return ret;
}

int GimicWinUSB::getModuleType(enum ChipType *type)
{
	if( !type )
		return C86CTL_ERR_INVALID_PARAM;

	*type = chiptype;
	return C86CTL_ERR_NONE;
}

int GimicWinUSB::getFWVer( UINT *major, UINT *minor, UINT *rev, UINT *build )
{
	uint8_t rx[16];
	MSG d = { 2, { 0xfd, 0x92 } };
	int ret;

	if( C86CTL_ERR_NONE == (ret = transaction( &d, rx, 16 )) ){
		if( major ) *major = *((uint32_t*)&rx[0]);
		if( minor ) *minor = *((uint32_t*)&rx[4]);
		if( rev )   *rev   = *((uint32_t*)&rx[8]);
		if( build ) *build = *((uint32_t*)&rx[12]);
	}
	return ret;
}

int GimicWinUSB::getChipStatus( UINT addr, UCHAR *status )
{
	if( !status )
		return C86CTL_ERR_INVALID_PARAM;
	
	uint8_t rx[4];
	MSG d = { 3, { 0xfd, 0x93, addr&0x01 } };
	int ret;

	if( C86CTL_ERR_NONE == (ret = transaction( &d, rx, 4 )) ){
		*status = *((uint32_t*)&rx[0]);
	}
	return ret;
}

/*
int GimicWinUSB::adpcmZeroClear(void)
{
	uint8_t rx[1];
	MSG d = { 2, { 0xfd, 0xa0 } };
	int ret;

	ret = transaction( &d, rx, 1 );
	return ret;
}

int GimicWinUSB::adpcmWrite( UINT startAddr, UINT size, UCHAR *data )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}

int GimicWinUSB::adpcmRead( UINT startAddr, UINT size, UCHAR *data )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}
*/

void GimicWinUSB::directOut(UINT addr, UCHAR data)
{
	switch( chiptype ){
	case CHIP_OPNA:
	case CHIP_OPN3L:
		if( 0x100<=addr && addr<=0x110 )
			addr -= 0x40;
		break;
	case CHIP_OPM:
		if( 0xfc<=addr && addr<=0xff )
			addr -= 0xe0;
		break;
	case CHIP_SPC:
		spcControlDevice->doWriteDspHw(addr, data);
		return;
	}
	if( addr < 0xfc ){
		MSG d = { 2, { addr&0xff, data } };
		sendMsg(&d);
	}else if( 0x100 <= addr && addr <= 0x1fb ){
		MSG d = { 3, { 0xfe, addr&0xff, data } };
		sendMsg(&d);
	}
}

void GimicWinUSB::directOut2(DWORD* addr, UCHAR* data, DWORD sz, UCHAR type)
{
	for (DWORD i = 0; i < sz; i++)
	{
		UINT a = addr[i];
		UCHAR dt = data[i];

		bool flag = true;
		if (chip) {
			flag = chip->setReg(a, dt);
			chip->filter(a, &dt);
		}

		switch (chiptype) {
		case CHIP_OPNA:
		case CHIP_OPN3L:
			if (0x100 <= a && a <= 0x110)
				a -= 0x40;
			break;
		case CHIP_OPM:
			if (0xfc <= a && a <= 0xff)
				a -= 0xe0;
			break;
		case CHIP_SPC:
			if(type == 0)
				spcControlDevice->doWriteDspHw(a, dt);
			else if (type == 1)
				spcControlDevice->doWriteRamHw(a, dt);
			cal += 8;
			continue;
		}
		if (a < 0xfc) {
			MSG d = { 3, { a & 0xff, dt, 0xff } };
			sendMsg2(&d.dat[0], d.len);
			cal += d.len;
		}
		else if (0x100 <= a && a <= 0x1fb) {
			MSG d = { 4, { 0xfe, a & 0xff, dt, 0xff } };
			sendMsg2(&d.dat[0], d.len);
			cal += d.len;
		}
	}
}

void GimicWinUSB::out2buf(UINT addr, UCHAR data)
{
	bool flag = true;
	if( chip ){
		flag = chip->setReg( addr, data );
		chip->filter( addr, &data );
	}
	if( flag ){
		switch( chiptype ){
		case CHIP_OPNA:
		case CHIP_OPN3L:
			if( 0x100<=addr && addr<=0x110 )
				addr -= 0x40;
			break;
		case CHIP_OPM:
			if( 0xfc<=addr && addr<=0xff )
				addr -= 0xe0;
			break;
		case CHIP_SPC:
			MSG d = { 3, { 0x11, data, addr & 0xff } };
			rbuff.push(d);
			return;
		}
		if( addr < 0xfc ){
			MSG d = { 2, { addr&0xff, data } };
			rbuff.push(d);
		}else if( 0x100 <= addr && addr <= 0x1fb ){
			MSG d = { 3, { 0xfe, addr&0xff, data } };
			rbuff.push(d);
		}
	}
}
void GimicWinUSB::out(UINT addr, UCHAR data)
{
	if( 0<delay ){
		REQ r = { ::timeGetTime()+delay, addr, data };
		dqueue.push(r);
		return;
	}

	out2buf(addr, data);
}

UCHAR GimicWinUSB::in(UINT addr)
{
	if( chip )
		return chip->getReg(addr);

	return 0;
}

void GimicWinUSB::tick(void)
{
	int ret;
	
	if( !dqueue.isempty() ){
		UINT t = timeGetTime();
		while( !dqueue.isempty() && t>=dqueue.front()->t ){
			if( rbuff.remain()<4 ) break;
			REQ req;
			dqueue.pop(&req);
			out2buf( req.addr, req.dat );
		}
	}

	LARGE_INTEGER et, ct;
	::QueryPerformanceCounter(&et);
	et.QuadPart += freq.QuadPart;
	while( !rbuff.isempty() ){
		UCHAR buff[128];
		UINT sz=0, i=0;
		MSG d;

		for(;;){
			UINT l = rbuff.front()->len;
			if( 64<(sz+l) )
				break;
			if( !rbuff.pop(&d) )
				break;
			sz += d.len;
			for( UINT j=0; j<d.len; j++ )
				buff[i++] = d.dat[j];
			if( rbuff.isempty() )
				break;
		}

		if( sz<64 )
			memset( &buff[sz], 0xff, 64-sz );

		// WriteFileがスレッドセーフかどうかよく分からないので
		// 念のため保護しているが、たぶんいらない。
		// (directOut()と重なる可能性がある)
		::EnterCriticalSection(&csection);
		ret = devWrite(buff, 64);
		::LeaveCriticalSection(&csection);

		if( ret == C86CTL_ERR_NONE )
			cal+=64;

		// 1tickの処理が1msを超えたら一回抜ける
		::QueryPerformanceCounter(&ct);
		if( et.QuadPart<ct.QuadPart ){
			break;
		}
	}

	return;
}

void GimicWinUSB::update(void)
{
	if( chip )
		chip->update();

	if( 1 <= calcount++ ){
		cps = cal;
		cal = 0;
		calcount = 0;
	}
};
void GimicWinUSB::checkConnection(void)
{
	UCHAR buff[65];
	buff[0] = 0;
	memset( buff, 0, 65 );

	::EnterCriticalSection(&csection);
	devWrite(buff, 64);
	::LeaveCriticalSection(&csection);
};

int GimicWinUSB::setDelay(int d)
{
	if(d!=delay){
		delay = d;
	}
	return C86CTL_ERR_NONE;
}

int GimicWinUSB::getDelay(int *d)
{
	if(d){
		*d = delay;
		return C86CTL_ERR_NONE;
	}
	return C86CTL_ERR_INVALID_PARAM;
}

// C700 -----------------------------------------------------

bool GimicWinUSB::resetrPipe()
{
	WinUsb_FlushPipe(hWinUsb, inPipeId);
	return true;
}
bool GimicWinUSB::resetwPipe()
{
	WinUsb_FlushPipe(hWinUsb, outPipeId);
	return true;
}

int	 GimicWinUSB::bulkWrite(UINT8* buf, UINT32 size)
{
	if (!isValid()) return -1;

	ULONG len = size;
	UINT32	bufferRest = WRITE_BUFFER_SIZE - mWriteBufferPtr;
	if (bufferRest < len) {
		mWriteBufferPtr = 0;
	}
	memcpy(&mWriteBuffer[mWriteBufferPtr], buf, len);

	DWORD wlen;
	::EnterCriticalSection(&csection);
	BOOL ret = WinUsb_WritePipe(hWinUsb, outPipeId, &mWriteBuffer[mWriteBufferPtr], len, &wlen, NULL);
	::LeaveCriticalSection(&csection);

	if (ret == FALSE || size != wlen) {
		//printf("Write error in bulkWrite.\n");
		//DWORD err = GetLastError();
		//printErr(err);
		return -1;
	}
	mWriteBufferPtr += len;
	return wlen;
}
int	 GimicWinUSB::bulkWriteAsync(UINT8* buf, UINT32 size)
{
	return bulkWrite(buf, size);
}
int	 GimicWinUSB::bulkRead(UINT8* buf, UINT32 size, UINT32 timeout)
{
	if (!isValid()) return -1;

	DWORD rlen;

	::EnterCriticalSection(&csection);
	BOOL ret = WinUsb_ReadPipe(hWinUsb, inPipeId, buf, size, &rlen, NULL);
	::LeaveCriticalSection(&csection);

	if (ret == FALSE) {
		//printf("Write error in bulkRead.\n");
		//DWORD err = GetLastError();
		//printErr(err);
		return -1;
	}
	return rlen;
}
int	 GimicWinUSB::read(UINT8* buf, UINT32 size)
{
	return 0;
}
int	 GimicWinUSB::getReadableBytes()
{
	return 0;
}

#endif

