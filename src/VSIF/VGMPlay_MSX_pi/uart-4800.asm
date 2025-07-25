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


; port 0x40に0x69を書くと有効、0x69以外を書くと無効になります。
; 起動・リセット時の初期状態は無効状態です。

; 無効状態時
; port 0x40(write)	0x69を書き込むと有効、0x69以外の書込みは無視(無効のまま)

; 有効状態時
; port 0x40(write)	0x69以外の書込みで無効、0x69の書込みは無視(有効のまま)
; port 0x40(read)		0x96(0x69のビット反転値)を返す。これによりボードの有無を判定できます。
; port 0x41(write)	i8151設定(無視します・USB接続なので設定は無意味)
; port 0x41(read)		i8151状態を返す
; port 0x42(write)	i8151書込み
; port 0x42(read)		i8151読み出し

; i8151状態
; D7      D6      D5      D4      D3      D2      D1      D0
; 1固定   0固定   0固定   0固定   0固定   TXEMPTY RXRDY   TXRDY

; TXEMPTY送信バッファが空で1
; RXRDY受信データがあると1
; TXRDY送信可能(送信バッファに空きがある)で1

; 送信(受信)は必ずTXRDY(RXRDY)が1である事を確認してから行なう事

.INCLUDE "macro.inc"

SUPPORT_Z80B = 0

EXPTBL = 0xFCC1
VER_NO = 0x002D
CHGCPU = 0x0180
RDSLT  = 0x000C
CALSLT = 0x001C

JPOFST = 0x60

    .globl STRPUT

    .area   _HEADER (ABS)
;=======================================================
    .ORG 0x5000

STR_TR:	            .ascii	"\r\n*turboR mode$"
STR_WX_Z80B:	    .ascii	"\r\n*WX Z80B mode$"

_uart_processVgm::
	; LD	A,#'0'
 	; CALL	CHPUT

; Check turboR and set turboR mode
    LD	    A,(EXPTBL)	;Get version from EXPTBL
	LD	    HL,#VER_NO  ;VER_NO
	CALL	RDSLT   ;RDSLT
	CP	    #3
	JP	    C,chk_CKey	; VER<3 is not tR

