/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "stdafx.h"
#include <stdlib.h>
#include "opn3l.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;

void COPN3L::filter( int addr, UCHAR *data )
{
	int ch;
	if( 0x200 <= addr ) return;

	switch( addr ){
	case 0x29: // SCH/IRQ ENABLE
		*data |= 0x80;
	case 0xb4: // FM -- LR, AMS, PMS
	case 0xb5:
	case 0xb6:
		ch = addr - 0xb4;
		if( getMixedMask( ch ) )
			*data &= 0x3f;
		break;
			
	case 0x1b4: // FM -- LR, AMS, PMS
	case 0x1b5:
	case 0x1b6:
		ch = addr - 0x1b4;
		if( getMixedMask( 6+ch ) )
			*data &= 0x3f;
		break;

	case 0x08: // SSG amp level
	case 0x09:
	case 0x0a:
		ch = addr-0x08;
		if( getMixedMask( 9+ch ) )
			*data &= 0xf0;
		break;
		
	case 0x11: // Rhythm total level
		if( getMixedMask( 12 ) )
			*data = 0;
		break;
	}
}

bool COPN3L::setReg( int addr, UCHAR data ){
	if( 0x200 <= addr ) return false;
	int idx = 0;
	if( 0x100 <= addr ){
		addr -= 0x100;
		idx = 1;
	}
	reg[idx][addr] = data;
	INT c = regATime[idx][addr];
	c+=64;
	regATime[idx][addr] = 255<c ? 255 : c;

	if( idx == 0 ){
		if( ssg->setReg( addr, data ) )
			return true;
		if( rhythm->setReg( addr, data ) )
			return true;
		if( fmCommonRegHandling( addr, data ) )
			return true;
	}
	if( fm->setReg( idx, addr, data ) )
		return true;

	return false;
};

UCHAR COPN3L::getReg( int addr ){
	if( 0x200 <= addr ) return 0;
	int idx = 0;
	if( 0x100 <= addr ){
		addr -= 0x100;
		idx = 1;
	}
	return reg[idx][addr];
};


bool COPN3L::fmCommonRegHandling( UCHAR adrs, UCHAR data )
{
	bool handled = true;
	
	switch(adrs){
	case 0x10:	// STATUS MASK
		//data&0x80; // IRQ RESET
		//data&0x10; // MASK ZERO (ADPCM)
		//data&0x08; // MASK BRDY (ADPCM)
		//data&0x04; // MASK EOS (ADPCM)
		//data&0x02; // MASK TIMER-B
		//data&0x01; // MASK TIMER-A
		break;
		
	// prescaler
	case 0x2d:
	case 0x2e:
	case 0x2f:
		break;

	case 0x24:	// Timer-A Corse
	case 0x25:	// Timer-A Fine
	//case 0x26:	// Timer-B
	case 0x29:	// IRQ/SCH
		break;
		
	case 0x20:	// new
		break;

	default:
		handled = false;
		break;
	}

	return handled;
}

void COPN3L::applyMask(int ch)
{
	UCHAR data;
	bool mask = getMixedMask(ch);
	
	if( 0<=ch && ch<=2 ){ // FM 1~3
		int fmNo = ch;
		data = (fm->ch[fmNo]->getAMS()<<4) | fm->ch[fmNo]->getPMS();
		if( !mask ){
			bool l,r;
			fm->ch[fmNo]->getLR( l,r );
			data |= (l?0x80:0) | (r?0x40:0);
		}
		pIF->directOut( 0xb4+ch, data );
	}
	else if( 6<=ch && ch<=8 ){ // FM 4~6
		int fmNo = ch - 3;
		data = (fm->ch[fmNo]->getAMS()<<4) | fm->ch[fmNo]->getPMS();
		if( !mask ){
			bool l,r;
			fm->ch[fmNo]->getLR( l,r );
			data |= (l?0x80:0) | (r?0x40:0);
		}
		pIF->directOut( 0x1b4+(ch-6), data );
	}
	else if( 9<=ch && ch<=11 ){ // SSG 1~3
		int ssgNo = ch-9;
		data = mask ? 0 : ssg->ch[ssgNo]->getLevel();
		data |= ssg->ch[ssgNo]->isUseEnv() ? 0x10 : 0;
		pIF->directOut( 0x08+ssgNo, data );
	}
	else if( ch == 12 ){ // RHYTHM
		data = mask ? 0 : rhythm->getTotalLevel();
		pIF->directOut( 0x11, data );
	}
}

void COPN3L::setPartMask(int ch, bool mask)
{
	if( ch<0 || 13<=ch ) return;

	if( mask ){
		partMask |= 1<<ch;
	}else{
		partMask &= ~(1<<ch);
	}
	applyMask(ch);
}

void COPN3L::setPartSolo(int ch, bool mask)
{
	if( ch<0 || 13<=ch ) return;
	
	if( mask )	partSolo |= 1<<ch;
	else		partSolo &= ~(1<<ch);
	
	for( int i=0; i<13; i++ )
		applyMask(i);
}

