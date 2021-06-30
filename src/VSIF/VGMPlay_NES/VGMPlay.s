.setcpu		"6502"
.autoimport	on
.export		_VGMPlay_FTDI2XX_DIRECT
.export		_VGMPlay_FTDI2XX_INDIRECT

; 115200bps 15.54clk @ 1.789773 MHz (NTSC)
;  57600bps 31.07clk @ 1.789773 MHz (NTSC)
;  57600bps 28.86clk @ 1.662607 MHz (PAL)
;  31250bps 57.27clk @ 1.789773 MHz (NTSC)

.segment	"FILE0_DAT"

.macro FTDI2XX_BITBANG	ADRS
.scope
	lda		#$02	; 2	2
:					; Retreive Address	Hi
	bit		$4016	; 4 6
	bne     :-		; 2	8

	lda		$4017	; 4 12
	asl		a		; 2 14
	asl		a		; 2 16
	asl		a		; 2 18
	and		#$F0	; 2 20
	sta		ADRS	; 3 23

	lda		#$02	; 2	2
:					; Retreive Address	Lo
	bit		$4016	; 4 6
	beq     :-		; 2	8

	lda		$4017	; 4 12
	lsr		a		; 2 14
	and		#$0F	; 2 16
	ora		ADRS	; 3 19
.endscope
.endmacro

.macro FTDI2XX_DPCM ADRS
.scope
	lda		ADRS				;  2 25 31
	sta		$73					;  2 27 33	//DPCM BUFFER ADDRESS(HI)
Loop:
	FTDI2XX_BITBANG	$7f			; 19 19
	sta		($72),Y				;  6 25
	iny							;  2 27
	bne		Loop				;  2 29
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
	FTDI2XX_BITBANG	$70		; 19 19
	cmp		DPCM_CMD		;  2 21 
	beq		LoadDPCM		;  2 23
	tax						;  2 25	X = address/idx

	FTDI2XX_BITBANG	$71		; 19 19
	WR_DATA					; 5/6	24/25
	jmp		LoopBITBANG		; 3 27/28

LoadDPCM:
	FTDI2XX_BITBANG	$71		; 19 19	//dummy
	ldy		#0				;  2 29 27
	
	FTDI2XX_DPCM	#$c0
	FTDI2XX_DPCM	#$c1
	FTDI2XX_DPCM	#$c2
	FTDI2XX_DPCM	#$c3
	FTDI2XX_DPCM	#$c4
	FTDI2XX_DPCM	#$c5
	FTDI2XX_DPCM	#$c6
	FTDI2XX_DPCM	#$c7
	FTDI2XX_DPCM	#$c8
	FTDI2XX_DPCM	#$c9
	FTDI2XX_DPCM	#$ca
	FTDI2XX_DPCM	#$cb
	FTDI2XX_DPCM	#$cc
	FTDI2XX_DPCM	#$cd
	FTDI2XX_DPCM	#$ce
	FTDI2XX_DPCM	#$cf
	FTDI2XX_DPCM	#$d0
	FTDI2XX_DPCM	#$d1
	FTDI2XX_DPCM	#$d2
	FTDI2XX_DPCM	#$d3
	FTDI2XX_DPCM	#$d4
	FTDI2XX_DPCM	#$d5
	FTDI2XX_DPCM	#$d6
	FTDI2XX_DPCM	#$d7
	FTDI2XX_DPCM	#$d8
	FTDI2XX_DPCM	#$d9
	FTDI2XX_DPCM	#$da
	FTDI2XX_DPCM	#$db
	FTDI2XX_DPCM	#$dc
	FTDI2XX_DPCM	#$dd
	FTDI2XX_DPCM	#$de
	FTDI2XX_DPCM	#$df
	jmp		LoopBITBANG			; 3 27/28
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
