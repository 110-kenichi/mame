;
;*** MAmi-VGM Driver for PC-6001 ROM image ***
;	P6-MSX sound cartridge adaptor version
;	Originated by ioken@SNDR_SNDL
;	Arranged by Niga
;	2023/04/15
;============================================================================
;使い方：
;
;このROMイメージを4000-7FFFHにマッピングされるROMカートリッジにインストールして
;P6のスロットに挿入してください。
;
;モード選択すると画面が暗転して信号入力待になります。
;「MAmi-VSIF for MSX」をP6のジョイスティックポート1に挿してください。
;=============================================================================
;
;対応音源：
;   Select device (*=Default)
;	*:OPN								;I/O : P6SR FM :AD=A0H,WR=A1H, P6m55 FM :AD=70H,WR=71H
;	*:OPNA								;I/O : FM1:AD=14H,WR=15H/ FM2:AD=16H,WR=17H / OPNA_PseudoDAC: AD=16H,WR=17H
;	*:OPN2								;I/O : FM1:AD=14H,WR=15H/ FM2:AD=16H,WR=17H
;	*:OPM								;I/O : FM :AD=3FF0H,WR=3FF1H
;	*:MSX-AUDIO							;I/O : FM :AD=C0H,WR=C1H/ FM2:AD=C2H,WR=C3H
;	*:OPLL								;I/O : FM :AD=7CH,WR=7DH
;	*:OPL3								;I/O : FM :AD=C0H,WR=C1H/ FM2:AD=C2H,WR=C3H
;	*:DCSG								;I/O : WR=3FH
;	1:SCC-I(SNATCHER,SD SNATCHER)		;I/O : PSG:AD=A0H,WR=A1H
;	2:SCC-I(MEGA FLASH ROM SCC+)		;I/O : PSG:AD=A0H,WR=A1H ※MEGA FLASH ROM SCC+ のPSG(PSG:AD=10H,WR=11H)は使用しない
;	3:SCC  (MEGAROM)					;I/O : PSG:AD=A0H,WR=A1H
;
;=============================================================================
;
;データ構造：
;
;[<Chip種別>,<Address>, <Data>],[<Chip種別>,<Address>, <Data>],[<Chip種別>,<Address>, <Data>],....
;[<Chip種別>,<Address>, <Data>],[<連続データを示すマジック番号0x1F>,<Data>],[<連続データを示すマジック番号0x1F>, <Data>],....
;
;=============================================================================
;
;修正履歴
;
; 2023/04/26 DCSG音源(SN76489AN)対応
; ドライバ起動時のDCSG音源の音量オフ 並びに DCSG音源の処理追加。
;
; 2023/04/30 SCC音源対応
; ドライバ起動時の初期設定 並びに SCC音源の処理追加。その他、I/Oポート番号の自己書き換えをやめる。
;
; 2023/05/16 OPM音源対応
; OPM音源の処理追加。
;
; 2023/06/02 OPL3音源対応
; MSX-AUDIO、OPL3音源の処理追加。
;
;=============================================================================

LF	equ	0AH
CR	equ	0DH
;--- P6 BASIC ROM 
PUTCHR	EQU	01075H
CLS		EQU	01DFBH
LOCATE	EQU	0116DH
PUTSTR	EQU	030CFH
PUTCRLF	EQU	02739H
KEYIN	EQU	00FC4H
CONSOLE	EQU	01D73H

;ワークエリアラベル定義
DEVICE	EQU	0FA58H		;OUTPUT DEVICE (0=CRT)

;Ｉ／Ｏラベル定義
PSGAD	EQU	0A0H
PSGWR	EQU	0A1H
PSGRD	EQU	0A2H

;=======================START

	ORG	04000H
	DEFB	"AB"
	DW	INIT							; INIT
	DW	00000H							; STATEMENT
	DW	00000H							; DEVICE
	DW	00000H							; TEXT
	DB	000H,000H,000H,000H,000H,000H	; システム予約領域

INIT:
;========================MSX VER No. check
	LD	A,(0002DH)		;機種判定（0:MSX1、1:MSX2、2:MSX2+、3:MSXturboR、$8E:PC-6001系）メインROMの$2Dから取得
	AND	11111100B
	OR	A
	JP	Z,ERRMSX

;========================画面作成
MES:
	XOR	A
	LD	(DEVICE),A

	LD	A,3
	LD	(0FD93H),A		; COLOR第1パラメータ（前景色）設定（COLOR n,,）
	CALL	CLS			; CLS命令
	LD	A,1				; COLOR第3パラメータ設定（COLOR ,,n）
	CALL	01DBBH		; COLOR第3パラメータ実行

	XOR	A				; Aゼロクリア
	LD	(0FDA6H),A		; ファンクションキー非表示

	LD	A,1				; 緑
	LD	(0FD93H),A		; COLOR第1パラメータ（前景色）設定（COLOR n,,）
	LD	HL,MESVER		;オープニングメッセージ
	CALL	PUTSTR

	LD	A,4				; 橙反転
	LD	(0FD93H),A		; COLOR第1パラメータ（前景色）設定（COLOR n,,）
	LD	HL,MESMNU1		;MENU表示
	CALL	PUTSTR

	LD	A,3				; 橙
	LD	(0FD93H),A		; COLOR第1パラメータ（前景色）設定（COLOR n,,）
	LD	HL,MESMNU2		;MENU表示
	CALL	PUTSTR

