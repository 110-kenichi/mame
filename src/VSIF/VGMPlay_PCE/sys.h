/* 
 * File:   sys.h
 * Author: krik
 *
 * Created on December 15, 2014, 8:55 PM
 */

#ifndef SYS_H
#define	SYS_H

#define JOY_U           0x01
#define JOY_R           0x02
#define JOY_D           0x04
#define JOY_L           0x08
#define JOY_A           0x20 //II
#define JOY_B           0x10 //I
#define JOY_SEL         0x40
#define JOY_STA         0x80


#define G_SCREEN_W      40
#define G_SCREEN_H      28

#define PAL_00          0x0000   //black bgr, gray
#define PAL_01          0x1000   //black bgr, white
#define PAL_OK          0x2000   //black bgr, green
#define PAL_ER          0x3000   //black bgr, red

void sysInit();
u8 sysJoyRead();
u8 sysJoyWait();
void sysMemSet(void *dst, u8 val, u16 len);
void sysMemCopy(void *src, void *dst, u16 len);
void sysSetBank(u8 bank);
u8 sysGetBank();
void sysIrqON();
void sysIrqOFF();
void vdcToDefault();

void gVsync();
void gSetColor(u16 color, u16 val);
void gSetPal(u16 pal);
void gSetXY(u8 x, u8 y);
void gSetX(u8 x);
void gSetY(u8 y);
u8 gGetX();
u8 gGetY();
void gCleanScreen();
void gAppendChar(u8 val);
void gAppendString(u8 *str);
void gAppendString_ML(u8 *str, u8 max_len);
void gFillRect(u8 val, u8 x, u8 y, u8 w, u8 h);
void gFillRow(u8 val, u8 x, u8 y, u8 w);
void gFillCol(u8 val, u8 x, u8 y, u8 h);
void gAppendHex8(u8 val);
void gAppendHex16(u16 val);
void gAppendHex32(u32 val);
void gAppendNum(u32 num);
void gConsPrint(u8 *str);
void gConsPrint_ML(u8 *str, u8 maxlen);
void gConsPrintCX_ML(u8 *str, u8 maxlen);
void gConsPrintCX(u8 *str);
void gPrintHex(void *src, u16 len);
void gAppendHex(void *src, u16 len);

void sysMemToReg(void *src, void *dst, u16 len);
void sysRegToMem(void *src, void *dst, u16 len);

void sys_mem_to_reg_asm();
void sys_reg_to_mem_asm();
void sys_mem_to_mem_asm();

extern u8 zp_cmd;
extern void *zp_src;
extern void *zp_dst;
extern u16 zp_len;
#pragma zpsym("zp_cmd")
#pragma zpsym("zp_src")
#pragma zpsym("zp_dst")
#pragma zpsym("zp_len")

#endif	/* SYS_H */

