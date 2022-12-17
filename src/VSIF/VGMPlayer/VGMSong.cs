using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using zanac.VGMPlayer.Properties;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using zanac.VGMPlayer;
using static zanac.VGMPlayer.FormMain;
using System.Diagnostics;
using System.Xml.Linq;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;

//Sega Genesis VGM player. Player written and emulators ported by Landon Podbielski. 
namespace zanac.VGMPlayer
{
    public class VGMSong : SongBase
    {
        private const uint FCC_VGM = 0x206D6756;    // 'Vgm '

        private byte[] vgmData;
        private int vgmDataOffset;
        private int vgmDataCurrentOffset;
        private uint vgmDataLen;
        private VGM_HEADER vgmHead;

        private VsifClient comPortDCSG;
        private VsifClient comPortOPLL;
        private VsifClient comPortOPN2;
        private VsifClient comPortSCC;
        private VsifClient comPortY8910;
        private VsifClient comPortOPM;
        private VsifClient comPortOPL3;
        private VsifClient comPortOPNA;
        private VsifClient comPortY8950;

        private BinaryReader vgmReader;

        private List<byte> dacData = new List<byte>();
        private List<int> dacDataOffset = new List<int>();
        private List<int> dacDataLength = new List<int>();

        private int dacOffset = 0;

        private Dictionary<int, StreamData> streamTable = new Dictionary<int, StreamData>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public VGMSong(string fileName) : base(fileName)
        {
            OpenFile(fileName);

            //streamingThread = new Thread(new ThreadStart(playStream));
            //streamingThread.Priority = ThreadPriority.AboveNormal;
            //streamingThread.Start();
        }

