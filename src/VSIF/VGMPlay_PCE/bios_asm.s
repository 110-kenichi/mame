
.importzp _zp_cmd, _zp_src, _zp_dst, _zp_len, _zp_rts
;******************************************************************************* regs
REG_FIFO_DATA   = $1FF0
REG_FIFO_STAT   = $1FF2
REG_SYS_STAT    = $1FF3

STATUS_REBOOT   = $10
FIFO_CPU_RXF    = $80
;******************************************************************************* ram
.segment "BSS"
ram_app:        .res 128

.export _bi_halt_arg
_bi_halt_arg:   .res 1

.segment "BNK00"

;******************************************************************************* dma halt

.export _bi_init_asm
_bi_init_asm:
    tii bi_ram_app_code, ram_app,  bi_ram_app_code_end - bi_ram_app_code
    rts

.export _bi_halt_asm
_bi_halt_asm:
    jmp ram_app

bi_ram_app_code:
    lda _bi_halt_arg    ;required status
    sta REG_SYS_STAT
    lda #0
    sta REG_FIFO_DATA   ;exec
@0: 
    lda REG_SYS_STAT
    and #$f8
    cmp #$a8            ;strobe1
    bne @0
    lda REG_SYS_STAT
    tax
    and #$f8
    cmp #$a0            ;strobe0
    bne @0
    
    txa
    and _bi_halt_arg    ;repeat till all bits specifid in bi_halt_arg will not be cleared
    bne @0
    
    lda _bi_halt_arg
    and #STATUS_REBOOT
    beq skip_reboot
    jmp ($fffe)
skip_reboot:
    rts
bi_ram_app_code_end:

;******************************************************************************* start app
.export _bi_start_app_asm
_bi_start_app_asm:
    tii bi_start_app_code, ram_app,  bi_start_app_end - bi_start_app_code
    jmp ram_app
bi_start_app_code:
    lda _bi_halt_arg
    sta REG_SYS_STAT
    lda #0
    sta REG_FIFO_DATA   ;exec mem wr
@0:
    lda REG_SYS_STAT
    and _bi_halt_arg    ;repeat till all bits specifid in bi_halt_arg will not be cleared
    bne @0
    
    lda #$ff
    stz $0C01   ;turn off timer
    sta $0C00   ;set timer val
    stz $1403   ;ack timer irq
    stz $1402   ;turn on irq (default val)

    lda #$04
    tam #$7d    ;only ram stay in place
    lda #00
    csl
    jmp ($fffe)
bi_start_app_end:

;*******************************************************************************
.export _bi_fifo_rd_asm
_bi_fifo_rd_asm:
    ldy #0
    ldx _zp_len
@0:
    lda REG_FIFO_STAT
    bmi @0
    lda REG_FIFO_DATA
    sta (_zp_dst), y

    iny
    bne @1
    inc _zp_dst+1
@1:
    dex
    bne @0
    rts

.export _bi_fifo_wr_asm
_bi_fifo_wr_asm:
    ldy #0
    ldx _zp_len
@0:
    lda (_zp_src), y
    sta REG_FIFO_DATA

    iny
    bne @1
    inc _zp_src+1
@1:
    dex
    bne @0

    rts

.export _bi_cmd_tx_asm
_bi_cmd_tx_asm:

    lda #'+'
    sta REG_FIFO_DATA
    eor #$ff
    sta REG_FIFO_DATA
    lda _zp_cmd
    sta REG_FIFO_DATA
    eor #$ff
    sta REG_FIFO_DATA
    rts