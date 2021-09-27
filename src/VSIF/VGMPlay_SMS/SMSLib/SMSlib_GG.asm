;--------------------------------------------------------
; File Created by SDCC : free open source ANSI-C Compiler
; Version 4.1.0 #12072 (MINGW64)
;--------------------------------------------------------
	.module SMSlib
	.optsdcc -mz80
	
;--------------------------------------------------------
; Public variables in this module
;--------------------------------------------------------
	.globl _SMS_setLineInterruptHandler
	.globl _nop
	.globl _SMS_copySpritestoSAT
	.globl _SMS_initSprites
	.globl _SMS_crt0_RST18
	.globl _SMS_crt0_RST08
	.globl _SMS_theNmiInterruptHandler
	.globl _SMS_theVBlankInterruptHandler
	.globl _spritesTileOffset
	.globl _spritesWidth
	.globl _spritesHeight
	.globl _VDPReg
	.globl _SMS_theLineInterruptHandler
	.globl _PreviousKeysStatus
	.globl _KeysStatus
	.globl _SMS_Port3EBIOSvalue
	.globl _SMS_VDPFlags
	.globl _VDPBlank
	.globl _SMS_SRAM
	.globl _SRAM_bank_to_be_mapped_on_slot2
	.globl _ROM_bank_to_be_mapped_on_slot2
	.globl _ROM_bank_to_be_mapped_on_slot1
	.globl _VDPReg_init
	.globl _SMS_init
	.globl _SMS_VDPturnOnFeature
	.globl _SMS_VDPturnOffFeature
	.globl _SMS_setBGScrollX
	.globl _SMS_setBGScrollY
	.globl _SMS_setBackdropColor
	.globl _SMS_useFirstHalfTilesforSprites
	.globl _SMS_setSpriteMode
	.globl _GG_setBGPaletteColor
	.globl _GG_setSpritePaletteColor
	.globl _GG_loadBGPalette
	.globl _GG_loadSpritePalette
	.globl _SMS_waitForVBlank
	.globl _SMS_getKeysStatus
	.globl _SMS_getKeysPressed
	.globl _SMS_getKeysHeld
	.globl _SMS_getKeysReleased
	.globl _SMS_setLineCounter
	.globl _SMS_setVBlankInterruptHandler
	.globl _SMS_setNmiInterruptHandler
	.globl _SMS_getVCount
	.globl _SMS_getHCount
	.globl _SMS_isr
	.globl _SMS_nmi_isr
;--------------------------------------------------------
; special function registers
;--------------------------------------------------------
_VDPControlPort	=	0x00bf
_VDPStatusPort	=	0x00bf
_VDPDataPort	=	0x00be
_VDPVCounterPort	=	0x007e
_VDPHCounterPort	=	0x007f
_IOPortL	=	0x00dc
_GGIOPort	=	0x0000
;--------------------------------------------------------
; ram data
;--------------------------------------------------------
	.area _DATA
_ROM_bank_to_be_mapped_on_slot1	=	0xfffe
_ROM_bank_to_be_mapped_on_slot2	=	0xffff
_SRAM_bank_to_be_mapped_on_slot2	=	0xfffc
_SMS_SRAM	=	0x8000
_VDPBlank::
	.ds 1
_SMS_VDPFlags::
	.ds 1
_SMS_Port3EBIOSvalue::
	.ds 1
_KeysStatus::
	.ds 2
_PreviousKeysStatus::
	.ds 2
_SMS_theLineInterruptHandler::
	.ds 2
;--------------------------------------------------------
; ram data
;--------------------------------------------------------
	.area _INITIALIZED
_VDPReg::
	.ds 2
_spritesHeight::
	.ds 1
_spritesWidth::
	.ds 1
_spritesTileOffset::
	.ds 1
_SMS_theVBlankInterruptHandler::
	.ds 2
_SMS_theNmiInterruptHandler::
	.ds 2
;--------------------------------------------------------
; absolute external ram data
;--------------------------------------------------------
	.area _DABS (ABS)
;--------------------------------------------------------
; global & static initialisations
;--------------------------------------------------------
	.area _HOME
	.area _GSINIT
	.area _GSFINAL
	.area _GSINIT
;--------------------------------------------------------
; Home
;--------------------------------------------------------
	.area _HOME
	.area _HOME
;--------------------------------------------------------
; code
;--------------------------------------------------------
	.area _CODE
