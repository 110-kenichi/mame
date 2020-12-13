// copyright-holders:K.Ito
#ifndef MAME_SOUND_CM32P
#define MAME_SOUND_CM32P

#pragma once

#include "..\..\FluidLite\include\fluidlite.h"
//#include "cm32p_BReverbModel.h"

//**************************************************************************
//  TYPE DEFINITIONS
//**************************************************************************


#ifdef _MSC_VER
#define  ALIGN_PACKED __declspec(align(1))
#else
#define ALIGN_PACKED __attribute__((packed))
#endif

#define CM32P_MEMADDR(x) ((((x) & 0x7f0000) >> 2) | (((x) & 0x7f00) >> 1) | ((x) & 0x7f))
#define CM32P_SYSEXMEMADDR(x) ((((x) & 0x1FC000) << 2) | (((x) & 0x3F80) << 1) | ((x) & 0x7f))

const u8 SYSEX_MANUFACTURER_ROLAND = 0x41;

const u8 SYSEX_MDL_CM32P = 0x16;

const u8 SYSEX_CMD_RQ1 = 0x11; // Request data #1
const u8 SYSEX_CMD_DT1 = 0x12; // Data set 1
const u8 SYSEX_CMD_WSD = 0x40; // Want to send data
const u8 SYSEX_CMD_RQD = 0x41; // Request data
const u8 SYSEX_CMD_DAT = 0x42; // Data set
const u8 SYSEX_CMD_ACK = 0x43; // Acknowledge
const u8 SYSEX_CMD_EOD = 0x45; // End of data
const u8 SYSEX_CMD_ERR = 0x4E; // Communications error
const u8 SYSEX_CMD_RJC = 0x4F; // Rejection

const unsigned int SYSTEM_MASTER_TUNE_OFF = 0;
const unsigned int SYSTEM_REVERB_MODE_OFF = 1;
const unsigned int SYSTEM_REVERB_TIME_OFF = 2;
const unsigned int SYSTEM_REVERB_LEVEL_OFF = 3;
const unsigned int SYSTEM_RESERVE_SETTINGS_START_OFF = 4;
const unsigned int SYSTEM_RESERVE_SETTINGS_END_OFF = 9;
const unsigned int SYSTEM_CHAN_ASSIGN_START_OFF = 0xa;
const unsigned int SYSTEM_CHAN_ASSIGN_END_OFF = 0xf;
const unsigned int SYSTEM_MASTER_VOL_OFF = 0x10;

const float DEFAULT_GAIN_BASE = 1000.0f;

struct PatchParam {
	u8 toneMedia; // TONE MEDIA 0 - 1 Internal,Card
	u8 toneNumber; // TONE NUMBER 0 -127
	u8 keyShift; // KEY SHIFT 0-24 (-12 - +12 semitones)
	u8 fineTune; // FINE TUNE 0-100 (-50 - +50 cents)
	u8 benderRange; // BENDER RANGE 0-12
	u8 keyRangeLower; // KEY RANGE LOWER 0 - 127 C1 - G9
	u8 keyRangeUpper; // KEY RANGE UPPER 0 - 127 C1 - G9
	u8 assignMode;  // ASSIGN MODE 0-3 (POLY1, POLY2, POLY3, POLY4)
	u8 reverbSwitch;  // REVERB SWITCH 0-1 (OFF,ON)
	u8 velocitySense;  // VELOCITY SENSE 0 - 15
	u8 envAttackRate;  // ENV ATTACK RATE 0 - 127
	u8 envReleaseRate;  // ENV RELEASE RATE 0 - 127
	u8 lfoRate;  // LFO RATE 0 - 127
	u8 lfoAutoDelayTime;  // LFO AUTO DELAY TIME 0 - 15
	u8 lfoAutoRiseTime;  // LFO AUTO RISE TIME 0 - 15
	u8 lfoAutoDepth;  // LFO AUTO DEPTH 0 - 15
	u8 lfoManRiseTime;  // LFO MAN RISE TIME 0 - 15
	u8 lfoManDepth;  // LFO MAN DEPTH 0 - 15
	u8 detuneDepth;  // DETUNE DEPTH 0 - 50
} ALIGN_PACKED;

