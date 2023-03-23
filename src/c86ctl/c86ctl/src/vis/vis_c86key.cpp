/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "stdafx.h"
#include <stdio.h>
#include "vis_c86key.h"
#include "vis_c86sub.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif


using namespace c86ctl;
using namespace c86ctl::vis;

void CVisC86Key::drawFMTrackView( IVisBitmap *canvas, int ltx, int lty,
								  int trNo, int fmNo, bool isMute, COPNFmCh *pFMCh )
{
	int sy = 0;
	char str[64], tmp[64];
	int cx=6, cy=8;
	CVisC86Skin *skin = &gVisSkin;

	sprintf( str, "%02d", trNo+1 );
	skin->drawNumStr1( canvas, ltx+5, lty+sy+2, str );
	sprintf( str, "FM-CH%d", fmNo+1 );
	skin->drawStr( canvas, 0, ltx+5+cx*10, lty+sy+5, str );

	if( !isMute ){
		strcpy(str, "SLOT:");
		for( int i=0; i<4; i++ ){
			int sw = pFMCh->slot[i]->isOn();
			sprintf( tmp, "%d", i );
			if( sw ) strncat( str, tmp, 64 );
			else  strncat( str, "_", 64 );
		}
		skin->drawStr( canvas, 0, ltx+5+cx*24, lty+sy+5, str );

		if( !pFMCh->getExMode() ){
			int fblock = pFMCh->getFBlock();
			int fnum = pFMCh->getFNum();
			//sprintf( str, "BLK:%d  FNUM:%04d", fblock, fnum );
			sprintf( str, "B/F:%d+%04d  NOTE:", fblock, fnum );
			skin->drawStr( canvas, 0, ltx+5+cx*35, lty+sy+5, str );
			skin->drawKeyboard( canvas, ltx, lty+sy+15 );

			if( pFMCh->isKeyOn() && pFMCh->getMixLevel() != 127 ){
				int oct, note;
				pFMCh->getNote( oct, note );
				skin->drawHilightKey( canvas, ltx, lty+sy+15, oct, note );
				sprintf( str, "O%d%s", oct, noteStr[note] );
				skin->drawStr( canvas, 0, ltx+5+cx*52, lty+sy+5, str );
			}
			skin->drawHBar( canvas, 290, lty+sy+15, pFMCh->getKeyOnLevel(), 0 );
		}else{
			int fblock = pFMCh->getFBlockEx(0);
			int fnum = pFMCh->getFNumEx(0);
			//sprintf( str, "BLK:%d  FNUM:%04d", fblock, fnum );
			sprintf( str, "B/F:%d+%04d  NOTE:", fblock, fnum );
			skin->drawStr( canvas, 0, ltx+5+cx*35, lty+sy+5, str );
			skin->drawKeyboard( canvas, ltx, lty+sy+15 );

			if( pFMCh->slot[0]->isOn() && pFMCh->slot[0]->getTotalLevel() != 127 ){
				int oct, note;
				pFMCh->getNoteEx( 0, oct, note );
				skin->drawHilightKey( canvas, ltx, lty+sy+15, oct, note );
				sprintf( str, "O%d%s", oct, noteStr[note] );
				skin->drawStr( canvas, 0, ltx+5+cx*52, lty+sy+5, str );
			}
			skin->drawHBar( canvas, 290, lty+sy+15, pFMCh->getKeyOnLevelEx(0), 0 );
		}
	}else{
		skin->drawDarkKeyboard( canvas, ltx, lty+sy+15 );
		skin->drawHBar( canvas, 290, lty+sy+15, 0, 0 );
	}
}

