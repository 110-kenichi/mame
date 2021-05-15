#include <stdbool.h>
#include <math.h>

#include "Types.h"

void AddF24_s16(FixedF24 *base, short lo16)
{
	if(lo16 < 0)
	{
		SubF24_s16(base, -lo16);
		return;
	}
/*
    short blo16 = base->LoWord.Lo16;
    signed char bhi8 = base->LoWord.Hi8;

    blo16 += lo16;
    bhi8 += 1;
    base->LoWord.Hi8 = bhi8;
    base->LoWord.Lo16 = blo16;
*/
    __asm

	push	ix
	ld	ix,#0
	add	ix,sp
	dec	sp
;Types.c:5: short blo16 = base->LoWord.Lo16;
	ld	c, 4 (ix)
	ld	b, 5 (ix)
	ld	l, c
	ld	h, b
	ld	e, (hl)
	inc	hl
	ld	d, (hl)
;Types.c:6: signed char bhi8 = base->LoWord.Hi8;
	ld	l, c
	ld	h, b
	inc	hl
	inc	hl
	ld	a, (hl)
	ld	-1 (ix), a
;Types.c:8: blo16 += lo16;
	ld	a, e
	add	a, 6 (ix)
	ld	e, a
	ld	a, d
	adc	a, 7 (ix)
	ld	d, a
;Types.c:9: bhi8 += 1;
	JR NC,0$
	ld	a, -1 (ix)
	inc	a
;Types.c:10: base->LoWord.Hi8 = bhi8;
	ld	(hl), a
0$:
;Types.c:11: base->LoWord.Lo16 = blo16;
	ld	a, e
	ld	(bc), a
	inc	bc
	ld	a, d
	ld	(bc), a
;Types.c:12: }
	inc	sp
	pop	ix
	ret

    __endasm;
}

void SubF24_s16(FixedF24 *base, short lo16)
{
	if(lo16 < 0)
	{
		AddF24_s16(base, -lo16);
		return;
	}

    /*
    short blo16 = base->LoWord.Lo16;
    signed char bhi8 = base->LoWord.Hi8;

    blo16 -= lo16;
    bhi8 -= 1;
    base->LoWord.Hi8 = bhi8;
    base->LoWord.Lo16 = blo16;
    */
    __asm

	push	ix
	ld	ix,#0
	add	ix,sp
	dec	sp
;Types.c:65: short blo16 = base->LoWord.Lo16;
	ld	c, 4 (ix)
	ld	b, 5 (ix)
	ld	l, c
	ld	h, b
	ld	e, (hl)
	inc	hl
	ld	d, (hl)
;Types.c:66: signed char bhi8 = base->LoWord.Hi8;
	ld	l, c
	ld	h, b
	inc	hl
	inc	hl
	ld	a, (hl)
	ld	-1 (ix), a
;Types.c:68: blo16 -= lo16;
	ld	a, e
	sub	a, 6 (ix)
	ld	e, a
	ld	a, d
	sbc	a, 7 (ix)
	ld	d, a
;Types.c:69: bhi8 -= 1;
	JR NC,0$
	ld	a, -1 (ix)
	dec	a
;Types.c:70: base->LoWord.Hi8 = bhi8;
	ld	(hl), a
0$:
;Types.c:71: base->LoWord.Lo16 = blo16;
	ld	a, e
	ld	(bc), a
	inc	bc
	ld	a, d
	ld	(bc), a
;Types.c:77: }
	inc	sp
	pop	ix
	ret
    
    __endasm;
}

short abs(short a)
{
	if(a < 0)
		return -a;
	else
		return a;
}

/// 2018/03 imo lab.
///https://garchiving.com
///https://garchiving.com/approximation-atan2/
short Atan2(short _y, short _x)
{
  short x = abs(_x);
  short y = abs(_y);
  float   z;
  bool    c;

  c = y < x;
  if (c)
  	z = (float)y / x;
  else
  	  z = (float)x / y;

  short a;
  a = z * (-1556 * z + 6072);                     //2次曲線近似
  //a = z * (z * (-448 * z - 954) + 5894);          //3次曲線近似
  //a = z * (z * (z * (829 * z - 2011) - 58) + 5741); //4次曲線近似

  if (c) {
    if (_x > 0) {
      if (_y < 0)a *= -1;
    }
    if (_x < 0) {
      if (_y > 0)a = 18000 - a;
      if (_y < 0)a = a - 18000;
    }
  }

  if (!c) {
    if (_x > 0) {
      if (_y > 0) a = 9000 - a;
      if (_y < 0) a = a - 9000;
    }
    if (_x < 0) {
      if (_y > 0) a = a + 9000;
      if (_y < 0) a = -a - 9000;
    }
  }

  return a;
}
