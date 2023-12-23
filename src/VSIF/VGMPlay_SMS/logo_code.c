#pragma codeseg BANK_C1

#include "main.h"
#include "logo_tile.h"


void processLogo(char vblank)
{
    if (SMS_getKeysPressed() == PORT_A_KEY_2)
    {
        PhaseLocalCounter = 0;
        PhaseCounter = 12;
    }

    if (vblank)
    {
        switch (PhaseCounter)
        {
        case 0:
        {
            DisableVDPProcessing = true;
            SMS_displayOff();
            InitGameVars();
            SMS_setSpriteMode(SPRITEMODE_TALL_ZOOMED);
            SMS_VDPturnOnFeature(VDPFEATURE_SHIFTSPRITES);
            SMS_VDPturnOnFeature(VDPFEATURE_HIDEFIRSTCOL);

            SMS_loadPSGaidencompressedTiles(logo_bg_tile_psgcompr, BG_TILES_NO_S);
            SMS_loadTileMapArea(16 - (192 / 8 / 2), 11, logo_bg_map_bin_sp, 24, 2);
            SMS_loadPSGaidencompressedTiles(logo_sp_tile_psgcompr, SP_TILES_NO_S);
       
            PhaseCounter++;
            PhaseLocalCounter = 0;

            SMS_displayOn();
            DisableVDPProcessing = false;
            break;
        }
        case 1:
        {
            FadeInPalette(PhaseLocalCounter, logo_bg_pal_bin, logo_bg_pal_bin_size, 0);
            PhaseLocalCounter++;
            if (PhaseLocalCounter == 7)
            {
                PhaseCounter++;
                PhaseLocalCounter = 0;
            }
            break;
        }
        case 2:
        {
            PhaseCounter++;
            PhaseLocalCounter = 0;

            break;
        }
        case 3:
        {
            FadeOutPalette(PhaseLocalCounter, logo_bg_pal_bin, logo_bg_pal_bin_size, 0);
            PhaseLocalCounter++;
            if (PhaseLocalCounter == 7)
            {
                PhaseCounter++;
                PhaseLocalCounter = 0;
                SetVBlankInterruptHandler(0);
                SMS_disableLineInterrupt();
            }
            break;
        }
        case 4:
        {
            GamePhase = G_PHASE_PLAYER;
            break;
        }
        }
    }
}
