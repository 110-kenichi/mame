// license:GPL-2.0+
// copyright-holders:Matthew Conte
/*****************************************************************************

  MAME/MESS NES APU CORE

  Based on the Nofrendo/Nosefart NES N2A03 sound emulation core written by
  Matthew Conte (matt@conte.com) and redesigned for use in MAME/MESS by
  Who Wants to Know? (wwtk@mail.com)

  This core is written with the advise and consent of Matthew Conte and is
  released under the GNU Public License.  This core is freely available for
  use in any freeware project, subject to the following terms:

  Any modifications to this code must be duly noted in the source and
  approved by Matthew Conte and myself prior to public submission.

  timing notes:
  master = 21477270
  2A03 clock = master/12
  sequencer = master/89490 or CPU/7457

 *****************************************************************************

   NES_APU.CPP

   Actual NES APU interface.

   LAST MODIFIED 02/29/2004

   - Based on Matthew Conte's Nofrendo/Nosefart core and redesigned to
	 use MAME system calls and to enable multiple APUs.  Sound at this
	 point should be just about 100% accurate, though I cannot tell for
	 certain as yet.

	 A queue interface is also available for additional speed.  However,
	 the implementation is not yet 100% (DPCM sounds are inaccurate),
	 so it is disabled by default.

 *****************************************************************************

   BUGFIXES:

   - Various bugs concerning the DPCM channel fixed. (Oliver Achten)
   - Fixed $4015 read behaviour. (Oliver Achten)

 *****************************************************************************/

#include "emu.h"
#include "nes_apu.h"
 /* INTERNAL FUNCTIONS */

 /* INITIALIZE WAVE TIMES RELATIVE TO SAMPLE RATE */
static void create_vbltimes(u32 * table, const u8 *vbl, unsigned int rate)
{
	int i;

	for (i = 0; i < 0x20; i++)
		table[i] = vbl[i] * rate;
}

/* INITIALIZE SAMPLE TIMES IN TERMS OF VSYNCS */
void nesapu_device::create_syncs(unsigned long sps)
{
	int i;
	unsigned long val = sps;

	for (i = 0; i < SYNCS_MAX1; i++)
	{
		m_sync_times1[i] = val;
		val += sps;
	}

	val = 0;
	for (i = 0; i < SYNCS_MAX2; i++)
	{
		m_sync_times2[i] = val;
		m_sync_times2[i] >>= 2;
		val += sps;
	}
}

DEFINE_DEVICE_TYPE(NES_APU, nesapu_device, "nesapu", "N2A03 APU")

nesapu_device::nesapu_device(const machine_config &mconfig, const char *tag, device_t *owner, u32 clock)
	: device_t(mconfig, NES_APU, tag, owner, clock)
	, device_sound_interface(mconfig, *this)
	, m_samps_per_sync(0)
	, m_buffer_size(0)
	, m_stream(nullptr)
	, m_irq_handler(*this)
	, m_mem_read_cb(*this)
{
	for (auto & elem : m_vbl_times)
	{
		elem = 0;
	}

	for (auto & elem : m_sync_times1)
	{
		elem = 0;
	}

	for (auto & elem : m_sync_times2)
	{
		elem = 0;
	}
}

void nesapu_device::device_reset()
{
	write(0x15, 0x00);
	memset(&m_APU.fds, 0, sizeof(m_APU.fds));
	memset(&m_APU.vrc6rect, 0, sizeof(m_APU.vrc6rect));
	memset(&m_APU.vrc6saw, 0, sizeof(m_APU.vrc6saw));
}

void nesapu_device::device_clock_changed()
{
	calculate_rates();
}

void nesapu_device::calculate_rates()
{
	int rate = clock() / 4;
	sampling_rate = rate;
	cycle_rate = (int)(clock()*65536.0f / (float)sampling_rate);

	m_samps_per_sync = 89490 / 12; // Is there a different PAL value?
	m_buffer_size = m_samps_per_sync;

	create_vbltimes(m_vbl_times, vbl_length, m_samps_per_sync);
	create_syncs(m_samps_per_sync);

	/* Adjust buffer size if 16 bits */
	m_buffer_size += m_samps_per_sync;

	if (m_stream != nullptr)
		m_stream->set_sample_rate(rate);
	else
		m_stream = machine().sound().stream_alloc(*this, 0, 2, rate);
}

//-------------------------------------------------
//  device_start - device-specific startup
//-------------------------------------------------

