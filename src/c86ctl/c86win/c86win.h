
// c86win.h : PROJECT_NAME アプリケーションのメイン ヘッダー ファイルです。
//

#pragma once

#include "c86ctl.h"

#ifndef __AFXWIN_H__
	#error "PCH に対してこのファイルをインクルードする前に 'stdafx.h' をインクルードしてください"
#endif

#include "resource.h"		// メイン シンボル


// C86winApp:
// このクラスの実装については、c86win.cpp を参照してください。
//

class C86winApp : public CWinApp
{
public:
	C86winApp();

// オーバーライド
public:
	virtual BOOL InitInstance();

// 実装
public:
	HMODULE hC86DLL;
	c86ctl::IRealChipBase *pChipBase;
	
	DECLARE_MESSAGE_MAP()
};

extern C86winApp theApp;

