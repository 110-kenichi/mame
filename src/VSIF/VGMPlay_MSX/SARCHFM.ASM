LF	= #0xa
CR	= #0xd

;--- MAIN-ROM内
RDSLT	= #0x000C
WRSLT	= #0x0014
ENASLT	= #0x0024
VER_NO	= #0x002D
INT_NO	= #0x002B
RDRES	= #0x017A		;ﾘｾｯﾄｽﾃｰﾀｽをAﾚｼﾞｽﾀｰに転送するＢＩＯＳ
RWRES	= #0x017D		;Aﾚｼﾞｽﾀｰをﾘｾｯﾄｽﾃｰﾀｽに転送するＢＩＯＳ
INIT32	= #0x006F		;スプライトＯＦＦ（スクリーン１へ）

CHGMOD	= #0x005F		;SCREEN MODEを変更
CHGCLR	= #0x0062		;画面の色を変える
INITXT	= #0x006C		;TEXT1 MODE
CLS	=     #0x0C3		;CLS
CHPUT	= #0x0A2		;1文字出力

;ワークエリアラベル定義
EXPTBL	= #0xFCC1		;拡張スロット存在テーブル 4bytes
LINL40	= #0xF3AE		;TEXT1モードの1行の幅
FORCLR	= #0xF3E9		;前景色
HIMEM	= #0xFC4A		;メモリ上限
H_TIMI	= #0xFD9F		;タイマー割り込みフック


;ユーザー定義のワークリア
SCC0_S	= #0xE000		;+0 SCC0 スロット番号
				;+1 SCC0 切り替え用拡張スロットレジスタ
				;+2 SCC0 運用時基本スロットレジスタ
				;+3 SCC0 P2+P3切り替え用基本スロットレジスタ
SCC1_S	= #0xE004		;以下同様

OPLL0_S	= #0xE008

OPLL1_S	= #0xE00C

OPM0_S	= #0xE010

OPM1_S	= #0xE014

DCSG_F	= #0xE018		;発見された音源の数
OPLL_F	= #0xE019		;OPLLは bit7:1 = EXT / bit0 = INT
SCC_F	= #0xE01A
OPM_F	= #0xE01B

;===============ROMヘッダ

	; ORG	04000H

	; DEFB	"AB"		;ROMID
	; DEFB	010H,040H	;INIT
	; DEFB	0,0		;STATEMENT
	; DEFB	0,0		;DEVICE
	; DEFB	0,0		;TEXT
	; DEFB	0,0,0,0,0,0	;RESERVE


;==============初期化

	; ORG	04010H

_SARCHFM::
	DI
	IM	1		;割り込みモード１
	LD	SP,#0xF000	;スタックポインタ設定

	LD	HL,#0xE000	;ワークエリアのクリア
	LD	DE,#0xE001	;EOOOH-E7FFH
	LD	BC,#0x07FF
	LD	(HL),#0
	LDIR

	;HACK:
	; LD	HL,FORCLR	;前景色ワークエリア
	; LD	A,15
	; LD	(HL),A		;前景色15にする

	; INC	HL
	; XOR	A
	; LD	(HL),A		;背景色0にする

	; LD      A,32
	; LD      (LINL40),A	;1行の幅を指定

	; CALL	INITXT		;テキスト画面を初期化
	; CALL	CLS

	LD	HL,#STR_VER	;バージョン表示
	CALL	STRPUT

;===================================
;スロットスキャン開始
;===================================

	
SSCAN:	XOR	A		;基本スロット番号を初期化

SSCAN_1:	
	LD	E,A		;調査する基本スロット番号をEに保存
	LD	HL,#EXPTBL	;拡張スロットテーブル FCC1h
	LD	C,A		;BC=0A
	LD	B,#0
	ADD	HL,BC
	LD	A,(HL)
	AND	#0x80		;bit7抽出
	OR	E		;A=拡張フラグ付きスロット番号に変換

	;========拡張スロット検索ループ
SSCAN_2:
	CALL	CKSLT		;カートリッジチェックへ
	JP	NC,SSCAN_6	;音源は見つからず

	;---------------発見された音源情報の表示
	PUSH	BC
	LD	HL,#STR_NAME	;音源名の文字列先頭アドレス
	INC	B
	DEC	B
	JR	Z,SSCAN_4
	LD	DE,#10		;1音源につき10文字

