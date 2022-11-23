#include "main.h"

void processPlayer();

//SDCCでMSXのクロス開発環境を作る
//https://aburi6800.hatenablog.com/entry/2019/04/29/010151

//https://w.atwiki.jp/msx-sdcc/pages/49.html

void main(void)
{
    //BIOS List
    //http://ngs.no.coocan.jp/doc/wiki.cgi/TechHan?page=Appendix+A%2E1+BIOS+%B0%EC%CD%F7
__asm
    ;//Init SP
    LD SP,#0xF380
    
    ;//how-change-text2-mode-screen0-width80
    ;//https://www.msx.org/forum/msx-talk/development/how-change-text2-mode-screen0-width80-asm
    LD A,#32              ;// 80 columns in screen 0
    LD (#0xF3AE),A
    CALL #0x006C
    CALL #0x0C3
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
