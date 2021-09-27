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