SSCAN_3:
	ADD	HL,DE
	DJNZ	SSCAN_3

SSCAN_4:
	PUSH	AF
	CALL	STRPUT		;音源名
	LD	HL,#STR_DTCT	; detected on SLOT #
	CALL	STRPUT
	POP	AF

	PUSH	AF
	AND	#0b00000011	;基本スロット番号を抽出
	ADD	A,#48		;ASCIIコードに変換
	CALL	CHPUT		;基本スロット番号表示表示
	POP	AF

	BIT	7,A
	JR	Z,SSCAN_5	;拡張されていないならスキップ

	PUSH	AF
	LD	A,#45		;-表示
	CALL	CHPUT
	POP	AF

	PUSH	AF
	AND	#12
	SRL	A
	SRL	A
	ADD	A,#48
	CALL	CHPUT		;拡張スロット番号表示
	POP	AF

SSCAN_5:
	PUSH	AF		;改行
	LD	A,#CR
	CALL	CHPUT
	LD	A,#LF
	CALL	CHPUT
	POP	AF
	POP	BC

	CALL	FM_INI		;音源割り当て


	;=======次のスロット検索へ
SSCAN_6:
	BIT	7,A		;拡張されて無いなら
	JP	Z,SSCAN_7

	ADD	A,#0x04		;拡張スロット番号＋１
	BIT	4,A		    ;桁があふれてないなら
	JP	Z,SSCAN_2	;SSCAN_2 へ戻って検索

	AND	#3		;桁があふれたら拡張スロット番号リセット

SSCAN_7:
	INC	A		;基本スロット番号＋１
	CP	#4		;＃4未満なら
	JP	C,SSCAN_1	;SSCAN_1 に戻る

