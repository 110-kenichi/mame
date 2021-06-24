export PATH := $(PATH):/mnt/c/cc65/bin/

PROGRAM = VGMPlay_nes.nes
PROGRAM_VRC6 = VGMPlay_nes_vrc6.nes
PROGRAM_FDS = VGMPlay_nes_fds.fds
CC65 = cc65.exe
CL65 = cl65.exe
CA65 = ca65.exe
LD65 = ld65.exe
CFLAGS = -t nes

.PHONY: all
all: $(PROGRAM) $(PROGRAM_VRC6) $(PROGRAM_FDS)

$(PROGRAM): main_direct.o VGMPlay.o
	$(LD65) -t nes -m $@.map $^ nes.lib -o $(PROGRAM)
	cmd.exe /C copy.bat

$(PROGRAM_VRC6): boot_vrc6.o VGMPlay.o
	$(LD65)  -m $@.map $^ -o $(PROGRAM_VRC6) --config vrc6.cfg
	cmd.exe /C copy_vrc6.bat

$(PROGRAM_FDS): boot_fds.o VGMPlay.o
	$(LD65)  -m $@.map $^ -o $(PROGRAM_FDS) --config fds.cfg
	cmd.exe /C copy_fds.bat

.PHONY: run
run:
#	cmd.exe /C copy.bat
#	/mnt/e/Emu/Mesen/Mesen.exe VGMPlay_nes.nes
	/mnt/e/Emu/Mesen/Mesen.exe VGMPlay_nes_fds.fds
#	/mnt/e/Emu/Mesen/Mesen.exe VGMPlay_nes_vrc6.nes

main_direct.s: main.c
	$(CC65) -D DIRECT_ADDRESS $(CFLAGS) -o main_direct.s $<

#main_vrc6.s: main.c
#	$(CC65) -D VRC6_ADDRESS $(CFLAGS) -o main_vrc6.s $<

main_direct.o: main_direct.s
	$(CA65) $(CFLAGS) $<

#main_vrc6.o: main_vrc6.s
#	$(CA65) $(CFLAGS) $<

VGMPlay.o: VGMPlay.s
	$(CA65) $(CFLAGS) $<

boot_vrc6.o: boot_vrc6.s
	$(CA65) $(CFLAGS) $<

boot_fds.o: boot_fds.s
	$(CA65) $(CFLAGS) $<

.PHONY: clean
clean:
	rm $(PROGRAM) $(PROGRAM_VRC6) $(PROGRAM_FDS) *.o main_vrc6.s main_direct.s
