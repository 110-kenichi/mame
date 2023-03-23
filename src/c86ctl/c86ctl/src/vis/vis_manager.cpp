/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "StdAfx.h"

#include "vis_manager.h"
#include "vis_c86wnd.h"

#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;
using namespace c86ctl::vis;

CVisManager* CVisManager::pInstance = NULL;

void CVisManager::add( CVisWnd *wnd )
{
	::EnterCriticalSection(&cs);
	clients.push_back(wnd);
	::LeaveCriticalSection(&cs);
}

void CVisManager::del( CVisWnd *wnd )
{
	::EnterCriticalSection(&cs);
	auto ei = std::remove(clients.begin(), clients.end(), wnd);
	clients.erase(ei, clients.end());
	::LeaveCriticalSection(&cs);
}

void CVisManager::draw(void)
{
	::EnterCriticalSection(&cs);
	std::for_each( clients.begin(), clients.end(), [](CVisWnd* x){ x->redraw(); } );
	::LeaveCriticalSection(&cs);

	fps = counter.getFPS();
}

