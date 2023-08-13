//
//  SpcControlDevice.cpp
//  gimicUsbSpcPlay
//
//  Created by osoumen on 2014/06/14.
//  Copyright (c) 2014年 osoumen. All rights reserved.
//

#include "../interface/if_gimic_winusb.h"
#include "SpcControlDevice.h"
#include <iomanip>
#include <iostream>
#ifdef _MSC_VER
#include <Windows.h>
// デバイスドライバのinf内で定義したGUID
// (WinUSB.sys使用デバイスに対する識別子）
// {63275336-530B-4069-92B6-5F8AE3465462}
//DEFINE_GUID(GUID_DEVINTERFACE_WINUSBTESTTARGET,
//	0x63275336, 0x530b, 0x4069, 0x92, 0xb6, 0x5f, 0x8a, 0xe3, 0x46, 0x54, 0x62);
#else
#include <unistd.h>
#endif
#include <string.h>


//DSP------------------------------
unsigned char SpcControlDevice::dspregAccCode[] =
{
	0x8F ,0x00 ,0xF1 //       	mov SPC_CONTROL,#$00
	,0x8F ,0x6C ,0xF2 //       	mov SPC_REGADDR,#DSP_FLG
	,0x8F ,0x00 ,0xF3 //       	mov SPC_REGDATA,#$00
	,0x8D ,0x00       //     	mov y,#0
	,0xE8 ,0x00       //     	mov a,#0
	,0x8F ,0x00 ,0x04 //       	mov $04,#$00
	,0x8F ,0x06 ,0x05 //       	mov $05,#$06
	// initloop:
	,0xD7 ,0x04       //     	mov [$04]+y,a	; 7
	,0x3A ,0x04       //     	incw $04		; 6
	,0x78 ,0x7E ,0x05 //       	cmp $05,#$7e	; 5
	,0xD0 ,0xF7       //     	bne initloop	; 4
	,0xE4 ,0xF4       //     	mov a,SPC_PORT0
	// ack:
	,0x8F ,0xEE ,0xF7 //       	mov SPC_PORT3,#$ee
	// loop:
	,0x64 ,0xF4       //     	cmp a,SPC_PORT0		; 3
	,0xF0 ,0xFC       //     	beq loop			; 2
	,0xE4 ,0xF4       //     	mov a,SPC_PORT0		; 3
	,0x30 ,0x26       //     	bmi toram			; 2
	,0xF8 ,0xF6       //     	mov x,SPC_PORT2		; 3
	,0xD8 ,0xF2       //     	mov SPC_REGADDR,x	; 4
	,0xFA ,0xF5 ,0xF3 //       	mov SPC_REGDATA,SPC_PORT1
	,0xC4 ,0xF4       //     	mov SPC_PORT0,a		; 4
	// 	; wait 64 - 32 cycle
	,0xC8 ,0x4C       //     	cmp x,#DSP_KON	; 3
	,0xF0 ,0x12       //     	beq konWait	; 4
	,0xC8 ,0x5C       //     	cmp x,#DSP_KOF	; 3
	,0xD0 ,0xE7       //     	bne loop	; 4
	// koffWait:
	,0x8D ,0x0A       //     	mov y,#10	; 2
	// -
	,0xFE ,0xFE       //     	dbnz y,-	; 4/6
	,0x8F ,0x00 ,0xF3 //       	mov SPC_REGDATA,#0	;5
	//
	,0x8D ,0x05       //     	mov y,#5	; 2
	// -
	,0xFE ,0xFE       //     	dbnz y,-	; 4/6
	,0x00             //   	nop			; 2
	,0x2F ,0xD9       //     	bra loop	; 4
	// konWait:
	,0x8D ,0x05       //     	mov y,#5	; 2
	// -
	,0xFE ,0xFE       //     	dbnz y,-	; 4/6
	,0x00             //   	nop			; 2
	,0x2F ,0xD2       //     	bra loop	; 4
	// toram:
	,0x5D             //   	mov x,a
	//
	,0x80             //   	setc
	,0xA8 ,0x40       //     	sbc a,#P0FLG_BLKTRAS
	,0x30 ,0x0F       //     	bmi blockTrans
	,0x28 ,0x20       //     	and a,#P0FLG_P0RST
	,0xD0 ,0x39       //     	bne resetP0
	//
	,0x8D ,0x00       //     	mov y,#0
	,0xE4 ,0xF5       //     	mov a,SPC_PORT1
	,0xD7 ,0xF6       //     	mov [SPC_PORT2]+y,a
	,0x7D             //   	mov a,x
	,0xC4 ,0xF4       //     	mov SPC_PORT0,a
	,0x2F ,0xBD       //     	bra loop
	// blockTrans:
	,0x8F ,0x00 ,0xF7 //       	mov SPC_PORT3,#$0
	,0xFA ,0xF6 ,0x04 //       	mov $04,SPC_PORT2
	,0xFA ,0xF7 ,0x05 //       	mov $05,SPC_PORT3
	,0x7D             //   	mov a,x
	,0x8D ,0x00       //     	mov y,#0
	,0xC4 ,0xF4       //     	mov SPC_PORT0,a
	// loop2:
	,0x64 ,0xF4       //     	cmp a,SPC_PORT0
	,0xF0 ,0xFC       //     	beq loop2
	,0xE4 ,0xF4       //     	mov a,SPC_PORT0
	,0x30 ,0xA4       //     	bmi ack
	,0x5D             //   	mov x,a
	,0xE4 ,0xF5       //     	mov a,SPC_PORT1
	,0xD7 ,0x04       //     	mov [$04]+y,a
	,0x3A ,0x04       //     	incw $04
	,0xE4 ,0xF6       //     	mov a,SPC_PORT2
	,0xD7 ,0x04       //     	mov [$04]+y,a
	,0x3A ,0x04       //     	incw $04
	,0xE4 ,0xF7       //     	mov a,SPC_PORT3
	,0xD7 ,0x04       //     	mov [$04]+y,a
	,0x3A ,0x04       //     	incw $04
	,0x7D             //   	mov a,x
	,0xC4 ,0xF4       //     	mov SPC_PORT0,a
	,0x2F ,0xE0       //     	bra loop2
	// resetP0:
	,0x8F ,0xB0 ,0xF1 //       	mov SPC_CONTROL,#$b0
	,0x20             //   	clrp
	,0xD8 ,0xF4       //     	mov SPC_PORT0,x
	,0x5F ,0xC0 ,0xFF //       	jmp !$ffc0
};
//DSP------------------------------

