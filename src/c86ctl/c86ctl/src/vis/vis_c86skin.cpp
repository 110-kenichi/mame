/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#include "stdafx.h"
#include "resource.h"
#include "c86ctlmain.h"
#include "vis_c86skin.h"
#include "vis_c86sub.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;
using namespace c86ctl::vis;

static HBITMAP hSkinBMP = NULL;
static HDC hSkinDC = NULL;

struct BMPREG{
	INT left;
	INT top;
	INT width;
	INT height;
};

const BMPREG skinreg_frame[] = {
	{98,307,8,15},		// window frame left
	{233,307,8,15},		// window frame right
	{106,307,129,15},	// window frame center
	{242,307,15,15},	// window frame close button
	{98,291,8,15},		// window frame left (LIGHT)
	{233,291,8,15},		// window frame right (LIGHT)
	{106,291,129,15},	// window frame center (LIGHT)
	{242,291,15,15}		// window frame close button (LIGHT)
};
const BMPREG skinreg_fonts[] = {
	{0,0,6,8},			// 0:font1
	{97,0,6,8},			// 1:font2
	{194,0,6,8},		// 2:font3
	{0,143,6,8},		// 3:vfont1
	{48,143,6,8},		// 4:vfont2
	{96,143,6,8},		// 5:vfont3
	{144,67,8,11},		// 6:num font1 (mini)
	{144,48,10,18},		// 7:num font2 (big)
	{144,79,5,5},		// 8:num font3 (nano)
	{144,85,5,5},		// 9:num font3 (nano)
	{144,91,5,5},		// 10:num font3 (nano)
};
const BMPREG skinreg_alg[] = {
	{  1,323,94,62},	// ALG0
	{ 96,323,94,62},	// ALG1
	{191,323,94,62},	// ALG2
	{  1,386,94,62},	// ALG3
	{ 96,386,94,62},	// ALG4
	{191,386,94,62},	// ALG5
	{  1,449,94,62},	// ALG6
	{ 96,449,94,62},	// ALG7
};
const BMPREG skinreg_ssgeg[] = {
	{ 45,231,52,11},	// SSG EG0
	{ 45,241,52,11},	// SSG EG0
	{ 45,251,52,11},	// SSG EG1
	{ 45,261,52,11},	// SSG EG2
	{ 45,271,52,11},	// SSG EG3
	{ 45,281,52,11},	// SSG EG4
	{ 45,291,52,11},	// SSG EG5
	{ 45,301,52,11},	// SSG EG6
	{ 45,311,52,11},	// SSG EG7
};
const BMPREG skinreg_tick[] = {
	{0,145,31,31},		// rot none
	{32,145,31,31},		// rot0
	{64,145,31,31},		// rot1
	{96,145,31,31},		// rot2
	{128,145,31,31},	// rot3
	{160,145,31,31},	// rot4
	{192,145,31,31},	// rot5
	{224,145,31,31},	// rot6
	{256,145,31,31}		// rot7
};

#define ID_BAR_SHADOW		0
#define ID_BAR_LIGHT		1
#define ID_KEYBOARD			2
#define ID_DARK_KEYBOARD	3
#define ID_DIPSW_OFF		4
#define ID_DIPSW_ON			5
#define ID_LOGO				6
#define ID_SLOT1_ON			7
#define ID_SLOT2_ON			8
#define ID_SLOT3_ON			9
#define ID_SLOT4_ON			10
#define ID_SLOT1_OFF		11
#define ID_SLOT2_OFF		12
#define ID_SLOT3_OFF		13
#define ID_SLOT4_OFF		14
#define ID_KNOB_DARK		15
#define ID_KNOB_LIGHT		16
#define ID_KNOB_TRANS		17
#define ID_CHECK_OFF		18
#define ID_CHECK_ON			19
#define ID_MUTE_OFF			20
#define ID_MUTE_ON			21
#define ID_SOLO_OFF			22
#define ID_SOLO_ON			23


