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
#include <typeinfo>

#include "config.h"
#include "version.h"
#include "resource.h"
#include "vis_c86sub.h"
#include "vis_c86main.h"
#include "c86ctlmain.h"
#include "c86ctlmainwnd.h"


#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

#define INIKEY_REGWND		TEXT("m%d_regwnd")
#define INIKEY_KEYWND		TEXT("m%d_keywnd")
#define INIKEY_FMWND		TEXT("m%d_fm%d_wnd")

using namespace c86ctl;
using namespace c86ctl::vis;

static const int modHeight = 74;

#define KEYBUFLEN 32

bool CVisC86Main::update()
{
	// CHECKME: この時点でattachされてないといけない。
	auto gimics = GetC86CtlMain()->getGimics();
	int sz = static_cast<int>(info.size());
	int st = sz;

	if( sz == gimics.size() )
		return true;

	sz = gimics.size();
	info.resize(sz);
	for( int i=0; i<sz; i++ ){
		gimics[i]->getMBInfo(&info[i].mbinfo);
		gimics[i]->getFWVer(&info[i].major, &info[i].minor, &info[i].rev, &info[i].build);
		gimics[i]->getModuleInfo(&info[i].chipinfo);
		gimics[i]->getModuleType(&info[i].chiptype);
	}

	// frame=19 + topinfo=40 + module=74*N + bottominfo=10
	int windowHeight = 19+40+modHeight*sz+10;
	int windowWidth = 334;

	if( !CVisWnd::resize( windowWidth, windowHeight ) )
		return false;
	
	int y=40+modHeight*st;
	
	for( int i=st; i<sz; i++ ){
		TCHAR key[KEYBUFLEN];
		Chip *pchip = gimics[i]->getChip();
		hwinfo *pinfo = &info[i];
		if (!pchip || !pinfo) continue;

		_sntprintf(key, KEYBUFLEN, INIKEY_REGWND, i);
		pinfo->regView = visC86RegViewFactory(pchip, i);
		if (pinfo->regView) {
			pinfo->regView->closeEvent.push_back(
				[pinfo](CVisWnd* h) {
					pinfo->checkReg->setCheck(0, false);
				});
		}

		pinfo->checkReg = CVisCheckBoxPtr(new CVisCheckBox(this,260,y, "REGISTER"));
		if (pinfo->checkReg) {
			pinfo->checkReg->changeEvent.push_back(
				[this, pinfo, i](CVisWidget* w) {
					TCHAR key[KEYBUFLEN];
					_sntprintf(key, KEYBUFLEN, INIKEY_REGWND, i);

					if (dynamic_cast<CVisCheckBox*>(w)->getValue()) {
						if (pinfo->regView)
							pinfo->regView->create(hWnd);
						gConfig.writeInt(windowClass.c_str(), key, 1);
					}
					else {
						if (pinfo->regView)
							pinfo->regView->close();
						gConfig.writeInt(windowClass.c_str(), key, 0);
					}
				});
			widgets.push_back(pinfo->checkReg);
			if (gConfig.getInt(windowClass.c_str(), key, 0)) {
				info[i].checkReg->setCheck(1, true);
			}
		}

		if( pinfo->chiptype == CHIP_OPNA ||
			pinfo->chiptype == CHIP_OPN3L ||
			pinfo->chiptype == CHIP_OPM ){

			_sntprintf(key, KEYBUFLEN, INIKEY_KEYWND, i);
			pinfo->keyView = visC86KeyViewFactory(pchip, i);
			if (pinfo->keyView) {
				pinfo->keyView->closeEvent.push_back(
					[pinfo](CVisWnd* h) {
						pinfo->checkKey->setCheck(0, false);
					});
			}
			
			pinfo->checkKey = CVisCheckBoxPtr(new CVisCheckBox(this,180,y, "KEYBOARD"));
			if (pinfo->checkKey) {
				pinfo->checkKey->changeEvent.push_back(
					[this, pinfo, i](CVisWidget* w) {
						TCHAR key[KEYBUFLEN];
						_sntprintf(key, KEYBUFLEN, INIKEY_KEYWND, i);

						if (dynamic_cast<CVisCheckBox*>(w)->getValue()) {
							if (pinfo->keyView)
								pinfo->keyView->create(hWnd);
							gConfig.writeInt(windowClass.c_str(), key, 1);
						}
						else {
							if (pinfo->keyView)
								pinfo->keyView->close();
							gConfig.writeInt(windowClass.c_str(), key, 0);
						}
					});
				widgets.push_back(pinfo->checkKey);
				if (gConfig.getInt(windowClass.c_str(), key, 0)) {
					pinfo->checkKey->setCheck(1, true);
				}
			}

			int nch = 6;
			if( info[i].chiptype == CHIP_OPM )
				nch = 8;
			
			for( int ch=0; ch<nch; ch++ ){
				char str[10];
				sprintf(str, "FM%d", ch+1);
				
				_sntprintf(key, KEYBUFLEN, INIKEY_FMWND, i, ch+1);
				pinfo->fmView[ch] = visC86FmViewFactory(pchip, i, ch);
				if (pinfo->fmView[ch]) {
					pinfo->fmView[ch]->closeEvent.push_back(
						[pinfo, ch](CVisWnd* h) {
							pinfo->checkFM[ch]->setCheck(0, false);
						});

					pinfo->checkFM[ch] = CVisCheckBoxPtr(new CVisCheckBox(this, 180 + 40 * (ch % 3), y + ((ch / 3) + 1) * 14, str));
					pinfo->checkFM[ch]->changeEvent.push_back(
						[this, pinfo, i, ch](CVisWidget* w) {
							TCHAR key[KEYBUFLEN];
							_sntprintf(key, KEYBUFLEN, INIKEY_FMWND, i, ch + 1);

							if (dynamic_cast<CVisCheckBox*>(w)->getValue()) {
								if (pinfo->fmView[ch])
									pinfo->fmView[ch]->create(hWnd);
								gConfig.writeInt(windowClass.c_str(), key, 1);
							}
							else {
								if (pinfo->fmView[ch])
									pinfo->fmView[ch]->close();
								gConfig.writeInt(windowClass.c_str(), key, 0);
							}
						});
					widgets.push_back(info[i].checkFM[ch]);
					if (gConfig.getInt(windowClass.c_str(), key, 0)) {
						info[i].checkFM[ch]->setCheck(1, true);
					}
				}
			}
		}

		/*
		info[i].checkSSG = CVisCheckBoxPtr(new CVisCheckBox(this,180,y+42, "SSG"));
		widgets.push_back(info[i].checkSSG);

		info[i].checkRHYTHM = CVisCheckBoxPtr(new CVisCheckBox(this,220,y+42, "RHYTHM"));
		widgets.push_back(info[i].checkRHYTHM);

		info[i].checkADPCM = CVisCheckBoxPtr(new CVisCheckBox(this,280,y+42, "ADPCM"));
		widgets.push_back(info[i].checkADPCM);
		 */

		y+=modHeight;
	}

	return true;
}

