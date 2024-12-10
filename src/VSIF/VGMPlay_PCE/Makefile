
CC65_HOME   = ./cc65
MAKEFLAGS   = -j

SYS	    = nes

croper	= $(CC65_HOME)/bin/croper.exe
CL	= $(CC65_HOME)/bin/cl65.exe
CC	= $(CC65_HOME)/bin/cc65.exe
AS	= $(CC65_HOME)/bin/ca65.exe
LD	= $(CC65_HOME)/bin/ld65.exe

INC	= $(CC65_HOME)/include
ASMINC	= $(CC65_HOME)/asminc
# This one comes with VICE

# --static-locals
CFLAGS		:= --cpu HuC6280 -O
AFLAGS		:= --cpu HuC6280 -t $(SYS) 
BDIR		:=build

vpath %.o $(BDIR)/


SRC_S := crt0.s sys_asm.s bios_asm.s
SRC_C := $(wildcard *.c)
#SRC_C := $(filter-out app-%.c, $(SRC_C))

OBJ	= $(SRC_S:.s=.o)
OBJ	+= $(SRC_C:.c=.o)
OFILES	= $(OBJ:%.o=$(BDIR)/%.o)
	
# --------------------------------------------------------------------------
all: $(OBJ) 
	$(LD) -C pce.cfg -o VGMPlay_PCE.pce $(OFILES) nes.lib -m $(BDIR)/mem.map
	./turbolink.exe VGMPlay_PCE.pce

%.o: %.s
	$(AS) $(AFLAGS) -o $(basename $(BDIR)/$<).o $<

%.o: %.c
	$(CC) $(CFLAGS) -o $(basename $(BDIR)/$<).s $<
	$(AS) $(AFLAGS) $(basename $(BDIR)/$<).s


clean:
	$(RM) $(BDIR)/*.o
	$(RM) $(BDIR)/*.s
	$(RM) $(APDIR)/*.o
