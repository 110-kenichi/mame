#pragma once

#ifndef __VGMWRITE_H__
#define __VGMWRITE_H__

// VGM Chip Constants
// v1.00
#define VGMC_SN76496	0x00
#define VGMC_YM2413		0x01
#define VGMC_YM2612		0x02
#define VGMC_YM2151		0x03
// v1.51
#define VGMC_SEGAPCM	0x04
#define VGMC_RF5C68		0x05
#define VGMC_YM2203		0x06
#define VGMC_YM2608		0x07
#define VGMC_YM2610		0x08
#define VGMC_YM3812		0x09
#define VGMC_YM3526		0x0A
#define VGMC_Y8950		0x0B
#define VGMC_YMF262		0x0C
#define VGMC_YMF278B	0x0D
#define VGMC_YMF271		0x0E
#define VGMC_YMZ280B	0x0F
#define VGMC_T6W28		0x7F	// note: emulated via 2xSN76496
#define VGMC_RF5C164	0x10
#define VGMC_PWM		0x11
#define VGMC_AY8910		0x12
// v1.61
#define VGMC_GBSOUND	0x13
#define VGMC_NESAPU		0x14
#define VGMC_MULTIPCM	0x15
#define VGMC_UPD7759	0x16
#define VGMC_OKIM6258	0x17
#define VGMC_OKIM6295	0x18
#define VGMC_K051649	0x19
#define VGMC_K054539	0x1A
#define VGMC_C6280		0x1B
#define VGMC_C140		0x1C
#define VGMC_K053260	0x1D
#define VGMC_POKEY		0x1E
#define VGMC_QSOUND		0x1F
// v1.71
#define VGMC_SCSP		0x20
#define VGMC_WSWAN		0x21
#define VGMC_VSU		0x22
#define VGMC_SAA1099	0x23
#define VGMC_ES5503		0x24
#define VGMC_ES5506		0x25
#define VGMC_X1_010		0x26
#define VGMC_C352		0x27
#define VGMC_GA20		0x28

typedef uint16_t gd3char_t;	// UTF-16 character, for VGM GD3 tag

typedef struct _vgm_file_header VGM_HEADER;
struct _vgm_file_header
{
	uint32_t fccVGM;
	uint32_t lngEOFOffset;
	uint32_t lngVersion;
	uint32_t lngHzPSG;
	uint32_t lngHz2413;
	uint32_t lngGD3Offset;
	uint32_t lngTotalSamples;
	uint32_t lngLoopOffset;
	uint32_t lngLoopSamples;
	uint32_t lngRate;
	uint16_t shtPSG_Feedback;
	uint8_t bytPSG_SRWidth;
	uint8_t bytPSG_Flags;
	uint32_t lngHz2612;
	uint32_t lngHz2151;
	uint32_t lngDataOffset;
	uint32_t lngHzSPCM;
	uint32_t lngSPCMIntf;

	uint32_t lngHzRF5C68;
	uint32_t lngHz2203;
	uint32_t lngHz2608;
	uint32_t lngHz2610;
	uint32_t lngHz3812;
	uint32_t lngHz3526;
	uint32_t lngHz8950;
	uint32_t lngHz262;
	uint32_t lngHz278B;
	uint32_t lngHz271;
	uint32_t lngHz280B;
	uint32_t lngHzRF5C164;
	uint32_t lngHzPWM;
	uint32_t lngHzAY8910;
	uint8_t lngAYType;
	uint8_t lngAYFlags;
	uint8_t lngAYFlagsYM2203;
	uint8_t lngAYFlagsYM2608;
	uint8_t bytModifiers[0x04];

	uint32_t lngHzGBDMG;		// part of the LR35902 (GB Main CPU)
	uint32_t lngHzNESAPU;		// part of the N2A03 (NES Main CPU)
	uint32_t lngHzMultiPCM;
	uint32_t lngHzUPD7759;
	uint32_t lngHzOKIM6258;
	uint8_t bytOKI6258Flags;
	uint8_t bytK054539Flags;
	uint8_t bytC140Type;
	uint8_t bytReservedFlags;
	uint32_t lngHzOKIM6295;
	uint32_t lngHzK051649;
	uint32_t lngHzK054539;
	uint32_t lngHzHuC6280;
	uint32_t lngHzC140;
	uint32_t lngHzK053260;
	uint32_t lngHzPokey;
	uint32_t lngHzQSound;
	uint32_t lngHzSCSP;
	uint32_t lngExtraOfs;

	uint32_t lngHzWSwan;
	uint32_t lngHzVSU;
	uint32_t lngHzSAA1099;
	uint32_t lngHzES5503;
	uint32_t lngHzES5506;
	uint8_t bytES5503Chns;
	uint8_t bytES5506Chns;
	uint8_t bytC352ClkDiv;
	uint8_t bytESReserved;
	uint32_t lngHzX1_010;
	uint32_t lngHzC352;

