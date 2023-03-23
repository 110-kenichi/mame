/***
	c86ctl
	
	Copyright (c) 2009-2012, honet. All rights reserved.
	This software is licensed under the BSD license.

	honet.kk(at)gmail.com
 */

#include "stdafx.h"
#include "opx.h"

// noteの中間周波数*1024のテーブル
extern const uint32_t c86ctl::ftable[12] = {
	/*260277,*/  // C#<->C   : 254.1775933119Hz
	275754,  // C <->C#  : 269.291779527024Hz
	292152,  // C#<->D   : 285.304702023222Hz
	309524,  // D <->D#  : 302.26980244078Hz
	327929,  // D#<->E   : 320.243700225281Hz
	347429,  // E <->F   : 339.286381589747Hz
	368088,  // F <->F#  : 359.461399713042Hz
	389976,  // F#<->G   : 380.836086842703Hz
	413165,  // G <->G#  : 403.481779010055Hz
	437733,  // G#<->A   : 427.474054107587Hz
	463762,  // A <->A#  : 452.892984123137Hz
	491339,  // A#<->B   : 479.823402372713Hz
	/*520555,*/  // B <->C   : 508.3551866238Hz
	0xffffffff // stopper
};

extern const uint32_t c86ctl::fmin = 260277;//254177;  // C#<->C   : 254.1775933119Hz
extern const uint32_t c86ctl::fmax = c86ctl::fmin*2;

extern const char *c86ctl::noteStr[12] = { "C ", "C+", "D ", "D+", "E ", "F ", "F+", "G ", "G+", "A ", "A+", "B " };

