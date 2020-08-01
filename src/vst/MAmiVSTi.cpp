// copyright-holders:K.Ito
#include "MAmiVSTi.h"

// ============================================================================================
// このVSTを生成するための関数
// ============================================================================================
AudioEffect* createEffectInstance(audioMasterCallback audioMaster)
{
	//newでこのVSTを生成したポインタを返す
	return new MAmiVSTi(audioMaster);
}

int  main(int argc, char *argv[]);

int HasExited();
void SetVSTiMode();
int IsStartMAmidiMEmoMainStarted();
void CloseApplication();
void  LoadData(byte* data, int length);
int SaveData(void** saveBuf);

char modulePath[MAX_PATH];

typedef void(*STREAM_UPDATE_CALLBACK)(int32_t *buffer, int32_t size);
extern "C" void set_stream_update_callback(char* name, STREAM_UPDATE_CALLBACK callback);
extern "C" int sample_rate();

DWORD WINAPI StartMAmiVSTMainThread(LPVOID lpParam)
{
	char *argv[] = { modulePath, const_cast<char*>("-sound"), const_cast<char*>("none") };
	main(sizeof(argv) / sizeof(char *), argv);

	return 0;
}

std::mutex MAmiVSTi::mtxBuffer;
std::vector<int32_t> MAmiVSTi::streamBufferL;
std::vector<int32_t> MAmiVSTi::streamBufferR;
bool MAmiVSTi::isFirstRead;
bool MAmiVSTi::isClosed;
bool MAmiVSTi::isSuspend;
int MAmiVSTi::machine_sample_rate;
bool MAmiVSTi::initialized;

// ============================================================================================
// このVSTの初期化
// ============================================================================================
MAmiVSTi::MAmiVSTi(audioMasterCallback audioMaster)
	: AudioEffectX(audioMaster, MY_VST_PRESET_NUM, MY_VST_PARAMETER_NUM)
{
	//VSTの初期化を行う。
	if (initialized)
		return;
	initialized = true;

	//以下の関数を呼び出して入力数、出力数等の情報を設定する。
	//必ず呼び出さなければならない。
	setNumInputs(MY_VST_INPUT_NUM);    //入力数の設定
	setNumOutputs(MY_VST_OUTPUT_NUM);  //出力数の設定
	setUniqueID(MY_VST_UNIQUE_ID);     //ユニークIDの設定

	isSynth(true);          //このVSTがSynthかどうかのフラグを設定。
							 //Synthの場合…true、Effectorの場合…false

	canProcessReplacing();  //このVSTが音声処理可能かどうかのフラグを設定。
							 //音声処理を行わないVSTはないので必ずこの関数を呼び出す。

	programsAreChunks(true);

	//上記の関数を呼び出した後に初期化を行う


	//Set VSTi Mode to MAmi
	SetVSTiMode();

	// Launch MAmi in thread
	HMODULE hm = NULL;
	GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCSTR)&createEffectInstance, &hm);
	GetModuleFileName(hm, modulePath, sizeof(modulePath));
	DWORD dwThreadId = 0L;
	HANDLE hThread = CreateThread(NULL, 0, StartMAmiVSTMainThread, 0, 0, &dwThreadId);

	// wait MAmi
	while (IsStartMAmidiMEmoMainStarted() == 0)
		Sleep(100);

	set_stream_update_callback(const_cast<char*>("lspeaker"), leftStreamUpdated);
	set_stream_update_callback(const_cast<char*>("rspeaker"), rightStreamUpdated);

	machine_sample_rate = sample_rate();

	isFirstRead = true;
}

void MAmiVSTi::setSampleRate(float sampleRate)
{
	this->sampleRate = sampleRate;
}

///< Host stores plug-in state. Returns the size in bytes of the chunk (plug-in allocates the data array)
VstInt32 MAmiVSTi::getChunk(void** data, bool isPreset)
{
	VstInt32 lenf = SaveData(data);
	return lenf;
}

///< Host restores plug-in state
VstInt32 MAmiVSTi::setChunk(void* data, VstInt32 byteSize, bool isPreset)
{
	LoadData((byte*)data, byteSize);
	return 0;
}

///< Called when plug-in is initialized
void MAmiVSTi::open()
{

}

///< Called when plug-in will be released
void MAmiVSTi::close()
{
	isClosed = true;
	CloseApplication();
}

