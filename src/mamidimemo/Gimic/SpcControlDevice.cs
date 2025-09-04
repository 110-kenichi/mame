using System;
using System.Text;
using System.Threading;

/// <summary>
/// Controls the Sound Production Chip (SPC700) on a GIMIC device.
/// This class handles low-level communication for initializing the hardware,
/// uploading programs, and writing to DSP registers and RAM.
/// </summary>
public class SpcControlDevice : IDisposable
{
    #region Constants

    // DSP Registers
    private const int DSP_MVOLL = 0x0c;
    private const int DSP_MVOLR = 0x1c;
    private const int DSP_EVOLL = 0x2c;
    private const int DSP_EVOLR = 0x3c;
    private const int DSP_KON = 0x4c;
    private const int DSP_KOF = 0x5c;
    private const int DSP_FLG = 0x6c;
    private const int DSP_ESA = 0x6d;
    private const int DSP_EDL = 0x7d;

    // Communication Constants
    private const int DSP_ACC_CODE_ADDR = 0x0010;
    private const int P3_WAIT_VALUE = 0xee;

    #endregion

    #region DSP Access Code

    //
    //  SpcControlDevice.cpp
    //  gimicUsbSpcPlay
    //
    //  Created by osoumen on 2014/06/14.
    //  Copyright (c) 2014年 osoumen. All rights reserved.
    //
    /// <summary>
    /// A small program uploaded to the SPC700 to enable direct access to DSP registers.
    /// </summary>
    private static readonly byte[] DspRegAccCode =
    {
        0x8F, 0x00, 0xF1, // mov SPC_CONTROL,#$00
        0x8F, 0x6C, 0xF2, // mov SPC_REGADDR,#DSP_FLG
        0x8F, 0x00, 0xF3, // mov SPC_REGDATA,#$00
        0x8D, 0x00,       // mov y,#0
        0xE8, 0x00,       // mov a,#0
        0x8F, 0x00, 0x04, // mov $04,#$00
        0x8F, 0x06, 0x05, // mov $05,#$06
        // initloop:
        0xD7, 0x04,       // mov [$04]+y,a ; 7
        0x3A, 0x04,       // incw $04 ; 6
        0x78, 0x7E, 0x05, // cmp $05,#$7e ; 5
        0xD0, 0xF7,       // bne initloop ; 4
        0xE4, 0xF4,       // mov a,SPC_PORT0
        // ack:
        0x8F, 0xEE, 0xF7, // mov SPC_PORT3,#$ee
        // loop:
        0x64, 0xF4,       // cmp a,SPC_PORT0 ; 3
        0xF0, 0xFC,       // beq loop ; 2
        0xE4, 0xF4,       // mov a,SPC_PORT0 ; 3
        0x30, 0x26,       // bmi toram ; 2
        0xF8, 0xF6,       // mov x,SPC_PORT2 ; 3
        0xD8, 0xF2,       // mov SPC_REGADDR,x ; 4
        0xFA, 0xF5, 0xF3, // mov SPC_REGDATA,SPC_PORT1
        0xC4, 0xF4,       // mov SPC_PORT0,a ; 4
        // ; wait 64 - 32 cycle
        0xC8, 0x4C,       // cmp x,#DSP_KON ; 3
        0xF0, 0x12,       // beq konWait ; 4
        0xC8, 0x5C,       // cmp x,#DSP_KOF ; 3
        0xD0, 0xE7,       // bne loop ; 4
        // koffWait:
        0x8D, 0x0A,       // mov y,#10 ; 2
        // -
        0xFE, 0xFE,       // dbnz y,- ; 4/6
        0x8F, 0x00, 0xF3, // mov SPC_REGDATA,#0 ;5
        //
        0x8D, 0x05,       // mov y,#5 ; 2
        // -
        0xFE, 0xFE,       // dbnz y,- ; 4/6
        0x00,             // nop ; 2
        0x2F, 0xD9,       // bra loop ; 4
        // konWait:
        0x8D, 0x05,       // mov y,#5 ; 2
        // -
        0xFE, 0xFE,       // dbnz y,- ; 4/6
        0x00,             // nop ; 2
        0x2F, 0xD2,       // bra loop ; 4
        // toram:
        0x5D,             // mov x,a
        //
        0x80,             // setc
        0xA8, 0x40,       // sbc a,#P0FLG_BLKTRAS
        0x30, 0x0F,       // bmi blockTrans
        0x28, 0x20,       // and a,#P0FLG_P0RST
        0xD0, 0x39,       // bne resetP0
        //
        0x8D, 0x00,       // mov y,#0
        0xE4, 0xF5,       // mov a,SPC_PORT1
        0xD7, 0xF6,       // mov [SPC_PORT2]+y,a
        0x7D,             // mov a,x
        0xC4, 0xF4,       // mov SPC_PORT0,a
        0x2F, 0xBD,       // bra loop
        // blockTrans:
        0x8F, 0x00, 0xF7, // mov SPC_PORT3,#$0
        0xFA, 0xF6, 0x04, // mov $04,SPC_PORT2
        0xFA, 0xF7, 0x05, // mov $05,SPC_PORT3
        0x7D,             // mov a,x
        0x8D, 0x00,       // mov y,#0
        0xC4, 0xF4,       // mov SPC_PORT0,a
        // loop2:
        0x64, 0xF4,       // cmp a,SPC_PORT0
        0xF0, 0xFC,       // beq loop2
        0xE4, 0xF4,       // mov a,SPC_PORT0
        0x30, 0xA4,       // bmi ack
        0x5D,             // mov x,a
        0xE4, 0xF5,       // mov a,SPC_PORT1
        0xD7, 0x04,       // mov [$04]+y,a
        0x3A, 0x04,       // incw $04
        0xE4, 0xF6,       // mov a,SPC_PORT2
        0xD7, 0x04,       // mov [$04]+y,a
        0x3A, 0x04,       // incw $04
        0xE4, 0xF7,       // mov a,SPC_PORT3
        0xD7, 0x04,       // mov [$04]+y,a
        0x3A, 0x04,       // incw $04
        0x7D,             // mov a,x
        0xC4, 0xF4,       // mov SPC_PORT0,a
        0x2F, 0xE0,       // bra loop2
        // resetP0:
        0x8F, 0xB0, 0xF1, // mov SPC_CONTROL,#$b0
        0x20,             // clrp
        0xD8, 0xF4,       // mov SPC_PORT0,x
        0x5F, 0xC0, 0xFF  // jmp !$ffc0
    };

