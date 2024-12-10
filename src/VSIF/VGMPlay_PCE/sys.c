
#include "main.h"

#pragma codeseg ("BNK00")

#define PCE_IO          *((vu8 *)0x1000)
#define VCE_CTRL        *((vu8 *)0x0400)
#define PAL_ADDR        *((vu16 *)0x0402)
#define PAL_DATA        *((vu16 *)0x0404)

#define VDC_REG         *((vu8 *)0x0100)
#define VDC_DATA        *((vu16 *)0x0102)

#define VDC_REG_WRA     0x00 //mem wr addr
#define VDC_REG_RDA     0x01 //mem rd addr
#define VDC_REG_MRW     0x02 //mem rw
#define VDC_REG_CNT     0x05 //control
#define VDC_REG_RCR     0x06 //scanning line detection
#define VDC_REG_BXR     0x07 //scroll x
#define VDC_REG_BYR     0x08 //scroll y
#define VDC_REG_MWR     0x09 //plan size, dot width, etc.
#define VDC_REG_HSR     0x0A //horizontal sync
#define VDC_REG_HDR     0x0B //horizontal display
#define VDC_REG_VPR     0x0C //vertical sync
#define VDC_REG_BDW     0x0D //vertical display
#define VDC_REG_BCR     0x0E //vertical display end
#define VDC_REG_DCR     0x0F //block transfer control
#define VDC_REG_SOUR    0x10 //block transfer source address	
#define VDC_REG_DESR    0x11 //block transfer destination address	
#define VDC_REG_LENR	0x12 //block transfer length
#define VDC_REG_SATB    0x13 //VRAM-SATB block transfer source

#define VDC_STAT_VBL    0x20
#define VDC_STAT_DMA    0x40

#define VDC_CNT_VIN_ON  0x08
#define VDC_CNT_BGR_ON  0x80


#define G_PLAN_W        64
#define G_PLAN_H        32
#define G_ADDR_PTRN     256//tile data
#define G_ADDR_TSET     0//tileset visible
#define G_ADDR_TBUF     0//G_PLAN_W * G_PLAN_H//tilesetback buff


#define RGB(r,g,b) ((r << 3) | (g << 6) | b)

#define RGB_BLK         RGB(0, 0, 0)
#define RGB_GRA         RGB(4, 4, 4)
#define RGB_WHT         RGB(6, 6, 1)
#define RGB_RED         RGB(7, 2, 2)
#define RGB_GRN         RGB(2, 7, 2)

static const u16 vdc_regs[] = {

    VDC_REG_WRA, 0x0000, //0x00 VRAM wr addr
    VDC_REG_RDA, 0x0000, //0x01 VRAM rd addr
    VDC_REG_CNT, 0x0000, //0x05 control
    VDC_REG_RCR, 0x0147, //0x06 raster Compare
    VDC_REG_BXR, 0x0000, //0x07 scroll x
    VDC_REG_BYR, 0x0000, //0x08 scroll y
    VDC_REG_MWR, 0x0050, //0x09 screen size
    VDC_REG_HSR, 0x0502, //0x0A horizontal sync
    VDC_REG_HDR, 0x0427, //0x0B horizontal display
    VDC_REG_VPR, 0x1702, //0x0C vertical sync
    VDC_REG_BDW, 0x00DF, //0x0D vertical display
    VDC_REG_BCR, 0x000c, //0x0E vertical display end
    0xffff
};

u16 palette[] = {

    0, 0, 0, 0, RGB_BLK, RGB_GRA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, RGB_BLK, RGB_WHT, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, RGB_BLK, RGB_GRN, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    0, 0, 0, 0, RGB_BLK, RGB_RED, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
};

void sys_set_bank();
void g_clean_plan();

void vdcSetReg(u8 reg, u16 val);
void vdcSetAddrWR(u16 addr);
void gSetFont(u8 *src);

extern u8 font[];

u16 g_pal;
u16 g_addr;
u8 sys_bank;

//****************************************************************************** sys base

