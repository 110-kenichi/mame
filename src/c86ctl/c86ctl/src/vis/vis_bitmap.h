/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include <memory>

#pragma once

namespace c86ctl{
namespace vis {

class CVisChildBitmap;

interface IVisBitmap
{
public:
	// accessor
	virtual UCHAR* getRow0(int y) = 0;
	virtual UCHAR* getPtr(int x, int y) = 0;
	virtual int getStep(void) = 0;
	virtual int getWidth(void) = 0;
	virtual int getHeight(void) = 0;
};

class CVisBitmap : public IVisBitmap
{
public:
	// constructor&destructor
	CVisBitmap()
		: hbmp(0),
		  pBits(0),
		  width(0),
		  height(0),
		  step(0),
		  bitmap(0)
	{
	};

	CVisBitmap(int w, int h)
		: hbmp(0),
		  pBits(0),
		  width(w),
		  height(h),
		  step(0),
		  bitmap(0)
	{
		createDIB(w,h);
	};
	
	virtual ~CVisBitmap(){};
	
public:
	// construction
	static CVisBitmap* LoadFromResource(UINT pResourceID, LPCTSTR pResourceType, HMODULE hInstance=NULL);


public:
	// IVisBitmap
	virtual UCHAR* getRow0(int y){ return (pBits + y*step); };
	virtual UCHAR* getPtr(int x, int y){ return (pBits + y*step + x*4); };
	virtual int getStep(void){ return step; };
	virtual int getWidth(void){ return width; };
	virtual int getHeight(void){ return height; };
	
public:
	// accessor
	const BITMAPINFO* getBMPINFO(void){ return &bmpinfo; };


protected:
	void createDIB(int width, int height);
	void deleteDIB();

protected:
	std::shared_ptr<Gdiplus::Bitmap> bitmap;
	
	HBITMAP hbmp;
	BITMAPINFO bmpinfo;
	UCHAR *pBits;
	int width;
	int height;
	int step;
};



class CVisChildBitmap : public IVisBitmap
{
public:
	// constructor & destructor
	CVisChildBitmap( CVisBitmap *p, int x, int y, int w, int h )
		: parent(p), xs(x), ys(y), width(w), height(h)
	{
	};
	
	virtual ~CVisChildBitmap(){
	};

public:
	// IVisBitmap
	virtual UCHAR* getRow0(int y){ return parent->getPtr(xs, y+ys); };
	virtual UCHAR* getPtr(int x, int y){ return parent->getPtr(x+xs, y+ys); };
	virtual int getStep(void){ return parent->getStep(); };
	virtual int getWidth(void){ return width; };
	virtual int getHeight(void){ return height; };

protected:
	int xs;
	int ys;
	int width;
	int height;
	
	CVisBitmap *parent;
};


};
};