    /*
     * このSPC700（スーパーファミコンのサウンドチップ）のアセンブリコードについて、セクションごとにその役割と動作を解説します。

このコードは全体として、**ホストコンピュータ（PC）からSPC700のDSP（デジタルシグナルプロセッサ）やRAMを直接制御するための小さなプログラム（ファームウェア）**です。USB経由でSPC700のRAMに転送され、実行されることを前提としています。

## 1. 初期化処理
コードの最初の部分は、SPC700とDSPを制御可能な状態にするための初期化を行います。

コード スニペット

; SPC700のコントロールレジスタを初期化
mov SPC_CONTROL, #$00

; DSPの全レジスタを0でクリアする準備
mov SPC_REGADDR, #DSP_FLG ; 最初の書き込み先をDSPレジスタ$6Cに設定
mov SPC_REGDATA, #$00   ; 書き込むデータを0にする

; ループカウンタやポインタの準備
mov y, #0
mov a, #0
mov $04, #$00   ; メモリアドレス$04,$05をポインタとして使用
mov $05, #$06

initloop:
    ; Aレジスタ(値は0)を、[$04]が指すアドレスに書き込む
    ; [$04]+y は、アドレス$04,$05が示すアドレスにYレジスタの値を足した場所を指す
    mov [$04]+y, a
    incw $04          ; ポインタ($04,$05)をインクリメント
    cmp $05, #$7e     ; ポインタが$7E00に達したかチェック
    bne initloop      ; 達していなければループを続ける
目的: DSPのサウンドレジスタ（$00〜$7D）をすべてゼロでクリアし、ノイズの発生を防ぎ、予測可能な状態にします。

動作:

まず、DSPレジスタへの書き込み準備をします。

initloop というループに入り、Aレジスタ（中身は0）の値をRAMの特定領域に書き込んでいきます。incw $04 で書き込み先アドレスを1ずつ増やし、これを繰り返すことでメモリをゼロクリアしています。

cmp $05, #$7e は、このゼロクリア処理がDSPレジスタの範囲全体をカバーしたかをチェックしています。

## 2. メインの通信ループとコマンド待機
初期化が終わると、ホストからのコマンドを待つためのメインループに入ります。

コード スニペット

; ホストから送られてくるコマンドをAレジスタに読み込む
mov a, SPC_PORT0

ack:
    ; ホストに「準備完了」を知らせるため、ポート3に$EEを書き込む
    mov SPC_PORT3, #$ee

loop:
    ; ホストがPORT0に新しい値を書き込むまで待つ
    cmp a, SPC_PORT0
    beq loop            ; 変化がなければ、その場でループし続ける

    ; PORT0に変化があったら、その新しい値をAレジスタに読み込む
    mov a, SPC_PORT0
目的: ホストとの間で通信のタイミングを合わせる「ハンドシェイク」を行い、新しいコマンドを受け取る準備をします。

ホストとの通信: SPC700には外部（この場合はホストPC）とデータをやり取りするための4つのポート（SPC_PORT0〜SPC_PORT3）があります。

SPC_PORT0: 主にコマンドやステータスの送受信に使われます。

SPC_PORT1: データ（書き込む値など）の送受信に使われます。

SPC_PORT2, SPC_PORT3: アドレスの送受信に使われます。

動作:

ack ラベルで、まずポート3に 0xEE という値を書き込みます。これはホストに対して「コマンドを受け付ける準備ができました」と知らせる合図です。

loop ラベルでは、SPC_PORT0 の値が前回読み込んだ値（Aレジスタに保持）から変化するまで、無限に待ち続けます。

ホストが新しいコマンドを SPC_PORT0 に書き込むと値が変化し、ループを抜けて次の処理に進みます。

## 3. コマンド分岐とDSP/RAM書き込み処理
メインループで新しいコマンドを受け取ると、その内容に応じて処理を分岐させます。

コード スニペット

    ; Aレジスタの値を見て処理を分岐
    ; 最上位ビットが1(負数)ならRAMアクセス系の処理(toram)へ
    bmi toram

    ; --- DSPレジスタ書き込み処理 ---
    mov x, SPC_PORT2          ; ポート2からDSPアドレスをXレジスタへ
    mov SPC_REGADDR, x        ; Xレジスタの値をDSPアドレスレジスタへ設定
    mov SPC_REGDATA, SPC_PORT1; ポート1のデータをDSPデータレジスタへ書き込み
    mov SPC_PORT0, a          ; ホストに応答を返す（ハンドシェイク）
目的: ホストから送られてきたコマンドが「DSPレジスタへの書き込み」なのか「RAMへの書き込み」なのかを判断し、適切な処理を実行します。

動作:

bmi toram (Branch if Minus) 命令が鍵です。ホストから送られてくるコマンド (SPC_PORT0 の値) の最上位ビットが1であれば、RAM関連の処理を行う toram ラベルにジャンプします。

最上位ビットが0であれば、DSPレジスタへの書き込み処理を実行します。

SPC_PORT2 から書き込みたいDSPレジスタのアドレスを読み込みます。

SPC_PORT1 から書き込みたいデータを読み込み、指定されたDSPレジスタに書き込みます。

最後に mov SPC_PORT0, a でホストに処理完了を伝え、再び loop の先頭に戻って次のコマンドを待ちます。

## 4. 特殊なDSPコマンドの処理 (ウェイト)
DSPレジスタの中でも、KON（キーオン）や KOF（キーオフ）は特別な処理が必要です。

コード スニペット

    ; 書き込んだDSPアドレスがKONかKOFかチェック
    cmp x, #DSP_KON     ; キーオン($4C)か？
    beq konWait         ; そうならkonWaitへ
    cmp x, #DSP_KOF     ; キーオフ($5C)か？
    bne loop            ; どちらでもなければメインループへ

koffWait:
    ; KOFの後のウェイト処理
    mov y, #10
-   dbnz y, -          ; Yが0になるまでループ (ウェイト)
    mov SPC_REGDATA, #0; 安全のためデータレジスタを0にする
    mov y, #5
-   dbnz y, -
    nop
    bra loop            ; メインループへ

konWait:
    ; KONの後のウェイト処理
    mov y, #5
-   dbnz y, -
    nop
    bra loop            ; メインループへ
目的: KON や KOF コマンドはDSP内部で少し時間がかかるため、コマンドが確実に実行されるように短い時間待機（ウェイト）します。これにより、連続してコマンドを送っても処理落ちせず、音が正しく鳴ります。

動作: dbnz (Decrement and Branch if Not Zero) 命令を使って、指定した回数だけ空ループを回し、意図的に時間を稼いでいます。

## 5. RAMアクセス処理とブロック転送 (toram)
SPC_PORT0 の最上位ビットが1だった場合にジャンプしてくるのが toram セクションです。ここではさらにコマンドを細かく解析します。

コード スニペット

toram:
    mov x, a

    ; コマンドをさらに解析
    setc
    sbc a, #P0FLG_BLKTRAS ; ブロック転送フラグ($40)を引いてみる
    bmi blockTrans        ; 結果が負ならブロック転送へ
    and a, #P0FLG_P0RST   ; リセットフラグ($20)をチェック
    bne resetP0           ; 0でなければリセット処理へ

    ; --- 単一バイトRAM書き込み ---
    mov y, #0
    mov a, SPC_PORT1          ; ポート1からデータを取得
    mov [SPC_PORT2]+y, a      ; ポート2が示すアドレスにデータを書き込み
    mov a, x
    mov SPC_PORT0, a          ; ホストに応答
    bra loop                  ; メインループへ

blockTrans:
    ; --- ブロック転送処理 ---
    mov SPC_PORT3, #$0
    mov $04, SPC_PORT2        ; ポート2,3から転送先ベースアドレスを取得
    mov $05, SPC_PORT3
    ; ... (loop2でホストから連続的にデータを受け取りRAMに書き込む)

resetP0:
    ; --- リセット処理 ---
    mov SPC_CONTROL, #$b0
    clrp
    mov SPC_PORT0, x
    jmp !$ffc0                ; IPL-ROMの開始アドレスにジャンプしてリセット
目的: RAMに対する3種類の操作（単一バイト書き込み、ブロック転送、リセット）を切り替えます。

動作:

単一バイト書き込み: ホストから指定されたアドレス（SPC_PORT2）に、指定されたデータ（SPC_PORT1）を1バイトだけ書き込みます。

ブロック転送 (blockTrans): 大量のデータを効率的に転送するためのモードです。ホストは立て続けにデータを送信し、SPC700側は loop2 という専用ループでそれらを受け取ってRAMに次々と書き込んでいきます。曲データやサンプルデータの転送に使われます。

リセット (resetP0): jmp !$ffc0 でSPC700のIPL-ROM（起動プログラム）の開始アドレスにジャンプします。これにより、SPC700はCPUリセットと同様の状態になります。

このコードは、限られたリソースの中で、非常に効率的かつ柔軟な通信プロトコルを実現しています。






ブロック転送の処理を詳しく説明



思考プロセスを表示
もちろんです。SPC700コードの「ブロック転送」部分の処理について、その目的と動作を詳しく解説します。

## ブロック転送の目的 🚚
ブロック転送は、曲データや波形データ（サウンドサンプル）のように、まとまった量のデータをホストPCからSPC700のRAMへ高速に転送するための専用モードです。

1バイトずつアドレスとデータを指定して転送する「単一バイト書き込み」に比べ、最初に転送先のアドレスを指定するだけで、あとはホストがデータを連続的に送り、SPC700がそれを次々とRAMに書き込んでいくため、通信のオーバーヘッドが劇的に減り、非常に効率的です。

## 処理の流れ
ブロック転送の処理は、大きく分けて「① セットアップ」と「② データ転送ループ」の2段階で構成されます。

① セットアップ (blockTransラベル直後)
toramラベルからbmi blockTrans命令によって、この処理にジャンプしてきます。まず、データ転送を開始するための準備を行います。

コード スニペット

blockTrans:
    ; 1. ホストへの状態通知
    mov SPC_PORT3, #$0

    ; 2. 転送先アドレスの取得と設定
    mov $04, SPC_PORT2
    mov $05, SPC_PORT3

    ; 3. ループ準備とホストへの応答 (ハンドシェイク)
    mov a, x              ; (XレジスタにはPORT0の元のコマンド値が保存されている)
    mov y, #0
    mov SPC_PORT0, a      ; ホストに「セットアップ完了、データ送信OK」と応答
1. ホストへの状態通知: mov SPC_PORT3, #$0 を実行し、ポートの状態をクリアします。これはホスト側に「ブロック転送モードに入った」ことを伝えるための信号として機能します。

2. 転送先アドレスの取得: ホストは事前に、データの転送先となるRAMのアドレスを SPC_PORT2 (アドレス下位8bit) と SPC_PORT3 (アドレス上位8bit) にセットしています。このコードは、その16bitアドレスをSPC700のゼロページメモリ $04 と $05 にコピーします。これにより、$04と$05は、RAMへの書き込み位置を示すポインタとして機能します。

3. ホストへの応答: 最後に mov SPC_PORT0, a を実行します。これはホストに対して「アドレス設定が完了し、データを受け取る準備ができました。最初のデータを送ってください」と伝える重要なハンドシェイク（応答確認）です。

② データ転送ループ (loop2ラベル)
セットアップが完了すると、いよいよ実際のデータ転送ループに入ります。このループは、ホストから転送終了の合図が来るまで回り続けます。

コード スニペット

loop2:
    ; 4. ホストからのデータ送信を待つ
    cmp a, SPC_PORT0
    beq loop2

    ; 5. 新しいデータパケットを受信
    mov a, SPC_PORT0
    bmi ack               ; もしホストから終了信号が来たら、ループを抜けてackへ

    ; 6. 3バイトのデータをRAMに書き込む
    mov x, a              ; ハンドシェイク用にPORT0の値をXに保存
    mov a, SPC_PORT1      ; 1バイト目を取得
    mov [$04]+y, a        ; ポインタ($04)が指すアドレスに書き込み
    incw $04              ; ポインタを1つ進める

    mov a, SPC_PORT2      ; 2バイト目を取得
    mov [$04]+y, a        ; ポインタが指すアドレスに書き込み
    incw $04              ; ポインタを1つ進める

    mov a, SPC_PORT3      ; 3バイト目を取得
    mov [$04]+y, a        ; ポインタが指すアドレスに書き込み
    incw $04              ; ポインタを1つ進める

    ; 7. ホストへ「受信完了」を応答
    mov a, x
    mov SPC_PORT0, a
    bra loop2             ; 次のデータパケットを待つためループの先頭へ
4. データ送信を待つ: beq loop2 で、ホストが SPC_PORT0 の値を更新する（＝新しいデータパケットを送ってくる）まで待機します。

5. データパケット受信と終了判定: ホストから新しいパケットが来たら、まず SPC_PORT0 の値をチェックします。もしこの値の最上位ビットが1（負数）であれば、それはホストからの「転送終了」の合図です。bmi ack 命令でループを抜け、メインのコマンド待機状態 (ack) に戻ります。

6. 3バイトのデータ書き込み: このプロトコルの特徴は、1回のループで3バイトのデータを転送する点です。ホストは SPC_PORT1, SPC_PORT2, SPC_PORT3 の3つのポートに同時にデータをセットします。SPC700側はそれを順番に読み取り、ポインタ ($04) が指すRAMのアドレスへ書き込みます。1バイト書き込むごとに incw $04 でポインタをインクリメントするため、データは指定されたアドレスから連番で正しく書き込まれていきます。

7. ホストへの応答: 3バイトの書き込みが終わると、mov SPC_PORT0, a でホストに応答を返します。これは「今送られた3バイトは正常に受信・書き込みしました。次のデータをください」というハンドシェイク信号です。

この loop2 のハンドシェイクと3バイト転送を繰り返すことで、大量のデータを極めて高速にRAMへ転送することができるのです。


3バイト未満のデータ転送は？



思考プロセスを表示
3バイト未満の余ったデータは、高速なブロック転送モードではなく、「単一バイト書き込み」モードを使って1バイトずつ個別に転送されます。

これは、効率性を最大限に高めるための仕組みです。

## 詳しい処理の流れ 📜
ホストコンピュータ側の WriteRam 関数は、データを転送する際に以下の手順を踏みます。

データの分割
まず、転送したいデータ全体を「3バイト単位のまとまり」と「3バイト未満の余り（0, 1, または2バイト）」に分割します。

ブロック転送の実行
次に、「3バイト単位のまとまり」の部分を、先ほど解説した非常に高速なブロック転送（loop2 を使う方法）で一気に転送します。

モードの切り替えと個別転送
ブロック転送が完了すると、ホストはSPC700に転送モードの終了を伝えます。その後、残った1バイトまたは2バイトの「余り」のデータを、アドレスとデータを毎回ペアにして、1バイトずつ個別に転送します。この個別転送には、ブロック転送とは異なるコマンドが使われ、SPC700側はtoramセクション内の「単一バイト書き込み」処理を実行します。

## 例：8バイトのデータを転送する場合
8バイトを (3バイト + 3バイト) + 2バイト のように分割します。

最初の6バイト（3バイト×2）は、高速なブロック転送で一気に送ります。

残った2バイトは、単一バイト書き込みモードで、1バイトずつアドレスを指定しながら2回に分けて送ります。

このように、大量のデータはトラック（ブロック転送）で効率よく運び、最後に残った小さな荷物（余りのデータ）は手で一つずつ運ぶ（単一バイト転送）ようなイメージです。これにより、常に最適な速度でデータ転送が行われます。
     */