;========================MSX-AUDIO(Y8950)の有効化設定
	LD  A,1
	LD	(07FFFH),A		;FM-PAC(OPLL)の 有効化

;========================FM-PAC(OPLL)の有効化設定
	LD  A,1
	LD	(07FF6H),A		;FM-PAC(OPLL)の 有効化

;========================MMMのミュート解除とDCSGの有効化設定（初代機のみ）
MMM:
; 初代機とそれ以外をポート0xF6（内部タイマのカウントアップ値指定（mkⅡ以降））を使って判別
; 書き込んだ値と読みだした値が一致しなければ初代機と判定し、MMMのミュート解除とDCSG音源の音量をゼロにする。
	DI					;割り込み禁止
	LD	C,0F6H
	IN	B,(C)			;ポート0xF6 の値を保存する
	LD	DE,55AAH
	OUT	(C),D			;ポート0xF6 に0x55を出力してテストする
	IN	A,(C)
	CP	D				;読み出して確認
	JR	NZ,MMM_P6
	OUT	(C),E			;ポート0xF6 に0xAAを出力してテストする
	IN	A,(C)
	CP	E				;読み出して確認
	JR	NZ,MMM_P6

	OUT	(C),B			;ポート0xF6 の値を戻す
	JR	MMM_SKIP		;mkⅡ以降の為、以下スキップする
	EI					;割り込み解除

MMM_P6:
	DI					;割り込み禁止
	LD	A,080H
	OUT	(03CH),A		;MMM mapped IO enable

	LD	A,040H
	LD	(0803CH),A		;DCSGの有効化
	XOR	A
	OUT	(03CH),A		;MMM mapped IO disable

	LD  A,09FH			;DCSG VOLUME=0に初期化
	OUT (03FH),A
	LD  A,0BFH
	OUT (03FH),A
	LD  A,0DFH
	OUT (03FH),A
	LD  A,0FFH
	OUT (03FH),A	
	EI					;割り込み解除
MMM_SKIP:

;========================SCCの場合はSCC有効化設定
LOOP0:
	CALL	KEYIN		;1文字入力
	CALL	PUTCHR		;入力した1文字を出力

	PUSH	AF			;A退避
	LD		HL,00H		;ウェイトを入れる
	LD		B,01H
	CALL	WAIT
	POP		AF			;A戻す

	CP	031H			;6:SCC-I？
	JP	NZ,SET_SKIP1

	LD	A,01010101b		;全ページ(00,01,02,03)に拡張スロット01を指定してSCCを表に出す
	LD	(0FFFFH),A
    LD  HL,0BFFEH		;SCC-I: スナッチャー系SCCの有効化（固有モード）
    LD  A,020H			;
    LD  (HL),A			;
    LD  HL,0B000H		;
    LD  A,080H			;
    LD  (HL),A			;
	JP	START

SET_SKIP1:
	CP	032H			;7:SCC-I(MEGA FLASH ROM SCC+)？
	JP	NZ,SET_SKIP2

	LD	A,01010101b		;全ページ(00,01,02,03)に拡張スロット01を指定してSCCを表に出す
	LD	(0FFFFH),A
    LD  HL,0BFFEH		;SCC-I: MEGA FLASH ROM SCC+の有効化（互換モード）
    LD  A,0
    LD  (HL),A
    LD  HL,09000H
    LD  A,03FH
    LD  (HL),A
	JP	START

SET_SKIP2:
	CP	033H			;8:SCC(MEGAROM)？
	JP	NZ,SET_SKIP3

	LD	A,01010101b		;全ページ(00,01,02,03)に拡張スロット01を指定してSCCを表に出す
	LD	(0FFFFH),A
    LD  HL,09000H		;MEGAROM系SCCの有効化
    LD  A,03FH
    LD  (HL),A
	JP	START

SET_SKIP3:
	CP	020H			;スペースキー？
	JP	Z,START
	CP	00DH			;リターンキー？
	JP	Z,START
	LD	H,2				;X座標設定
	LD	L,15			;Y座標設定
	CALL 	LOCATE		;LOCATE命令
	LD	A,020H
	CALL	PUTCHR		;入力した1文字を出力
	LD	H,2				;X座標設定
	LD	L,15			;Y座標設定
	CALL 	LOCATE		;LOCATE命令
	JP	LOOP0			;いずれも該当しない場合は入力待ちに戻る

START:
	IN	A,(0C8H)
	OR	00000010b
	OUT	(0C8H),A		;P6SRのBUSREQオフ
	LD	A,2
	OUT	(093H),A		;全機種画面オフ
	JP	05000H			;MAINへ

