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

DCSG_F	= #0xE018		;発見された音源の数
OPLL_F	= #0xE019		;OPLLは bit7:1 = EXT / bit0 = INT
SCC_F	= #0xE01A
OPM_F	= #0xE01B

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
    IN  A,(PSGRD)       ; 11 11
    AND #0x10           ;  8 19*
    JP  NZ, 1$          ; 10 29
    IN  A,(PSGRD)       ; 11 40
    LD  E,A             ;  4 44
    SLA E               ;  8 52
    SLA E               ;  8 60
    SLA E               ;  8 68
    SLA E               ;  8 76
2$:
    ; 2ND DATA Lo 4bit
    IN  A,(PSGRD)       ; 11 11
    AND #0x10           ;  8 19*
    JP  Z, 2$           ; 10 29
    IN  A,(PSGRD)       ; 11 40
    AND #0xf            ;  7 47
    OR  E               ;  4 51

    ;WRITE BOTH 1st BYTE AND 2nd BYTE
    LD  (HL), D         ;  7 58
    INC L               ;  4 62
    LD  (HL), A         ;  7 69
    ;LD  E,#0x30         ;  7 76
    JP  __VGM_LOOP      ; 10 86
    .endm

;=======================================================
    .macro WRITE_SCC_31_BYTES
    LD  B,#31           ;  7  7
1$:
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 11 18
    AND #0x10           ;  8 26*
    JP  NZ,1$           ; 10 36
    IN  A,(PSGRD)       ; 11 47
    LD  D,A             ;  4 51
    SLA D               ;  8 59
    SLA D               ;  8 67
    SLA D               ;  8 75
    SLA D               ;  8 83
2$:
    ; DATA Lo 4bit
    IN  A,(PSGRD)       ; 11 11
    AND #0x10           ;  8 19*
    JP  Z,2$            ; 10 29
    IN  A,(PSGRD)       ; 11 40
    AND #0xf            ;  7 47
    OR  D               ;  4 51

    ;WRITE NEXT BYTE
    INC L               ;  4 55
    LD  (HL), A         ;  7 62
    DJNZ 1$             ; 13 75  8 70
    JP  __VGM_LOOP      ;       10 80
    .endm

;=======================================================
    .macro READ_ADRS
;__VGM_ADRS_HI:
5$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  NZ, 5$          ; 10 28
    IN  A,(PSGRD)       ; 11 39
    LD  B,A             ;  4 43
    SLA B               ;  8 51
    SLA B               ;  8 59
    SLA B               ;  8 67
    SLA B               ;  8 75

;__VGM_ADRS_LO:
6$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x10           ;  7 18
    JP  Z, 6$           ; 10 28
    ; ADDRESS Lo 4bit
    IN  A,(PSGRD)       ; 11 39
    AND #0xf            ;  7 46
    ; ADDRESS 8bit
    OR  B               ;  4 50
    LD  B, A            ;  4 54
    .endm

;=======================================================
    .macro READ_DATA
    LD      H,#0x10     ; 7 81
;__VGM_DATA_HI:
3$:
    IN  A,(PSGRD)       ; 11 11
    AND H               ;  4 15
    JP  NZ, 3$          ; 10 25 __VGM_DATA_HI
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 11 36
    LD  D,A             ;  4 40
    SLA D               ;  8 48
    SLA D               ;  8 56
    SLA D               ;  8 64
    SLA D               ;  8 72

    LD  E,#0xF          ;  7 79
;__VGM_DATA_LO:
4$:
    IN  A,(PSGRD)       ; 11 11
    AND H               ;  4 15
    JP  Z,4$            ; 10 25 __VGM_DATA_LO
    ; DATA Lo 4bit
    IN  A,(PSGRD)       ; 11 36
    AND E               ;  4 40
    ; DATA 8bit
    OR  D               ;  4 44
    LD  D,A             ;  4 48
    .endm

;=======================================================
_uart_processVgm::
    DI
    LD  A,#15        ;  7
    OUT (PSGAD),A    ; 11
    LD  A,#0xCF      ;  7
    OUT (PSGWR),A    ; 11   Set Joy2 Pin Input Mode

    INIT_CONST
    ; JOY2 Pin Read Mode
    LD  A,C             ;  4  4
    OUT (PSGAD),A       ; 11 15

