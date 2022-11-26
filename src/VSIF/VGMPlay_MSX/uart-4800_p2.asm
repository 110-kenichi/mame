.INCLUDE "const.inc"

;ステート数表(MSXはM1で1 WAIT入る)
;https://web.archive.org/web/20220527125012/https://taku.izumisawa.jp/Msx/ktecho2.htm
;http://www.yamamo10.jp/yamamoto/comp/Z80/instructions/index.php
;Slots
;https://www.msx.org/wiki/Slots#Program_samples


; A ;音源の存在するスロット番号
; B ;ターゲットの拡張スロットレジスタの値（音源が存在する基本スロットのFFFFhに書き込む値）
; C ;ターゲットの基本スロットレジスタの値（音源アクセス時の基本スロットの状態）
; D ;ターゲットとP3の基本スロットレジスタの値（拡張スロット選択レジスタに値を書き込むときの基本スロットの状態）

; ・ボーレート 115200bps（１ビット 30.79clk@3.5466MHz PAL）
; ・ボーレート 115200bps（１ビット 31.07clk@3.5793MHz NTSC）
; ・ボーレート  57600bps（１ビット 61.58clk@3.547MHz PAL） 53.203MHz/15
; ・ボーレート  57600bps（１ビット 62.14clk@3.580MHz NTSC）53.693MHz/15
; ・ボーレート  38400bps（１ビット 92.4clk@3.547MHz PAL
; ・ボーレート  38400bps（１ビット 93.2clk@3.580MHz NTSC

.INCLUDE "macro.inc"

    JPOFST = 0xA0

    .area   _HEADER2 (ABS)
;=======================================================
    .ORG 0x9000
    .globl _uart_processVgm_P2
_uart_processVgm_P2:
	; LD	A,#'1'
 	; CALL	CHPUT

    DI
    LD  A,#15        ; 
    OUT (PSGAD),A    ; 
    LD  A,#0xCF      ; 
    OUT (PSGWR),A    ;    Set Joy2 Pin Input Mode

    LD  C,#14           ; 7 14
    ; JOY2 Pin Read Mode
    LD  A,C             ; 
    OUT (PSGAD),A       ; 

__VGM_LOOP:
__VGM_TYPE:
    IN  A,(PSGRD)       ; 12
    AND #0x20           ;  8
    JP  Z, __VGM_TYPE   ; 11
    IN  A,(PSGRD)       ; 12
    AND	#0x1F           ;  8
    OR  #JPOFST         ;  8
    LD  H,A             ;  5
    LD  L,#0            ;  8
    JP  (HL)            ;  5 77

_END_VGM:
    JP  _END_VGM

    _P0_CHG
    _P1_CHG

;=======================================================
    .ORG 0xA000
    __WRITE_PSG_IO

;=======================================================
    .ORG 0xA100
    __WRITE_OPLL_IO

;=======================================================
    .ORG 0xA200
    .globl __SELECT_OPLL_SLOT_P2
__SELECT_OPLL_SLOT:
    READ_ADRS           ;61
    READ_DATA           ;55
__SELECT_OPLL_SLOT_P2:
    INC D               ; 
    DEC D               ; 
    JP  NZ,OPLL1_P2     ;21 76

;================================= 1,2,3,4
;OPLL0のスロットのPAGE1を表に出す(要DI,P2上で実行)
OPLL0_P2:
    LD  A,(OPLL0_S)
    OR  A
    JP  Z,__VGM_LOOP
	LD	HL,(OPLL0_S+2)      ; 47
    P1_CHG2                 ;143 190

    ; LD  A,(OPLL0_S+3)
    ; OR  A
    ; JP  Z,__VGM_LOOP      ;

	; LD	A,(OPLL0_S)
    ; CALL    P1_CHG;

	; LD	HL,(OPLL0_S+1)
    ; OUT	(#0xA8),A	    ;   P2+P3をOPLL0の基本スロットに切り替え
	; LD	A,L
	; LD	(#0xFFFF),A	    ;   拡張スロット切り替え
	; LD	A,H
	; OUT	(#0xA8),A	    ;   P3をRAMに戻す

    JP  __VGM_LOOP          ; 11 201

;================================= 1,2,3,4
;OPLL1のスロットのPAGE1を表に出す(要DI,P2上で実行)
OPLL1_P2:
    LD  A,(OPLL1_S)
    OR  A
    JP  Z,__VGM_LOOP
	LD	HL,(OPLL1_S+2)
    P1_CHG2

    ; LD  A,(OPLL1_S+3)
    ; OR  A
    ; JP  Z,__VGM_LOOP    ;

	; LD	A,(OPLL1_S)
    ; CALL    P1_CHG;

	; LD	HL,(OPLL1_S+1)
	; OUT	(#0xA8),A	    ;   P2+P3をOPLL1の基本スロットに切り替え
	; LD	A,L
	; LD	(#0xFFFF),A	    ;   拡張スロット切り替え
	; LD	A,H
	; OUT	(#0xA8),A	    ;   P3をRAMに戻す

    JP  __VGM_LOOP      ;106
;=================================

;=======================================================
    .ORG 0xA300
__SELECT_SCC_SLOT:
    READ_ADRS           ;61
    READ_DATA           ;55

	LD	HL,(ROM1_S+2)   ; 17
    P1_CHG2             ;143

    ; PUSH BC
    ; LD   A,(ROM1_S)
    ; CALL P1_CHG          ; 48 + 300
    ; POP  BC              ; 11
    .globl __SELECT_SCC_SLOT_P2
    JP  __SELECT_SCC_SLOT_P2  ; 11

;=======================================================
    .ORG 0xA400
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xA500
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xA600
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xA700
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xA800
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xA900
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xAA00
    __WRITE_OPL3_IO1

;=======================================================
    .ORG 0xAB00
    __WRITE_OPL3_IO2

;=======================================================
    .ORG 0xAC00
    __WRITE_OPLL_MEM

;=======================================================
    .ORG 0xAD00
    __SELECT_OPM_SLOT

;=======================================================
    .ORG 0xAE00
    __WRITE_OPM_MEM

;=======================================================
    .ORG 0xAF00
    __WRITE_DCSG

;=======================================================
    .ORG 0xB000
    __WRITE_OPN2_IO1

;=======================================================
    .ORG 0xB100
    __WRITE_OPN2_IO2

;=======================================================
    .ORG 0xB200
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xB300
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xB400
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xB500
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xB600
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xB700
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xB800
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xB900
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xBA00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xBB00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xBC00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xBD00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xBE00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xBF00
    JP __VGM_LOOP       ; 

;=======================================================
