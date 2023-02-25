;
;*** MAmi-VGM Driver for PC-6001 TAPE image ***
;	Originated by ioken@SNDR_SNDL
;	Arranged by Niga
;	2023/02/25
;============================================================================
;使い方：
;
;起動するとPrimary OPNデバイスの選択メニューが出ます。
;1/2/3キーでモード選択すると画面が暗転してVGMデータ入力待機になります。
;
;「MAmi-VSIF for MSX」はP6のジョイスティックポート１に挿してください。
;MAmidiMEmoのVGMPlayerを起動し、下記のようにコネクトしてください。
;なお、似非P6ROMではOPNAデータのADPCM部は発音されません。
;---------------------------------------------------------------------------
;１:Primary OPN = P6SR選択時
;
;	・非SR内蔵PSG :「AY-3-8910(PSG)」としてコネクト
;	・SR内蔵OPN :「YM2203(OPN)」と「AY-3-8910(PSG)」としてコネクト
;	・似非P6ROMのOPN2 :「YM261(OPN2)」としてコネクト
;
;	PSG用の楽曲データは本体のSSG(PSG)で発音されます。
;	OPN用の楽曲データはSR本体のFM+SSGで発音されます。
;	OPN2用の楽曲データはOPN2のFMで発音されます。
;	OPNA用の楽曲データはOPN2のFM+本体のSSG(PSG)で発音されます。
;
;本体音源のみ、またはSR機＋似非P6ROM使用時はこのモードに設定します
;----------------------------------------------------------------------------
;２:Primary OPN = PC-60m55選択時
;
;	・本体内蔵PSG :「AY-3-8910(PSG)」としてコネクト
;	・PC-60m55 :「YM2203(OPN)」としてコネクト
;
;	PSG用の楽曲データは本体のPSGで発音されます。
;	OPN用の楽曲データはPC-60m55のFM+SSGで発音されます。
;
;非SR機にPC-60m55を使用する場合はこのモードに設定します
;----------------------------------------------------------------------------
;３:Primary OPN = OPN2(似非P6ROM)選択時
;
;	・本体内蔵PSG :「AY-3-8910(PSG)」としてコネクト
;	・似非P6ROMのOPN2 :「YM2612(OPN2)」としてコネクト
;
;	PSG用の楽曲データは本体のPSGで発音されます。
;	OPN用の楽曲データはOPN2のFM+本体のPSGで発音されます。
;	OPN2用の楽曲データはOPN2のFMで発音されます。
;	OPNA用の楽曲データはOPN2のFM+本体のPSGで発音されます。
;
;非SR機＋似非P6ROM使用時はこのモードに設定します
;SR機＋似非P6ROMでOPN2メインで使いたい時もこのモードにできます
;----------------------------------------------------------------------------
;
;対応音源：
;
;	PSG(AY-3-8910)	初代P6/SR内蔵OPNのSSG部	IO address = A0-A1h
;	OPN(YM2203)	SR内蔵OPN/PC-60m55	IO address = A0-A1h
;	OPN2(YM2612)	似非P6ROM		IO address = 14-17h
;	OPNA(YM2608)	未確認			IO address = 14-17h
;	OPLL(YM2413)	未確認			IO address = 7C-7Dh
;
;	SR機ののPSG = 内蔵OPNのSSG部であり、IOアドレスを共有しています。
;	OPNAのADPCMも鳴ると思いますが実ハードが無いため未確認です。
;	FMPACも鳴ると思いますが、ROMを無効化しておく必要があると思います。
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

	ORG	0C600h

	DI
	LD	A,011h
	OUT	(0F0H),A	;メモリマップを初期化
;	LD	A,0DDh
;	OUT	(0F1h),A
	EI

MES:	CALL	CLS
	XOR	A
	LD	(DEVICE),A
	LD	HL,MESVER	;オープニングメッセージ
	CALL	PUTSTR

MES2:	LD	HL,MESMNU	;MENU表示
	CALL	PUTSTR

LOOP0:	CALL	KEYIN		;MODE選択
	CP	"1"
	JR	Z,START
	CP	"2"
	JR	Z,MODE2
	CP	"3"
	JR	NZ,LOOP0

MODE3:	LD	A,014h
	JR	SET_OPN_AD	;60m55をPrimaryOPNに割り当てる

MODE2:	LD	A,070H		;PC-60m55をPrimaryOPNとして使用する

SET_OPN_AD:
	LD	(OPN_AD1+1),A
	INC	A
	LD	(OPN_AD2+1),A	;IOアドレスを自己書き換え

START:	DI
	IN	A,(0C8h)
	OR	00000010b
	OUT	(0C8h),A	;P6SRのBUSREQオフ
	LD	A,2
	OUT	(093h),A	;全機種画面オフ
	JP	MAIN

;=======================DATA

MESVER:	DEFB	"MAmi-VSIF P6 DRIVER ver 0.05",CR,LF
	DEFB	"(C)2023 itoken@SNDR_SNDL",CR,LF
	DEFB	"P6 ver. Programed by Niga",CR,LF,CR,LF,0

