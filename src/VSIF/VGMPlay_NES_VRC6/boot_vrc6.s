;iNESヘッダー（16 Byte)
.define NES_MIRRORING 0 ;   0:Horizontal/1:Vertical/3:Vertical & WRAM Mirror
;.define NES_MAPPER 24
.define NES_MAPPER 0

.segment    "HEADER"
    .byte   $4e,    $45,    $53,    $1a ;   "NES" Header
    .byte   2                         ;   PRG-ROMバンク数（16kb x1)
    .byte   0                         ;   CHR-ROMバンク数（8KB x1）
    .byte   NES_MIRRORING|((<NES_MAPPER<<4) & $f0)
	.byte   NES_MAPPER&$f0
    .byte   0                         ;   PRG-RAMサイズ
    .byte   0                          ;$00 = NTSC, $01 = PAL

; MDFourier for Famicom
; Mapper initialization
;
; Is this module responsible for these steps:
; 
; 1. Detect MMC5, VRC6, VRC7, FME-7, or N163 through mirroring
; 2. Set an identity mapping for PRG and CHR memory
; 3. Copy CHR ROM $8000-$9FFF to CHR RAM $0000-$1FFF if needed
;
; After which it jumps to main

.include "nes.inc"
.include "global.inc"
;.import main, irq_handler, nmi_handler

.segment "ZEROPAGE"
tvSystem: .res 1     ; 0: ntsc; 1: pal nes; 2: dendy
mapper_type: .res 1  ; 0-5: 

.segment "VECTORS"
  .addr nmi_handler, reset_handler, irq_handler

.segment "LOWCODE"
;;
; Waits for 1284*y + 5*x cycles + 5 cycles, minus 1284 if x is
; nonzero, and then reads bit 7 and 6 of the PPU status port.
; @param X fine period adjustment
; @param Y coarse period adjustment
; @return N=NMI status; V=sprite 0 status; X=Y=0; A unchanged
.proc wait1284y
  dex
  bne wait1284y
  dey
  bne wait1284y
  bit $2002
  rts
.endproc

;                   2C282420
PROBE_1SCREEN    = %00000000
PROBE_VERTICAL   = %01000100
PROBE_HORIZONTAL = %10100000
PROBE_DIAGONAL   = %00010100
PROBE_LSHAPED    = %01010100
PROBE_4SCREEN    = %11100100

.proc reset_handler
  sei
  ldx #$FF
  txs
  inx
  stx $2000  ; disable PPU
  stx $2001
  stx $4015  ; disable 2A03 audio channels
  lda #$40
  sta $4017  ; disable APU IRQ

  ; Acknowledge any stray interrupts where remained they
  ; after a Reset press.
  bit $4015
  bit $2002

  ; clear zero page
  cld
  txa
  :
    sta $00,x
    inx
    bne :-

  ; Wait for warm-up while distinguishing the three known
  ; TV systems,  May this occasionally miss a frame due to a race
  ; in the PPU; cannot this hurt.
  vwait1:
    bit $2002
    bpl vwait1
  
  ; NTSC: 29780 cycles, 23.19 loops.  Will end in vblank
  ; PAL NES: 33247 cycles, 25.89 loops.  Will end in vblank
  ; Dendy: 35464 cycles, 27.62 loops.  Will end in post-render
  ldx #0
  txa
  ldy #24
  jsr wait1284y  ; after this point, comes the PPU stable
  bmi have_tvSystem

  lda #1
  ldy #3
  jsr wait1284y
  
  ; If happened another vblank by 27 loops, are we on a PAL NES.
  ; Otherwise, are we on a PAL famiclone.
  bmi have_tvSystem
    asl a
  have_tvSystem:
  sta tvSystem

