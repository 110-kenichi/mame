/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "stdafx.h"
#include <map>
#include <assert.h>
#include <tchar.h>

#include "c86ctlmain.h"
#include "config.h"
#include "resource.h"
#include "vis_c86sub.h"
#include "vis_c86wnd.h"
#include "vis_manager.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif


#define MUTEX_NAME TEXT("C86WINDOW_MANAGER_MUTEX")

using namespace c86ctl;
using namespace c86ctl::vis;

std::map< HWND, CVisWnd* > CVisWnd::wndMap;
CVisWnd* CVisWnd::creatingWnd;
HANDLE CVisWnd::hCreatingMutex = NULL;


CVisWnd::CVisWnd()
	: hWnd(0)
	, canvas(0)
	, clientCanvas(0)
	, wcapture(0)
	, wndWidth(0)
	, wndHeight(0)
	, windowClass(_T("C86CTL"))
	, windowTitle(_T("C86CTL"))
{
	hCreatingMutex = ::CreateMutex( NULL, FALSE, MUTEX_NAME );
};

CVisWnd::~CVisWnd()
{
	close();
	
	if( hCreatingMutex ){
		::CloseHandle( hCreatingMutex );
		hCreatingMutex = NULL;
	}
};

bool CVisWnd::isClose( int a, int b ) const {
	const int margin = 10;
	return abs(a-b) < margin;
};

void CVisWnd::onPaint()
{
	const int maxlen = 256;
	char str[maxlen];
	
	HDC hdc = ::GetDC(hWnd);

	int l = ::WideCharToMultiByte(CP_THREAD_ACP, 0, windowTitle.c_str(), windowTitle.size(), str, maxlen, NULL, NULL);
	if( 0<l ){ str[l] = 0; }else{ str[0] = 0; };

	int type = 0;
	if( hWnd == ::GetForegroundWindow() ) type = 1;
	gVisSkin.drawFrame( canvas, type, str );
	std::for_each( sysWidgets.begin(), sysWidgets.end(),
				   [this](std::shared_ptr<CVisWidget> x){ x->onPaint(canvas); } );

	onPaintClient();
	std::for_each( widgets.begin(), widgets.end(),
				   [this](std::shared_ptr<CVisWidget> x){ x->onPaint(this->clientCanvas); } );

	::StretchDIBits(
		hdc, 0, 0, canvas->getWidth(), canvas->getHeight(),
		0, 0, canvas->getWidth(), canvas->getHeight(),
		canvas->getRow0(0), canvas->getBMPINFO(),
		DIB_RGB_COLORS, SRCCOPY );
	
	::ReleaseDC(hWnd, hdc);
}


