/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "stdafx.h"

#include <stdlib.h>
#include <stdio.h>
#include <tchar.h>
#include <assert.h>
#include <process.h>
#include <mmsystem.h>
#include <WinUser.h>
#include <Dbt.h>
#include <ShellAPI.h>

#include <string>
#include <list>
#include <vector>

extern "C" {
#include "hidsdi.h"
}

#include "resource.h"
#include "c86ctl.h"
#include "c86ctlmain.h"
#include "c86ctlmainwnd.h"

#include "config.h"
#include "vis/vis_c86main.h"
#include "ringbuff.h"
#include "interface/if.h"
#include "interface/if_gimic_hid.h"
#include "interface/if_gimic_winusb.h"
#include "interface/if_gimic_midi.h"


#pragma comment(lib,"hid.lib")

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;
using namespace c86ctl::vis;


// ------------------------------------------------------------------
// グローバル変数
C86CtlMain theC86CtlMain;


HINSTANCE C86CtlMain::hInstance = 0;
ULONG_PTR C86CtlMain::gdiToken = 0;
Gdiplus::GdiplusStartupInput C86CtlMain::gdiInput;
HWND C86CtlMain::hActiveDlg = 0;

// ------------------------------------------------------------------
// imprement

C86CtlMain* c86ctl::GetC86CtlMain(void)
{
	return &theC86CtlMain;
}

INT C86CtlMain::init(HINSTANCE h)
{
	hInstance = h;
	return 0;
}
INT C86CtlMain::deinit(void)
{
	hInstance = NULL;
	return 0;
}

HINSTANCE C86CtlMain::getInstanceHandle()
{
	return hInstance;
}

withlock< std::vector< std::shared_ptr<GimicIF> > > &C86CtlMain::getGimics()
{
	return gGIMIC;
}

// ---------------------------------------------------------
// UIメッセージ処理スレッド
unsigned int WINAPI C86CtlMain::threadMain(LPVOID param)
{
	C86CtlMain *pThis = reinterpret_cast<C86CtlMain*>(param);
	MSG msg;
	BOOL b;

	ZeroMemory(&msg, sizeof(msg));
	::PeekMessage(&msg, NULL, 0, 0, PM_NOREMOVE);

	try{
		if( Gdiplus::Ok != Gdiplus::GdiplusStartup(&gdiToken, &gdiInput, NULL) )
			throw "failed to initialize GDI+";

		C86CtlMainWnd *pwnd = C86CtlMainWnd::getInstance();

		pwnd->createMainWnd(param);
		pThis->mainThreadReady = true;

		// メッセージループ
		while( (b = ::GetMessage(&msg, NULL, 0, 0)) ){
			if( b==-1 ) break;
			switch( msg.message ){
			case WM_THREADEXIT:
				pwnd->destroyMainWnd(param);
				break;

			case WM_MYDEVCHANGE:
				//::OutputDebugString(L"DEVICE CHANGED!\r\n");
				//GimicHID::UpdateInstances(pThis->gGIMIC);
				GimicWinUSB::UpdateInstances(pThis->gGIMIC);
				pwnd->deviceUpdate();
				break;
			}

			if( !hActiveDlg || !IsDialogMessage(hActiveDlg, &msg) ){
				::TranslateMessage(&msg);
				::DispatchMessage(&msg);
			}
		}

		C86CtlMainWnd::shutdown();
		Gdiplus::GdiplusShutdown(gdiToken);

		pThis->mainThreadReady = false;
	}
	catch(...){
		::OutputDebugString(_T("ERROR\r\n"));
	}
	
	return (DWORD)msg.wParam;
}



// ---------------------------------------------------------
// 演奏処理スレッド
// mm-timerによる(だいたい)1ms単位処理
// note: timeSetEvent()だと転送処理がタイマ周期より遅いときに
//       再入されるのが怖かったので自前ループにした
unsigned int WINAPI C86CtlMain::threadSender(LPVOID param)
{
	try{
		const UINT period = 1;
		UINT now = ::timeGetTime();
		UINT next = now + period;
		UINT nextSec10 = now + 50;
		C86CtlMain *pThis = reinterpret_cast<C86CtlMain*>(param);

		pThis->senderThreadReady = true;

		while(1){
			if( pThis->terminateFlag )
				break;
		
			now = ::timeGetTime();
			if(now < next){
				if( pThis->terminateFlag ) break;
				Sleep(1);
				//LARGE_INTEGER d;
				//d.QuadPart = 1000; // 100ns-units = 0.1ms
				//NtDelayExecution(FALSE, &d); // delay 100ns-units. <-非公開関数@ntdll.dll
				continue;
			}
			next += period;

			// ここでループ内サイズ確定。
			// 別スレッドでサイズ拡張される事があるので注意。
			size_t sz = pThis->gGIMIC.size(); 

			// update
			for( size_t i=0; i<sz; i++ ){ pThis->gGIMIC[i]->tick(); };
			
			// per 50msec
			if( nextSec10 < now ){
				nextSec10 += 50;
				for( size_t i=0; i<sz; i++ ){ pThis->gGIMIC[i]->update(); };
			}
		}

		pThis->senderThreadReady = false;
	}catch(...){
	}
	
	return 0;
}


// ---------------------------------------------------------
HRESULT C86CtlMain::QueryInterface( REFIID riid, LPVOID *ppvObj )
{
	if( ::IsEqualGUID( riid, IID_IRealChipBase ) ){
		*ppvObj = (LPVOID)this;
		return NOERROR;
	}
	*ppvObj = 0;
	return E_NOINTERFACE;
}

ULONG C86CtlMain::AddRef(VOID)
{
	return ++refCount;
}

