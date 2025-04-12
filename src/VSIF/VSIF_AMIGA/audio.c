
#include "main.h"

struct PcmData pcmDataTable[256];
volatile struct PlayData curPlayData[4] = {0};


void aud_memcpy(volatile struct AudChannel *dest, volatile struct AudChannel *src) {
    UWORD  *d32 = (UWORD *)dest;
    UWORD *s32 = (UWORD *)src;

    // 2バイト単位でコピー
	*d32++ = *s32++;
	*d32++ = *s32++;
	*d32++ = *s32++;
	*d32++ = *s32++;
	*d32++ = *s32++;
}


void reqPlayPcm(UBYTE ch, UBYTE id,  UWORD volume, UWORD period)
{
	if(pcmDataTable[id].dataPtr == 0)
		return;
#ifdef NO_LOOP
	{
		custom->dmacon = (DMAF_AUD0 << ch); // Stop DMA for AUD

		struct PcmData *pcm = &pcmDataTable[id];

		ULONG *audl = (ULONG *)&custom->aud[ch];
		*audl++ = (ULONG)pcm->dataPtr;
		UWORD *aud = (UWORD *)audl;
		*aud++ = pcm->length >> 1;
		*aud++ = period;
		*aud = volume;

		custom->dmacon = DMAF_SETCLR | (DMAF_AUD0 << ch); // Start DMA for AUD
	}
#elif
	{
		struct PcmData *pcm = &pcmDataTable[id];
		struct PlayData pd = {0};
		volatile struct AudChannel *aud = &pd.aud;
		aud->ac_ptr = (UWORD*)pcm->dataPtr;
		aud->ac_len = pcm->length >> 1;
		aud->ac_per = period;
		aud->ac_vol = volume; //64:MAX
		pd.pcm = pcm;
		//pd.playCount = 0;

	#ifdef LOG
		ULONG arg[] = {pcm->length, pcm->loop};
		VWritef("PCM %N %N\n", arg);
	#endif

		// custom->intena = (INTF_AUD0 << ch); // Stop INT for AUD
		// custom->intreq = (INTF_AUD0 << ch); custom->intreq = (INTF_AUD0 << ch);
		custom->dmacon = (DMAF_AUD0 << ch); // Stop DMA for AUD
		//aud_memcpy(&custom->aud[ch], aud);
		custom->aud[ch].ac_len = 2;
		custom->aud[ch].ac_ptr = (UWORD*)StopData;
		custom->aud[ch].ac_vol = 0;
		custom->aud[ch].ac_per = 1;

		curPlayData[ch] = pd;

		//custom->intena = INTF_SETCLR | (INTF_AUD0 << ch); // Start INT for AUD
		custom->dmacon = DMAF_SETCLR | (DMAF_AUD0 << ch); // Start DMA for AUD
	}
#endif
}

/*
*/
void reqStopPcm(UBYTE ch)
{
	// custom->intena = (INTF_AUD0 << ch);
	// custom->intreq = (INTF_AUD0 << ch); custom->intreq = (INTF_AUD0 << ch);

	custom->dmacon = (DMAF_AUD0 << ch); // Stop DMA for AUD

	ULONG *audl = (ULONG *)&custom->aud[ch];
	*audl++ = (ULONG)StopData;
	UWORD *aud = (UWORD *)audl;
	*aud++ = 0;
	*aud++ = 1;
	*aud = 0;
	// custom->dmacon = (DMAF_AUD0 << ch); // Stop DMA for AUD
	// custom->aud[ch].ac_len = 0;
	// custom->aud[ch].ac_ptr = (UWORD*)StopData;
	// custom->aud[ch].ac_vol = 0;
	// custom->aud[ch].ac_per = 1;
}

//https://www.youtube.com/watch?v=EDVRdlnHyoE
static __attribute__((interrupt)) void audioInterruptHandler() {
	UWORD intreqr = custom->intreqr;
	UWORD intreq = 0;
	
	volatile struct PlayData *cpd = curPlayData;
	volatile struct AudChannel *aud = custom->aud;
	//int ch = 0;
	for(int ch=0;ch<4; ch++)
	{
		if(intreqr & (INTF_AUD0 << ch))
		{
			if(cpd->playCount == 0)
			{
				aud_memcpy(&custom->aud[ch], &cpd->aud);
				cpd->playCount++;
			}else if(cpd->playCount == 1)
			{
				volatile struct PcmData *pcm = cpd->pcm;
				if(pcm->loop != 0xFFFF)
				{
					aud->ac_ptr = (volatile UWORD*)(pcm->dataPtr + pcm->loop);
					aud->ac_len = (pcm->length - pcm->loop) >> 1;
				}else{
					cpd->playCount++;
				}
			}else if(cpd->playCount == 2)
			{
				//volatile struct PcmData *pcm = cpd->pcm;
				//custom->intena = (INTF_AUD0 << ch); // Stop INT for AUD
				custom->dmacon = (DMAF_AUD0 << ch); // Stop DMA for AUD
				aud->ac_len = 0;
				aud->ac_ptr = (UWORD*)StopData;
				aud->ac_vol = 0;
				aud->ac_per = 1;
				cpd->playCount++;
			}
			intreq |= (INTF_AUD0 << ch);
		}
		cpd++;
		aud++;
	}
	custom->intreq = intreq; custom->intreq = intreq; //twice for a4000 bug.
}


ULONG MyHookFunction()
{
	VWritef("MIDI Rcv\n", NULL);

    return 0;
}

void InitHook(struct Hook *hook, ULONG (*c_function)(), APTR userdata)
{
	ULONG (*hookEntry)();
	hookEntry = NULL;

	hook->h_Entry	= hookEntry;
    hook->h_SubEntry = c_function;
    hook->h_Data	= userdata;
}

