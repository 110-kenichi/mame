/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com


	note: honet.kk
	iniファイルベースの仕組みで実装しているのは
	単にテキストエディタで編集出来るファイルに設定を
	保存したいという作者の趣味なので大した意味はありません。
 */
#pragma once

#include <tchar.h>

#define INISC_MAIN		TEXT("c86ctl")
#define INISC_KEY		TEXT("c86key")
#define INISC_REG		TEXT("c86reg")
#define INISC_FMx		TEXT("c86fm%d")

#define INIKEY_WNDLEFT		TEXT("wndleft")
#define INIKEY_WNDTOP		TEXT("wndtop")

#define INIKEY_DELAY			TEXT("m%d_delay")
#define INIKEY_GIMIC_SSGVOL		TEXT("m%d_gimic_ssgvol")
#define INIKEY_GIMIC_PLLCLK		TEXT("m%d_gimic_pllclk")


namespace c86ctl{

class CC86CtlConfig
{
protected:
	TCHAR inipath[_MAX_PATH];
	
public:
	void init(HMODULE hModule);
	
	UINT get( LPCTSTR section, LPCTSTR key, LPCTSTR defstr, LPTSTR val, UINT sz_val ){
		return GetPrivateProfileString( section, key, defstr, val, sz_val, inipath );
	};
	
	BOOL write( LPCTSTR section, LPCTSTR key, LPCTSTR val ){
		return WritePrivateProfileString( section, key, val, inipath );
	};
	
	int getInt( LPCTSTR section, LPCTSTR key, int defval ){
		return GetPrivateProfileInt( section, key, defval, inipath );
	};
	
	BOOL writeInt( LPCTSTR section, LPCTSTR key, INT val );
};

}; // namespace c86ctl

extern class c86ctl::CC86CtlConfig gConfig;

