
#ifndef WAVWRITER_H
#define WAVWRITER_H

#include <stdint.h>

#include <vector>

class WavWriter
{
public:
	WavWriter();
	WavWriter(std::vector<int16_t> samples);
	virtual ~WavWriter();

	const std::vector<int16_t> & GetSamples() const {
		return samples;
	}

	inline int32_t GetLoopSample() const {
		return loop_sample;
	}

	inline void SetLoopSample(int32_t loop_sample) {
		this->loop_sample = loop_sample;
		this->looped = true;
	}

	inline void SetNoLoop() {
		looped = false;
	}

	inline const std::string & message() const {
		return m_message;
	}

	void AddSample(int16_t sample);
	void AddSample(std::vector<int16_t> samples);
	bool WriteFile(const std::string & filename);

	int16_t channels;
	int32_t samplerate;
	int16_t bitwidth;

protected:
	std::string m_message;

	std::vector<int16_t> samples;
	int32_t loop_sample;
	bool looped;
};

#endif
