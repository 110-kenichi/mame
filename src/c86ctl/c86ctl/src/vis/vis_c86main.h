/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#pragma once

#include "chip/opna.h"
#include "vis_c86wnd.h"
#include "vis_c86key.h"
#include "vis_c86reg.h"
#include "vis_c86fm.h"
#include "vis_widget.h"

#include "interface/if.h"
#include "interface/if_gimic_hid.h"
#include "interface/if_gimic_midi.h"

namespace c86ctl{
namespace vis {


class CVisC86Main : public CVisWnd
{
public:
	CVisC86Main()
		: CVisWnd(),
		  tick(0),
		  hMaster(0)
	{
		windowClass = TEXT("C86CTL");
		windowTitle = TEXT("C86 CONTROL");
	};
	~CVisC86Main(){};

public:
	virtual bool create(HWND parent);
	bool update(void);

protected:
	// message handler
	virtual void onPaintClient();
	virtual void onMouseEvent(UINT msg, WPARAM wp, LPARAM lp);
	virtual void onCommand(HWND hwnd, DWORD id, DWORD notifyCode);
	
protected:
	struct hwinfo{
		Devinfo mbinfo;
		Devinfo chipinfo;
		ChipType chiptype;
		UINT major, minor, rev, build;

		CVisCheckBoxPtr checkKey;
		CVisCheckBoxPtr checkReg;
		CVisCheckBoxPtr checkFM[9];
		CVisCheckBoxPtr checkSSG;
		CVisCheckBoxPtr checkRHYTHM;
		CVisCheckBoxPtr checkADPCM;

		CVisC86RegPtr regView;
		CVisC86KeyPtr keyView;
		CVisC86FmPtr fmView[9];
	};
	std::vector<hwinfo> info;
	HWND hMaster;

	UINT tick;
};

};
};

