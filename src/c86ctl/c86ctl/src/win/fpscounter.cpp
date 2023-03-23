/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#include "stdafx.h"

#include <mmsystem.h>
#include "fpscounter.h"

#pragma comment(lib, "winmm.lib")


#ifdef _DEBUG
#define new new(_NORMAL_BLOCK,__FILE__,__LINE__)
#endif

using namespace c86ctl;


FPSCounter::FPSCounter() : dsum(0)
{
	setNSample(30);
	lastcount = ::timeGetTime();
}

FPSCounter::~FPSCounter()
{
}

double FPSCounter::getFPS()
{
	UINT d = ::timeGetTime();
	double fps = update( d-lastcount );

	lastcount = d;	
	return fps;
}

double FPSCounter::update(double d)
{
	dsum -= sample.front();
	sample.pop_front();
	sample.push_back(d);
	dsum += d;

	return dsum>0 ? (1000.0 * nsamples / dsum ) : 0;
}


