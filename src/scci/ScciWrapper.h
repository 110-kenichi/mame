// copyright-holders:K.Ito

#include "scci.h"
#include "SCCIDefines.h"

#pragma once

HMODULE	hScci = NULL;

// �T�E���h�C���^�[�t�F�[�X�}�l�[�W���[�擾
SoundInterfaceManager *pManager = NULL;

extern "C" {

	__declspec(dllexport) DWORD __cdecl  ScciInitialize();

	__declspec(dllexport) BOOL __cdecl  ScciRelease();

	__declspec(dllexport) SoundChip* __cdecl  ScciGetSoundChip(int iSoundChipType, DWORD dClock);

	__declspec(dllexport) BOOL __cdecl  ScciReleaseSoundChip(SoundChip * pSoundChip);

	__declspec(dllexport) BOOL __cdecl  ScciSetRegister(void* pChip, DWORD dAddr, DWORD dData);

	__declspec(dllexport) DWORD __cdecl  ScciGetWrittenRegisterData(void* pChip, DWORD addr);

	__declspec(dllexport) BOOL __cdecl  ScciIsBufferEmpty(void* pChip);
}
