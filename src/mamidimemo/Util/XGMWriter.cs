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

namespace zanac.MAmidiMEmo.Util
{
    /// <summary>
    /// 
    /// </summary>
    public class XGMWriter
    {

        private static object RecordingLock = new object();

        private MidiEvent currentProcessingMidiEvent;

        private long currentProcessingMidiEventTicks;

        private long previousProcessingMidiEventTicks;

        private string f_OutputFileName;

        private int recordDataCommandType;

        private long lastWriteTicks;

        //private bool startXGM;

        //private StringBuilder sb = new StringBuilder();

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
        public XGMWriter()
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
                    targetYM2612.XgmWriter = this;
                    f_RecordingEnabled = true;
                }
                if (unitNumber < dcsgs.Length)
                {
                    targetSN76496 = (SN76496)dcsgs[unitNumber];
                    targetSN76496.XgmWriter = this;
                    f_RecordingEnabled = true;
                }

                var now = DateTime.Now;
                string fname = $"MAmi_{unitnum}" + "_" + now.ToShortDateString().Replace('/', '-') + "_" + now.ToLongTimeString().Replace(':', '-');

                f_OutputFileName = $"{fname}.xgm";

                f_OutputDir = outputDir;

                f_RecordingData = new List<PortWriteData>();
                previousProcessingMidiEventTicks = 0;
                currentProcessingMidiEventTicks = 0;
                lastWriteTicks = 0;


                //startXGM = false;
                //sb = new StringBuilder();

                recordDataCommandType = -1;
                targetYM2612?.AllSoundOff();
                targetSN76496?.AllSoundOff();
                recordDataCommandType = 0;

