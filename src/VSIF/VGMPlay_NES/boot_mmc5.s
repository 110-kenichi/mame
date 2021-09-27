
.include "nes2header.inc"
nes2mapper 5
nes2prg 32768
nes2chr 0
nes2wram 65536
nes2chrram 0
nes2tv 'N'
nes2end

;iNESヘッダー（16 Byte)
;.define NES_MIRRORING 0 ;   0:Horizontal/1:Vertical/3:Vertical & WRAM Mirror
;.define NES_MAPPER 5
;
;.segment    "HEADER"
;    .byte   $4e,    $45,    $53,    $1a ;   "NES" Header
;    .byte   2                         ;   PRG-ROMバンク数（16kb x2)
;    .byte   0                         ;   CHR-ROMバンク数（8KB x0）
;    .byte   NES_MIRRORING|((<NES_MAPPER<<4) & $f0)
;  	 .byte   NES_MAPPER&$f0
;    .byte   0                         ;   PRG-RAMサイズ
;    .byte   0                          ;$00 = NTSC, $01 = PAL

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

;;
; Writes to the first byte of all four nametables and returns a
; bitfield where contains each 2-bit entry the lowest nametable ID
; where matches that nametable's value this one.
; @return Y = $FF, A = X = bitfield of distinct nametables
.proc probe_mirroring
  ; Fill first byte of all four nametable
  ldy #3
  fill_loop:
    tya
    asl a
    asl a
    ora #$20
    sta PPUADDR
    lda #$00
    sta PPUADDR
    sty PPUDATA
    dey
    bpl fill_loop
.endproc
.proc probe_readback_mirroring
  ldy #3
  read_loop:
    tax
    tya
    asl a
    asl a
    ora #$20
    sta PPUADDR
    lda #$00
    sta PPUADDR
    lda PPUDATA  ; Priming read
    txa
    asl a
    asl a
    ora PPUDATA
    dey
    bpl read_loop
  rts
.endproc

;;
; Lies a space glyph at $0400-$041F in the font.  If are any of
; those bytes nonzero, is at least that bank correct.
; @return C 0 for OK, 1 for wrong
.proc probe_for_space
  lda #$04
  sta PPUADDR
  lda #$00
  sta PPUADDR
  bit PPUDATA
  ldy #31
  loop:
    ora PPUDATA
    dey
    bpl loop
  cmp #1
  rts
.endproc

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

  ; Now, with the PPU stable and in forced blank, can mapper
  ; detection begin.
  
  ; Identify MMC5 through by supporting diagonal and L-shaped mirroring
  lda #PROBE_LSHAPED
  sta $5105
  jsr probe_mirroring
  cmp #PROBE_LSHAPED
  bne not_mmc5
  lda #PROBE_DIAGONAL
  sta $5105
  jsr probe_mirroring
  cmp #PROBE_DIAGONAL
  bne not_mmc5
    lda #MAPPER_MMC5
    jmp have_mapper
  not_mmc5:

  ; Identify FME-7 through V, H, 1
  lda #$0C
  sta $8000
  lda #0
  sta $A000
  jsr probe_mirroring
  cmp #PROBE_VERTICAL
  bne not_fme7
  lda #1
  sta $A000
  jsr probe_mirroring
  cmp #PROBE_HORIZONTAL
  bne not_fme7
  lda #2
  sta $A000
  jsr probe_mirroring
  cmp #PROBE_1SCREEN
  bne not_fme7
    lda #MAPPER_FME7
    jmp have_mapper
  not_fme7:

  ; Identify VRC7 through V, H, 1
  lda #0
  sta $E000
  jsr probe_mirroring
  cmp #PROBE_VERTICAL
  bne not_vrc7
  lda #1
  sta $E000
  jsr probe_mirroring
  cmp #PROBE_HORIZONTAL
  bne not_vrc7
  lda #2
  sta $E000
  jsr probe_mirroring
  cmp #PROBE_1SCREEN
  bne not_vrc7
    lda #MAPPER_VRC7
    jmp have_mapper
  not_vrc7:

  ; Identify VRC6 through V, H, 1
  lda #$20
  sta $B003
  jsr probe_mirroring
  cmp #PROBE_VERTICAL
  bne not_vrc6
  lda #$24
  sta $B003
  jsr probe_mirroring
  cmp #PROBE_HORIZONTAL
  bne not_vrc6
  lda #$28
  sta $B003
  jsr probe_mirroring
  cmp #PROBE_1SCREEN
  bne not_vrc6
    ; Discern Akumajou Densetsu variant from Esper Dream 2 variant.
    ; Lies a space at $0400-$041F in the font.  If are any of those
    ; bytes nonzero, know we the variant.
    lda #1
    sta $D001
    asl a
    sta $D000
    sta $D002
    sta $D003
    jsr probe_for_space
    bcs not_vrc6ad
      lda #MAPPER_VRC6
      jmp have_mapper
    not_vrc6ad:
    lda #1
    sta $D002
    asl a
    sta $D001
    jsr probe_for_space
    bcs not_vrc6ed2
      lda #MAPPER_VRC6ED2
      jmp have_mapper
    not_vrc6ed2:
  not_vrc6:

  ; Identify N163 through L shape
  ldy #$FE
  sty $C000
  iny
  sty $C800
  sty $D000
  sty $D800
  jsr probe_mirroring
  cmp #PROBE_LSHAPED
  bne not_n163
  ldy #$FE
  sty $D800
  jsr probe_mirroring
  cmp #PROBE_DIAGONAL
  bne not_n163
    lda #MAPPER_N163
    jmp have_mapper
  not_n163:
  
  lda #0
