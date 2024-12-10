/* 
 * File:   bios.h
 * Author: igor
 *
 * Created on August 25, 2022, 12:58 AM
 */

#ifndef BIOS_H
#define	BIOS_H

/* addr     size 
 * -------------------------------- cpu map 
 * 0x0000   8K     sys regs
 * 0x2000   8K     sys ram
 * 0x4000   8K     app banks frame
 * 0x6000   32K    base rom
 * 0xE000   8K     app bank 00
 * -------------------------------- ram map
 * RAM0
 * 0x0000000 4M    huc rom (up to 8M if cd features turned off)
 * 0x0400000 2.2M  huc ram (2M+256K)
 * 0x0640000 64K   CD wram
 * 0x0650000       reserved
 * RAM1
 * 0x0C00000 64K   CD ADPCM ram
 * 0x0C10000       reserved
 * 0x0F60000 64K   theme
 * 0x0F70000 64K   ram app
 * 0x0F80000 256K  save state buff
 * 0x0FC0000 128K  gp buffer
 * 0x0FE0000 128K  menu
 * -------------------------------- pi map
 * 0x0000000 8M    ram0
 * 0x0800000 8M    ram1
 * 0x1000000 8M    reserved
 * 0x1800000 1M    sys
 */

#define ERR_UNXP_STAT   0x40
#define ERR_NULL_PATH   0x41

//****************************************************************************** versions
#define DEVID_TPRO_M22  0x20
#define STATUS_KEY      0x5A
#define PROTOCOL_ID     0x02
//****************************************************************************** mapper regs
#define REGA_FIFO_DATA  0x1FF0
#define REGA_MEM_DATA   0x1FF6

#define REG_FIFO_DATA   *((u8  *)0x1FF0) //system mapper internal nametable address
#define REG_FIFO_STAT   *((u8  *)0x1FF2) //system mapper internal nametable data
#define REG_SYS_STAT    *((u8  *)0x1FF3) //when load new bytes to nametable, value of this register will be applied as attribut.
#define REG_TIMER       *((u16 *)0x1FF4)
#define REG_MEM_DATA    *((u8  *)0x1FF6) //dma mem data
#define REG_MEM_ADDR    *((u8  *)0x1FF8) //dma mem addr. required using MEM_WP_MASK
#define REG_SST_DATA    *((u8  *)0x1FFA)
#define REG_SST_ADDR    *((u8  *)0x1FFC) 

#define FIFO_CPU_RXF    0x80 //fifo flags. system cpu can read
#define FIFO_ARM_RXF    0x40 //fifo flags. mcu can read

#define STATUS_CFG_OK   0x01 //mcu completed system configuration and. System ready for cpu execution
#define STATUS_CMD_OK   0x02 //mcu finished command execution
#define STATUS_FPG_OK   0x04 //fpga reboot complete
#define STATUS_STROBE   0x08 //strobe indicator
#define STATUS_REBOOT   0x10 //isn't real status. just an request to reboot at the end of halt

#define SUB_STATUS_DISK 0x00
#define SUB_STATUS_USB  0x01
#define SUB_STATUS_CUE  0x02
#define SUB_STATUS_BOOT 0x03

#define USB_ST_RX_BUSY  0x01
#define USB_ST_TX_BUSY  0x02
#define USB_ST_DISCONN  0x04

#define SYS_CTRL_RSTDL  0x01 //return to menu only if reset keeps pressed more than 1.3sec
#define SYS_CTRL_BRM_ON 0x02 //enable bram with auto backup
#define SYS_CTRL_SST_ON 0x04 //irq hook for in-game menu
#define SYS_CTRL_CC_ON  0x08 //cheat codes engine
#define SYS_CTRL_STEREO 0x10 //dac stereo mode
#define SYS_CTRL_CRTON  0x40 //cart off
#define SYS_CTRL_CRTOFF 0x60 //cart on


#define HUC_SYS         0x00 //menu mapper
//#define HUC_USR         0x01 //user mapper (third party rbf)
#define HUC_STD         0x02 //regular games
#define HUC_SF2         0x03 //street fighter
#define HUC_POP         0x04 //populous
#define HUC_384         0x05 //256K+128K
#define HUC_TNB         0x06 //brab card 8K
#define HUC_SCA         0x08 //system card
#define HUC_SSC         0x09 //super system card
#define HUC_ARC         0x0A //arcade card

#define EXP_OFF         0x00
#define EXP_CDR         0x01
#define EXP_TNB         0x02

