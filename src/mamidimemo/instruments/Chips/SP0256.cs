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
using zanac.MAmidiMEmo.Properties;

//http://spatula-city.org/~im14u2c/sp0256-al2/Archer_SP0256-AL2.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class SP0256 : InstrumentBase
    {

        public override string Name => "SP0256";

        public override string Group => "VOICE";

        public override InstrumentType InstrumentType => InstrumentType.SP0256;

        [Browsable(false)]
        public override string ImageKey => "SP0256";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "sp0256_";

        /// <summary>
        /// 
        /// </summary>
        private static readonly string[] AllophoneTable = new string[]
        {
                "PA1",
                "PA2",
                "PA3",
                "PA4",
                "PA5",
                "OY",
                "AY",
                "EH",
                "KK3",
                "PP",
                "JH",
                "NN1",
                "IH",
                "TT2",
                "RR1",
                "AX",
                "MM",
                "TT1",
                "DH1",
                "IY",
                "EY",
                "DD1",
                "UW1",
                "AO",
                "AA",
                "YY2",
                "AE",
                "HH1",
                "BB1",
                "TH",
                "UH",
                "UW2",
                "AW",
                "DD2",
                "GG3",
                "VV",
                "GG1",
                "SH",
                "ZH",
                "RR2",
                "FF",
                "KK2",
                "KK1",
                "ZZ",
                "NG",
                "LL",
                "WW",
                "XR",
                "WH",
                "YY1",
                "CH",
                "ER1",
                "ER2",
                "OW",
                "DH2",
                "SS",
                "NN2",
                "HH2",
                "OR",
                "AR",
                "YR",
                "GG2",
                "EL",
                "BB2"
        };

        private static Dictionary<string, byte> AllophoneDictionary;

        //https://github.com/nmacadam/SP0256-AL2-Lexicon/blob/master/SP0256-AL2 Lexicon/SP0256-AL2 Lexicon/Lexicon.cpp
        private static readonly string[] CmuTable = new string[]
        {
            "OY","OY",
            "OY0","OY",
            "OY1","OY",
            "OY2","OY",
            "AY","AY",
            "AY0","AY",
            "AY1","AY",
            "AY2","AY",
            "EH","EY",
            "EH0","EY",
            "EH1","EY",
            "EH2","EY",
            "K","KK1",
            //"K","KK3",
            "P","PP",
            "JH","JH",
            "N","NN1",
            //"N","NN2",
            "IH","IH",
            "IH0","IH",
            "IH1","IH",
            "IH2","IH",
            "T","TT1",
            //"T","TT2",
            "R","RR1",
            //"R","RR2",
            //"R","XR",
            "AH","AX",
            "AH0","AX",
            "AH1","AX",
            "AH2","AX",
            "M","MM",
            "DH","DH1",
            //"DH","DH2",
            "IY","IY",
            "IY0","IY",
            "IY1","IY",
            "IY2","IY",
            "EY","EY",
            "EY0","EY",
            "EY1","EY",
            "EY2","EY",
            "D","D1",
            //"D","D2",
            "UW","UW1",
            "UW0","UW1",
            "UW1","UW2",
            "UW2","UW2",
            //"UW","UW2",
            //"UW0","UW2",
            //"UW1","UW2",
            //"UW2","UW2",
            "AO","AO",
            "AO0","AO",
            "AO1","AO",
            "AO2","AO",
            "AA","AA",
            "AA0","AA",
            "AA1","AA",
            "AA2","AA",
            "Y","YY1",
            //"Y","YY2",
            "AE","AE",
            "AE0","AE",
            "AE1","AE",
            "AE2","AE",
            "HH","HH1",
            //"HH","HH2",
            "B","BB1",
            //"B","BB2"
            "TH","TH",
            "UH","UH",
            "UH0","UH",
            "UH1","UH",
            "UH2","UH",
            "AW","AW",
            "AW0","AW",
            "AW1","AW",
            "AW2","AW",
            "G","GG2",
            //"G","GG2",
            //"G","GG3",
            "V","VV",
            "SH","SH",
            "ZH","ZH",
            "F","FF",
            //"K","KK2",
            "Z","ZZ",
            "NG","NG",
            "L","LL",
            //"L","EL",
            "W","WW",
            //WH
            "CH","CH",
            "ER","ER1",
            "ER0","ER1",
            "ER1","ER2",
            "ER2","ER2",
            //"ER","ER2",
            //"ER0","ER2",
            //"ER1","ER2",
            //"ER2","ER2",
            "OW","OW",
            "OW0","OW",
            "OW1","OW",
            "OW2","OW",
            "S","S",
            //OR
            //AR
            //YR
        };

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
                return 25;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public override TimbreBase[] BaseTimbres
        {
            get
            {
                return Timbres;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public SP0256Timbre[] Timbres
        {
            get;
            set;
        }


        private const float DEFAULT_GAIN = 1.0f;

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
        private delegate void delegate_sp0256_ald_w(uint unitNumber, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sp0256_ald_w sp0256_ald_w
        {
            get;
            set;
        }

        private Queue<byte> commandDataQueue = new Queue<byte>();

        /// <summary>
        /// 
        /// </summary>
        private void SP0256AldW(byte data)
        {
            lock (fifoQueue)
            {
                fifoQueue.Enqueue(() =>
                {
                    try
                    {
                        Program.SoundUpdating();
                        sp0256_ald_w(UnitNumber, data);
                    }
                    finally
                    {
                        Program.SoundUpdated();
                    }
                    while (true)
                    {
                        try
                        {
                            Program.SoundUpdating();
                            if (sp0256_lrq_r(UnitNumber) != 0)
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
            fifoDataAdded.Set();

            //DeferredWriteData(SP0256_data_w, unitNumber, data);
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
                        if (sp0256_lrq_r(UnitNumber) != 0)
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
        private delegate int delegate_sp0256_sby_r(uint unitNumber);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_sp0256_sby_r sp0256_sby_r
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private int SP0256SbyR(uint unitNumber)
        {
            try
            {
                Program.SoundUpdating();

                return sp0256_sby_r(unitNumber);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int delegate_sp0256_lrq_r(uint unitNumber);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_sp0256_lrq_r sp0256_lrq_r
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private int SP0256LrqR(uint unitNumber)
        {
            try
            {
                Program.SoundUpdating();

                return sp0256_lrq_r(unitNumber);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_sp0256_set_clock(uint unitNumber, int clock);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sp0256_set_clock sp0256_set_clock
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SP0256SetClock(int clock)
        {
            try
            {
                Program.SoundUpdating();

                sp0256_set_clock(UnitNumber, clock);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        private static Dictionary<string, string[]> CmuDictionary;

        private static Dictionary<string, string> PhoneDictionary;

        /// <summary>
        /// 
        /// </summary>
        static SP0256()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("sp0256_ald_w");
            if (funcPtr != IntPtr.Zero)
            {
                sp0256_ald_w = (delegate_sp0256_ald_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sp0256_ald_w));
            }
            funcPtr = MameIF.GetProcAddress("sp0256_sby_r");
            if (funcPtr != IntPtr.Zero)
            {
                sp0256_sby_r = (delegate_sp0256_sby_r)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sp0256_sby_r));
            }
            funcPtr = MameIF.GetProcAddress("sp0256_lrq_r");
            if (funcPtr != IntPtr.Zero)
            {
                sp0256_lrq_r = (delegate_sp0256_lrq_r)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sp0256_lrq_r));
            }
            funcPtr = MameIF.GetProcAddress("sp0256_set_clock");
            if (funcPtr != IntPtr.Zero)
            {
                sp0256_set_clock = (delegate_sp0256_set_clock)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sp0256_set_clock));
            }

            AllophoneDictionary = new Dictionary<string, byte>();
            for (byte i = 0; i < AllophoneTable.Length; i++)
                AllophoneDictionary.Add(AllophoneTable[i], i);

            PhoneDictionary = new Dictionary<string, string>();
            for (int i = 0; i < CmuTable.Length / 2; i++)
                PhoneDictionary.Add(CmuTable[i * 2], CmuTable[i * 2 + 1]);

            CmuDictionary = new Dictionary<string, string[]>();
            using (StringReader sr = new StringReader(Resources.cmudict_0_7b))
            {
                string line;
                string[] split1 = new string[] { "  " };
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith(";"))
                        continue;
                    string[] word = line.Split(split1, StringSplitOptions.RemoveEmptyEntries);
                    if (word.Length == 2)
                        CmuDictionary.Add(word[0], convertAllophones(word[1].Split(' ')));
                }
            }
            CmuDictionary.Add("MAmidiMEmo", new string[] { "MM", "AR", "MM", "IY", "DH1", "IY", "MM", "EH", "MM", "OR" });
            CmuDictionary.Add("Chiptune", new string[] { "CH", "IH", "PP", "CH", "UW1", "NG" });
        }

        private static string[] convertAllophones(string[] phones)
        {
            for (int i = 0; i < phones.Length; i++)
                phones[i] = PhoneDictionary[phones[i]];
            return phones;
        }

        private SP0256SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public SP0256(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new SP0256Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new SP0256Timbre();
            setPresetInstruments();

            this.soundManager = new SP0256SoundManager(this);

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
            Timbres[0].Allophones = "MM AR MM IY DH1 IY MM EH MM OR PA5 CH IH PP CH UW1 NG";
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
        private class SP0256SoundManager : SoundManagerBase
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

            private static SoundList<SP0256Sound> psgOnSounds = new SoundList<SP0256Sound>(1);

            private SP0256 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SP0256SoundManager(SP0256 parent) : base(parent)
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
                foreach (SP0256Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    SP0256Sound snd = new SP0256Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
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
            private (SP0256 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
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

        /// <summary>
        /// 
        /// </summary>
        private class SP0256Sound : SoundBase
        {

            private SP0256 parentModule;

            private SP0256Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SP0256Sound(SP0256 parentModule, SP0256SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (SP0256Timbre)timbre;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                parentModule.hardReset();

                parentModule.SP0256SetClock((int)Math.Round(3120000d * (CalcCurrentFrequency() / 440d)));

                var tb = NoteOnEvent.Tag as NoteOnTimbreInfo;
                if (tb?.Tag is string all)
                {
                    var lpcd = extractAllophones(all);
                    if (lpcd != null)
                        foreach (var data in lpcd)
                            parentModule.SP0256AldW(data);
                }
                else
                {
                    var lpcd = timbre.RawAllophones;
                    if (lpcd != null)
                        foreach (var data in lpcd)
                            parentModule.SP0256AldW(data);
                }
            }

            private byte[] extractAllophones(string allophones)
            {
                List<byte> data = new List<byte>();
                StringBuilder sb = new StringBuilder();

                var alps = allophones.Split(new char[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);
                foreach (var alp in alps)
                {
                    var ALP = alp.ToUpperInvariant();
                    if (AllophoneDictionary.ContainsKey(ALP))
                    {
                        sb.Append(ALP);
                        sb.Append(" ");
                        data.Add((byte)AllophoneDictionary[ALP]);
                    }
                }

                return data.ToArray();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                parentModule.SP0256SetClock((int)Math.Round(3120000d * (CalcCurrentFrequency() / 440d)));

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
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SP0256Timbre>))]
        [DataContract]
        [InstLock]
        public class SP0256Timbre : TimbreBase
        {
            private string words;

            [DataMember]
            [Category("Sound")]
            [Description("Set natural words to convert to allophones.  Base frequency is 440Hz(A4).")]
            [DefaultValue(null)]
            public string Words
            {
                get
                {
                    return words;
                }
                set
                {
                    if (words != value)
                    {
                        if (value != null)
                        {
                            value = value.Replace(".", " ").Replace(",", " ").Replace("(", " ").Replace(")", " ").Replace("!", " ").Replace("?", " ");
                            value = value.Replace("0", "Zero ");
                            value = value.Replace("1", "One ");
                            value = value.Replace("2", "Two ");
                            value = value.Replace("3", "Three ");
                            value = value.Replace("4", "Four ");
                            value = value.Replace("5", "Five ");
                            value = value.Replace("6", "Six ");
                            value = value.Replace("7", "Seven ");
                            value = value.Replace("8", "Eight ");
                            value = value.Replace("9", "Nine ");
                        }
                        words = value;

                        if (!string.IsNullOrWhiteSpace(words))
                        {
                            var awords = words.Split(new char[] { ' ' },
                                StringSplitOptions.RemoveEmptyEntries);
                            List<byte> lpcd = new List<byte>();
                            foreach (var word in awords)
                            {
                                var WORD = word.ToUpperInvariant();
                                if (CmuDictionary.ContainsKey(WORD))
                                {
                                    string[] alps = CmuDictionary[WORD];
                                    foreach (var alp in alps)
                                    {
                                        var ALP = alp.ToUpperInvariant();
                                        if (AllophoneDictionary.ContainsKey(ALP))
                                            lpcd.Add((byte)AllophoneDictionary[ALP]);
                                    }
                                    lpcd.Add(3);    //PA4
                                }
                                else
                                {
                                    lpcd.Add(0x37);    //SS
                                    lpcd.Add(0x37);    //SS
                                    lpcd.Add(0x37);    //SS
                                    lpcd.Add(3);    //PA4
                                }
                            }
                            StringBuilder sb = new StringBuilder();
                            foreach (var lpc in lpcd)
                            {
                                sb.Append(AllophoneTable[lpc]);
                                sb.Append(" ");
                            }
                            var t = sb.ToString();
                            string tmp = words;
                            Allophones = t.Trim();
                            words = tmp;
                        }
                    }
                }
            }

            private string allophones;

            [DataMember]
            [Category("Sound")]
            [Description("Set allophones.  Base frequency is 440Hz(A4).")]
            [DefaultValue(null)]
            [Editor(typeof(AllophonesUITypeEditor), typeof(UITypeEditor)), Localizable(false)]
            public string Allophones
            {
                get
                {
                    return allophones;
                }
                set
                {
                    if (allophones != value)
                    {
                        List<byte> data = new List<byte>();
                        StringBuilder sb = new StringBuilder();
                        if (value == null)
                            value = string.Empty;
                        var alps = value.Split(new char[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries);
                        foreach (var alp in alps)
                        {
                            var ALP = alp.ToUpperInvariant();
                            if (AllophoneDictionary.ContainsKey(ALP))
                            {
                                sb.Append(ALP);
                                sb.Append(" ");
                                data.Add(AllophoneDictionary[ALP]);
                            }
                        }

                        allophones = sb.ToString().Trim();
                        RawAllophones = data.ToArray();
                        words = null;
                    }
                }
            }

            [Browsable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            public byte[] RawAllophones
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            public SP0256Timbre()
            {
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SP0256Timbre>(serializeData);
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

    }
}