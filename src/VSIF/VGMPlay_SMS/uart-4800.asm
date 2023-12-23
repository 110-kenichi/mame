
;
; Rule number one of embedded development is "make a LED" blink. Rule number
; two should be "Now get a serial port going". This file aims to do just that
; on the SEGA master system.
;
; Peripheral ports have two selectable direction pins readable through ports
; DDh DCh with the added bonus that TH on port 2 is the high order bit of the
; port register, allowing an easy shift-out of the bit through the carry flag
;
; Now, down to business... We want to:
;   - Sample the TH line until a low level is detected.
;   - Sample the start bit multiple times for reliable framing.
;   - Sample TH line at regular intervals for the data bits.
;   - Make result available.
;   - Prepare for the next bit.
;   - Being able to do this 130 times in a row because... XMODEM ;)
;   - Get to 4800bps or, as the bare minimum, 1200bps
;
; The width of a single bit in T-States:
;   300bps:     11822 T-PAL  11931 T-NTSC  11877 T-AVG
;   600bps:      5911 T-PAL   5965 T-NTSC   5938 T-AVG
;  1200bps:      2955 T-PAL   2982 T-NTSC   2969 T-AVG
;  2400bps:      1477 T-PAL   1491 T-NTSC   1484 T-AVG
;  4800bps:       738 T-PAL    745 T-NTSC    742 T-AVG
;  9600bps:       369 T-PAL    372 T-NTSC    371 T-AVG
;
; UART:
; _______     ___ ___ ___ ___ ___ ___ ___ ___________
;        |   |   |   |   |   |   |   |   |   |
;        |   |   |   |   |   |   |   |   |   |
;        |STA| 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 |STO
;        |   |   |   |   |   |   |   |   |   |
;        |___|___|___|___|___|___|___|___|___|
;
; Start bit (4800bps):
; __________                                                            __
;           |<-------------------  ~742 T-States  -------------------->|
;        -->| |<-- 36T                                                 |
;           | |<--------------------  706 T-States  ------------------>|
;           |>|   |<-- 46T                                             |
;           |                                                          |
;           |     S     S     S     S     S     S     S     S          |
;           |<82T>|<82T>|<82T>|<82T>|<82T>|<82T>|<82T>|<82T>|<82T> <4T>|
;           |     |     |     |     |     |     |     |     |          |
;           |_____|_____|_____|_____|_____|_____|_____|_____|__________|
;
;
; Data bit (4800bps):
; __________                                                            __
;           |<-------------------  ~742 T-States  -------------------->|
;           |                                                          |
;           |                            S                             |
;           |<--------- 371T ----------->|<---------- 371T ----------->|
;           |                            |                             |
;           |____________________________|_____________________________|
;
;
; Sending a bit should be easier. We use port 0x3F to set the TR bit on port 2
; at regular intervals.
;
; Setting the port bit would be:
;   PORT 0x3f <-- 0b01001011 (0x4B)
;
; Resetting the port would be:
;   PORT 0x3f <-- 0b00001011 (0x0B)
;        
;

; --------------------------
; --- Values from uart.h ---
; --------------------------

UART_STATUS_OK  = 0x00
UART_STATUS_NOK = 0xFF

PERIPHERAL_PORT  = 0xDD
PRESAMPLE_DELAY  = 26; 24   ; Original value 26  ; Tune these two to select the bit width and
POSTSAMPLE_DELAY = 23   ; Original value 24  ; where in the bit the sample is made.
UART_VALID_START = 0x00 ; What the sampled start bit looks like if correctly sampled.

UART_DOWN = 0xBB
UART_UP   = 0xFB
IO_PORT   = 0x3F


; データ受信＆書き込み
; ・エラーチェックなし
; ・スタート、ストップ１ビット
; ・パリティなし
; ・ボーレート 115200bps（１ビット 30.79clk@3.5466MHz PAL）
; ・ボーレート 115200bps（１ビット 31.07clk@3.5793MHz NTSC）
; ・ボーレート  57600bps（１ビット 61.58clk@3.547MHz PAL） 53.203MHz/15
; ・ボーレート  57600bps（１ビット 62.14clk@3.580MHz NTSC）53.693MHz/15
;
; 設計思想
; ・スタート、ストップとも１ビットのため、全体で１０ビット
;
; ・１ビットのデータは、31/32クロック弱
;
; ・１０ビット全体
; 　　スタートビットをすぐに見つける
; 　　８ビットデータを取り込む
; 　　データ格納、サイズチェックをして最初に戻る
; 　までを、307クロック以内に終了させる（10ビットでデータ全体が 307.9/310.7clk のため）
;
; ・スタートビット検出と、最初のデータ取り込みのタイミングは
; 　32クロック以上にする
; 　（31クロックにした場合、スタートビットがぎりぎり取れた時に
; 　　最初のビットを取り損なう可能性があるため）

.macro start_bit_wait_57K ?wait1, ?wait2, ?wait3

    JP wait1                    ; 10T(42)
wait1:
    JP wait2                    ; 10T(52)
wait2:
    JP wait3                    ; 10T(62)
wait3:

.endm

