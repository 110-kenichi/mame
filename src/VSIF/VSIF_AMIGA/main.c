#include "support/gcc8_c_support.h"
#include <proto/exec.h>
#include <proto/dos.h>

#include <exec/execbase.h>
#include <exec/io.h>
#include <hardware/custom.h>
#include <hardware/dmabits.h>
#include <hardware/intbits.h>
#include <devices/serial.h>
#include <dos/dos.h>
#include <dos/stdio.h>

#include <workbench/startup.h>
#include <proto/intuition.h>
#include <intuition/intuition.h>

#include <midi/camdbase.h>
#include <midi/camd.h>
#include <midi/mididefs.h>
#include <midi/midiprefs.h>
#include <proto/camd.h>


//config
//#define NOGUI
//#define LOG
#define NO_INT
#define SERIAL
//#define MIDI
#define SER_BPS 31250
#define SERIAL_BUFFER_SIZE 256
#define SAMPLE_PCM

#define DMAF_AUD_AGAIN(yy) DMAF_AUD ## yy
#define DMAF_AUD(y) DMAF_AUD_AGAIN(y)

#define INTENA_AUD_AGAIN(yy) INTF_AUD ## yy
#define INTENA_AUD(y) INTENA_AUD_AGAIN(y)

#define XON 0x11
#define XOFF 0x13

struct ExecBase *SysBase = NULL;
struct DosLibrary *DOSBase = NULL;
struct IntuitionBase *IntuitionBase = NULL;

#ifdef MIDI
struct MidiNode *midiNode = NULL;
struct MidiLink *midiLink = NULL;
struct Library *CamdBase = NULL;
#endif 

//backup
static UWORD SystemInts = 0;
static UWORD SystemDMA = 0;
static UWORD SystemADKCON = 0;
static volatile APTR VBR = 0;
static APTR SystemIrq = 0;

static BOOL closeDev = FALSE;
static BOOL deleteIo = FALSE;
struct MsgPort *serialPort = NULL;
struct IOExtSer *serialIO = NULL;
volatile struct Custom *custom = NULL;
static struct Window *mainWin = NULL;

UBYTE recvBufferId = 0;
UBYTE recvBuffer[2][SERIAL_BUFFER_SIZE] = {0};

static BYTE* SineData = NULL;

static BYTE* StopData = NULL;

struct PcmData
{
	BYTE *dataPtr;
	UWORD length;
	UWORD loop;
};

struct PcmData pcmDataTable[256];

struct PlayData
{
	UWORD playCount; 
	struct AudChannel aud;
	struct PcmData *pcm;
};

static volatile struct PlayData curPlayData[4] = {0};

static APTR GetVBR(void) {
	APTR vbr = 0;
	UWORD getvbr[] = { 0x4e7a, 0x0801, 0x4e73 }; // MOVEC.L VBR,D0 RTE

	if (SysBase->AttnFlags & AFF_68010) 
	{
		CacheClearU();
		vbr = (APTR)Supervisor((ULONG (*)())getvbr);
	}

	return vbr;
}

// $64, 68, 6c, 70, 74, 78, 7c for level 1-7 interrupts
// http://www.winnicki.net/amiga/memmap/MoreInts.html
void SetAudioInterruptHandler(APTR interrupt) {
	*(volatile APTR*)(((UBYTE*)VBR)+0x70) = interrupt;
}

APTR GetAudioInterruptHandler() {
	return *(volatile APTR*)(((UBYTE*)VBR)+0x70);
}

static __attribute__((interrupt)) void audioInterruptHandler();

