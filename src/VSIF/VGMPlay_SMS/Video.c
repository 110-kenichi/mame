#include "Video.h"

#include "PSGlib.h"
#include "SMSLib/SMSLib.h"
#include "Sound.h"
#include "Types.h"

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

void FadeInPalette(int count, unsigned char *pal, char pal_size, char sprite) {
  if (count >= 3) {
    int col = count - 3;
    for (int i = 0; i < pal_size; i++) {
      int ob = pal[i] >> 4;
      int og = (pal[i] >> 2) & 0x3;
      int or = (pal[i] & 0x3);
      if (sprite)
        SetSpritePaletteColor(
            i, RGB(or > col ? col : or, og > col ? col : og, ob));
      else
        SetBGPaletteColor(i, RGB(or > col ? col : or, og > col ? col : og, ob));
    }
  } else {
    int col = count;
    for (int i = 0; i < pal_size; i++) {
      int ob = pal[i] >> 4;
      int og = (pal[i] >> 2) & 0x3;
      int or = (pal[i] & 0x3);
      if (sprite)
        SetSpritePaletteColor(i, RGB(0, 0, ob > col ? col : ob));
      else
        SetBGPaletteColor(i, RGB(0, 0, ob > col ? col : ob));
    }
  }
}

void FadeOutPalette(int count, unsigned char *pal, char pal_size,
                    char forSprite) {
  if (count >= 3) {
    int col = count - 3;
    for (int i = 0; i < pal_size; i++) {
      int ob = pal[i] >> 4;
      ob -= col;
      if (forSprite)
        SetSpritePaletteColor(i, RGB(0, 0, ob < 0 ? 0 : ob));
      else
        SetBGPaletteColor(i, RGB(0, 0, ob < 0 ? 0 : ob));
    }
  } else {
    int col = count;
    for (int i = 0; i < pal_size; i++) {
      int ob = pal[i] >> 4;
      int og = (pal[i] >> 2) & 0x3;
      og -= col;
      int or = (pal[i] & 0x3);
      or -= col;
      if (forSprite)
        SetSpritePaletteColor(i, RGB(or < 0 ? 0 : or, og < 0 ? 0 : og, ob));
      else
        SetBGPaletteColor(i, RGB(or < 0 ? 0 : or, og < 0 ? 0 : og, ob));
    }
  }
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
  if (!playingPcm) {
    // PSGFrame();
    // PSGSFXFrame();
  }
  if (!DisableVDPProcessing) {
    if (theVBlankInterruptHandler != 0) theVBlankInterruptHandler();
  }
}

void UpdateSpritePosition(signed char sprite, short x, short y) {
  if (x < 0) {
    SMS_updateSpritePosition(sprite, 0, 0xE0);
    return;
  } else if (x > 255) {
    SMS_updateSpritePosition(sprite, 0, 0xE0);
    return;
  }

  if (y < -16) {
    SMS_updateSpritePosition(sprite, 0, 0xE0);
    return;
  } else if (y > 192) {
    SMS_updateSpritePosition(sprite, 0, 0xE0);
    return;
  }

  SMS_updateSpritePosition(sprite, x, y);
}

signed char AddSprite(short x, short y, signed char tile) {
  if (x < 0) {
    return SMS_addSprite(0, 0xE0, tile);
  } else if (x > 255) {
    return SMS_addSprite(0, 0xE0, tile);
  }

  if (y < -16) {
    return SMS_addSprite(0, 0xE0, tile);
  } else if (y > 192) {
    return SMS_addSprite(0, 0xE0, tile);
  }

  return SMS_addSprite(x, y, tile);
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

void PrintWait(unsigned short value) {
  unsigned int adr = SMS_PNTAddress;  // write to 3800H
  SetTileatAddr(adr += 2, hexNumToTileNo[value & 0xf]);
  value = value >> 4;
  SetTileatAddr(adr += 2, hexNumToTileNo[value & 0xf]);
  value = value >> 4;
  SetTileatAddr(adr += 2, hexNumToTileNo[value & 0xf]);
  value = value >> 4;
  SetTileatAddr(adr += 2, hexNumToTileNo[value & 0xf]);

  // adr = 0x7000 | 0x3000 | vramadr; //write to 3800H
  // SMS_setAddr(adr += 2);
  // SMS_setTile(hexNumToTileNo[value & 0xf]);
  // value = value >> 4;
  // SMS_setAddr(adr += 2);
  // SMS_setTile(hexNumToTileNo[value & 0xf]);
  // value = value >> 4;
  // SMS_setAddr(adr += 2);
  // SMS_setTile(hexNumToTileNo[value & 0xf]);

  // vramadr = (vramadr + 1) & 0x7ff;
}
