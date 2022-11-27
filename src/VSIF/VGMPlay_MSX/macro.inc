
;=======================================================
    .macro WRITE_SCC_2BYTES
1$:
    ; 2ND DATA Hi 4bit
    IN  A,(PSGRD)       ; 12 12
    AND #0x10           ;  8 
    JP  NZ, 1$          ; 11 
    IN  A,(PSGRD)       ; 12 
    LD  E,A             ;  5 
    SLA E               ; 10 
    SLA E               ; 10 
    SLA E               ; 10 
    SLA E               ; 10 88
2$:
    ; 2ND DATA Lo 4bit
    IN  A,(PSGRD)       ; 12 12
    AND #0x10           ;  8
    JP  Z, 2$           ; 11
    IN  A,(PSGRD)       ; 12
    AND #0xf            ;  8
    OR  E               ;  5 56

    ;WRITE BOTH 1st BYTE AND 2nd BYTE
    LD  (HL), D         ;  8 
    INC L               ;  5 
    LD  (HL), A         ;  8 
    JP  __VGM_LOOP      ; 11 88
    .endm

;=======================================================
    .macro WRITE_SCC_31_BYTES
    LD  B,#31           ;  8  8
1$:
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 12
    AND #0x10           ;  8
    JP  NZ,1$           ; 11
    IN  A,(PSGRD)       ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10 96
2$:
    ; DATA Lo 4bit
    IN  A,(PSGRD)       ; 12 12
    AND #0x10           ;  8 
    JP  Z,2$            ; 11 
    IN  A,(PSGRD)       ; 12 
    AND #0xf            ;  8 
    OR  D               ;  5 

    ;WRITE NEXT BYTE
    INC L               ;  5 
    LD  (HL), A         ;  8 
    DJNZ 1$             ; 14 83  9 78
    JP  __VGM_LOOP      ; 11 94 11 89
    .endm
;=======================================================

    .macro READ_ADRS
;__VGM_ADRS_HI:
5$:
    IN  A,(PSGRD)       ; 12
    AND #0x20           ;  8
    JP  NZ, 5$          ; 11
    IN  A,(PSGRD)       ; 12
    LD  B,A             ;  5
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10 88

;__VGM_ADRS_LO:
6$:
    IN  A,(PSGRD)       ; 12
    AND #0x10           ;  8
    JP  Z, 6$           ; 11
    ; ADDRESS Lo 4bit
    IN  A,(PSGRD)       ; 12
    AND #0xf            ;  8
    ; ADDRESS 8bit
    OR  B               ;  5
    LD  B, A            ;  5 61
    .endm
;=======================================================

    .macro READ_DATA
    LD      H,#0x10     ;  8
;__VGM_DATA_HI:
3$:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  NZ, 3$          ; 11 __VGM_DATA_HI
    ; DATA Hi 4bit
    IN  A,(PSGRD)       ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10

    LD  E,#0xF          ;  8 101
;__VGM_DATA_LO:
4$:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  Z,4$            ; 11 __VGM_DATA_LO
    ; DATA Lo 4bit
    IN  A,(PSGRD)       ; 12
    AND E               ;  5
    ; DATA 8bit
    OR  D               ;  5
    LD  D,A             ;  5 55
    .endm
;=======================================================

	.macro __WRITE_PSG_IO
__WRITE_PSG_IO:
    READ_ADRS           ; 61
    LD  C,#14           ; 7 14
;=======================
    READ_DATA           ; 55

    LD  A,B             ; 
    OUT (PSGAD),A       ; 
    LD  A,D             ; 
    OUT (PSGWR),A       ; 89

    ; JOY2 Pin Read Mode
    LD  A,C             ; 
    OUT (PSGAD),A       ; 12 101
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	#0x1F           ; 
    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ;       ;Zero clear
    JP  (HL)            ; 94
    .endm
;=======================================================

	.macro __WRITE_OPLL_IO
