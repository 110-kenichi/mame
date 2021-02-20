// copyright-holders:K.Ito
#include "MAmiVSTi.h"
#include <shlwapi.h>
#include <random>


// ============================================================================================
// ����VST�𐶐����邽�߂̊֐�
// ============================================================================================
AudioEffect* createEffectInstance(audioMasterCallback audioMaster)
{
	//new�ł���VST�𐶐������|�C���^��Ԃ�
	return new MAmiVSTi(audioMaster);
}

void strip_ext(char* fname)
{
	char* end = fname + strlen(fname);

	while (end > fname && *end != '.') {
		--end;
	}

	if (end > fname) {
		*end = '\0';
	}
}

USHORT FindUnusedPort(USHORT defaultPort)
{
	WSADATA data;
	WSAStartup(MAKEWORD(2, 0), &data);

	int addrLen;
	SOCKET fd = socket(AF_INET, SOCK_STREAM, 0);
	struct sockaddr_in addr;
	USHORT port = defaultPort;
	if (fd != -1)
	{
		addr.sin_family = AF_INET;
		addr.sin_port = 0;
		addr.sin_addr.s_addr = INADDR_ANY;
		if (bind(fd, (const struct sockaddr*)&addr, sizeof(addr)) != -1)
		{
			addrLen = sizeof(addr);
			if (getsockname(fd, (struct sockaddr*)&addr, &addrLen) != -1)
				port = addr.sin_port;
		}
	}

	closesocket(fd);
	WSACleanup();

	return port;
}

int StartMAmi(LPCTSTR lpApplicationName, int sampleRate, int port)
{
	// additional information
	STARTUPINFO si;
	PROCESS_INFORMATION pi;

	// set the size1ch of the structures
	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	TCHAR cmdLine[MAX_PATH * 2] = _T("");

	sprintf_s(cmdLine,
		"%s -audio_latency 0 -samplerate %d -sound none -http_port %d", lpApplicationName, sampleRate, port);

	//https://www.ne.jp/asahi/hishidama/home/tech/c/windows/CreateProcess.html

	// start the program up
	if (!CreateProcess(NULL,   // the path
		cmdLine,        // Command line
		NULL,           // Process handle not inheritable
		NULL,           // Thread handle not inheritable
		FALSE,          // Set handle inheritance to FALSE
		0,              // No creation flags
		NULL,           // Use parent's environment block
		NULL,           // Use parent's starting directory 
		&si,            // Pointer to STARTUPINFO structure
		&pi             // Pointer to PROCESS_INFORMATION structure (removed extra parentheses)
	)) {
		MessageBox(0, _T("MAmiVST: Failed to launch MAmi."), 0, 0);
		return 0;
	}

	// Close process and thread handles. 
	CloseHandle(pi.hProcess);
	CloseHandle(pi.hThread);

	return 1;
}

// ============================================================================================
// ����VST�̏�����
// ============================================================================================
MAmiVSTi::MAmiVSTi(audioMasterCallback audioMaster)
	: AudioEffectX(audioMaster, MY_VST_PRESET_NUM, MY_VST_PARAMETER_NUM)
	, m_vstIsClosed(false)
	, m_vstIsSuspend(true)
	, soxr(NULL)
	, m_mami_sample_rate(48000)
	, m_vst_sample_rate(0)
	, m_vstInited(false)
	, m_rpcClient(NULL)
	, m_rpcSrv(NULL)
	, m_vstPort(0)
	, m_mamiPort(0)
	, m_vstCtor(false)
	, m_lastSampleFrames(0)
	, m_cpSharedMemory(NULL)
	, m_hSharedMemory(0)
	, m_mamiPath("")
	, m_streamBufferOverflowed(false)
{
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

	//Get MAmi path
	char dllPath[MAX_PATH];
	char dllDir[MAX_PATH];
	char iniPath[MAX_PATH];
	HMODULE hm = NULL;
	GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCSTR)&createEffectInstance, &hm);
	GetModuleFileName(hm, dllPath, sizeof(dllPath));

	strcpy_s(dllDir, MAX_PATH, dllPath);
	PathRemoveFileSpec(dllDir);

	strcpy_s(iniPath, MAX_PATH, dllPath);
	strip_ext(iniPath);
	strcat_s(iniPath, MAX_PATH, ".ini");
	char mamiPath[MAX_PATH];
	if (!GetPrivateProfileString("MAmi", "MAmiDir", ".\\", mamiPath, sizeof(mamiPath), iniPath))
	{
		MessageBox(0, _T("MAmiVST: Failed to load MAmi.ini."), 0, 0);
		return;
	}
	PathCombineA(m_mamiPath, dllDir, mamiPath);

	m_vstCtor = true;
}