void nesapu_device::device_start()
{
	// resolve callbacks
	m_irq_handler.resolve_safe();
	m_mem_read_cb.resolve_safe(0x00);

	calculate_rates();

	/* register for save */
	for (int i = 0; i < 2; i++)
	{
		save_item(NAME(m_APU.squ[i].regs), i);
		save_item(NAME(m_APU.squ[i].vbl_length), i);
		save_item(NAME(m_APU.squ[i].freq), i);
		save_item(NAME(m_APU.squ[i].phaseacc), i);
		save_item(NAME(m_APU.squ[i].output_vol), i);
		save_item(NAME(m_APU.squ[i].env_phase), i);
		save_item(NAME(m_APU.squ[i].sweep_phase), i);
		save_item(NAME(m_APU.squ[i].adder), i);
		save_item(NAME(m_APU.squ[i].env_vol), i);
		save_item(NAME(m_APU.squ[i].enabled), i);
	}

	save_item(NAME(m_APU.tri.regs));
	save_item(NAME(m_APU.tri.linear_length));
	save_item(NAME(m_APU.tri.vbl_length));
	save_item(NAME(m_APU.tri.write_latency));
	save_item(NAME(m_APU.tri.phaseacc));
	save_item(NAME(m_APU.tri.output_vol));
	save_item(NAME(m_APU.tri.adder));
	save_item(NAME(m_APU.tri.counter_started));
	save_item(NAME(m_APU.tri.enabled));

	save_item(NAME(m_APU.noi.regs));
	save_item(NAME(m_APU.noi.seed));
	save_item(NAME(m_APU.noi.vbl_length));
	save_item(NAME(m_APU.noi.phaseacc));
	save_item(NAME(m_APU.noi.output_vol));
	save_item(NAME(m_APU.noi.env_phase));
	save_item(NAME(m_APU.noi.env_vol));
	save_item(NAME(m_APU.noi.enabled));

	save_item(NAME(m_APU.dpcm.regs));
	save_item(NAME(m_APU.dpcm.address));
	save_item(NAME(m_APU.dpcm.length));
	save_item(NAME(m_APU.dpcm.bits_left));
	save_item(NAME(m_APU.dpcm.phaseacc));
	save_item(NAME(m_APU.dpcm.output_vol));
	save_item(NAME(m_APU.dpcm.cur_byte));
	save_item(NAME(m_APU.dpcm.enabled));
	save_item(NAME(m_APU.dpcm.irq_occurred));
	save_item(NAME(m_APU.dpcm.vol));

	save_item(NAME(m_APU.regs));

#ifdef USE_QUEUE
	save_item(NAME(m_APU.queue));
	save_item(NAME(m_APU.head));
	save_item(NAME(m_APU.tail));
#else
	save_item(NAME(m_APU.buf_pos));
	save_item(NAME(m_APU.step_mode));
#endif

	m_vgm_writer = new vgm_writer(machine());
}

void nesapu_device::vgm_start(char *name)
{
	m_vgm_writer->vgm_start(name);

	m_vgm_writer->vgm_open(VGMC_NESAPU, clock());
};

void nesapu_device::vgm_stop(void)
{
	m_vgm_writer->vgm_stop();
};

/* TODO: sound channels should *ALL* have DC volume decay */

/* OUTPUT SQUARE WAVE SAMPLE (VALUES FROM -16 to +15) */
s8 nesapu_device::apu_square(apu_t::square_t *chan)
{
	int env_delay;
	int sweep_delay;
	s8 output;

	/* reg0: 0-3=volume, 4=envelope, 5=hold, 6-7=duty cycle
	** reg1: 0-2=sweep shifts, 3=sweep inc/dec, 4-6=sweep length, 7=sweep on
	** reg2: 8 bits of freq
	** reg3: 0-2=high freq, 7-4=vbl length counter
	*/

	if (false == chan->enabled)
		return 0;

	/* enveloping */
	env_delay = m_sync_times1[chan->regs[0] & 0x0F];

	/* decay is at a rate of (env_regs + 1) / 240 secs */
	chan->env_phase -= 4;
	while (chan->env_phase < 0)
	{
		chan->env_phase += env_delay;
		if (chan->regs[0] & 0x20)
			chan->env_vol = (chan->env_vol + 1) & 15;
		else if (chan->env_vol < 15)
			chan->env_vol++;
	}

	/* vbl length counter */
	if (chan->vbl_length > 0 && 0 == (chan->regs[0] & 0x20))
		chan->vbl_length--;

	if (0 == chan->vbl_length)
		return 0;

	/* freqsweeps */
	if ((chan->regs[1] & 0x80) && (chan->regs[1] & 7))
	{
		sweep_delay = m_sync_times1[(chan->regs[1] >> 4) & 7];
		chan->sweep_phase -= 2;
		while (chan->sweep_phase < 0)
		{
			chan->sweep_phase += sweep_delay;
			if (chan->regs[1] & 8)
				chan->freq -= chan->freq >> (chan->regs[1] & 7);
			else
				chan->freq += chan->freq >> (chan->regs[1] & 7);
		}
	}

	if ((1 == (chan->regs[1] & 8) && (chan->freq >> 16) > freq_limit[chan->regs[1] & 7])
		|| (chan->freq >> 16) < 4)
		return 0;

	chan->phaseacc -= 4;

	while (chan->phaseacc < 0)
	{
		chan->phaseacc += (chan->freq >> 16);
		chan->adder = (chan->adder + 1) & 0x0F;
	}

	if (chan->regs[0] & 0x10) /* fixed volume */
		output = chan->regs[0] & 0x0F;
	else
		output = 0x0F - chan->env_vol;

	if (chan->adder < (duty_lut[chan->regs[0] >> 6]))
		output = -output;

	return (s8)output;
}

