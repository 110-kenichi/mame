/***
	c86ctl
	gimic コントロール HID版
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
	Thanks to Nagai "Guu" Osamu 2011/12/08 for his advice.

	HID-APIによる転送方式。
	HID Reportをベンダ定義の64Byte固定パケットにして、
	無理矢理データを詰め込む事で転送を行う。

	HIDの仕組みを利用しているため、デバイスドライバが不要で
	プラットフォーム依存が少ない利点が有る。
	反面、HIDの規格上1パケット/1msという制約が有るため、
	あまり転送速度を出すことが出来ない。


	HIDメッセージ体系 ------------------------------------------
		注意：HIDパケット先頭＝メッセージの開始バイトで有ること
		　　　（HIDパケット２つにまたがるメッセージは不可）
		
	 XX YY             : 2byte レジスタアクセスコマンド, XX=アドレス(00~fb), YY=データ(0~ff)
	 FE XX YY          : 3byte Exレジスタアクセスコマンド, XX=アドレス(00~fb), YY=データ(0~ff)
	 FE FC             : 予約(利用禁止)
	 FE FD             : 予約(利用禁止)
	 FE FE             : 予約(利用禁止)
	 FE FF             : 予約(利用禁止)

	 FD 81             : HARDWARE Reset
	 FD 82             : Software Reset
	 FD 83 WW XX YY ZZ : set PLL clock, WW=clock[7:0], XX=clock[15:8], YY=clock[23:16], ZZ=clock[31:24]
	 FD 84 XX          : set SSG volume (OPNAモジュール時のみ処理), XX=volume(0-127)
	 FD 85             : get PLL clock
	 FD 86             : get SSG volume
	 FD 91 XX          : get HW report, XX=番号(00=モジュール, FF=マザーボード)
	 FD 92             : get Firmware version
	 FD 93 XX          : get STATUS, XX=番号(00=STATUS0, 01=STATUS1)

	 FD A0             : 予約(利用禁止)
	 FD A1             : 予約(利用禁止)
	 FD A2             : 予約(利用禁止)
	 FD A3             : 予約(利用禁止)

	 FC                : 予約(利用禁止)
	 FF                : terminator

	  ポートリマップ仕様
		YM2608 & YMF288
		  実アドレス      通信時のアドレス
		  100~110     ->  C0~D0
		
		YM2151
		  実アドレス      通信時のアドレス
		  FC~FF       ->  1C~1F
		  
 */
#include "stdafx.h"
#include "if_gimic_hid.h"

#ifdef SUPPORT_HID

#define GIMIC_USBVID 0x16c0
#define GIMIC_USBPID 0x05e4
#define C86USB_USBVID 0x0525 // TEST
#define C86USB_USBPID 0xa4ac // TEST

#include <setupapi.h>
#include <algorithm>
#include "chip/chip.h"
#include "chip/opm.h"
#include "chip/opna.h"
#include "chip/opn3l.h"
#include "chip/opl3.h"

extern "C" {
#include "hidsdi.h"
}

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;

/*----------------------------------------------------------------------------
	追加ライブラリ
----------------------------------------------------------------------------*/
#pragma comment(lib, "hid.lib")
#pragma comment(lib, "setupapi.lib")
#pragma comment(lib, "winmm.lib")

/*----------------------------------------------------------------------------
	コンストラクタ
----------------------------------------------------------------------------*/
GimicHID::GimicHID( HANDLE h )
	: hHandle(h), chip(0), chiptype(CHIP_UNKNOWN), cps(0), cal(0), calcount(0), delay(0)
{
	rbuff.alloc( 128 );
	dqueue.alloc(1024*16);
	::InitializeCriticalSection(&csection);
}

/*----------------------------------------------------------------------------
	デストラクタ
----------------------------------------------------------------------------*/
GimicHID::~GimicHID(void)
{
	::DeleteCriticalSection(&csection);
	CloseHandle(hHandle);
	hHandle = NULL;
	if( chip )
		delete chip;
}

