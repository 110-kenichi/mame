/***
	c86win
	
	Copyright (c) 2009-2010, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com

	note: honet
	元々 S98のエディタ用に作ったクラスなので重い＆いろいろ冗長です。
*/
#pragma once

#include <vector>
#include <iostream>
#include <iterator>
#include <algorithm>

//#include "log_common.h"

#define ERR_NONE				0
#define ERR_IO					-10
#define ERR_INVALID_FORMAT		-11


class CS98Header
{
public:
	CS98Header(){};
	~CS98Header(){};

	friend std::istream& operator>>( std::istream &lhs, CS98Header &rhs ){
		UCHAR magic[3];
		lhs.read( (char*)magic, 3 );
		if( magic[0] != 'S' || magic[1] != '9' || magic[2] != '8' )
			throw std::exception("Format Error.");
		lhs.read( (char*)&rhs.version, 1 );
		if( rhs.version != '1' && rhs.version != '3' )
			throw std::exception("Unknown Version File.");
		lhs.read( (char*)&rhs.timer_info[0], 8 );
		lhs.read( (char*)&rhs.compressing, 4 );
		if( rhs.compressing != 0 )
			throw std::exception("Unsupported format.");
		lhs.read( (char*)&rhs.offset_to_tag, 4 );
		lhs.read( (char*)&rhs.offset_to_dump, 4 );
		lhs.read( (char*)&rhs.offset_to_loop_point, 4 );

		if( rhs.version == '1' ){
			rhs.num_device = 0;
			char d[36];
			lhs.read(d, 36);
		}else if( rhs.version == '3' ){
			lhs.read( (char*)&rhs.num_device, 4 );
		}
		return lhs;
	};

	friend std::ostream &operator <<( std::ostream &lhs, const CS98Header &rhs ){
		UCHAR magic[3] = { 'S', '9', '8' };
		lhs.write( (char*)magic, 3 );
		lhs.write( (char*)&rhs.version, 1 );
		lhs.write( (char*)&rhs.timer_info[0], 8 );
		lhs.write( (char*)&rhs.compressing, 4 );
		lhs.write( (char*)&rhs.offset_to_tag, 4 );
		lhs.write( (char*)&rhs.offset_to_dump, 4 );
		lhs.write( (char*)&rhs.offset_to_loop_point, 4 );

		if( rhs.version == '1' ){
			char d[36];
			memset(d, 0, sizeof(d));
			lhs.write(d, 36);
		}else if( rhs.version == '3' ){
			lhs.write( (char*)&rhs.num_device, 4 );
		}
		return lhs;
	};

public:
	UCHAR	version;
	UINT	timer_info[2];
	UINT	compressing;
	UINT	offset_to_tag;
	UINT	offset_to_dump;
	UINT	offset_to_loop_point;
	UINT	num_device;
};

class CS98DeviceInfo
{
public:
	CS98DeviceInfo(){};
	~CS98DeviceInfo(){};

	friend std::istream& operator>>(std::istream &lhs, CS98DeviceInfo &rhs ){
		lhs.read( (char*)&rhs.type, 4 );
		lhs.read( (char*)&rhs.clock, 4 );
		lhs.read( (char*)&rhs.pan, 4 );
		lhs.read( (char*)&rhs.reserved, 4 );
		return lhs;
	};
	friend std::ostream &operator <<( std::ostream &lhs, const CS98DeviceInfo &rhs ){
		lhs.write( (char*)&rhs.type, 4 );
		lhs.write( (char*)&rhs.clock, 4 );
		lhs.write( (char*)&rhs.pan, 4 );
		lhs.write( (char*)&rhs.reserved, 4 );
		return lhs;
	};

public:
	UINT	type;
	UINT	clock;
	UINT	pan;
	UINT	reserved;
};

class CDumpRow
{
public:
	CDumpRow()
	{
		gtick = 0;
		sync = 0;
		len = 0;
		cmd = 0;
	};
	~CDumpRow(){};

	int getTick(){
		return sync;
	};

	friend std::istream& operator>>(std::istream &lhs, CDumpRow &rhs ){
		lhs.read( (char*)&rhs.data[0], 1 );
		rhs.cmd = rhs.data[0];
		if( rhs.data[0] <= 0x7f ){
			lhs.read( (char*)&rhs.data[1], 2 );
			rhs.len = 3;
			rhs.sync = 0;
		}else if( rhs.data[0] == 0xff ){ // 1sync
			rhs.len = 1;
			rhs.sync = 1;
		}else if( rhs.data[0] == 0xfe ){ // nsync
		    int s=0, n=0, l=1;
			UCHAR *d=&rhs.data[1], tmp;
			do{
				lhs.read( (char*)&tmp, 1 );
				n += (unsigned int)(tmp & 0x7f) << s;
				s += 7;
				*d++ = tmp;
				l++;
			}while( tmp&0x80 && !lhs.eof() );
			rhs.sync = n+2;
			rhs.len = l;
		}else if( rhs.data[0] == 0xfd ){ // end/loop
			rhs.len = 1;
			rhs.sync = 1;
		}
		return lhs;
	};
	friend std::ostream &operator <<( std::ostream &lhs, const CDumpRow &rhs ){
		lhs.write( (char*)&rhs.data[0], rhs.len );
		return lhs;
	}

