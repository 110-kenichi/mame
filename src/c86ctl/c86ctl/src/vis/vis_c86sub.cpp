/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "stdafx.h"
#include <stdlib.h>
#include <stdio.h>
#include <math.h>
#include "resource.h"
#include "vis_c86sub.h"
#include "vis_c86skin.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

// --------------------------------------------------------------------------------------
void c86ctl::vis::blt(
	IVisBitmap *dst, int dst_x, int dst_y, int w, int h,
	IVisBitmap *src, int src_x, int src_y )
{
	if( dst_x<0 || dst_y<0 || src_x<0 || src_y<0 )
		return;
	if( src_x+w > src->getWidth() || src_y+h > src->getHeight() ||
		dst_x+w > dst->getWidth() || dst_y+h > dst->getHeight() )
		return;

	for( int y=0; y<h; y++ ){
		UINT *ps = ((UINT*)src->getRow0( src_y+y )) + src_x;
		UINT *pd = ((UINT*)dst->getRow0( dst_y+y )) + dst_x;
		for( int x=0; x<w; x++ ){
			*pd++ = (*ps++|0xff000000);
		}
	}
}

void c86ctl::vis::alphablt(
	IVisBitmap *dst, int dst_x, int dst_y, int w, int h,
	IVisBitmap *src, int src_x, int src_y )
{
	if( dst_x<0 || dst_y<0 || src_x<0 || src_y<0 )
		return;
	if( src_x+w > src->getWidth() || src_y+h > src->getHeight() ||
		dst_x+w > dst->getWidth() || dst_y+h > dst->getHeight() )
		return;

	for( int y=0; y<h; y++ ){
		UINT *ps = ((UINT*)src->getRow0( src_y+y )) + src_x;
		UINT *pd = ((UINT*)dst->getRow0( dst_y+y )) + dst_x;
		for( int x=0; x<w; x++ ){
			UINT a = *ps >> 24;
			UINT na = a^0xff; // 255-a
			
			UINT s = *ps;
			UINT d = *pd;
			UINT t;
			// (s*a) + (d*(1-a))
			t = (((((s&0x00ff00ff) * ( a+1)) >> 8) & 0x00ff00ff) |
			     ((((s&0x0000ff00) * ( a+1)) >> 8) & 0x0000ff00)) +
			  + (((((d&0x00ff00ff) * (na+1)) >> 8) & 0x00ff00ff) |
			     ((((d&0x0000ff00) * (na+1)) >> 8) & 0x0000ff00));
			*pd = t;

			ps++;
			pd++;
		}
	}
}

void c86ctl::vis::transblt(
	IVisBitmap *dst, int dst_x, int dst_y, int w, int h,
	IVisBitmap *src1, int src1_x, int src1_y,
	IVisBitmap *src2, int src2_x, int src2_y,
	IVisBitmap *trans, int trans_x, int trans_y, int t )
{
	if( dst_x<0 || dst_y<0 || trans_x<0 || trans_y<0 )
		return;
	if( src1_x<0 || src1_y<0 || src2_x<0 || src2_y<0 )
		return;
	if( dst_x+w > dst->getWidth() || dst_y+h > dst->getHeight() ||
		src1_x+w > src1->getWidth() || src1_y+h > src1->getHeight() ||
		src2_x+w > src2->getWidth() || src2_y+h > src2->getHeight() ||
		trans_x+w > trans->getWidth() || trans_y+h > trans->getHeight() )
		return;

	for( int y=0; y<h; y++ ){
		UINT *pd = (UINT*)dst->getRow0( dst_y+y ) + dst_x;
		UINT *ps1 = (UINT*)src1->getRow0( src1_y+y ) + src1_x;
		UINT *ps2 = (UINT*)src2->getRow0( src2_y+y ) + src2_x;
		UINT *ts = (UINT*)trans->getRow0( trans_y+y ) + trans_x;
		for( int x=0; x<w; x++ ){
			int a = *ts&0xff;
			*pd = ( (t<=a) ? *ps1 : *ps2 ) | 0xff000000;
			pd++; ps1++; ps2++; ts++; 
		}
	}
}

