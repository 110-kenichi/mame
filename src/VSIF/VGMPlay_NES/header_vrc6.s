;iNESヘッダー（16 Byte)
.segment    "HEADER_VRC6"
    .byte   $4e,    $45,    $53,    $1a ;   "NES" Header
    .byte   16                         ;   PRG-ROMバンク数（16kb x2)
    .byte   16                         ;   CHR-ROMバンク数（8KB x0）
    .byte   $80                         ;   0:Horizontal/1:Vertical/3:Vertical & WRAM Mirror
    .byte   $10                         ;   Mapper 0
    .byte   0                         ;   PRG-RAMサイズ
    .byte   0                          ;$00 = NTSC, $01 = PAL
