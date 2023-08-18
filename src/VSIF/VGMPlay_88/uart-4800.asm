INCLUDE "const.inc"

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

INCLUDE "macro.inc"

JPOFST equ $10

XDEF _uart_processVgm

;=======================================================
;    SECTION S_VGM
_uart_processVgm:
    DI

    LD  A,$0
    ;OUT ($51),A         ; Stop DMA (for pre SR machine)
    OUT	($E6),A          ; Disable interrupts
    OUT	($E4),A          ; Disable interrupts

    TRANSFER_PROC $1000,S_X000,E_X000
    TRANSFER_PROC $1100,S_X100,E_X100
    TRANSFER_PROC $1200,S_X200,E_X200
    TRANSFER_PROC $1300,S_X300,E_X300
    TRANSFER_PROC $1400,S_X400,E_X400
    TRANSFER_PROC $1500,S_X500,E_X500
    TRANSFER_PROC $1600,S_X600,E_X600
    TRANSFER_PROC $1700,S_X700,E_X700
    TRANSFER_PROC $1800,S_X800,E_X800
    TRANSFER_PROC $1900,S_X900,E_X900
    TRANSFER_PROC $1A00,S_XA00,E_XA00
    TRANSFER_PROC $1B00,S_XB00,E_XB00
    TRANSFER_PROC $1C00,S_XC00,E_XC00
    TRANSFER_PROC $1D00,S_XD00,E_XD00
    TRANSFER_PROC $1E00,S_XE00,E_XE00
    TRANSFER_PROC $1F00,S_XF00,E_XF00

    LD  A,$07           ;  8
    OUT (OPNAAD1),A     ; 17 78
    LD  A,$3F           ; 
    OUT (OPNAWR1),A     ; Set Joy Pin Input Mode & SSG OFF

    LD  C,$0E           ;  8
    LD  E,$0F           ;  8

__VGM_LOOP:
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
__VGM_TYPE:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  Z, __VGM_TYPE   ; 11

    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 17 78
    IN  A,(OPNAWR1)     ; 12
    AND	E               ;  5
    OR  JPOFST          ;  8
    LD  H,A             ;  5
    LD  L,0             ;  8
    JP  (HL)            ;  5 77

_END_VGM:
    JP  _END_VGM 

S_X000:
BINARY "VGM_88_S_X000.bin"
E_X000:
S_X100:
BINARY "VGM_88_S_X100.bin"
E_X100:
S_X200:
BINARY "VGM_88_S_X200.bin"
E_X200:
S_X300:
BINARY "VGM_88_S_X300.bin"
E_X300:
S_X400:
BINARY "VGM_88_S_X400.bin"
E_X400:
S_X500:
BINARY "VGM_88_S_X500.bin"
E_X500:
S_X600:
BINARY "VGM_88_S_X600.bin"
E_X600:
S_X700:
BINARY "VGM_88_S_X700.bin"
E_X700:
S_X800:
BINARY "VGM_88_S_X800.bin"
E_X800:
S_X900:
BINARY "VGM_88_S_X900.bin"
E_X900:
S_XA00:
BINARY "VGM_88_S_XA00.bin"
E_XA00:
S_XB00:
BINARY "VGM_88_S_XB00.bin"
E_XB00:
S_XC00:
BINARY "VGM_88_S_XC00.bin"
E_XC00:
S_XD00:
BINARY "VGM_88_S_XD00.bin"
E_XD00:
S_XE00:
BINARY "VGM_88_S_XE00.bin"
E_XE00:
S_XF00:
BINARY "VGM_88_S_XF00.bin"
E_XF00:

;=======================================================
    SECTION S_X000
    ORG $1000
    JP __VGM_LOOP       ; 

;=======================================================
    SECTION S_X100
    ORG $1100
    __WRITE_OPNA_IO1

;=======================================================
    SECTION S_X200
    ORG $1200
    __WRITE_OPNA_IO2

;=======================================================
    SECTION S_X300
    ORG $1300
    __WRITE_OPNA_DAC

;=======================================================
    SECTION S_X400
    ORG $1400
    JP __VGM_LOOP       ; 

;=======================================================
    SECTION S_X500
    ORG $1500
    __WRITE_SB2_IO1

;=======================================================
    SECTION S_X600
    ORG $1600
    __WRITE_SB2_IO2

;=======================================================
    SECTION S_X700
    ORG $1700
    __WRITE_SB2_DAC

;=======================================================
    SECTION S_X800
    ORG $1800
    JP __VGM_LOOP       ; 

;=======================================================
    SECTION S_X900
    ORG $1900
    JP __VGM_LOOP       ; 

;=======================================================
    SECTION S_XA00
    ORG $1A00
    JP __VGM_LOOP       ; 

;=======================================================
    SECTION S_XB00
    ORG $1B00
    JP __VGM_LOOP       ; 

;=======================================================
    SECTION S_XC00
    ORG 0x6C00
    JP __VGM_LOOP       ; 

;=======================================================
    SECTION S_XD00
    ORG $1D00
    JP __VGM_LOOP       ; 

;=======================================================
    SECTION S_XE00
    ORG $1E00
    JP __VGM_LOOP       ; 

;=======================================================
    SECTION S_XF00
    ORG $1F00
    JP __VGM_LOOP       ; 	Continuous Write

;=======================================================
