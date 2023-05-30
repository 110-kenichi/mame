.setcpu		"6502"
.autoimport	on
.export		_VGMPlay_FTDI2XX_DIRECT
.export		_VGMPlay_FTDI2XX_INDIRECT

; 115200bps 15.54clk @ 1.789773 MHz (NTSC)
;  57600bps 31.07clk @ 1.789773 MHz (NTSC)
;  57600bps 28.86clk @ 1.662607 MHz (PAL)
;  31250bps 57.27clk @ 1.789773 MHz (NTSC)

.segment	"FILE0_DAT"

;http://hp.vector.co.jp/authors/VA042397/nes/6502.html#calculate
;https://www.nesdev.org/wiki/Input_devices#Other_I/O_devices
;https://www.nesdev.org/wiki/Expansion_port

.macro FTDI2XX_BITBANG_A	ADRS
.scope
	lda		#$02	; 2	2
:					; Retreive Address	Hi
	bit		$4016	; 4 6
	beq     :-		; 2	8
	bit		$4017	; 4 12
	bne     Normal	; 2	14
PlayDac:
	lda		$4017	; 4 18
	and		#$1C	; 2 20
	asl		a		; 2 22
	asl		a		; 2 24
	sta		ADRS	; 3 29
;;;;;;;;;;;;;;;;;;;;;;;;;;;
	lda		#$02	; 2	2
:					; Retreive Address	Mid
	bit		$4016	; 4 6
	bne     :-		; 2	8

	lda		$4017	; 4 18
	and		#$1E	; 2 20
	lsr		a		; 2 22
	ora		ADRS	; 3 25
	sta 	$4011	; 4 29
	jmp		LoopBITBANG
Normal:
	lda		$4017	; 4 18
	and		#$1C	; 2 20
	asl		a		; 2 22
	asl		a		; 2 24
	asl		a		; 2 26
	sta		ADRS	; 3 29
;;;;;;;;;;;;;;;;;;;;;;;;;;;
	lda		#$02	; 2	2
:					; Retreive Address	Mid
	bit		$4016	; 4 6
	bne     :-		; 2	8
:					; Retreive Address	Mid
	bit		$4017	; 4 12
	bne     :-		; 2	14

	lda		$4017	; 4 18
	and		#$1C	; 2 20
	ora		ADRS	; 3 23
	sta     ADRS	; 3 26
;;;;;;;;;;;;;;;;;;;;;;;;;;;
	lda		#$02	; 2	2
:					; Retreive Address	Lo
	bit		$4016	; 4 6
	bne     :-		; 2	8
:					; Retreive Address	Lo
	bit		$4017	; 4 12
	beq     :-		; 2	14

	lda		$4017	; 4 18
	and		#$0C	; 2 20
	lsr		a		; 2 22
	lsr		a		; 2 24
	ora		ADRS	; 3 27

.endscope
.endmacro

.macro FTDI2XX_BITBANG_D	ADRS
.scope
	lda		#$02	; 2	2
:					; Retreive Data	Hi
	bit		$4016	; 4 6
	bne     :-		; 2	8
:					; Retreive Data	Hi
	bit		$4017	; 4 12
	bne     :-		; 2	14

	lda		$4017	; 4 18
	and		#$1C	; 2 20
	asl		a		; 2 22
	asl		a		; 2 24
	asl		a		; 2 26
	sta		ADRS	; 3 29
;;;;;;;;;;;;;;;;;;;;;;;;;;;
	lda		#$02	; 2	2
:					; Retreive Data	Mid
	bit		$4016	; 4 6
	bne     :-		; 2	8
:					; Retreive Data	Mid
	bit		$4017	; 4 12
	beq     :-		; 2	14

	lda		$4017	; 4 18
	and		#$1C	; 2 20
	ora		ADRS	; 3 23
	sta     ADRS	; 3 26
;;;;;;;;;;;;;;;;;;;;;;;;;;;
	lda		#$02	; 2	2
