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
    AND #0x20           ;  8 19*
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
    AND #0x20           ;  8 19*
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
    AND #0x20           ;  8 26*
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
    AND #0x20           ;  8 19*
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
    .macro READ_DATA
;__VGM_DATA_HI:
3$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  NZ, 3$          ; 10 28 __VGM_DATA_HI
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 11 39
    LD  D,A             ;  4 43
    SLA D               ;  8 51
    SLA D               ;  8 59
    SLA D               ;  8 67
    SLA D               ;  8 75

;__VGM_DATA_LO:
4$:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  7 18
    JP  Z,4$            ; 10 28 __VGM_DATA_LO
    ; DATA Lo 4bit
    IN  A,(PSGRD)       ; 11 39
    AND E               ;  4 43
    ; DATA 8bit
    OR  D               ;  4 47
    LD  D,A             ;  4 51
    .endm

;=======================================================
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
    AND #0x10           ;  8 34*
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
    AND #0x10           ;  7 18
    JP  NZ,__VGM_ADRS_LO ; 10 28
    ; ADDRESS Lo 4bit
    IN  A,(PSGRD)       ; 11 39
    AND #0xf            ;  7 46
    SLA B               ;  8 54 ;X
    SLA B               ;  8 62 ;X
    SLA B               ;  8 70 ;X
    ; ADDRESS 8bit
    OR  B               ;  4 74
    LD  B, A            ;  4 78

    LD  L,#0            ;  7 85 Zero clear
    LD  E,#0xF          ;  7 92

__VGM_TYPE:
    IN  A,(PSGRD)       ; 11 11
    AND #0x20           ;  8 19*
    JP  Z, __VGM_TYPE   ; 10 29
    IN  A,(PSGRD)       ; 11 40
    AND	E               ;  4 44
    OR  #0x50           ;  7 51
    LD  H,A             ;  4 55
    JP  (HL)            ;  4 59

_END_VGM:
    EI                  ;  4
    ret                 ; 10

    .area   _HEADER (ABS)

;=======================================================
    .ORG 0x5000
__WRITE_PSG_IO:
    READ_DATA           ; 51

    LD  A,B             ;  4 55
    OUT (PSGAD),A       ; 11 66
    LD  A,D             ;  4 70
    OUT (PSGWR),A       ; 11 81
    JP  __VGM_LOOP      ; 10 91

;=======================================================
    .ORG 0x5100
__WRITE_OPLL_IO:
    LD  A,B             ;  4 63
    OUT (OPLLAD),A      ; 11 74

    READ_DATA           ; 51

    LD  A,D             ;  4 55
    OUT (OPLLWR),A      ; 11 66
    JP  __VGM_LOOP      ; 10 76

    .ORG 0x5200
__WRITE_OPLL_ENA:
    READ_DATA           ;51

    PUSH    DE          ;11 70 59+11
    LD      HL,#0x7FF6  ;10 80     ; Address for EXT OPLL ENA FLAG 
    LD      A,D         ; 4 84     ; SLOT #
    CALL    RDSLT       ;17 101
    SET     0,A         ; 8 ???+8  ; ENA OPLL BIT 1
    POP     DE          ;10 ???+18

    LD      E,A         ; 4 ???+22 ; Write val
    LD      A,D         ; 4 ???+26 ; SLOT #
    LD      HL,#0x7FF6  ;10 ???+36 ; Address  for EXT OPLL ENA FLAG 
    CALL    WRSLT       ;17 ???+73
    INIT_CONST          ; 7 ???+80
    JP  __VGM_LOOP      ;10 ???+90

;=======================================================
    .ORG 0x5300
    ;https://www.msx.org/forum/msx-talk/software/scc-music-altera-de-1