;SMSlib.c:70: void nop()
;	---------------------------------
; Function nop
; ---------------------------------
_nop::
;SMSlib.c:73: }
	ret
_VDPReg_init:
	.db #0x04	; 4
	.db #0x20	; 32
	.db #0xff	; 255
	.db #0xff	; 255
	.db #0xff	; 255
	.db #0xff	; 255
	.db #0xff	; 255
	.db #0x00	; 0
	.db #0x00	; 0
	.db #0x00	; 0
	.db #0xff	; 255
;SMSlib.c:98: void SMS_init (void) {
;	---------------------------------
; Function SMS_init
; ---------------------------------
_SMS_init::
;SMSlib.c:105: GG_setSpritePaletteColor(0, RGB(0,0,0));
	ld	hl, #0x0000
	push	hl
	xor	a, a
	push	af
	inc	sp
	call	_GG_setSpritePaletteColor
	pop	af
	inc	sp
;SMSlib.c:108: for (i=0;i<0x0B;i++)
	ld	bc, #_VDPReg_init+0
	ld	e, #0x00
00103$:
;SMSlib.c:109: SMS_write_to_VDPRegister(i,VDPReg_init[i]);
	ld	l, e
	ld	h, #0x00
	add	hl, bc
	ld	a, (hl)
;SMSlib_common.c:37: DISABLE_INTERRUPTS;
	di	
;SMSlib_common.c:38: VDPControlPort=v;
	out	(_VDPControlPort), a
;SMSlib_common.c:39: VDPControlPort=VDPReg|0x80;
	ld	a, e
	or	a, #0x80
	out	(_VDPControlPort), a
;SMSlib_common.c:40: ENABLE_INTERRUPTS;
	ei	
;SMSlib.c:108: for (i=0;i<0x0B;i++)
	inc	e
	ld	a, e
	sub	a, #0x0b
	jr	C, 00103$
;SMSlib.c:111: SMS_initSprites();
	call	_SMS_initSprites
;SMSlib.c:113: SMS_copySpritestoSAT();
;SMSlib.c:122: }
	jp	_SMS_copySpritestoSAT
;SMSlib.c:132: void SMS_VDPturnOnFeature (unsigned int feature) __z88dk_fastcall {
;	---------------------------------
; Function SMS_VDPturnOnFeature
; ---------------------------------
_SMS_VDPturnOnFeature::
	ex	de, hl
;SMSlib.c:134: VDPReg[HI(feature)]|=LO(feature);
	ld	c, d
	ld	b, #0x00
	ld	hl, #_VDPReg
	add	hl, bc
	ld	a, (hl)
	or	a, e
	ld	(hl), a
;SMSlib.c:135: SMS_write_to_VDPRegister (HI(feature),VDPReg[HI(feature)]);
;SMSlib_common.c:37: DISABLE_INTERRUPTS;
	di	
;SMSlib_common.c:38: VDPControlPort=v;
	out	(_VDPControlPort), a
;SMSlib_common.c:39: VDPControlPort=VDPReg|0x80;
	ld	a, c
	or	a, #0x80
	out	(_VDPControlPort), a
;SMSlib_common.c:40: ENABLE_INTERRUPTS;
	ei	
;SMSlib.c:135: SMS_write_to_VDPRegister (HI(feature),VDPReg[HI(feature)]);
;SMSlib.c:136: }
	ret
;SMSlib.c:138: void SMS_VDPturnOffFeature (unsigned int feature) __z88dk_fastcall {
;	---------------------------------
; Function SMS_VDPturnOffFeature
; ---------------------------------
_SMS_VDPturnOffFeature::
	ex	de, hl
;SMSlib.c:140: unsigned char val=~LO(feature);
	ld	a, e
	cpl
	ld	c, a
;SMSlib.c:141: VDPReg[HI(feature)]&=val;
	ld	e, d
	ld	d, #0x00
	ld	hl, #_VDPReg
	add	hl, de
	ld	a, (hl)
	and	a, c
	ld	(hl), a
;SMSlib.c:142: SMS_write_to_VDPRegister (HI(feature),VDPReg[HI(feature)]);
;SMSlib_common.c:37: DISABLE_INTERRUPTS;
	di	
;SMSlib_common.c:38: VDPControlPort=v;
	out	(_VDPControlPort), a
;SMSlib_common.c:39: VDPControlPort=VDPReg|0x80;
	ld	a, e
	or	a, #0x80
	out	(_VDPControlPort), a
;SMSlib_common.c:40: ENABLE_INTERRUPTS;
	ei	
;SMSlib.c:142: SMS_write_to_VDPRegister (HI(feature),VDPReg[HI(feature)]);
;SMSlib.c:143: }
	ret
