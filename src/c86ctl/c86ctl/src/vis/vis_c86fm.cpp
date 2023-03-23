/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#include "stdafx.h"
#include "chip/opna.h"
#include "vis_c86fm.h"
#include "vis_c86sub.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;
using namespace c86ctl::vis;

#define WINDOW_WIDTH  (316+4)
#define WINDOW_HEIGHT (75*5+4+15)

bool CVisC86Fm::createFmSlotView( COPXFmSlot *slot, int i, int x, int y )
{
	// AR
	knobAR[i] = CVisKnobPtr( new CVisKnob(this, x+161, y+17));
	knobAR[i]->setRange(0,31);
	knobAR[i]->getter = [slot, i]() -> int{ return slot->getAttackRate(); };
	widgets.push_back(knobAR[i]);

	// DR
	knobDR[i] = CVisKnobPtr( new CVisKnob(this, x+189, y+17));
	knobDR[i]->setRange(0,31);
	knobDR[i]->getter = [slot, i]() -> int{ return slot->getDecayRate(); };
	widgets.push_back(knobDR[i]);

	// SR
	knobSR[i] = CVisKnobPtr( new CVisKnob(this, x+216, y+17));
	knobSR[i]->setRange(0,31);
	knobSR[i]->getter = [slot, i]() -> int{ return slot->getSustainRate(); };
	widgets.push_back(knobSR[i]);

	// RR
	knobRR[i] = CVisKnobPtr( new CVisKnob(this, x+243, y+17));
	knobRR[i]->setRange(0,31);
	knobRR[i]->getter = [slot, i]() -> int{ return slot->getReleaseRate(); };
	widgets.push_back(knobRR[i]);

	// SL
	knobSL[i] = CVisKnobPtr( new CVisKnob(this, x+270, y+17));
	knobSL[i]->setRange(0,15);
	knobSL[i]->getter = [slot, i]() -> int{ return slot->getSustainLevel(); };
	widgets.push_back(knobSL[i]);

	// TL
	knobTL[i] = CVisKnobPtr( new CVisKnob(this, x+297, y+17));
	knobTL[i]->setRange(0,127);
	knobTL[i]->getter = [slot, i]() -> int{ return slot->getTotalLevel(); };
	widgets.push_back(knobTL[i]);

	// MUL
	knobMUL[i] = CVisKnobPtr( new CVisKnob(this, x+161, y+50));
	knobMUL[i]->setRange(0,15);
	knobMUL[i]->getter = [slot, i]() -> int{ return slot->getMultiple(); };
	widgets.push_back(knobMUL[i]);

	// DET
	knobDET[i] = CVisKnobPtr( new CVisKnob(this, x+189, y+50));
	knobDET[i]->setRange(0,7);
	knobDET[i]->getter = [slot, i]() -> int{ return slot->getDetune(); };
	widgets.push_back(knobDET[i]);

	// DET2 - OPNAでは使えない。
	knobDET2[i] = CVisKnobPtr( new CVisKnob(this, x+216, y+50));
	knobDET2[i]->setRange(0,31);
	widgets.push_back(knobDET2[i]);

	// AM
	dipswAM[i] = CVisDipSwPtr( new CVisDipSw(this, x+248, y+50));
	dipswAM[i]->getter = [slot, i]() -> int{ return slot->isAM(); };
	widgets.push_back(dipswAM[i]);
	
	// SSG-EG
	graphSSGEG[i] = CVisSSGEGGraphPtr( new CVisSSGEGGraph(this, x+270, y+58) );
	graphSSGEG[i]->getter = [slot, i]() -> int{ return slot->getSSGEGType(); };
	widgets.push_back(graphSSGEG[i]);
	
	return true;
}

