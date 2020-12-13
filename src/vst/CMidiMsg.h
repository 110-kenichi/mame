// copyright-holders:K.Ito
#pragma once
#include <string.h>
#include <vector>
#include "audioeffectx.h"

// ============================================================================================
// MIDI処理用の定義
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
	std::vector<VstMidiEventBase*> midiMsgBuf; //受け取ったMIDIメッセージを保管するバッファ
public:
	CMidiMsg(void);
	~CMidiMsg(void);

	// バッファのクリア等を行う。
	virtual void clearMidiMsg();

	// バッファからMIDIメッセージを取り出す
	virtual VstMidiEventBase* popMidiMsg();

	virtual void pushMidiMsg(VstMidiEventBase *midiEvent);

	// バッファ中にあるMIDIメッセージの数を返す
	virtual size_t getMidiMessageNum();

	// バッファから最初に取り出せるMIDIメッセージのDeltaFramesを返す
	virtual VstInt32 getNextDeltaFrames();

	virtual void midiMsgProc();

};
