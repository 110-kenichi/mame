#include "main.h"

void uart_processVgm();
void SARCHFM();

#define CLS   #0x00C3
#define PSGAD #0xA0
#define PSGWR #0xA1
#define PSGRD #0xA2

void processPlayer() {
  
/*  
    print("*WAITING FOR VSIF FTDI CONNECTION*");
__asm
    DI
    LD  A,#15
    OUT (PSGAD),A
    LD  A,#0xCF
    OUT (PSGWR),A

    LD  A,#14
    OUT (PSGAD),A
LOOP:
    IN  A,(PSGRD)
    AND #0x3F
    JP  NZ,LOOP

    CALL CLS
__endasm;
*/
    print("MAMI VGM SOUND DRIVER BY ITOKEN\r\n");
    print("\r\n");
    print("*PUSH PANIC BTN WHEN GET WEIRD\r\n");
    print("*CONNECT PORT2 PIN TO FTDI2XX\r\n");
    print(" ___________\r\n");
    print(" \\1 2 3 4 5/->TX,RX,RTS,CTS,Vcc\r\n");
    print("  \\6 7 * 9/ ->DTR,DSR,GND\r\n");
    print("   -------\r\n\r\n");

    SARCHFM();
    //uart_processVgm();
}
