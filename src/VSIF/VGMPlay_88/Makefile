export PATH := $(PATH):"c:/z88dk/bin"

PROGRAM = VGM_88.d88
CC = zcc.exe
Z88DK_DIR = c:/z88dk/
ZCCCFG = $(Z88DK_DIR)/lib/config

.SUFFIXES: .c .rel .asm

.PHONY: all
all: $(PROGRAM)

#ihx2bin http://www5d.biglobe.ne.jp/~hra/note/msx_game/chapter0B.htm

$(PROGRAM): player_code.c uart-4800.asm const.inc macro.inc
	$(CC) +pc88 -create-app -subtype=disk -o $(PROGRAM) player_code.c uart-4800.asm 
	$(CC) +pc88 -create-app -subtype=disk -o $(PROGRAM) player_code.c uart-4800.asm 
#	makebin.exe -s 65536 VGM_88.hex > VGM_88.bin
#	perl romout.pl VGM_88.bin > VGM_88.rom
#	cmd.exe /C copy.bat

#	/mnt/e/Emu/blueMSX/blueMSX.exe VGM_msx.rom

.PHONY: run
run:
	cmd.exe /C copy.bat
#	/mnt/e/Emu/blueMSX/blueMSX.exe "E:\mame_src\mami_dev\mame\src\VSIF\VGMPlay_MSX\VGM_msx.rom"

.PHONY: clean
clean:
	rm $(PROGRAM) $(OBJS) *.lst *.sym
