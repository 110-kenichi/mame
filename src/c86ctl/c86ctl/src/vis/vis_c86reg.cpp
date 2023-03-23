/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "stdafx.h"
#include <stdlib.h>
#include <stdio.h>
#include "vis_c86reg.h"
#include "vis_c86sub.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;
using namespace c86ctl::vis;

// --------------------------------------------------------
void CVisC86Reg::drawRegView( IVisBitmap *canvas, int ltx, int lty,
							  const UCHAR *regval, const UCHAR *regatime )
{
	const int dc = 8;
	CHAR str[64];
	CVisC86Skin *skin = &gVisSkin;

	int ox = 0;
	int oy = 0;
	int cx=6, cy=8;
	for( int x=0; x<16; x++ ){
		sprintf( str, "+%X", x );
		int sx = ltx+ox+(cx*2+4)*(x+1);
		int sy = lty+oy;

		skin->drawStr( canvas, 1, sx, sy, str );
	}
	for( int y=0; y<16; y++ ){
		sprintf( str, "%02X", y*16 );
		int sx = ltx+ox;
		int sy = lty+oy+cy*(y+1);
		skin->drawStr( canvas, 1, sx, sy, str );
	}
	
	ox = cx*2+4;
	oy = cy;
	for( int y=0; y<16; y++ ){
		for( int x=0; x<16; x++ ){
			int a = y*16+x;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             
			int v = regval[a];
			int c = regatime[a];

			sprintf( str,"%02X", v );
			int sx = ltx+ox+(cx*2+4)*x;
			int sy = lty+oy+cy*y;
			
			visFillRect( canvas, sx, sy, cx*2, cy-1, skin->getColTbl(c) );
			skin->drawStr( canvas, 2, sx, sy, str );
		}
	}
}

bool CVisC86Reg::create( HWND parent )
{
	if( !CVisWnd::create( _windowWidth, _windowHeight,
		WS_EX_TOOLWINDOW, (WS_POPUP | WS_CLIPCHILDREN), parent ) )
		return false;

	::ShowWindow( hWnd, SW_SHOWNOACTIVATE );
	
	return true;
}

void CVisC86Reg::close()
{
	CVisWnd::close();
}

// --------------------------------------------------------
void CVisC86OPNAReg::onPaintClient()
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );

	if( pOPNA ){
		int sx=5, sy=5, cx=6, cy=8;
		gVisSkin.drawStr( clientCanvas, 1, sx, sy, "REGISTER BANK0 ------------------------------" );
		drawRegView( clientCanvas, sx, sy+cy* 1, pOPNA->reg[0], pOPNA->regATime[0] );
		gVisSkin.drawStr( clientCanvas, 1, sx, sy+cy*20, "REGISTER BANK1 ------------------------------" );
		drawRegView( clientCanvas, sx, sy+cy*21, pOPNA->reg[1], pOPNA->regATime[1] );
	}
}

void CVisC86OPN3LReg::onPaintClient()
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );

	if( pOPN3L ){
		int sx=5, sy=5, cx=6, cy=8;
		gVisSkin.drawStr( clientCanvas, 1, sx, sy, "REGISTER BANK0 ------------------------------" );
		drawRegView( clientCanvas, sx, sy+cy* 1, pOPN3L->reg[0], pOPN3L->regATime[0] );
		gVisSkin.drawStr( clientCanvas, 1, sx, sy+cy*20, "REGISTER BANK1 ------------------------------" );
		drawRegView( clientCanvas, sx, sy+cy*21, pOPN3L->reg[1], pOPN3L->regATime[1] );
	}
}

void CVisC86OPMReg::onPaintClient()
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );

	if( pOPM ){
		int sx=5, sy=5, cx=6, cy=8;
		gVisSkin.drawStr( clientCanvas, 1, sx, sy, "REGISTER BANK0 ------------------------------" );
		drawRegView( clientCanvas, sx, sy+cy* 1, pOPM->reg, pOPM->regATime );
	}
}

void CVisC86OPL3Reg::onPaintClient()
{
	visFillRect( clientCanvas, 0, 0, clientCanvas->getWidth(), clientCanvas->getHeight(), ARGB(255,0,0,0) );

	if( pOPL3 ){
		int sx=5, sy=5, cx=6, cy=8;
		gVisSkin.drawStr( clientCanvas, 1, sx, sy, "REGISTER BANK0 ------------------------------" );
		drawRegView( clientCanvas, sx, sy+cy* 1, pOPL3->reg[0], pOPL3->regATime[0] );
		gVisSkin.drawStr( clientCanvas, 1, sx, sy+cy*20, "REGISTER BANK1 ------------------------------" );
		drawRegView( clientCanvas, sx, sy+cy*21, pOPL3->reg[1], pOPL3->regATime[1] );
	}
}

// --------------------------------------------------------
CVisC86RegPtr c86ctl::vis::visC86RegViewFactory(Chip *pchip, int id)
{
	if (!pchip) return 0;
	if( typeid(*pchip) == typeid(COPNA) ){
		return CVisC86RegPtr( new CVisC86OPNAReg(dynamic_cast<COPNA*>(pchip), id ) );
	}else if( typeid(*pchip) == typeid(COPN3L) ){
		return CVisC86RegPtr( new CVisC86OPN3LReg(dynamic_cast<COPN3L*>(pchip), id) );
	}else if( typeid(*pchip) == typeid(COPM) ){
		return CVisC86RegPtr( new CVisC86OPMReg(dynamic_cast<COPM*>(pchip), id) );
	}else if( typeid(*pchip) == typeid(COPL3) ){
		return CVisC86RegPtr( new CVisC86OPL3Reg(dynamic_cast<COPL3*>(pchip), id) );
	}
	return 0;
}