__VGM_LOOP:

__VGM_TYPE:
    IN  A,(PSGRD)       ; 11 25
    AND #0x20           ;  7 32
    JP  Z, __VGM_TYPE   ; 10 42
    IN  A,(PSGRD)       ; 11 53
    AND	#0x1F           ;  7 60
    OR  #0x60           ;  7 67
    LD  H,A             ;  4 71
    LD  L,#0            ;  7 78 Zero clear
    JP  (HL)            ;  4 82

_END_VGM:
    EI                  ;  4
    ret                 ; 10

    .area   _HEADER (ABS)
;=======================================================
    .ORG 0x6000
__WRITE_PSG_IO:
    READ_ADRS
    READ_DATA           ; 48

    LD  A,B             ;  4 52
    OUT (PSGAD),A       ; 11 63
    LD  A,D             ;  4 67
    OUT (PSGWR),A       ; 11 78

    ; JOY2 Pin Read Mode
    LD  A,C             ;  4 82
    OUT (PSGAD),A       ; 11 93
1$:
    IN  A,(PSGRD)       ; 11 25
    AND #0x20           ;  7 32
    JP  Z, 1$           ; 10 42
    IN  A,(PSGRD)       ; 11 53
    AND	#0x1F           ;  7 60
    OR  #0x60           ;  7 67
    LD  H,A             ;  4 71
    LD  L,#0            ;  7 78 Zero clear
    JP  (HL)            ;  4 82

;=======================================================
    .ORG 0x6100
__WRITE_OPLL_IO:
    READ_ADRS
0$:
    LD  A,B             ;  4 62
    OUT (OPLLAD),A      ; 11 73

    READ_DATA           ; 48
    LD  A,D             ;  4 52
    OUT (OPLLWR),A      ; 11 63

    ; Continuous Write
    INC B               ;  4 67
    LD  E,#0x1F         ;  7 74
1$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z, 1$           ; 10 28
    IN  A,(PSGRD)       ; 11 39
    AND	E               ;  4 43
    CP  E               ;  4 47
    JP  Z,0$            ; 12 59    ; Continuous Write

    OR  #0x60           ;  7 66
    LD  H,A             ;  4 70
    LD  L,#0            ;  7 77
    JP  (HL)            ;  4 81    ; Jump to other ID

;=======================================================
    .ORG 0x6200
__SELECT_OPLL_SLOT:
    READ_ADRS
    READ_DATA           ;48
    INC D               ; 4 52
    DEC D               ; 4 56
    JP  NZ,OPLL1_P2     ;10 66
;=================================