/* OUTPUT TRIANGLE WAVE SAMPLE (VALUES FROM -16 to +15) */
s8 nesapu_device::apu_triangle(apu_t::triangle_t *chan)
{
	int freq;
	s8 output;
	/* reg0: 7=holdnote, 6-0=linear length counter
	** reg2: low 8 bits of frequency
	** reg3: 7-3=length counter, 2-0=high 3 bits of frequency
	*/

	if (false == chan->enabled) {
		//HACK: -> prevent noise (mamidimemo)
		chan->adder = 0;
		chan->output_vol = 0;
		//HACK: <-
		return 0;
	}

	if (false == chan->counter_started && 0 == (chan->regs[0] & 0x80))
	{
		if (chan->write_latency)
			chan->write_latency--;
		if (0 == chan->write_latency)
			chan->counter_started = true;
	}

	if (chan->counter_started)
	{
		if (chan->linear_length > 0)
			chan->linear_length--;
		if (chan->vbl_length && 0 == (chan->regs[0] & 0x80))
			chan->vbl_length--;

		if (0 == chan->vbl_length)
			return 0;
	}

	if (0 == chan->linear_length)
		return 0;

	freq = (((chan->regs[3] & 7) << 8) + chan->regs[2]) + 1;

	if (freq < 4) /* inaudible */
		return 0;

	chan->phaseacc -= 4;
	while (chan->phaseacc < 0)
	{
		chan->phaseacc += freq;
		chan->adder = (chan->adder + 1) & 0x1F;

		output = (chan->adder & 7) << 1;
		if (chan->adder & 8)
			output = 0x10 - output;
		if (chan->adder & 0x10)
			output = -output;

		chan->output_vol = output;
	}

	return (s8)chan->output_vol;
}

/* OUTPUT NOISE WAVE SAMPLE (VALUES FROM -16 to +15) */
s8 nesapu_device::apu_noise(apu_t::noise_t *chan)
{
	int freq, env_delay;
	u8 outvol;
	u8 output;

	/* reg0: 0-3=volume, 4=envelope, 5=hold
	** reg2: 7=small(93 byte) sample,3-0=freq lookup
	** reg3: 7-4=vbl length counter
	*/

	if (false == chan->enabled)
		return 0;

	/* enveloping */
	env_delay = m_sync_times1[chan->regs[0] & 0x0F];

	/* decay is at a rate of (env_regs + 1) / 240 secs */
	chan->env_phase -= 4;
	while (chan->env_phase < 0)
	{
		chan->env_phase += env_delay;
		if (chan->regs[0] & 0x20)
			chan->env_vol = (chan->env_vol + 1) & 15;
		else if (chan->env_vol < 15)
			chan->env_vol++;
	}

	/* length counter */
	if (0 == (chan->regs[0] & 0x20))
	{
		if (chan->vbl_length > 0)
			chan->vbl_length--;
	}

	if (0 == chan->vbl_length)
		return 0;

	freq = noise_freq[chan->regs[2] & 0x0F];
	chan->phaseacc -= 4;
	while (chan->phaseacc < 0)
	{
		chan->phaseacc += freq;
		chan->seed = (chan->seed >> 1) | ((BIT(chan->seed, 0) ^ BIT(chan->seed, (chan->regs[2] & 0x80) ? 6 : 1)) << 14);
	}

	if (chan->regs[0] & 0x10) /* fixed volume */
		outvol = chan->regs[0] & 0x0F;
	else
		outvol = 0x0F - chan->env_vol;

	output = chan->seed & 0xff;
	if (output > outvol)
		output = outvol;

	if (chan->seed & 0x80) /* make it negative */
		output = -output;

	return (s8)output;
}

/* RESET DPCM PARAMETERS */
static inline void apu_dpcmreset(apu_t::dpcm_t *chan)
{
	//chan->address = 0xC000 + u16(chan->regs[2] << 6);
	chan->address = 0x0000;
	chan->length = u16(chan->regs[3] << 4) + 1;
	chan->bits_left = chan->length << 3;
	chan->irq_occurred = false;
	chan->enabled = true; /* Fixed * Proper DPCM channel ENABLE/DISABLE flag behaviour*/
	chan->vol = 0; /* Fixed * DPCM DAC resets itself when restarted */

	// I have no idea how to do this at all since DPCM reads are done using a callback function.
	//vgm_write_large_data(vgm_idx, 0x01, 0x10000, chan->address, chan->length, chan->memory->get_read_ptr(0xC000));
}

