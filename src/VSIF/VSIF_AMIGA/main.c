#include "main.h"

struct ExecBase *SysBase = NULL;
struct DosLibrary *DOSBase = NULL;
struct IntuitionBase *IntuitionBase = NULL;

#ifdef MIDI
struct MidiNode *midiNode = NULL;
struct MidiLink *midiLink = NULL;
struct Library *CamdBase = NULL;
#endif 

//backup

BOOL closeDev = FALSE;
BOOL deleteIo = FALSE;

volatile struct Custom *custom = NULL;
struct Window *mainWin = NULL;

BYTE* SineData = NULL;
BYTE* StopData = NULL;

int WBmain(struct WBStartup *wb)
{
	SysBase = *((struct ExecBase**)4UL);
	custom = (struct Custom*)0xdff000;

	// used for printing
	DOSBase = (struct DosLibrary*)OpenLibrary((CONST_STRPTR)"dos.library", 0);
	if (!DOSBase)
		FreeSystem();
	
	TakeSystem();

	if (wb) {
		// Intuition ライブラリを開く
		IntuitionBase = (struct IntuitionBase *)OpenLibrary((CONST_STRPTR)"intuition.library", 0);
		if (!IntuitionBase) {
			showMessage("Failed to load GUI!");
			FreeSystem();
		}

		// ウィンドウを開く
		mainWin = OpenWindowTags(NULL,
			WA_Width,  300,                 // ウィンドウの幅
			WA_Height, 100,                 // ウィンドウの高さ
			WA_Left,   50,                  // 画面左からの位置
			WA_Top,    50,                  // 画面上からの位置
			WA_Title,  (ULONG)"MAmi VSIF Driver", // ウィンドウタイトル
			WA_Flags, WFLG_CLOSEGADGET | WFLG_DRAGBAR | WFLG_DEPTHGADGET | WFLG_ACTIVATE, 
			WA_IDCMP, IDCMP_CLOSEWINDOW,
			TAG_END);
		if (!mainWin)
		{
			showMessage("Failed to create GUI!");
			FreeSystem();
		}
	}else
	{
        // CLI から実行された
		showMessage("MAmi VSIF Driver");
	}

#ifdef MIDI
	CamdBase = OpenLibrary((CONST_STRPTR)"camd.library", 0);
	if (!CamdBase) {
		FreeSystem();
	}
#endif

#ifdef LOG
	VWritef("Completed take system.\n", NULL);
#endif

#ifdef SERIAL

#ifdef LOG
	//VWritef("Serial setting 1.\n", NULL);
#endif
	// 1. メッセージポートを作成
	serialPort = CreateMsgPort();
	if (!serialPort) {
		showMessage("Failed to create Port!");
		FreeSystem();
	}
#ifdef LOG
	//VWritef("Serial setting 2.\n", NULL);
#endif

	// 2. I/O リクエストを作成
	serialIO = (struct IOExtSer *)CreateIORequest(serialPort, sizeof(struct IOExtSer));
	if (!serialIO) {
		showMessage("Failed to create IO Request!");
		FreeSystem();
	}
#ifdef LOG
	//VWritef("Serial setting 3.\n", NULL);
#endif

	// 3. serial.device を開く
	serialIO->io_Baud = SER_BPS;              // 9600bps
	serialIO->io_ReadLen = 8;              // データ長: 8bit
	serialIO->io_WriteLen = 8;
	serialIO->io_StopBits = 1;             // ストップビット: 1
	serialIO->io_RBufLen = 8192;
	serialIO->io_SerFlags = 0;
//	serialIO->io_SerFlags &= ~SERF_PARTY_ON;  // パリティなし
	serialIO->io_SerFlags |= SERF_XDISABLED;
	serialIO->io_SerFlags |= SERF_RAD_BOOGIE;
	serialIO->io_SerFlags |= SERF_7WIRE;
	if (OpenDevice("serial.device", 0, (struct IORequest *)serialIO, 0) != 0) {
		showMessage("Failed to open serial.device!");
		FreeSystem();
	}
	deleteIo = TRUE;

#ifdef LOG
	//VWritef("Serial setting 4.\n", NULL);
#endif

	// 4. シリアル通信の設定 (9600bps, 8N1)
	serialIO->io_Baud = SER_BPS;              // 9600bps
	serialIO->io_ReadLen = 8;              // データ長: 8bit
	serialIO->io_WriteLen = 8;
	serialIO->io_StopBits = 1;             // ストップビット: 1
	serialIO->io_RBufLen = 8192;
	serialIO->io_SerFlags = 0;
//	serialIO->io_SerFlags &= ~SERF_PARTY_ON;  // パリティなし
	serialIO->io_SerFlags |= SERF_XDISABLED;
	serialIO->io_SerFlags |= SERF_RAD_BOOGIE;
	serialIO->io_SerFlags |= SERF_7WIRE;
	serialIO->IOSer.io_Command = SDCMD_SETPARAMS;
	if (DoIO((struct IORequest *)serialIO) != 0) {
		showMessage("Failed to set serial parameters!");
		FreeSystem();
	}
	closeDev = TRUE;

	// 5. データを受信 (最大 255 バイト)
	serialIO->IOSer.io_Command = CMD_READ;
#ifdef LOG
VWritef("Completed serial setting.\n", NULL);
#endif

	#endif

	for(int i=0;i<256;i++)
	{
		pcmDataTable[i].dataPtr = 0;
		pcmDataTable[i].length = 0;
		pcmDataTable[i].loop  = 0;
	}

#ifdef LOG
	VWritef("Completed init handler.\n", NULL);
#endif

	StopData = (BYTE*)AllocMem(4, MEMF_CHIP);
	StopData[0] = 0;
	StopData[1] = 0;
	StopData[2] = 0;
	StopData[3] = 0;

#ifdef SAMPLE_PCM

	SineData = (BYTE*)AllocMem(12*100, MEMF_CHIP);

	SineData[0] = 0;
	SineData[1] = 64;
	SineData[2] = 111;
	SineData[3] = 127;
	SineData[4] = 111;
	SineData[5] = 64;
	SineData[6] = 0;
	SineData[7] = -64;
	SineData[8] = -111;
	SineData[9] = -127;
	SineData[10] = -111;
	SineData[11] = -64;

	for(int i=0;i<12 * (100);i++)
		SineData[12 + i] = SineData[i];
/*
	SineData[12 * 100 - 12] = -125;
	SineData[12 * 100 - 11] = -100;
	SineData[12 * 100 - 10] = -75;
	SineData[12 * 100 - 9] = -50;
	SineData[12 * 100 - 8] = -25;
	SineData[12 * 100 - 7] = 0;

	SineData[12 * 100 - 6] = 25;
	SineData[12 * 100 - 5] = 50;
	SineData[12 * 100 - 4] = 75;
	SineData[12 * 100 - 3] = 100;
	SineData[12 * 100 - 2] = 125;
	SineData[12 * 100 - 1] = 127;
*/
	pcmDataTable[0].dataPtr = SineData;
	pcmDataTable[0].length = 12 * 100;
	//pcmDataTable[0].loop  = 12*100 - 12;
	//pcmDataTable[0].loop  = 0;

	// PLAY SOUND
	//playPcm(0, 0, 64, 3546895 / (12 * 1000));
	//reqStopPcm(0, TRUE);
	//reqPlayDummyPcm(1, TRUE);
	//reqPlayDummyPcm(2, TRUE);
	//reqPlayDummyPcm(3, TRUE);
#endif

	if(fileExists("default.vpcm"))
	{
		loadPcm("default.vpcm");
	}

#ifdef MIDI

    // MIDIポートを作成
	BYTE midisig = AllocSignal(-1);
	//struct MidiNode *ournode = NULL;
	struct Hook hook;
	//InitHook(&hook,MyHookFunction,NULL);
    midiNode = CreateMidi(
		MIDI_Name, (ULONG)"MAmi VSIF Driver",
		//MIDI_RecvHook, (ULONG)&hook,
		MIDI_MsgQueue, 256L,
		MIDI_SysExSize, 256L,
		MIDI_RecvSignal, midisig,
		MIDI_ClientType, MLTYPE_Receiver,
		TAG_END);
    if (midiNode == NULL)
	{
		VWritef("Failed to create MIDI Port!\n", NULL);
		//showMessage("Failed to create MIDI in!\n");
		FreeSystem();
    }

	ULONG arg[] = { (ULONG)midiNode->mi_ClientType, (ULONG)midiNode->mi_ReceiveHook, midiNode->mi_MsgQueueSize };
	VWritef("MIDI %X8 %X8 %X8\n", arg);
	
	// // MIDI リンクを作成 (すべてのポートから受信)
	// midiLink = AddMidiLink(midiNode, MLTYPE_Receiver, (ULONG)"in.1", 0, TAG_END);
	// if (!midiLink)
	// {
	// 	VWritef("Failed to create MIDI IN!\n", NULL);
	// 	FreeSystem();
	// }

	while (1) {
		ULONG signal = Wait(SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F | 1L << midisig);
		if(signal & SIGBREAKF_CTRL_C)
		{
			printText("Exited");
		 	FreeSystem();
		}
		VWritef("MIDI \n", NULL);

		MidiMsg msg;
		//WaitMidi(midiIn, &msg);
		//while (GetMidi(midiLink->ml_MidiNode, &msg))
		while (GetMidi(midiNode, &msg))
		{
			//ULONG arg[] = {msg.b[0], msg.b[1], msg.b[2], msg.b[3]};
			//VWritef("MIDI Rcv: Status=%X, Data1=%X, Data2=%X Data3=%X\n",arg);
			ULONG arg[] = { (ULONG)msg.l };
			VWritef("MIDI Rcv %X8\n", arg);
		}
		//VWritef("MIDI No", NULL);
	}
#endif

#ifdef SERIAL
	/*
	{
		requestSerial(1);
		UBYTE *dataBufPtr = waitSerialData();
		while(dataBufPtr != NULL)
		{
			ULONG arg[] = {*dataBufPtr};
			VWritef("%X2", arg);
			// char arg[] = {*dataBufPtr , 0};
			// VWritef(arg, NULL);
			requestSerial(1);
			dataBufPtr = waitSerialData();
		}
		VWritef("\nExited\n", NULL);
		FreeSystem();
	}
	//*/
	/*
	while(1)
	{
		{
			char xon[] = {(char)XON, 0};
			writeArray(xon, 2);
		}
		BYTE *dataBufPtr = readArray(recvBuffer[recvBufferId], 1);
		{
			char xof[] = {(char)XOFF, 0};
			writeArray(xof, 2);
		}
		if(dataBufPtr == NULL)
			break;
		char arg[] = {*dataBufPtr , 0};
		VWritef(arg, NULL);
		// ULONG args[] = {*dataBufPtr};
		// VWritef("%X2 ", args);
	}
	VWritef("\nExited\n", NULL);
	FreeSystem();
	//*/

/*
	//reqPlayPcm(0, 0, 64, 3546895 / (12 * 500));
	int waitMask = SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F;
	while(1)
	{
		if (Wait(waitMask) & SIGBREAKF_CTRL_C)
		{
			FreeSystem();
		}
	}
*/
/*
	serialIO->IOSer.io_Data = (APTR)recvBuffer[recvBufferId++];
	//serialIO->IOSer.io_Length = SERIAL_BUFFER_SIZE;
	//serialIO->IOSer.io_Length = 1;

	while(1){
		ULONG arg[] = {0};
		int v = readCMD();
		if(v < 0)
		{
			arg[0] = serialIO->IOSer.io_Error;
			VWritef("E2 %N\n", arg);
			break;
		}

		arg[0] = v;
		VWritef("S1 %N\n", arg);

		UBYTE *dataBufPtr = (UBYTE *)readArray(1);
		if(dataBufPtr == NULL)
		{
			arg[0] = serialIO->IOSer.io_Error;
			VWritef("E2 %N\n", arg);
			break;
		}
		arg[0] = dataBufPtr[0];
		VWritef("S2 %N\n", arg);
	}
	FreeSystem();
*/

#endif
	if(mainWin)
	{
		printText("MAmi VSIF driver ready.", 0, 0);
		printText("DO NOT touch mouse/key while sounding!", 0, 16);
	}else
	{
		printText("Ready.\n**Press Ctrl-C** to exit. If not, System may crash.", 0, 0);
	}

	//int val = readCMD();
	int error = 0;
	//UBYTE *dataBufPtr = (UBYTE *)readArray(6);
	requestSerial(5);
	UBYTE *dataBufPtr = waitSerialData();
	while(dataBufPtr != NULL)
	{
		UBYTE type = *dataBufPtr++;
		UBYTE ch = type >> 4;
		type = type & 0xf;
		switch(type)
		{
			case 1:	// Volume
				{
					requestSerial(5);

					UWORD vol = *dataBufPtr;
#ifndef NO_LOOP
					curPlayData[ch].aud.ac_vol = vol;
#endif
					custom->aud[ch].ac_vol = vol;
#ifdef LOG
					VWritef("Vol\n", NULL);
#endif
				}
				break;
			case 2:	// Pitch
				{
					requestSerial(5);

					UWORD per = ((UWORD)*dataBufPtr++ << 8) + *dataBufPtr;
#ifndef NO_LOOP
					curPlayData[ch].aud.ac_per = per;
#endif
					custom->aud[ch].ac_per = per;
#ifdef LOG
					VWritef("Pitch\n", NULL);
#endif
				}
				break;
			case 3:	//KEY ON
				{
					requestSerial(5);

					// inst id 0 - 255
					UBYTE id = *dataBufPtr++;
					// vol 0 - 64;
					UWORD vol = *dataBufPtr++;
					// period 0 - 65535
					UWORD per = ((UWORD)*dataBufPtr++ << 8) + *dataBufPtr;

					reqPlayPcm(ch, id, vol, per);
#ifdef LOG
					ULONG arg[] = {ch, id, vol, per};
					VWritef("KON %N %N %N %N\n", arg);
#endif
				}
				break;
			case 4: //KEY OFF
				{
					requestSerial(5);

					reqStopPcm(ch);
#ifdef LOG
					ULONG arg[] = {ch};
					VWritef("KOFF %N\n", arg);
#endif
				}
				break;
			case 5: //Filter ON/OFF
				{
					requestSerial(5);

					if(*dataBufPtr == 0)
		{
						//Filter off
						__asm volatile (
							"BSET.B #1,0xbfe001\n"
							:
							:
							:
							"cc", "memory");
#ifdef LOG
						VWritef("Flt OFF\n", NULL);
#endif
					}else
					{
						//Filter on
						__asm volatile (
							"BCLR.B #1,0xbfe001\n"
							:
							:
							:
							"cc", "memory");
#ifdef LOG
							VWritef("Flt ON\n", NULL);
#endif
					}
				}
				break;
			case 6:
				{
#ifdef LOG
					VWritef("PCM Receiving...\n", NULL);
#endif
						
					UBYTE id = *dataBufPtr++;
					UWORD len = ((UWORD)*dataBufPtr++ << 8) + *dataBufPtr++;
					//UWORD loop = ((UWORD)*dataBufPtr++ << 8) + *dataBufPtr;
					//if(loop >= len)
					//	loop = 0xFFFF;
					UWORD loop = 0;
#ifdef LOG
					ULONG arg[] = {id, len, loop};
					VWritef("%N %N %N\n", arg);
#endif
					struct PcmData *pdt = &pcmDataTable[id];
					BYTE *oldPcm = pdt->dataPtr;
					UWORD oldLen = pdt->length;
					if(len != 0)
					{
						BYTE* pcmDataPtr = (BYTE*)AllocMem(len, MEMF_CHIP);
						if(pcmDataPtr != NULL)
						{
							UBYTE *pcmptr = (UBYTE *)readArray(pcmDataPtr, len);
							if(pcmptr == NULL)
							{
			error = -1;
								FreeMem(pcmDataPtr, len);
								break;
		}
						}else
						{
							UWORD received = 0;
							UWORD chunk_size;
							while (received < len) {
								// 256バイト単位で受信（最後のブロックは余りを処理）
								chunk_size = (len - received >= SERIAL_BUFFER_SIZE) ? SERIAL_BUFFER_SIZE : (len - received);
								readArray(recvBuffer[recvBufferId], chunk_size);
								received += chunk_size;
	}
							showMessage("No PCM memory!");
							error = -1;
							break;
						}

						pdt->dataPtr = pcmDataPtr;
						pdt->length = len;
						pdt->loop  = loop;
					}else{
						pdt->dataPtr = NULL;
						pdt->length = 0;
						pdt->loop  = 0;
					}
					requestSerial(5);

					if(oldPcm != NULL)
						FreeMem(oldPcm, oldLen);

#ifdef LOG
					VWritef("PCM Received\n", NULL);
#endif
				}
				break;
			case 7:	// PCM Loop
	{
					requestSerial(5);

					UBYTE id = *dataBufPtr++;
					UWORD loop = ((UWORD)*dataBufPtr++ << 8) + *dataBufPtr;
					UWORD len = pcmDataTable[id].length;
					if(loop >= len)
						loop = 0xFFFF;
					pcmDataTable[id].loop = loop;
#ifdef LOG
					ULONG arg[] = {id, loop};
					VWritef("LOOP %N %N\n", arg);
#endif
				}
				break;
			default:
				error = -1;
				break;
		}
		if(error != 0)
			break;
		/*
		if(val < 0)
			break;
		val = readCMD();
		*/
		//dataBufPtr = (UBYTE *)readArray(6);
		dataBufPtr = (UBYTE *)waitSerialData();
	}
	if(error == 0 && dataBufPtr != (UBYTE *)0xFFFFFFFF){
		if (!wb)
			VWritef("Exited\n", NULL);
			//printText("Exited");
	}else{
		VWritef("Aborted by transfer error!\n", NULL);
		//showMessage("Aborted by transfer error!");
	}
	reqStopPcm(0); reqStopPcm(1); reqStopPcm(2); reqStopPcm(3);

	// END
	FreeSystem();
}

/*
*/
/* 通常のmain関数：CLIまたはWorkbench起動両方をここから呼ぶ */
int main(int argc, char **argv) {
	struct WBStartup *wb = (struct WBStartup *)argv;
    if (argc == 0) {
        // GUIから起動された
    } else {
        // CLIから起動された
    }
#ifdef NOGUI
	wb = NULL;
#endif
	WBmain(wb);
}