// license:BSD-3-Clause
// copyright-holders:David Haywood
#ifndef MAME_INCLUDES_MEGADRIV_H
#define MAME_INCLUDES_MEGADRIV_H

#pragma once

#include "coreutil.h"
#include "cpu/m68000/m68000.h"
#include "cpu/z80/z80.h"
#include "sound/2612intf.h"
#include "sound/ym2151.h"
#include "sound/ym2413.h"
#include "sound/emu2413.h"
#include "sound/gb.h"
#include "sound/sn76496.h"
#include "sound/namco.h"
#include "sound/nes_apu.h"
#include "sound/k051649.h"
#include "sound/3812intf.h"
#include "sound/msm5232.h"
#include "sound/ay8910.h"
#include "sound/mos6581.h"
#include "sound/beep.h"
#include "sound/c140.h"
#include "sound/c6280.h"
#include "audio/snes_snd.h"
#include "sound/pokey.h"
#include "sound/2610intf.h"
#include "video/315_5313.h"
#include "sound/mt32.h"
#include "sound/cm32p.h"
#include "sound/262intf.h"
#include "sound/2608intf.h"
#include "sound/tms5220.h"
#include "sound/sp0256.h"
#include "sound/samples.h"
#include "sound/sn76477.h"
#include "sound/upd1771.h"
#include "sound/multipcm.h"
#include "sound/rf5c68.h"
#include "sound/ymfm/src/ymfm_opz.h"
#include "sound/ymfm/src/ymfm_opn.h"
#include "sound/ymfm/src/ymfm_opl.h"
#include "sound/ymfm/src/ymfm_opq.h"
#include "sound/saa1099.h"

/* Megadrive Console Specific */
#include "bus/megadrive/md_slot.h"
#include "bus/megadrive/md_carts.h"
#include "machine/mega32x.h"
#include "machine/megacd.h"

#define MASTER_CLOCK_NTSC 53693175
#define MASTER_CLOCK_PAL  53203424

#define MD_CPU_REGION_SIZE 0x800000


/*----------- defined in machine/megadriv.cpp -----------*/

INPUT_PORTS_EXTERN(md_common);
INPUT_PORTS_EXTERN(megadriv);
INPUT_PORTS_EXTERN(megadri6);
INPUT_PORTS_EXTERN(ssf2mdb);
INPUT_PORTS_EXTERN(mk3mdb);

struct genesis_z80_vars
{
	int z80_is_reset;
	int z80_has_bus;
	uint32_t z80_bank_addr;
	std::unique_ptr<uint8_t[]> z80_prgram;
};


