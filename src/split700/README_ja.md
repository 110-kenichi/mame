split700
========
[![Travis Build Status](https://travis-ci.org/gocha/split700.svg?branch=master)](https://travis-ci.org/gocha/split700) [![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/2styuijab21vtr07/branch/master?svg=true)](https://ci.appveyor.com/project/gocha/split700/branch/master)

SNES SPC700 フォーマット（*.spc）から BRR 形式のサンプルを抽出します。

使用方法
------------------------

Syntax: `split700 (options) *.spc`

`split700 --help` でヘルプを表示することができます。

SPC ファイルを split700.exe にドロップすれば、入力ディレクトリに BRR サンプルが得られます。

### オプション

|短形式 |長形式         |説明                                                               |
|-------|---------------|-------------------------------------------------------------------|
|`-f`   |`--force`      |すべてのサンプル（異常なサンプルを含む）も強制的に出力します。     |
|`-n N` |`--srcn N`     |対象サンプルナンバーを指定します。（例: `--srcn "1, 2, $10-20"`）  |
|`-l`   |`--list`       |音声の一覧を表示しますが、BRR ファイルを出力しません。             |
|       |`--wav`        |BRR サンプルを Microsoft WAVE ファイルに変換します。               |
|       |`--pitch HEX`  |WAVE ファイル出力のサンプルレートを指定します（0x1000 = 32000 Hz） |
|`-L`   |N/A            |ループポイント情報をサンプルの出力ファイル名に付加します。         |
|`-M`   |N/A            |AddMusicM 向けのファイルヘッダを付加します（ループポイント出力）   |
|`-?`   |`--help`       |ヘルプを表示します。                                               |

スペシャルサンクス
------------------------

- [Snes9x](http://www.snes9x.com/): 有名な SNES エミュレーターです。BRR デコード処理を参考にしました。
- [snesbrr](http://www.romhacking.net/utilities/407/): 優れた BRR コンバーターです。一方、より単純な生の出力を得るために brr2wav というツールを自作しました（split700 とともに同梱されています）。
