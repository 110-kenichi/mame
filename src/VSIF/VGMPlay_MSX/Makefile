export PATH := $(PATH):"/mnt/c/Program Files/SDCC/bin"

PROGRAM = VGM_msx.rom
PROGRAMV = VGM_msx_Vkey.rom
CC = sdcc.exe
CFLAGS = -c -mz80 --std-c2x --opt-code-speed --debug 
OBJS =  main.rel player_code.rel 
OBJSV =  main_v.rel player_code.rel 

.SUFFIXES: .c .rel .asm

.PHONY: all
all: depend $(PROGRAM) $(PROGRAMV)

#ihx2bin http://www5d.biglobe.ne.jp/~hra/note/msx_game/chapter0B.htm

$(PROGRAM): $(OBJS) uart-4800.rel uart-4800_p2.rel SARCHFM.rel start.rel
	$(CC) -o VGM_msx.hex -mz80 --no-std-crt0 --code-loc 0x4100 --data-loc 0x5000 $^
	makebin.exe -s 65536 VGM_msx.hex > VGM_msx.bin
#	./ihx2bin.exe VGM_msx.hex VGM_msx.bin
#	./hex2rom.exe VGM_msx.hex VGM_msx.rom
	perl romout.pl VGM_msx.bin > VGM_msx.rom
	cmd.exe /C copy.bat

$(PROGRAMV): $(OBJSV) uart-4800.rel uart-4800_p2.rel SARCHFM.rel start.rel
	$(CC) -o VGM_msx_Vkey.hex -mz80 --no-std-crt0 --code-loc 0x4100 --data-loc 0x5000 $^
	makebin.exe -s 65536 VGM_msx_Vkey.hex > VGM_msx_Vkey.bin
	perl romout.pl VGM_msx_Vkey.bin > VGM_msx_Vkey.rom

#	/mnt/e/Emu/blueMSX/blueMSX.exe VGM_msx.rom

.PHONY: run
run:
	cmd.exe /C copy.bat
#	/mnt/e/Emu/blueMSX/blueMSX.exe "E:\mame_src\mami_dev\mame\src\VSIF\VGMPlay_MSX\VGM_msx.rom"

start.rel: start.asm
	sdasz80.exe -y -o start.rel start.asm

uart-4800.rel: uart-4800.asm const.inc macro.inc
	sdasz80.exe -y -o uart-4800.rel uart-4800.asm

uart-4800_p2.rel: uart-4800_p2.asm const.inc macro.inc
	sdasz80.exe -y -o uart-4800_p2.rel uart-4800_p2.asm

SARCHFM.rel: SARCHFM.asm const.inc macro.inc
	sdasz80.exe -y -o SARCHFM.rel SARCHFM.asm

main_v.rel: 
	$(CC) $(CFLAGS) -DVKEY -o main_v.rel main.c

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