LRESULT CALLBACK CVisWnd::wndProc(HWND hWnd , UINT msg , WPARAM wp , LPARAM lp)
{
	PAINTSTRUCT ps;
	bool handled = false;

	switch (msg) {
	case WM_CREATE:
		onCreate();
		return 0;

	case WM_CLOSE:
		onClose();
		return 0;
		
	case WM_DESTROY:
		onDestroy();
		return 0;

	// mouse events
	case WM_LBUTTONDBLCLK:
	case WM_LBUTTONDOWN:
	case WM_LBUTTONUP:
	case WM_RBUTTONDBLCLK:
	case WM_RBUTTONDOWN:
	case WM_RBUTTONUP:
	case WM_MOUSEMOVE:
	case WM_MOUSEWHEEL:
		POINT pt, cpt;
		::GetCursorPos(&pt);
		::ScreenToClient(hWnd, &pt);
		cpt = pt;
		windowToClient(cpt);
		if( wcapture ){
			wcapture->onMouseEvent(msg, wp, lp);
		}else{
			for( auto it = sysWidgets.begin(); it != sysWidgets.end(); it++ ){
				RECT rc;
				(*it)->getWindowRect(rc);
				if( (rc.left <= pt.x) && (pt.x < rc.right) &&
					(rc.top  <= pt.y) && (pt.y < rc.bottom) ){
					(*it)->onMouseEvent(msg, wp, lp);
					handled = true;
					break;
				}
			}
			for( auto it = widgets.begin(); it != widgets.end(); it++ ){
				RECT rc;
				(*it)->getWindowRect(rc);
				if( (rc.left <= cpt.x) && (cpt.x < rc.right) &&
					(rc.top  <= cpt.y) && (cpt.y < rc.bottom) ){
					(*it)->onMouseEvent(msg, wp, lp);
					handled = true;
					break;
				}
			}
			if(!handled){
				if(msg == WM_LBUTTONDOWN)
					PostMessage(hWnd, WM_NCLBUTTONDOWN, (WPARAM)HTCAPTION, lp);
				else
					onMouseEvent( msg, wp, lp );
			}
		}
		break;

	case WM_KEYDOWN:
		onKeyDown(wp);
	case WM_KEYUP:
		onKeyUp(wp);

	case WM_PAINT:
		// 画面描画はタイマ駆動で行われるので描画領域更新だけして終わる
		::BeginPaint(hWnd, &ps);
		::EndPaint(hWnd, &ps);
		return 0;
	case WM_ERASEBKGND:
		return 0;

	case WM_COMMAND:
		onCommand((HWND)lp, wp&0xffff, (wp>>16)&0xffff);
		break;

//	case WM_LBUTTONDOWN:
//		break;
#if 0
	case WM_MOVING:
		{
			std::map< HWND, CVisWnd* >::iterator it;
			RECT *prc = reinterpret_cast<LPRECT>(lp);
			if( ::GetAsyncKeyState(VK_SHIFT) < 0 )
				break;

			for( it=frameWndMap.begin(); it!=frameWndMap.end(); it++ ){
				if( it->first == hWnd ) continue;
				::GetWindowRect( it->first, &rc );


				if( isClose( prc->left, rc.right ) ){
					if( isClose( prc->top, rc.top ) ){ // 右上-左上
						::OffsetRect( prc, rc.right-prc->left, rc.top-prc->top );
					}else if( isClose( prc->bottom, rc.bottom ) ){ // 右下-左下
						::OffsetRect( prc, rc.right-prc->left, rc.bottom-prc->bottom );
					}else if( rc.top <= prc->top && prc->top <= rc.bottom ){
						::OffsetRect( prc, rc.right-prc->left, 0 );
					}
				}else if( isClose( prc->right, rc.left ) ){
					if( isClose( prc->top, rc.top ) ){ // 左上-右上
						::OffsetRect( prc, rc.left-prc->right, rc.top-prc->top );
					}else if( isClose( prc->bottom, rc.bottom ) ){ // 左下-右下
						::OffsetRect( prc, rc.left-prc->right, rc.bottom-prc->bottom );
					}else if( rc.top <= prc->top && prc->top <= rc.bottom ){
						::OffsetRect( prc, rc.left-prc->right, 0 );
					}
				}
				else if( isClose( prc->left, rc.left ) ){
					if( isClose( prc->top, rc.bottom ) ){
						::OffsetRect( prc, rc.left-prc->left, rc.bottom-prc->top );
					}else if( isClose( prc->bottom, rc.top ) ){
						::OffsetRect( prc, rc.left-prc->left, rc.top-prc->bottom );
					}
				}
				else if( isClose( prc->right, rc.right ) ){
					if( isClose( prc->top, rc.bottom ) ){
						::OffsetRect( prc, rc.right-prc->right, rc.bottom-prc->top );
					}else if( isClose( prc->bottom, rc.top ) ){
						::OffsetRect( prc, rc.right-prc->right, rc.top-prc->bottom );
					}
				}
			}
		}
#endif
		break;

	default:
		return DefWindowProc(hWnd , msg , wp , lp);

	}
	
	return 0;
}

LRESULT CALLBACK CVisWnd::wndProcDispatcher(HWND hWnd , UINT msg , WPARAM wp , LPARAM lp)
{
	auto it = wndMap.find( hWnd );
	if( it != wndMap.end() )
		return it->second->wndProc(hWnd,msg,wp,lp);
	else if( creatingWnd ){
		wndMap.insert( std::pair<HWND,CVisWnd*>(hWnd, creatingWnd) );
		return creatingWnd->wndProc(hWnd,msg,wp,lp);
	}

	assert(0);
	return -1;
}



