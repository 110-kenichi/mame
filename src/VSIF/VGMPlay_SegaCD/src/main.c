#include "genesis.h"
#include "resources.h"
#include "vgmPlayZ80.h"

vu8 *port1 = (vu8 *)0xA10003;  // Player1 port
vu8 *port2 = (vu8 *)0xA10005;  // Player2 port
vu8 *port2ctrl = (vu8 *)0xA1000B;  // Player2 ctrl port
vu8 *port3 = (vu8 *)0xA10007;  // EXT port
vu8 *regs[] = {(vu8 *)0xA04000, (vu8 *)0xA04001, (vu8 *)0xA04002,
               (vu8 *)0xA04003, (vu8 *)0xC00011};
volatile int counter;

extern int VGMPlay_InitMCD();
extern void VGMPlay_FTDI2XX();

int main() {
  VDP_setScreenWidth320();
  VDP_setHInterrupt(0);
  VDP_setHilightShadow(0);

  VDP_setPalette(PAL0, font_pal_lib.data);
  VDP_setPaletteColor((PAL1 * 16) + 15, 0x0888);
  VDP_setTextPalette(PAL0);

  SYS_setInterruptMaskLevel(7); /* disable ints */

  YM2612_reset();
  PSG_init();
  Z80_upload(0, VGMPlayZ80, sizeof(VGMPlayZ80), TRUE);

  VDP_drawText("MAMI VGM SOUND DRIVER BY ITOKEN", 0, 0);

  SYS_disableInts();

  VDP_drawText("INIT MEGA CD/SEGA CD...", 0, 4);
  if(VGMPlay_InitMCD() != 0)
  {
      VDP_drawText("*MEGA CD/SEGA CD BIOS NOT FOUND*", 0, 4);
      while(1);
  }
  VDP_drawText("                       ", 0, 4);

/*
  if(((*port2) & 0x40) == 0)
  {
    VDP_drawText("*WAITING FOR VSIF FTDI CONNECTION*", 0, 4);
    while(((*port2) & 0x40) == 0);
    VDP_clearText(0, 4, 40);
  }
*/

  VDP_drawText("FTDI2XX MODE READY TO PLAY.", 0, 2);
  VDP_drawText("NOTE: PLEASE RESET AFTER RECONNECTED", 0, 3);

  VDP_drawText("-CONNECT P2 PORT PIN1-4,6-9 TO FTDI2XX.", 0, 5);

  VDP_drawText("___________ ", 0, 7);
  VDP_drawText("\\1 2 3 4 5/->FTDI2XX TX,RX,RTS,CTS,VCC", 0, 8);
  VDP_drawText(" \\6 7 8 9/ ->FTDI2XX DTR,DCD,GND,DSR", 0, 9);
  VDP_drawText("  -------   ", 0, 10);

  VGMPlay_FTDI2XX();

  // if (!busTaken)
  // Z80_releaseBus();
}
