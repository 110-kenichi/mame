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
extern void VGMPlay_Low();
extern void VGMPlay_FTDI2XX();
extern int VGMPlay_InitMCD();
extern void VGMPlay_FTDI2XX_MDCD();

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

  VDP_drawText("CHECK MEGA CD/SEGA CD...", 0, 2);
  if(VGMPlay_InitMCD() != 0)
  {
    VDP_drawText("NORMAL MEGA DRIVE MODE", 0, 2);

    VDP_drawText("  PLEASE SELECT DRIVER TYPE.", 0, 4);

    VDP_drawText("  (UP)", 0, 7);
    VDP_drawText("   -GENERAL UART(163840BPS) -", 0, 8);

    VDP_drawText("  (DOWN)", 0, 10);
    VDP_drawText("   -FTDI2XX DONGLE(BIT BANG)-", 0, 11);

    VDP_drawText("  (LEFT)", 0, 13);
    VDP_drawText("   -GENERAL UART(115200BPS) -", 0, 15);

    bool UART_MODE = FALSE;
    bool UART_LOW_MODE = FALSE;
    bool FTDI_MODE = FALSE;
    while (1) {
      vu8 value = *port1;
      if ((value & 1) == 0) {
        UART_MODE = TRUE;
        break;
      } else if ((value & 2) == 0) {
        FTDI_MODE = TRUE;
        break;
      } else if ((value & 4) == 0) {
        UART_LOW_MODE = TRUE;
        break;
      }
      VDP_waitVSync();
    }
    VDP_clearText(0, 4, 40);
    VDP_clearText(0, 7, 40);
    VDP_clearText(0, 8, 40);
    VDP_clearText(0, 10, 40);
    VDP_clearText(0, 11, 40);
    VDP_clearText(0, 13, 40);
    VDP_clearText(0, 14, 40);

    //  u16 busTaken = Z80_getAndRequestBus(TRUE);
    SYS_disableInts();
    //*port2ctrl = 0x00;  //ALL INPUT and DISABLE INT
    if (UART_MODE == TRUE) {
      VDP_drawText("MD UART MODE(16K) READY TO PLAY.", 0, 2);

      VDP_drawText("-CONNECT P2 PORT PIN1 TO TX.", 0, 4);
      VDP_drawText("-CONNECT P2 PORT PIN8 TO GND.", 0, 5);

      VDP_drawText("___________ ", 0, 7);
      VDP_drawText("\\1 * * * */->UART TX", 0, 8);
      VDP_drawText(" \\* * 8 */ ->UART GND", 0, 9);
      VDP_drawText("  -------   ", 0, 10);

      VGMPlay();
    }else if (UART_LOW_MODE == TRUE) {
      VDP_drawText("MD UART MODE(11K) READY TO PLAY.", 0, 2);

      VDP_drawText("-CONNECT P2 PORT PIN1 TO TX.", 0, 4);
      VDP_drawText("-CONNECT P2 PORT PIN8 TO GND.", 0, 5);

      VDP_drawText("___________ ", 0, 7);
      VDP_drawText("\\1 * * * */->UART TX", 0, 8);
      VDP_drawText(" \\* * 8 */ ->UART GND", 0, 9);
      VDP_drawText("  -------   ", 0, 10);

      VGMPlay_Low();
    } else if (FTDI_MODE == TRUE) {
      VDP_drawText("MD FTDI2XX MODE READY TO PLAY.", 0, 2);
      VDP_drawText("NOTE: PLEASE RESET AFTER RECONNECTED", 0, 3);

      VDP_drawText("-CONNECT P2 PORT PIN1-4,6-9 TO FTDI2XX.", 0, 5);

      VDP_drawText("___________ ", 0, 7);
      VDP_drawText("\\1 2 3 4 5/->FTDI2XX TX,RX,RTS,CTS,VCC", 0, 8);
      VDP_drawText(" \\6 7 8 9/ ->FTDI2XX DTR,DCD,GND,DSR", 0, 9);
      VDP_drawText("  -------   ", 0, 10);

      VGMPlay_FTDI2XX();
    }
  }
  else
  {
    VDP_drawText("                       ", 0, 2);

    VDP_drawText("MDCD FTDI2XX MODE READY TO PLAY.", 0, 2);
    VDP_drawText("NOTE: PLEASE RESET AFTER RECONNECTED", 0, 3);

    VDP_drawText("-CONNECT P2 PORT PIN1-4,6-9 TO FTDI2XX.", 0, 5);

    VDP_drawText("___________ ", 0, 7);
    VDP_drawText("\\1 2 3 4 5/->FTDI2XX TX,RX,RTS,CTS,VCC", 0, 8);
    VDP_drawText(" \\6 7 8 9/ ->FTDI2XX DTR,DCD,GND,DSR", 0, 9);
    VDP_drawText("  -------   ", 0, 10);

    VGMPlay_FTDI2XX_MDCD();
  }
}
