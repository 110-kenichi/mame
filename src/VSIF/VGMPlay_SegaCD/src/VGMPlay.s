    .globl VGMPlay_FTDI2XX
    .globl VGMPlay_InitMCD
    .globl VGMPlay_SendMCDInt2

|■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

    .equ    PERIPHERAL_PORT_P2_B, 0xA10005   | P2 PORT(Byte access)

|163840bps 46.82clk @ 7.670454 MHz (NTSC)
|115200bps 66.58clk @ 7.670454 MHz (NTSC)

VGMPlay_FTDI2XX:
    move.b  #1,0xA11200
    move.b  #1,0xA11100
Reset_FTDI2XX:
    btst.b  #0,0xA11100
    bne.b   Reset_FTDI2XX

    move.l  #PERIPHERAL_PORT_P2_B, %a0 | PORT2 Address
    move.l  #ADRESS_TABLE, %a1         | Sound Register TABLE(OPNA2(Idx No 1,2,3,5), PSG(Idx No 6))
                                       | a2 is write address
    move.l  #_VGM_ADDRESS_FTDI2XX, %a3 | Jmp Address
    move.l  #0xFF0000,%a4              | PCM AREA ADRS
    move.l  #0xA1200E,%a5              | COMM CMD ADRS

    clr.l   %d0                        | for Recv
    move.b  #0xC0,%d1                  | for And Data(Hi 2bit)
    move.b  #6,%d2                     | for Check Bit 6
                                       | d4 for PCM ADRS
    move.w  #0,%d5                     | for PCM ADRS 
    move.l  #0xA00001,%d6              | for PCM ADRS MAGIC KEY

    move.b  #0x00, (%a0) 
    move.b  #0x00, (%a5) 

_VGM_ADDRESS_FTDI2XX:

_VGM_ADDRESS_FTDI2XX_LOOP:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    beq.b   _VGM_ADDRESS_FTDI2XX_LOOP         | +8 16   Wait pullup
    move.b  (%a0),%d0                         | +8 24   Get Address Idx(Lo 4bit) and Data(Hi 2bit)
    | Get Write Address
    | 0CDDAAAA -> DDAAAA00
    lsl.b   #2,%d0                            |+10 34   Shift Left
    move.l  (%d0.w, %a1), %a2                 |+16 50   Get Register Address

_VGM_DATA_FTDI2XX_LOOP:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    bne.b   _VGM_DATA_FTDI2XX_LOOP            | +8 16   Wait pulldown
    and.b   %d1,%d0                           |+ 4 20   DDAAAA00 -> DD000000
    or.b    (%a0),%d0                         |  8 28   Get Data(Hi 2bit | Lo 6bit)
    | Write Data to Address
    move.b  %d0,(%a2)                         |+ 8 36   Write DATA to register

    cmpa.l  %d6,%a2                           |  6 42         
    bne     _VGM_ADDRESS_FTDI2XX              | 10/8    jump/not jump

||||||||||||||||||||||||||||

_PCM_PROC_ADDRESS_HI_LO:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    beq.b   _PCM_PROC_ADDRESS_HI_LO           | +8 16   Wait pullup
    move.b  (%a0),(%a5)                       |+12 28
_PCM_PROC_ADDRESS_HI_HI:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    bne.b   _PCM_PROC_ADDRESS_HI_HI           | +8 16   Wait pulldown
    move.b  (%a0),(%a5)                       |+12 28

_PCM_PROC_ADDRESS_LO_LO:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    beq.b   _PCM_PROC_ADDRESS_LO_LO           | +8 16   Wait pullup
    move.b  (%a0),(%a5)                       |+12 28
_PCM_PROC_ADDRESS_LO_HI:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    bne.b   _PCM_PROC_ADDRESS_LO_HI           | +8 16   Wait pulldown
    move.b  (%a0),(%a5)                       |+12 28

_PCM_PROC_DATA_HI:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    beq.b   _PCM_PROC_DATA_HI                 | +8 16   Wait pullup
    move.b  (%a0),%d0                         |
    move.b  %d0,(%a5)                         |
