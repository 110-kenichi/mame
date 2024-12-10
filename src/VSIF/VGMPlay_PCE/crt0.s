.export rst,__STARTUP__:absolute=1
.import __RAM_START__   ,__RAM_SIZE__
.import __ROM0_START__  ,__ROM0_SIZE__
.import __STARTUP_LOAD__,__STARTUP_RUN__,__STARTUP_SIZE__
.import	__CODE_LOAD__   ,__CODE_RUN__   ,__CODE_SIZE__
.import	__RODATA_LOAD__ ,__RODATA_RUN__ ,__RODATA_SIZE__

.include "zeropage.inc"
.import initlib, push0, popa, popax, _main, zerobss, copydata
.import _main

.segment "BNK00"
.word $0000 ;date
.word $0000 ;time
.word $0000
.word $0000
.word $0000
.word $0000
.word $0000
.word $0000
rst:
    sei
    cld
    ldx #$ff
    txs
    csh
    
    lda #$FF    ; map the I/O bank in the first page
    tam #1
    lda #$F8    ; and the RAM bank in the second page
    tam #2
    
;set rom banks
    lda #$1
    tam #8
    lda #$2
    tam #16
    lda #$3
    tam #32
    lda #$4
    tam #64
    lda #$0
    tam #128

;clr ram
    lda #$02
    sta 0
    lda #$20
    sta 1
@0:
    lda #0
@1:
    sta (0)
    inc 0
    bne @1
    inc 1    
    lda 1
    cmp #$40
    bne @0
    lda #0
    sta 0
    sta 1

    jsr	zerobss
    jsr	copydata

    lda #<(__RAM_START__+__RAM_SIZE__)
    sta	sp
    lda	#>(__RAM_START__+__RAM_SIZE__)
    sta	sp+1            ; Set argument stack ptr

    jsr	initlib

    jmp _main

irq:
    rti

irq1:
    rti

irq2:
    rti

.segment "VECTORS"

;.word $0100
.word irq
.word irq 
.word irq 
.word irq2  ;irq2
.word irq1  ;irq1

.word irq   ;$fffa tmr
.word irq   ;$fffc nmi
.word rst   ;$fffe irq / brk








