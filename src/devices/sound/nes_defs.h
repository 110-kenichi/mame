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

 *****************************************************************************

   NES_DEFS.H

   NES APU internal type definitions and constants.

 *****************************************************************************/

#ifndef MAME_SOUND_NES_DEFS_H
#define MAME_SOUND_NES_DEFS_H

#pragma once

#define	INT2FIX(x)	((x)<<16)
#define	FIX2INT(x)	((x)>>16)
#define	RECTANGLE_VOL_SHIFT	8
#define	SAWTOOTH_VOL_SHIFT	6

/* APU type */
struct apu_t
{
	/* REGULAR TYPE DEFINITIONS */
	typedef int8_t          int8;
	typedef int16_t         int16;
	typedef int32_t         int32;
	typedef uint8_t         uint8;
	typedef uint16_t        uint16;
	typedef uint32_t        uint32;


	/* CHANNEL TYPE DEFINITIONS */

	/* Square Wave */
	struct square_t
	{
		square_t()
		{
			for (auto & elem : regs)
				elem = 0;
		}

		uint8 regs[4];
		int vbl_length = 0;
		int freq = 0;
		float phaseacc = 0.0;
		float output_vol = 0.0;
		float env_phase = 0.0;
		float sweep_phase = 0.0;
		uint8 adder = 0;
		uint8 env_vol = 0;
		bool enabled = false;
	};

	/* Triangle Wave */
	struct triangle_t
	{
		triangle_t()
		{
			for (auto & elem : regs)
				elem = 0;
		}

		uint8 regs[4]; /* regs[1] unused */
		int linear_length = 0;
		int vbl_length = 0;
		int write_latency = 0;
		float phaseacc = 0.0;
		float output_vol = 0.0;
		uint8 adder = 0;
		bool counter_started = false;
		bool enabled = false;
	};

	/* Noise Wave */
	struct noise_t
	{
		noise_t()
		{
			for (auto & elem : regs)
				elem = 0;
		}

		uint8 regs[4]; /* regs[1] unused */
		u32 seed = 1;
		int vbl_length = 0;
		float phaseacc = 0.0;
		float output_vol = 0.0;
		float env_phase = 0.0;
		uint8 env_vol = 0;
		bool enabled = false;
	};

	/* DPCM Wave */
	struct dpcm_t
	{
		dpcm_t()
		{
			for (auto & elem : regs)
				elem = 0;
		}

		uint8 regs[4];
		uint32 address = 0;
		uint32 length = 0;
		int bits_left = 0;
		float phaseacc = 0.0;
		float output_vol = 0.0;
		uint8 cur_byte = 0;
		bool enabled = false;
		bool irq_occurred = false;
		signed char vol = 0;
	};

	/* FDS */ //VirtuaNES
	struct	fds_t {
		fds_t()
		{
			for (auto & elem : reg)
				elem = 0;
			for (auto & elem : main_wavetable)
				elem = 0;
			for (auto & elem : lfo_wavetable)
				elem = 0;
			for (auto & elem : output_buf)
				elem = 0;
		}

		uint8	reg[0x80];

		uint8	volenv_mode;		// Volume Envelope
		uint8	volenv_gain;
		uint8	volenv_decay;
		double	volenv_phaseacc;

		uint8	swpenv_mode;		// Sweep Envelope
		uint8	swpenv_gain;
		uint8	swpenv_decay;
		double	swpenv_phaseacc;

		// For envelope unit
		uint8	envelope_enable;	// $4083 bit6
		uint8	envelope_speed;		// $408A

									// For $4089
		uint8	wave_setup;		// bit7
		int	master_volume;		// bit1-0

								// For Main unit
		int	main_wavetable[64];
		uint8	main_enable;
		int	main_frequency;
		int	main_addr;

		// For Effector(LFO) unit
		uint8	lfo_wavetable[64];
		uint8	lfo_enable;		// 0:Enable 1:Wavetable setup
		int	lfo_frequency;
		int	lfo_addr;
		double	lfo_phaseacc;

		// For Sweep unit
		int	sweep_bias;