_PCM_PROC_DATA_LO:
    btst.b  %d2,(%a0)                         | +8 8    Check Burst Flag
    bne.b   _PCM_PROC_DATA_LO                 | +8 16   Wait pulldown
    move.b  (%a0),(%a5)                       |+12 28

|    btst    %d5,%d0                          | +6 34   Check CLK
|    bne     _PCM_PROC_DATA_HI                |+10/8

    .macro  DummyWaitCDGen
1:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    beq.b   1b                                | +8 16   Wait pullup
    move.b  (%a0),(%a5)                       |+12 28
2:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    bne.b   2b                                | +8 16   Wait pulldown
    move.b  (%a0),(%a5)                       |+12 28
    .endm

/*
    DummyWaitCDGen
    DummyWaitCDGen
    DummyWaitCDGen
    DummyWaitCDGen
    DummyWaitCDGen
*/

    jmp     (%a3)                             |+ 8 52   Loop

|■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

    .equ YMPORT0, 0xA04000 |; YM2612 port 0
    .equ YMPORT1, 0xA04001 |; YM2612 port 1
    .equ YMPORT2, 0xA04002 |; YM2612 port 2
    .equ YMPORT3, 0xA04003 |; YM2612 port 3
    .equ PSGPORT, 0xC00011 |; PSG port
    .equ DUMMY,   0xA00000 |; dummy memory
    .equ PCMKEY,  0xA00001 |; PCM MAGIC WORD(dummy memory)

ADRESS_TABLE:
    .rept 8
    dc.l DUMMY   		|;00
    dc.l YMPORT0 		|;04
    dc.l YMPORT1 		|;08
    dc.l YMPORT2 		|;0C
    dc.l YMPORT3 		|;10
    dc.l PSGPORT 		|;14
    dc.l PCMKEY 		|;18
    dc.l DUMMY 		    |;1c
    .endr

|■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■

    .macro TESTSOUND
|||||||||||||||||||||||| Debug
    move.b  #0x80,0xFF000F

    move.l  #0xFF2000,%a1              | PCM AREA ADRS
    move.l  #0xFFE/4,%d0
SET_PCM:
    move.w  #0xFDFD,(%a1)+
    Waits
    move.w  #0xFDFD,(%a1)+
    Waits
    move.w  #0x7F7F,(%a1)+
    Waits
    move.w  #0x7F7F,(%a1)+
    Waits
    dbf     %d0,SET_PCM
    move.w  #0xFFFF,(%a1)+
    Waits
    move.w  #0xFFFF,(%a1)+
    Waits
    move.w  #0xFFFF,(%a1)+
    Waits
    move.w  #0xFFFF,(%a1)+
    Waits

    move.b  #0xc0,0xFF000F  | SON ch0
    Waits
    move.b  #0x00,0xFF000D  | PCM ADR
    Waits
    move.b  #0x00,0xFF000B  | LOOP
    Waits
    move.b  #0x00,0xFF0009  | LOOP
    Waits
    move.b  #0x08,0xFF0007  | PITCH HI
    Waits
    move.b  #0x08,0xFF0005  | PITCH LO
    Waits
    move.b  #0xFF,0xFF0003  | PAN
    Waits
    move.b  #0xFF,0xFF0001  | VOL
    Waits
    move.b  #0xFE,0xFF0011  | KON
    Waits

||||||||||||||||||||||||
    .endm

VGMPlay_InitMCD:
   	movem.l	%d1-%d7/%a0-%a6,-(%sp)

    jsr     FindMCDBIOS
    bcs     McdBiosNotFound

