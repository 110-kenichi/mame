export PATH := $(PATH):"/mnt/c/Program Files/SDCC/bin"

PROGRAM = VGMPlay_sms.sms
CC = sdcc.exe
CFLAGS = -c -mz80 --std-c2x --opt-code-speed --debug 
OBJS =  main.rel Types.rel font.rel sound.rel video.rel font.rel font_tile.rel uart.rel logo_code.rel logo_tile.rel player_code.rel player_tile.rel

.SUFFIXES: .c .rel .asm

.PHONY: all
all: depend $(PROGRAM)

$(PROGRAM): crt0b_sms.rel $(OBJS) uart-4800.rel
	$(CC) -o VGMPlay_sms.ihx -mz80 --no-std-crt0 --data-loc 0xC000\
	 SMSLib/SMSlib.lib\
	 -Wl-b_BANK_C1=0x14000\
	 -Wl-b_BANK_C2=0x24000\
	 -Wl-b_BANK_D1=0x38000\
	 -Wl-b_BANK_D2=0x48000\
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

logo_tile.rel: logo_tile.c 
	sdcc.exe $(CFLAGS) --constseg BANK_D1 logo_tile.c

player_tile.rel: player_tile.c 
	sdcc.exe $(CFLAGS) --constseg BANK_D2 player_tile.c

uart-4800.rel: uart-4800.asm
	sdasz80.exe -y -o uart-4800.rel uart-4800.asm

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
