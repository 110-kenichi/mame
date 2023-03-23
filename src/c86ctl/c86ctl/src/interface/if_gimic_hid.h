/***
	c86ctl
	gimic コントロール HID版
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
	Thanks to Nagai "Guu" Osamu 2011/12/08 for his advice.
 */
#pragma once

#include "if.h"

#ifdef SUPPORT_HID

#include <mmsystem.h>
#include <vector>
#include "ringbuff.h"
#include "withlock.h"
#include "chip/chip.h"


namespace c86ctl{

class GimicHID : public GimicIF
{
// ファクトリ -------------------------------------------------------
public:
	static int UpdateInstances( withlock< std::vector< std::shared_ptr<GimicIF> > > &gimics);

// 公開インタフェイス -----------------------------------------------
public:
	// IGimic
	virtual int __stdcall setSSGVolume(UCHAR vol);
	virtual int __stdcall getSSGVolume(UCHAR *vol);
	virtual int __stdcall setPLLClock(UINT clock);
	virtual int __stdcall getPLLClock(UINT *clock);
	virtual int __stdcall getMBInfo(struct Devinfo *info);
	virtual int __stdcall getModuleInfo(struct Devinfo *info);
	virtual int __stdcall getFWVer( UINT *major, UINT *minor, UINT *rev, UINT *build );
public:
	// IGimic2
	virtual int __stdcall getModuleType(enum ChipType *type);

public:
	// IRealChip
	virtual int __stdcall reset(void);
	virtual void __stdcall out(UINT addr, UCHAR data);
	virtual UCHAR __stdcall in( UINT addr );

public:
	// IRealChip2
	virtual int __stdcall getChipStatus( UINT addr, UCHAR *status );
	virtual void __stdcall directOut(UINT addr, UCHAR data);
	virtual void __stdcall directOut2(DWORD* addr, UCHAR* data, DWORD size);


// 実験中 -----------------------------------------------
public:
//	virtual int __stdcall adpcmZeroClear(void);
//	virtual int __stdcall adpcmWrite( UINT startAddr, UINT size, UCHAR *data );
//	virtual int __stdcall adpcmRead( UINT startAddr, UINT size, UCHAR *data );
	virtual int __stdcall setDelay(int d);
	virtual int __stdcall getDelay(int *d);
	virtual int __stdcall isValid(void);

// C86CTL内部利用 ---------------------------------------------------
private:
	GimicHID(HANDLE h);

public:
	~GimicHID(void);

public:
	virtual int init(void);
	virtual void tick(void);
	virtual Chip* getChip(void){ return chip; };
	virtual const GimicParam* getParam(){ return &gimicParam; };

public:
	virtual UINT getCPS(void){ return cps; };
	virtual void update(void);
	virtual void checkConnection(void);


// プライベート -----------------------------------------------------
private:
	struct MSG{
		// なんとなく合計2-DWORDになるようにしてみた。
		UCHAR len;
		UCHAR dat[7];	// 最大メッセージ長は今のところ6byte.
	};
	
	struct REQ{
		UINT t;
		USHORT addr;
		UCHAR dat;
		UCHAR dummy;
	};

private:
	int sendMsg( MSG *data );
	int sendMsg2( UCHAR *data, DWORD sz);
	int transaction( MSG *txdata, uint8_t *rxdata, uint32_t rxsz );
	void out2buf(UINT addr, UCHAR data);
	
	int devWrite( LPCVOID data, DWORD sz )
	{
		if(!hHandle)
			return C86CTL_ERR_NODEVICE;
		
		DWORD len;
		int ret = WriteFile(hHandle, data, sz, &len, NULL);
		
		if(ret == 0 || sz != len){
			CloseHandle(hHandle);
			hHandle = NULL;
			return C86CTL_ERR_UNKNOWN;
		}
		return C86CTL_ERR_NONE;
	};
	
	int devRead( LPVOID data )
	{
		if(!hHandle)
			return C86CTL_ERR_NODEVICE;
		
		DWORD len;
		if( !ReadFile( hHandle, data, 65, &len, NULL) ){
			CloseHandle(hHandle);
			hHandle = NULL;
			return C86CTL_ERR_UNKNOWN;
		}
		return C86CTL_ERR_NONE;
	}


private:
	HANDLE hHandle;
	std::basic_string<TCHAR> devPath;

	CRITICAL_SECTION csection;
	CRingBuff<MSG> rbuff;
	UINT cps, cal, calcount;

	int delay;
	CRingBuff<REQ> dqueue;
	
	Chip *chip;
	ChipType chiptype;
	GimicParam gimicParam;
};

typedef std::shared_ptr<GimicHID> GimicHIDPtr;

};

#endif

