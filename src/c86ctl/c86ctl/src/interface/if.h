/***
	c86ctl
	I/Fアクセスインタフェース定義 for GIMIC
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
	Thanks to Nagai "Guu" Osamu 2011/12/08 for his advice.
 */
#pragma once
#include <memory>
#include "c86ctl.h"
#include "chip/chip.h"

//#define SUPPORT_MIDI <=全然メンテしていません
#define SUPPORT_WINUSB
#define SUPPORT_HID

namespace c86ctl{

class GimicIF : public IRealChip2, public IGimic2
{
public:
	GimicIF() : refcount(0){ };
	virtual ~GimicIF(){};

public:
	// IUnknown
	virtual HRESULT __stdcall QueryInterface( REFIID riid, LPVOID *ppvObj ){
		if( !ppvObj )
			return ERROR_INVALID_PARAMETER;
		
		if( ::IsEqualIID( riid, IID_IRealChip ) ){
			*ppvObj = static_cast<IRealChip*>(this);
			return NOERROR;
		}else if( ::IsEqualIID( riid, IID_IRealChip2 ) ){
			*ppvObj = static_cast<IRealChip2*>(this);
			return NOERROR;
		}else if( ::IsEqualIID( riid, IID_IGimic ) ){
			*ppvObj = static_cast<IGimic*>(this);
			return NOERROR;
		}else if( ::IsEqualIID( riid, IID_IGimic2 ) ){
			*ppvObj = static_cast<IGimic2*>(this);
			return NOERROR;
		}
		*ppvObj = NULL;
		return E_NOINTERFACE;
	};

	virtual ULONG __stdcall AddRef(VOID){ return ++refcount; };
	virtual ULONG __stdcall Release(VOID){ return --refcount; };

public:
	// IRealChip
	virtual int __stdcall reset(void){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual void __stdcall out( UINT addr, UCHAR data){};
	virtual UCHAR __stdcall in( UINT addr ){ return 0; };

public:
	// IRealChip2
	virtual int __stdcall getChipStatus( UINT addr, UCHAR *status ){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual void __stdcall directOut(UINT addr, UCHAR data){};
	virtual void __stdcall directOut2(DWORD* addr, UCHAR* data, DWORD size, UCHAR type) {};

public:
	// 実験中
	//virtual int __stdcall adpcmZeroClear(void){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	//virtual int __stdcall adpcmWrite( UINT startAddr, UINT size, UCHAR *data ){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	//virtual int __stdcall adpcmRead( UINT startAddr, UINT size, UCHAR *data ){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	
public:
	// 非公開
	virtual int __stdcall setDelay(int delay){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual int __stdcall getDelay(int *delay){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual int __stdcall isValid(void){ return 0; };

public:
	// IGimic
	virtual int __stdcall setSSGVolume(UCHAR vol){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual int __stdcall getSSGVolume(UCHAR *vol){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual int __stdcall setPLLClock(UINT clock){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual int __stdcall getPLLClock(UINT *clock){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual int __stdcall getFWVer( UINT *major, UINT *minor, UINT *rev, UINT *build ){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual int __stdcall getMBInfo(struct Devinfo *info){ return C86CTL_ERR_NOT_IMPLEMENTED; };
	virtual int __stdcall getModuleInfo(struct Devinfo *info){ return C86CTL_ERR_NOT_IMPLEMENTED; };
public:
	// IGimic2
	virtual int __stdcall getModuleType(enum ChipType *type){ return C86CTL_ERR_NOT_IMPLEMENTED; };


public:
	// 非公開
	virtual int init(void){ return 0; };
	virtual void tick(void){};
	virtual void update(void){};
	virtual UINT getCPS(void){ return 0; };
	virtual void checkConnection(void){};

	virtual Chip* getChip(){ return 0; };
	virtual const GimicParam* getParam(){ return 0; };

public:
	


protected:
	int refcount;
};

typedef std::shared_ptr<GimicIF> GimicIFPtr;

};
