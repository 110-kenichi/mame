;ステート数表(MSXはM1で1 WAIT入る)
;https://web.archive.org/web/20220527125012/https://taku.izumisawa.jp/Msx/ktecho2.htm
;http://www.yamamo10.jp/yamamoto/comp/Z80/instructions/index.php
;Slots
;https://www.msx.org/wiki/Slots#Program_samples

PSGAD = #0xA0
PSGWR = #0xA1
PSGRD = #0xA2

OPLLAD = #0x7C
OPLLWR = #0x7D

OPL3AD1 = #0xC4
OPL3WR  = #0xC5
OPL3AD2 = #0xC6

DCSGAD = #0x3F

OPN2AD1 = #0x14 ;:OPN2 ch1-3 address(W) / status(R)
OPN2WR1 = #0x15 ;:OPN2 ch1-3 data(W)
OPN2AD2 = #0x16 ;:OPN2 ch4-6 address(W)
OPN2WR2 = #0x17 ;:OPN2 ch4-6 data(W)

WRSLT = #0x14
RDSLT = #0x0c
ENASLT = #0x24

CHPUT = #0xA2

;ユーザー定義のワークリア
SCC0_S	= #0xE000		;+0 SCC0 スロット番号
			        	;+1 SCC0 切り替え用拡張スロットレジスタ
			        	;+2 SCC0 運用時基本スロットレジスタ
			        	;+3 SCC0 P2+P3切り替え用基本スロットレジスタ
SCC1_S	= #0xE004		;以下同様

OPLL0_S	= #0xE008

OPLL1_S	= #0xE00C

OPM0_S	= #0xE010

OPM1_S	= #0xE014

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

; void uart_processVgm()
    ;41BCH

;=======================================================
    .macro INIT_CONST   ;   14
    ;LD  E,#0x30         ; 7  7
    LD  C,#14           ; 7 14
    .endm

;=======================================================
    .macro WRITE_SCC_2BYTES
1$:
    ; 2ND DATA Hi 4bit
    IN  A,(PSGRD)       ; 12 12
    AND #0x10           ;  8 
    JP  NZ, 1$          ; 11 
    IN  A,(PSGRD)       ; 12 
    LD  E,A             ;  5 
    SLA E               ; 10 
    SLA E               ; 10 
    SLA E               ; 10 
    SLA E               ; 10 88
2$:
    ; 2ND DATA Lo 4bit
    IN  A,(PSGRD)       ; 12 12
    AND #0x10           ;  8
    JP  Z, 2$           ; 11
    IN  A,(PSGRD)       ; 12
    AND #0xf            ;  8
    OR  E               ;  5 56

    ;WRITE BOTH 1st BYTE AND 2nd BYTE
    LD  (HL), D         ;  8 
    INC L               ;  5 
    LD  (HL), A         ;  8 
    JP  __VGM_LOOP      ; 11 88
    .endm

;=======================================================
    .macro WRITE_SCC_31_BYTES
    LD  B,#31           ;  8  8
1$:
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 12
    AND #0x10           ;  8
    JP  NZ,1$           ; 11
    IN  A,(PSGRD)       ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10 96
2$:
    ; DATA Lo 4bit
    IN  A,(PSGRD)       ; 12 12
    AND #0x10           ;  8 
    JP  Z,2$            ; 11 
    IN  A,(PSGRD)       ; 12 
    AND #0xf            ;  8 
    OR  D               ;  5 

    ;WRITE NEXT BYTE
    INC L               ;  5 
    LD  (HL), A         ;  8 
    DJNZ 1$             ; 14 83  9 78
    JP  __VGM_LOOP      ; 11 94 11 89
    .endm

;=======================================================
    .macro READ_ADRS
;__VGM_ADRS_HI:
5$:
    IN  A,(PSGRD)       ; 12
    AND #0x20           ;  8
    JP  NZ, 5$          ; 11
    IN  A,(PSGRD)       ; 12
    LD  B,A             ;  5
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10 88

;__VGM_ADRS_LO:
6$:
    IN  A,(PSGRD)       ; 12
    AND #0x10           ;  8
    JP  Z, 6$           ; 11
    ; ADDRESS Lo 4bit
    IN  A,(PSGRD)       ; 12
    AND #0xf            ;  8
    ; ADDRESS 8bit
    OR  B               ;  5
    LD  B, A            ;  5 61
    .endm

;=======================================================
    .macro READ_DATA
    LD      H,#0x10     ;  8
;__VGM_DATA_HI:
3$:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  NZ, 3$          ; 11 __VGM_DATA_HI
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10

    LD  E,#0xF          ;  8 101
;__VGM_DATA_LO:
4$:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  Z,4$            ; 11 __VGM_DATA_LO
    ; DATA Lo 4bit
    IN  A,(PSGRD)       ; 12
    AND E               ;  5
    ; DATA 8bit
    OR  D               ;  5
    LD  D,A             ;  5 55
    .endm

