// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.MusicTheory;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using static zanac.MAmidiMEmo.Instruments.Chips.SP0256;

//https://www.dexsilicium.com/tms5220.pdf   
//http://www.stuartconner.me.uk/ti_portable_speech_lab/ti_portable_speech_lab.htm

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class TMS5220 : InstrumentBase
    {

        public override string Name => "TMS5220";

        public override string Group => "VOICE";

        public override InstrumentType InstrumentType => InstrumentType.TMS5220;

        [Browsable(false)]
        public override string ImageKey => "TMS5220";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "tms5220_";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        [IgnoreDataMember]
        [JsonIgnore]
        public override uint DeviceID
        {
            get
            {
                return 24;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TimbreBase[] BaseTimbres
        {
            get
            {
                return Timbres;
            }
            set
            {
                Timbres = (TMS5220Timbre[])value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(TimbresArrayUITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public TMS5220Timbre[] Timbres
        {
            get;
            set;
        }


        private const float DEFAULT_GAIN = 0.5f;

        public override bool ShouldSerializeGainLeft()
        {
            return GainLeft != DEFAULT_GAIN;
        }

        public override void ResetGainLeft()
        {
            GainLeft = DEFAULT_GAIN;
        }

        public override bool ShouldSerializeGainRight()
        {
            return GainRight != DEFAULT_GAIN;
        }

        public override void ResetGainRight()
        {
            GainRight = DEFAULT_GAIN;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<Beep>(serializeData))
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad"}), obj);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(Exception))
                    throw;
                else if (ex.GetType() == typeof(SystemException))
                    throw;


                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_tms5220_data_w(uint unitNumber, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_tms5220_data_w Tms5220_data_w
        {
            get;
            set;
        }

        private Queue<byte> commandDataQueue = new Queue<byte>();

        /// <summary>
        /// 
        /// </summary>
        private void Tms5220DataW(byte data)
        {
            lock (fifoQueue)
            {
                fifoQueue.Enqueue(() =>
                {
                    while (true)
                    {
                        try
                        {
                            Program.SoundUpdating();
                            if ((Tms5220_status_r(UnitNumber) & 0x70) != 0)
                            {
                                Tms5220_data_w(UnitNumber, data);
                                break;
                            }
                        }
                        finally
                        {
                            Program.SoundUpdated();
                        }
                        Thread.Sleep(0);
                    }
                });
            }
            fifoDataAdded.Set();

            //DeferredWriteData(Tms5220_data_w, unitNumber, data);
        }

        private void hardReset()
        {
            lock (fifoQueue)
                fifoQueue.Clear();
            try
            {
                Program.SoundUpdating();
                DeviceReset(UnitNumber, SoundInterfaceTagNamePrefix);
            }
            finally
            {
                Program.SoundUpdated();
            }
            fifoQueue.Enqueue(() =>
            {
                while (true)
                {
                    try
                    {
                        Program.SoundUpdating();
                        if ((Tms5220_status_r(UnitNumber) & 0x80) == 0)
                            break;
                    }
                    finally
                    {
                        Program.SoundUpdated();
                    }
                    Thread.Sleep(0);
                }
            });
        }

        Queue<Action> fifoQueue = new Queue<Action>();

        ManualResetEvent fifoDataAdded = new ManualResetEvent(false);
        ManualResetEvent terminateFifoThread = new ManualResetEvent(false);
        ManualResetEvent waitingFifoData = new ManualResetEvent(false);

        Thread processingThread;

        private void processFifoQueue()
        {
            while (true)
            {
                waitingFifoData.Set();

                int idx = ManualResetEvent.WaitAny(new WaitHandle[] { fifoDataAdded, terminateFifoThread });
                if (idx == 1)
                {
                    lock (fifoQueue)
                        fifoQueue.Clear();
                    return;
                }

                fifoDataAdded.Reset();
                waitingFifoData.Reset();

                while (true)
                {
                    Action action = null;
                    lock (fifoQueue)
                    {
                        if (fifoQueue.Count() == 0)
                            break;
                        action = fifoQueue.Dequeue();
                    }
                    action();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int delegate_tms5220_status_r(uint unitNumber);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_tms5220_status_r Tms5220_status_r
        {
            get;
            set;
        }

        /**********************************************************************************************

     update_fifo_status_and_ints -- check to see if the various flags should be on or off
     Description of flags, and their position in the status register:
      From the data sheet:
        bit D0(bit 7) = TS - Talk Status is active (high) when the VSP is processing speech data.
                Talk Status goes active at the initiation of a Speak command or after nine
                bytes of data are loaded into the FIFO following a Speak External command. It
                goes inactive (low) when the stop code (Energy=1111) is processed, or
                immediately by a buffer empty condition or a reset command.
        bit D1(bit 6) = BL - Buffer Low is active (high) when the FIFO buffer is more than half empty.
                Buffer Low is set when the "Last-In" byte is shifted down past the half-full
                boundary of the stack. Buffer Low is cleared when data is loaded to the stack
                so that the "Last-In" byte lies above the half-full boundary and becomes the
                eighth data byte of the stack.
        bit D2(bit 5) = BE - Buffer Empty is active (high) when the FIFO buffer has run out of data
                while executing a Speak External command. Buffer Empty is set when the last bit
                of the "Last-In" byte is shifted out to the Synthesis Section. This causes
                Talk Status to be cleared. Speech is terminated at some abnormal point and the
                Speak External command execution is terminated.

***********************************************************************************************/

        /// <summary>
        /// 
        /// </summary>
        private int Tms5220StatusR(uint unitNumber)
        {
            try
            {
                Program.SoundUpdating();

                return Tms5220_status_r(unitNumber);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static TMS5220()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("tms5220_data_w");
            if (funcPtr != IntPtr.Zero)
            {
                Tms5220_data_w = (delegate_tms5220_data_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_tms5220_data_w));
            }
            funcPtr = MameIF.GetProcAddress("tms5220_status_r");
            if (funcPtr != IntPtr.Zero)
            {
                Tms5220_status_r = (delegate_tms5220_status_r)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_tms5220_status_r));
            }
        }

        private TMS5220SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public TMS5220(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new TMS5220Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new TMS5220Timbre();
            setPresetInstruments();

            this.soundManager = new TMS5220SoundManager(this);

            processingThread = new Thread(new ThreadStart(processFifoQueue));
            processingThread.IsBackground = true;
            processingThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();

            terminateFifoThread.Set();
            processingThread.Join();

            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].ToneType = ToneType.Custom;
            Timbres[0].CustomLpcData =
                "0 02 00 12 06 03 06\r\n" +
                "0 04 00 12 08 04 08\r\n" +
                "0 05 00 13 10 06 09\r\n" +
                "0 12 35 21 12 07 05 10 05 11 5 1 3\r\n" +
                "0 14 36 20 11 10 07 10 03 11 6 2 3\r\n" +
                "0 13 36 18 11 13 10 09 02 09 6 3 3\r\n" +
                "0 11 36 16 12 15 10 07 01 09 6 2 4\r\n" +
                "0 08 34 14 17 15 10 08 01 06 4 2 5\r\n" +
                "0 07 30 15 07 15 10 07 05 05 2 3 4\r\n" +
                "0 07 28 15 04 15 11 08 04 06 3 3 5\r\n" +
                "0 05 26 15 15 15 12 09 07 06 3 3 4\r\n" +
                "0 01 00 09 09 11 08\r\n" +
                "0 01 00 11 03 05 08\r\n" +
                "0 01 00 09 04 07 08\r\n" +
                "0 01 00 09 04 07 08\r\n" +
                "0 01 00 09 05 08 08\r\n" +
                "0 03 00 00 09 11 10\r\n" +
                "0 02 00 00 07 10 10";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(TaggedNoteOnEvent midiEvent)
        {
            soundManager.ProcessKeyOn(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOffEvent(NoteOffEvent midiEvent)
        {
            soundManager.ProcessKeyOff(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            base.OnControlChangeEvent(midiEvent);

            soundManager.ProcessControlChange(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataMsb"></param>
        /// <param name="dataLsb"></param>
        protected override void OnNrpnDataEntered(ControlChangeEvent dataMsb, ControlChangeEvent dataLsb)
        {
            base.OnNrpnDataEntered(dataMsb, dataLsb);

            soundManager.ProcessNrpnData(dataMsb, dataLsb);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caft"></param>
        protected override void OnChannelAfterTouchEvent(ChannelAftertouchEvent caft)
        {
            base.OnChannelAfterTouchEvent(caft);

            soundManager.ProcessChannelAftertouch(caft);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnPitchBendEvent(PitchBendEvent midiEvent)
        {
            base.OnPitchBendEvent(midiEvent);

            soundManager.ProcessPitchBend(midiEvent);
        }

        internal override void AllSoundOff()
        {
            soundManager.ProcessAllSoundOff();
        }

        /// <summary>
        /// 
        /// </summary>
        private class TMS5220SoundManager : SoundManagerBase
        {
            private static SoundList<SoundBase> allSound = new SoundList<SoundBase>(-1);

            /// <summary>
            /// 
            /// </summary>
            protected override SoundList<SoundBase> AllSounds
            {
                get
                {
                    return allSound;
                }
            }

            private static SoundList<TMS5220Sound> psgOnSounds = new SoundList<TMS5220Sound>(1);

            private TMS5220 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public TMS5220SoundManager(TMS5220 parent) : base(parent)
            {
                this.parentModule = parent;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override SoundBase[] SoundOn(TaggedNoteOnEvent note)
            {
                List<SoundBase> rv = new List<SoundBase>();

                int tindex = 0;
                foreach (TMS5220Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    TMS5220Sound snd = new TMS5220Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    psgOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn SP0256 ch" + emptySlot + " " + note.ToString());
                    rv.Add(snd);
                }
                for (int i = 0; i < rv.Count; i++)
                {
                    var snd = rv[i];
                    if (!snd.IsDisposed)
                    {
                        ProcessKeyOn(snd);
                    }
                    else
                    {
                        rv.Remove(snd);
                        i--;
                    }
                }

                return rv.ToArray();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private (TMS5220 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 1);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                parentModule.hardReset();
            }
        }

        private class BitWriter
        {
            private TMS5220 parentModule;

            private int writeData;

            private int currentBitPos;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            public BitWriter(TMS5220 parentModule)
            {
                this.parentModule = parentModule;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="bitLength"></param>
            public void WriteBits(int data, int bitLength)
            {
                for (int i = bitLength - 1; i >= 0; i--)
                {
                    writeData |= ((data >> i) & 1) << currentBitPos;
                    currentBitPos++;
                    if (currentBitPos == 8)
                    {
                        parentModule.Tms5220DataW((byte)writeData);

                        //Debug.Write(writeData + " ");

                        currentBitPos = 0;
                        writeData = 0;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class TMS5220Sound : SoundBase
        {

            private TMS5220 parentModule;

            private TMS5220Timbre timbre;

            private BitWriter bw;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public TMS5220Sound(TMS5220 parentModule, TMS5220SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (TMS5220Timbre)timbre;

                bw = new BitWriter(parentModule);
            }
            
            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                parentModule.hardReset();

                //START
                parentModule.Tms5220DataW((byte)(0x60));

                // Set freq
                if (timbre.BaseFreqency != 0)
                {
                    parentModule.SetClock(parentModule.UnitNumber,
                        (uint)Math.Round(640000 * (CalcCurrentFrequency() / timbre.BaseFreqency)));
                }

                // Send commands
                if (timbre.ToneType == ToneType.Custom)
                {
                    var lpcd = timbre.RawCustomLpcData;
                    if (lpcd != null)
                    {
                        foreach (var line in lpcd)
                            sendCommand(line);
                    }
                }
                else
                {
                    var lpcd = timbre.PresetLpcData;
                    if (lpcd != null)
                        foreach (var data in lpcd)
                            parentModule.Tms5220DataW(data);
                }

                //STOP
                sendCommand(0, 15, 0, 0, 0, 0, 0);
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="arg"></param>
            private void sendCommand(params byte[] arg)
            {
                // NRG
                bw.WriteBits(arg[1] & 0xf, 4); //4         1

                if (arg[1] == 0xf || arg[1] == 0x0)
                    return;

                // RPT
                bw.WriteBits(arg[0] & 1, 1);   //5

                // PITCH
                bw.WriteBits(arg[2] & 63, 6);//8,13      2

                if ((arg[0] & 1) == 1)
                    return;

                // K1
                bw.WriteBits(arg[3] & 31, 5);   //16        3
                // K2
                bw.WriteBits(arg[4] & 31, 5);   //21
                // K3
                bw.WriteBits(arg[5] & 15, 4);   //24,25     4
                // K4
                bw.WriteBits(arg[6] & 15, 4);   //29

                if (arg[2] == 0)
                    return;

                // K5
                bw.WriteBits(arg[7] & 15, 4);   //32,33     5
                // K6
                bw.WriteBits(arg[8] & 15, 4);   //37
                // K7
                bw.WriteBits(arg[9] & 15, 4);   //40,41     6
                // K8
                bw.WriteBits(arg[10] & 7, 3);    //44
                // K9
                bw.WriteBits(arg[11] & 7, 3);    //47
                // K10
                bw.WriteBits(arg[12] & 7, 3);   //48, 50    7
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                if (timbre.BaseFreqency != 0)
                {
                    parentModule.SetClock(parentModule.UnitNumber,
                        (uint)Math.Round(640000 * (CalcCurrentFrequency() / timbre.BaseFreqency)));
                }
                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                parentModule.hardReset();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<TMS5220Timbre>))]
        [DataContract]
        [InstLock]
        public class TMS5220Timbre : TimbreBase
        {

            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(ToneType.Custom)]
            public ToneType ToneType
            {
                get;
                set;
            }

            [DataMember]
            [TypeConverter(typeof(OpenEditorTypeConverter))]
            [Category("Sound")]
            [Editor(typeof(LpcUITypeEditor), typeof(UITypeEditor)), Localizable(false)]
            [DefaultValue(null)]
            public byte[] PresetLpcData
            {
                get;
                set;
            }

            private string customLpcData;

            [DataMember]
            [Category("Sound")]
            [Editor(typeof(FormTextUITypeEditor), typeof(UITypeEditor)), Localizable(false)]
            [TypeConverter(typeof(OpenEditorTypeConverter))]
            [Description("Set LPC Data like the following format.\r\n" +
                "RPT(0-1) NRG(1-14) PITCH(0-63) K1(0-31) K2(0-31) K3(0-15) K4(0-15) (K5(0-15) K6(0-15) K7(0-15) K8(0-7) K9(0-7) K10(0-7))\r\n" +
                "\r\n" +
                "e.g)" +
                "0 02 00 12 06 03 06\r\n" +
                "0 13 36 18 11 13 10 09 02 09 6 3 3")]
            [DefaultValue(null)]
            public string CustomLpcData
            {
                get
                {
                    return customLpcData;
                }
                set
                {
                    if (customLpcData != value)
                    {
                        List<byte[]> data = new List<byte[]>();
                        if (value != null)
                        {
                            using (StringReader sr = new StringReader(value))
                            {
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    var tps = line.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                                    List<byte> ps = new List<byte>();
                                    for (int i = 0; i < tps.Length; i++)
                                    {
                                        byte val = 0;
                                        byte.TryParse(tps[i], out val);
                                        {
                                            switch (i)
                                            {
                                                case 0:
                                                    // RPT
                                                    byte rpt = (byte)(val & 1);
                                                    ps.Add(rpt);
                                                    break;
                                                case 1:
                                                    // NRG
                                                    byte nrg = (byte)(val & 0xf);
                                                    ps.Add(nrg);
                                                    if (nrg == 0xf || nrg == 0x0)
                                                        i = tps.Length;
                                                    break;
                                                case 2:
                                                    // PITCH
                                                    byte pitch = (byte)(val & 63);
                                                    ps.Add(pitch);
                                                    if (ps[0] == 1) //RPT
                                                        i = tps.Length;
                                                    break;
                                                case 3:
                                                case 4:
                                                    // K1-2
                                                    byte k1_2 = (byte)(val & 31);
                                                    ps.Add(k1_2);
                                                    break;
                                                case 5:
                                                case 6:
                                                    // K3-4
                                                    byte k3_4 = (byte)(val & 15);
                                                    ps.Add(k3_4);
                                                    if (i == 6 && ps[2] == 0)   //PITCH
                                                        i = tps.Length;
                                                    break;
                                                case 7:
                                                case 8:
                                                case 9:
                                                    // K5-7
                                                    byte k5_7 = (byte)(val & 15);
                                                    ps.Add(k5_7);
                                                    break;
                                                case 10:
                                                case 11:
                                                case 12:
                                                    // K8-10
                                                    byte k8_10 = (byte)(val & 3);
                                                    ps.Add(k8_10);
                                                    break;
                                            }
                                        }
                                    }

                                    data.Add(ps.ToArray());
                                }
                            }
                        }

                        var sb = new StringBuilder();
                        foreach (byte[] lineb in data)
                        {
                            var line = new StringBuilder();
                            foreach (byte p in lineb)
                            {
                                if (line.Length != 0)
                                    line.Append(" ");
                                line.Append(p.ToString((IFormatProvider)null));
                            }
                            sb.AppendLine(line.ToString());
                        }
                        customLpcData = sb.ToString();

                        RawCustomLpcData = data.ToArray();
                    }
                }
            }

            [Browsable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            public byte[][] RawCustomLpcData
            {
                get;
                set;
            }

            [DataMember]
            [Category("Sound")]
            [Description("Set base frequency [Hz]")]
            [DefaultValue(typeof(double), "440")]
            [DoubleSlideParametersAttribute(100, 2000, 1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double BaseFreqency
            {
                get;
                set;
            } = 440;

            /// <summary>
            /// 
            /// </summary>
            public TMS5220Timbre()
            {
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<TMS5220Timbre>(serializeData);
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad"}), obj);
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ToneType
        {
            Preset,
            Custom,
        }


    }
}