void CVisC86Key::drawFM3EXTrackView( IVisBitmap *canvas, int ltx, int lty,
										 int trNo, bool isMute, COPNFmCh *pFMCh )
{
	int sy = 0;
	int cx=6, cy=8;
	char str[64];
	CVisC86Skin *skin = &gVisSkin;

	drawFMTrackView( canvas, ltx, lty, trNo, 2, isMute, pFMCh );
	lty+=35; trNo++;

	for( int exNo=1; exNo<4; exNo++, trNo++, lty+=35 ){
		sprintf( str, "%02d", trNo+1 );
		skin->drawNumStr1( canvas, ltx+5, lty+sy+2, str );
		sprintf( str, "FM-CH%dEX%d", 3, exNo );
		skin->drawStr( canvas, 0, ltx+5+60, lty+sy+5, str );
		skin->drawKeyboard( canvas, ltx, lty+sy+15 );

		if( pFMCh->getExMode() && !isMute ){
			int fblock = pFMCh->getFBlockEx(exNo);
			int fnum = pFMCh->getFNumEx(exNo);
			//sprintf( str, "BLK:%d  FNUM:%04d", fblock, fnum );
			sprintf( str, "B/F:%d+%04d  NOTE:", fblock, fnum );
			skin->drawStr( canvas, 0, ltx+5+cx*35, lty+sy+5, str );

			skin->drawKeyboard( canvas, ltx, lty+sy+15 );
			if( pFMCh->slot[exNo]->isOn() && pFMCh->slot[exNo]->getTotalLevel() != 127 ){
				int oct, note;
				pFMCh->getNoteEx( exNo, oct, note );
				skin->drawHilightKey( canvas, ltx, lty+sy+15, oct, note );
				sprintf( str, "O%d%s", oct, noteStr[note] );
				skin->drawStr( canvas, 0, ltx+5+cx*52, lty+sy+5, str );
			}
			skin->drawHBar( canvas, 290, lty+sy+15, pFMCh->getKeyOnLevelEx(exNo), 0 );
		}else{
			skin->drawDarkKeyboard( canvas, ltx, lty+sy+15 );
			skin->drawHBar( canvas, 290, lty+sy+15, 0, 0 );
		}
	}
}

void CVisC86Key::drawSSGTrackView( IVisBitmap *canvas, int ltx, int lty,
									int trNo, int ssgNo, bool isMute, COPNSsg *pSsg )
{
	int sy = 0;
	int cx=6, cy=8;
	char str[64];
	CVisC86Skin *skin = &gVisSkin;
	
	sprintf( str, "%02d", trNo+1 );
	skin->drawNumStr1( canvas, ltx+5, lty+sy+2, str );
	sprintf( str, "SSG-CH%d", ssgNo+1 );
	skin->drawStr( canvas, 0, ltx+5+cx*10, lty+sy+5, str );

	int period = pSsg->getNoisePeriod();
	COPNSsgCh *pSsgCh = pSsg->ch[ssgNo];
	
	if( !isMute ){
		skin->drawKeyboard( canvas, ltx, lty+sy+15 );

		skin->drawStr( canvas, 0, ltx+5+cx*24, lty+sy+5, "NOISE:" );
		skin->drawStr( canvas, 0, ltx+5+cx*35, lty+sy+5, "TONE:" );
		skin->drawStr( canvas, 0, ltx+5+cx*47, lty+sy+5, "NOTE:" );
		
		if( pSsgCh->isNoiseOn() ){
			sprintf( str, "%04d", period );
			skin->drawStr( canvas, 0, ltx+5+cx*30, lty+sy+5, str );
		}
		if( pSsgCh->isToneOn() ){
			int tune = pSsgCh->getTune();
			sprintf( str, "%04d", tune );
			skin->drawStr( canvas, 0, ltx+5+cx*40, lty+sy+5, str );
		}
		if( pSsgCh->isOn() && pSsgCh->getLevel()!=0 ){
			int oct, note;
			if( pSsgCh->isToneOn() )
				pSsgCh->getNote( oct, note );
			else
				pSsg->getNoiseNote( oct, note );
			
			skin->drawHilightKey( canvas, ltx, lty+sy+15, oct, note );
			sprintf( str, "O%d%s", oct, noteStr[note] );
			skin->drawStr( canvas, 0, ltx+5+cx*52, lty+sy+5, str );
		}
		skin->drawHBar( canvas, 290, lty+sy+15, pSsgCh->getKeyOnLevel(), 0 );
	}else{
		skin->drawDarkKeyboard( canvas, ltx, lty+sy+15 );
		skin->drawHBar( canvas, 290, lty+sy+15, 0, 0 );
	}
}

