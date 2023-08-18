#include "stdio.h"

void uart_processVgm();

#define CLS   #0x00C3
#define PSGAD #0xA0
#define PSGWR #0xA1
#define PSGRD #0xA2

void main() {
  
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
    printf("MAMI VGM SOUND DRIVER BY ITOKEN\r\n");
    printf("\r\n");
    printf("*PUSH PANIC BTN WHEN GET WEIRD\r\n");
    printf("*CONNECT PORT PIN TO FTDI2XX\r\n");
    printf(" ___________\r\n");
    printf(" \\1 2 3 4 5/->TX,RX,RTS,CTS,Vcc\r\n");
    printf("  \\6 7 * 9/ ->DTR,DSR,GND\r\n");
    printf("   -------\r\n\r\n");
    printf("\r\n");

    uart_processVgm();
}