/* OUTPUT DPCM WAVE SAMPLE (VALUES FROM -64 to +63) */
/* TODO: centerline naughtiness */
s8 nesapu_device::apu_dpcm(apu_t::dpcm_t *chan)
{
	int freq, bit_pos;

	/* reg0: 7=irq gen, 6=looping, 3-0=pointer to clock table
	** reg1: output dc level, 7 bits unsigned
	** reg2: 8 bits of 64-byte aligned address offset : $C000 + (value * 64)
	** reg3: length, (value * 16) + 1
	*/
	if (chan->enabled)
	{
		freq = dpcm_clocks[chan->regs[0] & 0x0F];
		chan->phaseacc -= 4;

		while (chan->phaseacc < 0)
		{
			chan->phaseacc += freq;

			if (0 == chan->length)
			{
				chan->enabled = false; /* Fixed * Proper DPCM channel ENABLE/DISABLE flag behaviour*/
				chan->vol = 0; /* Fixed * DPCM DAC resets itself when restarted */
				if (chan->regs[0] & 0x40)
					apu_dpcmreset(chan);
				else
				{
					if (chan->regs[0] & 0x80) /* IRQ Generator */
					{
						chan->irq_occurred = true;
						m_irq_handler(true);
					}
					break;
				}
			}


			chan->bits_left--;
			bit_pos = 7 - (chan->bits_left & 7);
			if (7 == bit_pos)
			{
				//				chan->cur_byte = m_mem_read_cb(chan->address);
				chan->cur_byte = m_dpcm_buffer[chan->address];
				chan->address++;
				chan->length--;
			}

			if (chan->cur_byte & (1 << bit_pos))
				//              chan->regs[1]++;
				chan->vol += 2; /* FIXED * DPCM channel only uses the upper 6 bits of the DAC */
			else
				//              chan->regs[1]--;
				chan->vol -= 2;
		}
	}

	if (chan->vol > 63)
		chan->vol = 63;
	else if (chan->vol < -64)
		chan->vol = -64;

	return (s8)(chan->vol);
}

s8 nesapu_device::apu_fds(apu_t::fds_t *fds)
{
	// Envelope unit
	if (fds->envelope_enable && fds->envelope_speed) {
		// Volume envelope
		if (fds->volenv_mode < 2) {
			double	decay = ((double)fds->envelope_speed * (double)(fds->volenv_decay + 1) * (double)sampling_rate) / (232.0*960.0);
			fds->volenv_phaseacc -= 1.0;
			while (fds->volenv_phaseacc < 0.0) {
				fds->volenv_phaseacc += decay;

				if (fds->volenv_mode == 0) {
					// 減少モード
					if (fds->volenv_gain)
						fds->volenv_gain--;
				}
				else
					if (fds->volenv_mode == 1) {
						if (fds->volenv_gain < 0x20)
							fds->volenv_gain++;
					}
			}
		}

		// Sweep envelope
		if (fds->swpenv_mode < 2) {
			double	decay = ((double)fds->envelope_speed * (double)(fds->swpenv_decay + 1) * (double)sampling_rate) / (232.0*960.0);
			fds->swpenv_phaseacc -= 1.0;
			while (fds->swpenv_phaseacc < 0.0) {
				fds->swpenv_phaseacc += decay;

				if (fds->swpenv_mode == 0) {
					// 減少モード
					if (fds->swpenv_gain)
						fds->swpenv_gain--;
				}
				else
					if (fds->swpenv_mode == 1) {
						if (fds->swpenv_gain < 0x20)
							fds->swpenv_gain++;
					}
			}
		}
	}

	// Effector(LFO) unit
	int	sub_freq = 0;
	//	if( fds.lfo_enable && fds.envelope_speed && fds.lfo_frequency ) {
	if (fds->lfo_enable) {
		if (fds->lfo_frequency)
		{
			static int tbl[8] = { 0, 1, 2, 4, 0, -4, -2, -1 };

			fds->lfo_phaseacc -= (clock()*(double)fds->lfo_frequency) / 65536.0;
			while (fds->lfo_phaseacc < 0.0) {
				fds->lfo_phaseacc += (double)sampling_rate;

				if (fds->lfo_wavetable[fds->lfo_addr] == 4)
					fds->sweep_bias = 0;
				else
					fds->sweep_bias += tbl[fds->lfo_wavetable[fds->lfo_addr]];

				fds->lfo_addr = (fds->lfo_addr + 1) & 63;
			}
		}

		if (fds->sweep_bias > 63)
			fds->sweep_bias -= 128;
		else if (fds->sweep_bias < -64)
			fds->sweep_bias += 128;

		int	sub_multi = fds->sweep_bias * fds->swpenv_gain;

		if (sub_multi & 0x0F) {
			// 16で割り切れない場合
			sub_multi = (sub_multi / 16);
			if (fds->sweep_bias >= 0)
				sub_multi += 2;    // 正の場合
			else
				sub_multi -= 1;    // 負の場合
		}
		else {
			// 16で割り切れる場合
			sub_multi = (sub_multi / 16);
		}
		// 193を超えると-258する(-64へラップ)
		if (sub_multi > 193)
			sub_multi -= 258;
		// -64を下回ると+256する(192へラップ)
		if (sub_multi < -64)
			sub_multi += 256;

		sub_freq = (fds->main_frequency) * sub_multi / 64;
	}

	// Main unit
	int	output = 0;
	if (fds->main_enable && fds->main_frequency && !fds->wave_setup) {
		u64	freq;
		int	main_addr_old = fds->main_addr;

		freq = ((u64)fds->main_frequency + (u64)sub_freq) * (u64)clock() / (u64)65536; //mamidimemo 1789772.5 / 65536.0;
		fds->main_addr = (fds->main_addr + freq + 64 * sampling_rate) % (64 * sampling_rate);

		// 1周期を超えたらボリューム更新
		if (main_addr_old > fds->main_addr)
			fds->now_volume = (fds->volenv_gain < 0x21) ? fds->volenv_gain : 0x20;

		output = fds->main_wavetable[(fds->main_addr / sampling_rate) & 0x3f] * 8 * fds->now_volume * fds->master_volume / 30;

		if (fds->now_volume)
			fds->now_freq = freq * 4;
		else
			fds->now_freq = 0;
	}
	else {
		fds->now_freq = 0;
		output = 0;
	}

	// LPF
#if	1
	output = (fds->output_buf[0] * 2 + output) / 3;
	fds->output_buf[0] = output;
#else
	output = (output_buf[0] + output_buf[1] + output) / 3;
	output_buf[0] = output_buf[1];
	output_buf[1] = output;
#endif

	fds->output = output;
	return	(fds->output >> 8) & 0xff;
}