void TakeSystem() {
#ifndef NO_INT
	Forbid();
	//Save current interrupts and DMA settings so we can restore them upon exit. 
	SystemADKCON=custom->adkconr;
	SystemInts=custom->intenar;
	SystemDMA=custom->dmaconr;

	//https://amigadev.elowar.com/read/ADCD_2.1/Includes_and_Autodocs_3._guide/node0203.html
	//	Disable -- disable interrupt processing.
	Disable();

	//custom->intena=0x7fff;//disable all interrupts
	//custom->intreq=0x7fff;//Clear any interrupts that were pending
	custom->intena = INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3 | INTF_PORTS;
	custom->intreq = INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3 | INTF_PORTS;

	custom->dmacon = DMAF_AUDIO;
	//custom->dmacon=0x7fff;//Clear all DMA channels

	VBR=GetVBR();
	SystemIrq=GetAudioInterruptHandler(); //store interrupt register

	//There is one int handler for all aud events
	//but the interrupts for each audio ch can be ene/dis indivisually
	SetAudioInterruptHandler((APTR)audioInterruptHandler);
	custom->intena = INTF_SETCLR | INTF_INTEN | INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3 | INTF_PORTS;
#endif
}

void FreeSystem() { 
#ifndef NO_INT
	//custom->intena=0x7fff;//disable all interrupts
	custom->intena = INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3 | INTF_PORTS;
	//custom->intreq=0x7fff;//Clear any interrupts that were pending
	custom->intreq=INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3 | INTF_PORTS;
	custom->dmacon = DMAF_AUDIO;
	//custom->dmacon=0x7fff;//Clear all DMA channels
#endif
	if(!StopData)
		FreeMem(StopData, 4);

	if(serialIO != NULL)
	{
		if(closeDev)
		{
			// Ask device to abort request, if pending
			AbortIO((struct IORequest *)serialIO);
			// Wait for abort, then clean up
			WaitIO((struct IORequest *)serialIO);
			// 8. デバイスを閉じる
			CloseDevice((struct IORequest *)serialIO);
		}
		if(deleteIo)
		{
			// 9. I/O リクエストを解放
			DeleteIORequest((struct IORequest *)serialIO);
		}
	}
	if(serialPort != NULL)
	{
		// 10. メッセージポートを解放
		DeleteMsgPort(serialPort);
	}

#ifndef NO_INT
	//restore interrupts
	SetAudioInterruptHandler(SystemIrq);

	//Restore all interrupts and DMA settings.
	custom->intena=SystemInts|0x8000;
	custom->dmacon=SystemDMA|0x8000;
	custom->adkcon=SystemADKCON|0x8000;
#endif
	for(int i=0; i<256; i++)
	{
		if(pcmDataTable[i].dataPtr != NULL)
			FreeMem(pcmDataTable[i].dataPtr, pcmDataTable[i].length);
	}

#ifdef MIDI
	if (midiLink)
		RemoveMidiLink(midiLink);
	if (midiNode)
		DeleteMidi(midiNode);
	if(CamdBase != NULL)
		CloseLibrary((struct Library *)CamdBase);
#endif

	if(mainWin != NULL)
		CloseWindow(mainWin);

	if(IntuitionBase != NULL)
		CloseLibrary((struct Library *)IntuitionBase);

	if(DOSBase != NULL)
		CloseLibrary((struct Library*)DOSBase);

#ifndef NO_INT
	Enable();
	Permit();
#endif
	Exit(0);
}

void aud_memcpy(volatile struct AudChannel *dest, volatile struct AudChannel *src) {
    UWORD  *d32 = (UWORD *)dest;
    UWORD *s32 = (UWORD *)src;

    // 2バイト単位でコピー
	*d32++ = *s32++;
	*d32++ = *s32++;
	*d32++ = *s32++;
	*d32++ = *s32++;
	*d32++ = *s32++;
}

void wait_next_scanline() {
    UWORD current_line = custom->vhposr & 0xFF; // 現在のスキャンライン取得

    while ((custom->vhposr & 0xFF) == current_line)
	{
        // 次のスキャンラインに進むまで待機
    }
}

