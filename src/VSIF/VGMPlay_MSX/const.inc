PSGAD = #0xA0
PSGWR = #0xA1
PSGRD = #0xA2

OPLLAD = #0x7C
OPLLWR = #0x7D

OPL3AD1 = #0xC0
OPL3WR1  = #0xC1
OPL3AD2 = #0xC2
OPL3WR2  = #0xC3

DCSGAD = #0x3F

OPN2AD1 = #0x14 ;:OPN2 ch1-3 address(W) / status(R)
OPN2WR1 = #0x15 ;:OPN2 ch1-3 data(W)
OPN2AD2 = #0x16 ;:OPN2 ch4-6 address(W)
OPN2WR2 = #0x17 ;:OPN2 ch4-6 data(W)

OPNAD = #0x12 ;:OPN ch1-3 address(W) / status(R)
OPNWR = #0x13 ;:OPN ch1-3 data(W)

TRDAC = #0xA4

WRSLT = #0x14
RDSLT = #0x0c
ENASLT = #0x24

CHPUT = #0xA2

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

ROM2_S	= #0xE018

ROM1_S	= #0xE01C

DCSG_F	= #0xE020		;発見された音源の数
OPLL_F	= #0xE021		;OPLLは bit7:1 = EXT / bit0 = INT
SCC_F	= #0xE022
OPM_F	= #0xE023

;==========以下v0.05差分
AUDIO_F	= #0xE024
;==========以上v0.05差分

; LD  A, H
; ADD A, #0x30
; CALL CHPUT

; LD  A, B
; SRL A
; SRL A
; SRL A
; SRL A
; ADD A, #0x30
; CALL CHPUT
; LD  A, B
; AND #0xf
; ADD A, #0x30
; CALL CHPUT

; LD  A, D
; SRL A
; SRL A
; SRL A
; SRL A
; ADD A, #0x30
; CALL CHPUT
; LD  A, D
; AND #0xf
; ADD A, #0x30
; CALL CHPUT

; LD A, #0x20
; CALL CHPUT

; JP  __VGM_LOOP
