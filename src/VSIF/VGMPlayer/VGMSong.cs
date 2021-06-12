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
        private const uint FCC_VGM = 0x206D6756;    // 'Vgm '

        private byte[] vgmData;
        private int vgmDataOffset;
        private uint vgmDataLen;
        private VGM_HEADER vgmHead;

        private VsifClient comPortDCSG;
        private VsifClient comPortOPLL;
        private VsifClient comPortOPNA2;

        private BinaryReader vgmReader;

        private List<byte> dacData;
        private List<int> dacDataOffset;
        private List<int> dacDataLength;

        private int dacOffset = 0;

        private Dictionary<int, StreamData> streamTable = new Dictionary<int, StreamData>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public VGMSong(string fileName) : base(fileName)
        {
            openVGMFile(fileName);

            dacData = new List<byte>();
            dacDataOffset = new List<int>();
            dacDataLength = new List<int>();

            //streamingThread = new Thread(new ThreadStart(playStream));
            //streamingThread.Priority = ThreadPriority.AboveNormal;
            //streamingThread.Start();
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
                            comPortDCSG.WriteData(0x14, (byte)(0x80 | i << 5 | 0x1f));
                        comPortDCSG.WriteData(0x14, (byte)(0x80 | 3 << 5 | 0x1f));
                        break;
                    case VsifSoundModuleType.SMS:
                        for (int i = 0; i < 3; i++)
                            comPortDCSG.WriteData(0xFF, (byte)(0x80 | i << 5 | 0x1f));
                        comPortDCSG.WriteData(0xFF, (byte)(0x80 | 3 << 5 | 0x1f));
                        break;
                }
                comPortDCSG.FlushDeferredWriteData();
            }
            if (comPortOPLL != null)
            {
                comPortOPLL.ClearDeferredWriteData();

                for (int i = 0; i < 9; i++)
                {
                    comPortOPLL.DeferredWriteData((byte)(0x20 + i), (byte)(0));
                }
                comPortOPLL.DeferredWriteData(0xe, (byte)(0x20));

                //TL
                if (volumeOff)
                {
                    for (int i = 0; i < 9; i++)
                        comPortOPLL.DeferredWriteData((byte)(0x30 + i), 64);
                    comPortOPLL.DeferredWriteData(0x36, 64);
                    comPortOPLL.DeferredWriteData(0x37, 64);
                    comPortOPLL.DeferredWriteData(0x38, 64);
                }
                comPortOPLL.FlushDeferredWriteData();
            }
            if (comPortOPNA2 != null)
            {
                comPortOPNA2.ClearDeferredWriteData();

                for (int i = 0; i < 3; i++)
                {
                    comPortOPNA2.DeferredWriteData(4, (byte)(0xB4 | i));
                    comPortOPNA2.DeferredWriteData(8, 0xC0);
                    comPortOPNA2.DeferredWriteData(12, (byte)(0xB4 | i));
                    comPortOPNA2.DeferredWriteData(16, 0xC0);
                }

                // disable LFO
                comPortOPNA2.DeferredWriteData(4, 0x22);
                comPortOPNA2.DeferredWriteData(8, 0x00);

                // disable timer & set channel 6 to normal mode
                comPortOPNA2.DeferredWriteData(4, 0x27);
                comPortOPNA2.DeferredWriteData(8, 0x00);

                // ALL KEY OFF
                comPortOPNA2.DeferredWriteData(4, 0x28);
                for (int i = 0; i < 3; i++)
                {
                    comPortOPNA2.DeferredWriteData(8, (byte)(0x00 | i));
                    comPortOPNA2.DeferredWriteData(8, (byte)(0x04 | i));
                }

                // disable DAC
                comPortOPNA2.DeferredWriteData(4, 0x2B);
                comPortOPNA2.DeferredWriteData(8, 0x00);

                for (int slot = 0; slot < 6; slot++)
                {
                    uint reg = (uint)(slot / 3) * 2;
                    Ym2612WriteData(0x28, 0, 0, (byte)(0x00 | (reg << 1) | (byte)(slot % 3)));

                    //TL
                    if (volumeOff)
                        for (int op = 0; op < 4; op++)
                            Ym2612WriteData(0x40, op, slot, 127);
                }
                comPortOPNA2.FlushDeferredWriteData();
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
            comPortOPNA2?.DeferredWriteData((byte)((1 + (yreg + 0)) * 4), (byte)(address + (op * 4) + (slot % 3)));
            comPortOPNA2?.DeferredWriteData((byte)((1 + (yreg + 1)) * 4), data);
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
                    case 3:
                        comPortDCSG = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
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
                    case 2:
                        comPortOPNA2 = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low,
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
                    vgmReader = new BinaryReader(stream);
                    zipped = true;
                }
                else
                {
                    FileSize = (uint)vgmFile.Length;
                    vgmReader = new BinaryReader(vgmFile);
                }

                uint fccHeader;
                fccHeader = (uint)vgmReader.ReadUInt32();
                if (fccHeader != FCC_VGM)
                {
                    throw new IOException("VGM file error");
                }

                vgmDataLen = FileSize;
                vgmHead = readVGMHeader(vgmReader);

                //Figure out header offset
                offset = (int)vgmHead.lngDataOffset;
                if (offset == 0 || offset == 0x0000000C)
                    offset = 0x40;
                vgmDataOffset = offset;
            }
            using (FileStream vgmFile = File.Open(fileName, FileMode.Open))
            {
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
                vgmData = vgmReader.ReadBytes((int)(FileSize - offset));

                vgmReader = new BinaryReader(new MemoryStream(vgmData));
            }
            return true;
        }

        private int readByte()
        {
            if (vgmReader.BaseStream == null)
                return -1;
            if (vgmReader.BaseStream.Position == vgmReader.BaseStream.Length)
                return -1;

            byte data = vgmReader.ReadByte();
            return data;
        }

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
                                    case 0x4F:
                                        {
                                            var data = readByte();
                                            if (data < 0)
                                                break;
                                        }
                                        break;

                                    case 0x50:  //PSG
                                        {
                                            var data = readByte();
                                            if (data < 0)
                                                break;
                                            switch (comPortDCSG?.SoundModuleType)
                                            {
                                                case VsifSoundModuleType.Genesis_FTDI:
                                                case VsifSoundModuleType.Genesis:
                                                case VsifSoundModuleType.Genesis_Low:
                                                    comPortDCSG?.DeferredWriteData(0x14, (byte)data);
                                                    break;
                                                case VsifSoundModuleType.SMS:
                                                    comPortDCSG?.DeferredWriteData(0xFF, (byte)data);
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
                                            comPortOPLL?.DeferredWriteData((byte)adrs, (byte)dt);
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
                                            if (adrs < 0x22 || adrs == 0x23 || adrs == 0x29 || (0x2c <= adrs && adrs < 0x30))
                                                break;
                                            comPortOPNA2?.DeferredWriteData(0x04, (byte)adrs);
                                            comPortOPNA2?.DeferredWriteData(0x08, (byte)dt);
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
                                            comPortOPNA2?.DeferredWriteData(0x0C, (byte)adrs);
                                            comPortOPNA2?.DeferredWriteData(0x10, (byte)dt);
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

                                    case 0xE0: //Seek to offset in PCM databank
                                        uint offset = vgmReader.ReadUInt32();
                                        dacOffset = (int)offset;
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
                                                dacDataOffset.Add(dacData.Count);
                                                dacDataLength.Add((int)size);
                                                if (size == 0)
                                                    dacData.AddRange(new byte[] { 0 });
                                                else
                                                    dacData.AddRange(vgmReader.ReadBytes((int)size));
                                            }
                                            //_vgmReader.BaseStream.Position += size;
                                        }
                                        break;
                                    case 0x66:
                                        //End of song
                                        flushDeferredWriteData();
                                        if (Looped == false)
                                        {
                                            vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                            break;
                                        }
                                        else
                                        {
                                            if (vgmHead.lngLoopOffset != 0 && vgmDataOffset < vgmHead.lngLoopOffset)
                                                vgmReader.BaseStream?.Seek((vgmHead.lngLoopOffset - (vgmDataOffset)) /*+ 0x1C*/, SeekOrigin.Begin);
                                            else
                                                vgmReader.BaseStream?.Seek(0, SeekOrigin.Begin);
                                        }
                                        break;
                                    case 0x90: //Setup Stream Control:
                                        {
                                            //stream id
                                            var sid = readByte();
                                            //chip type
                                            var ct = readByte();
                                            if (ct == 2)    //YM2612
                                            {
                                                //port
                                                var port = readByte();
                                                //command
                                                var cmd = readByte();
                                                if (port == 0x00 && cmd == 0x2a)    //PCM
                                                {
                                                    comPortOPNA2?.DeferredWriteData(0x04, 0x2b);
                                                    comPortOPNA2?.DeferredWriteData(0x08, 0x80);

                                                    if (!streamTable.ContainsKey(sid))
                                                        streamTable.Add(sid, new StreamData(sid));
                                                }
                                            }
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

                                            comPortOPNA2?.DeferredWriteData(0x04, (byte)0x2a);
                                            comPortOPNA2?.DeferredWriteData(0x08, (byte)dacData[dacOffset]);
                                            dacOffset++;
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
                                if (Looped == false || LoopCount == 0)
                                {
                                    State = SoundState.Stopped;
                                    NotifyFinished();
                                    break;
                                }
                                LoopCount--;
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

                                    comPortOPNA2?.DeferredWriteData(0x04, (byte)0x2a);
                                    comPortOPNA2?.DeferredWriteData(0x08, data);

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
                        if (Looped == false || LoopCount == 0)
                        {
                            State = SoundState.Stopped;
                            NotifyFinished();
                            break;
                        }
                        LoopCount--;
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


        /// <summary>
        /// 
        /// </summary>
        private void flushDeferredWriteData()
        {
            comPortOPLL?.FlushDeferredWriteData();
            comPortDCSG?.FlushDeferredWriteData();
            comPortOPNA2?.FlushDeferredWriteData();
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

                vgmReader?.Dispose();

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
