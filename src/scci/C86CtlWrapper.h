// copyright-holders:K.Ito

#include "c86ctl.h"

#pragma once

typedef HRESULT(WINAPI* TCreateInstance)(REFIID, LPVOID*);

HMODULE	hC87Ctl = NULL;
c86ctl::IRealChipBase* pChipBase = NULL;

extern "C" {

	__declspec(dllexport) DWORD __cdecl  GimicInitialize();

	__declspec(dllexport) DWORD __cdecl  GimicDeinitialize();

	__declspec(dllexport) DWORD __cdecl  GimicGetNumberOfChip();

	__declspec(dllexport) DWORD __cdecl  GimicGetModule(UINT moduleIndex, UINT chipType);

	__declspec(dllexport) DWORD __cdecl  GimicSetClock(UINT moduleIndex, UINT clock);

	__declspec(dllexport) void __cdecl  GimicSetRegister(UINT moduleIndex, UINT addr, DWORD data);

	__declspec(dllexport) void __cdecl  GimicSetRegisterDirect(UINT moduleIndex, UINT addr, DWORD data);

	__declspec(dllexport) DWORD __cdecl  GimicGetWrittenRegisterData(UINT moduleIndex, DWORD addr);

	__declspec(dllexport) void __cdecl  GimicSetSSGVolume(UINT moduleIndex, UCHAR volume);

}
