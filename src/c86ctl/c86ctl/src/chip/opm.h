/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#pragma once
#include "chip.h"
#include "opx.h"

namespace c86ctl{

// ---------------------------------------------------------------------------------------
class COPMFmCh : public COPXFmCh {
	friend class COPM;
	friend class COPMFm;
	
public:
	COPMFmCh(IRealChip2 *p) : COPXFmCh(p) {
		setMasterClock(3579545);
		reset();
	};
	virtual ~COPMFmCh(void){
	};

	virtual void reset(){
		COPXFmCh::reset();
		kcoct = 0;
		kcnote = 0;
		kfcent = 0;
	};

public:
	virtual void getNote(int &oct, int &note);
	void setMasterClock( UINT clock );
	UCHAR getKeyCodeOct(){ return kcoct; };
	UCHAR getKeyCodeNote(){ return kcnote; };
	UCHAR getKeyFraction(){ return kfcent; };
	
protected:
	virtual void keyOn( UCHAR slotsw ){
		for( int i=0; i<4; i++ ){ // D3:4, D2:3, D1:2, D0:1
			if( slotsw & 0x01 ){
				slot[i]->on();
				keyOnLevel[i] = (127-slot[i]->getTotalLevel())>>2;
			}else{
				slot[i]->off();
			}
			slotsw >>= 1;
		}
	};

	void setKeyCode( UCHAR kc ){
		kcoct = (kc>>4)&0x7;
		kcnote = kc&0xf;
	};
	void setKeyFraction( UCHAR kf ){
		kfcent = (kf >> 2);
	};

protected:
	uint32_t mclk;
	uint32_t dcent;
	
	uint32_t kcoct;
	uint32_t kcnote;
	uint32_t kfcent;
	
	IRealChip2 *pIF;
};

// ---------------------------------------------------------------------------------------

class COPMFm{
	friend class COPM;

public:
	COPMFm(IRealChip2 *p) : pIF(p){
		for( int i=0; i<9; i++ )
			ch[i] = new COPMFmCh(p);
		reset();
	};
	virtual ~COPMFm(){
		for( int i=0; i<9; i++ )
			if(ch[i]) delete ch[i];
	};
	
	void reset(){
		lfo = 0;
		lfo_sw = false;
		for( int i=0; i<9; i++ )
			ch[i]->reset();
	};
	
	void update(){
		for( int i=0; i<9; i++ )
			ch[i]->update();
	};
	
public:
	int getLFO(){ return lfo; };
	
protected:
	bool isLFOOn(){ return lfo_sw; };
	bool setReg( UCHAR addr, UCHAR data );
	

public:
	COPMFmCh *ch[9];
	
protected:
	int lfo; //3bit
	bool lfo_sw;

	IRealChip2 *pIF;
};


// ---------------------------------------------------------------------------------------

class COPM : public Chip
{
public:
	COPM(IRealChip2 *p) : pIF(p) {
		fm = new COPMFm(p);
		partMask = 0;
		partSolo = 0;
		reset();
	};
	virtual ~COPM(){
		if(fm) delete fm;
	};
	
	void reset(){
		memset( reg, 0, 256 );
		memset( regATime, 0, 256 );
		fm->reset();
	}

public:
	virtual void filter( int addr, UCHAR *data );
	virtual bool setReg( int addr, UCHAR data );
	virtual UCHAR getReg( int addr );
	virtual void setMasterClock( UINT clock ){};
	
	void setPartMask(int ch, bool mask);
	void setPartSolo(int ch, bool mask);
	bool getPartMask(int ch){ return partMask&(1<<ch) ? true : false; };
	bool getPartSolo(int ch){ return partSolo&(1<<ch) ? true : false; };
	bool getMixedMask(int ch){
		if( partSolo ) return (((~partSolo) | partMask) & (1<<ch)) ? true : false;
		else return getPartMask(ch);
	};
	
public:
	void update(){
		int dc = 4;
		for( int i=0; i<256; i++ ){
			UCHAR c=regATime[i];
			if( 64<c ){
				c-=dc;
				regATime[i] = 64<c ? c : 64;
			}
		}
		fm->update();
	};

public:
	UCHAR reg[256];
	UCHAR regATime[256];
	
public:
	COPMFm *fm;
	
protected:
	bool fmCommonRegHandling( UCHAR adrs, UCHAR data );
	void applyMask(int ch);
	
protected:
	UINT partMask;
	UINT partSolo;
	IRealChip2 *pIF;
};

};
