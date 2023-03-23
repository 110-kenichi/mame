/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#pragma once
#include "chip.h"

namespace c86ctl{

// ---------------------------------------------------------------------------------------
class COPL3 : public Chip
{
public:
	COPL3(){ reset(); };
	virtual ~COPL3(){};

	void reset(){
		memset( reg, 0, 256*2 );
		memset( regATime, 0, 256*2 );
	};
	
	void update(){
		int dc = 8;
		for( int j=0; j<2; j++ ){
			for( int i=0; i<256; i++ ){
				UCHAR c=regATime[j][i];
				if( 64<c ){
					c-=dc;
					regATime[j][i] = 64<c? c : 64;
				}
			}
		}
	};

public:
	bool setReg( int addr, UCHAR data ){
		if( 0x200 <= addr )
			return false;
		
		int bank = addr>>8;
		addr &= 0xff;
		reg[bank][addr] = data;
		regATime[bank][addr] = 255;
		return true;
		//return false;
	}
	UCHAR getReg( int addr ){
		if( addr < 0x200 )
			return reg[addr>>8][addr&0xff];
		return 0;
	};
	virtual void setMasterClock( UINT clock ){};

public:
	UCHAR reg[2][256];
	UCHAR regATime[2][256];
};

};

