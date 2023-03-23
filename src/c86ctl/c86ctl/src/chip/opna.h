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

namespace c86ctl{

// ---------------------------------------------------------------------------------------
class COPNAAdpcm{
	friend class COPNA;

	static const size_t ramsize = 256*1024;
	static const size_t minimapsize = 512;
public:
	COPNAAdpcm(IRealChip2 *p) : pIF(p){
		dram = new UCHAR[ramsize];
		map = new UCHAR[ramsize];
		wav = new SHORT[ramsize*2];
		minimap = new UCHAR[minimapsize];
		reset();
	};
	virtual ~COPNAAdpcm(){
		if( dram ) delete [] dram;
		if( map ) delete [] map;
		if( wav ) delete [] wav;
		if( minimap ) delete [] minimap;
	};

	void reset(){
		startAddr = 0;
		stopAddr = 0;
		limitAddr = 0;
		currentAddr = 0;
		prescale = 0;
		deltaN = 0;
		level = 0;
		keyOnLevel = 0;
		sw = false;

		memset(dram,0,ramsize);
		memset(map,0,ramsize);
		memset(wav,0,ramsize*2*2);
		memset(minimap,0,minimapsize);
	};
	void update(){
		if(keyOnLevel) keyOnLevel--;
	};

	void getLR( bool &l, bool &r ){ l = left; r = right; };
	int getPan(){ // ±1
		if( left&&right ) return 0;
		else if(left) return -1;
		else if(right) return 1;
		else return -2;
	};

	int getKeyOnLevel(){ return keyOnLevel; };
	
public:
	bool isOn(){ return sw; };
	int getLevel(){ return level; };
	UINT getStartAddr(){ return startAddr; };
	UINT getStopAddr(){ return stopAddr; };
	UINT getLimitAddr(){ return limitAddr; };
	
protected:
	bool setReg( UCHAR addr, UCHAR data );
	void setLR( bool l, bool r ){ left = l; right = r; };

protected:
	UINT startAddr;	//21bit
	UINT stopAddr;	//21bit
	UINT limitAddr;	//21bit
	UINT currentAddr;//21bit (write pointer)
	int prescale;	//16bit
	int deltaN;		//16bit
	int level;		//8bit;
	int keyOnLevel;
	
	UCHAR control1;
	UCHAR control2;
	
	bool left;
	bool right;
	bool sw;

	IRealChip2 *pIF;

public:
	UCHAR *dram; // 2samples per 1byte
	UCHAR *map;
	SHORT *wav;
	UCHAR *minimap;

};

// ---------------------------------------------------------------------------------------
class COPNA : public Chip
{
public:
	COPNA(IRealChip2 *p) : pIF(p){
		fm = new COPNFm(p);
		ssg = new COPNSsg(p);
		adpcm = new COPNAAdpcm(p);
		rhythm = new COPNRhythm(p);
		partMask = 0;
		partSolo = 0;
		//modeOPNA = true;
		reset();
	};
	virtual ~COPNA(){
		if(fm) delete fm;
		if(ssg) delete ssg;
		if(adpcm) delete adpcm;
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
		adpcm->reset();

		// 強制的にOPNAモードに切り替え
		pIF->directOut( 0x29, 0x9f );
		reg[0][0x29] = 0x9f;

		for( int i=0; i<14; i++ )
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
		adpcm->update();
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
	int getFMPrescale(){ return prescale_fm; };
	int getSSGPrescale(){ return prescale_ssg; };


public:
	COPNFm *fm;
	COPNSsg *ssg;
	COPNRhythm *rhythm;
	COPNAAdpcm *adpcm;
	
	UCHAR reg[2][256];
	UCHAR regATime[2][256];

protected:
	bool fmCommonRegHandling( UCHAR adrs, UCHAR data );
	void applyMask(int ch);
	
protected:
	int timerA; //10bit
	int timerB; //8bit
	int prescale_fm;
	int prescale_ssg;
	
	bool timerA_sw;
	bool timerB_sw;
	//bool modeOPNA;
	
	UINT partMask;
	UINT partSolo;
	IRealChip2 *pIF;
};

};