void CVisC86OPNAKey::drawADPCMTrackView( IVisBitmap *canvas, int ltx, int lty, int trNo )
{
	int sy = 0;
	char str[64];
	CVisC86Skin *skin = &gVisSkin;
	//const int cx=6;

	COPNAAdpcm *adpcm = pOPNA->adpcm;
	sprintf( str, "%02d", trNo+1 );
	skin->drawNumStr1( canvas, ltx+5, lty+sy+2, str );
	sprintf( str, "ADPCM" );
	skin->drawStr( canvas, 0, ltx+5+60, lty+sy+5, str );

	UINT col_mid = skin->getPal(CVisC86Skin::IDCOL_MID);
	UINT col_light = skin->getPal(CVisC86Skin::IDCOL_KEYLIGHT);

	visDrawLine( canvas, ltx    , lty+sy+5+10, ltx+280, lty+sy+5+10, col_mid );
	visDrawLine( canvas, ltx+280, lty+sy+5+10, ltx+280, lty+sy+5+30, col_mid );
	visDrawLine( canvas, ltx+280, lty+sy+5+30, ltx    , lty+sy+5+30, col_mid );
	visDrawLine( canvas, ltx    , lty+sy+5+30, ltx    , lty+sy+5+10, col_mid );

	UINT stAddr = adpcm->getStartAddr()*280/256/1024;
	UINT edAddr = adpcm->getStopAddr()*280/256/1024;
	for( int i=0; i<280; i++ ){
		if(adpcm->minimap[i*512/280]&0x1){
			visDrawLine( canvas, ltx+i, lty+sy+5+10, ltx+i, lty+sy+5+30, col_mid);
		}
		if( adpcm->isOn() ){
			if( stAddr <= (UINT)i && (UINT)i <= edAddr )
				visDrawLine( canvas, ltx+i, lty+sy+5+10, ltx+i, lty+sy+5+30, col_light);
		}
	}
	//sprintf( str, "ST:%02x ED:%02x", stAddr, edAddr);
	//skin->drawStr( canvas, 0, ltx+5+60+cx*10, lty+sy+5, str );

	if( !pOPNA->getMixedMask(trNo) ){
		//skin->drawKeyboard( canvas, ltx, lty+sy+15 );
		skin->drawHBar( canvas, 290, lty+sy+15, adpcm->getKeyOnLevel(), 0 );
	}else{
		//skin->drawDarkKeyboard( canvas, ltx, lty+sy+15 );
		skin->drawHBar( canvas, 290, lty+sy+15, 0, 0 );
	}
	
}

void CVisC86Key::drawRhythmTrackView( IVisBitmap *canvas, int ltx, int lty,
										  int trNo, bool isMute, COPNRhythm *rhythm )
{
	int sy = 0;
	int cx=6, cy=8;
	int x1, y1, l;
	char str[64];
	CVisC86Skin *skin = &gVisSkin;

	UINT col_mid = skin->getPal(CVisC86Skin::IDCOL_MID);
	UINT col_light = skin->getPal(CVisC86Skin::IDCOL_LIGHT);
	
	sprintf( str, "%02d", trNo+1 );
	skin->drawNumStr1( canvas, ltx+5, lty+sy+2, str );
	sprintf( str, "RHYTHM" );
	skin->drawStr( canvas, 0, ltx+5+60, lty+sy+5, str );

	visDrawLine( canvas, ltx    , lty+sy+5+10, ltx+280, lty+sy+5+10, col_mid );
	visDrawLine( canvas, ltx+280, lty+sy+5+10, ltx+280, lty+sy+5+30, col_mid );
	visDrawLine( canvas, ltx+280, lty+sy+5+30, ltx    , lty+sy+5+30, col_mid );
	visDrawLine( canvas, ltx    , lty+sy+5+30, ltx    , lty+sy+5+10, col_mid );

	if( !isMute ){
		skin->drawStr( canvas, 0, ltx+5+cx*3, lty+sy+5+18, "RIM" );
		if( rhythm->rim->isOn() ||1){
			l=rhythm->rim->getKeyOnLevel()/2;
			x1=ltx+5+cx*6+3; y1=lty+sy+16+16-l;
			visFillRect( canvas,x1, y1, 8, l, col_light );
		}
		skin->drawStr( canvas, 0, ltx+5+cx*10, lty+sy+5+18, "TOM" );
		if( rhythm->tom->isOn() ){
			l=rhythm->tom->getKeyOnLevel()/2;
			x1=ltx+5+cx*13+3; y1=lty+sy+16+16-l;
			visFillRect( canvas,x1, y1, 8, l, col_light );
		}
		skin->drawStr( canvas, 0, ltx+5+cx*17, lty+sy+5+18, "HH" );
		if( rhythm->hh->isOn() ){
			l=rhythm->hh->getKeyOnLevel()/2;
			x1=ltx+5+cx*19+3; y1=lty+sy+16+16-l;
			visFillRect( canvas,x1, y1, 8, l, col_light );
		}
		skin->drawStr( canvas, 0, ltx+5+cx*24, lty+sy+5+18, "TOP" );
		if( rhythm->top->isOn() ){
			l=rhythm->top->getKeyOnLevel()/2;
			x1=ltx+5+cx*27+3; y1=lty+sy+16+16-l;
			visFillRect( canvas,x1, y1, 8, l, col_light );
		}
		skin->drawStr( canvas, 0, ltx+5+cx*31, lty+sy+5+18, "SD" );
		if( rhythm->sd->isOn() ){
			l=rhythm->sd->getKeyOnLevel()/2;
			x1=ltx+5+cx*33+3; y1=lty+sy+16+16-l;
			visFillRect( canvas,x1, y1, 8, l, col_light );
		}
		skin->drawStr( canvas, 0, ltx+5+cx*38, lty+sy+5+18, "BD");
		if( rhythm->bd->isOn() ){
			l=rhythm->bd->getKeyOnLevel()/2;
			x1=ltx+5+cx*40+3; y1=lty+sy+16+16-l;
			visFillRect( canvas,x1, y1, 8, l, col_light );
		}
		
		skin->drawHBar( canvas, 290, lty+sy+16, rhythm->getKeyOnLevel(), 0 );
	}else{
		skin->drawHBar( canvas, 290, lty+sy+16, 0, 0 );
	}
}

