// copyright-holders:K.Ito
/***************************************************************************



****************************************************************************/

#include "emu.h"
#include "cm32p.h"
#include "..\..\FluidLite\include\fluidlite.h"
#include "cm32p_BReverbModel.h"

// device type definition
DEFINE_DEVICE_TYPE(CM32P, cm32p_device, "cm32p", "CM32P")


//**************************************************************************
//  LIVE DEVICE
//**************************************************************************

//-------------------------------------------------
//  beep_device - constructor
//-------------------------------------------------

cm32p_device::cm32p_device(const machine_config &mconfig, const char *tag, device_t *owner, uint32_t clock)
	: device_t(mconfig, CM32P, tag, owner, clock)
	, device_sound_interface(mconfig, *this)
	, m_stream(nullptr)
	, m_frequency(clock)
	, cm32p_ram(*new MemParams)
	, memory_initialized(0)
{
	initMemoryRegions();

	for (int mode = ReverbMode::REVERB_MODE_ROOM; mode <= ReverbMode::REVERB_MODE_TAP_DELAY; mode++) {
		reverbModels[mode] = CM32P_BReverbModel::createBReverbModel(ReverbMode(mode), false, RendererType::REVERB_FLOAT);
		reverbModels[mode]->open();
	}
}

void cm32p_device::set_enable(int enable)
{
	if (m_enable != enable)
	{
		if (enable != 0)
		{
			synth_rev_off = new_fluid_synth(settings);
			synth_rev_on = new_fluid_synth(settings);

			//fluid_synth_set_polyphony(synth, 31);
			fluid_synth_set_reverb_on(synth_rev_off, 0);
			fluid_synth_set_reverb_on(synth_rev_on, 0);
			fluid_synth_set_chorus_on(synth_rev_off, 0);
			fluid_synth_set_chorus_on(synth_rev_on, 0);
			clipping_overflow_l = 0.0f;
			clipping_overflow_r = 0.0f;
		}
		else
		{
			delete_fluid_synth(synth_rev_off);
			delete_fluid_synth(synth_rev_on);
			//fluid_synth_system_reset(synth_rev_off);
			//fluid_synth_system_reset(synth_rev_on);
		}
		m_enable = enable;
	}
}