ULONG C86CtlMain::Release(VOID)
{
	return --refCount;
}


int C86CtlMain::initialize(void)
{
	int tout;
	const int TIMEOUT=100;

	if( isInitialized )
		return C86CTL_ERR_UNKNOWN;
	
	// インスタンス生成
	//GimicHID::UpdateInstances(gGIMIC);
	GimicWinUSB::UpdateInstances(gGIMIC);
	//gGIMIC = GimicMIDI::CreateInstances(); // deprecated.
	
	// タイマ分解能設定
	TIMECAPS timeCaps;
	if( ::timeGetDevCaps(&timeCaps, sizeof(timeCaps)) == TIMERR_NOERROR ){
		::timeBeginPeriod(timeCaps.wPeriodMin);
		timerPeriod = timeCaps.wPeriodMin;
	}

	// 描画/UIスレッド開始
	hMainThread = (HANDLE)_beginthreadex( NULL, 0, &threadMain, this, 0, &mainThreadID );
	if( !hMainThread )
		return C86CTL_ERR_UNKNOWN;

 	for(tout=0; !mainThreadReady && (tout<TIMEOUT); tout++)
		Sleep(10);

	// 演奏スレッド開始
	hSenderThread = (HANDLE)_beginthreadex( NULL, 0, &threadSender, this, 0, &senderThreadID );
	if( !hSenderThread ){
		::PostThreadMessage( mainThreadID, WM_THREADEXIT, 0, 0 );
		::WaitForSingleObject( hMainThread, INFINITE );
		return C86CTL_ERR_UNKNOWN;
	}

 	for(tout=0; !senderThreadReady && (tout<TIMEOUT); tout++)
		Sleep(10);

	SetThreadPriority( hSenderThread, THREAD_PRIORITY_ABOVE_NORMAL );

	loadConfig();

	isInitialized = true;
	return C86CTL_ERR_NONE;
}


int C86CtlMain::deinitialize(void)
{
	if( !isInitialized )
		return C86CTL_ERR_UNKNOWN;

	reset();

	// 各種スレッド終了

	if( hMainThread ){
		::PostThreadMessage( mainThreadID, WM_THREADEXIT, 0, 0 );
		::WaitForSingleObject( hMainThread, INFINITE );
		hMainThread = NULL;
		mainThreadID = 0;
	}

	if( hSenderThread ){
		terminateFlag = true;
		::WaitForSingleObject( hSenderThread, INFINITE );
		hSenderThread = NULL;
		senderThreadID = 0;
	}
	terminateFlag = false;

	// インスタンス削除
	// note: このタイミングで終了処理が行われる。
	//       gGIMICを参照する演奏・描画スレッドは終了していなければならない。
	gGIMIC.clear();

	// タイマ分解能設定解除
	::timeEndPeriod(timerPeriod);
	isInitialized = false;
	
	mainThreadReady = false;
	senderThreadReady = false;

	return C86CTL_ERR_NONE;
}

void C86CtlMain::loadConfig(void)
{
	TCHAR key[128];
	int val=0;
	
	for( size_t i=0; i<gGIMIC.size(); i++ ){
		_sntprintf(key, sizeof(key), INIKEY_DELAY, i);
		val = gConfig.getInt(INISC_MAIN, key, -1);
		if( val>=0 ) gGIMIC[i]->setDelay(val);

		_sntprintf(key, sizeof(key), INIKEY_GIMIC_SSGVOL, i);
		val = gConfig.getInt(INISC_MAIN, key, -1);
		if( val>=0 ) gGIMIC[i]->setSSGVolume((UCHAR)val);

		_sntprintf(key, sizeof(key), INIKEY_GIMIC_PLLCLK, i);
		val = gConfig.getInt(INISC_MAIN, key, -1);
		if( val>=0 ) gGIMIC[i]->setPLLClock((UINT)val);
	}
}

void C86CtlMain::saveConfig(void)
{
	TCHAR key[128];
	for( size_t i=0; i<gGIMIC.size(); i++ ){
		int delay=0;
		gGIMIC[i]->getDelay(&delay);
		_sntprintf(key, sizeof(key), INIKEY_DELAY, i);
		gConfig.writeInt(INISC_MAIN, key, delay);

		UCHAR vol=0;
		gGIMIC[i]->getSSGVolume(&vol);
		_sntprintf(key, sizeof(key), INIKEY_GIMIC_SSGVOL, i);
		gConfig.writeInt(INISC_MAIN, key, vol);

		UINT clock=0;
		gGIMIC[i]->getPLLClock(&clock);
		_sntprintf(key, sizeof(key), INIKEY_GIMIC_PLLCLK, i);
		gConfig.writeInt(INISC_MAIN, key, clock);
		
	}
}


int C86CtlMain::reset(void)
{
	gGIMIC.lock();
	std::for_each( gGIMIC.begin(), gGIMIC.end(), [](std::shared_ptr<GimicIF> x){ x->reset(); } );
	gGIMIC.unlock();

	return 0;
}

int C86CtlMain::getNumberOfChip(void)
{
	return static_cast<int>(gGIMIC.size());
}

HRESULT C86CtlMain::getChipInterface( int id, REFIID riid, void** ppi )
{
	if( id < gGIMIC.size() ){
		return gGIMIC[id]->QueryInterface( riid, ppi );
	}
	return E_NOINTERFACE;
}

void C86CtlMain::out( UINT addr, UCHAR data )
{
	if( gGIMIC.size() ){
		gGIMIC.front()->out(addr,data);
	}
}

UCHAR C86CtlMain::in( UINT addr )
{
	if( gGIMIC.size() ){
		return gGIMIC.front()->in(addr);
	}else
		return 0;
}