// [tmin, tmax)
void c86ctl::vis::transblt2(
	IVisBitmap *dst, int dst_x, int dst_y, int w, int h,
	IVisBitmap *src1, int src1_x, int src1_y,
	IVisBitmap *src2, int src2_x, int src2_y,
	IVisBitmap *trans, int trans_x, int trans_y, int tmin, int tmax )
{
	if( dst_x<0 || dst_y<0 || trans_x<0 || trans_y<0 )
		return;
	if( src1_x<0 || src1_y<0 || src2_x<0 || src2_y<0 )
		return;
	if( dst_x+w > dst->getWidth() || dst_y+h > dst->getHeight() ||
		src1_x+w > src1->getWidth() || src1_y+h > src1->getHeight() ||
		src2_x+w > src2->getWidth() || src2_y+h > src2->getHeight() ||
		trans_x+w > trans->getWidth() || trans_y+h > trans->getHeight() )
		return;

	for( int y=0; y<h; y++ ){
		UINT *pd = (UINT*)dst->getRow0( dst_y+y ) + dst_x;
		UINT *ps1 = (UINT*)src1->getRow0( src1_y+y ) + src1_x;
		UINT *ps2 = (UINT*)src2->getRow0( src2_y+y ) + src2_x;
		UINT *ts = (UINT*)trans->getRow0( trans_y+y ) + trans_x;
		for( int x=0; x<w; x++ ){
			int a = *ts&0xff;
			*pd = ( (tmin<=a && a<tmax) ? *ps2 : *ps1 ) | 0xff000000;
			pd++; ps1++; ps2++; ts++; 
		}
	}
}



void c86ctl::vis::visDrawLine(
	IVisBitmap *bmp, int xs, int ys, int xe, int ye, COLORREF col )
{
	// note: めんどくさがって bpp==4専用コードで書いてるので注意
	if( !bmp )
		return;
	if( xs<0 || ys<0 || bmp->getWidth()<=xs || bmp->getHeight()<=ys )
		return;
	if( xe<0 || ye<0 || bmp->getWidth()<=xe || bmp->getHeight()<=ye )
		return;
	
	int step = bmp->getStep()>>2;

	if( xs == xe ){	// 垂直線
		if( ye<ys ) SWAP(ys,ye);
		UINT *p = ((UINT*)bmp->getRow0(ys)) + xs;
		for( int y=ys; y<=ye; y++ ){
			*p = col;
			p += step;
		}
	}else if( ys == ye ){ //水平線
		if( xe<xs ) SWAP(xs,xe);
		UINT *p = ((UINT*)bmp->getRow0(ys)) + xs;
		for( int x=xs; x<=xe; x++ ){
			*p++ = col;
		}
	}else{ // 斜め
		// TODO : デバッグしてないの。
		int dx = abs(xe-xs);
		int dy = abs(ye-ys);
		int dx2=dx*2;
		int dy2=dy*2;
		
		if( dx > dy ){
			if( xe<xs ){
				SWAP(xs,xe);
				SWAP(ys,ye);
			}
			UINT *p = ((UINT*)bmp->getRow0(ys)) + xs;
			int dstep = ys<ye ? step : -step;
			
			for( int e=dy, x=xs; x<=xe; x++, p+=1 ){
				*p = col;
				e+=dy2;
				if( e>=dx2 ){
					e-=dx2;
					p+=dstep;
				}
			}
		}else{
			if( ye<ys ){
				SWAP(xs,xe);
				SWAP(ys,ye);
			}
			UINT *p = ((UINT*)bmp->getRow0(ys)) + xs;
			int dstep = xs<xe ? 1 : -1;
			
			for( int e=dx, y=ys; y<=ye; y++, p+=step ){
				*p = col;
				e+=dx2;
				if( e>=dy2 ){
					e-=dy2;
					p+=dstep;
				}
			}
		}
	}
}

void c86ctl::vis::visFillRect(
	IVisBitmap *bmp, int xs, int ys, int w, int h, COLORREF col )
{
	if( !bmp )
		return;
	if( xs<0 || ys<0 || bmp->getWidth()<(xs+w) || bmp->getHeight()<(ys+h) )
		return;
	
	for( int y=0; y<h; y++ ){
		UINT *p = ((UINT*)bmp->getRow0(ys+y)) + xs;
		for( int x=0; x<w; x++ ){
			*p++ = col;
		}
	}
}