void cm32p_device::initialize_memory()
{
	fluid_synth_system_reset(synth_rev_off);
	fluid_synth_system_reset(synth_rev_on);

	for (int mode = ReverbMode::REVERB_MODE_ROOM; mode <= ReverbMode::REVERB_MODE_TAP_DELAY; mode++) {
		reverbModels[mode]->mute();
	}
	//system params
	cm32p_ram.system.masterTune = 0x4A;
	cm32p_ram.system.reverbMode = ReverbMode::REVERB_MODE_HALL;
	cm32p_ram.system.reverbTime = 5;
	cm32p_ram.system.reverbLevel = 5;

	reverbModels[cm32p_ram.system.reverbMode]->setParameters(cm32p_ram.system.reverbTime, cm32p_ram.system.reverbLevel);

	cm32p_ram.system.reserveSettings[0] = 2;
	cm32p_ram.system.reserveSettings[1] = 8;
	cm32p_ram.system.reserveSettings[2] = 21;
	cm32p_ram.system.reserveSettings[3] = 0;
	cm32p_ram.system.reserveSettings[4] = 0;
	cm32p_ram.system.reserveSettings[5] = 0;
	for (int i = 6; i < 16; i++)
		cm32p_ram.system.reserveSettings[i] = 0;

	cm32p_ram.system.chanAssign[0] = 10;
	cm32p_ram.system.chanAssign[1] = 11;
	cm32p_ram.system.chanAssign[2] = 12;
	cm32p_ram.system.chanAssign[3] = 13;
	cm32p_ram.system.chanAssign[4] = 14;
	cm32p_ram.system.chanAssign[5] = 15;
	for (int i = 0; i < 10; i++)
		cm32p_ram.system.chanAssign2[i] = 16;

	cm32p_ram.system.masterVol = 100;
	fluid_synth_set_gain(synth_rev_off, 100.f / DEFAULT_GAIN_BASE);
	fluid_synth_set_gain(synth_rev_on, 100.f / DEFAULT_GAIN_BASE);

	//patch temp
	for (int i = 0; i < 16; i++) {
		MemParams::PatchTemp *patchTemp = &cm32p_ram.patchTemp[i];

		// Note that except for the rhythm part, these patch fields will be set in setProgram() below anyway.
		patchTemp->patch.toneMedia = ToneMedia::TONE_MEDIA_INTERNAL;
		switch (i)
		{
		case 0:
			patchTemp->patch.toneNumber = 40;	// Fretless 1
			break;
		case 1:
			patchTemp->patch.toneNumber = 43;	// Choir 1
			break;
		case 2:
			patchTemp->patch.toneNumber = 0;	// A.Piano 1
			break;
		case 3:
			patchTemp->patch.toneNumber = 51;	// E.Oegan 1
			break;
		case 4:
			patchTemp->patch.toneNumber = 20;	// E.Guitar 1
			break;
		case 5:
			patchTemp->patch.toneNumber = 64;	// Soft Tp 1
			break;
		default:
			patchTemp->patch.toneNumber = 0;
			break;
		}
		if (i < 6)
			program_select(cm32p_ram.system.chanAssign[i], patchTemp->patch.toneMedia, 0, patchTemp->patch.toneNumber);
		else
			program_select(cm32p_ram.system.chanAssign2[i - 6], patchTemp->patch.toneMedia, 0, patchTemp->patch.toneNumber);

		patchTemp->patch.keyShift = 12;
		patchTemp->patch.fineTune = 50;
		patchTemp->patch.benderRange = 12;
		patchTemp->patch.assignMode = 0;
		patchTemp->patch.reverbSwitch = 1;
		patchTemp->patch.velocitySense = 15;
		patchTemp->patch.envAttackRate = 64;
		patchTemp->patch.envReleaseRate = 64;
		patchTemp->patch.lfoRate = 50;
		patchTemp->patch.lfoAutoDelayTime = 0;
		patchTemp->patch.lfoAutoRiseTime = 0;
		patchTemp->patch.lfoAutoDepth = 0;
		patchTemp->patch.lfoManRiseTime = 2;
		patchTemp->patch.lfoManDepth = 4;
		patchTemp->patch.detuneDepth = 12;

		memset(bank_no, 0, sizeof(bank_no));
		memset(rpn_lsb, 0, sizeof(rpn_lsb));
		memset(rpn_msb, 0, sizeof(rpn_msb));
		memset(rpn_data, 0, sizeof(rpn_data));
		memset(rpn_type, 0, sizeof(rpn_type));

		switch (i)
		{
		case 0:
			patchTemp->panpot = 64;
			break;
		case 1:
			patchTemp->panpot = 81;
			break;
		case 2:
			patchTemp->panpot = 64;
			break;
		case 3:
			patchTemp->panpot = 99;
			break;
		case 4:
			patchTemp->panpot = 27;
			break;
		case 5:
			patchTemp->panpot = 45;
			break;
		default:
			patchTemp->panpot = 64;
			break;
		}
		if (i < 6)
		{
			fluid_synth_cc(synth_rev_off, cm32p_ram.system.chanAssign[i], 10, 127 - patchTemp->panpot);
			fluid_synth_cc(synth_rev_on, cm32p_ram.system.chanAssign[i], 10, 127 - patchTemp->panpot);
		}
		else
		{
			fluid_synth_cc(synth_rev_off, cm32p_ram.system.chanAssign2[i - 6], 10, 127 - patchTemp->panpot);
			fluid_synth_cc(synth_rev_on, cm32p_ram.system.chanAssign2[i - 6], 10, 127 - patchTemp->panpot);
		}
		patchTemp->outputLevel = 100;
		if (i < 6)
		{
			fluid_synth_cc(synth_rev_off, cm32p_ram.system.chanAssign[i], 7, (int)roundf((float)127 * ((float)patchTemp->outputLevel / (float)100)));
			fluid_synth_cc(synth_rev_on, cm32p_ram.system.chanAssign[i], 7, (int)roundf((float)127 * ((float)patchTemp->outputLevel / (float)100)));
		}
		else
		{
			fluid_synth_cc(synth_rev_off, cm32p_ram.system.chanAssign2[i - 6], 7, (int)roundf((float)127 * ((float)patchTemp->outputLevel / (float)100)));
			fluid_synth_cc(synth_rev_on, cm32p_ram.system.chanAssign2[i - 6], 7, (int)roundf((float)127 * ((float)patchTemp->outputLevel / (float)100)));
		}
	}

	//patch
	for (int i = 0; i < 128; i++) {
		PatchParam *patch = &cm32p_ram.patches[i];

		patch->toneMedia = cm32p_device::default_patch_table_media[i];
		patch->toneNumber = cm32p_device::default_patch_table_no[i];

		patch->keyShift = 12;
		patch->fineTune = 50;
		patch->benderRange = 12;
		patch->assignMode = 0;
		patch->reverbSwitch = 1;
		patch->velocitySense = 15;
		patch->envAttackRate = 64;
		patch->envReleaseRate = 64;
		patch->lfoRate = 50;
		patch->lfoAutoDelayTime = 0;
		patch->lfoAutoRiseTime = 0;
		patch->lfoAutoDepth = 0;
		patch->lfoManRiseTime = 2;
		patch->lfoManDepth = 4;
		patch->detuneDepth = 12;
	}

	memory_initialized = 1;
}

