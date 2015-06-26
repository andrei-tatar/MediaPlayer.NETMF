#include "RLP.h"
#include "LPC24xx.h"
#include "Extension.h"

RLP_Task bufTask, cancelTask;
int bufCount;
unsigned char bufPass;
unsigned char *buf;
unsigned char noMoreData;

//VS1053 is connected to SSP1 
//DREQ	-	P0.6	(IO33)
//SCK	-	P0.7
//SO	-	P0.8
//SI	-	P0.9
//XDCS	-	p2.1	(IO32)
//RESET	-	P3.18	(IO34)
//XCS	-	P3.30	(IO31)

#define TASK_INTERVAL	10000 //check after (us)

#define DREQ (FIO0PIN & (1<<6))

#define GREEN_ON	FIO3SET = (1 << 26)
#define GREEN_OFF	FIO3CLR = (1 << 26)
#define RED_ON		FIO3SET = (1 << 17)
#define RED_OFF		FIO3CLR = (1 << 17)

static void VsInitPorts()
{
	PINSEL0 &= ~((0x03 << 12) | (0x03 << 14) | (0x03 << 16) | (0x03 << 19)); //set DREQ, SCK, SO and SI as GPIO
	PINSEL0 |= (0x02 << 14) | (0x02 << 16) | (0x02 << 18); //set SCK, SO and SI as SSP1 (leave DREQ as GPIO)

	PINSEL4 &= ~(0x03 << 2); //set XDCS as GPIO
	PINSEL7 &= ~((0x03 << 4) | (0x03 << 28)); //set RESET and XCS as GPIO

	FIO0DIR &= ~(1<<6); //set DREQ as input
	FIO2DIR |= (1<<1); //set XDCS as output
	FIO3DIR |= (1<<18) | (1<<30); //set RESET and XCS as output
}

static void VsClearRxFifo()
{
#pragma GCC diagnostic ignored "-Wunused-but-set-variable"
	volatile unsigned char dummy;
	unsigned char i;
	for (i=0; i<8; i++)
		dummy = SSP1DR;
#pragma GCC diagnostic pop
}

void SSP1_Interrupt(void *arg);

static void VsInitSSP1()
{
	PCONP |= (1 << 10); //make sure power is on for SSP1
	
	//Set DSS data to 8-bit, Frame format SPI, CPOL = 0, CPHA = 0
	SSP1CR0 = 0x07;
	SSP1CPSR = 0x48; //clock prescaler 72 -> 1MHz clock
	SSP1CR1 = 0x02; //Enable SSP1 in master mode, normal operation
	SSP1IMSC = 0x00; //disable interrupts

	VsClearRxFifo();

	//install SSP1 interrupt
	RLPext->Interrupt.Install(11, SSP1_Interrupt, (void*)0);
	//make sure it's enabled
	RLPext->Interrupt.Enable(11);
}

static unsigned char VsReadWriteByte(unsigned char data)
{
	while (!(SSP1SR & SSPSR_TNF));
	SSP1DR = data;
	while (!(SSP1SR & SSPSR_RNE));
    return SSP1DR;
}

static void VsWriteRegister(unsigned char reg, unsigned short data)
{
	FIO2SET = (1UL << 1); //set XDCS high
	FIO3CLR = (1UL << 30); //set XCS low
	while (!DREQ);

	VsReadWriteByte(0x02);
	VsReadWriteByte(reg);
	VsReadWriteByte(data >> 8);
	VsReadWriteByte(data);
	
	FIO3SET = (1UL << 30); //set XCS high
	FIO2CLR = (1UL << 1); //set XDCS low
}

