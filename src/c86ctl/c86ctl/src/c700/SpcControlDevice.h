//
//  SpcControlDevice.h
//  gimicUsbSpcPlay
//
//  Created by osoumen on 2014/06/14.
//  Copyright (c) 2014年 osoumen. All rights reserved.
//

#ifndef __gimicUsbSpcPlay__SpcControlDevice__
#define __gimicUsbSpcPlay__SpcControlDevice__

//#include "ControlUSB.h"

//DSP------------------------------------- 
#define DSP_VOL		(0x00)
#define DSP_P		(0x02)
#define DSP_SRCN	(0x04)
#define DSP_ADSR	(0x05)
#define DSP_GAIN	(0x07)
#define DSP_ENVX	(0x08)
#define DSP_OUTX	(0x09)
#define DSP_MVOLL	(0x0c)
#define DSP_MVOLR	(0x1c)
#define DSP_EVOLL	(0x2c)
#define DSP_EVOLR	(0x3c)
#define DSP_KON		(0x4c)
#define DSP_KOF		(0x5c)
#define DSP_FLG		(0x6c)
#define DSP_ENDX	(0x7c)
#define DSP_EFB		(0x0d)
#define DSP_PMON	(0x2d)
#define DSP_NON		(0x3d)
#define DSP_EON		(0x4d)
#define DSP_DIR		(0x5d)
#define DSP_ESA		(0x6d)
#define DSP_EDL		(0x7d)
#define DSP_FIR		(0x0F)
//DSP------------------------------------- 

class c86ctl::GimicWinUSB;

class SpcControlDevice {
public:
    SpcControlDevice(c86ctl::GimicWinUSB* usb);
    ~SpcControlDevice();
    
    int Init();
    int Close();
    void HwReset();
    void SwReset();
    bool CheckHasRequiredModule();
    void PortWrite(int addr, unsigned char data);
    unsigned char PortRead(int addr);
    void BlockPush(int addr, unsigned char data);
    void BlockPush(int addr, unsigned char data, unsigned char data2);
    void BlockPush(int addr, unsigned char data, unsigned char data2, unsigned char data3);
    void BlockPush(int addr, unsigned char data, unsigned char data2, unsigned char data3, unsigned char data4);
    void ReadAndWait(int addr, unsigned char waitValue);
    void PushAndWait(int addr, unsigned char waitValue);
    void SendBuffer();
    
    int UploadRAMDataIPL(const unsigned char *ram, int addr, int size, unsigned char initialP0state);
    int WaitReady();
    int JumpToCode(int addr, unsigned char initialP0state);
    
    int CatchTransferError();
    
    void setDeviceAddedFunc( void (*func) (void* ownerClass), void* ownerClass );
	void setDeviceRemovedFunc( void (*func) (void* ownerClass) , void* ownerClass );

private:
    static const int GIMIC_USBVID = 0x16c0;
    static const int GIMIC_USBPID = 0x05e5;
    static const int GIMIC_USBWPIPE = 0x02;
    static const int GIMIC_USBRPIPE = 0x85;
    static const int BLOCKWRITE_CMD_LEN = 4;
    static const int MAX_ASYNC_READ = 64;
    static const int PACKET_SIZE = 64;

    //ControlUSB      *mUsbDev;
    c86ctl::GimicWinUSB     *mUsbDev;
    unsigned char   mWriteBuf[64] = {};
    unsigned char   mReadBuf[64] = {};
    int             mWriteBytes;
    
    //int mNumReads;  // Read待ちのパケット数

    //DSP------------------------------------- 
    typedef DWORD MSTime;
    inline void WaitMicroSeconds(MSTime usec) {
        ::Sleep(usec / 1000);	// 現状1ms未満の箇所は無い
    }
    static unsigned char dspregAccCode[];
    static const int dspAccCodeAddr = 0x0010;
    static const int dspOutBufSize = 4096;
    static const int p3waitValue = 0xee;

    int mPort0stateHw;

public:
    void WriteRam(int addr, const unsigned char* data, int size);
    void doWriteDspHw(int addr, unsigned char data);
    void doWriteRamHw(int addr, unsigned char data);
    //DSP------------------------------------- 
};

#endif /* defined(__gimicUsbSpcPlay__SpcControlDevice__) */

