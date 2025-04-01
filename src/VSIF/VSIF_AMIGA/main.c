#include "support/gcc8_c_support.h"
#include <proto/exec.h>
#include <proto/dos.h>
#include <proto/graphics.h>
#include <graphics/gfxbase.h>
#include <graphics/view.h>
#include <exec/execbase.h>
#include <exec/io.h>
#include <exec/types.h>
#include <exec/memory.h>
#include <graphics/gfxmacros.h>
#include <hardware/custom.h>
#include <hardware/dmabits.h>
#include <hardware/intbits.h>
#include <devices/serial.h>
#include <exec/ports.h>
#include <dos/dos.h>
#include <dos/dosextens.h>
#include <devices/audio.h>
#include <dos/stdio.h>

//config
//#define LOG
#define SERIAL
#define SERIAL_BUFFER_SIZE 256
#define SAMPLE_PCM

#define DMAF_AUD_AGAIN(yy) DMAF_AUD ## yy
#define DMAF_AUD(y) DMAF_AUD_AGAIN(y)

#define INTENA_AUD_AGAIN(yy) INTF_AUD ## yy
#define INTENA_AUD(y) INTENA_AUD_AGAIN(y)

static UBYTE recvBuffer[SERIAL_BUFFER_SIZE] = {0};

struct MsgPort *serialPort;

struct IOExtSer *serialIO;

BYTE* SineData;

BYTE* StopData;

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

static volatile struct PlayData reqPlayData[4][256] = {0};

//static volatile struct PlayData *curPlayData[4] = {0};
static volatile struct PlayData curPlayData[4] = {0};

struct ExecBase *SysBase;
volatile struct Custom *custom;
struct DosLibrary *DOSBase;
//struct GfxBase *GfxBase;

//backup
static UWORD SystemInts;
static UWORD SystemDMA;
static UWORD SystemADKCON;
static volatile APTR VBR=0;
static APTR SystemIrq;

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

void WaitVbl() {
	debug_start_idle();
	while (1) {
		volatile ULONG vpos=*(volatile ULONG*)0xDFF004;
		vpos&=0x1ff00;
		if (vpos!=(311<<8))
			break;
	}
	while (1) {
		volatile ULONG vpos=*(volatile ULONG*)0xDFF004;
		vpos&=0x1ff00;
		if (vpos==(311<<8))
			break;
	}
	debug_stop_idle();
}

// $64, 68, 6c, 70, 74, 78, 7c for level 1-7 interrupts
// http://www.winnicki.net/amiga/memmap/MoreInts.html
void SetAudioInterruptHandler(APTR interrupt) {
	*(volatile APTR*)(((UBYTE*)VBR)+0x70) = interrupt;
}

APTR GetAudioInterruptHandler() {
	return *(volatile APTR*)(((UBYTE*)VBR)+0x70);
}

void TakeSystem() {
	Forbid();
	//Save current interrupts and DMA settings so we can restore them upon exit. 
	SystemADKCON=custom->adkconr;
	SystemInts=custom->intenar;
	SystemDMA=custom->dmaconr;

	//ActiView=GfxBase->ActiView; //store current view
	//https://amigadev.elowar.com/read/ADCD_2.1/Includes_and_Autodocs_2._guide/node0459.html
	//https://amigadev.elowar.com/read/ADCD_2.1/Libraries_Manual_guide/node0333.html
	//LoadView(0);
	//Wait for the top of the next video frame.
	//http://amigadev.elowar.com/read/ADCD_2.1/Includes_and_Autodocs_2._guide/node048A.html
	//WaitTOF();
	//WaitTOF();

	//WaitVbl();
	//WaitVbl();

	//https://amigadev.elowar.com/read/ADCD_2.1/Includes_and_Autodocs_3._guide/node030C.html
	//get the blitter for private usage
	//OwnBlitter();
	//WaitBlit();	
	//https://amigadev.elowar.com/read/ADCD_2.1/Includes_and_Autodocs_3._guide/node0203.html
	//	Disable -- disable interrupt processing.
	//Disable();

	//custom->intena=0x7fff;//disable all interrupts
	//custom->intreq=0x7fff;//Clear any interrupts that were pending

	custom->dmacon = DMAF_AUDIO;
	//custom->dmacon=0x7fff;//Clear all DMA channels
#if 0
	//set all colors black
	for(int a=0;a<32;a++)
		custom->color[a]=0;
#endif	

	//WaitVbl();
	//WaitVbl();

	VBR=GetVBR();
	SystemIrq=GetAudioInterruptHandler(); //store interrupt register
}