//-------------------------------------------------
//  device_start - device-specific startup
//-------------------------------------------------

void cm32p_device::device_start()
{
	m_stream = stream_alloc(0, 2, machine().sample_rate());
	m_enable = 0;
	m_passthru = 0;

	settings = new_fluid_settings();
	fluid_settings_setnum(settings, "synth.sample-rate", machine().sample_rate());
	//synth_rev_off = new_fluid_synth(settings);
	//synth_rev_on = new_fluid_synth(settings);
}

//-------------------------------------------------
//  sound_stream_update - handle a stream update
//-------------------------------------------------

void cm32p_device::sound_stream_update(sound_stream &stream, stream_sample_t **inputs, stream_sample_t **outputs, int samples)
{
	stream_sample_t *buffer1 = outputs[0];
	stream_sample_t *buffer2 = outputs[1];

	if (memory_initialized == 0)
	{
		memset(buffer1, 0, samples * sizeof(*buffer1));
		memset(buffer2, 0, samples * sizeof(*buffer2));
		return;
	}

	float *ptr = (float *)malloc(sizeof(float) * samples * 6);
	float *buf1 = ptr;
	float *buf2 = buf1 + samples;
	float *rbuf1 = buf2 + samples;
	float *rbuf2 = rbuf1 + samples;
	float *nbuf1 = rbuf2 + samples;
	float *nbuf2 = nbuf1 + samples;

	fluid_synth_write_float(synth_rev_on, samples, buf1, 0, 1, buf2, 0, 1);
	reverbModels[cm32p_ram.system.reverbMode]->process(buf1, buf2, rbuf1, rbuf2, samples);

	fluid_synth_write_float(synth_rev_off, samples, nbuf1, 0, 1, nbuf2, 0, 1);

	while (samples-- > 0)
	{
		float outl = *buf1++ + *rbuf1++ + *nbuf1++;
		float outr = *buf2++ + *rbuf2++ + *nbuf2++;
		/*
		outl += clipping_overflow_l;
		clipping_overflow_l = 0;
		if (outl > 1.0f) {
			clipping_overflow_l = outl - 1.0f;
			outl = 1.0f;
		} else if (outl < -1.0f) {
			clipping_overflow_l = outl + 1.0f;
			outl = -1.0f;
		}
		outr += clipping_overflow_r;
		clipping_overflow_r = 0;
		if (outr > 1.0f) {
			clipping_overflow_r = outr - 1.0f;
			outr = 1.0f;
		} else if (outr < -1.0f) {
			clipping_overflow_r = outr + 1.0f;
			outr = -1.0f;
		}*/
		*buffer1++ = ((stream_sample_t)(outl * 32767.f * 3.f));
		*buffer2++ = ((stream_sample_t)(outr * 32767.f * 3.f));
	}

	free(ptr);
}


fluid_sfont_t *cm32p_device::load_sf(u8 card_id, const char* filename)
{
	if (sf_table.count(card_id) != 0) {
		fluid_synth_sfunload(synth_rev_off, sf_table[card_id], 0);
		fluid_synth_sfunload(synth_rev_on, sf_table[card_id], 0);
	}

	sf_table[card_id] = fluid_synth_sfload(synth_rev_off, filename, 0);
	fluid_sfont_t* sf = fluid_synth_get_sfont_by_id(synth_rev_off, sf_table[card_id]);
	fluid_synth_add_sfont(synth_rev_on, sf);

	return sf;
}


void cm32p_device::add_sf(u8 card_id, fluid_sfont_t *sf)
{
	if (sf_table.count(card_id) != 0) {
		fluid_synth_sfunload(synth_rev_off, sf_table[card_id], 0);
		fluid_synth_sfunload(synth_rev_on, sf_table[card_id], 0);
	}


	fluid_sfont_t* tsf = fluid_synth_get_sfont_by_id(synth_rev_off, sf->id);
	if (tsf == 0)
	{
		sf_table[card_id] = fluid_synth_add_sfont(synth_rev_off, sf);
		fluid_synth_add_sfont(synth_rev_on, sf);
	}
	else
	{
		sf_table[card_id] = sf->id;
	}
}

void cm32p_device::set_tone(u8 card_id, u16 tone_no, u16 sf_preset_no)
{
	tone_table[card_id << 16 | tone_no] = sf_preset_no;
}

void cm32p_device::set_card(u8 id)
{
	if (card_id != id)
	{
		if (id >= 16)
		{
			for (int i = 0; i < 128; i++) {
				PatchParam *patch = &cm32p_ram.patches[i];
				patch->toneMedia = 1;
				patch->toneNumber = i;
			}
		}
		else 
		{
			if (card_id >= 16)
			{
				for (int i = 0; i < 128; i++) {
					PatchParam *patch = &cm32p_ram.patches[i];

					patch->toneMedia = cm32p_device::default_patch_table_media[i];
					patch->toneNumber = cm32p_device::default_patch_table_no[i];
				}
			}
		}
		card_id = id;
	}
}

