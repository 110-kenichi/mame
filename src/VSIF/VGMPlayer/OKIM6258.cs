/*
 * Copyright (c) 2001 Tetsuya Isaki. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. All advertising materials mentioning features or use of this software
 *    must display the following acknowledgement:
 *      This product includes software developed by Tetsuya Isaki.
 * 4. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
 * AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */
//https://github.com/umjammer/vavi-sound/blob/master/src/main/java/vavi/sound/adpcm/oki/Oki.java
//umjammer
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.VGMPlayer
{

    /// <summary>
    /// 
    /// </summary>
    public class OKIM6258
    {
        /** */
        private int mc_amp;
        /** */
        private int mc_estim;

        /** 次回の PCM 値を予測するためのテーブル */
        private static int[] adpcm_estimindex = {
                2,  6,  10,  14,  18,  22,  26,  30,
                -2, -6, -10, -14, -18, -22, -26, -30
            };

        /** 量子化幅 */
        private static int[] adpcm_estim = {
                16,  17,  19,  21,  23,  25,  28,  31,  34,  37,
                41,  45,  50,  55,  60,  66,  73,  80,  88,  97,
                107, 118, 130, 143, 157, 173, 190, 209, 230, 253,
                279, 307, 337, 371, 408, 449, 494, 544, 598, 658,
                724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552
            };

        /** 何するテーブル? */
        private static int[] adpcm_estimstep = {
                -1, -1, -1, -1, 2, 4, 6, 8,
                -1, -1, -1, -1, 2, 4, 6, 8
            };

        /**
         * signed linear 16 の 1 サンプルを Oki ADPCM 1 サンプルに変換します。
         * <p>
         * MSM6258 が期待する PCM は 12bit 符号付 PCM (slinear12 にあたる) である。
         * 従ってこの関数では同時に振幅変換(16bit -> 12bit) も行なっている。
         * </p>
         *
         * @param a 16bit signed linear pcm
         * @return 4bit oki adpcm
         */
        public int encode(int a)
        {
            // mc->mc_estim には、前回の差分比予測値インデックスが入っている
            int estim = this.mc_estim;
            int b;
            int s;

            // df は、実 PCM 値 a と前回の予測 PCM 値との差分である
            int df = a - this.mc_amp;
            // dl は、差分比予測値テーブル adpcm_estim[] から、予測された
            // 差分比を取り出したもの
            int dl = adpcm_estim[estim];
            // c は、差分を 12bit に変換し、(dl/8) に対する比率を出したもの。
            // /16 は 16bit と 12bit との差、すなわち 2^4 である。
            // *8 は、dl すなわち adpcm_estim[] の値が8倍値で記録してあること
            // に起因する
            int c = (df / 16) * 8 / dl;
            // c が正か負かによって処理が変わる。
            // ただし c は除算により 0 になっているかも知れず、その場合正として
            // 扱われてしまうのを避けるため符号判定には df を用いている。
            //
            // 実際にエンコードする ADPCM データは、符号ビットと振幅ビットの
            // 組み合わせである。振幅ビットは c を 2 で割ったもの
            if (df < 0)
            {
                b = -c / 2;
                s = 0x08;
            }
            else
            {
                b = c / 2;
                s = 0;
            }
            // 振幅は 3bit なので、7 で制限する
            if (b > 7)
            {
                b = 7;
            }
            // こうしておくことで、以後 s は符号つき 4bit。
            // b は符号なしの絶対値として使い分けることができる
            s |= b;
            // ここまでの変換で、振幅ビット b の実際の比率 c との関係は、
            // b : 比率範囲 c
            // 0 : 0 <= 比率 < 2
            // 1 : 2 <= 比率 < 4
            // 2 : 4 <= 比率 < 6
            // 3 : 6 <= 比率 < 8
            // 4 : 8 <= 比率 < 10
            // 5 : 10 <= 比率 < 12
            // 6 : 12 <= 比率 < 14
            // 7 : 14 <= 比率
            // のようになる。
            //
            // 次回のPCM値を予測する。すごくすっきりしているが、実際には
            //
            //  static int adpcm_estimindex_0[16] = {
            //    1,  3,  5,  7,  9,  11,  13,  15,
            //   -1, -3, -5, -7, -9, -11, -13, -15
            //  };
            //  mc->mc_amp += (short) (adpcm_estimindex_0[(int) s] * 16 / 8 * dl);
            //
            // なのである。adpcm_estimindex_0[] は adpcm_estimindex[] の
            // 1/2 値であり、この数列の意味は上記比率範囲の中央値である。
            // この比率に、16 = 2 ^ 4 を掛けて 16 bit 化し、(dl / 8) を掛けることに
            // よって予測差分値が求められ、これを記録するのである。
            // そういうわけで、adpcm_estimindex[] をあらかじめ2倍しておくと
            // すっきりするのである。
            this.mc_amp += adpcm_estimindex[s] * dl;
            // b の値に従って、次回使用する差分比を予測し mc->mc_estim に
            // 保存しておく。全部で 49 段階。
            // 余談であるが、ここだけだと adpcm_estimstep[16] は [8] でよい。
            // [16] なのは adpcm2pcm との関係上必要だからである。
            estim += adpcm_estimstep[b];
            if (estim < 0)
            {
                estim = 0;
            }
            else if (estim > 48)
            {
                estim = 48;
            }

            this.mc_estim = estim;
            return s;
        }

        /**
         * Oki ADPCM 1 サンプルを signed linear 16 の 1 サンプルに変換します。
         * <p>
         * MSM6258 が出力する PCM は 12bit 符号付 PCM (slinear12 にあたる) である。
         * 従ってこの関数では同時に振幅変換(12bit -> 16bit) も行なっている。
         * </p>
         *
         * @param b 4bit adpcm
         * @return 16bit linear pcm
         */
        public int decode(int b)
        {
            // mc->mc_estim には、前回の差分比予測値インデックスが入っている
            int estim = this.mc_estim;

            // 実 PCM 値を計算している。本来の式は
            //
            //  mc->mc_amp += adpcm_estim[estim] / 8 * adpcm_estimindex_0[b] * 16;
            //
            // である。pcm2adpcm_step() でも述べた通り adpcm_estim[] は8倍値で
            // あるので 8 で割る。それに比率である adpcm_estimindex_0[] を掛ける。
            // 更に 12bit -> 16bit 変換のために 16 (= 2^4) を掛けている。
            // adpcm_estimindex_0[] * 2 は adpcm_estimindex[] であるので、
            // 上式は実際に使われている以下の式となる
            this.mc_amp += adpcm_estim[estim] * adpcm_estimindex[b];
            // 次回の差分比を予測して mc->mc_estim に保存しておく
            estim += adpcm_estimstep[b];

            if (estim < 0)
            {
                estim = 0;
            }
            else if (estim > 48)
            {
                estim = 48;
            }

            this.mc_estim = estim;

            if (this.mc_amp > short.MaxValue)
                this.mc_amp = short.MaxValue;
            else if (this.mc_amp < short.MinValue)
                this.mc_amp = short.MinValue;

            return this.mc_amp;
        }
    }
}