#define MEM_WP_MASK     0x80000000
//****************************************************************************** addr and size
#define ADDR_RAM0       0x0000000                               //0x0000000 8MB PSRAM
#define ADDR_RAM1       0x0800000                               //0x0800000 8MB PSRAM
#define ADDR_ROM        ADDR_RAM0                               //0x0000000 ROM memory
#define ADDR_THEME      (ADDR_RAM_APP - SIZE_THEME)             //0x0F60000 64K
#define ADDR_RAM_APP    (ADDR_SST_BUFF - SIZE_RAM_APP)          //0x0F70000 64K
#define ADDR_SST_BUFF   (ADDR_GP_BUFF - SIZE_SST_BUFF)          //0x0F80000 256K (reduce to 128K?)
#define ADDR_GP_BUFF    (ADDR_MENU - SIZE_GP_BUFF)              //0x0FC0000 128K
#define ADDR_MENU       (ADDR_RAM1 + SIZE_RAM1 - SIZE_MENU)     //0x0FE0000 menu rom
#define ADDR_MARKS      (ADDR_MENU + SIZE_MENU - 16)

#define ADDR_SYS        0x1800000                               //system registers 
#define ADDR_CFG        (ADDR_SYS + 0x000)                      //system config
#define ADDR_CC_RAM     (ADDR_CFG + 0x100)                      //ram cheats handler asm
#define ADDR_CC_ROM     (ADDR_CFG + 0x200)                      //rom cheats
#define ADDR_EXP        0x1830000                               //pi to exp device
#define ADDR_CD_CFG     (ADDR_EXP + 0x10)
#define ADDR_BRAM_DAT   0x1840000 
#define ADDR_BRAM_EXP   (ADDR_BRAM_DAT + 0)
#define ADDR_BRAM_HUC   (ADDR_BRAM_DAT + 8192)

#define ADDR_FLA_ICOR   0x00000 //flash buffer for mcu firmware update
#define ADDR_FLA_EFU    0x80000

#define SIZE_RAM0       0x800000
#define SIZE_RAM1       0x800000
#define SIZE_MENU       0x20000
#define SIZE_GP_BUFF    0x20000
#define SIZE_SST_BUFF   0x40000
#define SIZE_BRAM_EXP   2048
#define SIZE_BRAM_HUC   8192
#define SIZE_RAM_APP    0x10000 //actual max size is 0xFF00
#define SIZE_THEME      0x10000
#define SIZE_IOCORE     0x20008 //IO core update size

#define ACK_BLOCK_SIZE  1024
//****************************************************************************** file mode
#define	FA_READ		 0x01
#define	FA_WRITE	 0x02
#define	FA_OPEN_EXISTING 0x00
#define	FA_CREATE_NEW	 0x04
#define	FA_CREATE_ALWAYS 0x08
#define	FA_OPEN_ALWAYS	 0x10
#define	FA_OPEN_APPEND	 0x30

#define	AT_RDO          0x01    // Read only
#define	AT_HID          0x02	// Hidden
#define	AT_SYS          0x04	// System
#define AT_DIR          0x10	// Directory
#define AT_ARC          0x20	// Archive

#define DIR_OPT_SORTED  0x01
#define DIR_OPT_HIDESYS 0x02
#define DIR_OPT_SEEKCUE 0x04
#define DIR_OPT_FILTCUE 0x08
#define DIR_OPT_FILTROM 0x10
#define DIR_OPT_FILTRBF 0x20
//****************************************************************************** 
#define FILT_LOPASS     0x00
#define FILT_HIPASS     0x01
#define FILT_FRACTION   4096

typedef struct {
    u32 size;
    u16 date;
    u16 time;
    u8 is_dir; //structure size hardcoded in bi_rx_file_info
    u8 *file_name;
} FileInfo;

typedef struct {
    u8 ctrl;
    u8 sst_key_save;
    u8 sst_key_load;
    u8 sst_key_menu;
    u8 reserved[2];
    u8 exp_type;
    u8 huc_type;
} CartCfg;

typedef struct {
    s16 a1;
    s16 a2;
    s16 a3;
    s16 b1;
    s16 b2;
} FiltPoly;

typedef struct {
    u8 ftype;
    u16 fraction;
    u16 gain;
    u32 srate;
    u32 cutoff;
} FiltCfg;

typedef struct {
    u8 vol_adpcm;
    u8 vol_cdda;
    FiltPoly adpcm_filter;
} CdCfg;