s8 nesapu_device::vrc6_RectangleRender(apu_t::VRC6_RECT_t *ch)
{
	// Enable?
	if (!ch->enable) {
		ch->output_vol = 0;
		ch->adder = 0;
		return	ch->output_vol;
	}

	// Digitized output
	if (ch->gate) {
		ch->output_vol = ch->volume; // << RECTANGLE_VOL_SHIFT;
		return	ch->output_vol & 0xff;
	}

	// 一定以上の周波数は処理しない(無駄)
	if (ch->freq < INT2FIX(8)) {
		ch->output_vol = 0;
		return	ch->output_vol;
	}

	ch->phaseacc -= cycle_rate;
	if (ch->phaseacc >= 0)
		return	ch->output_vol & 0xff;

	int	output = ch->volume; // << RECTANGLE_VOL_SHIFT;

	if (ch->freq > cycle_rate) {
		// add 1 step
		ch->phaseacc += ch->freq;
		ch->adder = (ch->adder + 1) & 0x0F;
		if (ch->adder <= ch->duty_pos)
			ch->output_vol = output;
		else
			ch->output_vol = -output;
	}
	else {
		// average calculate
		int	num_times, total;
		num_times = total = 0;
		while (ch->phaseacc < 0) {
			ch->phaseacc += ch->freq;
			ch->adder = (ch->adder + 1) & 0x0F;
			if (ch->adder <= ch->duty_pos)
				total += output;
			else
				total += -output;
			num_times++;
		}
		ch->output_vol = total / num_times;
	}

	return	ch->output_vol & 0xff;
}

s8 nesapu_device::vrc6_SawtoothRender(apu_t::VRC6_SAW_t *ch)
{
	// Digitized output
	if (!ch->enable) {
		ch->output_vol = 0;
		return	ch->output_vol;
	}

	// 一定以上の周波数は処理しない(無駄)
	if (ch->freq < INT2FIX(9)) {
		return	ch->output_vol & 0xff;
	}

	ch->phaseacc -= cycle_rate / 2;
	if (ch->phaseacc >= 0)
		return	ch->output_vol & 0xff;

	if (ch->freq > cycle_rate / 2) {
		// add 1 step
		ch->phaseacc += ch->freq;
		if (++ch->adder >= 7) {
			ch->adder = 0;
			ch->accum = 0;
		}
		ch->accum += ch->phaseaccum;
		ch->output_vol = ch->accum;// << SAWTOOTH_VOL_SHIFT;
	}
	else {
		// average calculate
		int	num_times, total;
		num_times = total = 0;
		while (ch->phaseacc < 0) {
			ch->phaseacc += ch->freq;
			if (++ch->adder >= 7) {
				ch->adder = 0;
				ch->accum = 0;
			}
			ch->accum += ch->phaseaccum;
			total += ch->accum;// << SAWTOOTH_VOL_SHIFT;
			num_times++;
		}
		ch->output_vol = (total / num_times);
	}

	return ch->output_vol & 0xff;
}



