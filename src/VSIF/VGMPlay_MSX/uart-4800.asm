PSGAD = 0xA0
PSGWR = 0xA1
PSGRD = 0xA2

CHPUT = 0xA2

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
    OUT (#PSGAD),A   ; 11
    LD  A,#0xCF      ; 7
    OUT (#PSGWR),A   ; 11   Select Joy2 Input

    LD  E,#0x30      ; 7
    XOR L            ; Zero clear

__VGM_LOOP:
    ; Select PSG REG14
    LD  A,#14           ;  7  7
    OUT (#PSGAD),A      ; 11 18

__VGM_ADRS1:
    ; Read JOY2
    IN  A,(#PSGRD)      ; 11 29
    LD  B,A             ;  4 33
    AND E               ;  4 37
    ; Check data type
    CP  #0x10           ;  7 44
    JP  NZ, __VGM_ADRS1 ; 10 54

    ; ADDRESS Hi 4bit
    SLA B               ;  8 62

__VGM_ADRS2:
    ; Read JOY2
    IN  A,(#PSGRD)      ; 11 11
    ; Check data type
    BIT 4,A             ;  8 19
    JP  NZ, __VGM_ADRS2 ; 10 29

    ; ADDRESS Lo 4bit
    AND #0xf            ;  7 36
    SLA B               ;  8 44
    SLA B               ;  8 52
    SLA B               ;  8 60
    ; ADDRESS 8bit
    OR  B               ;  4 64
    LD  B, A            ;  4 68

__VGM_DATA1:
    ; Read JOY2
    IN  A,(#PSGRD)      ; 11 11
    LD  D, A            ;  4 15
    AND E               ;  4 19
    ; Check data type
    CP  #0x20           ;  7 26
    JP  NZ, __VGM_DATA1 ; 10 36

    ; DATA Hi 4bit
    SLA D               ;  8 44
    SLA D               ;  8 52

__VGM_DATA2:
    ; Read JOY2
    IN  A,(#PSGRD)      ; 11 11
    ; Check data type
    BIT 5,A             ;  8 19
    JP  NZ, __VGM_DATA2 ; 10 29

    ; DATA Lo 4bit
    AND #0xf            ;  7 36
    SLA D               ;  8 44
    SLA D               ;  8 52
    ; DATA 
    OR  D               ;  4 56
    LD  D,A             ;  4 60

__VGM_TYPE1:
    ; Read JOY2
    IN  A,(#PSGRD)      ; 11 11
    LD  H, A            ;  4 22
    AND E               ;  4 26
    ; Check data type
    CP  #0x30           ;  7 33
    JP  NZ, __VGM_TYPE1 ; 10 43
    LD  A, H
    AND #0x3F           ;  7 50
    ADD #0x20           ;  7 57
    LD  H, A            ;  4 61
    JP  (HL)            ;  4 65

_END_VGM:
    EI
    ret              ; 10

    .area   _HEADER (ABS)
    .ORG 0x5000
__WRITE_PSG:
    LD  A,B             ;  4 69
    OUT (#PSGAD),A      ; 11 80
    LD  A,D             ;  4 84
    OUT (#PSGWR),A      ; 11 95
    JP  __VGM_LOOP      ; 10 105

    .ORG 0x5100
__WRITE_OPLL:
    LD  A,B
    OUT (#PSGAD),A
    LD  A,D
    OUT (#PSGWR),A
    JP  __VGM_LOOP      ; 10 83

    .ORG 0x5200
__WRITE_SCC1:
    LD  H,#0x80         ;  7 81
    LD  L,B             ;  4 85
    LD  (HL), D         ;  7 92
    JP __VGM_LOOP       ; 10 102

    .ORG 0x5300
__WRITE_SCC2:
    LD  H,#0x80         ;  7 81
    LD  L,B             ;  4 85
    LD  (HL), D         ;  7 92
    JP __VGM_LOOP       ; 10 102

    .ORG 0x5400
__WRITE_SCC3:
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
