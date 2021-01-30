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

//http://spatula-city.org/~im14u2c/sp0256-al2/Archer_SP0256-AL2.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
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
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
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
            //investigating
            Timbres[0].Allophones = "IH IH NN1 VV EH EH SS PA2 PA3 TT2 IH PA1 GG1 EY PA2 TT2 IH NG";
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

        protected override void OnNrpnDataEntered(ControlChangeEvent dataMsb, ControlChangeEvent dataLsb)
        {
            base.OnNrpnDataEntered(dataMsb, dataLsb);

            soundManager.ProcessNrpnData(dataMsb, dataLsb);
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

                foreach (SP0256Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    SP0256Sound snd = new SP0256Sound(emptySlot.inst, this, timbre, note, emptySlot.slot);
                    psgOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn SP0256 ch" + emptySlot + " " + note.ToString());
                    rv.Add(snd);
                }
                for (int i = 0; i < rv.Count; i++)
                {
                    var snd = rv[i];
                    if (!snd.IsDisposed)
                    {
                        snd.KeyOn();
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
            public SP0256Sound(SP0256 parentModule, SP0256SoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
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
                    for (int i = 0; i < AllophoneTable.Length; i++)
                    {
                        if (AllophoneTable[i].Equals(alp,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append(alp);
                            sb.Append(" ");
                            data.Add((byte)i);
                            break;
                        }
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
        public class SP0256Timbre : TimbreBase
        {

            private string allophones;

            [DataMember]
            [Category("Sound")]
            [Description("Set allophones. 440Hz(A4) is a base frequency.")]
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

                        var alps = value.Split(new char[] { ' ' },
                            StringSplitOptions.RemoveEmptyEntries);
                        foreach (var alp in alps)
                        {
                            for (int i = 0; i < AllophoneTable.Length; i++)
                            {
                                if (AllophoneTable[i].Equals(alp,
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    sb.Append(alp);
                                    sb.Append(" ");
                                    data.Add((byte)i);
                                    break;
                                }
                            }
                        }

                        allophones = sb.ToString().Trim();
                        RawAllophones = data.ToArray();
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
                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SP0256Timbre>(serializeData);
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
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