;========================MSXで実行した場合
ERRMSX:
	LD	HL,MESMSX		;"This soft cannot work on MSX."をしばらく表示する
	LD	B,116
MSXLP:
	LD	A,(HL)
	CALL	(000A2H)
	INC	HL
	DJNZ	MSXLP

	LD	HL,00H			;ウェイトを入れる
	LD	B,03H
	CALL	WAIT

MSXEND:
	SCF					;キャリーフラグを設定して他の全てのカートリッジに
	RET					;BASICへ戻る

;=======================WAIT
;ウェイト処理（HL:長さ、B:HLで指定した長さを何回分回すか）
WAIT:
WAIT_LOOP:
	DEC		HL
	LD		A,L
	OR		H
	JP		NZ,WAIT_LOOP
	DJNZ	WAIT
	RET

;=======================DATA

MESVER:	
	DEFB	"MAmi-VSIF P6 DRIVER ver 1.0     "
	DEFB	"(C)2023 itoken@SNDR_SNDL        "
	DEFB	"P6 ver. Programed by Niga & Sei ",0

MESMNU1:
	DEFB	"Select device (*=Default)       ",0
MESMNU2:
	DEFB	"*:OPN (YM2203)",CR,LF
	DEFB	"*:OPNA(YM2608)",CR,LF
	DEFB	"*:OPM (YM2151)",CR,LF
	DEFB	"*:OPN2(YM2612)",CR,LF
	DEFB	"*:OPLL(YM2413)",CR,LF
	DEFB	"*:OPL3(YMF289),MSX-AUDIO(Y8950)",CR,LF
	DEFB	"*:DCSG(SN76489)",CR,LF
	DEFB	"1:SCC-I(SNATCHER,SD SNATCHER)",CR,LF
	DEFB	"2:SCC-I(MEGA FLASH ROM SCC+)",CR,LF
	DEFB	"3:SCC  (MEGAROM)",CR,LF
	DEFB	">",0

MESMSX:
	DEFB	"                             "
	DEFB	"MAmi-VGM Driver for PC-6001  "
	DEFB	"          cannot work on MSX."
	DEFB	"                             "

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
;============================== main program
	ORG	05000H

MAIN:
	DI
	LD	A,15 
	OUT	(PSGAD),A
	LD	A,0C0H			;MSXの場合0CFH
	OUT	(PSGWR),A		;P6はSet Joy1 Pin Input Mode

	LD	C,14			;7 14 ;set PSG address to C
	LD	A,C
	OUT	(PSGAD),A

MAIN_LOOP:
	IN	A,(PSGRD)		; 12
	AND	00100000b		;  8	bit5=1 まで待機
	JP	Z, MAIN_LOOP	; 11
	IN	A,(PSGRD)		; 12
	AND	00011111b		;  8	コマンドIDを抜き出す
	OR	01100000b		;  8	60Hを足す
	LD	H,A				;  5
	LD	L,0				;  8
	JP	(HL)			;  5 77 コマンドIDのジャンプ先アドレスに飛ぶ（下記「コマンドIDの概念」参照）

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
;	コマンドIDの概念
;
;	IN A,(PSGRD)時のAの bit5=1 のとき、bit4:0がコマンドIDなっている
;	コマンドIDは 5bit= 32通り(0-1FH)
;	コマンドIDに60Hを加算するとJPアドレスの上位8bitになる(6000-7F00H)
;	----------------------------------------------------------------
;
;	コマンドID一覧
;
;	ID	JP先	役割(*はP1ルーチンのみ)	P6での利用状況
;	-------------------------------------------------------
;	00	6000	WRITE_PSG_IO			ACTIVE
;	01	6100	WRITE_OPLL_IO			ACTIVE
;	02	6200	SELECT_OPLL_SLOT		NG
;	03	6300	SELECT_SCC_SLOT			NG
;	04	6400	WRITE_SCC1 				*
;	05	6500	WRITE_SCC 				*
;	06	6600	WRITE_SCC1_2BYTES 		*
;	07	6700	WRITE_SCC_2BYTES 		*
;	08	6800	WRITE_SCC1_32_BYTES 	*
;	09	6900	WRITE_SCC_32_BYTES 		*
;	0A	6A00	WRITE_OPL3_IO1          ACTIVE(MSX-AUDIO共通)
;	0B	6B00	WRITE_OPL3_IO2          ACTIVE(MSX-AUDIO共通)
;	0C	6C00	N/A(P2はWRITE_OPLL_MEM)
;	0D	6D00	SELECT_OPM_SLOT			NG
;	0E	6E00	WRITE_OPM_MEM			ACTIVE
;	0F	6F00	WRITE_DCSG				ACTIVE
;	10	7000	WRITE_OPN2_IO1			ACTIVE
;	11	7100	WRITE_OPN2_IO2			ACTIVE
;	12	7200	WRITE_OPN_IO			ACTIVE
;	13	7300	WRITE_OPNA_PseudoDAC	ACTIVE
;	14	7400	WRITE_OPN2_DAC			ACTIVE
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
    	ORG 06000H
;    __WRITE_PSG_IO