;=======================================================
_uart_processVgm::
    DI
    LD  A,#15        ; 
    OUT (PSGAD),A    ; 
    LD  A,#0xCF      ; 
    OUT (PSGWR),A    ;    Set Joy2 Pin Input Mode

    INIT_CONST
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
    OR  #0x60           ;  8
    LD  H,A             ;  5
    LD  L,#0            ;  8
    JP  (HL)            ;  5 77

_END_VGM:
    EI                  ; 
    ret                 ; 

    .area   _HEADER (ABS)
;=======================================================
    .ORG 0x6000
__WRITE_PSG_IO:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  A,B             ; 
    OUT (PSGAD),A       ; 
    LD  A,D             ; 
    OUT (PSGWR),A       ; 89

    ; JOY2 Pin Read Mode
    LD  A,C             ; 
    OUT (PSGAD),A       ; 12 101
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	#0x1F           ; 
    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ;       ;Zero clear
    JP  (HL)            ; 94

;=======================================================
    .ORG 0x6100
__WRITE_OPLL_IO:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPLLAD),A      ; 17 88
;=======================

    READ_DATA           ; 55 
    LD  A,D             ; 
    OUT (OPLLWR),A      ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 30 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID

;=======================================================
    .ORG 0x6200
__SELECT_OPLL_SLOT:
    READ_ADRS           ;61
    READ_DATA           ;55
    INC D               ; 
    DEC D               ; 
    JP  NZ,OPLL1_P2     ;21 76
;=================================

