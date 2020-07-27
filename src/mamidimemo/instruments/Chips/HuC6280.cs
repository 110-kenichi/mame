// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
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

//http://www.magicengine.com/mkit/doc_hard_psg.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class HuC6280 : InstrumentBase
    {
        public override string Name => "HuC6280";

        public override string Group => "WSG";

        public override InstrumentType InstrumentType => InstrumentType.HuC6280;

        [Browsable(false)]
        public override string ImageKey => "HuC6280";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "c6280_";

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
                return 16;
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
        [Category("Chip")]
        [Description("Timbres (0-127)")]
        [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public HuC6280Timbre[] Timbres
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<HuC6280>(serializeData))
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
        private delegate void delegate_c6280_w(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static void C6280WriteData(uint unitNumber, uint address, int? slot, byte data)
        {
            if (slot != null)
                DeferredWriteData(C6280_w, unitNumber, 0x800, (byte)slot.Value);
            DeferredWriteData(C6280_w, unitNumber, address, data);
            /*
            try
            {
                Program.SoundUpdating();
                if (slot != null)
                    C6280_w(unitNumber, 0x800, (byte)slot.Value);
                C6280_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_c6280_w C6280_w
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        static HuC6280()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("c6280_w");
            if (funcPtr != IntPtr.Zero)
                C6280_w = (delegate_c6280_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_c6280_w));
        }

        private Hu6280SoundManager soundManager;


        private const float DEFAULT_GAIN = 6.0f;

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
        public HuC6280(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new HuC6280Timbre[InstrumentBase.MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.MAX_TIMBRES; i++)
                Timbres[i] = new HuC6280Timbre();
            setPresetInstruments();

            this.soundManager = new Hu6280SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].SoundType = SoundType.WSG;
            Timbres[0].WsgData = new byte[]
            {
                0x0f, 0x12, 0x15, 0x17, 0x19, 0x1b, 0x1d, 0x1e,
                0x1e, 0x1e, 0x1d, 0x1b, 0x19, 0x17, 0x15, 0x12,
                0x0f, 0x0c, 0x09, 0x07, 0x05, 0x03, 0x01, 0x00,
                0x00, 0x00, 0x01, 0x03, 0x05, 0x07, 0x09, 0x0c  };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vgmPath"></param>
        public override void StartVgmRecordingTo(string vgmPath)
        {
            base.StartVgmRecordingTo(vgmPath);

            //Sound On
            C6280WriteData(UnitNumber, 0x801, null, (byte)0xff);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(TaggedNoteOnEvent midiEvent)
        {
            soundManager.KeyOn(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOffEvent(NoteOffEvent midiEvent)
        {
            soundManager.KeyOff(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnControlChangeEvent(ControlChangeEvent midiEvent)
        {
            base.OnControlChangeEvent(midiEvent);

            soundManager.ControlChange(midiEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnPitchBendEvent(PitchBendEvent midiEvent)
        {
            base.OnPitchBendEvent(midiEvent);

            soundManager.PitchBend(midiEvent);
        }

        internal override void AllSoundOff()
        {
            soundManager.AllSoundOff();
        }

        /// <summary>
        /// 
        /// </summary>
        private class Hu6280SoundManager : SoundManagerBase
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

            private static SoundList<Hu6280Sound> lfoOnSounds = new SoundList<Hu6280Sound>(1);

            private static SoundList<Hu6280Sound> wsgOnSounds = new SoundList<Hu6280Sound>(2);

            private static SoundList<Hu6280Sound> noiseOnSounds = new SoundList<Hu6280Sound>(1);


            private HuC6280 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public Hu6280SoundManager(HuC6280 parent) : base(parent)
            {
                this.parentModule = parent;

                C6280WriteData(parentModule.UnitNumber, 0x801, null, (byte)0xff);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            public override SoundBase[] SoundOn(TaggedNoteOnEvent note)
            {
                List<SoundBase> rv = new List<SoundBase>();

                foreach (HuC6280Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    Hu6280Sound snd = new Hu6280Sound(emptySlot.inst, this, timbre, note, emptySlot.slot);
                    switch (timbre.SoundType)
                    {
                        case SoundType.WSGLFO:
                            lfoOnSounds.Add(snd);
                            break;
                        case SoundType.WSG:
                            wsgOnSounds.Add(snd);
                            break;
                        case SoundType.NOISE:
                            noiseOnSounds.Add(snd);
                            break;
                    }
                    snd.KeyOn();
                    rv.Add(snd);
                }

                return rv.ToArray();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private (HuC6280 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, HuC6280Timbre timbre)
            {
                var emptySlot = (parentModule, -1);

                switch (timbre.SoundType)
                {
                    case SoundType.WSGLFO:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, lfoOnSounds, note, 1);
                            break;
                        }
                    case SoundType.WSG:
                        {
                            if (timbre.PartialReserveWSGLFO)
                            {
                                if (timbre.PartialReserveNOISE)
                                    emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 2);
                                else
                                    emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 4);
                            }
                            else
                            {
                                if (timbre.PartialReserveNOISE)
                                    emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 4);
                                else
                                    emptySlot = SearchEmptySlotAndOffForLeader(parentModule, wsgOnSounds, note, 6);
                            }
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, noiseOnSounds, note, 2);
                            break;
                        }
                }

                return emptySlot;
            }

            internal override void AllSoundOff()
            {
                for (int i = 0; i < 6; i++)
                {
                    C6280WriteData(parentModule.UnitNumber, 0x804, i, (byte)0);
                    C6280WriteData(parentModule.UnitNumber, 0x807, i, (byte)0);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class Hu6280Sound : SoundBase
        {

            private HuC6280 parentModule;

            private HuC6280Timbre timbre;

            private SoundType lastSoundType;

            private int partialReserveLfo;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public Hu6280Sound(HuC6280 parentModule, Hu6280SoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (HuC6280Timbre)timbre;

                lastSoundType = this.timbre.SoundType;
                if (lastSoundType == SoundType.WSG && this.timbre.PartialReserveWSGLFO)
                    partialReserveLfo = 2;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                        {
                            //ch0 WSG
                            C6280WriteData(parentModule.UnitNumber, 0x800, null, (byte)Slot);
                            C6280WriteData(parentModule.UnitNumber, 0x804, null, (byte)Slot);
                            foreach (var d in timbre.WsgData)
                                C6280WriteData(parentModule.UnitNumber, 0x806, null, d);

                            //ch1 LFO
                            C6280WriteData(parentModule.UnitNumber, 0x800, null, (byte)(Slot + 1));
                            foreach (var d in timbre.LfoData)
                                C6280WriteData(parentModule.UnitNumber, 0x806, null, d);

                            C6280WriteData(parentModule.UnitNumber, 0x808, null, timbre.LfoFreq);
                            C6280WriteData(parentModule.UnitNumber, 0x809, null, (byte)((timbre.LfoEnable ? 0x00 : 0x80) | timbre.LfoMode));

                            FormMain.OutputDebugLog("KeyOn LFO ch" + Slot + " " + NoteOnEvent.ToString());
                            break;
                        }
                    case SoundType.WSG:
                        {
                            C6280WriteData(parentModule.UnitNumber, 0x800, null, (byte)(Slot + partialReserveLfo));
                            foreach (var d in timbre.WsgData)
                                C6280WriteData(parentModule.UnitNumber, 0x806, null, d);

                            FormMain.OutputDebugLog("KeyOn PSG ch" + (Slot + partialReserveLfo) + " " + NoteOnEvent.ToString());
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            C6280WriteData(parentModule.UnitNumber, 0x800, null, (byte)(Slot + 4));
                            for (int i = 0; i < 32; i++)
                                C6280WriteData(parentModule.UnitNumber, 0x806, null, 0);

                            FormMain.OutputDebugLog("KeyOn NOISE ch" + Slot + " " + NoteOnEvent.ToString());
                            break;
                        }
                }
                OnPanpotUpdated();

                OnPitchUpdated();

                OnVolumeUpdated();
            }


            public override void OnSoundParamsUpdated()
            {
                base.OnSoundParamsUpdated();

                OnPanpotUpdated();

                OnPitchUpdated();

                OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                if (IsSoundOff)
                    return;

                var vol = CalcCurrentVolume();
                byte wvol = (byte)Math.Round(31d * vol);

                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                    case SoundType.WSG:
                        {
                            C6280WriteData(parentModule.UnitNumber, 0x804, (Slot + partialReserveLfo), (byte)(0x80 | wvol));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            C6280WriteData(parentModule.UnitNumber, 0x804, Slot + 4, (byte)(0x80 | wvol));
                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                if (IsSoundOff)
                    return;

                double freq = CalcCurrentFrequency();

                //Freq
                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                    case SoundType.WSG:
                        {
                            ushort wsgfreq = convertWsgFrequency(freq);

                            C6280WriteData(parentModule.UnitNumber, 0x800, null, (byte)(Slot + partialReserveLfo));
                            C6280WriteData(parentModule.UnitNumber, 0x802, null, (byte)(wsgfreq & 0xff));
                            C6280WriteData(parentModule.UnitNumber, 0x803, null, (byte)((wsgfreq >> 8) & 0x0f));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            ushort noisefreq = convertNoiseFrequency(freq);

                            C6280WriteData(parentModule.UnitNumber, 0x807, Slot + 4, (byte)(0x80 | noisefreq));

                            break;
                        }
                }

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPanpotUpdated()
            {
                //Pan
                int pan = parentModule.Panpots[NoteOnEvent.Channel] - 1;
                if (pan < 0)
                    pan = 0;

                byte left = (byte)Math.Round(15d * Math.Cos(Math.PI / 2 * (pan / 126d)));
                byte right = (byte)Math.Round(15d * Math.Sin(Math.PI / 2 * (pan / 126d)));

                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                    case SoundType.WSG:
                        {
                            C6280WriteData(parentModule.UnitNumber, 0x805, (Slot + partialReserveLfo), (byte)(left << 4 | right));
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            C6280WriteData(parentModule.UnitNumber, 0x805, Slot + 4, (byte)(left << 4 | right));
                            break;
                        }
                }
            }


            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                switch (lastSoundType)
                {
                    case SoundType.WSGLFO:
                    case SoundType.WSG:
                        {
                            C6280WriteData(parentModule.UnitNumber, 0x804, (Slot + partialReserveLfo), (byte)0);
                            break;
                        }
                    case SoundType.NOISE:
                        {
                            C6280WriteData(parentModule.UnitNumber, 0x804, Slot + 4, (byte)0);
                            C6280WriteData(parentModule.UnitNumber, 0x807, Slot + 4, (byte)0);
                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <returns></returns>
            private ushort convertWsgFrequency(double freq)
            {
                double f = (3580000d / (32 * freq));
                if (f > 4095d)
                    f = 4095d;
                return (ushort)Math.Round(f);
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <returns></returns>
            private ushort convertNoiseFrequency(double freq)
            {
                double f = (3580000d / (64 * freq));
                if (f > 4095d)
                    f = 4095d;
                return (ushort)(((ushort)Math.Round(f)) ^ 31);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<HuC6280Timbre>))]
        [DataContract]
        public class HuC6280Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(SoundType.WSG)]
            public SoundType SoundType
            {
                get;
                set;
            }

            private bool f_PartialReserveLFO;

            [DataMember]
            [Category("Chip")]
            [Description("LFOWSG partial reserve against with WSG.\r\n" +
                "WSG w/ LFO shared 1,2 ch with WSG." +
                "So, you can choose whether to give priority to WSG w/ LFO over WSG")]
            [DefaultValue(false)]
            public bool PartialReserveWSGLFO
            {
                get
                {
                    return f_PartialReserveLFO;
                }
                set
                {
                    f_PartialReserveLFO = value;
                }
            }


            private bool f_PartialReserveNoise;

            [DataMember]
            [Category("Chip")]
            [Description("NOISE partial reserve against with WSG.\r\n" +
                "NOISE shared 5,6 ch with WSG." +
                "So, you can choose whether to give priority to NOISE over WSG")]
            [DefaultValue(false)]
            public bool PartialReserveNOISE
            {
                get
                {
                    return f_PartialReserveNoise;
                }
                set
                {
                    f_PartialReserveNoise = value;
                }
            }

            private byte[] f_wsgdata = new byte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(5)]
            [DataMember]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 0-31 levels)")]
            public byte[] WsgData
            {
                get
                {
                    return f_wsgdata;
                }
                set
                {
                    f_wsgdata = value;
                }
            }

            public bool ShouldSerializeWsgData()
            {
                foreach (var dt in WsgData)
                {
                    if (dt != 0)
                        return true;
                }
                return false;
            }

            public void ResetWsgData()
            {
                f_wsgdata = new byte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 0-31 levels)")]
            [IgnoreDataMember]
            [JsonIgnore]
            public string WsgDataSerializeData
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < WsgData.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        sb.Append(WsgData[i].ToString((IFormatProvider)null));
                    }
                    return sb.ToString();
                }
                set
                {
                    string[] vals = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<byte> vs = new List<byte>();
                    foreach (var val in vals)
                    {
                        byte v = 0;
                        if (byte.TryParse(val, out v))
                            vs.Add(v);
                    }
                    for (int i = 0; i < Math.Min(WsgData.Length, vs.Count); i++)
                        WsgData[i] = vs[i] > 31 ? (byte)31 : vs[i];
                }
            }


            private bool f_LfoEnable = true;

            [DataMember]
            [Category("Sound(LFO)")]
            [Description("Where the LFO enable or not when the ToneType is WSGLFO.")]
            [DefaultValue(true)]
            public bool LfoEnable
            {
                get
                {
                    return f_LfoEnable;
                }
                set
                {
                    f_LfoEnable = value;
                }
            }

            private byte f_LfoMode = 1;

            [DataMember]
            [Category("Sound(LFO)")]
            [Description("Select LFO mode (0=Disable, 1=On, 2=On(x4), 3=On(x8))")]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)1)]
            public byte LfoMode
            {
                get
                {
                    return f_LfoMode;
                }
                set
                {
                    f_LfoMode = (byte)(value & 3);
                }
            }

            private byte f_LfoFreq = 32;

            [DataMember]
            [Category("Sound(LFO)")]
            [Description("Set LFO freqency")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)32)]
            public byte LfoFreq
            {
                get
                {
                    return f_LfoFreq;
                }
                set
                {
                    f_LfoFreq = value;
                }
            }


            private byte[] f_lfodata = new byte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(5)]
            [DataMember]
            [Category("Sound")]
            [Description("LFO Table (32 samples, 0-31 levels)")]
            public byte[] LfoData
            {
                get
                {
                    return f_lfodata;
                }
                set
                {
                    f_lfodata = value;
                }
            }

            public bool ShouldSerializeLfoData()
            {
                foreach (var dt in LfoData)
                {
                    if (dt != 0)
                        return true;
                }
                return false;
            }

            public void ResetLfoData()
            {
                f_lfodata = new byte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("LFO Table (32 samples, 0-31 levels)")]
            [IgnoreDataMember]
            [JsonIgnore]
            public string LfoDataSerializeData
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < LfoData.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        sb.Append(LfoData[i].ToString((IFormatProvider)null));
                    }
                    return sb.ToString();
                }
                set
                {
                    string[] vals = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<byte> vs = new List<byte>();
                    foreach (var val in vals)
                    {
                        byte v = 0;
                        if (byte.TryParse(val, out v))
                            vs.Add(v);
                    }
                    for (int i = 0; i < Math.Min(LfoData.Length, vs.Count); i++)
                        LfoData[i] = vs[i] > 31 ? (byte)31 : vs[i];
                }
            }


            /// <summary>
            /// 
            /// </summary>
            public HuC6280Timbre()
            {
                SDS.FxS = new BasicFxSettings();
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="serializeData"></param>
            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<HuC6280Timbre>(serializeData);
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

        /// <summary>
        /// 
        /// </summary>
        public enum SoundType
        {
            WSG,
            WSGLFO,
            NOISE,
        }

    }
}