|    move.l  #CD_PCM_PROC,%a1
|    move.l  #CD_PCM_PROC_END-CD_PCM_PROC,%d0
|    jsr     InitSubCPU
|	 bne.s	 McdBiosNotFound

	jsr 	ResetSubCPU
    
    bsr.w	ReqSubCPUBus			| Request Sub CPU bus
	move.b	%d2,%d3
	bne.s	McdBiosNotFound			| If it failed to do that, branch

	move.b	#0,0xA12002				| Disable write protect on Sub CPU memory

    | Copy User program
    move.l  #CD_PCM_PROC,%a0
    move.l  #CD_PCM_PROC_END-CD_PCM_PROC,%d0
    move.l  #0,%d1
    jsr     CopyToPRGRAM
	bne.s	McdBiosNotFound

|   jsr     SendMCDInt2
 WAIT_CD_INIT:
    cmpi.b  #0x55,0xa1200f
    bne     WAIT_CD_INIT

	move.b	#0x2A,0xA12002			| Enable write protect on Sub CPU memory
	bsr.w	ReturnSubCPUBus			| Return Sub CPU bus
   	movem.l	(%sp)+,%d1-%d7/%a0-%a6
    rts

McdBiosNotFound:
	move.b	#0x2A,0xA12002			| Enable write protect on Sub CPU memory
	bsr.w	ReturnSubCPUBus			| Return Sub CPU bus
   	movem.l	(%sp)+,%d1-%d7/%a0-%a6
    move.l  #1,%d0
    rts

||||||||||||

.macro  Vectors
vector\@:
    dc.l    CD_Init - vector\@ + 4
.endm

.macro  Waits
    move    #40,%d7
1:
    dbf     %d7,1b
.endm

CD_PCM_PROC:
_Vecteurs_68K:
    dc.l    CD_PCM_PROC_END - _Vecteurs_68K + 4             /* Stack address */
    .rept   255
Vectors
    .endr

    .equ GA_LED_STATUS, 0xFFFF8000
    .equ GA_LEDR, 0
    .equ GA_LEDG, 0
    .equ GA_RESET, 0xFFFF8001
    .equ GA_RES0, 0
    .equ GA_INT_MASK, 0xFFFF8033
    .equ GA_CDD_CONTROL, 0xFFFF8037
    .equ GA_MEMORY_MODE, 0xFFFF8003
	.equ GA_PM1, 4
	.equ GA_PM0, 3
	.equ GA_MODE, 2
	.equ GA_DMNA, 1
	.equ GA_RET, 0
	.equ GA_CDC_ADDRESS      , 0xFFFF8005
	.equ GA_CDC_REGISTER     , 0xFFFF8007
	.equ GA_CDC_DATA         , 0xFFFF8008   | Read-only
	.equ GA_CDC_DMA_ADDRESS  , 0xFFFF800A
	.equ GA_STOPWATCH        , 0xFFFF800C

    .equ GA_COMM_SUBFLAGS    , 0xFFFF800F
    .equ GA_COMM_SUBDATA     , 0xFFFF8020

    .equ CDC_WRITE_RESET, 0xF

    .equ GA_CDD_FADER        , 0xFFFF8034

CD_Init:
	clr.b   (GA_INT_MASK).w
    move.w  #0x2700, %sr               | DI

	| Trigger peripheral reset
	bclr #GA_RES0, (GA_RESET).w

	| Red LED on
	move.b #1, (GA_LED_STATUS).w

	| Set Word RAM to 2M mode
	lea (GA_MEMORY_MODE).w, %a0
loc_27C:
    btst  #GA_MODE, (%a0)
    beq.s loc_288
    bclr  #GA_MODE, (%a0)
    bra.s loc_27C
loc_288:
	| Give Word RAM to main CPU
	btst  #GA_DMNA, (%a0)
	beq.s loc_292
	bset  #GA_RET, (%a0)
loc_292:
	| Reset CDC (Sanyo LC89510)
	move.b #CDC_WRITE_RESET, (GA_CDC_ADDRESS).w
	move.b #0, (GA_CDC_REGISTER).w
	move.w #0, (GA_STOPWATCH).w

	| Clear communication registers
	moveq  #0, %d0
	move.b %d0, (GA_COMM_SUBFLAGS).w
	lea    (GA_COMM_SUBDATA).w, %a0
	move.l %d0, (%a0)+
	move.l %d0, (%a0)+
	move.l %d0, (%a0)+
	move.l %d0, (%a0)

	| Mute CD audio
	move.w #0, (GA_CDD_FADER).w