void cm32p_device::set_chanAssign(u8* assign) {
	for (int i = 0; i < 16; i++)
	{
		if (i < 6)
			cm32p_ram.system.chanAssign[i] = assign[i];
		else if (i >= 6)
			cm32p_ram.system.chanAssign2[i - 6] = assign[i];
	}
}

void cm32p_device::get_chanAssign(u8* assign) {
	for (int i = 0; i < 16; i++)
	{
		if (i < 6)
			assign[i] = cm32p_ram.system.chanAssign[i];
		else if (i >= 6)
			assign[i] = cm32p_ram.system.chanAssign2[i - 6];
	}
}


void cm32p_device::program_select(u8 channel, u8 tone_media, u8 bank_no, u8 tone_no)
{
	u8 cid = card_id;
	if (tone_media == 0)
		cid = 0;
	u16 sf_preset_no = tone_table[cid << 16 | bank_no << 8 | tone_no];
	fluid_synth_program_select(synth_rev_off, channel, sf_table[cid], (sf_preset_no >> 8) & 0xff, sf_preset_no & 0xff);
	fluid_synth_program_select(synth_rev_on, channel, sf_table[cid], (sf_preset_no >> 8) & 0xff, sf_preset_no & 0xff);
}

void cm32p_device::play_msg(u8 type, u8 channel, u32 param1, u32 param2)
{
	for (int i = 0; i < 16; i++)
	{
		if (i < 6 && cm32p_ram.system.chanAssign[i] != channel)
			continue;
		if (i >= 6 && cm32p_ram.system.chanAssign2[i - 6] != channel)
			continue;

		switch (type)
		{
		case 0x80:
			fluid_synth_noteoff(synth_rev_off, channel, param1);
			fluid_synth_noteoff(synth_rev_on, channel, param1);
			break;
		case 0x90:
		{
			MemParams::PatchTemp pt = cm32p_ram.patchTemp[i];
			PatchParam pp = pt.patch;
			if (pp.keyShift > 24)
				pp.keyShift = 24;
			int key = param1;
			key -= pp.keyShift - 12;
			if (key < 0)
				key = 0;
			else if (key > 127)
				key = 127;
			if (pp.reverbSwitch == 0) {
				fluid_synth_noteon(synth_rev_off, channel, key, param2);
			}
			else {
				fluid_synth_noteon(synth_rev_on, channel, key, param2);
			}
		}
		break;
		case 0xb0:
		{
			fluid_synth_cc(synth_rev_off, channel, param1, param2);
			fluid_synth_cc(synth_rev_on, channel, param1, param2);
			MemParams::PatchTemp pt = cm32p_ram.patchTemp[i];
			PatchParam pp = pt.patch;
			switch (param1)
			{
			case 0:
				bank_no[channel] = param2;
				break;
			case 6:
				if (rpn_lsb[channel] == 0 && rpn_msb[channel] == 0)
				{
					switch (rpn_type[channel])
					{
					case 2:
						pp.benderRange = rpn_data[channel];
						fluid_synth_pitch_wheel_sens(synth_rev_off, channel, rpn_data[channel]);
						fluid_synth_pitch_wheel_sens(synth_rev_on, channel, rpn_data[channel]);
						break;
					}
				}
				break;
			case 7:
				pt.outputLevel = u8(param2 * 100 / 127);
				break;
			case 10:
				pt.panpot = param2;
				break;
			case 98:
				rpn_type[channel] = 1;
				break;
			case 99:
				rpn_type[channel] = 1;
				break;
			case 100:
				rpn_lsb[channel] = u8(param2);
				rpn_type[channel] = 2;
				break;
			case 101:
				rpn_msb[channel] = u8(param2);
				rpn_type[channel] = 2;
				break;
			}
		}
		break;
		case 0xc0:
		{
			cm32p_ram.patchTemp[i].patch = cm32p_ram.patches[param1];
			MemParams::PatchTemp pt = cm32p_ram.patchTemp[i];
			PatchParam pp = pt.patch;
			applyPatchParameters(channel, pp, pt);
		}
		break;
		case 0xe0:
			fluid_synth_pitch_bend(synth_rev_off, channel, (param2 << 7) | param1);
			fluid_synth_pitch_bend(synth_rev_on, channel, (param2 << 7) | param1);
			break;
		}
	}
}

