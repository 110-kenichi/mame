export PATH := $(PATH):"/mnt/c/Program Files/SDCC/bin"

PROGRAM = VGMPlay_msx.rom
CC = sdcc.exe
CFLAGS = -c -mz80 --std-c2x --opt-code-speed --debug 
OBJS =  main.rel player_code.rel 

.SUFFIXES: .c .rel .asm

.PHONY: all
all: depend $(PROGRAM)

#ihx2bin http://www5d.biglobe.ne.jp/~hra/note/msx_game/chapter0B.htm

$(PROGRAM): $(OBJS) uart-4800.rel SARCHFM.rel start.rel
	$(CC) -o VGMPlay_msx.hex -mz80 --no-std-crt0 --code-loc 0x4100 --data-loc 0x5000 $^
	makebin.exe -s 65536 VGMPlay_msx.hex > VGMPlay_msx.bin
#	./ihx2bin.exe VGMPlay_msx.hex VGMPlay_msx.bin
#	./hex2rom.exe VGMPlay_msx.hex VGMPlay_msx.rom
	perl romout.pl VGMPlay_msx.bin > VGMPlay_msx.rom
	cmd.exe /C copy.bat
#	/mnt/e/Emu/blueMSX/blueMSX.exe VGMPlay_msx.rom

.PHONY: run
run:
	cmd.exe /C copy.bat
	/mnt/e/Emu/blueMSX/blueMSX.exe "E:\mame_src\mami_dev\mame\src\VSIF\VGMPlay_MSX\VGMPlay_msx.rom"
#	/mnt/e/Emu/Fusion/Fusion.exe VGMPlay_sms.sms

start.rel:
	sdasz80.exe -y -o start.rel start.asm

uart-4800.rel: uart-4800.asm
	sdasz80.exe -y -o uart-4800.rel uart-4800.asm

SARCHFM.rel: SARCHFM.asm
	sdasz80.exe -y -o SARCHFM.rel SARCHFM.asm

.c.rel: 
	$(CC) $(CFLAGS) $<

.PHONY: clean
clean:
	rm $(PROGRAM) $(OBJS) *.lst *.sym

.PHONY: depend
depend: $(OBJS:.rel=.c)
	-@ rm depend.inc
	-@ for i in $^; do cpp -MM $$i | sed "s/\ [_a-zA-Z0-9][_a-zA-Z0-9]*\.c//g" >> depend.inc; done

-include depend.inc