/* WRITE REGISTER VALUE */
inline void nesapu_device::apu_regwrite(int address, u8 value)
{
	int chan = (address & 4) ? 1 : 0;

	switch (address)
	{
		/* squares */
	case apu_t::WRA0:
	case apu_t::WRB0:
		m_APU.squ[chan].regs[0] = value;
		break;

	case apu_t::WRA1:
	case apu_t::WRB1:
		m_APU.squ[chan].regs[1] = value;
		break;

	case apu_t::WRA2:
	case apu_t::WRB2:
		m_APU.squ[chan].regs[2] = value;
		if (m_APU.squ[chan].enabled)
			m_APU.squ[chan].freq = ((((m_APU.squ[chan].regs[3] & 7) << 8) + value) + 1) << 16;
		break;

	case apu_t::WRA3:
	case apu_t::WRB3:
		m_APU.squ[chan].regs[3] = value;

		if (m_APU.squ[chan].enabled)
		{
			m_APU.squ[chan].vbl_length = m_vbl_times[value >> 3];
			m_APU.squ[chan].env_vol = 0;
			m_APU.squ[chan].freq = ((((value & 7) << 8) + m_APU.squ[chan].regs[2]) + 1) << 16;
		}

		break;

		/* triangle */
	case apu_t::WRC0:
		m_APU.tri.regs[0] = value;

		if (m_APU.tri.enabled)
		{                                          /* ??? */
			if (false == m_APU.tri.counter_started)
				m_APU.tri.linear_length = m_sync_times2[value & 0x7F];
		}

		break;

	case 0x4009:
		/* unused */
		m_APU.tri.regs[1] = value;
		break;

	case apu_t::WRC2:
		m_APU.tri.regs[2] = value;
		break;

	case apu_t::WRC3:
		m_APU.tri.regs[3] = value;

		/* this is somewhat of a hack.  there is some latency on the Real
		** Thing between when trireg0 is written to and when the linear
		** length counter actually begins its countdown.  we want to prevent
		** the case where the program writes to the freq regs first, then
		** to reg 0, and the counter accidentally starts running because of
		** the sound queue's timestamp processing.
		**
		** set to a few NES sample -- should be sufficient
		**
		**    3 * (1789772.727 / 44100) = ~122 cycles, just around one scanline
		**
		** should be plenty of time for the 6502 code to do a couple of table
		** dereferences and load up the other triregs
		*/

		/* used to be 3, but now we run the clock faster, so base it on samples/sync */
		m_APU.tri.write_latency = (m_samps_per_sync + 239) / 240;

		if (m_APU.tri.enabled)
		{
			m_APU.tri.counter_started = false;
			m_APU.tri.vbl_length = m_vbl_times[value >> 3];
			m_APU.tri.linear_length = m_sync_times2[m_APU.tri.regs[0] & 0x7F];
		}

		break;

		/* noise */
	case apu_t::WRD0:
		m_APU.noi.regs[0] = value;
		break;

	case 0x400D:
		/* unused */
		m_APU.noi.regs[1] = value;
		break;

	case apu_t::WRD2:
		m_APU.noi.regs[2] = value;
		break;

	case apu_t::WRD3:
		m_APU.noi.regs[3] = value;

		if (m_APU.noi.enabled)
		{
			m_APU.noi.vbl_length = m_vbl_times[value >> 3];
			m_APU.noi.env_vol = 0; /* reset envelope */
		}
		break;

		/* DMC */
	case apu_t::WRE0:
		m_APU.dpcm.regs[0] = value;
		if (0 == (value & 0x80)) {
			m_irq_handler(false);
			m_APU.dpcm.irq_occurred = false;
		}
		break;

	case apu_t::WRE1: /* 7-bit DAC */
		//m_APU.dpcm.regs[1] = value - 0x40;
		m_APU.dpcm.regs[1] = value & 0x7F;
		m_APU.dpcm.vol = (m_APU.dpcm.regs[1] - 64);
		break;

	case apu_t::WRE2:
		m_APU.dpcm.regs[2] = value;
		//apu_dpcmreset(m_APU.dpcm);
		break;

	case apu_t::WRE3:
		m_APU.dpcm.regs[3] = value;
		break;

	case apu_t::IRQCTRL:
		if (value & 0x80)
			m_APU.step_mode = 5;
		else
			m_APU.step_mode = 4;
		break;

	case apu_t::SMASK:
		if (value & 0x01)
			m_APU.squ[0].enabled = true;
		else
		{
			m_APU.squ[0].enabled = false;
			m_APU.squ[0].vbl_length = 0;
		}

		if (value & 0x02)
			m_APU.squ[1].enabled = true;
		else
		{
			m_APU.squ[1].enabled = false;
			m_APU.squ[1].vbl_length = 0;
		}

		if (value & 0x04)
			m_APU.tri.enabled = true;
		else
		{
			m_APU.tri.enabled = false;
			m_APU.tri.vbl_length = 0;
			m_APU.tri.linear_length = 0;
			m_APU.tri.counter_started = false;
			m_APU.tri.write_latency = 0;
		}

		if (value & 0x08)
			m_APU.noi.enabled = true;
		else
		{
			m_APU.noi.enabled = false;
			m_APU.noi.vbl_length = 0;
		}

		if (value & 0x10)
		{
			/* only reset dpcm values if DMA is finished */
			if (false == m_APU.dpcm.enabled)
			{
				m_APU.dpcm.enabled = true;
				apu_dpcmreset(&m_APU.dpcm);
			}
		}
		else
			m_APU.dpcm.enabled = false;

		//m_irq_handler(false);
		m_APU.dpcm.irq_occurred = false;

		break;
	default:
#ifdef MAME_DEBUG
		logerror("invalid apu write: $%02X at $%04X\n", value, address);
#endif
		break;
	}
}