void MAmiVSTi::initVst()
{
	m_rpcClient = new rpc::client("localhost", m_mamiPort);

	m_mami_sample_rate = m_rpcClient->call("sample_rate").as<int>();

	m_sampleFramesBlock = std::max(m_lastSampleFrames, ((VstInt32)m_mami_sample_rate / 50)) * 2;

	if (m_vst_sample_rate != 0)
		updateSampleRateCore();

	m_rpcClient->async_call("VstStarted");

	m_vstInited = true;
}

DWORD WINAPI MAmidiMEmoMainStartedProc(LPVOID lpParam)
{
	MAmiVSTi* mami = (MAmiVSTi*)lpParam;

	mami->initVst();

	return 0;
}

void MAmiVSTi::startRpcServer()
{
	m_vstPort = FindUnusedPort(10000);

	m_rpcSrv = new rpc::server(m_vstPort);

	m_rpcSrv->bind("MAmiMainStarted", [&]()
	{
		DWORD dwThreadId = 0L;
		CreateThread(NULL, 0, MAmidiMEmoMainStartedProc, (void*)this, 0, &dwThreadId);
	});
	m_rpcSrv->bind("AllocateMAmiPort", [&]()
	{
		m_mamiPort = FindUnusedPort(10001);
		return m_mamiPort;
	});
	m_rpcSrv->bind("StreamUpdatedL", [&](int32_t size)
	{
		//set_stream_update_callback(const_cast<char*>("lspeaker"), MAmiVSTi::StreamUpdatedL);
		streamUpdatedL(size);
	});
	m_rpcSrv->bind("StreamUpdatedR", [&](int32_t size)
	{
		//set_stream_update_callback(const_cast<char*>("rspeaker"), MAmiVSTi::StreamUpdatedR);
		streamUpdatedR(size);
	});

	m_rpcSrv->async_run();
}

int MAmiVSTi::isVstDisabled()
{
	if (!m_vstInited)
		return 1;

	if (m_vstIsClosed)
		return 1;

	if (m_rpcClient == NULL ||
		m_rpcClient->get_connection_state() != rpc::client::connection_state::connected)
		return 1;

	return 0;
}

void MAmiVSTi::setSampleRate(float srate)
{
	std::lock_guard<std::shared_mutex> lock(mtxSoxrBuffer);

	sampleRate = srate;
	m_vst_sample_rate = srate;

	if (m_vstInited)
		updateSampleRateCore();
}

void MAmiVSTi::setBlockSize(VstInt32 blockSize)
{
	AudioEffect::setBlockSize(blockSize);

	std::lock_guard<std::shared_mutex> lock(mtxSoxrBuffer);

	if (m_lastSampleFrames != blockSize)
	{
		m_lastSampleFrames = blockSize;
		m_sampleFramesBlock = std::max(m_lastSampleFrames, ((VstInt32)m_mami_sample_rate / 50)) * 2;
	}
}

void MAmiVSTi::updateSampleRateCore()
{
	soxr_datatype_t itype = SOXR_INT32_I;
	soxr_datatype_t otype = SOXR_INT32_I;
	soxr_io_spec_t iospec = soxr_io_spec(itype, otype);
	soxr_quality_spec_t qSpec = soxr_quality_spec(SOXR_20_BITQ, 0);

	if (soxr != NULL)
		soxr_delete(soxr);

	soxr = soxr_create(m_mami_sample_rate, sampleRate, 2, NULL, &iospec, &qSpec, NULL);
}

///< Host stores plug-in state. Returns the size1ch in bytes of the chunk (plug-in allocates the data array)
VstInt32 MAmiVSTi::getChunk(void** data, bool isPreset)
{
	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	if (isVstDisabled())
		return 0;

	auto buff = m_rpcClient->call("SaveData").as<std::vector<unsigned char>>();

	*data = &buff[0];

	return (VstInt32)buff.size();
}

///< Host restores plug-in state
VstInt32 MAmiVSTi::setChunk(void* data, VstInt32 byteSize, bool isPreset)
{
	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	if (isVstDisabled())
		return 0;

	//	LoadData((byte*)data, byteSize);
	std::vector<unsigned char> buffer((unsigned char*)data, ((unsigned char*)data) + byteSize);
	m_rpcClient->call("LoadData", buffer, byteSize);

	return 0;
}

///< Called when plug-in is initialized
void MAmiVSTi::open()
{

}