        protected override void StopAllSounds(bool volumeOff)
        {
            if (comPortDCSG != null)
            {
                switch (comPortDCSG.SoundModuleType)
                {
                    case VsifSoundModuleType.Genesis:
                    case VsifSoundModuleType.Genesis_Low:
                    case VsifSoundModuleType.Genesis_FTDI:
                        for (int i = 0; i < 3; i++)
                            comPortDCSG.DeferredWriteData(0, 0x14, (byte)(0x80 | i << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        comPortDCSG.DeferredWriteData(0, 0x14, (byte)(0x80 | 3 << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        break;
                    case VsifSoundModuleType.SMS:
                        for (int i = 0; i < 3; i++)
                            comPortDCSG.DeferredWriteData(0, 0xFF, (byte)(0x80 | i << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        comPortDCSG.DeferredWriteData(0, 0xFF, (byte)(0x80 | 3 << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        break;
                    case VsifSoundModuleType.MSX_FTDI:
                        for (int i = 0; i < 3; i++)
                            comPortDCSG.DeferredWriteData(0xF, 0, (byte)(0x80 | i << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        comPortDCSG.DeferredWriteData(0xF, 0, (byte)(0x80 | 3 << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        break;
                }
            }
            if (comPortOPLL != null)
            {
                //KOFF
                for (int i = 0x20; i < 0x28; i++)
                    deferredWriteOPLL(i, 0);

                //comPortOPLL.DeferredWriteData(type, 0xe, (byte)(0x20), (int)Settings.Default.BitBangWaitOPLL);

                if (volumeOff)
                {
                    //TL
                    for (int i = 0x30; i <= 0x38; i++)
                        deferredWriteOPLL(i, 0xFF);
                    //RR
                    for (int i = 0x6; i <= 0x7; i++)
                        deferredWriteOPLL(i, 0xFF);
                }
            }

            if (comPortOPN2 != null)
            {
                if (comPortOPN2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                {
                    // ALL KEY OFF
                    for (int i = 0; i < 3; i++)
                    {
                        deferredWriteOPN2_P0(0x28, i);
                        deferredWriteOPN2_P0(0x28, 0x4 | i);
                    }
                }

                if (volumeOff)
                {
                    //TL
                    for (int i = 0x40; i <= 0x4F; i++)
                        deferredWriteOPN2_P0(i, 0xFF);
                    for (int i = 0x40; i <= 0x4F; i++)
                        deferredWriteOPN2_P1(i, 0xFF);
                    //RR
                    for (int i = 0x80; i <= 0x8F; i++)
                        deferredWriteOPN2_P0(i, 0xFF);
                    for (int i = 0x80; i <= 0x8F; i++)
                        deferredWriteOPN2_P1(i, 0xFF);
                }
            }

            if (comPortOPNA != null)
            {
                if (comPortOPNA.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                {
                    // ALL KEY OFF
                    for (int i = 0; i < 3; i++)
                    {
                        deferredWriteOPNA_P0(0x28, i);
                        deferredWriteOPNA_P0(0x28, 0x4 | i);
                    }
                    //RHYTHM
                    deferredWriteOPNA_P0(0x10, 0x80);
                    //SSG
                    deferredWriteOPNA_P0(0x07, 0xFF);
                    //ADPCM
                    deferredWriteOPNA_P1(0x00, 1);
                }

                if (volumeOff)
                {
                    //TL
                    for (int i = 0x40; i <= 0x4F; i++)
                        deferredWriteOPNA_P0(i, 0xFF);
                    for (int i = 0x40; i <= 0x4F; i++)
                        deferredWriteOPNA_P1(i, 0xFF);
                    //RR
                    for (int i = 0x80; i <= 0x8F; i++)
                        deferredWriteOPNA_P0(i, 0xFF);
                    for (int i = 0x80; i <= 0x8F; i++)
                        deferredWriteOPNA_P1(i, 0xFF);
                    //RHYTHM
                    deferredWriteOPNA_P0(0x11, 0xFF);
                    //SSG
                    deferredWriteOPNA_P0(0x08, 0);
                    deferredWriteOPNA_P0(0x09, 0);
                    deferredWriteOPNA_P0(0x0A, 0);
                    //ADPCM
                    deferredWriteOPNA_P1(0x00, 1);
                }
            }

            if (comPortOPM != null)
            {
                //KOFF
                for (int i = 0; i < 8; i++)
                    deferredWriteOPM(0x08, i);

                if (volumeOff)
                {
                    //TL
                    for (int i = 0x60; i <= 0x7f; i++)
                        deferredWriteOPM(i, 0xFF);
                    //RR
                    for (int i = 0xE0; i <= 0xFF; i++)
                        deferredWriteOPM(i, 0xFF);
                }
            }
            if (comPortOPL3 != null)
            {
                //KOFF
                for (int i = 0; i < 9; i++)
                    deferredWriteOPL3_P0(0xB0 + i, 0);
                for (int i = 0; i < 9; i++)
                    deferredWriteOPL3_P1(0xB0 + i, 0);

                if (volumeOff)
                {
                    //TL
                    for (int i = 0x40; i <= 0x55; i++)
                        deferredWriteOPL3_P0(i, 0xFF);
                    for (int i = 0x40; i <= 0x55; i++)
                        deferredWriteOPL3_P1(i, 0xFF);
                    //RR
                    for (int i = 0x80; i <= 0x95; i++)
                        deferredWriteOPL3_P0(i, 0xFF);
                    for (int i = 0x80; i <= 0x95; i++)
                        deferredWriteOPL3_P1(i, 0xFF);
                }
            }

            if (comPortY8950 != null)
            {
                //KOFF
                for (int i = 0; i < 9; i++)
                    deferredWriteY8950(0xB0 + i, 0);

                if (volumeOff)
                {
                    //TL
                    for (int i = 0x40; i <= 0x55; i++)
                        deferredWriteY8950(i, 0xFF);
                    //RR
                    for (int i = 0x80; i <= 0x95; i++)
                        deferredWriteY8950(i, 0xFF);
                }
            }

            //SCC
            if (comPortSCC != null)
            {
                if (comPortSCC.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                {
                    comPortSCC.DeferredWriteData(4, (byte)0xaf, (byte)00, (int)Settings.Default.BitBangWaitSCC);
                    comPortSCC.DeferredWriteData(5, (byte)0x8f, (byte)00, (int)Settings.Default.BitBangWaitSCC);

                    if (volumeOff)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            comPortSCC.DeferredWriteData(4, (byte)(0xaa + i), (byte)00, (int)Settings.Default.BitBangWaitSCC);
                            comPortSCC.DeferredWriteData(5, (byte)(0x8a + i), (byte)00, (int)Settings.Default.BitBangWaitSCC);
                        }
                    }
                }
            }

            //Y8910
            if (comPortY8910 != null)
            {
                if (comPortY8910.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                {
                    //comPortY8910?.DeferredWriteData(0, (byte)0x07, (byte)0xff, (int)Settings.Default.BitBangWaitAY8910);
                    comPortY8910.DeferredWriteData(0, (byte)0x08, (byte)0x00, (int)Settings.Default.BitBangWaitAY8910);
                    comPortY8910.DeferredWriteData(0, (byte)0x09, (byte)0x00, (int)Settings.Default.BitBangWaitAY8910);
                    comPortY8910.DeferredWriteData(0, (byte)0x0a, (byte)0x00, (int)Settings.Default.BitBangWaitAY8910);
                }
            }

            flushDeferredWriteDataAndWait();
            Thread.Sleep(250);
        }

        private static byte[] chAddressOffset = new byte[] { 0x00, 0x01, 0x02, 0x08, 0x09, 0x0a, 0x10, 0x11, 0x12 };

        private static byte[,] op2chConselTable = new byte[,] {
            { 00, 01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16 ,17 },
            { 01, 02, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16 ,17 ,00 ,00 },
            { 02, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16 ,17 ,00, 00, 00, 00 },
            { 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16 ,17 ,00, 00, 00, 00, 00, 00 },
            { 06, 07, 08, 10, 11, 13, 14, 15, 16 ,17 ,00, 00, 00, 00, 00, 00, 00, 00 },
            { 06, 07, 08, 11, 14, 15, 16 ,17 ,00, 00, 00, 00, 00, 00, 00, 00, 00, 00 },
            { 06, 07, 08, 15, 16 ,17 ,00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 },
        };

        /// <summary>
        /// 
        /// </summary>
        private void YMF262WriteData(VsifClient comPort, uint address, int op, int slot, byte opmode, int consel, byte data, bool defferred)
        {
            //useCache = false;
            var adrL = address & 0xff;
            var adrH = (address & 0x100) >> 7;  // 0 or 2
            switch (opmode)
            {
                //Channel        0   1   2   3   4   5   6   7   8
                //Operator 1    00  01  02  06  07  09  0C  0D  0E
                //Operator 2    03  04  05  09  0A  0B  0F  10  11
                case 0:
                case 1:
                    slot = op2chConselTable[consel, slot];
                    if (slot >= 9)
                    {
                        adrH = 2;
                        slot = slot - 9;
                    }
                    break;
                //Channel        0   1   2    
                //Operator 1    00  01  02  
                //Operator 2    03  04  05  
                //Operator 3    07  08  09  
                //Operator 4    0A  0B  0C  
                default:
                    if (slot >= 3)
                    {
                        adrH = 2;
                        slot -= 3;
                    }
                    if (op >= 2)
                    {
                        op -= 2;
                        slot += 3;
                    }
                    break;
            }

            byte chofst = 0;
            switch (adrL)
            {
                case 0x20:
                case 0x40:
                case 0x60:
                case 0x80:
                case 0xe0:
                    chofst = chAddressOffset[slot];
                    break;
                case 0xa0:
                case 0xb0:
                case 0xc0:
                    chofst = (byte)slot;
                    break;
            }

            var adr = (byte)(adrL + (op * 3) + chofst);
            address = (adrH << 8) | adr;

            if (defferred)
            {
                switch (adrH)
                {
                    case 0:
                        comPort?.DeferredWriteData(10, adr, data, (int)Settings.Default.BitBangWaitOPL3);
                        break;
                    case 2:
                        comPort?.DeferredWriteData(11, adr, data, (int)Settings.Default.BitBangWaitOPL3);
                        break;
                }
            }
            else
            {
                switch (adrH)
                {
                    case 0:
                        comPort?.WriteData(10, adr, data, (int)Settings.Default.BitBangWaitOPL3);
                        break;
                    case 2:
                        comPort?.WriteData(11, adr, data, (int)Settings.Default.BitBangWaitOPL3);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void YM2608WriteData(byte address, int op, int slot, byte data, bool deferred)
        {
            switch (op)
            {
                case 0:
                    op = 0;
                    break;
                case 1:
                    op = 2;
                    break;
                case 2:
                    op = 1;
                    break;
                case 3:
                    op = 3;
                    break;
            }

            if (comPortOPNA != null)
            {
                if (comPortOPNA.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                {
                    comPortOPNA.DeferredWriteData((byte)(0x10 + (slot / 3)), (byte)(address + (op * 4) + (slot % 3)), data, (int)Settings.Default.BitBangWaitOPNA);
                }
            }
        }

        private VGM_HEADER readVGMHeader(BinaryReader hFile)
        {
            VGM_HEADER curHead = new VGM_HEADER();
            FieldInfo[] fields = typeof(VGM_HEADER).GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(uint))
                {
                    uint val = hFile.ReadUInt32();
                    field.SetValue(curHead, val);
                }
                else if (field.FieldType == typeof(ushort))
                {
                    ushort val = hFile.ReadUInt16();
                    field.SetValue(curHead, val);
                }
                else if (field.FieldType == typeof(sbyte))
                {
                    sbyte val = hFile.ReadSByte();
                    field.SetValue(curHead, val);
                }
                else if (field.FieldType == typeof(byte))
                {
                    byte val = hFile.ReadByte();
                    field.SetValue(curHead, val);
                }
            }

            // Header preperations
            if (curHead.lngVersion < 0x00000101)
            {
                curHead.lngRate = 0;
            }
            if (curHead.lngVersion < 0x00000110)
            {
                curHead.shtPSG_Feedback = 0x0000;
                curHead.bytPSG_SRWidth = 0x00;
                curHead.lngHzYM2612 = curHead.lngHzYM2413;
                curHead.lngHzYM2151 = curHead.lngHzYM2413;
            }

            if (curHead.lngHzDCSG != 0)
            {
                if (curHead.shtPSG_Feedback == 0)
                    curHead.shtPSG_Feedback = 0x0009;
                if (curHead.bytPSG_SRWidth == 0)
                    curHead.bytPSG_SRWidth = 0x10;

                if (Settings.Default.DCSG_Enable)
                {
                    coonectToDCSG();
                }
            }
            if (curHead.lngHzYM2413 != 0)
            {
                if (Settings.Default.OPLL_Enable)
                {
                    connectToOPLL();
                }
            }
            if (curHead.lngHzYM2612 != 0 && curHead.lngVersion >= 0x00000110)
            {
                if (Settings.Default.OPNA2_Enable)
                {
                    connectToOPN2();
                }
                else if (Settings.Default.OPNA_Enable)
                {
                    if (connectToOPNA())
                    {
                        //Force OPN mode
                        deferredWriteOPNA_P0(0x29, 0x80);
                    }
                }
            }
            if (curHead.lngHzYM2151 != 0 && curHead.lngVersion >= 0x00000110)
            {
                if (Settings.Default.OPM_Enable)
                {
                    connectToOPM();
                }
            }
            if (curHead.lngHzYM3812 != 0 && curHead.lngVersion >= 0x00000151)
            {
                if (Settings.Default.OPL3_Enable)
                {
                    connectToOPL3();
                }
                else if (Settings.Default.Y8950_Enable)
                {
                    connectToMsxAudio();
                }
            }
            if (curHead.lngHzYM3526 != 0 && curHead.lngVersion >= 0x00000151)
            {
                if (Settings.Default.OPL3_Enable)
                {
                    connectToOPL3();
                }
                else if (Settings.Default.Y8950_Enable)
                {
                    connectToMsxAudio();
                }
            }
            if (curHead.lngHzYMF262 != 0 && curHead.lngVersion >= 0x00000151)
            {
                if (Settings.Default.OPL3_Enable)
                {
                    connectToOPL3();
                }
            }
            if (curHead.lngHzYM2203 != 0 && curHead.lngVersion >= 0x00000151)
            {
                if (Settings.Default.OPNA_Enable)
                {
                    connectToOPNA();
                }
                else if (Settings.Default.OPNA2_Enable)
                {
                    if (connectToOPN2())
                    {
                        if (Settings.Default.Y8910_Enable)
                        {
                            connectToPSG();
                        }
                    }
                }
            }
            if (curHead.lngHzYM2608 != 0 && curHead.lngVersion >= 0x00000151)
            {
                if (Settings.Default.OPNA_Enable)
                {
                    connectToOPNA();
                }
                else if (Settings.Default.OPNA2_Enable)
                {
                    if (connectToOPN2())
                    {
                        if (Settings.Default.Y8910_Enable)
                        {
                            connectToPSG();
                        }
                    }
                }
            }
            if (curHead.lngHzYM2610 != 0 && curHead.lngVersion >= 0x00000151)
            {
                if (Settings.Default.OPNA_Enable)
                {
                    connectToOPNA();
                }
                else if (Settings.Default.OPNA2_Enable)
                {
                    if (connectToOPN2())
                    {
                        if (Settings.Default.Y8910_Enable)
                        {
                            connectToPSG();
                        }
                    }
                }
            }
            if (curHead.lngHzY8950 != 0 && curHead.lngVersion >= 0x00000151)
            {
                if (Settings.Default.Y8950_Enable)
                {
                    connectToMsxAudio();
                }
                else if (Settings.Default.OPL3_Enable)
                {
                    connectToOPL3();
                }
            }
            if (curHead.lngHzK051649 != 0 && curHead.lngVersion >= 0x00000161)
            {
                if (Settings.Default.SCC_Enable)
                {
                    connectToSCC();
                }
            }
            if (curHead.lngHzAY8910 != 0 && curHead.lngVersion >= 0x00000151)
            {
                if (Settings.Default.Y8910_Enable)
                {
                    connectToPSG();
                }
            }

            return curHead;
        }

        private bool connectToPSG()
        {
            if (comPortY8910 == null)
            {
                switch (Settings.Default.Y8910_IF)
                {
                    case 0:
                        if (comPortY8910 == null)
                            comPortY8910 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.Y8910_Port);
                        break;
                    case 1:
                        if (comPortY8910 == null)
                            comPortY8910 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Generic_UART,
                                (PortId)Settings.Default.Y8910_Port);
                        break;
                }
                if (comPortY8910 != null)
                {
                    Accepted = true;

                    return true;
                }
            }
            return false;
        }

        private bool connectToSCC()
        {
            if (comPortSCC == null)
            {
                switch (Settings.Default.SCC_IF)
                {
                    case 0:
                        if (comPortSCC == null)
                            comPortSCC = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                              (PortId)Settings.Default.SCC_Port);
                        break;
                }
                if (comPortSCC != null)
                {
                    Accepted = true;

                    switch (comPortSCC.SoundModuleType)
                    {
                        case VsifSoundModuleType.MSX_FTDI:
                            SCCType type = (SCCType)comPortSCC.Tag["SCC.Type"];
                            var slot = (int)comPortSCC.Tag["SCC.Slot"];
                            if ((int)slot < 0)
                                //自動選択方式
                                comPortSCC.DeferredWriteData(3, (byte)type,
                                    (byte)(-((int)slot + 1)), (int)Settings.Default.BitBangWaitSCC);
                            else
                                //従来方式
                                comPortSCC.DeferredWriteData(3, (byte)(type + 4),
                                    (byte)slot, (int)Settings.Default.BitBangWaitSCC);

                            for (int i = 0; i < 0xFF; i++)
                            {
                                switch (type)
                                {
                                    case SCCType.SCC1:
                                        comPortSCC.DeferredWriteData(4, (byte)i, (byte)0, (int)Settings.Default.BitBangWaitSCC);
                                        break;
                                    case SCCType.SCC1_Compat:
                                    case SCCType.SCC:
                                        comPortSCC.DeferredWriteData(5, (byte)i, (byte)0, (int)Settings.Default.BitBangWaitSCC);
                                        break;
                                }
                            }
                            break;
                    }

                    return true;
                }
            }
            return false;
        }

        private bool connectToMsxAudio()
        {
            if (comPortY8950 == null)
            {
                switch (Settings.Default.Y8950_IF)
                {
                    case 0:
                        if (comPortY8950 == null)
                            comPortY8950 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.Y8950_Port);
                        break;
                }
                if (comPortY8950 != null)
                {
                    Accepted = true;

                    for (int i = 0x20; i <= 0x3F; i++)
                        deferredWriteY8950(i, 0);
                    //for (int i = 0x40; i <= 0x5F; i++)
                    //    deferredWriteY8950(i, 0xff);
                    for (int i = 0x60; i <= 0xC8; i++)
                        deferredWriteY8950(i, 0);

                    return true;
                }
            }
            return false;
        }

        private bool connectToOPL3()
        {
            if (comPortOPL3 == null)
            {
                switch (Settings.Default.OPL3_IF)
                {
                    case 0:
                        if (comPortOPL3 == null)
                            comPortOPL3 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.OPL3_Port);
                        break;
                }
                if (comPortOPL3 != null)
                {
                    Accepted = true;

                    //WSE Disable
                    deferredWriteOPL3_P0(0x0, 0);
                    //Force 2op mode
                    deferredWriteOPL3_P1(0x4, 0);
                    //Force OPL mode
                    deferredWriteOPL3_P1(0x5, 0);

                    for (int i = 0x20; i <= 0x3F; i++)
                        deferredWriteOPL3_P0(i, 0);
                    //for (int i = 0x40; i <= 0x5F; i++)
                    //    deferredWriteOPL3_P0(i, 0xFF);
                    for (int i = 0x60; i <= 0xF5; i++)
                        deferredWriteOPL3_P0(i, 0);

                    for (int i = 0x20; i <= 0x3F; i++)
                        deferredWriteOPL3_P1(i, 0);
                    //for (int i = 0x40; i <= 0x5F; i++)
                    //    deferredWriteOPL3_P1(i, 0xFF);
                    for (int i = 0x60; i <= 0xF5; i++)
                        deferredWriteOPL3_P1(i, 0);

                    return true;
                }
            }
            return false;
        }

        private bool connectToOPM()
        {
            if (comPortOPM == null)
            {
                switch (Settings.Default.OPM_IF)
                {
                    case 0:
                        if (comPortOPM == null)
                            comPortOPM = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                               (PortId)Settings.Default.OPM_Port);
                        break;
                }
                if (comPortOPM != null)
                {
                    Accepted = true;

                    for (int i = 0x00; i <= 0x1F; i++)
                        deferredWriteOPM(i, 0x00);
                    for (int i = 0x20; i <= 0x27; i++)
                        deferredWriteOPM(i, 0xC0);
                    for (int i = 0x28; i <= 0x5F; i++)
                        deferredWriteOPM(i, 0x00);
                    //for (int i = 0x60; i <= 0x7F; i++)
                    //    deferredWriteOPM(i, 0xff);
                    for (int i = 0x80; i <= 0xFF; i++)
                        deferredWriteOPM(i, 0);

                    return true;
                }
            }
            return false;
        }

        private bool connectToOPNA()
        {
            if (comPortOPNA == null)
            {
                switch (Settings.Default.OPNA_IF)
                {
                    case 0:
                        if (comPortOPNA == null)
                            comPortOPNA = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.OPNA_Port);
                        break;
                }
                if (comPortOPNA != null)
                {
                    Accepted = true;

                    //LFO
                    deferredWriteOPNA_P0(0x22, 0x00);
                    //channel 3 mode
                    deferredWriteOPNA_P0(0x27, 0x00);
                    //Force OPN mode
                    deferredWriteOPNA_P0(0x29, 0x00);

                    for (int i = 0x30; i <= 0x3F; i++)
                        deferredWriteOPNA_P0(i, 0);
                    //for (int i = 0x40; i <= 0x4F; i++)
                    //    deferredWriteOPNA_P0(i, 0xff);
                    for (int i = 0x50; i <= 0xB3; i++)
                        deferredWriteOPNA_P0(i, 0);
                    for (int i = 0xB4; i <= 0xB6; i++)
                        deferredWriteOPNA_P0(i, 0xC0);

                    for (int i = 0x30; i <= 0x3F; i++)
                        deferredWriteOPNA_P1(i, 0);
                    //for (int i = 0x40; i <= 0x4F; i++)
                    //    deferredWriteOPNA_P1(i, 0xff);
                    for (int i = 0x50; i <= 0xB3; i++)
                        deferredWriteOPNA_P1(i, 0);
                    for (int i = 0xB4; i <= 0xB6; i++)
                        deferredWriteOPNA_P1(i, 0xC0);

                    return true;
                }
            }
            return false;
        }

        private bool connectToOPN2()
        {
            if (comPortOPN2 == null)
            {
                switch (Settings.Default.OPNA2_IF)
                {
                    case 0:
                        if (comPortOPN2 == null)
                            comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                                (PortId)Settings.Default.OPNA2_Port);
                        break;
                    case 1:
                        if (comPortOPN2 == null)
                            comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                                (PortId)Settings.Default.OPNA2_Port);
                        break;
                    case 2:
                        if (comPortOPN2 == null)
                            comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
                                (PortId)Settings.Default.OPNA2_Port);
                        break;
                    case 3:
                        if (comPortOPN2 == null)
                            comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.OPNA2_Port);
                        break;
                }
                if (comPortOPN2 != null)
                {
                    Accepted = true;

                    //LFO
                    deferredWriteOPN2_P0(0x22, 0x00);
                    //channel 3 mode
                    deferredWriteOPN2_P0(0x27, 0x00);
                    //DAC OFF
                    deferredWriteOPN2_P0(0x2B, 0x00);

                    for (int i = 0x30; i <= 0x3F; i++)
                        deferredWriteOPN2_P0(i, 0x00);
                    //for (int i = 0x40; i <= 0x4F; i++)
                    //    deferredWriteOPN2_P0(i, 0xff);
                    for (int i = 0x50; i <= 0xB3; i++)
                        deferredWriteOPN2_P0(i, 0x00);
                    for (int i = 0xB4; i <= 0xB6; i++)
                        deferredWriteOPN2_P0(i, 0xC0);

                    for (int i = 0x30; i <= 0x3F; i++)
                        deferredWriteOPN2_P1(i, 0x00);
                    //for (int i = 0x40; i <= 0x4F; i++)
                    //    deferredWriteOPN2_P1(i, 0xff);
                    for (int i = 0x50; i <= 0xB3; i++)
                        deferredWriteOPN2_P1(i, 0x00);
                    for (int i = 0xB4; i <= 0xB6; i++)
                        deferredWriteOPN2_P1(i, 0xC0);

                    return true;
                }
            }
            return false;
        }

        private bool connectToOPLL()
        {
            if (comPortOPLL == null)
            {
                switch (Settings.Default.OPLL_IF)
                {
                    case 0:
                        if (comPortOPLL == null)
                            comPortOPLL = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                                (PortId)Settings.Default.OPLL_Port);
                        break;
                    case 1:
                        if (comPortOPLL == null)
                            comPortOPLL = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.OPLL_Port);
                        break;
                }
                if (comPortOPLL != null)
                {
                    Accepted = true;

                    for (int i = 0x00; i <= 0x2F; i++)
                        deferredWriteOPLL(i, 0);
                    //for (int i = 0x30; i <= 0x38; i++)
                    //    deferredWriteOPLL(i, 0xff);

                    return true;
                }
            }
            return false;
        }

        private bool coonectToDCSG()
        {
            if (comPortDCSG == null)
            {
                switch (Settings.Default.DCSG_IF)
                {
                    case 0:
                        if (comPortDCSG == null)
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                                (PortId)Settings.Default.DCSG_Port);
                        break;
                    case 1:
                        if (comPortDCSG == null)
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                                (PortId)Settings.Default.DCSG_Port);
                        break;
                    case 2:
                        if (comPortDCSG == null)
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                                (PortId)Settings.Default.DCSG_Port);
                        break;
                    case 3:
                        if (comPortDCSG == null)
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
                                (PortId)Settings.Default.DCSG_Port);
                        break;
                    case 4:
                        if (comPortDCSG == null)
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                               (PortId)Settings.Default.DCSG_Port);
                        break;
                }
                if (comPortDCSG != null)
                {
                    Accepted = true;

                    return true;
                }
            }
            return false;
        }

        bool checkIfZip(string filepath, int signatureSize, string expectedSignature)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fs.Length < signatureSize)
                    return false;
                byte[] signature = new byte[signatureSize];
                int bytesRequired = signatureSize;
                int index = 0;
                while (bytesRequired > 0)
                {
                    int bytesRead = fs.Read(signature, index, bytesRequired);
                    bytesRequired -= bytesRead;
                    index += bytesRead;
                }
                string actualSignature = BitConverter.ToString(signature);
                if (actualSignature == expectedSignature)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected virtual void OpenFile(string fileName)
        {
            bool zipped = checkIfZip(fileName, 3, "1F-8B-08");

            //Read size
            using (Stream vgmFile = File.Open(fileName, FileMode.Open))
            {
                ReadVgmFile(zipped, vgmFile);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipped"></param>
        /// <param name="vgmFile"></param>
        /// <returns></returns>
        protected void ReadVgmFile(bool zipped, Stream vgmFile)
        {
            uint fileSize = 0;
            int offset = 0;

            if (zipped)
            {
                vgmFile.Position = vgmFile.Length - 4;
                byte[] b = new byte[4];
                vgmFile.Read(b, 0, 4);
                fileSize = BitConverter.ToUInt32(b, 0);
                vgmFile.Position = 0;

                GZipStream stream = new GZipStream(vgmFile, CompressionMode.Decompress);
                vgmReader = new BinaryReader(stream);
                zipped = true;
            }
            else
            {
                fileSize = (uint)vgmFile.Length;
                vgmReader = new BinaryReader(vgmFile);
            }

            uint fccHeader;
            fccHeader = (uint)vgmReader.ReadUInt32();
            if (fccHeader != FCC_VGM)
            {
                throw new IOException("VGM file error");
            }

            vgmDataLen = fileSize;
            vgmHead = readVGMHeader(vgmReader);

            //Figure out header offset
            offset = (int)vgmHead.lngDataOffset;
            if (offset == 0 || offset == 0x0000000C)
                offset = 0x40;
            vgmDataOffset = offset;
            vgmDataCurrentOffset = vgmDataOffset;
            //}
            //using (FileStream vgmFile = File.Open(fileName, FileMode.Open))
            //{
            vgmFile.Seek(0, SeekOrigin.Begin);
            if (zipped)
            {
                GZipStream stream = new GZipStream(vgmFile, CompressionMode.Decompress);
                vgmReader = new BinaryReader(stream);
            }
            else
            {
                vgmReader = new BinaryReader(vgmFile);
                vgmReader.BaseStream.Seek(0, SeekOrigin.Begin);
            }
            vgmReader.ReadBytes(offset);
            vgmData = vgmReader.ReadBytes((int)(fileSize - offset));

            if (comPortOPNA != null)
                ym2608_adpcmbit8 = searchOpnaRamType(vgmData);

            if (comPortY8950 != null)
                y8950_adpcmbit64k = searchY8950RamType(vgmData);

            vgmReader = new BinaryReader(new MemoryStream(vgmData));
        }

        //https://github.com/kuma4649/MDPlayer/blob/stable/LICENSE.txt
        //https://github.com/kuma4649/MDPlayer/blob/stable/MDPlayer/MDPlayer/Driver/vgm.cs

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vgmBuf"></param>
        /// <param name="adr"></param>
        /// <returns></returns>
        private UInt32 getLE32(byte[] vgmBuf, UInt32 adr)
        {
            UInt32 dat;
            dat = (UInt32)vgmBuf[adr] + (UInt32)vgmBuf[adr + 1] * 0x100 + (UInt32)vgmBuf[adr + 2] * 0x10000 + (UInt32)vgmBuf[adr + 3] * 0x1000000;

            return dat;
        }

        /// <summary>
        /// OPNAのRAMTypeをデータから調べる
        /// </summary>
        /// <returns>true:x8bit false:x1bit</returns>
        private bool searchOpnaRamType(byte[] vgmBuf)
        {
            try
            {
                long adr = 0;

                while (adr < vgmBuf.Length && vgmBuf[adr] != 0x66)
                {
                    byte dat = vgmBuf[adr];
                    if (dat <= 0x31) adr++;
                    else if (0x4f <= dat && dat < 0x51) adr += 2;
                    else if (dat < 0x57) adr += 3;
                    else if (dat == 0x57)
                    {
                        byte reg = vgmBuf[adr + 1];
                        byte val = vgmBuf[adr + 2];
                        adr += 3;
                        if (reg == 1)
                        {
                            if ((val & 2) != 0)
                            {
                                return true;
                            }
                        }
                    }
                    else if (dat < 0x62) adr += 3;
                    else if (dat < 0x64) adr++;
                    else if (dat == 0x64) adr += 4;
                    else if (dat == 0x66) adr++;
                    else if (dat == 0x67)
                    {
                        uint bLen = getLE32(vgmBuf, (uint)(adr + 3));
                        bLen &= 0x7fffffff;
                        adr += bLen + 7;
                    }
                    else if (dat == 0x68)
                    {
                        adr += 12;
                    }
                    else if (dat < 0x90) adr++;
                    else if (dat == 0x90) adr += 5;
                    else if (dat == 0x91) adr += 5;
                    else if (dat == 0x92) adr += 6;
                    else if (dat == 0x93) adr += 11;
                    else if (dat == 0x94) adr += 2;
                    else if (dat == 0x95) adr += 5;
                    else if (dat < 0xc0) adr += 3;
                    else if (dat < 0xe0) adr += 4;
                    else if (dat == 0xe0) adr += 5;
                    else if (dat == 0xe1) adr += 4;
                    else adr++;
                }
            }
            catch
            {
                ;
            }

            return false;
        }


        /// <summary>
        /// Y8950のRAMTypeをデータから調べる
        /// </summary>
        /// <returns>true:64kbit false:256kbit</returns>
        private bool searchY8950RamType(byte[] vgmBuf)
        {
            try
            {
                long adr = 0;

                while (adr < vgmBuf.Length && vgmBuf[adr] != 0x66)
                {
                    byte dat = vgmBuf[adr];
                    if (dat <= 0x31) adr++;
                    else if (0x4f <= dat && dat < 0x51) adr += 2;
                    else if (dat < 0x5c) adr += 3;
                    else if (dat == 0x5c)
                    {
                        byte reg = vgmBuf[adr + 1];
                        byte val = vgmBuf[adr + 2];
                        adr += 3;
                        if (reg == 8)
                        {
                            if ((val & 2) != 0)
                            {
                                return true;
                            }
                        }
                    }
                    else if (dat < 0x62) adr += 3;
                    else if (dat < 0x64) adr++;
                    else if (dat == 0x64) adr += 4;
                    else if (dat == 0x66) adr++;
                    else if (dat == 0x67)
                    {
                        uint bLen = getLE32(vgmBuf, (uint)(adr + 3));
                        bLen &= 0x7fffffff;
                        adr += bLen + 7;
                    }
                    else if (dat == 0x68)
                    {
                        adr += 12;
                    }
                    else if (dat < 0x90) adr++;
                    else if (dat == 0x90) adr += 5;
                    else if (dat == 0x91) adr += 5;
                    else if (dat == 0x92) adr += 6;
                    else if (dat == 0x93) adr += 11;
                    else if (dat == 0x94) adr += 2;
                    else if (dat == 0x95) adr += 5;
                    else if (dat < 0xc0) adr += 3;
                    else if (dat < 0xe0) adr += 4;
                    else if (dat == 0xe0) adr += 5;
                    else if (dat == 0xe1) adr += 4;
                    else adr++;
                }
            }
            catch
            {
                ;
            }

            return false;
        }

        private int readByte()
        {
            vgmDataCurrentOffset++;

            if (vgmReader.BaseStream == null)
                return -1;
            if (vgmReader.BaseStream.Position == vgmReader.BaseStream.Length)
                return -1;

            byte data = vgmReader.ReadByte();
            return data;
        }

        private bool ym2608_adpcmbit8;
        private int ym2608_pcm_start;
        private int ym2608_pcm_stop;
#if DEBUG
        private string y8950_adpcmbit64kString;
#endif
        private bool y8950_adpcmbit64k;
        private bool y8950_adpcm_ram;
        private int y8950_pcm_start;
        private int y8950_pcm_stop;

        protected override void StreamSong()
        {
            vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
            double wait = 0;
            double vgmWaitDelta = 0;
            double streamWaitDelta = 0;
            double lastDiff = 0;
            {
                long freq, before, after;
                QueryPerformanceFrequency(out freq);

                bool streaming = false;
                int currentStreamIdx = 0;
                int currentStreamIdxDir = 0;
                StreamData currentStreamData = null;
                StreamParam streamParam = null;
                StreamParam currentStreamParam = null;

                while (true)
                {
                    QueryPerformanceCounter(out before);

                    if (RequestedStat == SoundState.Stopped)
                    {
                        break;
                    }
                    else if (RequestedStat == SoundState.Paused)
                    {
                        if (State != SoundState.Paused)
                        {
                            State = SoundState.Paused;
                            StopAllSounds(false);
                        }
                        Thread.Sleep(1);
                        continue;
                    }
                    else if (RequestedStat == SoundState.Freezed)
                    {
                        if (State != SoundState.Freezed)
                            State = SoundState.Freezed;
                        Thread.Sleep(1);
                        continue;
                    }
                    State = SoundState.Playing;
                    try
                    {
                        if (vgmWaitDelta <= 0)
                        {
                            int command = readByte();
                            if (command != -1)
                            {
                                switch (command)
                                {
                                    case -1:
                                        vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                        break;

                                    case 0x31:  //AY8910 stereo mask
                                        readByte();
                                        break;

                                    case 0x4F:
                                        {
                                            var data = readByte();
                                            if (data < 0)
                                                break;
                                        }
                                        break;

                                    case 0x50:  //DCSG
                                        {
                                            var data = readByte();
                                            if (data < 0)
                                                break;
                                            switch (comPortDCSG?.SoundModuleType)
                                            {
                                                case VsifSoundModuleType.Genesis_FTDI:
                                                case VsifSoundModuleType.Genesis:
                                                case VsifSoundModuleType.Genesis_Low:
                                                    comPortDCSG?.DeferredWriteData(0, 0x14, (byte)data, (int)Settings.Default.BitBangWaitDCSG);
                                                    break;
                                                case VsifSoundModuleType.SMS:
                                                    comPortDCSG?.DeferredWriteData(0, 0xFF, (byte)data, (int)Settings.Default.BitBangWaitDCSG);
                                                    break;
                                                case VsifSoundModuleType.MSX_FTDI:
                                                    comPortDCSG?.DeferredWriteData(0xF, 0, (byte)data, (int)Settings.Default.BitBangWaitDCSG);
                                                    break;
                                            }
                                        }
                                        break;

                                    case 0x51: //YM2413
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break; var dt = readByte();
                                            if (dt < 0)
                                                break;

                                            if (comPortOPLL != null)
                                                deferredWriteOPLL(adrs, dt);
                                        }
                                        break;

                                    case 0x52: //YM2612 Write Port 0
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;

                                            //ignore test and unknown registers
                                            if (adrs < 0x22 || adrs == 0x23 || adrs == 0x29 || (0x2c < adrs && adrs < 0x30))
                                                break;
                                            if (adrs > 0xb6)
                                                break;

                                            if (comPortOPN2 != null)
                                            {
                                                deferredWriteOPN2_P0(adrs, dt);
                                            }
                                            else if (comPortOPNA != null)
                                            {
                                                deferredWriteOPNA_P0(adrs, dt);
                                            }
                                        }
                                        break;

                                    case 0x53: //YM2612 Write Port 1
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;

                                            //ignore test and unknown registers
                                            if (adrs < 0x22 || adrs == 0x23 || adrs == 0x29 || (0x2c < adrs && adrs < 0x30))
                                                break;
                                            if (adrs > 0xb6)
                                                break;

                                            if (comPortOPN2 != null)
                                            {
                                                deferredWriteOPN2_P1(adrs, dt);
                                            }
                                            else if (comPortOPNA != null)
                                            {
                                                deferredWriteOPNA_P1(adrs, dt);
                                            }
                                        }
                                        break;

                                    case 0x54: //YM2151
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;

                                            if (comPortOPM != null)
                                            {
                                                deferredWriteOPM(adrs, dt);
                                            }
                                        }
                                        break;

                                    case 0x55: //YM2203
                                    case 0x56: //YM2608 Write Port 0
                                    case 0x58: //YM2610
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;

                                            if (comPortOPNA != null)
                                            {
                                                deferredWriteOPNA_P0(adrs, dt);
                                            }
                                            else if (comPortOPN2 != null)
                                            {
                                                if (adrs <= 0xd)
                                                {
                                                    if (adrs == 7)
                                                        dt &= 0x3f;
                                                    if (comPortY8910 != null)
                                                        deferredWritePSG(adrs, dt);
                                                }

                                                //ignore test and unknown registers
                                                if (adrs < 0x22 || adrs == 0x23 || adrs == 0x29 || (0x2c < adrs && adrs < 0x30))
                                                    break;
                                                if (adrs > 0xb6)
                                                    break;

                                                deferredWriteOPN2_P0(adrs, dt);
                                            }
                                        }
                                        break;

                                    case 0x57: //YM2608 Write Port 1
                                    case 0x59: //YM2610
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;

                                            if (adrs >= 0 && adrs <= 0x10)
                                            {
                                                if (adrs == 0x1)
                                                {
                                                    if ((dt & 1) == 0)
                                                    {
                                                        ym2608_adpcmbit8 = (dt & 2) == 2;

                                                        if (ym2608_adpcmbit8)
                                                        {
                                                            //FormMain.TopForm.SetStatusText("YM2608: Set 8bit ADPCM mode");
#if DEBUG
                                                            Console.WriteLine("YM2608: Set 8bit ADPCM mode");
#endif
                                                        }
                                                        else
                                                        {
                                                            //FormMain.TopForm.SetStatusText("YM2608: Set 1bit ADPCM mode");
#if DEBUG
                                                            Console.WriteLine("YM2608: Set 1bit ADPCM mode");
#endif
                                                        }
                                                    }
                                                }
                                                else if (adrs == 0x2)
                                                {
                                                    ym2608_pcm_start = (ym2608_pcm_start & 0xff00) | dt;
                                                }
                                                else if (adrs == 0x3)
                                                {
                                                    ym2608_pcm_start = (ym2608_pcm_start & 0xff) | (dt << 8);
                                                }
                                                else if (adrs == 0x4)
                                                {
                                                    ym2608_pcm_stop = (ym2608_pcm_stop & 0xff00) | dt;
                                                }
                                                else if (adrs == 0x5)
                                                {
                                                    ym2608_pcm_stop = (ym2608_pcm_stop & 0xff) | (dt << 8);
                                                    if (ym2608_adpcmbit8)
                                                    {
#if DEBUG
                                                        Console.WriteLine("YM2608: Play 8bit ADPCM(" + (ym2608_pcm_start << 5).ToString("x") + " - " + ((ym2608_pcm_stop << 5) | 0x1f).ToString("x") + ")");
#endif
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("YM2608: Play 1bit ADPCM mode(" + (ym2608_pcm_start << 2).ToString("x") + " - " + ((ym2608_pcm_stop << 2) | 0x3).ToString("x") + ")");
                                                    }
                                                }
                                            }
                                            if (adrs != 0x8)    //ignore ADPCM adrs
                                            {
                                                if (comPortOPNA != null)
                                                {
                                                    deferredWriteOPNA_P1(adrs, dt);
                                                }
                                                else if (comPortOPN2 != null)
                                                {
                                                    //ignore test and unknown registers
                                                    if (adrs < 0x22 || adrs == 0x23 || adrs == 0x29 || (0x2c < adrs && adrs < 0x30))
                                                        break;
                                                    if (adrs > 0xb6)
                                                        break;

                                                    deferredWriteOPN2_P1(adrs, dt);
                                                }
                                            }
                                        }
                                        break;

                                    case 0x5A: //YM3812
                                    case 0x5B: //YM3526
                                        if (comPortOPL3 != null)
                                            goto case 0x5E;
                                        else if (comPortY8950 != null)
                                            goto case 0x5C;
                                        else
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;
                                        }
                                        break;

                                    case 0x5C: //Y8950
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;

                                            if (adrs == 0x8)
                                            {
                                                y8950_adpcmbit64k = (dt & 2) == 2;
                                                if ((dt & 1) == 1)
                                                {
                                                    //HACK: ROM -> RAM
                                                    if (comPortY8950?.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                                                    {
                                                        dt -= 1;
#if DEBUG

                                                        if (y8950_adpcmbit64k)
                                                            y8950_adpcmbit64kString = "64Kbit Fake RAM mode";
                                                        else
                                                            y8950_adpcmbit64kString = "256Kbit Fake RAM mode";
#endif
                                                    }
                                                    else
                                                    {
#if DEBUG
                                                        if ((dt & 2) == 2)
                                                            y8950_adpcmbit64kString = "64Kbit ROM mode";
                                                        else
                                                            y8950_adpcmbit64kString = "256Kbit ROM mode";
#endif
                                                    }
                                                    y8950_adpcm_ram = false;
                                                }
                                                else
                                                {
#if DEBUG
                                                    if (y8950_adpcmbit64k)
                                                        y8950_adpcmbit64kString = "64Kbit RAM mode";
                                                    else
                                                        y8950_adpcmbit64kString = "256Kbit RAM mode";
#endif
                                                    y8950_adpcm_ram = true;
                                                }
#if DEBUG
                                                Console.WriteLine("Y8950: Set " + y8950_adpcmbit64kString);
#endif
                                            }

                                            if (adrs >= 0 && adrs <= 0x10)
                                            {
                                                if (adrs == 0x9)
                                                {
                                                    y8950_pcm_start = (y8950_pcm_start & 0xff00) | dt;
                                                    if (!y8950_adpcm_ram && comPortY8950?.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                                                    {
                                                        var dt2 = ((y8950_pcm_start << 3) & 0xff00) >> 8;
                                                        writePcmAddressData(dt2, 0xa);

                                                        dt = (y8950_pcm_start << 3) & 0xff;
                                                    }
                                                }
                                                else if (adrs == 0xa)
                                                {
                                                    y8950_pcm_start = (y8950_pcm_start & 0xff) | (dt << 8);
                                                    if (!y8950_adpcm_ram && comPortY8950?.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                                                    {
                                                        dt = ((y8950_pcm_start << 3) & 0xff00) >> 8;
                                                    }
                                                }
                                                else if (adrs == 0xb)
                                                {
                                                    y8950_pcm_stop = (y8950_pcm_stop & 0xff00) | dt;
                                                    if (!y8950_adpcm_ram && comPortY8950?.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                                                    {
                                                        var dt2 = (((y8950_pcm_stop << 3) | 0b111) & 0xff00) >> 8;
                                                        writePcmAddressData(dt2, 0xc);

                                                        dt = ((y8950_pcm_stop << 3) | 0b111) & 0xff;
                                                    }
                                                }
                                                else if (adrs == 0xc)
                                                {
                                                    y8950_pcm_stop = (y8950_pcm_stop & 0xff) | (dt << 8);
                                                    if (!y8950_adpcm_ram && comPortY8950?.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                                                    {
                                                        dt = (((y8950_pcm_stop << 3) | 0b111) & 0xff00) >> 8;
                                                    }
#if DEBUG
                                                    Console.WriteLine("Y8950: Play " + y8950_adpcmbit64kString + "(" + (y8950_pcm_start << 5).ToString("x") + " - " + ((y8950_pcm_stop << 5) | 0x1f).ToString("x") + ")");
#endif
                                                }
                                            }
                                            if (adrs != 0xf)   //ignore ADPCM adrs
                                            {
                                                if (comPortY8950 != null)
                                                {
                                                    deferredWriteY8950(adrs, dt);
                                                }
                                                else if (comPortOPL3 != null)
                                                {
                                                    deferredWriteOPL3_P0(adrs, dt);
                                                }
                                            }
                                        }
                                        break;

                                    case 0x5D: //YMZ280B
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;
                                        }
                                        break;

                                    case 0x5E: //YMF262
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;

                                            if (comPortOPL3 != null)
                                            {
                                                deferredWriteOPL3_P0(adrs, dt);
                                            }
                                        }
                                        break;

                                    case 0x5F: //YMF262
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;

                                            if (comPortOPL3 != null)
                                            {
                                                deferredWriteOPL3_P1(adrs, dt);
                                            }
                                        }
                                        break;


                                    case 0x61: //Wait N samples
                                        {
                                            ushort time = vgmReader.ReadUInt16();
                                            vgmWaitDelta += time;
                                        }
                                        break;

                                    case 0x62: //Wait 735 samples
                                        vgmWaitDelta += 735;
                                        break;

                                    case 0x63: //Wait 882 samples
                                        vgmWaitDelta += 882;
                                        break;

                                    case 0x66:
                                        //End of song
                                        flushDeferredWriteData();
                                        if (!LoopByCount && !LoopByElapsed)
                                        {
                                            vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                            break;
                                        }
                                        else
                                        {
                                            if (vgmHead.lngLoopOffset != 0 && vgmDataOffset < vgmHead.lngLoopOffset)
                                                vgmReader.BaseStream?.Seek((vgmHead.lngLoopOffset + 0x1c) - (vgmDataOffset), SeekOrigin.Begin);
                                            else
                                                vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                        }
                                        break;

                                    case 0x67: //Data Block
                                        {
                                            //0x66
                                            var data = readByte();
                                            //data type
                                            var dtype = readByte();
                                            //data size
                                            uint size = vgmReader.ReadUInt32();
                                            if (0 <= size && size <= Int32.MaxValue)
                                            {
                                                switch (dtype)
                                                {
                                                    case 0:
                                                        {
                                                            dacDataOffset.Add(dacData.Count);
                                                            dacDataLength.Add((int)size);
                                                            if (size == 0)
                                                                dacData.AddRange(new byte[] { 0 });
                                                            else
                                                                dacData.AddRange(vgmReader.ReadBytes((int)size));
                                                        }
                                                        break;
                                                    case 0x81:  //YM2608
                                                        {
                                                            uint romSize = vgmReader.ReadUInt32();
                                                            uint saddr = vgmReader.ReadUInt32();
                                                            size -= 8;

#if DEBUG
                                                            Console.WriteLine("YM2608: Transferring ADPCM(" +
                                                                (saddr).ToString("x") + " - " + ((saddr + size - 1)).ToString("x") +
                                                                " (" + size.ToString("x") + ")");
#endif

                                                            //FormProgress.RunDialog("Updating ADPCM data",
                                                            //    new Action<FormProgress>((f) =>
                                                            {
                                                                if (size > 0)
                                                                    sendAdpcmDataYM2608(vgmReader.ReadBytes((int)size), (int)saddr, null);
                                                            }
                                                            //));
#if DEBUG
                                                            Console.WriteLine("YM2608: Transferred ADPCM(" +
                                                                (saddr).ToString("x") + " - " + ((saddr + size - 1)).ToString("x") +
                                                                " (" + size.ToString("x") + ")");
#endif
                                                            flushDeferredWriteData();
                                                        }
                                                        break;
                                                    case 0x88:  //YM8950
                                                        {
                                                            uint romSize = vgmReader.ReadUInt32();
                                                            uint saddr = vgmReader.ReadUInt32();
                                                            size -= 8;

                                                            FormMain.TopForm.SetStatusText("Y8950: Transferring ADPCM(" +
                                                                (saddr).ToString("x") + " - " + ((saddr + size - 1)).ToString("x") +
                                                                " (" + size.ToString("x") + ")");

#if DEBUG
                                                            Console.WriteLine("Y8950: Transferring ADPCM(" +
                                                                (saddr).ToString("x") + " - " + ((saddr + size - 1)).ToString("x") +
                                                                " (" + size.ToString("x") + ")");
#endif
                                                            //FormProgress.RunDialog("Updating ADPCM data",
                                                            //    new Action<FormProgress>((f) =>
                                                            {
                                                                if (size > 0)
                                                                    sendAdpcmDataY8950(vgmReader.ReadBytes((int)size), (int)saddr, null);
                                                            }
                                                            //));

                                                            FormMain.TopForm.SetStatusText("Y8950: Transferred ADPCM(" +
                                                                (saddr).ToString("x") + " - " + ((saddr + size - 1)).ToString("x") +
                                                                " (" + size.ToString("x") + ")");

#if DEBUG
                                                            Console.WriteLine("Y8950: Transferred ADPCM(" +
                                                                (saddr).ToString("x") + " - " + ((saddr + size - 1)).ToString("x") +
                                                                " (" + size.ToString("x") + ")");
#endif
                                                            flushDeferredWriteData();
                                                        }
                                                        break;
                                                    default:
                                                        vgmReader.ReadBytes((int)size);
                                                        break;
                                                }
                                            }
                                            //_vgmReader.BaseStream.Position += size;
                                        }
                                        break;

                                    case 0x68: //PCM RAM Write
                                        {
                                            //compatibility command to
                                            readByte();
                                            //chip type
                                            int ct = readByte();
                                            //read offset in data block
                                            int rofset = readByte() | (readByte() << 8) | (readByte() << 16);
                                            //write offset in chip's ram (affected by chip's registers)
                                            int wofset = readByte() | (readByte() << 8) | (readByte() << 16);
                                            //size of data, in bytes
                                            int size = readByte() | (readByte() << 8) | (readByte() << 16);
                                            vgmReader.ReadBytes((int)size);
                                        }
                                        break;

                                    case int cmd when 0x70 <= cmd && cmd <= 0x7F:
                                        {
                                            var time = (cmd & 15) + 1;
                                            vgmWaitDelta += time;
                                        }
                                        break;

                                    case int cmd when 0x80 <= cmd && cmd <= 0x8F:
                                        {
                                            var time = (command & 15);
                                            vgmWaitDelta += time;
                                            //_chip.WritePort0(0x2A, _DACData[_DACOffset]);
                                            if (dacData != null && dacOffset < dacData.Count)
                                            {
                                                comPortOPN2?.DeferredWriteData(0, 0x04, (byte)0x2a, (int)Settings.Default.BitBangWaitOPNA2);
                                                comPortOPN2?.DeferredWriteData(0, 0x08, (byte)dacData[dacOffset], (int)Settings.Default.BitBangWaitOPNA2);
                                            }
                                            dacOffset++;
                                        }
                                        break;

                                    case 0x90: //Setup Stream Control:
                                        {
                                            //stream id
                                            var sid = readByte();
                                            //chip type
                                            var ct = readByte();
                                            //port
                                            var port = readByte();
                                            //command
                                            var cmd = readByte();
                                            switch (ct)
                                            {
                                                case 2:   //YM2612
                                                    if (port == 0x00 && cmd == 0x2a)    //PCM
                                                    {
                                                        comPortOPN2?.DeferredWriteData(0, 0x04, 0x2b, (int)Settings.Default.BitBangWaitOPNA2);
                                                        comPortOPN2?.DeferredWriteData(0, 0x08, 0x80, (int)Settings.Default.BitBangWaitOPNA2);
                                                    }
                                                    break;
                                            }
                                            if (!streamTable.ContainsKey(sid))
                                                streamTable.Add(sid, new StreamData(sid));
                                        }
                                        break;

                                    case 0x91: //Set Stream Data:
                                        {
                                            //stream id
                                            var sid = readByte();
                                            //data bank
                                            var dbank = readByte();
                                            //step size
                                            var ssz = readByte();
                                            //step base
                                            var sbase = readByte();

                                            if (streamTable.ContainsKey(sid))
                                            {
                                                streamTable[sid].StreamDataBanks[dbank].StepSize = ssz;
                                                streamTable[sid].StreamDataBanks[dbank].StepBase = sbase;
                                            }
                                        }
                                        break;

                                    case 0x92: //Set Stream Frequency:
                                        {
                                            //stream id
                                            var sid = readByte();
                                            //sample rate
                                            uint sfreq = vgmReader.ReadUInt32();

                                            if (streamTable.ContainsKey(sid))
                                                streamTable[sid].Frequency = sfreq;
                                        }
                                        break;

                                    case 0x93: //Start Stream:
                                        {
                                            //stream id
                                            var sid = readByte();
                                            uint ofst = vgmReader.ReadUInt32();
                                            var lenMode = readByte();
                                            uint dataLen = vgmReader.ReadUInt32();
                                            if (lenMode == 1)
                                            {
                                                StreamParam param = new StreamParam();
                                                param.StreamID = sid;
                                                param.BlockID = 0;
                                                param.Offset = (int)ofst;
                                                param.Length = (int)dataLen;
                                                if ((lenMode & 0x80) != 0)
                                                    param.Mode |= StreamModes.Loop;
                                                else if ((lenMode & 0x10) != 0)
                                                    param.Mode |= StreamModes.Reverse;
                                                streamParam = param;
                                            }
                                        }
                                        break;

                                    case 0x94:  //Stop Stream
                                        {
                                            //stream id
                                            var sid = readByte();
                                            streamParam = null;
                                        }
                                        break;

                                    case 0x95: //Start Stream (fast call):
                                        {
                                            //stream id
                                            var sid = readByte();
                                            //block id
                                            var bid = readByte();
                                            //flags
                                            var flgs = readByte();

                                            StreamParam param = new StreamParam();
                                            param.StreamID = sid;
                                            param.BlockID = bid;
                                            param.Offset = dacDataOffset[bid];
                                            param.Length = dacDataLength[bid];
                                            if ((flgs & 0x01) != 0)
                                                param.Mode |= StreamModes.Loop;
                                            else if ((flgs & 0x10) != 0)
                                                param.Mode |= StreamModes.Reverse;

                                            streamParam = param;
                                        }
                                        break;

                                    case 0xA0:  //Y8910
                                        {
                                            var aa = readByte();
                                            if (aa < 0)
                                                break;
                                            var dd = readByte();
                                            if (dd < 0)
                                                break;
                                            if (comPortY8910 != null)
                                                deferredWritePSG(aa, dd);
                                        }
                                        break;

                                    case int cmd when 0xB0 <= cmd && cmd <= 0xBF:
                                        {
                                            var adrs = readByte();
                                            if (adrs < 0)
                                                break;
                                            var dt = readByte();
                                            if (dt < 0)
                                                break;
                                        }
                                        break;

                                    case int cmd when 0xC0 <= cmd && cmd <= 0xD1:
                                        {
                                            var pp = readByte();
                                            if (pp < 0)
                                                break;
                                            var aa = readByte();
                                            if (aa < 0)
                                                break;
                                            var dd = readByte();
                                            if (dd < 0)
                                                break;
                                        }
                                        break;

                                    case 0xD2:  //SCC1
                                        {
                                            var pp = readByte();
                                            if (pp < 0)
                                                break;
                                            var aa = readByte();
                                            if (aa < 0)
                                                break;
                                            var dd = readByte();
                                            if (dd < 0)
                                                break;

                                            switch (comPortSCC?.SoundModuleType)
                                            {
                                                case VsifSoundModuleType.MSX_FTDI:
                                                    SCCType type = (SCCType)comPortSCC.Tag["SCC.Type"];
                                                    var slot = (int)comPortSCC.Tag["SCC.Slot"];
                                                    if ((int)slot < 0)
                                                        //自動選択方式
                                                        comPortSCC.DeferredWriteData(3, (byte)type,
                                                            (byte)(-((int)slot + 1)), (int)Settings.Default.BitBangWaitSCC);
                                                    else
                                                        //従来方式
                                                        comPortSCC.DeferredWriteData(3, (byte)(type + 4),
                                                            (byte)slot, (int)Settings.Default.BitBangWaitSCC);

                                                    switch (type)
                                                    {
                                                        case SCCType.SCC1:
                                                            switch (pp)
                                                            {
                                                                case 1: //Freq
                                                                    aa += 0xa0;
                                                                    break;
                                                                case 2: //Vol
                                                                    aa += 0xaa;
                                                                    break;
                                                                case 3: //Ena
                                                                    aa = 0xaf;
                                                                    break;
                                                                case 5: //
                                                                    aa += 0xe0;
                                                                    break;
                                                                default:
                                                                    break;
                                                            }

                                                            comPortSCC.DeferredWriteData(4, (byte)aa, (byte)dd, (int)Settings.Default.BitBangWaitSCC);
                                                            break;
                                                        case SCCType.SCC1_Compat:
                                                        case SCCType.SCC:
                                                            switch (pp)
                                                            {
                                                                case 1: //Freq
                                                                    //Console.WriteLine("Freq " + aa / 2 + "ch: " + dd);
                                                                    aa += 0x80;
                                                                    break;
                                                                case 2: //Vol
                                                                    //Console.WriteLine("Vol  " + aa / 2 + "ch: " + dd);
                                                                    aa += 0x8a;
                                                                    break;
                                                                case 3: //Ena
                                                                    //Console.WriteLine("En   " + Convert.ToString((int)dd, 2));
                                                                    aa = 0x8f;
                                                                    break;
                                                                case 5: //
                                                                    //Console.WriteLine("Mode " + aa / 2 + "ch: " + dd);
                                                                    aa += 0xe0;
                                                                    break;
                                                                default:
                                                                    break;
                                                            }

                                                            comPortSCC.DeferredWriteData(5, (byte)aa, (byte)dd, (int)Settings.Default.BitBangWaitSCC);
                                                            break;
                                                    }
                                                    break;
                                            }
                                        }
                                        break;

                                    case int cmd when 0xD3 <= cmd && cmd <= 0xD6:
                                        {
                                            var pp = readByte();
                                            if (pp < 0)
                                                break;
                                            var aa = readByte();
                                            if (aa < 0)
                                                break;
                                            var dd = readByte();
                                            if (dd < 0)
                                                break;
                                        }
                                        break;

                                    case 0xE0: //Seek to offset in PCM databank
                                        uint offset = vgmReader.ReadUInt32();
                                        dacOffset = (int)offset;
                                        break;

                                    case 0xE1:
                                        {
                                            var mm = readByte();
                                            if (mm < 0)
                                                break;
                                            var ll = readByte();
                                            if (ll < 0)
                                                break;
                                            var aa = readByte();
                                            if (aa < 0)
                                                break;
                                            var dd = readByte();
                                            if (dd < 0)
                                                break;
                                        }
                                        break;

                                    default:
                                        break;
                                }

                                //if (_wait != 0)
                                //    _wait -= 1;

                            }
                            if ((command == 0x66 || command == -1))
                            {
                                flushDeferredWriteData();
                                if ((!LoopByCount || (LoopByCount && CurrentLoopedCount >= 0 && CurrentLoopedCount >= LoopedCount))
                                    && !LoopByElapsed)
                                {
                                    break;
                                }
                                if (CurrentLoopedCount >= 0)
                                    CurrentLoopedCount++;
                            }
                        }

                        if (currentStreamParam != streamParam)
                        {
                            currentStreamParam = streamParam;
                            if (currentStreamParam == null)
                            {
                                streaming = false;
                            }
                            else
                            {
                                if ((currentStreamParam.Mode & StreamModes.Reverse) != StreamModes.Reverse)
                                {
                                    currentStreamIdx = currentStreamParam.Offset;
                                    currentStreamIdxDir = 1;
                                }
                                else
                                {
                                    currentStreamIdx = currentStreamParam.Offset + currentStreamParam.Length - 1;
                                    currentStreamIdxDir = -1;
                                }
                                currentStreamData = streamTable[currentStreamParam.StreamID];
                                streamWaitDelta = 0;
                                streaming = true;
                            }
                        }
                        if (streaming)
                        {
                            if (streamWaitDelta <= 0)
                            {
                                if (currentStreamIdxDir > 0)
                                {
                                    if (currentStreamIdx >= currentStreamParam.Offset + currentStreamParam.Length)
                                    {
                                        if ((currentStreamParam.Mode & StreamModes.Loop) != StreamModes.Loop)
                                            streaming = false;
                                        else
                                            currentStreamIdx = currentStreamParam.Offset;
                                    }
                                }
                                else
                                {
                                    if (currentStreamIdx < currentStreamParam.Offset)
                                    {
                                        if ((currentStreamParam.Mode & StreamModes.Loop) != StreamModes.Loop)
                                            streaming = false;
                                        else
                                            currentStreamIdx = currentStreamParam.Offset + currentStreamParam.Length - 1;
                                    }
                                }
                                if (streaming)
                                {
                                    byte data = dacData[currentStreamIdx];
                                    currentStreamIdx += currentStreamIdxDir;

                                    comPortOPN2?.DeferredWriteData(0, 0x04, (byte)0x2a, (int)Settings.Default.BitBangWaitOPNA2);
                                    comPortOPN2?.DeferredWriteData(0, 0x08, data, (int)Settings.Default.BitBangWaitOPNA2);

                                    streamWaitDelta += 44.1 * 1000 / currentStreamData.Frequency;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(Exception))
                            throw;
                        else if (ex.GetType() == typeof(SystemException))
                            throw;

                        flushDeferredWriteData();
                        if ((!LoopByCount || (LoopByCount && CurrentLoopedCount >= 0 && CurrentLoopedCount >= LoopedCount))
                            && !LoopByElapsed)
                        {
                            break;
                        }
                        if (CurrentLoopedCount >= 0)
                            CurrentLoopedCount++;
                        vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                    }

                    if (streamWaitDelta < vgmWaitDelta)
                    {
                        if (streamWaitDelta <= 0)
                        {
                            wait += vgmWaitDelta;
                            vgmWaitDelta = 0;
                        }
                        else
                        {
                            wait += streamWaitDelta;
                            vgmWaitDelta -= streamWaitDelta;
                            streamWaitDelta = 0;
                        }
                    }
                    else
                    {
                        if (vgmWaitDelta <= 0)
                        {
                            wait += streamWaitDelta;
                            streamWaitDelta = 0;
                        }
                        else
                        {
                            wait += vgmWaitDelta;
                            streamWaitDelta -= vgmWaitDelta;
                            vgmWaitDelta = 0;
                        }
                    }

                    if (wait <= (double)Settings.Default.VGMWait)
                        continue;

                    //while (!IsDeferredDataFlushed()) ;

                    flushDeferredWriteData();

                    QueryPerformanceCounter(out after);

#if DEBUG
                    //Console.WriteLine(vgmDataCurrentOffset.ToString("X"));
#endif

                    double pwait = (wait / PlaybackSpeed);
                    if (((double)(after - before) / freq) > (pwait / (44.1 * 1000)))
                    {
                        lastDiff = ((double)(after - before) / freq) - (pwait / (44.1 * 1000));
                        wait = -(lastDiff * 44.1 * 1000);
                        //wait = 0;
                        NotifyProcessLoadOccurred();
                    }
                    else
                    {
                        while (((double)(after - before) / freq) <= (pwait / (44.1 * 1000)))
                            QueryPerformanceCounter(out after);
                        wait = 0;
                        HighLoad = false;
                    }
                }
            }

            StopAllSounds(true);
            State = SoundState.Stopped;
            NotifyFinished();
        }

        private void deferredWritePSG(int adrs, int dt)
        {
            switch (comPortY8910.SoundModuleType)
            {
                case VsifSoundModuleType.MSX_FTDI:
                    comPortY8910.DeferredWriteData(0, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitAY8910);
                    break;
                case VsifSoundModuleType.Generic_UART:
                    comPortY8910.DeferredWriteData(0, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitAY8910);
                    break;
            }
        }

        private void deferredWriteOPM(int adrs, int dt)
        {
            if (comPortOPM.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                comPortOPM.DeferredWriteData(0xe, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPM);
            }
        }

        private void deferredWriteOPLL(int adrs, int dt)
        {
            switch (comPortOPLL.SoundModuleType)
            {
                case VsifSoundModuleType.MSX_FTDI:
                    var slot = (int)comPortOPLL.Tag["OPLL.Slot"];
                    if (slot == 1 || slot == 2)
                        comPortOPLL.DeferredWriteData(2, (byte)0, (byte)(slot - 1), (int)Settings.Default.BitBangWaitOPLL);
                    if ((int)comPortOPLL.Tag["OPLL.Slot"] == 0)
                        comPortOPLL.DeferredWriteData(1, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPLL);
                    else
                        comPortOPLL.DeferredWriteData(0xC, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPLL);
                    break;
                case VsifSoundModuleType.SMS:
                    comPortOPLL.DeferredWriteData(0, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPLL);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferData"></param>
        /// <param name="saddr"></param>
        /// <param name="fp"></param>
        private void sendAdpcmDataYM2608(byte[] transferData, int saddr, FormProgress fp)
        {
            //File.WriteAllBytes(transferData.Length.ToString(), transferData);

            YM2608WriteData(0x00, 0, 3, 0x01, false);  //RESET

            //flag
            YM2608WriteData(0x10, 0, 3, 0x13, false);   //CLEAR MASK
            YM2608WriteData(0x10, 0, 3, 0x80, false);   //IRQ RESET
                                                        //Ctrl1
            YM2608WriteData(0x00, 0, 3, 0x60, false);   //REC, EXTMEM
            //Ctrl2
            //START
            if (ym2608_adpcmbit8)
            {
                YM2608WriteData(0x01, 0, 3, 0x02, false);   //LR, 8bit DRAM
                YM2608WriteData(0x02, 0, 3, (byte)((saddr >> 5) & 0xff), false);
                YM2608WriteData(0x03, 0, 3, (byte)((saddr >> (5 + 8)) & 0xff), false);
            }
            else
            {
                YM2608WriteData(0x01, 0, 3, 0x00, false);   //LR, 1bit DRAM
                YM2608WriteData(0x02, 0, 3, (byte)((saddr >> 2) & 0xff), false);
                YM2608WriteData(0x03, 0, 3, (byte)((saddr >> (2 + 8)) & 0xff), false);
            }
            //STOP
            YM2608WriteData(0x04, 0, 3, 0xff, false);
            YM2608WriteData(0x05, 0, 3, 0xff, false);
            //LIMIT
            YM2608WriteData(0x0C, 0, 3, 0xff, false);
            YM2608WriteData(0x0D, 0, 3, 0xff, false);

            //Transfer
            int len = transferData.Length;
            int index = 0;
            int percentage = 0;
            int lastPercentage = 0;
            for (int i = 0; i < len; i++)
            {
                YM2608WriteData(0x08, 0, 3, transferData[i], true);

                percentage = (100 * index) / len;
                if (percentage != lastPercentage)
                {
                    FormMain.TopForm.SetStatusText("YM2608: Transferring ADPCM(" + percentage + "%)");
                    //fp.Percentage = percentage;
                    comPortOPNA?.FlushDeferredWriteDataAndWait();
                }
                lastPercentage = percentage;
                index++;
                if (RequestedStat == SoundState.Stopped)
                    break;
                else updateStatusForDataTransfer();
            }
            FormMain.TopForm.SetStatusText("YM2608: Transferred ADPCM");

            // Finish
            YM2608WriteData(0x10, 0, 3, 0x80, false);
            YM2608WriteData(0x00, 0, 3, 0x01, false);  //RESET
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferData"></param>
        /// <param name="saddr"></param>
        /// <param name="fp"></param>
        private void sendAdpcmDataY8950(byte[] transferData, int saddr, FormProgress fp)
        {
            if (transferData.Length == 0)
                return;

            //File.WriteAllBytes(transferData.Length.ToString(), transferData);
            int slot = 0;
            if (comPortY8950 != null)
            {
                switch ((int)comPortY8950.Tag["Y8950.Slot"])
                {
                    case 0:
                        slot = 0;
                        break;
                    case 1:
                        slot = 9;
                        break;
                }
            }

            //http://ngs.no.coocan.jp/doc/wiki.cgi/datapack?page=4%2E5+Y8950%28MSX%2DAUDIO%29

            //$07レジスタリセット
            YMF262WriteData(comPortY8950, 0x07, 0, slot, 0, 0, (byte)0x01, false);

            //各フラグをイネーブルにする。
            YMF262WriteData(comPortY8950, 0x04, 0, slot, 0, 0, 0x00, false);
            //各フラグをリセット。
            YMF262WriteData(comPortY8950, 0x04, 0, slot, 0, 0, 0x80, false);

            //メモリライトモードにする。
            YMF262WriteData(comPortY8950, 0x07, 0, slot, 0, 0, 0x60, false);

            if (comPortY8950 != null)
            {
                //switch ((int)comPortY8950.Tag["Y8950.Slot"])
                //{
                //    case 0:
                //    case 2:
                //        //RAMタイプの指定。64Kbit
                //        YMF262WriteData(comPortY8950, 0x08, 0, slot, 0, 0, 0x02);

                //        if ((saddr & 0b1000) == 0b1000)
                //            saddr += 0b1000;
                //        if ((saddr & 0b1_0000_0000_0000) == 0b1_0000_0000_0000)
                //            saddr += 0b1_0000_0000_0000;

                //        //START
                //        YMF262WriteData(comPortY8950, 0x09, 0, slot, 0, 0, (byte)((saddr >> 5) & 0xff));
                //        YMF262WriteData(comPortY8950, 0x0a, 0, slot, 0, 0, (byte)((saddr >> (5 + 8)) & 0xff));
                //        //STOP
                //        YMF262WriteData(comPortY8950, 0x0b, 0, slot, 0, 0, (byte)(0xff - 0b1000));
                //        YMF262WriteData(comPortY8950, 0x0c, 0, slot, 0, 0, (byte)(0xff - 0b10000));
                //        break;
                //    case 1:
                //    case 3:
                //        //RAMタイプの指定。256Kbit
                //        YMF262WriteData(comPortY8950, 0x08, 0, slot, 0, 0, 0x00);
                //        //START
                //        YMF262WriteData(comPortY8950, 0x09, 0, slot, 0, 0, (byte)((saddr >> 5) & 0xff));
                //        YMF262WriteData(comPortY8950, 0x0a, 0, slot, 0, 0, (byte)((saddr >> (5 + 8)) & 0xff));
                //        //STOP
                //        YMF262WriteData(comPortY8950, 0x0b, 0, slot, 0, 0, (byte)0xff);
                //        YMF262WriteData(comPortY8950, 0x0c, 0, slot, 0, 0, (byte)0xff);
                //        break;
                //}
                int eaddr = saddr + transferData.Length - 1;
                saddr = saddr >> 2;
                eaddr = eaddr >> 2;

                if (y8950_adpcmbit64k)
                {
                    //メモリのタイプ指定。64Kbit
                    YMF262WriteData(comPortY8950, 0x08, 0, slot, 0, 0, 0x02, false);

                    if ((saddr & 0b1000) == 0b1000)
                        saddr += 0b1000;
                    if ((saddr & 0b1_0000_0000_0000) == 0b1_0000_0000_0000)
                        saddr += 0b1_0000_0000_0000;
                    if ((eaddr & 0b1000) == 0b1000)
                        eaddr += 0b1000;
                    if ((eaddr & 0b1_0000_0000_0000) == 0b1_0000_0000_0000)
                        eaddr += 0b1_0000_0000_0000;

                    //START
                    YMF262WriteData(comPortY8950, 0x09, 0, slot, 0, 0, (byte)(saddr & 0xff), false);
                    YMF262WriteData(comPortY8950, 0x0a, 0, slot, 0, 0, (byte)((saddr >> 8) & 0xff), false);
                    //STOP
                    YMF262WriteData(comPortY8950, 0x0b, 0, slot, 0, 0, (byte)(eaddr & 0xff), false);
                    YMF262WriteData(comPortY8950, 0x0c, 0, slot, 0, 0, (byte)((eaddr >> 8) & 0xff), false);
                }
                else
                {
                    //メモリのタイプ指定。256Kbit
                    YMF262WriteData(comPortY8950, 0x08, 0, slot, 0, 0, 0x00, false);
                    //START
                    YMF262WriteData(comPortY8950, 0x09, 0, slot, 0, 0, (byte)(saddr & 0xff), false);
                    YMF262WriteData(comPortY8950, 0x0a, 0, slot, 0, 0, (byte)((saddr >> 8) & 0xff), false);
                    //STOP
                    YMF262WriteData(comPortY8950, 0x0b, 0, slot, 0, 0, (byte)(eaddr & 0xff), false);
                    YMF262WriteData(comPortY8950, 0x0c, 0, slot, 0, 0, (byte)((eaddr >> 8) & 0xff), false);
                }
            }

            //Transfer
            int address = saddr;

            int len = transferData.Length;
            int index = 0;
            int percentage = 0;
            int lastPercentage = 0;
            for (int i = 0; i < transferData.Length; i++)
            {
                YMF262WriteData(comPortY8950, 0x0f, 0, slot, 0, 0, (byte)transferData[i], true);

                percentage = (100 * index) / len;
                if (percentage != lastPercentage)
                {
                    FormMain.TopForm.SetStatusText("Y8950: Transferring ADPCM(" + percentage + "%)");
                    //fp.Percentage = percentage;
                    comPortY8950?.FlushDeferredWriteDataAndWait();
                }
                lastPercentage = percentage;
                index++;

                address++;
                if (comPortY8950 != null)
                {
                    //switch ((int)comPortY8950.Tag["Y8950.Slot"])
                    //{
                    //    case 0:
                    //    case 2:
                    //        //RAMタイプの指定。64Kbit
                    //        if ((endAddress & 0b1000) == 0b1000)
                    //            endAddress += 0b1000;
                    //        if ((endAddress & 0b1_0000_0000_0000) == 0b1_0000_0000_0000)
                    //            endAddress += 0b1_0000_0000_0000;
                    //        break;
                    //    case 1:
                    //    case 3:
                    //        break;
                    //}
                    //if (y8950_adpcmbit64k)
                    //{
                    //    if ((endAddress & 0b1000) == 0b1000)
                    //        endAddress += 0b1000;
                    //    if ((endAddress & 0b1_0000_0000_0000) == 0b1_0000_0000_0000)
                    //        endAddress += 0b1_0000_0000_0000;
                    //}
                }
                if (RequestedStat == SoundState.Stopped)
                    break;
                else updateStatusForDataTransfer();
            }
            FormMain.TopForm.SetStatusText("Y8950: Transferred ADPCM");

            //○リセット
            YMF262WriteData(comPortY8950, 0x04, 0, slot, 0, 0, (byte)0x80, false);
            //$07レジスタリセット
            YMF262WriteData(comPortY8950, 0x07, 0, slot, 0, 0, (byte)0x01, false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void updateStatusForDataTransfer()
        {
            if (RequestedStat == SoundState.Paused)
            {
                if (State != SoundState.Paused)
                    State = SoundState.Paused;
            }
            else if (RequestedStat == SoundState.Freezed)
            {
                if (State != SoundState.Freezed)
                    State = SoundState.Freezed;
            }
            else if (RequestedStat == SoundState.Playing)
            {
                if (State != SoundState.Playing)
                    State = SoundState.Freezed;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        private void deferredWriteY8950(int adrs, int dt)
        {
            if (comPortY8950.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                int ytype = (int)comPortY8950.Tag["Y8950.Slot"];
                switch (ytype)
                {
                    case 0:
                        comPortY8950.DeferredWriteData(10, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitY8950);
                        break;
                    case 1:
                        comPortY8950.DeferredWriteData(11, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitY8950);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        private void deferredWriteOPL3_P0(int adrs, int dt)
        {
            if (comPortOPL3.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                if (adrs != 0x10)   //ignore for Y8950 simulation
                    comPortOPL3.DeferredWriteData(10, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPL3);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        private void deferredWriteOPL3_P1(int adrs, int dt)
        {
            if (comPortOPL3.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                comPortOPL3.DeferredWriteData(11, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPL3);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        private void deferredWriteOPN2_P0(int adrs, int dt)
        {
            if (comPortOPN2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                comPortOPN2.DeferredWriteData(0x10, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPNA2);
            }
            else //Genesis
            {
                comPortOPN2.DeferredWriteData(0, 0x04, (byte)adrs, (int)Settings.Default.BitBangWaitOPNA2);
                comPortOPN2.DeferredWriteData(0, 0x08, (byte)dt, (int)Settings.Default.BitBangWaitOPNA2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        private void deferredWriteOPN2_P1(int adrs, int dt)
        {
            if (comPortOPN2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                comPortOPN2.DeferredWriteData(0x11, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPNA2);
            }
            else
            {
                comPortOPN2.DeferredWriteData(0, 0x0C, (byte)adrs, (int)Settings.Default.BitBangWaitOPNA2);
                comPortOPN2.DeferredWriteData(0, 0x10, (byte)dt, (int)Settings.Default.BitBangWaitOPNA2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        private void deferredWriteOPNA_P0(int adrs, int dt)
        {
            if (comPortOPNA.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                comPortOPNA.DeferredWriteData(0x10, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPNA);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        private void deferredWriteOPNA_P1(int adrs, int dt)
        {
            if (comPortOPNA.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
            {
                comPortOPNA.DeferredWriteData(0x11, (byte)adrs, (byte)dt, (int)Settings.Default.BitBangWaitOPNA);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt2"></param>
        /// <param name="adrs2"></param>
        private void writePcmAddressData(int dt2, int adrs2)
        {
            if (comPortY8950 != null)
            {
                if (comPortY8950.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                {
                    int ytype = (int)comPortY8950.Tag["Y8950.Slot"];
                    switch (ytype)
                    {
                        case 0:
                            comPortY8950.DeferredWriteData(10, (byte)adrs2, (byte)dt2, (int)Settings.Default.BitBangWaitY8950);
                            break;
                        case 1:
                            comPortY8950.DeferredWriteData(11, (byte)adrs2, (byte)dt2, (int)Settings.Default.BitBangWaitY8950);
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void flushDeferredWriteData()
        {
            comPortDCSG?.FlushDeferredWriteData();
            comPortOPLL?.FlushDeferredWriteData();
            comPortOPN2?.FlushDeferredWriteData();
            comPortSCC?.FlushDeferredWriteData();
            comPortY8910?.FlushDeferredWriteData();
            comPortOPM?.FlushDeferredWriteData();
            comPortOPL3?.FlushDeferredWriteData();
            comPortOPNA?.FlushDeferredWriteData();
            comPortY8950?.FlushDeferredWriteData();
        }

        /// <summary>
        /// 
        /// </summary>
        private void flushDeferredWriteDataAndWait()
        {
            comPortDCSG?.FlushDeferredWriteDataAndWait();
            comPortOPLL?.FlushDeferredWriteDataAndWait();
            comPortOPN2?.FlushDeferredWriteDataAndWait();
            comPortSCC?.FlushDeferredWriteDataAndWait();
            comPortY8910?.FlushDeferredWriteDataAndWait();
            comPortOPM?.FlushDeferredWriteDataAndWait();
            comPortOPL3?.FlushDeferredWriteDataAndWait();
            comPortOPNA?.FlushDeferredWriteDataAndWait();
            comPortY8950?.FlushDeferredWriteDataAndWait();
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsDeferredDataFlushed()
        {
            if (comPortDCSG != null && !comPortDCSG.DeferredDataFlushed)
                return false;
            if (comPortOPLL != null && !comPortOPLL.DeferredDataFlushed)
                return false;
            if (comPortOPN2 != null && !comPortOPN2.DeferredDataFlushed)
                return false;
            if (comPortSCC != null && !comPortSCC.DeferredDataFlushed)
                return false;
            if (comPortY8910 != null && !comPortY8910.DeferredDataFlushed)
                return false;
            if (comPortOPM != null && !comPortOPM.DeferredDataFlushed)
                return false;
            if (comPortOPL3 != null && !comPortOPL3.DeferredDataFlushed)
                return false;
            if (comPortOPNA != null && !comPortOPNA.DeferredDataFlushed)
                return false;
            if (comPortY8950 != null && !comPortY8950.DeferredDataFlushed)
                return false;

            return true;
        }


        private const int WAIT_TIMEOUT = 120 * 1000;


        [DllImport("kernel32.dll")]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle hTimer,
            [In] ref long pDueTime, int lPeriod,
            IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32.dll")]
        internal static extern uint WaitForSingleObject(SafeWaitHandle hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern void GetSystemTimeAsFileTime(out long lpSystemTimeAsFileTime);

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // マネージド状態を破棄します (マネージド オブジェクト)
                    Stop();
                }

                vgmReader?.Dispose();

                // アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                comPortDCSG?.Dispose();
                comPortDCSG = null;
                comPortOPLL?.Dispose();
                comPortOPLL = null;
                comPortOPN2?.Dispose();
                comPortOPN2 = null;
                comPortSCC?.Dispose();
                comPortSCC = null;
                comPortY8910?.Dispose();
                comPortY8910 = null;
                comPortOPM?.Dispose();
                comPortOPM = null;
                comPortOPL3?.Dispose();
                comPortOPL3 = null;
                comPortOPNA?.Dispose();
                comPortOPNA = null;
                comPortY8950?.Dispose();
                comPortY8950 = null;

                // 大きなフィールドを null に設定します
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

        private class StreamParam
        {
            public int StreamID;
            public int BlockID;
            public int Offset;
            public int Length;
            public StreamModes Mode;
        }

        [Flags]
        private enum StreamModes
        {
            Loop = 0x01,
            Reverse = 0x02,
        }

        // 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~VGMSong()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: false);
        }

        public override void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class StreamData
    {
        public int StreamID
        {
            get;
            private set;
        }

        public uint Frequency
        {
            get;
            set;
        }

        public StreamDataBank[] StreamDataBanks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamID"></param>
        public StreamData(int streamID)
        {
            StreamID = streamID;

            StreamDataBanks = new StreamDataBank[0x40];
            for (int i = 0; i < StreamDataBanks.Length; i++)
                StreamDataBanks[i] = new StreamDataBank();
        }
    }

    public class StreamDataBank
    {
        public int DataBankID
        {
            get;
            set;
        }

        public int StepSize
        {
            get;
            set;
        }

        public int StepBase
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public StreamDataBank()
        {
        }
    }

#pragma warning disable 0649, 0169

    /// <summary>
    /// 
    /// </summary>
    public class VGM_HEADER
    {
        public uint lngEOFOffset;
        public uint lngVersion;
        public uint lngHzDCSG;
        public uint lngHzYM2413;
        public uint lngGD3Offset;
        public uint lngTotalSamples;
        public uint lngLoopOffset;
        public uint lngLoopSamples;
        public uint lngRate;
        public ushort shtPSG_Feedback;
        public byte bytPSG_SRWidth;
        public byte bytPSG_Flags;
        public uint lngHzYM2612;
        public uint lngHzYM2151;
        public uint lngDataOffset;
        public uint lngHzSPCM;
        public uint lngSPCMIntf;
        public uint lngHzRF5C68;
        public uint lngHzYM2203;
        public uint lngHzYM2608;
        public uint lngHzYM2610;
        public uint lngHzYM3812;
        public uint lngHzYM3526;
        public uint lngHzY8950;
        public uint lngHzYMF262;
        public uint lngHzYMF278B;
        public uint lngHzYMF271;
        public uint lngHzYMZ280B;
        public uint lngHzRF5C164;
        public uint lngHzPWM;
        public uint lngHzAY8910;
        public byte bytAYType;
        public byte bytAYFlag;
        public byte bytAYFlagYM2203;
        public byte bytAYFlagYM2608;
        public byte bytVolumeModifier;
        public byte bytReserved2;
        public sbyte bytLoopBase;
        public byte bytLoopModifier;
        public uint lngHzGBDMG;
        public uint lngHzNESAPU;
        public uint lngHzMultiPCM;
        public uint lngHzUPD7759;
        public uint lngHzOKIM6258;
        public byte bytOKI6258Flags;
        public byte bytK054539Flags;
        public byte bytC140Type;
        public byte bytReservedFlags;
        public uint lngHzOKIM6295;
        public uint lngHzK051649;
        public uint lngHzK054539;
        public uint lngHzHuC6280;
        public uint lngHzC140;
        public uint lngHzK053260;
        public uint lngHzPokey;
        public uint lngHzQSound;
        public uint lngHzSCSP;
        public uint lngExtraOffset;
        public uint lngHzWSwan;
        public uint lngHzVSU;
        public uint lngHzSAA1099;
        public uint lngHzES5503;
        public uint lngHzES5506;
        public ushort shtESchns;
        public byte bytCD;
        public byte bytReservedFlags2;
        public uint lngHzX1_010;
        public uint lngHzC352;
        public uint lngHzGA20;
    }
#pragma warning restore 0649, 0169

}
