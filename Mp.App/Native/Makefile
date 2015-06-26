####################################################
###### Copyright(c) GHI Electronics, LLC ######
####################################################

OUTFILE=Extension
LINKERSCRIPT=LinkScript.lds

INCL=./include

CC		=arm-none-eabi-gcc
LD		=arm-none-eabi-gcc

CCFLAGS=  -g -mlittle-endian -mcpu=arm7tdmi  -Wall -I. -I$(INCL)
CCFLAGS+= -mapcs-frame -fno-builtin -g0 -O3

LDFLAGS =-nostartfiles -Wl,--Map -Wl,$(OUTFILE).map
LDFLAGS+=-lc -lgcc -Wl,--omagic
LDFLAGS+=-T $(LINKERSCRIPT)

SRCS=Extension.c Lcd.c
OBJS=$(SRCS:.c=.o)

rebuild: clean all

all: $(OBJS)
	$(LD) $(LDFLAGS) -o $(OUTFILE).elf $(OBJS) -lm
	
%.o: %.c 
	$(CC) -c $(CCFLAGS) $< -o $@

clean:
	-rm *.o *.elf *.map