bool CVisC86Main::create(HWND parent)
{
	// frame=19 + topinfo=40 + module=74*N + bottominfo=10
	int sz = static_cast<int>(info.size());
	int windowHeight = 19+40+modHeight*sz+10;
	int windowWidth = 334;
	
	if ( !CVisWnd::create( windowWidth, windowHeight, 0, WS_POPUP | WS_CLIPCHILDREN, NULL ) ){
		return false;
	}
	hMaster = parent;

	update();
	::ShowWindow( hWnd, SW_SHOWNOACTIVATE );

	return true;
}

void CVisC86Main::onPaintClient()
{
	auto gimic = GetC86CtlMain()->getGimics();

	// NOTE: gimic.size != info.size() となることがあるので注意
	size_t sz = info.size();

	CVisC86Skin *skin = &gVisSkin;
	const int maxlen = 256;
	char str[maxlen];

	int cw = clientCanvas->getWidth();
	int ch = clientCanvas->getHeight();

	// 背景消去
	visFillRect( clientCanvas, 0, 0, cw, ch, ARGB(255,0,0,0) );
	
	// ロゴ・バージョン
	skin->drawLogo( clientCanvas, 2, 2 );
	skin->drawStr( clientCanvas, 0, 130, 7, "OPx STATUS DISP FOR GIMIC/C86BOX" );
	skin->drawStr( clientCanvas, 0, 130, 17, "(C)HONET " VERSION_MESSAGE );

	// 横線
	int w = clientCanvas->getWidth()-4;
	UINT *p = (UINT*)clientCanvas->getPtr(2,28);
	int step = clientCanvas->getStep()/2;
	int r=199,g=200,b=255;
	for( int x=0; x<w; x++ ){
		int d = tick*2+(w-x);
		int a = abs(d%512 - 255);
		*p = *(p+step) = ARGB(255,(r*a)>>8,(g*a)>>8,(b*a)>>8);
		p++;
	}
	tick++;

	int y=40;
	for( size_t i=0; i<sz; i++ ){
		int dy=0;
		// 枠線
		visFillRect( clientCanvas, 5, y-2, 5, 68, skin->getPal(CVisC86Skin::IDCOL_MID) );
		visFillRect( clientCanvas, 5, y+64, 320, 2, skin->getPal(CVisC86Skin::IDCOL_MID) );

		if( gimic[i]->isValid() ){
			const GimicParam* param = gimic[i]->getParam();
			// デバイス名
			sprintf(str, "MODULE%d: %s Rev.%c", i, &info[i].mbinfo.Devname[0], info[i].mbinfo.Rev );
			skin->drawStr( clientCanvas, 1, 15, y+dy, str ); dy+=10;
			// Firmware version
			sprintf( str, "FW-VER : %d.%d.%d.%d", info[i].major, info[i].minor, info[i].rev, info[i].build );
			skin->drawStr( clientCanvas, 1, 15, y+dy, str ); dy+=10;
			// MODULE名
			sprintf(str, "CHIP   : %s Rev.%c", &info[i].chipinfo.Devname[0], info[i].chipinfo.Rev );
			skin->drawStr( clientCanvas, 1, 15, y+dy, str ); dy+=10;
			// PLL Clock
			sprintf(str, "CLOCK  : %d Hz", param->clock );
			skin->drawStr( clientCanvas, 1, 15, y+dy, str ); dy+=10;
			// SSG-Volume
			sprintf(str, "SSG-VOL: %d", param->ssgVol );
			skin->drawStr( clientCanvas, 1, 15, y+dy, str ); dy+=10;
			// カロリー(w
			sprintf(str, "CALORIE: %d CPS", gimic[i]->getCPS() );
			skin->drawStr( clientCanvas, 1, 15, y+dy, str ); dy+=10;
		}else{
			sprintf(str, "MODULE%d: DISCONNECTED", i );
			skin->drawStr( clientCanvas, 1, 15, y+dy, str ); dy+=10;
			skin->drawStr( clientCanvas, 1, 15, y+dy, "FW-VER :" ); dy+=10;
			skin->drawStr( clientCanvas, 1, 15, y+dy, "CHIP   :" ); dy+=10;
			skin->drawStr( clientCanvas, 1, 15, y+dy, "CLOCK  :" ); dy+=10;
			skin->drawStr( clientCanvas, 1, 15, y+dy, "SSG-VOL:" ); dy+=10;
			skin->drawStr( clientCanvas, 1, 15, y+dy, "CALORIE:" ); dy+=10;
		}
		y+=modHeight;
	}

	if( sz == 0 ){
		skin->drawStr( clientCanvas, 0, 5, ch-12, "NO DEVICE!" );
	}

	// FPS
	sprintf(str, "FPS: %0.1f", CVisManager::getInstance()->getCurrentFPS() );
	skin->drawStr( clientCanvas, 0, 260, ch-12, str );

}