class md_base_state : public driver_device
{
public:
	md_base_state(const machine_config &mconfig, device_type type, const char *tag) :
		driver_device(mconfig, type, tag),
		m_maincpu(*this, "maincpu"),
		/*
		m_z80snd(*this, "genesis_snd_z80"),
		m_ymsnd(*this, "ymsnd"),
		m_scan_timer(*this, "md_scan_timer"),
		m_vdp(*this, "gen_vdp"),
		m_megadrive_ram(*this, "megadrive_ram"),
		m_io_reset(*this, "RESET"),
		*/
		m_megadrive_io_read_data_port_ptr(*this),
		m_megadrive_io_write_data_port_ptr(*this)
	{
		//mamidimemo

		for (int i = 0; i < 8; i++)
		{
			int didx = 0;
			std::string num = std::to_string(i);

			//YM2151
			strcpy(device_names[didx][i], (std::string("ym2151_") + num).c_str());
			m_ym2151[i] = new optional_device<ym2151_device>(*this, device_names[didx][i]);
			didx++;
			//YM2612
			strcpy(device_names[didx][i], (std::string("ym2612_") + num).c_str());
			m_ym2612[i] = new optional_device<ymfm_opn2_device>(*this, device_names[didx][i]);
			didx++;
			//YM3812
			strcpy(device_names[didx][i], (std::string("ym3812_") + num).c_str());
			m_ym3812[i] = new optional_device<ym3812_device>(*this, device_names[didx][i]);
			didx++;
			//YM2413
			strcpy(device_names[didx][i], (std::string("ym2413_") + num).c_str());
			m_ym2413[i] = new optional_device<ymfm_opll_device>(*this, device_names[didx][i]);
			didx++;
			//sn76496(PSG)
			strcpy(device_names[didx][i], (std::string("sn76496_") + num).c_str());
			m_sn76496[i] = new optional_device<sn76496_device>(*this, device_names[didx][i]);
			didx++;
			//namco cus30
			strcpy(device_names[didx][i], (std::string("namco_cus30_") + num).c_str());
			m_namco_cus30[i] = new optional_device<namco_cus30_device>(*this, device_names[didx][i]);
			didx++;
			//GB APU
			strcpy(device_names[didx][i], (std::string("gbsnd_") + num).c_str());
			m_gbsnd[i] = new optional_device<gameboy_sound_device>(*this, device_names[didx][i]);
			didx++;
			//nes apu
			strcpy(device_names[didx][i], (std::string("nes_apu_") + num).c_str());
			m_nesapu[i] = new optional_device<nesapu_device>(*this, device_names[didx][i]);
			didx++;
			//SCC1
			strcpy(device_names[didx][i], (std::string("scc1_") + num).c_str());
			m_scc1[i] = new optional_device<k051649_device>(*this, device_names[didx][i]);
			didx++;
			//MSM5232
			strcpy(device_names[didx][i], (std::string("msm5232_") + num).c_str());
			m_msm5232[i] = new optional_device<msm5232_device>(*this, device_names[didx][i]);
			didx++;
			//AY-3-8910
			strcpy(device_names[didx][i], (std::string("ay8910_") + num).c_str());
			m_ay8910[i] = new optional_device<ay8910_device>(*this, device_names[didx][i]);
			didx++;
			//mos8580
			strcpy(device_names[didx][i], (std::string("mos8580_") + num).c_str());
			m_sid8580[i] = new optional_device<mos8580_device>(*this, device_names[didx][i]);
			didx++;
			//mos6581
			strcpy(device_names[didx][i], (std::string("mos6581_") + num).c_str());
			m_sid6581[i] = new optional_device<mos6581_device>(*this, device_names[didx][i]);
			didx++;
			//mos6581
			strcpy(device_names[didx][i], (std::string("beep_") + num).c_str());
			m_beep[i] = new optional_device<beep_device>(*this, device_names[didx][i]);
			didx++;
			//c140
			strcpy(device_names[didx][i], (std::string("c140_") + num).c_str());
			m_c140[i] = new optional_device<c140_device>(*this, device_names[didx][i]);
			didx++;
			//c6280
			strcpy(device_names[didx][i], (std::string("c6280_") + num).c_str());
			m_c6280[i] = new optional_device<c6280_device>(*this, device_names[didx][i]);
			didx++;
			//spc700
			strcpy(device_names[didx][i], (std::string("snes_sound_") + num).c_str());
			m_spc700[i] = new optional_device<snes_sound_device>(*this, device_names[didx][i]);
			didx++;
			//pokey
			strcpy(device_names[didx][i], (std::string("pokey_") + num).c_str());
			m_pokey[i] = new optional_device<pokey_device>(*this, device_names[didx][i]);
			didx++;
			//ym2610b
			strcpy(device_names[didx][i], (std::string("ym2610b_") + num).c_str());
			m_ym2610b[i] = new optional_device<ym2610b_device>(*this, device_names[didx][i]);
			didx++;
			//mt32
			strcpy(device_names[didx][i], (std::string("mt32_") + num).c_str());
			m_mt32[i] = new optional_device<mt32_device>(*this, device_names[didx][i]);
			didx++;
			//cm32p
			strcpy(device_names[didx][i], (std::string("cm32p_") + num).c_str());
			m_cm32p[i] = new optional_device<cm32p_device>(*this, device_names[didx][i]);
			didx++;
			//OPL3
			strcpy(device_names[didx][i], (std::string("ymf262_") + num).c_str());
			m_ymf262[i] = new optional_device<ymf262_device>(*this, device_names[didx][i]);
			didx++;
			//ym2608
			strcpy(device_names[didx][i], (std::string("ym2608_") + num).c_str());
			m_ym2608[i] = new optional_device<ym2608_device>(*this, device_names[didx][i]);
			didx++;
			//tms5220
			strcpy(device_names[didx][i], (std::string("tms5220_") + num).c_str());
			m_tms5220[i] = new optional_device<tms5220_device>(*this, device_names[didx][i]);
			didx++;
			//sp0256
			strcpy(device_names[didx][i], (std::string("sp0256_") + num).c_str());
			m_sp0256[i] = new optional_device<sp0256_device>(*this, device_names[didx][i]);
			didx++;
			//sam
			strcpy(device_names[didx][i], (std::string("sam_") + num).c_str());
			m_sam[i] = new optional_device<samples_device>(*this, device_names[didx][i]);
			didx++;
			//sn76477
			strcpy(device_names[didx][i], (std::string("sn76477_") + num).c_str());
			m_sn76477[i] = new optional_device<sn76477_device>(*this, device_names[didx][i]);
			didx++;
			//upd1771
			strcpy(device_names[didx][i], (std::string("upd1771_") + num).c_str());
			m_upd1771[i] = new optional_device<upd1771c_device>(*this, device_names[didx][i]);
			didx++;
			//m_ymfm_opz
			strcpy(device_names[didx][i], (std::string("ymfm_opz_") + num).c_str());
			m_ymfm_opz[i] = new optional_device<ymfm_opz_device>(*this, device_names[didx][i]);
			didx++;
			//m_ymfm_opq
			strcpy(device_names[didx][i], (std::string("ymfm_opq_") + num).c_str());
			m_ymfm_opq[i] = new optional_device<ymfm_opq_device>(*this, device_names[didx][i]);
			didx++;
			//multipcm
			strcpy(device_names[didx][i], (std::string("multipcm_") + num).c_str());
			m_multipcm[i] = new optional_device<multipcm_device>(*this, device_names[didx][i]);
			didx++;
			//rf5c68
			strcpy(device_names[didx][i], (std::string("rf5c164_") + num).c_str());
			m_rf5c68[i] = new optional_device<rf5c164_device>(*this, device_names[didx][i]);
			didx++;
			//saa1099
			strcpy(device_names[didx][i], (std::string("saa1099_") + num).c_str());
			m_saa1099[i] = new optional_device<saa1099_device>(*this, device_names[didx][i]);
			didx++;
		}
	}