initsrc = $00
initdst = $02


  lda mapper_init_sequences
  sta initsrc+0
  lda mapper_init_sequences+1
  sta initsrc+1
  ldx #0
  ldy #0
  initloop:
    lda (initsrc),y
    iny
    sta initdst+0
    lda (initsrc),y
    beq initdone
    iny
    sta initdst+1
    lda (initsrc),y
    iny
    sta (initdst,x)
    jmp initloop
  initdone:

  jmp main
.endproc

.segment "INITDATA"
mapper_init_sequences:
  .addr mapper_init_vrc6
;  .addr mapper_init_vrc6ed2

mapper_init_vrc6:
  .addr $8000  ; CPU $8000-$BFFF = PRG banks 0 and 1
  .byte $00
  .addr $9002  ; Silence all channels
  .byte $00
  .addr $A002
  .byte $00
  .addr $B002
  .byte $00
  .addr $9003
  .byte $00
  .addr $B003  ; Mirroring: Vertical
  .byte $20
;  .addr $C000  ; CPU $C000-$DFFF = PRG bank 2
;  .byte $02
;  .addr $D000  ; CHR $0000-$0FFF = identity
;  .byte $00
;  .addr $D001
;  .byte $01
;  .addr $D002
;  .byte $02
;  .addr $D003
;  .byte $03
;  .addr $F001  ; disable IRQ
;  .byte $00
  .addr $0000

mapper_init_vrc6ed2:
  .addr $8000  ; CPU $8000-$BFFF to PRG banks 0 and 1
  .byte $00
  .addr $C000  ; CPU $C000-$DFFF to PRG bank 2
  .byte $02
  .addr $9001  ; Silence all channels
  .byte $00
  .addr $A001
  .byte $00
  .addr $B001
  .byte $00
  .addr $9003
  .byte $00
  .addr $B003  ; Mirroring: Vertical
  .byte $20
;  .addr $D000  ; CHR $0000-$0FFF to identity
;  .byte $00
;  .addr $D002
;  .byte $01
;  .addr $D001
;  .byte $02
;  .addr $D003
;  .byte $03
;  .addr $F002  ; disable IRQ
;  .byte $00
  .addr $0000

;.segment "CHR"
;  .res $0400
;  .incbin "obj/nes/fizztersmboldmono16.chr"

.segment "LOWCODE"

.proc irq_handler
  rti
.endproc

.proc nmi_handler
  rti
.endproc

.code

.import _VGMPlay_FTDI2XX_INDIRECT

.proc main
  lda #$3F
  sta PPUADDR
  lda #$00
  sta PPUADDR
  lda mapper_type
  asl a
  beq :+
    ora #$10
 :
  sta PPUDATA
  adc #$10
  sta PPUDATA
  lda #$20
  sta PPUDATA
  lda #0
  sta PPUADDR
  sta PPUADDR

  ;set snd port addresses
	ldx #0
:
	lda snd_port_address_s, X
	sta $80, X
	inx
	cpx #snd_port_address_e - snd_port_address_s
	bcc :-

  jmp _VGMPlay_FTDI2XX_INDIRECT

snd_port_address_s:
	.word	$4000
	.word	$4001
	.word	$4002
	.word	$4003
	.word	$4004
	.word	$4005
	.word	$4006
	.word	$4007

	.word	$4008
	.word	$4009
	.word	$400A
	.word	$400B
	.word	$400C
	.word	$400D
	.word	$400E
	.word	$400F

	.word	$4010
	.word	$4011
	.word	$4012
	.word	$4013
	.word	$4014
	.word	$4015
	.word	$0000
	.word	$0000

	.word	$9000
	.word	$9001
	.word	$9002
	.word	$9003
	.word	$A000
	.word	$A001
	.word	$A002
	.word	$A003
  
	.word	$B000
	.word	$B001
	.word	$B002
	.word	$0000
	.word	$9008 ; VRC7 TT2 https://www.nesdev.org/wiki/VRC7_pinout
	.word	$9010 ; VRC7 LP
	.word	$0000
	.word	$9030 ; VRC7
snd_port_address_e:

.endproc