__WRITE_OPLL_IO:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPLLAD),A      ; 17 88
;=======================

    READ_DATA           ; 55 
    LD  A,D             ; 
    OUT (OPLLWR),A      ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 30 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
    .endm
;=======================================================

	.macro __WRITE_SCC1
__WRITE_SCC1:
    READ_ADRS           ; 61
;=======================
0$:
    READ_DATA           ; 55

    LD  H,#0xB8         ; 
    LD  L,B             ; 
    LD  (HL), D         ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 34 89
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
    .endm
;=======================================================

	.macro __WRITE_SCC
__WRITE_SCC:
    READ_ADRS           ; 61
;=======================
0$:
    READ_DATA           ; 55

    LD  H,#0x98         ; 
    LD  L,B             ; 
    LD  (HL), D         ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 34 89
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
    .endm
;=======================================================

	.macro __WRITE_SCC1_2BYTES
__WRITE_SCC1_2BYTES:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  H,#0xB8         ;  
    LD  L,B             ; 13
;=======================
    WRITE_SCC_2BYTES
    .endm
;=======================================================

	.macro __WRITE_SCC_2BYTES
__WRITE_SCC_2BYTES:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  H,#0x98         ; 
    LD  L,B             ;  5 59
;=======================
    WRITE_SCC_2BYTES    ; 88
    .endm
;=======================================================

	.macro __WRITE_SCC1_32_BYTES
__WRITE_SCC1_32_BYTES:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  H,#0xB8         ; 
    LD  L,B             ; 
    LD  (HL), D         ; 21 76
;=======================
    WRITE_SCC_31_BYTES  ; 94
    .endm
;=======================================================

	.macro __WRITE_SCC_32_BYTES
__WRITE_SCC_32_BYTES:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  H,#0x98         ; 
    LD  L,B             ; 
    LD  (HL), D         ; 21 76
;=======================
    WRITE_SCC_31_BYTES  ; 94
    .endm
;=======================================================

	.macro __WRITE_OPL3_IO1
__WRITE_OPL3_IO1:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPL3AD1),A     ; 17 78
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (OPL3WR1),A      ; 17 72

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 13 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
    .endm
;=======================================================

	.macro __WRITE_OPL3_IO2
__WRITE_OPL3_IO2:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPL3AD2),A     ; 17 78
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (OPL3WR2),A      ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 30 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
    .endm

	.macro __WRITE_OPLL_MEM
	;https://www.msx.org/wiki/MSX-MUSIC_programming