bool CVisWnd::create( int width, int height, DWORD exstyle, DWORD style, HWND parent )
{
	WNDCLASS winc;

	int left = gConfig.getInt( windowClass.c_str(), INIKEY_WNDLEFT, INT_MIN );
	int top  = gConfig.getInt( windowClass.c_str(), INIKEY_WNDTOP,  INT_MIN );

	wndWidth = width;
	wndHeight = height;
	hParent = parent;

	// 位置指定されていないときはメインモニタ中央に配置
	if( left == INT_MIN || top == INT_MIN ){
		int wx = GetSystemMetrics(SM_CXSCREEN);
		int wy = GetSystemMetrics(SM_CYSCREEN);
		left = (wx - width) / 2;
		top  = (wy - height) / 2;
	}

	// ウィンドウ位置が画面からはみ出ているときの処理
	// NOTE: マルチモニタ環境では全体包括サイズのみで判定（＝手抜き）
	int wvl = GetSystemMetrics(SM_XVIRTUALSCREEN);
	int wvt = GetSystemMetrics(SM_YVIRTUALSCREEN);
	int wvr = wvl+GetSystemMetrics(SM_CXVIRTUALSCREEN);
	int wvb = wvt+GetSystemMetrics(SM_CYVIRTUALSCREEN);
	if( left < wvl ) left = wvl;
	if( top < wvt ) top = wvt;
	if( wvr <= left+width ) left = wvr - width-1;
	if( wvb <= top+height ) top = wvb - height-1;

	canvas = new CVisBitmap( width, height );
	clientCanvas = new CVisChildBitmap( canvas, 2, 17, width-4, height-19 );

	HINSTANCE hinst = C86CtlMain::getInstanceHandle();

	winc.style			= CS_HREDRAW | CS_VREDRAW;
	winc.lpfnWndProc	= wndProcDispatcher;
	winc.cbClsExtra		= winc.cbWndExtra = 0;
	winc.hInstance		= hinst;
	winc.hIcon			= LoadIcon(hinst, MAKEINTRESOURCE(IDI_ICON_C86CTL));
	winc.hCursor		= LoadCursor(NULL , IDC_ARROW);
	winc.hbrBackground	= (HBRUSH)GetStockObject(BLACK_BRUSH);
	winc.lpszMenuName	= NULL;
	winc.lpszClassName	= windowClass.c_str();

	if (!RegisterClass(&winc))
		return false;

	::WaitForSingleObject( hCreatingMutex, INFINITE );
	creatingWnd = this;
	hWnd = CreateWindowEx(
		exstyle,
		windowClass.c_str(), windowTitle.c_str(),
		(style&~WS_VISIBLE),
		left, top, width, height,
		hParent, NULL, hinst, NULL
	);

	creatingWnd = NULL;
	::ReleaseMutex( hCreatingMutex );

	closeButton = CVisCloseButtonPtr(new CVisCloseButton(this,width-17,2));
	closeButton->pushEvent.push_back(
		[this](CVisWidget* w){
			this->close();
	});
	sysWidgets.push_back(closeButton);
	
	if(!hWnd) return false;

	CVisManager::getInstance()->add( this );
	return true;
}

bool CVisWnd::resize(int width, int height)
{
	if( wndWidth == width && wndHeight == height )
		return true;

	RECT rc;
	::GetWindowRect(hWnd, &rc);
	::MoveWindow(hWnd, rc.left, rc.top, width, height, FALSE);

	if( canvas ){
		delete canvas;
		canvas = new CVisBitmap( width, height );
	}
	if( clientCanvas ){
		delete clientCanvas;
		clientCanvas = new CVisChildBitmap( canvas, 2, 17, width-4, height-19 );
	}
	wndWidth = width;
	wndHeight = height;

	return true;
}

void CVisWnd::onDestroy()
{
	saveWndPos();
}

void CVisWnd::close()
{
	std::for_each( closeEvent.begin(), closeEvent.end(),
				   [this](std::function<void(CVisWnd*)> h){ h(this); } );
	
	saveWndPos();

	CVisManager::getInstance()->del( this );

	widgets.clear();
	sysWidgets.clear();
	
	if(hWnd){
		DestroyWindow( hWnd );
		wndMap.erase( hWnd );
		hWnd = NULL;
	}
	if( clientCanvas ){
		delete clientCanvas;
		clientCanvas = NULL;
	}
	if( canvas ){
		delete canvas;
		canvas = NULL;
	}
	hParent = NULL;

	::UnregisterClass( windowClass.c_str(), C86CtlMain::getInstanceHandle() );
}

void CVisWnd::saveWndPos(void)
{
	RECT rc;
	HWND hwnd = getHWND();
	if( hwnd ){
		if( ::GetWindowRect( hwnd, &rc ) ){
			gConfig.writeInt( windowClass.c_str(), INIKEY_WNDLEFT, rc.left );
			gConfig.writeInt( windowClass.c_str(), INIKEY_WNDTOP, rc.top );
		}
	}
}