typedef struct {
    u8 fla_id[8];
    u8 cpu_id[12];
    u32 serial_g;
    u32 serial_l;
    u32 boot_ctr;
    u32 game_ctr;
    u16 asm_date;
    u16 asm_time;
    u16 sw_date;
    u16 sw_time;
    u16 sw_ver;
    u16 hv_ver;
    u16 boot_ver;
    u8 device_id;
    u8 rapp_status;
    u8 rst_src;
    u8 boot_status;
    u8 bat_dry;
    u8 disk_status;
    u8 pwr_sys;
    u8 pwr_usb;
    u8 eep_id[6];
} SysInfoIO;

typedef struct {
    u16 v50;
    u16 v25;
    u16 v12;
    u16 vbt;
} Vdc;

typedef struct {
    u16 vector_f6;
    u16 vector_f8;
    u16 vector_fa;
    u8 codesize;
    u8 code[128 - 7];
} CCRamCodes;

typedef struct {
    u8 addr[3];
    u8 val;
} CCRomSlot;

typedef struct {
    CCRomSlot slot[16];
} CCRomCodes;

typedef struct {
    u8 yar;
    u8 mon;
    u8 dom;
    u8 hur;
    u8 min;
    u8 sec;
} RtcTime;



void bi_init();
u8 bi_fifo_busy();
void bi_fifo_rd(u8 *data, u16 len);
void bi_fifo_wr(void *data, u16 len);
u8 bi_check_status();
void bi_tx_string(u8 *string);
void bi_rx_string(u8 *string);
u8 bi_rx_next_rec(FileInfo *inf);
u16 bi_get_ticks();
void bi_set_ctrl(u8 ctrl);
void bi_app_start(CartCfg *cfg);


u8 bi_cmd_rapp_set(u32 addr);
void bi_cmd_rapp_cln();
u8 bi_cmd_cd_mount(u8 *path);
void bi_cmd_sys_inf(SysInfoIO *inf);
u8 bi_cmd_sub_status(u8 stat_req);
void bi_cmd_get_vdc(Vdc *vdc);
void bi_set_mem_addr(u32 addr);
u8 bi_cmd_bram_save();
void bi_cmd_game_ctr();
u8 bi_cmd_fpga_init(u8 *path);
void bi_cmd_reboot();
u8 bi_cmd_efu_unpack();
u8 bi_cmd_efu_update(u8 *path);
u8 bi_cmd_fla_wr_sdc(u32 addr, u32 len);
void bi_cmd_upd_exec(u32 addr, u32 crc);
void bi_cmd_rtc_get(RtcTime *time);
void bi_cmd_rtc_set(RtcTime *time);
void bi_cmd_usb_wr(void *data, u16 len);
void bi_cmd_calc_filt(FiltCfg *cfg, FiltPoly *poly);
u8 bi_cmd_rom_path(u8 *path, u8 path_type);

void bi_cmd_tx(u8 cmd);
void bi_cmd_status(void *status);
u8 bi_cmd_disk_init();
u8 bi_cmd_file_open(u8 *path, u8 mode);
u8 bi_cmd_file_close();
u8 bi_cmd_file_read(void *dst, u32 len);
u8 bi_cmd_file_read_mem(u32 addr, u32 len);
u8 bi_cmd_file_write(void *src, u32 len);
u8 bi_cmd_file_write_mem(u32 addr, u32 len);
u8 bi_cmd_file_copy(u8 *src, u8 *dst, u8 dst_mode);
u32 bi_cmd_file_available();
u8 bi_cmd_file_set_ptr(u32 addr);
u8 bi_cmd_file_info(u8 *path, FileInfo *inf);
u8 bi_cmd_file_del(u8 *path);
u8 bi_cmd_file_crc(u32 len, u32 *crc_base);
u8 bi_cmd_file_seek_pat(u8 *pat, u8 psize, u32 fsize, u32 *paddr);
u8 bi_cmd_dir_make(u8 *path);

u8 bi_cmd_dir_load(u8 *path, u8 dir_opt);
void bi_cmd_dir_get_size(u16 *size);
void bi_cmd_dir_seek_idx(u16 *idx);
void bi_cmd_dir_get_recs(u16 start_idx, u16 amount, u16 max_name_len);

void bi_cmd_mem_set(u8 val, u32 addr, u32 len);
u8 bi_cmd_mem_test(u8 val, u32 addr, u32 len);
void bi_cmd_mem_rd(u32 addr, void *dst, u32 len);
void bi_cmd_mem_wr(u32 addr, u8 *src, u32 len);
void bi_cmd_mem_crc(u32 addr, u32 len, u32 *crc_base);

void bi_mem_rdd(u32 mem_addr, void *dst, u32 len); //dma mem. skip mcu
void bi_mem_wrd(u32 mem_addr, void *src, u32 len); //dma mem. skip mcu
#endif	/* BIOS_H */

