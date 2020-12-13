// copyright-holders:K.Ito
#include "CMidiMsg.h"

CMidiMsg::CMidiMsg(void)
{
	clearMidiMsg();
}

CMidiMsg::~CMidiMsg(void)
{
}

void CMidiMsg::clearMidiMsg()
{
	// ƒƒ“ƒo[•Ï”‚ð‰Šú‰»‚·‚é
	midiMsgBuf.clear();
}

VstMidiEventBase* CMidiMsg::popMidiMsg()
{
	VstMidiEventBase* me = midiMsgBuf[0];
	midiMsgBuf.erase(midiMsgBuf.begin());
	return me;
}

void CMidiMsg::pushMidiMsg(VstMidiEventBase *midiEvent)
{
	midiMsgBuf.push_back(midiEvent);
}

size_t CMidiMsg::getMidiMessageNum()
{
	return midiMsgBuf.size();
}

VstInt32 CMidiMsg::getNextDeltaFrames()
{
	return midiMsgBuf[0]->deltaFrames;
}

void SendMidiEvent(unsigned char data1, unsigned char data2, unsigned char data3);
void SendMidiSysEvent(unsigned char *data, int length);

void CMidiMsg::midiMsgProc()
{
	VstMidiEventBase* meb = popMidiMsg();
	switch (meb->type)
	{
	case kVstMidiType: {
		VstMidiEvent * midievent = (VstMidiEvent *)meb;
		SendMidiEvent(midievent->midiData[0], midievent->midiData[1], midievent->midiData[2]);
	}
		break;
	case kVstSysExType: {
		VstMidiSysexEvent * midievent = (VstMidiSysexEvent *)meb;
		SendMidiSysEvent((unsigned char *)midievent->sysexDump, midievent->dumpBytes);
	}
		break;
	}
};
