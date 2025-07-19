;
;	CARTRIDGE HEADER (16 ROM, PAGEADDR 1, 0x4000-0x8000)
;
;	0,	0x0000-0x3FFF,	ROM MAIN | (SFG(OPM))
;	1,	0x4000-0x7FFF,	ROM VSIF | (OPLL EXT)
;	2,	0x8000-0xBFFF,	ROM VSIF | (SCC/SCC-1) | (NEOTRON(OPMB))
;	3,	0xC000-0xFFFF,	RAM WORK AREA
;

	.module	start
	.globl	_main
;
	.area	_ROM_HDR (ABS)
	.org	0x4000
;
	.db	'A
	.db	'B
	.dw	_main
	.dw	0
	.dw	0
	.dw	0
	.dw	0
	.dw	0
	.dw	0
;
	.ascii	"END ROMHEADER"
;