PSG_AH:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,PSG_AH		; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

PSG_AL:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,PSG_AL		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61
	LD	C,14			; 7 14

;=======================
	LD	H,00010000b		;  8
PSG_DH:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, PSG_DH		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

PSG_DL:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,PSG_DL		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,B             ; 
	OUT	(0A0H),A        ; 本体PSG想定
	NOP
	NOP
	NOP
	NOP
	NOP
	OUT	(070H),A        ; PC-60m55想定
	NOP
	NOP
	NOP
	NOP
	NOP
	LD	A,D             ; 
	OUT	(0A1H),A        ; 本体PSG想定
	NOP
	NOP
	NOP
	NOP
	NOP
	OUT	(071H),A        ; PC-60m55想定
	NOP
	NOP
	NOP
	NOP
	NOP

	LD	A,C             ; JOY1 Pin Read Mode
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
	JP	(HL)			; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG 06100H
;    __WRITE_OPLL_IO
OPLL_AH:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,OPLL_AH		; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

OPLL_AL:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,OPLL_AL		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

OPLL_LP:
	LD	A,B
	OUT	(07CH),A

;=======================
	LD	H,00010000b		;  8
OPLL_DH:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, OPLL_DH		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

OPLL_DL:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPLL_DL		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,D             ; 
	OUT	(07DH),A       	; 89
	INC	B
	LD	E,01FH

;=======================
OPLL_CLR:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPLL_CLR		;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E				;ダミーID(1FH)の場合
	JP	Z,OPLL_LP		;連続書き込みへ
	OR	060H
	LD	H,A
	LD	L,0
	JP	(HL)			; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06200H

SELECT_OPLL_SLOT:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,SELECT_OPLL_SLOT
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06300H

SELECT_SCC_SLOT:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,SELECT_SCC_SLOT
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06400H

WRITE_SCC1:
SCC1_AH:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,SCC1_AH		; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

SCC1_AL:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,SCC1_AL		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

;=======================
SCC1_DH:
	LD	H,00010000b		;  8
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ,SCC1_DH		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

SCC1_DL:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,SCC1_DL		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  H,0B8H 	        ; SCCレジスタ設定
    LD  L,B             ; 
    LD  (HL), D         ; 

    ; Continuous Write
    INC B               ; 
    LD  E,01FH          ; 34 89 連続データを示すマジック番号
;=======================
SCC1_CLR:
	IN	A,(PSGRD)       ; 12
	AND	00100000b       ;  5
	JP	Z, SCC1_CLR     ; 11
	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
    CP  E               ;  5
    JP  Z,SCC1_DH   	; 11 61 Continuous Write

	OR  001100000b         	
	LD	H,A 
	LD	L,0
	JP	(HL)			; Jump to other ID

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06500H

WRITE_SCC:
SCC_AH:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,SCC_AH		; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

SCC_AL:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,SCC_AL		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

;=======================
SCC_DH:
	LD	H,00010000b		;  8
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, SCC_DH		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

SCC_DL:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,SCC_DL		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  H,098H         ; SCCレジスタ設定
    LD  L,B             ; 
    LD  (HL), D         ; 

    ; Continuous Write
    INC B               ; 
    LD  E,01FH         ; 34 89 連続データを示すマジック番号
;=======================
SCC_CLR:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, SCC_CLR
	IN	A,(PSGRD)
	AND	E
    CP  E               ; 
    JP  Z,SCC_DH   		; Continuous Write

	OR  001100000b         	
	LD	H,A 
	LD	L,0
	JP	(HL)			; Jump to otHer ID

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06600H

WRITE_SCC1_2BYTES:
SCC1_2BYTES_AH:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,SCC1_2BYTES_AH	; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

SCC1_2BYTES_AL:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,SCC1_2BYTES_AL	; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

;=======================1ND DATA
SCC1_2BYTES_DH:
	LD	H,00010000b		;  8
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, SCC1_2BYTES_DH	; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

SCC1_2BYTES_DL:
	IN	A,(PSGRD)       ; 12
	AND	00010000b       ;  8
	JP	Z,SCC1_2BYTES_DL	; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  H,0B8H          ; SCCレジスタ設定
    LD  L,B             ; 

;=======================2ND DATA
SCC1_2BYTES_DH2:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	NZ, SCC1_2BYTES_DH2	; 11

	IN	A,(PSGRD)		; 12
	LD	E,A				;  5
	SLA	E				; 10
	SLA	E				; 10
	SLA	E				; 10
	SLA	E				; 10

SCC1_2BYTES_DL2:
	IN	A,(PSGRD)       ; 12
	AND	00010000b       ;  8
	JP	Z,SCC1_2BYTES_DL2	; 11

	IN	A,(PSGRD)       ; 12
	AND	00001111b		;  8
    OR  E               ;  5 56

    ;WRITE BOTH 1st BYTE AND 2nd BYTE
    LD  (HL), D         ;  8 
    INC L               ;  5 
    LD  (HL), A         ;  8 
	JP	MAIN_LOOP		; Jump to other ID

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06700H