;===================================
;OPLLの初期化
;
;OPLL_F = 0 : EXTx0 INTx0 = 初期化不要
;OPLL_F = 1 : EXTx0 INTx1 = 初期化不要
;OPLL_F = 2 : EXTx1 INTx0 = OPLL0=IOアクセス
;OPLL_F = 3 : EXTx1 INTx1 = OPLL0=メモリマップドIOアクセス
;OPLL_F = 4 : EXTx2 INTx0 = OPLL0=メモリマップドIOアクセス,OPLL1=IOアクセス
;OPLL_F = 5 : EXTx2 INTx1 = OPLL0+OPLL1=メモリマップドIOアクセス
;
;===================================
OPLL_INI:
	LD	A,(OPLL_F)
	CP	#2
	JP	C,_END		;初期化不要
	PUSH	AF

	LD	A,(OPLL0_S)
	ld	h,#0x80
	CALL	ENASLT		;OPLL0 をP2に出す

	POP	AF
	JP	Z,OPLL_INI1

	;----------OPLL0=メモリマップドIOで初期化
	XOR	A
	LD	(#0x7FF6),A	;IO disable
	LD	A,(OPLL_F)
	CP	#4
	JP	C,_END
	PUSH	AF

	LD	A,(OPLL1_S)
	ld	h,#0x80
	CALL	ENASLT		;OPLL1 をP2に出す

	POP	AF
	JP	Z,OPLL_INI1

	;----------OPLL1=メモリマップドIOで初期化
	XOR	A
	LD	(#0x7FF6),A	;IO disable
	JP	_END

	;----------OPLLx=IOアクセスで初期化
OPLL_INI1:
	LD	A,#1
	LD	(#0x7FF6),A	;IO enable


;===================================
;終了
;===================================

_END:
	LD	HL,#STR_COMP	;complete MES
	CALL	STRPUT

	.globl _uart_processVgm
	JP	_uart_processVgm


;=========================サブルーチン==========================

;============================================================
;指定スロットの音源の有無、種類をチェック
;============================================================
;準備 :A :スロット　(Aは保存される)
;
;戻り：	B=0:DCSG 1:SCC-1 2:SCC 3:OPLL(I) 4:OPLL(E) 5:OPM
;	Cy=1 音源発見／　Cy=0 音源無し
;
;	注：PAGE2はターゲットスロットに切り替わる
;============================================================

CKSLT:	PUSH	AF		;スロット番号をスタックに退避
	ld	h,#0x80
	call	ENASLT		;ページ２のスロットをターゲットに
	DI

	;-------RAMを除外
	ld	hl,#0x8000
	ld	a,(hl)		;P2 8000h がＲＡＭかどうか調べる
	cpl
	ld	(hl),a
	cp	(hl)
	cpl
	ld	(hl),a
	jp	NZ,S_SCC	;ＲＡＭでない＝SCC検索へ

S_MMM:	;------------------------P2=RAMだったのでMMMかどうか調べる

	DI
	LD	A,#0x80
	OUT	(#0x3C),A	;MMM mapped IO enable
	LD	HL,#0x80FE

	LD	A,#0x13
	OUT	(#0xFE),A	;Page2 mapper register write
	LD	B,A
	LD	A,(HL)
	AND	#0b00111111
	XOR	B
	JP	NZ,S_MMM3

	LD	A,#0x2F
	OUT	(#0xFE),A	;Page2 mapper register write
	LD	B,A
	LD	A,(HL)
	AND	#0b00111111
	XOR	B
	JP	NZ,S_MMM3

	LD	A,#0x3D
	OUT	(#0xFE),A	;Page2 mapper register write
	LD	B,A
	LD	A,(HL)
	AND	#0b00111111
	XOR	B
	JP	NZ,S_MMM3

S_MMM2:	LD	A,#0x40
	LD	(#0x803C),A	;DCSG 有効化
	XOR	A
	OUT	(#0x3C),A	;MMM mapped IO disable

	LD      A,#0x9F		;DCSG VOLUME=0に初期化
	OUT     (#0x3F),A
	LD      A,#0xBF
	OUT     (#0x3F),A
	LD      A,#0xDF
	OUT     (#0x3F),A
	LD      A,#0xFF
	OUT     (#0x3F),A

	LD	A,(DCSG_F)
	INC	A
	LD	(DCSG_F),A

	XOR	A		;A=0 DCSG発見
	JP	S_HIT

S_MMM3:	XOR	A
	OUT	(#0x3C),A	;MMM mapped IO disable
	JP	S_END

S_SCC:	;-----------------------SCCがあるか調べる
	XOR	A
	LD	(#0xBFFE),A
	LD	(#0x9000),A
	LD	HL,#0x9800
	LD	A,(HL)		;9800hに書き込めたらSCCなし
	CPL
	LD	(HL),A
	CP	(HL)
	CPL
	LD	(HL),A
	JP	Z,S_OPLL	;書き込めればSCC無し

	LD	A,#0x3F
	LD	(#0x9000),A	;SCCを表に出す
	LD	HL,#0x9800
	LD	A,(HL)		;9800hに書き込めればSCCあり
	CPL
	LD	(HL),A
	CP	(HL)
	CPL
	LD	(HL),A
	JR	NZ,S_OPLL	;書き込めないのでSCC無し
	
	;-------------------SCCかSCC-1か調べる
	LD	A,(SCC_F)
	INC	A
	LD	(SCC_F),A

	LD	A,#0x20
	LD	(#0xBFFE),A
	LD	A,#0x80	
	LD	(#0xB000),A	;SCC-1を固有モードにする
	LD	HL,#0xB800

	LD	A,(HL)		;B800hに書き込めればSCC-1あり
	CPL
	LD	(HL),A
	CP	(HL)
	CPL
	LD	(HL),A
	LD	A,#1		;A=1 SCC-1発見
	JP	Z,S_HIT		;書き込めればSCC-1

	INC	A		;A=2 SCC発見
	JP	S_HIT

S_OPLL:	;------------------------ OPLLか調べる
	LD	HL,#0x4018
	EXX
	LD	HL,#STR_APRL
	LD	B,#8

S_OPLL_IN:
	POP	AF		;スロット番号を復帰
	PUSH	AF

	EXX
	CALL	RDSLT		;SLOT A の4018H-を読む
	EXX

	CP	(HL)
	JR	NZ,S_OPLL_EX
	INC	HL
	EXX
	INC	HL
	EXX
	DJNZ	S_OPLL_IN

	LD	A,(OPLL_F)
	OR	#1
	LD	(OPLL_F),A
	LD	A,#3		;A=3 OPLL(INT)発見
	JP	S_HIT

S_OPLL_EX:
	LD	HL,#0x0401C
	EXX
	LD	HL,#STR_APRL+4
	LD	B,#4

S_OPLL_EX2:
	POP	AF		;スロット番号を復帰
	PUSH	AF
	
	EXX
	CALL	RDSLT		;SLOT A の401CH-を読む
	EXX

	CP	(HL)
	JR	NZ,S_OPM
	INC	HL
	EXX
	INC	HL
	EXX
	DJNZ	S_OPLL_EX2

	LD	A,(OPLL_F)
	INC	A
	INC	A
	LD	(OPLL_F),A
	LD	A,#4		;A=4 OPLL(EXT)発見
	JP	S_HIT

S_OPM:	;------------------------ OPMか調べる
	LD	HL,#0x0080
	EXX
	LD	HL,#STR_OPM
	LD	B,#6

S_OPM0:
	POP	AF		;スロット番号を復帰
	PUSH	AF
	
	EXX
	CALL	RDSLT		;SLOT A の0080H-を読む
	EXX

	CP	(HL)
	JP	NZ,S_END
	INC	HL
	EXX
	INC	HL
	EXX
	DJNZ	S_OPM0

	LD	A,(OPM_F)
	INC	A
	LD	(OPM_F),A
	LD	A,#5		;A=5 OPM発見
	JP	S_HIT

	;----------戻り処理
S_HIT:	LD	B,A
	POP	AF
	SCF
	EI
	RET

S_END:	POP	AF
	SCF
	CCF
	EI
	RET

;============================================================
;音源の割り当て
;============================================================
;準備 :	A :スロット　(Aは保存される)
;	B=0:DCSG 1:SCC-1 2:SCC 3:OPLL(I) 4:OPLL(E) 5:OPM
;
;戻り：	ワークエリアの各スロット選択レジスタの値
;
;注：	BC,HLは破壊される
;	外付けOPLLの初期化は別途必要
;============================================================

FM_INI:
	PUSH	AF
	LD	A,B
	OR	A
	JP	Z,FM_INI4	;DCSGは初期化済み、何もしない
	DEC	A
	JP	Z,FM_INI2	;SCC-1
	DEC	A
	JP	Z,FM_INI2	;SCC
	DEC	A
	JP	Z,FM_INI4	;OPLL(I)は何もしない
	DEC	A
	JP	Z,FM_INI1	;OPLL(E)

	;========OPM

	LD	A,(OPM_F)
	CP	#3
	JP	NC,FM_INI4	;3個目以上は無視
	DEC	A
	ADD	A,A
	ADD	A,A
	LD	B,#0
	LD	C,A
	LD	HL,#OPM0_S
	ADD	HL,BC		;ワークエリアのアドレス算出

	POP	AF
	PUSH	AF
	CALL	P0_CAL		;スロット選択レジスタ事前計算
	JP	FM_INI3

	;========OPLL(E)
FM_INI1:
	LD	A,(OPLL_F)
	SRL	A		;bit0（内蔵OPLL）を除外
	CP	#3
	JP	NC,#FM_INI4	;3個目以上は無視
	DEC	A
	ADD	A,A
	ADD	A,A
	LD	B,#0
	LD	C,A
	LD	HL,#OPLL0_S
	ADD	HL,BC		;ワークエリアのアドレス算出

	POP	AF
	PUSH	AF
	CALL	P2_CAL		;スロット選択レジスタ事前計算
	JP	FM_INI3

	;========SCC
FM_INI2:
	LD	A,(SCC_F)
	CP	#3
	JP	NC,FM_INI4	;3個目以上は無視
	DEC	A
	ADD	A,A
	ADD	A,A
	LD	B,#0
	LD	C,A
	LD	HL,#SCC0_S
	ADD	HL,BC		;ワークエリアのアドレス算出

	POP	AF
	PUSH	AF
	CALL	P2_CAL		;スロット選択レジスタ事前計算

	;----------ワークエリアに値を書き込む
FM_INI3:
	LD	(HL),A		;音源の存在するスロット番号
	INC	HL
	LD	(HL),B		;P2   ターゲットの拡張スロットレジスタの値
	INC	HL
	LD	(HL),C		;P2   ターゲットの基本スロットレジスタの値
	INC	HL
	LD	(HL),D		;P2+P3ターゲットの基本スロットレジスタの値

	;----------パラメータ表示
	LD	HL,#STR_A
	;HACK:
	;CALL	STRPUT
	POP	AF
	PUSH	AF
	;HACK:
	;CALL	_PUT_A

	LD	HL,#STR_B
	;HACK:
	;CALL	STRPUT
	LD	A,B
	;HACK:
	;CALL	_PUT_A

	LD	HL,#STR_C
	;HACK:
	;CALL	STRPUT
	LD	A,C
	;HACK:
	;CALL	_PUT_A

	LD	HL,#STR_D
	;HACK:
	;CALL	STRPUT
	LD	A,D
	;HACK:
	;CALL	_PUT_A

	LD	HL,#STR_CRLF
	;HACK:
	;CALL	STRPUT

FM_INI4:
	POP	AF
	RET

;============================================================
;スロット選択レジスタの事前計算　PAGE0(OPM)用
;============================================================
;準備 :A :スロット　(Aは保存される)
;
;戻り：	B = P0   ターゲットの拡張スロットレジスタの値
;	C = P0   ターゲットの基本スロットレジスタの値
;	D = P0+P3ターゲットの基本スロットレジスタの値
;注：	裏AF、Eは破壊される
;============================================================


P0_CAL:	PUSH	AF
	LD	B,A
	EX	AF,AF'		;裏Aにスロット番号をコピー
	LD	A,B

	AND	#0b00000011
	LD	B,A
	IN	A,(#0xA8)	;基本スロットレジスタを読む
	LD	E,A		;Eに元の基本スロットレジスタを保存
	AND	#0b11111100
	OR	B
	LD	C,A		;C = P0ターゲットの基本スロットレジスタ

	RRC	B
	RRC	B		;基本スロット番号をP3の位置に
	LD	A,C
	AND	#0b00111111
	OR	B
	LD	D,A	    	;D = P0+P3ターゲットの基本スロットレジスタ
	DI	    		;（以下スタック使用不可）
	OUT	(#0xA8),A	;一旦P0+P3をターゲットの基本スロットにする
	    			;（以下スタック使用禁止）
	EX	AF,AF'		;裏Aからスロット番号を復帰
	SRA	A
	SRA	A
	AND	#0b00000011
	LD	B,A		;拡張スロット番号をP0の位置に
	LD	A,(#0xFFFF)	;拡張スロット情報を読む
	CPL
	AND	#0b11111100
	OR	B
	LD	B,A		;B = P0ターゲットの拡張スロットレジスタ

	LD	A,E
	OUT	(#0xA8),A	;元の基本スロットの状態に戻す
	EI			;（以下スタック使用可）

	POP	AF
	RET


;============================================================
;スロット選択レジスタの事前計算　PAGE2(SCC/OPLL)用
;============================================================
;準備 :A :スロット　(Aは保存される)
;
;戻り：	B = P2   ターゲットの拡張スロットレジスタの値
;	C = P2   ターゲットの基本スロットレジスタの値
;	D = P2+P3ターゲットの基本スロットレジスタの値
;注：	裏AF、Eは破壊される
;============================================================

P2_CAL:	PUSH	AF
	LD	B,A
	EX	AF,AF'		;裏Aにスロット番号をコピー
	LD	A,B

	RLCA
	RLCA
	RLCA
	RLCA
	AND	#0b00110000	;基本スロット番号をP2の位置に
	LD	B,A
	IN	A,(#0xA8)	;基本スロットレジスタを読む
	LD	E,A		;Eに元の基本スロットレジスタを保存
	AND	#0b11001111
	OR	B
	LD	C,A		;C = P2ターゲットの基本スロットレジスタ

	RLC	B
	RLC	B		;基本スロット番号をP3の位置に
	LD	A,C
	AND	#0b00111111
	OR	B
	LD	D,A		;D = P0+P3ターゲットの基本スロットレジスタ
	DI			;（以下スタック使用不可）
	OUT	(#0xA8),A	;一旦P0+P3をターゲットの基本スロットにする

	EX	AF,AF'		;裏Aからスロット番号を復帰
	SLA	A
	SLA	A
	AND	#0b00110000
	LD	B,A		;拡張スロット番号をP2の位置に
	LD	A,(#0xFFFF)	;拡張スロット情報を読む
	CPL
	AND	#0b11001111
	OR	B
	LD	B,A		;B = P0ターゲットの拡張スロットレジスタ

	LD	A,E
	OUT	(#0xA8),A	;元の基本スロットの状態に戻す
	EI			;（以下スタック使用可）

	POP	AF
	RET



;=======================================
;文字列表示サブ
;入力：HL 文字列の先頭アドレス
;出力:'$'が来るまで文字表示
;
;注）A,HLは破壊される
;=======================================

STRPUT:	LD	A,(HL)
	CP	#'$'
	RET	Z
	CALL	CHPUT
	INC	HL
	JR	STRPUT

;=================================================
;Aの内容１バイトを16進数文字で表示するサブ
;入力　	A 	Aは保存される
;=================================================
_PUT_A:	PUSH	AF
	PUSH	DE
	CALL	PutNum
	LD	A,D
	CALL	CHPUT
	LD	A,E
 	CALL	CHPUT
	POP	DE
	POP	AF
	RET

;============================================
;16進数をアスキーコードに変換するサブ
;入力　A：数値　　出力D:上位　E：下位コード
;	Aは破壊される
;============================================
PutNum:	PUSH	AF
	SRL	A		;上位4ビットを取り出す
	SRL	A
	SRL	A
	SRL	A
	ADD	A,#48
	CP	#58		;0-9の数字か
	JR	C,PutH01
	ADD	A,#7		;A-Fまで
PutH01:	LD	D,A

	POP	AF
	AND	#15		;下位4ビットを取り出す
	ADD	A,#48
	CP	#58
	JR	C,PutL01
	ADD	A,#7
PutL01:	LD	E,A
	RET


;====================================================================
;（参考）P0/P2のBIOSコールを使わないスロット切り替えルーチン
;====================================================================
;=======================================
;Page0 切り替えサブ
;入力：A Page0に割り当てるスロット
;
;注）以後BIOSコールと割り込みは使用不可
;	BCと裏AFは破壊される
;	DIしてからコールする
;=======================================

P0_CHG:	PUSH	AF

	AND	#0b00000011
	LD	B,A
	IN	A,(#0xA8)	;基本スロット情報を読む
	AND	#0b11111100
	OR	B
	OUT	(#0xA8),A	;基本スロットP0をターゲットにする
	LD	C,A		;P3をRAMに戻す情報をCに入れる

	POP	AF
	BIT	7,A		;拡張スロット指定か？
	RET	Z

	;==============P3切り替え
	PUSH	AF
	EX	AF,AF'		;スロット番号を裏Aに退避
	RRC	B
	RRC	B		;基本スロット番号をP3の位置に
	
	LD	A,C
	AND	#0b00111111
	OR	B		;（以下スタック使用不可）
	OUT	(#0xA8),A	;P3をターゲットの基本スロットにする

	EX	AF,AF'		;裏Aからスロット番号を復帰
	RRCA
	RRCA
	AND	#0b00000011
	LD	B,A		;拡張スロット番号をP0の位置に
	LD	A,(#0xFFFF)	;拡張スロット情報を読む
	CPL
	AND	#0b11111100
	OR	B
	LD	(#0xFFFF),A	;拡張スロットをP0ターゲットにする

	LD	A,C
	OUT	(#0xA8),A	;P3をRAMに戻す

	;=============P3 切り替え完了（以下スタック使用可）

	POP	AF
	RET

;=======================================
;Page2 切り替えサブ
;入力：A Page2に割り当てるスロット
;
;注）	BCと裏AFは破壊される
;	DIしてからコールする
;=======================================

P2_CHG:	PUSH	AF
	RLCA
	RLCA
	RLCA
	RLCA
	AND	#0b00110000
	LD	B,A
	IN	A,(#0xA8)	;基本スロット情報を読む
	AND	#0b11001111
	OR	B
	OUT	(#0xA8),A	;基本スロットP2をターゲットにする
	LD	C,A		;P3をRAMに戻す情報をCに入れる

	POP	AF
	BIT	7,A		;拡張スロット指定か？
	RET	Z

	;==============P3切り替え
	PUSH	AF
	EX	AF,AF'		;スロット番号を裏Aに退避
	RLC	B
	RLC	B		;基本スロット番号をP3の位置に
	
	LD	A,C
	AND	#0b00111111
	OR	B		;（以下スタック使用不可）
	OUT	(#0xA8),A	;P3をターゲットの基本スロットにする

	EX	AF,AF'		;裏Aからスロット番号を復帰
	RLCA
	RLCA
	AND	#0b00110000
	LD	B,A		;拡張スロット番号をP2の位置に
	LD	A,(#0xFFFF)	;拡張スロット情報を読む
	CPL
	AND	#0b11001111
	OR	B
	LD	(#0xFFFF),A	;拡張スロットをP2ターゲットにする

	LD	A,C
	OUT	(#0xA8),A	;P3をRAMに戻す

	;=============P3 切り替え完了（以下スタック使用可）

	POP	AF
	RET


;====================================================================
;（参考）各音源アクセス時のスロット切り替えプログラム
;====================================================================

;===========SCC0のスロットのPAGE2を表に出す(要DIで実行)
SCC0_P2:
	LD	BC,(SCC0_S+1)
	LD	A,(SCC0_S+3)
	OUT	(#0xA8),A	;P2+P3をSCC0の基本スロットに切り替え
	LD	A,C
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,B
	OUT	(#0xA8),A	;P3をRAMに戻す

;===========SCC1のスロットのPAGE2を表に出す(要DIで実行)
SCC1_P2:
	LD	BC,(SCC1_S+1)
	LD	A,(SCC1_S+3)
	OUT	(#0xA8),A	;P2+P3をSCC1の基本スロットに切り替え
	LD	A,C
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,B
	OUT	(#0xA8),A	;P3をRAMに戻す

;===========OPLL0のスロットのPAGE2を表に出す(要DIで実行)
OPLL0_P2:
	LD	BC,(OPLL0_S+1)
	LD	A,(OPLL0_S+3)
	OUT	(#0xA8),A	;P2+P3をOPLL0の基本スロットに切り替え
	LD	A,C
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,B
	OUT	(#0xA8),A	;P3をRAMに戻す

;===========OPLL1のスロットのPAGE2を表に出す(要DIで実行)
OPLL1_P2:
	LD	BC,(OPLL1_S+1)
	LD	A,(OPLL1_S+3)
	OUT	(#0xA8),A	;P2+P3をOPLL1の基本スロットに切り替え
	LD	A,C
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,B
	OUT	(#0xA8),A	;P3をRAMに戻す

;===========OPM0のスロットのPAGE0を表に出す(要DIで実行)
OPM0_P0:
	LD	BC,(OPM0_S+1)
	LD	A,(OPM0_S+3)
	OUT	(#0xA8),A	;P0+P3をOPM0の基本スロットに切り替え
	LD	A,C
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,B
	OUT	(#0xA8),A	;P3をRAMに戻す

;===========OPM1のスロットのPAGE0を表に出す(要DIで実行)
OPM1_P0:
	LD	BC,(OPM1_S+1)
	LD	A,(OPM1_S+3)
	OUT	(#0xA8),A	;P0+P3をOPM1の基本スロットに切り替え
	LD	A,C
	LD	(#0xFFFF),A	;拡張スロット切り替え
	LD	A,B
	OUT	(#0xA8),A	;P3をRAMに戻す

;=================================
;文字列データ
;=================================
STR_VER:	.ascii	"SoundCatridge detctor ver.0.01\r\n"
		    .ascii	"Copyright (C)2022 Niga\r\n\r\n$"
STR_APRL:	.ascii	"APRLOPLL"
STR_OPM:	.ascii	"MCHFM0"
STR_NAME:	.ascii	"DCSG(MMM)$"
	    	.ascii	"SCC-1    $"
	    	.ascii	"SCC      $"
	    	.ascii	"OPLL(INT)$"
	    	.ascii	"OPLL(EXT)$"
	    	.ascii	"OPM (SFG)$"
STR_DTCT:	.ascii	" detected on SLOT #$"
STR_A:		.ascii	" A:$"
STR_B:		.ascii	" B:$"
STR_C:		.ascii	" C:$"
STR_D:		.ascii	" D:$"
STR_CRLF:	.ascii	"\r\n\r\n$"
STR_COMP:	.ascii	"\r\nOPLL initialize completed.$"