void sysInit() {

    u16 i;

    VCE_CTRL = 0x01; //dot clk (screen resolution)

    for (i = 0; vdc_regs[i] != 0xffff; i += 2) {
        vdcSetReg(vdc_regs[i + 0], vdc_regs[i + 1]);
    }

    PAL_ADDR = 256;
    PAL_DATA = 0;

    PAL_ADDR = 0;
    for (i = 0; i < sizeof (palette) / 2; i++) {
        PAL_DATA = palette[i];
    }

    gSetFont(font);

    gSetPal(0);
    gCleanScreen();

    VDC_REG = VDC_REG_CNT;
    VDC_DATA = VDC_CNT_BGR_ON | VDC_CNT_VIN_ON;

    *((u8 *) 0x1402) = 6; //enable IRQ2 only

}

void vdcSetReg(u8 reg, u16 val) {
    VDC_REG = reg;
    VDC_DATA = val;
}

void vdcSetAddrWR(u16 addr) {

    VDC_REG = VDC_REG_WRA;
    VDC_DATA = addr;
    VDC_REG = VDC_REG_MRW;
}

void vdcToDefault() {

    u8 i;

    for (i = 0; i < 15; i++) {
        if (i == 13) {
            continue;
        }
        VDC_REG = i;
        VDC_DATA = 0;
    }

    VDC_REG = 13;
    VDC_DATA = 0;
    VDC_DATA = 0;

    i = VDC_REG;
    VCE_CTRL = 0;
}

u8 sysJoyRead() {

    u8 joy;

    PCE_IO = 1;
    asm("nop");
    asm("nop");
    PCE_IO = 3;
    asm("nop");
    asm("nop");

    PCE_IO = 1;
    asm("nop");
    asm("nop");

    joy = PCE_IO & 15;

    PCE_IO = 0;
    asm("nop");
    asm("nop");

    joy |= (PCE_IO & 15) << 4;

    joy ^= 0xff;

    return joy;
}

u8 sysJoyWait() {

    u8 joy = 1; //sysJoyRead();

    while (joy != 0) {
        gVsync();
        joy = sysJoyRead();
    }

    while (joy == 0) {
        gVsync();
        joy = sysJoyRead();
    }

    return joy;
}

void sysMemSet(void *dst, u8 val, u16 len) {

    u8 *ptr = (u8 *) dst;
    while (len--)*ptr++ = val;
}

void sysMemCopy(void *src, void *dst, u16 len) {

    u8 *src8 = (u8 *) src;
    u8 *dst8 = (u8 *) dst;

    while (len--) {
        *dst8++ = *src8++;
    }
}

void sysSetBank(u8 bank) {

    sys_bank = bank;
    sys_set_bank();
}

u8 sysGetBank() {
    return sys_bank;
}

void sysIrqON() {
    asm("cli");
}

void sysIrqOFF() {
    asm("sei");
}
//****************************************************************************** graphics

void gVsync() {

    while ((VDC_REG & VDC_STAT_VBL) != 0);
    while ((VDC_REG & VDC_STAT_VBL) == 0);
}

void gSetColor(u16 color, u16 val) {
    PAL_ADDR = color;
    PAL_DATA = val;
}

void gSetPal(u16 pal) {

    g_pal = G_ADDR_PTRN | (pal & 0xf000); //(pal << 12);
}

void gSetXY(u8 x, u8 y) {

    g_addr = G_ADDR_TBUF + x + y * G_PLAN_W;
    vdcSetAddrWR(g_addr);
}

void gSetX(u8 x) {

    u8 y = gGetY();
    gSetXY(x, y);
}

void gSetY(u8 y) {

    u8 x = gGetX();
    gSetXY(x, y);
}

u8 gGetX() {
    return g_addr % G_PLAN_W;
}

u8 gGetY() {
    return (g_addr - G_ADDR_TBUF) / G_PLAN_W;
}

void gCleanScreen() {

    vdcSetAddrWR(G_ADDR_TBUF);
    g_clean_plan();

    gSetPal(0);
    gSetXY(0, 0);
}

void gAppendChar(u8 val) {

    VDC_REG = VDC_REG_MRW;
    VDC_DATA = g_pal + val;
}

void gAppendString(u8 *str) {

    VDC_REG = VDC_REG_MRW;
    while (*str != 0) {
        VDC_DATA = g_pal + *str++;
    }
}

void gAppendString_ML(u8 *str, u8 max_len) {

    VDC_REG = VDC_REG_MRW;
    while (*str != 0 && max_len--) {
        VDC_DATA = g_pal + *str++;
    }
}