void CVisC86Main::onMouseEvent(UINT msg, WPARAM wp, LPARAM lp)
{
	POINT point;
	GetCursorPos(&point);

	switch(msg){
	case WM_RBUTTONUP:
		::SetForegroundWindow(hWnd);
		
		HMENU hMenu = ::LoadMenu(C86CtlMain::getInstanceHandle(), MAKEINTRESOURCE(IDR_MENU_SYSPOPUP));
		if( !hMenu )
			break;
		HMENU hSubMenu = ::GetSubMenu(hMenu, 0);
		if( !hSubMenu ){
			::DestroyMenu(hMenu);
			break;
		}
		::CheckMenuItem(hMenu, ID_POPUP_SHOWVIS, MF_BYCOMMAND | MFS_CHECKED );
		
		TrackPopupMenu(hSubMenu, TPM_LEFTALIGN, point.x, point.y, 0, hWnd, NULL);
		::DestroyMenu(hMenu);
	}
}
void CVisC86Main::onCommand(HWND hwnd, DWORD id, DWORD notifyCode)
{
	switch(id){
	case ID_POPUP_CONFIG:
		C86CtlMainWnd::getInstance()->openConfigDialog();
		break;
	case ID_POPUP_SHOWVIS:
		::PostMessage(hMaster, WM_COMMAND, (notifyCode<<16)|id, (DWORD)hwnd);

		break;
	}
}
