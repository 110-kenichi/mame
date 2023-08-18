
;=======================================================

    macro TRANSFER_PROC DADDR,SADDR,EADDR
    LD  DE,DADDR
    LD  HL,SADDR
    LD  BC,EADDR-SADDR
    LDIR
    endm

    macro READ_ADRS
    LOCAL L5,L6
;__VGM_ADRS_HI:
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L5:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  NZ, L5          ; 11
    ; ADDRESS Hi 4bit
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    LD  B,A             ;  5
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10 122

;__VGM_ADRS_LO:
    LD  H,$01         ;  8
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L6:
    IN  A,(OPNAWR1)     ; 12
    AND H               ;  5
    JP  Z,L6            ; 11
    ; ADDRESS Lo 4bit
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND E               ;  5
    ; ADDRESS 8bit
    OR  B               ;  5
    LD  B, A            ;  5 97
    endm
;=======================================================

    macro READ_DATA
    LOCAL L3,L4
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
;__VGM_DATA_HI:
L3:
    IN  A,(OPNAWR1)     ; 12
    AND H               ;  5
    JP  NZ, L3          ; 11
    ; DATA Hi 4bit
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10 119

    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
;__VGM_DATA_LO:
L4:
    IN  A,(OPNAWR1)     ; 12
    AND H               ;  5
    JP  Z,L4            ; 11
    ; DATA Lo 4bit
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND E               ;  5
    ; DATA 8bit
    OR  D               ;  5
    LD  D,A             ;  5  89
    endm
;=======================================================

    macro __WRITE_OPNA_IO1
    LOCAL L0,L1
;__WRITE_OPNA_IO1:
    READ_ADRS           ; 97
L0:
;=======================
    READ_DATA           ; 89

    LD  A,B             ; 
    OUT (OPNAAD1),A     ;
    LD  A,D             ; 
    OUT (OPNAWR1),A     ;  34+89 = 123

    ; Continuous Write
    INC B               ;  5 
;=======================
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L1:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  Z,L1            ; 11
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND	E               ;  5
    CP  E               ;  5
    JP  Z,L0            ; 11  ; Continuous Write

    OR  JPOFST          ;  8
    LD  H,A             ;  5
    LD  L,0             ;  8
    JP  (HL)            ;  5  ; Jump to other ID
    endm
;=======================================================

	macro __WRITE_OPNA_IO2
    LOCAL L0,L1,L2,L3,__WRITE_OPNA_IO2_ADPCM
;__WRITE_OPNA_IO2:
    READ_ADRS           ; 97
L0:
    CP  $08           ;  8
    JP  Z, __WRITE_OPNA_IO2_ADPCM ; 11 116
;=======================
    READ_DATA           ;  89

    LD  A,B             ; 
    OUT (OPNAAD2),A     ;
    LD  A,D             ; 
    OUT (OPNAWR2),A     ; 34+89 = 123

    ; Continuous Write
    INC B               ; 
;=======================
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L1:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  Z,L1            ; 11
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND	E               ;  5
    CP  E               ;  5
    JP  Z,L0            ; 11  ; Continuous Write

    OR  JPOFST          ;  8
    LD  H,A             ;  5
    LD  L,0             ;  8
    JP  (HL)            ;  5  ; Jump to other ID

;=======================  ADPCM burst write mode
__WRITE_OPNA_IO2_ADPCM:
L2:
    READ_DATA           ; 89

    LD  A,B             ; 
    OUT (OPNAAD2),A     ;
    LD  A,D             ; 
    OUT (OPNAWR2),A     ; 34+89 = 123

    ; Continuous Write (Do not increment)
    ;INC B               ; 
;=======================
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L3:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  Z,L3            ; 11
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND	E               ;  5
    CP  E               ;  5
    JP  Z,L2            ; 11  ; Continuous Write

    OR  JPOFST         ;  8
    LD  H,A             ;  5
    LD  L,0            ;  8
    JP  (HL)            ;  5  ; Jump to other ID
    endm
;=======================================================

    macro __WRITE_OPNA_DAC
    LOCAL L0,L1