void gFillRect(u8 val, u8 x, u8 y, u8 w, u8 h) {

    u16 addr, addr_old, i;
    u16 fval = g_pal + val;

    gSetXY(x, y);
    addr = addr_old = g_addr;

    while (h--) {

        i = w;

        vdcSetAddrWR(addr);

        while (i--) {
            VDC_DATA = fval;
        }

        addr += G_PLAN_W;
    }

    vdcSetAddrWR(addr_old);
}

void gFillRow(u8 val, u8 x, u8 y, u8 w) {

    u16 fval = val + g_pal;

    gSetXY(x, y);

    //VDC_REG = VDC_REG_MRW;
    while (w--) {
        VDC_DATA = fval;
    }
}

void gFillCol(u8 val, u8 x, u8 y, u8 h) {

    u16 fval = val + g_pal;
    u16 addr;

    gSetXY(x, y);
    addr = g_addr;

    while (h--) {
        vdcSetAddrWR(addr);
        VDC_DATA = fval;
        addr += G_PLAN_W;
    }
}

void gAppendHex4(u8 val) {

    val += (val < 10 ? '0' : '7');
    gAppendChar(val);
}

void gAppendHex8(u8 val) {

    gAppendHex4(val >> 4);
    gAppendHex4(val & 15);
}

void gAppendHex16(u16 val) {

    gAppendHex8(val >> 8);
    gAppendHex8(val & 0xff);
}

void gAppendHex32(u32 val) {

    gAppendHex16(val >> 16);
    gAppendHex16(val & 0xffff);
}

void gAppendNum(u32 num) {

    u16 i;
    u8 buff[11];
    u8 *str = (u8 *) & buff[10];


    *str = 0;
    if (num == 0)*--str = '0';
    for (i = 0; num != 0; i++) {

        *--str = num % 10 + '0';
        num /= 10;
    }

    gAppendString(str);
}

void gConsPrint(u8 *str) {

    g_addr += G_PLAN_W;
    vdcSetAddrWR(g_addr);
    gAppendString(str);
}

void gConsPrint_ML(u8 *str, u8 maxlen) {

    g_addr += G_PLAN_W;
    vdcSetAddrWR(g_addr);
    gAppendString_ML(str, maxlen);
}

void gConsPrintCX(u8 *str) {

    gConsPrintCX_ML(str, MAX_STR_LEN);
}

void gConsPrintCX_ML(u8 *str, u8 maxlen) {

    u8 str_len = 0;
    while (str[str_len])str_len++;
    if (str_len > maxlen)str_len = maxlen;
    gSetX((G_SCREEN_W - str_len) / 2);
    gConsPrint_ML(str, maxlen);
}

void gSetFont(u8 *src) {

    u8 i, u;

    vdcSetAddrWR(4096 + 16 * 32);

    for (i = 0; i < 128; i++) {
        for (u = 0; u < 8; u++)VDC_DATA = *src++;
        for (u = 0; u < 8; u++)VDC_DATA = 0x00ff;
    }
}

void gAppendHex(void *src, u16 len) {

    u8 *src8 = (u8 *) src;

    u16 i;
    for (i = 0; i < len; i++) {
        gAppendHex8(*src8++);
    }

}

void gPrintHex(void *src, u16 len) {

    u8 *src8 = (u8 *) src;

    u16 i;
    for (i = 0; i < len; i++) {

        if (i % 16 == 0) {
            gConsPrint("");
        }

        if (i % 16 == 8) {
            gAppendChar('.');
        }

        if ((i & 1) == 0) {
            gSetPal(PAL_01);
        } else {
            gSetPal(PAL_00);
        }

        gAppendHex8(*src8++);
    }

    gSetPal(PAL_00);
}

void sysMemToReg(void *src, void *dst, u16 len) {

    if (len == 0)return;
    zp_src = src;
    zp_dst = dst;
    zp_len = len;
    sys_mem_to_reg_asm();
}

void sysRegToMem(void *src, void *dst, u16 len) {

    if (len == 0)return;
    zp_src = src;
    zp_dst = dst;
    zp_len = len;
    sys_reg_to_mem_asm();
}