inline void nesapu_device::apu_regwrite_fds(int addr, u8 data)
{
	if (addr < 0x40 || addr > 0xBF)
		return;

	m_APU.fds.reg[addr - 0x40] = data;
	if (addr >= 0x40 && addr <= 0x7F) {
		if (m_APU.fds.wave_setup) {
			m_APU.fds.main_wavetable[addr - 0x40] = 0x20 - ((int)data & 0x3F);
		}
	}
	else {
		int rate = clock() / 4;

		switch (addr) {
		case	0x80:	// Volume Envelope
			m_APU.fds.volenv_mode = data >> 6;
			if (data & 0x80) {
				m_APU.fds.volenv_gain = data & 0x3F;

				// 即時反映
				if (!m_APU.fds.main_addr) {
					m_APU.fds.now_volume = (m_APU.fds.volenv_gain < 0x21) ? m_APU.fds.volenv_gain : 0x20;
				}
			}
			// エンベロープ1段階の演算
			m_APU.fds.volenv_decay = data & 0x3F;
			m_APU.fds.volenv_phaseacc = (double)m_APU.fds.envelope_speed * (double)(m_APU.fds.volenv_decay + 1) * rate / (232.0*960.0);
			break;

		case	0x82:	// Main Frequency(Low)
			m_APU.fds.main_frequency = (m_APU.fds.main_frequency&~0x00FF) | (int)data;
			break;
		case	0x83:	// Main Frequency(High)
			m_APU.fds.main_enable = (~data)&(1 << 7);
			m_APU.fds.envelope_enable = (~data)&(1 << 6);
			if (!m_APU.fds.main_enable) {
				m_APU.fds.main_addr = 0;
				m_APU.fds.now_volume = (m_APU.fds.volenv_gain < 0x21) ? m_APU.fds.volenv_gain : 0x20;
			}
			//				m_APU.fds.main_frequency  = (m_APU.fds.main_frequency&0x00FF)|(((INT)data&0x3F)<<8);
			m_APU.fds.main_frequency = (m_APU.fds.main_frequency & 0x00FF) | (((int)data & 0x0F) << 8);
			break;

		case	0x84:	// Sweep Envelope
			m_APU.fds.swpenv_mode = data >> 6;
			if (data & 0x80) {
				m_APU.fds.swpenv_gain = data & 0x3F;
			}
			// エンベロープ1段階の演算
			m_APU.fds.swpenv_decay = data & 0x3F;
			m_APU.fds.swpenv_phaseacc = (double)m_APU.fds.envelope_speed * (double)(m_APU.fds.swpenv_decay + 1) * rate / (232.0*960.0);
			break;

		case	0x85:	// Sweep Bias
			if (data & 0x40) m_APU.fds.sweep_bias = (data & 0x3f) - 0x40;
			else		m_APU.fds.sweep_bias = data & 0x3f;
			m_APU.fds.lfo_addr = 0;
			break;

		case	0x86:	// Effector(LFO) Frequency(Low)
			m_APU.fds.lfo_frequency = (m_APU.fds.lfo_frequency&(~0x00FF)) | (int)data;
			break;
		case	0x87:	// Effector(LFO) Frequency(High)
			m_APU.fds.lfo_enable = (~data & 0x80);
			m_APU.fds.lfo_frequency = (m_APU.fds.lfo_frequency & 0x00FF) | (((int)data & 0x0F) << 8);
			break;

		case	0x88:	// Effector(LFO) wavetable
			if (!m_APU.fds.lfo_enable) {
				// FIFO?
				for (int i = 0; i < 31; i++) {
					m_APU.fds.lfo_wavetable[i * 2 + 0] = m_APU.fds.lfo_wavetable[(i + 1) * 2 + 0];
					m_APU.fds.lfo_wavetable[i * 2 + 1] = m_APU.fds.lfo_wavetable[(i + 1) * 2 + 1];
				}
				m_APU.fds.lfo_wavetable[31 * 2 + 0] = data & 0x07;
				m_APU.fds.lfo_wavetable[31 * 2 + 1] = data & 0x07;
			}
			break;

		case	0x89:	// Sound control
		{
			int	tbl[] = { 30, 20, 15, 12, 0, 0, 0, 0 };	//HACK: mamidimemo
			m_APU.fds.master_volume = tbl[data & 7];
			m_APU.fds.wave_setup = data & 0x80;
		}
		break;

		case	0x8A:	// Sound control 2
			m_APU.fds.envelope_speed = data;
			break;

		default:
			break;
		}
	}
}