void printBytes(const unsigned char* data, int bytes)
{
	/*
	for (int i=0; i<bytes; i++) {
		std::cout << std::hex << std::uppercase << std::setw(2) << std::setfill('0') << static_cast<int>(data[i]) << " ";
	}
	std::cout << std::endl;
	 */
}

SpcControlDevice::SpcControlDevice(c86ctl::GimicWinUSB* usb)
	: mWriteBytes(BLOCKWRITE_CMD_LEN), mPort0stateHw(0)
{
	mUsbDev = usb;
	//* mUsbDev = new ControlUSB();
}

SpcControlDevice::~SpcControlDevice()
{
	//* delete mUsbDev;
}

int SpcControlDevice::Init()
{
	mWriteBytes = BLOCKWRITE_CMD_LEN;    // 0xFD,0xB2,0xNN分

	/**
#ifdef _MSC_VER
	mUsbDev->BeginPortWait((LPGUID)&GUID_DEVINTERFACE_WINUSBTESTTARGET);
#else
	mUsbDev->BeginPortWait(GIMIC_USBVID, GIMIC_USBPID, 1, 2);
#endif

#ifdef USB_CONSOLE_TEST
	int retryRemain = 100;
	while (!mUsbDev->isPlugged() && retryRemain > 0) {
#ifdef _MSC_VER
		::Sleep(10);
#else
		usleep(10000);
#endif
		retryRemain--;
	}
	if (!mUsbDev->isPlugged()) {
		return 1;
	}
#endif
	*/
	int err = 0;

	// ハードウェアリセット
	HwReset();
	// ソフトウェアリセット
	SwReset();

	// $BBAA 待ち
	err = WaitReady();
	if (err) {
		return -1;
	}

	// ノイズ回避のため音量を0に
	unsigned char dspWrite[2];
	err = 0xcc - 1;
	dspWrite[0] = DSP_MVOLL;
	dspWrite[1] = 0;
	err = UploadRAMDataIPL(dspWrite, 0x00f2, 2, err + 1);
	if (err < 0) {
		return -1;
	}
	dspWrite[0] = DSP_MVOLR;
	dspWrite[1] = 0;
	err = UploadRAMDataIPL(dspWrite, 0x00f2, 2, err + 1);
	if (err < 0) {
		return -1;
	}
	dspWrite[0] = DSP_EVOLL;
	dspWrite[1] = 0;
	err = UploadRAMDataIPL(dspWrite, 0x00f2, 2, err + 1);
	if (err < 0) {
		return -1;
	}
	dspWrite[0] = DSP_EVOLR;
	dspWrite[1] = 0;
	err = UploadRAMDataIPL(dspWrite, 0x00f2, 2, err + 1);
	if (err < 0) {
		return -1;
	}

	// EDL,ESAを初期化
	dspWrite[0] = DSP_FLG;
	dspWrite[1] = 0x20;
	err = UploadRAMDataIPL(dspWrite, 0x00f2, 2, err + 1);
	if (err < 0) {
		return -1;
	}
	dspWrite[0] = DSP_EDL;
	dspWrite[1] = 0;
	err = UploadRAMDataIPL(dspWrite, 0x00f2, 2, err + 1);
	if (err < 0) {
		return -1;
	}
	dspWrite[0] = DSP_ESA;
	dspWrite[1] = 0x06; // DIRの直後
	err = UploadRAMDataIPL(dspWrite, 0x00f2, 2, err + 1);
	if (err < 0) {
		return -1;
	}
	WaitMicroSeconds(240000); // EDL,ESAを変更したので240ms待ち

	// DSPアクセス用コードを転送
	err = UploadRAMDataIPL(dspregAccCode, dspAccCodeAddr, sizeof(dspregAccCode), err + 1);
	if (err < 0) {
		return -1;
	}

	// 転送済みコードへジャンプ
	err = JumpToCode(dspAccCodeAddr, err + 1);
	if (err < 0) {
		return -1;
	}

	while (PortRead(3) != p3waitValue) {
		WaitMicroSeconds(10000);
	}

	mPort0stateHw = 1;

	return 0;
}

