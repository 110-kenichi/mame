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
    BIT 5,A             ;  8 37
    JP  NZ, 5$          ; 10 47
    LD  B,A             ;  4 51
    SLA B               ;  8 59
    SLA B               ;  8 68
    SLA B               ;  8 76
    SLA B               ;  8 84

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
__VGM_TYPE2:
    IN  A,(PSGRD)       ; 11 25
    AND #0x20           ;  7 32
    JP  Z, __VGM_TYPE2  ; 10 42
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
    LD  A,B             ;  4 62
    OUT (OPLLAD),A      ; 11 73

    READ_DATA           ; 51

    LD  A,D             ;  4 55
    OUT (OPLLWR),A      ; 11 66
    JP  __VGM_LOOP      ; 10 76

;=======================================================
    .ORG 0x6200
__WRITE_OPLL_ENA:
    READ_ADRS
    READ_DATA           ;51

    PUSH    DE          ;11 62
    LD      HL,#0x7FF6  ;10 72     ; Address for EXT OPLL ENA FLAG 
    LD      A,D         ; 4 76     ; SLOT #
    CALL    RDSLT       ;17 93
    SET     0,A         ; 8 ???+8  ; ENA OPLL BIT 1
    POP     DE          ;10 ???+18

    LD      E,A         ; 4 ???+22 ; Write val
    LD      A,D         ; 4 ???+26 ; SLOT #
    LD      HL,#0x7FF6  ;10 ???+36 ; Address  for EXT OPLL ENA FLAG 
    CALL    WRSLT       ;17 ???+73
    INIT_CONST          ; 7 ???+80
    JP  __VGM_LOOP      ;10 ???+90

;=======================================================
    .ORG 0x6300
    ;https://www.msx.org/forum/msx-talk/software/scc-music-altera-de-1
__WRITE_SCC_SLOT:
    READ_ADRS
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
    .ORG 0x6400
__WRITE_SCC1:
    READ_ADRS
    READ_DATA           ; 51

    LD  H,#0xB8         ;  7 57
    LD  L,B             ;  4 61
    LD  (HL), D         ;  7 68
    JP  __VGM_LOOP      ; 10 78

;=======================================================
    .ORG 0x6500
__WRITE_SCC:
    READ_ADRS
    READ_DATA           ; 51

    LD  H,#0x98         ;  7 57
    LD  L,B             ;  4 64
    LD  (HL), D         ;  7 68
    JP  __VGM_LOOP      ; 10 78

;=======================================================
    .ORG 0x6600
__WRITE_SCC1_2BYTES:
    READ_ADRS
    READ_DATA           ; 51

    LD  H,#0xB8         ;  7 58
    LD  L,B             ;  4 62
    WRITE_SCC_2BYTES

;=======================================================
    .ORG 0x6700
__WRITE_SCC_2BYTES:
    READ_ADRS
    READ_DATA           ; 51

    LD  H,#0x98         ;  7 58
    LD  L,B             ;  4 62
    WRITE_SCC_2BYTES

;=======================================================
    .ORG 0x6800
__WRITE_SCC1_32_BYTES:
    READ_ADRS
    READ_DATA           ; 51

    LD  H,#0xB8         ;  7 58
    LD  L,B             ;  4 62
    LD  (HL), D         ;  7 69
    WRITE_SCC_31_BYTES

;=======================================================
    .ORG 0x6900
__WRITE_SCC_32_BYTES:
    READ_ADRS
    READ_DATA           ; 51

    LD  H,#0x98         ;  7 58
    LD  L,B             ;  4 62
    LD  (HL), D         ;  7 69
    WRITE_SCC_31_BYTES

;=======================================================
    .ORG 0x6A00
__WRITE_OPL3_IO1:
    READ_ADRS
    LD  A,B             ;  4 62
    OUT (OPL3AD1),A     ; 11 76

    READ_DATA           ; 51

    LD  A,D             ;  4 55
    OUT (OPL3WR),A      ; 11 66
    JP  __VGM_LOOP      ; 10 76

;=======================================================
    .ORG 0x6B00
__WRITE_OPL3_IO2:
    READ_ADRS
    LD  A,B             ;  4 62
    OUT (OPL3AD2),A     ; 11 76

    READ_DATA           ; 51

    LD  A,D             ;  4 55
    OUT (OPL3WR),A      ; 11 66
    JP  __VGM_LOOP      ; 10 76

;=======================================================
    .ORG 0x6C00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x6D00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x6E00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x6F00
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7000
    JP __VGM_LOOP       ; 10 69

;=======================================================
    .ORG 0x7100
    JP __VGM_LOOP       ; 10 69

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