const BMPREG skinreg_tool[] = {
	{435,385,21,96},	// 0:bar shadow
	{457,385,21,96},	// 1:bar light
	{  0,178,54,20},	// 2:keyboard
	{ 55,178,35,20},	// 3:dark keyboard
	{103,178,11,20},	// 4:SW OFF
	{ 91,178,11,20},	// 5:SW ON
	{  0,199,115,24},	// 6:logo
	{144,101,13,13},	// 7:(A)
	{158,101,13,13},	// 8:(B)
	{172,101,13,13},	// 9:(C)
	{186,101,13,13},	// 10:(D)
	{144,115,13,13},	// 11:(A)
	{158,115,13,13},	// 12:(B)
	{172,115,13,13},	// 13:(C)
	{186,115,13,13},	// 14:(D)
	{116,178,22,22},	// 15:knob-dark
	{139,178,22,22},	// 16:knob-light
	{162,178,22,22},	// 17:knob-trans
	{200,113,7,7},		// 18:check off
	{200,121,7,7},		// 19:check on
	{208,105,13,11},	// 20:mute-off
	{208,117,13,11},	// 21:mute-on
	{222,105,13,11},	// 22:solo-off
	{222,117,13,11},	// 23:solo-on
};

const BMPREG skinreg_view[] = {
	{310,0,330,75},			// FM view1
	{310,75,330,75},		// FM view2
	{310,150,330,75},		// FM slot view
};

const int keyW1 = 5;	// 白鍵
const int keyH1 = 20;
const int keyW2 = 3;	// 黒鍵
const int keyH2 = 10;
const int keyXOffset[] = { 0,3,5,8,10,15,18,20,23,25,28,30 };
const int keyHiXOffset[] = { 35,50,40,50,45,35,50,40,50,40,50,45 };
const int keyHiW[] = { 4,3,4,3,4,4,3,4,3,4,3,4,3,4 };
const int keyHiH[] = { 20,10,20,10,20,20,10,20,10,20,10,20,10,20 };



CVisC86Skin gVisSkin;

CVisC86Skin::CVisC86Skin() : skinbmp(0)
{
	pallet.resize(5);
	coltbl.resize(256);
}

CVisC86Skin::~CVisC86Skin(){
	deinit();
}

void CVisC86Skin::init(void){
	skinbmp = CVisBitmap::LoadFromResource( IDB_PNGSKIN1, _T("PNG"), C86CtlMain::getInstanceHandle() );
	UINT *p = (UINT*)skinbmp->getPtr(639,511);
	for( int i=0; i<5; i++ ){
		pallet[i] = *p--;
	}
	p = (UINT*)skinbmp->getPtr(639,510);
	for( int i=0; i<256; i++ ){
		coltbl[i] = *p--;
	}
}

void CVisC86Skin::deinit(void){
	if(skinbmp){
		delete skinbmp;
		skinbmp = 0;
	}
}

// ウィンドウフレームの描画
void CVisC86Skin::drawFrame( IVisBitmap *canvas, int type, CHAR *title )
{
	int xe = canvas->getWidth() - 1;
	int ye = canvas->getHeight() - 1;

	UINT col_light = pallet[IDCOL_LIGHT];
	UINT col_shadow = pallet[IDCOL_SHADOW];
	UINT col_mid = pallet[IDCOL_SHADOW];

	// frame
	visDrawLine( canvas, 0, 0, xe, 0, col_light );
	visDrawLine( canvas, 0, 0, 0, ye, col_light );

	visDrawLine( canvas, xe, 1, xe, ye, col_shadow ); // 右
	visDrawLine( canvas, 1, ye, xe, ye, col_shadow ); // 下
	
	visDrawLine( canvas, 1, 1, xe-1, 1, col_mid ); // 上
	visDrawLine( canvas, 1, 1, 1, ye-1, col_mid ); // 左
	visDrawLine( canvas, xe-1, 1, xe-1, ye-1, col_mid ); // 右
	visDrawLine( canvas, 1, ye-1, xe-1, ye-1, col_mid ); // 下

	// topbar
	BMPREG reg_l, reg_r, reg_c;
	if( type==0 ){
		reg_l = skinreg_frame[0];
		reg_r = skinreg_frame[1];
		reg_c = skinreg_frame[2];
	}else{
		reg_l = skinreg_frame[4];
		reg_r = skinreg_frame[5];
		reg_c = skinreg_frame[6];
	}

	blt(canvas,    2, 2, reg_l.width, reg_l.height, skinbmp, reg_l.left, reg_l.top );
	blt(canvas, xe-9-14, 2, reg_r.width, reg_r.height, skinbmp, reg_r.left, reg_r.top );

	int tw = xe - 3 - reg_l.width - reg_r.width - 14;
	int l = 10;
	while(0<tw){
		INT w = __min( tw, reg_c.width );
		blt(canvas, l, 2, w, reg_c.height, skinbmp, reg_c.left, reg_c.top);
		l+=w;  tw-=w;
	}

	// title
	l = (int)strlen(title);
	if( 0<l ){
		visFillRect( canvas, 3, 3, (6*(l+2)), 13, RGB(0,0,0) );
		drawStr( canvas, 1, 10, 6, title );
	}
}

