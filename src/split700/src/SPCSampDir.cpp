#include "pch.h"

#include <stdint.h>

#include <vector>

#include "SPCSampDir.h"

static inline int32_t sclip15(int32_t x) {
	return ((x & 16384) ? (x | ~16383) : (x & 16383));
}

static inline int32_t sclamp16(int32_t x) {
	return ((x > 32767) ? 32767 : (x < -32768) ? -32768 : x);
}

SPCSampDir::SPCSampDir() :
	start_address(0),
	loop_address(0),
	end_address(0),
	looped(false),
	valid(false) {
}

SPCSampDir::~SPCSampDir()
{
}

size_t SPCSampDir::read(const uint8_t * data, size_t size)
{
	if (size < 4) {
		return 0;
	}

	start_address = data[0] | (data[1] << 8);
	loop_address = data[2] | (data[3] << 8);
	return 4;
}

void SPCSampDir::parse_brr(const uint8_t * brr, size_t available_size)
{
	bool valid_range = true;
	bool valid_end = false;

	looped = false;

	size_t brr_size = 0;
	while (brr_size + BRR_CHUNK_SIZE <= available_size) {
		uint8_t flags = brr[brr_size];
		bool chunk_end = (flags & 1) != 0;
		bool chunk_loop = (flags & 2) != 0;
		uint8_t range = flags >> 4;

		if (!chunk_end && range > 0x0c) {
			// test case: Actraiser
			valid_range = false;
		}

		brr_size += 9;

		if (chunk_end) {
			if (chunk_loop) {
				looped = true;
			}

			valid_end = true;
			break;
		}
	}
	end_address = (uint16_t)(start_address + brr_size);
	valid = valid_end && valid_addresses();
}

std::vector<int16_t> SPCSampDir::decode_brr(const uint8_t * brr, size_t size, bool * ptr_looped)
{
	std::vector<int16_t> raw_samples;

	int32_t prev[2] = { 0, 0 };
	size_t decoded_size = 0;
	while (decoded_size + BRR_CHUNK_SIZE <= size) {
		const uint8_t * brr_chunk = &brr[decoded_size];

		uint8_t flags = brr_chunk[0];
		bool chunk_end = (flags & 1) != 0;
		bool chunk_loop = (flags & 2) != 0;
		uint8_t filter = (flags >> 2) & 3;
		uint8_t range = flags >> 4;
		bool valid_range = (range <= 0x0c);

		int32_t S1 = prev[0];
		int32_t S2 = prev[1];

		for (int byte_index = 0; byte_index < 8; byte_index++)
		{
			int8_t sample1 = brr_chunk[1 + byte_index];
			int8_t sample2 = sample1 << 4;
			sample1 >>= 4;
			sample2 >>= 4;

			for (int nybble = 0; nybble < 2; nybble++)
			{
				int32_t out;
				
				out = nybble ? (int32_t)sample2 : (int32_t)sample1;
				out = valid_range ? ((out << range) >> 1) : (out & ~0x7FF);

				switch (filter)
				{
				case 0: // Direct
					break;

				case 1: // 15/16
					out += S1 + ((-S1) >> 4);
					break;

				case 2: // 61/32 - 15/16
					out += (S1 << 1) + ((-((S1 << 1) + S1)) >> 5) - S2 + (S2 >> 4);
					break;

				case 3: // 115/64 - 13/16
					out += (S1 << 1) + ((-(S1 + (S1 << 2) + (S1 << 3))) >> 6) - S2 + (((S2 << 1) + S2) >> 4);
					break;
				}

				out = sclip15(sclamp16(out));

				S2 = S1;
				S1 = out;

				raw_samples.push_back(out << 1);
			}
		}

		prev[0] = S1;
		prev[1] = S2;

		decoded_size += BRR_CHUNK_SIZE;

		if (chunk_end) {
			if (ptr_looped != NULL) {
				*ptr_looped = chunk_loop;
			}
			break;
		}
	}

	return raw_samples;
}
