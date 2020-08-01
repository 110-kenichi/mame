// copyright-holders:K.Ito
#include "MAmiVSTi.h"

// ============================================================================================
// ����VST�𐶐����邽�߂̊֐�
// ============================================================================================
AudioEffect* createEffectInstance(audioMasterCallback audioMaster)
{
	//new�ł���VST�𐶐������|�C���^��Ԃ�
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
// ����VST�̏�����
// ============================================================================================
MAmiVSTi::MAmiVSTi(audioMasterCallback audioMaster)
	: AudioEffectX(audioMaster, MY_VST_PRESET_NUM, MY_VST_PARAMETER_NUM)
{
	//VST�̏��������s���B
	if (initialized)
		return;
	initialized = true;

	//�ȉ��̊֐����Ăяo���ē��͐��A�o�͐����̏���ݒ肷��B
	//�K���Ăяo���Ȃ���΂Ȃ�Ȃ��B
	setNumInputs(MY_VST_INPUT_NUM);    //���͐��̐ݒ�
	setNumOutputs(MY_VST_OUTPUT_NUM);  //�o�͐��̐ݒ�
	setUniqueID(MY_VST_UNIQUE_ID);     //���j�[�NID�̐ݒ�

	isSynth(true);          //����VST��Synth���ǂ����̃t���O��ݒ�B
							 //Synth�̏ꍇ�ctrue�AEffector�̏ꍇ�cfalse

	canProcessReplacing();  //����VST�����������\���ǂ����̃t���O��ݒ�B
							 //�����������s��Ȃ�VST�͂Ȃ��̂ŕK�����̊֐����Ăяo���B

	programsAreChunks(true);

	//��L�̊֐����Ăяo������ɏ��������s��


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
// �����M����������
// ============================================================================================
void MAmiVSTi::processReplacing(float** inputs, float** outputs, VstInt32 sampleFrames)
{
	if (isSuspend || isClosed || HasExited())
		return;

	//���́A�o�͂�2�����z��œn�����B
	//���͂�-1.0f�`1.0f�̊Ԃœn�����B
	//�o�͂�-1.0f�`1.0f�̊Ԃŏ������ޕK�v������B
	//sampleFrames����������o�b�t�@�̃T�C�Y
	float* inL = inputs[0];  //���� ���p
	float* inR = inputs[1];  //���� �E�p
	float* outL = outputs[0]; //�o�� ���p
	float* outR = outputs[1]; //�o�� �E�p

	// �������ꂽ�t���[����
	VstInt32 processedFrames = 0;

	while (getMidiMessageNum() > 0)
	{
		// �������ׂ��t���[����
		VstInt32 Frames = getNextDeltaFrames() - processedFrames;
		processReplacing2(inL, inR, outL, outR, Frames);

		// MIDI���b�Z�[�W�̏������s��
		midiMsgProc();

		// �������ꂽ�t���[�������v�Z
		// �����ɉ����M���o�b�t�@�̃|�C���^�ɂ��Ă��i�߂�B
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

	//�����ŉ����������s���B
	for (VstInt32 i = 0; i < sampleFrames; i++)
	{
		*(outL++) = *(inL++) + (float)streamBufferL[i] / (float)32767;
		*(outR++) = *(inR++) + (float)streamBufferR[i] / (float)32767;
	}
	streamBufferL.erase(streamBufferL.begin(), streamBufferL.begin() + sampleFrames);
	streamBufferR.erase(streamBufferR.begin(), streamBufferR.begin() + sampleFrames);
}

// MIDI���b�Z�[�W��VST�ɕۑ�����B
// processReplacing()�̑O�ɕK��1�x�����Ăяo�����B
VstInt32 MAmiVSTi::processEvents(VstEvents* events)
{
	// MIDI�̃��X�g�����������܂��B
	clearMidiMsg();

	int loops = (events->numEvents);

	// VST�C�x���g�̉񐔂������[�v���܂킷�B
	for (int i = 0; i < loops; i++)
	{
		// �^����ꂽ�C�x���g��MIDI�Ȃ��midimsgbuf�ɃX�g�b�N����
		switch ((events->events[i])->type)
		{
		case kVstMidiType:
		case kVstSysExType:
			VstMidiEventBase * midievent = (VstMidiEventBase*)(events->events[i]);
			pushMidiMsg(midievent);
			break;
		}
	}

	//�@1��Ԃ��Ȃ���΂Ȃ�Ȃ�
	return 1;
}
