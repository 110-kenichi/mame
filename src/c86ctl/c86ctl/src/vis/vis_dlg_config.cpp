/***
	c86ctl
	
	Copyright (c) 2009-2013, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */
#include "stdafx.h"
#include <CommCtrl.h>
#include <WindowsX.h>

#include "c86ctlmain.h"
#include "c86ctlmainwnd.h"
#include "vis_dlg_config.h"
#include "resource.h"

using namespace c86ctl::vis;

HWND CVisDlgConfig::hDlg = 0;
bool CVisDlgConfig::isInit = false;

const int opna_clocklist[] = {
	1996800,
	1989300,
	2000000,
	2457600,
	2500000,
	3000000,
	3579545,
	3978600,
	3993600,
	4000000,
	4915200,
	5000000,
	6000000,
	6144000,
	7093800,
	7159000,
	7159090,
	7160000,
	7670454,
	7987200,
	8000000,
	8055050,
	9000000,
	-1
};

const int opm_clocklist[] = {
	3000000,
	3375000,
	3427190,
	3500000,
	3578333,
	3579545,
	3579580,
	3580000,
	3582071,
	3727625,
	4000000,
	-1
};

const int opl3_clocklist[] = {
	10000000,
	12000000,
	14000000,
	14318000,
	14318180,
	15974400,
	16000000,
	16666666,
	-1
};


INT_PTR CALLBACK CVisDlgConfig::dlgProc(HWND hWnd , UINT msg , WPARAM wp , LPARAM lp)
{
	switch (msg) {
	case WM_ACTIVATE:
		switch(LOWORD(wp)){
		case WA_CLICKACTIVE:
		case WA_ACTIVE:
			C86CtlMain::setActiveDlg(hDlg);
			return TRUE;
			break;
		case WA_INACTIVE:
			C86CtlMain::setActiveDlg(NULL);
			return TRUE;
			break;
		}
		break;

	case WM_COMMAND:
		onCommand((HWND)lp, wp&0xffff, (wp>>16)&0xffff);
		break;
		
	case WM_INITDIALOG:
		onInitDialog(hWnd);
		break;

	case WM_CLOSE:
		GetC86CtlMain()->saveConfig();
		destroy();
		return TRUE;
		break;
	}

	return FALSE;
}

