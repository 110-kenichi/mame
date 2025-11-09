/*
  ==============================================================================

	This file contains the basic framework code for a JUCE plugin processor.

  ==============================================================================
*/

#include "PluginProcessor.h"
#include "PluginEditor.h"



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


void MAmidiMEmoAudioProcessor::initVst()
{
	m_rpcClient = new rpc::client("localhost", m_mamiPort);

	m_mami_sample_rate = m_rpcClient->call("sample_rate").as<int>();

	m_sampleFramesBlock = std::max(m_lastSampleFrames, (m_mami_sample_rate / 50)) * 2;

	if (m_vst_sample_rate != 0)
		updateSampleRateCore();

	m_rpcClient->async_call("VstStarted");

	m_vstInited = true;
}

DWORD WINAPI MAmidiMEmoMainStartedProc(LPVOID lpParam)
{
	MAmidiMEmoAudioProcessor* mami = (MAmidiMEmoAudioProcessor*)lpParam;

	mami->initVst();

	return 0;
}

DWORD WINAPI startRpcServerCore(LPVOID lpParam)
{
	rpc::server* m_rpcSrv = reinterpret_cast<rpc::server*>(lpParam);

	m_rpcSrv->run();

	return 0;
}

void MAmidiMEmoAudioProcessor::startRpcServer()
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
			//setParameterAutomated(0, 0);
		});

	{
		//DWORD dwThreadId = 0L;
		//CreateThread(NULL, 0, startRpcServerCore, (void*)m_rpcSrv, 0, &dwThreadId);
		m_rpcSrv->async_run();
	}
}

int MAmidiMEmoAudioProcessor::isVstDisabled()
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

void MAmidiMEmoAudioProcessor::updateSampleRateCore()
{
	soxr_datatype_t itype = SOXR_INT32_I;
	soxr_datatype_t otype = SOXR_INT32_I;
	soxr_io_spec_t iospec = soxr_io_spec(itype, otype);
	soxr_quality_spec_t qSpec = soxr_quality_spec(SOXR_20_BITQ, 0);

	if (soxr != NULL)
		soxr_delete(soxr);

	soxr = soxr_create(m_mami_sample_rate, m_vst_sample_rate, 2, NULL, &iospec, &qSpec, NULL);
}


DWORD WINAPI closeRpcServer(LPVOID lpParam)
{
	rpc::server* m_rpcSrv = reinterpret_cast<rpc::server*>(lpParam);

	if (m_rpcSrv != NULL)
	{
		m_rpcSrv->suppress_exceptions(true);
		m_rpcSrv->close_sessions();
		try {
			m_rpcSrv->stop();
			m_rpcSrv->~server();
		}
		catch (...)
		{
			MessageBox(0, _T("MAmiVST: rpc error"), 0, 0);
		}
	}

	return 0;
}


bool MAmidiMEmoAudioProcessor::createSharedMemory()
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


//==============================================================================
MAmidiMEmoAudioProcessor::MAmidiMEmoAudioProcessor()
#ifndef JucePlugin_PreferredChannelConfigurations
	: AudioProcessor(BusesProperties()
#if ! JucePlugin_IsMidiEffect
#if ! JucePlugin_IsSynth
		.withInput("Input", juce::AudioChannelSet::stereo(), true)
#endif
		.withOutput("Output", juce::AudioChannelSet::stereo(), true)
#endif
	)
