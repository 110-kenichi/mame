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
    class VGMSong : SongBase
    {
        private VsifClient comPortDCSG;
        private VsifClient comPortOPLL;
        private VsifClient comPortOPNA2;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public VGMSong(string fileName) : base(fileName)
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
            if (comPortOPLL != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    comPortOPLL.WriteData((byte)(0x20 + i), (byte)(0));
                }
                comPortOPLL.WriteData(0xe, (byte)(0x20));

                //TL
                if (volumeOff)
                {
                    for (int i = 0; i < 9; i++)
                        comPortOPLL.WriteData((byte)(0x30 + i), 64);
                    comPortOPLL.WriteData(0x36, 64);
                    comPortOPLL.WriteData(0x37, 64);
                    comPortOPLL.WriteData(0x38, 64);
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

        const uint FCC_VGM = 0x206D6756;    // 'Vgm '
        uint _VGMDataLen;
        VGM_HEADER _VGMHead;

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

            if (curHead.lngHzPSG != 0)
            {
                if (curHead.shtPSG_Feedback == 0)
                    curHead.shtPSG_Feedback = 0x0009;
                if (curHead.bytPSG_SRWidth == 0)
                    curHead.bytPSG_SRWidth = 0x10;

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
            }
            if (curHead.lngHzYM2413 != 0)
            {
                comPortOPLL = VsifManager.TryToConnectVSIF(VsifSoundModuleType.SMS,
                    (ComPort)Settings.Default.OPLL_Port);
            }
            if (curHead.lngHzYM2612 != 0)
            {
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
            }
            return curHead;
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

        private BinaryReader _vgmReader;
        private byte[] _DACData;
        private byte[] _VGMData;
        private int _DACOffset = 0;
        private int _VGMDataOffset;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool openVGMFile(string fileName)
        {
            bool zipped = checkIfZip(fileName, 3, "1F-8B-08");

            //Read size
            uint FileSize = 0;
            int offset = 0;
            using (FileStream vgmFile = File.Open(fileName, FileMode.Open))
            {
                if (zipped)
                {
                    vgmFile.Position = vgmFile.Length - 4;
                    byte[] b = new byte[4];
                    vgmFile.Read(b, 0, 4);
                    uint fileSize = BitConverter.ToUInt32(b, 0);
                    FileSize = fileSize;
                    vgmFile.Position = 0;

                    GZipStream stream = new GZipStream(vgmFile, CompressionMode.Decompress);
                    _vgmReader = new BinaryReader(stream);
                    zipped = true;
                }
                else
                {
                    FileSize = (uint)vgmFile.Length;
                    _vgmReader = new BinaryReader(vgmFile);
                }

                uint fccHeader;
                fccHeader = (uint)_vgmReader.ReadUInt32();
                if (fccHeader != FCC_VGM)
                    return false;

                _VGMDataLen = FileSize;
                _VGMHead = readVGMHeader(_vgmReader);

                //Figure out header offset
                offset = (int)_VGMHead.lngDataOffset;
                if (offset == 0 || offset == 0x0000000C)
                    offset = 0x40;
                _VGMDataOffset = offset;
            }
            using (FileStream vgmFile = File.Open(fileName, FileMode.Open))
            {
                if (zipped)
                {
                    GZipStream stream = new GZipStream(vgmFile, CompressionMode.Decompress);
                    _vgmReader = new BinaryReader(stream);
                }
                else
                {
                    _vgmReader = new BinaryReader(vgmFile);
                    _vgmReader.BaseStream.Seek(0, SeekOrigin.Begin);
                }
                _vgmReader.ReadBytes(offset);
                _VGMData = _vgmReader.ReadBytes((int)(FileSize - offset));


                _vgmReader = new BinaryReader(new MemoryStream(_VGMData));
                if ((byte)_vgmReader.PeekChar() == 0x67)
                {
                    _vgmReader.ReadByte();
                    if ((byte)_vgmReader.PeekChar() == 0x66)
                    {
                        _vgmReader.ReadByte();
                        byte type = _vgmReader.ReadByte();
                        uint size = _vgmReader.ReadUInt32();
                        _DACData = _vgmReader.ReadBytes((int)size);
                    }
                }
            }
            return true;
        }

        private int readByte()
        {
            if (_vgmReader.BaseStream == null)
                return -1;
            if (_vgmReader.BaseStream.Position == _vgmReader.BaseStream.Length)
                return -1;

            byte data = _vgmReader.ReadByte();
            return data;
        }

        protected override void StreamSong()
        {
            _vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
            using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, false, null))
            {
                long lpSystemTimeAsFileTime;
                GetSystemTimeAsFileTime(out lpSystemTimeAsFileTime);
                double lastTime100ns = lpSystemTimeAsFileTime;

                while (true)
                {
                    if (State == SoundState.Stopped)
                    {
                        break;
                    }
                    else if (State == SoundState.Paused)
                    {
                        Thread.Sleep(1);
                        GetSystemTimeAsFileTime(out lpSystemTimeAsFileTime);
                        lastTime100ns = lpSystemTimeAsFileTime;
                        continue;
                    }
                    try
                    {
                        int data = readByte();
                        int command = data;
                        if (data != -1)
                        {
                            switch (data)
                            {
                                case 0x4F:
                                    {
                                        data = readByte();
                                        if (data < 0)
                                            break;
                                    }
                                    break;

                                case 0x50:
                                    {
                                        data = readByte();
                                        if (data < 0)
                                            break;
                                        switch (comPortDCSG?.SoundModuleType)
                                        {
                                            case VsifSoundModuleType.Genesis_FTDI:
                                            case VsifSoundModuleType.Genesis:
                                                comPortDCSG?.DeferredWriteData(0x14, (byte)data);
                                                break;
                                            case VsifSoundModuleType.SMS:
                                                comPortDCSG?.DeferredWriteData(0xFF, (byte)data);
                                                break;
                                        }
                                    }
                                    break;

                                case 0x51: //YM2413 Write Port 0
                                    {
                                        byte aa = _vgmReader.ReadByte();
                                        byte dd = _vgmReader.ReadByte();
                                        comPortOPLL?.DeferredWriteData(aa, dd);
                                    }
                                    break;

                                case 0x52: //YM2612 Write Port 0
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
                                    break;

                                case 0x53: //YM2612 Write Port 1
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
                                    break;

                                case 0x61: //Wait N samples
                                    {
                                        ushort time = _vgmReader.ReadUInt16();
                                        Wait += time;
                                        flushDeferredWriteData();
                                    }
                                    break;

                                case 0x62: //Wait 735 samples
                                    Wait += 735;
                                    flushDeferredWriteData();
                                    break;

                                case 0x63: //Wait 882 samples
                                    Wait += 882;
                                    flushDeferredWriteData();
                                    break;

                                case 0xE0: //Seek to offset in PCM databank
                                    uint offset = _vgmReader.ReadUInt32();
                                    _DACOffset = (int)offset;
                                    break;

                                case 0x67: //Skip VGM Data
                                    {
                                        data = readByte();
                                        if (data < 0)
                                            break;
                                        //type
                                        data = readByte();
                                        if (data < 0)
                                            break;
                                        uint size = _vgmReader.ReadUInt32();
                                        _vgmReader.BaseStream.Position += size;
                                    }
                                    break;
                                case 0x66:
                                    //End of song
                                    flushDeferredWriteData();
                                    if (Looped == false)
                                    {
                                        _vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                        break;
                                    }
                                    else
                                    {
                                        if (_VGMHead.lngLoopOffset != 0 && _VGMDataOffset < _VGMHead.lngLoopOffset)
                                            _vgmReader.BaseStream?.Seek((_VGMHead.lngLoopOffset - (_VGMDataOffset)) /*+ 0x1C*/, SeekOrigin.Begin);
                                        else
                                            _vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                    }
                                    break;
                            }
                            if (command >= 0x70 && command <= 0x7F)
                            {
                                Wait += (data & 15) + 1;
                                flushDeferredWriteData();
                            }
                            else if (command >= 0x80 && command <= 0x8F)
                            {
                                Wait += (data & 15);
                                //_chip.WritePort0(0x2A, _DACData[_DACOffset]);
                                comPortOPNA2?.DeferredWriteData(0x04, (byte)0x2a);
                                comPortOPNA2?.DeferredWriteData(0x08, (byte)_DACData[_DACOffset]);
                                _DACOffset++;
                                flushDeferredWriteData();
                            }

                            //if (_wait != 0)
                            //    _wait -= 1;
                        }
                        if ((command == 0x66 || data == -1))
                        {
                            flushDeferredWriteData();
                            if (Looped == false || LoopCount == 0)
                            {
                                State = SoundState.Stopped;
                                NotifyFinished();
                                break;
                            }
                            LoopCount--;
                            _vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                        }
                    }
                    catch
                    {
                        flushDeferredWriteData();
                        if (Looped == false || LoopCount == 0)
                        {
                            State = SoundState.Stopped;
                            NotifyFinished();
                            break;
                        }
                        LoopCount--;
                        _vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                    }

                    double wait_100ns = (22.67573 * 10 * Wait);
                    if (wait_100ns / PlaybackSpeed >= Program.MinimumResolution)
                    {
                        lastTime100ns += wait_100ns / PlaybackSpeed;
                        long dueTime = (long)Math.Round(lastTime100ns);
                        SetWaitableTimer(handle, ref dueTime, 0, IntPtr.Zero, IntPtr.Zero, false);
                        WaitForSingleObject(handle, WAIT_TIMEOUT);
                        Wait = 0;

                        // Next time is past time?
                        GetSystemTimeAsFileTime(out lpSystemTimeAsFileTime);
                        if (lpSystemTimeAsFileTime > lastTime100ns)
                        {
                            Wait = -(long)Math.Round((lpSystemTimeAsFileTime - dueTime) / (22.67573 * 10));
                            lastTime100ns = lpSystemTimeAsFileTime;  // adjust to current time
                            NotifyProcessLoadOccurred();
                        }
                    }
                }
            }
        }

        private void flushDeferredWriteData()
        {
            comPortOPLL?.FlushDeferredWriteData();
            comPortOPNA2?.FlushDeferredWriteData();
            comPortDCSG?.FlushDeferredWriteData();
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
                    StopAllSounds(true);
                }

                _vgmReader?.Dispose();

                // アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                comPortDCSG?.Dispose();
                comPortDCSG = null;
                comPortOPLL?.Dispose();
                comPortOPLL = null;
                comPortOPNA2?.Dispose();
                comPortOPNA2 = null;

                // 大きなフィールドを null に設定します
                disposedValue = true;
            }
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

#pragma warning disable 0649, 0169
    class VGM_HEADER
    {
        public uint lngEOFOffset;
        public uint lngVersion;
        public uint lngHzPSG;
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
        public char bytLoopBase;
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
