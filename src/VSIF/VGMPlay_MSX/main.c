#include "main.h"

void processPlayer();

//SDCCでMSXのクロス開発環境を作る
//https://aburi6800.hatenablog.com/entry/2019/04/29/010151

void main(void)
{
  //BIOS List
  //http://ngs.no.coocan.jp/doc/wiki.cgi/TechHan?page=Appendix+A%2E1+BIOS+%B0%EC%CD%F7
__asm
  ;//how-change-text2-mode-screen0-width80
  ;//https://www.msx.org/forum/msx-talk/development/how-change-text2-mode-screen0-width80-asm
  LD A,#80              ; 80 columns in screen 0
  LD (#0xF3AE),A
  XOR A
  CALL CHGMOD          ; SCREEN0   
__endasm;

/*
 __asm
 PSGAD = 0xA0
 PSGWR = 0xA1
 PSGRD = 0xA2
 LOOP:
     LD  A,#15        ; 7
     OUT (#PSGAD),A   ; 11
     LD  A,#0xCF      ; 7
     OUT (#PSGWR),A   ; 11   Select Joy2 Input

;     LD  C, #PSGAD    ; 7
;     LD  B,#14        ; 7
;     OUT (C),B        ; 12 Select PSG REG14
;     IN  A,(#PSGRD)   ; 11 Read JOY2
;     call    CHPUT
;     JP  LOOP

      LD  A,#7
      OUT (#PSGAD),A
      LD  A,#255
      OUT (#PSGWR),A
 
      LD  A,#7
      OUT (#PSGAD),A
      LD  A,#63
      OUT (#PSGWR),A

      LD  A,#7
      OUT (#PSGAD),A
      LD  A,#62
      OUT (#PSGWR),A
      
      LD  A,#0
      OUT (#PSGAD),A
      LD  A,#172
      OUT (#PSGWR),A

      LD  A,#1
      OUT (#PSGAD),A
      LD  A,#1
      OUT (#PSGWR),A

      LD  C,#0xa0
      LD  D,#8
      OUT (C),D
      LD  C,#0xa1
      LD  D,#15
      OUT (C),D
 __endasm;
//*/

    processPlayer();
}

void print(char * pc) __z88dk_fastcall
{
    while (* pc != '\0')
    {
        putchar(*pc);
        pc++;
    }
}
void putchar(char c) __z88dk_fastcall
{
__asm
    ld      a,l
    call    CHPUT
__endasm;
}