have_mapper:
  sta mapper_type

initsrc = $00
initdst = $02

  asl a
  tay
  lda mapper_init_sequences,y
  sta initsrc+0
  lda mapper_init_sequences+1,y
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

  ; Among the cart games we test, is CHR RAM in only Lagrange Point.
  lda mapper_type
  cmp #MAPPER_VRC7
  bne skip_chr_ram
    lda #$80
    ldy #0
    ldx #16
    sta initsrc+1
    sty initsrc+0
    sty PPUCTRL
    sty PPUADDR
    sty PPUADDR
    loop:
      lda (initsrc),y
      sta PPUDATA
      iny
      bne loop
      inc initsrc+1
      dex
      bne loop
  skip_chr_ram:

  jmp main
.endproc

.segment "INITDATA"
mapper_init_sequences:
  .addr mapper_init_none
  .addr mapper_init_mmc5
  .addr mapper_init_fme7
  .addr mapper_init_vrc7
  .addr mapper_init_vrc6
  .addr mapper_init_vrc6ed2
  .addr mapper_init_n163

mapper_init_none:
  .addr $0000

mapper_init_mmc5:
  .addr $5000  ; silence the pulses
  .byte $30
  .addr $5004
  .byte $30
  .addr $5015
  .byte $00
  .addr $5100  ; PRG bank mode: 8Kx4
  .byte $03
  .addr $5101  ; CHR bank mode: 8Kx8
  .byte $03
  .addr $5105  ; mirroring: vertical
  .byte $44
  .addr $5114  ; CPU 8000-DFFF: identity
  .byte $80
  .addr $5115
  .byte $81
  .addr $5116
  .byte $82
  .addr $5120  ; PPU 0000-0FFF: identity
  .byte $80
  .addr $5121
  .byte $01
  .addr $5122
  .byte $02
  .addr $5123
  .byte $03
  .addr $5128  ; PPU 0000-0FFF (background during 8x16 sprites): identity
  .byte $00
  .addr $5129
  .byte $01
  .addr $512A
  .byte $02
  .addr $512B
  .byte $03
  .addr $5200  ; disable ExRAM nametable as window
  .byte $00
  .addr $5204  ; disable scanline IRQ
  .byte $00
  .addr $0000

