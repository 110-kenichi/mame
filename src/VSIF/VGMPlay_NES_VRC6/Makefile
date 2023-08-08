export PATH := $(PATH):/mnt/c/cc65/bin/

PROGRAM_VRC6 = VGMPlay_nes_mamicart_vrc6.nes
CC65 = cc65.exe
CL65 = cl65.exe
CA65 = ca65.exe
LD65 = ld65.exe
CFLAGS = -t nes

.PHONY: all
all: $(PROGRAM_VRC6)

$(PROGRAM_VRC6): boot_vrc6.o VGMPlay.o
	$(LD65)  -m $@.map $^ -o $(PROGRAM_VRC6) --config vrc6.cfg
	cmd.exe /C copy_vrc6.bat

.PHONY: run
run:
#	cmd.exe /C copy.bat
	/mnt/e/Emu/Mesen/Mesen.exe VGMPlay_nes_mamicart_vrc6.nes

main_direct.s: main.c
	$(CC65) -D DIRECT_ADDRESS $(CFLAGS) -o main_direct.s $<

#main_vrc6.s: main.c
#	$(CC65) -D VRC6_ADDRESS $(CFLAGS) -o main_vrc6.s $<

main_direct.o: main_direct.s
	$(CA65) $(CFLAGS) $<

VGMPlay.o: VGMPlay.s
	$(CA65) $(CFLAGS) $<
	
boot_vrc6.o: boot_vrc6.s
	$(CA65) $(CFLAGS) $<

.PHONY: clean
clean:
	rm $(PROGRAM_VRC6) *.o main_vrc6.s main_direct.s
