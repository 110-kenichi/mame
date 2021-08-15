PSGAD = #0xA0
PSGWR = #0xA1
PSGRD = #0xA2

OPLLAD = #0x7C
OPLLWR = #0x7D

CHPUT = #0xA2

; ・ボーレート 115200bps（１ビット 30.79clk@3.5466MHz PAL）
; ・ボーレート 115200bps（１ビット 31.07clk@3.5793MHz NTSC）
; ・ボーレート  57600bps（１ビット 61.58clk@3.547MHz PAL） 53.203MHz/15
; ・ボーレート  57600bps（１ビット 62.14clk@3.580MHz NTSC）53.693MHz/15
; ・ボーレート  38400bps（１ビット 92.4clk@3.547MHz PAL
; ・ボーレート  38400bps（１ビット 93.2clk@3.580MHz NTSC

; void uart_processVgm()
    ;41BCH

_uart_processVgm::
    DI
    LD  A,#15        ; 7
    OUT (PSGAD),A    ; 11
    LD  A,#0xCF      ; 7
    OUT (PSGWR),A    ; 11   Select Joy2 Input

    LD  E,#0x30      ; 7
    XOR L            ; Zero clear
    LD  C,#14

__VGM_LOOP:
    ; Select PSG REG14
    LD  A,C             ;  4  4
    OUT (PSGAD),A       ; 11 15

__VGM_ADRS1:
    ; Read JOY2
    IN  A,(PSGRD)       ; 11 26
    BIT 4,A             ;  8 34
    JP  Z, __VGM_ADRS1  ; 10 44
    ; ADDRESS Hi 4bit
    IN  A,(PSGRD)       ; 11 55
    LD  B,A             ;  4 59
    SLA B               ;  8 67
    SLA B               ;  8 75
;    SLA B               ; exec later X
;    SLA B               ; exec later X

__VGM_ADRS2:
    IN  A,(PSGRD)       ; 11 11
    BIT 4,A             ;  8 19
    JP  NZ, __VGM_ADRS2 ; 10 29
    ; ADDRESS Lo 4bit
    IN  A,(PSGRD)       ; 11 40
    AND #0xf            ;  7 47
    SLA B               ;  8 55 ;X
    SLA B               ;  8 63 ;X
    ; ADDRESS 8bit
    OR  B               ;  4 67
    LD  B, A            ;  4 71

__VGM_DATA1:
    IN  A,(PSGRD)       ; 11 11
    BIT 5,A             ;  8 19
    JP  Z, __VGM_DATA1  ; 10 36
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 11 47
    LD  D,A             ;  4 51
    SLA D               ;  8 59
    SLA D               ;  8 67
    SLA D               ;  8 75
;    SLA D               ; exec later X

__VGM_DATA2:
    IN  A,(PSGRD)       ; 11 11
    BIT 5,A             ;  8 19
    JP  NZ, __VGM_DATA2 ; 10 29
    ; DATA Lo 4bit
    IN  A,(PSGRD)       ; 11 40
    AND #0xf            ;  7 47 ;X
    SLA D               ;  8 55
    ; DATA 8bit
    OR  D               ;  4 59
    LD  D,A             ;  4 63

__VGM_TYPE1:
    IN  A,(PSGRD)       ; 11 11
    BIT 5,A             ;  8 19
    JP  Z, __VGM_TYPE1  ; 10 29
    IN  A,(PSGRD)       ; 11 40
    XOR #0xB0           ;  7 47
    LD  H,A             ;  4 51
    JP  (HL)            ;  4 55

_END_VGM:
    EI                  ;  4
    ret                 ; 10

    .area   _HEADER (ABS)
    .ORG 0x5000
__WRITE_PSG_IO:
    LD  A,B             ;  4 59
    OUT (PSGAD),A       ; 11 70
    LD  A,D             ;  4 74
    OUT (PSGWR),A       ; 11 85
    JP  __VGM_LOOP      ; 10 95

    .ORG 0x5100
__WRITE_OPLL_IO:
    LD  A,B
    OUT (OPLLAD),A
    LD  A,D
    OUT (OPLLWR),A
    JP  __VGM_LOOP      ; 10 83

    .ORG 0x5200
__WRITE_OPLL_ADR:
    JP __VGM_LOOP       ; 10 102

    .ORG 0x5300
__WRITE_SCC1:
    LD  H,#0x80         ;  7 81
    LD  L,B             ;  4 85
    LD  (HL), D         ;  7 92
    JP __VGM_LOOP       ; 10 102

    .ORG 0x5400
__WRITE_SCC2:
    LD  H,#0x80         ;  7 81
    LD  L,B             ;  4 85
    LD  (HL), D         ;  7 92
    JP __VGM_LOOP       ; 10 102

    .ORG 0x5500
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5600
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5700
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5800
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5900
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5A00
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5B00
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5C00
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5D00
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5E00
    JP __VGM_LOOP       ; 10 98

    .ORG 0x5F00
    JP __VGM_LOOP       ; 10 98


LD  A, H
ADD A, #0x30
CALL CHPUT

LD  A, B
SRL A
SRL A
SRL A
SRL A
ADD A, #0x30
CALL CHPUT
LD  A, B
AND #0xf
ADD A, #0x30
CALL CHPUT

LD  A, D
SRL A
SRL A
SRL A
SRL A
ADD A, #0x30
CALL CHPUT
LD  A, D
AND #0xf
ADD A, #0x30
CALL CHPUT

LD A, #0x20
CALL CHPUT

JP  __VGM_LOOP
