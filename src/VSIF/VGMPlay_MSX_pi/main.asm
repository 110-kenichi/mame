;--------------------------------------------------------
; File Created by SDCC : free open source ANSI-C Compiler
; Version 4.1.0 #12072 (MINGW64)
;--------------------------------------------------------
	.module main
	.optsdcc -mz80
	
;--------------------------------------------------------
; Public variables in this module
;--------------------------------------------------------
	.globl _main
	.globl _processPlayer
	.globl _print
	.globl _putchar
;--------------------------------------------------------
; special function registers
;--------------------------------------------------------
;--------------------------------------------------------
; ram data
;--------------------------------------------------------
	.area _DATA
;--------------------------------------------------------
; ram data
;--------------------------------------------------------
	.area _INITIALIZED
;--------------------------------------------------------
; absolute external ram data
;--------------------------------------------------------
	.area _DABS (ABS)
;--------------------------------------------------------
; global & static initialisations
;--------------------------------------------------------
	.area _HOME
	.area _GSINIT
	.area _GSFINAL
	.area _GSINIT
;--------------------------------------------------------
; Home
;--------------------------------------------------------
	.area _HOME
	.area _HOME
;--------------------------------------------------------
; code
;--------------------------------------------------------
	.area _CODE
	G$main$0$0	= .
	.globl	G$main$0$0
	C$main.c$10$0_0$56	= .
	.globl	C$main.c$10$0_0$56
;main.c:10: void main(void)
;	---------------------------------
; Function main
; ---------------------------------
_main::
	C$main.c$39$1_0$56	= .
	.globl	C$main.c$39$1_0$56
;main.c:39: __endasm;
;
	LD	SP,#0xF380
;
;
	LD	A,#32 ;
	LD	(#0xF3AE),A
	CALL	#0x006C
	CALL	#0x0C3
	C$main.c$41$1_0$56	= .
	.globl	C$main.c$41$1_0$56
;main.c:41: processPlayer();
	C$main.c$42$1_0$56	= .
	.globl	C$main.c$42$1_0$56
;main.c:42: }
	C$main.c$42$1_0$56	= .
	.globl	C$main.c$42$1_0$56
	XG$main$0$0	= .
	.globl	XG$main$0$0
	jp	_processPlayer
	G$print$0$0	= .
	.globl	G$print$0$0
	C$main.c$44$1_0$58	= .
	.globl	C$main.c$44$1_0$58
;main.c:44: void print(char * pc) __z88dk_fastcall
;	---------------------------------
; Function print
; ---------------------------------
_print::
	ex	de, hl
	C$main.c$46$1_0$58	= .
	.globl	C$main.c$46$1_0$58
;main.c:46: while (* pc != '\0')
00101$:
	ld	a, (de)
	ld	l, a
	or	a, a
	ret	Z
	C$main.c$48$2_0$59	= .
	.globl	C$main.c$48$2_0$59
;main.c:48: putchar(*pc);
	push	de
	call	_putchar
	pop	de
	C$main.c$49$2_0$59	= .
	.globl	C$main.c$49$2_0$59
;main.c:49: pc++;
	inc	de
	C$main.c$51$1_0$58	= .
	.globl	C$main.c$51$1_0$58
;main.c:51: }
	C$main.c$51$1_0$58	= .
	.globl	C$main.c$51$1_0$58
	XG$print$0$0	= .
	.globl	XG$print$0$0
	jr	00101$
	G$putchar$0$0	= .
	.globl	G$putchar$0$0
	C$main.c$52$1_0$61	= .
	.globl	C$main.c$52$1_0$61
;main.c:52: void putchar(char c) __z88dk_fastcall
;	---------------------------------
; Function putchar
; ---------------------------------
_putchar::
	C$main.c$57$1_0$61	= .
	.globl	C$main.c$57$1_0$61
;main.c:57: __endasm;
	ld	a,l
	call	#0x00A2
	C$main.c$58$1_0$61	= .
	.globl	C$main.c$58$1_0$61
;main.c:58: }
	C$main.c$58$1_0$61	= .
	.globl	C$main.c$58$1_0$61
	XG$putchar$0$0	= .
	.globl	XG$putchar$0$0
	ret
	.area _CODE
	.area _INITIALIZER
	.area _CABS (ABS)