chk_ZKey:
	IN      A,(#0xAA)	    ;Read PPI reg C
	AND     #0xF0		    ;Select KB Row (bit=0000)
	OR      #5		        ;Set KB Matrix 5 for "Z" key
	OUT     (#0xAA),A	
	NOP
	IN	    A,(#0xA9)	    ;Read KB Matrix
	AND     #0b10000000     ;Check "Z" key (bit0)
	JP      Z,chk_CKey     ;If "Z" key pressed, skip.

tR_mode:
    LD      A, #0x82    ;Change CPU
	LD	    IX,#CHGCPU  ;CHGCPU
	LD	    IY,(EXPTBL)
	CALL	CALSLT  ;CALSLT

    ;PCM MUTE=OFF,ADDABUFF=SINGLE
	LD	    A,#3
    OUT     (#0xA5),A

    LD      A,#127
    OUT     (TRDAC),A

	LD	    HL,#STR_TR
	CALL	STRPUT
    JP      endCheckCPU

chk_CKey:
.if SUPPORT_Z80B
	IN      A,(#0xAA)	    ;Read PPI reg C
	AND     #0xF0		    ;Select KB Row (bit=0000)
	OR      #3		        ;Set KB Matrix 3 for "C" key
	OUT     (#0xAA),A	
	NOP
	IN	    A,(#0xA9)	    ;Read KB Matrix
	AND     #0b00000001     ;Check "C" key (bit0)
	JP      NZ,endCheckCPU  ;If "C" key does not pressed, skip.

chk_WX:
	LD	    A,#8 
	OUT	    (0x40),A
	IN	    A,(0x40)
	CPL
	CP	    #8
	JP	    NZ,endCheckCPU  ;Not exists WX hi-speed clock

Z80B_mode:
	LD	    A,#8		    ;Set WX hi-speed clock
	OUT	    (0x40),A
	XOR	    A
	OUT	    (0x41),A

	LD	    HL,#STR_WX_Z80B
	CALL	STRPUT
.endif
endCheckCPU:

    DI
    LD  E,#2

__VGM_LOOP:
__VGM_TYPE:
    READ_DATA
;     IN  A,(UART_STAT)   ; 12
;     IN  A,(UART_STAT)   ; 12
; ; CP  #50
; ; JP  Z, _TEST
;     AND E               ;  5
;     JP  Z, __VGM_TYPE   ; 11
;     IN  A,(UART_READ)   ; 12
; ; IN  A,(UART_STAT)   ; 12
; ; JP  __VGM_TYPE  ; 11
    OR  #JPOFST         ;  8
    LD  H,A             ;  5
    LD  L,#0            ;  8
    JP  (HL)            ;  5 77

_TEST:
 	CALL	CHPUT
    JP  __VGM_TYPE  ; 11

_END_VGM2:
	LD	A,#'E'
 	CALL	CHPUT

_END_VGM:
    JP  _END_VGM 

    _P2_CHG

TEST_CHG_P2:
    LD   A,(ROM2_S)
    LD   HL,#0x8000
    CALL    ENASLT
    .globl _uart_processVgm_P2
    JP  _uart_processVgm_P2  ; 11

;=======================================================
    .ORG 0x6000
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x6100
    __WRITE_OPLL_IO

;=======================================================
    .ORG 0x6200
__SELECT_OPLL_SLOT:
    READ_ADRS           ;61
    READ_DATA           ;50
    LD  D,A             ; 5 55
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
    READ_DATA           ;50
    LD  D,A             ; 5 55
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
    __WRITE_OPN_IO

;=======================================================
    .ORG 0x7300
    __WRITE_OPNA_PSEUDO_DAC

;=======================================================
    .ORG 0x7400
    __WRITE_OPN2_DAC

;=======================================================
    .ORG 0x7500
    __WRITE_TR_DAC

;=======================================================
    .ORG 0x7600
    __WRITE_OPNA_DAC

;=======================================================
    .ORG 0x7700
    __WRITE_PSG_IO

;=======================================================
    .ORG 0x7800
    .globl __SELECT_SIOS_SLOT_P2
__SELECT_SIOS_SLOT:
    READ_ADRS
    READ_DATA
__SELECT_SIOS_SLOT_P2:
    LD   A,(NTRON0_S)
    CALL P2_CHG         ; A = Page2に割り当てるスロット(0b0000_0000 ～ 0b1000_1111)
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7900
__SIOS_WRITE_PCM_DATA1:
    READ_ADRS           ; B:Memory ID(1:ADPCM-A, 2:ADPCM-B)
    READ_DATA           ; A:Memory Address Hi[23..16]
    LD      D, A
__SIOS_WRITE_PCM_DATA2:
    READ_DATA           ; A:Memory Address Lo[15..8]
    LD      E, A

;    OR A    ; reset carry flag
;    CALL    SIOS_ENABLE_IO_10 ; enable register access

    LD      A,B        ;Memory ID(1:ADPCM-A, 2:ADPCM-B)
;   LD      DE, 0x1000 ; address to access = 0x1000
    CALL    SIOS_SET_SMEM_10 ; set access address

__SIOS_ERASE_SIOS_MEM:
    PUSH DE   ; Save Memory Address 
    PUSH BC   ; Save B(Memory ID)
        LD  E,#2
        READ_DATA
        AND #1
        JP  Z, __SIOS_WRITE_PCM_DATA3 ; if bit0=1, erase SIOS memory
        LD      A,B  ; Memory ID(1:ADPCM-A, 2:ADPCM-B)
        LD      B,#1 ; B = size of data to write (B*64K bytes) 
        CALL    SIOS_ERASE_SMEM_10 ; erase SIOS memory
        ; ACK
        ;LD  A, #0xFF
        ;OUT (UART_STAT),A
        __SIOS_WRITE_PCM_DATA3:

        ; Receive PCM data
        LD  E,#2

        LD	HL,#NTRON_PCM_BUF
        LD  B,#0x00 ; B = size of data to write (0x00 means 256 bytes)
        __SIOS_WRITE_PCM_DATA4:
            READ_DATA           ; A:PCM DATA
            LD  (HL), A
            INC L
        DJNZ __SIOS_WRITE_PCM_DATA4
    POP BC   ; Restore B(Memory ID)
    POP DE   ; Restore Memory Address 

    LD      A,B    ;Memory ID(1:ADPCM-A, 2:ADPCM-B)
;   LD      DE, 0x1000 ; address to access = 0x1000
    CALL    SIOS_SET_SMEM_10 ; set access address

    ; Tranfer PCM data to SIOS memory
    LD      A,B    ;Memory ID(1:ADPCM-A, 2:ADPCM-B)
    LD      HL,#NTRON_PCM_BUF ; HL = source address
    LD      BC,#0x0100 ; BC = size of data to write (256 bytes)
    CALL    SIOS_WRITE_SMEM_10

    CALL    SIOS_RESET_10 ; enable register access

    ;AND A   ; reset carry flag
    ;CALL    SIOS_ENABLE_IO_10 ; enable register access

    LD  E,#2
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7A00
__WRITE_OPNB_IO1:
    READ_ADRS           ; 45
0$:
    LD  A,B             ; 
    LD (#0xBC00),A     ; 17 78
;=======================
    READ_DATA           ; 40

    LD (#0xBC01),A     ; 12 62
    ; LD  D,A
    ; LD  A,B
    ; CALL    SIOS_WRITE_REG0_10

    INC B               ;  5 67
    CONT_WRITE

;=======================================================
    .ORG 0x7B00
__WRITE_OPNB_IO2:
    READ_ADRS           ; 45
0$:
    LD  A,B             ; 
    LD (#0xBC02),A     ; 17 78
;=======================
    READ_DATA           ; 40

    LD (#0xBC03),A     ; 12 62
    ; LD  D,A
    ; LD  A,B
    ; CALL    SIOS_WRITE_REG1_10

    INC B               ;  5 67
    CONT_WRITE

;=======================================================
    .ORG 0x7C00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7D00
    JP __VGM_LOOP       ; 

;=======================================================
    .ORG 0x7E00
    __WRITE_ANY_IO

;=======================================================
    .ORG 0x7F00
    JP __VGM_LOOP       ; 

;=======================================================
