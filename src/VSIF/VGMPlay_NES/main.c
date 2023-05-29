#include <conio.h>
#include <nes.h>

extern void VGMPlay_FTDI2XX_DIRECT();
extern void VGMPlay_FTDI2XX_INDIRECT();

volatile unsigned char *port4016 = (volatile unsigned char *)0x4016;  // Port
volatile unsigned char *port4017 = (volatile unsigned char *)0x4017;  // Port

#ifdef VRC6_ADDRESS
// volatile unsigned char *port9000 = (volatile unsigned char *)0x9000;  // Port
// volatile unsigned char *port9001 = (volatile unsigned char *)0x9001;  // Port
// volatile unsigned char *port9002 = (volatile unsigned char *)0x9002;  // Port

void init_zp_for_vrc6()
{
    int i = 0;
    unsigned short *zpg = (unsigned short *)0x80;
    unsigned short address[] = {
        0x4000, 0x4001, 0x4002, 0x4003, 0x4004, 0x4005, 0x4006, 0x4007,
        0x4008, 0x4009, 0x400a, 0x400b, 0x400c, 0x400d, 0x400e, 0x400f,
        0x4010, 0x4011, 0x4012, 0x4013, 0x4014, 0x4015, 0x0000, 0x0000,
        0x9000, 0x9001, 0x9002, 0x9003, 0xa000, 0xa001, 0xa002, 0xa003,
        0xb000, 0xb001, 0xb002, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
    };
    unsigned short *adr = address;
    for (i = 0; i < sizeof(address); i++) {
      *zpg++ = *adr++;
    }  
}
#endif

int main(void) {
  clrscr();

  // cputsxy(0, 0, "MAMI VGM SOUND DRIVER BY ITOKEN");
  // if (((*port4016) & 0x02) == 0) {
  //   cputsxy(0, 2, "*WAITING FOR VSIF FTDI*");
  //   while (((*port4016) & 0x02) == 0)
  //     ;
  //   clrscr();
  // }
  cputsxy(0, 0, "MAMI VGM SOUND DRIVER BY ITOKEN");

  cputsxy(0, 2, "FTDI2XX MODE READY TO PLAY.");
  cputsxy(0, 3, "*PLEASE RESET AFTER RECONNECTED");

  cputsxy(0, 5, " 1  4567 -> GND,CTS,RTS,RX,TX");
  cputsxy(0, 6, "___________");
  cputsxy(0, 7, "\\*oo****o /");
  cputsxy(0, 8, " \\oooo*oo/ ");
  cputsxy(0, 9, "  -------  ");
  cputsxy(0, 10, "     13 ->  DTR");
  waitvsync();

  // *port9000 = 0x0f;
  // *port9001 = 0xff;
  // *port9002 = 0x01;

  while (1) {
    /*
    unsigned char adrs = 0;
    unsigned char data = 0;

    while ((*(volatile unsigned char *)0x4016 & 0x02) == 0)
      ;
    while ((*(volatile unsigned char *)0x4017 & 0x02) == 0)
      ;
    adrs = ((*port4017) & 0x1c) << 3;

    while ((*(volatile unsigned char *)0x4016 & 0x02) != 0)
      ;
    while ((*(volatile unsigned char *)0x4017 & 0x02) != 0)
      ;
    adrs |= ((*port4017) & 0x1c);

    while ((*(volatile unsigned char *)0x4016 & 0x02) != 0)
      ;
    while ((*(volatile unsigned char *)0x4017 & 0x02) == 0)
      ;
    adrs |= ((*port4017) & 0x0c) >> 2;

    while ((*(volatile unsigned char *)0x4016 & 0x02) != 0)
      ;
    while ((*(volatile unsigned char *)0x4017 & 0x02) != 0)
      ;
    data = ((*port4017) & 0x1c) << 3;

    while ((*(volatile unsigned char *)0x4016 & 0x02) != 0)
      ;
    while ((*(volatile unsigned char *)0x4017 & 0x02) == 0)
      ;
    data |= ((*port4017) & 0x1c);

    while ((*(volatile unsigned char *)0x4016 & 0x02) != 0)
      ;
    while ((*(volatile unsigned char *)0x4017 & 0x02) != 0)
      ;
    data |= ((*port4017) & 0x0c) >> 2;

    cprintf("%4x = %2x \r\n", 0x4000 + adrs, data);
    // // adrs = (*port4017);
    // // data = (*port4016);
    *(volatile unsigned char *)(0x4000+adrs) = data;
    // // cputsxy(0,1,"");
  }*/
#ifdef DIRECT_ADDRESS
    VGMPlay_FTDI2XX_DIRECT();
#endif

#ifdef VRC6_ADDRESS
    init_zp_for_vrc6();
    VGMPlay_FTDI2XX_INDIRECT();
#endif
  }

  return 0;
}