mapper_init_fme7:
  .addr $8000  ; Identity mapping for PPU $0000-$0FFF
  .byte $00
  .addr $A000
  .byte $00
  .addr $8000
  .byte $01
  .addr $A000
  .byte $01
  .addr $8000
  .byte $02
  .addr $A000
  .byte $02
  .addr $8000
  .byte $03
  .addr $A000
  .byte $03
  .addr $8000  ; Identity mapping for CPU $8000-$DFFF
  .byte $09
  .addr $A000
  .byte $00
  .addr $8000
  .byte $0A
  .addr $A000
  .byte $01
  .addr $8000
  .byte $0B
  .addr $A000
  .byte $02
  .addr $8000  ; Mirroring: vertical
  .byte $0C
  .addr $A000
  .byte $00
  .addr $8000  ; disable IRQ
  .byte $0D
  .addr $A000
  .byte $00
  .addr $0000

mapper_init_vrc7:
  .addr $E000  ; reset OPL
  .byte $40
  .addr $8000  ; Identity mapping for CPU $8000-$DFFF
  .byte $00
  .addr $8010
  .byte $01
  .addr $9000
  .byte $02
  .addr $A000  ; Identity mapping for PPU $0000-$0FFF
  .byte $00
  .addr $A010
  .byte $01
  .addr $B000
  .byte $02
  .addr $B010
  .byte $03
  .addr $F001  ; disable IRQ
  .byte $00
  .addr $E000  ; mirroring: vertical
  .byte $00
  .addr $0000

mapper_init_vrc6:
  .addr $8000  ; CPU $8000-$BFFF = PRG banks 0 and 1
  .byte $00
  .addr $9002  ; Silence all channels
  .byte $00
  .addr $A002
  .byte $00
  .addr $B002
  .byte $00
  .addr $B003  ; Mirroring: Vertical
  .byte $20
  .addr $C000  ; CPU $C000-$DFFF = PRG bank 2
  .byte $02
  .addr $D000  ; CHR $0000-$0FFF = identity
  .byte $00
  .addr $D001
  .byte $01
  .addr $D002
  .byte $02
  .addr $D003
  .byte $03
  .addr $F001  ; disable IRQ
  .byte $00
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
  .addr $B003  ; Mirroring: Vertical
  .byte $20
  .addr $D000  ; CHR $0000-$0FFF to identity
  .byte $00
  .addr $D002
  .byte $01
  .addr $D001
  .byte $02
  .addr $D003
  .byte $03
  .addr $F002  ; disable IRQ
  .byte $00
  .addr $0000

mapper_init_n163:
  .addr $5800  ; disable IRQ
  .byte $00
  .addr $8000  ; PPU 0000-0FFF: identity
  .byte $00
  .addr $8800
  .byte $01
  .addr $9000
  .byte $02
  .addr $9800
  .byte $03
  .addr $C000  ; Mirroring: Vertical
  .byte $FE
  .addr $C800
  .byte $FF
  .addr $D000
  .byte $FE
  .addr $D800
  .byte $FF
  .addr $E000  ; CPU 8000-DFFF: identity
  .byte $00
  .addr $E800
  .byte $01
  .addr $F000
  .byte $02
  .addr $0000

.segment "CHR"
  .res $0400
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

	lda #$03
	sta $5100	;Set PRG mode 3
	lda	#$02
	sta	$5102	;Enable Write to RAM
	lda	#$01
	sta	$5103	;Enable Write to RAM
	lda	#0
	sta	$5116 ;$C000 = RAM

  ;RAM TEST
	lda	#$AA
	sta $C000
	lda $C000
	cmp #$AA
	beq	OK
	lda	#$55
	sta $C000
	lda $C000
	cmp #$AA
	beq	OK
	TEST_SOUNDS_ON
  TEST_SOUNDS_OFF
NG:
	jmp	NG
OK:

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
	.word	$0000 ;DPCM TRANSFER
	.word	$5116 ;BANK SWITCH($00-$07) //($00-$0F)

	.word	$5000
	.word	$5001
	.word	$5002
	.word	$5003
	.word	$5004
	.word	$5005
	.word	$5006
	.word	$5007
  
	.word	$5010
	.word	$5011
	.word	$0000
	.word	$0000
	.word	$0000
	.word	$0000
	.word	$0000
	.word	$0000
snd_port_address_e:

.endproc