void FreeSystem() { 
	//WaitVbl();
	//WaitBlit();
	custom->intena=0x7fff;//disable all interrupts
	//custom->intena = INTF_INTEN | INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3;
	custom->intreq=0x7fff;//Clear any interrupts that were pending
	//custom->dmacon = DMAF_AUDIO;
	custom->dmacon=0x7fff;//Clear all DMA channels

	//restore interrupts
	SetAudioInterruptHandler(SystemIrq);

	/*Restore system copper list(s). */
	//custom->cop1lc=(ULONG)GfxBase->copinit;
	//custom->cop2lc=(ULONG)GfxBase->LOFlist;
	//custom->copjmp1=0x7fff; //start coppper

	/*Restore all interrupts and DMA settings. */
	custom->intena=SystemInts|0x8000;
	custom->dmacon=SystemDMA|0x8000;
	custom->adkcon=SystemADKCON|0x8000;

	for(int i=0; i<256; i++)
	{
		if(pcmDataTable[i].dataPtr != NULL)
			FreeMem(pcmDataTable[i].dataPtr, pcmDataTable[i].length);
	}

	//WaitBlit();	
	//DisownBlitter();
	Enable();

	//LoadView(ActiView);
	//WaitTOF();
	//WaitTOF();

	Permit();
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

	//wait_next_scanline(); wait_next_scanline();
	//aud_memcpy(&custom->aud[ch], aud);
	// custom->aud[ch].ac_ptr = (UWORD*)StopData;
	// custom->aud[ch].ac_len = 4;
	// custom->aud[ch].ac_per = 1;
	//custom->aud[ch].ac_per = aud->ac_per;
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
	// wait_next_scanline(); wait_next_scanline();
	// custom->aud[ch].ac_len = 4;
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
	int waitMask = SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F | (1L << serialPort->mp_SigBit);
	SendIO((struct IORequest *)serialIO);  // 非同期リクエストを送信
	while(1)
	{
		if (Wait(waitMask) & SIGBREAKF_CTRL_C)
		{
			ret = -2;
			break;
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
	//int waitMask = SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F | (1L << serialPort->mp_SigBit);

	// if(length > SERIAL_BUFFER_SIZE)
	// {
	// 	serialIO->IOSer.io_Data = (APTR)array;
	// 	serialIO->IOSer.io_Length = SERIAL_BUFFER_SIZE;
	// 	array += SERIAL_BUFFER_SIZE;
	// 	length -= SERIAL_BUFFER_SIZE;
	// }else{
	// 	serialIO->IOSer.io_Data = (APTR)array;
	// 	serialIO->IOSer.io_Data = (APTR)array;
	// 	serialIO->IOSer.io_Length = length;
	// 	length -= length;
	// }
	//SendIO((struct IORequest *)serialIO);  // 非同期リクエストを送信

	//while(1)
	{
		// if (Wait(waitMask) & SIGBREAKF_CTRL_C)
		// {
		// 	ret = -2;
		// 	break;
		// }
		/*
		if(length > SERIAL_BUFFER_SIZE)
		{
			serialIO->IOSer.io_Data = (APTR)array;
			serialIO->IOSer.io_Length = SERIAL_BUFFER_SIZE;
			array += SERIAL_BUFFER_SIZE;
			length -= SERIAL_BUFFER_SIZE;
		}else{
			serialIO->IOSer.io_Data = (APTR)array;
			serialIO->IOSer.io_Data = (APTR)array;
			serialIO->IOSer.io_Length = length;
			length -= length;
		}*/
		serialIO->IOSer.io_Data = (APTR)array;
		serialIO->IOSer.io_Data = (APTR)array;
		serialIO->IOSer.io_Length = length;
		if(DoIO((struct IORequest *)serialIO) == 0)
		{
			ret = 0;
			//break;
		}
		// if(length == 0)
		// {
		// 	ret = 0;
		// 	break;
		// }

		// 受信待機
		//This function determines the current state of an I/O request and returns FALSE if the I/O has not yet completed. 
		// if(CheckIO((struct IORequest *)serialIO))
		// {
		// 	if(WaitIO((struct IORequest *)serialIO))
		// 	{
		// 		ret = -1;
		// 		break;
		// 	}
		// 	if(length == 0)
		// 	{
		// 		ret = 0;
		// 		break;
		// 	}
		// 	if(length > SERIAL_BUFFER_SIZE)
		// 	{
		// 		serialIO->IOSer.io_Data = (APTR)array;
		// 		serialIO->IOSer.io_Length = SERIAL_BUFFER_SIZE;
		// 		array += SERIAL_BUFFER_SIZE;
		// 		length -= SERIAL_BUFFER_SIZE;
		// 	}else{
		// 		serialIO->IOSer.io_Data = (APTR)array;
		// 		serialIO->IOSer.io_Data = (APTR)array;
		// 		serialIO->IOSer.io_Length = length;
		// 		length -= length;
		// 	}
		// 	SendIO((struct IORequest *)serialIO);  // 非同期リクエストを送信
		// }
	}
	serialIO->IOSer.io_Data = (APTR)recvBuffer;
	serialIO->IOSer.io_Length = 1;
	return ret;
}

int main() {
	SysBase = *((struct ExecBase**)4UL);
	custom = (struct Custom*)0xdff000;

	// We will use the graphics library only to locate and restore the system copper list once we are through.
	/*
	GfxBase = (struct GfxBase *)OpenLibrary((CONST_STRPTR)"graphics.library",0);
	if (!GfxBase)
		Exit(0);
	*/

	// used for printing
	DOSBase = (struct DosLibrary*)OpenLibrary((CONST_STRPTR)"dos.library", 0);
	if (!DOSBase)
		Exit(0);

	VWritef("MAmi VSIF Driver\n", NULL);

	TakeSystem();
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
		Write(Output(), (APTR)"Failed to create Port!\n", strlen("Failed to create Port!\n"));
		CloseLibrary((struct Library*)DOSBase);
		Exit(0);
	}

#ifdef LOG
	//VWritef("Serial setting 2.\n", NULL);
#endif

	// 2. I/O リクエストを作成
	serialIO = (struct IOExtSer *)CreateIORequest(serialPort, sizeof(struct IOExtSer));
	if (!serialIO) {
		Write(Output(), (APTR)"Failed to create IO Request!\n", strlen("Failed to create IO Request!\n"));
		DeleteMsgPort(serialPort);
		CloseLibrary((struct Library*)DOSBase);
		Exit(0);
	}

#ifdef LOG
	//VWritef("Serial setting 3.\n", NULL);
#endif

	// 3. serial.device を開く
	if (OpenDevice("serial.device", 0, (struct IORequest *)serialIO, 0) != 0) {
		Write(Output(), (APTR)"Failed to open serial.device!\n", strlen("Failed to open serial.device!\n"));
		DeleteIORequest((struct IORequest *)serialIO);
		DeleteMsgPort(serialPort);
		CloseLibrary((struct Library*)DOSBase);
		Exit(0);
	}

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
		Write(Output(), (APTR)"Failed to set serial parameters!\n", strlen("Failed to set serial parameters!\n"));
		CloseDevice((struct IORequest *)serialIO);
		DeleteIORequest((struct IORequest *)serialIO);
		DeleteMsgPort(serialPort);
		CloseLibrary((struct Library*)DOSBase);
		Exit(0);
	}
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

	//There is one int handler for all aud events
	//but the interrupts for each audio ch can be ene/dis indivisually
	SetAudioInterruptHandler((APTR)audioInterruptHandler);
	custom->intena = INTF_SETCLR | INTF_INTEN | INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3 | INTF_PORTS;

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

	// SineData[12 * 100 - 12] = -125;
	// SineData[12 * 100 - 11] = -100;
	// SineData[12 * 100 - 10] = -75;
	// SineData[12 * 100 - 9] = -50;
	// SineData[12 * 100 - 8] = -25;
	// SineData[12 * 100 - 7] = 0;

	// SineData[12 * 100 - 6] = 25;
	// SineData[12 * 100 - 5] = 50;
	// SineData[12 * 100 - 4] = 75;
	// SineData[12 * 100 - 3] = 100;
	// SineData[12 * 100 - 2] = 125;
	// SineData[12 * 100 - 1] = 127;

	pcmDataTable[0].dataPtr = SineData;
	pcmDataTable[0].length = 12 * 100;
	//pcmDataTable[0].loop  = 12*100 - 12;
	pcmDataTable[0].loop  = 0;

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
			FreeSystem();

			CloseLibrary((struct Library*)DOSBase);
			//CloseLibrary((struct Library*)GfxBase);

			VWritef("Exited.\n", NULL);
			Exit(0);
			return 0;
		}
	}
