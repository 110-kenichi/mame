#include "main.h"

SMS_EMBED_SEGA_ROM_HEADER(9999, 0);

#include "Font.h"

unsigned char FreeSpaceC000_C3ff[0x400];

GamePhaseType GamePhase = G_PHASE_PLAYER;
GamePhaseType LastGamePhase = G_PHASE_LOGO;

unsigned short PhaseCounter = 0;
unsigned short PhaseLocalCounter = 0;
unsigned short PhaseLocalCounter2 = 0;
unsigned short PhaseLocalCounter3 = 0;
unsigned short PhaseLocalCounter4 = 0;

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

  currentBgPalette = defBgPalette;
  currentSpPalette = defSpPalette;

  SMS_setVBlankInterruptHandler(VinterruptHandler);

  while (1) {
    waitCount = 0;
    delayCount = 0;

    // GAME****************************************
    ProcessGamePhase(G_PHASE_PLAYER);
    FinishVBlank();
  }
}

//void processLogo(char vblank);
void processPlayer(char vblank);

void ProcessGamePhase(char vblank) {
  switch (GamePhase) {
    case G_PHASE_PLAYER: {
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
}