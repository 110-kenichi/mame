#include "SMSlib/SMSlib.h"
#include "Font.h"
#include "Video.h"
#include "Font_tile.h"

void InitFont()
{
	SMS_loadBGPalette(font_pal_bin);
	SMS_loadTiles(font_tile_bin, 0, font_tile_bin_size);
}

const char hexNumToTileNo[] = {16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26 + 7, 27 + 7, 28 + 7, 29 + 7, 30 + 7, 31 + 7};

void PrintHexShort(unsigned short value, unsigned char x, unsigned char y, unsigned char width)
{
	x += width;
	for (int i = 0; i < width; i++)
	{
		SMS_setNextTileatXY(x--, y);
		SMS_setTile(hexNumToTileNo[value & 0xf]);
		value = value >> 4;
	}
}

/*
void printHex(unsigned long value, unsigned char x, unsigned char y, unsigned char width)
{
    x += width;
    for(int i=0;i<width;i++)
    {
        int v = value & 0xf;
        signed char tile = DATA_ROOT + v + FONT_TILES_NO_S;
        if(v >= 10)
            tile += 7;
        SMS_setNextTileatXY(x, y);
        SMS_setTile(tile);
        value = value >> 4;
        x-=1;
    }
}
*/

signed char AddSpriteText(unsigned char *text, unsigned char x, unsigned char y)
{
	signed char retv = 0;

	while ('\0' != *text)
	{
		signed char tileNo = (*text++);
		retv = SMS_addSprite(x, y, tileNo - 32);
		x += 8;
		break;
	}
	while ('\0' != *text)
	{
		signed char tileNo = (*text++);
		SMS_addSprite(x, y, tileNo - 32);
		x += 8;
	}
	return retv;
}

void PrintText(unsigned char *text, unsigned char x, unsigned char y)
{
	//const unsigned int *pnt = font__tilemap__bin;

	while ('\0' != *text)
	{
		signed char tileNo = (*text++) - 33 + FONT_TILES_NO_S;
		SetTileatXY(x++, y, tileNo);
	}
}

void PrintChar(unsigned char ch, unsigned char x, unsigned char y)
{
	signed char tileNo = ch - 33 + FONT_TILES_NO_S;
	SetTileatXY(x++, y, tileNo);
}

/*
void engine_font_manager_draw_data(unsigned int data, unsigned char x, unsigned char y)
{
	//const unsigned int *pnt = font__tilemap__bin;

	unsigned char idx;
	signed char tileNo;

	unsigned int quotient = 0;
	unsigned char remainder = 0;

	char hold[DATA_LONG];
	for (idx = 0; idx < DATA_LONG; ++idx)
	{
		quotient  = data / UNIT_ROOT;
		remainder = data % UNIT_ROOT;

		hold[idx] = remainder;
		data /= UNIT_ROOT;

		tileNo = hold[idx] + DATA_ROOT + FONT_TILES_NO_S;
		if (0 == quotient && 0 == remainder && idx > 0)
		{
			// Replace with space!
			tileNo = -1;
		}

		SMS_setNextTileatXY(x--, y);
		//SMS_setTile (*pnt + tile);
		SMS_setTile (tileNo);
	}
}
*/
void PrintData(unsigned int data, unsigned char x, unsigned char y)
{
	//const unsigned int *pnt = font__tilemap__bin;

	unsigned char idx;
	signed char tileNo;

	char hold[DATA_LONG];
	for (idx = 0; idx < DATA_LONG; ++idx)
	{
		hold[idx] = data % UNIT_ROOT;
		data /= UNIT_ROOT;

		tileNo = hold[idx] + DATA_ROOT + FONT_TILES_NO_S;

		SMS_setNextTileatXY(x--, y);
		//SMS_setTile (*pnt + tile);
		SMS_setTile(tileNo);
	}
}
