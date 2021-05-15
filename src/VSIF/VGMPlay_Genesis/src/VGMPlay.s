    .globl VGMPlay

    |230400bps 65.98clk @ 7.600489 MHz (PAL)
    |230400bps 33.30clk @ 7.670454 MHz (NTSC)
    |115200bps 65.98clk @ 7.600489 MHz (PAL)
    |115200bps 66.58clk @ 7.670454 MHz (NTSC)

    .equ    PERIPHERAL_PORT, 0xA10005   | P2 PORT
| .equ    PERIPHERAL_PORT, 0xA10007  | EXT PORT

    .macro start_bit_wait_230K
    NOP                     | 4
    .endm   | 4

    .macro start_bit_wait_115K
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    .endm   | 36

    .macro start_bit_wait
    |start_bit_wait_230K
    start_bit_wait_115K
    .endm

    .macro stop_bit_wait_230K
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    .endm   | 16

    .macro stop_bit_wait_115K
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    .endm   | 28

    .macro stop_bit_wait
    |stop_bit_wait_230K
    stop_bit_wait_115K
    .endm   | 36

	.macro sample_bit_230K
    NOP                     | 4
    move.b  (%a0), %d0      | 8
    roxr.b  #1,%d0 			| 8
    roxr.b  #1,%d1 			| 8
	.endm   | 32
    
	.macro sample_bit_114K
    NOP                     | 4
    NOP                     | 4
    move.b  (%a0), %d0      | 8
    roxr.b  #1,%d0 			| 8
    roxr.b  #1,%d1 			| 8

    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
    NOP                     | 4
	.endm   | 64

    .macro sample_bit
    |sample_bit_230K | 32
    sample_bit_114K | 36
	.endm

	.macro sample_bit_230K_nowait
    NOP                     | 4
    move.b  (%a0), %d0      | 8
    roxr.b  #1,%d0 			| 8
    roxr.b  #1,%d1 			| 8
	.endm   | 28

	.macro sample_bit_114K_nowait
    NOP                     | 4
    NOP                     | 4
    move.b  (%a0), %d0      | 8
    roxr.b  #1,%d0 			| 8
    roxr.b  #1,%d1 			| 8
	.endm   | 32

    .macro sample_bit_nowait
    |sample_bit_230K_nowait   | 28
    sample_bit_114K_nowait  | 28
	.endm

    .macro sample_bits_230K
	sample_bit 		| 32 +0.5
	sample_bit 		| 32 -0.5
	sample_bit 		| 32 -1.0
	sample_bit 		| 32 -1.5
	sample_bit 		| 32 -2.0
    nop
	sample_bit 		| 32  2.0
	sample_bit 		| 32  1.5
	sample_bit_nowait | 28
    .endm

    .macro sample_bits_114K
    nop             | 4
	sample_bit 		| 64 +3.5
	sample_bit 		| 64 -0.5
    nop             | 4 
	sample_bit 		| 64 +3.5
	sample_bit 		| 64 -0.5
    nop             | 4 
	sample_bit 		| 64 +3.5
	sample_bit 		| 64 -0.5
    nop             | 4 
	sample_bit 		| 64 +3.5
	sample_bit_nowait | 32
    .endm
    
    .macro sample_bits
    |sample_bits_230K
    sample_bits_114K
    .endm

|115200bps 66.58clk @ 7.670454 MHz (NTSC)

VGMPlay:
    move.b  #1,0xA11200
    move.b  #1,0xA11100
Reset:
    btst.b  #0,0xA11100
    bne.b   Reset

    move.l  #PERIPHERAL_PORT, %a0   | 12
    move.l  #ADRESS_TABLE, %a1      | 12

_VGM_ADDRESS:
    move.b  (%a0), %d0      | +8
    btst.l  #0,%d0          | +10 18
    bne.b   _VGM_ADDRESS    | +8  26
    clr.l   %d1             | +4  30
    start_bit_wait          | +36 66

    sample_bits
    
    andi.b #0xfc,%d1        | + 8
    move.l (%d1, %a1), %a2  | +16
    stop_bit_wait           | +28 52
    
_VGM_DATA:
    move.b  (%a0), %d0      | +8
    btst.l  #0,%d0          | +10 18
    bne.b   _VGM_DATA       | +8  26
    clr.l   %d1             | +4  30
    start_bit_wait          | +36 66

    sample_bits

    move.b %d1, (%a2)       | +12
    nop                     | + 4 
    nop                     | + 4 
    nop                     | + 4 
    stop_bit_wait           | +28 52

    bra _VGM_ADDRESS

    .equ YMPORT0, 0xA04000 |; YM2612 port 0
    .equ YMPORT1, 0xA04001 |; YM2612 port 1
    .equ YMPORT2, 0xA04002 |; YM2612 port 2
    .equ YMPORT3, 0xA04003 |; YM2612 port 3
    .equ PSGPORT, 0xC00011 |; PSG port

    dc.l PSGPORT 		|;4
ADRESS_TABLE:	
    dc.l YMPORT0 		|;0
    dc.l YMPORT1 		|;1
    dc.l YMPORT2 		|;2
    dc.l YMPORT3 		|;3
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4 16
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4 32
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4 48
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4
    dc.l PSGPORT 		|;4 64