:					; Retreive Data	Lo
	bit		$4016	; 4 6
	bne     :-		; 2	8
:					; Retreive Data	Lo
	bit		$4017	; 4 12
	bne     :-		; 2	14

	lda		$4017	; 4 18
	and		#$0C	; 2 20
	lsr		a		; 2 22
	lsr		a		; 2 24
	ora		ADRS	; 3 27

.endscope
.endmacro

.macro FTDI2XX_DPCM ADRS
.scope
	lda		ADRS				;  2 25 31
	sta		$73					;  2 27 33	//DPCM BUFFER ADDRESS(HI)
Loop:
	FTDI2XX_BITBANG_A	$7f		; 27 27
	sta		($72),Y				;  6 33
	iny							;  2 35
	FTDI2XX_BITBANG_D	$7f		; 27 27
	sta		($72),Y				;  6 33
	iny							;  2 35
	bne		Loop				;  2 37

	jmp		Loop
LoopEnd:
.endscope
.endmacro

.macro FTDI2XX_CORE	WR_DATA,DPCM_CMD
.scope
	lda		#%00000000
	sta		$2000			; disable VINT
	sei						; disable IRQ

	lda		#00				;  2
	sta		$72				;  2	//DPCM BUFFER ADDRESS(LO)
LoopBITBANG:
	FTDI2XX_BITBANG_A	$70	; 27 27
	cmp		DPCM_CMD		;  2 29 
	beq		LoadDPCM		;  2 31
	tax						;  2 33	X = address/idx

	FTDI2XX_BITBANG_D	$71	; 27 27
	WR_DATA					; 5/6	32/33
	jmp		LoopBITBANG		; 3 35/36

LoadDPCM:
	FTDI2XX_BITBANG_D	$71		; 27 27	//dummy

	ldy		#0					;  2 29 27
	lda		#$c0				;  2 31
	sta		$74					;  2 33
.scope
LoadLoopStart:
	sta		$73					;  2  2 //DPCM BUFFER ADDRESS(HI)
Loop256:
	FTDI2XX_BITBANG_A	$7f		; 27 29
	sta		($72),Y				;  6 35
	iny							;  2 37
	FTDI2XX_BITBANG_D	$7f		; 27 27
	sta		($72),Y				;  6 33
	iny							;  2 35
	beq		NextLoop			;  2 37
	jmp		Loop256				;  3 40
NextLoop:
	inc		$74					;  5 45
	lda		#$e0				;  2 47
	cmp		$74					;  2 49
	beq		EndLoop				;  2 51
	jmp		LoadLoopStart		;  3 54
EndLoop:
.endscope
	jmp		LoopBITBANG			;  3 57
.endscope
.endmacro

.macro TEST_SOUNDS_ON
.scope
	lda #%00000001
	sta $4015
	lda #%10111111
	sta $4000
	lda #$00
	sta $4001
	lda #%11010101
	sta $4002
	lda #$00
	sta $4003
	ldx	#$80
LoopX:
	ldy	#0
LoopY:
	iny
	bne	LoopY
	inx
	bne	LoopX
.endscope
.endmacro

.macro TEST_SOUNDS_OFF
.scope
	lda #%00000000
	sta $4015
	ldx	#$80
LoopX:
	ldy	#0
LoopY:
	iny
	bne	LoopY
	inx
	bne	LoopX
.endscope
.endmacro

_VGMPlay_FTDI2XX_DIRECT:
	TEST_SOUNDS_ON
	TEST_SOUNDS_OFF
	TEST_SOUNDS_ON
	TEST_SOUNDS_OFF

	FTDI2XX_CORE {sta $4000,x},{#$16}	; 6 24	write A data to address

_VGMPlay_FTDI2XX_INDIRECT:
	TEST_SOUNDS_ON
	TEST_SOUNDS_OFF
	TEST_SOUNDS_ON
	TEST_SOUNDS_OFF

	FTDI2XX_CORE {sta ($80,x)},{#$2C}	; 6 25	write A data to address