	char device_names[ 33 ][8][100];
	optional_device<ym2151_device> *m_ym2151[8];	//1
	optional_device<ymfm_opn2_device> *m_ym2612[8];	//2
	optional_device<sn76496_device> *m_sn76496[8];	//3
	optional_device<namco_cus30_device> *m_namco_cus30[8];	//4
	optional_device<gameboy_sound_device> *m_gbsnd[8];	//5
	optional_device<nesapu_device> *m_nesapu[8];	//6
	optional_device<k051649_device> *m_scc1[8];	//7
	optional_device<ym3812_device> *m_ym3812[8];	//8
	optional_device<ymfm_opll_device> *m_ym2413[8];	//9
	optional_device<msm5232_device> *m_msm5232[8];	//10
	optional_device<ay8910_device> *m_ay8910[8];	//11
	optional_device<mos8580_device> *m_sid8580[8];	//12
	optional_device<mos6581_device> *m_sid6581[8];	//13
	optional_device<beep_device> *m_beep[8];	//14
	optional_device<c140_device> *m_c140[8];	//15
	optional_device<c6280_device> *m_c6280[8];	//16
	optional_device<snes_sound_device> *m_spc700[8];	//17
	optional_device<pokey_device> *m_pokey[8];	//18
	optional_device<ym2610b_device> *m_ym2610b[8];	//19
	optional_device<mt32_device> *m_mt32[8];	//20
	optional_device<cm32p_device> *m_cm32p[8];	//21
	optional_device<ymf262_device> *m_ymf262[8];	//22
	optional_device<ym2608_device> *m_ym2608[8];	//23
	optional_device<tms5220_device> *m_tms5220[8];	//24
	optional_device<sp0256_device>* m_sp0256[8];	//25
	optional_device<samples_device>* m_sam[8];	//26
	optional_device<sn76477_device>* m_sn76477[8];	//27
	optional_device<upd1771c_device>* m_upd1771[8];	//28
	optional_device<ymfm_opz_device>* m_ymfm_opz[8];	//29
	optional_device<ymfm_opq_device>* m_ymfm_opq[8];	//30
	optional_device<multipcm_device>* m_multipcm[8];	//31
	optional_device<rf5c164_device>* m_rf5c68[8];	//32
	optional_device<saa1099_device>* m_saa1099[8];	//33