///< Called when plug-in will be released
void MAmiVSTi::close()
{
	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	m_vstIsClosed = true;

	if (NULL != m_cpSharedMemory)
		::UnmapViewOfFile(m_cpSharedMemory);
	if (NULL != m_hSharedMemory)
		::CloseHandle(m_hSharedMemory);

	if (soxr != NULL)
		soxr_delete(soxr);

	if (m_vstInited)
	{
		if (m_rpcClient != NULL)
		{
			if (m_rpcClient->get_connection_state() == rpc::client::connection_state::connected)
				m_rpcClient->async_call("CloseApplication");
			m_rpcClient->~client();
			m_rpcClient = NULL;
		}

		if (m_rpcSrv != NULL)
		{
			m_rpcSrv->close_sessions();
			m_rpcSrv->stop();
			m_rpcSrv->~server();
			m_rpcSrv = NULL;
		}

		m_vstInited = false;
	}
}

///< Called when plug-in is switched to off
void MAmiVSTi::suspend()
{

}

bool MAmiVSTi::createSharedMemory()
{
	DWORD dwSharedMemorySize = ((m_mami_sample_rate / 50) * 2) * sizeof(int32_t);
	char name[256];
	sprintf_s(name, 256, "MAmi_%d", m_vstPort);
	m_hSharedMemory = ::CreateFileMapping(
		INVALID_HANDLE_VALUE  // �t�@�C���n���h��( ���L�������̏ꍇ�́A0xffffffff(INVALID_HANDLE_VALUE)���w�� )
		, NULL                  // SECURITY_ATTRIBUTES�\����
		, PAGE_READWRITE        // �ی쑮��( PAGE_READONLY / PAGE_READWRITE / PAGE_WRITECOPY, SEC_COMMIT / SEC_IMAGE / SEC_NOCACHE / SEC_RESERVE )
		, 0                     // �t�@�C���}�b�s���O�ő�T�C�Y(HIGH)
		, dwSharedMemorySize    // �t�@�C���}�b�s���O�ő�T�C�Y(LOW)
		, name // ���L����������
	);
	if (NULL == m_hSharedMemory)
	{
		MessageBox(0, _T("MAmiVST: Failed to createSharedMemory(1)."), 0, 0);
		return false;
	}
	// ���L�������̃}�b�s���O
	m_cpSharedMemory = (CHAR*)::MapViewOfFile(
		m_hSharedMemory         // �t�@�C���}�b�s���O�I�u�W�F�N�g�̃n���h��
		, FILE_MAP_WRITE        // �A�N�Z�X���[�h( FILE_MAP_WRITE/ FILE_MAP_READ / FILE_MAP_ALL_ACCESS / FILE_MAP_COPY )
		, 0                     // �}�b�s���O�J�n�I�t�Z�b�g(LOW)
		, 0                     // �}�b�s���O�J�n�I�t�Z�b�g(HIGH)
		, dwSharedMemorySize    // �}�b�v�Ώۂ̃t�@�C���̃o�C�g��
	);
	if (NULL == m_cpSharedMemory)
	{
		::CloseHandle(m_hSharedMemory);
		m_hSharedMemory = NULL;
		MessageBox(0, _T("MAmiVST: Failed to createSharedMemory(2)."), 0, 0);
		return false;
	}

	return true;
}

///< Called when plug-in is switched to on
void MAmiVSTi::resume()
{
	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	if (!m_vstInited)
	{
		//Start RPC server
		startRpcServer();

		if (!StartMAmi(m_mamiPath, (int)m_vst_sample_rate, m_vstPort))
		{
			MessageBox(0, _T("MAmiVST: Failed to launch MAmi.exe."), 0, 0);
			return;
		}

		while (!m_vstInited)
			Sleep(100);

		if (!createSharedMemory())
			return;
	}

	m_vstIsSuspend = false;
}

// does not call from MAmi
void MAmiVSTi::streamUpdatedL(int32_t size)
{
	if (isVstDisabled())
		return;
	if (m_cpSharedMemory == NULL)
		return;

	//m_tmpBuffer2ch = (int32_t*)m_cpSharedMemory;
}