    #endregion

    private readonly ISpc700 _spc700;
    private byte _port0StateHw;
    private bool _port0StateHwFirst;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spc700"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SpcControlDevice(ISpc700 spc700)
    {
        _spc700 = spc700 ?? throw new ArgumentNullException(nameof(spc700));
    }

    /// <summary>
    /// Initializes the SPC device, performing hardware and software resets,
    /// uploading the control program, and preparing for communication.
    /// </summary>
    /// <returns>0 on success, -1 on failure.</returns>
    public int Init()
    {
        // Hardware and software reset
        HwReset();
        //SwReset();

        // Wait for the device to signal readiness ($BBAA)
        if (WaitReady() != 0)
            return -1;

        // Initializing the Transfer
        PortWrite(1, 1);
        PortWrite(2, 0);
        PortWrite(3, 0);
        int ret = PushAndWait(0, 0xcc);
        if (ret != 0xcc)
            return -1;

        // Mute all volumes to avoid noise during setup
        _port0StateHwFirst = true;
        _port0StateHw = 0x0;
        if (!TransferIplData(0xf2, new byte[] { DSP_MVOLL, 0 }))
            return -1;
        if (!TransferIplData(0xf2, new byte[] { DSP_MVOLR, 0 }))
            return -1;
        if (!TransferIplData(0xf2, new byte[] { DSP_EVOLL, 0 }))
            return -1;
        if (!TransferIplData(0xf2, new byte[] { DSP_EVOLR, 0 }))
            return -1;

        // Initialize flags
        if (!TransferIplData(0xf2, new byte[] { DSP_FLG, 0x20 }))
            return -1;
        if (!TransferIplData(0xf2, new byte[] { DSP_EDL, 0x00 }))
            return -1;
        if (!TransferIplData(0xf2, new byte[] { DSP_ESA, 0x06 }))
            return -1;

        WaitMicroSeconds(240000); // Wait 240ms after changing EDL/ESA

        // Upload the DSP access program to the device's RAM
        if (!TransferIplData(DSP_ACC_CODE_ADDR, DspRegAccCode))
            return -1;

        // Jump to the uploaded program's address
        PortWrite(1, 0);
        PortWrite(2, (byte)(DSP_ACC_CODE_ADDR & 0xFF));
        PortWrite(3, (byte)(DSP_ACC_CODE_ADDR >> 8));
        _port0StateHw += 2;
        PortWrite(0, _port0StateHw); //APUユーザープログラム実行モード

        // Wait for the acknowledgement value on Port 3
        ret = ReadAndWait(3, P3_WAIT_VALUE, 10000);
        if (ret < 0)
            return -1;

        _port0StateHw = 1;
        return 0;
    }