void nesapu_device::vrc6_write(int addr, u8 data)
{
	switch (addr) {
		// VRC6 CH0 rectangle
	case	0x9000:
		m_APU.vrc6rect[0].reg[0] = data;
		m_APU.vrc6rect[0].gate = data & 0x80;
		m_APU.vrc6rect[0].volume = data & 0x0F;
		m_APU.vrc6rect[0].duty_pos = (data >> 4) & 0x07;
		break;
	case	0x9001:
		m_APU.vrc6rect[0].reg[1] = data;
		m_APU.vrc6rect[0].freq = INT2FIX((((m_APU.vrc6rect[0].reg[2] & 0x0F) << 8) | data) + 1);
		break;
	case	0x9002:
		m_APU.vrc6rect[0].reg[2] = data;
		m_APU.vrc6rect[0].enable = data & 0x80;
		m_APU.vrc6rect[0].freq = INT2FIX((((data & 0x0F) << 8) | m_APU.vrc6rect[0].reg[1]) + 1);
		break;
		// VRC6 CH1 rectangle
	case	0xA000:
		m_APU.vrc6rect[1].reg[0] = data;
		m_APU.vrc6rect[1].gate = data & 0x80;
		m_APU.vrc6rect[1].volume = data & 0x0F;
		m_APU.vrc6rect[1].duty_pos = (data >> 4) & 0x07;
		break;
	case	0xA001:
		m_APU.vrc6rect[1].reg[1] = data;
		m_APU.vrc6rect[1].freq = INT2FIX((((m_APU.vrc6rect[1].reg[2] & 0x0F) << 8) | data) + 1);
		break;
	case	0xA002:
		m_APU.vrc6rect[1].reg[2] = data;
		m_APU.vrc6rect[1].enable = data & 0x80;
		m_APU.vrc6rect[1].freq = INT2FIX((((data & 0x0F) << 8) | m_APU.vrc6rect[1].reg[1]) + 1);
		break;
		// VRC6 CH2 sawtooth
	case	0xB000:
		m_APU.vrc6saw.reg[0] = data;
		m_APU.vrc6saw.phaseaccum = data & 0x3F;
		break;
	case	0xB001:
		m_APU.vrc6saw.reg[1] = data;
		m_APU.vrc6saw.freq = INT2FIX((((m_APU.vrc6saw.reg[2] & 0x0F) << 8) | data) + 1);
		break;
	case	0xB002:
		m_APU.vrc6saw.reg[2] = data;
		m_APU.vrc6saw.enable = data & 0x80;
		m_APU.vrc6saw.freq = INT2FIX((((data & 0x0F) << 8) | m_APU.vrc6saw.reg[1]) + 1);
		//			ch2.adder = 0;	// クリアするとノイズの原因になる
		//			ch2.accum = 0;	// クリアするとノイズの原因になる
		break;
	}
}

/* READ VALUES FROM REGISTERS */
u8 nesapu_device::read(offs_t address)
{
	if (address == 0x15) /*FIXED* Address $4015 has different behaviour*/
	{
		int readval = 0;
		if (m_APU.squ[0].vbl_length > 0)
			readval |= 0x01;

		if (m_APU.squ[1].vbl_length > 0)
			readval |= 0x02;

		if (m_APU.tri.vbl_length > 0)
			readval |= 0x04;

		if (m_APU.noi.vbl_length > 0)
			readval |= 0x08;

		if (m_APU.dpcm.enabled == true)
			readval |= 0x10;

		if (m_APU.dpcm.irq_occurred == true)
			readval |= 0x80;

		return readval;
	}
	else
		return m_APU.regs[address];
}

/* WRITE VALUE TO TEMP REGISTRY AND QUEUE EVENT */
void nesapu_device::write(offs_t address, u8 value)
{
	if (address <= 0xff)
	{
		m_APU.regs[address] = value;
	}

	m_stream->update();

	m_vgm_writer->vgm_write(0x00, address, value);

	if (address <= 0xff)
	{
		apu_regwrite(address, value);
		apu_regwrite_fds(address, value);
	}
	else if (0x9000 <= address && address <= 0xb002)
	{
		vrc6_write(address, value);
	}
}

void nesapu_device::set_dpcm(u8 *dpcm_data, u32 length)
{
	if (length > 4081)
		length = 4081;
	memcpy(m_dpcm_buffer, dpcm_data, length);
}


//-------------------------------------------------
//  sound_stream_update - handle a stream update
//-------------------------------------------------

void nesapu_device::sound_stream_update(sound_stream &stream, stream_sample_t **inputs, stream_sample_t **outputs, int samples)
{
	int accum;
	memset(outputs[0], 0, samples * sizeof(*outputs[0]));
	memset(outputs[1], 0, samples * sizeof(*outputs[1]));

	int num = samples;
	while (samples--)
	{
		accum = apu_square(&m_APU.squ[0]);
		accum += apu_square(&m_APU.squ[1]);
		accum += apu_triangle(&m_APU.tri);
		accum += apu_noise(&m_APU.noi);
		accum += apu_dpcm(&m_APU.dpcm);
		accum += apu_fds(&m_APU.fds);
		accum += vrc6_RectangleRender(&m_APU.vrc6rect[0]);
		accum += vrc6_RectangleRender(&m_APU.vrc6rect[1]);
		accum += vrc6_SawtoothRender(&m_APU.vrc6saw);

		/* 8-bit clamps */
		if (accum > 127)
			accum = 127;
		else if (accum < -128)
			accum = -128;

		*(outputs[0]++) = accum << 8;
		*(outputs[1]++) = accum << 8;

	}

}