int SpcControlDevice::Close()
{
	//* mUsbDev->removeDevice();
	return 0;
}

void SpcControlDevice::HwReset()
{
	mUsbDev->resetrPipe();
	mUsbDev->resetwPipe();

	unsigned char cmd[] = { 0xfd, 0x81, 0xff };
	int wb = sizeof(cmd);
	mUsbDev->bulkWrite(cmd, wb);

	printBytes(cmd, wb);
}

void SpcControlDevice::SwReset()
{
	unsigned char cmd[] = { 0xfd, 0x82, 0xff };
	int wb = sizeof(cmd);
	mUsbDev->bulkWrite(cmd, wb);

	printBytes(cmd, wb);
}

bool SpcControlDevice::CheckHasRequiredModule()
{
	// SPCモジュールとFWバージョンのチェック
	{
		unsigned char cmd[] = { 0xfd, 0x91, 0x00, 0xff };
		int wb = sizeof(cmd);
		mUsbDev->bulkWrite(cmd, wb);

		int rb = 64;
		mUsbDev->bulkRead(mReadBuf, rb, 500);

		if (::strncmp((char*)mReadBuf, "GMC-SPC", 7) != 0) {
			return false;
		}
	}

	{
		unsigned char cmd[] = { 0xfd, 0x92, 0xff };
		int wb = sizeof(cmd);
		mUsbDev->bulkWrite(cmd, wb);

		int rb = 64;
		mUsbDev->bulkRead(mReadBuf, rb, 500);

		int verNum = mReadBuf[0] * 100 + mReadBuf[4];

		if (verNum < 504) {
			return false;
		}
	}
	return true;
}

void SpcControlDevice::PortWrite(int addr, unsigned char data)
{
	unsigned char cmd[] = { 0x00, 0x00, 0xff };
	cmd[0] = addr;
	cmd[1] = data;
	int wb = sizeof(cmd);
	mUsbDev->bulkWrite(cmd, wb);

	printBytes(cmd, wb);
}

unsigned char SpcControlDevice::PortRead(int addr)
{
	unsigned char cmd[] = { 0xfd, 0xb0, 0x00, 0x00, 0xff };
	cmd[2] = addr;
	int wb = sizeof(cmd);
	mUsbDev->bulkWrite(cmd, wb);
	printBytes(cmd, wb);

	int rb = 64;
#if 0
	int retry = 500;
	while (mUsbDev->getReadableBytes() < rb) {
		usleep(1000);
		retry--;
		if (retry == 0) {
			break;
		}
	}
	if (retry > 0) {
		mUsbDev->read(mReadBuf, rb);
	}
#else
	mUsbDev->bulkRead(mReadBuf, rb, 500);
	//std::cout << ">";
	printBytes(mReadBuf, 1);
#endif
	return mReadBuf[0];
}

void SpcControlDevice::BlockPush(int addr, unsigned char data)
{
	// 残り2バイト未満なら書き込んでから追加する
	if (mWriteBytes >= (PACKET_SIZE - 2)) {
		SendBuffer();
	}
	mWriteBuf[mWriteBytes] = addr & 0x03;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data;
	mWriteBytes++;
}

