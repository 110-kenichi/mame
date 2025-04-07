#include "main.h"


extern struct PcmData pcmDataTable[256];

BOOL fileExists(STRPTR filename)
{
    BPTR lock = Lock(filename, ACCESS_READ);
    if (lock)
    {
        UnLock(lock);
        return TRUE;  // ファイルあり
    }
    return FALSE;     // ファイルなし
}

void loadPcm(CONST_STRPTR filename) {
    LONG readByte = 0;
    BPTR file = Open(filename, MODE_OLDFILE);
    if (!file)
    {
        ULONG arg[] = { (ULONG)filename};
        VWritef("Failed to open the %s!\n", arg);
        Close(file);
        return;
    }

    //vsif
    char vsifId[4] = {0};
    readByte = Read(file, vsifId, 4);
    if (readByte < 4)
    {
        ULONG arg[] = {(ULONG)filename};
        VWritef("Unknown format file %s!\n", arg);
        Close(file);
        return;
    }
    if (vsifId[0] != 'V' || vsifId[1] != 'A'||  vsifId[2] != 'P' || vsifId[3] != 'C')
    {
        ULONG arg[] = {(ULONG)filename};
        VWritef("Unknown format file header %s!\n", arg);
        Close(file);
        return;
    }
    //00 00 : Len
    //00 00 : Loop
    //....
    // x 256
    VWritef("Read pcm file... %s!\n", NULL);
    for(int i=0;i<256;i++)
    {
        if(pcmDataTable[i].dataPtr != NULL)
            FreeMem(pcmDataTable[i].dataPtr, pcmDataTable[i].length);
        pcmDataTable[i].dataPtr = 0;

        UBYTE len_[2] = {0};
        readByte = Read(file, len_, 2);
        if(readByte < 2){
            ULONG arg[] = {i, (ULONG)filename};
            VWritef("Unknown format file(%N) %s!\n", arg);
            VWritef("Aborted!\n", NULL);
            break;
        }
        pcmDataTable[i].length = *((USHORT*)len_);

        UBYTE loop_[2] = {0};
        readByte = Read(file, loop_, 2);
        if(readByte < 2){
            ULONG arg[] = {i, (ULONG)filename};
            VWritef("Unknown format file(%N) %s!\n", arg);
            VWritef("Aborted!\n", NULL);
            break;
        }

        pcmDataTable[i].loop = *((USHORT*)loop_);
        if(pcmDataTable[i].loop >= pcmDataTable[i].length)
            pcmDataTable[i].loop = 0xFFFF;

        if(pcmDataTable[i].length == 0)
            continue;
        BYTE* pcmDataPtr = (BYTE*)AllocMem(pcmDataTable[i].length, MEMF_CHIP);
        if(pcmDataPtr != NULL)
        {
            readByte = Read(file, pcmDataPtr, pcmDataTable[i].length);
            if(readByte != pcmDataTable[i].length)
            {
                FreeMem(pcmDataPtr, pcmDataTable[i].length);
                ULONG arg[] = {i, (ULONG)filename};
                VWritef("File read error(%N) %s!\n", arg);
                VWritef("Aborted!\n", NULL);
                break;
            }else{
                pcmDataTable[i].dataPtr = pcmDataPtr;          
            }
        }else
        {
            ULONG arg[] = {i, (ULONG)filename};
            VWritef("No PCM memory(%N) %s!\n", arg);
            VWritef("Aborted!\n", NULL);
            break;
        }

    }
    VWritef("Done!\n", NULL);

    Close(file);
}
