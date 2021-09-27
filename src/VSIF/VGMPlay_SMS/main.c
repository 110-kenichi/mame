#include "main.h"

SMS_EMBED_SEGA_ROM_HEADER(9999, 0);

#include "Font.h"

unsigned char FreeSpaceC000_C3ff[0x400];

const FixedF16 SIN_TABLE[128] = {
    {0},    {12},   {25},   {37},   {49},   {62},   {74},   {86},   {97},
    {109},  {120},  {131},  {142},  {152},  {162},  {171},  {181},  {189},
    {197},  {205},  {212},  {219},  {225},  {231},  {236},  {241},  {244},
    {248},  {251},  {253},  {254},  {255},  {255},  {254},  {253},  {251},
    {248},  {244},  {241},  {236},  {231},  {225},  {219},  {212},  {205},
    {197},  {189},  {181},  {171},  {162},  {152},  {142},  {131},  {120},
    {109},  {97},   {86},   {74},   {62},   {49},   {37},   {25},   {12},
    {0},    {0},    {-12},  {-25},  {-37},  {-49},  {-62},  {-74},  {-86},
    {-97},  {-109}, {-120}, {-131}, {-142}, {-152}, {-162}, {-171}, {-181},
    {-189}, {-197}, {-205}, {-212}, {-219}, {-225}, {-231}, {-236}, {-241},
    {-244}, {-248}, {-251}, {-253}, {-254}, {-255}, {-255}, {-254}, {-253},
    {-251}, {-248}, {-244}, {-241}, {-236}, {-231}, {-225}, {-219}, {-212},
    {-205}, {-197}, {-189}, {-181}, {-171}, {-162}, {-152}, {-142}, {-131},
    {-120}, {-109}, {-97},  {-86},  {-74},  {-62},  {-49},  {-37},  {-25},
    {-12},  {0}};

GamePhaseType GamePhase = G_PHASE_PLAYER;
GamePhaseType LastGamePhase = G_PHASE_LOGO;

unsigned short PhaseCounter = 0;
unsigned short PhaseLocalCounter = 0;
unsigned short PhaseLocalCounter2 = 0;
unsigned short PhaseLocalCounter3 = 0;
unsigned short PhaseLocalCounter4 = 0;

Character Characters[128];

void ProcessGamePhase(char vblank);

void main() {
  SMS_init();
  // SMS_VDPturnOnFeature(VDPFEATURE_SHIFTSPRITES);
  // SMS_VDPturnOnFeature(VDPFEATURE_HIDEFIRSTCOL);
  // SMS_VDPturnOnFeature(VDPFEATURE_LOCKVSCROLL);
  // SMS_setSpriteMode(SPRITEMODE_TALL_ZOOMED);
  SMS_setSpriteMode(SPRITEMODE_NORMAL);
  SMS_zeroBGPalette();
  SMS_zeroSpritePalette();

  InitFont();

  /*
  //Create sin table
  for (int i = 0; i < 32; i++)
  {
      sin_table[i].Word = (int)(sinf((float)i / 64.f * PI) * 256.f);
      sin_table[63 - i] = sin_table[i];
      sin_table[64 + i].Word = -sin_table[i].Word;
      sin_table[127 - i].Word = -sin_table[i].Word;
  }*/

  currentBgPalette = defBgPalette;
  currentSpPalette = defSpPalette;

  SMS_setVBlankInterruptHandler(VinterruptHandler);
  // SMS_setLineCounter(192 - 95);
  // SMS_enableLineInterrupt();

  while (1) {
    // GAME****************************************
    ProcessGamePhase(0);

    // VBlank****************************************
    
    //VDPBlank = false;
    //while (!VDPBlank) waitCount++;

    // PrintWait(waitCount | delayCount);
    // printHexShort(scx, 1, 1, 4);
    waitCount = 0;
    delayCount = 0;

    // GAME****************************************
    ProcessGamePhase(1);
    FinishVBlank();
  }
}

void processLogo(char vblank);
void processPlayer(char vblank);

#define CODE_BANK_S 1
#define DATA_BANK_S 3

void ProcessGamePhase(char vblank) {
  switch (GamePhase) {
    case G_PHASE_LOGO: {
      SMS_mapCODEBank(CODE_BANK_S + 0);
      SMS_mapROMBank(DATA_BANK_S + 0);
      processLogo(vblank);
      break;
    }
    case G_PHASE_PLAYER: {
      SMS_mapCODEBank(CODE_BANK_S + 1);
      SMS_mapROMBank(DATA_BANK_S + 1);
      processPlayer(vblank);
      break;
    }
    default:
      break;
  }
  if (LastGamePhase != GamePhase) {
    PhaseCounter = 0;
    PhaseLocalCounter = 0;
    PhaseLocalCounter2 = 0;
    PhaseLocalCounter3 = 0;
    PhaseLocalCounter4 = 0;
  }
  LastGamePhase = GamePhase;
}

void InitGameVars() {
  SMS_initSprites();
  SMS_zeroBGPalette();
  for (int i = 0; i < 16; i++) SetBGPaletteColor(i, 0);
  SMS_zeroSpritePalette();
  for (int i = 0; i < 16; i++) SetSpritePaletteColor(i, 0);
  ClearBG();
  PhaseCounter = 0;
  PhaseLocalCounter = 0;
  PhaseLocalCounter2 = 0;
  PhaseLocalCounter3 = 0;
  PhaseLocalCounter4 = 0;
  ScreenX = 0;
  ScreenY = 0;
  lscx = 0;
  lscy = 0;
  SMS_setBGScrollX(0);
  SMS_setBGScrollY(0);
  IntCount = 0;

  for (int i = 0; i < 128; i++) {
    Characters[i].counter = 0;
    Characters[i].dir = 0;
    Characters[i].spriteNo = 0;
    Characters[i].status = 0;
    Characters[i].tag = 0;
    Characters[i].type = 0;
    Characters[i].x.Body.Integer = 0;
    Characters[i].x.Body.Fraction = 0;
    Characters[i].y.Body.Integer = 0;
    Characters[i].y.Body.Fraction = 0;
    Characters[i].dx.Word = 0;
    Characters[i].dy.Word = 0;
  }
}