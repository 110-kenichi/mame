#include "main.h"

struct MsgPort *serialPort = NULL;
struct IOExtSer *serialIO = NULL;
UBYTE recvBufferId = 0;
UBYTE recvBuffer[2][SERIAL_BUFFER_SIZE] = {0};


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
	while(1)
	{
		int waitMask = SIGBREAKF_CTRL_C | SIGBREAKF_CTRL_F |
         (1L << serialPort->mp_SigBit) |
          (1L << mainWin->UserPort->mp_SigBit);
		waitMask = Wait(waitMask);
		if (waitMask & (1L << serialPort->mp_SigBit))
		{
			// 受信待機
			//This function determines the current state of an I/O request and returns FALSE if the I/O has not yet completed. 
			if(CheckIO((struct IORequest *)serialIO))
            {
                if(!WaitIO((struct IORequest *)serialIO))
                {
                    LONG error = serialIO->IOSer.io_Error;
                    if(error)
                    {
                        if (error & SERD_OVERRUN)
                            showMessage("Overrun error");
                        if (error & SERD_FRAMING)
                            showMessage("Framing error");
                        if (error & SERD_PARITY)
                            showMessage("Parity error");
                        if (error & SERD_BREAK)
                            showMessage("Break signal received");
						ULONG arg[] = {error};
						VWritef("Wait error (%N)\n", arg);
                    }
                    return serialIO->IOSer.io_Data;
                }else{
                    LONG error = serialIO->IOSer.io_Error;
                    if(error)
                    {
                        if (error & SERD_OVERRUN)
                            showMessage("Overrun error");
                        if (error & SERD_FRAMING)
                            showMessage("Framing error");
                        if (error & SERD_PARITY)
                            showMessage("Parity error");
                        if (error & SERD_BREAK)
                            showMessage("Break signal received");
                    }
					ULONG arg[] = {error};
					VWritef("Wait I/O was aborted (%N)\n", arg);
                    return NULL;
                }
            }else{
				LONG error = serialIO->IOSer.io_Error;
				ULONG arg[] = {error};
				VWritef("Check I/O was aborted (%N)\n", arg);

                // 念のため強制終了
                AbortIO((struct IORequest *)serialIO);
                WaitIO((struct IORequest *)serialIO);
                // バッファ・状態をクリア
                serialIO->IOSer.io_Command = CMD_CLEAR;
                DoIO((struct IORequest *)serialIO);

                // 改めて受信リクエスト発行
                serialIO->IOSer.io_Command = CMD_READ;
                SendIO((struct IORequest *)serialIO);
            }
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
	return (UBYTE *)0xFFFFFFFF;
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