// --------------------------------------------------------
bool CVisC86OPNAKey::create( HWND parent )
{
	if( !CVisWnd::create( _windowWidth, _windowHeight,
		WS_EX_TOOLWINDOW, (WS_POPUP | WS_CLIPCHILDREN), parent ) )
		return false;

	::ShowWindow( hWnd, SW_SHOWNOACTIVATE );

	for( int i=0; i<14; i++ ){
		if( 3<=i && i<=5 ) continue;
		muteSw[i] = CVisMuteSwPtr( new CVisMuteSw( this, 5+28, 7+i*35 ) );
		muteSw[i]->getter = [this, i]()-> int { return this->pOPNA->getPartMask(i);};
		muteSw[i]->setter = [this, i](int mask) { this->pOPNA->setPartMask(i, mask ? true : false); };
		widgets.push_back(muteSw[i]);

		soloSw[i] = CVisSoloSwPtr( new CVisSoloSw( this, 5+28+16, 7+i*35 ) );
		soloSw[i]->getter = [this, i]()-> int { return this->pOPNA->getPartSolo(i);};
		soloSw[i]->setter = [this, i](int mask) { this->pOPNA->setPartSolo(i, mask ? true : false); };
		widgets.push_back(soloSw[i]);
	}
	
	
	return true;
}

void CVisC86OPNAKey::onPaintClient(void)
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );
	
	if( pOPNA ){
		int sx=5, sy=5, cx=6, cy=8;
		int tr=0;
		for( int i=0; i<2; i++ ){
			drawFMTrackView( clientCanvas, sx, sy, tr, i,
							 pOPNA->getMixedMask(tr), pOPNA->fm->ch[i] );
			sy+=35; tr++;
		}
		
		drawFM3EXTrackView( clientCanvas, sx, sy, tr,
							pOPNA->getMixedMask(tr), pOPNA->fm->ch[2] );//, 1+i );
		sy+=35*4; tr+=4;
		
		for( int i=3; i<6; i++ ){
			drawFMTrackView( clientCanvas, sx, sy, tr, i, 
							 pOPNA->getMixedMask(tr), pOPNA->fm->ch[i] );
			sy+=35; tr++;
		}
		for( int i=0; i<3; i++ ){
			drawSSGTrackView( clientCanvas, sx, sy, tr, i,
							  pOPNA->getMixedMask(tr), pOPNA->ssg );
			sy+=35; tr++;
		}
		drawADPCMTrackView( clientCanvas, sx, sy, tr );
		sy+=35; tr++;
		drawRhythmTrackView( clientCanvas, sx, sy, tr,
							 pOPNA->getMixedMask(tr), pOPNA->rhythm );
	}
}


