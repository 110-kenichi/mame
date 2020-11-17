// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
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

//http://ngs.no.coocan.jp/doc/wiki.cgi/TechHan?page=1%BE%CF+PSG%A4%C8%B2%BB%C0%BC%BD%D0%CE%CF
//https://w.atwiki.jp/msx-sdcc/pages/45.html
//http://f.rdw.se/AY-3-8910-datasheet.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class AY8910 : InstrumentBase
    {

        public override string Name => "AY-3-8910";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.AY8910;

        [Browsable(false)]
        public override string ImageKey => "AY-3-8910";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ay8910_";

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
                return 11;
            }
        }

        private byte f_EnvelopeFrequencyCoarse = 2;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Set Envelope Coarse Frequency")]
        [SlideParametersAttribute(0, 255)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)2)]
        public byte EnvelopeFrequencyCoarse
        {
            get => f_EnvelopeFrequencyCoarse;
            set
            {
                if (f_EnvelopeFrequencyCoarse != value)
                {
                    f_EnvelopeFrequencyCoarse = value;
                    Ay8910WriteData(UnitNumber, 0, (byte)(12));
                    Ay8910WriteData(UnitNumber, 1, value);
                    Ay8910WriteData(UnitNumber, 0, (byte)(11));
                    Ay8910WriteData(UnitNumber, 1, EnvelopeFrequencyFine);
                }
            }
        }

        private byte f_EnvelopeFrequencyFine;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Set Envelope Fine Frequency")]
        [SlideParametersAttribute(0, 255)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte EnvelopeFrequencyFine
        {
            get => f_EnvelopeFrequencyFine;
            set
            {
                if (f_EnvelopeFrequencyFine != value)
                {
                    f_EnvelopeFrequencyFine = value;
                    Ay8910WriteData(UnitNumber, 0, (byte)(12));
                    Ay8910WriteData(UnitNumber, 1, EnvelopeFrequencyCoarse);
                    Ay8910WriteData(UnitNumber, 0, (byte)(11));
                    Ay8910WriteData(UnitNumber, 1, value);
                }
            }
        }

        private byte f_EnvelopeType;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Set Envelope Type (0-15)")]
        [SlideParametersAttribute(0, 15)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte EnvelopeType
        {
            get => f_EnvelopeType;
            set
            {
                byte v = (byte)(value & 15);
                if (f_EnvelopeType != v)
                {
                    f_EnvelopeType = v;
                    Ay8910WriteData(UnitNumber, 0, (byte)(13));
                    Ay8910WriteData(UnitNumber, 1, EnvelopeType);
                }
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
        public AY8910Timbre[] Timbres
        {
            get;
            set;
        }

        private const float DEFAULT_GAIN = 2.0f;

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
                using (var obj = JsonConvert.DeserializeObject<AY8910>(serializeData))
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
        private delegate void delegate_ay8910_address_data_w(uint unitNumber, int offset, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ay8910_address_data_w ay8910_address_data_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void Ay8910WriteData(uint unitNumber, int offset, byte data)
        {
            DeferredWriteData(ay8910_address_data_w, unitNumber, offset, data);
            /*
            try
            {
                Program.SoundUpdating();
                ay8910_address_data_w(unitNumber, offset, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_ay8910_read_ym(uint unitNumber);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ay8910_read_ym ay8910_read_ym
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static byte Ay8910ReadData(uint unitNumber)
        {
            try
            {
                Program.SoundUpdating();
                FlushDeferredWriteData();

                return ay8910_read_ym(unitNumber);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static AY8910()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ay8910_address_data_w");
            if (funcPtr != IntPtr.Zero)
            {
                ay8910_address_data_w = (delegate_ay8910_address_data_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ay8910_address_data_w));
            }
            funcPtr = MameIF.GetProcAddress("ay8910_read_ym");
            if (funcPtr != IntPtr.Zero)
            {
                ay8910_read_ym = (delegate_ay8910_read_ym)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ay8910_read_ym));
            }
        }

        private AY8910SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public AY8910(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new AY8910Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new AY8910Timbre();
            setPresetInstruments();

            this.soundManager = new AY8910SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            Ay8910WriteData(UnitNumber, 0, (byte)(7));
            Ay8910WriteData(UnitNumber, 1, (byte)(0x3f));
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].SoundType = SoundType.PSG;
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
        private class AY8910SoundManager : SoundManagerBase
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

            private static SoundList<AY8910Sound> psgOnSounds = new SoundList<AY8910Sound>(3);

            private AY8910 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public AY8910SoundManager(AY8910 parent) : base(parent)
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

                foreach (AY8910Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    AY8910Sound snd = new AY8910Sound(emptySlot.inst, this, timbre, note, emptySlot.slot);
                    psgOnSounds.Add(snd);

                    FormMain.OutputDebugLog("KeyOn ch" + emptySlot + " " + note.ToString());
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
            private (AY8910 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, AY8910Timbre timbre)
            {
                var emptySlot = (parentModule, -1);

                //if (((int)(((AY8910Timbre)timbre).SoundType) & 0x3) != 0)
                emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 3);

                return emptySlot;
            }

            internal override void AllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ControlChange(me);

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(7));
                Ay8910WriteData(parentModule.UnitNumber, 1, 0xff);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class AY8910Sound : SoundBase
        {

            private AY8910 parentModule;

            private AY8910Timbre timbre;

            private SoundType lastSoundType;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public AY8910Sound(AY8910 parentModule, AY8910SoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (AY8910Timbre)timbre;

                lastSoundType = this.timbre.SoundType;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.EnvelopeType.HasValue)
                        parentModule.EnvelopeType = gs.EnvelopeType.Value;
                    if (gs.EnvelopeFrequencyFine.HasValue)
                        parentModule.EnvelopeFrequencyFine = gs.EnvelopeFrequencyFine.Value;
                    if (gs.EnvelopeFrequencyCoarse.HasValue)
                        parentModule.EnvelopeFrequencyCoarse = gs.EnvelopeFrequencyCoarse.Value;
                }

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

                byte fv = (byte)(((byte)Math.Round(15 * CalcCurrentVolume()) & 0xf));

                SoundType st = lastSoundType;
                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (AyFxEngine)FxEngine;
                    if (eng.SoundType != null)
                        st = (SoundType)(eng.SoundType.Value & 7);
                }

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(8 + Slot));
                if (((int)st & 4) == 0)
                    // PSG/Noise
                    Ay8910WriteData(parentModule.UnitNumber, 1, fv);
                else
                    //Envelope
                    Ay8910WriteData(parentModule.UnitNumber, 1, (byte)(0x10 | fv));

                //key on
                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(7));
                byte data = Ay8910ReadData(parentModule.UnitNumber);
                data |= (byte)((1 | 8) << Slot);
                switch ((int)st & 3)
                {
                    case 1:
                        data &= (byte)(~(1 << Slot));
                        break;
                    case 2:
                        data &= (byte)(~(8 << Slot));
                        break;
                    case 3:
                        data &= (byte)(~((1 | 8) << Slot));
                        break;
                }
                Ay8910WriteData(parentModule.UnitNumber, 1, data);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                SoundType st = lastSoundType;
                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (AyFxEngine)FxEngine;
                    if (eng.SoundType != null)
                        st = (SoundType)(eng.SoundType.Value & 7);
                }

                //key on
                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(7));
                byte data = Ay8910ReadData(parentModule.UnitNumber);
                data |= (byte)((1 | 8) << Slot);
                switch ((int)st & 3)
                {
                    case 1:
                        data &= (byte)(~(1 << Slot));
                        break;
                    case 2:
                        data &= (byte)(~(8 << Slot));
                        break;
                    case 3:
                        data &= (byte)(~((1 | 8) << Slot));
                        break;
                }
                Ay8910WriteData(parentModule.UnitNumber, 1, data);

                if (((int)st & 1) == 1)
                    updatePsgPitch();

                if (((int)st & 2) == 2)
                    updateNoisePitch();

                if (((int)st & 4) != 0)
                {
                    var gs = timbre.GlobalSettings;
                    if (gs.Enable)
                    {
                        double freq = CalcCurrentFrequency();

                        // fE = fCLOCK / (256*EP)
                        // EP = CT+FT

                        // 256*EP * fE = fCLOCK
                        // EP = fCLOCK/(256 * fE)
                        // EP = 1.7897725 * 1000 * 1000 / (
                        if (gs.SyncWithNoteFrequency.HasValue)
                        {
                            var fm = freq;
                            if (gs.SyncWithNoteFrequencyDivider.HasValue)
                                fm /= gs.SyncWithNoteFrequencyDivider.Value;

                            var EP = (int)Math.Round((1.7897725 * 1000 * 1000) / (256 * fm));

                            parentModule.f_EnvelopeFrequencyCoarse = (byte)(EP >> 8);
                            parentModule.f_EnvelopeFrequencyFine = (byte)(EP & 0xff);
                        }
                    }

                    Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(12));
                    Ay8910WriteData(parentModule.UnitNumber, 1, parentModule.EnvelopeFrequencyCoarse);
                    Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(11));
                    Ay8910WriteData(parentModule.UnitNumber, 1, parentModule.EnvelopeFrequencyFine);
                    Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(13));
                    Ay8910WriteData(parentModule.UnitNumber, 1, parentModule.EnvelopeType);
                }

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            private void updatePsgPitch()
            {
                double freq = CalcCurrentFrequency();

                //freq = 111860.78125 / TP
                //TP = 111860.78125 / freq
                freq = Math.Round(111860.78125 / freq);
                if (freq > 0xfff)
                    freq = 0xfff;
                ushort tp = (ushort)freq;

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(0 + (Slot * 2)));
                Ay8910WriteData(parentModule.UnitNumber, 1, (byte)(tp & 0xff));

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(1 + (Slot * 2)));
                Ay8910WriteData(parentModule.UnitNumber, 1, (byte)((tp >> 8) & 0xf));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            private void updateNoisePitch()
            {
                double d = CalcCurrentPitchDeltaNoteNumber() * 63d;

                int kf = 0;
                if (d > 0)
                    kf = (int)d % 63;
                else if (d < 0)
                    kf = 63 + ((int)d % 63);

                int noted = (int)d / 63;
                if (d < 0)
                    noted -= 1;

                int nn = NoteOnEvent.NoteNumber;
                if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                    nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;
                int noteNum = nn + noted;
                if (noteNum > 127)
                    noteNum = 127;
                else if (noteNum < 0)
                    noteNum = 0;
                int n = 31 - (noteNum % 32);

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(6));
                Ay8910WriteData(parentModule.UnitNumber, 1, (byte)n);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(7));
                byte data = Ay8910ReadData(parentModule.UnitNumber);
                data |= (byte)((1 | 8) << Slot);
                Ay8910WriteData(parentModule.UnitNumber, 1, data);

                Ay8910WriteData(parentModule.UnitNumber, 0, (byte)(8 + Slot));
                Ay8910WriteData(parentModule.UnitNumber, 1, 0);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<AY8910Timbre>))]
        [DataContract]
        public class AY8910Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(SoundType.PSG)]
            [TypeConverter(typeof(FlagsEnumConverter))]
            public SoundType SoundType
            {
                get;
                set;
            }

            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public AY8910GlobalSettings GlobalSettings
            {
                get;
                set;
            }

            public AY8910Timbre()
            {
                GlobalSettings = new AY8910GlobalSettings();
                this.SDS.FxS = new AyFxSettings();

                SoundType = SoundType.PSG;
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<AY8910Timbre>(serializeData);
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

        [JsonConverter(typeof(NoTypeConverterJsonConverter<AyFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [MidiHook]
        public class AyFxSettings : BasicFxSettings
        {

            private string f_SoundTypeEnvelopes;

            [DataMember]
            [Description("Set dutysound type envelop by text. Input sound type value and split it with space like the Famitracker.\r\n" +
                       "1:PSG 2:NOISE 4:ENVELOPE \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 7)]
            public string SoundTypeEnvelopes
            {
                get
                {
                    return f_SoundTypeEnvelopes;
                }
                set
                {
                    if (f_SoundTypeEnvelopes != value)
                    {
                        SoundTypeEnvelopesRepeatPoint = -1;
                        SoundTypeEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            SoundTypeEnvelopesNums = new int[] { };
                            f_SoundTypeEnvelopes = string.Empty;
                            return;
                        }
                        f_SoundTypeEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                SoundTypeEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                SoundTypeEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 7)
                                        v = 7;
                                    vs.Add(v);
                                }
                            }
                        }
                        SoundTypeEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < SoundTypeEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (SoundTypeEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (SoundTypeEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            sb.Append(SoundTypeEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_SoundTypeEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeDutyEnvelopes()
            {
                return !string.IsNullOrEmpty(SoundTypeEnvelopes);
            }

            public void ResetDutyEnvelopes()
            {
                SoundTypeEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] SoundTypeEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int SoundTypeEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int SoundTypeEnvelopesReleasePoint { get; set; } = -1;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new AyFxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class AyFxEngine : BasicFxEngine
        {
            private AyFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public AyFxEngine(AyFxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_SoundType;

            public byte? SoundType
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);

                SoundType = null;
                if (settings.SoundTypeEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.SoundTypeEnvelopesNums.Length;
                        if (settings.SoundTypeEnvelopesReleasePoint >= 0)
                            vm = settings.SoundTypeEnvelopesReleasePoint;
                        if (f_SoundType >= vm)
                        {
                            if (settings.SoundTypeEnvelopesRepeatPoint >= 0)
                                f_SoundType = (uint)settings.SoundTypeEnvelopesRepeatPoint;
                            else
                                f_SoundType = (uint)vm;
                        }
                    }
                    else
                    {
                        if (settings.SoundTypeEnvelopesReleasePoint < 0)
                            f_SoundType = (uint)settings.SoundTypeEnvelopesNums.Length;

                        //if (f_dutyCounter >= settings.DutyEnvelopesNums.Length)
                        //{
                        //    if (settings.DutyEnvelopesRepeatPoint >= 0)
                        //        f_dutyCounter = (uint)settings.DutyEnvelopesRepeatPoint;
                        //}
                    }
                    if (f_SoundType < settings.SoundTypeEnvelopesNums.Length)
                    {
                        int vol = settings.SoundTypeEnvelopesNums[f_SoundType++];

                        SoundType = (byte)vol;
                        process = true;
                    }
                }

                return process;
            }
        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<AY8910GlobalSettings>))]
        [DataContract]
        [MidiHook]
        public class AY8910GlobalSettings : ContextBoundObject
        {
            [DataMember]
            [Category("Chip")]
            [Description("Override global settings")]
            public bool Enable
            {
                get;
                set;
            }

            private bool? f_SyncWithNoteFrequency;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Set Env frequency to Note on (nearly) frequency.")]
            [DefaultValue(null)]
            public bool? SyncWithNoteFrequency
            {
                get => f_SyncWithNoteFrequency;
                set
                {
                    f_SyncWithNoteFrequency = value;
                }
            }

            private double? f_SyncWithNoteFrequencyMultiple;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Set Env frequency divider for SyncWithNoteFrequency prop.")]
            [DoubleSlideParametersAttribute(0d, 5d, 1d)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? SyncWithNoteFrequencyDivider
            {
                get => f_SyncWithNoteFrequencyMultiple;
                set
                {
                    f_SyncWithNoteFrequencyMultiple = value;
                }
            }

            private byte? f_EnvelopeFrequencyCoarse;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [DefaultValue(null)]
            [Description("Set Envelope Coarse Frequency")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? EnvelopeFrequencyCoarse
            {
                get => f_EnvelopeFrequencyCoarse;
                set
                {
                    if (value.HasValue)
                    {
                        if (f_EnvelopeFrequencyCoarse != value)
                            f_EnvelopeFrequencyCoarse = value;
                    }
                    else
                        f_EnvelopeFrequencyCoarse = value;
                }
            }

            private byte? f_EnvelopeFrequencyFine;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Set Envelope Fine Frequency")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? EnvelopeFrequencyFine
            {
                get => f_EnvelopeFrequencyFine;
                set
                {
                    if (value.HasValue)
                    {
                        if (f_EnvelopeFrequencyFine != value)
                            f_EnvelopeFrequencyFine = value;
                    }
                    else
                        f_EnvelopeFrequencyFine = value;
                }
            }

            private byte? f_EnvelopeType;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Set Envelope Type")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? EnvelopeType
            {
                get => f_EnvelopeType;
                set
                {
                    if (value.HasValue)
                    {
                        byte v = (byte)(value & 15);
                        if (f_EnvelopeType != v)
                            f_EnvelopeType = v;
                    }
                    else
                        f_EnvelopeType = value;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum SoundType
        {
            NONE = 0,
            PSG = 1,
            NOISE = 2,
            ENVELOPE = 4,
        }

    }
}