static unsigned short VsReadRegister(unsigned char reg)
{
	while (SSP1SR & SSPSR_BSY);
	SSP1CPSR = 0x0A; //clock prescaler 10 -> 7.2MHz clock
	VsClearRxFifo();

	FIO2SET = (1UL << 1); //set XDCS high
	FIO3CLR = (1UL << 30); //set XCS low
	while (!DREQ);

	VsReadWriteByte(0x03);
	VsReadWriteByte(reg);
	
	unsigned short result = VsReadWriteByte(0x00) << 8;
	result |= VsReadWriteByte(0x00);

	FIO3SET = (1UL << 30); //set XCS high
	FIO2CLR = (1UL << 1); //set XDCS low
	
	while (SSP1SR & SSPSR_BSY);
	SSP1CPSR = 0x06; //clock prescaler 6 -> 12MHz clock

	return result;
}

static void VsSoftReset()
{
	while (SSP1SR & SSPSR_BSY);
	SSP1CPSR = 0x48; //clock prescaler 72 -> 1MHz clock

	VsWriteRegister(0x00, 0x0004); //software reset
	RLPext->Delay(5);
	VsWriteRegister(0x03, 0xA4E2); //4x multiplier, no addition, 13 MHz => CLKI = 52 MHz
	VsWriteRegister(0x00, 0x0802); //allow MPEG I & II, VS1002 native mode
	VsWriteRegister(0x02, 0x0000); //bass and treble controls are off

	while (SSP1SR & SSPSR_BSY);
	SSP1CPSR = 0x06; //clock prescaler 6 -> 12MHz clock
}

static unsigned char VsLoadPlugin(unsigned char *pluginBuffer, int length)
{
	int i=0;
	while (i < length)
	{
		unsigned char addr = pluginBuffer[i++];

		if (addr > 15) return 3;

		unsigned short n, val;

		n = pluginBuffer[i++];
		n |= pluginBuffer[i++] << 8;

		if ((n & 0x8000) != 0)
		{
			n &= 0x7FFF;

			val = pluginBuffer[i++];
			val |= pluginBuffer[i++] << 8;

			while (n-- != 0) VsWriteRegister(addr, val);
		}
		else
			while (n-- != 0)
			{
				val = pluginBuffer[i++];
				val |= pluginBuffer[i++] << 8;

				VsWriteRegister(addr, val);
			}
	}

	return 0;
}

static void VsStreamSameByte32Times(unsigned char data)
{
	unsigned char i = 32;
	while (i--)
	{
		while (!(SSP1SR & SSPSR_TNF));
		SSP1DR = data;
	}
}

static unsigned char VsCancelDataStream()
{
	VsSoftReset();
	return 0;

	VsWriteRegister(0x07, 0x1E06); //WRAM_ADDR = end fill byte
	unsigned char endFillByte = VsReadRegister(0x06), i=70, stoppedOk;

	//min 2080 endFill bytes
	while (i--)
    {
		while (!DREQ); //wait dreq to rise
		VsStreamSameByte32Times(endFillByte); 
    }
	//SCI_MODE - set SM_CANCEL
    VsWriteRegister(0x00, 0x080AU); 

	stoppedOk = 0;
	i = 64;
	while (i--)
	{
		while (!DREQ); //wait dreq to rise
		VsStreamSameByte32Times(endFillByte);
		
		//check if SM_CANCEL is cleared
		if ((VsReadRegister(0x00) & 0x0008) == 0)
		{
			stoppedOk = 1;
			break;
		}
	}

	if (stoppedOk == 0)
	{
		VsSoftReset(); //do a software reset
		return 1;
	}

	return 0;
}

void SSP1_Interrupt(void *arg)
{
	GREEN_ON;
	if (bufCount == 0)
	{
		//streaming finished...
		SSP1IMSC = 0x00; //stop interrupt

		//notify that it's finished
		RLPext->PostManagedEvent(0x01);
		GREEN_OFF;
		return;
	}

	//check if VS1053 can receive more data
	if (bufPass == 0 && !DREQ)
	{		
		//it can't...
		SSP1IMSC = 0x00; //stop interrupt
		RLPext->Task.ScheduleTimeOffset(&bufTask, TASK_INTERVAL); //recheck later
		GREEN_OFF;
		return;
	}

	switch (bufCount)
	{
	case 1:
		SSP1DR = *buf++; 
		bufCount--;
		break;
	case 2:
		SSP1DR = *buf++;
		SSP1DR = *buf++;
		bufCount -= 2;
		break;
	case 3:
		SSP1DR = *buf++;
		SSP1DR = *buf++;
		SSP1DR = *buf++;
		bufCount -= 3;
		break;
	default:
		SSP1DR = *buf++;
		SSP1DR = *buf++;
		SSP1DR = *buf++;
		SSP1DR = *buf++;
		bufCount -= 4;
		break;
	}
	
	bufPass = (bufPass + 1) & 0x07;
	GREEN_OFF;
}