struct MemParams {
	// NOTE: The MT-32 documentation only specifies PatchTemp areas for parts 1-8.
	// The LAPC-I documentation specified an additional area for rhythm at the end,
	// where all parameters but fine tune, assign mode and output level are ignored
	struct PatchTemp {
		PatchParam patch;
		u8 panpot; // PANPOT 0-127 (R-L)
		u8 outputLevel; // OUTPUT LEVEL 0-100
	} ALIGN_PACKED patchTemp[16];

	PatchParam patches[128];

	struct System {
		u8 masterTune; // MASTER TUNE 0-127 432.1-457.6Hz
		u8 reverbMode; // REVERB MODE 0-3 (room, hall, plate, tap delay)
		u8 reverbTime; // REVERB TIME 0-7 (1-8)
		u8 reverbLevel; // REVERB LEVEL 0-7 (1-8)
		u8 reserveSettings[6]; // PARTIAL RESERVE (PART 1) 0-31
		u8 chanAssign[6]; // MIDI CHANNEL (PART1) 0-16 (1-16,OFF)
		u8 masterVol; // MASTER VOLUME 0-100
		u8 chanAssign2[10]; // MIDI CHANNEL (PART1) 0-16 (1-16,OFF)
	} ALIGN_PACKED system;
};

enum ReverbMode {
	REVERB_MODE_ROOM,
	REVERB_MODE_HALL,
	REVERB_MODE_PLATE,
	REVERB_MODE_TAP_DELAY
};

enum ToneMedia {
	TONE_MEDIA_INTERNAL,
	TONE_MEDIA_CARD
};


enum MemoryRegionType {
	MR_PatchTemp, MR_Patches, MR_System, MR_Reset
};

class MemoryRegion;
class PatchTempMemoryRegion;
class PatchesMemoryRegion;
class SystemMemoryRegion;
class ResetMemoryRegion;
class CM32P_BReverbModel;

class cm32p_device : public device_t,
	public device_sound_interface
{
public:
	cm32p_device(const machine_config &mconfig, const char *tag, device_t *owner, uint32_t clock);

	void play_msg(u8 type, u8 channel, u32 param1, u32 param2);

	void play_sysex(const u8 *sysex, u32 len);

	fluid_sfont_t *load_sf(u8 card_id, const char* filename);

	void add_sf(u8 card_id, fluid_sfont_t *sf);

	void set_tone(u8 card_id, u16 tone_no, u16 sf_preset_no);

	void set_card(u8 card_id);

	void set_enable(int enable);

	void initialize_memory();

	void set_chanAssign(u8* assign);

	void get_chanAssign(u8* assign);

protected:
	// device-level overrides
	virtual void device_start() override;

	// sound stream update overrides
	virtual void sound_stream_update(sound_stream &stream, stream_sample_t **inputs, stream_sample_t **outputs, int samples) override;

public:

private:
	sound_stream * m_stream;   /* stream number */
	int m_frequency;          /* set frequency - this can be changed using the appropriate function */

	fluid_settings_t *settings;
	fluid_synth_t *synth_rev_on;
	fluid_synth_t *synth_rev_off;

	u8 bank_no[16] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	u8 rpn_lsb[16] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	u8 rpn_msb[16] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	u8 rpn_data[16] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	u8 rpn_type[16] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

	std::map<u32, u16> tone_table;
	std::map<u8, unsigned int> sf_table;
	u8 card_id;

	MemParams &cm32p_ram;
	int memory_initialized;

	CM32P_BReverbModel *reverbModels[4];
	float clipping_overflow_l;
	float clipping_overflow_r;

	const u8 default_patch_table_no[128] =
	{
		 0, 2, 3, 4, 5, 6, 8,10,12,14,15,17,18,20,21,26,
		27,28,29,32,33,34,35,36,37,38,39,49,42,43,44,45,
		46,47,48,49,50,52,54,56,58,59,60,61,62,63,64,66,
		67,68,69,70,71,72,73,74,75,77,78,79,80,81,82,83,
		 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,11,12,13,14,15,
		16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,
		32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,
		48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,
	};

	const u8 default_patch_table_media[128] =
	{
		 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,

		 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
	};

	void program_select(u8 channel, u8 tone_media, u8 bank_no, u8 tone_no);

	void playSysexWithoutFraming(const u8 *sysex, u32 len);

	void playSysexWithoutHeader(u8 device, u8 command, const u8 *sysex, u32 len);

	u8 calcSysexChecksum(const u8 *data, const u32 len, const u8 initChecksum = 0);

	void writeSysex(u8 device, const u8 *sysex, u32 len);

	void writeSysexGlobal(u32 addr, const u8 *sysex, u32 len);

	void readMemory(u32 addr, u32 len, u8 *data);

	void initMemoryRegions();

	void deleteMemoryRegions();

	MemoryRegion *findMemoryRegion(u32 addr);

	void readMemoryRegion(const MemoryRegion *region, u32 addr, u32 len, u8 *data);

	void writeMemoryRegion(const MemoryRegion *region, u32 addr, u32 len, const u8 *data);

	void applyPatchParameters(const u8 &channel, PatchParam &pp, MemParams::PatchTemp &pt);

	PatchTempMemoryRegion *patchTempMemoryRegion;
	PatchesMemoryRegion *patchesMemoryRegion;
	SystemMemoryRegion *systemMemoryRegion;
	ResetMemoryRegion *resetMemoryRegion;
};