// 閉じるボタン
void CVisC86Skin::drawCloseButton( IVisBitmap *canvas, int type, int x, int y )
{
	BMPREG reg = skinreg_frame[(type==0)?3:7];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// 文字描画
void CVisC86Skin::drawStr( IVisBitmap *canvas, int type, int x, int y, const char *str )
{
	if( type < 0 || 3 <= type )
		return;

	int left = skinreg_fonts[type].left;
	int top = skinreg_fonts[type].top;
	int len = (int)strlen(str);
	for( int i=0; i<len; i++ ){
		int c = str[i]-0x20;
		if( 0<=c && c<96 ){
			int srcx = left + 6 * (c&0x0f);
			int srcy = top  + 8 * (c>>4);
			alphablt( canvas, x+6*i, y, 6, 7, skinbmp, srcx, srcy );
		}
	}
}

// 縦文字描画
void CVisC86Skin::drawVStr( IVisBitmap *canvas, int type, int x, int y, const char *str )
{
	if( type < 0 || 3 <= type )
		return;

	int left = skinreg_fonts[3+type].left;
	int top = skinreg_fonts[3+type].top;
	int len = (int)strlen(str);
	for( int i=0; i<len; i++ ){
		int c = str[i]-0x20;
		if( 0<=c && c<96 ){
			int srcx = left + (8 * (c>>4));
			int srcy = top  - (6 * (c&0x0f)) - 5;
			alphablt(canvas, x-5, y-i*6, 7, 6, skinbmp, srcx, srcy );
		}
	}
}

// ちっちゃい数字描画
void CVisC86Skin::drawNanoNumStr( IVisBitmap *canvas, int x, int y, const char *str )
{
	int left = skinreg_fonts[8].left;
	int top = skinreg_fonts[8].top;
	int cx = skinreg_fonts[8].width;
	int cy = skinreg_fonts[8].height;
	int len = (int)strlen(str);
	for( int i=0; i<len; i++ ){
		char c = str[i];
		if( '0' <= c && c <= '9' )
			c = c - '0';
		else if( c == '-' )
			c = 14;
		else if( c == '+' )
			c = 15;
		else 
			continue;
		int srcx = left + cx * c;
		int srcy = top;
		alphablt(canvas, x+cx*i, y, cx, cy, skinbmp, srcx, srcy );
	}
}

// 大きい数字描画
void CVisC86Skin::drawNumStr1( IVisBitmap *canvas, int x, int y, const char *str )
{
	int left = skinreg_fonts[6].left;
	int top = skinreg_fonts[6].top;
	int cx = 8;//skinreg_fonts[6].width;
	int cy = 11;//skinreg_fonts[6].height;
	int len = (int)strlen(str);
	for( int i=0; i<len; i++ ){
		char c = str[i]==' ' ? 0 : str[i] - '0' + 1;
		int srcx = left + cx * c;
		int srcy = top;
		alphablt(canvas, x+cx*i, y, cx, cy, skinbmp, srcx, srcy );
	}
}

// キーボード描画
void CVisC86Skin::drawKeyboard( IVisBitmap *canvas, int x, int y )
{
	int sx = skinreg_tool[ID_KEYBOARD].left;
	int sy = skinreg_tool[ID_KEYBOARD].top;
	
	for( int i=0; i<8; i++ ){
		int w = keyW1*7;
		alphablt(canvas, x+w*i, y, w, keyH1, skinbmp, sx, sy );
	}
}

// 暗い色のキーボード描画
void CVisC86Skin::drawDarkKeyboard( IVisBitmap *canvas, int x, int y )
{
	int sx = skinreg_tool[ID_DARK_KEYBOARD].left;
	int sy = skinreg_tool[ID_DARK_KEYBOARD].top;
	
	for( int i=0; i<8; i++ ){
		int w = keyW1*7;
		alphablt(canvas, x+w*i, y, w, keyH1, skinbmp, sx, sy );
	}
}

// 発音中キー表示
void CVisC86Skin::drawHilightKey( IVisBitmap *canvas, int x, int y, int oct, int note )
{
	if( oct < 0 || 8 <= oct ) return;
	if( note < 0 || 12 <= note ) return;
	int srcx = skinreg_tool[ID_KEYBOARD].left + keyHiXOffset[note];
	int srcy = skinreg_tool[ID_KEYBOARD].top;
	int dstx = keyW1*7*oct + keyXOffset[note];
	int w = keyHiW[note];
	int h = keyHiH[note];
	alphablt(canvas, x+dstx, y, w, h, skinbmp, srcx, srcy );
}

// くるくるサークル
void CVisC86Skin::drawTickCircle( IVisBitmap *canvas, int x, int y, int tick )
{
	int idx = (tick==-1) ? 0 : 1+(tick&0x7);
	BMPREG reg = skinreg_tick[idx];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// ロゴ描画
void CVisC86Skin::drawLogo(IVisBitmap *canvas, int x, int y )
{
	BMPREG reg = skinreg_tool[ID_LOGO];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// dip-sw
void CVisC86Skin::drawDipSw(IVisBitmap *canvas, int x, int y, int sw )
{
	BMPREG reg = skinreg_tool[(sw ? ID_DIPSW_ON : ID_DIPSW_OFF)];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// checkbox
void CVisC86Skin::drawCheckBox(IVisBitmap *canvas, int x, int y, int sw )
{
	BMPREG reg = skinreg_tool[(sw ? ID_CHECK_ON : ID_CHECK_OFF)];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// つまみ
void CVisC86Skin::drawKnob(IVisBitmap *canvas, int x, int y, int minval, int maxval, int numval )
{
	BMPREG reg_d = skinreg_tool[ID_KNOB_DARK];
	BMPREG reg_l = skinreg_tool[ID_KNOB_LIGHT];
	BMPREG reg_t = skinreg_tool[ID_KNOB_TRANS];
	char buff[10];
	
	transblt2( canvas, x, y, reg_d.width, reg_d.height,
		skinbmp, reg_d.left, reg_d.top,
		skinbmp, reg_l.left, reg_l.top,
		skinbmp, reg_t.left, reg_t.top, minval, maxval );

	sprintf( buff, "%d", numval );
	if( numval < 10 )       x+=9;
	else if( numval < 100 ) x+=7;
	else if( numval < 300 ) x+=3;
	drawNanoNumStr( canvas, x, y+8, buff );
}

// SSG-EG
void CVisC86Skin::drawSSGEG(IVisBitmap *canvas, int x, int y, int no )
{
	BMPREG reg = skinreg_ssgeg[no];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// FMアルゴリズム
void CVisC86Skin::drawFMAlgorithm(IVisBitmap *canvas, int x, int y, int no )
{
	BMPREG reg = skinreg_alg[no];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// FM部背景
void CVisC86Skin::drawFMSkin(IVisBitmap *canvas, int x, int y, int exmode )
{
	BMPREG reg = skinreg_view[ 1 ];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// FM slot部背景
void CVisC86Skin::drawFMSlotSkin(IVisBitmap *canvas, int x, int y )
{
	BMPREG reg = skinreg_view[2];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

void CVisC86Skin::drawFMSlotLight(IVisBitmap *canvas, int x, int y, int no, int sw )
{
	BMPREG reg = skinreg_tool[7+sw*4+no];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// mute switch
void CVisC86Skin::drawMuteSw(IVisBitmap *canvas, int x, int y, int sw )
{
	BMPREG reg = skinreg_tool[(sw ? ID_MUTE_ON : ID_MUTE_OFF)];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}

// solo switch
void CVisC86Skin::drawSoloSw(IVisBitmap *canvas, int x, int y, int sw )
{
	BMPREG reg = skinreg_tool[(sw ? ID_SOLO_ON : ID_SOLO_OFF)];
	blt( canvas, x, y, reg.width, reg.height, skinbmp, reg.left, reg.top );
}


// horizontal bar graph
void CVisC86Skin::drawHBar( IVisBitmap *bmp, int xs, int ys, int level, int peak )
{
	UINT *pd = (UINT*)bmp->getRow0(ys) + xs;
	UINT step = bmp->getStep()>>2;
	UINT mid = pallet[IDCOL_MID];
	UINT high = pallet[IDCOL_HIGH];
	UINT shadow = pallet[IDCOL_SHADOW];

	UINT *pd2 = pd;
	for( int y=0; y<20; y++ ){
		*pd2 = mid;
		pd2 += step;
	}
	pd+=2;
	for( int x=1; x<32; x++ ){
		pd2 = pd;
		if( x<=level || x == peak ){
			for( int y=0; y<20; y++ ){
				*pd2 = high;;
				pd2 += step;
			}
		}else{
			for( int y=0; y<20; y++ ){
				*pd2 = shadow;
				pd2 += step;
			}
		}
		pd+=2;
	}
}