void CVisC86OPNAKey::onKeyDown(DWORD keycode)
{
	bool sw;
	if( ::GetAsyncKeyState(VK_SHIFT) < 0 ){
		switch(keycode){
		case '1': //FM1
			sw = pOPNA->getPartSolo(0);
			pOPNA->setPartSolo(0, sw?false:true);
			break;
		case '2': //FM2
			sw = pOPNA->getPartSolo(1);
			pOPNA->setPartSolo(1, sw?false:true);
			break;
		case '3': //FM3
			sw = pOPNA->getPartSolo(2);
			pOPNA->setPartSolo(2, sw?false:true);
			break;
		case '7': //FM4
			sw = pOPNA->getPartSolo(6);
			pOPNA->setPartSolo(6, sw?false:true);
			break;
		case '8': //FM5
			sw = pOPNA->getPartSolo(7);
			pOPNA->setPartSolo(7, sw?false:true);
			break;
		case '9': //FM6
			sw = pOPNA->getPartSolo(8);
			pOPNA->setPartSolo(8, sw?false:true);
			break;
		case 'Q': //SSG1
			sw = pOPNA->getPartSolo(9);
			pOPNA->setPartSolo(9, sw?false:true);
			break;
		case 'W': //SSG2
			sw = pOPNA->getPartSolo(10);
			pOPNA->setPartSolo(10, sw?false:true);
			break;
		case 'E': //SSG3
			sw = pOPNA->getPartSolo(11);
			pOPNA->setPartSolo(11, sw?false:true);
			break;
		case 'R': //ADPCM
			sw = pOPNA->getPartSolo(12);
			pOPNA->setPartSolo(12, sw?false:true);
			break;
		case 'T': //RYTHM
			sw = pOPNA->getPartSolo(13);
			pOPNA->setPartSolo(13, sw?false:true);
			break;
		case '0': //全クリア
			for( int i=0; i<14; i++ )
				pOPNA->setPartSolo(i, false);
			break;
		}
	}else{
		switch(keycode){
		case '1': //FM1
			sw = pOPNA->getPartMask(0);
			pOPNA->setPartMask(0, sw?false:true);
			break;
		case '2': //FM2
			sw = pOPNA->getPartMask(1);
			pOPNA->setPartMask(1, sw?false:true);
			break;
		case '3': //FM3
			sw = pOPNA->getPartMask(2);
			pOPNA->setPartMask(2, sw?false:true);
			break;
		// FM3Ex0~3は未対応
		//	case '4':
		//		sw = pOPNA->getPartMask(3);
		//		pOPNA->setPartMask(3, sw?false:true);
		//		break;
		//	case '5':
		//		sw = pOPNA->getPartMask(4);
		//		pOPNA->setPartMask(4, sw?false:true);
		//		break;
		//	case '6':
		//		sw = pOPNA->getPartMask(5);
		//		pOPNA->setPartMask(5, sw?false:true);
		//		break;
		case '7': //FM4
			sw = pOPNA->getPartMask(6);
			pOPNA->setPartMask(6, sw?false:true);
			break;
		case '8': //FM5
			sw = pOPNA->getPartMask(7);
			pOPNA->setPartMask(7, sw?false:true);
			break;
		case '9': //FM6
			sw = pOPNA->getPartMask(8);
			pOPNA->setPartMask(8, sw?false:true);
			break;
		case 'Q': //SSG1
			sw = pOPNA->getPartMask(9);
			pOPNA->setPartMask(9, sw?false:true);
			break;
		case 'W': //SSG2
			sw = pOPNA->getPartMask(10);
			pOPNA->setPartMask(10, sw?false:true);
			break;
		case 'E': //SSG3
			sw = pOPNA->getPartMask(11);
			pOPNA->setPartMask(11, sw?false:true);
			break;
		case 'R': //ADPCM
			sw = pOPNA->getPartMask(12);
			pOPNA->setPartMask(12, sw?false:true);
			break;
		case 'T': //RYTHM
			sw = pOPNA->getPartMask(13);
			pOPNA->setPartMask(13, sw?false:true);
			break;
		case '0': //全クリア
			for( int i=0; i<14; i++ )
				pOPNA->setPartMask(i, false);
			break;
		}
	}
}


// --------------------------------------------------------
bool CVisC86OPN3LKey::create( HWND parent )
{
	if( !CVisWnd::create( _windowWidth, _windowHeight,
		WS_EX_TOOLWINDOW, (WS_POPUP | WS_CLIPCHILDREN), parent ) )
		return false;

	::ShowWindow( hWnd, SW_SHOWNOACTIVATE );

	for( int i=0; i<13; i++ ){
		if( 3<=i && i<=5 ) continue;
		muteSw[i] = CVisMuteSwPtr( new CVisMuteSw( this, 5+28, 7+i*35 ) );
		muteSw[i]->getter = [this, i]()-> int { return this->pOPN3L->getPartMask(i);};
		muteSw[i]->setter = [this, i](int mask) { this->pOPN3L->setPartMask(i, mask ? true : false); };
		widgets.push_back(muteSw[i]);

		soloSw[i] = CVisSoloSwPtr( new CVisSoloSw( this, 5+28+16, 7+i*35 ) );
		soloSw[i]->getter = [this, i]()-> int { return this->pOPN3L->getPartSolo(i);};
		soloSw[i]->setter = [this, i](int mask) { this->pOPN3L->setPartSolo(i, mask ? true : false); };
		widgets.push_back(soloSw[i]);
	}
	
	
	return true;
}

