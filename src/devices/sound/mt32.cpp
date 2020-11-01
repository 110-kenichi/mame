// copyright-holders:K.Ito
/***************************************************************************



****************************************************************************/

#include "emu.h"
#include "sound/mt32.h"
#include "..\munt\mt32emu\src\c_interface\c_interface.h"

// device type definition
DEFINE_DEVICE_TYPE(MT32, mt32_device, "mt32", "MT32")


//**************************************************************************
//  LIVE DEVICE
//**************************************************************************

//-------------------------------------------------
//  beep_device - constructor
//-------------------------------------------------

mt32_device::mt32_device(const machine_config &mconfig, const char *tag, device_t *owner, uint32_t clock)
	: device_t(mconfig, MT32, tag, owner, clock)
	, device_sound_interface(mconfig, *this)
	, m_stream(nullptr)
{

}

void mt32_device::set_enable(int enable)
{
	if (m_enable != enable)
	{
		if (enable != 0)
		{
			mt32emu_open_synth(context);
			clipping_overflow_l = 0.0f;
			clipping_overflow_r = 0.0f;
		}
		else
		{
			mt32emu_close_synth(context);
		}
		m_enable = enable;
	}
}

//-------------------------------------------------
//  device_start - device-specific startup
//-------------------------------------------------

void mt32_device::device_start()
{
	m_frequency = clock();

	m_stream = stream_alloc(0, 2, machine().sample_rate());
	m_enable = 0;

	mt32emu_report_handler_i a;
	a.v0 = NULL;
	context = mt32emu_create_context(a, NULL);

	mt32emu_set_stereo_output_samplerate(context, machine().sample_rate());
	mt32emu_set_analog_output_mode(context, mt32emu_analog_output_mode::MT32EMU_AOM_ACCURATE);
	mt32emu_set_samplerate_conversion_quality(context, mt32emu_samplerate_conversion_quality::MT32EMU_SRCQ_BEST);
	mt32emu_select_renderer_type(context, mt32emu_renderer_type::MT32EMU_RT_FLOAT);
	mt32emu_set_nice_partial_mixing_enabled(context, mt32emu_boolean::MT32EMU_BOOL_TRUE);
	mt32emu_preallocate_reverb_memory(context, mt32emu_boolean::MT32EMU_BOOL_TRUE);
	mt32emu_set_midi_delay_mode(context, mt32emu_midi_delay_mode::MT32EMU_MDM_IMMEDIATE);
	//mt32emu_set_output_gain(context, 0.75);

	mt32emu_add_rom_file(context, "MT32_CONTROL.ROM");
	mt32emu_add_rom_file(context, "MT32_PCM.ROM");
}

//-------------------------------------------------
//  sound_stream_update - handle a stream update
//-------------------------------------------------

/** Enqueues a single short MIDI message to be processed ASAP. The message must contain a status byte. */
void mt32_device::play_msg(mt32emu_bit32u msg)
{
	mtxBuffer.lock();

	attotime diff = machine().time() - lastUpdateTime;
	mt32emu_bit32u tim = mt32emu_get_internal_rendered_sample_count(context);
	tim += (mt32emu_bit32u)round(diff.as_double() / mt32_tick.as_double());
	mt32emu_play_msg_at(context, msg, tim);

	mtxBuffer.unlock();
}
/** Enqueues a single well formed System Exclusive MIDI message to be processed ASAP. */
void mt32_device::play_sysex(const mt32emu_bit8u *sysex, mt32emu_bit32u len)
{
	mtxBuffer.lock();

	attotime diff = machine().time() - lastUpdateTime;
	mt32emu_bit32u tim = mt32emu_get_internal_rendered_sample_count(context);
	tim += (mt32emu_bit32u)round(diff.as_double() / mt32_tick.as_double());
	mt32emu_play_sysex_at(context, sysex, len, tim);

	mtxBuffer.unlock();
}

void mt32_device::sound_stream_update(sound_stream &stream, stream_sample_t **inputs, stream_sample_t **outputs, int samples)
{
	stream_sample_t *buffer1 = outputs[0];
	stream_sample_t *buffer2 = outputs[1];

	float *orgptr = (float*)malloc(sizeof(float) * samples * 2);
	float *renderBuf = orgptr;
	float *streamBuf = orgptr;
#if true	//MAME_DEBUG
	mtxBuffer.lock();
	mt32emu_render_float(context, renderBuf, samples);
	lastUpdateTime = machine().time();
	mtxBuffer.unlock();
#else
	int inc = samples / 10;
	int idx = 0;
	if (inc != 0)
	{
		for (idx = 0; idx < samples - inc; idx += inc, renderBuf += inc * 2)
		{
			mtxBuffer.lock();
			mt32emu_render_float(context, renderBuf, inc);
			lastUpdateTime = machine().time();
			mtxBuffer.unlock();
		}
	}
	mtxBuffer.lock();
	mt32emu_render_float(context, renderBuf, samples - idx);
	lastUpdateTime = machine().time();
	mtxBuffer.unlock();
#endif

	while (samples-- > 0)
	{
		float outl = *streamBuf++;
		float outr = *streamBuf++;
		/*
		outl += clipping_overflow_l;
		clipping_overflow_l = 0;
		if (outl > 1.0f) {
			clipping_overflow_l = outl - 1.0f;
			outl = 1.0f;
		}
		else if (outl < -1.0f) {
			clipping_overflow_l = outl + 1.0f;
			outl = -1.0f;
		}
		outr += clipping_overflow_r;
		clipping_overflow_r = 0;
		if (outr > 1.0f) {
			clipping_overflow_r = outr - 1.0f;
			outr = 1.0f;
		}
		else if (outr < -1.0f) {
			clipping_overflow_r = outr + 1.0f;
			outr = -1.0f;
		}*/
		*buffer1++ = ((stream_sample_t)(outl * 32767));
		*buffer2++ = ((stream_sample_t)(outr * 32767));
	}

	free(orgptr);
}

