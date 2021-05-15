#include "genesis.h"
#include "resources.h"
#include "vgmPlayZ80.h"

// vu8 *port1 = (vu8 *)0xA10003;  // Player1 port
// vu8 *port2 = (vu8 *)0xA10005;  // Player2 port
vu8 *port3 = (vu8 *)0xA10007;  // EXT port
vu8* regs[] = {(vu8 *)0xA04000, (vu8 *)0xA04001, (vu8 *)0xA04002, (vu8 *)0xA04003, (vu8 *)0xC00011};
volatile int counter;

extern void VGMPlay();

int main() {
  VDP_setScreenWidth320();
  // VDP_setHInterrupt(0);
  // VDP_setHilightShadow(0);

  VDP_setPalette(PAL0, font_pal_lib.data);
  VDP_setPaletteColor((PAL1 * 16) + 15, 0x0888);
  VDP_setTextPalette(PAL0);

  //Z80_setBank(0x142);
  Z80_upload(0, VGMPlayZ80, sizeof(VGMPlayZ80), TRUE);

  VDP_drawText("MAMI VGM SOUND PLAYER BY ITOKEN", 0, 0);
  VDP_drawText("READY TO PLAY.", 0, 1);

  VDP_drawText("-CONNECT EXT PORT PIN1 TO TX.", 0, 3);
  VDP_drawText("-CONNECT EXT PORT PIN8 TO GND.", 0, 4);

  VDP_drawText("  1 ------> UART TX ", 0, 6);
  VDP_drawText(" ___________        ", 0, 7);
  VDP_drawText(" \\* o o o o/        ", 0, 8);
  VDP_drawText("  \\o o * o/         ", 0, 9);
  VDP_drawText("   -------          ", 0, 10);
  VDP_drawText("       8 -> UART GND", 0, 11);

  SYS_setInterruptMaskLevel(7); /* disable ints */
  //        SND_set68KBUSProtection_XGM(FALSE);

  YM2612_reset();
  PSG_init();

//  u16 busTaken = Z80_getAndRequestBus(TRUE);
  VGMPlay();
  //if (!busTaken)
    //Z80_releaseBus();
}

