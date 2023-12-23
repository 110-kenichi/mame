export PATH := $(PATH):"/mnt/c/Program Files/SDCC/bin"

PROGRAM = VGMPlay_sms_ftdi.sms
CC = sdcc.exe
CFLAGS = -c -mz80 --std-c2x --opt-code-speed --debug 
OBJS =  main.rel font.rel video.rel font.rel font_tile.rel player_code.rel

.SUFFIXES: .c .rel .asm

.PHONY: all
all: depend $(PROGRAM)

$(PROGRAM): crt0b_sms.rel $(OBJS) uart-4800.rel uart-ftdi.rel
	$(CC) -o VGMPlay_sms.ihx -mz80 --no-std-crt0 --data-loc 0xC000\
	 SMSLib/SMSlib.lib\
	 $^
	makesms.exe VGMPlay_sms.ihx VGMPlay_sms.sms
	cmd.exe /C copy.bat
#	/mnt/e/Emu/Emulicious/Emulicious.exe VGMPlay_sms.sms
#	/mnt/e/Emu/Fusion/Fusion.exe VGMPlay_sms.sms

.PHONY: run
run:
	cmd.exe /C copy.bat
	/mnt/e/Emu/Emulicious/Emulicious.exe VGMPlay_sms.sms
#	/mnt/e/Emu/Fusion/Fusion.exe VGMPlay_sms.sms

uart-4800.rel: uart-4800.asm
	sdasz80.exe -y -o uart-4800.rel uart-4800.asm

uart-ftdi.rel: uart-ftdi.asm
	sdasz80.exe -y -o uart-ftdi.rel uart-ftdi.asm

crt0b_sms.rel: crt0b_sms.s
	sdasz80.exe -g -y -o crt0b_sms.rel crt0b_sms.s

.c.rel: 
	$(CC) $(CFLAGS) $<

.PHONY: clean
clean:
	rm $(PROGRAM) $(OBJS) *.lst *.sym *.asm

.PHONY: depend
depend: $(OBJS:.rel=.c)
	-@ rm depend.inc
	-@ for i in $^; do cpp -MM $$i | sed "s/\ [_a-zA-Z0-9][_a-zA-Z0-9]*\.c//g" >> depend.inc; done

-include depend.inc