WRITE_SCC_2BYTES:
SCC_2BYTES_AH:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,SCC_2BYTES_AH	; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

SCC_2BYTES_AL:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,SCC_2BYTES_AL	; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

;=======================1ND DATA
SCC_2BYTES_DH:
	LD	H,00010000b		;  8
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, SCC_2BYTES_DH	; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

SCC_2BYTES_DL:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,SCC_2BYTES_DL	; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  H,098H          ; SCCレジスタ設定
    LD  L,B             ; 

;=======================2ND DATA
SCC_2BYTES_DH2:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	NZ, SCC_2BYTES_DH2	; 11

	IN	A,(PSGRD)		; 12
	LD	E,A				;  5
	SLA	E				; 10
	SLA	E				; 10
	SLA	E				; 10
	SLA	E				; 10

SCC_2BYTES_DL2:
	IN	A,(PSGRD)       ; 12
	AND	00010000b       ;  8
	JP	Z,SCC_2BYTES_DL2	; 11

	IN	A,(PSGRD)       ; 12
	AND	00001111b		;  8
    OR  E               ;  5 56

    ;WRITE BOTH 1st BYTE AND 2nd BYTE
    LD  (HL), D         ;  8
    INC L               ;  5 
    LD  (HL), A         ;  8 
	JP	MAIN_LOOP		; Jump to other ID

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06800H

WRITE_SCC1_32_BYTES:
SCC1_32_BYTES_AH:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,SCC1_32_BYTES_AH		; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

SCC1_32_BYTES_AL:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,SCC1_32_BYTES_AL		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

;=======================1ST DATA
SCC1_32_BYTES_DH:
	LD	H,00010000b		;  8
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ,SCC1_32_BYTES_DH		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

SCC1_32_BYTES_DL:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,SCC1_32_BYTES_DL		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  H,0B8H          ; SCCレジスタ設定
    LD  L,B             ; 
    LD  (HL), D         ; 

;=======================2～32 DATA
    LD  B,031H          ;  8  8	カウント設定
SCC1_32BYTES_DH2:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	NZ, SCC1_32BYTES_DH2	; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10

SCC1_32BYTES_DL2:
	IN	A,(PSGRD)       ; 12
	AND	00010000b       ;  8
	JP	Z,SCC1_32BYTES_DL2	; 11

	IN	A,(PSGRD)       ; 12
	AND	00001111b		;  8
    OR  D               ;  5 56

    ;WRITE NEXT BYTE
    INC L               ;  5 
    LD  (HL), A         ;  8 
    DJNZ SCC1_32BYTES_DH2 ; 14 83  9 78
	JP	MAIN_LOOP		; Jump to other ID

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06900H

WRITE_SCC_32_BYTES:
SCC_32_BYTES_AH:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,SCC_32_BYTES_AH		; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

SCC_32_BYTES_AL:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,SCC_32_BYTES_AL		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

;=======================1ST DATA
SCC_32_BYTES_DH:
	LD	H,00010000b		;  8
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, SCC_32_BYTES_DH		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

SCC_32_BYTES_DL:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,SCC_32_BYTES_DL		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  H,098H          ; SCCレジスタ設定
    LD  L,B             ; 
    LD  (HL), D         ;

;=======================2～32 DATA
    LD  B,031H          ;  8  8	カウント設定
SCC_32BYTES_DH2:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	NZ, SCC_32BYTES_DH2	; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10

SCC_32BYTES_DL2:
	IN	A,(PSGRD)       ; 12
	AND	00010000b       ;  8
	JP	Z,SCC_32BYTES_DL2	; 11

	IN	A,(PSGRD)       ; 12
	AND	00001111b		;  8
    OR  D               ;  5 56

    ;WRITE NEXT BYTE
    INC L               ;  5 
    LD  (HL), A         ;  8 
    DJNZ SCC_32BYTES_DH2 ; 14 83  9 78
	JP	MAIN_LOOP		; Jump to other ID

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06A00H

WRITE_OPL3_IO1:
    IN  A,(PSGRD)       ; 12
    AND 00100000b       ;  8
    JP  NZ,WRITE_OPL3_IO1 ; 11
    IN  A,(PSGRD)       ; 12
    LD  B,A             ;  5
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10 88

OPL3_AL1:
    IN  A,(PSGRD)       ; 12
    AND 00010000b       ;  8
    JP  Z,OPL3_AL1      ; 11

    IN  A,(PSGRD)       ; 12
    AND 00001111b       ;  8
    OR  B               ;  5
    LD  B,A             ;  5 61

OPL3_LP1:
    LD  A,B
    OUT (0C0H),A		;MSX-AUDIO,OPL3共通
	NOP
	NOP
	NOP
	NOP
	NOP

    CP  0FH             ;  8 86
    JP  Z,WRITE_OPL3_IO1_ADPCM  ; 11 97
;=======================
    LD  H,00010000b     ;  8
OPL3_DH1:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  NZ,OPL3_DH1     ; 11

    IN  A,(PSGRD)       ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    LD  E,00001111b     ;  8 101