void MAmiVSTi::streamUpdatedR(int32_t size1ch)
{
	if (isVstDisabled())
		return;
	if (m_cpSharedMemory == NULL)
		return;

	int32_t* tmpBuf2ch = (int32_t*)m_cpSharedMemory;

	//std::lock_guard<std::shared_mutex> lock1(mtxSoxrBuffer);
	mtxSoxrBuffer.lock();
	if (sampleRate != (double)m_mami_sample_rate)
	{
		size_t cnvSize = (size_t)round((double)size1ch * ((double)sampleRate / (double)m_mami_sample_rate));
		int32_t* outBuf2ch = new int32_t[cnvSize * 2];

		size_t idone = 0;
		size_t odone = 0;
		soxr_process(soxr, tmpBuf2ch, size1ch, &idone, outBuf2ch, cnvSize, &odone);

		size1ch = (int32_t)odone;
		tmpBuf2ch = outBuf2ch;
	}
	mtxSoxrBuffer.unlock();

	std::lock_guard<std::shared_mutex> lock2(mtxBuffer);
	//mtxBuffer.lock();	// Lock buffer

	size_t sz = m_streamBuffer2ch.size();
	m_streamBuffer2ch.resize(sz + (size_t)size1ch * 2);
	int* sbuf = &m_streamBuffer2ch[sz];
	for (size_t i = sz; i < sz + (size_t)size1ch; i++)
	{
		//interleave
		*sbuf++ = *tmpBuf2ch++;
		*sbuf++ = *tmpBuf2ch++;
	}

	//triple buffer size1ch to reduce sounding lag
	if (m_streamBuffer2ch.size() > (size_t)m_sampleFramesBlock * 3)
	{
		//Remove first block buffer
		m_streamBuffer2ch.erase(m_streamBuffer2ch.begin(), m_streamBuffer2ch.begin() + m_sampleFramesBlock);

		m_streamBufferOverflowed = true;
	}

	//mtxBuffer.unlock();	// Unlock buffer
}

// ============================================================================================
// �����M����������
// ============================================================================================

void MAmiVSTi::processReplacing(float** inputs, float** outputs, VstInt32 sampleFrames1ch)
{
	//���́A�o�͂�2�����z��œn�����B
	//���͂�-1.0f�`1.0f�̊Ԃœn�����B
	//�o�͂�-1.0f�`1.0f�̊Ԃŏ������ޕK�v������B
	float* inL = inputs[0];  //���� ���p
	float* inR = inputs[1];  //���� �E�p
	float* outL = outputs[0]; //�o�� ���p
	float* outR = outputs[1]; //�o�� �E�p

	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	if (m_streamBufferOverflowed)
	{
		//Remove first block buffer to prevent removing the next block by streamUpdatedR()
		m_streamBuffer2ch.erase(m_streamBuffer2ch.begin(), m_streamBuffer2ch.begin() + m_sampleFramesBlock);

		m_streamBufferOverflowed = false;
	}

	int* sbuf2ch = &m_streamBuffer2ch[0];
	auto size1ch = m_streamBuffer2ch.size() / 2;
	if (size1ch < (size_t)sampleFrames1ch)
	{
		for (VstInt32 i = 0; i < (VstInt32)size1ch; i++)
		{
			*(outL++) = *(inL++) + (float)*(sbuf2ch++) / (float)32767;
			*(outR++) = *(inR++) + (float)*(sbuf2ch++) / (float)32767;
		}
		for (VstInt32 i = (VstInt32)size1ch; i < sampleFrames1ch; i++)
		{
			*(outL++) = *(inL++);
			*(outR++) = *(inR++);
		}
		m_streamBuffer2ch.erase(m_streamBuffer2ch.begin(), m_streamBuffer2ch.begin() + size1ch * 2);
	}
	else
	{
		for (VstInt32 i = 0; i < sampleFrames1ch; i++)
		{
			*(outL++) = *(inL++) + (float)*(sbuf2ch++) / (float)32767;
			*(outR++) = *(inR++) + (float)*(sbuf2ch++) / (float)32767;
		}
		m_streamBuffer2ch.erase(m_streamBuffer2ch.begin(), m_streamBuffer2ch.begin() + (size_t)sampleFrames1ch * 2);
	}
}

// MIDI���b�Z�[�W��VST�ɕۑ�����B
// processReplacing()�̑O�ɕK��1�x�����Ăяo�����B
VstInt32 MAmiVSTi::processEvents(VstEvents* events)
{
	int loops = (events->numEvents);

	// VST�C�x���g�̉񐔂������[�v���܂킷�B
	for (int i = 0; i < loops; i++)
	{
		VstMidiEventBase* meb = (VstMidiEventBase*)(events->events[i]);
		switch (meb->type)
		{
		case kVstMidiType:
		{
			VstMidiEvent* midievent = (VstMidiEvent*)meb;

			m_rpcClient->async_call("SendMidiEvent",
				(unsigned char)midievent->midiData[0], (unsigned char)midievent->midiData[1], (unsigned char)midievent->midiData[2]);
			break;
		}
		case kVstSysExType:
		{
			VstMidiSysexEvent* midievent = (VstMidiSysexEvent*)meb;
			std::vector<unsigned char> buffer(midievent->sysexDump, midievent->sysexDump + midievent->dumpBytes);

			m_rpcClient->async_call("SendMidiSysEvent", buffer, midievent->dumpBytes);
			break;
		}
		}
	}

	//�@1��Ԃ��Ȃ���΂Ȃ�Ȃ�
	return 1;
}
