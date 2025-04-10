#include "main.h"

UWORD SystemInts = 0;
UWORD SystemDMA = 0;
UWORD SystemADKCON = 0;
APTR VBR = 0;
APTR SystemIrq = 0;

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
	SystemDMA=custom->dmaconr;

	Forbid();

#ifndef NO_INT
	//Save current interrupts and DMA settings so we can restore them upon exit. 
	SystemADKCON=custom->adkconr;
	SystemInts=custom->intenar;

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
#else
	SystemInts=custom->intenar;
	custom->intena = INTF_AUD0 | INTF_AUD1 | INTF_AUD2 | INTF_AUD3;
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

	if(serialIO)
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
	if(serialPort)
	{
		// 10. メッセージポートを解放
		DeleteMsgPort(serialPort);
	}

#ifndef NO_INT
	//restore interrupts
	SetAudioInterruptHandler(SystemIrq);

	//Restore all interrupts and DMA settings.
	custom->intena=SystemInts|0x8000;
	custom->adkcon=SystemADKCON|0x8000;
#else
	custom->intena=SystemInts|0x8000;
#endif
	custom->dmacon=SystemDMA|0x8000;

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

	if(mainWin)
		CloseWindow(mainWin);

	if(IntuitionBase)
		CloseLibrary((struct Library *)IntuitionBase);

	if(DOSBase)
		CloseLibrary((struct Library*)DOSBase);

#ifndef NO_INT
	Enable();
#endif
	Permit();

	Exit(0);
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

void printText(char* message, int ofst_x, int ofst_y)
{
	if(mainWin)
	{
		int clientWidth = mainWin->Width - mainWin->BorderLeft - mainWin->BorderRight;
		int clientHeight = mainWin->Height - mainWin->BorderTop - mainWin->BorderBottom;
		int text_x = mainWin->BorderLeft + ofst_x;
		int text_y = mainWin->BorderTop + ofst_y;
		struct IntuiText label = {
			1, 0, JAM2, text_x, text_y, NULL, (UBYTE *)message, NULL
		};
		
		PrintIText(mainWin->RPort, &label, 0, 0);  // ウィンドウにラベルを描画
	}else{
		VWritef(message, NULL);
		VWritef("\n", NULL);
	}
}

void wait_next_scanline() {
    UWORD current_line = custom->vhposr & 0xFF; // 現在のスキャンライン取得

    while ((custom->vhposr & 0xFF) == current_line)
	{
        // 次のスキャンラインに進むまで待機
    }
}