__WRITE_SCC_SLOT:
    READ_DATA           ;51

    ; CHANGE PAGE2 TO SCC SLOT PAGE2
    PUSH    BC          ;11 70 59+11
    LD      A,D         ; 4 74
    LD      H,#0x80     ; 7 81
    CALL    ENASLT      ;17 98
    POP     BC          ;10 ???+10

    ; ENA SCC
    LD      A,B          ; 4 ???+14
    CP      #0x01        ; 7 ???+21
    JP      Z,__ENA_SCC1 ;12 ???+33
    CP      #0x02        ; 7 ???+40
    JP      Z,__ENA_SCC1_COMPAT ;12 ???+52
    CP      #0x03        ; 7 ???+59
    JP      Z,__ENA_SCC  ;12 ???+71

    JP      __VGM_LOOP   ;10 ???+81

__ENA_SCC1:
    LD      HL,#0xBFFE   ;10 ???+ 43
    LD      A,#0x20      ; 7 ???+ 50
    LD      (HL),A       ; 7 ???+ 57

    LD      HL,#0xB000   ;10 ???+ 67
    LD      A,#0x80      ; 7 ???+ 74
    LD      (HL),A       ; 7 ???+ 81
    INIT_CONST           ; 7 ???+ 88
    JP  __VGM_LOOP       ;10 ???+ 98

__ENA_SCC1_COMPAT:
    LD      HL,#0xBFFE
    LD      A,#0x00
    LD      (HL),A
__ENA_SCC:
    LD      HL,#0x9000
    LD      A,#0x3F
    LD      (HL),A
    INIT_CONST
    JP  __VGM_LOOP       ;10 ???+ 98+19

;=======================================================
    .ORG 0x5400
__WRITE_SCC1:
    LD  H,#0xB8         ;  7 66
    LD  L,B             ;  4 70

    READ_DATA           ; 51

    LD  (HL), D         ;  7 58
    JP  __VGM_LOOP      ; 10 68

;=======================================================
    .ORG 0x5500
__WRITE_SCC:
    LD  H,#0x98         ;  7 66
    LD  L,B             ;  4 70

    READ_DATA           ; 51

    LD  (HL), D         ;  7 58
    JP  __VGM_LOOP      ; 10 68

;=======================================================
    .ORG 0x5600
__WRITE_SCC1_2BYTES:
    LD  H,#0xB8         ;  7 66
    LD  L,B             ;  4 70

    READ_DATA           ; 51

    WRITE_SCC_2BYTES

;=======================================================
    .ORG 0x5700
__WRITE_SCC_2BYTES:
    LD  H,#0x98         ;  7 66
    LD  L,B             ;  4 70

    READ_DATA           ; 51

    WRITE_SCC_2BYTES

;=======================================================
    .ORG 0x5800
__WRITE_SCC1_32_BYTES:
    LD  H,#0xB8         ;  7 66
    LD  L,B             ;  4 70

    READ_DATA           ; 51

    LD  (HL), D         ;  7 58
    WRITE_SCC_31_BYTES

;=======================================================
    .ORG 0x5900
__WRITE_SCC_32_BYTES:
    LD  H,#0x98         ;  7 66
    LD  L,B             ;  4 70

    READ_DATA           ; 51

    LD  (HL), D         ;  7 58
    WRITE_SCC_31_BYTES

;=======================================================
    .ORG 0x5A00
__WRITE_OPL3_IO1:
    LD  A,B             ;  4 63
    OUT (OPL3AD1),A     ; 11 77

    READ_DATA           ; 52

    LD  A,D             ;  4 56
    OUT (OPL3WR),A      ; 11 67
    JP  __VGM_LOOP      ; 10 77

;=======================================================
    .ORG 0x5B00
__WRITE_OPL3_IO2:
    LD  A,B             ;  4 63
    OUT (OPL3AD2),A     ; 11 77

    READ_DATA           ; 52

    LD  A,D             ;  4 56
    OUT (OPL3WR),A      ; 11 67
    JP  __VGM_LOOP      ; 10 77

;=======================================================
    .ORG 0x5C00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x5D00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x5E00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x5F00
    JP __VGM_LOOP       ; 10 69

;=======================================================

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
