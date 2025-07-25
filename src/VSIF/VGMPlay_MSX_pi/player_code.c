#include "main.h"

void uart_processVgm();
void SARCHFM();

#define CLS   #0x00C3
#define PSGAD #0xA0
#define PSGWR #0xA1
#define PSGRD #0xA2

/*
port 0x40に0x69を書くと有効、0x69以外を書くと無効になります。
起動・リセット時の初期状態は無効状態です。

無効状態時
port 0x40(write)	0x69を書き込むと有効、0x69以外の書込みは無視(無効のまま)

有効状態時
port 0x40(write)	0x69以外の書込みで無効、0x69の書込みは無視(有効のまま)
port 0x40(read)		0x96(0x69のビット反転値)を返す。これによりボードの有無を判定できます。
port 0x41(write)	i8151設定(無視します・USB接続なので設定は無意味)
port 0x41(read)		i8151状態を返す
port 0x42(write)	i8151書込み
port 0x42(read)		i8151読み出し

i8151状態
D7      D6      D5      D4      D3      D2      D1      D0
1固定   0固定   0固定   0固定   0固定   TXEMPTY RXRDY   TXRDY

TXEMPTY送信バッファが空で1
RXRDY受信データがあると1
TXRDY送信可能(送信バッファに空きがある)で1

送信(受信)は必ずTXRDY(RXRDY)が1である事を確認してから行なう事
*/

// 8ビットI/Oポートを宣言する例
// ポートアドレス 0x1F にアクセスするIO8という名前のポートを定義
__sfr __at 0x40 MSX_Pi_Port40; // MSX Piのポート40

void processPlayer() {
    int value;

    MSX_Pi_Port40 = 0x69; // ポート0x40に0x69を書き込むことで有効化
    value = MSX_Pi_Port40; // ポート0x40から値を読み取る
    if(value != 0x96)
    {
        print("MSX Pi not found!\r\n");
        while(1);
    }
    
    print("MAMI VGM SOUND DRIVER BY ITOKEN\r\n");
    print("\r\n");
    print("*PUSH PANIC BTN WHEN GET WEIRD\r\n");
    print("*CONNECT MSX Pi TO Windows\r\n");

    SARCHFM();
    //uart_processVgm();
}