;SMSlib.c:145: void SMS_setBGScrollX (unsigned char scrollX) __z88dk_fastcall {
;	---------------------------------
; Function SMS_setBGScrollX
; ---------------------------------
_SMS_setBGScrollX::
;SMSlib_common.c:37: DISABLE_INTERRUPTS;
	di	
;SMSlib_common.c:38: VDPControlPort=v;
	ld	a, l
	out	(_VDPControlPort), a
;SMSlib_common.c:39: VDPControlPort=VDPReg|0x80;
	ld	a, #0x88
	out	(_VDPControlPort), a
;SMSlib_common.c:40: ENABLE_INTERRUPTS;
	ei	
;SMSlib.c:146: SMS_write_to_VDPRegister(0x08,scrollX);
;SMSlib.c:147: }
	ret
;SMSlib.c:149: void SMS_setBGScrollY (unsigned char scrollY) __z88dk_fastcall {
;	---------------------------------
; Function SMS_setBGScrollY
; ---------------------------------
_SMS_setBGScrollY::
;SMSlib_common.c:37: DISABLE_INTERRUPTS;
	di	
;SMSlib_common.c:38: VDPControlPort=v;
	ld	a, l
	out	(_VDPControlPort), a
;SMSlib_common.c:39: VDPControlPort=VDPReg|0x80;
	ld	a, #0x89
	out	(_VDPControlPort), a
;SMSlib_common.c:40: ENABLE_INTERRUPTS;
	ei	
;SMSlib.c:150: SMS_write_to_VDPRegister(0x09,scrollY);
;SMSlib.c:151: }
	ret
;SMSlib.c:153: void SMS_setBackdropColor (unsigned char entry) __z88dk_fastcall {
;	---------------------------------
; Function SMS_setBackdropColor
; ---------------------------------
_SMS_setBackdropColor::
;SMSlib_common.c:37: DISABLE_INTERRUPTS;
	di	
;SMSlib_common.c:38: VDPControlPort=v;
	ld	a, l
	out	(_VDPControlPort), a
;SMSlib_common.c:39: VDPControlPort=VDPReg|0x80;
	ld	a, #0x87
	out	(_VDPControlPort), a
;SMSlib_common.c:40: ENABLE_INTERRUPTS;
	ei	
;SMSlib.c:154: SMS_write_to_VDPRegister(0x07,entry);
;SMSlib.c:155: }
	ret
;SMSlib.c:157: void SMS_useFirstHalfTilesforSprites (_Bool usefirsthalf) __z88dk_fastcall {
;	---------------------------------
; Function SMS_useFirstHalfTilesforSprites
; ---------------------------------
_SMS_useFirstHalfTilesforSprites::
;SMSlib.c:158: SMS_write_to_VDPRegister(0x06,usefirsthalf?0xFB:0xFF);
	bit	0, l
	jr	Z, 00104$
	ld	bc, #0x00fb
	jr	00105$
00104$:
	ld	bc, #0x00ff
00105$:
	ld	a, c
;SMSlib_common.c:37: DISABLE_INTERRUPTS;
	di	
;SMSlib_common.c:38: VDPControlPort=v;
	out	(_VDPControlPort), a
;SMSlib_common.c:39: VDPControlPort=VDPReg|0x80;
	ld	a, #0x86
	out	(_VDPControlPort), a
;SMSlib_common.c:40: ENABLE_INTERRUPTS;
	ei	
;SMSlib.c:158: SMS_write_to_VDPRegister(0x06,usefirsthalf?0xFB:0xFF);
;SMSlib.c:159: }
	ret
