.setcpu		"6502"
.autoimport	on
.export		_VGMPlay_FTDI2XX_DIRECT
.export		_VGMPlay_FTDI2XX_INDIRECT

; 115200bps 15.54clk @ 1.79 MHz (NTSC)
;  57600bps 31.08clk @ 1.79 MHz (NTSC)

.segment	"FILE0_DAT"

.macro FTDI2XX_CORE	INST
.scope
;TEST SOUNDS
;lda #%00000001
;sta $4015
;lda #%10111111
;sta $4000
;lda #$00
;sta $4001
;lda #%11010101
;sta $4002
;lda #$00
;sta $4003

	lda		#%00000000
	sta		$2000			; disable VINT
	SEI						; disable IRQ
LoopS:
	lda		#$02	; 2	2
Loop1_1:					; Retreive Address	Hi
	bit		$4016	; 4 6
	bne     Loop1_1	; 2	8

	lda		$4017	; 4 12
	asl		a		; 2 14
	asl		a		; 2 16
	asl		a		; 2 18
	and		#$F0	; 2 20
	sta		$f0		; 3 23

	lda		#$02	; 2	2
Loop1_2:					; Retreive Address	Lo
	bit		$4016	; 4 6
	beq     Loop1_2	; 2	8

	lda		$4017	; 4 12
	lsr		a		; 2 14
	and		#$0F	; 2 16
	ora		$f0		; 3 19
	tax				; 2 21	X = address

	lda		#$02	; 2	2
Loop2_1:					; Retreive Data	Hi
	bit		$4016	; 4 6
	bne     Loop2_1	; 2	8

	lda		$4017	; 4 12
	asl		a		; 2 14
	asl		a		; 2 16
	asl		a		; 2 18
	and		#$F0	; 2 20
	sta		$f1		; 3 23

	lda		#$02	; 2	2
Loop2_2:					; Retreive Data	Lo
	bit		$4016	; 4 6
	beq     Loop2_2	; 2	8

	lda		$4017	; 4 12
	lsr		a		; 2 14
	and		#$0F	; 2 16
	ora		$f1		; 3 19

	INST

	jmp		LoopS	; 3 28
.endscope
.endmacro

_VGMPlay_FTDI2XX_DIRECT:
	FTDI2XX_CORE {sta $4000,x}	; 6 24	write A data to address

_VGMPlay_FTDI2XX_INDIRECT:
	FTDI2XX_CORE {sta ($80,x)}	; 6 25	write A data to address