	uint32_t lngHzGA20;
	uint8_t bytReserved[0x1C];
};	// -> 0x100 Bytes
typedef struct _vgm_gd3_tag GD3_TAG;
struct _vgm_gd3_tag
{
	uint32_t fccGD3;
	uint32_t lngVersion;
	uint32_t lngTagLength;
	gd3char_t strTrackNameE[0x70];
	gd3char_t strTrackNameJ[0x10];	// Japanese Names are not used
	gd3char_t strGameNameE[0x70];
	gd3char_t strGameNameJ[0x10];
	gd3char_t strSystemNameE[0x30];
	gd3char_t strSystemNameJ[0x10];
	gd3char_t strAuthorNameE[0x30];
	gd3char_t strAuthorNameJ[0x10];
	gd3char_t strReleaseDate[0x10];
	gd3char_t strCreator[0x20];
	gd3char_t strNotes[0x50];
};	// -> 0x200 Bytes

typedef struct _vgm_rom_data_block VGM_ROM_DATA;
struct _vgm_rom_data_block
{
	uint8_t Type;
	uint8_t dstart_msb;
	uint32_t DataSize;
	const void* Data;
};
typedef struct _vgm_rom_init_command VGM_INIT_CMD;
struct _vgm_rom_init_command
{
	uint8_t CmdLen;
	uint8_t Data[0x08];
};
typedef struct _vgm_file_inf VGM_INF;
struct _vgm_file_inf
{
	FILE* hFile;
	VGM_HEADER Header;
	uint8_t WroteHeader;
	uint32_t HeaderBytes;
	uint32_t BytesWrt;
	uint32_t SmplsWrt;
	uint32_t EvtDelay;

	uint32_t DataCount;
	VGM_ROM_DATA DataBlk[0x20];
	//uint32_t CmdAlloc;
	uint32_t CmdCount;
	VGM_INIT_CMD Commands[0x100];

	uint8_t NesMemEmpty;
	uint8_t NesMem[0x4000];
};
typedef struct _vgm_chip VGM_CHIP;
typedef struct _vgm_chip_pcmcache VGM_PCMCACHE;
struct _vgm_chip
{
	uint16_t VgmID;
	uint8_t ChipType;
	uint8_t HadWrite;
	VGM_PCMCACHE* PCMCache;
};
struct _vgm_chip_pcmcache
{
	uint32_t Start;
	uint32_t Next;
	uint32_t Pos;
	uint32_t CacheSize;
	uint8_t* CacheData;
};

class vgm_writer
{
public:
	vgm_writer(running_machine &machine);

	void vgm_start(char *name);
	void vgm_stop(void);
	void vgm_open(uint8_t chip_type, int clock);
	void vgm_header_set(uint8_t attr, uint32_t data);
	void vgm_write(uint8_t port, uint16_t r, uint8_t v);
	void vgm_write_large_data(uint16_t chip_id, uint8_t type, uint32_t datasize, uint32_t value1, uint32_t value2, const void* data);
	uint16_t vgm_get_chip_idx(uint8_t chip_type, uint8_t Num);
	void vgm_change_rom_data(uint32_t oldsize, const void* olddata, uint32_t newsize, const void* newdata);
	void vgm_dump_sample_rom(uint16_t chip_id, uint8_t type, memory_region* region);
	void vgm_dump_sample_rom(uint16_t chip_id, uint8_t type, address_space& space);

	uint8_t LOG_VGM_FILE = 0x00;
	
	emu_timer * m_timer;

	TIMER_CALLBACK_MEMBER(vgmfile_callback);

#define MAX_VGM_FILES	0x10
#define MAX_VGM_CHIPS	0x80
	char vgm_namebase[_MAX_PATH];
	VGM_INF VgmFile[MAX_VGM_FILES];
	VGM_CHIP VgmChip[MAX_VGM_CHIPS];
	VGM_PCMCACHE VgmPCache[MAX_VGM_CHIPS];
	GD3_TAG VgmTag;
	running_machine* hMachine;

	// Function Prototypes
	static size_t str2utf16(gd3char_t* dststr, const char* srcstr, size_t max);
	static size_t wcs2utf16(gd3char_t* dststr, const wchar_t* srcstr, size_t max);
	static size_t utf16len(const gd3char_t* str);
	void vgm_header_postwrite(uint16_t vgm_id);
	void vgm_header_sizecheck(uint16_t vgm_id, uint32_t MinVer, uint32_t MinSize);
	void vgm_header_clear(uint16_t vgm_id);
	void vgm_setup_pcmcache(VGM_PCMCACHE* TempPC, uint32_t Size);
	void vgm_close(uint16_t vgm_id);
	void vgm_write_delay(uint16_t vgm_id);
	uint8_t vgm_nes_ram_check(VGM_INF* VI, uint32_t datasize, uint32_t* value1, uint32_t* value2, const uint8_t* data);
	void vgm_flush_pcm(VGM_CHIP* VC);
private:
	uint16_t m_chip_id;
};

//#define VGMC_OKIM6376	0xFF
#endif /* __VGMWRITE_H__ */
