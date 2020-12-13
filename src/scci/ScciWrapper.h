// copyright-holders:K.Ito

#include "scci.h"
#include "SCCIDefines.h"

#pragma once

HMODULE	hScci = NULL;

// サウンドインターフェースマネージャー取得
SoundInterfaceManager *pManager = NULL;

extern "C" {

	__declspec(dllexport) DWORD __stdcall InitializeScci();

	__declspec(dllexport) BOOL __stdcall ReleaseScci();

	__declspec(dllexport) SoundChip* __stdcall GetSoundChip(int iSoundChipType, DWORD dClock);

	__declspec(dllexport) BOOL __stdcall ReleaseSoundChip(SoundChip * pSoundChip);

	__declspec(dllexport) BOOL __stdcall SetRegister(void* pChip, DWORD dAddr, DWORD dData);

	__declspec(dllexport) DWORD __stdcall GetWrittenRegisterData(void* pChip, DWORD addr);

	__declspec(dllexport) BOOL __stdcall IsBufferEmpty(void* pChip);
}
