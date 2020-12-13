// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
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

//http://bifi.msxnet.org/msxnet/tech/scc.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class SCC1 : InstrumentBase
    {

        public override string Name => "SCC1";

        public override string Group => "WSG";

        public override InstrumentType InstrumentType => InstrumentType.SCC1;

        [Browsable(false)]
        public override string ImageKey => "SCC1";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "scc1_";

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
                return 7;
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
        public SCC1Timbre[] Timbres
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
            using (var obj = JsonConvert.DeserializeObject<SCC1>(serializeData))
                this.InjectFrom(new LoopInjection(new[] { "SerializeData" }), obj);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_SCC1_w(uint unitNumber, uint address, byte data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_SCC1_w_array(uint unitNumber, uint address, sbyte[] data, int length);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_SCC1_r(uint unitNumber, uint address);

        private static delegate_SCC1_w_array SCC1_waveform_w;

        private static delegate_SCC1_w SCC1_volume_w;

        private static delegate_SCC1_w SCC1_frequency_w;

        private static delegate_SCC1_w SCC1_keyonoff_w;

        private static delegate_SCC1_r SCC1_keyonoff_r;

        /// <summary>
        /// 
        /// </summary>
        private static void Scc1VolumeWriteData(uint unitNumber, uint address, byte data)
        {
            DeferredWriteData(SCC1_volume_w, unitNumber, address, data);
            /*
            try
            {
                Program.SoundUpdating();
                SCC1_volume_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Scc1FrequencyWriteData(uint unitNumber, uint address, byte data)
        {
            DeferredWriteData(SCC1_frequency_w, unitNumber, address, data);
            /*
            try
            {
                Program.SoundUpdating();
                SCC1_frequency_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Scc1KeyOnOffWriteData(uint unitNumber, byte data)
        {
            DeferredWriteData(SCC1_keyonoff_w, unitNumber, (byte)0, data);
            /*
            try
            {
                Program.SoundUpdating();
                SCC1_keyonoff_w(unitNumber, 0, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static byte Scc1KeyOnOffReadData(uint unitNumber)
        {
            try
            {
                Program.SoundUpdating();
                FlushDeferredWriteData();

                return SCC1_keyonoff_r(unitNumber, 0);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void Scc1WriteWaveData(uint unitNumber, uint address, sbyte[] data)
        {
            DeferredWriteData(SCC1_waveform_w, unitNumber, address, data, data.Length);

            /*
            try
            {
                Program.SoundUpdating();

                SCC1_waveform_w(unitNumber, address, data, data.Length);
            }
            finally
            {
                Program.SoundUpdated();
            }//*/
        }

        /// <summary>
        /// 
        /// </summary>
        static SCC1()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("SCC1_waveform_w");
            if (funcPtr != IntPtr.Zero)
                SCC1_waveform_w = (delegate_SCC1_w_array)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_w_array));
            funcPtr = MameIF.GetProcAddress("SCC1_volume_w");
            if (funcPtr != IntPtr.Zero)
                SCC1_volume_w = (delegate_SCC1_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_w));
            funcPtr = MameIF.GetProcAddress("SCC1_frequency_w");
            if (funcPtr != IntPtr.Zero)
                SCC1_frequency_w = (delegate_SCC1_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_w));
            funcPtr = MameIF.GetProcAddress("SCC1_keyonoff_w");
            if (funcPtr != IntPtr.Zero)
                SCC1_keyonoff_w = (delegate_SCC1_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_w));
            funcPtr = MameIF.GetProcAddress("SCC1_keyonoff_r");
            if (funcPtr != IntPtr.Zero)
                SCC1_keyonoff_r = (delegate_SCC1_r)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_SCC1_r));
        }

        private SCC1SoundManager soundManager;

        private const float DEFAULT_GAIN = 0.7f;

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
        public SCC1(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new SCC1Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new SCC1Timbre();
            setPresetInstruments();

            this.soundManager = new SCC1SoundManager(this);
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
            Timbres[0].WsgData = new sbyte[] { 8 * 16 - 127, 9 * 16 - 127, 11 * 16 - 127, 12 * 16 - 127, 13 * 16 - 127, 14 * 16 - 127, 15 * 16 - 127, 15 * 16 - 127, 15 * 16 - 127, 15 * 16 - 127, 14 * 16 - 127, 14 * 16 - 127, 13 * 16 - 127, 11 * 16 - 127, 10 * 16 - 127, 9 * 16 - 127, 7 * 16 - 127, 6 * 16 - 127, 4 * 16 - 127, 3 * 16 - 127, 2 * 16 - 127, 1 * 16 - 127, 0 * 16 - 127, 0 * 16 - 127, 0 * 16 - 127, 0 * 16 - 127, 1 * 16 - 127, 1 * 16 - 127, 2 * 16 - 127, 4 * 16 - 127, 5 * 16 - 127, 6 };
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
        private class SCC1SoundManager : SoundManagerBase
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

            private static SoundList<SCC1Sound> sccOnSounds = new SoundList<SCC1Sound>(5);

            private SCC1 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SCC1SoundManager(SCC1 parent) : base(parent)
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

                foreach (SCC1Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    SCC1Sound snd = new SCC1Sound(emptySlot.inst, this, timbre, note, emptySlot.slot);
                    sccOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn SCC ch" + emptySlot + " " + note.ToString());
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
            private (SCC1 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, sccOnSounds, note, 5);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 5; i++)
                    Scc1KeyOnOffWriteData(parentModule.UnitNumber, 0);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class SCC1Sound : SoundBase
        {

            private SCC1 parentModule;

            private SCC1Timbre timbre;

            private byte lastWaveTable;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SCC1Sound(SCC1 parentModule, SCC1SoundManager manager, TimbreBase timbre, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (SCC1Timbre)timbre;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                SetTimbre();
                //Freq
                OnPitchUpdated();
                //Volume
                OnVolumeUpdated();

                byte data = Scc1KeyOnOffReadData(parentModule.UnitNumber);
                data |= (byte)(1 << Slot);
                Scc1KeyOnOffWriteData(parentModule.UnitNumber, data);
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                Scc1WriteWaveData(parentModule.UnitNumber, (uint)(Slot << 5), timbre.WsgData);
            }

            /// <summary>
            /// 
            /// </summary>
            protected override void OnProcessFx()
            {
                updateWsgData();

                base.OnProcessFx();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnSoundParamsUpdated()
            {
                base.OnSoundParamsUpdated();

                updateWsgData();

                OnPitchUpdated();

                OnVolumeUpdated();
            }

            private void updateWsgData()
            {
                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (SccFxEngine)FxEngine;
                    if (eng.MorphValue != null)
                    {
                        var no = (byte)(eng.MorphValue.Value & 3);
                        if (lastWaveTable != no)
                        {
                            lastWaveTable = no;
                            sbyte[] wsgData;
                            switch (no)
                            {
                                case 1:
                                    wsgData = timbre.WsgDataMorph1;
                                    break;
                                case 2:
                                    wsgData = timbre.WsgDataMorph2;
                                    break;
                                case 3:
                                    wsgData = timbre.WsgDataMorph3;
                                    break;
                                default:
                                    wsgData = timbre.WsgData;
                                    break;
                            }
                            Scc1WriteWaveData(parentModule.UnitNumber, (uint)(Slot << 5), wsgData);
                        }
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                byte fv = (byte)((int)Math.Round(15 * CalcCurrentVolume()) & 0xf);

                Scc1VolumeWriteData(parentModule.UnitNumber, (uint)Slot, fv);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                double freq = CalcCurrentFrequency();

                /*
                 *                fclock
                 *     ftone = -------------
                 *             32 * (TP + 1)
                 *             
                 *     fclock is the clock frequency of the computer. 3,579,545 Hz
                 */
                // TP = (fclock / (32 * ftone))-1
                freq = Math.Round((3579545 / (32 * freq)) - 1);
                if (freq > 0xfff)
                    freq = 0xfff;
                uint n = (uint)freq;
                Scc1FrequencyWriteData(parentModule.UnitNumber, (uint)((Slot << 1)) + 0, (byte)(n & 0xff));
                Scc1FrequencyWriteData(parentModule.UnitNumber, (uint)((Slot << 1)) + 1, (byte)((n >> 8) & 0xf));

                base.OnPitchUpdated();
            }

            public override void SoundOff()
            {
                base.SoundOff();

                byte data = Scc1KeyOnOffReadData(parentModule.UnitNumber);
                data &= (byte)~(1 << Slot);
                Scc1KeyOnOffWriteData(parentModule.UnitNumber, data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SCC1Timbre>))]
        [DataContract]
        public class SCC1Timbre : TimbreBase
        {

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(8)]
            [DataMember]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 8 bit signed data)")]
            public sbyte[] WsgData
            {
                get;
                set;
            } = new sbyte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(8)]
            [DataMember]
            [Category("Sound")]
            [Description("Morphing Wave Table 1 (32 samples, 8 bit signed data)")]
            public sbyte[] WsgDataMorph1
            {
                get;
                set;
            } = new sbyte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(8)]
            [DataMember]
            [Category("Sound")]
            [Description("Morphing Wave Table 2 (32 samples, 8 bit signed data)")]
            public sbyte[] WsgDataMorph2
            {
                get;
                set;
            } = new sbyte[32];

            [TypeConverter(typeof(ArrayConverter))]
            [Editor(typeof(WsgUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [WsgBitWideAttribute(8)]
            [DataMember]
            [Category("Sound")]
            [Description("Morphing Wave Table 3 (32 samples, 8 bit signed data)")]
            public sbyte[] WsgDataMorph3
            {
                get;
                set;
            } = new sbyte[32];

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
                WsgData = new sbyte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("Wave Table (32 samples, 8 bit signed data)")]
            [IgnoreDataMember]
            [JsonIgnore]
            public string WsgDataSerializeData
            {
                get
                {
                    return createWsgDataSerializeData(WsgData);
                }
                set
                {
                    applyWsgSerializeData(value, WsgData);
                }
            }

            public bool ShouldSerializeWsgDataMorph1()
            {
                foreach (var dt in WsgDataMorph1)
                {
                    if (dt != 0)
                        return true;
                }
                return false;
            }

            public void ResetWsgDataMorph1()
            {
                WsgDataMorph1 = new sbyte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("Morphing Wave Table 1 (32 samples, 8 bit signed data)")]
            [IgnoreDataMember]
            [JsonIgnore]
            public string WsgDataMorph1SerializeData
            {
                get
                {
                    return createWsgDataSerializeData(WsgDataMorph1);
                }
                set
                {
                    applyWsgSerializeData(value, WsgDataMorph1);
                }
            }


            public bool ShouldSerializeWsgDataMorph2()
            {
                foreach (var dt in WsgDataMorph2)
                {
                    if (dt != 0)
                        return true;
                }
                return false;
            }

            public void ResetWsgDataMorph2()
            {
                WsgDataMorph2 = new sbyte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("Morphing Wave Table 2 (32 samples, 8 bit signed data)")]
            [IgnoreDataMember]
            [JsonIgnore]
            public string WsgDataMorph2SerializeData
            {
                get
                {
                    return createWsgDataSerializeData(WsgDataMorph2);
                }
                set
                {
                    applyWsgSerializeData(value, WsgDataMorph2);
                }
            }

            public bool ShouldSerializeWsgDataMorph3()
            {
                foreach (var dt in WsgDataMorph3)
                {
                    if (dt != 0)
                        return true;
                }
                return false;
            }

            public void ResetWsgDataMorph3()
            {
                WsgDataMorph3 = new sbyte[32];
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
            [Category("Sound")]
            [Description("Morphing Wave Table 3 (32 samples, 8 bit signed data)")]
            [IgnoreDataMember]
            [JsonIgnore]
            public string WsgDataMorph3SerializeData
            {
                get
                {
                    return createWsgDataSerializeData(WsgDataMorph3);
                }
                set
                {
                    applyWsgSerializeData(value, WsgDataMorph3);
                }
            }


            private static void applyWsgSerializeData(string value, sbyte[] data)
            {
                string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var vs = new List<sbyte>();
                foreach (var val in vals)
                {
                    sbyte v = 0;
                    if (sbyte.TryParse(val, out v))
                        vs.Add(v);
                }
                for (int i = 0; i < Math.Min(data.Length, vs.Count); i++)
                    data[i] = vs[i];
            }

            private static string createWsgDataSerializeData(sbyte[] data)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    if (sb.Length != 0)
                        sb.Append(' ');
                    sb.Append(data[i].ToString((IFormatProvider)null));
                }
                return sb.ToString();
            }

            public SCC1Timbre()
            {
                this.SDS.FxS = new SccFxSettings();


            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SCC1Timbre>(serializeData);
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



        [JsonConverter(typeof(NoTypeConverterJsonConverter<SccFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [MidiHook]
        public class SccFxSettings : BasicFxSettings
        {

            private string f_MorphEnvelopes;

            [DataMember]
            [Description("Set wave table number by text. Input wave table number and split it with space like the Famitracker.\r\n" +
                       "0-3(0 is normal table) \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 3)]
            public string MorphEnvelopes
            {
                get
                {
                    return f_MorphEnvelopes;
                }
                set
                {
                    if (f_MorphEnvelopes != value)
                    {
                        MorphEnvelopesRepeatPoint = -1;
                        MorphEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            MorphEnvelopesNums = new int[] { };
                            f_MorphEnvelopes = string.Empty;
                            return;
                        }
                        f_MorphEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                MorphEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                MorphEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 3)
                                        v = 3;
                                    vs.Add(v);
                                }
                            }
                        }
                        MorphEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < MorphEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (MorphEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (MorphEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            sb.Append(MorphEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_MorphEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeDutyEnvelopes()
            {
                return !string.IsNullOrEmpty(MorphEnvelopes);
            }

            public void ResetDutyEnvelopes()
            {
                MorphEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] MorphEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int MorphEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int MorphEnvelopesReleasePoint { get; set; } = -1;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new SccFxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class SccFxEngine : BasicFxEngine
        {
            private SccFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public SccFxEngine(SccFxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_morphCounter;

            public byte? MorphValue
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);

                MorphValue = null;
                if (settings.MorphEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.MorphEnvelopesNums.Length;
                        if (settings.MorphEnvelopesReleasePoint >= 0)
                            vm = settings.MorphEnvelopesReleasePoint;
                        if (f_morphCounter >= vm)
                        {
                            if (settings.MorphEnvelopesRepeatPoint >= 0)
                                f_morphCounter = (uint)settings.MorphEnvelopesRepeatPoint;
                            else
                                f_morphCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        if (settings.MorphEnvelopesReleasePoint < 0)
                            f_morphCounter = (uint)settings.MorphEnvelopesNums.Length;

                        //if (f_dutyCounter >= settings.DutyEnvelopesNums.Length)
                        //{
                        //    if (settings.DutyEnvelopesRepeatPoint >= 0)
                        //        f_dutyCounter = (uint)settings.DutyEnvelopesRepeatPoint;
                        //}
                    }
                    if (f_morphCounter < settings.MorphEnvelopesNums.Length)
                    {
                        int vol = settings.MorphEnvelopesNums[f_morphCounter++];

                        MorphValue = (byte)vol;
                        process = true;
                    }
                }

                return process;
            }
        }
    }
}