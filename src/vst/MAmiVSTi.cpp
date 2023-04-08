// copyright-holders:K.Ito
#include "MAmiVSTi.h"
#include <shlwapi.h>
#include <random>


// ============================================================================================
// このVSTを生成するための関数
// ============================================================================================
AudioEffect* createEffectInstance(audioMasterCallback audioMaster)
{
	//newでこのVSTを生成したポインタを返す
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
		"\"%s\" -audio_latency 0 -samplerate %d -sound none -http_port %d", lpApplicationName, sampleRate, port);

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
// このVSTの初期化
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
	, eventId(0)
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

	AEffEditor* editor = new DummyVstEditor(this);
	setEditor(editor);

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
	m_rpcSrv->bind("ParameterAutomated", [&]()
		{
			setParameterAutomated(0, 0);
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

	//saveData.clear();
	saveData.resize(buff.size());
	std::copy(buff.begin(), buff.end(), saveData.data());

	*data = saveData.data();

	return (VstInt32)saveData.size();
}

///< Host restores plug-in state
VstInt32 MAmiVSTi::setChunk(void* data, VstInt32 byteSize, bool isPreset)
{
	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	if (isVstDisabled())
		return 0;

	//	LoadData((byte*)data, byteSize);
	saveData.resize(byteSize);
	std::copy((unsigned char*)data, (unsigned char*)data + byteSize, saveData.data());
	m_rpcClient->call("LoadData", saveData, byteSize);

	//std::vector<unsigned char> buffer((unsigned char*)data, ((unsigned char*)data) + byteSize);
	//m_rpcClient->call("LoadData", buffer, byteSize);

	return 0;
}

///< Called when plug-in is initialized
void MAmiVSTi::open()
{
}

DWORD WINAPI closeRpcServer(LPVOID lpParam)
{
	rpc::server* m_rpcSrv = reinterpret_cast<rpc::server*>(lpParam);

	if (m_rpcSrv != NULL)
	{
		m_rpcSrv->close_sessions();
		try {
			m_rpcSrv->stop();
			m_rpcSrv->~server();
		}
		catch (...)
		{
		}
	}

	return 0;
}

///< Called when plug-in will be released
void MAmiVSTi::close()
{
	std::lock_guard<std::shared_mutex> lock(mtxBuffer);
	std::lock_guard<std::shared_mutex> lock2(mtxSoxrBuffer);

	m_vstIsClosed = true;

	if (NULL != m_cpSharedMemory)
		::UnmapViewOfFile(m_cpSharedMemory);
	m_cpSharedMemory = NULL;
	if (NULL != m_hSharedMemory)
		::CloseHandle(m_hSharedMemory);
	m_hSharedMemory = NULL;

	if (soxr != NULL)
		soxr_delete(soxr);
	soxr = NULL;

	if (m_vstInited)
	{
		if (m_rpcClient != NULL)
		{
			if (m_rpcClient->get_connection_state() == rpc::client::connection_state::connected)
				m_rpcClient->call("CloseApplication");
			m_rpcClient->~client();
			m_rpcClient = NULL;
		}

		if (m_rpcSrv != NULL)
		{
			DWORD dwThreadId = 0L;
			CreateThread(NULL, 0, closeRpcServer, (void*)m_rpcSrv, 0, &dwThreadId);
			m_rpcSrv = NULL;
		}

		//if (m_rpcSrv != NULL)
		//{
		//	m_rpcSrv->close_sessions();
		//	m_rpcSrv->stop();
		//	m_rpcSrv->~server();
		//	m_rpcSrv = NULL;
		//}

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
		INVALID_HANDLE_VALUE  // ファイルハンドル( 共有メモリの場合は、0xffffffff(INVALID_HANDLE_VALUE)を指定 )
		, NULL                  // SECURITY_ATTRIBUTES構造体
		, PAGE_READWRITE        // 保護属性( PAGE_READONLY / PAGE_READWRITE / PAGE_WRITECOPY, SEC_COMMIT / SEC_IMAGE / SEC_NOCACHE / SEC_RESERVE )
		, 0                     // ファイルマッピング最大サイズ(HIGH)
		, dwSharedMemorySize    // ファイルマッピング最大サイズ(LOW)
		, name // 共有メモリ名称
	);
	if (NULL == m_hSharedMemory)
	{
		MessageBox(0, _T("MAmiVST: Failed to createSharedMemory(1)."), 0, 0);
		return false;
	}
	// 共有メモリのマッピング
	m_cpSharedMemory = (CHAR*)::MapViewOfFile(
		m_hSharedMemory         // ファイルマッピングオブジェクトのハンドル
		, FILE_MAP_WRITE        // アクセスモード( FILE_MAP_WRITE/ FILE_MAP_READ / FILE_MAP_ALL_ACCESS / FILE_MAP_COPY )
		, 0                     // マッピング開始オフセット(LOW)
		, 0                     // マッピング開始オフセット(HIGH)
		, dwSharedMemorySize    // マップ対象のファイルのバイト数
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
	//if (isVstDisabled())
	//	return;
	//if (m_cpSharedMemory == NULL)
	//	return;

	//m_tmpBuffer2ch = (int32_t*)m_cpSharedMemory;
}

void MAmiVSTi::streamUpdatedR(int32_t size1ch)
{
	std::lock_guard<std::shared_mutex> lock1(mtxSoxrBuffer);

	if (isVstDisabled())
		return;
	if (m_cpSharedMemory == NULL)
		return;
	if (soxr == NULL)
		return;

	int32_t* tmpBuf2ch = (int32_t*)m_cpSharedMemory;

	//std::lock_guard<std::shared_mutex> lock1(mtxSoxrBuffer);
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
// 音声信号処理部分
// ============================================================================================

void MAmiVSTi::processReplacing(float** inputs, float** outputs, VstInt32 sampleFrames1ch)
{
	//入力、出力は2次元配列で渡される。
	//入力は-1.0f～1.0fの間で渡される。
	//出力は-1.0f～1.0fの間で書き込む必要がある。
	float* inL = inputs[0];  //入力 左用
	float* inR = inputs[1];  //入力 右用
	float* outL = outputs[0]; //出力 左用
	float* outR = outputs[1]; //出力 右用

	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	if (m_streamBufferOverflowed)
	{
		//Remove first block buffer to prevent removing the next block by streamUpdatedR()
		m_streamBuffer2ch.erase(m_streamBuffer2ch.begin(), m_streamBuffer2ch.begin() + m_sampleFramesBlock);

		m_streamBufferOverflowed = false;
	}

	int* sbuf2ch = m_streamBuffer2ch.data();
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

// MIDIメッセージをVSTに保存する。
// processReplacing()の前に必ず1度だけ呼び出される。
VstInt32 MAmiVSTi::processEvents(VstEvents* events)
{
	int loops = (events->numEvents);
	VstTimeInfo* ti = getTimeInfo(kVstTransportChanged | kVstTransportPlaying | kVstTransportRecording);

	// VSTイベントの回数だけループをまわす。
	std::vector<unsigned char> midiEvents1;
	std::vector<unsigned char> midiEvents2;
	std::vector<unsigned char> midiEvents3;
	int count = 0;

	for (int i = 0; i < loops; i++)
	{
		VstMidiEventBase* meb = (VstMidiEventBase*)(events->events[i]);
		switch (meb->type)
		{
			case kVstMidiType:
			{
				VstMidiEvent* midievent = (VstMidiEvent*)meb;
				midiEvents1.push_back((unsigned char)midievent->midiData[0]);
				midiEvents2.push_back((unsigned char)midievent->midiData[1]);
				midiEvents3.push_back((unsigned char)midievent->midiData[2]);
				count++;
				//m_rpcClient->async_call("SendMidiEvent", processId,
				//	(unsigned char)midievent->midiData[0], (unsigned char)midievent->midiData[1], (unsigned char)midievent->midiData[2]);
				break;
			}
			case kVstSysExType:
			{
				VstMidiSysexEvent* midievent = (VstMidiSysexEvent*)meb;
				std::vector<unsigned char> buffer(midievent->sysexDump, midievent->sysexDump + midievent->dumpBytes);
				m_rpcClient->async_call("SendMidiSysEvent", eventId, buffer, midievent->dumpBytes);
				break;
			}
		}
	}

	LONG64 eid = eventId;
	if (!(ti->flags & (kVstTransportPlaying | kVstTransportRecording)))
		eid = 0;

	if (count != 0)
	{
		int frame = (int)(ti->samplePos / (ti->sampleRate / 60.0));
		m_rpcClient->async_call("SendMidiEvents", eid, frame, midiEvents1, midiEvents2, midiEvents3, count);
	}

	eventId++;

	//　1を返さなければならない
	return 1;
}


// ============================================================================================
// パラメーターの設定、表示を行うメンバー関数
// ============================================================================================
void  MAmiVSTi::setParameter(VstInt32 index, float value)
{
	//indexで指定されたパラメータに値を設定する。valueは0.0f～1.0fで与えられる。
	switch (index)
	{
	case 0: // indexが0の場合、tremolospeedの値を設定
		break;
	}
}

float MAmiVSTi::getParameter(VstInt32 index)
{
	//indexで指定されたパラメータの値を0.0f～1.0fの範囲で返す。
	float value = 0;
	switch (index)
	{
	case 0: // indexが0の場合、tremolospeedの値を0～1の範囲にして返す
		break;
	}
	return value;
}

void  MAmiVSTi::getParameterName(VstInt32 index, char* text)
{
	//indexで指定されたパラメータの名前をtextに格納する
	switch (index)
	{
	case 0: // indexが0の場合、tremolospeedのパラメーター名を返す
		vst_strncpy(text, "(Dummy)", kVstMaxParamStrLen);
		break;
	}
}

void  MAmiVSTi::getParameterLabel(VstInt32 index, char* label)
{
	//indexで指定されたパラメータの単位をlabelに格納する
	switch (index)
	{
	case 0: // indexが0の場合、tremolospeedの単位名を返す
		vst_strncpy(label, " ", kVstMaxParamStrLen);
		break;
	}
}

void  MAmiVSTi::getParameterDisplay(VstInt32 index, char* text)
{
	//indexで指定されたパラメータの表示内容をtextに格納する
	switch (index)
	{
	case 0: // indexが0の場合、tremolospeedの表示値を返す
		break;
	}
	float2string(0, text, kVstMaxParamStrLen);
}

// ============================================================================================
// プリセットプログラムの設定を行うメンバー関数
// ============================================================================================
void MAmiVSTi::setProgram(VstInt32 program)
{
	// programで指定された設定を内部のパラメータに反映する
	switch (program)
	{
	case 0: // 1つ目のプリセットの指定があった場合
		break;
	}

	// 現在のプリセット番号を記憶する (curProgramは継承元クラスの変数)
	curProgram = program;
}

DummyVstEditor::DummyVstEditor(AudioEffect* effectx)
	: AEffEditor(effectx), hwnd_e(NULL), fParam1(1.0f) {}

bool DummyVstEditor::getRect(ERect** erect) {
	static ERect r = { 0, 0, HEIGHT, WIDTH };
	*erect = &r;
	return true;
}

bool DummyVstEditor::open(void* ptr) {
	systemWindow = ptr;

	if (regist_count == 0) {
		WNDCLASS wd;
		wd.style = 0;
		wd.lpfnWndProc = WindowProc;
		wd.cbClsExtra = 0;
		wd.cbWndExtra = 0;
		wd.hInstance = (HINSTANCE)hInstance;
		wd.hIcon = NULL;
		wd.hCursor = NULL;
		wd.hbrBackground = GetSysColorBrush(COLOR_BTNFACE);
		wd.lpszMenuName = NULL;
		wd.lpszClassName = lpszAppName;

		RegisterClass(&wd);
	}

	regist_count++;

	HWND hwnd = CreateWindowEx(
		NULL,
		lpszAppName,
		"",
		WS_CHILD | WS_VISIBLE,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		WIDTH,
		HEIGHT,
		(HWND)systemWindow,
		NULL,
		(HINSTANCE)hInstance,
		NULL);

	hwnd_e = hwnd;

	SetProp(hwnd, PROP_WINPROC, this);

	SetWindowLong(hwnd, GWLP_USERDATA, (LONG_PTR)this);

	return true;
}

void DummyVstEditor::close() {
	hwnd_e = NULL;

	regist_count--;

	if (regist_count == 0) {
		UnregisterClass(lpszAppName, (HINSTANCE)hInstance);
	}
}

void DummyVstEditor::idle() {
	AEffEditor::idle();
	effect->setParameterAutomated(0, 0);
}

void DummyVstEditor::setParameter(VstInt32 index, float value) {
	if (hwnd_e == NULL) {
		return;
	}

	setParam1(effect->getParameter(index));

	InvalidateRect(hwnd_e, NULL, TRUE);
}

void DummyVstEditor::valueChanged(VstInt32 index, float value) {
	effect->setParameterAutomated(index, value);
}

/* プロシージャ */
LRESULT WINAPI DummyVstEditor::WindowProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam) {
	static char buf[256];
	static HWND hButton1;

	switch (message) {
	case WM_CREATE:
		//hButton1 = CreateWindow(
		//	"BUTTON",
		//	"0.5",
		//	WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
		//	10,
		//	10,
		//	80,
		//	20,
		//	hWnd,
		//	(HMENU)ID_B1,
		//	(HINSTANCE)hInstance
		//	, NULL);
		return 0;
	case WM_COMMAND:
		switch (LOWORD(wParam)) {
			//ボタン１を押したとき
		case ID_B1:
			DummyVstEditor* nmve = (DummyVstEditor*)GetProp(hWnd, PROP_WINPROC);
			nmve->valueChanged(0, 0.5f);
			return 0;
		}
		break;
	case WM_PAINT:
	{
		RECT rect;
		SIZE size;
		PAINTSTRUCT ps;
		HDC hDC = BeginPaint(hWnd, &ps);

		GetClientRect(hWnd, &rect);

		//nmVstEditor* nmve = (nmVstEditor*)GetProp(hWnd, PROP_WINPROC);

		//sprintf(buf, "%f", nmve->getParam1());
		sprintf_s(buf, "Keep this window open to prevent DATA LOSS or save data manually.");

		GetTextExtentPoint32(hDC, buf, strlen(buf), &size);

		SetBkMode(hDC, TRANSPARENT);

		TextOut(hDC, ((rect.right - rect.left) - size.cx) / 2, ((rect.bottom - rect.top) - size.cy) / 2, buf, strlen(buf));

		EndPaint(hWnd, &ps);
	}
	return 0;
	case WM_DESTROY:
		RemoveProp(hWnd, PROP_WINPROC);
		PostQuitMessage(0);
		return 0;
	default:
		break;
	}

	return DefWindowProc(hWnd, message, wParam, lParam);
}
