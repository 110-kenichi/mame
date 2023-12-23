#include "Video.h"

#include "SMSLib/SMSLib.h"

volatile unsigned char IntCount = 0;
volatile unsigned short waitCount = 0;
volatile unsigned short delayCount = 0;

short ScreenX = 0;
short ScreenY = 0;
short lscx = 0;
short lscy = 0;

unsigned short vramadr = 0x3800;

unsigned char defBgPalette[] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
unsigned char defSpPalette[] = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

bool bgPaletteChanged = false;
bool spPaletteChanged = false;
unsigned char *currentBgPalette;
unsigned char *currentSpPalette;

volatile bool DisableVDPProcessing = false;

inline void SMS_write_to_VDPRegister(unsigned char VDPReg,
                                     unsigned char value) {
  // INTERNAL FUNCTION
  DISABLE_INTERRUPTS;
  VDPControlPort = value;
  VDPControlPort = VDPReg | 0x80;
  ENABLE_INTERRUPTS;
}

inline void SMS_byte_to_VDP_data(unsigned char data) {
  // INTERNAL FUNCTION
  VDPDataPort = data;
}

void SetTileatAddr(unsigned int addr, unsigned int tile) {
  __asm__("di");
  __asm__("ld iy, #2");
  __asm__("add iy, sp");
  __asm__("ld c, #0xBF");
  __asm__("ld a, 0(iy)");
  __asm__("out(c), a");
  __asm__("ld a, 1(iy)");
  __asm__("out(c), a");

  __asm__("dec c");
  __asm__("ld a, 2(iy)");
  __asm__("out(c), a");
  __asm__("ld a, 3(iy)");
  __asm__("out(c), a");
  __asm__("ei");
}

void SetBGPaletteColor(unsigned char entry, unsigned char color) {
  currentBgPalette[entry] = color;
  bgPaletteChanged = true;
}

void SetSpritePaletteColor(unsigned char entry, unsigned char color) {
  currentSpPalette[entry] = color;
  spPaletteChanged = true;
}

void ClearBG() {
  for (int y = 0; y < 28; y++)
    for (int x = 0; x < 32; x++) SetTileatXY(x, y, 0);
}

void (*theVBlankInterruptHandler)(void) = 0;

void SetVBlankInterruptHandler(void (*theHandlerFunction)(void))
    __z88dk_fastcall {
  theVBlankInterruptHandler = theHandlerFunction;
}

void VinterruptHandler()
{
  if (!DisableVDPProcessing) {
    if (theVBlankInterruptHandler != 0) theVBlankInterruptHandler();
  }
}

void FinishVBlank() {
  if (bgPaletteChanged) {
    SMS_loadBGPalette(currentBgPalette);
    bgPaletteChanged = false;
  }
  if (spPaletteChanged) {
    SMS_loadSpritePalette(currentSpPalette);
    spPaletteChanged = false;
  }

  if (ScreenX != lscx) {
    SMS_setBGScrollX(255 - (ScreenX & 0xff));
    lscx = ScreenX;
  }
  if (ScreenY != lscy) {
    SMS_setBGScrollY(ScreenY % 224);
    lscy = ScreenY;
  }
  SMS_copySpritestoSAT();
}