class MemoryRegion {
private:
	cm32p_device * synth;
	u8 *realMemory;
	u8 *maxTable;
public:
	MemoryRegionType type;
	u32 startAddr, entrySize, entries;

	MemoryRegion(cm32p_device *useSynth, u8 *useRealMemory, u8 *useMaxTable, MemoryRegionType useType, u32 useStartAddr, u32 useEntrySize, u32 useEntries) {
		synth = useSynth;
		realMemory = useRealMemory;
		maxTable = useMaxTable;
		type = useType;
		startAddr = useStartAddr;
		entrySize = useEntrySize;
		entries = useEntries;
	}
	int lastTouched(u32 addr, u32 len) const {
		return (offset(addr) + len - 1) / entrySize;
	}
	int firstTouchedOffset(u32 addr) const {
		return offset(addr) % entrySize;
	}
	int firstTouched(u32 addr) const {
		return offset(addr) / entrySize;
	}
	u32 regionEnd() const {
		return startAddr + entrySize * entries;
	}
	bool contains(u32 addr) const {
		return addr >= startAddr && addr < regionEnd();
	}
	int offset(u32 addr) const {
		return addr - startAddr;
	}
	u32 getClampedLen(u32 addr, u32 len) const {
		if (addr + len > regionEnd())
			return regionEnd() - addr;
		return len;
	}
	u32 next(u32 addr, u32 len) const {
		if (addr + len > regionEnd()) {
			return regionEnd() - addr;
		}
		return 0;
	}
	u8 getMaxValue(int off) const {
		if (maxTable == NULL)
			return 0xFF;
		return maxTable[off % entrySize];
	}
	u8 *getRealMemory() const {
		return realMemory;
	}
	bool isReadable() const {
		return getRealMemory() != NULL;
	}
	void read(unsigned int entry, unsigned int off, u8 *dst, unsigned int len) const;
	void write(unsigned int entry, unsigned int off, const u8 *src, unsigned int len, bool init = false) const;
}; // class MemoryRegion


class PatchTempMemoryRegion : public MemoryRegion {
public:
	PatchTempMemoryRegion(cm32p_device *useSynth, u8 *useRealMemory, u8 *useMaxTable) : MemoryRegion(useSynth, useRealMemory, useMaxTable, MR_PatchTemp, CM32P_MEMADDR(0x500000), sizeof(MemParams::PatchTemp), 16) {}
};
class PatchesMemoryRegion : public MemoryRegion {
public:
	PatchesMemoryRegion(cm32p_device *useSynth, u8 *useRealMemory, u8 *useMaxTable) : MemoryRegion(useSynth, useRealMemory, useMaxTable, MR_Patches, CM32P_MEMADDR(0x510000), sizeof(PatchParam), 128) {}
};
class SystemMemoryRegion : public MemoryRegion {
public:
	SystemMemoryRegion(cm32p_device *useSynth, u8 *useRealMemory, u8 *useMaxTable) : MemoryRegion(useSynth, useRealMemory, useMaxTable, MR_System, CM32P_MEMADDR(0x520000), sizeof(MemParams::System), 1) {}
};
class ResetMemoryRegion : public MemoryRegion {
public:
	ResetMemoryRegion(cm32p_device *useSynth) : MemoryRegion(useSynth, NULL, NULL, MR_Reset, CM32P_MEMADDR(0x7F0000), 0x3FFF, 1) {}
};

DECLARE_DEVICE_TYPE(CM32P, cm32p_device)

#endif