	required_device<m68000_base_device> m_maincpu;
	/*
	optional_device<cpu_device> m_z80snd;
	optional_device<ym2612_device> m_ymsnd;
	optional_device<timer_device> m_scan_timer;
	required_device<sega315_5313_device> m_vdp;
	optional_shared_ptr<uint16_t> m_megadrive_ram;

	optional_ioport m_io_reset;
	*/
	ioport_port *m_io_pad_3b[4];
	ioport_port *m_io_pad_6b[4];

	genesis_z80_vars m_genz80;
	int m_version_hi_nibble;

	void init_megadriv_c2();
	void init_megadrie();
	void init_megadriv();
	void init_megadrij();

	uint8_t megadriv_68k_YM2612_read(offs_t offset, uint8_t mem_mask = ~0);
	void megadriv_68k_YM2612_write(offs_t offset, uint8_t data, uint8_t mem_mask = ~0);
	IRQ_CALLBACK_MEMBER(genesis_int_callback);
	void megadriv_init_common();

	void megadriv_z80_bank_w(uint16_t data);
	void megadriv_68k_z80_bank_write(uint16_t data);
	void megadriv_z80_z80_bank_w(uint8_t data);
	uint16_t megadriv_68k_io_read(offs_t offset);
	void megadriv_68k_io_write(offs_t offset, uint16_t data, uint16_t mem_mask = ~0);
	uint16_t megadriv_68k_read_z80_ram(offs_t offset, uint16_t mem_mask = ~0);
	void megadriv_68k_write_z80_ram(offs_t offset, uint16_t data, uint16_t mem_mask = ~0);
	uint16_t megadriv_68k_check_z80_bus(offs_t offset, uint16_t mem_mask = ~0);
	void megadriv_68k_req_z80_bus(offs_t offset, uint16_t data, uint16_t mem_mask = ~0);
	void megadriv_68k_req_z80_reset(offs_t offset, uint16_t data, uint16_t mem_mask = ~0);
	uint8_t z80_read_68k_banked_data(offs_t offset);
	void z80_write_68k_banked_data(offs_t offset, uint8_t data);
	void megadriv_z80_vdp_write(offs_t offset, uint8_t data);
	uint8_t megadriv_z80_vdp_read(offs_t offset);
	uint8_t megadriv_z80_unmapped_read();
	TIMER_CALLBACK_MEMBER(megadriv_z80_run_state);

	/* Megadrive / Genesis has 3 I/O ports */
	emu_timer *m_io_timeout[3];
	int m_io_stage[3];
	uint8_t m_megadrive_io_data_regs[3];
	uint8_t m_megadrive_io_ctrl_regs[3];
	uint8_t m_megadrive_io_tx_regs[3];
	read8sm_delegate m_megadrive_io_read_data_port_ptr;
	write16sm_delegate m_megadrive_io_write_data_port_ptr;

	WRITE_LINE_MEMBER(vdp_sndirqline_callback_genesis_z80);
	WRITE_LINE_MEMBER(vdp_lv6irqline_callback_genesis_68k);
	WRITE_LINE_MEMBER(vdp_lv4irqline_callback_genesis_68k);

	TIMER_CALLBACK_MEMBER(io_timeout_timer_callback);
	void megadrive_reset_io();
	uint8_t megadrive_io_read_data_port_6button(offs_t offset);
	uint8_t megadrive_io_read_data_port_3button(offs_t offset);
	uint8_t megadrive_io_read_ctrl_port(int portnum);
	uint8_t megadrive_io_read_tx_port(int portnum);
	uint8_t megadrive_io_read_rx_port(int portnum);
	uint8_t megadrive_io_read_sctrl_port(int portnum);