///< Called when plug-in is switched to off
void MAmiVSTi::suspend()
{
	std::lock_guard<std::mutex> lock(mtxBuffer);

	isSuspend = true;
	streamBufferL.clear();
	streamBufferR.clear();
}

///< Called when plug-in is switched to on
void MAmiVSTi::resume()
{
	isSuspend = false;
	isFirstRead = true;
}

// ============================================================================================
// 音声信号処理部分
// ============================================================================================
void MAmiVSTi::processReplacing(float** inputs, float** outputs, VstInt32 sampleFrames)
{
	if (isSuspend || isClosed || HasExited())
		return;

	//入力、出力は2次元配列で渡される。
	//入力は-1.0f～1.0fの間で渡される。
	//出力は-1.0f～1.0fの間で書き込む必要がある。
	//sampleFramesが処理するバッファのサイズ
	float* inL = inputs[0];  //入力 左用
	float* inR = inputs[1];  //入力 右用
	float* outL = outputs[0]; //出力 左用
	float* outR = outputs[1]; //出力 右用

	// 処理されたフレーム数
	VstInt32 processedFrames = 0;

	while (getMidiMessageNum() > 0)
	{
		// 処理すべきフレーム数
		VstInt32 Frames = getNextDeltaFrames() - processedFrames;
		processReplacing2(inL, inR, outL, outR, Frames);

		// MIDIメッセージの処理を行う
		midiMsgProc();

		// 処理されたフレーム数を計算
		// 同時に音声信号バッファのポインタについても進める。
		processedFrames += Frames;
		inL += Frames;
		inR += Frames;
		outL += Frames;
		outR += Frames;
	}

	processReplacing2(inL, inR, outL, outR, sampleFrames - processedFrames);
}

void MAmiVSTi::leftStreamUpdated(int32_t *buffer, int32_t size)
{
	std::lock_guard<std::mutex> lock(mtxBuffer);

	if (isSuspend || isClosed || HasExited())
		return;

	for (int i = 0; i < size; i++)
		streamBufferL.push_back(buffer[i]);
}

void MAmiVSTi::rightStreamUpdated(int32_t *buffer, int32_t size)
{
	std::lock_guard<std::mutex> lock(mtxBuffer);

	if (isSuspend || isClosed || HasExited())
		return;

	for (int i = 0; i < size; i++)
		streamBufferR.push_back(buffer[i]);
}


void MAmiVSTi::processReplacing2(float* inL, float* inR, float* outL, float* outR, VstInt32 sampleFrames)
{
	std::lock_guard<std::mutex> lock(mtxBuffer);

	if (streamBufferL.size() < (size_t)sampleFrames)
		return;
	if (streamBufferR.size() < (size_t)sampleFrames)
		return;

	if (isFirstRead)
	{
		isFirstRead = false;
		streamBufferL.erase(streamBufferL.begin(), streamBufferL.end() - sampleFrames);
		streamBufferR.erase(streamBufferR.begin(), streamBufferR.end() - sampleFrames);
	}

	//ここで音声処理を行う。
	for (VstInt32 i = 0; i < sampleFrames; i++)
	{
		*(outL++) = *(inL++) + (float)streamBufferL[i] / (float)32767;
		*(outR++) = *(inR++) + (float)streamBufferR[i] / (float)32767;
	}
	streamBufferL.erase(streamBufferL.begin(), streamBufferL.begin() + sampleFrames);
	streamBufferR.erase(streamBufferR.begin(), streamBufferR.begin() + sampleFrames);
}

// MIDIメッセージをVSTに保存する。
// processReplacing()の前に必ず1度だけ呼び出される。
VstInt32 MAmiVSTi::processEvents(VstEvents* events)
{
	// MIDIのリストを初期化します。
	clearMidiMsg();

	int loops = (events->numEvents);

	// VSTイベントの回数だけループをまわす。
	for (int i = 0; i < loops; i++)
	{
		// 与えられたイベントがMIDIならばmidimsgbufにストックする
		switch ((events->events[i])->type)
		{
		case kVstMidiType:
		case kVstSysExType:
			VstMidiEventBase * midievent = (VstMidiEventBase*)(events->events[i]);
			pushMidiMsg(midievent);
			break;
		}
	}

	//　1を返さなければならない
	return 1;
}
