;
;*** MAmi-VGM Driver for PC-6001 ROM image ***
;	Originated by ioken@SNDR_SNDL
;	Arranged by Niga
;	2022/12/27
;============================================================================
;使い方：
;
;このROMイメージを戦士のカートリッジ、または4000-7FFFhにマッピングされる
;ROMカートリッジにインストールしてP6のスロットに挿入してください。
;起動するとPrimary OPNデバイスの選択メニューが出ます。
;
;１:OPN(P6SR)選択時
;	SR内蔵OPN : MAmi上で「OPN」としてアクセスできます
;	似非P6ROMのOPN2 : MAmi上で「OPN2」としてアクセスできます
;	OPN用の楽曲データはOPNのFM音源部+SSG音源部で発音されます
;
;２:OPN2(ESE-P6)を選択時
;	SR内蔵OPN : MAmi上で「AY-3-8910」としてアクセスできます
;	似非P6ROMのOPN2 : MAmi上で「OPN2」としてアクセスできます
;	OPN用の楽曲データはOPN2のFM音源部+OPNのSSG音源部で発音されます
;
;だだし、本ROMプログラムを戦士のカートリッジではなくROMで運用した場合は
;いずれを選択しても「1.OPN(P6SR)」のモードになります。
;
;モード選択すると画面が暗転して信号入力待になります。
;「MAmi-VSIF for MSX」をP6のジョイスティックポート１に挿してください。
;=============================================================================
;
;対応音源：
;
;	PSG(AY-3-8910)	IO address = A0-A1h
;	OPN(YM2203)	IO address = A0-A1h
;	OPN2(YM2612)	IO address = 14-17h
;	OPNA(YM2608)	IO address = 14-17h（未確認）
;
;	PSGとOPN、OPN2とOPNAは排他利用です。いずれか１つを選択してください。
;
;	OPNAのADPCMも鳴ると思いますがP6用カートリッジが無いため未確認です。
;
;	MSX仕様のOPLL(#7C-7Dh)は戦士のカートリッジのバンクレジスタ(#70-7Fh)と
;	IOアドレスが重なるため非対応ですが、ROMで運用することを前提に少々
;	プログラムを改造すれば鳴ると思います。
;=============================================================================

LF	equ	0ah
CR	equ	0dh
;--- P6 BASIC ROM 
PUTCHR	EQU	01075H
CLS	EQU	01DFBH
LOCATE	EQU	0116DH
PUTSTR	EQU	030CFH
PUTCRLF	EQU	02739H
KEYIN	EQU	00FC4H

;ワークエリアラベル定義
DEVICE	EQU	0FA58H		;OUTPUT DEVICE (0=CRT)

;Ｉ／Ｏラベル定義
PSGAD	EQU	0A0h
PSGWR	EQU	0A1h
PSGRD	EQU	0A2h

;=======================START

	ORG	04000H
	DEFB	"AB"
	DEFB	004H,040H

INIT:	LD	A,(0002DH)	;MSX VER No. check
	AND	11111100B
	OR	A
	JP	Z,ERRMSX

	XOR	A
	OUT	(070H),A	;BANK REGISTER INIT

	LD	BC,00100H	;RAM 8000h-にプログラム転送
	LD	DE,08000H
	LD	HL,P6LDIR
	LDIR

	LD	BC,00100H	;転送できているか（8000H-がRAMか）確認
	LD	DE,08000H
	LD	HL,P6LDIR
LOOP1:	LD	A,(DE)
	CP	(HL)
	JR	NZ,MES		;戦士のカートリッジではない(ROMかも？)
	INC	DE
	INC	HL
	DEC	BC
	LD	A,B
	OR	C
	JR	NZ,LOOP1
	JP	08000H		;6000-7FFFH(RAM)にプログラム転送

MES:	CALL	CLS
	XOR	A
	LD	(DEVICE),A
	LD	HL,MESVER	;オープニングメッセージ
	CALL	PUTSTR

MES2:	LD	HL,MESMNU	;MENU表示
	CALL	PUTSTR

LOOP0:	CALL	KEYIN		;OPNをSR内蔵使用か、OPN2代理発音か選択
	CP	"1"
	JR	Z,START
	CP	"2"
	JR	NZ,LOOP0

	LD	A,014H		;OPN2 Ch1をPrimaryOPNとして使用する
	LD	(OPN_AD1+1),A
	INC	A
	LD	(OPN_AD2+1),A	;OPNのIOアドレスを自己書き換え

START:	IN	A,(0C8h)
	OR	00000010b
	OUT	(0C8h),A	;P6SRのBUSREQオフ
	LD	A,2
	OUT	(093h),A	;全機種画面オフ
	JP	05000h		;MAINへ

;========================MSXで実行した場合
ERRMSX:	DI
	LD	HL,MESMSX
	LD	B,29
MSXLP:	LD	A,(HL)
	CALL	(000A2H)
	INC	HL
	DJNZ	MSXLP

MSXEND:	JP	MSXEND

;========================8000Hに転送されるルーチン

P6LDIR:	LD	A,1		;戦士のカートリッジ用
	OUT	(070H),A	;4000-5FFFh ROM BANK=1を表に出す

	LD	BC,02000H	;RAM 6000-7FFFh-にプログラム転送
	LD	DE,06000H
	LD	HL,04000H
	LDIR

	XOR	A
	OUT	(070H),A	;4000-5FFFh ROM BANK=0に戻す
	JP	MES

;=======================DATA

MESVER:	DEFB	"MAmi-VSIF P6 DRIVER ver 0.02",CR,LF
	DEFB	"(C)2023 itoken@SNDR_SNDL",CR,LF
	DEFB	"P6 ver. Programed by Niga",CR,LF,CR,LF,0

MESMNU:	DEFB	"Select OPN Primary device",CR,LF
	DEFB	"1:OPN(P6SR) 2:OPN2(ESE-P6)",0

MESMSX:	DEFB	"This soft cannot work on MSX."

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
;============================== main program
	ORG	05000h

MAIN:	DI
	LD	A,15 
	OUT	(PSGAD),A
	LD	A,0C0h		;MSXの場合0CFh
	OUT	(PSGWR),A	;Set Joy2 Pin Input Mode

	LD	C,14		;7 14 ;set PSG address to C
	LD	A,C
	OUT	(PSGAD),A

MAIN_LOOP:
	IN	A,(PSGRD)	; 12
	AND	00100000b	;  8	bit5=1 まで待機
	JP	Z, MAIN_LOOP	; 11
	IN	A,(PSGRD)	; 12
	AND	00011111b	;  8	コマンドIDを抜き出す
	OR	01100000b	;  8	60hを足す
	LD	H,A		;  5
	LD	L,0		;  8
	JP	(HL)		;  5 77

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
;	コマンドIDの概念
;
;	IN A,(PSGRD)時のAの bit5=1 のとき、bit4:0がコマンドIDなっている
;	コマンドIDは 5bit= 32通り(0-1Fh)
;	コマンドIDに60hを加算するとJPアドレスの上位8bitになる(6000-7F00h)
;	----------------------------------------------------------------
;
;	コマンドID一覧
;
;	ID	JP先	役割(*はP1ルーチンのみ)	P6での利用状況
;	-------------------------------------------------------
;	00	6000	WRITE_PSG_IO		ACTIVE
;	01	6100	WRITE_OPLL_IO
;	02	6200	SELECT_OPLL_SLOT
;	03	6300	SELECT_SCC_SLOT	
;	04	6400	WRITE_SCC1 *
;	05	6500	WRITE_SCC *
;	06	6600	WRITE_SCC1_2BYTES *
;	07	6700	WRITE_SCC_2BYTES *
;	08	6800	WRITE_SCC1_32_BYTES *
;	09	6900	WRITE_SCC_32_BYTES *
;	0A	6A00	WRITE_OPL3_IO1
;	0B	6B00	WRITE_OPL3_IO2
;	0C	6C00	N/A(P2はWRITE_OPLL_MEM)
;	0D	6D00	SELECT_OPM_SLOT
;	0E	6E00	WRITE_OPM_MEM
;	0F	6F00	WRITE_DCSG
;	10	7000	WRITE_OPN2_IO1		ACTIVE
;	11	7100	WRITE_OPN2_IO2		ACTIVE
;	12	7200	WRITE_OPN_IO		ACTIVE
;	13	7300	N/A
;	14	7400	N/A
;	15	7500	N/A
;	16	7600	N/A
;	17	7700	N/A
;	18	7800	N/A
;	19	7900	N/A
;	1A	7A00	N/A
;	1B	7B00	N/A
;	1C	7C00	N/A
;	1D	7D00	N/A
;	1E	7E00	N/A
;	1F	7F00	DUMMY ID
;
;=============================================================================
    	ORG 06000h
;    __WRITE_PSG_IO

PSG_AL:
	IN 	A,(PSGRD)	; 12
	AND	00100000b	;  8
	JP	NZ,PSG_AL	; 11
	IN 	A,(PSGRD)	; 12
	LD 	B,A		;  5
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10 88

PSG_AH:
	IN	A,(PSGRD)	; 12
	AND	00010000b	;  8
	JP	Z,PSG_AH	; 11

	IN	A,(PSGRD)	; 12
	AND	00001111b	;  8
	OR	B		;  5
	LD	B,A		;  5 61
	LD	C,14		; 7 14

;=======================
	LD	H,00010000b	;  8
PSG_DL:
	IN	A,(PSGRD)	; 12
	AND	H		;  5
	JP	NZ, PSG_DL	; 11

	IN	A,(PSGRD)	; 12
	LD	D,A		;  5
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	LD	E,00001111b	;  8 101

PSG_DH:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,PSG_DH	; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,B             ; 
	OUT	(PSGAD),A       ; 
	LD	A,D             ; 
	OUT	(PSGWR),A       ; 89

	LD	A,C             ; JOY2 Pin Read Mode
	OUT	(PSGAD),A       ; 12 101
;=======================
PSG_CLR:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, PSG_CLR
	IN	A,(PSGRD)
	AND	00011111b
	OR	01100000b
	LD	H,A 
	LD	L,0
	JP	(HL)		; 94

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG 06100h
;    __WRITE_OPLL_IO
OPLL_AL:
	IN 	A,(PSGRD)	; 12
	AND	00100000b	;  8
	JP	NZ,OPLL_AL	; 11
	IN 	A,(PSGRD)	; 12
	LD 	B,A		;  5
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10 88

OPLL_AH:
	IN	A,(PSGRD)	; 12
	AND	00010000b	;  8
	JP	Z,OPLL_AH	; 11

	IN	A,(PSGRD)	; 12
	AND	00001111b	;  8
	OR	B		;  5
	LD	B,A		;  5 61

OPLL_LP:
	LD	A,B
;	OUT	(07Ch),A

;=======================
	LD	H,00010000b	;  8
OPLL_DL:
	IN	A,(PSGRD)	; 12
	AND	H		;  5
	JP	NZ, OPLL_DL	; 11

	IN	A,(PSGRD)	; 12
	LD	D,A		;  5
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	LD	E,00001111b	;  8 101

OPLL_DH:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPLL_DH	; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,D             ; 
;	OUT	(07Dh),A       ; 89
	INC	B
	LD	E,01Fh

;=======================
OPLL_CLR:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPLL_CLR	;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E		;ダミーID(1Fh)の場合
	JP	Z,OPLL_LP	;連続書き込みへ
	OR	060h
	LD	H,A
	LD	L,0
	JP	(HL)		; 94

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06200h

SELECT_OPLL_SLOT:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,SELECT_OPLL_SLOT
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06300h

SELECT_SCC_SLOT:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,SELECT_SCC_SLOT
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06400h

WRITE_SCC1:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_SCC1
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06500h

WRITE_SCC:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_SCC
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06600h

WRITE_SCC1_2BYTES:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_SCC1_2BYTES
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06700h

WRITE_SCC_2BYTES:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_SCC_2BYTES
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06800h

WRITE_SCC1_32_BYTES:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_SCC1_32_BYTES
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06900h

WRITE_SCC_32_BYTES:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_SCC_32_BYTES
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06A00h

WRITE_OPL3_IO1:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_OPL3_IO1
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06B00h

WRITE_OPL3_IO2:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_OPL3_IO2
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06C00h

VGM_LOOP6C:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP6C
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06D00h

SELECT_OPM_SLOT:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,SELECT_OPM_SLOT
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06E00h

WRITE_OPM_MEM:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_OPM_MEM
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	06F00h

WRITE_DCSG:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,WRITE_DCSG
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07000h

WRITE_OPN2_IO1:
	IN 	A,(PSGRD)	; 12
	AND	00100000b	;  8
	JP	NZ,WRITE_OPN2_IO1	; 11
	IN 	A,(PSGRD)	; 12
	LD 	B,A		;  5
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10 88

OPN_AH1:
	IN	A,(PSGRD)	; 12
	AND	00010000b	;  8
	JP	Z,OPN_AH1	; 11

	IN	A,(PSGRD)	; 12
	AND	00001111b	;  8
	OR	B		;  5
	LD	B,A		;  5 61

OPN_LP1:
	LD	A,B
	OUT	(014h),A
;=======================
	LD	H,00010000b	;  8
OPN_DL1:
	IN	A,(PSGRD)	; 12
	AND	H		;  5
	JP	NZ, OPN_DL1	; 11

	IN	A,(PSGRD)	; 12
	LD	D,A		;  5
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	LD	E,00001111b	;  8 101

OPN_DH1:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPN_DH1	; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,D             ; 
	OUT	(015h),A       ; 89
	INC	B
	LD	E,01Fh
;=======================
OPN_CLR1:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPN_CLR1	;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E		;ダミーID(1Fh)の場合
	JP	Z,OPN_LP1	;連続書き込みへ
	OR	060h
	LD	H,A
	LD	L,0
	JP	(HL)		; 94

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07100h

WRITE_OPN2_IO2:
	IN 	A,(PSGRD)	; 12
	AND	00100000b	;  8
	JP	NZ,WRITE_OPN2_IO2	; 11
	IN 	A,(PSGRD)	; 12
	LD 	B,A		;  5
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10 88

OPN_AH2:
	IN	A,(PSGRD)	; 12
	AND	00010000b	;  8
	JP	Z,OPN_AH2	; 11

	IN	A,(PSGRD)	; 12
	AND	00001111b	;  8
	OR	B		;  5
	LD	B,A		;  5 61

OPN_LP2:
	LD	A,B
	OUT	(016h),A
	CP	008h
	JP	Z,WRITE_OPN2_IO2_ADPCM


;=======================
	LD	H,00010000b	;  8
OPN_DL2:
	IN	A,(PSGRD)	; 12
	AND	H		;  5
	JP	NZ, OPN_DL2	; 11

	IN	A,(PSGRD)	; 12
	LD	D,A		;  5
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	LD	E,00001111b	;  8 101

OPN_DH2:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPN_DH2	; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,D             ; 
	OUT	(017h),A       ; 89
	INC	B
	LD	E,01Fh

;=======================
OPN_CLR2:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPN_CLR2	;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E		;ダミーID(1Fh)の場合
	JP	Z,OPN_LP2	;連続書き込みへ
	OR	060h
	LD	H,A
	LD	L,0
	JP	(HL)		; 94

;=======================  ADPCM burst write mode
WRITE_OPN2_IO2_ADPCM:
	LD	H,00010000b	;  8

OPN_PCM_DL:
	IN	A,(PSGRD)	; 12
	AND	H		;  5
	JP	NZ, OPN_PCM_DL	; 11

	IN	A,(PSGRD)	; 12
	LD	D,A		;  5
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	LD	E,00001111b	;  8 101

OPN_PCM_DH:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPN_PCM_DH	; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD 	A,D
	OUT	(017h),A	; 17 72

	; Continuous Write
    	LD 	E,01Fh		; 13 85
;=======================
OPN_PCM_CLR:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPN_PCM_CLR	;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E		;ダミーID(1Fh)の場合
	JP	Z,WRITE_OPN2_IO2_ADPCM	;連続書き込みへ
	OR	060h
	LD	H,A
	LD	L,0
	JP	(HL)		; 94

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07200h

WRITE_OPN_IO:
	IN 	A,(PSGRD)	; 12
	AND	00100000b	;  8
	JP	NZ,WRITE_OPN_IO	; 11
	IN 	A,(PSGRD)	; 12
	LD 	B,A		;  5
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10 88

OPN_AH3:
	IN	A,(PSGRD)	; 12
	AND	00010000b	;  8
	JP	Z,OPN_AH3	; 11

	IN	A,(PSGRD)	; 12
	AND	00001111b	;  8
	OR	B		;  5
	EX	AF,AF'		;  5 61 裏Aにアドレス値を退避

;=======================
	LD	H,00010000b	;  8
OPN_DL3:
	IN	A,(PSGRD)	; 12
	AND	H		;  5
	JP	NZ, OPN_DL3	; 11

	IN	A,(PSGRD)	; 12
	LD	D,A		;  5
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	LD	E,00001111b	;  8 101

OPN_DH3:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPN_DH3	; 11

	IN	A,(PSGRD)       ; 12
	EX	AF,AF'		;  5 読み取り値退避しつつアドレス値復帰

OPN_AD1:
	OUT	(PSGAD),A	; 12 OPNで2代用時は自己書き換えされる

	EX	AF,AF'		;  5  読み取り値復帰しつつアドレス値退避
	AND	E               ;  5
	OR	D               ;  5
OPN_AD2:
	OUT	(PSGWR),A	; 12 OPN2で代用時は自己書き換えされる
				;	以下83Clkのwait必要
	EX	AF,AF'		;  5 アドレス値復帰
	INC	A		;  5 
	EX	AF,AF'		;  5 アドレス+1して退避

	LD	E,01Fh		;  8
	NOP			;  40
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP
	NOP
	LD	A,14            ;   8 JOY2 Pin Read Mode
	OUT	(PSGAD),A	;  12 167
;=======================
OPN_CLR3:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPN_CLR3	;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E		;ダミーID(1Fh)の場合
	JP	Z,OPN_DL3	;連続書き込みへ
	OR	060h
	LD	H,A
	LD	L,0
	JP	(HL)		; 94

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07300h

VGM_LOOP73:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP73
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07400h

VGM_LOOP74:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP74
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07500h

VGM_LOOP75:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP75
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07600h

VGM_LOOP76:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP76
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07700h

VGM_LOOP77:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP77
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07800h

VGM_LOOP78:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP78
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07900h

VGM_LOOP79:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP79
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07A00h

VGM_LOOP7A:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7A
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07B00h

VGM_LOOP7B:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7B
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07C00h
VGM_LOOP7C:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7C
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07D00h

VGM_LOOP7D:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7D
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07E00h

VGM_LOOP7E:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7E
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================
	ORG	07F00h

VGM_LOOP7F:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7F
	JP	MAIN

	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh	;境界識別用
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh
	DEFB	0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh,0FFh

;=============================================================================


	end