void CVisC86OPN3LKey::onPaintClient(void)
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );
	
	if( pOPN3L ){
		int sx=5, sy=5, cx=6, cy=8;
		int tr=0;
		for( int i=0; i<2; i++ ){
			drawFMTrackView( clientCanvas, sx, sy, tr, i,
							 pOPN3L->getMixedMask(tr), pOPN3L->fm->ch[i] );
			sy+=35; tr++;
		}
		
		drawFM3EXTrackView( clientCanvas, sx, sy, tr,
							pOPN3L->getMixedMask(tr), pOPN3L->fm->ch[2] );//, 1+i );
		sy+=35*4; tr+=4;
		
		for( int i=3; i<6; i++ ){
			drawFMTrackView( clientCanvas, sx, sy, tr, i, 
							 pOPN3L->getMixedMask(tr), pOPN3L->fm->ch[i] );
			sy+=35; tr++;
		}
		for( int i=0; i<3; i++ ){
			drawSSGTrackView( clientCanvas, sx, sy, tr, i,
							  pOPN3L->getMixedMask(tr), pOPN3L->ssg );
			sy+=35; tr++;
		}
		drawRhythmTrackView( clientCanvas, sx, sy, tr,
							 pOPN3L->getMixedMask(tr), pOPN3L->rhythm );
	}
}

void CVisC86OPN3LKey::onKeyDown(DWORD keycode)
{
	bool sw;
	if( ::GetAsyncKeyState(VK_SHIFT) < 0 ){
		switch(keycode){
		case '1': //FM1
			sw = pOPN3L->getPartSolo(0);
			pOPN3L->setPartSolo(0, sw?false:true);
			break;
		case '2': //FM2
			sw = pOPN3L->getPartSolo(1);
			pOPN3L->setPartSolo(1, sw?false:true);
			break;
		case '3': //FM3
			sw = pOPN3L->getPartSolo(2);
			pOPN3L->setPartSolo(2, sw?false:true);
			break;
		case '7': //FM4
			sw = pOPN3L->getPartSolo(6);
			pOPN3L->setPartSolo(6, sw?false:true);
			break;
		case '8': //FM5
			sw = pOPN3L->getPartSolo(7);
			pOPN3L->setPartSolo(7, sw?false:true);
			break;
		case '9': //FM6
			sw = pOPN3L->getPartSolo(8);
			pOPN3L->setPartSolo(8, sw?false:true);
			break;
		case 'Q': //SSG1
			sw = pOPN3L->getPartSolo(9);
			pOPN3L->setPartSolo(9, sw?false:true);
			break;
		case 'W': //SSG2
			sw = pOPN3L->getPartSolo(10);
			pOPN3L->setPartSolo(10, sw?false:true);
			break;
		case 'E': //SSG3
			sw = pOPN3L->getPartSolo(11);
			pOPN3L->setPartSolo(11, sw?false:true);
			break;
		case 'R': //RYTHM
			sw = pOPN3L->getPartSolo(12);
			pOPN3L->setPartSolo(12, sw?false:true);
			break;
		case '0': //全クリア
			for( int i=0; i<13; i++ )
				pOPN3L->setPartSolo(i, false);
			break;
		}
	}else{
		switch(keycode){
		case '1': //FM1
			sw = pOPN3L->getPartMask(0);
			pOPN3L->setPartMask(0, sw?false:true);
			break;
		case '2': //FM2
			sw = pOPN3L->getPartMask(1);
			pOPN3L->setPartMask(1, sw?false:true);
			break;
		case '3': //FM3
			sw = pOPN3L->getPartMask(2);
			pOPN3L->setPartMask(2, sw?false:true);
			break;
		// FM3Ex0~3は未対応
		//	case '4':
		//		sw = pOPN3L->getPartMask(3);
		//		pOPN3L->setPartMask(3, sw?false:true);
		//		break;
		//	case '5':
		//		sw = pOPN3L->getPartMask(4);
		//		pOPN3L->setPartMask(4, sw?false:true);
		//		break;
		//	case '6':
		//		sw = pOPN3L->getPartMask(5);
		//		pOPN3L->setPartMask(5, sw?false:true);
		//		break;
		case '7': //FM4
			sw = pOPN3L->getPartMask(6);
			pOPN3L->setPartMask(6, sw?false:true);
			break;
		case '8': //FM5
			sw = pOPN3L->getPartMask(7);
			pOPN3L->setPartMask(7, sw?false:true);
			break;
		case '9': //FM6
			sw = pOPN3L->getPartMask(8);
			pOPN3L->setPartMask(8, sw?false:true);
			break;
		case 'Q': //SSG1
			sw = pOPN3L->getPartMask(9);
			pOPN3L->setPartMask(9, sw?false:true);
			break;
		case 'W': //SSG2
			sw = pOPN3L->getPartMask(10);
			pOPN3L->setPartMask(10, sw?false:true);
			break;
		case 'E': //SSG3
			sw = pOPN3L->getPartMask(11);
			pOPN3L->setPartMask(11, sw?false:true);
			break;
		case 'R': //RYTHM
			sw = pOPN3L->getPartMask(12);
			pOPN3L->setPartMask(12, sw?false:true);
			break;
		case '0': //全クリア
			for( int i=0; i<13; i++ )
				pOPN3L->setPartMask(i, false);
			break;
		}
	}
}


