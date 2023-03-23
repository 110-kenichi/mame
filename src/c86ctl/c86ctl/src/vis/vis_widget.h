/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#pragma once

#include <functional>
#include "vis_c86sub.h"
#include "vis_c86skin.h"
#include "vis_c86wnd.h"

namespace c86ctl{
namespace vis {

// ---------------------------------------------------------------------------
class CVisWidget
{
public:
	CVisWidget(CVisWnd *parentWnd)
		: parent(parentWnd), sx(0), sy(0), ex(0), ey(0){};
	~CVisWidget(){};
	
public:
	// message handler
	virtual void onPaint(IVisBitmap *canvas){};
	virtual void onMouseEvent(UINT msg, WPARAM wp, LPARAM lp){};
	virtual void getWindowRect(RECT &rc){
		rc.left = sx;
		rc.top = sy;
		rc.right = ex;
		rc.bottom = ey;
	};

protected:
	int sx, sy, ex, ey;
	CVisWnd *parent;
};


// ---------------------------------------------------------------------------
// close-button
class CVisCloseButton : public CVisWidget
{
public:
	CVisCloseButton( CVisWnd *parentWnd, int x, int y )
		: CVisWidget(parentWnd){
		sx = x;
		sy = y;
		ex = x + 15;
		ey = y + 15;
	};
	~CVisCloseButton(){};

public:
	virtual void onPaint(IVisBitmap *canvas);
	virtual void onMouseEvent(UINT msg, WPARAM wp, LPARAM lp);

public:
	std::list< std::function< void(CVisWidget*) > > pushEvent;
};

typedef std::shared_ptr<CVisCloseButton> CVisCloseButtonPtr;

// ---------------------------------------------------------------------------
// checkbox
class CVisCheckBox : public CVisWidget
{
public:
	CVisCheckBox( CVisWnd *parentWnd, int x, int y, char *title )
		: CVisWidget(parentWnd), sw(0), str(title){
		sx = x;
		sy = y;
		ex = x + 10 + 6*static_cast<int>(str.length());
		ey = y + 8;
	};
	~CVisCheckBox(){};

public:
	virtual void onPaint(IVisBitmap *canvas);
	virtual void onMouseEvent(UINT msg, WPARAM wp, LPARAM lp);
	virtual void setCheck(int newval, bool pulseEvent);

public:
	int getValue(){ return sw; };

public:
	std::list< std::function< void(CVisWidget*) > > changeEvent;
	
protected:
	int sw;
	std::string str;
};

typedef std::shared_ptr<CVisCheckBox> CVisCheckBoxPtr;


// ---------------------------------------------------------------------------
// SWITCH
class CVisSwitchBase : public CVisWidget
{
public:
	CVisSwitchBase( CVisWnd *parentWnd, int x, int y, int w, int h )
		: CVisWidget(parentWnd){
			sx = x;
			sy = y;
			ex = x+w;
			ey = y+h;
		};
	~CVisSwitchBase(){};

public:
	virtual void onPaint(IVisBitmap *canvas){};
	virtual void onMouseEvent(UINT msg, WPARAM wp, LPARAM lp);
	
public:
	std::list< std::function< void(CVisWidget*) > > changeEvent;
	std::function< int() > getter;
	std::function< void(int) > setter;
};

// ---------------------------------------------------------------------------
// DIPSW
class CVisDipSw : public CVisSwitchBase
{
public:
	CVisDipSw( CVisWnd *parentWnd, int x, int y )
		: CVisSwitchBase(parentWnd, x, y, 11, 20){};
	~CVisDipSw(){};
public:
	virtual void onPaint(IVisBitmap *canvas);
};
typedef std::shared_ptr<CVisDipSw> CVisDipSwPtr;

// ---------------------------------------------------------------------------
// MUTE-SW
class CVisMuteSw : public CVisSwitchBase
{
public:
	CVisMuteSw( CVisWnd *parentWnd, int x, int y )
		: CVisSwitchBase(parentWnd, x, y, 13, 11){};
	~CVisMuteSw(){};

public:
	virtual void onPaint(IVisBitmap *canvas);
};
typedef std::shared_ptr<CVisMuteSw> CVisMuteSwPtr;

// ---------------------------------------------------------------------------
// SOLO-SW
class CVisSoloSw : public CVisSwitchBase
{
public:
	CVisSoloSw( CVisWnd *parentWnd, int x, int y )
		: CVisSwitchBase(parentWnd, x, y, 13, 11){};
	~CVisSoloSw(){};
public:
	virtual void onPaint(IVisBitmap *canvas);
};
typedef std::shared_ptr<CVisSoloSw> CVisSoloSwPtr;

// ---------------------------------------------------------------------------
// knob
class CVisKnob : public CVisWidget
{
public:
	CVisKnob( CVisWnd *parentWnd, int x, int y )
		: CVisWidget(parentWnd), minval(0), maxval(127),
		  bMoving(false){
		sx = x;
		sy = y;
		ex = sx+22;
		ey = sy+22;
	};
	~CVisKnob(){};

public:
	virtual void onPaint(IVisBitmap *canvas);
	virtual void onMouseEvent(UINT msg, WPARAM wp, LPARAM lp);

	void setRange(int minv, int maxv){
		minval = minv;
		maxval = maxv;
	};
	
public:
	std::list< std::function< void(CVisWidget*) > > changeEvent;
	std::function< int() > getter;
	std::function< void(int) > setter;
	
protected:
	int maxval;
	int minval;
	bool bMoving;
	POINT spos;
	int svalue;
};
typedef std::shared_ptr<CVisKnob> CVisKnobPtr;

// ---------------------------------------------------------------------------
// SSG-EG
class CVisSSGEGGraph : public CVisWidget
{
public:
	CVisSSGEGGraph( CVisWnd *parentWnd, int x, int y )
		: CVisWidget(parentWnd){
			sx=x;
			sy=y;
			ex=x+52;
			ey=11;
		};
	~CVisSSGEGGraph(){};
	
public:
	virtual void onPaint(IVisBitmap *canvas);

public:
	std::list< std::function< void(CVisWidget*) > > changeEvent;
	std::function< int() > getter;
	std::function< void(int) > setter;
};
typedef std::shared_ptr<CVisSSGEGGraph> CVisSSGEGGraphPtr;

}; // namespace vis
}; // namespace c86ctl


// ---------------------------------------------------------------------------
// support macro.
#define HANDLER_DELEGATE( method )  [this](c86ctl::vis::CVisWidget* s){ this-> ## method (s); }
#define ADD_HANDLER( e, method )  e ## .push_back( [this](c86ctl::vis::CVisWidget* s){ this-> ## method (s); } );



