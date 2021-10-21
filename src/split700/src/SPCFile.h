
#ifndef SPCFILE_H_INCLUDED
#define SPCFILE_H_INCLUDED

#include <stdint.h>

#include <string>
#include <vector>
#include <map>

#include "SPCSampDir.h"

class SPCFile
{
public:
	SPCFile();
	virtual ~SPCFile();

	enum ID666EmulatorId {
		ID666_EMU_UNKNOWN = 0x00,
		ID666_EMU_ZSNES = 0x01,
		ID666_EMU_SNES9X = 0x02,
		ID666_EMU_ZST2SPC = 0x03,
		ID666_EMU_SNESHOUT = 0x05,
		ID666_EMU_ZSNES_W = 0x07,
		ID666_EMU_SNESGT = 0x08
	};

	enum XID6ItemId {
		XID6_SONG_NAME = 0x01,
		XID6_GAME_NAME = 0x02,
		XID6_ARTIST_NAME = 0x03,
		XID6_DUMPER_NAME = 0x04,
		XID6_DUMPED_DATE = 0x05,
		XID6_EMULATOR = 0x06,
		XID6_COMMENT = 0x07,
		XID6_OST_TITLE = 0x10,
		XID6_OST_DISC = 0x11,
		XID6_OST_TRACK_NUMBER = 0x12,
		XID6_PUBLISHER_NAME = 0x13,
		XID6_COPYRIGHT_YEAR = 0x14,
		XID6_INTRO_LENGTH = 0x30,
		XID6_LOOP_LENGTH = 0x31,
		XID6_END_LENGTH = 0x32,
		XID6_FADE_LENGTH = 0x33,
		XID6_MUTED_VOICES = 0x34,
		XID6_LOOP_COUNT = 0x35,
		XID6_VOLUME = 0x36
	};

	enum XID6TypeId {
		XID6_TYPE_LENGTH = 0,
		XID6_TYPE_STRING = 1,
		XID6_TYPE_INTEGER = 4
	};

	struct XID6TagItem {
		XID6TypeId type;
		std::vector<char> value;
	};

	struct SPCRegisters {
		uint16_t pc;
		uint8_t a;
		uint8_t x;
		uint8_t y;
		uint8_t psw;
		uint8_t sp;
	} regs;

	uint8_t ram[0x10000];
	uint8_t dsp[0x80];
	uint8_t extra_ram[0x40];
	std::map<XID6ItemId, XID6TagItem> tags;

	SPCSampDir samples[256];
	int samp_dir_length;

	static bool IsSPCFile(const std::string& filename);
	static SPCFile * Load(const std::string& filename);
	bool Save(const std::string& filename) const;

	std::vector<uint8_t> GetXID6Block() const;

	bool DoesTagRequireXID6(XID6ItemId id) const;
	int GetIntegerTag(XID6ItemId id) const;
	std::string GetStringTag(XID6ItemId id) const;
	void SetLengthTag(XID6ItemId id, uint16_t value);
	void SetIntegerTag(XID6ItemId id, uint32_t value, size_t size);
	void SetStringTag(XID6ItemId id, const std::string & str);

	uint32_t GetPlaybackLength() const;

	static uint32_t XID6TicksToMilliSeconds(uint32_t ticks);
	static uint32_t MilliSecondsToXID6Ticks(uint32_t msecs);
	static std::string XID6TicksToTimeString(uint32_t ticks, bool padding);
	static uint32_t TimeStringToXID6Ticks(const std::string & str, bool * p_valid_format);

	static std::string ID666IdToEmulatorName(ID666EmulatorId id);
	static ID666EmulatorId EmulatorNameToID666Id(const std::string & name);

	bool ImportPSFTag(const std::map<std::string, std::string> & psf_tags);
	std::map<std::string, std::string> ExportPSFTag(bool unofficial_tags) const;

private:
	SPCFile(const SPCFile&);
	SPCFile& operator=(const SPCFile&);

	void ParseSampDir();
	static bool ParseDateString(const std::string & str, int & year, int & month, int & day);
	void SetTagValue(XID6ItemId id, XID6TypeId type, const uint8_t * binary, size_t size);
};

#endif /* !SPCFILE_H_INCLUDED */
