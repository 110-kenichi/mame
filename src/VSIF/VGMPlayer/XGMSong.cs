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
    class XGMSong : SongBase
    {
        private VsifClient comPortDCSG;

        private VsifClient comPortOPNA2;

        private const uint FCC_VGM = 0x204D4758;    // 'XGM '

        private XGM_HEADER _XGMHead;

        private BinaryReader _xgmReader;

        private byte[] _DACData;

        private byte[] _VGMData;

        private SampleData[] Samples = new SampleData[63];

        private class SampleData
        {
            public uint Address;
            public uint Size;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="adress"></param>
            /// <param name="size"></param>
            public SampleData(uint adress, uint size)
            {
                Address = adress;
                Size = size;
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
                switch (comPortDCSG?.SoundModuleType)
                {
                    case VsifSoundModuleType.Genesis:
                    case VsifSoundModuleType.Genesis_FTDI:
                        for (int i = 0; i < 3; i++)
                            comPortDCSG.WriteData(0x14, (byte)(0x80 | i << 5 | 0x1f));
                        comPortDCSG.WriteData(0x14, (byte)(0x80 | 3 << 5 | 0x1f));
                        break;
                    case VsifSoundModuleType.SMS:
                        for (int i = 0; i < 3; i++)
                            comPortDCSG.WriteData(0xFF, (byte)(0x80 | i << 5 | 0x1f));
                        comPortDCSG.WriteData(0xFF, (byte)(0x80 | 3 << 5 | 0x1f));
                        break;
                }
            }
            if (comPortOPNA2 != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    comPortOPNA2?.WriteData(4, (byte)(0xB4 | i));
                    comPortOPNA2?.WriteData(8, 0xC0);
                    comPortOPNA2?.WriteData(12, (byte)(0xB4 | i));
                    comPortOPNA2?.WriteData(16, 0xC0);
                }

                // disable LFO
                comPortOPNA2?.WriteData(4, 0x22);
                comPortOPNA2?.WriteData(8, 0x00);

                // disable timer & set channel 6 to normal mode
                comPortOPNA2?.WriteData(4, 0x27);
                comPortOPNA2?.WriteData(8, 0x00);

                // ALL KEY OFF
                comPortOPNA2?.WriteData(4, 0x28);
                for (int i = 0; i < 3; i++)
                {
                    comPortOPNA2?.WriteData(8, (byte)(0x00 | i));
                    comPortOPNA2?.WriteData(8, (byte)(0x04 | i));
                }

                // disable DAC
                comPortOPNA2?.WriteData(4, 0x2B);
                comPortOPNA2?.WriteData(8, 0x00);

                for (int slot = 0; slot < 6; slot++)
                {
                    uint reg = (uint)(slot / 3) * 2;
                    Ym2612WriteData(0x28, 0, 0, (byte)(0x00 | (reg << 1) | (byte)(slot % 3)));

                    //TL
                    if (volumeOff)
                        for (int op = 0; op < 4; op++)
                            Ym2612WriteData(0x40, op, slot, 127);
                }
            }
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

            uint yreg = (uint)(0 / 3) * 2;
            comPortOPNA2?.WriteData((byte)((1 + (yreg + 0)) * 4), (byte)(address + (op * 4) + (slot % 3)));
            comPortOPNA2?.WriteData((byte)((1 + (yreg + 1)) * 4), data);
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
                _xgmReader = new BinaryReader(vgmFile);

                uint fccHeader;
                fccHeader = (uint)_xgmReader.ReadUInt32();
                if (fccHeader != FCC_VGM)
                    return false;

                for (int i = 0; i < 63; i++)
                {
                    ushort adr = _xgmReader.ReadUInt16();
                    ushort sz = _xgmReader.ReadUInt16();
                    if (adr == 0xffff && sz == 0x1)
                        Samples[i] = new SampleData(0, 0);
                    else
                        Samples[i] = new SampleData((uint)(adr * 256), (uint)(sz * 256));
                }

                _XGMHead = readXGMHeader(_xgmReader);

                //read dac
                _DACData = _xgmReader.ReadBytes(_XGMHead.shtSampleDataBlkSize * 256);

                uint mlen = _xgmReader.ReadUInt32();
                if (mlen < 0)
                    return false;

                //read vgm
                _VGMData = _xgmReader.ReadBytes((int)mlen);

                _xgmReader = new BinaryReader(new MemoryStream(_VGMData));

                switch (Settings.Default.DCSG_IF)
                {
                    case 0:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                            (ComPort)Settings.Default.DCSG_Port);
                        break;
                    case 1:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                            (ComPort)Settings.Default.DCSG_Port);
                        break;
                    case 2:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                            (ComPort)Settings.Default.DCSG_Port);
                        break;
                }
                switch (Settings.Default.OPNA2_IF)
                {
                    case 0:
                        comPortOPNA2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis,
                            (ComPort)Settings.Default.OPNA2_Port);
                        break;
                    case 1:
                        comPortOPNA2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI,
                            (ComPort)Settings.Default.OPNA2_Port);
                        break;
                }

                return true;
            }
        }

        private int readByte()
        {
            if (_xgmReader.BaseStream == null)
                return -1;
            if (_xgmReader.BaseStream.Position == _xgmReader.BaseStream.Length)
                return -1;

            byte data = _xgmReader.ReadByte();
            return data;
        }

        protected override void StreamSong()
        {
            _xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
            double wait = 0;
            double lastDiff = 0;
            using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, false, null))
            {
                long freq, before, after;
                QueryPerformanceFrequency(out freq);

                while (true)
                {
                    if (State == SoundState.Stopped)
                    {
                        break;
                    }
                    else if (State == SoundState.Paused)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    QueryPerformanceCounter(out before);

                    try
                    {
                        int data = readByte();
                        int command = data;
                        if (data != -1)
                        {
                            switch (data & 0xf0)
                            {
                                case 0x0:
                                    {
                                        switch (_XGMHead.bytNTSC_PAL & 1)
                                        {
                                            case 0:
                                                wait += 735;
                                                break;
                                            case 1:
                                                wait += 882;
                                                break;
                                        }
                                        flushDeferredWriteData();
                                        break;
                                    }
                                case 0x10:
                                    {
                                        int size = data & 0xf;
                                        for (int i = 0; i <= size; i++)
                                        {
                                            data = readByte();
                                            if (data < 0)
                                                break;
                                            switch (comPortDCSG?.SoundModuleType)
                                            {
                                                case VsifSoundModuleType.Genesis:
                                                case VsifSoundModuleType.Genesis_FTDI:
                                                    comPortDCSG?.DeferredWriteData(0x14, (byte)data);
                                                    break;
                                                case VsifSoundModuleType.SMS:
                                                    comPortDCSG?.DeferredWriteData(0xFF, (byte)data);
                                                    break;
                                            }
                                        }
                                    }
                                    break;

                                case 0x20: //YM2612 Write Port 0
                                    {
                                        int size = data & 0xf;
                                        for (int i = 0; i <= size; i++)
                                        {
                                            data = readByte();
                                            if (data < 0)
                                                break;
                                            comPortOPNA2?.DeferredWriteData(0x04, (byte)data);
                                            data = readByte();
                                            if (data < 0)
                                                break;
                                            comPortOPNA2?.DeferredWriteData(0x08, (byte)data);
                                        }
                                    }
                                    break;

                                case 0x30: //YM2612 Write Port 1
                                    {
                                        int size = data & 0xf;
                                        for (int i = 0; i <= size; i++)
                                        {
                                            data = readByte();
                                            if (data < 0)
                                                break;
                                            comPortOPNA2?.DeferredWriteData(0x0C, (byte)data);
                                            data = readByte();
                                            if (data < 0)
                                                break;
                                            comPortOPNA2?.DeferredWriteData(0x10, (byte)data);
                                        }
                                    }
                                    break;

                                case 0x40: //YM2612 Write Port 0
                                    {
                                        int size = data & 0xf;
                                        for (int i = 0; i <= size; i++)
                                        {
                                            data = readByte();
                                            if (data < 0)
                                                break;
                                            comPortOPNA2?.DeferredWriteData(0x04, 0x28);
                                            comPortOPNA2?.DeferredWriteData(0x08, (byte)data);
                                        }
                                    }
                                    break;

                                case 0x50:
                                    {
                                        //Play PCM
                                        data = readByte();
                                        if (data < 0)
                                            break;
                                        data = readByte();
                                        if (data < 0)
                                            break;
                                    }
                                    break;
                                case 0x7E:
                                    flushDeferredWriteData();
                                    uint ofst = _xgmReader.ReadUInt32();
                                    if (ofst < 0)
                                        _xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                    else
                                        _xgmReader.BaseStream?.Seek((int)ofst, SeekOrigin.Begin);
                                    break;
                                case 0x7F:
                                    flushDeferredWriteData();
                                    //End of song
                                    _xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                    break;
                            }
                        }
                        if ((command == 0x7E || command == 0x7F || data == -1))
                        {
                            flushDeferredWriteData();
                            if (Looped == false || LoopCount == 0)
                            {
                                State = SoundState.Stopped;
                                NotifyFinished();
                                break;
                            }
                            LoopCount--;
                            _xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
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
                            NotifyFinished();
                            break;
                        }
                        LoopCount--;
                        _xgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                    }

                    if (wait <= 0)
                        continue;

                    flushDeferredWriteData();

                    QueryPerformanceCounter(out after);
                    double pwait = (wait / PlaybackSpeed) - lastDiff;
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
                    StopAllSounds(true);
                }

                _xgmReader?.Dispose();

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
