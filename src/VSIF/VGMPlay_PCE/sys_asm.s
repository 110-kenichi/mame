
.importzp sp
.segment "ZEROPAGE"

.exportzp _zp_cmd, _zp_src, _zp_dst, _zp_len, _zp_rts
_zp_cmd:    .res 1 ;do not change cmd/args order
_zp_src:    .res 2
_zp_dst:    .res 2
_zp_len:    .res 2
_zp_rts:    .res 1

.segment "BNK00"

VDC_REG     = $100
VDC_REG_MRW = $02

cmd_rts     = $60
cmd_tia     = $E3
cmd_tai     = $F3
cmd_tii     = $73

cval        = $0120 
clr_row:
.word cval, cval, cval, cval, cval, cval, cval, cval
.word cval, cval, cval, cval, cval, cval, cval, cval
.word cval, cval, cval, cval, cval, cval, cval, cval
.word cval, cval, cval, cval, cval, cval, cval, cval

.export _g_clean_plan
_g_clean_plan:
    ldx #28
    lda #VDC_REG_MRW
    sta VDC_REG
@1:
    tia clr_row, 2,  64
    tia clr_row, 2,  64
  
    dex
    bne @1
    rts

.export _sys_set_bank
.import _sys_bank
_sys_set_bank:
    lda _sys_bank
    tam #4
    rts


.export _sys_mem_to_reg_asm
_sys_mem_to_reg_asm:
    lda #cmd_tia
    sta _zp_cmd
    lda #cmd_rts
    sta _zp_rts
    jmp _zp_cmd+8192

.export _sys_reg_to_mem_asm
_sys_reg_to_mem_asm:
    lda #cmd_tai
    sta _zp_cmd
    lda #cmd_rts
    sta _zp_rts
    jmp _zp_cmd+8192

.export _sys_mem_to_mem_asm
_sys_mem_to_mem_asm:
    lda #cmd_tii
    sta _zp_cmd
    lda #cmd_rts
    sta _zp_rts
    jmp _zp_cmd+8192


.export _font
_font:
.incbin "font.bin"
nop