void CVisDlgConfig::onInitDialog(HWND hWnd)
{
	auto gimics = GetC86CtlMain()->getGimics();
	int n = gimics.size();
	const int bufsz = 128;
	TCHAR str[bufsz];

	Button_SetCheck(GetDlgItem(hWnd, IDC_CHECK_SHOWVIS), BST_CHECKED);
	
	const int nMaxModules = 4;
	const UINT delay_editid[nMaxModules] = { IDC_EDIT_DELAY0, IDC_EDIT_DELAY1, IDC_EDIT_DELAY2, IDC_EDIT_DELAY3 };
	const UINT delay_spinid[nMaxModules] = { IDC_SPIN_DELAY0, IDC_SPIN_DELAY1, IDC_SPIN_DELAY2, IDC_SPIN_DELAY3 };
	const UINT ssgvol_editid[nMaxModules] = { IDC_EDIT_SSGVOL0, IDC_EDIT_SSGVOL1, IDC_EDIT_SSGVOL2, IDC_EDIT_SSGVOL3 };
	const UINT ssgvol_spinid[nMaxModules] = { IDC_SPIN_SSGVOL0, IDC_SPIN_SSGVOL1, IDC_SPIN_SSGVOL2, IDC_SPIN_SSGVOL3 };
	const UINT clocks_cmbid[nMaxModules] = { IDC_COMBO_PLLCLOCK0, IDC_COMBO_PLLCLOCK1, IDC_COMBO_PLLCLOCK2, IDC_COMBO_PLLCLOCK3 };

	for( int i=0; i<nMaxModules; i++ ){
		if(i<n && gimics[i]->isValid()){
			auto gimic = gimics[i];
			HWND hspin, hedit;
			
			hspin = GetDlgItem(hWnd, delay_spinid[i]);
			EnableWindow(hspin,TRUE);
			SendMessage(hspin, UDM_SETRANGE32, 0, 5000);

			int delay=0;
			gimic->getDelay(&delay);
			_sntprintf(str, bufsz, _T("%d"), delay);
			hedit = GetDlgItem(hWnd, delay_editid[i]);
			EnableWindow(hedit,TRUE);
			Edit_SetText(hedit, str);

			hspin = GetDlgItem(hWnd, ssgvol_spinid[i]);
			EnableWindow(hspin,TRUE);
			SendMessage(hspin, UDM_SETRANGE32, 0, 127);

			UCHAR vol=0;
			gimic->getSSGVolume(&vol);
			_sntprintf(str, bufsz, _T("%d"), vol);
			hedit = GetDlgItem(hWnd, ssgvol_editid[i]);
			EnableWindow(hedit,TRUE);
			Edit_SetText(hedit, str);

			ChipType type;
			gimic->getModuleType(&type);

			const int *clklist = NULL;
			switch(type){
			case CHIP_OPM:  clklist = opm_clocklist; break;
			case CHIP_OPNA: clklist = opna_clocklist; break;
			case CHIP_OPL3: clklist = opl3_clocklist; break;
			case CHIP_OPN3L: break; // unsupported.
			}
			
			HWND hcmb = GetDlgItem(hWnd, clocks_cmbid[i]);
			if( clklist ){
				EnableWindow(hcmb, TRUE);
				for(int i=0;;i++){
					if( clklist[i]<0 ) break;
					_sntprintf(str, bufsz, _T("%d Hz"), clklist[i]);
					int idx = ComboBox_AddItemData( hcmb, str );
					if( idx != CB_ERR && idx != CB_ERRSPACE )
						ComboBox_SetItemData( hcmb, idx, clklist[i] );
				}
				
				UINT clk;
				gimic->getPLLClock(&clk);
				for(int i=0;;i++){
					if( clklist[i]<0 ) break;
					if( clklist[i]==clk ){
						ComboBox_SetCurSel(hcmb, i);
						break;
					}
				}
				
			}else{
				EnableWindow(hcmb, FALSE);
			}
		}else{
			HWND hspin,hedit,hcmb;
			hspin = GetDlgItem(hWnd, delay_spinid[i]);
			EnableWindow(hspin, FALSE);
			hedit = GetDlgItem(hWnd, delay_editid[i]);
			EnableWindow(hedit, FALSE);
			hspin = GetDlgItem(hWnd, ssgvol_spinid[i]);
			EnableWindow(hspin, FALSE);
			hedit = GetDlgItem(hWnd, ssgvol_editid[i]);
			EnableWindow(hedit, FALSE);
			hcmb = GetDlgItem(hWnd, clocks_cmbid[i]);
			EnableWindow(hcmb,FALSE);
		}

	}
	
	isInit = true;
}


void CVisDlgConfig::onDelayEditNotify(HWND hwnd, DWORD id, DWORD notifyCode)
{
	auto gimics = GetC86CtlMain()->getGimics();
	TCHAR buff[256];
	int delay=0;

	if(id>=gimics.size() || !gimics[id].get()->isValid())
		return;

	switch(notifyCode){
	case EN_CHANGE:
		if( isInit ){
			Edit_GetText(hwnd, buff, sizeof(buff));
			delay = _ttoi(buff);
			gimics[id]->setDelay(delay);
		}
		break;
	}
}

void CVisDlgConfig::onSSGVolEditNotify(HWND hwnd, DWORD id, DWORD notifyCode)
{
	auto gimics = GetC86CtlMain()->getGimics();
	TCHAR buff[256];
	UCHAR vol=0;

	if(id>=gimics.size() || !gimics[id].get()->isValid())
		return;

	switch(notifyCode){
	case EN_CHANGE:
		if( isInit ){
			Edit_GetText(hwnd, buff, sizeof(buff));
			vol = (UCHAR)_ttoi(buff);
			if( vol<0 ) vol = 0;
			if( vol>127 ) vol = 127;
			gimics[id]->setSSGVolume(vol);
		}
		break;
	}
}