__WRITE_OPLL_MEM:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    LD  (#0x7FF4),A     ; 19 80 
;=======================

    READ_DATA           ; 55
    LD  A,D             ; 
    LD  (#0x7FF5),A     ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 32 87
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
;=======================================================
    .endm
;=======================================================

	.macro __SELECT_OPM_SLOT
__SELECT_OPM_SLOT:
    READ_ADRS           ;61
;=======================
    READ_DATA           ;55
    INC D               ;
    DEC D               ;
    JP  NZ,OPM1_P0      ;21 76
;=======================

;OPM0のスロットのPAGE0を表に出す(要DIで実行)
OPM0_P0:
    LD  A,(OPM0_S)
    OR  A
    JP  Z,__VGM_LOOP
	LD	HL,(OPM0_S+2)       ; 47
    P0_CHG2                 ;143 200

    ; LD  A,(OPM0_S+3)
    ; OR  A
    ; JP  Z,__VGM_LOOP    ;

	; LD	A,(OPM0_S)
    ; CALL    P0_CHG;

	; LD	HL,(OPM0_S+1)
	; OUT	(#0xA8),A	    ;   P0+P3をOPM0の基本スロットに切り替え
	; LD	A,L
	; LD	(#0xFFFF),A	    ;   拡張スロット切り替え
	; LD	A,H
	; OUT	(#0xA8),A	    ;   P3をRAMに戻す

    JP __VGM_LOOP           ; 11 211

;OPM1のスロットのPAGE0を表に出す(要DIで実行)
OPM1_P0:
    LD  A,(OPM1_S)
    OR  A
    JP  Z,__VGM_LOOP
	LD	HL,(OPM1_S+2)
    P0_CHG2

    ; LD  A,(OPM1_S+3)
    ; OR  A
    ; JP  Z,__VGM_LOOP    ;

	; LD	A,(OPM1_S)
    ; CALL    P0_CHG;     ;300

	; LD	HL,(OPM1_S+1)
	; OUT	(#0xA8),A	    ;   P0+P3をOPM1の基本スロットに切り替え
	; LD	A,L
	; LD	(#0xFFFF),A	    ;   拡張スロット切り替え
	; LD	A,H
	; OUT	(#0xA8),A	    ;   P3をRAMに戻す

    JP __VGM_LOOP       ;111
;=================================
    .endm
;=======================================================

	.macro __WRITE_OPM_MEM
	;http://niga2.sytes.net/sp/eseopm.pdf
__WRITE_OPM_MEM:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    LD  (#0x3ff0),A     ; 19 80
;=======================
    READ_DATA           ; 55
    LD  A,D             ; 
    LD  (#0x3ff1),A     ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 32 87
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
    .endm
;=======================================================

	.macro __WRITE_DCSG
__WRITE_DCSG:
    READ_ADRS           ; 61
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (DCSGAD),A      ; 

    JP __VGM_LOOP       ; 28 93
    .endm

    .macro __WRITE_OPN2_IO1
__WRITE_OPN2_IO1:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPN2AD1),A     ; 17 78
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (OPN2WR1),A     ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 30 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
    .endm
;=======================================================

	.macro __WRITE_OPN2_IO2
__WRITE_OPN2_IO2:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPN2AD2),A     ; 17 78
;=======================
    READ_DATA           ; 55

    LD  A,D             ; 
    OUT (OPN2WR2),A     ; 

    ; Continuous Write
    INC B               ; 
    LD  E,#0x1F         ; 30 85
;=======================
1$:
    IN  A,(PSGRD)       ; 
    AND #0x20           ; 
    JP  Z, 1$           ; 
    IN  A,(PSGRD)       ; 
    AND	E               ; 
    CP  E               ; 
    JP  Z,0$            ;     ; Continuous Write

    OR  #JPOFST         ; 
    LD  H,A             ; 
    LD  L,#0            ; 
    JP  (HL)            ; 90  ; Jump to other ID
    .endm
;=======================================================

;====================================================================
;（参考）P0/P2のBIOSコールを使わないスロット切り替えルーチン
;====================================================================
;=======================================
;Page0 切り替えサブ
;入力：A Page0に割り当てるスロット
;
;注）以後BIOSコールと割り込みは使用不可
;	BCと裏AFは破壊される
;	DIしてからコールする
;=======================================

	.macro _P0_CHG
P0_CHG:
	PUSH	AF

	AND	#0b00000011
	LD	B,A
	IN	A,(#0xA8)	;基本スロット情報を読む
	AND	#0b11111100
	OR	B
	OUT	(#0xA8),A	;基本スロットP0をターゲットにする
	LD	C,A		;P3をRAMに戻す情報をCに入れる

	POP	AF
	BIT	7,A		;拡張スロット指定か？
	RET	Z

	;==============P3切り替え
	PUSH	AF
	EX	AF,AF'		;スロット番号を裏Aに退避
	RRC	B
	RRC	B		;基本スロット番号をP3の位置に
	
	LD	A,C
	AND	#0b00111111
	OR	B		;（以下スタック使用不可）
	OUT	(#0xA8),A	;P3をターゲットの基本スロットにする

	EX	AF,AF'		;裏Aからスロット番号を復帰
	RRCA
	RRCA
	AND	#0b00000011
	LD	B,A		;拡張スロット番号をP0の位置に
	LD	A,(#0xFFFF)	;拡張スロット情報を読む
	CPL
	AND	#0b11111100
	OR	B
	LD	(#0xFFFF),A	;拡張スロットをP0ターゲットにする

	LD	A,C
	OUT	(#0xA8),A	;P3をRAMに戻す

	;=============P3 切り替え完了（以下スタック使用可）

	POP	AF
	RET
    .endm

;=======================================
;Page1 切り替えサブ
;入力：A Page1に割り当てるスロット
;
;注）以後BIOSコールと割り込みは使用不可
;	BCと裏AFは破壊される
;	DIしてからコールする
;=======================================
	.macro _P1_CHG
P1_CHG:
	PUSH	AF
	RLCA
	RLCA
	AND	#0b00001100
	LD	B,A
	IN	A,(#0xA8)	;基本スロット情報を読む
	AND	#0b11110011
	OR	B
	OUT	(#0xA8),A	;基本スロットP1をターゲットにする
	LD	C,A		;P3をRAMに戻す情報をCに入れる

	POP	AF
	BIT	7,A		;拡張スロット指定か？
	RET	Z

	;==============P3切り替え
	PUSH	AF
	EX	AF,AF'		;スロット番号を裏Aに退避
	RLC	B
	RLC	B		;基本スロット番号をP3の位置に
    RLC	B
	RLC	B		;基本スロット番号をP3の位置に

	LD	A,C
	AND	#0b00111111
	OR	B		;（以下スタック使用不可）
	OUT	(#0xA8),A	;P3をターゲットの基本スロットにする

	EX	AF,AF'		;裏Aからスロット番号を復帰
	RLCA
	RLCA
   	RLCA
	RLCA
	AND	#0b00001100
	LD	B,A		;拡張スロット番号をP1の位置に
	LD	A,(#0xFFFF)	;拡張スロット情報を読む
	CPL
	AND	#0b11110011
	OR	B
	LD	(#0xFFFF),A	;拡張スロットをP1ターゲットにする

	LD	A,C
	OUT	(#0xA8),A	;P3をRAMに戻す

	;=============P3 切り替え完了（以下スタック使用可）

	POP	AF
	RET
    .endm

;=======================================
;Page2 切り替えサブ
;入力：A Page2に割り当てるスロット
;
;注）	A BCと裏AFは破壊される
;	DIしてからコールする
;=======================================
	.macro _P2_CHG
P2_CHG:
	PUSH	AF
	RLCA
	RLCA
	RLCA
	RLCA
	AND	#0b00110000
	LD	B,A
	IN	A,(#0xA8)	;基本スロット情報を読む
	AND	#0b11001111
	OR	B
	OUT	(#0xA8),A	;基本スロットP2をターゲットにする
	LD	C,A		;P3をRAMに戻す情報をCに入れる

	POP	AF
	BIT	7,A		;拡張スロット指定か？
	RET	Z

	;==============P3切り替え
	PUSH	AF
	EX	AF,AF'		;スロット番号を裏Aに退避
	RLC	B
	RLC	B		;基本スロット番号をP3の位置に
	
	LD	A,C
	AND	#0b00111111
	OR	B		;（以下スタック使用不可）
	OUT	(#0xA8),A	;P3をターゲットの基本スロットにする

	EX	AF,AF'		;裏Aからスロット番号を復帰
	RLCA
	RLCA
	AND	#0b00110000
	LD	B,A		;拡張スロット番号をP2の位置に
	LD	A,(#0xFFFF)	;拡張スロット情報を読む
	CPL
	AND	#0b11001111
	OR	B
	LD	(#0xFFFF),A	;拡張スロットをP2ターゲットにする

	LD	A,C
	OUT	(#0xA8),A	;P3をRAMに戻す

	;=============P3 切り替え完了（以下スタック使用可）

	POP	AF
	RET
    .endm

;=======================================
;Page0 切り替えサブ
;入力：H,L Page0に割り当てる基本スロット(+3),拡張スロット(+2)
;
;注）  A,Cは破壊される
;	DIしてからコールする
;=======================================

    .macro P0_CHG2  ; 143
	in	a,(#0xA8)	;Read primary slots register
	ld	C,a		;Store current configuration
	and	#0b00111100	;Resets the bits of pages 0000h-03FFFh and C000h-0FFFFh
    OR  H
	out	(#0xA8),a	;Select primary slot H for the pages 0000h-03FFFh and C000h-0FFFFh

	ld	a,(#0xFFFF)	;Read secondary slots register of selected primary slot
	cpl			;Reverses the bits
	and	#0b11111100	;Resets the bits of page 0000h-03FFFh
    OR  L
	ld	(#0xFFFF),a	;Select secondary slot H-L

	ld	a,C		;Restore initial configuration of primary slots to A
	and	#0b11000000	;Keep only the bits of page C000h-0FFFFh
	ld	C,a		;Store the bits of page C000h-0FFFFh to C
	in	a,(#0xA8)	;Read primary slots register
	and	#0b00111111	;Resets the bits of page C000h-0FFFFh
	or	C		;Sets the bits of page C000h-0FFFF to initial value
	out	(#0xA8),a	;Select initial primary slot for the page C000h-0FFFFh
    .endm

;=======================================
;Page1 切り替えサブ
;入力：H,L Page1に割り当てる基本スロット(+3),拡張スロット(+2)
;
;注）  A,Cは破壊される
;	DIしてからコールする
;=======================================

    .macro P1_CHG2  ; 143
	in	a,(#0xA8)	;Read primary slots register
	ld	C,a		;Store current configuration
	and	#0b00110011	;Resets the bits of pages 4000h-07FFFh and C000h-0FFFFh
    OR  H
	out	(#0xA8),a	;Select primary slot H for the pages 4000h-07FFFh and C000h-0FFFFh

	ld	a,(#0xFFFF)	;Read secondary slots register of selected primary slot
	cpl			;Reverses the bits
	and	#0b11110011	;Resets the bits of page 4000h-07FFFh 
    OR  L
	ld	(#0xFFFF),a	;Select secondary slot H-L

	ld	a,C		;Restore initial configuration of primary slots to A
	and	#0b11000000	;Keep only the bits of page C000h-0FFFFh
	ld	C,a		;Store the bits of page C000h-0FFFFh to C
	in	a,(#0xA8)	;Read primary slots register
	and	#0b00111111	;Resets the bits of page C000h-0FFFFh
	or	C		;Sets the bits of page C000h-0FFFF to initial value
	out	(#0xA8),a	;Select initial primary slot for the page C000h-0FFFFh
    .endm

;=======================================
;Page2 切り替えサブ
;入力：H,L Page2に割り当てる基本スロット(+3),拡張スロット(+2)
;
;注）  A,Cは破壊される
;	DIしてからコールする
;=======================================

    .macro P2_CHG2  ; 143
	in	a,(#0xA8)	;Read primary slots register
	ld	C,a		;Store current configuration
	and	#0b00001111	;Resets the bits of pages 8000h-0BFFFh and C000h-0FFFFh
    OR  H
	out	(#0xA8),a	;Select primary slot H for the pages 8000h-0BFFFh and C000h-0FFFFh

	ld	a,(#0xFFFF)	;Read secondary slots register of selected primary slot
	cpl			;Reverses the bits
	and	#0b11001111	;Resets the bits of page 8000h-0BFFFh 
    OR  L
	ld	(#0xFFFF),a	;Select secondary slot H-L

	ld	a,C		;Restore initial configuration of primary slots to A
	and	#0b11000000	;Keep only the bits of page C000h-0FFFFh
	ld	C,a		;Store the bits of page C000h-0FFFFh to C
	in	a,(#0xA8)	;Read primary slots register
	and	#0b00111111	;Resets the bits of page C000h-0FFFFh
	or	C		;Sets the bits of page C000h-0FFFF to initial value
	out	(#0xA8),a	;Select initial primary slot for the page C000h-0FFFFh
    .endm