void reqPlayPcm(int ch, int id,  UWORD volume, UWORD period)
{
	if(pcmDataTable[id].dataPtr == 0)
		return;

	{
		custom->dmacon = (DMAF_AUD0 << ch); // Stop DMA for AUD

		// volatile UWORD *ac_ptr; /* ptr to start of waveform data */
		// volatile UWORD ac_len;	/* length of waveform in words */
		// volatile UWORD ac_per;	/* sample period */
		// volatile UWORD ac_vol;	/* volume */
		struct PcmData *pcm = &pcmDataTable[id];

		// volatile struct AudChannel *aud = &custom->aud[ch];
		// aud->ac_ptr = (UWORD*)pcm->dataPtr;
		// aud->ac_len = pcm->length >> 1;
		// aud->ac_per = period;
		// aud->ac_vol = volume;

		ULONG *audl = (ULONG *)&custom->aud[ch];
		*audl++ = (ULONG)pcm->dataPtr;
		UWORD *aud = (UWORD *)audl;
		*aud++ = pcm->length >> 1;
		*aud++ = period;
		*aud = volume;

		custom->dmacon = DMAF_SETCLR | (DMAF_AUD0 << ch); // Start DMA for AUD
	}
	return;
	{
		struct PcmData *pcm = &pcmDataTable[id];
		struct PlayData pd = {0};
		volatile struct AudChannel *aud = &pd.aud;
		aud->ac_ptr = (UWORD*)pcm->dataPtr;
		aud->ac_len = pcm->length >> 1;
		aud->ac_per = period;
		aud->ac_vol = volume; //64:MAX
		pd.pcm = pcm;
		//pd.playCount = 0;

	#ifdef LOG
		ULONG arg[] = {pcm->length, pcm->loop};
		VWritef("PCM %N %N\n", arg);
	#endif

		// custom->intena = (INTF_AUD0 << ch); // Stop INT for AUD
		// custom->intreq = (INTF_AUD0 << ch); custom->intreq = (INTF_AUD0 << ch);
		custom->dmacon = (DMAF_AUD0 << ch); // Stop DMA for AUD
		//aud_memcpy(&custom->aud[ch], aud);
		custom->aud[ch].ac_len = 2;
		custom->aud[ch].ac_ptr = (UWORD*)StopData;
		custom->aud[ch].ac_vol = 0;
		custom->aud[ch].ac_per = 1;

		curPlayData[ch] = pd;

		//custom->intena = INTF_SETCLR | (INTF_AUD0 << ch); // Start INT for AUD
		custom->dmacon = DMAF_SETCLR | (DMAF_AUD0 << ch); // Start DMA for AUD
	}
}

/*
*/
void reqStopPcm(int ch)
{
	// custom->intena = (INTF_AUD0 << ch);
	// custom->intreq = (INTF_AUD0 << ch); custom->intreq = (INTF_AUD0 << ch);
	custom->dmacon = (DMAF_AUD0 << ch); // Stop DMA for AUD
	custom->aud[ch].ac_len = 0;
	custom->aud[ch].ac_ptr = (UWORD*)StopData;
	custom->aud[ch].ac_vol = 0;
	custom->aud[ch].ac_per = 1;
}

//https://www.youtube.com/watch?v=EDVRdlnHyoE
static __attribute__((interrupt)) void audioInterruptHandler() {
	UWORD intreqr = custom->intreqr;
	UWORD intreq = 0;
	
	volatile struct PlayData *cpd = curPlayData;
	volatile struct AudChannel *aud = custom->aud;
	//int ch = 0;
	for(int ch=0;ch<4; ch++)
	{
		if(intreqr & (INTF_AUD0 << ch))
		{
			if(cpd->playCount == 0)
			{
				aud_memcpy(&custom->aud[ch], &cpd->aud);
				cpd->playCount++;
			}else if(cpd->playCount == 1)
			{
				volatile struct PcmData *pcm = cpd->pcm;
				if(pcm->loop != 0xFFFF)
				{
					aud->ac_ptr = (volatile UWORD*)(pcm->dataPtr + pcm->loop);
					aud->ac_len = (pcm->length - pcm->loop) >> 1;
				}else{
					cpd->playCount++;
				}
			}else if(cpd->playCount == 2)
			{
				//volatile struct PcmData *pcm = cpd->pcm;
				//custom->intena = (INTF_AUD0 << ch); // Stop INT for AUD
				custom->dmacon = (DMAF_AUD0 << ch); // Stop DMA for AUD
				aud->ac_len = 0;
				aud->ac_ptr = (UWORD*)StopData;
				aud->ac_vol = 0;
				aud->ac_per = 1;
				cpd->playCount++;
			}
			intreq |= (INTF_AUD0 << ch);
		}
		cpd++;
		aud++;
	}
	custom->intreq = intreq; custom->intreq = intreq; //twice for a4000 bug.
}