bool CVisC86Fm::createFmView( COPNFmCh *pFmCh )
{
	// AMS
	knobAMS = CVisKnobPtr( new CVisKnob(this, 134-11, 25-10));
	knobAMS->setRange(0,3);
	knobAMS->getter = [pFmCh]() -> int{ return pFmCh->getAMS(); };
	//knobAMS->setter = [pOPNAFm](int ams) { pOPNAFm->setAMS(ams); };
	widgets.push_back(knobAMS);

	// PMS
	knobPMS = CVisKnobPtr( new CVisKnob(this, 161-11, 25-10));
	knobPMS->setRange(0,7);
	knobPMS->getter = [pFmCh]() -> int{ return pFmCh->getPMS(); };
	widgets.push_back(knobPMS);

	// FB
	knobFB = CVisKnobPtr( new CVisKnob(this, 134-11, 59-10));
	knobFB->setRange(0,7);
	knobFB->getter = [pFmCh]() -> int{ return pFmCh->getFeedback(); };
	widgets.push_back(knobFB);

	// PAN
	knobPAN = CVisKnobPtr( new CVisKnob(this, 161-11, 59-10));
	knobPAN->setRange(-1,1);
	knobPAN->getter = [pFmCh]() -> int{ return pFmCh->getPan(); };
	widgets.push_back(knobPAN);

	int x=0, y=75;
	for(int i=0; i<4; i++){
		COPXFmSlot *slot = pFmCh->slot[i];
		createFmSlotView( pFmCh->slot[i], i, x, y );
		y+=75;
	}
	
	return true;
}


void CVisC86Fm::drawFMView( IVisBitmap *canvas, int x, int y, COPNFmCh *pFmCh )
{
	int exmode = pFmCh->getExMode();
	CVisC86Skin *skin = &gVisSkin;
	skin->drawFMSkin( canvas, x, y, exmode );
	
	int algno = pFmCh->getAlgorithm();
	skin->drawFMAlgorithm( canvas, x+20, y+7, algno );
	
	//slot ON/OFF
	const int lx[4] = {248, 263, 278, 293};
	for( int i=0; i<4; i++ ){
		skin->drawFMSlotLight( canvas, x+lx[i], y+56, i, pFmCh->slot[i]->isOn() );
	}

	char str[128];
	if( !exmode ){
		// NOTE
		skin->drawStr( canvas, 0, 186, 6, "NOTE: " );
		if( pFmCh->isKeyOn() && pFmCh->getMixLevel()!=127 ){
			int oct=0, note=0;
			pFmCh->getNote( oct, note );
			sprintf( str, "O%d%s", oct, noteStr[note] );
			skin->drawStr( canvas, 0, 186+5*7, 6, str );
		}
		// FREQ
		double freq = pFmCh->getFreq();
		sprintf( str, "FREQ: % 8.1f Hz", freq );
		skin->drawStr( canvas, 0, 186, 6+12, str );
	}else{
		const char *modestr[] = { "EFF", "CSM", "EFF" };
		skin->drawStr( canvas, 0, 188, 59, modestr[exmode-1] );
		
		for( int i=0; i<4; i++ ){
			double freq = pFmCh->getFreqEx(i);
			sprintf( str, "FREQ: % 8.1f Hz", freq );
			skin->drawStr( canvas, 0, 186, 6+12*i, str );
		}
	}
}

