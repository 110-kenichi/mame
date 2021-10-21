#include "pch.h"

#include <stdint.h>

#include <string>
#include <vector>
#include <iterator>

#include "WavWriter.h"

static void write16(std::vector<uint8_t> & data, uint16_t value)
{
	data.push_back(value & 0xff);
	data.push_back((value >> 8) & 0xff);
}

static void write32(std::vector<uint8_t> & data, uint32_t value)
{
	data.push_back(value & 0xff);
	data.push_back((value >> 8) & 0xff);
	data.push_back((value >> 16) & 0xff);
	data.push_back((value >> 24) & 0xff);
}

static void write(std::vector<uint8_t> & data, const char * value, size_t size)
{
	for (size_t i = 0; i < size; i++) {
		data.push_back(value[i]);
	}
}

WavWriter::WavWriter() :
	channels(1),
	samplerate(44100),
	bitwidth(16),
	loop_sample(0),
	looped(false)
{
}

WavWriter::WavWriter(std::vector<int16_t> samples) :
	channels(1),
	samplerate(44100),
	bitwidth(16),
	loop_sample(0),
	looped(false)
{
	this->samples.assign(samples.begin(), samples.end());
}

WavWriter::~WavWriter()
{
}