	void megadrive_io_write_data_port_3button(offs_t offset, uint16_t data);
	void megadrive_io_write_data_port_6button(offs_t offset, uint16_t data);
	void megadrive_io_write_ctrl_port(int portnum, uint16_t data);
	void megadrive_io_write_tx_port(int portnum, uint16_t data);
	void megadrive_io_write_rx_port(int portnum, uint16_t data);
	void megadrive_io_write_sctrl_port(int portnum, uint16_t data);

	void megadriv_stop_scanline_timer();

	DECLARE_MACHINE_START(megadriv);
	DECLARE_MACHINE_RESET(megadriv);
	DECLARE_VIDEO_START(megadriv);
	uint32_t screen_update_megadriv(screen_device &screen, bitmap_rgb32 &bitmap, const rectangle &cliprect);
	DECLARE_WRITE_LINE_MEMBER(screen_vblank_megadriv);

	void megadriv_tas_callback(offs_t offset, uint8_t data);

	void megadriv_timers(machine_config &config);
	void md_ntsc(machine_config &config);
	void md_pal(machine_config &config);
	void md_bootleg(machine_config &config);
	void dcat16_megadriv_base(machine_config &config);
	void dcat16_megadriv_map(address_map &map);
	void megadriv_map(address_map &map);
	void megadriv_z80_io_map(address_map &map);
	void megadriv_z80_map(address_map &map);
};

class md_cons_state : public md_base_state
{
public:
	md_cons_state(const machine_config &mconfig, device_type type, const char *tag) :
		md_base_state(mconfig, type, tag),
		m_32x(*this, "sega32x"),
		m_segacd(*this, "segacd"),
		m_cart(*this, "mdslot"),
		m_tmss(*this, "tmss")
	{ }

	ioport_port *m_io_ctrlr;
	ioport_port *m_io_pad3b[4];
	ioport_port *m_io_pad6b[2][4];

	optional_device<sega_32x_device> m_32x;
	optional_device<sega_segacd_device> m_segacd;
	optional_device<md_cart_slot_device> m_cart;
	optional_region_ptr<uint16_t> m_tmss;

	void init_mess_md_common();
	void init_genesis();
	void init_md_eur();
	void init_md_jpn();

	uint8_t mess_md_io_read_data_port(offs_t offset);
	void mess_md_io_write_data_port(offs_t offset, uint16_t data);

	DECLARE_MACHINE_START(md_common);     // setup ioport_port
	DECLARE_MACHINE_START(ms_megadriv);   // setup ioport_port + install cartslot handlers
	DECLARE_MACHINE_START(ms_megacd);     // setup ioport_port + dma delay for cd
	DECLARE_MACHINE_RESET(ms_megadriv);

	DECLARE_WRITE_LINE_MEMBER(screen_vblank_console);

	DECLARE_DEVICE_IMAGE_LOAD_MEMBER(_32x_cart);

	void _32x_scanline_callback(int x, uint32_t priority, uint32_t &lineptr);
	void _32x_interrupt_callback(int scanline, int irq6);
	void _32x_scanline_helper_callback(int scanline);

	void install_cartslot();
	void install_tmss();
	uint16_t tmss_r(offs_t offset);
	void tmss_swap_w(uint16_t data);
	void genesis_32x_scd(machine_config &config);
	void mdj_32x_scd(machine_config &config);
	void ms_megadpal(machine_config &config);
	void dcat16_megadriv_base(machine_config &config);
	void dcat16_megadriv(machine_config &config);
	void md_32x_scd(machine_config &config);
	void mdj_32x(machine_config &config);
	void ms_megadriv(machine_config &config);
	void mdj_scd(machine_config &config);
	void md_32x(machine_config &config);
	void genesis_32x(machine_config &config);
	void md_scd(machine_config &config);
	void genesis_scd(machine_config &config);
	void genesis_tmss(machine_config &config);
};

#endif // MAME_INCLUDES_MEGADRIV_H
