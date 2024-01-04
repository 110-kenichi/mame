
; Physical port configuration
; The port is a standard DE-9 (commonly mis-titled "DB9") male connector, accepting a female controller plug. Viewed from the front of the port:

;       1       5
;       o o o o o
;        o o o o
;        6     9

; Pin number	Function
; 1	Up
; 2	Down
; 3	Left
; 4	Right
; 5	VCC
; 6	TL
; 7	TH (Gnd on Mark III)
; 8	Gnd
; 9	TR

; Port $DC: I/O port A and B
; Bit	Function
; 7	Port B Down pin input
; 6	Port B Up pin input
; 5	Port A TR pin input
; 4	Port A TL pin input
; 3	Port A Right pin input
; 2	Port A Left pin input
; 1	Port A Down pin input
; 0	Port A Up pin input

; (1= not pressed, 0= pressed)


; Port $DD: I/O port B and miscellaneous
; Bit	Function
; 7	Port B TH pin input
; 6	Port A TH pin input
; 5	Cartridge slot CONT pin *
; 4	Reset button *
; 3	Port B TR pin input
; 2	Port B TL pin input
; 1	Port B Right pin input
; 0	Port B Left pin input

; (1= not pressed, 0= pressed)

PORT_A  = 0xDC
PORT_B  = 0xDD

    .macro READ_ADRS
;__VGM_ADRS_HI:
5$:
    IN  A,(PORT_A)          ; 11
    AND #0x80               ;  7    Down
    JP  Z, 5$               ; 10
    IN  A,(PORT_B)          ; 11    TR,TL,R,L
    LD  B,A                 ;  4
    SLA B                   ;  8
    SLA B                   ;  8
    SLA B                   ;  8    +67
    SLA B                   ;  8    +75

;__VGM_ADRS_LO:
6$:
    IN  A,(PORT_A)          ; 11
    AND H                   ;  4    Up
    JP  NZ, 6$              ; 10
    IN  A,(PORT_B)          ; 11
    AND C                   ;  4
    ; ADDRESS 8bit
    OR  B                   ;  4    +44
    .endm

    .macro READ_DATA
;__VGM_DATA_HI:
3$:
    IN  A,(PORT_A)          ; 11
    AND H                   ;  4    UP
    JP  Z, 3$               ; 10
    ; DATA Hi 4bit
    IN  A,(PORT_B)          ; 11
    LD  D,A                 ;  4
    SLA D                   ;  8
    SLA D                   ;  8
    SLA D                   ;  8
    SLA D                   ;  8

;__VGM_DATA_LO:
4$:
    IN  A,(PORT_A)          ; 11
    AND H                   ;  4    UP
    JP  NZ,4$               ; 10
    ; DATA Lo 4bit
    IN  A,(PORT_B)          ; 11
    AND C                   ;  4
    ; DATA 8bit
    OR  D                   ;  4    +44
    .endm

; void uart_processVgm_FTDI()
_uart_processVgm_FTDI::
    DI
    LD  H,#0x40             ; for Up Button
    LD  L,#0x01             ; for EN
    LD  C,#0x0F             ; for Lo bit
__VGM_LOOP:
    READ_ADRS               ; 44
    JP  Z, __VGM_PSG        ; 10 0x00 = PSG

__VGM_OPLL:
    DEC A                   ;  4 58
    OUT (#0xF0), A          ; 12 70 WRITE OPLL ADRS

    LD  A, L                ;  4 74
    OUT (#0xF2), A          ; 11 85 ENABLE OPLL

    READ_DATA               ; 44
    OUT (#0xF1), A          ; 12 56 WRITE OPLL DATA
    JP __VGM_LOOP           ; 10 66

__VGM_PSG:
    XOR A                   ;  4 Clr
    OUT (#0xF2), A          ; 11 ENABLE PSG

    READ_DATA               ; 47
    OUT (#0x7f), A          ; 11
    JP __VGM_LOOP           ; 10    +68

; void uart_processVgm_FTDI()
_uart_processVgm_FTDI_SMS::
    DI
    LD  H,#0x40             ; for Up Button
    LD  L,#0x01             ; for EN
    LD  C,#0x0F             ; for Lo bit
__VGM_LOOP_SMS:
    READ_ADRS               ; 44
    JP  Z, __VGM_PSG_SMS    ; 10 0x00 = PSG

__VGM_OPLL_SMS:
    DEC A                   ;  4 58
    OUT (#0xF0), A          ; 12 70 WRITE OPLL ADRS

    READ_DATA               ; 44
    OUT (#0xF1), A          ; 12 56 WRITE OPLL DATA
    JP __VGM_LOOP_SMS       ; 10 66

__VGM_PSG_SMS:
    READ_DATA               ; 47
    OUT (#0x7f), A          ; 11
    JP __VGM_LOOP_SMS       ; 10    +68
