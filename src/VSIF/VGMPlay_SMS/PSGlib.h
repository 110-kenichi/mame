/* **************************************************
   PSGlib - C programming library for the SEGA PSG
   ( part of devkitSMS - github.com/sverx/devkitSMS )
   ************************************************** */

#define PSG_STOPPED         0
#define PSG_PLAYING         1

#define SFX_CHANNEL2        #0x01
#define SFX_CHANNEL3        #0x02
#define SFX_CHANNELS2AND3   SFX_CHANNEL2|SFX_CHANNEL3

void PSGPlay (void *song);
void PSGCancelLoop (void);
void PSGPlayNoRepeat (void *song);
void PSGStop (void);
void PSGResume (void);
unsigned char PSGGetStatus (void);
void PSGSetMusicVolumeAttenuation (unsigned char attenuation);

void PSGSFXPlay (void *sfx, unsigned char channels);
void PSGSFXPlayLoop (void *sfx, unsigned char channels);
void PSGSFXCancelLoop (void);
void PSGSFXStop (void);
unsigned char PSGSFXGetStatus (void);

void PSGSilenceChannels (void);
void PSGRestoreVolumes (void);

void PSGFrame (void);
void PSGSFXFrame (void);


extern unsigned char PSGChan0Volume;              // the volume for channel 0
extern unsigned char PSGChan1Volume;              // the volume for channel 1
extern unsigned char PSGChan2Volume;              // the volume for channel 2
extern unsigned char PSGChan3Volume;              // the volume for channel 3
extern unsigned char PSGChan0HighTone;            // the high tone bits for channel 0
extern unsigned char PSGChan1HighTone;            // the high tone bits for channel 1
extern unsigned char PSGChan2HighTone;            // the high tone bits for channel 2
extern unsigned char PSGChan0LowTone;             // the low tone bits for channels 0
extern unsigned char PSGChan1LowTone;             // the low tone bits for channels 1
extern unsigned char PSGChan2LowTone;             // the low tone bits for channels 2
extern unsigned char PSGChan3LowTone;             // the tone bits for channels 3