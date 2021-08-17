PSGAD = #0xA0
PSGWR = #0xA1
PSGRD = #0xA2

OPLLAD = #0x7C
OPLLWR = #0x7D

OPL3AD1 = #0xC4
OPL3WR  = #0xC5
OPL3AD2 = #0xC6

WRSLT = #0x14
RDSLT = #0x0c
ENASLT = #0x24

CHPUT = #0xA2

; ・ボーレート 115200bps（１ビット 30.79clk@3.5466MHz PAL）
; ・ボーレート 115200bps（１ビット 31.07clk@3.5793MHz NTSC）
; ・ボーレート  57600bps（１ビット 61.58clk@3.547MHz PAL） 53.203MHz/15
; ・ボーレート  57600bps（１ビット 62.14clk@3.580MHz NTSC）53.693MHz/15
; ・ボーレート  38400bps（１ビット 92.4clk@3.547MHz PAL
; ・ボーレート  38400bps（１ビット 93.2clk@3.580MHz NTSC

; void uart_processVgm()
    ;41BCH

    .macro INIT_CONST   ;   14
    LD  E,#0x30         ; 7  7
    LD  C,#14           ; 7 14
    .endm

    .macro WRITE_SCC_2BYTES
1$:
    ; 2ND DATA Hi 4bit
    IN  A,(PSGRD)       ; 11 11
    BIT 5,A             ;  8 19
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
    BIT 5,A             ;  8 19
    JP  Z, 2$           ; 10 29
    IN  A,(PSGRD)       ; 11 40
    AND #0xf            ;  7 47
    OR  E               ;  4 51

    ;WRITE BOTH BYTE AND NEXT BYTE
    LD  (HL), D         ;  7 58
    INC L               ;  4 62
    LD  (HL), A         ;  7 69
    LD  E,#0x30         ;  7 76
    JP  __VGM_LOOP      ; 10 86
    .endm

    .macro WRITE_SCC_31_BYTES
    LD  B,#31           ;  7  7
1$:
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 11 18
    BIT 5,A             ;  8 26
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
    BIT 5,A             ;  8 19
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

_uart_processVgm::
    DI
    LD  A,#15        ;  7
    OUT (PSGAD),A    ; 11
    LD  A,#0xCF      ;  7
    OUT (PSGWR),A    ; 11   Set Joy2 Pin Input Mode

    INIT_CONST

__VGM_LOOP:
    ; JOY2 Pin Read Mode
    LD  A,C             ;  4  4
    OUT (PSGAD),A       ; 11 15

__VGM_ADRS_HI:
    IN  A,(PSGRD)       ; 11 26
    BIT 4,A             ;  8 34
    JP  Z,__VGM_ADRS_HI ; 10 44
    IN  A,(PSGRD)       ; 11 55
    BIT 5,A             ;  8 63
    JP  NZ,__VGM_ADRS_HI  ; 10 73
    LD  B,A             ;  4 77
    SLA B               ;  8 85
;    SLA B               ; exec later X
;    SLA B               ; exec later X
;    SLA B               ; exec later X

__VGM_ADRS_LO:
    IN  A,(PSGRD)       ; 11 11
    BIT 4,A             ;  8 19
    JP  NZ,__VGM_ADRS_LO ; 10 29
    ; ADDRESS Lo 4bit
    IN  A,(PSGRD)       ; 11 40
    AND #0xf            ;  7 47
    SLA B               ;  8 55 ;X
    SLA B               ;  8 63 ;X
    SLA B               ;  8 71 ;X
    ; ADDRESS 8bit
    OR  B               ;  4 75
    LD  B, A            ;  4 79

__VGM_DATA_HI:
    IN  A,(PSGRD)       ; 11 11
    BIT 5,A             ;  8 19
    JP  Z, __VGM_DATA_HI ; 10 36
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 11 47
    LD  D,A             ;  4 51
    SLA D               ;  8 59
    SLA D               ;  8 67
    SLA D               ;  8 75
;    SLA D               ; exec later X

__VGM_DATA_LO:
    IN  A,(PSGRD)       ; 11 11
    BIT 5,A             ;  8 19
    JP  NZ,__VGM_DATA_LO ; 10 29
    ; DATA Lo 4bit
    IN  A,(PSGRD)       ; 11 40
    AND #0xf            ;  7 47
    SLA D               ;  8 55 ;X
    ; DATA 8bit
    OR  D               ;  4 59
    LD  D,A             ;  4 63

    LD  L,#0            ;  7 70 Zero clear
