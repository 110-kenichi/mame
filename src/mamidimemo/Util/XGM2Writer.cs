//#define DEBUG_PRINT
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.VSIF;
using System.IO;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Core;
using FastColoredTextBoxNS;
using System.Collections.ObjectModel;
using zanac.MAmidiMEmo.Properties;
using System.Xml.Linq;
using System.Windows.Interop;
using System.Windows.Documents;

namespace zanac.MAmidiMEmo.Util
{
    /// <summary>
    /// 
    /// </summary>
    public class XGM2Writer
    {

        private static object RecordingLock = new object();

        private MidiEvent currentMidiEvent;

        private long currentMidiEventTicks;

        private long previousMidiEventTicks;

        private string f_OutputFileName;

        //private int recordDataCommandType;

        private long lastWriteTicks;

#if DEBUG_PRINT
        private bool startXGM;

        private StringBuilder sb = new StringBuilder();
#endif

        private string f_OutputDir;

        private List<PortWriteData> f_RecordingData;

        private Boolean f_RecordingEnabled;

        /// <summary>
        /// 
        /// </summary>
        private YM2612 targetYM2612;

        /// <summary>
        /// 
        /// </summary>
        private SN76496 targetSN76496;

        public static event EventHandler RecodingStarted;

        public static event EventHandler RecodingStopped;

