// copyright-holders:K.Ito
#include <windows.h>
#include <stdio.h>

//memidimemo

typedef void(CALLBACK* MainWrapperProc)(HMODULE);

typedef int(CALLBACK* InitializeDotNetProc)();
typedef int(CALLBACK* HasExitedProc)();
typedef void(CALLBACK* VstStartedProc)();
typedef void(CALLBACK* SoundUpdatingProc)();
typedef void(CALLBACK* SoundUpdatedProc)();
typedef void(CALLBACK* RestartApplicationProc)();
typedef void(CALLBACK* SetVSTiModeProc)();
typedef int(CALLBACK* IsVSTiModeProc)();
typedef void(CALLBACK* SendMidiEventProc)(unsigned char data1, unsigned char data2, unsigned char data3);
typedef void(CALLBACK* SendMidiSysEventProc)(unsigned char *data, int length);
typedef int(CALLBACK* CloseApplicationProc)();
typedef void(CALLBACK*  LoadDataProc)(unsigned char* data, int length);
typedef int(CALLBACK* SaveDataProc)(void** saveBuf);
typedef void(CALLBACK* SoundTimerCallbackProc)();

InitializeDotNetProc initializeDotNet = 0;
HasExitedProc hasExited = 0;
VstStartedProc vstStarted = 0;
SoundUpdatingProc soundUpdating = 0;
SoundUpdatedProc soundUpdated = 0;
RestartApplicationProc restartApplication = 0;
SetVSTiModeProc setVSTiMode = 0;
IsVSTiModeProc isVSTiMode = 0;
SendMidiEventProc sendMidiEvent = 0;
SendMidiSysEventProc sendMidiSysEvent = 0;
CloseApplicationProc closeApplication = 0;
LoadDataProc loadData = 0;
SaveDataProc saveData = 0;
SoundTimerCallbackProc soundTimerCallback = 0;

DWORD WINAPI StartMAmidiMEmoMainThread(LPVOID lpParam)
{
	HMODULE hModule = LoadLibrary("wrapper.dll");
	if (hModule != NULL)
	{
		FARPROC proc = GetProcAddress(hModule, "_MainWarpper@4");
		if (proc != NULL)
		{
			MainWrapperProc main = reinterpret_cast<MainWrapperProc>(proc);
			main(GetModuleHandle(NULL));
		}
	}
	return 0;
}

int isMAmidiMEmoMainStarted = 0;

int IsStartMAmidiMEmoMainStarted()
{
	return isMAmidiMEmoMainStarted;
}

int isVSTiModeFlag = 0;

void StartMAmidiMEmoMain()
{
	HMODULE hModule = LoadLibrary("wrapper.dll");
	FARPROC proc;

	//init .NET
	proc = GetProcAddress(hModule, "InitializeDotNet");
	if (proc != NULL)
		proc();

	//VSTi mode
	proc = GetProcAddress(hModule, "SetVSTiMode");
	if (proc != NULL)
		setVSTiMode = reinterpret_cast<SetVSTiModeProc>(proc);
	if (isVSTiModeFlag)
		setVSTiMode();

	// API handle
	proc = GetProcAddress(hModule, "HasExited");
	if (proc != NULL)
		hasExited = reinterpret_cast<HasExitedProc>(proc);
	proc = GetProcAddress(hModule, "VstStarted");
	if (proc != NULL)
		vstStarted = reinterpret_cast<VstStartedProc>(proc);
	proc = GetProcAddress(hModule, "SoundUpdating");
	if (proc != NULL)
		soundUpdating = reinterpret_cast<SoundUpdatingProc>(proc);
	proc = GetProcAddress(hModule, "SoundUpdated");
	if (proc != NULL)
		soundUpdated = reinterpret_cast<SoundUpdatedProc>(proc);
	proc = GetProcAddress(hModule, "RestartApplication");
	if (proc != NULL)
		restartApplication = reinterpret_cast<RestartApplicationProc>(proc);
	proc = GetProcAddress(hModule, "IsVSTiMode");
	if (proc != NULL)
		isVSTiMode = reinterpret_cast<IsVSTiModeProc>(proc);
	proc = GetProcAddress(hModule, "SendMidiEvent");
	if (proc != NULL)
		sendMidiEvent = reinterpret_cast<SendMidiEventProc>(proc);
	proc = GetProcAddress(hModule, "SendMidiSysEvent");
	if (proc != NULL)
		sendMidiSysEvent = reinterpret_cast<SendMidiSysEventProc>(proc);
	proc = GetProcAddress(hModule, "CloseApplication");
	if (proc != NULL)
		closeApplication = reinterpret_cast<CloseApplicationProc>(proc);
	proc = GetProcAddress(hModule, "LoadData");
	if (proc != NULL)
		loadData = reinterpret_cast<LoadDataProc>(proc);
	proc = GetProcAddress(hModule, "SaveData");
	if (proc != NULL)
		saveData = reinterpret_cast<SaveDataProc>(proc);
	proc = GetProcAddress(hModule, "SoundTimerCallback");
	if (proc != NULL)
		soundTimerCallback = reinterpret_cast<SoundTimerCallbackProc>(proc);

	// Launch MAmi
	proc = GetProcAddress(hModule, "MainWarpper");
	if (proc != NULL)
	{
		char path[MAX_PATH];
		HMODULE hm = NULL;

		GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS |
			GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
			(LPCSTR)&StartMAmidiMEmoMain, &hm);
		GetModuleFileName(hm, path, sizeof(path));

		MainWrapperProc main = reinterpret_cast<MainWrapperProc>(proc);
		main(hm);
	}

	/*
	DWORD dwThreadId = 0L;
	HANDLE hThread = CreateThread(NULL, 0, StartMAmidiMEmoMainThread, 0, 0, &dwThreadId);
	WaitForSingleObject(hThread, INFINITE);
	CloseHandle(hThread);*/

	isMAmidiMEmoMainStarted = 1;
}

int HasExited()
{
	if (hasExited == 0)
		return 0;

	return hasExited();
}

void VstStarted()
{
	vstStarted();
}

void SoundUpdating()
{
	return soundUpdating();
}

void SoundUpdated()
{
	return soundUpdated();
}

void MamiOutputDebugString(char* pszFormat, ...)
{
	va_list	argp;
	char pszBuf[256];
	va_start(argp, pszFormat);
	vsprintf(pszBuf, pszFormat, argp);
	va_end(argp);
	OutputDebugString(pszBuf);
}

void RestartApplication()
{
	return restartApplication();
}

void SetVSTiMode()
{
	isVSTiModeFlag = 1;
}

int IsVSTiMode()
{
	if (isVSTiMode == 0)
		return 0;

	return isVSTiMode();
}

void SendMidiEvent(unsigned char data1, unsigned char data2, unsigned char data3)
{
	sendMidiEvent(data1, data2, data3);
}

void SendMidiSysEvent(unsigned char *data, int length)
{
	sendMidiSysEvent(data, length);
}

void CloseApplication()
{
	closeApplication();
}

void  LoadData(unsigned char* data, int length)
{
	loadData(data, length);
}

int SaveData(void** saveBuf)
{
	return saveData(saveBuf);
}

void SoundTimerCallback()
{
	if(soundTimerCallback != 0)
		soundTimerCallback();
}