BYTE writeArray(UBYTE *buffptr,ULONG length)
{
	serialIO->IOSer.io_Command = CMD_WRITE;
	serialIO->IOSer.io_Data = buffptr;
	serialIO->IOSer.io_Length = length;
	recvBufferId = (recvBufferId + 1) & 1;
	BYTE error = DoIO((struct IORequest *)serialIO);
	serialIO->IOSer.io_Command = CMD_READ;

	return error;
}

void requestSerial(ULONG length)
{
	serialIO->IOSer.io_Data = (APTR)recvBuffer[recvBufferId];
	serialIO->IOSer.io_Length = length;
	SendIO((struct IORequest *)serialIO);  // 非同期リクエストを送信
	recvBufferId = (recvBufferId + 1) & 1;
}
UBYTE * waitSerialData()
{
	//while(1)
#ifndef NOGUI 
	//while(1)
	{
		int waitMask = SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F | (1L << serialPort->mp_SigBit) | (1L << mainWin->UserPort->mp_SigBit);
		waitMask = Wait(waitMask);
		if (waitMask & (1L << serialPort->mp_SigBit))
		{
			// 受信待機
			//This function determines the current state of an I/O request and returns FALSE if the I/O has not yet completed. 
			if(CheckIO((struct IORequest *)serialIO))
				if(!WaitIO((struct IORequest *)serialIO))
					return serialIO->IOSer.io_Data;
		}else if (waitMask & SIGBREAKF_CTRL_C)
		{
			return NULL;
		}else if(waitMask & (1L << mainWin->UserPort->mp_SigBit))
		{
			struct IntuiMessage *msg;
			int close = 0;
			while ((msg = (struct IntuiMessage *)GetMsg(mainWin->UserPort))) {
				if (msg->Class == IDCMP_CLOSEWINDOW)
					close = 1;
				ReplyMsg((struct Message *)msg);
			}
			if (close == 1)
			{
				CloseWindow(mainWin);
				mainWin = NULL;
				return NULL;
			}
		}
	}
#endif
#ifdef NOGUI
	//while(1){
		ULONG waitMask = SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F | (1L << serialPort->mp_SigBit);
		waitMask = Wait(waitMask);
		if (waitMask & (1L << serialPort->mp_SigBit))
		{
			// 受信待機
			//This function determines the current state of an I/O request and returns FALSE if the I/O has not yet completed. 
			if(CheckIO((struct IORequest *)serialIO))
				if(!WaitIO((struct IORequest *)serialIO))
					return serialIO->IOSer.io_Data;
		}else if (waitMask & SIGBREAKF_CTRL_C)
		{
			return NULL;
		}
	//}
	#endif
	return NULL;
}

UBYTE *readArray(UBYTE *buffptr,ULONG length)
{
	serialIO->IOSer.io_Data = buffptr;
	serialIO->IOSer.io_Length = length;
	recvBufferId = (recvBufferId + 1) & 1;
	if(DoIO((struct IORequest *)serialIO) == 0)
		return (UBYTE *)serialIO->IOSer.io_Data;
	return NULL;
}

