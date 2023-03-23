/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#include <vector>

#include "c86ctl.h"
#include "withlock.h"
#include "interface/if.h"


#define WM_THREADEXIT       (WM_APP+10)
#define WM_TASKTRAY_EVENT   (WM_APP+11)
#define WM_MYDEVCHANGE      (WM_APP+12)
#define WM_MYDEVUPDATED     (WM_APP+13)


// ------------------------------------------------------------------
namespace c86ctl{

class C86CtlMain : public IRealChipBase
{
public:
	C86CtlMain() : 
	    isInitialized(false),
		hMainThread(0),
		mainThreadID(0),
		mainThreadReady(false),
		hSenderThread(0),
		senderThreadID(0),
		senderThreadReady(false),
		timerPeriod(0),
		terminateFlag(false),
		refCount(0)
	{
	};
	~C86CtlMain(){};

public:
	// IUnknown
	virtual HRESULT __stdcall QueryInterface( REFIID riid, LPVOID *ppvObj );
	virtual ULONG __stdcall AddRef(VOID);
	virtual ULONG __stdcall Release(VOID);

public:
	// IRealChipBase
	virtual int __stdcall initialize(void);
	virtual int __stdcall deinitialize(void);
	virtual int __stdcall getNumberOfChip(void);
	virtual HRESULT __stdcall getChipInterface( int id, REFIID riid, void** ppi );

public:
	int reset(void);
	void out( UINT addr, UCHAR data );
	UCHAR in( UINT addr );

public:
	static INT init(HINSTANCE h);
	static INT deinit(void);
	static HINSTANCE getInstanceHandle();
	static void setActiveDlg(HWND dlg){ hActiveDlg = dlg; };

public:
	void loadConfig(void);
	void saveConfig(void);
	
private:
	bool terminateFlag;
	withlock< std::vector< std::shared_ptr<GimicIF> > > gGIMIC;


public:
	withlock< std::vector< std::shared_ptr<GimicIF> > > &getGimics();
	
private:
	static unsigned int WINAPI threadMain(LPVOID param);
	static unsigned int WINAPI threadSender(LPVOID param);

private:
	HANDLE hMainThread;
	UINT mainThreadID;
	volatile bool mainThreadReady;
	HANDLE hSenderThread;
	UINT senderThreadID;
	volatile bool senderThreadReady;
	
	static HINSTANCE hInstance;
	static ULONG_PTR gdiToken;
	static Gdiplus::GdiplusStartupInput gdiInput;
	static HWND hActiveDlg;
	
	DWORD timerPeriod;
	bool isInitialized;

private:
	UINT refCount;
};

C86CtlMain* GetC86CtlMain(void);

};

