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

    JPOFST = 0x60

    .area   _HEADER (ABS)
;=======================================================
    .ORG 0x5000
_uart_processVgm::
	; LD	A,#'0'
 	; CALL	CHPUT

    DI
    LD  A,#15        ; 
    OUT (PSGAD),A    ; 
    LD  A,#0xCF      ; 
    OUT (PSGWR),A    ;    Set Joy2 Pin Input Mode

    ; JOY2 Pin Read Mode
    LD  C,#14           ; 7 14
    LD  A,C             ; 
    OUT (PSGAD),A       ; 

    ; LD   A,(ROM2_S)
    ; CALL P2_CHG          ; 48 + 300
    ; .globl _uart_processVgm_P2
    ; JP  _uart_processVgm_P2  ; 11

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

    ;_P0_CHG
    _P2_CHG

;=======================================================
    .ORG 0x6000
    __WRITE_PSG_IO

;=======================================================
    .ORG 0x6100
    __WRITE_OPLL_IO

;=======================================================
    .ORG 0x6200
__SELECT_OPLL_SLOT:
    READ_ADRS           ;61
    READ_DATA           ;55

;================================= 1,2,3,4
	LD	HL,(ROM2_S+2)   ; 17
    P2_CHG2             ;143
    ; PUSH BC
    ; LD   A,(ROM2_S)
    ; CALL P2_CHG          ; 48 + 300
    ; POP  BC              ; 11
    .globl __SELECT_OPLL_SLOT_P2
    JP  __SELECT_OPLL_SLOT_P2  ; 11

;=======================================================
    .ORG 0x6300
    ;https://www.msx.org/forum/msx-talk/software/scc-music-altera-de-1
    .globl __SELECT_SCC_SLOT_P2
__SELECT_SCC_SLOT:
    READ_ADRS           ;61
    READ_DATA           ;55
;================================= 1
__SELECT_SCC_SLOT_P2:
    LD  A,B
    CP  #4
    JP  NC,CALL_P2_CHG  ;24 85 ;4または4以上で従来のスロット選択方式
    INC D               ;
    DEC D               ;
    JP  NZ,SCC1_P2      ;45 100

;================================= 2,3
;SCC0のスロットのPAGE2を表に出す(要DI,P1上で実行)
SCC0_P2:
    LD  A,(SCC0_S)
    OR  A
    JP  Z,__VGM_LOOP
	LD	HL,(SCC0_S+2)   ; 47
    P2_CHG2             ;143 190

    ; LD  A,(SCC0_S+3)
    ; OR  A
    ; JP  Z,__VGM_LOOP    ;

    ; PUSH BC
	; LD	A,(SCC0_S)
    ; CALL    P2_CHG;      ; 300
    ; POP  BC              ; 11

	; LD	HL,(SCC0_S+1)   ;
	; OUT	(#0xA8),A	    ;    ;P2+P3をSCC0の基本スロットに切り替え
	; LD	A,L             ;
	; LD	(#0xFFFF),A	    ;    ;拡張スロット切り替え
	; LD	A,H             ;
	; OUT	(#0xA8),A	    ;    ;P3をRAMに戻す

    JP  _ENA_SCC        ; 106
;SCC1のスロットのPAGE2を表に出す(要DI,P1上で実行)
;================================= 2,3
SCC1_P2:
    LD  A,(SCC1_S)
    OR  A
    JP  Z,__VGM_LOOP
	LD	HL,(SCC1_S+2)
    P2_CHG2

    ; LD  A,(SCC1_S+3)
    ; OR  A
    ; JP  Z,__VGM_LOOP    ;

    ; PUSH BC
	; LD	A,(SCC1_S)
    ; CALL    P2_CHG;      ; 300
    ; POP  BC              ; 11

	; LD	HL,(SCC1_S+1)   ;
	; OUT	(#0xA8),A   	;      ;P2+P3をSCC1の基本スロットに切り替え
	; LD	A,L
	; LD	(#0xFFFF),A	    ;      ;拡張スロット切り替え
	; LD	A,H
	; OUT	(#0xA8),A	    ;  95  ;P3をRAMに戻す

    JP  _ENA_SCC        ; 106

;================================= 2,3,4,5
CALL_P2_CHG:
    SUB  #4
    LD   B,A
    LD   A,D
    PUSH BC              ; 30
    CALL P2_CHG          ; 18 + 300
    POP  BC              ; 11
;================================= 4/6
_ENA_SCC:
    LD      A,B          ; 
    DEC     A            ; 
    JP      Z,__ENA_SCC1 ; 
    DEC     A            ; 
    JP      Z,__ENA_SCC1_COMPAT ;
    DEC     A            ; 
    JP      Z,__ENA_SCC  ; 
    JP      __VGM_LOOP   ; 64

__ENA_SCC1:
    LD      HL,#0xBFFE   ;
    LD      A,#0x20      ;
    LD      (HL),A       ;

    LD      HL,#0xB000   ;
    LD      A,#0x80      ;
    LD      (HL),A       ;
    JP  __VGM_LOOP       ; 65
;================================= 4/6

__ENA_SCC1_COMPAT:       ;
    LD      HL,#0xBFFE
    LD      A,#0x00
    LD      (HL),A
__ENA_SCC:
    LD      HL,#0x9000
    LD      A,#0x3F
    LD      (HL),A
    JP  __VGM_LOOP       ; 65
;=================================

;=======================================================
    .ORG 0x6400
    __WRITE_SCC1

;=======================================================
    .ORG 0x6500
    __WRITE_SCC

;=======================================================
    .ORG 0x6600
    __WRITE_SCC1_2BYTES

;=======================================================
    .ORG 0x6700
    __WRITE_SCC_2BYTES

;=======================================================
    .ORG 0x6800
    __WRITE_SCC1_32_BYTES

;=======================================================
    .ORG 0x6900
    __WRITE_SCC_32_BYTES

;=======================================================
    .ORG 0x6A00
    __WRITE_OPL3_IO1

;=======================================================
    .ORG 0x6B00
    __WRITE_OPL3_IO2

;=======================================================
    .ORG 0x6C00
__WRITE_OPLL_MEM_DUMMY:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x6D00
    __SELECT_OPM_SLOT

;=======================================================
    .ORG 0x6E00
    __WRITE_OPM_MEM

;=======================================================
    .ORG 0x6F00
    __WRITE_DCSG

;=======================================================
    .ORG 0x7000
    __WRITE_OPN2_IO1

;=======================================================
    .ORG 0x7100
    __WRITE_OPN2_IO2

;=======================================================
    .ORG 0x7200
__DUMMY_0x7200:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7300
__DUMMY_0x7300:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7400
__DUMMY_0x7400:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7500
__DUMMY_0x7500:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7600
__DUMMY_0x7600:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7700
__DUMMY_0x7700:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7800
__DUMMY_0x7800:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7900
__DUMMY_0x7900:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7A00
__DUMMY_0x7A00:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7B00
__DUMMY_0x7B00:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7C00
__DUMMY_0x7C00:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7D00
__DUMMY_0x7D00:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7E00
__DUMMY_0x7E00:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7F00
__DUMMY_0x7F00:
    READ_ADRS           ;61
    READ_DATA           ;55
    JP __VGM_LOOP       ; 

;=======================================================