OPL3_DL1:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  Z,OPL3_DL1      ; 11

    IN  A,(PSGRD)       ; 12
    AND E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  A,D             ; 
    OUT (0C1H),A        ;MSX-AUDIO,OPL3共通
	NOP
	NOP
	NOP

    INC B
    LD  E,01FH
;=======================
OPL3_CLR1:
    IN  A,(PSGRD) 
    AND 00100000b 
    JP  Z, OPL3_CLR1    ;コマンドIDを待つ
    IN  A,(PSGRD)
    AND E
    CP  E               ;ダミーID(1FH)の場合
    JP  Z,OPL3_LP1      ;連続書き込みへ
    OR  060H
    LD  H,A
    LD  L,0
    JP  (HL)            ; 94

;=======================  ADPCM burst write mode
WRITE_OPL3_IO1_ADPCM:
    LD  H,00010000b     ;  8
OPL3_PCM_DH1:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  NZ,OPL3_PCM_DH1 ; 11

    IN  A,(PSGRD)       ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    LD  E,00001111b     ;  8 101

OPL3_PCM_DL1:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  Z,OPL3_PCM_DL1  ; 11

    IN  A,(PSGRD)       ; 12
    AND E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  A,D             ; 
    OUT (0C1H),A        ;MSX-AUDIO,OPL3共通
	NOP
	NOP
	NOP

    ; Continuous Write
	LD	E,01FH			; 13 85

;=======================
OPL3_PCM_CLR1:
    IN  A,(PSGRD) 
    AND 00100000b 
    JP  Z, OPL3_PCM_CLR1        ;コマンドIDを待つ
    IN  A,(PSGRD)
    AND E
    CP  E                       ;ダミーID(1FH)の場合
    JP  Z,WRITE_OPL3_IO1_ADPCM  ;連続書き込みへ
    OR  060H
    LD  H,A
    LD  L,0
    JP  (HL)            ; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06B00H

WRITE_OPL3_IO2:
    IN  A,(PSGRD)       ; 12
    AND 00100000b       ;  8
    JP  NZ,WRITE_OPL3_IO2 ; 11
    IN  A,(PSGRD)       ; 12
    LD  B,A             ;  5
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10
    SLA B               ; 10 88

OPL3_AL2:
    IN  A,(PSGRD)       ; 12
    AND 00010000b       ;  8
    JP  Z,OPL3_AL2      ; 11

    IN  A,(PSGRD)       ; 12
    AND 00001111b       ;  8
    OR  B               ;  5
    LD  B,A             ;  5 61

OPL3_LP2:
    LD  A,B
    OUT (0C2H),A        ;MSX-AUDIO,OPL3共通
	NOP
	NOP
	NOP
	NOP
	NOP
	
	CP  0FH             ;  8 86
    JP  Z,WRITE_OPL3_IO2_ADPCM  ; 11 97
;=======================
    LD  H,00010000b     ;  8
OPL3_DH2:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  NZ,OPL3_DH2     ; 11

    IN  A,(PSGRD)       ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    LD  E,00001111b     ;  8 101

OPL3_DL2:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  Z,OPL3_DL2      ; 11

    IN  A,(PSGRD)       ; 12
    AND E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  A,D             ; 
    OUT (0C3H),A        ;MSX-AUDIO,OPL3共通
	NOP
	NOP
	NOP

    INC B
    LD  E,01FH
;=======================
OPL3_CLR2:
    IN  A,(PSGRD) 
    AND 00100000b 
    JP  Z, OPL3_CLR2    ;コマンドIDを待つ
    IN  A,(PSGRD)
    AND E
    CP  E               ;ダミーID(1FH)の場合
    JP  Z,OPL3_LP2      ;連続書き込みへ
    OR  060H
    LD  H,A
    LD  L,0
    JP  (HL)            ; 94

;=======================  ADPCM burst write mode
WRITE_OPL3_IO2_ADPCM:
    LD  H,00010000b     ;  8
OPL3_PCM_DH2:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  NZ,OPL3_PCM_DH2 ; 11

    IN  A,(PSGRD)       ; 12
    LD  D,A             ;  5
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    SLA D               ; 10
    LD  E,00001111b     ;  8 101

OPL3_PCM_DL2:
    IN  A,(PSGRD)       ; 12
    AND H               ;  5
    JP  Z,OPL3_PCM_DL2  ; 11

    IN  A,(PSGRD)       ; 12
    AND E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55

    LD  A,D             ; 
    OUT (0C3H),A        ;MSX-AUDIO,OPL3共通
	NOP
	NOP
	NOP

    ; Continuous Write
	LD	E,01FH			; 13 85

;=======================
OPL3_PCM_CLR2:
    IN  A,(PSGRD) 
    AND 00100000b 
    JP  Z, OPL3_PCM_CLR2        ;コマンドIDを待つ
    IN  A,(PSGRD)
    AND E
    CP  E                       ;ダミーID(1FH)の場合
    JP  Z,WRITE_OPL3_IO2_ADPCM  ;連続書き込みへ
    OR  060H
    LD  H,A
    LD  L,0
    JP  (HL)            ; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06C00H

