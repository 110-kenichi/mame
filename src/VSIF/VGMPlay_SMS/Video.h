#include <stdlib.h>
#include <math.h>
#include <stdbool.h>

#include "SMSlib/SMSlib.h"
#include "Font.h"

#define DISABLE_INTERRUPTS    __asm di __endasm
#define ENABLE_INTERRUPTS     __asm ei __endasm

extern volatile bool VDPBlank; /* used by INTerrupt */
extern volatile unsigned short waitCount;
extern volatile unsigned short delayCount;

/* declare various I/O ports in SDCC z80 syntax */
/* define VDPControlPort */
__sfr __at 0xBF VDPControlPort;
/* define VDPStatusPort */
__sfr __at 0xBF VDPStatusPort;
/* define VDPDataPort */
__sfr __at 0xBE VDPDataPort;
/* define VDPVcounter */
__sfr __at 0x7E VDPVCounterPort;
/* define VDPHcounter */
__sfr __at 0x7F VDPHCounterPort;

typedef struct {
  unsigned int Address;
  unsigned int Data;
} VdpData;

extern VdpData VdpDataQueue[];
extern unsigned short VdpDataQueueCount;

extern volatile unsigned char IntCount;

extern short ScreenX;
extern short ScreenY;
extern short lscx;
extern short lscy;

extern unsigned short vramadr;

extern unsigned char defBgPalette[];
extern unsigned char defSpPalette[];

extern bool bgPaletteChanged;
extern bool spPaletteChanged;
extern unsigned char *currentBgPalette;
extern unsigned char *currentSpPalette;

extern volatile bool DisableVDPProcessing;

extern inline void SMS_write_to_VDPRegister(unsigned char VDPReg, unsigned char value);
extern inline void SMS_byte_to_VDP_data(unsigned char data);

extern void (*theVBlankInterruptHandler)(void);
extern void SetVBlankInterruptHandler (void (*theHandlerFunction)(void)) __z88dk_fastcall;
extern void VinterruptHandler();

extern void ClearBG();

#define SetTileatXY(x, y, tile) SetTileatAddr(XYtoADDR(x, y), tile)

extern void SetTileatAddr(unsigned int addr, unsigned int tile);

extern signed char AddSprite(short x, short y, signed char tile);
extern void UpdateSpritePosition(signed char sprite, short x, short y);

extern void FinishVBlank();
