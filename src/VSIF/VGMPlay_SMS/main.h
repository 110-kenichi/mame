#include <stdlib.h>
#include <math.h>
#include <stdbool.h>

#include "SMSlib/SMSlib.h"
#include "Font.h"
#include "Video.h"

typedef enum 
{
  G_PHASE_PLAYER = 1,
} GamePhaseType;

extern GamePhaseType GamePhase;
extern unsigned short PhaseCounter;
extern unsigned short PhaseLocalCounter;
extern unsigned short PhaseLocalCounter2;
extern unsigned short PhaseLocalCounter3;
extern unsigned short PhaseLocalCounter4;

extern volatile bool DisableVDPProcessing;

extern void SMS_write_to_VDPRegister(unsigned char VDPReg, unsigned char value);

extern void PrintWait(unsigned short value);

extern void SetBGPaletteColor(unsigned char entry, unsigned char color);
extern void SetSpritePaletteColor(unsigned char entry, unsigned char color);

extern void FadeInPalette(int count, unsigned char* pal, char pal_size, char forSprite);
extern void FadeOutPalette(int count, unsigned char* pal, char pal_size, char forSprite);

extern void InitGameVars();