void cm32p_device::applyPatchParameters(const u8 &channel, PatchParam &pp, MemParams::PatchTemp &pt)
{
	if (pt.outputLevel > 100)
		pt.outputLevel = 100;
	if (pt.patch.toneMedia > 1)
		pt.patch.toneMedia = 1;

	program_select(channel, pp.toneMedia, bank_no[channel], pp.toneNumber);
	fluid_synth_pitch_wheel_sens(synth_rev_off, channel, pp.benderRange);
	fluid_synth_pitch_wheel_sens(synth_rev_on, channel, pp.benderRange);
	fluid_synth_cc(synth_rev_off, channel, 10, 127 - pt.panpot);
	fluid_synth_cc(synth_rev_on, channel, 10, 127 - pt.panpot);
	fluid_synth_cc(synth_rev_off, channel, 7, 127 * pt.outputLevel / 100);
	fluid_synth_cc(synth_rev_on, channel, 7, 127 * pt.outputLevel / 100);
	fluid_synth_set_gen(synth_rev_off, channel, GEN_FINETUNE, pp.fineTune - 50);
	fluid_synth_set_gen(synth_rev_on, channel, GEN_FINETUNE, pp.fineTune - 50);

	//8000.0f,  -12000.0f
	{
		float ar = 0;
		if (pp.envAttackRate < 64)
			ar = 8000.f * ((64.f - pp.envAttackRate) / 64.f);
		else if (pp.envAttackRate > 64)
			ar = -12000.f * ((pp.envAttackRate - 64.f) / 64.f);
		fluid_synth_set_gen(synth_rev_off, channel, GEN_VOLENVATTACK, ar);
		fluid_synth_set_gen(synth_rev_on, channel, GEN_VOLENVATTACK, ar);
	}
	{
		float rr = 0;
		if (pp.envReleaseRate < 64)
			rr = 8000.f * ((64.f - pp.envReleaseRate) / 64.f);
		else if (pp.envReleaseRate > 64)
			rr = -12000.f * ((pp.envReleaseRate - 64.f) / 64.f);
		fluid_synth_set_gen(synth_rev_off, channel, GEN_VOLENVRELEASE, rr);
		fluid_synth_set_gen(synth_rev_on, channel, GEN_VOLENVRELEASE, rr);
	}
	//-16000.0f,   4500.0f,       0.0f 
	{
		float lr = 0;
		if (pp.lfoRate < 50)
			lr = -10000.f  * ((50.f - pp.lfoRate) / 50.f);
		else if (pp.lfoRate > 50)
			lr = 2000.f * ((pp.lfoRate - 50.f) / 77.f);
		fluid_synth_set_gen(synth_rev_off, channel, GEN_MODLFOFREQ, lr);
		fluid_synth_set_gen(synth_rev_on, channel, GEN_MODLFOFREQ, lr);
	}
	//-12000.0f,  12000.0f,       0.0f
	{
		if (pp.lfoAutoDepth > 15)
			pp.lfoAutoDepth = 15;
		float ld = 250.0f * (pp.lfoAutoDepth / 15.f);
		fluid_synth_set_gen(synth_rev_off, channel, GEN_MODLFOTOPITCH, ld);
		fluid_synth_set_gen(synth_rev_on, channel, GEN_MODLFOTOPITCH, ld);
	}
	//-12000.0f,  12000.0f,       0.0f
	{
		if (pp.lfoAutoDelayTime > 15)
			pp.lfoAutoDelayTime = 15;
		float ld = 12000.0f * (pp.lfoAutoDelayTime / 15.f);
		fluid_synth_set_gen(synth_rev_off, channel, GEN_MODLFODELAY, ld);
		fluid_synth_set_gen(synth_rev_on, channel, GEN_MODLFODELAY, ld);
	}
}

void cm32p_device::play_sysex(const u8 *sysex, u32 len)
{
	if (len < 2) {
		//printDebug("playSysex: Message is too short for sysex (%d bytes)", len);
		return;
	}
	if (sysex[0] != 0xF0) {
		//printDebug("playSysex: Message lacks start-of-sysex (0xF0)");
		return;
	}
	// Due to some programs (e.g. Java) sending buffers with junk at the end, we have to go through and find the end marker rather than relying on len.
	u32 endPos;
	for (endPos = 1; endPos < len; endPos++) {
		if (sysex[endPos] == 0xF7) {
			break;
		}
	}
	if (endPos == len) {
		//printDebug("playSysex: Message lacks end-of-sysex (0xf7)");
		return;
	}
	playSysexWithoutFraming(sysex + 1, endPos - 1);
}

void cm32p_device::playSysexWithoutFraming(const u8 *sysex, u32 len)
{
	if (len < 4) {
		//printDebug("playSysexWithoutFraming: Message is too short (%d bytes)!", len);
		return;
	}
	if (sysex[0] != SYSEX_MANUFACTURER_ROLAND) {
		//printDebug("playSysexWithoutFraming: Header not intended for this device manufacturer: %02x %02x %02x %02x", int(sysex[0]), int(sysex[1]), int(sysex[2]), int(sysex[3]));
		return;
	}
	else if (sysex[2] != SYSEX_MDL_CM32P) {
		//printDebug("playSysexWithoutFraming: Header not intended for model MT-32: %02x %02x %02x %02x", int(sysex[0]), int(sysex[1]), int(sysex[2]), int(sysex[3]));
		return;
	}
	playSysexWithoutHeader(sysex[1], sysex[3], sysex + 4, len - 4);
}

