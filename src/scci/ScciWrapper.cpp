// copyright-holders:K.Ito

#include "ScciWrapper.h"


__declspec(dllexport) DWORD __stdcall InitializeScci()
{
	// scci.dllの読込み
	hScci = ::LoadLibrary(L"scci");
	if (hScci == NULL) {
		::FreeLibrary(hScci);
		return GetLastError();
	}

	// サウンドインターフェースマネージャー取得用関数アドレス取得
	SCCIFUNC getSoundInterfaceManager = (SCCIFUNC)(::GetProcAddress(hScci, "getSoundInterfaceManager"));
	if (getSoundInterfaceManager == NULL)
	{
		::FreeLibrary(hScci);
		return GetLastError();
	}

	// サウンドインターフェースマネージャー取得
	pManager = getSoundInterfaceManager();

	// サウンドインターフェースマネージャーインスタンス初期化
	// 必ず最初に実行してください
	pManager->initializeInstance();

	// リセットを行う
	pManager->reset();

	// データ送信遅延時間設定
	// 設定しない場合は、scci.iniファイルに定義されている時間が適用されます
	//pManager->setDelay(20);

	return 0;
}

__declspec(dllexport) BOOL __stdcall ReleaseScci()
{
	if (pManager == NULL)
		return TRUE;

	// 一括開放する場合（チップ一括開放の場合）
	BOOL rv = pManager->releaseAllSoundChip();
	if(rv == FALSE)
		return FALSE;

	// サウンドインターフェースマネージャーインスタンス開放
	// FreeLibraryを行う前に必ず呼び出ししてください
	return pManager->releaseInstance();
}

__declspec(dllexport) SoundChip* __stdcall GetSoundChip(int iSoundChipType, DWORD dClock)
{
	if (pManager == NULL)
		return NULL;

	return pManager->getSoundChip(iSoundChipType, dClock);
}

__declspec(dllexport) BOOL __stdcall ReleaseSoundChip(SoundChip * pSoundChip)
{
	if (pSoundChip == NULL)
		return TRUE;

	return pManager->releaseSoundChip(pSoundChip);
}

__declspec(dllexport) BOOL __stdcall SetRegister(void* pChip, DWORD dAddr, DWORD dData)
{
	if (pChip == NULL)
		return FALSE;

	return ((SoundChip *)pChip)->setRegister(dAddr, dData);
}

__declspec(dllexport) DWORD __stdcall GetWrittenRegisterData(void* pChip, DWORD addr)
{
	if (pChip == NULL)
		return FALSE;

	return ((SoundChip *)pChip)->getWrittenRegisterData(addr);
}

__declspec(dllexport) BOOL __stdcall IsBufferEmpty(void* pChip)
{
	if (pChip == NULL)
		return FALSE;

	return ((SoundChip *)pChip)->isBufferEmpty();
}
