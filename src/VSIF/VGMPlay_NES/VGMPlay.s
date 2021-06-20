.setcpu		"6502"
.autoimport	on
.export		_VGMPlay_FTDI2XX_DIRECT
.export		_VGMPlay_FTDI2XX_INDIRECT

; 115200bps 15.54clk @ 1.79 MHz (NTSC)
;  57600bps 31.08clk @ 1.79 MHz (NTSC)

.segment	"CODE"

_VGMPlay_FTDI2XX_DIRECT:
	lda		#%00000000
	sta		$2000			; disable VINT
	SEI						; disable IRQ
DLoopS:
	lda		#$02	; 2	2
DLoop1_1:					; Retreive Address	Hi
	bit		$4016	; 4 6
	bne     DLoop1_1	; 2	8

	lda		$4017	; 4 12
	asl		a		; 2 14
	asl		a		; 2 16
	asl		a		; 2 18
	and		#$F0	; 2 20
	sta		$f0		; 3 23

	lda		#$02	; 2	2
DLoop1_2:					; Retreive Address	Lo
	bit		$4016	; 4 6
	beq     DLoop1_2	; 2	8

	lda		$4017	; 4 12
	lsr		a		; 2 14
	and		#$0F	; 2 16
	ora		$f0		; 3 19
	tax				; 2 21	X = address

	lda		#$02	; 2	2
DLoop2_1:					; Retreive Data	Hi
	bit		$4016	; 4 6
	bne     DLoop2_1	; 2	8

	lda		$4017	; 4 12
	asl		a		; 2 14
	asl		a		; 2 16
	asl		a		; 2 18
	and		#$F0	; 2 20
	sta		$f1		; 3 23

	lda		#$02	; 2	2
DLoop2_2:					; Retreive Data	Lo
	bit		$4016	; 4 6
	beq     DLoop2_2	; 2	8

	lda		$4017	; 4 12
	lsr		a		; 2 14
	and		#$0F	; 2 16
	ora		$f1		; 3 19

	sta		$4000,x	; 5 24	write A data to address

	jmp		DLoopS	; 3 28


_VGMPlay_FTDI2XX_INDIRECT:
	lda		#%00000000
	sta		$2000			; disable VINT
	SEI						; disable IRQ
ILoopS:
	lda		#$02	; 2	2
ILoop1_1:					; Retreive Address	Hi
	bit		$4016	; 4 6
	bne     ILoop1_1	; 2	8

	lda		$4017	; 4 12
	asl		a		; 2 14
	asl		a		; 2 16
	asl		a		; 2 18
	and		#$F0	; 2 20
	sta		$f0		; 3 23

	lda		#$02	; 2	2
ILoop1_2:					; Retreive Address	Lo
	bit		$4016	; 4 6
	beq     ILoop1_2	; 2	8

	lda		$4017	; 4 12
	lsr		a		; 2 14
	and		#$0F	; 2 16
	ora		$f0		; 3 19
	tax				; 2 21	X = address

	lda		#$02	; 2	2
ILoop2_1:					; Retreive Data	Hi
	bit		$4016	; 4 6
	bne     ILoop2_1	; 2	8

	lda		$4017	; 4 12
	asl		a		; 2 14
	asl		a		; 2 16
	asl		a		; 2 18
	and		#$F0	; 2 20
	sta		$f1		; 3 23

	lda		#$02	; 2	2
ILoop2_2:					; Retreive Data	Lo
	bit		$4016	; 4 6
	beq     ILoop2_2	; 2	8

	lda		$4017	; 4 12
	lsr		a		; 2 14
	and		#$0F	; 2 16
	ora		$f1		; 3 19

	sta		($80,x)	; 6 25	write A data to address

	jmp		ILoopS	; 3 28


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
