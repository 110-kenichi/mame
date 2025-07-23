.INCLUDE "const.inc"

tR = 1
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
.if UART_MODE_135
    LD  E,#135
.else
    LD  E,#2
.endif


__VGM_LOOP:
__VGM_TYPE:
    READ_UART_STAT
.if UART_MODE_135
    CP  E               ;  5
    JP  NZ, __VGM_TYPE  ; 11 28
.else
    AND E               ;  5
    JP  Z, __VGM_TYPE   ; 11
.endif
    IN  A,(UART_READ)   ; 12
    OR  #JPOFST         ;  8
    LD  H,A             ;  5
    LD  L,#0            ;  8
    JP  (HL)            ;  5 77

_END_VGM2:
	LD	A,#'E'
 	CALL	CHPUT

_END_VGM:
    JP  _END_VGM

    _P0_CHG
    _P1_CHG

;=======================================================
    .ORG 0xA000
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0xA100
    __WRITE_OPLL_IO

;=======================================================
    .ORG 0xA200
    .globl __SELECT_OPLL_SLOT_P2
__SELECT_OPLL_SLOT:
    READ_ADRS           ;45
    READ_DATA           ;40
    LD  D,A             ; 5 45
__SELECT_OPLL_SLOT_P2:
    INC D               ; 5
    DEC D               ; 5
    JP  NZ,OPLL1_P2     ;11 66

;================================= 1,2,3,4
;OPLL0のスロットのPAGE1を表に出す(要DI,P2上で実行)
OPLL0_P2:
    LD  A,(OPLL0_S)     ; 14
    OR  A               ;  5  19
    JP  Z,__VGM_LOOP    ; 11  21
	LD	HL,(OPLL0_S+2)  ; 17  38
    P1_CHG2             ;143 171

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
    READ_DATA           ;50
    LD  D,A             ; 5 55
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
    __WRITE_OPN_IO

;=======================================================
    .ORG 0xB300
    __WRITE_OPNA_PSEUDO_DAC

;=======================================================
    .ORG 0xB400
    __WRITE_OPN2_DAC

;=======================================================
    .ORG 0xB500
    __WRITE_TR_DAC

;=======================================================
    .ORG 0xB600
    __WRITE_OPNA_DAC

;=======================================================
    .ORG 0xB700
    __WRITE_PSG_IO

;=======================================================
    .ORG 0xB800
__SELECT_SIOS_SLOT:
    READ_ADRS           ;61
    READ_DATA           ;50
    LD  D,A             ; 5 55
	LD	HL,(ROM1_S+2)   ; 17
    P1_CHG2             ;143

    ; PUSH BC
    ; LD   A,(ROM1_S)
    ; CALL P1_CHG          ; 48 + 300
    ; POP  BC              ; 11
    .globl __SELECT_SIOS_SLOT_P2
    JP  __SELECT_SIOS_SLOT_P2  ; 11

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
    __WRITE_ANY_IO

;=======================================================
    .ORG 0xBF00
    JP __VGM_LOOP       ; 

;=======================================================
