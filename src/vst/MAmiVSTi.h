// copyright-holders:K.Ito
// ============================================================================================
// インクルードファイル
// ============================================================================================
#include <stdlib.h>
#include <tchar.h>
#include <vector>
#include <shared_mutex>
#include "windows.h"
#include "audioeffectx.h"
#include "..\..\src\munt\mt32emu\soxr\src\soxr.h"

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

#define MY_VST_PRESET_NUM    1 //プリセットプログラムの数
#define MY_VST_PARAMETER_NUM 1 //パラメータの数

struct VstMidiEventBase
{
	//-------------------------------------------------------------------------------------------------------
	VstInt32 type;			///< #kVstSysexType or kVstMidiType
	VstInt32 byteSize;		///< sizeof(VstMidiEventBase)
	VstInt32 deltaFrames;	///< sample frames related to the current block start sample position
	VstInt32 flags;			///< none defined yet (should be zero)
};

// ============================================================================================
// VSTの基本となるクラス
// ============================================================================================
class MAmiVSTi : public AudioEffectX
{
private:
	std::shared_mutex mtxBuffer;
	std::shared_mutex mtxSoxrBuffer;

	std::vector<int32_t> m_streamBuffer2ch;

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
	VstInt32 m_sampleFramesBlock;
	char m_mamiPath[MAX_PATH];

	void updateSampleRateCore();
	int isVstDisabled();
	void startRpcServer();
	bool createSharedMemory();

	bool m_streamBufferOverflowed;
	bool silence;

	CHAR* m_cpSharedMemory;
	HANDLE m_hSharedMemory;

protected:
	float m_vst_sample_rate;
	int m_mami_sample_rate;
	soxr_t soxr;
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
		vst_strncpy(name, "MAmidiMEmo", kVstMaxParamStrLen);
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

	///< Called when the Maximun block size changes (always in a suspend state). Note that the sampleFrames in Process Calls could be smaller than this block size, but NOT bigger.
	virtual void setBlockSize(VstInt32 blockSize);

	// 音声信号を処理するメンバー関数
	virtual void processReplacing(float** inputs, float** outputs, VstInt32 sampleFrames);

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

	virtual void getParameterDisplay(VstInt32 index, char* text);	///< Stuff \e text with a string representation ("0.5", "-3", "PLATE", etc...) of the value of parameter \e index. Limited to #kVstMaxParamStrLen.
	virtual void getParameterLabel(VstInt32 index, char* label);	///< Stuff \e label with the units in which parameter \e index is displayed (i.e. "sec", "dB", "type", etc...). Limited to #kVstMaxParamStrLen.
	virtual void setParameter(VstInt32 index, float value);	///< Called when a parameter changed
	virtual float getParameter(VstInt32 index);	///< Return the value of the parameter with \e index
	virtual void getParameterName(VstInt32 index, char* text);    ///< Stuff \e text with the name ("Time", "Gain", "RoomType", etc...) of parameter \e index. Limited to #kVstMaxParamStrLen.
	virtual void setProgram(VstInt32 program);	///< Set the current program to \e program
};

#include "aeffeditor.h"
#include "audioeffectx.h"

#define WIDTH 600
#define HEIGHT 100

#define ID_B1 100

#define PROP_WINPROC "PropClassWindowProc"

extern void* hInstance;

static int regist_count = 0;
static LPCTSTR lpszAppName = "DummyMamiEditor";

class DummyVstEditor : public AEffEditor
{
public:
	DummyVstEditor(AudioEffect* effect);
	virtual ~DummyVstEditor() {};

	virtual bool getRect(ERect** erect);
	virtual bool open(void* ptr);
	virtual void close();
	virtual void idle();
	virtual void setParam1(float param1) { fParam1 = param1; }
	virtual float getParam1() { return fParam1; }
	virtual void setParameter(VstInt32 index, float value);
	virtual void valueChanged(VstInt32 index, float value);

	static LRESULT WINAPI WindowProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);

private:
	HWND hwnd_e;
	float fParam1;
};
