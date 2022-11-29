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

        private VsifClient comPortOPNA2;

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
                            comPortDCSG.WriteData(0, 0x14, (byte)(0x80 | i << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        comPortDCSG.WriteData(0, 0x14, (byte)(0x80 | 3 << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        break;
                    case VsifSoundModuleType.SMS:
                        for (int i = 0; i < 3; i++)
                            comPortDCSG.WriteData(0, 0xFF, (byte)(0x80 | i << 5 | 0x1f), (int)Settings.Default.BitBangWaitOPNA2);
                        comPortDCSG.WriteData(0, 0xFF, (byte)(0x80 | 3 << 5 | 0x1f), (int)Settings.Default.BitBangWaitOPNA2);
                        break;
                    case VsifSoundModuleType.MSX_FTDI:
                        for (int i = 0; i < 3; i++)
                            comPortDCSG.WriteData(0xF, 0, (byte)(0x80 | i << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        comPortDCSG.WriteData(0xF, 0, (byte)(0x80 | 3 << 5 | 0x1f), (int)Settings.Default.BitBangWaitDCSG);
                        break;
                }
                comPortDCSG.FlushDeferredWriteData();
            }
            if (comPortOPNA2 != null)
            {
                comPortOPNA2.ClearDeferredWriteData();

                if (comPortOPNA2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        comPortOPNA2.DeferredWriteData(0x10, (byte)(0xB4 | i), 0xC0, (int)Settings.Default.BitBangWaitOPNA2);
                        comPortOPNA2.DeferredWriteData(0x11, (byte)(0xB4 | i), 0xC0, (int)Settings.Default.BitBangWaitOPNA2);
                    }

                    // disable LFO
                    comPortOPNA2.DeferredWriteData(0x10, 0x22, 0x00, (int)Settings.Default.BitBangWaitOPNA2);

                    // disable timer & set channel 6 to normal mode
                    comPortOPNA2.DeferredWriteData(0x10, 0x27, 0x00, (int)Settings.Default.BitBangWaitOPNA2);

                    // ALL KEY OFF
                    for (int i = 0; i < 3; i++)
                    {
                        comPortOPNA2.DeferredWriteData(0x10, 0x28, (byte)(0x00 | i), (int)Settings.Default.BitBangWaitOPNA2);
                        comPortOPNA2.DeferredWriteData(0x10, 0x28, (byte)(0x04 | i), (int)Settings.Default.BitBangWaitOPNA2);
                    }

                    // disable DAC
                    comPortOPNA2.DeferredWriteData(0x10, 0x2B, 0x00, (int)Settings.Default.BitBangWaitOPNA2);
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        comPortOPNA2.DeferredWriteData(0, 4, (byte)(0xB4 | i), (int)Settings.Default.BitBangWaitOPNA2);
                        comPortOPNA2.DeferredWriteData(0, 8, 0xC0, (int)Settings.Default.BitBangWaitOPNA2);
                        comPortOPNA2.DeferredWriteData(0, 12, (byte)(0xB4 | i), (int)Settings.Default.BitBangWaitOPNA2);
                        comPortOPNA2.DeferredWriteData(0, 16, 0xC0, (int)Settings.Default.BitBangWaitOPNA2);
                    }

                    // disable LFO
                    comPortOPNA2.DeferredWriteData(0, 4, 0x22, (int)Settings.Default.BitBangWaitOPNA2);
                    comPortOPNA2.DeferredWriteData(0, 8, 0x00, (int)Settings.Default.BitBangWaitOPNA2);

                    // disable timer & set channel 6 to normal mode
                    comPortOPNA2.DeferredWriteData(0, 4, 0x27, (int)Settings.Default.BitBangWaitOPNA2);
                    comPortOPNA2.DeferredWriteData(0, 8, 0x00, (int)Settings.Default.BitBangWaitOPNA2);

                    // ALL KEY OFF
                    comPortOPNA2.DeferredWriteData(0, 4, 0x28, (int)Settings.Default.BitBangWaitOPNA2);
                    for (int i = 0; i < 3; i++)
                    {
                        comPortOPNA2.DeferredWriteData(0, 8, (byte)(0x00 | i), (int)Settings.Default.BitBangWaitOPNA2);
                        comPortOPNA2.DeferredWriteData(0, 8, (byte)(0x04 | i), (int)Settings.Default.BitBangWaitOPNA2);
                    }

                    // disable DAC
                    comPortOPNA2.DeferredWriteData(0, 4, 0x2B, (int)Settings.Default.BitBangWaitOPNA2);
                    comPortOPNA2.DeferredWriteData(0, 8, 0x00, (int)Settings.Default.BitBangWaitOPNA2);
                }

                if (volumeOff)
                {
                    //TL
                    for (int slot = 0; slot < 6; slot++)
                    {
                        for (int op = 0; op < 4; op++)
                            Ym2612WriteData(0x40, op, slot, 127);
                    }
                }

                comPortOPNA2.FlushDeferredWriteData();
            }

            Thread.Sleep(50);
        }

        private void Ym2612WriteData(byte address, int op, int slot, byte data)
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

            if (comPortOPNA2 != null)
            {
                if (comPortOPNA2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                {
                    comPortOPNA2.DeferredWriteData((byte)(0x10 + (slot / 3)), (byte)(address + (op * 4) + (slot % 3)), data, (int)Settings.Default.BitBangWaitOPNA2);
                }
                else
                {
                    uint yreg = (uint)(0 / 3) * 2;
                    comPortOPNA2.DeferredWriteData(0, (byte)((1 + (yreg + 0)) * 4), (byte)(address + (op * 4) + (slot % 3)), (int)Settings.Default.BitBangWaitOPNA2);
                    comPortOPNA2.DeferredWriteData(0, (byte)((1 + (yreg + 1)) * 4), data, (int)Settings.Default.BitBangWaitOPNA2);
                }
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

                uint mlen = xgmReader.ReadUInt32();
                if (mlen < 0)
                    return false;

                //read vgm
                vgmData = xgmReader.ReadBytes((int)mlen);

                xgmReader = new BinaryReader(new MemoryStream(vgmData));

                if (Settings.Default.DCSG_Enable)
                {
                    switch (Settings.Default.DCSG_IF)
                    {
                        case 0:
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                                (PortId)Settings.Default.DCSG_Port);
                            break;
                        case 1:
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                                (PortId)Settings.Default.DCSG_Port);
                            break;
                        case 2:
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                                (PortId)Settings.Default.DCSG_Port);
                            break;
                        case 3:
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
                                (PortId)Settings.Default.DCSG_Port);
                            break;
                        case 4:
                            comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.DCSG_Port);
                            break;
                    }
                }
                if (Settings.Default.OPNA2_Enable)
                {
                    switch (Settings.Default.OPNA2_IF)
                    {
                        case 0:
                            comPortOPNA2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                                (PortId)Settings.Default.OPNA2_Port);
                            break;
                        case 1:
                            comPortOPNA2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                                (PortId)Settings.Default.OPNA2_Port);
                            break;
                        case 2:
                            comPortOPNA2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
                                (PortId)Settings.Default.OPNA2_Port);
                            break;
                        case 3:
                            comPortOPNA2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI,
                                (PortId)Settings.Default.OPNA2_Port);
                            break;
                    }
                }

                return true;
            }
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

                    if (State == SoundState.Stopped)
                    {
                        break;
                    }
                    else if (State == SoundState.Paused)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
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
                                            int size = cmd & 0xf;
                                            for (int i = 0; i <= size; i++)
                                            {
                                                var data = readByte();
                                                if (data < 0)
                                                    break;
                                                switch (comPortDCSG?.SoundModuleType)
                                                {
                                                    case VsifSoundModuleType.Genesis:
                                                    case VsifSoundModuleType.Genesis_Low:
                                                    case VsifSoundModuleType.Genesis_FTDI:
                                                        comPortDCSG?.DeferredWriteData(0, 0x14, (byte)data, (int)Settings.Default.BitBangWaitOPNA2);
                                                        break;
                                                    case VsifSoundModuleType.SMS:
                                                        comPortDCSG?.DeferredWriteData(0, 0xFF, (byte)data, (int)Settings.Default.BitBangWaitOPNA2);
                                                        break;
                                                    case VsifSoundModuleType.MSX_FTDI:
                                                        comPortDCSG?.DeferredWriteData(0xF, 0, (byte)data, (int)Settings.Default.BitBangWaitDCSG);
                                                        break;
                                                }
                                            }
                                        }
                                        break;

                                    case int cmd when 0x20 <= cmd && cmd <= 0x2F: //YM2612 Write Port 0
                                        {
                                            int size = cmd & 0xf;
                                            for (int i = 0; i <= size; i++)
                                            {
                                                var data = readByte();
                                                if (data < 0)
                                                    break;

                                                if (comPortOPNA2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                                                {
                                                    var adrs = data;
                                                    data = readByte();
                                                    if (data < 0)
                                                        break;
                                                    comPortOPNA2.DeferredWriteData(0x10, (byte)adrs, (byte)data, (int)Settings.Default.BitBangWaitOPNA2);
                                                }
                                                else
                                                {
                                                    comPortOPNA2?.DeferredWriteData(0, 0x04, (byte)data, (int)Settings.Default.BitBangWaitOPNA2);
                                                    data = readByte();
                                                    if (data < 0)
                                                        break;
                                                    comPortOPNA2?.DeferredWriteData(0, 0x08, (byte)data, (int)Settings.Default.BitBangWaitOPNA2);
                                                }
                                            }
                                        }
                                        break;

                                    case int cmd when 0x30 <= cmd && cmd <= 0x3F: //YM2612 Write Port 1
                                        {
                                            int size = cmd & 0xf;
                                            for (int i = 0; i <= size; i++)
                                            {
                                                var data = readByte();
                                                if (data < 0)
                                                    break;
                                                if (comPortOPNA2.SoundModuleType == VsifSoundModuleType.MSX_FTDI)
                                                {
                                                    var adrs = data;
                                                    data = readByte();
                                                    if (data < 0)
                                                        break;
                                                    comPortOPNA2.DeferredWriteData(0x11, (byte)adrs, (byte)data, (int)Settings.Default.BitBangWaitOPNA2);
                                                }
                                                else
                                                {
                                                    comPortOPNA2?.DeferredWriteData(0, 0x0C, (byte)data, (int)Settings.Default.BitBangWaitOPNA2);
                                                    data = readByte();
                                                    if (data < 0)
                                                        break;
                                                    comPortOPNA2?.DeferredWriteData(0, 0x10, (byte)data, (int)Settings.Default.BitBangWaitOPNA2);
                                                }
                                            }
                                        }
                                        break;

                                    case int cmd when 0x40 <= cmd && cmd <= 0x4F: //YM2612 Write Port 0
                                        {
                                            int size = cmd & 0xf;
                                            for (int i = 0; i <= size; i++)
                                            {
                                                var data = readByte();
                                                if (data < 0)
                                                    break;
                                                comPortOPNA2?.DeferredWriteData(0, 0x04, 0x28, (int)Settings.Default.BitBangWaitOPNA2);
                                                comPortOPNA2?.DeferredWriteData(0, 0x08, (byte)data, (int)Settings.Default.BitBangWaitOPNA2);
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
                                        uint ofst = xgmReader.ReadUInt32();
                                        if (ofst < 0)
                                            xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                        else
                                            xgmReader.BaseStream?.Seek((int)ofst, SeekOrigin.Begin);
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
                                if (Looped == false || LoopCount == 0)
                                {
                                    State = SoundState.Stopped;
                                    StopAllSounds(true);
                                    NotifyFinished();
                                    break;
                                }
                                LoopCount--;
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

                                comPortOPNA2?.DeferredWriteData(0, 0x04, (byte)0x2a, (int)Settings.Default.BitBangWaitOPNA2);
                                comPortOPNA2?.DeferredWriteData(0, 0x08, (byte)dacData, (int)Settings.Default.BitBangWaitOPNA2);

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
                        if (Looped == false || LoopCount == 0)
                        {
                            State = SoundState.Stopped;
                            StopAllSounds(true);
                            NotifyFinished();
                            break;
                        }
                        LoopCount--;
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
        }

        private void flushDeferredWriteData()
        {
            comPortOPNA2?.FlushDeferredWriteData();
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
                comPortOPNA2?.Dispose();
                comPortOPNA2 = null;

                // 大きなフィールドを null に設定します
                disposedValue = true;
            }
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