MESMNU:	DEFB	"Select Primary OPN device",CR,LF
	DEFB	"1:P6SR 2:PC-60m55 3:OPN2",0

;=============================================================================
;	コマンドIDの概念 (MSX ROM版オリジナル)
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
;	01	6100	WRITE_OPLL_IO		ACTIVE（未確認）
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
;	13	7300	WRITE_OPNA_PseudoDAC
;	14	7400	WRITE_OPN2_DAC		ACTIVE
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
;----------------------------------------------
JP_TABLE:
	JP	WRITE_PSG_IO		;00
	NOP
	JP	WRITE_OPLL_IO		;01
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	WRITE_OPN2_IO1		;10
	NOP
	JP	WRITE_OPN2_IO2		;11
	NOP
	JP	WRITE_OPN_IO		;12
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	WRITE_OPN2_DAC		;14
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
	JP	DUMMY_LOOP
	NOP
;=============================================================================
;main program
;=============================================================================

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

	ADD	A,A		;  5
	ADD	A,A		;  5
	LD	B,0		;  8
	LD	C,A		;  5
	LD	HL,JP_TABLE	; 11
	ADD	HL,BC		; 12
	JP	(HL)		;  5

;=============================================================================
WRITE_PSG_IO:
	IN 	A,(PSGRD)	; 12
	AND	00100000b	;  8
	JP	NZ,WRITE_PSG_IO	; 11
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

	ADD	A,A		;  5
	ADD	A,A		;  5
	LD	B,0		;  8
	LD	C,A		;  5
	LD	HL,JP_TABLE	; 11
	ADD	HL,BC		; 12
	JP	(HL)		;  5

;=============================================================================
WRITE_OPLL_IO:
	IN 	A,(PSGRD)	; 12
	AND	00100000b	;  8
	JP	NZ,WRITE_OPLL_IO	; 11
	IN 	A,(PSGRD)	; 12
	LD 	B,A		;  5
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10
	SLA	B		; 10 88

	LD	A,1		;FMPAC IO  ENABLE
	LD	(07FF6h),A

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
	OUT	(07Ch),A

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
	OUT	(07Dh),A       ; 89
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

	ADD	A,A		;  5
	ADD	A,A		;  5
	LD	B,0		;  8
	LD	C,A		;  5
	LD	HL,JP_TABLE	; 11
	ADD	HL,BC		; 12
	JP	(HL)		;  5

;=============================================================================
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

	ADD	A,A		;  5
	ADD	A,A		;  5
	LD	B,0		;  8
	LD	C,A		;  5
	LD	HL,JP_TABLE	; 11
	ADD	HL,BC		; 12
	JP	(HL)		;  5

;=============================================================================
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

	ADD	A,A		;  5
	ADD	A,A		;  5
	LD	B,0		;  8
	LD	C,A		;  5
	LD	HL,JP_TABLE	; 11
	ADD	HL,BC		; 12
	JP	(HL)		;  5

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
	JP	Z, OPN_PCM_CLR		;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E			;ダミーID(1Fh)の場合
	JP	Z,WRITE_OPN2_IO2_ADPCM	;連続書き込みへ

	ADD	A,A		;  5
	ADD	A,A		;  5
	LD	B,0		;  8
	LD	C,A		;  5
	LD	HL,JP_TABLE	; 11
	ADD	HL,BC		; 12
	JP	(HL)		;  5

;=============================================================================
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

	ADD	A,A		;  5
	ADD	A,A		;  5
	LD	B,0		;  8
	LD	C,A		;  5
	LD	HL,JP_TABLE	; 11
	ADD	HL,BC		; 12
	JP	(HL)		;  5

;=============================================================================
WRITE_OPN2_DAC:
	LD	A,02Ah 		;  8
	OUT	(014h),A 	; 12 20
	;=======================
	LD	H,00010000b	;  8
OPN2_DAC2:
	IN	A,(PSGRD)	; 12
	AND	H		;  5
	JP	NZ,OPN2_DAC2	; 11

	IN	A,(PSGRD)	; 12
	LD	D,A		;  5
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	SLA	D		; 10
	LD	E,00001111b	;  8 101

OPN2_DAC3:
	IN	A,(PSGRD)	; 12
	AND	H		;  5
	JP	Z,OPN2_DAC3	; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,D		;  5
	OUT	(015h),A	; 12 17 
	LD	 E,01Fh		; 30 85
;=======================
OPN2_DAC4:
	IN	A,(PSGRD)
	AND	00100000b 
	JP	Z,OPN2_DAC4	;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E		;ダミーID(1Fh)の場合
	JP	Z,OPN2_DAC2	;連続書き込みへ

	ADD	A,A		;  5
	ADD	A,A		;  5
	LD	B,0		;  8
	LD	C,A		;  5
	LD	HL,JP_TABLE	; 11
	ADD	HL,BC		; 12
	JP	(HL)		;  5

;=============================================================================
DUMMY_LOOP:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,DUMMY_LOOP
	JP	MAIN

;=============================================================================


	end