/*----------------------------------------------------------------------------
	HIDIF factory
----------------------------------------------------------------------------*/
int GimicHID::UpdateInstances( withlock< std::vector< std::shared_ptr<GimicIF> > > &gimics)
{
	gimics.lock();
	std::for_each( gimics.begin(), gimics.end(), []( std::shared_ptr<GimicIF> x ){ x->checkConnection(); } );

	GUID hidGuid;
	HDEVINFO devinf;
	SP_DEVICE_INTERFACE_DATA spid;
	SP_DEVICE_INTERFACE_DETAIL_DATA* fc_data = NULL;

	HidD_GetHidGuid(&hidGuid);
	devinf = SetupDiGetClassDevs(
		&hidGuid,
		NULL,
		0,
		DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

	if( devinf ){
		for(int i=0; ;i++) {
			ZeroMemory(&spid, sizeof(spid));
			spid.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
			if(!SetupDiEnumDeviceInterfaces(devinf, NULL, &hidGuid, i, &spid)){
				//DWORD err = GetLastError();
				break;
			}

			unsigned long sz;
			std::basic_string<TCHAR> devpath;

			// 必要なバッファサイズ取得
			SetupDiGetDeviceInterfaceDetail(devinf, &spid, NULL, 0, &sz, 0);
			PSP_INTERFACE_DEVICE_DETAIL_DATA dev_det = (PSP_INTERFACE_DEVICE_DETAIL_DATA)(malloc(sz));
			dev_det->cbSize = sizeof(SP_INTERFACE_DEVICE_DETAIL_DATA);

			// デバイスノード取得
			if(!SetupDiGetDeviceInterfaceDetail(devinf, &spid, dev_det, sz, &sz, 0)){
				free(dev_det);
				break;
			}

			// デバイスオープン
			HANDLE hHID = CreateFile(
				dev_det->DevicePath,
				GENERIC_READ|GENERIC_WRITE,
				0/*FILE_SHARE_READ|FILE_SHARE_WRITE*/,
				NULL,
				OPEN_EXISTING,
				0,//FILE_FLAG_NO_BUFFERING,
				NULL);

			if(hHID == INVALID_HANDLE_VALUE){
				free(dev_det);
				continue;
			}
			devpath = dev_det->DevicePath;
			free(dev_det);
			dev_det = NULL;

			// VID, PID, version 取得
			HIDD_ATTRIBUTES attr;
			if( !HidD_GetAttributes(hHID, &attr) )
				continue;

			if((attr.VendorID == GIMIC_USBVID && attr.ProductID == GIMIC_USBPID)||
				(attr.VendorID == C86USB_USBVID && attr.ProductID == C86USB_USBPID)){
				
				auto it = std::find_if( gimics.begin(), gimics.end(),
					[devpath](std::shared_ptr<GimicIF> x) -> bool {
						GimicHID *ghid = dynamic_cast<GimicHID*>(x.get());
						if(!ghid) return false;
						if(ghid->devPath != devpath ) return false;
						if(!ghid->isValid()) return false;
						return true;
					}
				);
				if( it == gimics.end() ){
					// タイムアウト設定
					COMMTIMEOUTS commTimeOuts;
					commTimeOuts.ReadIntervalTimeout = 0;
					commTimeOuts.ReadTotalTimeoutConstant = 500; //ms
					commTimeOuts.ReadTotalTimeoutMultiplier = 0;
					commTimeOuts.WriteTotalTimeoutConstant = 500; //ms
					commTimeOuts.WriteTotalTimeoutMultiplier = 0;
					::SetCommTimeouts( hHID, &commTimeOuts );

					// インスタンス生成
					GimicHID *gimicHid = new GimicHID(hHID);
					if( gimicHid ){
						gimicHid->devPath = devpath;
						gimics.push_back( GimicIFPtr(gimicHid) );
						gimicHid->init();
					}
				}
			}else{
				CloseHandle(hHID);
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
int GimicHID::sendMsg( MSG *data )
{
	UCHAR buff[66];
	int ret = C86CTL_ERR_UNKNOWN;

	buff[0] = 0; // HID interface id.

	UINT sz = data->len;
	if( 0<sz ){
		memcpy( &buff[1], &data->dat[0], sz );
		if( sz<64 )
			memset( &buff[1+sz], 0xff, 64-sz );

		::EnterCriticalSection(&csection);
		int ret = devWrite(buff, 65);
		::LeaveCriticalSection(&csection);
	}

	return ret;
}

int GimicHID::sendMsg2(UCHAR* data, DWORD sz)
{
	::EnterCriticalSection(&csection);
	int ret = devWrite(data, sz);
	::LeaveCriticalSection(&csection);

	return ret;
}

int GimicHID::transaction( MSG *txdata, uint8_t *rxdata, uint32_t rxsz )
{
	UCHAR buff[66];
	buff[0] = 0; // HID interface id.
	DWORD len = 0;
	int ret = C86CTL_ERR_UNKNOWN;

	::EnterCriticalSection(&csection);
	{
		UINT sz = txdata->len;
		if( 0<sz ){
			memcpy( &buff[1], &txdata->dat[0], sz );
			if( sz<64 )
				memset( &buff[1+sz], 0xff, 64-sz );

			ret = devWrite(buff, 65);
		}

		if( C86CTL_ERR_NONE==ret ){
			len = 0;
			ret = devRead(buff);
			if( C86CTL_ERR_NONE == ret )
				memcpy( rxdata, &buff[1], rxsz ); // 1byte目はUSBのInterfaceNo.なので飛ばす
		}
	}
	::LeaveCriticalSection(&csection);

	return ret;
}

/*----------------------------------------------------------------------------
	実装
----------------------------------------------------------------------------*/

int GimicHID::init(void)
{
	Devinfo info;
	getModuleInfo(&info);
	if( !memcmp( info.Devname, "GMC-OPN3L", 9 ) ){
		chiptype = CHIP_OPN3L;
		chip = new COPN3L(this);
	}else if( !memcmp( info.Devname, "GMC-OPM", 7 ) ){
		chiptype = CHIP_OPM;
		chip = new COPM(this);
	}else if( !memcmp( info.Devname, "GMC-OPNA", 8 ) ){
		chiptype = CHIP_OPNA;
		chip = new COPNA(this);
	}else if( !memcmp( info.Devname, "GMC-OPL3", 8 ) ){
		chiptype = CHIP_OPL3;
		chip = new COPL3();
	}else if( !memcmp( info.Devname, "GMC-OPLMN", 9 ) ){
		chiptype = CHIP_OPN3L;
		chip = new COPN3L(this);
	//	}else if( !memcmp( info.Devname, "GMC-SPC", 8 ) ){
	}
	
	// 値をキャッシュさせるためのダミー呼び出し
	UCHAR vol;
	getSSGVolume(&vol);
	UINT clock;
	getPLLClock(&clock);
	
	return C86CTL_ERR_NONE;
}

int GimicHID::reset(void)
{
	int ret;
	
	// ディレイキューの廃棄
	dqueue.flush();

	// リセットコマンド送信
	MSG d = { 2, { 0xfd, 0x82, 0 } };
	ret =  sendMsg( &d );
	
	if( C86CTL_ERR_NONE == ret ){
		// 各ステータス値リセット
		//   マスクの適用をreset内でする（送信処理が発生する）ので
		//   リセット後に処理しないとダメ。
		if( chip )
			chip->reset();
	}

	return ret;
}

int GimicHID::isValid(void)
{
	return (hHandle == 0) ? FALSE : TRUE;
}

int GimicHID::setSSGVolume(UCHAR vol)
{
	if( chiptype != CHIP_OPNA )
		return C86CTL_ERR_UNSUPPORTED;

	gimicParam.ssgVol = vol;
	MSG d = { 3, { 0xfd, 0x84, vol } };
	return sendMsg( &d );
}

int GimicHID::getSSGVolume(UCHAR *vol)
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

int GimicHID::setPLLClock(UINT clock)
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

int GimicHID::getPLLClock(UINT *clock)
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

int GimicHID::getMBInfo( struct Devinfo *info )
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

int GimicHID::getModuleInfo( struct Devinfo *info )
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

int GimicHID::getModuleType(enum ChipType *type)
{
	if( !type )
		return C86CTL_ERR_INVALID_PARAM;

	*type = chiptype;
	return C86CTL_ERR_NONE;
}

int GimicHID::getFWVer( UINT *major, UINT *minor, UINT *rev, UINT *build )
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

int GimicHID::getChipStatus( UINT addr, UCHAR *status )
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
int GimicHID::adpcmZeroClear(void)
{
	uint8_t rx[1];
	MSG d = { 2, { 0xfd, 0xa0 } };
	int ret;

	ret = transaction( &d, rx, 1 );
	return ret;
}

int GimicHID::adpcmWrite( UINT startAddr, UINT size, UCHAR *data )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}

int GimicHID::adpcmRead( UINT startAddr, UINT size, UCHAR *data )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}
*/

void GimicHID::directOut(UINT addr, UCHAR data)
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
	}
	if( addr < 0xfc ){
		MSG d = { 2, { addr&0xff, data } };
		sendMsg(&d);
	}else if( 0x100 <= addr && addr <= 0x1fb ){
		MSG d = { 3, { 0xfe, addr&0xff, data } };
		sendMsg(&d);
	}
}

void GimicHID::directOut2(DWORD* addr, UCHAR* data, DWORD sz)
{
	for (int i = 0; i < sz; i++)
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
		}
		if (a < 0xfc) {
			MSG d = { 3, { a & 0xff, dt, 0xff } };
			sendMsg2(&d.dat[0], d.len);
		}
		else if (0x100 <= a && a <= 0x1fb) {
			MSG d = { 4, { 0xfe, a & 0xff, dt, 0xff } };
			sendMsg2(&d.dat[0], d.len);
		}
	}
}