#endif
	, m_vstIsClosed(false)
	, m_vstIsSuspend(true)
	, soxr(NULL)
	, m_mami_sample_rate(0)
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
	, parameters(*this, nullptr)
{
	//Get MAmi path
	char dllPath[MAX_PATH];
	char dllDir[MAX_PATH];
	char iniPath[MAX_PATH];
	HMODULE hm = NULL;
	GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT, (LPCSTR)&StartMAmi, &hm);
	GetModuleFileName(hm, dllPath, sizeof(dllPath));

	strcpy_s(dllDir, MAX_PATH, dllPath);
	PathRemoveFileSpec(dllDir);

	strcpy_s(iniPath, MAX_PATH, dllPath);
	strip_ext(iniPath);
	strcat_s(iniPath, MAX_PATH, ".ini");
	char mamiPath[MAX_PATH];
	if (!GetPrivateProfileString("MAmi", "MAmiDir", ".\\", mamiPath, sizeof(mamiPath), iniPath))
	{
		MessageBox(0, _T("MAmiVST: Failed to load MAmiDir from MAmi ini file."), 0, 0);
		return;
	}
	PathCombineA(m_mamiPath, dllDir, mamiPath);

	char sampleRate[256];
	if (!GetPrivateProfileString("MAmi", "MAmiSampleRate", ".\\", sampleRate, sizeof(sampleRate), iniPath))
	{
		MessageBox(0, _T("MAmiVST: Failed to load MAmiSampleRate from MAmi ini file."), 0, 0);
		return;
	}
	try {
		// char配列をstd::stringに変換してからstoiに渡す
		m_mami_sample_rate = std::stoi(sampleRate);
	}
	catch (const std::invalid_argument&) {
		// 数字以外の文字が含まれていた場合
		MessageBox(0, _T("MAmiVST: Failed to load MAmiSampleRate from MAmi ini file."), 0, 0);
	}
	catch (const std::out_of_range&) {
		// intの範囲を超えていた場合
		MessageBox(0, _T("MAmiVST: Failed to load MAmiSampleRate from MAmi ini file."), 0, 0);
	}
	//resume() ==============================================================================

	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	if (!m_vstInited)
	{
		//Start RPC server
		startRpcServer();

		if (!StartMAmi(m_mamiPath, (int)m_mami_sample_rate, m_vstPort))
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

MAmidiMEmoAudioProcessor::~MAmidiMEmoAudioProcessor()
{
	//close() ==============================================================================

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
			//DWORD dwThreadId = 0L;
			//CreateThread(NULL, 0, closeRpcServer, (void*)m_rpcSrv, 0, &dwThreadId);

			//try {
			//	m_rpcSrv->stop();
			//	m_rpcSrv->~server();
			//}
			//catch (...)
			//{
			//}

			m_rpcSrv = NULL;
		}

		m_vstInited = false;
	}
}

//==============================================================================
const juce::String MAmidiMEmoAudioProcessor::getName() const
{
	return JucePlugin_Name;
}

bool MAmidiMEmoAudioProcessor::acceptsMidi() const
{
#if JucePlugin_WantsMidiInput
	return true;
#else
	return false;
#endif
}

bool MAmidiMEmoAudioProcessor::producesMidi() const
{
#if JucePlugin_ProducesMidiOutput
	return true;
#else
	return false;
#endif
}

bool MAmidiMEmoAudioProcessor::isMidiEffect() const
{
#if JucePlugin_IsMidiEffect
	return true;
#else
	return false;
#endif
}

double MAmidiMEmoAudioProcessor::getTailLengthSeconds() const
{
	return 0.0;
}

int MAmidiMEmoAudioProcessor::getNumPrograms()
{
	return 1;   // NB: some hosts don't cope very well if you tell them there are 0 programs,
	// so this should be at least 1, even if you're not really implementing programs.
}

int MAmidiMEmoAudioProcessor::getCurrentProgram()
{
	return 0;
}

void MAmidiMEmoAudioProcessor::setCurrentProgram(int index)
{
}

const juce::String MAmidiMEmoAudioProcessor::getProgramName(int index)
{
	return {};
}

void MAmidiMEmoAudioProcessor::changeProgramName(int index, const juce::String& newName)
{
}

//==============================================================================
void MAmidiMEmoAudioProcessor::prepareToPlay(double sampleRate, int samplesPerBlock)
{
	// Use this method as the place to do any pre-playback
	// initialisation that you need..

	std::lock_guard<std::shared_mutex> lock(mtxSoxrBuffer);

	sampleRate = sampleRate;
	m_vst_sample_rate = sampleRate;

	if (m_vstInited)
		updateSampleRateCore();

	if (m_lastSampleFrames != samplesPerBlock)
	{
		m_lastSampleFrames = samplesPerBlock;
		m_sampleFramesBlock = std::max(m_lastSampleFrames, (m_mami_sample_rate / 50)) * 2;
	}
}

void MAmidiMEmoAudioProcessor::releaseResources()
{
	// When playback stops, you can use this as an opportunity to free up any
	// spare memory, etc.
}

