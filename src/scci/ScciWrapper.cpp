// copyright-holders:K.Ito

#include "ScciWrapper.h"


__declspec(dllexport) DWORD __stdcall InitializeScci()
{
	// scci.dll�̓Ǎ���
	hScci = ::LoadLibrary(L"scci");
	if (hScci == NULL) {
		::FreeLibrary(hScci);
		return GetLastError();
	}

	// �T�E���h�C���^�[�t�F�[�X�}�l�[�W���[�擾�p�֐��A�h���X�擾
	SCCIFUNC getSoundInterfaceManager = (SCCIFUNC)(::GetProcAddress(hScci, "getSoundInterfaceManager"));
	if (getSoundInterfaceManager == NULL)
	{
		::FreeLibrary(hScci);
		return GetLastError();
	}

	// �T�E���h�C���^�[�t�F�[�X�}�l�[�W���[�擾
	pManager = getSoundInterfaceManager();

	// �T�E���h�C���^�[�t�F�[�X�}�l�[�W���[�C���X�^���X������
	// �K���ŏ��Ɏ��s���Ă�������
	pManager->initializeInstance();

	// ���Z�b�g���s��
	pManager->reset();

	// �f�[�^���M�x�����Ԑݒ�
	// �ݒ肵�Ȃ��ꍇ�́Ascci.ini�t�@�C���ɒ�`����Ă��鎞�Ԃ��K�p����܂�
	//pManager->setDelay(20);

	return 0;
}

__declspec(dllexport) BOOL __stdcall ReleaseScci()
{
	if (pManager == NULL)
		return TRUE;

	// �ꊇ�J������ꍇ�i�`�b�v�ꊇ�J���̏ꍇ�j
	BOOL rv = pManager->releaseAllSoundChip();
	if(rv == FALSE)
		return FALSE;

	// �T�E���h�C���^�[�t�F�[�X�}�l�[�W���[�C���X�^���X�J��
	// FreeLibrary���s���O�ɕK���Ăяo�����Ă�������
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
