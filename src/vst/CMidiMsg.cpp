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

void CMidiMsg::midiMsgProc(rpc::client* m_rpc_client)
{
	VstMidiEventBase* meb = popMidiMsg();
	switch (meb->type)
	{
	case kVstMidiType: {
		VstMidiEvent * midievent = (VstMidiEvent *)meb;

		m_rpc_client->async_call("SendMidiEvent",
			(unsigned char)midievent->midiData[0], (unsigned char)midievent->midiData[1], (unsigned char)midievent->midiData[2]);
	}
		break;
	case kVstSysExType: {
		VstMidiSysexEvent * midievent = (VstMidiSysexEvent *)meb;

		std::vector<unsigned char> buffer(midievent->sysexDump, midievent->sysexDump + midievent->dumpBytes);

		m_rpc_client->async_call("SendMidiSysEvent", buffer, midievent->dumpBytes);
	}
		break;
	}
};
