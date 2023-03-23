/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#pragma once

namespace c86ctl{

class critical_section
{
public:
	critical_section()
	{
		::InitializeCriticalSection(&cs);
	};

	critical_section(critical_section &obj)
	{
		::InitializeCriticalSection(&cs);
	};
	
	virtual ~critical_section()
	{
		::DeleteCriticalSection(&cs);
	}


public:
	void lock(){
		::EnterCriticalSection(&cs);
	};
	
	void unlock(){
		::LeaveCriticalSection(&cs);
	};
	
private:
	CRITICAL_SECTION cs;
};


template<class T>
class withlock : public T
{
public:
	withlock(){};
	virtual ~withlock(){};
	
public:
	void lock(){
		cs.lock();
	};
	
	void unlock(){
		cs.unlock();
	};
	
private:
	critical_section cs;
};


};