void SpcControlDevice::BlockPush(int addr, unsigned char data, unsigned char data2)
{
	// 残り3バイト未満なら書き込んでから追加する
	if (mWriteBytes >= (PACKET_SIZE - 3)) {
		SendBuffer();
	}
	mWriteBuf[mWriteBytes] = (addr & 0x03) | 0x10;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data2;
	mWriteBytes++;
}

void SpcControlDevice::BlockPush(int addr, unsigned char data, unsigned char data2, unsigned char data3)
{
	// 残り4バイト未満なら書き込んでから追加する
	if (mWriteBytes >= (PACKET_SIZE - 4)) {
		SendBuffer();
	}
	mWriteBuf[mWriteBytes] = (addr & 0x03) | 0x20;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data2;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data3;
	mWriteBytes++;
}

void SpcControlDevice::BlockPush(int addr, unsigned char data, unsigned char data2, unsigned char data3, unsigned char data4)
{
	// 残り5バイト未満なら書き込んでから追加する
	if (mWriteBytes >= (PACKET_SIZE - 5)) {
		SendBuffer();
	}
	mWriteBuf[mWriteBytes] = (addr & 0x03) | 0x30;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data2;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data3;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = data4;
	mWriteBytes++;
}

void SpcControlDevice::ReadAndWait(int addr, unsigned char waitValue)
{
	if (mWriteBytes >= (PACKET_SIZE - 2)) {
		SendBuffer();
	}
	mWriteBuf[mWriteBytes] = addr | 0x80;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = waitValue;
	mWriteBytes++;
}

void SpcControlDevice::PushAndWait(int addr, unsigned char waitValue)
{
	if (mWriteBytes >= (PACKET_SIZE - 2)) {
		SendBuffer();
	}
	mWriteBuf[mWriteBytes] = addr | 0xc0;
	mWriteBytes++;
	mWriteBuf[mWriteBytes] = waitValue;
	mWriteBytes++;
}

void SpcControlDevice::SendBuffer()
{
	/*
	if (mWriteBytes > 62) {
		// TODO: Assert
		return;
	}
	*/
	if (mWriteBytes > BLOCKWRITE_CMD_LEN) {
		mWriteBuf[0] = 0xfd;
		mWriteBuf[1] = 0xb2;
		mWriteBuf[2] = 0x00;
		mWriteBuf[3] = 0x00;
		for (int i = 0; i < 1; i++) {
			if (mWriteBytes < 64) {
				mWriteBuf[mWriteBytes] = 0xff;
				mWriteBytes++;
			}
		}
		/*
		puts("\n--Dump--");
		for (int i=3; i<64; i+=2) {
			int blockaddr = mWriteBuf[i];
			int blockdata = mWriteBuf[i+1];
			printf("Block : 0x%02X / 0x%02X\n", blockaddr, blockdata);
			if (blockaddr == 0xFF && blockdata == 0xFF)break;
		}
		 */
		 //printf("mWriteBytes:%d\n", mWriteBytes);
		 // GIMIC側のパケットは64バイト固定なので満たない場合0xffを末尾に追加
		if (mWriteBytes < 64) {
			mWriteBuf[mWriteBytes++] = 0xff;
		}
		//if (mWriteBuf[6] == 0x7d && mWriteBuf[7] == 0xc0) {
		printBytes(mWriteBuf, mWriteBytes);
		//}
		if (mUsbDev->isValid()) {
			mUsbDev->bulkWrite(mWriteBuf, mWriteBytes);
		}
		mWriteBytes = BLOCKWRITE_CMD_LEN;
	}
}

int SpcControlDevice::CatchTransferError()
{
	if (mUsbDev->getReadableBytes() >= 4) {
		unsigned char msg[4];
		mUsbDev->read(msg, 4);
		int err = *(reinterpret_cast<unsigned int*>(msg));
		if (err == 0xfefefefe) {
			return err;
		}
	}
	return 0;
}

void SpcControlDevice::setDeviceAddedFunc(void (*func) (void* ownerClass), void* ownerClass)
{
	//mUsbDev->setDeviceAddedFunc(func, ownerClass);
}

void SpcControlDevice::setDeviceRemovedFunc(void (*func) (void* ownerClass), void* ownerClass)
{
	//mUsbDev->setDeviceRemovedFunc(func, ownerClass);
}

//-----------------------------------------------------------------------------

int SpcControlDevice::WaitReady()
{
	if (mUsbDev->isValid()) {
		ReadAndWait(0, 0xaa);
		ReadAndWait(1, 0xbb);
		SendBuffer();
	}

	int err = CatchTransferError();
	if (err) {
		return err;
	}
	return 0;
}

