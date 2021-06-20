;iNESヘッダー（16 Byte)

.define NES_MIRRORING 0 ;   0:Horizontal/1:Vertical/3:Vertical & WRAM Mirror
.define NES_MAPPER 20

.segment    "HEADER_FDS"
    .byte   $4e,    $45,    $53,    $1a ;   "NES" Header
    .byte   2                         ;   PRG-ROMバンク数（16kb x2)
    .byte   1                         ;   CHR-ROMバンク数（8KB x0）
    .byte   NES_MIRRORING|((<NES_MAPPER<<4) & $f0)
	.byte   NES_MAPPER&$f0
    .byte   0                         ;   PRG-RAMサイズ
    .byte   0                          ;$00 = NTSC, $01 = PAL