;SMSlib.c:161: void SMS_setSpriteMode (unsigned char mode) __z88dk_fastcall {
;	---------------------------------
; Function SMS_setSpriteMode
; ---------------------------------
_SMS_setSpriteMode::
	ld	c, l
;SMSlib.c:162: if (mode & SPRITEMODE_TALL) {
	bit	0, c
	jr	Z, 00102$
;SMSlib.c:163: SMS_VDPturnOnFeature(VDPFEATURE_USETALLSPRITES);
	push	bc
	ld	hl, #0x0102
	call	_SMS_VDPturnOnFeature
	pop	bc
;SMSlib.c:164: spritesHeight=16;
	ld	hl, #_spritesHeight
	ld	(hl), #0x10
;SMSlib.c:165: spritesTileOffset=2;
	ld	hl, #_spritesTileOffset
	ld	(hl), #0x02
	jr	00103$
00102$:
;SMSlib.c:167: SMS_VDPturnOffFeature(VDPFEATURE_USETALLSPRITES);
	push	bc
	ld	hl, #0x0102
	call	_SMS_VDPturnOffFeature
	pop	bc
;SMSlib.c:168: spritesHeight=8;
	ld	hl, #_spritesHeight
	ld	(hl), #0x08
;SMSlib.c:169: spritesTileOffset=1;
	ld	hl, #_spritesTileOffset
	ld	(hl), #0x01
00103$:
;SMSlib.c:171: if (mode & SPRITEMODE_ZOOMED) {
	bit	1, c
	jr	Z, 00105$
;SMSlib.c:172: SMS_VDPturnOnFeature(VDPFEATURE_ZOOMSPRITES);
	ld	hl, #0x0101
	call	_SMS_VDPturnOnFeature
;SMSlib.c:173: spritesWidth=16;
	ld	hl, #_spritesWidth
	ld	(hl), #0x10
;SMSlib.c:174: spritesHeight*=2;
	ld	hl, #_spritesHeight
	ld	a, (hl)
	add	a, a
	ld	(hl), a
	ret
00105$:
;SMSlib.c:176: SMS_VDPturnOffFeature(VDPFEATURE_ZOOMSPRITES);
	ld	hl, #0x0101
	call	_SMS_VDPturnOffFeature
;SMSlib.c:177: spritesWidth=8;
	ld	hl, #_spritesWidth
	ld	(hl), #0x08
;SMSlib.c:179: }
	ret
;SMSlib.c:182: void GG_setBGPaletteColor (unsigned char entry, unsigned int color) {
;	---------------------------------
; Function GG_setBGPaletteColor
; ---------------------------------
_GG_setBGPaletteColor::
	push	ix
	ld	ix,#0
	add	ix,sp
;SMSlib.c:184: SMS_setAddr(0xC000+(entry*2));
	ld	e, 4 (ix)
	ld	d, #0x00
	ex	de, hl
	add	hl, hl
	ld	de, #0xc000
	add	hl, de
	rst	#0x08
;SMSlib.c:186: SMS_setTile(color);
	ld	l, 5 (ix)
	ld	h, 6 (ix)
;SMSlib.c:187: }
	pop	ix
	jp	_SMS_crt0_RST18
;SMSlib.c:189: void GG_setSpritePaletteColor (unsigned char entry, unsigned int color) {
;	---------------------------------
; Function GG_setSpritePaletteColor
; ---------------------------------
_GG_setSpritePaletteColor::
	push	ix
	ld	ix,#0
	add	ix,sp
;SMSlib.c:191: SMS_setAddr(0xC020+(entry*2));
	ld	e, 4 (ix)
	ld	d, #0x00
	ex	de, hl
	add	hl, hl
	ld	de, #0xc020
	add	hl, de
	rst	#0x08
;SMSlib.c:193: SMS_setTile(color);
	ld	l, 5 (ix)
	ld	h, 6 (ix)
;SMSlib.c:194: }
	pop	ix
	jp	_SMS_crt0_RST18
;SMSlib.c:212: void GG_loadBGPalette (const void *palette) __z88dk_fastcall {
;	---------------------------------
; Function GG_loadBGPalette
; ---------------------------------
_GG_loadBGPalette::
;SMSlib.c:214: ASM_LD_DE_IMM(#SMS_CRAMAddress);
	ld de,#0xC000 
;SMSlib.c:215: ASM_DE_TO_VDP_CONTROL;
	ld c,#_VDPControlPort 
	di 
	out (c),e 
	out (c),d 
	ei 
;SMSlib.c:216: ASM_LD_B_IMM(#32);
	ld b,#32 
;SMSlib.c:217: ASM_SHORT_XFER_TO_VDP_DATA;
	ld c,#_VDPDataPort 
	 1$:
	outi ; 16 
	jp nz,1$ ; 10 = 26 *VRAM SAFE* 
;SMSlib.c:218: }
	ret
;SMSlib.c:220: void GG_loadSpritePalette (const void *palette) __z88dk_fastcall {
;	---------------------------------
; Function GG_loadSpritePalette
; ---------------------------------
_GG_loadSpritePalette::
;SMSlib.c:222: ASM_LD_DE_IMM(#SMS_CRAMAddress+0x20);
	ld de,#0xC000 +0x20 
;SMSlib.c:223: ASM_DE_TO_VDP_CONTROL;
	ld c,#_VDPControlPort 
	di 
	out (c),e 
	out (c),d 
	ei 
;SMSlib.c:224: ASM_LD_B_IMM(#32);
	ld b,#32 
;SMSlib.c:225: ASM_SHORT_XFER_TO_VDP_DATA;
	ld c,#_VDPDataPort 
	 1$:
	outi ; 16 
	jp nz,1$ ; 10 = 26 *VRAM SAFE* 
;SMSlib.c:226: }
	ret
;SMSlib.c:251: void SMS_waitForVBlank (void) {
;	---------------------------------
; Function SMS_waitForVBlank
; ---------------------------------
_SMS_waitForVBlank::
;SMSlib.c:252: VDPBlank=false;
	ld	hl, #_VDPBlank
	ld	(hl), #0x00
;SMSlib.c:253: while (!VDPBlank);
00101$:
	ld	hl, #_VDPBlank
	bit	0, (hl)
	jr	Z, 00101$
;SMSlib.c:254: }
	ret
;SMSlib.c:256: unsigned int SMS_getKeysStatus (void) {
;	---------------------------------
; Function SMS_getKeysStatus
; ---------------------------------
_SMS_getKeysStatus::
;SMSlib.c:257: return (KeysStatus);
	ld	hl, (_KeysStatus)
;SMSlib.c:258: }
	ret
;SMSlib.c:260: unsigned int SMS_getKeysPressed (void) {
;	---------------------------------
; Function SMS_getKeysPressed
; ---------------------------------
_SMS_getKeysPressed::
;SMSlib.c:261: return (KeysStatus&(~PreviousKeysStatus));
	ld	hl, #_PreviousKeysStatus
	ld	a, (hl)
	cpl
	push	af
	inc	hl
	ld	a, (hl)
	cpl
	ld	c, a
	pop	af
	ld	hl, #_KeysStatus
	and	a, (hl)
	ld	e, a
	ld	a, c
	inc	hl
	and	a, (hl)
	ld	d, a
	ex	de, hl
;SMSlib.c:262: }
	ret
;SMSlib.c:264: unsigned int SMS_getKeysHeld (void) {
;	---------------------------------
; Function SMS_getKeysHeld
; ---------------------------------
_SMS_getKeysHeld::
;SMSlib.c:265: return (KeysStatus&PreviousKeysStatus);
	ld	a, (#_KeysStatus)
	ld	hl, #_PreviousKeysStatus
	and	a, (hl)
	ld	e, a
	ld	a, (#_KeysStatus + 1)
	ld	hl, #_PreviousKeysStatus + 1
	and	a, (hl)
	ld	d, a
	ex	de, hl
;SMSlib.c:266: }
	ret
;SMSlib.c:268: unsigned int SMS_getKeysReleased (void) {
;	---------------------------------
; Function SMS_getKeysReleased
; ---------------------------------
_SMS_getKeysReleased::
;SMSlib.c:269: return ((~KeysStatus)&PreviousKeysStatus);
	ld	hl, #_KeysStatus
	ld	a, (hl)
	cpl
	push	af
	inc	hl
	ld	a, (hl)
	cpl
	ld	c, a
	pop	af
	ld	hl, #_PreviousKeysStatus
	and	a, (hl)
	ld	e, a
	ld	a, c
	inc	hl
	and	a, (hl)
	ld	d, a
	ex	de, hl
;SMSlib.c:270: }
	ret
;SMSlib.c:300: void SMS_setLineInterruptHandler (void (*theHandlerFunction)(void)) __z88dk_fastcall {
;	---------------------------------
; Function SMS_setLineInterruptHandler
; ---------------------------------
_SMS_setLineInterruptHandler::
	ld	a, l
	ld	(_SMS_theLineInterruptHandler), a
	ld	a, h
	ld	(_SMS_theLineInterruptHandler + 1), a
;SMSlib.c:301: SMS_theLineInterruptHandler=theHandlerFunction;
;SMSlib.c:302: }
	ret
;SMSlib.c:304: void SMS_setLineCounter (unsigned char count) __z88dk_fastcall {
;	---------------------------------
; Function SMS_setLineCounter
; ---------------------------------
_SMS_setLineCounter::
;SMSlib_common.c:37: DISABLE_INTERRUPTS;
	di	
;SMSlib_common.c:38: VDPControlPort=v;
	ld	a, l
	out	(_VDPControlPort), a
;SMSlib_common.c:39: VDPControlPort=VDPReg|0x80;
	ld	a, #0x8a
	out	(_VDPControlPort), a
;SMSlib_common.c:40: ENABLE_INTERRUPTS;
	ei	
;SMSlib.c:305: SMS_write_to_VDPRegister(0x0A,count);
;SMSlib.c:306: }
	ret
;SMSlib.c:308: void SMS_setVBlankInterruptHandler (void (*theHandlerFunction)(void)) __z88dk_fastcall {
;	---------------------------------
; Function SMS_setVBlankInterruptHandler
; ---------------------------------
_SMS_setVBlankInterruptHandler::
	ld	a, l
	ld	(_SMS_theVBlankInterruptHandler), a
	ld	a, h
	ld	(_SMS_theVBlankInterruptHandler + 1), a
;SMSlib.c:309: SMS_theVBlankInterruptHandler=theHandlerFunction;
;SMSlib.c:310: }
	ret
;SMSlib.c:312: void SMS_setNmiInterruptHandler (void (*theHandlerFunction)(void)) __z88dk_fastcall {
;	---------------------------------
; Function SMS_setNmiInterruptHandler
; ---------------------------------
_SMS_setNmiInterruptHandler::
	ld	a, l
	ld	(_SMS_theNmiInterruptHandler), a
	ld	a, h
	ld	(_SMS_theNmiInterruptHandler + 1), a
;SMSlib.c:313: SMS_theNmiInterruptHandler=theHandlerFunction;
;SMSlib.c:314: }
	ret
;SMSlib.c:317: unsigned char SMS_getVCount (void) {
;	---------------------------------
; Function SMS_getVCount
; ---------------------------------
_SMS_getVCount::
;SMSlib.c:318: return(VDPVCounterPort);
	in	a, (_VDPVCounterPort)
	ld	l, a
;SMSlib.c:319: }
	ret
;SMSlib.c:322: unsigned char SMS_getHCount (void) {
;	---------------------------------
; Function SMS_getHCount
; ---------------------------------
_SMS_getHCount::
;SMSlib.c:323: return(VDPHCounterPort);
	in	a, (_VDPHCounterPort)
	ld	l, a
;SMSlib.c:324: }
	ret
;SMSlib.c:374: void SMS_isr (void) __naked {
;	---------------------------------
; Function SMS_isr
; ---------------------------------
_SMS_isr::
;SMSlib.c:423: __endasm;
	push	af
	push	hl
	in	a,(_VDPStatusPort)
	ld	(_SMS_VDPFlags),a
	rlca
	jr	nc,1$
	ld	hl,#_VDPBlank
	ld	(hl),#0x01
	ld	hl,(_KeysStatus)
	ld	(_PreviousKeysStatus),hl
	in	a,(_IOPortL)
	cpl
	ld	hl,#_KeysStatus
	ld	(hl),a
	in	a,(_GGIOPort)
	cpl
	inc	hl
	ld	(hl),a
	push	bc
	push	de
	push	iy
	ld	hl,(_SMS_theVBlankInterruptHandler)
	call	___sdcc_call_hl
	pop	iy
	pop	de
	pop	bc
	jr	2$
	1$:
	push	bc
	push	de
	push	iy
	ld	hl,(_SMS_theLineInterruptHandler)
	call	___sdcc_call_hl
	pop	iy
	pop	de
	pop	bc
	2$:
	pop	hl
	pop	af
	ei
	reti
;SMSlib.c:424: }
;SMSlib.c:427: void SMS_nmi_isr (void) __naked {          /* this is for NMI */
;	---------------------------------
; Function SMS_nmi_isr
; ---------------------------------
_SMS_nmi_isr::
;SMSlib.c:448: __endasm;
	retn
;SMSlib.c:449: }
	.area _CODE
	.area _INITIALIZER
__xinit__VDPReg:
	.db #0x04	; 4
	.db #0x20	; 32
__xinit__spritesHeight:
	.db #0x08	; 8
__xinit__spritesWidth:
	.db #0x08	; 8
__xinit__spritesTileOffset:
	.db #0x01	; 1
__xinit__SMS_theVBlankInterruptHandler:
	.dw _nop
__xinit__SMS_theNmiInterruptHandler:
	.dw _nop
	.area _CABS (ABS)
