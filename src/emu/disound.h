// license:BSD-3-Clause
// copyright-holders:Aaron Giles
/***************************************************************************

    disound.h

    Device sound interfaces.

***************************************************************************/

#pragma once

#ifndef __EMU_H__
#error Dont include this file directly; include emu.h instead.
#endif

#ifndef MAME_EMU_DISOUND_H
#define MAME_EMU_DISOUND_H

#include <functional>
#include <utility>


//**************************************************************************
//  CONSTANTS
//**************************************************************************

constexpr int ALL_OUTPUTS       = 65535;    // special value indicating all outputs for the current chip
constexpr int AUTO_ALLOC_INPUT  = 65535;



//**************************************************************************
//  TYPE DEFINITIONS
//**************************************************************************

typedef void(*VST_FX_CALLBACK)(stream_sample_t **buffer, int samples);
typedef void(*STREAM_UPDATE_CALLBACK)(int32_t *buffer, int32_t size);

// ======================> device_sound_interface

class device_sound_interface : public device_interface
{
public:
	class sound_route
	{
	public:
		u32                                 m_output;           // output index, or ALL_OUTPUTS
		u32                                 m_input;            // target input index
		u32                                 m_mixoutput;        // target mixer output
		float                               m_gain;             // gain
		std::reference_wrapper<device_t>    m_base;             // target search base
		std::string                         m_target;           // target tag
	};

	// construction/destruction
	device_sound_interface(const machine_config &mconfig, device_t &device);
	virtual ~device_sound_interface();

	virtual bool issound() { return true; } /// HACK: allow devices to hide from the ui

	// configuration access
	std::vector<sound_route> const &routes() const { return m_route_list; }

	// configuration helpers
	template <typename T, bool R>
	device_sound_interface &add_route(u32 output, const device_finder<T, R> &target, double gain, u32 input = AUTO_ALLOC_INPUT, u32 mixoutput = 0)
	{
		const std::pair<device_t &, const char *> ft(target.finder_target());
		return add_route(output, ft.first, ft.second, gain, input, mixoutput);
	}
	device_sound_interface &add_route(u32 output, const char *target, double gain, u32 input = AUTO_ALLOC_INPUT, u32 mixoutput = 0);
	device_sound_interface &add_route(u32 output, device_sound_interface &target, double gain, u32 input = AUTO_ALLOC_INPUT, u32 mixoutput = 0);
	device_sound_interface &add_route(u32 output, speaker_device &target, double gain, u32 input = AUTO_ALLOC_INPUT, u32 mixoutput = 0);
	device_sound_interface &reset_routes() { m_route_list.clear(); return *this; }

	// sound stream update overrides
	void sound_stream_update_callback(sound_stream &stream, stream_sample_t **inputs, stream_sample_t **outputs, int samples);

	void set_stream_update_callback(STREAM_UPDATE_CALLBACK callback) { m_stream_update_callback = callback; };

	void set_vst_fx_callback(VST_FX_CALLBACK callback) { m_vst_fx_callback = callback; };
	void apply_filter(stream_sample_t **inputs, int samples);

	// stream creation
	sound_stream *stream_alloc(int inputs, int outputs, int sample_rate);

	// helpers
	int inputs() const;
	int outputs() const;
	sound_stream *input_to_stream_input(int inputnum, int &stream_inputnum) const;
	sound_stream *output_to_stream_output(int outputnum, int &stream_outputnum) const;
	float input_gain(int inputnum) const;
	float output_gain(int outputnum) const;
	void set_input_gain(int inputnum, float gain);
	void set_output_gain(int outputnum, float gain);
	int inputnum_from_device(device_t &device, int outputnum = 0) const;
	volatile int m_enable;
	virtual void set_enable(int enable) { m_enable = enable; }

	//
	//  Filter.h
	//  Synthesis
	//
	//  Created by Martin on 08.04.14.
	//
	//
	enum FilterMode {
		FILTER_MODE_NONE = 0,
		FILTER_MODE_LOWPASS,
		FILTER_MODE_HIGHPASS,
		FILTER_MODE_BANDPASS,
		kNumFilterModes
	};
	double process(int ch, double inputValue);
	inline void setCutoff(double newCutoff) { cutoff = newCutoff; calculateFeedbackAmount(); };
	inline void setResonance(double newResonance) { resonance = newResonance; calculateFeedbackAmount(); };
	inline void setFilterMode(FilterMode newMode) { mode = newMode; }

	inline void setCutoffMod(double newCutoffMod) {
		cutoffMod = newCutoffMod;
		calculateFeedbackAmount();
	}

	stream_sample_t *lastOutBuffer;
	unsigned int lastOutBufferSamples;
	unsigned int lastOutBufferNumber;

	virtual void vgm_start(char *name) {};
	virtual void vgm_stop(void) {};

protected:
	virtual void sound_stream_update(sound_stream &stream, stream_sample_t **inputs, stream_sample_t **outputs, int samples) = 0;

	// configuration access
	std::vector<sound_route> &routes() { return m_route_list; }
	device_sound_interface &add_route(u32 output, device_t &base, const char *tag, double gain, u32 input, u32 mixoutput);

	// optional operation overrides
	virtual void interface_validity_check(validity_checker &valid) const override;
	virtual void interface_pre_start() override;
	virtual void interface_post_start() override;
	virtual void interface_pre_reset() override;

	// internal state
	std::vector<sound_route> m_route_list;      // list of sound routes
	int             m_outputs;                  // number of outputs from this instance
	int             m_auto_allocated_inputs;    // number of auto-allocated inputs targeting us

	STREAM_UPDATE_CALLBACK m_stream_update_callback;

private:
	double cutoff;
	double resonance;
	FilterMode mode;
	double feedbackAmount;
	inline void calculateFeedbackAmount() {
		feedbackAmount = resonance + resonance / (1.0 - getCalculatedCutoff());
	}
	double buf0[2];
	double buf1[2];
	double buf2[2];
	double buf3[2];
	double lastIn[2];
	double lastOut[2];

	double cutoffMod;

	inline double getCalculatedCutoff() const {
		return fmax(fmin(cutoff + cutoffMod, 0.99), 0.01);
	};

	VST_FX_CALLBACK m_vst_fx_callback;
};

// iterator
typedef device_interface_iterator<device_sound_interface> sound_interface_iterator;



// ======================> device_mixer_interface

class device_mixer_interface : public device_sound_interface
{
public:
	// construction/destruction
	device_mixer_interface(const machine_config &mconfig, device_t &device, int outputs = 1);
	virtual ~device_mixer_interface();

protected:
	// optional operation overrides
	virtual void interface_pre_start() override;
	virtual void interface_post_load() override;

	// sound interface overrides
	virtual void sound_stream_update(sound_stream &stream, stream_sample_t **inputs, stream_sample_t **outputs, int samples) override;

	// internal state
	u8                  m_outputs;              // number of outputs
	std::vector<u8>     m_outputmap;            // map of inputs to outputs
	sound_stream *      m_mixer_stream;         // mixing stream
};

// iterator
typedef device_interface_iterator<device_mixer_interface> mixer_interface_iterator;


#endif // MAME_EMU_DISOUND_H
