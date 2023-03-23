/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#pragma once
#include "chip.h"
#include "interface/if.h"
#include "opnx.h"
#include "opl3.h"

namespace c86ctl{


// ---------------------------------------------------------------------------------------
class COPN3L : public Chip
{
public:
	COPN3L(IRealChip2 *p) : pIF(p) {
		fm = new COPNFm(p);
		ssg = new COPNSsg(p);
		rhythm = new COPNRhythm(p);
		partMask = 0;
		partSolo = 0;
		reset();
	};
	virtual ~COPN3L(){
		if(fm) delete fm;
		if(ssg) delete ssg;
		if(rhythm) delete rhythm;
	};

	void reset(){
		for( int i=0; i<2; i++ ){
			memset( reg[i], 0, 256 );
			memset( regATime[i], 0, 256 );
		}

		for( int i=0x40; i<=0x4e;i++ ) // FM TL=127
			reg[0][i] = reg[1][i] = 0x7f;
		for( int i=0x80; i<=0x8e; i++ ) // FM SL,RR
			reg[0][i] = reg[1][i] = 0xff;
		for( int i=0xb4; i<0xb6+1; i++) // FM PAN/AMS/PMS
			reg[0][i] = reg[1][i] = 0xc0;
		reg[0][0x27] = 0x30; // Timer Control
		reg[0][0x29] = 0x80; // FM4-6 Enable
		reg[0][0x07] = 0x38; // SSG ミキサ
		reg[0][0x10] = 0xBF;

		fm->reset();
		ssg->reset();
		rhythm->reset();

		// 強制的にOPNAモードに切り替え
		pIF->directOut( 0x29, 0x9f );
		reg[0][0x29] = 0x9f;

		for( int i=0; i<13; i++ )
			applyMask(i);
	};
	
	
public:
	void update(){
		int dc = 4;
		for( int j=0; j<2; j++ ){
			for( int i=0; i<256; i++ ){
				UCHAR c=regATime[j][i];
				if( 64<c ){
					c-=dc;
					regATime[j][i] = 64<c ? c : 64;
				}
			}
		}
		fm->update();
		ssg->update();
		rhythm->update();
	};

public:
	virtual void filter( int addr, UCHAR *data );
	virtual bool setReg( int addr, UCHAR data );
	virtual UCHAR getReg( int addr );
	virtual void setMasterClock( UINT clock ){
		fm->setMasterClock(clock);
		ssg->setMasterClock(clock);
	};
	
	void setPartMask(int ch, bool mask);
	void setPartSolo(int ch, bool mask);
	bool getPartMask(int ch){ return partMask&(1<<ch) ? true : false; };
	bool getPartSolo(int ch){ return partSolo&(1<<ch) ? true : false; };
	bool getMixedMask(int ch){
		if( partSolo ) return (((~partSolo) | partMask) & (1<<ch)) ? true : false;
		else return getPartMask(ch);
	};
	

	int getTimerA(){ return timerA; };
	int getTimerB(){ return timerB; };


public:
	COPNFm *fm;
	COPNSsg *ssg;
	COPNRhythm *rhythm;
	
	UCHAR reg[2][256];
	UCHAR regATime[2][256];

protected:
	bool fmCommonRegHandling( UCHAR adrs, UCHAR data );
	void applyMask(int ch);
	
protected:
	int timerA; //10bit
	int timerB; //8bit
	
	bool timerA_sw;
	bool timerB_sw;
	
	UINT partMask;
	UINT partSolo;
	IRealChip2 *pIF;
};


};