                RecodingStarted?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        public void SetCurrentProcessingMidiEvent(MidiEvent midiEvent)
        {
            currentProcessingMidiEvent = midiEvent;
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

                writeData.Command = recordDataCommandType;

                //if (writeData.Command == 0)
                //    startXGM = true;

                if (currentProcessingMidiEvent != null)
                {
                    previousProcessingMidiEventTicks = currentProcessingMidiEventTicks;
                    currentProcessingMidiEventTicks = currentProcessingMidiEvent.DeltaTime;
                }

                //sb.AppendLine($"{CurrentProcessingMidiEventTicks} {PreviousProcessingMidiEventTicks}, {CurrentProcessingMidiEvent}");

                long lt = lastWriteTicks;
                //bool recalc = false;
                if (currentProcessingMidiEvent == null ||
                    currentProcessingMidiEventTicks == 0 || previousProcessingMidiEventTicks != currentProcessingMidiEventTicks ||
                    lt == 0)
                {
                    long count;
                    QueryPerformanceCounter(out count);
                    writeData.Tick = count;
                    lastWriteTicks = count;
                    //recalc = true;
                }
                else
                {
                    writeData.Tick = lt;
                }

                //if (startXGM)
                //{
                //    //Debug.WriteLine(writeData.ToString());
                //    sb.AppendLine($"{recalc} {CurrentProcessingMidiEventTicks} {PreviousProcessingMidiEventTicks}, {writeData.ToString()}");
                //    string s = sb.ToString();
                //}

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

                RecodingStopped?.Invoke(this, EventArgs.Empty);

                if (endMark)
                {
                    RecordData(new PortWriteData()
                    { Type = (byte)0x7f, Address = 0, Data = 0, Command = recordDataCommandType });
                }

                string fileName = System.IO.Path.Combine(f_OutputDir, f_OutputFileName);
                List<PortWriteData> rd = f_RecordingData;

                this.f_RecordingEnabled = false;
                f_RecordingData = null;

                targetYM2612 = null;
                targetSN76496 = null;

                Thread t = new Thread(new ThreadStart(() =>
                {
                    List<byte> wd = new List<byte>();

                    //https://github.com/Stephane-D/SGDK/blob/master/bin/xgm.txt
                    //File format (multi bytes value are in little endian format)
                    //Address            Size    Description
                    //$0000                 4    XGM file ident, should be "XGM "
                    wd.AddRange(Encoding.ASCII.GetBytes("XGM "));
                    //$0004               252    Sample id table.
                    //                           This table contain the address and the size for all sample (maximum = 63 samples)
                    //                           Each entry of the table consist of 4 bytes (2 bytes for address and 2 bytes for size):
                    //                               entry +$0: sample address / 256
                    //                               entry +$2: sample size / 256
                    //                             We don't need the low 8 bits information as each sample have its address and size aligned on 256 bytes.
                    //                             The sample address is relative to the start of the "Sample Data Bloc"(field $104).
                    //                             An empty entry should have its address set to $FFFF and size set to $0001.

                    Dictionary<Object, int> pcmIndexTable = new System.Collections.Generic.Dictionary<Object, int>();
                    Dictionary<int, Object> pcmDataTable = new System.Collections.Generic.Dictionary<int, Object>();

                    int pcmId = 0;
                    foreach (var writeData in rd)
                    {
                        if (writeData.Type == 6 && writeData.Data == 1) //Gather PCM Play command
                        {
                            if (!pcmIndexTable.ContainsKey(writeData.Tag))
                            {
                                pcmDataTable.Add(pcmId, writeData.Tag);
                                pcmIndexTable.Add(writeData.Tag, pcmId++);
                            }
                        }
                        if (pcmId > 63)
                            break;
                    }

                    ushort pcmAddress = 0;
                    List<byte> pcmDataBlock = new List<byte>();
                    for (int pid = 0; pid < 63; pid++)
                    {
                        if (pcmDataTable.ContainsKey(pid))
                        {
                            byte[] pcmdata = (byte[])pcmDataTable[pid];

                            ushort size = (ushort)((pcmdata.Length / 256) + 1);
                            ushort adr = pcmAddress;
                            wd.AddRange(BitConverter.GetBytes(adr));
                            wd.AddRange(BitConverter.GetBytes(size));
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
                            ushort adr = 0xffff;
                            ushort size = 0x1;
                            wd.AddRange(BitConverter.GetBytes(adr));
                            wd.AddRange(BitConverter.GetBytes(size));
                        }
                    }
                    //$0100                 2    Sample data bloc size / 256, ex: $0010 means 256*16 = 4096 bytes
                    //                           We will reference the value of this field as SLEN.
                    ushort sampleDataBlockSize = pcmAddress;
                    wd.AddRange(BitConverter.GetBytes(sampleDataBlockSize));
                    //$0102                 1    Version information (0x01 currently)                 
                    wd.Add(0x01);
                    //$0103                 1    bit #0: NTSC / PAL information: 0=NTSC  1=PAL
                    wd.Add(0x00);
                    //$0104              SLEN    Sample data bloc, contains all sample data (8 bits signed format)
                    //                           The size of this bloc is variable and is determined by the field $100.
                    //                           If field $100 contains $0000 the bloc is empty and the field is ignored.
                    //                           As explained in the 'Sample id table' field, sample size is aligned on 256 bytes
                    wd.AddRange(pcmDataBlock.ToArray());

                    int moffset = wd.Count;

                    List<byte> mdata = new List<byte>();

                    long f;
                    QueryPerformanceFrequency(out f);
                    double tick1frame = f / 60d;

                    double lastWaitTick = -1;

                    long loopAddress = -1;

                    //Optimize
                    //*
                    List<PortWriteData> optWriteData = new List<PortWriteData>();
                    {
                        List<PortWriteData> frame1Data = new List<PortWriteData>();
                        for (int i = 0; i < rd.Count; i++)
                        {
                            PortWriteData writeData = rd[i];
                            switch (writeData.Command)
                            {
                                case -1:
                                    //initial commands
                                    break;
                                case 0:
                                    //normal write
                                    if (lastWaitTick < 0)
                                    {
                                        lastWaitTick = writeData.Tick;
                                    }
                                    else if (writeData.Tick - lastWaitTick > tick1frame)
                                    {
                                        /*
                                        bool ignoreSplit = false;
                                        //Avoid 2byte data splitted by frame
                                        {
                                            //DCSG Freq
                                            if (writeData.Type == 4)
                                            {
                                                if (writeData.Address == 8 || writeData.Address == 10 || writeData.Address == 12)
                                                {
                                                    if (i + 1 < rd.Count)
                                                        if (rd[i + 1].Type == 5 && rd[i + 1].Address == writeData.Address)
                                                            ignoreSplit = true;
                                                }
                                            }
                                            //FM Freq
                                            if (writeData.Type == 0 && writeData.Type == 2)
                                            {
                                                if (0xa0 <= writeData.Address && writeData.Type <= 0xaf)
                                                {
                                                    if (i + 1 < rd.Count)
                                                        if (rd[i + 1].Type == writeData.Type && rd[i + 1].Address == writeData.Address - 4)
                                                            ignoreSplit = true;
                                                }
                                            }
                                        }
                                        if (ignoreSplit)
                                            break;
                                        */
                                        //$00              1    frame wait (1/60 of second in NTSC, 1/50 of second in PAL)
                                        optWriteData.AddRange(frame1Data.ToArray());
                                        frame1Data.Clear();

                                        var mod = (writeData.Tick - lastWaitTick) % tick1frame;
                                        if (mod >= tick1frame / 2)
                                            lastWaitTick = writeData.Tick + mod;
                                        else
                                            lastWaitTick = writeData.Tick - mod;
                                        //lastWaitTick = writeData.Tick;
                                    }
                                    break;
                            }
                            switch (writeData.Type)
                            {
                                case 0:
                                    if (writeData.Address != 0x28)
                                    {
                                        //$2X data  1+2(X+1)    YM2612 port 0 register write:
                                        removeSameAddressData(frame1Data, writeData);
                                        frame1Data.Add(writeData);
                                    }
                                    else
                                    {
                                        //$4X data   1+(X+1)    YM2612 key off/on ($28) command write:
                                        frame1Data.Add(writeData);
                                    }
                                    break;
                                case 2:
                                    //$3X data  1+2(X+1)    YM2612 port 1 register write:
                                    removeSameAddressData(frame1Data, writeData);
                                    frame1Data.Add(writeData);
                                    break;
                                case 4: //DCSG 1
                                    removeSameAddressData(frame1Data, writeData);
                                    frame1Data.Add(writeData);
                                    break;
                                case 5: //DCSG 2
                                    removeSameAddressData(frame1Data, writeData);
                                    frame1Data.Add(writeData);
                                    break;
                                case 6: //PCM
                                    removeSameAddressData(frame1Data, writeData);
                                    frame1Data.Add(writeData);
                                    break;
                                case 0x7d: //LOOP START
                                    frame1Data.Add(writeData);
                                    break;
                                case 0x7e: //LOOP END
                                    frame1Data.Add(writeData);
                                    break;
                                case 0x7f: //END
                                    frame1Data.Add(writeData);
                                    break;
                            }
                        }
                        optWriteData.AddRange(frame1Data.ToArray());
                    }//*/
                    //List<PortWriteData> optWriteData = new List<PortWriteData>(rd);

                    lastWaitTick = -1;
                    for (int i = 0; i < optWriteData.Count; i++)
                    {
                        PortWriteData writeData = optWriteData[i];

                        switch (writeData.Command)
                        {
                            case -1:
                                //initial commands
                                break;
                            case 0:
                                //normal write
                                if (lastWaitTick < 0)
                                {
                                    lastWaitTick = writeData.Tick;
                                }
                                else if (writeData.Tick - lastWaitTick > tick1frame)
                                {
                                    /*
                                    bool ignoreSplit = false;
                                    //Avoid 2byte data splitted by frame
                                    {
                                        //DCSG Freq
                                        if (writeData.Type == 4)
                                        {
                                            if (writeData.Address == 8 || writeData.Address == 10 || writeData.Address == 12)
                                            {
                                                if (i + 1 < optWriteData.Count)
                                                    if (optWriteData[i + 1].Type == 5 && optWriteData[i + 1].Address == writeData.Address)
                                                        ignoreSplit = true;
                                            }
                                        }
                                        //FM Freq
                                        if (writeData.Type == 0 || writeData.Type == 2)
                                        {
                                            if (0xa0 <= writeData.Address && writeData.Type <= 0xaf)
                                            {
                                                if (i + 1 < optWriteData.Count)
                                                    if (optWriteData[i + 1].Type == writeData.Type && optWriteData[i + 1].Address == writeData.Address - 4)
                                                        ignoreSplit = true;
                                            }
                                        }
                                    }
                                    if (ignoreSplit)
                                        break;
                                    */
                                    //$00              1    frame wait (1/60 of second in NTSC, 1/50 of second in PAL)
                                    long wait = (long)Math.Round((writeData.Tick - lastWaitTick) / tick1frame);
                                    for (long w = 0; w < wait; w++)
                                        mdata.Add(0x00);

                                    var mod = (writeData.Tick - lastWaitTick) % tick1frame;
                                    if (mod >= tick1frame / 2)
                                        lastWaitTick = writeData.Tick + mod;
                                    else
                                        lastWaitTick = writeData.Tick - mod;
                                    //lastWaitTick = writeData.Tick;
                                }
                                break;
                        }

                        switch (writeData.Type)
                        {
                            case 0:
                                if (writeData.Address != 0x28)
                                {
                                    //$2X data  1+2(X+1)    YM2612 port 0 register write:
                                    mdata.Add(0x20);
                                    mdata.Add(writeData.Address);
                                    mdata.Add(writeData.Data);
                                }
                                else
                                {
                                    //$4X data   1+(X+1)    YM2612 key off/on ($28) command write:
                                    mdata.Add(0x40);
                                    mdata.Add(writeData.Data);
                                }
                                break;
                            case 2:
                                //$3X data  1+2(X+1)    YM2612 port 1 register write:
                                mdata.Add(0x30);
                                mdata.Add(writeData.Address);
                                mdata.Add(writeData.Data);
                                break;
                            case 4: //DCSG 1
                                mdata.Add(0x10);
                                mdata.Add(((byte)(writeData.Address << 4 | writeData.Data)));
                                break;
                            case 5: //DCSG 2
                                mdata.Add(0x10);
                                mdata.Add((byte)writeData.Data);
                                break;
                            case 6: //PCM
                                if (writeData.Data == 1)
                                {
                                    if (pcmIndexTable.ContainsKey(writeData.Tag))
                                    {
                                        mdata.Add((byte)(0x50 + writeData.Address));
                                        mdata.Add((byte)(pcmIndexTable[writeData.Tag] + 1));
                                    }
                                    else
                                    {
                                        mdata.Add((byte)(0x50 + writeData.Address));
                                        mdata.Add((byte)0);
                                    }
                                }
                                break;
                            case 0x7d: //LOOP START
                                loopAddress = mdata.Count;
                                break;
                            case 0x7e: //LOOP END
                                //$7E              1    End command (end of music data).
                                mdata.Add(0x7e);
                                if (loopAddress >= 0)
                                {
                                    byte[] loopadrs = BitConverter.GetBytes(loopAddress);
                                    mdata.Add(loopadrs[0]);
                                    mdata.Add(loopadrs[1]);
                                    mdata.Add(loopadrs[2]);
                                }
                                //$7F              1    End command (end of music data).
                                mdata.Add(0x7f);
                                i = optWriteData.Count;
                                break;
                            case 0x7f: //END
                                //$7F              1    End command (end of music data).
                                mdata.Add(0x7f);
                                i = optWriteData.Count;
                                break;
                        }
                    }

                    //$0104+SLEN            4    Music data bloc size.
                    //                           We will reference the value of this field as MLEN.
                    //                           This fields may be used later to quickly browse multi track XGM file.
                    uint mlen = (uint)(mdata.Count);
                    wd.AddRange(BitConverter.GetBytes(mlen));

                    //$0108 + SLEN               MLEN Music data bloc. It contains the XGM music data(see the XGM command description below).
                    wd.AddRange(mdata.ToArray());

                    //Output
                    FileStream xgm = new FileStream(fileName, FileMode.CreateNew);
                    xgm.Write(wd.ToArray(), 0, wd.Count);

                }));
                t.Start();

                if (targetYM2612 != null)
                    targetYM2612.XgmWriter = null;
                if (targetSN76496 != null)
                    targetSN76496.XgmWriter = null;
            }
        }

    }
}