void showMessage(char *message)
{
	VWritef(message, NULL);
	VWritef("\n", NULL);

// 	if(mainWin)
// 	{
// 		// ダイアログを表示（標準の警告ウィンドウ）
// 		struct Window *dialog = OpenWindowTags(NULL,
// 			WA_Width,  300,
// 			WA_Height, 100,
// 			WA_Left,   50,
// 			WA_Top,    50,
// 			WA_Title,  (ULONG)"Message",
// 			WA_Flags, WFLG_CLOSEGADGET | WFLG_DRAGBAR | WFLG_DEPTHGADGET | WFLG_ACTIVATE, 
// 			WA_IDCMP, IDCMP_CLOSEWINDOW,
// 			TAG_END);

// 		if (dialog)
// 		{
// 			// メッセージテキスト（IntuiText）
// 			const struct IntuiText body = {
// 				1, // 前景色（1 = 白）
// 				0, // 背景色（0 = 黒）
// 				JAM2, // 描画モード（背景を塗りつぶす）
// 				10, 10, // 左上座標
// 				NULL, // フォント（NULL = デフォルト）
// 				(UBYTE *)message, // 表示する文字列
// 				NULL // 次のテキスト（NULL なら 1 行のみ）
// 			};
// 			// メッセージテキスト（IntuiText）
// 			const struct IntuiText ok = {
// 				1, // 前景色（1 = 白）
// 				0, // 背景色（0 = 黒）
// 				JAM2, // 描画モード（背景を塗りつぶす）
// 				10, 10, // 左上座標
// 				NULL, // フォント（NULL = デフォルト）
// 				(UBYTE *)"OK", // 表示する文字列
// 				NULL // 次のテキスト（NULL なら 1 行のみ）
// 			};
// 			AutoRequest(dialog, &body, &ok, NULL, 0, 0, 280, 60);
// 			CloseWindow(dialog);
// 		}
// #ifdef LOG
// 		VWritef(message, NULL);
// 		VWritef("\n", NULL);
// #endif
// 	}else{
// 		VWritef(message, NULL);
// 		VWritef("\n", NULL);
// 	}
}

void printText(char* message)
{
	if(mainWin)
	{
		int clientWidth = mainWin->Width - mainWin->BorderLeft - mainWin->BorderRight;
		int clientHeight = mainWin->Height - mainWin->BorderTop - mainWin->BorderBottom;
		int text_x = mainWin->BorderLeft;
		int text_y = mainWin->BorderTop;
		struct IntuiText label = {
			1, 0, JAM2, text_x, text_y, NULL, (UBYTE *)message, NULL
		};
		
		PrintIText(mainWin->RPort, &label, 0, 0);  // ウィンドウにラベルを描画
	}else{
		VWritef(message, NULL);
		VWritef("\n", NULL);
	}
}

ULONG MyHookFunction()
{
	VWritef("MIDI Rcv\n", NULL);

    return 0;
}

void InitHook(struct Hook *hook, ULONG (*c_function)(), APTR userdata)
{
	ULONG (*hookEntry)();
	hookEntry = NULL;

	hook->h_Entry	= hookEntry;
    hook->h_SubEntry = c_function;
    hook->h_Data	= userdata;
}

/*
*/
void main(struct WBStartup *wb)
{
	SysBase = *((struct ExecBase**)4UL);
	custom = (struct Custom*)0xdff000;

	TakeSystem();

	// used for printing
	DOSBase = (struct DosLibrary*)OpenLibrary((CONST_STRPTR)"dos.library", 0);
	if (!DOSBase)
		FreeSystem();
		
#ifdef NOGUI
	wb = NULL;
#endif
	if (wb) {
		// Intuition ライブラリを開く
		IntuitionBase = (struct IntuitionBase *)OpenLibrary((CONST_STRPTR)"intuition.library", 37);
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
	serialIO->io_SerFlags &= ~SERF_PARTY_ON;  // パリティなし
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
	serialIO->io_SerFlags &= ~SERF_PARTY_ON;  // パリティなし
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

	pcmDataTable[0].dataPtr = SineData;
	pcmDataTable[0].length = 12 * 100;
	pcmDataTable[0].loop  = 12*100 - 12;
	//pcmDataTable[0].loop  = 0;

	// PLAY SOUND
	//playPcm(0, 0, 64, 3546895 / (12 * 1000));
	//reqStopPcm(0, TRUE);
	//reqPlayDummyPcm(1, TRUE);
	//reqPlayDummyPcm(2, TRUE);
	//reqPlayDummyPcm(3, TRUE);
#endif

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
		printText("MAmi VSIF driver ready.");
	else
		printText("Ready.\n**Press Ctrl-C** to exit. If not, System may crash.");
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
					curPlayData[ch].aud.ac_vol = vol;
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
					curPlayData[ch].aud.ac_per = per;
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
	if(error == 0){
		if (!wb)
			VWritef("Exited\n", NULL);
			//printText("Exited");
	}else{
		VWritef("Aborted by transfer error!\n", NULL);
		//showMessage("Aborted by transfer error!");
	}

	// END
	FreeSystem();
}
