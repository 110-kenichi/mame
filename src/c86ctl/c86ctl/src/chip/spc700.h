#pragma once

#include "chip.h"

namespace c86ctl {

	// ---------------------------------------------------------------------------------------
	class CSPC700 : public Chip
	{
	public:
		CSPC700() { reset(); };
		virtual ~CSPC700() {};

		void reset() {
			memset(reg, 0, 256);
		};

		void update() {
			//int dc = 8;
			//for (int j = 0; j < 2; j++) {
			//	for (int i = 0; i < 256; i++) {
			//		UCHAR c = regATime[j][i];
			//		if (64 < c) {
			//			c -= dc;
			//			regATime[j][i] = 64 < c ? c : 64;
			//		}
			//	}
			//}
		};

	public:
		bool setReg(int addr, UCHAR data) {
			if (0x100 <= addr)
				return false;

			reg[addr] = data;
			return true;
		}
		UCHAR getReg(int addr) {
			if (addr < 0x100)
				return reg[addr & 0xff];
			return 0;
		};

	public:
		UCHAR reg[256];
	};

};

