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

//Sega Genesis VGM player. Player written and emulators ported by Landon Podbielski. 
namespace zanac.VGMPlayer
{
    public class XGMSong : SongBase
    {
        private VsifClient comPortDCSG;

        private VsifClient comPortOPN2;
        private VsifClient comPortOPNA;

        private const uint FCC_VGM = 0x204D4758;    // 'XGM '

        private XGM_HEADER xgmMHead;

        private BinaryReader xgmReader;

        private byte[] dacData;

        private byte[] vgmData;

        private SampleData[] SampleDataTable = new SampleData[63];

        private class SampleData
        {
            public int Address
            {
                get;
                private set;
            }

            public int Size
            {
                get;
                private set;
            }

            private XGMSong xgmData;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="adress"></param>
            /// <param name="size"></param>
            public SampleData(XGMSong xgmData, uint adress, uint size)
            {
                Address = (int)adress;
                Size = (int)size;
                this.xgmData = xgmData;
            }

            private int index;

            public void Restart()
            {
                index = 0;
            }

            public sbyte? GetDacData()
            {
                if (index >= Size)
                    return null;

                sbyte ret = (sbyte)xgmData.dacData[Address + index];
                index++;

                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public XGMSong(string fileName) : base(fileName)
        {
            openVGMFile(fileName);
        }

        protected override void StopAllSounds(bool volumeOff)
        {
            if (comPortDCSG != null)
            {
                comPortDCSG.ClearDeferredWriteData();

                switch (comPortDCSG?.SoundModuleType)
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
                            comPortDCSG.DeferredWriteData(0, 0xFF, (byte)(0x80 | i << 5 | 0x1f), (int)Settings.Default.BitBangWaitOPNA2);
                        comPortDCSG.DeferredWriteData(0, 0xFF, (byte)(0x80 | 3 << 5 | 0x1f), (int)Settings.Default.BitBangWaitOPNA2);
                        break;
                    case VsifSoundModuleType.MSX_FTDI:
                        for (int i = 0; i < 3; i++)
                            comPortDCSG.DeferredWriteData(0xF, 0, (byte)(0x80 | i << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        comPortDCSG.DeferredWriteData(0xF, 0, (byte)(0x80 | 3 << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        break;
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
                    for (int i = 0x40; i <= 0x8F; i++)
                        deferredWriteOPN2_P0(i, 0xFF);
                    for (int i = 0x40; i <= 0x8F; i++)
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

            flushDeferredWriteData();
            Thread.Sleep(250);
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

        private void deferredWriteOPNA_P0(int adrs, int dt, uint dclk)
        {
            comPortOPNA.RegTable[adrs] = dt;

            switch (adrs)
            {
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                    if (!ConvertChipClock ||  (double)comPortOPNA.ChipClockHz["OPNA"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertOpnFrequency(comPortOPNA.RegTable[adrs + 4], dt, comPortOPNA.ChipClockHz["OPNA"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPNA_P0(adrs + 4, ret.Hi);
                        deferredWriteOPNA_P0(adrs, dt);
                    }
                    break;
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xac:
                case 0xad:
                case 0xae:
                    if (!ConvertChipClock ||  (double)comPortOPNA.ChipClockHz["OPNA"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertOpnFrequency(dt, comPortOPNA.RegTable[adrs - 4], comPortOPNA.ChipClockHz["OPNA"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPNA_P0(adrs, dt);
                        deferredWriteOPNA_P0(adrs - 4, ret.Lo);
                    }
                    break;
                default:
                    deferredWriteOPNA_P0(adrs, dt);
                    break;
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

        private XGM_HEADER readXGMHeader(BinaryReader hFile)
        {
            XGM_HEADER curHead = new XGM_HEADER();
            FieldInfo[] fields = typeof(XGM_HEADER).GetFields();
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
                else if (field.FieldType == typeof(char))
                {
                    char val = hFile.ReadChar();
                    field.SetValue(curHead, val);
                }
                else if (field.FieldType == typeof(byte))
                {
                    byte val = hFile.ReadByte();
                    field.SetValue(curHead, val);
                }
            }

            return curHead;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool openVGMFile(string fileName)
        {
            //Read size
            using (FileStream vgmFile = File.Open(fileName, FileMode.Open))
            {
                var fileSize = (uint)vgmFile.Length;
                xgmReader = new BinaryReader(vgmFile);

                uint fccHeader;
                fccHeader = (uint)xgmReader.ReadUInt32();
                if (fccHeader != FCC_VGM)
                    return false;

                for (int i = 0; i < 63; i++)
                {
                    ushort adr = xgmReader.ReadUInt16();
                    ushort sz = xgmReader.ReadUInt16();
                    if (adr == 0xffff && sz == 0x1)
                        SampleDataTable[i] = new SampleData(this, 0, 0);
                    else
                        SampleDataTable[i] = new SampleData(this, (uint)(adr * 256), (uint)(sz * 256));
                }

                xgmMHead = readXGMHeader(xgmReader);

                //read dac
                dacData = xgmReader.ReadBytes(xgmMHead.shtSampleDataBlkSize * 256);

                //Music data bloc size.
                uint mlen = xgmReader.ReadUInt32();
                if (mlen < 0)
                    return false;

                //read vgm
                vgmData = xgmReader.ReadBytes((int)mlen);

                xgmReader = new BinaryReader(new MemoryStream(vgmData));

                if (Settings.Default.DCSG_Enable)
                {
                    coonectToDCSG();
                }
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

                return true;
            }
        }

        private bool connectToOPNA()
        {
            if (comPortOPNA == null)
            {
                switch (Settings.Default.OPNA_IF)
                {
                    case 0:
                        if (comPortOPNA == null)
                        {
                            comPortOPNA = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.OPNA_Port);
                            if (comPortOPNA != null)
                                comPortOPNA.ChipClockHz["OPNA"] = 8 * 1000 * 1000;
                        }
                        break;
                }
                if (comPortOPNA != null)
                {
                    Accepted = true;

                    ////Force OPN mode
                    //deferredWriteOPNA_P0(0x29, 0x0);

                    for (int i = 0x20; i <= 0x3F; i++)
                        deferredWriteOPNA_P0(i, 0);
                    //for (int i = 0x40; i <= 0x4F; i++)
                    //    deferredWriteOPNA_P0(i, 0xff);
                    for (int i = 0x50; i <= 0xB6; i++)
                        deferredWriteOPNA_P0(i, 0);

                    for (int i = 0x20; i <= 0x3F; i++)
                        deferredWriteOPNA_P1(i, 0);
                    //for (int i = 0x40; i <= 0x4F; i++)
                    //    deferredWriteOPNA_P1(i, 0xff);
                    for (int i = 0x50; i <= 0xB6; i++)
                        deferredWriteOPNA_P1(i, 0);

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
                        {
                            comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                                (PortId)Settings.Default.OPNA2_Port);
                            if (comPortOPN2 != null)
                                comPortOPN2.ChipClockHz["OPN2"] = 7670453;
                        }
                        break;
                    case 1:
                        if (comPortOPN2 == null)
                        {
                            comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                                (PortId)Settings.Default.OPNA2_Port);
                            if (comPortOPN2 != null)
                                comPortOPN2.ChipClockHz["OPN2"] = 7670453;
                        }
                        break;
                    case 2:
                        if (comPortOPN2 == null)
                        {
                            comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
                                (PortId)Settings.Default.OPNA2_Port);
                            if (comPortOPN2 != null)
                                comPortOPN2.ChipClockHz["OPN2"] = 7670453;
                        }
                        break;
                    case 3:
                        if (comPortOPN2 == null)
                        {
                            comPortOPN2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.OPNA2_Port);
                            if (comPortOPN2 != null)
                                comPortOPN2.ChipClockHz["OPN2"] = 7670453;
                        }
                        break;
                }
                if (comPortOPN2 != null)
                {
                    Accepted = true;

                    //DAC OFF
                    deferredWriteOPN2_P0(0x2B, 0);

                    for (int i = 0x20; i <= 0x3F; i++)
                        deferredWriteOPN2_P0(i, 0);
                    //for (int i = 0x40; i <= 0x4F; i++)
                    //    deferredWriteOPN2_P0(i, 0xff);
                    for (int i = 0x50; i <= 0xB6; i++)
                    {
                        if (0x80 <= i && i <= 0x8F)
                            continue;
                        deferredWriteOPN2_P0(i, 0x00);
                    }

                    for (int i = 0x20; i <= 0x3F; i++)
                        deferredWriteOPN2_P1(i, 0);
                    //for (int i = 0x40; i <= 0x4F; i++)
                    //    deferredWriteOPN2_P1(i, 0xff);
                    for (int i = 0x50; i <= 0xB6; i++)
                    {
                        if (0x80 <= i && i <= 0x8F)
                            continue;
                        deferredWriteOPN2_P1(i, 0x00);
                    }

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
                        {
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                                (PortId)Settings.Default.DCSG_Port);
                            if (comPortDCSG != null)
                                comPortDCSG.ChipClockHz["DCSG"] = 3579545;
                        }
                        break;
                    case 1:
                        if (comPortDCSG == null)
                        {
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                                (PortId)Settings.Default.DCSG_Port);
                            if (comPortDCSG != null)
                                comPortDCSG.ChipClockHz["DCSG"] = 3579545;
                        }
                        break;
                    case 2:
                        if (comPortDCSG == null)
                        {
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                                (PortId)Settings.Default.DCSG_Port);
                            if (comPortDCSG != null)
                                comPortDCSG.ChipClockHz["DCSG"] = 3579545;
                        }
                        break;
                    case 3:
                        if (comPortDCSG == null)
                        {
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
                                (PortId)Settings.Default.DCSG_Port);
                            if (comPortDCSG != null)
                                comPortDCSG.ChipClockHz["DCSG"] = 3579545;
                        }
                        break;
                    case 4:
                        if (comPortDCSG == null)
                        {
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                               (PortId)Settings.Default.DCSG_Port);
                            if (comPortDCSG != null)
                                comPortDCSG.ChipClockHz["DCSG"] = 3579545;
                        }
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

        private int readByte()
        {
            if (xgmReader.BaseStream == null)
                return -1;
            if (xgmReader.BaseStream.Position == xgmReader.BaseStream.Length)
                return -1;

            byte data = xgmReader.ReadByte();
            return data;
        }

        protected override void StreamSong()
        {
            xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
            double wait = 0;
            double xgmWaitDelta = 0;
            double streamWaitDelta = 0;
            double lastDiff = 0;
            using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, false, null))
            {
                long freq, before, after;
                QueryPerformanceFrequency(out freq);

                SampleData[] currentPlaySamples = new SampleData[4];

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
                        if (xgmWaitDelta <= 0)
                        {

                            int command = readByte();
                            if (command != -1)
                            {
                                switch (command)
                                {
                                    case int cmd when 0x0 <= cmd && cmd <= 0x0F:
                                        {
                                            switch (xgmMHead.bytNTSC_PAL & 1)
                                            {
                                                case 0:
                                                    xgmWaitDelta += 735;
                                                    break;
                                                case 1:
                                                    xgmWaitDelta += 882;
                                                    break;
                                            }
                                            flushDeferredWriteData();
                                            break;
                                        }
                                    case int cmd when 0x10 <= cmd && cmd <= 0x1F:
                                        {
                                            uint dclk = 3579545;

                                            int size = cmd & 0xf;
                                            for (int i = 0; i <= size; i++)
                                            {
                                                var data = readByte();
                                                if (data < 0)
                                                    break;
                                                if (comPortDCSG != null)
                                                {
                                                    deferredWriteDCSG(data, dclk);
                                                }
                                            }
                                        }
                                        break;

                                    case int cmd when 0x20 <= cmd && cmd <= 0x2F: //YM2612 Write Port 0
                                        {
                                            uint dclk = 7670453;

                                            int size = cmd & 0xf;
                                            for (int i = 0; i <= size; i++)
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
                                                    deferredWriteOPN2_P0(adrs, dt, dclk);
                                                }
                                                else if (comPortOPNA != null)
                                                {
                                                    deferredWriteOPNA_P0(adrs, dt, dclk);
                                                }
                                            }
                                        }
                                        break;

                                    case int cmd when 0x30 <= cmd && cmd <= 0x3F: //YM2612 Write Port 1
                                        {
                                            uint dclk = 7670453;

                                            int size = cmd & 0xf;
                                            for (int i = 0; i <= size; i++)
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
                                                    deferredWriteOPN2_P1(adrs, dt, dclk);
                                                }
                                                else if (comPortOPNA != null)
                                                {
                                                    deferredWriteOPNA_P1(adrs, dt, dclk);
                                                }
                                            }
                                        }
                                        break;

                                    case int cmd when 0x40 <= cmd && cmd <= 0x4F: //YM2612 Write Port 0
                                        {
                                            int size = cmd & 0xf;
                                            for (int i = 0; i <= size; i++)
                                            {
                                                var dt = readByte();
                                                if (dt < 0)
                                                    break;

                                                if (comPortOPN2 != null)
                                                {
                                                    deferredWriteOPN2_P0(0x28, dt);
                                                }
                                                else if (comPortOPNA != null)
                                                {
                                                    deferredWriteOPNA_P0(0x28, dt);
                                                }
                                            }
                                        }
                                        break;

                                    case int cmd when 0x50 <= cmd && cmd <= 0x5F: //Play PCM
                                        {
                                            int ch = cmd & 0x3;

                                            var id = readByte();
                                            if (id < 0)
                                                break;
                                            if (id == 0)
                                                currentPlaySamples[ch] = null;
                                            else
                                            {
                                                currentPlaySamples[ch] = SampleDataTable[id - 1];
                                                currentPlaySamples[ch].Restart();
                                            }
                                        }
                                        break;
                                    case 0x7E:
                                        flushDeferredWriteData();
                                        int ofst1 = readByte();
                                        if (ofst1 < 0)
                                            xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                        int ofst2 = readByte();
                                        if (ofst2 < 0)
                                            xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                        int ofst3 = readByte();
                                        if (ofst3 < 0)
                                            xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);

                                        xgmReader.BaseStream?.Seek((int)((ofst1 << 16)  | (ofst2 << 8) | ofst3), SeekOrigin.Begin);
                                        break;
                                    case 0x7F:
                                        flushDeferredWriteData();
                                        //End of song
                                        xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                        break;
                                }
                            }
                            if ((command == 0x7E || command == 0x7F || command == -1))
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

                        if (streamWaitDelta <= 0)
                        {
                            short dacData = 0;
                            bool playDac = false;
                            for (int i = 0; i < currentPlaySamples.Length; i++)
                            {
                                var dt = currentPlaySamples[i]?.GetDacData();
                                if (dt != null)
                                {
                                    dacData += (short)dt.Value;
                                    playDac = true;
                                }
                            }

                            if (playDac)
                            {
                                if (dacData > sbyte.MaxValue)
                                    dacData = sbyte.MaxValue;
                                else if (dacData < sbyte.MinValue)
                                    dacData = sbyte.MinValue;
                                dacData += 0x80;

                                comPortOPN2?.DeferredWriteData(0, 0x04, (byte)0x2a, (int)Settings.Default.BitBangWaitOPNA2);
                                comPortOPN2?.DeferredWriteData(0, 0x08, (byte)dacData, (int)Settings.Default.BitBangWaitOPNA2);

                                streamWaitDelta += 44.1d / 14d;
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
                        xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                    }

                    if (streamWaitDelta < xgmWaitDelta)
                    {
                        if (streamWaitDelta <= 0)
                        {
                            wait += xgmWaitDelta;
                            xgmWaitDelta = 0;
                        }
                        else
                        {
                            wait += streamWaitDelta;
                            xgmWaitDelta -= streamWaitDelta;
                            streamWaitDelta = 0;
                        }
                    }
                    else
                    {
                        if (xgmWaitDelta <= 0)
                        {
                            wait += streamWaitDelta;
                            streamWaitDelta = 0;
                        }
                        else
                        {
                            wait += xgmWaitDelta;
                            streamWaitDelta -= xgmWaitDelta;
                            xgmWaitDelta = 0;
                        }
                    }

                    if (wait <= (double)Settings.Default.VGMWait)
                        continue;

                    flushDeferredWriteData();

                    QueryPerformanceCounter(out after);
                    double pwait = (wait / PlaybackSpeed);
                    if (((double)(after - before) / freq) > (pwait / (44.1 * 1000)))
                    {
                        lastDiff = ((double)(after - before) / freq) - (pwait / (44.1 * 1000));
                        wait = -(lastDiff * 44.1 * 1000);
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

        private int deferredWriteOPN2_P1(int adrs, int dt, uint dclk)
        {
            comPortOPN2.RegTable[adrs + 0x100] = dt;

            switch (adrs)
            {
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                    if (!ConvertChipClock ||  (double)comPortOPN2.ChipClockHz["OPN2"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertOpnFrequency(comPortOPN2.RegTable[adrs + 4 + 0x100], dt, comPortOPN2.ChipClockHz["OPN2"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPN2_P1(adrs + 4, ret.Hi);
                        deferredWriteOPN2_P1(adrs, dt);
                    }
                    break;
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xac:
                case 0xad:
                case 0xae:
                    if (!ConvertChipClock ||  (double)comPortOPN2.ChipClockHz["OPN2"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertOpnFrequency(dt, comPortOPN2.RegTable[adrs - 4 + 0x100], comPortOPN2.ChipClockHz["OPN2"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPN2_P1(adrs, dt);
                        deferredWriteOPNA_P1(adrs - 4, ret.Lo);
                    }
                    break;
                default:
                    deferredWriteOPN2_P1(adrs, dt);
                    break;
            }

            return dt;
        }

        private void deferredWriteOPN2_P0(int adrs, int dt, uint dclk)
        {
            comPortOPN2.RegTable[adrs] = dt;

            switch (adrs)
            {
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                    if (!ConvertChipClock ||  (double)comPortOPN2.ChipClockHz["OPN2"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertOpnFrequency(comPortOPN2.RegTable[adrs + 4], dt, comPortOPN2.ChipClockHz["OPN2"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPN2_P0(adrs + 4, ret.Hi);
                        deferredWriteOPN2_P0(adrs, dt);
                    }
                    break;
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xac:
                case 0xad:
                case 0xae:
                    if (!ConvertChipClock ||  (double)comPortOPN2.ChipClockHz["OPN2"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertOpnFrequency(dt, comPortOPN2.RegTable[adrs - 4], comPortOPN2.ChipClockHz["OPN2"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPN2_P0(adrs, dt);
                        deferredWriteOPN2_P0(adrs - 4, ret.Lo);
                    }
                    break;
                default:
                    deferredWriteOPN2_P0(adrs, dt);
                    break;
            }
            deferredWriteOPN2_P0(adrs, dt);
        }

        private int deferredWriteOPNA_P1(int adrs, int dt, uint dclk)
        {
            comPortOPNA.RegTable[adrs + 0x100] = dt;

            switch (adrs)
            {
                case 0xa0:
                case 0xa1:
                case 0xa2:
                case 0xa8:
                case 0xa9:
                case 0xaa:
                    if (!ConvertChipClock ||  (double)comPortOPNA.ChipClockHz["OPNA"] == (double)dclk)
                        goto default;
                    {
                        //LO
                        var ret = convertOpnFrequency(comPortOPNA.RegTable[adrs + 4 + 0x100], dt, comPortOPNA.ChipClockHz["OPNA"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Lo;
                        deferredWriteOPNA_P1(adrs + 4, ret.Hi);
                        deferredWriteOPNA_P1(adrs, dt);
                    }
                    break;
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xac:
                case 0xad:
                case 0xae:
                    if (!ConvertChipClock ||  (double)comPortOPNA.ChipClockHz["OPNA"] == (double)dclk)
                        goto default;
                    {
                        //HI
                        var ret = convertOpnFrequency(dt, comPortOPNA.RegTable[adrs - 4 + 0x100], comPortOPNA.ChipClockHz["OPNA"], dclk);
                        if (ret.noConverted)
                            goto default;
                        dt = ret.Hi;
                        deferredWriteOPNA_P1(adrs, dt);
                        deferredWriteOPNA_P1(adrs - 4, ret.Lo);
                    }
                    break;
                default:
                    deferredWriteOPNA_P1(adrs, dt);
                    break;
            }

            return dt;
        }

        private void deferredWriteDCSG(int data, uint dclk)
        {
            byte adrs = (byte)(data >> 4);
            if ((data & 0x80) != 0)
                comPortDCSG.Tag["Last1stAddress"] = adrs;
            else if (comPortDCSG.Tag.ContainsKey("Last1stAddress"))
                adrs = (byte)((byte)comPortDCSG.Tag["Last1stAddress"] & 0x7);

            if (comPortDCSG != null)
            {
                switch (adrs)
                {
                    case 0:
                    case 2:
                    case 4:
                        if (!ConvertChipClock ||  (double)comPortDCSG.ChipClockHz["DCSG"] == (double)dclk)
                            goto default;
                        {
                            comPortDCSG.RegTable[adrs] = data & 0x3f;
                            //HI
                            var ret = convertDcsgFrequency(data & 0x3f, comPortDCSG.RegTable[adrs + 0x8], comPortDCSG.ChipClockHz["DCSG"], dclk);
                            if (ret.noConverted)
                                goto default;
                            deferredWriteDCSG((0x80 | (adrs << 4)) | ret.Lo);
                            deferredWriteDCSG(ret.Hi);
                        }
                        break;
                    case 0x8:
                    case 0x8 + 2:
                    case 0x8 + 4:
                        if (!ConvertChipClock ||  (double)comPortDCSG.ChipClockHz["DCSG"] == (double)dclk)
                            goto default;
                        {
                            comPortDCSG.RegTable[adrs] = data & 0xf;
                            //LO
                            var ret = convertDcsgFrequency(comPortDCSG.RegTable[adrs - 0x8], data & 0xf, comPortDCSG.ChipClockHz["DCSG"], dclk);
                            if (ret.noConverted)
                                goto default;
                            deferredWriteDCSG((adrs << 4) + ret.Lo);
                            deferredWriteDCSG(ret.Hi);
                        }
                        break;
                    default:
                        comPortDCSG.RegTable[adrs] = data;
                        deferredWriteDCSG(data);
                        break;
                }
            }
        }

        protected void deferredWriteDCSG(int data)
        {
            switch (comPortDCSG.SoundModuleType)
            {
                case VsifSoundModuleType.Genesis_FTDI:
                case VsifSoundModuleType.Genesis:
                case VsifSoundModuleType.Genesis_Low:
                    comPortDCSG.DeferredWriteData(0, 0x14, (byte)data, (int)Settings.Default.BitBangWaitDCSG);
                    break;
                case VsifSoundModuleType.SMS:
                    comPortDCSG.DeferredWriteData(0, 0xFF, (byte)data, (int)Settings.Default.BitBangWaitDCSG);
                    break;
                case VsifSoundModuleType.MSX_FTDI:
                    comPortDCSG.DeferredWriteData(0xF, 0, (byte)data, (int)Settings.Default.BitBangWaitDCSG);
                    break;
            }
        }

        private void flushDeferredWriteData()
        {
            comPortOPN2?.FlushDeferredWriteData();
            comPortDCSG?.FlushDeferredWriteData();
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // マネージド状態を破棄します (マネージド オブジェクト)
                    Stop();
                }

                xgmReader?.Dispose();

                // アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                comPortDCSG?.Dispose();
                comPortDCSG = null;
                comPortOPN2?.Dispose();
                comPortOPN2 = null;

                // 大きなフィールドを null に設定します
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

        // 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        ~XGMSong()
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

#pragma warning disable 0649, 0169
    class XGM_HEADER
    {
        /// <summary>
        /// Sample data bloc size / 256, ex: $0010 means 256*16 = 4096 bytes
        /// </summary>
        public ushort shtSampleDataBlkSize;
        /// <summary>
        /// Version information (0x01 currently)    
        /// </summary>
        public byte bytVersion;
        /// <summary>
        ///  bit #0: NTSC / PAL information: 0=NTSC  1=PAL
        ///  This field is used to determine how interpret the frame wait command.
        ///  In NTSC mode a frame wait command is equivalent to 1/60 of second.
        ///  In PAL mode a frame wait command is equivalent to 1/50 of second.
        ///  bit #1: GD3 tags: 0=No 1=Yes
        ///  If present the tags are located right after music data (address = $108+SLEN+MLEN)
        ///  bit #2: Multi track file: 0=No 1=Yes
        ///  When we have a multi track file, next track data can be found at the end of current music data.
        ///  Next track data directly starts on 'Music data bloc size' field (MLEN) means samples are shared
        ///  between all tracks. Depending the presence of GD3 tags, the next music data are located at
        ///  ($108+SLEN+MLEN) or($108+SLEN+MLEN+ size of GD3 tags)
        /// </summary>
        public byte bytNTSC_PAL;
    }
#pragma warning restore 0649, 0169

}
