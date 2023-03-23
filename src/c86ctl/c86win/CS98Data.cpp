/***
	c86win
	
	Copyright (c) 2009-2010, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com

	note: honet
	元々 S98のエディタ用に作ったクラスなので重い＆いろいろ冗長です。
*/
#include "stdafx.h"
#include "CS98Data.h"
#include <iostream>
#include <fstream>
#include "s98.h"

//#include "log_dcsg.h"
//#include "log_opl3.h"
//#include "log_opll.h"
//#include "log_opm.h"
//#include "log_opna.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif

using namespace std;

CS98Data::CS98Data()
{
}

CS98Data::~CS98Data()
{
}

BOOL CS98Data::loadFile( CString &fname )
{
	devinfo.clear();
	row.clear();
	songinfo.clear();

	try{
		ifstream ifs;
		ifs.exceptions( ios::failbit | ios::badbit | ios::eofbit );
		ifs.open(fname, ios::in | ios::binary );

		ifs >> header;

		if( header.version == '1' ){
			header.num_device = 0;
			if( header.offset_to_tag < header.offset_to_dump ){
				size_t sz = header.offset_to_dump - header.offset_to_tag;
				ifs.seekg( header.offset_to_tag, ios::beg );
				songinfo.resize(sz);
				ifs.read( &songinfo[0], sz );
			}

			DeviceInfo d;
			CS98DeviceInfo devi;
			devi.clock = 7987200;
			devi.pan = 0;
			devi.reserved = 0;
			devi.type = YM2608;
			d.first = devi;
			//d.second = std::shared_ptr<CS98LogAnalyzer>(new COPNAAnalyzer() );
			devinfo.push_back(d);

		}else{
			for( unsigned int i=0; i<header.num_device; i++ ){
				DeviceInfo d;
				CS98DeviceInfo devInfo;
				ifs >> devInfo;
				d.first = devInfo;
				switch( devInfo.type ){
				case YM2149:
				case YM2203:
					d.first.clock = 7987200;
					break;
				case YM2612:
				case YM2608:
				case AY_3_8910:
					//d.second = std::shared_ptr<CS98LogAnalyzer>(new COPNAAnalyzer() );
					break;

				case YM2151:
					//d.second = std::shared_ptr<CS98LogAnalyzer>(new COPMAnalyzer() );
					break;

				case YM2413:
					//d.second = std::shared_ptr<CS98LogAnalyzer>(new COPLLAnalyzer() );
					break;

				case YM3526:
				case YM3812:
				case YMF262:
					//d.second = std::shared_ptr<CS98LogAnalyzer>(new COPL3Analyzer() );
					break;

				case SN76489:
					//d.second = std::shared_ptr<CS98LogAnalyzer>(new CDCSGAnalyzer() );
					break;
				}
				devinfo.push_back(d);
			}
		}

		size_t fs = (size_t)ifs.seekg(0,std::ios::end).tellg();
		ifs.seekg( header.offset_to_dump, ios::beg );
		CDumpRow r;
		int last = header.offset_to_tag < header.offset_to_dump ? fs : header.offset_to_tag;
		int offset = header.offset_to_dump;
		loopidx = 0;
		do{
			ifs >> r;
			offset += r.len;
			row.push_back(r);
			if( offset == header.offset_to_loop_point ){
				loopidx = row.size() - 1;
			}
		}while( !ifs.eof() && (ifs.tellg() < last ) );

		if( header.version == '3' ){
			if( header.offset_to_tag ){
				size_t sz = fs - header.offset_to_tag;
				ifs.seekg( header.offset_to_tag, ios::beg );
				songinfo.resize(sz);
				ifs.read( &songinfo[0], sz );
			}
		}

	}catch(ifstream::failure e){
	}

	updateGTick();
	
	return TRUE;
}

BOOL CS98Data::saveFile( CString &fname )
{
	BOOL ret = FALSE;
	try{
		ofstream ofs;
		ofs.exceptions( ios::failbit | ios::badbit | ios::eofbit );
		ofs.open(fname, ios::out | ios::binary );

		// TODO: ヘッダの修正
		if( header.version == '1' ){
			header.offset_to_tag = 0x40;
			header.offset_to_dump = header.offset_to_tag + songinfo.size();
			header.offset_to_loop_point = header.offset_to_dump + getLoopOffsetPoint();
			ofs << header;
			ofs.write( &songinfo[0], songinfo.size() );
			for_each( row.begin(), row.end(), [&ofs](CDumpRow &d){ ofs << d; } );
		}
		else if( header.version == '3' ){
			header.offset_to_dump = 0x1c + 16*devinfo.size();
			header.offset_to_tag = header.offset_to_dump + getTotalRowSize();
			header.offset_to_loop_point = header.offset_to_dump + getLoopOffsetPoint();
			ofs << header;
			for_each( devinfo.begin(), devinfo.end(), [&ofs](DeviceInfo &d){ ofs << d.first; } );
			for_each( row.begin(), row.end(), [&ofs](CDumpRow &d){ ofs << d; } );
			ofs.write( &songinfo[0], songinfo.size() );
		}

		ofs.close();
		ret = TRUE;
	}catch(ofstream::failure e){
		ret = FALSE;
	}

	return ret;
}

