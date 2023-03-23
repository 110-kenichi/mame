/***
	c86ctl
	gimic コントロール MIDI版(実験コード)
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
	Thanks to Nagai "Guu" Osamu 2011/12/08 for his advice.

	note: honet/k.kotajima 2013/6/6
	HID/WinUSB版が実用十分に安定したのでMIDI版は廃止。
	ろくにメンテしていないので・・・


	転送仕様： sysexに3byteとごにパックして送る。
	 idx  data   mean
	   0  0xF0: Start of SysEx
	   1  0x7D:   device id
	3n+2  (d0):   {000, exaddr[1:0], addr[7:6]}
	3n+3  (d1):   {addr[5:0], data[7:7]}
	3n+4  (d2):   data[6:0]
	...
	3N+5  0xF7: End of SysEx

	システムメッセージ：
	SW Reset     : f0, 7d, 40, 01, 00, 7e, f7
	HW Reset     : f0, 7d, 40, 01, 00, 7f, f7
	PLL Set      : f0, 7d, 40, 01, 00, 70, v0, v1, v2, v3, f7
	GM System ON : f0, 7e, 7f, 09, 01, f7
	SSG Vol      : f0, 7f, 04, vol[6:0], vol[13:7], f7
 */

#include <stdafx.h>
#include <algorithm>
#include "if_gimic_midi.h"

#ifdef SUPPORT_MIDI

#include "config.h"
#include "chip/chip.h"
#include "chip/opm.h"
#include "chip/opna.h"
#include "chip/opn3l.h"


#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;

/*----------------------------------------------------------------------------
	コンストラクタ
----------------------------------------------------------------------------*/
GimicMIDI::GimicMIDI( HMIDIOUT h ) : hHandle(h), chip(0), chiptype(CHIP_OPNA)
{
	rbuff.alloc(128);
}

/*----------------------------------------------------------------------------
	デストラクタ
----------------------------------------------------------------------------*/
GimicMIDI::~GimicMIDI(void)
{
	if(hHandle) {
		midiOutClose(hHandle);
		hHandle = NULL;
	}
	if( chip )
		delete chip;
}


/*----------------------------------------------------------------------------
	MIDI-IF factory
----------------------------------------------------------------------------*/
int GimicMIDI::UpdateInstances( withlock< std::vector< std::shared_ptr<GimicIF> > > &gimics)
{
	gimics.lock();

	// MIDIOUTCAPSでgimicかどうか選別できないかと思ったけどダメだった。
//	UINT n = midiOutGetNumDevs();
//	for( UINT i=0; i<n; i++ ){
//		MIDIOUTCAPS midiOutCaps;
//		midiOutGetDevCaps( i, &midiOutCaps, sizeof(midiOutCaps) );
//	}

	int DeviceID = gConfig.getInt(INISC_MAIN, INIKEY_MIDIDEVICE, -1);
	HMIDIOUT hmidi = NULL;
	
	UINT res = midiOutOpen(&hmidi, DeviceID, 0, 0, CALLBACK_NULL);
	if(res == MMSYSERR_NOERROR){
		GimicMIDIPtr p = GimicMIDIPtr(new GimicMIDI(hmidi));
		gimics.push_back( p );
		p->init();
	}

	gimics.unlock();
	
	return 0;
}

void GimicMIDI::sendSysEx( uint8_t *data, uint32_t sz )
{
	MIDIHDR head;
	ZeroMemory(&head, sizeof(MIDIHDR));
	head.lpData = reinterpret_cast<LPSTR>(data);
	head.dwBufferLength = sz;

	midiOutPrepareHeader(hHandle, &head, sizeof(MIDIHDR));
	midiOutLongMsg(hHandle, &head, sizeof(MIDIHDR));
	// ↓これ無くてもいいですよね？(by Guu)
	//	while((head.dwFlags & MHDR_DONE) == 0) ;
	midiOutUnprepareHeader(hHandle, &head, sizeof(MIDIHDR));
}

/*----------------------------------------------------------------------------
	実装
----------------------------------------------------------------------------*/
int GimicMIDI::init(void)
{
	// MIDI-IFの場合はOPNA決めうち(module判定ifを作ってないから)
	chiptype = CHIP_OPNA;
	chip = new COPNA(static_cast<IRealChip2*>(this));
	//chiptype = CHIP_OPM;
	//chip = new COPM(static_cast<IRealChip2*>(this));
	return C86CTL_ERR_NONE;
}

int GimicMIDI::reset( void )
{
	// 転送完了待ち
	while(rbuff.length()){
		Sleep(10);
	}

	// GM System ON
	UCHAR d[] = { 0xf0, 0x7e, 0x7f, 0x9, 0x1, 0xf7 };
	sendSysEx( &d[0], 6 );
	return C86CTL_ERR_NONE;
}

void GimicMIDI::out(UINT addr, UCHAR data)
{
	bool flag = true;
	if( chip )
		flag = chip->setReg(addr, data );
	if( flag ){
		// data packing.
		UCHAR d[3] = { (addr>>6)&0x0f, (addr&0x3f)<<1 | (data>>7), (data&0x7f) };
		rbuff.push(d,3);
	}
}

UCHAR GimicMIDI::in(UINT addr)
{
	if( chip )
		return chip->getReg(addr);

	return 0;
}

int GimicMIDI::setSSGVolume(UCHAR vol)
{
	// master volume set.
	UCHAR d[] = { 0xf0, 0x7f, 0x04, (vol<<6)&0x7f, (vol>>1)&0x7f, 0xf7 };
	sendSysEx( &d[0], 6 );
	return C86CTL_ERR_NONE;
}

int GimicMIDI::getSSGVolume(UCHAR *vol)
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}

int GimicMIDI::setPLLClock(UINT clock)
{
	UCHAR d[] = { 0xf0, 0x7d, 0x40, 0x01, 0x00, 0x70,
		clock&0x7f, (clock>>7)&0x7f, (clock>>14)&0x7f, 0xf7 };
	sendSysEx( &d[0], 10 );
	return C86CTL_ERR_NONE;
}
int GimicMIDI::getPLLClock(UINT *clock)
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}

int GimicMIDI::getMBInfo( struct Devinfo *info )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}

int GimicMIDI::getModuleInfo( struct Devinfo *info )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}

int GimicMIDI::getFWVer( UINT *major, UINT *minor, UINT *rev, UINT *build )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}

int GimicMIDI::getChipStatus( UINT addr, UCHAR *status )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}
/*
int GimicMIDI::adpcmZeroClear(void)
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}

int GimicMIDI::adpcmWrite( UINT startAddr, UINT size, UCHAR *data )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}

int GimicMIDI::adpcmRead( UINT startAddr, UINT size, UCHAR *data )
{
	return C86CTL_ERR_NOT_IMPLEMENTED;
}
*/
void GimicMIDI::tick(void)
{
	UCHAR buff[150];
	
	if( !hHandle )
		return;

	// 転送配列準備
	buff[0] = 0xf0;		// start of sysex
	buff[1] = 0x7d;		// device id
	UINT msz = rbuff.length();
	UINT sz = msz / 3;
	if( 0 < sz ){
		rbuff.pop( &buff[2], MIN(sz*3,144) );
		buff[sz*3+2] = 0xf7;		// end of sysex
		sendSysEx( &buff[0], sz*3+3 );
	}
	
	return;
}

#endif // SUPPORT_MIDI
