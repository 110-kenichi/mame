// copyright-holders:K.Ito
#pragma once
#include <string.h>
#include <vector>
#include "audioeffectx.h"

// ============================================================================================
// MIDI�����p�̒�`
// ============================================================================================
#define MIDIMSG_MAXNUM 255

struct VstMidiEventBase
{
	//-------------------------------------------------------------------------------------------------------
	VstInt32 type;			///< #kVstSysexType or kVstMidiType
	VstInt32 byteSize;		///< sizeof(VstMidiEventBase)
	VstInt32 deltaFrames;	///< sample frames related to the current block start sample position
	VstInt32 flags;			///< none defined yet (should be zero)
};

class CMidiMsg
{
protected:
	std::vector<VstMidiEventBase*> midiMsgBuf; //�󂯎����MIDI���b�Z�[�W��ۊǂ���o�b�t�@
public:
	CMidiMsg(void);
	~CMidiMsg(void);

	// �o�b�t�@�̃N���A�����s���B
	virtual void clearMidiMsg();

	// �o�b�t�@����MIDI���b�Z�[�W�����o��
	virtual VstMidiEventBase* popMidiMsg();

	virtual void pushMidiMsg(VstMidiEventBase *midiEvent);

	// �o�b�t�@���ɂ���MIDI���b�Z�[�W�̐���Ԃ�
	virtual size_t getMidiMessageNum();

	// �o�b�t�@����ŏ��Ɏ��o����MIDI���b�Z�[�W��DeltaFrames��Ԃ�
	virtual VstInt32 getNextDeltaFrames();

	virtual void midiMsgProc();

};