void VsCancelTask(void *arg)
{
	RED_ON;

	//all data streamed and no more data coming?
	if (noMoreData && bufCount == 0)
	{
		if (VsCancelDataStream())
			//notify that the streaming could not be canceled ok
			RLPext->PostManagedEvent(0x02);
	}
	else
		//recheck later
		RLPext->Task.ScheduleTimeOffset(&cancelTask, TASK_INTERVAL);

	RED_OFF;
}

void VsTask(void *arg)
{
	RED_ON;

	if (DREQ)
		//vs can receive data
		SSP1IMSC = 0x08; //enable TX fifo half empty interrupt
	else
		//recheck later
		RLPext->Task.ScheduleTimeOffset(&bufTask, TASK_INTERVAL);

	RED_OFF;
}

int VsStreamData(unsigned char *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	if (argsCount < 1 || argsCount > 2) return 0x02;

	noMoreData = 0;

	if (argsCount == 2)
	{
		int bufOffset = *(int*)args[1];
		buf = &generalArray[bufOffset];
	}
	else
	{
		buf = generalArray;
	}

	bufCount = *(int*)args[0];
	bufPass = 0;

	SSP1IMSC = 0x08; //enable TX fifo half empty interrupt

	return 0;
}

int VsNoMoreData(unsigned char *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	noMoreData = 1;
	RLPext->Task.ScheduleTimeOffset(&cancelTask, TASK_INTERVAL); //recheck later

	return 0;
}

int VsLoadPluginFromArray(unsigned char *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	if (argsCount != 1) return 0x02;
	
	int pluginLength = *(int*)args[0];
	return VsLoadPlugin(generalArray, pluginLength);
}

int VsSetVolume(unsigned char *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	RLPext->Interrupt.Disable(11);
	VsWriteRegister(0x0B, (generalArray[0] << 8) | generalArray[1]);
	RLPext->Interrupt.Enable(11);
	return 0;
}

int VsSetBassAndTreble(unsigned char *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	if (argsCount != 2) return 1;

	unsigned char bass = *(unsigned char*)args[0];
	signed char treble = *(signed char*)args[1];
	
	unsigned int reg = (treble << 12) | (7 << 8) | (bass << 4) | 10;

	RLPext->Interrupt.Disable(11);
	VsWriteRegister(0x02, reg);
	RLPext->Interrupt.Enable(11);
	return 0;
}

int VsInit(unsigned char *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	if (RLPext->magic != RLP_EXT_MAGIC)
		return 29;

	bufCount = 0;
	if (argsCount > 1) return 2;

	VsInitPorts(); //init I/O
	VsInitSSP1(); //init SSP1

	FIO3CLR = (1UL << 18); //set reset low
	RLPext->Delay(5000); //wait 5ms
	FIO3SET = (1UL << 18); //set reset high

	VsSoftReset();

	//if there is an argument, it's the plugin to load
	if (argsCount == 1)
	{
		int pluginLength = *(int*)args[0];
		unsigned char result = VsLoadPlugin(generalArray, pluginLength);
		if (result != 0) return result;
	}

	VsWriteRegister(0x0B, 0x1515); //set volume
	if (VsReadRegister(0x0B) != 0x1515) return 1;

	RLPext->Task.Initialize(&cancelTask, VsCancelTask, 0, RLP_FALSE);
	RLPext->Task.Initialize(&bufTask, VsTask, 0, RLP_FALSE);

	return 0;
}