VGM_LOOP6C:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP6C
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06D00H

SELECT_OPM_SLOT:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,SELECT_OPM_SLOT
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06E00H

WRITE_OPM_MEM:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,WRITE_OPM_MEM	; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

OPM_AL3:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,OPM_AL3		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	EX	AF,AF'			;  5 61 裏Aにアドレス値を退避

;=======================
	LD	H,00010000b		;  8
OPM_DH3:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, OPM_DH3		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

OPM_DL3:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPM_DL3		; 11

	IN	A,(PSGRD)       ; 12
	EX	AF,AF'			;  5 読み取り値退避しつつアドレス値復帰

;OPM_AD1:
	LD	(03FF0H),A		; 12 PC-60m55W(左)
	EX	AF,AF'			;  5  読み取り値復帰しつつアドレス値退避
	AND	E               ;  5
	OR	D               ;  5
;OPM_AD2:
	LD	(03FF1H),A		; 12 PC-60m55W(左)
	EX	AF,AF'			;  5 アドレス値復帰
	INC	A				;  5 
	EX	AF,AF'			;  5 アドレス+1して退避

	LD	E,01FH			;  8
	LD	A,14            ;  8 JOY1 Pin Read Mode
	OUT	(PSGAD),A		; 12 167
;=======================
OPM_CLR3:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPM_CLR3		;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E				;ダミーID(1FH)の場合
	JP	Z,OPM_DH3		;連続書き込みへ
	OR	060H
	LD	H,A
	LD	L,0
	JP	(HL)			; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	06F00H

WRITE_DCSG:
DCSG_AH:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,DCSG_AH		; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

DCSG_AL:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,DCSG_AL		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

	LD	C,14			;  7 14

;=======================
	LD	H,00010000b		;  8
DCSG_DH:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, DCSG_DH		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

DCSG_DL:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,DCSG_DL		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
    OR  D               ;  5
    LD  D,A             ;  5 55
    LD  A,D             ; 
    OUT (03FH),A      	; 

	LD	A,C             ; JOY1 Pin Read Mode
	OUT	(PSGAD),A       ; 12 101
;=======================
DCSG_CLR:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, DCSG_CLR
	IN	A,(PSGRD)
	AND	00011111b
	OR	01100000b
	LD	H,A 
	LD	L,0
	JP	(HL)			; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07000H

WRITE_OPN2_IO1:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,WRITE_OPN2_IO1	; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

OPN2_AL1:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,OPN2_AL1		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

OPN2_LP1:
	LD	A,B
	OUT	(014H),A
;=======================
	LD	H,00010000b		;  8
OPN2_DH1:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, OPN2_DH1		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

OPN2_DL1:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPN2_DL1		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,D             ; 
	OUT	(015H),A     	; 89
	INC	B
	LD	E,01FH
;=======================
OPN2_CLR1:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPN2_CLR1		;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E				;ダミーID(1FH)の場合
	JP	Z,OPN2_LP1		;連続書き込みへ
	OR	060H
	LD	H,A
	LD	L,0
	JP	(HL)			; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07100H

WRITE_OPN2_IO2:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,WRITE_OPN2_IO2	; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

OPN2_AL2:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,OPN2_AL2		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	LD	B,A				;  5 61

OPN2_LP2:
	LD	A,B
	OUT	(016H),A
	CP	008H
	JP	Z,WRITE_OPN2_IO2_ADPCM


;=======================
	LD	H,00010000b		;  8
OPN2_DH2:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, OPN2_DH2		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

OPN2_DL2:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPN2_DL2		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,D             ; 
	OUT	(017H),A    	; 89
	INC	B
	LD	E,01FH

;=======================
OPN2_CLR2:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPN2_CLR2	;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E		;ダミーID(1FH)の場合
	JP	Z,OPN2_LP2	;連続書き込みへ
	OR	060H
	LD	H,A
	LD	L,0
	JP	(HL)			; 94

;=======================  ADPCM burst write mode
WRITE_OPN2_IO2_ADPCM:
	LD	H,00010000b		;  8

OPN2_PCM_DH:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, OPN2_PCM_DH	; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

OPN2_PCM_DL:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPN2_PCM_DL	; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD 	A,D
	OUT	(017H),A		; 17 72

	; Continuous Write
    	LD 	E,01FH		; 13 85
;=======================
OPN2_PCM_CLR:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPN2_PCM_CLR	;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E				;ダミーID(1FH)の場合
	JP	Z,WRITE_OPN2_IO2_ADPCM	;連続書き込みへ
	OR	060H
	LD	H,A
	LD	L,0
	JP	(HL)			; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07200H

WRITE_OPN_IO:
	IN 	A,(PSGRD)		; 12
	AND	00100000b		;  8
	JP	NZ,WRITE_OPN_IO	; 11
	IN 	A,(PSGRD)		; 12
	LD 	B,A				;  5
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10
	SLA	B				; 10 88

