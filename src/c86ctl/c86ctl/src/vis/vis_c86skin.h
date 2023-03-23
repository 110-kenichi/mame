/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#pragma once

#include "vis_bitmap.h"
#include <vector>

//#define RGBtoBGRA(x) 
#define ARGB(a,r,g,b)  ( (UINT)(b) | (UINT)(g) << 8 | (UINT)(r) << 16 | (UINT)(a) << 24 )

namespace c86ctl{
namespace vis {

class CVisC86Skin
{
public:
	CVisC86Skin();
	~CVisC86Skin();

public:
	void init(void);
	void deinit(void);
	
	void drawStr( IVisBitmap *canvas, int type, int x, int y, const char *str );
	void drawVStr( IVisBitmap *canvas, int type, int x, int y, const char *str );
	void drawNanoNumStr( IVisBitmap *canvas, int x, int y, const char *str );
	void drawNumStr1( IVisBitmap *canvas, int x, int y, const char *str );
	void drawFrame( IVisBitmap *canvas, int type, CHAR *title );
	void drawCloseButton( IVisBitmap *canvas, int type, int x, int y );
	
	void drawKeyboard( IVisBitmap *canvas, int x, int y );
	void drawDarkKeyboard( IVisBitmap *canvas, int x, int y );
	void drawHilightKey( IVisBitmap *canvas, int x, int y, int oct, int note );
	void drawTickCircle( IVisBitmap *canvas, int x, int y, int tick );
	void drawLogo(IVisBitmap *canvas, int x, int y );
	void drawDipSw(IVisBitmap *canvas, int x, int y, int sw );
	void drawCheckBox(IVisBitmap *canvas, int x, int y, int sw );
	void drawKnob(IVisBitmap *canvas, int x, int y, int minval, int maxval, int numval );
	void drawMuteSw(IVisBitmap *canvas, int x, int y, int sw );
	void drawSoloSw(IVisBitmap *canvas, int x, int y, int sw );
	
	void drawSSGEG(IVisBitmap *canvas, int x, int y, int no );
	void drawFMAlgorithm(IVisBitmap *canvas, int x, int y, int no );
	void drawFMSkin(IVisBitmap *canvas, int x, int y, int exmode );
	void drawFMSlotSkin(IVisBitmap *canvas, int x, int y );
	void drawFMSlotLight(IVisBitmap *canvas, int x, int y, int no, int sw );
	void drawHBar( IVisBitmap *bmp, int xs, int ys, int level, int peak );

	enum  PalletID{
		IDCOL_HIGH = 0,
		IDCOL_MID = 1,
		IDCOL_SHADOW = 2,
		IDCOL_LIGHT = 3,
		IDCOL_KEYLIGHT = 4
	};

	UINT getPal( enum PalletID id ){
		return pallet[id];
	};
	UINT getColTbl( int idx ){
		return coltbl[idx];
	}
	
protected:
	CVisBitmap *skinbmp;
	std::vector<UINT> pallet;
	std::vector<UINT> coltbl;
};

};
};

extern c86ctl::vis::CVisC86Skin gVisSkin;