;OPLL0のスロットのPAGE2を表に出す(要DIで実行)
OPLL0_P2:
    LD  A,(OPLL0_S)     ;13
    INC A               ; 4
    JP  Z,__VGM_LOOP    ;10

	LD	HL,(OPLL0_S+1)
	LD	A,(OPLL0_S+3)
	OUT	(#0xA8),A	;P2+P3をOPLL0の基本スロットに切り替え
	LD	A,L
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,H
	OUT	(#0xA8),A	;72 ;P3をRAMに戻す
    JP  __VGM_LOOP      ;10 72+10
;OPLL1のスロットのPAGE2を表に出す(要DIで実行)
OPLL1_P2:
    LD  A,(OPLL1_S)     ;13
    INC A               ; 4
    JP  Z,__VGM_LOOP    ;10

	LD	HL,(OPLL1_S+1)
	LD	A,(OPLL1_S+3)
	OUT	(#0xA8),A	;P2+P3をOPLL1の基本スロットに切り替え
	LD	A,L
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,H
	OUT	(#0xA8),A	;72 ;P3をRAMに戻す
    JP  __VGM_LOOP      ;10 72+10
;=================================

;=======================================================
    .ORG 0x6300
    ;https://www.msx.org/forum/msx-talk/software/scc-music-altera-de-1
__SELECT_SCC_SLOT:
    READ_ADRS
    READ_DATA           ;48
    INC D               ; 4 52
    DEC D               ; 4 56
    JP  NZ,SCC1_P2      ;10 66
;=================================

;SCC0のスロットのPAGE2を表に出す(要DIで実行)
SCC0_P2:
    LD  A,(SCC0_S)      ;13
    INC A               ; 4
    JP  Z,__VGM_LOOP    ;10

	LD	HL,(SCC0_S+1)   ;16
	LD	A,(SCC0_S+3)    ;13   
	OUT	(#0xA8),A	    ;11    ;P2+P3をSCC0の基本スロットに切り替え
	LD	A,L             ; 4 
	LD	(#0xFFFF),A	    ;13    ;拡張スロット切り替え
	LD	A,H             ; 4  
	OUT	(#0xA8),A	    ;11 72 ;P3をRAMに戻す

    JP  _ENA_SCC        ;10 82+27
;SCC1のスロットのPAGE2を表に出す(要DIで実行)
SCC1_P2:
    LD  A,(SCC1_S)      ;13
    INC A               ; 4
    JP  Z,__VGM_LOOP    ;10

	LD	HL,(SCC1_S+1)   ;
	LD	A,(SCC1_S+3)
	OUT	(#0xA8),A   	;      ;P2+P3をSCC1の基本スロットに切り替え
	LD	A,L
	LD	(#0xFFFF),A	    ;      ;拡張スロット切り替え
	LD	A,H
	OUT	(#0xA8),A	    ;   72+27 ;P3をRAMに戻す
;=================================

_ENA_SCC:
    LD      A,B          ; 4 
    DEC     A            ; 4 
    JP      Z,__ENA_SCC1 ;10        18
    DEC     A            ; 4 
    JP      Z,__ENA_SCC1_COMPAT ;10 32
    DEC     A            ; 4 
    JP      Z,__ENA_SCC  ;10 
    JP      __VGM_LOOP   ;10

__ENA_SCC1:
    LD      HL,#0xBFFE   ;10
    LD      A,#0x20      ; 7
    LD      (HL),A       ; 7

    LD      HL,#0xB000   ;10
    LD      A,#0x80      ; 7
    LD      (HL),A       ; 7
    JP  __VGM_LOOP       ;10 65+18=83

__ENA_SCC1_COMPAT:       ;32
    LD      HL,#0xBFFE
    LD      A,#0x00
    LD      (HL),A
__ENA_SCC:
    LD      HL,#0x9000
    LD      A,#0x3F
    LD      (HL),A
    JP  __VGM_LOOP       ;10 65+32+7
;=================================

;=======================================================
    .ORG 0x6400
__WRITE_SCC1:
    READ_ADRS
0$:
    READ_DATA           ; 48

    LD  H,#0xB8         ;  7 55
    LD  L,B             ;  4 59
    LD  (HL), D         ;  7 66

    ; Continuous Write
    INC B               ;  4 70
    LD  E,#0x1F         ;  7 77
1$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z, 1$           ; 10 28
    IN  A,(PSGRD)       ; 11 39
    AND	E               ;  4 43
    CP  E               ;  4 47
    JP  Z,0$            ; 12 59    ; Continuous Write

    OR  #0x60           ;  7 66
    LD  H,A             ;  4 70
    LD  L,#0            ;  7 77
    JP  (HL)            ;  4 81    ; Jump to other ID

;=======================================================
    .ORG 0x6500
__WRITE_SCC:
    READ_ADRS
0$:
    READ_DATA           ; 48

    LD  H,#0x98         ;  7 55
    LD  L,B             ;  4 59
    LD  (HL), D         ;  7 66

    ; Continuous Write
    INC B               ;  4 70
    LD  E,#0x1F         ;  7 77
1$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z, 1$           ; 10 28
    IN  A,(PSGRD)       ; 11 39
    AND	E               ;  4 43
    CP  E               ;  4 47
    JP  Z,0$            ; 12 59    ; Continuous Write

    OR  #0x60           ;  7 66
    LD  H,A             ;  4 70
    LD  L,#0            ;  7 77
    JP  (HL)            ;  4 81    ; Jump to other ID

;=======================================================
    .ORG 0x6600
__WRITE_SCC1_2BYTES:
    READ_ADRS
    READ_DATA           ; 48

    LD  H,#0xB8         ;  7 55
    LD  L,B             ;  4 59
    WRITE_SCC_2BYTES

;=======================================================
    .ORG 0x6700
__WRITE_SCC_2BYTES:
    READ_ADRS
    READ_DATA           ; 48

    LD  H,#0x98         ;  7 55
    LD  L,B             ;  4 59
    WRITE_SCC_2BYTES

;=======================================================
    .ORG 0x6800
__WRITE_SCC1_32_BYTES:
    READ_ADRS
    READ_DATA           ; 48

    LD  H,#0xB8         ;  7 55
    LD  L,B             ;  4 59
    LD  (HL), D         ;  7 66
    WRITE_SCC_31_BYTES

;=======================================================
    .ORG 0x6900
__WRITE_SCC_32_BYTES:
    READ_ADRS
    READ_DATA           ; 48

    LD  H,#0x98         ;  7 55
    LD  L,B             ;  4 59
    LD  (HL), D         ;  7 66
    WRITE_SCC_31_BYTES

;=======================================================
    .ORG 0x6A00
__WRITE_OPL3_IO1:
    READ_ADRS
0$:
    LD  A,B             ;  4 62
    OUT (OPL3AD1),A     ; 11 76

    READ_DATA           ; 48

    LD  A,D             ;  4 52
    OUT (OPL3WR),A      ; 11 63

    ; Continuous Write
    INC B               ;  4 67
    LD  E,#0x1F         ;  7 74
1$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z, 1$           ; 10 28
    IN  A,(PSGRD)       ; 11 39
    AND	E               ;  4 43
    CP  E               ;  4 47
    JP  Z,0$            ; 12 59    ; Continuous Write

    OR  #0x60           ;  7 66
    LD  H,A             ;  4 70
    LD  L,#0            ;  7 77
    JP  (HL)            ;  4 81    ; Jump to other ID

;=======================================================
    .ORG 0x6B00
__WRITE_OPL3_IO2:
    READ_ADRS
0$:
    LD  A,B             ;  4 62
    OUT (OPL3AD2),A     ; 11 76

    READ_DATA           ; 48

    LD  A,D             ;  4 52
    OUT (OPL3WR),A      ; 11 63

    ; Continuous Write
    INC B               ;  4 67
    LD  E,#0x1F         ;  7 74
1$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z, 1$           ; 10 28
    IN  A,(PSGRD)       ; 11 39
    AND	E               ;  4 43
    CP  E               ;  4 47
    JP  Z,0$            ; 12 59    ; Continuous Write

    OR  #0x60           ;  7 66
    LD  H,A             ;  4 70
    LD  L,#0            ;  7 77
    JP  (HL)            ;  4 81    ; Jump to other ID

;=======================================================
    .ORG 0x6C00
;https://www.msx.org/wiki/MSX-MUSIC_programming
__WRITE_OPLL_MEM:
    READ_ADRS
0$:
    LD  A,B             ;  4 62
    LD  (#0x7FF4),A     ; 13 75

    READ_DATA           ; 48
    LD  A,D             ;  4 52
    LD  (#0x7FF5),A     ; 13 65

    ; Continuous Write
    INC B               ;  4 69
    LD  E,#0x1F         ;  7 76
1$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z, 1$           ; 10 28
    IN  A,(PSGRD)       ; 11 39
    AND	E               ;  4 43
    CP  E               ;  4 47
    JP  Z,0$            ; 12 59    ; Continuous Write

    OR  #0x60           ;  7 66
    LD  H,A             ;  4 70
    LD  L,#0            ;  7 77
    JP  (HL)            ;  4 81    ; Jump to other ID
;=======================================================

    .ORG 0x6D00
__SELECT_OPM_SLOT:
    READ_ADRS
    READ_DATA           ;48
    INC D               ; 4 52
    DEC D               ; 4 56
    JP  NZ,OPM1_P0      ;10 66
;=================================

;OPM0のスロットのPAGE0を表に出す(要DIで実行)
OPM0_P0:
    LD  A,(OPM0_S)      ;13
    INC A               ; 4
    JP  Z,__VGM_LOOP    ;10

	LD	BC,(OPM0_S+1)
	LD	A,(OPM0_S+3)
	OUT	(#0xA8),A	;P0+P3をOPM0の基本スロットに切り替え
	LD	A,C
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,B
	OUT	(#0xA8),A	;P3をRAMに戻す
    JP __VGM_LOOP       ; 10 69

;OPM1のスロットのPAGE0を表に出す(要DIで実行)
OPM1_P0:
    LD  A,(OPM1_S)      ;13
    INC A               ; 4
    JP  Z,__VGM_LOOP    ;10

	LD	BC,(OPM1_S+1)
	LD	A,(OPM1_S+3)
	OUT	(#0xA8),A	;P0+P3をOPM1の基本スロットに切り替え
	LD	A,C
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,B
	OUT	(#0xA8),A	;P3をRAMに戻す
    JP __VGM_LOOP       ; 10 69
;=================================

;=======================================================
    .ORG 0x6E00
    ;http://niga2.sytes.net/sp/eseopm.pdf
__WRITE_OPM_MEM:
    READ_ADRS
0$:
    LD  A,B             ;  4 62
    LD  (#0x3ff0),A     ; 13 75

    READ_DATA           ; 48
    LD  A,D             ;  4 52
    LD  (#0x3ff1),A     ; 13 65

    ; Continuous Write
    INC B               ;  4 69
    LD  E,#0x1F         ;  7 76
1$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z, 1$           ; 10 28
    IN  A,(PSGRD)       ; 11 39
    AND	E               ;  4 43
    CP  E               ;  4 47
    JP  Z,0$            ; 12 59    ; Continuous Write

    OR  #0x60           ;  7 66
    LD  H,A             ;  4 70
    LD  L,#0            ;  7 77
    JP  (HL)            ;  4 81    ; Jump to other ID

;=======================================================
    .ORG 0x6F00
__WRITE_DCSG:
    READ_ADRS
    READ_DATA

    LD  A,B             ;  4 52
    OUT (DCSGAD),A      ; 11 63

    JP __VGM_LOOP       ; 10 73

;=======================================================
    .ORG 0x7000
__WRITE_OPN2_IO1:
    READ_ADRS
0$:
    LD  A,B             ;  4 62
    OUT (OPN2AD1),A     ; 11 76

    READ_DATA           ; 48

    LD  A,D             ;  4 52
    OUT (OPN2WR1),A     ; 11 63

    ; Continuous Write
    INC B               ;  4 67
    LD  E,#0x1F         ;  7 74
1$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z, 1$           ; 10 28
    IN  A,(PSGRD)       ; 11 39
    AND	E               ;  4 43
    CP  E               ;  4 47
    JP  Z,0$            ; 12 59    ; Continuous Write

    OR  #0x60           ;  7 66
    LD  H,A             ;  4 70
    LD  L,#0            ;  7 77
    JP  (HL)            ;  4 81    ; Jump to other ID

;=======================================================
    .ORG 0x7100
__WRITE_OPN2_IO2:
    READ_ADRS
0$:
    LD  A,B             ;  4 62
    OUT (OPN2AD2),A     ; 11 76

    READ_DATA           ; 48

    LD  A,D             ;  4 52
    OUT (OPN2WR2),A     ; 11 63

    ; Continuous Write
    INC B               ;  4 67
    LD  E,#0x1F         ;  7 74
1$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z, 1$           ; 10 28
    IN  A,(PSGRD)       ; 11 39
    AND	E               ;  4 43
    CP  E               ;  4 47
    JP  Z,0$            ; 12 59    ; Continuous Write

    OR  #0x60           ;  7 66
    LD  H,A             ;  4 70
    LD  L,#0            ;  7 77
    JP  (HL)            ;  4 81    ; Jump to other ID

;=======================================================
    .ORG 0x7200
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7300
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7400
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7500
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7600
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7700
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7800
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7900
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7A00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7B00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7C00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7D00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7E00
    JP __VGM_LOOP       ; 10 69

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