OPN_AL3:
	IN	A,(PSGRD)		; 12
	AND	00010000b		;  8
	JP	Z,OPN_AL3		; 11

	IN	A,(PSGRD)		; 12
	AND	00001111b		;  8
	OR	B				;  5
	EX	AF,AF'			;  5 61 裏Aにアドレス値を退避

;=======================
	LD	H,00010000b		;  8
OPN_DH3:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ, OPN_DH3		; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

OPN_DL3:
	IN	A,(PSGRD)       ; 12
	AND	H               ;  5
	JP	Z,OPN_DL3		; 11

	IN	A,(PSGRD)       ; 12
	EX	AF,AF'			;  5 読み取り値退避しつつアドレス値復帰

;OPN_AD1:
	OUT	(0A0H),A		; 12 本体OPN想定
;	OUT	(060H),A		; 12 PC-60m55W(左)
	NOP
	NOP
	NOP
	NOP
	NOP
	OUT	(070H),A		; 12 PC-60m55想定
	NOP
	NOP
	NOP
	NOP
	NOP

	EX	AF,AF'			;  5  読み取り値復帰しつつアドレス値退避
	AND	E               ;  5
	OR	D               ;  5
;OPN_AD2:
	OUT	(0A1H),A		; 12 OPN2で代用時は自己書き換えされる
						;	以下83Clkのwait必要
;	OUT	(061H),A		; 12 PC-60m55W(左)
	NOP					;  5
	NOP					;  5
	NOP					;  5
	OUT	(071H),A		; 12 PC-60m55想定
	NOP					;  5
	NOP					;  5
	NOP					;  5

	EX	AF,AF'			;  5 アドレス値復帰
	INC	A				;  5 
	EX	AF,AF'			;  5 アドレス+1して退避

	LD	E,01FH			;  8
	LD	A,14            ;  8 JOY1 Pin Read Mode
	OUT	(PSGAD),A		; 12 167
;=======================
OPN_CLR3:
	IN	A,(PSGRD) 
	AND	00100000b 
	JP	Z, OPN_CLR3		;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E				;ダミーID(1FH)の場合
	JP	Z,OPN_DH3		;連続書き込みへ
	OR	060H
	LD	H,A
	LD	L,0
	JP	(HL)			; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07300H

WRITE_OPNA_PseudoDAC:
	LD	A,00BH 			;  8
	OUT	(016H),A 		; 12 20
;=======================
	LD	H,00010000b		;  8
OPNA_DAC2:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ,OPNA_DAC2	; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

OPNA_DAC3:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	Z,OPNA_DAC3		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,D				;  5
	OUT	(017H),A		; 12 17 

	LD	 E,01FH			; 30 85
;=======================
OPNA_DAC4:
	IN	A,(PSGRD)
	AND	00100000b 
	JP	Z,OPNA_DAC4		;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E				;ダミーID(1FH)の場合
	JP	Z,OPNA_DAC2		;連続書き込みへ
	OR	060H
	LD	H,A
	LD	L,0
	JP	(HL)			; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07400H

WRITE_OPN2_DAC:
	LD	A,02AH 			;  8
	OUT	(014H),A 		; 12 20
;=======================
	LD	H,00010000b		;  8
OPN2_DAC2:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	NZ,OPN2_DAC2	; 11

	IN	A,(PSGRD)		; 12
	LD	D,A				;  5
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	SLA	D				; 10
	LD	E,00001111b		;  8 101

OPN2_DAC3:
	IN	A,(PSGRD)		; 12
	AND	H				;  5
	JP	Z,OPN2_DAC3		; 11

	IN	A,(PSGRD)       ; 12
	AND	E               ;  5
	OR	D               ;  5
	LD	D,A             ;  5 55

	LD	A,D				;  5
	OUT	(015H),A		; 12 17 

	LD	 E,01FH			; 30 85
;=======================
OPN2_DAC4:
	IN	A,(PSGRD)
	AND	00100000b 
	JP	Z,OPN2_DAC4		;コマンドIDを待つ
	IN	A,(PSGRD)
	AND	E
	CP	E				;ダミーID(1FH)の場合
	JP	Z,OPN2_DAC2		;連続書き込みへ
	OR	060H
	LD	H,A
	LD	L,0
	JP	(HL)			; 94

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07500H

VGM_LOOP75:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP75
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07600H

VGM_LOOP76:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP76
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07700H

VGM_LOOP77:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP77
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07800H

VGM_LOOP78:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP78
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07900H

VGM_LOOP79:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP79
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07A00H

VGM_LOOP7A:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7A
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07B00H

VGM_LOOP7B:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7B
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07C00H
VGM_LOOP7C:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7C
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07D00H

VGM_LOOP7D:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7D
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07E00H

VGM_LOOP7E:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7E
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================
	ORG	07F00H

VGM_LOOP7F:
	IN 	A,(PSGRD)
	AND	00100000b
	JP	NZ,VGM_LOOP7F
	JP	MAIN

	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH	;境界識別用
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH
	DEFB	0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH,0FFH

;=============================================================================


	end
