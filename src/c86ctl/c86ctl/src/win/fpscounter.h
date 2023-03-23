/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#pragma once
#include <list>

namespace c86ctl{

class FPSCounter{
public:
	FPSCounter();
	~FPSCounter();

	double getFPS();
	void setNSample(int n){
		sample.resize(n, 0.0);
		nsamples = n;
		dsum = 0;
	};
	
private:
	double update(double d);
	
private:
	UINT lastcount;
	
	std::list<double> sample;
	int nsamples;
	double dsum;
};

};

