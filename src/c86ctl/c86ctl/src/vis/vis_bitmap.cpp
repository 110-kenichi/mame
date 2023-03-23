/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#include "stdafx.h"

#include <Windows.h>
#include <tchar.h>
#include "vis_bitmap.h"

#pragma comment(lib, "Gdiplus.lib")

//#ifdef _DEBUG
//#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
//#endif

using namespace Gdiplus;
using namespace c86ctl;
using namespace c86ctl::vis;

void CVisBitmap::createDIB(int w, int h)
{
	if( hbmp )
		deleteDIB();
	
	ZeroMemory( &bmpinfo, sizeof(bmpinfo) );
	bmpinfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	bmpinfo.bmiHeader.biBitCount = 32;
	bmpinfo.bmiHeader.biPlanes = 1;
	bmpinfo.bmiHeader.biWidth = w;
	bmpinfo.bmiHeader.biHeight = -h;
	width = w;
	height = h;
	step = w*4;

	hbmp = CreateDIBSection( NULL, &bmpinfo, DIB_RGB_COLORS, (void**)(&pBits), NULL, 0 );
	bitmap = std::shared_ptr<Bitmap>( new Bitmap( hbmp, NULL ) );
}

void CVisBitmap::deleteDIB()
{
	DeleteObject(hbmp);
	hbmp = NULL;
	width = 0;
	height = 0;
	step = 0;
}

CVisBitmap* CVisBitmap::LoadFromResource(UINT pResourceID, LPCTSTR pResourceType, HMODULE hInstance)
{
	LPCTSTR pResourceName = MAKEINTRESOURCE(pResourceID);

	HRSRC hResource = ::FindResource(hInstance, pResourceName, pResourceType);
	if(!hResource) throw _T("failed to find resource");

	DWORD dwResourceSize = ::SizeofResource(hInstance, hResource);
	if(!dwResourceSize) throw _T("resource size is zero");

	const void* pResourceData = ::LockResource(LoadResource(hInstance, hResource));
	if(!pResourceData) throw _T("failed to load resource");

	HGLOBAL hResourceBuffer = ::GlobalAlloc(GMEM_MOVEABLE, dwResourceSize);
	if(!hResourceBuffer) throw _T("faild to allocate global heap memory");

	void* pResourceBuffer = GlobalLock(hResourceBuffer);
	if(!pResourceBuffer){
		::GlobalFree(hResourceBuffer);
		throw _T("failed to lock memory");
	}

	CopyMemory(pResourceBuffer, pResourceData, dwResourceSize);
	IStream* pIStream = NULL;
	if(CreateStreamOnHGlobal(hResourceBuffer, FALSE, &pIStream) != S_OK){
		GlobalUnlock(hResourceBuffer);
		GlobalFree(hResourceBuffer);
		throw _T("failed to create stream");
	}

	std::shared_ptr<Bitmap> tempBmp = std::shared_ptr<Bitmap>(Bitmap::FromStream(pIStream));
	pIStream->Release();
	
	GlobalUnlock(hResourceBuffer);
	GlobalFree(hResourceBuffer);

	if(!tempBmp) throw _T("failed to load bitmap");

	Gdiplus::Status result = tempBmp->GetLastStatus();
	if(result!=Gdiplus::Ok){
		tempBmp = NULL;
		throw _T("failed to load bitmap");
	}


	CVisBitmap *visbmp;
	//bitmap = tempBmp;
	int w = tempBmp->GetWidth();
	int h = tempBmp->GetHeight();
	visbmp = new CVisBitmap(w,h);
	
	auto bmpData = std::shared_ptr<BitmapData>(new BitmapData);
	Rect rc(0, 0, w, h);
	if( Ok != tempBmp->LockBits( &rc, ImageLockModeRead, PixelFormat32bppARGB, bmpData.get() ) ){
		throw _T("failed to lock bmp");
	}
	
	if( visbmp->getStep() == bmpData->Stride ){
		memcpy( visbmp->getRow0(0), bmpData->Scan0, visbmp->getStep()*h );
	}else{
		for( int y=0; y<h; y++ )
			memcpy( visbmp->getRow0(y), bmpData->Scan0, w*4 );
	}
	tempBmp->UnlockBits(bmpData.get());

	return visbmp;
}