#endif
	
	// for(int i=0;i<4;i++)
	// 	reqStopPcm(i, FALSE);

#ifdef SERIAL

	// // 6. データを送信
	// char *message = "Hello, Amiga!\n";
	// serialIO->IOSer.io_Command = CMD_WRITE;
	// serialIO->IOSer.io_Data = (APTR)message;
	// serialIO->IOSer.io_Length = strlen(message);
	//serialIO->IOSer.io_Flags |= IOF_QUICK;  /* Set QuickIO Flag */

	// DoIO((struct IORequest *)serialIO); // 送信完了を待つ
	// Write(Output(), (APTR)"Sent data: %s\n", message);

	// 7. データを受信 (最大 255 バイト)
	serialIO->IOSer.io_Command = CMD_READ;
	serialIO->IOSer.io_Data = (APTR)recvBuffer;
	//serialIO->IOSer.io_Length = SERIAL_BUFFER_SIZE;
	serialIO->IOSer.io_Length = 1;

#endif
	VWritef("Ready. Press Ctrl-C to exit.\n", NULL);
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
					VWritef("PCM ", NULL);

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

					VWritef("Receiving... ", NULL);
#ifdef LOG
					ULONG arg[] = {id, len, loop};
					VWritef(" %N %N %N", arg);
#endif
					VWritef("\n", NULL);

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
							VWritef("\n", NULL);
							VWritef("No PCM memory.\n", NULL);
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
	else{
		VWritef("Data transfer error.\n", NULL);
		VWritef("Aborted.\n", NULL);
	}

#ifdef MUSIC
	p61End();
#endif

	FreeMem(StopData, 4);

	AbortIO((struct IORequest *)serialIO);
	/* Ask device to abort request, if pending */
	WaitIO((struct IORequest *)serialIO);
	/* Wait for abort, then clean up */

	// 8. デバイスを閉じる
	CloseDevice((struct IORequest *)serialIO);
	// 9. I/O リクエストを解放
	DeleteIORequest((struct IORequest *)serialIO);
	// 10. メッセージポートを解放
	DeleteMsgPort(serialPort);

	// END
	FreeSystem();

	CloseLibrary((struct Library*)DOSBase);
	//CloseLibrary((struct Library*)GfxBase);

	Exit(0);
}
