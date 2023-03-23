/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#pragma once

#include <map>
#include "vis_manager.h"
#include "vis_c86skin.h"
#include "vis_widget.h"

#define BORDER			2
#define BORDER_TITLE	15

namespace c86ctl{
namespace vis {

class CVisManager;

class CVisWnd
{
public:
	CVisWnd();
	virtual ~CVisWnd();
	
public:
	virtual bool create( int width, int height, DWORD exstyle = 0, DWORD style = (WS_POPUP | WS_VISIBLE | WS_CLIPCHILDREN), HWND hParent = 0 );
	virtual bool resize( int width, int height );
	virtual void close(void);
	virtual void saveWndPos(void);
	
public:
	HWND getHWND(void){ return hWnd; };
	void redraw(){ onPaint(); };

public:
	void windowToClient( POINT &pt ){
		pt.x -= BORDER;
		pt.y -= (BORDER+BORDER_TITLE);
	};
	void windowToClient( RECT &rc ){
		rc.top -= (BORDER+BORDER_TITLE);
		rc.left -= BORDER;
		rc.bottom -= (2*BORDER+BORDER_TITLE);
		rc.right -= (2*BORDER);
	};

	void setCapture(CVisWidget *w){
		::SetCapture(hWnd);
		wcapture = w;
	};
	void releaseCapture(){
		wcapture = NULL;
		::ReleaseCapture();
	};
	virtual bool isWindowVisible(){
		if( !hWnd ) return false;
		if( ::IsWindowVisible(hWnd) )
			return true;
		else
			return false;
	};

public:
	std::list< std::function< void(CVisWnd*) > > closeEvent;
	
protected:
	// message handler
	virtual void onCreate(){};
	virtual void onClose(){};
	virtual void onDestroy();
	virtual void onPaint();
	virtual void onPaintClient(){};
	virtual void onMouseEvent(UINT msg, WPARAM wp, LPARAM lp){};
	virtual void onCommand(HWND hwnd, DWORD id, DWORD notifyCode){};
	virtual void onKeyDown(DWORD keycode){};
	virtual void onKeyUp(DWORD keycode){};
	
protected:
	virtual LRESULT CALLBACK wndProc(HWND hWnd , UINT msg , WPARAM wp , LPARAM lp);
	bool isClose( int a, int b ) const;

private:
	int wndWidth;
	int wndHeight;

protected:
	HWND hWnd;
	HWND hParent;
	tstring windowClass;
	tstring windowTitle;

protected:
	CVisBitmap *canvas;
	CVisChildBitmap *clientCanvas;
	CVisCloseButtonPtr closeButton;

	std::list< std::shared_ptr<CVisWidget> > widgets;
	std::list< std::shared_ptr<CVisWidget> > sysWidgets;

private:
	CVisWidget *wcapture;
	
private: // CVisWndとHWNDの対応付け用
	static LRESULT CALLBACK wndProcDispatcher(HWND hWnd , UINT msg , WPARAM wp , LPARAM lp);
	static std::map< HWND, CVisWnd* > wndMap;
	static CVisWnd* creatingWnd;
	static HANDLE hCreatingMutex;
};

};
};