;OPLL0のスロットのPAGE2を表に出す(要DIで実行)
OPLL0_P2:
    LD  A,(OPLL0_S+3)
    OR  A
    JP  Z,__VGM_LOOP    ;

	LD	HL,(OPLL0_S+1)
    OUT	(#0xA8),A	    ;   P2+P3をOPLL0の基本スロットに切り替え
	LD	A,L
	LD	(#0xFFFF),A	    ;   拡張スロット切り替え
	LD	A,H
	OUT	(#0xA8),A	    ;   P3をRAMに戻す
    JP  __VGM_LOOP      ; 89
;OPLL1のスロットのPAGE2を表に出す(要DIで実行)
OPLL1_P2:
    LD  A,(OPLL1_S+3)
    OR  A
    JP  Z,__VGM_LOOP    ;

	LD	HL,(OPLL1_S+1)
	OUT	(#0xA8),A	    ;   P2+P3をOPLL1の基本スロットに切り替え
	LD	A,L
	LD	(#0xFFFF),A	    ;   拡張スロット切り替え
	LD	A,H
	OUT	(#0xA8),A	    ;   P3をRAMに戻す
    JP  __VGM_LOOP      ;106
;=================================

;=======================================================
    .ORG 0x6300
    ;https://www.msx.org/forum/msx-talk/software/scc-music-altera-de-1
__SELECT_SCC_SLOT:
    READ_ADRS           ;61
    READ_DATA           ;55
    LD  A,B
    CP  #4
    JP  NC,CALL_P2_CHG  ;24 85 ;4または4以上で従来のスロット選択方式
    INC D               ;
    DEC D               ;
    JP  NZ,SCC1_P2      ;45 100
;=================================

;SCC0のスロットのPAGE2を表に出す(要DIで実行)
SCC0_P2:
    LD  A,(SCC0_S+3)
    OR  A
    JP  Z,__VGM_LOOP    ;

	LD	HL,(SCC0_S+1)   ;
	OUT	(#0xA8),A	    ;    ;P2+P3をSCC0の基本スロットに切り替え
	LD	A,L             ;
	LD	(#0xFFFF),A	    ;    ;拡張スロット切り替え
	LD	A,H             ;
	OUT	(#0xA8),A	    ;    ;P3をRAMに戻す

    JP  _ENA_SCC        ; 106
;SCC1のスロットのPAGE2を表に出す(要DIで実行)
SCC1_P2:
    LD  A,(SCC1_S+3)
    OR  A
    JP  Z,__VGM_LOOP    ;

	LD	HL,(SCC1_S+1)   ;
	OUT	(#0xA8),A   	;      ;P2+P3をSCC1の基本スロットに切り替え
	LD	A,L
	LD	(#0xFFFF),A	    ;      ;拡張スロット切り替え
	LD	A,H
	OUT	(#0xA8),A	    ;  95  ;P3をRAMに戻す

    JP  _ENA_SCC        ; 106
;=================================
CALL_P2_CHG:
    .globl P2_CHG
    SUB  #4
    LD   B,A
    LD   A,D
    PUSH BC
    CALL P2_CHG          ; 48 + 300
    POP  BC              ; 11
;=================================
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
;=================================

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
__WRITE_SCC1:
    READ_ADRS           ; 61
;=======================
0$:
    READ_DATA           ; 55

    LD  H,#0xB8         ; 
    LD  L,B             ; 
    LD  (HL), D         ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 34 89
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID

;=======================================================
    .ORG 0x6500
__WRITE_SCC:
    READ_ADRS           ; 61
;=======================
0$:
    READ_DATA           ; 55

    LD  H,#0x98         ; 
    LD  L,B             ; 
    LD  (HL), D         ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 34 89
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID

;=======================================================
    .ORG 0x6600
__WRITE_SCC1_2BYTES:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  H,#0xB8         ;  
    LD  L,B             ; 13
;=======================
    WRITE_SCC_2BYTES

;=======================================================
    .ORG 0x6700
__WRITE_SCC_2BYTES:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  H,#0x98         ; 
    LD  L,B             ;  5 59
;=======================
    WRITE_SCC_2BYTES    ; 88

;=======================================================
    .ORG 0x6800
__WRITE_SCC1_32_BYTES:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  H,#0xB8         ; 
    LD  L,B             ; 
    LD  (HL), D         ; 21 76
;=======================
    WRITE_SCC_31_BYTES  ; 94

;=======================================================
    .ORG 0x6900
__WRITE_SCC_32_BYTES:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  H,#0x98         ; 
    LD  L,B             ; 
    LD  (HL), D         ; 21 76
;=======================
    WRITE_SCC_31_BYTES  ; 94

;=======================================================
    .ORG 0x6A00
__WRITE_OPL3_IO1:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPL3AD1),A     ; 17 78
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (OPL3WR),A      ; 17 72

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 13 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID

;=======================================================
    .ORG 0x6B00
__WRITE_OPL3_IO2:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPL3AD2),A     ; 17 78
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (OPL3WR),A      ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 30 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID

;=======================================================
    .ORG 0x6C00
;https://www.msx.org/wiki/MSX-MUSIC_programming
__WRITE_OPLL_MEM:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    LD  (#0x7FF4),A     ; 19 80 
;=======================
    READ_DATA           ; 55
    LD  A,D             ; 
    LD  (#0x7FF5),A     ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 32 87
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
;=======================================================

    .ORG 0x6D00
__SELECT_OPM_SLOT:
    READ_ADRS           ;61
;=======================
    READ_DATA           ;55
    INC D               ;
    DEC D               ;
    JP  NZ,OPM1_P0      ;21 76
;=======================

;OPM0のスロットのPAGE0を表に出す(要DIで実行)
OPM0_P0:
    LD  A,(OPM0_S+3)
    OR  A
    JP  Z,__VGM_LOOP    ;

	LD	HL,(OPM0_S+1)
	OUT	(#0xA8),A	    ;   P0+P3をOPM0の基本スロットに切り替え
	LD	A,L
	LD	(#0xFFFF),A	    ;   拡張スロット切り替え
	LD	A,H
	OUT	(#0xA8),A	    ;   P3をRAMに戻す
    JP __VGM_LOOP       ;111

;OPM1のスロットのPAGE0を表に出す(要DIで実行)
OPM1_P0:
    LD  A,(OPM1_S+3)
    OR  A
    JP  Z,__VGM_LOOP    ;

	LD	HL,(OPM1_S+1)
	OUT	(#0xA8),A	    ;   P0+P3をOPM1の基本スロットに切り替え
	LD	A,L
	LD	(#0xFFFF),A	    ;   拡張スロット切り替え
	LD	A,H
	OUT	(#0xA8),A	    ;   P3をRAMに戻す
    JP __VGM_LOOP       ;111
;=================================

;=======================================================
    .ORG 0x6E00
    ;http://niga2.sytes.net/sp/eseopm.pdf
__WRITE_OPM_MEM:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    LD  (#0x3ff0),A     ; 19 80
;=======================
    READ_DATA           ; 55
    LD  A,D             ; 
    LD  (#0x3ff1),A     ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 32 87
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID

;=======================================================
    .ORG 0x6F00
__WRITE_DCSG:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (DCSGAD),A      ; 

    JP __VGM_LOOP       ; 28 93

;=======================================================
    .ORG 0x7000
__WRITE_OPN2_IO1:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPN2AD1),A     ; 17 78
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (OPN2WR1),A     ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 30 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID

;=======================================================
    .ORG 0x7100
__WRITE_OPN2_IO2:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPN2AD2),A     ; 17 78
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (OPN2WR2),A     ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 30 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #0x60           ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID

;=======================================================
    .ORG 0x7200
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7300
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7400
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7500
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7600
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7700
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7800
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7900
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7A00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7B00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7C00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7D00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7E00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7F00
    JP __VGM_LOOP       ; 10 69

;=======================================================

; LD  A, H
; ADD A, #0x30
; CALL CHPUT

; LD  A, B
; SRL A
; SRL A
; SRL A
; SRL A
; ADD A, #0x30
; CALL CHPUT
; LD  A, B
; AND #0xf
; ADD A, #0x30
; CALL CHPUT

; LD  A, D
; SRL A
; SRL A
; SRL A
; SRL A
; ADD A, #0x30
; CALL CHPUT
; LD  A, D
; AND #0xf
; ADD A, #0x30
; CALL CHPUT

; LD A, #0x20
; CALL CHPUT

; JP  __VGM_LOOP
