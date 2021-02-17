// copyright-holders:K.Ito
// ============================================================================================
// インクルードファイル
// ============================================================================================
#include <stdlib.h>
#include <tchar.h>
#include <vector>
#include <mutex>
#include "windows.h"
#include "audioeffectx.h"
#include "..\..\src\munt\mt32emu\soxr\src\soxr.h"

#include "CMidiMsg.h"

#include "rpc/server.h"
#include "rpc/client.h"

// ============================================================================================
// 設計情報の記入
// ============================================================================================
#define MY_VST_INPUT_NUM   2 //入力数。モノラル入力=1、ステレオ入力=2
#define MY_VST_OUTPUT_NUM  2 //出力数。モノラル出力=1、ステレオ出力=2

#define MY_VST_UNIQUE_ID  'MAME' //ユニークID
//公開する場合は以下URLで発行されたユニークIDを入力する。
//http://ygrabit.steinberg.de/~ygrabit/public_html/index.html

#define MY_VST_PRESET_NUM    0 //プリセットプログラムの数
#define MY_VST_PARAMETER_NUM 0 //パラメータの数

// ============================================================================================
// VSTの基本となるクラス
// ============================================================================================
class MAmiVSTi : public AudioEffectX, public CMidiMsg
{
private:
	std::mutex mtxBuffer;

	std::vector<int32_t> m_streamBufferL;
	std::vector<int32_t> m_streamBufferR;
	void streamUpdatedL(int32_t size);
	void streamUpdatedR(int32_t size);

	rpc::client* m_rpcClient;
	rpc::server* m_rpcSrv;

	bool m_vstCtor;
	bool m_vstInited;
	bool m_vstIsClosed;
	bool m_vstIsSuspend;
	USHORT m_vstPort;
	USHORT m_mamiPort;
	VstInt32 m_lastSampleFrames;
	char m_mamiPath[MAX_PATH];

	void updateSampleRateCore();
	int hasExited();
	void startRpcServer();
	bool createSharedMemory();

	int32_t* m_tmpBufferL;
	int32_t* m_tmpBufferR;

	CHAR* m_cpSharedMemory;
	HANDLE m_hSharedMemory;

protected:
	float m_vst_sample_rate;
	int m_mami_sample_rate;
	soxr_t soxr;
	soxr_t soxl;
public:
	MAmiVSTi(audioMasterCallback audioMaster);

	void initVst();

	///< Fill \e text with a string identifying the effect
	virtual bool getEffectName(char* name)
	{
		vst_strncpy(name, "MAmidiMEmo VSTi", kVstMaxEffectNameLen);
		return true;
	}
	///< Fill \e text with a string identifying the vendor
	virtual bool getVendorString(char* text)
	{
		vst_strncpy(text, "itoken", kVstMaxVendorStrLen);
		return true;
	}
	///< Fill \e text with a string identifying the product name
	virtual bool getProductString(char* text)
	{
		vst_strncpy(text, "MAmidiMEmo VSTi", kVstMaxProductStrLen);
		return true;
	}
	///< Return vendor-specific version
	virtual VstInt32 getVendorVersion()
	{
		return 100;
	}
	///< Reports what the plug-in is able to do (#plugCanDos in audioeffectx.cpp)
	virtual VstInt32 canDo(char* text)
	{
		if (std::strcmp(text, "receiveVstEvents") == 0)
			return 1;
		else if (std::strcmp(text, "receiveVstMidiEvent") == 0)
			return 1;
		else if (std::strcmp(text, "midiProgramNames") == 0)
			return 1;
		return 0;
	}
	///< Stuff \e name with the name of the current program. Limited to #kVstMaxProgNameLen.
	virtual void getProgramName(char* name)
	{
		vst_strncpy(name, "MAmiProg", kVstMaxParamStrLen);
	}

	///< Return number of MIDI input channels
	virtual VstInt32 getNumMidiInputChannels() { return 16; }
	///< Return number of MIDI output channels
	virtual VstInt32 getNumMidiOutputChannels() { return 16; }

	///< Host stores plug-in state. Returns the size in bytes of the chunk (plug-in allocates the data array)
	virtual VstInt32 getChunk(void** data, bool isPreset = false);
	///< Host restores plug-in state
	virtual VstInt32 setChunk(void* data, VstInt32 byteSize, bool isPreset = false);

	///< Called when the sample rate changes (always in a suspend state)
	virtual void setSampleRate(float sampleRate);

	// 音声信号を処理するメンバー関数
	virtual void processReplacing(float** inputs, float** outputs, VstInt32 sampleFrames);
	virtual void processReplacing2(float* inL, float* inR, float* outL, float* outR, VstInt32 sampleFrames);

	///< Called when plug-in is initialized
	virtual void open();
	///< Called when plug-in will be released
	virtual void close();
	///< Called when plug-in is switched to off
	virtual void suspend();
	///< Called when plug-in is switched to on
	virtual void resume();

	// MIDIメッセージをホストアプリケーションから受け取るためのメンバー関数
	VstInt32 processEvents(VstEvents* events);
};