#ifndef JucePlugin_PreferredChannelConfigurations
bool MAmidiMEmoAudioProcessor::isBusesLayoutSupported(const BusesLayout& layouts) const
{
#if JucePlugin_IsMidiEffect
	juce::ignoreUnused(layouts);
	return true;
#else
	// This is the place where you check if the layout is supported.
	// In this template code we only support mono or stereo.
	// Some plugin hosts, such as certain GarageBand versions, will only
	// load plugins that support stereo bus layouts.
	if (layouts.getMainOutputChannelSet() != juce::AudioChannelSet::mono()
		&& layouts.getMainOutputChannelSet() != juce::AudioChannelSet::stereo())
		return false;

	// This checks if the input layout matches the output layout
#if ! JucePlugin_IsSynth
	if (layouts.getMainOutputChannelSet() != layouts.getMainInputChannelSet())
		return false;
#endif

	return true;
#endif
}
#endif

// does not call from MAmi
void MAmidiMEmoAudioProcessor::streamUpdatedL(int32_t size)
{
	//if (isVstDisabled())
	//	return;
	//if (m_cpSharedMemory == NULL)
	//	return;

	//m_tmpBuffer2ch = (int32_t*)m_cpSharedMemory;
}

void MAmidiMEmoAudioProcessor::streamUpdatedR(int32_t size1ch)
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
	if (m_vst_sample_rate != (double)m_mami_sample_rate)
	{
		size_t cnvSize = (size_t)round((double)size1ch * ((double)m_vst_sample_rate / (double)m_mami_sample_rate));
		int32_t* outBuf2ch = new int32_t[cnvSize * 2];

		size_t idone = 0;
		size_t odone = 0;
		soxr_process(soxr, tmpBuf2ch, size1ch, &idone, outBuf2ch, cnvSize, &odone);

		if (odone == 0)
			return;

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


void MAmidiMEmoAudioProcessor::processBlock(juce::AudioBuffer<float>& buffer, juce::MidiBuffer& midiMessages)
{
	juce::ScopedNoDenormals noDenormals;

	if (buffer.getNumChannels() < 2)
		return;

	processEvents(midiMessages);

	generateSamples(buffer);
}

/// <summary>
/// 
/// </summary>
/// <param name="midiMessages"></param>
void MAmidiMEmoAudioProcessor::processEvents(juce::MidiBuffer& midiMessages)
{
	// VSTイベントの回数だけループをまわす。
	std::vector<unsigned char> midiEvents1;
	std::vector<unsigned char> midiEvents2;
	std::vector<unsigned char> midiEvents3;
	int count = 0;
	int lastSamplePos = -1;
	for (const auto metadata : midiMessages)
	{
		const auto msg = metadata.getMessage();
		// メッセージが持つ生のバイトデータへのポインタを取得
		const auto* rawData = msg.getRawData();
		// メッセージの総バイト長を取得
		int dataLength = msg.getRawDataSize();
		// 3バイトのチャンネルメッセージ（ノートオン、CCなど）の処理
		if (dataLength == 3)
		{
			// 3バイトのデータは rawData[0], rawData[1], rawData[2] でアクセス可能
			midiEvents1.push_back((unsigned char)rawData[0]);
			midiEvents2.push_back((unsigned char)rawData[1]);
			midiEvents3.push_back((unsigned char)rawData[2]);
			count++;

			if (lastSamplePos < 0)
			{
				lastSamplePos = metadata.samplePosition;
			}
			else if (lastSamplePos != metadata.samplePosition)
			{
				int frame = (int)((lastSamplePos / m_vst_sample_rate) * 1000.0 * 1000.0);
				m_rpcClient->async_call("SendMidiEvents", eventId, frame, midiEvents1, midiEvents2, midiEvents3, count);
				eventId++;
				count = 0;
				lastSamplePos = metadata.samplePosition;
				midiEvents1.clear();
				midiEvents2.clear();
				midiEvents3.clear();
			}
		}
		// SysExなどの可変長メッセージの処理
		else if (msg.isSysEx())
		{
			// SysExデータは dataLength に応じて処理
			// 最初のバイトは 0xF0 (SysEx開始)
			// 最後のバイトは 0xF7 (SysEx終了)
			dataLength = msg.getRawDataSize();
			std::vector<unsigned char> sysexEvent;
			for (int j = 0; j < dataLength; j++)
				sysexEvent.push_back((unsigned char)rawData[j]);

			int frame = (int)((lastSamplePos / m_vst_sample_rate) * 1000.0 * 1000.0);
			m_rpcClient->async_call("SendMidiSysEvent", eventId, frame, sysexEvent, sysexEvent.size());

			eventId++;
		}
	}
	if (midiEvents1.size() != 0)
	{
		int frame = (int)((lastSamplePos / m_vst_sample_rate) * 1000.0 * 1000.0);
		m_rpcClient->async_call("SendMidiEvents", eventId, frame, midiEvents1, midiEvents2, midiEvents3, count);
		eventId++;
		count = 0;
		midiEvents1.clear();
		midiEvents2.clear();
		midiEvents3.clear();
	}
}

/// <summary>
/// 
/// </summary>
/// <param name="buffer"></param>
void MAmidiMEmoAudioProcessor::generateSamples(juce::AudioSampleBuffer& buffer)
{
	//入力、出力は2次元配列で渡される。
	//入力は-1.0f～1.0fの間で渡される。
	//出力は-1.0f～1.0fの間で書き込む必要がある。
	float* outL = buffer.getWritePointer(0); //出力 左用
	float* outR = buffer.getWritePointer(1); //出力 右用

	std::unique_lock<std::shared_mutex> lock(mtxBuffer);

	if (m_streamBufferOverflowed)
	{
		//Remove first block buffer to prevent removing the next block by streamUpdatedR()
		m_streamBuffer2ch.erase(m_streamBuffer2ch.begin(), m_streamBuffer2ch.begin() + m_sampleFramesBlock);

		m_streamBufferOverflowed = false;
	}
	int* sbuf2ch = m_streamBuffer2ch.data();
	auto size1ch = m_streamBuffer2ch.size() / 2;
	if (size1ch < (size_t)buffer.getNumSamples())
	{
		for (int i = 0; i < size1ch; i++)
		{
			*(outL++) = (float)*(sbuf2ch++) / (float)32767;
			*(outR++) = (float)*(sbuf2ch++) / (float)32767;
		}
		for (auto i = size1ch; i < buffer.getNumSamples(); i++)
		{
			*(outL++) = 0;
			*(outR++) = 0;
		}
		m_streamBuffer2ch.erase(m_streamBuffer2ch.begin(), m_streamBuffer2ch.begin() + size1ch * 2);
	}
	else
	{
		for (int i = 0; i < buffer.getNumSamples(); i++)
		{
			*(outL++) = (float)*(sbuf2ch++) / (float)32767;
			*(outR++) = (float)*(sbuf2ch++) / (float)32767;
		}
		m_streamBuffer2ch.erase(m_streamBuffer2ch.begin(), m_streamBuffer2ch.begin() + (size_t)buffer.getNumSamples() * 2);
	}
}

//==============================================================================
bool MAmidiMEmoAudioProcessor::hasEditor() const
{
	return true; // (change this to false if you choose to not supply an editor)
}

juce::AudioProcessorEditor* MAmidiMEmoAudioProcessor::createEditor()
{
	return new MAmidiMEmoAudioProcessorEditor(*this);
}

//==============================================================================
void MAmidiMEmoAudioProcessor::getStateInformation(juce::MemoryBlock& destData)
{
	// You should use this method to store your parameters in the memory block.
	// You could do that either as raw data, or use the XML or ValueTree classes
	// as intermediaries to make it easy to save and load complex data.

	// getChunk() ==============================================================================
	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	if (isVstDisabled())
		return;

	auto buff = m_rpcClient->call("SaveData").as<std::vector<unsigned char>>();

	//saveData.clear();
	saveData.resize(buff.size());
	std::copy(buff.begin(), buff.end(), saveData.data());

	destData.append(saveData.data(), saveData.size());
}

void MAmidiMEmoAudioProcessor::setStateInformation(const void* data, int sizeInBytes)
{
	// You should use this method to restore your parameters from this memory block,
	// whose contents will have been created by the getStateInformation() call.

	// setChunk() ==============================================================================

	std::lock_guard<std::shared_mutex> lock(mtxBuffer);

	if (isVstDisabled())
		return;

	//	LoadData((byte*)data, byteSize);
	saveData.resize(sizeInBytes);
	std::copy((unsigned char*)data, (unsigned char*)data + sizeInBytes, saveData.data());
	m_rpcClient->call("LoadData", saveData, sizeInBytes);
}

//==============================================================================
// This creates new instances of the plugin..
juce::AudioProcessor* JUCE_CALLTYPE createPluginFilter()
{
	return new MAmidiMEmoAudioProcessor();
}
