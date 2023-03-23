/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#pragma once
#include "chip/opna.h"
#include "vis_bitmap.h"

namespace c86ctl{
namespace vis {

// bitmap operations ------------------------------------------------
void blt( IVisBitmap *dst, int dst_x, int dst_y, int w, int h,
		  IVisBitmap *src, int src_x, int src_y );
void alphablt( IVisBitmap *dst, int dst_x, int dst_y, int w, int h,
			   IVisBitmap *src, int src_x, int src_y );
void transblt( IVisBitmap *dst, int dst_x, int dst_y, int w, int h,
			   IVisBitmap *src1, int src1_x, int src1_y,
			   IVisBitmap *src2, int src2_x, int src2_y,
			   IVisBitmap *trans, int trans_x, int trans_y, int t );
void transblt2( IVisBitmap *dst, int dst_x, int dst_y, int w, int h,
			   IVisBitmap *src1, int src1_x, int src1_y,
			   IVisBitmap *src2, int src2_x, int src2_y,
			   IVisBitmap *trans, int trans_x, int trans_y, int tmin, int tmax );

void visDrawLine( IVisBitmap *bmp, int xs, int ys, int xe, int ye, COLORREF col );
void visFillRect( IVisBitmap *bmp, int xs, int ys, int w, int h, COLORREF col );


};
};

