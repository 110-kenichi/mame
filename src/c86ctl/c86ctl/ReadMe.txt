---------------------------------------------------------------------
  C86CTL   by honet.kk@gmail.com
---------------------------------------------------------------------

==============
 What's this?
==============
OPNAのパラメータビュアーです。本DLLに対して音源チップへのレジスタ
アクセス情報を送ると、チップの状態を可視化します。

また、ハードウェアＦＭ音源の制御機能を持っており、下記のハードウェア
が接続されている場合は制御を行います。
・G.I.M.I.C.   http://gimic.jp/


============
 使い方
============
未記述


==========
 LICENSE
==========
Copyright (c) 2009-2011, Kiichiro Kotajima. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS
BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.


For further information please contact.
 honet.kk (at) gmail.com

============
 ChangeLog
============
1.4.4(RELEASE)			・keyboardウィンドウが閉じれなかったバグ修正
						・keyboardウィンドウアクティブ時にキーボードでsolo/mute操作できるようにした
						　1～9, QWERTでMute, SHIFT+1～9,QWERTでSolo
						　0でMute全解除, SHIFT+0でSolo全解除

1.4.3(RELEASE)			・大幅機能アップ
						（いろいろ弄りすぎて詳細忘れたので必要ならコード参照で・・・）
						  MIDIインタフェイス方式廃止。

1.3.0.11				・GUI復活（GUIルーチン全書換）
						・OPMのアドレスフィルタ処理に問題があったのを修正
						・IGimic::getMBInfo/getModuleInfo において
						　文字列終端が０にならない場合があるバグを修正
						　（1.2.0.8で直ってませんでした。すいません。）
						・その他沢山変更。

1.2.0.8					・CreateInstance関数の呼び出し規約をWINAPIに変更
						　（1.2.0.6で付け忘れました。ごめんなさい。）
						・IRealChipBaseのGUID変更
						　（IRealChipと同じGUIDになっていたバグを修正）
						・HID通信時のトランザクションに同期処理追加
						・IGimic::getMBInfo/getModuleInfo において
						　文字列終端が０にならない場合があるバグを修正

1.2.0.7					・LICENSE文修正（コピーライト文にコピペ消し忘れの変な文が残ってた･･･）
						　※利用条件に変更はありません。
						・OPMのレジスタアクセスフィルタにおいて
						　0x20<addrの時にレジスタキャッシュ値が更新されていなかったバグを修正
						・OPNAのレジスタアクセスフィルタにおいて
						　0x100==addrのアクセスがおかしくなってしまっていたバグを修正

1.2.0.6					HID通信の仕様を大幅変更。gimic-r158以降用
1.1.0.3					バージョンリソース追加




::TODO::
・adpcmバースト転送
・ウインドウ表示状態保存
・try/catchコードを全ルーチンに
・WriteFileに対する保護をハンドルごとのMutexに変える
・OPN系のFNUM/BLOCKからOCT/NOTE計算処理をφMに対応するように直す。

