#include "main.h"

void processPlayer();

//SDCCでMSXのクロス開発環境を作る
//https://aburi6800.hatenablog.com/entry/2019/04/29/010151

//https://w.atwiki.jp/msx-sdcc/pages/49.html

void main(void)
{

#ifdef VKEY
__asm
	IN      A,(#0xAA)	//PPIレジスタCを読む
	AND     #0xF0		//キーマトリクス以外を残して
				        //キーボードRow選択bit=0000にする
	OR      #5		    //マトリクス#5を指定
	OUT     (#0xAA),A	
	NOP
	IN	    A,(#0xA9)	//キーマトリクスの列を読む
	AND     #0b00001000 //V押下を調べる(bit3)
	RET     NZ		    //Vキー押下されていなければROM起動しない
__endasm;
#endif

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
