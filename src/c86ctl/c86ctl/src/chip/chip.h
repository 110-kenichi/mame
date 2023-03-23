/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#pragma once

namespace c86ctl{

class Chip
{
public:
	Chip(){};
	virtual ~Chip(){};
	
public:
	virtual void reset(){};
	virtual void update(){};

public:
	virtual void filter( int addr, UCHAR *data ){};
	virtual bool setReg( int addr, UCHAR data ){ return false; };
	virtual UCHAR getReg( int addr ){ return 0; };
	virtual void setMasterClock( UINT clock ){};
};

class GimicParam
{
public:
	GimicParam() : ssgVol(0), clock(0) {
	};

	UCHAR ssgVol;
	UINT clock;
};

};