.macro sample_bit_114K_nowait
    in	A,(#PERIPHERAL_PORT)   ; 11T(11)	受信ビット（bit7）Port B TH pin7
    RRA                        ;  4T(15)
    RR   D                     ;  8T(23)
.endm

.macro sample_bit_114K
    in	A,(#PERIPHERAL_PORT)   ; 11T(11)	受信ビット（bit7）Port B TH pin7
    RRA                        ;  4T(15)
    RR   D                     ;  8T(23)
    nop		                   ;  4T(31)	時間調整用
    nop		                   ;  4T(27)	時間調整用
.endm

.macro sample_bit_57k_nowait
    PUSH IX                    ; 15T(48)
    POP  IX                    ; 14T(62)

    in	A,(#PERIPHERAL_PORT)   ; 11T(11)	受信ビット（bit7）Port B TH pin7
    RRA                        ;  4T(15)
    RR   D                     ;  8T(23)
.endm

.macro sample_bit_57k ?wait
    PUSH IX                    ; 15T(48)
    POP  IX                    ; 14T(62)

    in	A,(#PERIPHERAL_PORT)   ; 11T(11)	受信ビット（bit7）Port B TH pin7
    RRA                        ;  4T(15)
    RR   D                     ;  8T(23)

    JP wait                    ; 10T(33)
wait:
.endm

.macro sendbit_wait_115K    ; 19T
    NOP                ;  4T(23)     時間調整用
    NOP                ;  4T(27)     時間調整用
    NOP                ;  4T(31)     時間調整用
.endm   ; (31)

.macro sendbit_wait_57K  ?wait    ; 19T
    NOP                ;  4T(23)     時間調整用
    JP wait            ; 10T(33)
wait:
    PUSH IX            ; 15T(48)
    POP  IX            ; 14T(62)
.endm   ; (52)

.macro sendbit_wait
    sendbit_wait_115K
    ;sendbit_wait_57K
.endm

.macro start_bit_wait 
    ; no wait for 114K
    ;start_bit_wait_57K
.endm

.macro sample_bit_nowait
    sample_bit_114K
    ;sample_bit_57k
    sample_bit_114K_nowait
    ;sample_bit_57k_nowait
.endm

.macro sample_bit
    sample_bit_114K
    sample_bit_114K
    ;sample_bit_57k
    ;sample_bit_57k
.endm

; void uart_processVgm()
_uart_processVgm::
    DI
__VGM_LOOP:
    LD  C, #PERIPHERAL_PORT    ; 11T(11)
__VGM_ADDRESS:
    IN  A, (#PERIPHERAL_PORT)  ; 11T(11)
    RRA                        ;  4T(15)
    JP	C,__VGM_ADDRESS	       ; 10T(25)	スタートビットを待つ
    LD	D,#00		           ;  7T(32)
    start_bit_wait

    sample_bit  ;(31x2)
    sample_bit  ;(31x2)
    sample_bit  ;(31x2)
    sample_bit_nowait

    ;ストップビットの間に処理を済ませる
    ; ・ボーレート 115200bps（１ビット 30.79clk@3.5466MHz PAL）
    ; ・ボーレート 115200bps（１ビット 31.07clk@3.5793MHz NTSC）
    ; ・ボーレート  57600bps（１ビット 61.58clk@3.547MHz PAL） 53.203MHz/15
    ; ・ボーレート  57600bps（１ビット 62.14clk@3.580MHz NTSC）53.693MHz/15

    ;LD  A, #0xFF               ;  7T(7)
    ;CP  D                      ;  4T(11)
    ;JP Z,_END_VGM              ; 12T(23)

    ; 72以内@57600bps(1stopbit) / 115200bps(2stopbit)
    LD  E, D
    XOR A   ; clr
    INC D   ; 0xFF = PSG
    JP  Z, __VGM_PSG_EN
__VGM_OPLL_EN:
    DEC D
    LD  C, #0xf0             ; 7T
    OUT (C), D               ; 12T(19)  SET A0
    LD  A, #0x1              ;ENABLE OPLL
__VGM_PSG_EN:
    OUT (#0xf2), A           ;ENABLE PSG

VGM_DATA:
    IN  A, (#PERIPHERAL_PORT)  ; 11T(11)
    RRA                        ;  4T(15)
    JP	C,VGM_DATA  	       ; 10T(25)	スタートビットを待つ
    LD	D,#00		           ;  7T(32)
    start_bit_wait

    sample_bit  ;(31x2)
    sample_bit  ;(31x2)
    sample_bit  ;(31x2)
    sample_bit_nowait

    ;ストップビットの間に処理を済ませる
    ; ・ボーレート 115200bps（１ビット 30.79clk@3.5466MHz PAL）
    ; ・ボーレート 115200bps（１ビット 31.07clk@3.5793MHz NTSC）
    ; ・ボーレート  57600bps（１ビット 61.58clk@3.547MHz PAL） 53.203MHz/15
    ; ・ボーレート  57600bps（１ビット 62.14clk@3.580MHz NTSC）53.693MHz/15

    ; 72以内@57600bps(1stopbit) / 115200bps(2stopbit)
    ;Play Data
  	INC E   ; 0xFF = PSG
    JP  Z, __VGM_PSG
__VGM_OPLL:
    LD  C, #0xf1             ; 7T
    OUT (C), D               ; 12T(19)  SET A1
    JP __VGM_LOOP           ; 10T(29)
__VGM_PSG:
    LD  C, #0x7f             ; 7T
    OUT (C), D               ; 12T(19)
    JP __VGM_LOOP           ; 10T(29)

_END_VGM:
    EI
    ret                      ; 10T(30)
