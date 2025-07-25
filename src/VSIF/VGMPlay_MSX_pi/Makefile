#export PATH := $(PATH):"/mnt/c/Program Files/SDCC/bin"

PROGRAM = VGM_mpi.rom
PROGRAMV = VGM_mpi_VKey.rom

CC = sdcc
CFLAGS = -c -mz80 --std-c2x --opt-code-speed --debug 
OBJS =  main.rel player_code.rel 
OBJSV =  main_v.rel player_code.rel 

.SUFFIXES: .c .rel .asm

.PHONY: all
all: depend $(PROGRAM) $(PROGRAMV) $(PROGRAMtR) $(PROGRAMVtR)

#ihx2bin http://www5d.biglobe.ne.jp/~hra/note/msx_game/chapter0B.htm

$(PROGRAM): $(OBJS) uart-4800.rel uart-4800_p2.rel SARCHFM.rel start.rel
	$(CC) -o VGM_mpi.hex -mz80 --no-std-crt0 --code-loc 0x4100 --data-loc 0x5000 $^
	makebin.exe -s 65536 VGM_mpi.hex > VGM_mpi.bin
	perl romout.pl VGM_mpi.bin > VGM_mpi.rom
	cmd.exe /C copy.bat

$(PROGRAMV): $(OBJSV) uart-4800.rel uart-4800_p2.rel SARCHFM.rel start.rel
	$(CC) -o VGM_mpi_VKey.hex -mz80 --no-std-crt0 --code-loc 0x4100 --data-loc 0x5000 $^
	makebin.exe -s 65536 VGM_mpi_VKey.hex > VGM_mpi_VKey.bin
	perl romout.pl VGM_mpi_VKey.bin > VGM_mpi_VKey.rom

#	/mnt/e/Emu/blueMSX/blueMSX.exe VGM_msx.rom

.PHONY: run
run:
	cmd.exe /C copy.bat
#	/mnt/e/Emu/blueMSX/blueMSX.exe "E:\mame_src\mami_dev\mame\src\VSIF\VGMPlay_MSX\VGM_msx.rom"

start.rel: start.asm
	sdasz80 -y -o start.rel start.asm

uart-4800.rel: uart-4800.asm const.inc macro.inc
	sdasz80 -y -o uart-4800.rel uart-4800.asm

uart-4800_p2.rel: uart-4800_p2.asm const.inc macro.inc
	sdasz80 -y -o uart-4800_p2.rel uart-4800_p2.asm

SARCHFM.rel: SARCHFM.asm const.inc macro.inc
	sdasz80 -y -o SARCHFM.rel SARCHFM.asm

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
