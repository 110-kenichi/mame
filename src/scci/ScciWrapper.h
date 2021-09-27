// copyright-holders:K.Ito

#include "scci.h"
#include "SCCIDefines.h"

#pragma once

HMODULE	hScci = NULL;

// サウンドインターフェースマネージャー取得
SoundInterfaceManager *pManager = NULL;

extern "C" {

	__declspec(dllexport) DWORD __cdecl  InitializeScci();

	__declspec(dllexport) BOOL __cdecl  ReleaseScci();

	__declspec(dllexport) SoundChip* __cdecl  GetSoundChip(int iSoundChipType, DWORD dClock);

	__declspec(dllexport) BOOL __cdecl  ReleaseSoundChip(SoundChip * pSoundChip);

	__declspec(dllexport) BOOL __cdecl  SetRegister(void* pChip, DWORD dAddr, DWORD dData);

	__declspec(dllexport) DWORD __cdecl  GetWrittenRegisterData(void* pChip, DWORD addr);

	__declspec(dllexport) BOOL __cdecl  IsBufferEmpty(void* pChip);
}
