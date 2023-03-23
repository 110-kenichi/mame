/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "stdafx.h"
#include <stdlib.h>
#include <math.h>
#include "opm.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;


void COPMFmCh::setMasterClock( UINT clock ){
	mclk = clock;
	dcent = static_cast<uint32_t>( log(static_cast<double>(mclk) / 3579545.0) * 1200.0 / log(2.0) + 0.5 );
}

void COPMFmCh::getNote(int &oct, int &note){
	//                          0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14
	const uint32_t ntbl[15] = { 1, 2, 3, 3, 4, 5, 6, 6, 7, 8, 9, 9,10,11,12 }; 
	uint32_t cent = kcoct*1200 + ntbl[kcnote]*100 + (100*kfcent/63) + dcent + 50;
	oct = cent / 1200;
	note = (cent%1200) / 100;
};


bool COPMFm::setReg( UCHAR addr, UCHAR data )
{
	// M1==S1, M2==C3, C1==S2, C2==S4
	const int slotidx[4] = { 0, 2, 1, 3 };
	bool handled = true;

	if( 0x40<=addr ){
		
		// 0=M1, 1=M2, 2=C1, 3=C2
		int sidx = slotidx[ (addr>>3)&0x03 ];
		int cidx = addr&0x07;

		COPXFmSlot *slot = ch[cidx]->slot[sidx];
		switch( addr>>5 ){
		case 2: // DT1/MUL
			slot->setDetune( (data>>4)&0x07 );
			slot->setMultiple( data&0x0f );
			break;
		case 3: // TL
			slot->setTotalLevel( data&0x7f );
			break;
		case 4: // KS/AR
			slot->setKeyscale( data>>6 );
			slot->setAttackRate( data&0x1f );
			break;
		case 5: // AMS/D1R(DR)
			if(data&0x80) slot->AMOn();
			else slot->AMOff();
			slot->setDecayRate( data&0x1f );
			break;
		case 6: // DT2/D2R(SR)
			slot->setDetune2( data>>6 );
			slot->setSustainRate( data&0x1f );
			break;
		case 7: // D1L(SL)/RR
			slot->setSustainLevel( data>>4 );
			slot->setReleaseRate( data&0x0f );
			break;
		}
		
	}else if( 0x20<=addr ){
		int cidx = addr&0x07;
		
		COPMFmCh *fmCh = ch[cidx];
		switch( addr>>3 ){
		case 4: // RL/FL/CON
			fmCh->setLR( ((data&0x40)?true:false), ((data&0x80)?true:false) );
			fmCh->setFeedback( (data>>3)&0x07 );
			fmCh->setAlgorithm( data&0x07 );
			break;
		case 5: // KC
			fmCh->setKeyCode( data );
			break;
		case 6: // KF
			fmCh->setKeyFraction( data );
			break;
		case 7: // PMS/AMS
			fmCh->setAMS( data&0x03 );
			fmCh->setPMS( (data>>4)&0x07 );
			break;
		}
	}else{
		switch(addr){
		case 0x01: // TEST
			break;
		case 0x08: // KON
			{
				COPMFmCh *fmCh = ch[data&0x7];
				fmCh->keyOn( (data>>3)&0x0f );
			}
			break;
		case 0x0f: // NFREQ
			break;
		case 0x10: // CLKA1
			break;
		case 0x11: // CLKA2
			break;
		case 0x12: // CLKB
			break;
		case 0x14: // CSM/F-RESET/IRQEN/LOAD
			break;
		case 0x18: // LFRQ
			break;
		case 0x19: // PMD/AMD
			break;
		case 0x1b: // CT/W
			break;
		default:
			handled = false;
		}
	}
	
	return handled;
}

void COPM::filter( int addr, UCHAR *data )
{
	int ch;
	if( 0x100 <= addr ) return;

	if( 0x20<=addr && addr<=0x27 ){
		ch = addr-0x20;
		if( getMixedMask( ch ) ){
			*data &= 0x3f;
		}
	}
	return;
}

bool COPM::setReg( int addr, UCHAR data )
{
	if( 0x100 <= addr ) return false;
	
	reg[addr] = data;
	INT c = regATime[addr];
	c+=64;
	regATime[addr] = 255<c ? 255 : c;

	return fm->setReg( addr, data );
}

UCHAR COPM::getReg( int addr )
{
	if( addr < 0x100 )
		return reg[addr];
	return 0;
}

void COPM::applyMask(int ch)
{
	UCHAR data;
	bool mask = getMixedMask(ch);
	
	if( 0<=ch && ch<=7 ){ // FM 1~8
		int fmNo = ch;
		data = fm->ch[fmNo]->getAlgorithm() | (fm->ch[fmNo]->getFeedback()<<3);
		if( !mask ){
			bool l,r;
			fm->ch[fmNo]->getLR( l,r );
			data |= (l?0x80:0) | (r?0x40:0);
		}
		pIF->directOut( 0x20+ch, data );
	}
}

void COPM::setPartMask(int ch, bool mask)
{
	if( ch<0 || 8<=ch ) return;

	if( mask ){
		partMask |= 1<<ch;
	}else{
		partMask &= ~(1<<ch);
	}
	applyMask(ch);
}

void COPM::setPartSolo(int ch, bool mask)
{
	if( ch<0 || 8<=ch ) return;
	
	if( mask )	partSolo |= 1<<ch;
	else		partSolo &= ~(1<<ch);
	
	for( int i=0; i<8; i++ )
		applyMask(i);
}

