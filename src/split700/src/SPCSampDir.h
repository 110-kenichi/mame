
#ifndef SPCSAMPDIR_H_INCLUDED
#define SPCSAMPDIR_H_INCLUDED

#include <stdint.h>
#include <cstddef>

#include <vector>

class SPCSampDir {
public:
	SPCSampDir();
	virtual ~SPCSampDir();

	static const int BRR_CHUNK_SIZE = 9;

	uint16_t start_address; // start address (SA)
	uint16_t loop_address;  // loop start address (LSA)
	uint16_t end_address;

	bool looped;    // loop or one-shot
	bool valid;     // valid sample or garbage (can be mistaken)

	inline bool valid_addresses() const {
		if (start_address >= end_address) {
			return false;
		}

		// validate loop point
		if (looped) {
			// loop point must point to the head of brr chunk
			if ((loop_address - start_address) % BRR_CHUNK_SIZE != 0) {
				return false;
			}

			// loop point should be in between the start and the end
			if (loop_address < start_address || loop_address >= end_address) {
				return false;
			}
		}

		return true;
	}

	inline size_t compressed_size() const {
		if (start_address <= end_address) {
			return end_address - start_address;
		}
		else {
			return 0;
		}
	}

	inline int sample_count() const {
		if (start_address <= end_address) {
			return (end_address - start_address) / BRR_CHUNK_SIZE * 16;
		}
		else {
			return 0;
		}
	}

	inline int loop_sample() const {
		if (start_address <= loop_address) {
			return (loop_address - start_address) / BRR_CHUNK_SIZE * 16;
		}
		else {
			return 0;
		}
	}

	size_t read(const uint8_t * data, size_t size);
	void parse_brr(const uint8_t * brr, size_t available_size);

	static std::vector<int16_t> decode_brr(const uint8_t * brr, size_t size, bool * ptr_looped = NULL);
};

#endif /* !SPCSAMPDIR_H_INCLUDED */