// --------------------------------------------------------
bool CVisC86OPMKey::create( HWND parent )
{
	if( !CVisWnd::create( _windowWidth, _windowHeight,
		WS_EX_TOOLWINDOW, (WS_POPUP | WS_CLIPCHILDREN), parent ) )
		return false;

	::ShowWindow( hWnd, SW_SHOWNOACTIVATE );

	for( int i=0; i<8; i++ ){
		muteSw[i] = CVisMuteSwPtr( new CVisMuteSw( this, 5+28, 7+i*35 ) );
		muteSw[i]->getter = [this, i]()-> int { return this->pOPM->getPartMask(i);};
		muteSw[i]->setter = [this, i](int mask) { this->pOPM->setPartMask(i, mask ? true : false); };
		widgets.push_back(muteSw[i]);

		soloSw[i] = CVisSoloSwPtr( new CVisSoloSw( this, 5+28+16, 7+i*35 ) );
		soloSw[i]->getter = [this, i]()-> int { return this->pOPM->getPartSolo(i);};
		soloSw[i]->setter = [this, i](int mask) { this->pOPM->setPartSolo(i, mask ? true : false); };
		widgets.push_back(soloSw[i]);
	}
	
	
	return true;
}

void CVisC86OPMKey::onPaintClient(void)
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );
	
	if( pOPM ){
		int sx=5, sy=5, cx=6, cy=8;
		int tr=0;
		for( int i=0; i<8; i++ ){
			drawFMTrackView( clientCanvas, sx, sy, tr, i,
							 pOPM->getMixedMask(tr), pOPM->fm->ch[i] );
			sy+=35; tr++;
		}
	}
}

void CVisC86OPMKey::drawFMTrackView( IVisBitmap *canvas, int ltx, int lty,
								  int trNo, int fmNo, bool isMute, COPMFmCh *pFMCh )
{
	int sy = 0;
	char str[64], tmp[64];
	int cx=6, cy=8;
	CVisC86Skin *skin = &gVisSkin;

	sprintf( str, "%02d", trNo+1 );
	skin->drawNumStr1( canvas, ltx+5, lty+sy+2, str );
	sprintf( str, "FM-CH%d", fmNo+1 );
	skin->drawStr( canvas, 0, ltx+5+cx*10, lty+sy+5, str );

	if( !isMute ){
		strcpy(str, "SLOT:");
		for( int i=0; i<4; i++ ){
			int sw = pFMCh->slot[i]->isOn();
			sprintf( tmp, "%d", i );
			if( sw ) strncat( str, tmp, 64 );
			else  strncat( str, "_", 64 );
		}
		skin->drawStr( canvas, 0, ltx+5+cx*18, lty+sy+5, str );

		int kcoct = pFMCh->getKeyCodeOct();
		int kcnote = pFMCh->getKeyCodeNote();
		int kf = pFMCh->getKeyFraction();
		sprintf( str, "KC:%02d-%02d KF:%02d     NOTE:", kcoct, kcnote, kf );
		skin->drawStr( canvas, 0, ltx+5+cx*28, lty+sy+5, str );
		skin->drawKeyboard( canvas, ltx, lty+sy+15 );

		if( pFMCh->isKeyOn() && pFMCh->getMixLevel()!=127 ){
			int oct=0, note=0;
			pFMCh->getNote( oct, note );
			skin->drawHilightKey( canvas, ltx, lty+sy+15, oct, note );
			sprintf( str, "O%d%s", oct, noteStr[note] );
			skin->drawStr( canvas, 0, ltx+5+cx*52, lty+sy+5, str );
		}
		skin->drawHBar( canvas, 290, lty+sy+15, pFMCh->getKeyOnLevel(), 0 );
	}else{
		skin->drawDarkKeyboard( canvas, ltx, lty+sy+15 );
		skin->drawHBar( canvas, 290, lty+sy+15, 0, 0 );
	}
}