int SpcControlDevice::UploadRAMDataIPL(const unsigned char* ram, int addr, int size, unsigned char initialP0state)
{
	BlockPush(1, 0x01, addr & 0xff, (addr >> 8) & 0xff); // 非0なのでP2,P3は書き込み開始アドレス
	unsigned char port0State = initialP0state;
	PushAndWait(0, port0State & 0xff);
	SendBuffer();
	port0State = 0;
	for (int i = 0; i < size; i++) {
		BlockPush(1, ram[i]);
		PushAndWait(0, port0State);
		port0State++;
		if ((i % 256) == 255) {
			//std::cout << ".";
		}
		if (i == (size - 1)) {
			SendBuffer();
		}
		int err = CatchTransferError();
		if (err) {
			return err;
		}
	}
	return port0State;
}

int SpcControlDevice::JumpToCode(int addr, unsigned char initialP0state)
{
	BlockPush(2, addr & 0xff, (addr >> 8) & 0xff);
	BlockPush(1, 0);    // 0なのでP2,P3はジャンプ先アドレス
	unsigned char port0state = initialP0state & 0xff;
	BlockPush(0, port0state);
	SendBuffer();

	int err = CatchTransferError();
	if (err) {
		return err;
	}
	return 0;
}

void SpcControlDevice::doWriteDspHw(int addr, unsigned char data)
{
	//if (mIsHwAvailable) {
		//pthread_mutex_lock(&mHwMtx);
		/*
		 if (addr == DSP_EDL) {
		 std::cout << "addr:0x" << std::hex << std::setw(2) << std::setfill('0') << addr;
		 std::cout << " data:0x" << std::hex << std::setw(2) << std::setfill('0') << static_cast<int>(data) << std::endl;
		 }
		 */
	
	int rewrite = 2;
	if (((addr & 0x0f) < 0x0a) ||
		addr == DSP_KON ||
		addr == DSP_KOF ||
		addr == DSP_FLG) {
		rewrite = 1;
	}
	for (int i = 0; i < rewrite; i++) {
		BlockPush(1, data, addr & 0xff);
		PushAndWait(0, mPort0stateHw);
		mPort0stateHw = mPort0stateHw ^ 0x01;
	}
	SendBuffer();

	//pthread_mutex_unlock(&mHwMtx);
	//}
}

void SpcControlDevice::doWriteRamHw(int addr, unsigned char data)
{
	//if (mIsHwAvailable) {
		//pthread_mutex_lock(&mHwMtx);
	
	BlockPush(1, data, addr & 0xff, (addr >> 8) & 0xff);
	PushAndWait(0, mPort0stateHw | 0x80);
	SendBuffer();
	mPort0stateHw = mPort0stateHw ^ 0x01;
	
	//pthread_mutex_unlock(&mHwMtx);
	//}
}


void SpcControlDevice::WriteRam(int addr, const unsigned char* data, int size)
{
	if (size <= 0) {
		return;
	}

	//MutexLock(mHwMtx);
	BlockPush(2, addr & 0xff, (addr >> 8) & 0xff);
	PushAndWait(0, mPort0stateHw | 0xc0);
	SendBuffer();
	mPort0stateHw = mPort0stateHw ^ 0x01;
	int num = size / 3;
	int rest = size - num * 3;
	int ptr = 0;
	for (int i = 0; i < num; i++) {
		BlockPush(1, data[ptr], data[ptr + 1], data[ptr + 2]);
		ptr += 3;
		PushAndWait(0, mPort0stateHw);
		mPort0stateHw = mPort0stateHw ^ 0x01;
	}
	BlockPush(0, mPort0stateHw | 0x80);
	ReadAndWait(3, p3waitValue);
	SendBuffer();
	mPort0stateHw = mPort0stateHw ^ 0x01;
	/*
	while (mSpcDev.PortRead(3) != p3waitValue) {
		WaitMicroSeconds(5000);
	}
	 */
	addr += num * 3;
	for (int i = 0; i < rest; i++) {
		BlockPush(1, data[ptr], (addr + i) & 0xff, ((addr + i) >> 8) & 0xff);
		ptr++;
		PushAndWait(0, mPort0stateHw | 0x80);
		SendBuffer();
		mPort0stateHw = mPort0stateHw ^ 0x01;
	}

	// 直後のDSP書き込みが失敗する場合があるので無意味なDSP書き込みを１回行う
	doWriteDspHw(0x1d, 0);
	//MutexUnlock(mHwMtx);
}