		// Misc
		int	now_volume;
		int	now_freq;
		int	output;
		int	output_buf[8];
	};

	struct VRC6_RECT_t {
		VRC6_RECT_t()
		{
			for (auto & elem : reg)
				elem = 0;
		}

		uint8	reg[3];

		uint8	enable;
		uint8	gate;
		uint8	volume;

		int	phaseacc;
		int	freq;
		int	output_vol;

		uint8	adder;
		uint8	duty_pos;
	};

	struct VRC6_SAW_t{
		VRC6_SAW_t()
		{
			for (auto & elem : reg)
				elem = 0;
		}

		uint8	reg[3];

		uint8	enable;
		uint8	volume;

		int	phaseacc;
		int	freq;
		int	output_vol;

		uint8	adder;
		uint8	accum;
		uint8	phaseaccum;
	};

	/* REGISTER DEFINITIONS */
	static constexpr unsigned WRA0    = 0x00;
	static constexpr unsigned WRA1    = 0x01;
	static constexpr unsigned WRA2    = 0x02;
	static constexpr unsigned WRA3    = 0x03;
	static constexpr unsigned WRB0    = 0x04;
	static constexpr unsigned WRB1    = 0x05;
	static constexpr unsigned WRB2    = 0x06;
	static constexpr unsigned WRB3    = 0x07;
	static constexpr unsigned WRC0    = 0x08;
	static constexpr unsigned WRC2    = 0x0A;
	static constexpr unsigned WRC3    = 0x0B;
	static constexpr unsigned WRD0    = 0x0C;
	static constexpr unsigned WRD2    = 0x0E;
	static constexpr unsigned WRD3    = 0x0F;
	static constexpr unsigned WRE0    = 0x10;
	static constexpr unsigned WRE1    = 0x11;
	static constexpr unsigned WRE2    = 0x12;
	static constexpr unsigned WRE3    = 0x13;
	static constexpr unsigned SMASK   = 0x15;
	static constexpr unsigned IRQCTRL = 0x17;

	apu_t()
	{
		memset(regs, 0, sizeof(regs));
	}

	/* Sound channels */
	square_t   squ[2];
	triangle_t tri;
	noise_t    noi;
	dpcm_t     dpcm;
	fds_t     fds;
	VRC6_RECT_t vrc6rect[2];
	VRC6_SAW_t vrc6saw;

	/* APU registers */
	unsigned char regs[0xff];

	/* Sound pointers */
	void *buffer = nullptr;

#ifdef USE_QUEUE

	static constexpr unsigned QUEUE_SIZE = 0x2000;
	static constexpr unsigned QUEUE_MAX  = QUEUE_SIZE - 1;

	struct queue_t
	{
		queue_t() { }

		int pos = 0;
		unsigned char reg = 0, val = 0;
	};

	/* Event queue */
	queue_t queue[QUEUE_SIZE];
	int head, tail;

#else

	int buf_pos = 0;

#endif

	int step_mode = 0;
};

/* CONSTANTS */

/* vblank length table used for squares, triangle, noise */
static const apu_t::uint8 vbl_length[32] =
{
	5, 127, 10, 1, 19,  2, 40,  3, 80,  4, 30,  5, 7,  6, 13,  7,
	6,   8, 12, 9, 24, 10, 48, 11, 96, 12, 36, 13, 8, 14, 16, 15
};

/* frequency limit of square channels */
static const int freq_limit[8] =
{
	0x3FF, 0x555, 0x666, 0x71C, 0x787, 0x7C1, 0x7E0, 0x7F0,
};

/* table of noise frequencies */
static const int noise_freq[16] =
{
	4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 2046
};

/* dpcm transfer freqs */
static const int dpcm_clocks[16] =
{
	428, 380, 340, 320, 286, 254, 226, 214, 190, 160, 142, 128, 106, 85, 72, 54
};

/* ratios of pos/neg pulse for square waves */
/* 2/16 = 12.5%, 4/16 = 25%, 8/16 = 50%, 12/16 = 75% */
static const int duty_lut[4] =
{
	2, 4, 8, 12
};

#endif // MAME_SOUND_NES_DEFS_H