void CVisC86OPMKey::onKeyDown(DWORD keycode)
{
	bool sw;
	if( ::GetAsyncKeyState(VK_SHIFT) < 0 ){
		switch(keycode){
		case '1': //FM1
			sw = pOPM->getPartSolo(0);
			pOPM->setPartSolo(0, sw?false:true);
			break;
		case '2': //FM2
			sw = pOPM->getPartSolo(1);
			pOPM->setPartSolo(1, sw?false:true);
			break;
		case '3': //FM3
			sw = pOPM->getPartSolo(2);
			pOPM->setPartSolo(2, sw?false:true);
			break;
		case '4': //FM4
			sw = pOPM->getPartSolo(3);
			pOPM->setPartSolo(3, sw?false:true);
			break;
		case '5': //FM5
			sw = pOPM->getPartSolo(4);
			pOPM->setPartSolo(4, sw?false:true);
			break;
		case '6': //FM6
			sw = pOPM->getPartSolo(5);
			pOPM->setPartSolo(5, sw?false:true);
			break;
		case '7': //FM7
			sw = pOPM->getPartSolo(6);
			pOPM->setPartSolo(6, sw?false:true);
			break;
		case '8': //FM8
			sw = pOPM->getPartSolo(7);
			pOPM->setPartSolo(7, sw?false:true);
			break;
		case '0': //全クリア
			for( int i=0; i<8; i++ )
				pOPM->setPartSolo(i, false);
			break;
		}
	}else{
		switch(keycode){
		case '1': //FM1
			sw = pOPM->getPartMask(0);
			pOPM->setPartMask(0, sw?false:true);
			break;
		case '2': //FM2
			sw = pOPM->getPartMask(1);
			pOPM->setPartMask(1, sw?false:true);
			break;
		case '3': //FM3
			sw = pOPM->getPartMask(2);
			pOPM->setPartMask(2, sw?false:true);
			break;
		case '4': //FM4
			sw = pOPM->getPartMask(3);
			pOPM->setPartMask(3, sw?false:true);
			break;
		case '5': //FM5
			sw = pOPM->getPartMask(4);
			pOPM->setPartMask(4, sw?false:true);
			break;
		case '6': //FM6
			sw = pOPM->getPartMask(5);
			pOPM->setPartMask(5, sw?false:true);
			break;
		case '7': //FM7
			sw = pOPM->getPartMask(6);
			pOPM->setPartMask(6, sw?false:true);
			break;
		case '8': //FM8
			sw = pOPM->getPartMask(7);
			pOPM->setPartMask(7, sw?false:true);
			break;
		case '0': //全クリア
			for( int i=0; i<8; i++ )
				pOPM->setPartMask(i, false);
			break;
		}
	}
}


// --------------------------------------------------------
CVisC86KeyPtr c86ctl::vis::visC86KeyViewFactory(Chip *pchip, int id)
{
	if (!pchip) return 0;
	if( typeid(*pchip) == typeid(COPNA) ){
		return CVisC86KeyPtr( new CVisC86OPNAKey(dynamic_cast<COPNA*>(pchip), id ) );
	}else if( typeid(*pchip) == typeid(COPN3L) ){
		return CVisC86KeyPtr( new CVisC86OPN3LKey(dynamic_cast<COPN3L*>(pchip), id) );
	}else if( typeid(*pchip) == typeid(COPM) ){
		return CVisC86KeyPtr( new CVisC86OPMKey(dynamic_cast<COPM*>(pchip), id) );
	}
	return 0;
}