    /// <summary>
    /// Performs a hardware reset on the device.
    /// </summary>
    public void HwReset()
    {
        _spc700.Reset();
    }

    /// <summary>
    /// Writes a single byte to a specified SPC port(0-3).
    /// </summary>
    public void PortWrite(byte portAddr, byte data)
    {
        _spc700.Write(portAddr, data);
    }

    /// <summary>
    /// Reads a single byte from a specified SPC port(0-3).
    /// </summary>
    public byte PortRead(byte portAddr)
    {
        return _spc700.Read(portAddr);
    }

    //APUへデータ転送
    public bool TransferIplData(int addr, byte[] data)
    {
        //転送の開始
        int ret = 0;
        if (!_port0StateHwFirst)
        {
            PortWrite(1, 1);//APUユーザープログラム転送モード
            PortWrite(2, (byte)(addr & 0xFF));
            PortWrite(3, (byte)(addr >> 8));
            _port0StateHw += 2;
            if (_port0StateHw == 0)
                _port0StateHw += 2;
            ret = PushAndWait(0, _port0StateHw);
            if (ret < 0)
                return false;
        }
        else
        {
            _port0StateHwFirst = false;
        }

        _port0StateHw = 0;
        for (int i = 0; i < data.Length; i++)
        {
            PortWrite(1, data[i]);
            ret = PushAndWait(0, _port0StateHw++);
            if (ret < 0)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Low-level function to write a byte to a DSP register.
    /// </summary>
    public bool SetDspData(byte reg, byte data)
    {
        //ホストから送られてくるコマンド (SPC_PORT0 の値) の最上位ビットが1であれば、RAM関連の処理を行う toram ラベルにジャンプします。
        //最上位ビットが0であれば、DSPレジスタへの書き込み処理を実行します。
        //SPC_PORT2 から書き込みたいDSPレジスタのアドレスを読み込みます。
        //SPC_PORT1 から書き込みたいデータを読み込み、指定されたDSPレジスタに書き込みます。
        //最後に mov SPC_PORT0, a でホストに処理完了を伝え、再び loop の先頭に戻って次のコマンドを待ちます。
        PortWrite(2, reg);
        PortWrite(1, data);
        int ret = PushAndWait(0, _port0StateHw);
        _port0StateHw = (byte)(_port0StateHw ^ 0x01);
        
        if (ret < 0)
            return false;
        return ret >= 0;
    }

    /// <summary>
    /// Low-level function to write a single byte to a specific RAM address.
    /// </summary>
    public bool SetRamData(int addr, byte data)
    {
        PortWrite(1, data);
        PortWrite(2, (byte)(addr & 0xFF));
        PortWrite(3, (byte)(addr >> 8));
        // 0x80: Write 1 byte to RAM
        // 0xC0: Write <num> bytes to RAM
        // 0xA0: SW Reset
        int ret = PushAndWait(0, (byte)(0x80 | _port0StateHw));
        _port0StateHw = (byte)(_port0StateHw ^ 0x01);

        if (ret < 0)
            return false;
        return true;
    }

    /// <summary>
    /// Low-level function to write a single byte to a specific RAM address.
    /// </summary>
    public bool SetRamData(int addr, byte[] data)
    {
        PortWrite(2, (byte)(addr & 0xFF));
        PortWrite(3, (byte)(addr >> 8));
        int ret = PushAndWait(0, (byte)(0xC0 | _port0StateHw));
        _port0StateHw = (byte)(_port0StateHw ^ 0x01);
        if (ret < 0)
            return false;

        int idx = 0;
        for (int i = 0; i < data.Length / 3; i++)
        {
            PortWrite(1, data[idx++]);
            PortWrite(2, data[idx++]);
            PortWrite(3, data[idx++]);
            ret = PushAndWait(0, _port0StateHw);
            _port0StateHw = (byte)(_port0StateHw ^ 0x01);
            if (ret < 0)
                return false;
        }
        PortWrite(0, (byte)0x80);
        ret = ReadAndWait(3, P3_WAIT_VALUE, 10000);
        _port0StateHw = 1;
        if (ret < 0)
            return false;

        for (int i = 0; i < data.Length % 3; i++)
        {
            if(!SetRamData(addr + idx, data[idx]))
                return false;
            idx++;
        }
        return true;
    }

    private void WaitMicroSeconds(int usec)
    {
        Thread.Sleep(usec / 1000);
    }

    private int WaitReady()
    {
        for (int i = 0; i < 100; i++)
        {
            if ((_spc700.Read(0) == 0xaa) && (_spc700.Read(1) == 0xbb))
                return 0;
            WaitMicroSeconds(10000); // Wait 10ms before retrying
        }
        return -1; // Simplified from C++; error checking would happen in BulkRead/Write.
    }

    #region Wait Methods

    private int ReadAndWait(byte portAddr, byte waitValue, int ms = 0)
    {
        for (int i = 0; i < 100; i++)
        {
            if (_spc700.Read(portAddr) == waitValue)
                return waitValue;
            if (ms > 0)
                WaitMicroSeconds(ms);
        }
        return -1;
    }

    private int PushAndWait(byte portAddr, byte waitValue)
    {
        _spc700.Write(portAddr, waitValue);
        for (int i = 0; i < 100; i++)
        {
            if (_spc700.Read(portAddr) == waitValue)
                return waitValue;
        }
        return -1;
    }

    public void Dispose()
    {
        _spc700.Dispose();
    }

    #endregion
}

/// <summary>
/// Defines the contract for a GIMIC USB communication device.
/// </summary>
public interface ISpc700 : IDisposable
{
    void Reset();
    void Write(byte portAddr, byte data);
    byte Read(byte portAddr);
}