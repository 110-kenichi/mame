/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#pragma once

#include "chip/opna.h"
#include "chip/opn3l.h"
#include "chip/opm.h"
#include "vis_c86wnd.h"

namespace c86ctl{
namespace vis {


// --------------------------------------------------------
class CVisC86Key : public CVisWnd
{
public:
	CVisC86Key(int idx)
		: CVisWnd()
		, id(idx)
	{
		_windowWidth = 290+64+3+4;// 334;
		_windowHeight = 530;
	};
	~CVisC86Key(){};

public:
	virtual bool create( HWND parent = 0 ){ return false; };
	virtual int getId(void){ return id; };
	
protected:
	void drawFMTrackView( IVisBitmap *canvas, int ltx, int lty, int trNo, int fmNo, bool isMute, COPNFmCh *pFMCh );
	void drawFM3EXTrackView( IVisBitmap *canvas, int ltx, int lty, int trNo, bool isMute, COPNFmCh *pFMCh );
	void drawSSGTrackView( IVisBitmap *canvas, int ltx, int lty, int trNo, int ssgNo, bool isMute, COPNSsg *pSsg );
	void drawRhythmTrackView( IVisBitmap *canvas, int ltx, int lty, int trNo, bool isMute, COPNRhythm *rhythm );
	
protected:
	int id;
	int _windowWidth;
	int _windowHeight;
};

typedef std::shared_ptr<CVisC86Key> CVisC86KeyPtr;


// --------------------------------------------------------
class CVisC86OPNAKey : public CVisC86Key
{
public:
	CVisC86OPNAKey(COPNA *pchip, int id)
		: CVisC86Key(id)
		, pOPNA(pchip)
	{
		TCHAR str[40];
		_stprintf_s(str, _T("C86OPNAKEY%d"), id);
		windowClass = str;
		_stprintf_s(str, _T("[%d] OPNA KEYBOARD VIEW"), id);
		windowTitle = str;
	};
	~CVisC86OPNAKey(){};

protected:
	virtual bool create( HWND parent = 0 );
	virtual void onPaintClient(void);
	virtual void onKeyDown(DWORD keycode);
	
protected:
	void drawADPCMTrackView( IVisBitmap *canvas, int ltx, int lty, int trNo );

protected:
	CVisMuteSwPtr muteSw[14];
	CVisSoloSwPtr soloSw[14];
	
	COPNA *pOPNA;
};

// --------------------------------------------------------
class CVisC86OPN3LKey : public CVisC86Key
{
public:
	CVisC86OPN3LKey(COPN3L *pchip, int id)
		: CVisC86Key(id)
		, pOPN3L(pchip)
	{
		TCHAR str[40];
		_stprintf_s(str, _T("C86OPN3LKEY%d"), id);
		windowClass = str;
		_stprintf_s(str, _T("[%d] OPN3L KEYBOARD VIEW"), id);
		windowTitle = str;
		
		_windowHeight = 495;
	};
	~CVisC86OPN3LKey(){};

protected:
	virtual bool create( HWND parent = 0 );
	virtual void onPaintClient(void);
	virtual void onKeyDown(DWORD keycode);

protected:
	CVisMuteSwPtr muteSw[13];
	CVisSoloSwPtr soloSw[13];
protected:
	COPN3L *pOPN3L;
};

// --------------------------------------------------------
class CVisC86OPMKey : public CVisC86Key
{
public:
	CVisC86OPMKey(COPM *pchip, int id)
		: CVisC86Key(id)
		, pOPM(pchip)
	{
		TCHAR str[40];
		_stprintf_s(str, _T("C86OPMKEY%d"), id);
		windowClass = str;
		_stprintf_s(str, _T("[%d] OPM KEYBOARD VIEW"), id);
		windowTitle = str;

		_windowHeight = 315;
	};
	~CVisC86OPMKey(){};
	
protected:
	virtual bool create( HWND parent = 0 );
	virtual void onPaintClient(void);
	virtual void onKeyDown(DWORD keycode);

protected:
	void drawFMTrackView( IVisBitmap *canvas, int ltx, int lty,
						  int trNo, int fmNo, bool isMute, COPMFmCh *pFMCh );
protected:
	CVisMuteSwPtr muteSw[8];
	CVisSoloSwPtr soloSw[8];
protected:
	COPM *pOPM;
};

// --------------------------------------------------------
// factory
CVisC86KeyPtr visC86KeyViewFactory(Chip *pchip, int id);

};
};

