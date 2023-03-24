// copyright-holders:K.Ito

#include "../c86ctl/c86ctl/src/c86ctl.h"

#pragma once

typedef HRESULT(WINAPI* TCreateInstance)(REFIID, LPVOID*);

HMODULE	hC87Ctl = NULL;
c86ctl::IRealChipBase* pChipBase = NULL;

extern "C" {

	__declspec(dllexport) DWORD __cdecl  GimicInitialize();

	__declspec(dllexport) DWORD __cdecl  GimicDeinitialize();

	__declspec(dllexport) DWORD __cdecl  GimicGetNumberOfChip();

	__declspec(dllexport) DWORD __cdecl  GimicGetModule(DWORD moduleIndex, DWORD chipType);

	__declspec(dllexport) DWORD __cdecl  GimicSetClock(DWORD moduleIndex, DWORD clock);

	__declspec(dllexport) void __cdecl  GimicSetRegister(DWORD moduleIndex, DWORD addr, DWORD data);

	__declspec(dllexport) void __cdecl  GimicSetRegisterDirect(DWORD moduleIndex, DWORD addr, DWORD data);

	__declspec(dllexport) void __cdecl  GimicSetRegister2(DWORD moduleIndex, DWORD* addr, UCHAR* data, DWORD sz);

	__declspec(dllexport) DWORD __cdecl  GimicGetWrittenRegisterData(DWORD moduleIndex, DWORD addr);

	__declspec(dllexport) void __cdecl  GimicSetSSGVolume(DWORD moduleIndex, UCHAR volume);

}