__VGM_TYPE:
    IN  A,(PSGRD)       ; 11 11
    BIT 5,A             ;  8 19
    JP  Z, __VGM_TYPE   ; 10 29
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
    JP  __VGM_LOOP      ; 10 95

    .ORG 0x5200
__WRITE_OPLL_ENA:
    PUSH    DE
    LD      HL,#0x7FF6     ; Address for EXT OPLL ENA FLAG 
    LD      A,D            ; SLOT #
    CALL    RDSLT
    SET     0,A            ; ENA OPLL BIT 1
    POP     DE

    LD      E,A            ; Write val
    LD      A,D            ; SLOT #
    LD      HL,#0x7FF6     ; Address  for EXT OPLL ENA FLAG 
    CALL    WRSLT

    INIT_CONST
    JP  __VGM_LOOP

    .ORG 0x5300
    ;https://www.msx.org/forum/msx-talk/software/scc-music-altera-de-1
__WRITE_SCC_SLOT:
    ; CHANGE PAGE2 TO SCC SLOT PAGE2
    PUSH    BC
    LD      A,D
    LD      H,#0x80
    CALL    ENASLT
    POP     BC

    ; ENA SCC
    LD      A,B
    CP      #0x01
    JP      Z,__ENA_SCC1
    CP      #0x02
    JP      Z,__ENA_SCC1_COMPAT
    CP      #0x03
    JP      Z,__ENA_SCC

    JP      __WRITE_SCC_SLOT_END

__ENA_SCC1:
    LD      HL,#0xBFFE
    LD      A,#0x20
    LD      (HL),A

    LD      HL,#0xB000
    LD      A,#0x80
    LD      (HL),A
    JP      __WRITE_SCC_SLOT_END

__ENA_SCC1_COMPAT:
    LD      HL,#0xBFFE
    LD      A,#0x00
    LD      (HL),A

    LD      HL,#0x9000
    LD      A,#0x3F
    LD      (HL),A
    JP      __WRITE_SCC_SLOT_END

__ENA_SCC:
    LD      HL,#0x9000
    LD      A,#0x3F
    LD      (HL),A
    JP      __WRITE_SCC_SLOT_END

__WRITE_SCC_SLOT_END:
    INIT_CONST
    JP  __VGM_LOOP

    .ORG 0x5400
__WRITE_SCC1:
    LD  H,#0xB8         ;  7 62
    LD  L,B             ;  4 66
    LD  (HL), D         ;  7 73
    JP  __VGM_LOOP      ; 10 83

    .ORG 0x5500
__WRITE_SCC:
    LD  H,#0x98         ;  7 62
    LD  L,B             ;  4 66
    LD  (HL), D         ;  7 73
    JP  __VGM_LOOP      ; 10 83

    .ORG 0x5600
__WRITE_SCC1_2BYTES:
    LD  H,#0xB8         ;  7 62
    LD  L,B             ;  4 66
    WRITE_SCC_2BYTES

    .ORG 0x5700
__WRITE_SCC_2BYTES:
    LD  H,#0x98         ;  7 62
    LD  L,B             ;  4 66
    WRITE_SCC_2BYTES

    .ORG 0x5800
__WRITE_SCC1_32_BYTES:
    LD  H,#0xB8         ;  7 62
    LD  L,B             ;  4 66
    LD  (HL), D         ;  7 73
    WRITE_SCC_31_BYTES

    .ORG 0x5900
__WRITE_SCC_32_BYTES:
    LD  H,#0x98         ;  7 62
    LD  L,B             ;  4 66
    LD  (HL), D         ;  7 73
    WRITE_SCC_31_BYTES

    .ORG 0x5A00
__WRITE_OPL3_IO1:
    LD  A,B
    OUT (OPL3AD1),A
    LD  A,D
    OUT (OPL3WR),A
    JP  __VGM_LOOP      ; 10 95

    .ORG 0x5B00
__WRITE_OPL3_IO2:
    LD  A,B
    OUT (OPL3AD2),A
    LD  A,D
    OUT (OPL3WR),A
    JP  __VGM_LOOP      ; 10 95

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
