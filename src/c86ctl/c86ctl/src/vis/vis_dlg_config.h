/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#pragma once

namespace c86ctl{
namespace vis {

class CVisDlgConfig
{
public:
	static void create(HWND hParent);
	static void destroy();

private:
	static INT_PTR CALLBACK dlgProc(HWND hWnd , UINT msg , WPARAM wp , LPARAM lp);
	static void onCommand(HWND hwnd, DWORD id, DWORD notifyCode);
	static void onInitDialog(HWND hWnd);
	static void onDelayEditNotify(HWND hwnd, DWORD id, DWORD notifyCode);
	static void onSSGVolEditNotify(HWND hwnd, DWORD id, DWORD notifyCode);
	static void onPLLClockCmbNotify(HWND hwnd, DWORD id, DWORD notifyCode);
	static void onShowVisCheckNotify(HWND hwnd, DWORD notifyCode);
	
private:
	static HWND hDlg;
	static bool isInit;

};

};
};

