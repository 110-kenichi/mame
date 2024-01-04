#include "main.h"


__sfr __at 0xDC JPORT_A;
__sfr __at 0xDD JPORT_B;

__sfr __at 0x7F PSGPort;

__sfr __at 0xF0 OPLLPortA0;
__sfr __at 0xF1 OPLLPortA1;
__sfr __at 0xF2 ConfigPort;

#define PSGLatch # 0x80
#define PSGData # 0x40

#define PSGChannel0 # 0b00000000
#define PSGChannel1 # 0b00100000
#define PSGChannel2 # 0b01000000
#define PSGChannel3 # 0b01100000
#define PSGVolumeData # 0b00010000

void NmiInterruptHandler() { __asm__("RST 0"); }

void soundAllOff() {
  // PSG on
  ConfigPort = 0;
  PSGPort = PSGLatch | PSGChannel0 | PSGVolumeData |
            0x0F;  // latch channel 0, volume=0xF (silent)
  PSGPort = PSGLatch | PSGChannel1 | PSGVolumeData |
            0x0F;  // latch channel 1, volume=0xF (silent)
  PSGPort = PSGLatch | PSGChannel2 | PSGVolumeData |
            0x0F;  // latch channel 2, volume=0xF (silent)
  PSGPort = PSGLatch | PSGChannel3 | PSGVolumeData |
            0x0F;  // latch channel 3, volume=0xF (silent)

  // FM on
  ConfigPort = 1;
  for (int i = 0; i < 9; i++) {
    OPLLPortA0 = 0x20 + i;
    OPLLPortA1 = 0x00;
  }
  OPLLPortA0 = 0x0E;
  OPLLPortA1 = 0x00;

  //Enable OPLL and PSG
  ConfigPort = 3;
}

void uart_processVgm();
void uart_processVgm_FTDI();
void uart_processVgm_FTDI_SMS();
void printHex(unsigned long value, unsigned char x, unsigned char y, unsigned char width);

