/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#pragma once
#include "chip.h"
#include "interface/if.h"

namespace c86ctl{

class OPNA;
class OPN3L;
class OPM;

extern const uint32_t ftable[12];
extern const uint32_t fmin;
extern const uint32_t fmax;

extern const char *noteStr[12];

// ---------------------------------------------------------------------------------------
class COPXFmSlot {
	friend class COPXFmCh;
	friend class COPNFmCh;
	friend class COPNFm;
	friend class COPNA;
	friend class COPN3L;
	friend class COPMFmCh;
	friend class COPMFm;
	friend class COPM;
	
public:
	COPXFmSlot(IRealChip2 *p) : pIF(p){ reset(); };
	~COPXFmSlot(){};

	void reset(void){
		detune = 0;
		detune2 = 0;
		multiple = 0;
		keyscale = 0;
		attackRate = 0;
		decayRate = 0;
		sustainRate = 0;
		releaseRate = 63;
		ssgegType = 0;
		sustainLevel = 63;
		totalLevel = 127;
		enable = false;
		AM = false;
	};
	
public:
	int getDetune(){ return detune; };
	int getDetune2(){ return detune; };
	double getDetuneHz();

	int getMultiple(){ return multiple; };
	double getMultipleRatio();
	
	int getKeyscale(){ return keyscale; };
	int getAttackRate(){ return attackRate; };
	int getDecayRate(){ return decayRate; };
	int getSustainRate(){ return sustainRate; };
	int getReleaseRate(){ return releaseRate; };
	int getSSGEGType(){ return ssgegType; };
	int getSustainLevel(){ return sustainLevel; };
	int getTotalLevel(){ return totalLevel; };
	
	bool isOn(){ return enable; };
	bool isAM(){ return AM; };

//public:
//	void sendDetune(int dt){
//		setDetune(dt);
//	};
	
protected:
	void setDetune(int dt){ detune = dt&0x07; };
	void setDetune2(int dt2){ detune2 = dt2&0x07; };
	void setMultiple(int mult){ multiple = mult&0x7; };
	void setKeyscale(int scale){ keyscale = scale&0x3; };
	void setAttackRate(int ar){ attackRate = ar&0x1f; };
	void setDecayRate(int dr){ decayRate = dr&0x1f; };
	void setSustainRate(int sr){ sustainRate = sr&0x1f; };
	void setReleaseRate(int rr){ releaseRate = rr&0x1f; };
	void setSSGEGType(int type){ ssgegType = type&0xf; };
	void setSustainLevel(int sl){ sustainLevel = sl&0xf; };
	void setTotalLevel(int tl){ totalLevel = tl&0x7f; };
	void on(){ enable = true; };
	void off(){ enable = false; };
	void AMOn(){ AM = true; };
	void AMOff(){ AM = false; };

protected:
	int detune;			// 3bit
	int detune2;		// 2bit (for OPM)
	int multiple;		// 4bit
	int keyscale;		// 2bit
	int attackRate;		// 5bit
	int decayRate;		// 5bit
	int sustainRate;	// 5bit
	int releaseRate;	// 4bit
	int ssgegType;		// 4bit
	int sustainLevel;	// 4bit
	int totalLevel;	// 7bit

	bool enable;
	bool AM;

	IRealChip2 *pIF;
};


// ---------------------------------------------------------------------------------------
class COPXFmCh {
	friend class COPNFm;
	friend class COPNA;
	friend class COPN3L;
	friend class COPM;
	friend class COPMFm;
	
public:
	COPXFmCh(IRealChip2 *p) : pIF(p){
		for( int i=0; i<4; i++ )
			slot[i] = new COPXFmSlot(p);
		reset();
	};
	virtual ~COPXFmCh(void){
		for( int i=0; i<4; i++ )
			if(slot[i]) delete slot[i];
	};
	
public:
	virtual void reset(){
		for( int i=0; i<4; i++ )
			slot[i]->reset();

		feedback = 0;
		algorithm = 0;
		ams = 0;
		pms = 0;
		left = true;
		right = true;

		for( int i=0; i<4; i++ )
			keyOnLevel[i] = 0;
	};
	virtual void update(){
		for( int i=0; i<4; i++ )
			if(keyOnLevel[i]) keyOnLevel[i]--;
	};
	int getKeyOnLevel(){ return keyOnLevel[3]; };
	int getKeyOnLevelEx(int idx){ return keyOnLevel[idx]; };
	virtual void getNote(int &oct, int &note){};

public:
	bool isKeyOn(){
		if( left || right ){
			for( int i=0; i<4; i++ )
				if( slot[i]->isOn() )
					return true;
		}
		return false;
	};
	
public:
	int getAMS(){ return ams; };
	int getPMS(){ return pms; };
	int getFeedback(){ return feedback; };
	int getAlgorithm(){ return algorithm; };
	void getLR( bool &l, bool &r ){ l = left; r = right; };
	int getPan(){ // ±1
		if( left&&right ) return 0;
		else if(left) return -1;
		else if(right) return 1;
		else return 0;
	};

	int getMixLevel(){
		//if( !left && !right ) return 0;
		int level=0;
		for( int i=0; i<4; i++ ){
			level+=slot[i]->getTotalLevel();
		}
		return level>>2;
	};

protected:
	virtual void keyOn( UCHAR slotsw ){};

	void setAMS(int ams){ this->ams = ams; };
	void setPMS(int pms){ this->pms = pms; };
	void setFeedback(int feedback){ this->feedback = feedback; };
	void setAlgorithm(int algorithm){ this->algorithm = algorithm; };
	void setLR( bool l, bool r ){ left = l; right = r; };
	
public:
	COPXFmSlot *slot[4];

protected:
	int feedback;	// 3bit
	int algorithm;	// 3bit
	int ams;		// 2bit
	int pms;		// 3bit
	bool left;
	bool right;
	int keyOnLevel[4];
	
	IRealChip2 *pIF;
};

};