void cm32p_device::playSysexWithoutHeader(u8 device, u8 command, const u8 *sysex, u32 len) {
	if (device > 0x10) {
		// We have device ID 0x10 (default, but changeable, on real MT-32), < 0x10 is for channels
		//printDebug("playSysexWithoutHeader: Message is not intended for this device ID (provided: %02x, expected: 0x10 or channel)", int(device));
		return;
	}
	// This is checked early in the real devices (before any sysex length checks or further processing)
	// FIXME: Response to SYSEX_CMD_DAT reset with partials active (and in general) is untested.
	if ((command == SYSEX_CMD_DT1 || command == SYSEX_CMD_DAT) && sysex[0] == 0x7F) {
		initialize_memory();
		return;
	}

	if (command == SYSEX_CMD_EOD) {
#if MT32EMU_MONITOR_SYSEX > 0
		printDebug("playSysexWithoutHeader: Ignored unsupported command %02x", command);
#endif
		return;
	}
	if (len < 4) {
		//printDebug("playSysexWithoutHeader: Message is too short (%d bytes)!", len);
		return;
	}
	u8 checksum = calcSysexChecksum(sysex, len - 1);
	if (checksum != sysex[len - 1]) {
		//printDebug("playSysexWithoutHeader: Message checksum is incorrect (provided: %02x, expected: %02x)!", sysex[len - 1], checksum);
		return;
}
	len -= 1; // Exclude checksum
	switch (command) {
	case SYSEX_CMD_WSD:
#if MT32EMU_MONITOR_SYSEX > 0
		printDebug("playSysexWithoutHeader: Ignored unsupported command %02x", command);
#endif
		break;
	case SYSEX_CMD_DAT:
		/* Outcommented until we (ever) actually implement handshake communication
		if (hasActivePartials()) {
		printDebug("playSysexWithoutHeader: Got SYSEX_CMD_DAT but partials are active - ignoring");
		// FIXME: We should send SYSEX_CMD_RJC in this case
		break;
		}
		*/
		// Fall-through
	case SYSEX_CMD_DT1:
		writeSysex(device, sysex, len);
		break;
	case SYSEX_CMD_RQD:
		//if (hasActivePartials()) {
		//	//printDebug("playSysexWithoutHeader: Got SYSEX_CMD_RQD but partials are active - ignoring");
		//	// FIXME: We should send SYSEX_CMD_RJC in this case
		//	break;
		//}
		// Fall-through
	case SYSEX_CMD_RQ1:
		//readSysex(device, sysex, len);
		break;
	default:
		//printDebug("playSysexWithoutHeader: Unsupported command %02x", command);
		return;
	}
	}

void cm32p_device::writeSysex(u8 device, const u8 *sysex, u32 len)
{
	//if (!opened) return;
	//reportHandler->onMIDIMessagePlayed();
	u32 addr = (sysex[0] << 16) | (sysex[1] << 8) | (sysex[2]);
	addr = CM32P_MEMADDR(addr);
	sysex += 3;
	len -= 3;
	//printDebug("Sysex addr: 0x%06x", MT32EMU_SYSEXMEMADDR(addr));
	// NOTE: Please keep both lower and upper bounds in each check, for ease of reading

	writeSysexGlobal(addr, sysex, len);
}

// Process device-global sysex (possibly converted from channel-specific sysex above)
void cm32p_device::writeSysexGlobal(u32 addr, const u8 *sysex, u32 len) {
	for (;;) {
		// Find the appropriate memory region
		const MemoryRegion *region = findMemoryRegion(addr);

		if (region == NULL) {
			//printDebug("Sysex write to unrecognised address %06x, len %d", MT32EMU_SYSEXMEMADDR(addr), len);
			break;
		}
		writeMemoryRegion(region, addr, region->getClampedLen(addr, len), sysex);

		u32 next = region->next(addr, len);
		if (next == 0) {
			break;
		}
		addr += next;
		sysex += next;
		len -= next;
	}
}


void cm32p_device::readMemory(u32 addr, u32 len, u8 *data) {
	//if (!opened) return;
	const MemoryRegion *region = findMemoryRegion(addr);
	if (region != NULL) {
		readMemoryRegion(region, addr, len, data);
	}
}

