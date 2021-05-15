extern const unsigned char	font_map_bin[];
#define				font_map_bin_size 128

extern const unsigned char	font_pal_bin[];
#define				font_pal_bin_size 16

extern const unsigned char	font_tile_bin[];
#define				font_tile_bin_size 2048


#define FONT_TILES_NO_S		1
#define DATA_ROOT	16		// "0"
#define UNIT_ROOT	10		// 10 is decimal
#define DATA_LONG	7		// 7 placeholder
#define BG_TILES_NO_S		65
#define SP_TILES_NO_S		256

void InitFont();
void loadFont(int idx);

signed char AddSpriteText(unsigned char *text, unsigned char x, unsigned char y);

void PrintHexShort(unsigned short value, unsigned char x, unsigned char y, unsigned char width);
//void printHex(unsigned long value, unsigned char x, unsigned char y, unsigned char width);

void PrintText(unsigned char* text, unsigned char x, unsigned char y);
void PrintChar(unsigned char ch, unsigned char x, unsigned char y);
//void engine_font_manager_draw_data(unsigned int data, unsigned char x, unsigned char y);
void PrintData(unsigned int data, unsigned char x, unsigned char y);

extern const char hexNumToTileNo[];