	void setSync( unsigned int tick ){
		if( tick == 1 ){
			data[0] = cmd = 0xff;
			len = 1;
			sync = tick;
		}else{
			int c=1, x=0, t=tick;
			data[0] = cmd = 0xfe;
			do{
				x = (t & 0x7f);
				t >>= 7;
				if( t ) x |= 0x80;
				data[c++] = x;
			}while(t);
			len = c;
			sync = tick;
		}
	};
	

	int getDeviceNo(){
		return cmd<=0x7f ? cmd >> 1 : -1;
	};

	bool isSyncData(){
		if( cmd == 0xff || cmd == 0xfe || cmd == 0xfd )
			return true;
		else
			return false;
	};
	int isExtDevice(){
		return cmd<=0x7f ? cmd & 0x01 : -1;
	};

public:
	UCHAR cmd;
	UCHAR data[8];
	int len;
	int sync;
public:
	int gtick;
};

typedef std::pair<CS98DeviceInfo, int/*std::shared_ptr<CS98LogAnalyzer>*/ > DeviceInfo;

class CS98Data
{
public:
	CS98Data();
	~CS98Data();

	INT Initialize()
	{
		devinfo.clear();
		row.clear();
		return TRUE;
	}

	bool insertData( int index, int n )
	{
		auto it = getRowIt(index);
		CDumpRow d;
		d.len = 3;
		memset(d.data, 0, sizeof(d.data));
		row.insert( it, n, d );
		updateGTick();

		return true;
	};
	
	bool insertSync( int index, unsigned int tick )
	{
		CDumpRow d;
		d.setSync(tick);
		auto it = getRowIt(index);
		row.insert(it, d);
		updateGTick();

		return true;
	};
	bool insertEndLoop( int index )
	{
		CDumpRow d;
		d.len = 1;
		d.cmd = d.data[0] = 0xfd;
		auto it = getRowIt(index);
		row.insert(it, d);
		updateGTick();
		return true;
	};
	
	bool deleteData( std::vector<int> &lst )
	{
		std::sort( lst.begin(), lst.end() );
		int idx = 0;
		auto tit = lst.begin();
		auto it = row.begin();
		for( ; it != row.end() && tit != lst.end(); ){
			if( idx == *tit ){
				// 末尾のEND/LOOPは削除禁止
				if( it != row.end()-1 || it->cmd != 0xfd ){
					it = row.erase( it );
					tit++;
				}
			}else{
				it++;
			}
			idx++;
		}
		updateGTick();
		return true;
	};
	
	void updateGTick()
	{
		int gtick=0;
		for( std::vector<CDumpRow>::iterator it = row.begin(); it != row.end(); it++ ){
			it->gtick = gtick;
			gtick += it->getTick();
		}
	};


	INT loadFile(CString &fname);
	INT saveFile(CString &fname);

	// tick当たりの時間を返す。単位:ms
	DOUBLE getTimerPrec()
	{
		int numerator = header.timer_info[0] == 0 ? 10 : header.timer_info[0];
		int denominator = header.timer_info[1] == 0 ? 1000 : header.timer_info[1];

		return numerator*1000 / denominator;
	};

private:
	std::vector<CDumpRow>::iterator getRowIt(int index)
	{
		if( index<0 ) return row.begin();
		if( row.size() < index ) return row.end();

		auto it = row.begin();
		std::advance(it,index);
		return it;
	};

	size_t getTotalRowSize(){
		size_t sz = 0;
		std::for_each( row.begin(), row.end(), [&sz](const CDumpRow &d){ sz += d.len; } );
		return sz;
	};
	size_t getLoopOffsetPoint(){
		size_t sz = 0;
		std::for_each( row.begin(), 
			std::find_if( row.begin(), row.end(), [](const CDumpRow &d) -> bool { return d.cmd == 0xfd; } ),
			[&sz](const CDumpRow &d){ sz += d.len; } );
		return sz;

	};
	
public:
	CS98Header header;
	std::vector<char> songinfo;
	std::vector<DeviceInfo> devinfo;
	std::vector<CDumpRow> row;
	UINT loopidx;
};

