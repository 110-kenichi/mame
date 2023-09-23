
;=======================================================

    macro TRANSFER_PROC DADDR,SADDR,EADDR
    LD  DE,DADDR
    LD  HL,SADDR
    LD  BC,EADDR-SADDR
    LDIR
    endm

    macro FM_ADRS_WAIT
    NOP
    NOP
    NOP
    endm

    macro FM_DATA_WAIT
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    NOP
    endm

;=======================================================

    macro READ_ADRS
    LOCAL L5,L6
;__VGM_ADRS_HI:
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L5:
    IN  A,(OPNAWR1)     ; 12
    AND $01             ;  8
    JP  Z, L5           ; 11
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
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
L6:
    IN  A,(OPNAWR1)     ; 12
    AND $01             ;  5
    JP  NZ,L6           ; 11
    ; ADDRESS Lo 4bit
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND E               ;  5
    ; ADDRESS 8bit
    OR  B               ;  5
    LD  B, A            ;  5 92
    endm
;=======================================================

    macro READ_DATA
    LOCAL L3,L4
    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
;__VGM_DATA_HI:
L3:
    IN  A,(OPNAWR1)     ; 12
    AND $01             ;  8
    JP  Z, L3           ; 11
    ; DATA Hi 4bit
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10 122

    LD  A,E             ;  5
    OUT (OPNAAD1),A     ; 12
;__VGM_DATA_LO:
L4:
    IN  A,(OPNAWR1)     ; 12
    AND $01             ;  8
    JP  NZ,L4           ; 11
    ; DATA Lo 4bit
    LD  A,C             ;  5
    OUT (OPNAAD1),A     ; 12
    IN  A,(OPNAWR1)     ; 12
    AND E               ;  5
    ; DATA 8bit
    OR  D               ;  5
    LD  D,A             ;  5  92
    endm
;=======================================================

    macro __WRITE_OPNA_IO1
    LOCAL L0,L1
;__WRITE_OPNA_IO1:
    READ_ADRS           ; 97
L0:
;=======================
    READ_DATA           ; 89

    LD  A,B             ;  5
    OUT (OPNAAD1),A     ; 12
    FM_ADRS_WAIT        ; 15
    LD  A,D             ;  5
    OUT (OPNAWR1),A     ;  5
    FM_DATA_WAIT        ; 75 = 117

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
    LOCAL L0,L1
;__WRITE_OPNA_IO2:
    READ_ADRS           ; 97
L0:
;=======================
    READ_DATA           ;  89

    LD  A,B             ; 
    OUT (OPNAAD2),A     ;
    FM_ADRS_WAIT
    LD  A,D             ; 
    OUT (OPNAWR2),A     ; 49+89 = 138
    FM_DATA_WAIT

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
    endm

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
    FM_ADRS_WAIT
    LD  A,D             ; 
    OUT (SB2WR1),A      ;  49+89 = 138
    FM_DATA_WAIT

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
    LOCAL L0,L1
;__WRITE_SB2_IO2:
    READ_ADRS           ; 61
L0:
;=======================
    READ_DATA           ;  89

    LD  A,B             ; 
    OUT (SB2AD2),A      ;
    FM_ADRS_WAIT
    LD  A,D             ; 
    OUT (SB2WR2),A      ; 49+89 = 138
    FM_DATA_WAIT

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
    endm

;=======================================================

    macro __WRITE_OPNA_IO1_R
    LOCAL L0,L1
;__WRITE_OPNA_IO1:
    READ_ADRS           ; 97
L0:
;=======================
    READ_DATA           ; 89

    LD  A,B             ;  5
    OUT (OPNAAD1),A     ; 12
    FM_ADRS_WAIT        ; 15
    LD  A,D             ;  5
    OUT (OPNAWR1),A     ;  5
    FM_DATA_WAIT        ; 75 = 117
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT        ; 75 = 117

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

    macro __WRITE_SB2_IO1_R
    LOCAL L0,L1
;__WRITE_SB2_IO1:
    READ_ADRS           ; 92
L0:
;=======================
    READ_DATA           ; 92

    LD  A,B             ;  5
    OUT (SB2AD1),A      ;
    FM_ADRS_WAIT
    LD  A,D             ; 
    OUT (SB2WR1),A      ;  49+89 = 138
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT
    FM_DATA_WAIT        ; 75 = 117

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

    macro __WRITE_OPNA_PCM
    LOCAL L0,L1
;__WRITE_OPNA_DAC:
L0:
;=======================
    READ_DATA           ; 92

    LD  A,$08           ; 
    OUT (OPNAAD2),A     ; 
    LD  A,D             ; 
    OUT (OPNAWR2),A     ; 37+92=129

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

    macro __WRITE_SB2_PCM
    LOCAL L0,L1
;__WRITE_SB2_DAC:
L0:
;=======================
    READ_DATA          ; 92

    LD  A,$08          ; 
    OUT (SB2AD2),A     ; 
    LD  A,D            ; 
    OUT (SB2WR2),A     ; 37+92=129

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

    macro __WRITE_OPNA_PSEUDO_DAC
    LOCAL L0,L1
;__WRITE_OPNA_DAC:
L0:
;=======================
    READ_DATA           ; 89

    LD  A,$0B            ; 
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

    macro __WRITE_SB2_PSEUDO_DAC
    LOCAL L0,L1
;__WRITE_SB2_DAC:
L0:
;=======================
    READ_DATA          ; 89

    LD  A,$0B           ; 
    OUT (SB2AD2),A     ; 
    LD  A,D            ; 
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

    macro __WRITE_OPNA_DAC
    LOCAL L0,L1
;__WRITE_OPNA_DAC:
L0:
;=======================
    READ_DATA           ; 89

    LD  A,$0E            ; 
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

    macro __WRITE_SB2_DAC
    LOCAL L0,L1
;__WRITE_SB2_DAC:
L0:
;=======================
    READ_DATA          ; 89

    LD  A,$0E           ; 
    OUT (SB2AD2),A     ; 
    LD  A,D            ; 
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