        /// <summary>
        /// 
        /// </summary>
        public XGM2Writer()
        {

        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceFrequency(out long frequency);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputDir"></param>
        public void RecordStart(string outputDir, uint unitNumber)
        {
            lock (RecordingLock)
            {
                uint unitnum = 0;

                var opn2s = InstrumentManager.GetInstruments((int)(InstrumentType.YM2612 + 1)).ToArray();
                var dcsgs = InstrumentManager.GetInstruments((int)(InstrumentType.SN76496 + 1)).ToArray();

                if (unitNumber < opn2s.Length)
                {
                    targetYM2612 = (YM2612)opn2s[unitNumber];
                    targetYM2612.Xgm2Writer?.RecordAbort();
                    targetYM2612.Xgm2Writer = this;
                    f_RecordingEnabled = true;
                }
                if (unitNumber < dcsgs.Length)
                {
                    targetSN76496 = (SN76496)dcsgs[unitNumber];
                    targetSN76496.Xgm2Writer?.RecordAbort();
                    targetSN76496.Xgm2Writer = this;
                    f_RecordingEnabled = true;
                }

                var now = DateTime.Now;
                string fname = $"MAmi_{unitnum}" + "_" + now.ToShortDateString().Replace('/', '-') + "_" + now.ToLongTimeString().Replace(':', '-');

                f_OutputFileName = $"{fname}_xgm2.xgm";

                f_OutputDir = outputDir;

                f_RecordingData = new List<PortWriteData>();
                previousMidiEventTicks = -1;
                currentMidiEventTicks = -1;
                lastWriteTicks = 0;
#if DEBUG_PRINT
                startXGM = false;
                sb = new StringBuilder();
#endif
                /*
                recordDataCommandType = -1;
                targetYM2612?.AllSoundOff();
                recordDataCommandType = -2;
                targetSN76496?.AllSoundOff();
                recordDataCommandType = 0;
                */
                RecodingStarted?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        public void SetCurrentProcessingMidiEvent(MidiEvent midiEvent)
        {
            currentMidiEvent = midiEvent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputDir"></param>
        public void RecordData(PortWriteData writeData)
        {
            lock (RecordingLock)
            {
                if (!f_RecordingEnabled)
                    return;

                //if (recordDataCommandType < 0)
                //    writeData.Command = recordDataCommandType;

#if DEBUG_PRINT
                if (writeData.Command == 0)
                    startXGM = true;
#endif

                if (currentMidiEvent != null)
                {
                    previousMidiEventTicks = currentMidiEventTicks;
                    currentMidiEventTicks = currentMidiEvent.DeltaTime;
                }

#if DEBUG_PRINT
                sb.AppendLine($"{currentProcessingMidiEventTicks} {previousProcessingMidiEventTicks}, {currentProcessingMidiEventTicks}");
                bool recalc = false;
#endif

                if (currentMidiEvent == null ||   //MIDIメッセージではない書き込み(LFOやポルタメントなど)
                    currentMidiEventTicks == 0)
                {
                    long count;
                    QueryPerformanceCounter(out count);
                    writeData.Tick = count;
                    lastWriteTicks = count;
#if DEBUG_PRINT
                    recalc = true;
#endif
                }
                else if (previousMidiEventTicks != currentMidiEventTicks)
                {
                    writeData.Tick = currentMidiEventTicks;
                    lastWriteTicks = currentMidiEventTicks;
                }
                else
                {
                    if (lastWriteTicks == 0)    //初回書き込み
                    {
                        long count;
                        QueryPerformanceCounter(out count);
                        writeData.Tick = count;
                        lastWriteTicks = count;
                    }
                    else
                    {
                        writeData.Tick = lastWriteTicks;
                    }
                }

#if DEBUG_PRINT
                if (startXGM)
                {
                    //Debug.WriteLine(writeData.ToString());
                    sb.AppendLine($"{recalc} {currentProcessingMidiEventTicks} {previousProcessingMidiEventTicks}, {writeData.ToString()}");
                    string s = sb.ToString();
                }
#endif

                f_RecordingData?.Add(writeData);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="framedata"></param>
        /// <param name="writingData"></param>
        /// <returns></returns>
        public int removeSameAddressData(List<PortWriteData> framedata, PortWriteData writingData)
        {
            for (int i = 0; i < framedata.Count; i++)
            {
                if (framedata[i].Type == writingData.Type && framedata[i].Address == writingData.Address)
                {
                    framedata.RemoveAt(i);
                    return i;
                }
            }
            return -1;
        }


        /// <summary>
        /// 
        /// </summary>
        public void RecordStop(bool endMark)
        {
            lock (RecordingLock)
            {
                if (!f_RecordingEnabled)
                    return;

                if (endMark)
                {
                    var pwd = new PortWriteData() { Type = (byte)0x7f, Address = 0, Data = 0 };
                    RecordData(pwd);
                    pwd = new PortWriteData() { Type = (byte)0x7f, Address = 0, Data = 0, Command = 1 };
                    RecordData(pwd);
                }

                string fileName = System.IO.Path.Combine(f_OutputDir, f_OutputFileName);
                List<PortWriteData> rd = f_RecordingData;

                this.f_RecordingEnabled = false;
                f_RecordingData = null;

                if (targetYM2612 != null)
                    targetYM2612.Xgm2Writer = null;
                if (targetSN76496 != null)
                    targetSN76496.Xgm2Writer = null;

                targetYM2612 = null;
                targetSN76496 = null;

                Thread t = new Thread(new ThreadStart(() =>
                {
                    List<byte> fileData = new List<byte>();

                    //https://github.com/Stephane-D/SGDK/blob/master/bin/xgm.txt
                    //File format (multi bytes value are in little endian format)
                    //Address            Size    Description
                    //$0000                      4    XGM2 file ident, should be "XGM2" (not present on compiled XGM)
                    fileData.AddRange(Encoding.ASCII.GetBytes("XGM2"));
                    //$0004                      1    Version information (0x10 currently)
                    fileData.Add(0x10);
                    //$0005                      1    Format description
                    // NTSC, No Multi Track, No GD3, No packed
                    fileData.Add(0x00);


                    Dictionary<Object, int> pcmIndexTable = new System.Collections.Generic.Dictionary<Object, int>();
                    Dictionary<int, Object> pcmDataTable = new System.Collections.Generic.Dictionary<int, Object>();

                    int pcmId = 0;
                    foreach (var writeData in rd)
                    {
                        if (writeData.Type == 6 && writeData.Data == 1) //Gather PCM Play command
                        {
                            if (!pcmIndexTable.ContainsKey(writeData.Tag["PcmData"]))
                            {
                                float gain = (float)writeData.Tag["PcmGain"];
                                if (gain != 1.0f)
                                {
                                    byte[] dacData = (byte[])((byte[])writeData.Tag["PcmData"]).Clone();
                                    for (int pi = 0; pi < dacData.Length; pi++)
                                    {
                                        int val = (int)Math.Round((float)(dacData[pi] - 0x80) * gain);
                                        if (val > sbyte.MaxValue)
                                            val = sbyte.MaxValue;
                                        else if (val < sbyte.MinValue)
                                            val = sbyte.MinValue;
                                        dacData[pi] = (byte)(val + 0x80);
                                    }
                                    pcmDataTable.Add(pcmId, dacData);
                                }
                                else
                                {
                                    pcmDataTable.Add(pcmId, writeData.Tag["PcmData"]);
                                }
                                pcmIndexTable.Add(writeData.Tag["PcmData"], pcmId++);
                            }
                        }
                        if (pcmId > 123)
                            break;
                    }


                    //$0006                      2    SLEN: Sample data bloc size / 256(ex: $0200 means 512 * 256 = 131072 bytes)
                    ushort pcmAddress = 0;
                    for (int pid = 0; pid < 123; pid++)
                    {
                        if (pcmDataTable.ContainsKey(pid))
                        {
                            byte[] pcmdata = (byte[])pcmDataTable[pid];

                            ushort size = (ushort)((pcmdata.Length + 255) / 256);
                            ushort adr = pcmAddress;
                            pcmAddress += size;
                        }
                    }
                    ushort sampleDataBlockSize = pcmAddress;
                    fileData.AddRange(BitConverter.GetBytes(sampleDataBlockSize));


                    //Optimize
                    long qpc;
                    QueryPerformanceFrequency(out qpc);
                    double tick1frame = qpc / (double)(Settings.Default.XgmTVSystem == 0 ? 60d : 50d);
                    double errorCorrection = qpc / (double)Settings.Default.XgmErrorCorrection;

                    double lastFmWaitTick = -1;
                    double lastPsgWaitTick = -1;

                    long loopFmAddress = -1;
                    long loopPsgAddress = -1;
                    List<PortWriteData> optWriteFmData = new List<PortWriteData>();
                    List<PortWriteData> optWritePsgData = new List<PortWriteData>();
                    {
                        List<PortWriteData> frame1FmData = new List<PortWriteData>();
                        List<PortWriteData> frame1PsgData = new List<PortWriteData>();
                        for (int i = 0; i < rd.Count; i++)
                        {
                            PortWriteData writeData = rd[i];
                            switch (writeData.Command)
                            {
                                case -2:
                                    //initial PSG commands
                                    break;
                                case -1:
                                    //initial FM commands
                                    break;
                                case 0:
                                    //normal FM write
                                    if (lastFmWaitTick < 0)
                                    {
                                        lastFmWaitTick = writeData.Tick;
                                    }
                                    else if (writeData.Tick - lastFmWaitTick >= tick1frame - errorCorrection)
                                    {
#if DEBUG
                                        if (writeData.Tick - lastFmWaitTick < tick1frame)
                                        {
                                            double diff = (writeData.Tick - lastFmWaitTick) / qpc;
                                        }
#endif
                                        //$00              1    frame wait (1/60 of second in NTSC, 1/50 of second in PAL)
                                        optWriteFmData.AddRange(frame1FmData.ToArray());
                                        frame1FmData.Clear();

                                        long wait = (long)(((writeData.Tick - lastFmWaitTick) + errorCorrection) / tick1frame);
                                        lastFmWaitTick += wait * tick1frame;
                                        //var mod = (writeData.Tick - lastWaitTick) % tick1frame;
                                        //lastWaitTick = writeData.Tick - mod;
                                        //lastWaitTick = writeData.Tick;
                                    }
                                    break;
                                case 1:
                                    //normal PSG write
                                    if (lastPsgWaitTick < 0)
                                    {
                                        lastPsgWaitTick = writeData.Tick;
                                    }
                                    else if (writeData.Tick - lastPsgWaitTick >= tick1frame - errorCorrection)
                                    {
#if DEBUG
                                        if (writeData.Tick - loopPsgAddress < tick1frame)
                                        {
                                            double diff = (writeData.Tick - loopPsgAddress) / qpc;
                                        }
#endif
                                        //$00              1    frame wait (1/60 of second in NTSC, 1/50 of second in PAL)
                                        optWritePsgData.AddRange(frame1PsgData.ToArray());
                                        frame1PsgData.Clear();

                                        long wait = (long)(((writeData.Tick - lastPsgWaitTick) + errorCorrection) / tick1frame);
                                        lastPsgWaitTick += wait * tick1frame;
                                        //var mod = (writeData.Tick - lastWaitTick) % tick1frame;
                                        //lastWaitTick = writeData.Tick - mod;
                                        //lastWaitTick = writeData.Tick;
                                    }
                                    break;
                            }
                            if (writeData.Command == 0 || writeData.Command == -1)
                            {
                                switch (writeData.Type)
                                {
                                    case 0:
                                        if (writeData.Address != 0x28)
                                        {
                                            //$2X data  1+2(X+1)    YM2612 port 0 register write:
                                            removeSameAddressData(frame1FmData, writeData);
                                            frame1FmData.Add(writeData);
                                        }
                                        else
                                        {
                                            //$4X data   1+(X+1)    YM2612 key off/on ($28) command write:
                                            frame1FmData.Add(writeData);
                                        }
                                        break;
                                    case 2:
                                        //$3X data  1+2(X+1)    YM2612 port 1 register write:
                                        removeSameAddressData(frame1FmData, writeData);
                                        frame1FmData.Add(writeData);
                                        break;

                                    case 6: //PCM
                                        removeSameAddressData(frame1FmData, writeData);
                                        frame1FmData.Add(writeData);
                                        break;
                                    case 0x7d: //LOOP START
                                        frame1FmData.Add(writeData);
                                        break;
                                    case 0x7e: //LOOP END
                                        frame1FmData.Add(writeData);
                                        break;
                                    case 0x7f: //END
                                        frame1FmData.Add(writeData);
                                        break;
                                }
                            }
                            else if (writeData.Command == 1 || writeData.Command == -2)
                            {
                                switch (writeData.Type)
                                {
                                    case 4: //DCSG 1 Freq
                                        removeSameAddressData(frame1PsgData, writeData);
                                        frame1PsgData.Add(writeData);
                                        break;
                                    case 5: //DCSG 2 Env/Vol
                                        removeSameAddressData(frame1PsgData, writeData);
                                        frame1PsgData.Add(writeData);
                                        break;
                                    case 0x7d: //LOOP START
                                        frame1PsgData.Add(writeData);
                                        break;
                                    case 0x7e: //LOOP END
                                        frame1PsgData.Add(writeData);
                                        break;
                                    case 0x7f: //END
                                        frame1PsgData.Add(writeData);
                                        break;
                                }
                            }
                        }
                                optWriteFmData.AddRange(frame1FmData.ToArray());
                                optWritePsgData.AddRange(frame1PsgData.ToArray());
                    }//*/

                    List<byte> fmdata = new List<byte>();
                    lastFmWaitTick = -1;
                    for (int i = 0; i < optWriteFmData.Count; i++)
                    {
                        PortWriteData writeData = optWriteFmData[i];

                        switch (writeData.Command)
                        {
                            case -1:
                                //initial FM commands
                                break;
                            case 0:
                                //normal FM write
                                if (lastFmWaitTick < 0)
                                {
                                    lastFmWaitTick = writeData.Tick;
                                }
                                else if (writeData.Tick - lastFmWaitTick >= tick1frame - errorCorrection)
                                {
#if DEBUG
                                    if (writeData.Tick - lastFmWaitTick < tick1frame)
                                    {
                                        double diff = (writeData.Tick - lastFmWaitTick) / qpc;
                                    }
#endif
                                    //$00              1    frame wait (1/60 of second in NTSC, 1/50 of second in PAL)
                                    long wait = (long)(((writeData.Tick - lastFmWaitTick) + errorCorrection) / tick1frame);
                                    for (long w = 0; w < wait; w++)
                                        fmdata.Add(0x00);

                                    lastFmWaitTick += wait * tick1frame;
                                    //var mod = (writeData.Tick - lastWaitTick) % tick1frame;
                                    //lastWaitTick = writeData.Tick - mod;
                                    //lastWaitTick = writeData.Tick;
                                }
                                break;
                        }

                        switch (writeData.Type)
                        {
                            case 0:
                                //$Ex data  1+2(x+1)    YM2612 general register write
                                //YM2612 port 0 register write:
                                fmdata.Add(0xE0);
                                fmdata.Add(writeData.Address);
                                fmdata.Add(writeData.Data);
                                break;
                            case 2:
                                //$Ex data  1+2(x+1)    YM2612 general register write
                                //YM2612 port 1 register write:
                                fmdata.Add(0xE8);
                                fmdata.Add(writeData.Address);
                                fmdata.Add(writeData.Data);
                                break;
                            case 6: //PCM
                                    //$1x id           2    PCM play command
                                if (writeData.Data == 1)
                                {
                                    if (pcmIndexTable.ContainsKey(writeData.Tag["PcmData"]))
                                    {
                                        fmdata.Add((byte)(0x10 + (writeData.Address & 0x3)));
                                        fmdata.Add((byte)(pcmIndexTable[writeData.Tag["PcmData"]] + 1));
                                    }
                                }
                                else
                                {
                                    fmdata.Add((byte)(0x10 + (writeData.Address & 0x3)));
                                    fmdata.Add((byte)0);
                                }
                                break;
                            case 0x7d: //LOOP START
                                loopFmAddress = fmdata.Count;
                                break;
                            case 0x7e: //LOOP END
                                       //$FF dddddd       4    Loop / end command, always located at the end the music data block.
                                fmdata.Add(0xFF);
                                if (loopFmAddress >= 0)
                                {
                                    byte[] loopadrs = BitConverter.GetBytes(loopFmAddress);
                                    fmdata.Add(loopadrs[0]);
                                    fmdata.Add(loopadrs[1]);
                                    fmdata.Add(loopadrs[2]);
                                }
                                i = optWriteFmData.Count;
                                break;
                            case 0x7f: //END
                                       //$FF dddddd       4    Loop / end command, always located at the end the music data block.
                                fmdata.Add(0xFF);
                                //-1
                                fmdata.Add(0xFF);
                                fmdata.Add(0xFF);
                                fmdata.Add(0xFF);
                                i = optWriteFmData.Count;
                                break;
                        }
                    }
                    //$0008                      2    FMLEN: FM music data block size / 256 (ex: $0040 means 64*256 = 16384 bytes)
                    ushort fmLen = (ushort)((fmdata.Count + 255) / 256);
                    fileData.AddRange(BitConverter.GetBytes(fmLen));
                    for (int i = fmdata.Count; i < fmLen * 256; i++)
                        fmdata.Add(0);

                    List<byte> psgdata = new List<byte>();
                    lastPsgWaitTick = -1;
                    for (int i = 0; i < optWritePsgData.Count; i++)
                    {
                        PortWriteData writeData = optWritePsgData[i];

                        switch (writeData.Command)
                        {
                            case -2:
                                //initial PSG commands
                                break;
                            case 1:
                                //normal PSG write
                                if (lastPsgWaitTick < 0)
                                {
                                    lastPsgWaitTick = writeData.Tick;
                                }
                                else if (writeData.Tick - lastPsgWaitTick >= tick1frame - errorCorrection)
                                {
#if DEBUG
                                    if (writeData.Tick - lastPsgWaitTick < tick1frame)
                                    {
                                        double diff = (writeData.Tick - lastPsgWaitTick) / qpc;
                                    }
#endif
                                    //$00              1    frame wait (1/60 of second in NTSC, 1/50 of second in PAL)
                                    long wait = (long)(((writeData.Tick - lastPsgWaitTick) + errorCorrection) / tick1frame);
                                    for (long w = 0; w < wait; w++)
                                        psgdata.Add(0x00);

                                    lastPsgWaitTick += wait * tick1frame;
                                    //var mod = (writeData.Tick - lastWaitTick) % tick1frame;
                                    //lastWaitTick = writeData.Tick - mod;
                                    //lastWaitTick = writeData.Tick;
                                }
                                break;
                        }

                        switch (writeData.Type)
                        {
                            case 4: //DCSG 1 Freq
                                psgdata.Add((byte)(0x20 | writeData.Address));
                                psgdata.Add((byte)writeData.Data);
                                break;
                            case 5: //DCSG 2 Env/Vol
                                psgdata.Add((byte)(0x80 | (writeData.Address << 4) | writeData.Data));
                                break;
                            case 0x7d: //LOOP START
                                loopPsgAddress = psgdata.Count;
                                break;
                            case 0x7e: //LOOP END
                                //$0F dddddd       4    Loop / end command, always located at the end the music data block.
                                psgdata.Add(0x0F);
                                if (loopPsgAddress >= 0)
                                {
                                    byte[] loopadrs = BitConverter.GetBytes(loopPsgAddress);
                                    psgdata.Add(loopadrs[0]);
                                    psgdata.Add(loopadrs[1]);
                                    psgdata.Add(loopadrs[2]);
                                }
                                i = optWritePsgData.Count;
                                break;
                            case 0x7f: //END
                                //$0F dddddd       4    Loop / end command, always located at the end the music data block.
                                psgdata.Add(0x0F);
                                //-1
                                psgdata.Add(0xFF);
                                psgdata.Add(0xFF);
                                psgdata.Add(0xFF);
                                i = optWritePsgData.Count;
                                break;
                        }
                    }
                    //$000A                      2    PSGLEN: PSG music data block size / 256(ex: $0020 means 32 * 256 = 8192 bytes)
                    ushort psgLen = (ushort)((psgdata.Count + 255) / 256);
                    fileData.AddRange(BitConverter.GetBytes(psgLen));
                    for (int i = psgdata.Count; i < psgLen * 256; i++)
                        psgdata.Add(0);


                    //$0X04                   SLEN    SDAT: Sample data block, contains all sample data (8 bits signed format)
                    pcmAddress = 0;
                    List<byte> pcmDataBlock = new List<byte>();
                    bool firstEmptyPid = true;
                    for (int pid = 0; pid < 124; pid++)
                    {
                        if (pcmDataTable.ContainsKey(pid))
                        {
                            byte[] pcmdata = (byte[])pcmDataTable[pid];

                            ushort size = (ushort)((pcmdata.Length / 256) + 1);
                            ushort adr = pcmAddress;
                            fileData.AddRange(BitConverter.GetBytes(adr));
                            pcmAddress += size;

                            for (int pi = 0; pi < size * 256; pi++)
                            {
                                if (pi < pcmdata.Length)
                                    pcmDataBlock.Add((byte)((int)pcmdata[pi] - 0x80));
                                else
                                    pcmDataBlock.Add(0);
                            }
                        }
                        else
                        {
                            if (firstEmptyPid)
                            {
                                firstEmptyPid = false;
                                fileData.AddRange(BitConverter.GetBytes(pcmAddress));
                            }
                            else
                            {
                                ushort adr = 0xffff;
                                fileData.AddRange(BitConverter.GetBytes(adr));
                            }
                        }
                    }

                    //$0X04                   SLEN    SDAT: Sample data block, contains all sample data (8 bits signed format)
                    //The size of this block is variable and is determined by the SLEN field.
                    //If field SLEN = $0000 the bloc is empty and so completely ignored.
                    //As explained in the 'Sample id table' field, sample size is aligned to 256 bytes so is the block size.
                    fileData.AddRange(pcmDataBlock.ToArray());

                    //$0X04+SLEN              FMLEN   FMDAT: FM music data block. It contains the XGM FM music data (see the XGM FM command description below).
                    fileData.AddRange(fmdata.ToArray());

                    //$0X04+SLEN+FMLEN        PSGLEN  PSGDAT: PSG music data block. It contains the XGM PSG music data (see the XGM PSG command description below).
                    fileData.AddRange(psgdata.ToArray());

                    //Output
                    using (FileStream xgm = new FileStream(fileName, FileMode.CreateNew))
                        xgm.Write(fileData.ToArray(), 0, fileData.Count);

                }));
                t.Start();

                RecodingStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void RecordAbort()
        {
            if (targetYM2612 != null)
                targetYM2612.Xgm2Writer = null;
            if (targetSN76496 != null)
                targetSN76496.Xgm2Writer = null;

            targetYM2612 = null;
            targetSN76496 = null;

            RecodingStopped?.Invoke(this, EventArgs.Empty);
        }

    }
}
