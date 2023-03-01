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

//http://www.citylan.it/wiki/images/3/3e/5232.pdf
//http://sr4.sakura.ne.jp/acsound/taito/taito5232.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class MSM5232 : InstrumentBase, IStandardValues
    {

        public override string Name => "MSM5232+TA7630";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.MSM5232;

        [Browsable(false)]
        public override string ImageKey => "MSM5232";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "msm5232_";

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
                return 10;
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
        public MSM5232Timbre[] Timbres
        {
            get;
            set;
        }

        private double f_Capacitor;

        /// <summary>
        /// 
        /// </summary>
        [Category("Chip")]
        [Description("Set Capacitor capacity [uF]" +
            "The FairyLand Story: 1.0e-6\rn" +
            "Lady Frog: 0.65e-6\r\n" +
            "Equites: 0.47e-6\r\n" +
            "Buggy Challenge: 0.39e-6")]
        [TypeConverter(typeof(StandardValuesDoubleConverter))]
        public double Capacitor
        {
            get => f_Capacitor;
            set
            {
                if (f_Capacitor != value)
                {
                    f_Capacitor = value;
                    MSM5232SetCapasitors(UnitNumber, value, value, value, value, value, value, value, value);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ICollection GetStandardValues(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Capacitor):
                    {
                        var l = new List<double>();
                        l.Add(1.0e-6);
                        l.Add(0.65e-6);
                        l.Add(0.47e-6);
                        l.Add(0.39e-6);
                        return l;
                    }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializeData"></param>
        public override void RestoreFrom(string serializeData)
        {
            try
            {
                using (var obj = JsonConvert.DeserializeObject<MSM5232>(serializeData))
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
        private delegate void delegate_MSM5232_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_MSM5232_write MSM5232_write;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="ch"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_MSM5232_set_volume(uint unitNumber, int ch, byte data);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_MSM5232_set_volume MSM5232_set_volume;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="ch"></param>
        /// <param name="data"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_MSM5232_set_capacitors(uint unitNumber, double cap1, double cap2, double cap3, double cap4, double cap5, double cap6, double cap7, double cap8);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_MSM5232_set_capacitors MSM5232_set_capacitors;

        /// <summary>
        /// 
        /// </summary>
        private static void MSM5232WriteData(uint unitNumber, uint address, byte data)
        {
            DeferredWriteData(MSM5232_write, unitNumber, address, data);
            /*
            try
            {
                Program.SoundUpdating();
                MSM5232_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }


        /// <summary>
        /// 
        /// </summary>
        private static void MSM5232SetVolume(uint unitNumber, int ch, byte data)
        {
            DeferredWriteData(MSM5232_set_volume, unitNumber, ch, data);
            /*
            try
            {
                Program.SoundUpdating();
                MSM5232_set_volume(unitNumber, ch, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }


        /// <summary>
        /// 
        /// </summary>
        private static void MSM5232SetCapasitors(uint unitNumber, double cap1, double cap2, double cap3, double cap4, double cap5, double cap6, double cap7, double cap8)
        {
            DeferredWriteData(MSM5232_set_capacitors, unitNumber, cap1, cap2, cap3, cap4, cap5, cap6, cap7, cap8);
            /*
            try
            {
                Program.SoundUpdating();
                MSM5232_set_capacitors(unitNumber, cap1, cap2, cap3, cap4, cap5, cap6, cap7, cap8);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static void MSM5232SetCapacitors(uint unitNumber, int ch, byte data)
        {
            DeferredWriteData(MSM5232_set_volume, unitNumber, ch, data);
            /*
            try
            {
                Program.SoundUpdating();
                MSM5232_set_volume(unitNumber, ch, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        static MSM5232()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("msm5232_write");
            if (funcPtr != IntPtr.Zero)
            {
                MSM5232_write = (delegate_MSM5232_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_MSM5232_write));
            }
            funcPtr = MameIF.GetProcAddress("msm5232_set_volume");
            if (funcPtr != IntPtr.Zero)
            {
                MSM5232_set_volume = (delegate_MSM5232_set_volume)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_MSM5232_set_volume));
            }
            funcPtr = MameIF.GetProcAddress("msm5232_set_capacitors");
            if (funcPtr != IntPtr.Zero)
            {
                MSM5232_set_capacitors = (delegate_MSM5232_set_capacitors)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_MSM5232_set_capacitors));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        private MSM5232SoundManager soundManager;


        private const float DEFAULT_GAIN = 2.7f;

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
        public MSM5232(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new MSM5232Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new MSM5232Timbre();
            setPresetInstruments();

            this.soundManager = new MSM5232SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].SoundGroup = SoundGroup.Group1;
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            Capacitor = 1.0e-6;
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
        private class MSM5232SoundManager : SoundManagerBase
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

            private static SoundList<MSM5232Sound> chAOnSounds = new SoundList<MSM5232Sound>(4);
            private static SoundList<MSM5232Sound> chBOnSounds = new SoundList<MSM5232Sound>(4);

            private MSM5232 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public MSM5232SoundManager(MSM5232 parent) : base(parent)
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
                foreach (MSM5232Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    MSM5232Sound snd = new MSM5232Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    switch (timbre.SoundGroup)
                    {
                        case SoundGroup.Group1:
                            chAOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn A ch" + emptySlot + " " + note.ToString());
                            break;
                        case SoundGroup.Group2:
                            chBOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn B ch" + emptySlot + " " + note.ToString());
                            break;
                    }

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
            private (MSM5232 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note,MSM5232Timbre timbre)
            {
                var emptySlot = (parentModule, -1);

                switch (timbre.SoundGroup)
                {
                    case SoundGroup.Group1:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, chAOnSounds, note, 4);
                            break;
                        }
                    case SoundGroup.Group2:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, chBOnSounds, note, 4);
                            break;
                        }
                }

                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 4; i++)
                {
                    MSM5232WriteData(parentModule.UnitNumber, (uint)(i + (0 * 4)), 0);
                    MSM5232WriteData(parentModule.UnitNumber, (uint)(i + (1 * 4)), 0);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class MSM5232Sound : SoundBase
        {

            private MSM5232 parentModule;

            private MSM5232Timbre timbre;

            public int lastGroup = 0;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public MSM5232Sound(MSM5232 parentModule, MSM5232SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (MSM5232Timbre)timbre;
                this.lastGroup = (int)this.timbre.SoundGroup;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                SetTimbre();
                //Volume
                OnVolumeUpdated();
                //Freq
                OnPitchUpdated();
            }

            public override void OnSoundParamsUpdated()
            {
                SetTimbre();

                base.OnSoundParamsUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                MSM5232WriteData(parentModule.UnitNumber, (uint)(0x8 + lastGroup), timbre.AT);
                MSM5232WriteData(parentModule.UnitNumber, (uint)(0xa + lastGroup), timbre.DT);
                MSM5232WriteData(parentModule.UnitNumber, (uint)(0xc + lastGroup), (byte)(timbre.EGE << 5 | timbre.ARM << 4 | timbre.Harmonics));
            }

            /// <summary>
            /// Harmonics
            /// </summary>
            public override void OnVolumeUpdated()
            {
                double v = 1;
                v *= ParentModule.Expressions[NoteOnEvent.Channel] / 127d;
                v *= ParentModule.Volumes[NoteOnEvent.Channel] / 127d;
                byte fv = (byte)Math.Round(15 * v);

                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (MsmFxEngine)FxEngine;
                    if (eng.HarmonicsValue != null)
                        MSM5232WriteData(parentModule.UnitNumber, (uint)(0xc + lastGroup), (byte)(timbre.EGE << 5 | timbre.ARM << 4 | eng.HarmonicsValue));
                }

                MSM5232SetVolume(parentModule.UnitNumber, lastGroup, fv);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                if (!IsKeyOff)
                {
                    if (!timbre.NoiseTone)
                    {
                        int nn = NoteOnEvent.NoteNumber;
                        if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                            nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;
                        nn -= 36;
                        if (nn < 0)
                            nn = 0;
                        byte noteNum = (byte)nn;

                        MSM5232WriteData(parentModule.UnitNumber, (uint)(Slot + (lastGroup * 4)), (byte)(0x80 | noteNum));
                    }
                    else
                    {
                        MSM5232WriteData(parentModule.UnitNumber, (uint)(Slot + (lastGroup * 4)), (byte)(0xff));
                    }
                }

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                MSM5232WriteData(parentModule.UnitNumber, (uint)(Slot + (lastGroup * 4)), 0);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<MSM5232Timbre>))]
        [DataContract]
        [InstLock]
        public class MSM5232Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Group")]
            [DefaultValue(SoundGroup.Group1)]
            public SoundGroup SoundGroup
            {
                get;
                set;
            }

            private byte f_AT;

            [DataMember]
            [Category("Sound")]
            [Description("Attack Time (0-7)")]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte AT
            {
                get
                {
                    return f_AT;
                }
                set
                {
                    f_AT = (byte)(value & 7);
                }
            }

            private byte f_DT;

            [DataMember]
            [Category("Sound")]
            [Description("Decay Time (0-15)")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte DT
            {
                get
                {
                    return f_DT;
                }
                set
                {
                    f_DT = (byte)(value & 15);
                }
            }


            private byte f_EGE;

            [Browsable(false)]
            [DataMember]
            [Category("Sound")]
            [Description("Envelope Generator Mode (0:Off 1:On)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            public byte EGE
            {
                get
                {
                    return f_EGE;
                }
                set
                {
                    f_EGE = (byte)(value & 1);
                }
            }


            private byte f_ARM = 1;

            [DataMember]
            [Category("Sound")]
            [Description("Attack Release Mode (0:Off 1:On)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)1)]
            public byte ARM
            {
                get
                {
                    return f_ARM;
                }
                set
                {
                    f_ARM = (byte)(value & 1);
                }
            }

            private byte f_Horm = 15;

            [DataMember]
            [Category("Sound")]
            [Description("Harmonics mode (b0:Normal b1:1 Octave b2:2 Octave b3:3 Octave)")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)15)]
            public byte Harmonics
            {
                get
                {
                    return f_Horm;
                }
                set
                {
                    f_Horm = (byte)(value & 15);
                }
            }

            private bool f_NoiseTone;

            [DataMember]
            [Category("Sound")]
            [Description("Noise Tone")]
            [DefaultValue(false)]
            public bool NoiseTone
            {
                get
                {
                    return f_NoiseTone;
                }
                set
                {
                    f_NoiseTone = value;
                }
            }

            public MSM5232Timbre()
            {
                this.SDS.FxS = new MsmFxSettings();
            }

            protected override void InitializeFxS()
            {
                this.SDS.FxS = new MsmFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<MSM5232Timbre>(serializeData);
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
        public enum SoundGroup
        {
            Group1,
            Group2,
        }


        [JsonConverter(typeof(NoTypeConverterJsonConverter<MsmFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class MsmFxSettings : BasicFxSettings
        {

            private string f_HarmonicsEnvelopes;

            [DataMember]
            [Description("Set harmonics envelop by text. Input harmonics value and split it with space like the FamiTracker.\r\n" +
                       "0 ～ 15 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 15)]
            public string HarmonicsEnvelopes
            {
                get
                {
                    return f_HarmonicsEnvelopes;
                }
                set
                {
                    if (f_HarmonicsEnvelopes != value)
                    {
                        HarmonicsEnvelopesRepeatPoint = -1;
                        HarmonicsEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            HarmonicsEnvelopesNums = new int[] { };
                            f_HarmonicsEnvelopes = string.Empty;
                            return;
                        }
                        f_HarmonicsEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                HarmonicsEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                HarmonicsEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 15)
                                        v = 15;
                                    vs.Add(v);
                                }
                            }
                        }
                        HarmonicsEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i <= HarmonicsEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (HarmonicsEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (HarmonicsEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < HarmonicsEnvelopesNums.Length)
                                sb.Append(HarmonicsEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_HarmonicsEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeHarmonicsEnvelopes()
            {
                return !string.IsNullOrEmpty(HarmonicsEnvelopes);
            }

            public void ResetHarmonicsEnvelopes()
            {
                HarmonicsEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] HarmonicsEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int HarmonicsEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int HarmonicsEnvelopesReleasePoint { get; set; } = -1;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new MsmFxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class MsmFxEngine : BasicFxEngine
        {
            private MsmFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public MsmFxEngine(MsmFxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_HarmonicsCounter;

            public byte? HarmonicsValue
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);

                HarmonicsValue = null;
                if (settings.HarmonicsEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.HarmonicsEnvelopesNums.Length;
                        if (settings.HarmonicsEnvelopesReleasePoint >= 0)
                            vm = settings.HarmonicsEnvelopesReleasePoint;
                        if (f_HarmonicsCounter >= vm)
                        {
                            if (settings.HarmonicsEnvelopesRepeatPoint >= 0)
                                f_HarmonicsCounter = (uint)settings.HarmonicsEnvelopesRepeatPoint;
                            else
                                f_HarmonicsCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (settings.HarmonicsEnvelopesReleasePoint < 0)
                        //    f_HarmonicsCounter = (uint)settings.HarmonicsEnvelopesNums.Length;

                        if (f_HarmonicsCounter < settings.HarmonicsEnvelopesNums.Length)
                        {
                            if (settings.HarmonicsEnvelopesReleasePoint >= 0 && f_HarmonicsCounter < (uint)settings.HarmonicsEnvelopesReleasePoint)
                                f_HarmonicsCounter = (uint)settings.HarmonicsEnvelopesReleasePoint;
                        }
                    }
                    if (f_HarmonicsCounter < settings.HarmonicsEnvelopesNums.Length)
                    {
                        int vol = settings.HarmonicsEnvelopesNums[f_HarmonicsCounter++];

                        HarmonicsValue = (byte)vol;
                        process = true;
                    }
                }

                return process;
            }

        }

    }
}