void CVisDlgConfig::onPLLClockCmbNotify(HWND hwnd, DWORD id, DWORD notifyCode)
{
	auto gimics = GetC86CtlMain()->getGimics();

	if(id>=gimics.size() || !gimics[id].get()->isValid())
		return;

	switch(notifyCode){
	case CBN_SELCHANGE:
		if( isInit ){
			int idx = ComboBox_GetCurSel(hwnd);
			int clk = ComboBox_GetItemData(hwnd, idx);
			gimics[id]->setPLLClock(clk);
		}
		break;
	}
}

void CVisDlgConfig::onShowVisCheckNotify(HWND hwnd, DWORD notifyCode)
{
	switch(notifyCode){
	case BN_CLICKED:
		if( isInit ){
			LRESULT state = Button_GetCheck(hwnd);
			C86CtlMainWnd *pwnd = C86CtlMainWnd::getInstance();
			if( state == BST_CHECKED ){
				pwnd->startVis();
			}else{
				pwnd->stopVis();
			}
		}
		break;
	}
}


void CVisDlgConfig::onCommand(HWND hwnd, DWORD id, DWORD notifyCode)
{
	
	switch(id){
	case IDOK:
		SendMessage(hDlg, WM_CLOSE, 0, 0);
		break;

	case IDC_CHECK_SHOWVIS:
		onShowVisCheckNotify(hwnd, notifyCode);
		break;
		
	case IDC_EDIT_DELAY0:
		onDelayEditNotify(hwnd, 0, notifyCode);
		break;
	case IDC_EDIT_DELAY1:
		onDelayEditNotify(hwnd, 1, notifyCode);
		break;
	case IDC_EDIT_DELAY2:
		onDelayEditNotify(hwnd, 2, notifyCode);
		break;
	case IDC_EDIT_DELAY3:
		onDelayEditNotify(hwnd, 3, notifyCode);
		break;
		
	case IDC_EDIT_SSGVOL0:
		onSSGVolEditNotify(hwnd, 0, notifyCode);
		break;
	case IDC_EDIT_SSGVOL1:
		onSSGVolEditNotify(hwnd, 1, notifyCode);
		break;
	case IDC_EDIT_SSGVOL2:
		onSSGVolEditNotify(hwnd, 2, notifyCode);
		break;
	case IDC_EDIT_SSGVOL3:
		onSSGVolEditNotify(hwnd, 3, notifyCode);
		break;

	case IDC_COMBO_PLLCLOCK0:
		onPLLClockCmbNotify(hwnd, 0, notifyCode);
		break;
	case IDC_COMBO_PLLCLOCK1:
		onPLLClockCmbNotify(hwnd, 1, notifyCode);
		break;
	case IDC_COMBO_PLLCLOCK2:
		onPLLClockCmbNotify(hwnd, 2, notifyCode);
		break;
	case IDC_COMBO_PLLCLOCK3:
		onPLLClockCmbNotify(hwnd, 3, notifyCode);
		break;
	}
}

void CVisDlgConfig::create(HWND hParent)
{
	if( !hDlg ){
		hDlg = CreateDialog(C86CtlMain::getInstanceHandle(),
							MAKEINTRESOURCE(IDD_C86CTL_CONFIG), hParent, &dlgProc);

		HICON hIcon;
		hIcon = (HICON)LoadImage(C86CtlMain::getInstanceHandle(),
			MAKEINTRESOURCE(IDI_ICON_C86CTL), IMAGE_ICON,
			GetSystemMetrics(SM_CXSMICON), 
			GetSystemMetrics(SM_CYSMICON), 0);
		if(hIcon)
			SendMessage(hDlg, WM_SETICON, ICON_BIG, (LPARAM)hIcon);
	}
	ShowWindow( hDlg, SW_SHOW );


}


void CVisDlgConfig::destroy()
{
	DestroyWindow(hDlg);
	hDlg = NULL;
	isInit = false;
}

