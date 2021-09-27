#ifndef TypesDef

#define TypesDef

typedef enum 
{
  G_PHASE_LOGO = 0,
  G_PHASE_PLAYER = 1,
} GamePhaseType;

typedef struct
{
  unsigned char Fraction; /* 小数8ビット */
  short Integer; /* 整数8ビット */
} _fp24;

typedef struct
{
  short Lo16;
  unsigned char Hi8;
} _fp24L;

typedef struct
{
  unsigned char Lo8;
  short Hi16;
} _fp24H;

typedef union 
{
  _fp24 Body;
  _fp24L LoWord;
  _fp24H HiWord;
} FixedF24;

typedef struct
{
  unsigned char Fraction; /* 下位8ビット */
  unsigned char Integer; /* 上位8ビット */
} _fp16;

typedef union
{
  short Word;
  _fp16 Body;
} FixedF16;

typedef enum 
{
  ChDirRight = 1,
  ChDirLeft = 2,
  ChDirDown = 4,
  ChDirUp = 8,
} CharacterDir;

typedef struct 
{
  unsigned char type;
  unsigned char spriteNo;
  FixedF24 x;
  FixedF24 y;
  FixedF16 dx;
  FixedF16 dy;
  CharacterDir dir;
  unsigned short status;
  unsigned short counter;
  void *tag;
} Character;

extern void AddF24_s16(FixedF24 *base, short lo16);
extern void SubF24_s16(FixedF24 *base, short lo16);

extern short Atan2(short _y, short _x);

#endif