void CVisC86Fm::drawFMSlotView( IVisBitmap *canvas, int x, int y, COPXFmSlot *pSlot, int slotidx )
{
	CVisC86Skin *skin = &gVisSkin;
	skin->drawFMSlotSkin( canvas, x, y );

	// index
	char str[10];
	sprintf(str, "%d", slotidx+1);
	skin->drawVStr( canvas, 0, x+8, y+20, str );
	
#if 1
	// VOPMのアルゴリズムをぱくった。
	// 意味が理解出来てないのでバグ有るかも・・・
	double ar = pSlot->getAttackRate();
	double dr = pSlot->getDecayRate();
	double sr = pSlot->getSustainRate();
	double rr = pSlot->getReleaseRate();
	double sl = pSlot->getSustainLevel();
	double tl = pSlot->getTotalLevel();
	double mul = pSlot->getMultiple();
	double det = pSlot->getDetune();
	double dx = 127.0;
	double dy = 63.0;

	double ax, ay, d1x, d1y, d2x, d2y, rx, ry;
	
	if(ar==0 || tl==127){
		// 無音
	}else{
		// attack ----
		ay = tl;
		ax = (127.0-tl) / (5.0*ar);
		
		// decay1 ----
		if( (sl*4)>tl ){
			if( 0<dr ){
				d1y = 4.0*sl;
				d1x = (d1y-ay)/dr + ax;
			}else{
				d1y = ay;
				d1x = 3.0*127.0/4.0;
			}
		}else{
			d1x = ax;
			d1y = ay;
		}

		// decay2 ----
		d2x = 3.0*127/4.0;
		if( 0<dr ){
			d2y = sr * (d2x-d1x) + d1y;
		}else{
			d2y = d1y;
		}
		
		// release ----
		if( 0<rr ){
			rx = (127.0-d2y) / (2.0*rr) + d2x;
			ry = 127.0;
		}else{
			rx = 127.0;
			ry = d2y;
		}
		if( d1y>127.0 ) d1y = 127.0;
		if( d2y>127.0 ) d2y = 127.0;
		if( ry>127.0 ) ry = 127.0;
		if( d2x<d1x ) d2x = d1x;
		if( rx<d2x ) rx = d2x;
		
		// scaling
		ay = ay*dy/127.0;
		d1y = d1y*dy/127.0;
		d2y = d2y*dy/127.0;
		ry = ry*dy/127.0;

		int sx=x+23, sy=y+6;
		visDrawLine( canvas, sx         , sy+63      , sx+(int)ax , sy+(int)ay , 0xffffffff );
		visDrawLine( canvas, sx+(int)ax , sy+(int)ay , sx+(int)d1x, sy+(int)d1y, 0xffffffff );
		visDrawLine( canvas, sx+(int)d1x, sy+(int)d1y, sx+(int)d2x, sy+(int)d2y, 0xffffffff );
		visDrawLine( canvas, sx+(int)d2x, sy+(int)d2y, sx+(int)rx , sy+(int)ry, 0xffffffff );
	}
#endif
}

// ---------------------------------------------------------------
bool CVisC86OPNAFm::create( HWND parent )
{
	if( !CVisWnd::create( _windowWidth, _windowHeight,
		WS_EX_TOOLWINDOW, (WS_POPUP | WS_CLIPCHILDREN), parent ) )
		return false;

	createFmView(pOPNA->fm->ch[this->ch]);
	::ShowWindow( hWnd, SW_SHOWNOACTIVATE );
	
	return true;
}

void CVisC86OPNAFm::onPaintClient()
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );

	// slot view
	if( pOPNA ){
		int sx=5, sy=5, cx=6, cy=8;
		drawFMView( clientCanvas, 0, 0, pOPNA->fm->ch[ch] );
		for( int i=0; i<4; i++ )
			drawFMSlotView( clientCanvas, 0, 75*(1+i), pOPNA->fm->ch[ch]->slot[i], i );
	}
}

// ---------------------------------------------------------------
bool CVisC86OPN3LFm::create( HWND parent )
{
	if( !CVisWnd::create( _windowWidth, _windowHeight,
		WS_EX_TOOLWINDOW, (WS_POPUP | WS_CLIPCHILDREN), parent ) )
		return false;

	createFmView(pOPN3L->fm->ch[this->ch]);
	::ShowWindow( hWnd, SW_SHOWNOACTIVATE );
	
	return true;
}

void CVisC86OPN3LFm::onPaintClient()
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );

	// slot view
	if( pOPN3L ){
		int sx=5, sy=5, cx=6, cy=8;
		drawFMView( clientCanvas, 0, 0, pOPN3L->fm->ch[ch] );
		for( int i=0; i<4; i++ )
			drawFMSlotView( clientCanvas, 0, 75*(1+i), pOPN3L->fm->ch[ch]->slot[i], i );
	}
}
// -----------------------------------------------------------
bool CVisC86OPMFm::create( HWND parent )
{
	if( !CVisWnd::create( _windowWidth, _windowHeight,
		WS_EX_TOOLWINDOW, (WS_POPUP | WS_CLIPCHILDREN), parent ) )
		return false;

	createFmView(pOPM->fm->ch[this->ch]);
	::ShowWindow( hWnd, SW_SHOWNOACTIVATE );
	
	return true;
}

