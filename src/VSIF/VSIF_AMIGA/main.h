#include "support/gcc8_c_support.h"
#include <proto/exec.h>
#include <proto/dos.h>
#include <dos/dostags.h>
#include <exec/errors.h>

#include <exec/execbase.h>
#include <exec/io.h>
#include <exec/tasks.h>
#include <dos/dosextens.h>
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
#define NO_LOOP

#define DMAF_AUD_AGAIN(yy) DMAF_AUD ## yy
#define DMAF_AUD(y) DMAF_AUD_AGAIN(y)

#define INTENA_AUD_AGAIN(yy) INTF_AUD ## yy
#define INTENA_AUD(y) INTENA_AUD_AGAIN(y)

#define XON 0x11
#define XOFF 0x13

#define SERD_ERRMASK   0xF0
#define SERD_OVERRUN   0x10
#define SERD_PARITY    0x20
#define SERD_FRAMING   0x40
#define SERD_BREAK     0x80

extern UWORD SystemInts;
extern UWORD SystemDMA;
extern UWORD SystemADKCON;
extern APTR VBR;
extern APTR SystemIrq;

extern UBYTE recvBufferId;
extern UBYTE recvBuffer[2][SERIAL_BUFFER_SIZE];

extern BOOL closeDev;
extern BOOL deleteIo;
extern struct MsgPort *serialPort;
extern struct IOExtSer *serialIO;
extern volatile struct Custom *custom ;
extern struct Window *mainWin;

extern BYTE* SineData;
extern BYTE* StopData;

void TakeSystem();
void FreeSystem();

BOOL fileExists(STRPTR filename);
void loadPcm(CONST_STRPTR filename);
BYTE writeArray(UBYTE *buffptr,ULONG length);

void requestSerial(ULONG length);
UBYTE * waitSerialData();
UBYTE *readArray(UBYTE *buffptr,ULONG length);
void showMessage(char *message);
void printText(char* message, int ofst_x, int ofst_y);

void aud_memcpy(volatile struct AudChannel *dest, volatile struct AudChannel *src);
void wait_next_scanline();

struct PcmData
{
	BYTE *dataPtr;
	UWORD length;
	UWORD loop;
};

struct PlayData
{
	UWORD playCount; 
	struct AudChannel aud;
	struct PcmData *pcm;
};

// メッセージ構造体
struct AudioMessage
{
    struct Message msg;
    UBYTE data[5];
};

extern struct PcmData pcmDataTable[256];
extern volatile struct PlayData curPlayData[4];

void reqPlayPcm(UBYTE ch, UBYTE id,  UWORD volume, UWORD period);
void reqStopPcm(UBYTE ch);
void audioTask(void);

