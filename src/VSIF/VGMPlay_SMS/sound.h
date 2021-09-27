#include <stdlib.h>
#include <math.h>
#include <stdbool.h>

#include "SMSlib/SMSlib.h"
#include "PSGlib.h"

#define PRIORITY_HIGH                70
#define PRIORITY_NORMAL              50
#define PRIORITY_LOW                 30

extern unsigned char current_SFX_priority;
extern unsigned char current_SFX_ROM_bank;
extern unsigned char current_tune_ROM_bank;

extern unsigned char current_SFX_priority;
extern unsigned char current_SFX_ROM_bank;
extern unsigned char current_tune_ROM_bank;

extern bool playingPcm;

#define FIRESFX(which,bank,how,prio)                          \
   if((!PSGSFXGetStatus())||((prio)>=current_SFX_priority)){  \
     PSGSFXPlay ((which),(how));                              \
     current_SFX_ROM_bank=bank;                               \
     current_SFX_priority=prio;                               \
   }

extern void PcmInit();
extern void PlayPcmSound(const void *data) __z88dk_fastcall;
