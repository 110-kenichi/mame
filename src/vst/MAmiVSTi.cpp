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

char modulePath[MAX_PATH];

DWORD WINAPI StartMAmiVSTMainThread(LPVOID lpParam)
{
	char *argv[] = { modulePath, const_cast<char*>("-sound"), const_cast<char*>("none") };
	main(sizeof(argv) / sizeof(char *), argv);

	return 0;
}

bool MAmiVSTi::initialized;
MAmiVSTi* MAmiVSTi::instance;

// ============================================================================================
// このVSTの初期化
// ============================================================================================
MAmiVSTi::MAmiVSTi(audioMasterCallback audioMaster)
	: AudioEffectX(audioMaster, MY_VST_PRESET_NUM, MY_VST_PARAMETER_NUM)
	, initToFirstRead(false)
	, isClosed(false)
	, isSuspend(false)
	, soxl(NULL)
	, soxr(NULL)
	, mami_sample_rate(0)
{
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

	initToFirstRead = true;

	instance = this;

	//VSTの初期化を行う。
	if (initialized)
		return;
	initialized = true;

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

	set_stream_update_callback(const_cast<char*>("lspeaker"), MAmiVSTi::StreamUpdatedL);
	set_stream_update_callback(const_cast<char*>("rspeaker"), MAmiVSTi::StreamUpdatedR);

	mami_sample_rate = sample_rate();
}

void MAmiVSTi::setSampleRate(float sampleRate)
{
	this->sampleRate = sampleRate;

	soxr_datatype_t itype = SOXR_INT32_I;
	soxr_datatype_t otype = SOXR_INT32_I;
	soxr_io_spec_t iospec = soxr_io_spec(itype, otype);
	soxr_quality_spec_t qSpec = soxr_quality_spec(SOXR_LSR0Q, SOXR_STEEP_FILTER);

	if(soxl != NULL)
		soxr_delete(soxl);
	if (soxr != NULL)
		soxr_delete(soxr);

	soxl = soxr_create(mami_sample_rate, sampleRate, 1, NULL, &iospec, &qSpec, NULL);
	soxr = soxr_create(mami_sample_rate, sampleRate, 1, NULL, &iospec, &qSpec, NULL);
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

	set_stream_update_callback(const_cast<char*>("lspeaker"), NULL);
	set_stream_update_callback(const_cast<char*>("rspeaker"), NULL);

	if (soxl != NULL)
		soxr_delete(soxl);
	if (soxr != NULL)
		soxr_delete(soxr);

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
	initToFirstRead = true;
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

void MAmiVSTi::StreamUpdatedL(int32_t *buffer, int32_t size)
{
	MAmiVSTi::instance->streamUpdatedL(buffer, size);
}

void MAmiVSTi::streamUpdatedL(int32_t *buffer, int32_t size)
{
	if (isSuspend || isClosed || HasExited())
		return;

	//std::lock_guard<std::mutex> lock(mtxBuffer);

	mtxBuffer.lock();

	int cnvSize = (int)ceil((double)size * ((double)sampleRate / (double)mami_sample_rate));
	int32_t* cnvBuffer = new int32_t[cnvSize];

	//idone - To return actual # samples used (<= ilen).
	size_t idone = 0;
	//odone - To return actual # samples out (<= olen).
	size_t odone = 0;

	soxr_process(soxl, buffer, size, &idone, cnvBuffer, cnvSize, &odone);

	//int32_t* cb = cnvBuffer;
	//int silent = 1;
	//for (size_t i = 0; i < odone; i++)
	//{
	//	if (*cb++ != 0) {
	//		silent = 0;
	//		break;
	//	}
	//}
	//if (!silent)
	{
		for (size_t i = 0; i < odone; i++)
			streamBufferL.push_back(*cnvBuffer++);
	}
}

void MAmiVSTi::StreamUpdatedR(int32_t *buffer, int32_t size)
{
	MAmiVSTi::instance->streamUpdatedR(buffer, size);
}

void MAmiVSTi::streamUpdatedR(int32_t *buffer, int32_t size)
{
	//if (isSuspend || isClosed || HasExited())
	//	return;

	//std::lock_guard<std::mutex> lock(mtxBuffer);

	int cnvSize = (int)ceil((double)size * ((double)sampleRate / (double)mami_sample_rate));
	int32_t* cnvBuffer = new int32_t[cnvSize];

	size_t idone = 0;
	size_t odone = 0;

	soxr_process(soxr, buffer, size, &idone, cnvBuffer, cnvSize, &odone);

	//int32_t* cb = cnvBuffer;
	//int silent = 1;
	//for (size_t i = 0; i < odone; i++)
	//{
	//	if (*cb++ != 0) {
	//		silent = 0;
	//		break;
	//	}
	//}
	//if (!silent)
	{
		for (size_t i = 0; i < odone; i++)
			streamBufferR.push_back(*cnvBuffer++);
	}

	mtxBuffer.unlock();
}

void MAmiVSTi::processReplacing2(float* inL, float* inR, float* outL, float* outR, VstInt32 sampleFrames)
{
	std::lock_guard<std::mutex> lock(mtxBuffer);

	if (streamBufferL.size() < (size_t)sampleFrames)
		return;
	if (streamBufferR.size() < (size_t)sampleFrames)
		return;

	if (initToFirstRead)
	{
		initToFirstRead = false;
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
