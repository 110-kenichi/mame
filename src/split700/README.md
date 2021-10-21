split700
========
[![Travis Build Status](https://travis-ci.com/gocha/split700.svg?branch=master)](https://travis-ci.com/gocha/split700) [![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/2styuijab21vtr07/branch/master?svg=true)](https://ci.appveyor.com/project/gocha/split700/branch/master)

Extracts BRR samples from SNES SPC700 format (*.spc)

How To Use
----------

Syntax: `split700 (options) *.spc`

You can display a help by `split700 --help`

Drag and drop SPC file(s) into split700.exe, and you will get BRR samples in the input directory.

### Options

|Short   |Long           |Description                                                      |
|--------|---------------|-----------------------------------------------------------------|
|`-f`    |`--force`      |Force output every samples (including corrupt samples).          |
|`-n N`  |`--srcn N`     |Specify target sample number. (example: `--srcn "1, 2, $10-20"`) |
|`-l`    |`--list`       |Display only voice list, with no file outputs.                   |
|        |`--wav`        |Convert BRR samples to Microsoft WAVE files.                     |
|        |`--pitch HEX`  |Specify sample rate for output WAVE file (0x1000 = 32000 Hz).    |
|`-L`    |N/A            |Add loop point info to output filename of the sample.            |
|`-M`    |N/A            |Add file header for addmusicM (i.e. export loop-point).          |
|`-?`    |`--help`       |Display this help.                                               |

Thanks To
---------

- [Snes9x](http://www.snes9x.com/): One of famous SNES emulators. BRR decoding routine helped me a lot.
- [snesbrr](http://www.romhacking.net/utilities/407/): Good BRR converter. On the other hand, I created brr2wav to get more raw output. (It should be bundled with split700 package.)