void GimicHID::out2buf(UINT addr, UCHAR data)
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
void GimicHID::out(UINT addr, UCHAR data)
{
	if( 0<delay ){
		REQ r = { ::timeGetTime()+delay, addr, data };
		dqueue.push(r);
		return;
	}
	if(delay==0){
		if( dqueue.isempty() )
			delay = -1;
	}

	out2buf(addr, data);
}

UCHAR GimicHID::in(UINT addr)
{
	if( chip )
		return chip->getReg(addr);

	return 0;
}

void GimicHID::tick(void)
{
	int ret;
	
	if( 0<=delay && !dqueue.isempty() ){
		UINT t = timeGetTime();
		while( !dqueue.isempty() && t>=dqueue.front()->t ){
			if( rbuff.remain()<4 ) break;
			REQ req;
			dqueue.pop(&req);
			out2buf( req.addr, req.dat );
		}
	}
	
	if( !rbuff.isempty() ){
		UCHAR buff[128];
		UINT sz=0, i=1;
		MSG d;

		buff[0] = 0; // HID interface id.

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
			memset( &buff[1+sz], 0xff, 64-sz );

		// WriteFileがスレッドセーフかどうかよく分からないので
		// 念のため保護しているが、たぶんいらない。
		// (directOut()と重なる可能性がある)
		::EnterCriticalSection(&csection);
		ret = devWrite(buff, 65);
		::LeaveCriticalSection(&csection);

		if( ret == C86CTL_ERR_NONE )
			cal+=64;
	}

	return;
}

void GimicHID::update(void)
{
	if( chip )
		chip->update();

	if( 1 <= calcount++ ){
		cps = cal;
		cal = 0;
		calcount = 0;
	}
};
void GimicHID::checkConnection(void)
{
	UCHAR buff[65];
	buff[0] = 0;
	memset( buff, 0, 65 );

	::EnterCriticalSection(&csection);
	devWrite(buff, 65);
	::LeaveCriticalSection(&csection);
};

int GimicHID::setDelay(int d)
{
	return C86CTL_ERR_NONE;
}

int GimicHID::getDelay(int *d)
{
	if(d){
		*d = delay;
		return C86CTL_ERR_NONE;
	}
	return C86CTL_ERR_INVALID_PARAM;
}

#endif
