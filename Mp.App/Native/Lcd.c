#include "RLP.h"

unsigned int pin;

int LcdInit(unsigned char *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	if (argsCount != 1) return 2;
	
	pin = *(unsigned int*)args[0];
	if (!RLPext->GPIO.ReservePin(pin, RLP_TRUE)) return 1;
	RLPext->GPIO.EnableOutputMode(pin, RLP_FALSE);

	return 0;
}

int LcdSetLevel(unsigned char *generalArray, void **args, unsigned int argsCount, unsigned int *argSize)
{
	if (argsCount != 1) return 2;

	unsigned char level = *(unsigned char*)args[0] + 1;

	if (level == 33)
	{
		RLPext->GPIO.WritePin(pin, RLP_FALSE);
		return 0;
	}

	RLPext->Interrupt.GlobalInterruptDisable();
	while (level--)
	{
		RLPext->GPIO.WritePin(pin, RLP_FALSE);
		RLPext->Delay(5);
		RLPext->GPIO.WritePin(pin, RLP_TRUE);
		RLPext->Delay(5);
	}
	RLPext->Interrupt.GlobalInterruptEnable();

	return 0;
}