;__WRITE_OPNA_DAC:
L0:
;=======================
    READ_DATA           ; 89

    LD  A,$B          ; 
    OUT (OPNAAD2),A     ; 
    LD  A,D             ; 
    OUT (OPNAWR2),A     ; 37+89=126

    ; Continuous Write (Do not increment)
    ;INC B               ; 
    ;=======================
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L1:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  Z,L1            ; 11
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND	E               ;  5
    CP  E               ;  5
    JP  Z,L0            ; 11  ; Continuous Write

    OR  JPOFST          ;  8
    LD  H,A             ;  5
    LD  L,0             ;  8
    JP  (HL)            ;  5  ; Jump to other ID
    endm
;=======================================================




;=======================================================
    macro __WRITE_SB2_IO1
    LOCAL L0,L1
;__WRITE_SB2_IO1:
    READ_ADRS           ; 97
L0:
;=======================
    READ_DATA           ; 89

    LD  A,B             ; 
    OUT (SB2AD1),A      ;
    LD  A,D             ; 
    OUT (SB2WR1),A      ;  34+89 = 123

    ; Continuous Write
    INC B               ;  5 
;=======================
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L1:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  Z,L1            ; 11
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND	E               ;  5
    CP  E               ;  5
    JP  Z,L0            ; 11  ; Continuous Write

    OR  JPOFST          ;  8
    LD  H,A             ;  5
    LD  L,0             ;  8
    JP  (HL)            ;  5  ; Jump to other ID
    endm
;=======================================================

	macro __WRITE_SB2_IO2
    LOCAL L0,L1,L2,L3,__WRITE_SB2_IO2_ADPCM
;__WRITE_SB2_IO2:
    READ_ADRS           ; 61
L0:
    CP  $08           ;  8
    JP  Z, __WRITE_SB2_IO2_ADPCM ; 11 116
;=======================
    READ_DATA           ;  89

    LD  A,B             ; 
    OUT (SB2AD2),A      ;
    LD  A,D             ; 
    OUT (SB2WR2),A      ; 34+89 = 123

    ; Continuous Write
    INC B               ; 
;=======================
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L1:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  Z,L1            ; 11
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND	E               ;  5
    CP  E               ;  5
    JP  Z,L0            ; 11  ; Continuous Write

    OR  JPOFST          ;  8
    LD  H,A             ;  5
    LD  L,0             ;  8
    JP  (HL)            ;  5  ; Jump to other ID

;=======================  ADPCM burst write mode
__WRITE_SB2_IO2_ADPCM:
L2:
    READ_DATA           ; 55

    LD  A,B             ; 
    OUT (SB2AD2),A      ;
    LD  A,D             ; 
    OUT (SB2WR2),A      ; 34+89 = 123

    ; Continuous Write (Do not increment)
    ;INC B               ; 
;=======================
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L3:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  Z,L3            ; 11
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND	E               ;  5
    CP  E               ;  5
    JP  Z,L2            ; 11  ; Continuous Write

    OR  JPOFST          ;  8
    LD  H,A             ;  5
    LD  L,0             ;  8
    JP  (HL)            ;  5  ; Jump to other ID

    endm
;=======================================================

    macro __WRITE_SB2_DAC
    LOCAL L0,L1
;__WRITE_SB2_DAC:
    LD  A,$B          ;  8
    OUT (SB2AD2),A     ; 12 20
L0:
;=======================
    READ_DATA           ; 89

    LD  A,$B          ; 
    OUT (SB2AD2),A     ; 
    LD  A,D             ; 
    OUT (SB2WR2),A     ; 37+89=126

    ; Continuous Write (Do not increment)
    ;INC B               ; 
    ;=======================
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L1:
    IN  A,(OPNAWR1)     ; 12
    AND $02             ;  8
    JP  Z,L1            ; 11
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND	E               ;  5
    CP  E               ;  5
    JP  Z,L0            ; 11  ; Continuous Write

    OR  JPOFST          ;  8
    LD  H,A             ;  5
    LD  L,0             ;  8
    JP  (HL)            ;  5  ; Jump to other ID
    endm
;=======================================================
