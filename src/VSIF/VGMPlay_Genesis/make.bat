E:/MD/sgdk/bin/make -f E:/MD/sgdk/makefile.gen
copy out\rom.bin VGMPlay_md.bin
if exist G:\ xcopy /Y /D VGMPlay_md.bin G:
if exist F:\ xcopy /Y /D VGMPlay_md.bin F:
