#include <stdlib.h>
#include <math.h>
#include <stdbool.h>

#include "SMSlib/SMSlib.h"
#include "PSGlib.h"
#include "Font.h"
#include "sound.h"
#include "Video.h"
#include "Types.h"

extern const FixedF16 SIN_TABLE[128];

extern unsigned char FreeSpace[0x400];

extern GamePhaseType GamePhase;
extern unsigned short PhaseCounter;
extern unsigned short PhaseLocalCounter;
extern unsigned short PhaseLocalCounter2;
extern unsigned short PhaseLocalCounter3;
extern unsigned short PhaseLocalCounter4;

extern Character Characters[128];

extern volatile bool DisableVDPProcessing;

extern void SMS_write_to_VDPRegister(unsigned char VDPReg, unsigned char value);

extern void PrintWait(unsigned short value);

extern void SetBGPaletteColor(unsigned char entry, unsigned char color);
extern void SetSpritePaletteColor(unsigned char entry, unsigned char color);

extern void FadeInPalette(int count, unsigned char* pal, char pal_size, char forSprite);
extern void FadeOutPalette(int count, unsigned char* pal, char pal_size, char forSprite);

extern void InitGameVars();