void cm32p_device::initMemoryRegions() {

	patchTempMemoryRegion = new PatchTempMemoryRegion(this, reinterpret_cast<u8 *>(&cm32p_ram.patchTemp[0]), 0);	//TODO: mami maxtable
	patchesMemoryRegion = new PatchesMemoryRegion(this, reinterpret_cast<u8 *>(&cm32p_ram.patches[0]), 0);			//
	systemMemoryRegion = new SystemMemoryRegion(this, reinterpret_cast<u8 *>(&cm32p_ram.system), 0);				//
	resetMemoryRegion = new ResetMemoryRegion(this);
}

void cm32p_device::deleteMemoryRegions() {
	delete patchTempMemoryRegion;
	patchTempMemoryRegion = NULL;
	delete patchesMemoryRegion;
	patchesMemoryRegion = NULL;
	systemMemoryRegion = NULL;
	delete resetMemoryRegion;
	resetMemoryRegion = NULL;
}

MemoryRegion *cm32p_device::findMemoryRegion(u32 addr) {
	MemoryRegion *regions[] = {
		patchTempMemoryRegion,
		patchesMemoryRegion,
		systemMemoryRegion,
		resetMemoryRegion,
		NULL
	};
	for (int pos = 0; regions[pos] != NULL; pos++) {
		if (regions[pos]->contains(addr)) {
			return regions[pos];
		}
	}
	return NULL;
}

void cm32p_device::readMemoryRegion(const MemoryRegion *region, u32 addr, u32 len, u8 *data) {
	unsigned int first = region->firstTouched(addr);
	//unsigned int last = region->lastTouched(addr, len);
	unsigned int off = region->firstTouchedOffset(addr);
	len = region->getClampedLen(addr, len);

	unsigned int m;

	if (region->isReadable()) {
		region->read(first, off, data, len);
	}
	else {
		// FIXME: We might want to do these properly in future
		for (m = 0; m < len; m += 2) {
			data[m] = 0xff;
			if (m + 1 < len) {
				data[m + 1] = u8(region->type);
			}
		}
	}
}

void cm32p_device::writeMemoryRegion(const MemoryRegion *region, u32 addr, u32 len, const u8 *data) {
	unsigned int first = region->firstTouched(addr);
	unsigned int last = region->lastTouched(addr, len);
	unsigned int off = region->firstTouchedOffset(addr);
	switch (region->type) {
	case MR_PatchTemp:
		region->write(first, off, data, len);
		//printDebug("Patch temp: Patch %d, offset %x, len %d", off/16, off % 16, len);

		for (unsigned int i = first; i <= last; i++) {
			u8 channel = 0;
			if (i < 6)
				channel = cm32p_ram.system.chanAssign[i];
			else if (i >= 6)
				channel = cm32p_ram.system.chanAssign2[i - 6];

			MemParams::PatchTemp pt = cm32p_ram.patchTemp[i];
			PatchParam pp = pt.patch;

			applyPatchParameters(channel, pp, pt);
		}
		break;
	case MR_Patches:
		region->write(first, off, data, len);
#if MT32EMU_MONITOR_SYSEX > 0
		for (unsigned int i = first; i <= last; i++) {
			PatchParam *patch = &cm32p_ram.patches[i];
			int patchAbsTimbreNum = patch->timbreGroup * 64 + patch->timbreNum;
			char instrumentName[11];
			memcpy(instrumentName, cm32p_ram.timbres[patchAbsTimbreNum].timbre.common.name, 10);
			instrumentName[10] = 0;
			u8 *n = reinterpret_cast<u8 *>(patch);
			printDebug("WRITE-PATCH (%d-%d@%d..%d): %d; timbre=%d (%s) %02X%02X%02X%02X%02X%02X%02X%02X", first, last, off, off + len, i, patchAbsTimbreNum, instrumentName, n[0], n[1], n[2], n[3], n[4], n[5], n[6], n[7]);
		}
#endif
		break;
	case MR_System:
		region->write(0, off, data, len);

		//reportHandler->onDeviceReconfig();
		// FIXME: We haven't properly confirmed any of this behaviour
		// In particular, we tend to reset things such as reverb even if the write contained
		// the same parameters as were already set, which may be wrong.
		// On the other hand, the real thing could be resetting things even when they aren't touched
		// by the write at all.
#if MT32EMU_MONITOR_SYSEX > 0
		printDebug("WRITE-SYSTEM:");
#endif

		if (off <= SYSTEM_MASTER_TUNE_OFF && off + len > SYSTEM_MASTER_TUNE_OFF) {
			//TODO: mami refreshSystemMasterTune();
		}
		if (off <= SYSTEM_REVERB_LEVEL_OFF && off + len > SYSTEM_REVERB_MODE_OFF) {
			reverbModels[cm32p_ram.system.reverbMode]->setParameters(cm32p_ram.system.reverbTime, cm32p_ram.system.reverbLevel);
		}
		if (off <= SYSTEM_RESERVE_SETTINGS_END_OFF && off + len > SYSTEM_RESERVE_SETTINGS_START_OFF) {
			//TODO: mami refreshSystemReserveSettings();
		}
		if (off <= SYSTEM_CHAN_ASSIGN_END_OFF && off + len > SYSTEM_CHAN_ASSIGN_START_OFF) {
			int firstPart = off - SYSTEM_CHAN_ASSIGN_START_OFF;
			if (firstPart < 0)
				firstPart = 0;
			int lastPart = off + len - SYSTEM_CHAN_ASSIGN_START_OFF;
			if (lastPart > 8)
				lastPart = 8;
			//TODO: mami refreshSystemChanAssign(u8(firstPart), u8(lastPart));
		}
		if (off <= SYSTEM_MASTER_VOL_OFF && off + len > SYSTEM_MASTER_VOL_OFF) {
			if (cm32p_ram.system.masterVol > 100)
				cm32p_ram.system.masterVol = 100;
			fluid_synth_set_gain(synth_rev_off, (float)cm32p_ram.system.masterVol / DEFAULT_GAIN_BASE);
			fluid_synth_set_gain(synth_rev_on, (float)cm32p_ram.system.masterVol / DEFAULT_GAIN_BASE);
		}

		break;
	case MR_Reset:
		initialize_memory();
		break;
	}
}