void processPlayer(char vblank) {
  if (vblank) {
    switch (PhaseCounter) {
      case 0: {
        DisableVDPProcessing = true;
        SMS_displayOff();
        InitGameVars();
        SMS_VDPturnOffFeature(VDPFEATURE_SHIFTSPRITES);
        SMS_VDPturnOffFeature(VDPFEATURE_HIDEFIRSTCOL);
        SMS_setSpriteMode(SPRITEMODE_NORMAL);

        for (int i = 0; i < font_pal_bin_size; i++) {
          SetBGPaletteColor(i, font_pal_bin[i]);
          SetSpritePaletteColor(i, font_pal_bin[i] & 0xf);
        }

        SMS_setNmiInterruptHandler(NmiInterruptHandler);

        soundAllOff();

        DisableVDPProcessing = false;
        PhaseCounter++;
        SMS_displayOn();

        PrintText("MAMI VGM SOUND DRIVER BY ITOKEN", 0, 0);
        PrintText("PLEASE SELECT DRIVER TYPE.", 0, 3);

        PrintText("(UP)", 0, 6);
        PrintText(" -GENERAL UART(57600BPS)", 0, 7);

        PrintText("(DOWN)", 0, 9);
        PrintText(" -FTDI2XX DONGLE(BIT BANG)", 0, 10);

        PrintText("(LEFT)", 0, 12);
        PrintText(" -FTDI2XX DONGLE(BIT BANG)(SMS)", 0, 13);

        break;
      }
      case 1: {
        unsigned int key = SMS_getKeysPressed();
        switch(key)
        {
            case PORT_A_KEY_UP:
                PhaseCounter = 2;
                goto clear;
                break;
            case PORT_A_KEY_DOWN:
                PhaseCounter = 3;
                goto clear;
                break;
            case PORT_A_KEY_LEFT:
                PhaseCounter = 4;
                goto clear;
                break;
clear:
                PrintText("                          ", 0, 3);
                PrintText("    ", 0, 6);
                PrintText("                           ", 0, 7);
                PrintText("      ", 0, 9);
                PrintText("                           ", 0, 10);
                PrintText("      ", 0, 12);
                PrintText("                               ", 0, 13);

                break;
        }
        break;
      }
      // case 4: {
      //   PrintHexShort(JPORT_B & 0xf, 1, 1, 1);

      //   break;
      // }
      case 4: {
        PrintText("MAMI VGM SOUND DRIVER BY ITOKEN", 0, 0);
        PrintText("READY TO PLAY.", 0, 1);

        PrintText("-PRESS PAUSE BTN TO RESTART.", 0, 3);

        PrintText("___________        ", 0, 5);
        PrintText("\\1 2 3 4 5/->TX,RX,RTS,CTS,VCC", 0, 6);
        PrintText(" \\6 * 8 9/ ->DTR,GND,DSR", 0,7);
        PrintText("  -------          ", 0, 8);

        uart_processVgm_FTDI_SMS();
        soundAllOff();
        break;
      }
      case 3: {
        PrintText("MAMI VGM SOUND DRIVER BY ITOKEN", 0, 0);
        PrintText("READY TO PLAY.", 0, 1);

        PrintText("-PRESS PAUSE BTN TO RESTART.", 0, 3);

        PrintText("___________        ", 0, 5);
        PrintText("\\1 2 3 4 5/->TX,RX,RTS,CTS,VCC", 0, 6);
        PrintText(" \\6 * 8 9/ ->DTR,GND,DSR", 0,7);
        PrintText("  -------          ", 0, 8);

        uart_processVgm_FTDI();
        soundAllOff();
        break;
      }
      case 2: {
        // PrintText("WAITING CONNECTION...", 0, 0);
        // while (1) {
        //   uart_getc2();
        //   if (uart_status != UART_STATUS_OK) continue;
        //   uart_putc2(0x0f);
        //   break;
        // }
        // PrintText("CONNECTED. VGM READY.", 0, 0);
        // int y = 0;
        // while (1) {
        //   uart_getc2();
        //   if (uart_status != UART_STATUS_OK) continue;
        //   //PSGPort = uart_result;

        //   //if (y >= 5) {
        //   if (y >= 8) {
        //     y = 0;
        //     for (int y1 = 0; y1 < 5; y1++)
        //       for (int x1 = 0; x1 < 8; x1++) SetTileatXY(x1, y1, 0);
        //   }
        //   for (int x1 = 0; x1 < 8; x1++) {
        //     PrintChar(0x30 + (uart_result & 1), 7 - x1, y);
        //     uart_result = uart_result >> 1;
        //   }
        //   y++;
        //   /*
        //   if(uart_result & 0x80)
        //   {
        //     //LATCH
        //     switch(uart_result & 0x60)
        //     {
        //       case 0x00:
        //         PrintChar('1',0,y);
        //         break;
        //       case 0x20:
        //         PrintChar('2',0,y);
        //         break;
        //       case 0x40:
        //         PrintChar('3',0,y);
        //         break;
        //       case 0x60:
        //         PrintChar('N',0,y);
        //         break;
        //     }
        //     switch(uart_result & 0x10)
        //     {
        //       case 0x00:
        //         PrintChar('T',2,y);
        //         break;
        //       case 0x10:
        //         PrintChar('V',2,y);
        //         break;
        //     }
        //     SMS_setNextTileatXY(4, y);
        //     SMS_setTile(hexNumToTileNo[uart_result & 0xf]);
        //   }else
        //   {
        //     //DATA
        //     SMS_setNextTileatXY(0, y);
        //     SMS_setTile(hexNumToTileNo[uart_result >> 4]);
        //     SMS_setNextTileatXY(1, y);
        //     SMS_setTile(hexNumToTileNo[uart_result & 0xf]);
        //   }
        //   */
        // }

        PrintText("MAMI VGM SOUND DRIVER BY ITOKEN", 0, 0);
        PrintText("READY TO PLAY.", 0, 1);

        PrintText("-PRESS PAUSE BTN TO RESTART.", 0, 3);

        PrintText("___________        ", 0, 5);
        PrintText("\\* * 3 * */-> UART TX", 0, 6);
        PrintText(" \\* * 8 */ -> UART GND", 0,7);
        PrintText("  -------          ", 0, 8);

        uart_processVgm();
        soundAllOff();
        break;
      }
    }
  }
}
