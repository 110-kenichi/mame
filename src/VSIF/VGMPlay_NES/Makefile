export PATH := $(PATH):/mnt/c/cc65/bin/

PROGRAM = VGMPlay_nes.nes
PROGRAM_VRC6 = VGMPlay_nes_vrc6.nes
CC65 = cc65.exe
CL65 = cl65.exe
CA65 = ca65.exe
LD65 = ld65.exe
CFLAGS = -t nes
OBJS =  main.o VGMPlay.o

.PHONY: all
all: $(PROGRAM) $(PROGRAM_VRC6)

$(PROGRAM): $(OBJS)
	$(LD65) -t nes -m $@.map $^ nes.lib -o $(PROGRAM)
	cmd.exe /C copy.bat

#$(PROGRAM_VRC6): $(OBJS) header_vrc6.o
#	$(LD65)  -m $@.map $^ nes.lib -o $(PROGRAM_VRC6) --config vrc6.cfg
#	cmd.exe /C copy_vrc6.bat

.PHONY: run
run:
#	cmd.exe /C copy.bat
	/mnt/e/Emu/Mesen/Mesen.exe VGMPlay_nes.nes

main.s: main.c
	$(CC65) $(CFLAGS) $<

main.o: main.s
	$(CA65) $(CFLAGS) $<

VGMPlay.o: VGMPlay.s
	$(CA65) $(CFLAGS) $<

header_vrc6.o: header_vrc6.s
	$(CA65) $(CFLAGS) $<

.PHONY: clean
clean:
	rm $(PROGRAM) $(PROGRAM_VRC6) $(OBJS)