void MemoryRegion::read(unsigned int entry, unsigned int off, u8 *dst, unsigned int len) const {
	off += entry * entrySize;
	// This method should never be called with out-of-bounds parameters,
	// or on an unsupported region - seeing any of this debug output indicates a bug in the emulator
	if (off > entrySize * entries - 1) {
#if MT32EMU_MONITOR_SYSEX > 0
		synth->printDebug("read[%d]: parameters start out of bounds: entry=%d, off=%d, len=%d", type, entry, off, len);
#endif
		return;
}
	if (off + len > entrySize * entries) {
#if MT32EMU_MONITOR_SYSEX > 0
		synth->printDebug("read[%d]: parameters end out of bounds: entry=%d, off=%d, len=%d", type, entry, off, len);
#endif
		len = entrySize * entries - off;
}
	u8 *src = getRealMemory();
	if (src == NULL) {
#if MT32EMU_MONITOR_SYSEX > 0
		synth->printDebug("read[%d]: unreadable region: entry=%d, off=%d, len=%d", type, entry, off, len);
#endif
		return;
	}
	memcpy(dst, src + off, len);
}

void MemoryRegion::write(unsigned int entry, unsigned int off, const u8 *src, unsigned int len, bool init) const {
	unsigned int memOff = entry * entrySize + off;
	// This method should never be called with out-of-bounds parameters,
	// or on an unsupported region - seeing any of this debug output indicates a bug in the emulator
	if (off > entrySize * entries - 1) {
#if MT32EMU_MONITOR_SYSEX > 0
		synth->printDebug("write[%d]: parameters start out of bounds: entry=%d, off=%d, len=%d", type, entry, off, len);
#endif
		return;
}
	if (off + len > entrySize * entries) {
#if MT32EMU_MONITOR_SYSEX > 0
		synth->printDebug("write[%d]: parameters end out of bounds: entry=%d, off=%d, len=%d", type, entry, off, len);
#endif
		len = entrySize * entries - off;
	}
	u8 *dest = getRealMemory();
	if (dest == NULL) {
#if MT32EMU_MONITOR_SYSEX > 0
		synth->printDebug("write[%d]: unwritable region: entry=%d, off=%d, len=%d", type, entry, off, len);
#endif
		return;
	}

	for (unsigned int i = 0; i < len; i++) {
		u8 desiredValue = src[i];
		u8 maxValue = getMaxValue(memOff);
		// maxValue == 0 means write-protected unless called from initialisation code, in which case it really means the maximum value is 0.
		if (maxValue != 0 || init) {
			if (desiredValue > maxValue) {
#if MT32EMU_MONITOR_SYSEX > 0
				synth->printDebug("write[%d]: Wanted 0x%02x at %d, but max 0x%02x", type, desiredValue, memOff, maxValue);
#endif
				desiredValue = maxValue;
			}
			dest[memOff] = desiredValue;
		}
		else if (desiredValue != 0) {
#if MT32EMU_MONITOR_SYSEX > 0
			// Only output debug info if they wanted to write non-zero, since a lot of things cause this to spit out a lot of debug info otherwise.
			synth->printDebug("write[%d]: Wanted 0x%02x at %d, but write-protected", type, desiredValue, memOff);
#endif
		}
		memOff++;
	}
	}

u8 cm32p_device::calcSysexChecksum(const u8 *data, const u32 len, const u8 initChecksum) {
	unsigned int checksum = -initChecksum;
	for (unsigned int i = 0; i < len; i++) {
		checksum -= data[i];
	}
	return u8(checksum & 0x7f);
}