|    TESTSOUND
    move.b  #0xFF,0xFF0011  | KOFF

CD_Main:
    move.l  #0xFF800e,%a0              | COMM CMD Address
    move.l  #0xFF0000,%a4              | PCM AREA ADRS

    clr.l   %d0                        | for Recv 
    move.b  #0xC0,%d1                  | for And Data(Hi 2bit)
    move.b  #6,%d2                     | for Check Bit 6
                                       | d4 for PCM ADRS
    move.w  #2,%d5                     | for PCM burst flag 
    clr.l   %d4                        | for Recv 

    move.b  #0x55,0xFF800F             | Set OK

CD_PCM_LOOP:
CD_PCM_PROC_ADDRESS_HI_HI:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    beq.b   CD_PCM_PROC_ADDRESS_HI_HI         | +8 16   Wait pullup
    | 0CAA0000
    move.b  (%a0),%d4                         | +8 24   
    | 0CAA0000 -> AA000000
    lsl.b   #2,%d4                            |+10 34   Shift Left
CD_PCM_PROC_ADDRESS_HI_LO:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    bne.b   CD_PCM_PROC_ADDRESS_HI_LO         | +8 16   Wait pulldown
    | 00AAAAAA | AA000000
    or.b    (%a0),%d4                         |  8 24   

    | AAAAAAAA -> AAAAAAAA_00000000
    lsl.w   #8,%d4                            |+22 46   

CD_PCM_PROC_ADDRESS_LO_HI:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    beq.b   CD_PCM_PROC_ADDRESS_LO_HI         | +8 16   Wait pullup
    | 0CAA0000
    move.b  (%a0),%d0                         | +8 24   
    | 0CAA0000 -> AA000000
    lsl.b   #2,%d0                            |+10 34   Shift Left
CD_PCM_PROC_ADDRESS_LO_LO:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    bne.b   CD_PCM_PROC_ADDRESS_LO_LO         | +8 16   Wait pulldown
    | 00AAAAAA | AA000000
    or.b    (%a0),%d0                         |  8 24   

    | AAAAAAAA_00000000 | 00000000_AAAAAAAA
    or.b    %d0,%d4                           | +4 50  PCM Address(Hi+Lo)

CD_PCM_PROC_DATA_HI:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    beq.b   CD_PCM_PROC_DATA_HI                 | +8 16   Wait pullup
    | 0CDD0000
    move.b  (%a0),%d0                         | +8 24

    | 0CDD0000 -> DD000000
    lsl.b   #2,%d0                            |+10 34   Shift Left

    | DD000100
|    btst    %d5,%d0                           | +6 30   Check CLK
|    bne     CD_PCM_PROC_DATA_LO_2               |+10/12

CD_PCM_PROC_DATA_LO:
    btst.b  %d2,(%a0)                         | +8 8    Check Burst Flag
    bne.b   CD_PCM_PROC_DATA_LO               | +8 16   Wait pulldown
    | 00DDDDDD | DD000000
    or.b    (%a0),%d0                         |  8 24   Get Data(Lo 2bit)
    | Write Data to Address
    move.b  %d0,(%a4, %d4.w)                  |+14 38   Write DATA to register

    .macro  DummyWaitCDSub
1:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    beq.b   1b                                | +8 16   Wait pullup
    move.b  (%a0),%d0                         | +8 24

2:
    btst.b  %d2,(%a0)                         | +8 8    Check CLK
    bne.b   2b                                | +8 16   Wait pulldown
    move.b  (%a0),%d0                         | +8 24
    .endm
/*
    DummyWaitCDSub
    DummyWaitCDSub
    DummyWaitCDSub
    DummyWaitCDSub
    DummyWaitCDSub
*/
    bra     CD_PCM_LOOP                       |+10 48   Loop

CD_PCM_PROC_END:
