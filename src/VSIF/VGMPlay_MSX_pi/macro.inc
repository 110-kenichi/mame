;=======================================================
    .macro READ_UART_STAT
    IN  A,(UART_STAT)   ; 12
.if tR
    IN  A,(UART_STAT)   ; 12
.endif
    .endm

    .macro READ_ADRS
5$:
    READ_UART_STAT
.if UART_MODE_135
    CP  E               ;  5
    JP  NZ, 5$          ; 11 28
.else
    AND E               ;  5
    JP  Z, 5$   ; 11
.endif
    IN  A,(UART_READ)   ; 12 40
    LD  B, A            ;  5 45
    .endm
;=======================================================

    .macro READ_DATA
3$:
    READ_UART_STAT
.if UART_MODE_135
    CP  E               ;  5
    JP  NZ, 3$          ; 11 28
.else
    AND E               ;  5
    JP  Z, 3$           ; 11
.endif
    IN  A,(UART_READ)   ; 12 40
    .endm
;=======================================================

    .macro CONT_WRITE
    LD  L,#0            ;  8 67
;=======================
1$:
    READ_UART_STAT
.if UART_MODE_135
    CP  E               ;  5
    JP  NZ, 1$          ; 11 28
.else
    AND E               ;  5
    JP  Z, 1$           ; 11
.endif
    IN  A,(UART_READ)   ; 12 40
    AND	#0x1F           ;  8 48
    JP  Z,0$            ; 11 59 ; Continuous Write

    OR  #JPOFST         ;  8 67
    LD  H,A             ;  5 72
    JP  (HL)            ;  5 77 ; Jump to other ID
    .endm
;=======================================================

	.macro __WRITE_ANY_IO
__WRITE_ANY_IO:
    READ_ADRS           ; 45
;=======================
0$:
    READ_DATA           ; 40

    LD  C,B             ;  5 45
    OUT (C), A          ; 14 59

    CONT_WRITE
    .endm
;=======================================================

	.macro __WRITE_PSG_IO
__WRITE_PSG_IO:
    READ_ADRS           ; 45
    OUT (PSGAD),A       ; 12 57
;=======================
0$:
    READ_DATA           ; 45
    OUT (PSGWR),A       ; 12 57

    CONT_WRITE
    .endm

;=======================================================

	.macro __WRITE_OPLL_IO
__WRITE_OPLL_IO:
    READ_ADRS           ; 45
0$:
    LD  A,B             ;  5 50
    OUT (OPLLAD),A      ; 17 67
;=======================

    READ_DATA           ; 40 
    OUT (OPLLWR),A      ; 12 62

    ; Continuous Write
    INC B               ;  5 67
    CONT_WRITE
    .endm

;=======================================================
    .macro WRITE_SCC_2BYTES
1$:
    READ_UART_STAT
.if UART_MODE_135
    CP  E               ;  5
    JP  NZ, 1$          ; 11 28
.else
    AND E               ;  5
    JP  Z, 1$           ; 11
.endif
    IN  A,(UART_READ)   ; 12 40

    ;WRITE BOTH 1st BYTE AND 2nd BYTE
    LD  (HL), D         ;  8 48
    INC L               ;  5 53
    LD  (HL), A         ;  8 61
    JP  __VGM_LOOP      ; 11 72
    .endm

;=======================================================
	.macro __WRITE_SCC1
__WRITE_SCC1:
    READ_ADRS           ; 45
    LD  H,#0xB8         ;  8 53
;=======================
0$:
    READ_DATA           ; 40

    LD  L,B             ;  5 45
    LD  (HL), A         ;  8 53

    INC B               ;  5 58
    CONT_WRITE
    .endm
;=======================================================

	.macro __WRITE_SCC
__WRITE_SCC:
    READ_ADRS           ; 45
    LD  H,#0x98         ;  8 53
;=======================
0$:
    READ_DATA           ; 40

    LD  L,B             ;  5 45
    LD  (HL), A         ;  8 53

    INC B               ;  5 58
    CONT_WRITE
    .endm
;=======================================================

	.macro __WRITE_SCC1_2BYTES
__WRITE_SCC1_2BYTES:
    READ_ADRS           ; 45
    LD  H,#0xB8         ;  8 58
;=======================
    READ_DATA           ; 40
    LD  D,A             ;  5 50

    LD  L,B             ;  5 63
;=======================
    WRITE_SCC_2BYTES
    .endm
;=======================================================

	.macro __WRITE_SCC_2BYTES
__WRITE_SCC_2BYTES:
    READ_ADRS           ; 45
    LD  H,#0x98         ;  8
;=======================
    READ_DATA           ; 40
    LD  D,A             ;  5 55

    LD  L,B             ;  5 68
;=======================
    WRITE_SCC_2BYTES    ; 88
    .endm

;=======================================================
    .macro WRITE_SCC_31_BYTES
    LD  B,#31           ;  8  8
1$:
    READ_UART_STAT
.if UART_MODE_135
    CP  E               ;  5
    JP  NZ, 1$          ; 11 28
.else
    AND E               ;  5
    JP  Z, 1$           ; 11
.endif
    IN  A,(UART_READ)   ; 12 40

    ;WRITE NEXT BYTE
    INC L               ;  5 
    LD  (HL), A         ;  8 
    DJNZ 1$             ; 14 83  9 78
    JP  __VGM_LOOP      ; 11 94 11 89
    .endm

;=======================================================

	.macro __WRITE_SCC1_32_BYTES
__WRITE_SCC1_32_BYTES:
    READ_ADRS           ; 45
    LD  H,#0xB8         ; 
;=======================
    READ_DATA           ; 40

    LD  L,B             ;  5
    LD  (HL), A         ;  8 53
;=======================
    WRITE_SCC_31_BYTES  ; 94
    .endm
;=======================================================

	.macro __WRITE_SCC_32_BYTES
__WRITE_SCC_32_BYTES:
    READ_ADRS           ; 45
    LD  H,#0x98         ; 
;=======================
    READ_DATA           ; 40

    LD  L,B             ;  5
    LD  (HL), A         ;  8 53
;=======================
    WRITE_SCC_31_BYTES  ; 94
    .endm
;=======================================================

	.macro __WRITE_OPL3_IO1
__WRITE_OPL3_IO1:
    READ_ADRS           ; 45
0$:
    LD  A,B             ;  5 50
    OUT (OPL3AD1),A     ; 17 67
    CP  #0x0F           ;  8 75
    JP  Z, __WRITE_OPL3_IO1_ADPCM           ; 11 97
;=======================
    READ_DATA           ; 40

    OUT (OPL3WR1),A     ; 12 52

    ; Continuous Write
    INC B               ;  5 67
    CONT_WRITE

;=======================  ADPCM burst write mode
__WRITE_OPL3_IO1_ADPCM:
0$:
    READ_DATA           ; 40

    OUT (OPL3WR1),A     ; 12 62

    CONT_WRITE
    .endm
;=======================================================

	.macro __WRITE_OPL3_IO2
__WRITE_OPL3_IO2:
    READ_ADRS           ; 45
0$:
    LD  A,B             ; 
    OUT (OPL3AD2),A     ; 17 78
    CP  #0x0F           ;  8 86
    JP  Z, __WRITE_OPL3_IO2_ADPCM           ; 11 97
;=======================
    READ_DATA           ; 40

    OUT (OPL3WR2),A     ; 52 

    INC B               ;  5 57
    CONT_WRITE
;=======================
__WRITE_OPL3_IO2_ADPCM:
0$:
    READ_DATA           ; 45

    OUT (OPL3WR2),A     ; 12 57

    CONT_WRITE
    .endm

	.macro __WRITE_OPLL_MEM
	;https://www.msx.org/wiki/MSX-MUSIC_programming
__WRITE_OPLL_MEM:
    READ_ADRS           ; 45
0$:
    LD  A,B             ;  5
    LD  (#0x7FF4),A     ; 14 64 
;=======================

    READ_DATA           ; 40
    LD  (#0x7FF5),A     ; 14 54

    INC B               ;  5 59
    CONT_WRITE
;=======================================================
    .endm
;=======================================================

	.macro __SELECT_OPM_SLOT
__SELECT_OPM_SLOT:
    READ_ADRS           ;45
;=======================
    READ_DATA           ;40
    INC A               ;
    DEC A               ;
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
    READ_ADRS           ; 45
0$:
    LD  A,B             ;  5
    LD  (#0x3ff0),A     ; 14
;=======================
    READ_DATA           ; 40
    LD  (#0x3ff1),A     ; 14 54

    INC B               ;  5 59
    CONT_WRITE
    .endm
;=======================================================

	.macro __WRITE_DCSG
__WRITE_DCSG:
    READ_ADRS           ; 45
;=======================
    READ_DATA           ; 40

    OUT (DCSGAD),A      ; 12

    JP __VGM_LOOP       ; 11 73
    .endm

    .macro __WRITE_OPN2_IO1
__WRITE_OPN2_IO1:
    READ_ADRS           ; 45
0$:
    LD  A,B             ; 
    OUT (OPN2AD1),A     ; 17 78
;=======================
    READ_DATA           ; 40

    OUT (OPN2WR1),A     ; 12 62

    INC B               ;  5 67
    CONT_WRITE
    .endm
;=======================================================

	.macro __WRITE_OPN2_IO2
__WRITE_OPN2_IO2:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPN2AD2),A     ; 17 78
    CP  #0x08           ;  8 86
    JP  Z, __WRITE_OPN2_IO2_ADPCM           ; 11 97
;=======================
    READ_DATA           ; 50

    OUT (OPN2WR2),A     ; 12 62

    INC B               ;  5 67
    CONT_WRITE
;=======================  ADPCM burst write mode
__WRITE_OPN2_IO2_ADPCM:
0$:
    READ_DATA           ; 50

    OUT (OPN2WR2),A     ; 12 62

    CONT_WRITE
    .endm
;=======================================================

    .macro __WRITE_OPN_IO
__WRITE_OPN_IO:
    READ_ADRS           ; 61
0$:
    LD  A,B             ; 
    OUT (OPNAD),A       ; 17 78
;=======================
    READ_DATA           ; 50

    OUT (OPNWR),A       ; 12 62 

    INC B               ;  5 67
    CONT_WRITE
    .endm
;=======================================================

    .macro __WRITE_OPNA_PSEUDO_DAC
__WRITE_OPNA_PSEUDO_DAC:
    LD  A,#0xB          ;  8
    OUT (OPN2AD2),A     ; 12 20
0$:
;=======================
    READ_DATA           ; 40

    OUT (OPN2WR2),A     ; 12 62 

    CONT_WRITE  ;DAC
    .endm
;=======================================================

    .macro __WRITE_OPNA_DAC
__WRITE_OPNA_DAC:
    LD  A,#0xE          ;  8
    OUT (OPN2AD2),A     ; 12 20
0$:
;=======================
    READ_DATA           ; 40

    OUT (OPN2WR2),A     ; 12 62 

    CONT_WRITE
    .endm
;=======================================================

    .macro __WRITE_OPN2_DAC
__WRITE_OPN2_DAC:
    LD  A,#0x2A         ;  8
    OUT (OPN2AD1),A     ; 12 20
0$:
;=======================
    READ_DATA           ; 40

    OUT (OPN2WR1),A     ; 12 62

    CONT_WRITE  ;DAC
    .endm
;=======================================================

    .macro __WRITE_TR_DAC
__WRITE_TR_DAC:
    LD  C,#14           ; 7 14
0$:
;=======================
    READ_DATA           ; 40

    OUT (TRDAC),A       ; 12 52 

    CONT_WRITE  ;DAC
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