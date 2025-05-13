// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using zanac.MAmidiMEmo.Gimic;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Scci;
using zanac.MAmidiMEmo.VSIF;

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class TIA : InstrumentBase
    {

        public override string Name => "TIA";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.TIA;

        [Browsable(false)]
        public override string ImageKey => "TIA";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "tia_";

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
                return 35;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum MasterClockType : uint
        {
            NTSC = 3579545,
            PAL = 3546894,
        }

        private uint f_MasterClock;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set Master Clock of this chip.")]
        [TypeConverter(typeof(EnumConverter<MasterClockType>))]
        public uint MasterClock
        {
            get
            {
                return f_MasterClock;
            }
            set
            {
                if (f_MasterClock != value)
                {
                    f_MasterClock = value;
                    SetClock(UnitNumber, (uint)value / 114);
                }
            }
        }

        public bool ShouldSerializeMasterClock()
        {
            return MasterClock != (uint)MasterClockType.NTSC;
        }

        public void ResetMasterClock()
        {
            MasterClock = (uint)MasterClockType.NTSC;
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
                Timbres = (TiaTimbre[])value;
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
        public TiaTimbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<TIA>(serializeData))
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad" }), obj);
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
        private delegate void delegate_tia_device_write(uint unitNumber, uint offset, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_tia_device_write tia_device_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        internal override void DirectAccessToChip(uint address, uint data)
        {
            TIAWriteData(UnitNumber, (byte)address, (byte)data, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="offset"></param>
        /// <param name="data"></param>
        private void TIAWriteData(uint unitNumber, uint offset, byte data)
        {
            TIAWriteData(unitNumber, offset, data, true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void TIAWriteData(uint unitNumber, uint offset, byte data, bool useCache)
        {
            WriteData(offset, data, useCache, new Action(() =>
            {
                DeferredWriteData(tia_device_write, unitNumber, (uint)offset, (byte)data);
            }));

            /*
            try
            {
                Program.SoundUpdating();
                saa1099_device_write(unitNumber, offset, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        static TIA()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("tia_device_write");
            if (funcPtr != IntPtr.Zero)
            {
                tia_device_write = (delegate_tia_device_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_tia_device_write));
            }
        }

        private TiaSoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public TIA(uint unitNumber) : base(unitNumber)
        {
            MasterClock = (uint)MasterClockType.NTSC;

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new TiaTimbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new TiaTimbre();
            setPresetInstruments();

            this.soundManager = new TiaSoundManager(this);
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

            initGlobalRegisters();
        }

        private void initGlobalRegisters()
        {
            //Volume 0
            TIAWriteData(UnitNumber, 0x19, (byte)0);
            TIAWriteData(UnitNumber, 0x1a, (byte)0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vgmPath"></param>
        public override void StartVgmRecordingTo(string vgmPath)
        {
            base.StartVgmRecordingTo(vgmPath);

            initGlobalRegisters();
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {

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
            soundManager?.ProcessAllSoundOff();
        }

        internal override void ResetAll()
        {
            ClearWrittenDataCache();
            PrepareSound();
        }

        /// <summary>
        /// 
        /// </summary>
        private class TiaSoundManager : SoundManagerBase
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

            private static SoundList<TiaSound> psgOnSounds = new SoundList<TiaSound>(2);

            private TIA parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public TiaSoundManager(TIA parent) : base(parent)
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
                foreach (TiaTimbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    TiaSound snd = new TiaSound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    psgOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn ch" + emptySlot + " " + note.ToString());
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
            private (TIA inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, TiaTimbre timbre)
            {
                var emptySlot = (parentModule, -1);

                emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 2);

                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                parentModule.TIAWriteData(parentModule.UnitNumber, 0x19, 0);
                parentModule.TIAWriteData(parentModule.UnitNumber, 0x1a, 0);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class TiaSound : SoundBase
        {

            private TIA parentModule;

            private TiaTimbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public TiaSound(TIA parentModule, TiaSoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (TiaTimbre)timbre;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                parentModule.TIAWriteData(parentModule.UnitNumber, (uint)(0x15 + Slot), timbre.SoundTypes);

                base.KeyOn();

                OnPitchUpdated();
                //KEY ON
                OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            protected override void OnProcessFx()
            {
                OnSoundParamsUpdated();

                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (TiaFxEngine)FxEngine;
                    if (eng.SoundTypes != null)
                    {
                        parentModule.TIAWriteData(parentModule.UnitNumber, (uint)(0x15 + Slot), (byte)eng.SoundTypes);
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                if (IsSoundOff)
                    return;

                byte vl = (byte)Math.Round(15d * CalcCurrentVolume());

                parentModule.TIAWriteData(parentModule.UnitNumber, (uint)(0x19 + Slot), vl);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                //AUDF = (クロック / (32 × 周波数)) -1
                uint freq = (uint)Math.Round( (((double)parentModule.MasterClock / 3d) / (114d * CalcCurrentFrequency())) - 1);
                if (freq > 0x1f)
                    freq = 0x1f;

                parentModule.TIAWriteData(parentModule.UnitNumber, (uint)(0x17 + Slot), (byte)freq);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                parentModule.TIAWriteData(parentModule.UnitNumber, (uint)(0x19 + Slot), 0);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<TiaTimbre>))]
        [DataContract]
        [InstLock]
        public class TiaTimbre : TimbreBase
        {
            [Browsable(false)]
            public override bool AssignMIDIChtoSlotNum
            {
                get;
                set;
            }

            [Browsable(false)]
            public override int AssignMIDIChtoSlotNumOffset
            {
                get;
                set;
            }

            private byte f_SoundTypes = 4;

            [DataMember]
            [Description("Set Sound Types. Input sound type value and split it with space like the FamiTracker.\r\n" +
                       "0-15:Sound Type \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 15)]
            [DefaultValue((byte)4)]
            public byte SoundTypes
            {
                get
                {
                    return f_SoundTypes;
                }
                set
                {
                    f_SoundTypes = value;
                }
            }

            protected override void InitializeFxS()
            {
                this.SDS.FxS = new TiaFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<TiaTimbre>(serializeData);
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad" }), obj);
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

        [JsonConverter(typeof(NoTypeConverterJsonConverter<TiaFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class TiaFxSettings : BasicFxSettings
        {

            private string f_SoundTypes;

            [DataMember]
            [Description("Set Sound Types. Input sound type value and split it with space like the FamiTracker.\r\n" +
                       "0-15:Sound Type \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 15)]
            public string SoundTypes
            {
                get
                {
                    return f_SoundTypes;
                }
                set
                {
                    if (f_SoundTypes != value)
                    {
                        SoundTypesRepeatPoint = -1;
                        SoundTypesReleasePoint = -1;
                        if (value == null)
                        {
                            SoundTypesNums = new int[] { };
                            f_SoundTypes = string.Empty;
                            return;
                        }
                        f_SoundTypes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                SoundTypesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                SoundTypesReleasePoint = vs.Count;
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
                        SoundTypesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < SoundTypesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (SoundTypesRepeatPoint == i)
                                sb.Append("| ");
                            if (SoundTypesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < SoundTypesNums.Length)
                                sb.Append(SoundTypesNums[i].ToString((IFormatProvider)null));
                        }
                        f_SoundTypes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeSoundTypes()
            {
                return !string.IsNullOrEmpty(SoundTypes);
            }

            public void ResetSoundTypes()
            {
                SoundTypes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] SoundTypesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int SoundTypesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int SoundTypesReleasePoint { get; set; } = -1;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new TiaFxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class TiaFxEngine : BasicFxEngine
        {
            private TiaFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public TiaFxEngine(TiaFxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_SoundTypesCounter;

            public byte? SoundTypes
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);


                SoundTypes = null;
                if (settings.SoundTypesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.SoundTypesNums.Length;
                        if (settings.SoundTypesReleasePoint >= 0)
                            vm = settings.SoundTypesReleasePoint;
                        if (f_SoundTypesCounter >= vm)
                        {
                            if (settings.SoundTypesRepeatPoint >= 0)
                                f_SoundTypesCounter = (uint)settings.SoundTypesRepeatPoint;
                            else
                                f_SoundTypesCounter = (uint)vm - 1;
                        }
                    }
                    else
                    {
                        if (f_SoundTypesCounter < settings.SoundTypesNums.Length)
                        {
                            if (settings.SoundTypesReleasePoint >= 0 && f_SoundTypesCounter <= (uint)settings.SoundTypesReleasePoint)
                                f_SoundTypesCounter = (uint)settings.SoundTypesReleasePoint;
                            else if (settings.SoundTypesReleasePoint < 0 && settings.KeyOffStop)
                                f_SoundTypesCounter = (uint)settings.SoundTypesNums.Length;
                        }
                    }
                    if (f_SoundTypesCounter < settings.SoundTypesNums.Length)
                    {
                        int vol = settings.SoundTypesNums[f_SoundTypesCounter++];

                        SoundTypes = (byte)vol;
                        process = true;
                    }
                }

                return process;
            }
        }
    }
}