bool CVisC86OPMFm::createFmView( COPMFmCh *pFmCh )
{
	// AMS
	knobAMS = CVisKnobPtr( new CVisKnob(this, 134-11, 25-10));
	knobAMS->setRange(0,3);
	knobAMS->getter = [pFmCh]() -> int{ return pFmCh->getAMS(); };
	//knobAMS->setter = [pOPNAFm](int ams) { pOPNAFm->setAMS(ams); };
	widgets.push_back(knobAMS);

	// PMS
	knobPMS = CVisKnobPtr( new CVisKnob(this, 161-11, 25-10));
	knobPMS->setRange(0,7);
	knobPMS->getter = [pFmCh]() -> int{ return pFmCh->getPMS(); };
	widgets.push_back(knobPMS);

	// FB
	knobFB = CVisKnobPtr( new CVisKnob(this, 134-11, 59-10));
	knobFB->setRange(0,7);
	knobFB->getter = [pFmCh]() -> int{ return pFmCh->getFeedback(); };
	widgets.push_back(knobFB);

	// PAN
	knobPAN = CVisKnobPtr( new CVisKnob(this, 161-11, 59-10));
	knobPAN->setRange(-1,1);
	knobPAN->getter = [pFmCh]() -> int{ return pFmCh->getPan(); };
	widgets.push_back(knobPAN);

	int x=0, y=75;
	for(int i=0; i<4; i++){
		COPXFmSlot *slot = pFmCh->slot[i];
		createFmSlotView( pFmCh->slot[i], i, x, y );
		y+=75;
	}
	
	return true;
}

void CVisC86OPMFm::drawFMView( IVisBitmap *canvas, int x, int y, COPMFmCh *pFmCh )
{
	CVisC86Skin *skin = &gVisSkin;
	skin->drawFMSkin( canvas, x, y, 0 );
	char str[64];
	
	int algno = pFmCh->getAlgorithm();
	skin->drawFMAlgorithm( canvas, x+20, y+7, algno );
	
	//slot ON/OFF
	const int lx[4] = {248, 263, 278, 293};
	for( int i=0; i<4; i++ ){
		skin->drawFMSlotLight( canvas, x+lx[i], y+56, i, pFmCh->slot[i]->isOn() );
	}

	// NOTE
	skin->drawStr( canvas, 0, 186, 6, "NOTE: " );
	if( pFmCh->isKeyOn() && pFmCh->getMixLevel()!=127 ){
		int oct=0, note=0;
		pFmCh->getNote( oct, note );
		sprintf( str, "O%d%s", oct, noteStr[note] );
		skin->drawStr( canvas, 0, 186+5*7, 6, str );
	}

	// KC/KF
	int kcoct = pFmCh->getKeyCodeOct();
	int kcnote = pFmCh->getKeyCodeNote();
	int kf = pFmCh->getKeyFraction();
	sprintf( str, "KC  : %02d-%02d", kcoct, kcnote );
	skin->drawStr( canvas, 0, 186, 6+12, str );
	sprintf( str, "KF  : %02d", kf );
	skin->drawStr( canvas, 0, 186, 6+12*2, str );

}

void CVisC86OPMFm::onPaintClient()
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );

	// slot view
	if( pOPM ){
		int sx=5, sy=5, cx=6, cy=8;
		drawFMView( clientCanvas, 0, 0, pOPM->fm->ch[ch] );
		for( int i=0; i<4; i++ )
			drawFMSlotView( clientCanvas, 0, 75*(1+i), pOPM->fm->ch[ch]->slot[i], i );
	}
}

// --------------------------------------------------------
CVisC86FmPtr c86ctl::vis::visC86FmViewFactory(Chip *pchip, int id, int ch)
{
	if (!pchip) return 0;
	if( typeid(*pchip) == typeid(COPNA) ){
		return CVisC86FmPtr( new CVisC86OPNAFm(dynamic_cast<COPNA*>(pchip), id, ch ) );
	}else if( typeid(*pchip) == typeid(COPN3L) ){
		return CVisC86FmPtr( new CVisC86OPN3LFm(dynamic_cast<COPN3L*>(pchip), id, ch) );
	}else if( typeid(*pchip) == typeid(COPM) ){
		return CVisC86FmPtr( new CVisC86OPMFm(dynamic_cast<COPM*>(pchip), id, ch) );
	}
	return 0;
}

