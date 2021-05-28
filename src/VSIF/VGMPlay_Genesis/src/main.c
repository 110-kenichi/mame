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

extern void VGMPlay();
extern void VGMPlay_FTDI2XX();

int main() {
  VDP_setScreenWidth320();
  VDP_setHInterrupt(0);
  VDP_setHilightShadow(0);

  VDP_setPalette(PAL0, font_pal_lib.data);
  VDP_setPaletteColor((PAL1 * 16) + 15, 0x0888);
  VDP_setTextPalette(PAL0);

  SYS_setInterruptMaskLevel(7); /* disable ints */
  //        SND_set68KBUSProtection_XGM(FALSE);

  YM2612_reset();
  PSG_init();
  Z80_upload(0, VGMPlayZ80, sizeof(VGMPlayZ80), TRUE);

  VDP_drawText("MAMI VGM SOUND DRIVER BY ITOKEN", 0, 0);

  VDP_drawText("  PLEASE SELECT DRIVER TYPE.", 0, 3);

  VDP_drawText("  (UP)", 0, 6);
  VDP_drawText("   -GENERAL UART(163840BPS) -", 0, 7);
  VDP_drawText("  (DOWN)", 0, 9);
  VDP_drawText("   -FTDI2XX DONGLE(BIT BANG)-", 0, 10);

  SYS_enableInts();
  bool UART_MODE = FALSE;
  bool FTDI_MODE = FALSE;
  while (1) {
    vu8 value = *port1;
    if ((value & 1) == 0) {
      UART_MODE = TRUE;
      break;
    } else if ((value & 2) == 0) {
      FTDI_MODE = TRUE;
      break;
    }
    VDP_waitVSync();
  }
  VDP_clearText(0, 3, 40);
  VDP_clearText(0, 6, 40);
  VDP_clearText(0, 7, 40);
  VDP_clearText(0, 9, 40);
  VDP_clearText(0, 10, 40);

  //  u16 busTaken = Z80_getAndRequestBus(TRUE);
  SYS_disableInts();
  //*port2ctrl = 0x00;  //ALL INPUT and DISABLE INT
  if (UART_MODE == TRUE) {
    VDP_drawText("GENERAL UART MODE READY TO PLAY.", 0, 2);

    VDP_drawText("-CONNECT P2 PORT PIN1 TO TX.", 0, 4);
    VDP_drawText("-CONNECT P2 PORT PIN8 TO GND.", 0, 5);

    VDP_drawText("  1 ------> UART TX ", 0, 7);
    VDP_drawText(" ___________        ", 0, 8);
    VDP_drawText(" \\* o o o o/        ", 0, 9);
    VDP_drawText("  \\o o * o/         ", 0, 10);
    VDP_drawText("   -------          ", 0, 11);
    VDP_drawText("       8 -> UART GND", 0, 12);

    VGMPlay();
  } else if (FTDI_MODE == TRUE) {
    if(((*port2) & 0x40) == 0)
    {
      VDP_drawText("*WAITING FOR VSIF FTDI CONNECTION*", 0, 4);
      while(((*port2) & 0x40) == 0);
      VDP_clearText(0, 4, 40);
    }

    VDP_drawText("FTDI2XX MODE READY TO PLAY.", 0, 2);
    VDP_drawText("NOTE: PLEASE RESET AFTER RECONNECTED", 0, 3);

    VDP_drawText("-CONNECT P2 PORT PIN1-4,6-9 TO FTDI2XX.", 0, 5);

    VDP_drawText(" 1,2,3,4  -> FTDI2XX TX,RX,RTS,CTS", 0, 7);
    VDP_drawText("___________ ", 0, 8);
    VDP_drawText("\\* * * * o/", 0, 9);
    VDP_drawText(" \\* * * */ ", 0, 10);
    VDP_drawText("  -------   ", 0, 11);
    VDP_drawText("  6,7,8,9 -> FTDI2XX DTR,DCD,GND,DSR", 0, 12);

    // while(1)
    // {
    //   char stra[16];
    //   char strd[16];
    //   vu8 adr = 0;
    //   vu8 data = 0;
    //   vu8 tmp = 0;

    //   tmp = *port2;
    //   intToHex(tmp, strd, 2);
    //   VDP_drawText(strd, 0, 17);
    //   /*
    //   while(((tmp = *port2) & 0x40) != 0);
    //   adr = (tmp & 0x3f) << 2;
    //   while(((tmp = *port2) & 0x40) != 1);
    //   data = (tmp & 0x3f) << 2;
    //   while(((tmp = *port2) & 0x40) != 0);
    //   data |= (tmp & 0x3);
    //   while(((tmp = *port2) & 0x40) != 1);

    //   intToHex(adr, stra, 2);
    //   intToHex(data, strd, 2);
    //   VDP_drawText("     ", 0, 15);
    //   VDP_drawText(stra, 0, 15);
    //   VDP_drawText(strd, 3, 15);
    //   */
    // }

    VGMPlay_FTDI2XX();
  }
  // if (!busTaken)
  // Z80_releaseBus();
}
