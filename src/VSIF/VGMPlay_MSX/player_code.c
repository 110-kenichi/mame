#include "main.h"

void uart_processVgm();

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
    print("NOTE: PLEASE RESET AFTER RECONNECTED");
    print(" \r\n");
    print("READY TO PLAY.\r\n");
    print(" \r\n");
    print("-CONNECT P2 PORT PIN1-4,6,7,9 TO FTDI2XX.");
    print(" \r\n");
    print(" 1,2,3,4  -> FTDI2XX TX,RX,RTS,CTS\r\n");
    print("___________ \r\n");
    print("\\* * * * o/\r\n");
    print(" \\* * o */ \r\n");
    print("  -------   \r\n");
    print("  6,7,  9 -> FTDI2XX DTR,DSR,GND\r\n");

    uart_processVgm();
}
