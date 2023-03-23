/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "stdafx.h"

#include "vis_c86wnd.h"
#include "vis_widget.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;
using namespace c86ctl::vis;


// ---------------------------------------------------------------------------
void CVisCloseButton::onPaint(IVisBitmap *canvas)
{
	gVisSkin.drawCloseButton( canvas, 0, sx, sy );
}

void CVisCloseButton::onMouseEvent(UINT msg, WPARAM wp, LPARAM lp)
{
	switch(msg){
	case WM_LBUTTONUP:
		std::for_each( pushEvent.begin(), pushEvent.end(), [this](std::function<void(CVisWidget*)> h){ h(this); } );
		break;
	}
}


// --------------------------------------------------------
void CVisCheckBox::onPaint(IVisBitmap *canvas){
	gVisSkin.drawCheckBox( canvas, sx, sy, sw );
	gVisSkin.drawStr( canvas, sw?1:0, sx+10, sy, str.c_str() );
}
	
void CVisCheckBox::onMouseEvent(UINT msg, WPARAM wp, LPARAM lp){
	switch(msg){
	case WM_LBUTTONDOWN:
		sw ^= 1;
		std::for_each( changeEvent.begin(), changeEvent.end(), [this](std::function<void(CVisWidget*)> h){ h(this); } );
		break;
	}
}
void CVisCheckBox::setCheck(int newval, bool pulseEvent)
{
	if( newval != sw ){
		sw = newval;
		if(pulseEvent)
			std::for_each( changeEvent.begin(), changeEvent.end(), [this](std::function<void(CVisWidget*)> h){ h(this); } );
	}
}

// --------------------------------------------------------
void CVisSwitchBase::onMouseEvent(UINT msg, WPARAM wp, LPARAM lp){
	switch(msg){
	case WM_LBUTTONDOWN:
		int sw = 0;
		if( getter ) sw = getter();
		if( setter ) setter(sw^1);
		std::for_each( changeEvent.begin(), changeEvent.end(), [this](std::function<void(CVisWidget*)> h){ h(this); } );
		break;
	}
};

// --------------------------------------------------------
void CVisDipSw::onPaint(IVisBitmap *canvas){
	int sw = 0;
	if( getter ) sw = getter();
	gVisSkin.drawDipSw( canvas, sx, sy, sw );
};

void CVisMuteSw::onPaint(IVisBitmap *canvas){
	int sw = 0;
	if( getter ) sw = getter();
	gVisSkin.drawMuteSw( canvas, sx, sy, sw );
};

void CVisSoloSw::onPaint(IVisBitmap *canvas){
	int sw = 0;
	if( getter ) sw = getter();
	gVisSkin.drawSoloSw( canvas, sx, sy, sw );
};


// --------------------------------------------------------
void CVisKnob::onPaint(IVisBitmap *canvas){
	int value = 0;
	if( getter ) value = getter();
	int r = maxval-minval;
	int t1 = minval < 0 ? (255*(-minval))/r : 0;
	int t2 = (255*(value-minval))/r;

	if(t1<t2) gVisSkin.drawKnob( canvas, sx, sy, t1, t2, value );
	else      gVisSkin.drawKnob( canvas, sx, sy, t2, t1, value );
}

void CVisKnob::onMouseEvent(UINT msg, WPARAM wp, LPARAM lp)
{
	switch(msg){
	case WM_MOUSEMOVE:
		if( bMoving ){
			POINT cpos;
			::GetCursorPos(&cpos);
			int dx = (int)cpos.x - spos.x;
			int dy = (int)cpos.y - spos.y;
			int xvalue = svalue + (maxval - minval)*dx/512;
			if( maxval < xvalue ) xvalue = maxval;
			if( xvalue < minval ) xvalue = minval;
			if( setter ) setter(xvalue);
			std::for_each( changeEvent.begin(), changeEvent.end(), [this](std::function<void(CVisWidget*)> h){ h(this); } );
		}
		break;
	case WM_LBUTTONDOWN:
		::GetCursorPos(&spos);
		if( getter ) svalue = getter();
		bMoving = true;
		parent->setCapture(this);
		break;
	case WM_LBUTTONUP:
		bMoving = false;
		parent->releaseCapture();
		break;
	}
}

// --------------------------------------------------------
void CVisSSGEGGraph::onPaint(IVisBitmap *canvas){
	int no = 0;
	if( getter ) no = getter();
	gVisSkin.drawSSGEG( canvas, sx, sy, no );
}

