#include "support/gcc8_c_support.h"
#include <proto/exec.h>
#include <proto/dos.h>

#include <exec/execbase.h>
#include <hardware/custom.h>
#include <hardware/dmabits.h>
#include <hardware/intbits.h>
#include <devices/serial.h>
#include <dos/dos.h>
#include <dos/stdio.h>

#include <workbench/startup.h>
#include <proto/intuition.h>
#include <intuition/intuition.h>

//config
//#define LOG
#define SERIAL
#define SERIAL_BUFFER_SIZE 256
#define SAMPLE_PCM

#define DMAF_AUD_AGAIN(yy) DMAF_AUD ## yy
#define DMAF_AUD(y) DMAF_AUD_AGAIN(y)

#define INTENA_AUD_AGAIN(yy) INTF_AUD ## yy
#define INTENA_AUD(y) INTENA_AUD_AGAIN(y)

struct ExecBase *SysBase = NULL;
struct DosLibrary *DOSBase = NULL;
struct IntuitionBase *IntuitionBase = NULL;

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

UBYTE recvBuffer[SERIAL_BUFFER_SIZE] = {0};

static BYTE* SineData;

static BYTE* StopData;

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
}

void FreeSystem() { 
	//custom->intena=0x7fff;//disable all interrupts
	custom->intena = INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3 | INTF_PORTS;
	//custom->intreq=0x7fff;//Clear any interrupts that were pending
	custom->intreq=INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3 | INTF_PORTS;
	custom->dmacon = DMAF_AUDIO;
	//custom->dmacon=0x7fff;//Clear all DMA channels

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

	//restore interrupts
	SetAudioInterruptHandler(SystemIrq);

	//Restore all interrupts and DMA settings.
	custom->intena=SystemInts|0x8000;
	custom->dmacon=SystemDMA|0x8000;
	custom->adkcon=SystemADKCON|0x8000;

	for(int i=0; i<256; i++)
	{
		if(pcmDataTable[i].dataPtr != NULL)
			FreeMem(pcmDataTable[i].dataPtr, pcmDataTable[i].length);
	}

	if(mainWin != NULL)
		CloseWindow(mainWin);

	if(IntuitionBase != NULL)
		CloseLibrary((struct Library *)IntuitionBase);

	if(DOSBase != NULL)
		CloseLibrary((struct Library*)DOSBase);

	Enable();
	Permit();
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

/*
*/
void reqStopPcm(int ch, BOOL init)
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

int readCMD()
{
	int ret = -1;
	SendIO((struct IORequest *)serialIO);  // 非同期リクエストを送信
	while(1)
	{
		if (mainWin) {
			int waitMask = SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F | (1L << serialPort->mp_SigBit) | (1L << mainWin->UserPort->mp_SigBit);
			ULONG wait = Wait(waitMask);
			if (wait & SIGBREAKF_CTRL_C)
			{
				ret = -2;
				break;
			}
			struct IntuiMessage *msg;
			while ((msg = (struct IntuiMessage *)GetMsg(mainWin->UserPort))) {
				if (msg->Class == IDCMP_CLOSEWINDOW)
					ret = -2;
				ReplyMsg((struct Message *)msg);
			}
			if (ret == -2)
			{
				CloseWindow(mainWin);
				mainWin = NULL;
				break;
			}
		}else{
			int waitMask = SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F | (1L << serialPort->mp_SigBit);
			if (Wait(waitMask) & SIGBREAKF_CTRL_C)
			{
				ret = -2;
				break;
			}
		}
		// 受信待機
		//This function determines the current state of an I/O request and returns FALSE if the I/O has not yet completed. 
		if(CheckIO((struct IORequest *)serialIO))
		{
			if(!WaitIO((struct IORequest *)serialIO))
				ret = recvBuffer[0];
			break;
		}
	}
	return ret;
}

int readArray(UBYTE* array, USHORT length)
{
	int ret = -1;
	{
		serialIO->IOSer.io_Data = (APTR)array;
		serialIO->IOSer.io_Data = (APTR)array;
		serialIO->IOSer.io_Length = length;
		if(DoIO((struct IORequest *)serialIO) == 0)
			ret = 0;
	}
	serialIO->IOSer.io_Data = (APTR)recvBuffer;
	serialIO->IOSer.io_Length = 1;
	return ret;
}

void showMessage(char *message)
{
	if(mainWin)
	{
		// ダイアログを表示（標準の警告ウィンドウ）
		struct Window *dialog = OpenWindowTags(NULL,
			WA_Width,  300,
			WA_Height, 100,
			WA_Left,   50,
			WA_Top,    50,
			WA_Title,  (ULONG)"Message",
			WA_Flags,  WFLG_CLOSEGADGET | WFLG_DRAGBAR | WFLG_DEPTHGADGET,
			TAG_END);

		if (dialog)
		{
			// メッセージテキスト（IntuiText）
			const struct IntuiText body = {
				1, // 前景色（1 = 白）
				0, // 背景色（0 = 黒）
				JAM2, // 描画モード（背景を塗りつぶす）
				10, 10, // 左上座標
				NULL, // フォント（NULL = デフォルト）
				(UBYTE *)message, // 表示する文字列
				NULL // 次のテキスト（NULL なら 1 行のみ）
			};
			// メッセージテキスト（IntuiText）
			const struct IntuiText ok = {
				1, // 前景色（1 = 白）
				0, // 背景色（0 = 黒）
				JAM2, // 描画モード（背景を塗りつぶす）
				10, 10, // 左上座標
				NULL, // フォント（NULL = デフォルト）
				(UBYTE *)"OK", // 表示する文字列
				NULL // 次のテキスト（NULL なら 1 行のみ）
			};
			AutoRequest(dialog, &body, &ok, NULL, 0, 0, 280, 60);
			CloseWindow(dialog);
		}
	}else{
		VWritef(message, NULL);
	}
}

/*
*/
void main(int argc, char *argv[], struct WBStartup *wb)
{
	SysBase = *((struct ExecBase**)4UL);
	custom = (struct Custom*)0xdff000;

	TakeSystem();

	// used for printing
	DOSBase = (struct DosLibrary*)OpenLibrary((CONST_STRPTR)"dos.library", 0);
	if (!DOSBase)
		FreeSystem();
	
	if (argc == 0) {
		// Intuition ライブラリを開く
		IntuitionBase = (struct IntuitionBase *)OpenLibrary("intuition.library", 37);
		if (!IntuitionBase) {
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
			FreeSystem();

	}else
	{
        // CLI から実行された
		showMessage("MAmi VSIF Driver\n");
	}

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
		showMessage("Failed to create Port!\n");
		FreeSystem();
	}
#ifdef LOG
	//VWritef("Serial setting 2.\n", NULL);
#endif

	// 2. I/O リクエストを作成
	serialIO = (struct IOExtSer *)CreateIORequest(serialPort, sizeof(struct IOExtSer));
	if (!serialIO) {
		showMessage("Failed to create IO Request!\n");
		FreeSystem();
	}
#ifdef LOG
	//VWritef("Serial setting 3.\n", NULL);
#endif

	// 3. serial.device を開く
	if (OpenDevice("serial.device", 0, (struct IORequest *)serialIO, 0) != 0) {
		showMessage("Failed to open serial.device!\n");
		FreeSystem();
	}
	deleteIo = TRUE;

#ifdef LOG
	//VWritef("Serial setting 4.\n", NULL);
#endif

	// 4. シリアル通信の設定 (9600bps, 8N1)
	serialIO->io_Baud = 19200;              // 9600bps
	serialIO->io_ReadLen = 8;              // データ長: 8bit
	serialIO->io_WriteLen = 8;
	serialIO->io_StopBits = 1;             // ストップビット: 1
	serialIO->io_SerFlags = 0;
	serialIO->io_SerFlags &= ~SERF_PARTY_ON;  // パリティなし
	serialIO->io_SerFlags |= SERF_XDISABLED;
	serialIO->io_SerFlags |= SERF_RAD_BOOGIE;
	serialIO->io_SerFlags |= SERF_7WIRE;
	
#ifdef LOG
	//VWritef("Serial setting 5.\n", NULL);
#endif

	// 5. 設定を適用
	serialIO->IOSer.io_Command = SDCMD_SETPARAMS;
	if (DoIO((struct IORequest *)serialIO) != 0) {
		showMessage("Failed to set serial parameters!\n");
		FreeSystem();
	}
	closeDev = TRUE;
#endif

#ifdef LOG
	VWritef("Completed serial setting.\n", NULL);
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

#ifndef SERIAL
	for(int i = 0;i<50;i++)	
		WaitVbl();

	//reqPlayPcm(0, 0, 64, 3546895 / (12 * 500));
	int waitMask = SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F;
	while(1)
	{
		if (Wait(waitMask) & SIGBREAKF_CTRL_C)
		{
			// END
			VWritef("Exited.\n", NULL);
			FreeSystem();
			Exit(0);
		}
	}
#endif

#ifdef SERIAL

	// 7. データを受信 (最大 255 バイト)
	serialIO->IOSer.io_Command = CMD_READ;
	serialIO->IOSer.io_Data = (APTR)recvBuffer;
	//serialIO->IOSer.io_Length = SERIAL_BUFFER_SIZE;
	serialIO->IOSer.io_Length = 1;

#endif
	VWritef("Ready.\n**Press Ctrl-C** to exit. If not, System may crash.\n", NULL);
	int val = readCMD();
	while(val >= 0)
	{
		switch(val)
		{
			case 1:	// Volume
				{
					UBYTE dataPtr[] = {0, 0};
					val = readArray(dataPtr, 2);
					if(val < 0)
						break;

					UBYTE ch = dataPtr[0];
					UWORD vol = dataPtr[1];
					curPlayData[ch].aud.ac_vol = vol;
					custom->aud[ch].ac_vol = vol;
#ifdef LOG
					VWritef("Vol\n", NULL);
#endif
				}
				break;
			case 2:	// Pitch
				{
					UBYTE dataPtr[] = {0, 0, 0};
					val = readArray(dataPtr, 3);
					if(val < 0)
						break;

					UBYTE ch = dataPtr[0];
					UWORD per = ((UWORD)dataPtr[1] << 8) + dataPtr[2];
					curPlayData[ch].aud.ac_per = per;
					custom->aud[ch].ac_per = per;
#ifdef LOG
					VWritef("Pitch\n", NULL);
#endif
				}
				break;
			case 3:	//KEY ON
				{
					UBYTE dataPtr[] = {0, 0, 0, 0, 0};
					val = readArray(dataPtr, 5);
					if(val < 0)
						break;

					// ch 0 - 3
					UBYTE ch = dataPtr[0];
					// inst id 0 - 255
					UBYTE id = dataPtr[1];
					// vol 0 - 64;
					UWORD vol = dataPtr[2];
					// period 0 - 65535
					UWORD per = ((UWORD)dataPtr[3] << 8) + dataPtr[4];

					reqPlayPcm(ch, id, vol, per);
#ifdef LOG
					ULONG arg[] = {ch, id, vol, per};
					VWritef("KON %N %N %N %N\n", arg);
#endif
				}
				break;
			case 4: //KEY OFF
				{
					// ch 0 -3
					UBYTE dataPtr[] = {0};
					val = readArray(dataPtr, 1);
					if(val < 0)
						break;
					reqStopPcm(dataPtr[0], FALSE);
#ifdef LOG
					VWritef("KOFF\n", NULL);
#endif
				}
				break;
			case 5: //Filter ON/OFF
				{
					UBYTE dataPtr[] = {0};
					val = readArray(dataPtr, 1);
					if(val < 0)
						break;
					if(dataPtr[0] == 0)
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
			case 99:
				{
					VWritef("PCM Receiving...\n", NULL);

					reqStopPcm(0, FALSE);
					reqStopPcm(1, FALSE);
					reqStopPcm(2, FALSE);
					reqStopPcm(3, FALSE);

					UBYTE dataPtr[] = {0, 0, 0, 0, 0};
					val = readArray(dataPtr, 5);
					if(val < 0)
						break;
						
					UBYTE id = dataPtr[0];
					UWORD len = ((UWORD)dataPtr[1] << 8) + dataPtr[2];
					UWORD loop = ((UWORD)dataPtr[3] << 8) + dataPtr[4];
					if(loop >= len)
						loop = 0xFFFF;
#ifdef LOG
					ULONG arg[] = {id, len, loop};
					VWritef(" %N %N %N", arg);
#endif
					BYTE *oldPcm = pcmDataTable[id].dataPtr;
					UWORD oldLen = pcmDataTable[id].length;
					if(len != 0)
					{
						BYTE* pcmDataPtr = (BYTE*)AllocMem(len, MEMF_CHIP);
						if(pcmDataPtr != NULL)
						{
							val = readArray(pcmDataPtr, len);
							if(val < 0)
							{
								FreeMem(pcmDataPtr, len);
								break;
							}
						}else
						{
							UBYTE dataPtr[] = {0};
							for(int i=0;i<len;i++)
								val = readArray(dataPtr, 1);
							showMessage("No PCM memory!\n");
							val = -1;
							break;
						}

						pcmDataTable[id].dataPtr = pcmDataPtr;
						pcmDataTable[id].length = len;
						pcmDataTable[id].loop  = loop;
					}else{
						pcmDataTable[id].dataPtr = NULL;
						pcmDataTable[id].length = 0;
						pcmDataTable[id].loop  = 0;
					}
					if(oldPcm != NULL)
						FreeMem(oldPcm, oldLen);

					VWritef("PCM Received\n", NULL);
				}
				break;
			case 100:	// PCM Loop
				{
					UBYTE dataPtr[] = {0, 0, 0};
					val = readArray(dataPtr, 3);
					if(val < 0)
						break;
						
					UBYTE id = dataPtr[0];
					UWORD loop = ((UWORD)dataPtr[1] << 8) + dataPtr[2];
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
				break;
		}
		if(val < 0)
			break;
		val = readCMD();
	}
	if(val == -2)
		VWritef("Exited.\n", NULL);
	else
		showMessage("Data transfer error. Aborted.\n");

	// END
	FreeSystem();
}
