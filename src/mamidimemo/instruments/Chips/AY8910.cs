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
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Scci;
using zanac.MAmidiMEmo.VSIF;
using static zanac.MAmidiMEmo.Instruments.Chips.YMF262;

//http://ngs.no.coocan.jp/doc/wiki.cgi/TechHan?page=1%BE%CF+PSG%A4%C8%B2%BB%C0%BC%BD%D0%CE%CF
//https://w.atwiki.jp/msx-sdcc/pages/45.html
//http://f.rdw.se/AY-3-8910-datasheet.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
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


        private PortId portId = PortId.No1;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set FTDI or COM Port No for \"VSIF - MSX\".\r\n" +
            "See the manual about the VSIF.")]
        [DefaultValue(PortId.No1)]
        public PortId PortId
        {
            get
            {
                return portId;
            }
            set
            {
                if (portId != value)
                {
                    portId = value;
                    setSoundEngine(SoundEngine);
                }
            }
        }

        private VsifClient vsifClient;

        private object sndEnginePtrLock = new object();

        private IntPtr spfmPtr;

        private SoundEngineType f_SoundEngineType;

        private SoundEngineType f_CurrentSoundEngineType;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Select a sound engine type.\r\n" +
            "Supports Software and SPFM and VSIF - MSX.")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeAY8910))]
        public SoundEngineType SoundEngine
        {
            get
            {
                return f_SoundEngineType;
            }
            set
            {
                if (f_SoundEngineType != value &&
                    (value == SoundEngineType.Software ||
                    value == SoundEngineType.SPFM ||
                    value == SoundEngineType.VSIF_MSX_FTDI ||
                    value == SoundEngineType.VSIF_P6_FTDI))
                {
                    setSoundEngine(value);
                }
            }
        }

        private class EnumConverterSoundEngineTypeAY8910 : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.SPFM,
                    SoundEngineType.VSIF_MSX_FTDI,
                    SoundEngineType.VSIF_P6_FTDI,
                });

                return sc;
            }
        }

        private void setSoundEngine(SoundEngineType value)
        {
            AllSoundOff();

            lock (sndEnginePtrLock)
            {
                if (spfmPtr != IntPtr.Zero)
                {
                    ScciManager.ReleaseSoundChip(spfmPtr);
                    spfmPtr = IntPtr.Zero;
                }
                if (vsifClient != null)
                {
                    vsifClient.Dispose();
                    vsifClient = null;
                }

                f_SoundEngineType = value;

                switch (f_SoundEngineType)
                {
                    case SoundEngineType.Software:
                        f_CurrentSoundEngineType = f_SoundEngineType;
                        SetDevicePassThru(false);
                        break;
                    case SoundEngineType.SPFM:
                        spfmPtr = ScciManager.TryGetSoundChip(SoundChipType.SC_TYPE_AY8910, (SC_CHIP_CLOCK)MasterClock);
                        if (spfmPtr != IntPtr.Zero)
                        {
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.VSIF_MSX_FTDI:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_FTDI, PortId, false);
                        if (vsifClient != null)
                        {
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.VSIF_P6_FTDI:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.P6_FTDI, PortId, false);
                        if (vsifClient != null)
                        {
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                }
                ClearWrittenDataCache();
                PrepareSound();
            }
        }

        [Category("Chip(Dedicated)")]
        [Description("Current sound engine type.")]
        [DefaultValue(SoundEngineType.Software)]
        public SoundEngineType CurrentSoundEngine
        {
            get
            {
                return f_CurrentSoundEngineType;
            }
        }

        private int f_ftdiClkWidth = VsifManager.FTDI_BAUDRATE_MSX_CLK_WIDTH;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [SlideParametersAttribute(1, 100)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue(VsifManager.FTDI_BAUDRATE_MSX_CLK_WIDTH)]
        [Description("Set FTDI Clock Width[%].")]
        public int FtdiClkWidth
        {
            get
            {
                return f_ftdiClkWidth;
            }
            set

            {
                f_ftdiClkWidth = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum MasterClockType : uint
        {
            Default = 1789773,
            NEC = 1996800,
            OPNA = 2000000,
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
                    SetClock(UnitNumber, (uint)value);
                    if (CurrentSoundEngine == SoundEngineType.SPFM)
                        setSoundEngine(f_SoundEngineType);
                }
            }
        }

        public bool ShouldSerializeMasterClock()
        {
            return MasterClock != (uint)MasterClockType.Default;
        }

        public void ResetMasterClock()
        {
            MasterClock = (uint)MasterClockType.Default;
        }


        private byte f_EnvelopeFrequencyCoarse = 2;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
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
                    Ay8910WriteData(UnitNumber, 12, f_EnvelopeFrequencyCoarse);
                    Ay8910WriteData(UnitNumber, 11, EnvelopeFrequencyFine);
                }
            }
        }

        private byte f_EnvelopeFrequencyFine;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
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
                    Ay8910WriteData(UnitNumber, 12, EnvelopeFrequencyCoarse);
                    Ay8910WriteData(UnitNumber, 11, value);
                }
            }
        }

        private byte f_EnvelopeType;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
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
                    Ay8910WriteData(UnitNumber, 13, EnvelopeType);
                }
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
                Timbres = (AY8910Timbre[])value;
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
        public AY8910Timbre[] Timbres
        {
            get;
            set;
        }

        private const float DEFAULT_GAIN = 1.5f;

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
        private delegate void delegate_ay8910_address_data_w(uint unitNumber, uint offset, byte data);


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
        /// <param name="address"></param>
        /// <param name="data"></param>
        internal override void DirectAccessToChip(uint address, uint data)
        {
            if(address < 14)
            {
                if (address == 7)
                    data &= 0x3f;
                Ay8910WriteData(UnitNumber, (byte)address, (byte)data, !(address == 13));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="offset"></param>
        /// <param name="data"></param>
        private void Ay8910WriteData(uint unitNumber, uint offset, byte data)
        {
            Ay8910WriteData(unitNumber, offset, data, true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Ay8910WriteData(uint unitNumber, uint offset, byte data, bool useCache)
        {
            WriteData(offset, data, useCache, new Action(() =>
            {
                lock (sndEnginePtrLock)
                {
                    switch (CurrentSoundEngine)
                    {
                        case SoundEngineType.SPFM:
                            ScciManager.SetRegister(spfmPtr, offset, data, false);
                            break;
                        case SoundEngineType.VSIF_MSX_FTDI:
                        case SoundEngineType.VSIF_P6_FTDI:
                            vsifClient.WriteData(0, (byte)offset, (byte)data, f_ftdiClkWidth);
                            break;
                    }

                    DeferredWriteData(ay8910_address_data_w, unitNumber, (uint)0, (byte)offset);
                    DeferredWriteData(ay8910_address_data_w, unitNumber, (uint)1, data);

                    //try
                    //{
                    //    Program.SoundUpdating();
                    //    YM2608_write(unitNumber, yreg + 0, (byte)(address + (op * 4) + (slot % 3)));
                    //    YM2608_write(unitNumber, yreg + 1, data);
                    //}
                    //finally
                    //{
                    //    Program.SoundUpdated();
                    //}
                }
            }));

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
        private byte Ay8910ReadData(uint unitNumber, uint offset)
        {
            try
            {
                Program.SoundUpdating();
                FlushDeferredWriteData();

                var wd = GetCachedWrittenData(offset);
                if (wd != null)
                    return (byte)wd.Value;

                if (CurrentSoundEngine == SoundEngineType.SPFM)
                {
                    lock (sndEnginePtrLock)
                        return (byte)ScciManager.GetWrittenRegisterData(spfmPtr, offset);
                }
                else
                {
                    ay8910_address_data_w(UnitNumber, 0, (byte)offset);
                    return ay8910_read_ym(unitNumber);
                }
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
            MasterClock = (uint)MasterClockType.Default;

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

            lock (sndEnginePtrLock)
            {
                if (spfmPtr != IntPtr.Zero)
                {
                    ScciManager.ReleaseSoundChip(spfmPtr);
                    spfmPtr = IntPtr.Zero;
                }
                if (vsifClient != null)
                {
                    vsifClient.Dispose();
                }
            }
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        private void initGlobalRegisters()
        {
            Ay8910WriteData(UnitNumber, 7, (byte)(0x3f));

            Ay8910WriteData(UnitNumber, 13, EnvelopeType);
            Ay8910WriteData(UnitNumber, 12, EnvelopeFrequencyCoarse);
            Ay8910WriteData(UnitNumber, 11, EnvelopeFrequencyFine);
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
            Timbres[0].SoundType = SoundType.PSG;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(TaggedNoteOnEvent midiEvent)
        {
            soundManager.ProcessKeyOn(midiEvent);
            /*
            var rp = InstrumentManager.GetInstruments(6).ToArray()[0];
            var ay = InstrumentManager.GetInstruments(11).ToArray()[0];

            for (int i = 0; i < 256; i++)
            {
                RP2A03.RP2A03Timbre rpt = (RP2A03.RP2A03Timbre)rp.BaseTimbres[i];
                AY8910.AY8910Timbre ayt = (AY8910.AY8910Timbre)ay.BaseTimbres[i];

                switch (rpt.ToneType)
                {
                    case RP2A03.ToneType.SQUARE:
                        ayt.SoundType = SoundType.PSG;
                        break;
                    case RP2A03.ToneType.NOISE:
                        ayt.SoundType = SoundType.NOISE;
                        break;
                    case RP2A03.ToneType.TRIANGLE:
                        Debug.WriteLine(i);
                        ayt.SoundType = SoundType.PSG;
                        break;
                    default:
                        ayt.SoundType = SoundType.ENVELOPE;
                        break;
                }
                ayt.SDS = rpt.SDS;
                ayt.TimbreName = rpt.TimbreName;
            }

            for (int idx = 0; idx < 128; idx++)
            {
                if (rp.DrumTimbres[idx].TimbreNumber != null)
                {
                    ay.DrumTimbres[idx] = rp.DrumTimbres[idx];
                }
            }
            */
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
            ClearWrittenDataCache();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ClearWrittenDataCache()
        {
            base.ClearWrittenDataCache();
            initGlobalRegisters();
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

                int tindex = 0;
                foreach (AY8910Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    AY8910Sound snd = new AY8910Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
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
            private (AY8910 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, AY8910Timbre timbre)
            {
                var emptySlot = (parentModule, -1);

                //if (((int)(((AY8910Timbre)timbre).SoundType) & 0x3) != 0)
                emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 3);

                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                parentModule.Ay8910WriteData(parentModule.UnitNumber, 7, 0x3f);
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
            public AY8910Sound(AY8910 parentModule, AY8910SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
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

                if (((int)lastSoundType & 4) != 0)
                    parentModule.Ay8910WriteData(parentModule.UnitNumber, 13, parentModule.EnvelopeType, false);
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

                if (((int)st & 4) == 0)
                    // PSG/Noise
                    parentModule.Ay8910WriteData(parentModule.UnitNumber, (uint)(8 + Slot), fv);
                else
                    //Envelope
                    parentModule.Ay8910WriteData(parentModule.UnitNumber, (uint)(8 + Slot), (byte)(0x10 | fv));

                //key on
                byte data = parentModule.Ay8910ReadData(parentModule.UnitNumber, 7);
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
                parentModule.Ay8910WriteData(parentModule.UnitNumber, 7, data);
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
                byte data = parentModule.Ay8910ReadData(parentModule.UnitNumber, 7);
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
                parentModule.Ay8910WriteData(parentModule.UnitNumber, 7, data);

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

                            var EP = (int)Math.Round((double)parentModule.MasterClock / (256d * fm));

                            parentModule.f_EnvelopeFrequencyCoarse = (byte)(EP >> 8);
                            parentModule.f_EnvelopeFrequencyFine = (byte)(EP & 0xff);
                        }
                    }

                    parentModule.Ay8910WriteData(parentModule.UnitNumber, 12, parentModule.EnvelopeFrequencyCoarse);
                    parentModule.Ay8910WriteData(parentModule.UnitNumber, 11, parentModule.EnvelopeFrequencyFine);
                    parentModule.Ay8910WriteData(parentModule.UnitNumber, 13, parentModule.EnvelopeType);
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
                freq = Math.Round(((double)parentModule.MasterClock / 16d) / freq);
                if (freq > 0xfff)
                    freq = 0xfff;
                ushort tp = (ushort)freq;

                parentModule.Ay8910WriteData(parentModule.UnitNumber, (uint)(0 + (Slot * 2)), (byte)(tp & 0xff));
                parentModule.Ay8910WriteData(parentModule.UnitNumber, (uint)(1 + (Slot * 2)), (byte)((tp >> 8) & 0xf));
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

                parentModule.Ay8910WriteData(parentModule.UnitNumber, 6, (byte)n);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                byte data = parentModule.Ay8910ReadData(parentModule.UnitNumber, 7);
                data |= (byte)((1 | 8) << Slot);
                parentModule.Ay8910WriteData(parentModule.UnitNumber, 7, data);

                parentModule.Ay8910WriteData(parentModule.UnitNumber, (uint)(8 + Slot), 0);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<AY8910Timbre>))]
        [DataContract]
        [InstLock]
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
            [Category("Chip(Global)")]
            [Description("Global Settings")]
            public AY8910GlobalSettings GlobalSettings
            {
                get;
                set;
            }

            public virtual bool ShouldSerializeGlobalSettings()
            {
                return !string.Equals(JsonConvert.SerializeObject(GlobalSettings, Formatting.Indented), "{}", StringComparison.Ordinal);
            }

            public virtual void ResetGlobalSettings()
            {
                GlobalSettings.InjectFrom(new LoopInjection(), new AY8910GlobalSettings());
            }

            public AY8910Timbre()
            {
                GlobalSettings = new AY8910GlobalSettings();
                SoundType = SoundType.PSG;
            }

            protected override void InitializeFxS()
            {
                this.SDS.FxS = new AyFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<AY8910Timbre>(serializeData);
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

        [JsonConverter(typeof(NoTypeConverterJsonConverter<AyFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class AyFxSettings : BasicFxSettings
        {

            private string f_SoundTypeEnvelopes;

            [DataMember]
            [Description("Set dutysound type envelop by text. Input sound type value and split it with space like the FamiTracker.\r\n" +
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
                            if(i < SoundTypeEnvelopesNums.Length)
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
                        //if (settings.SoundTypeEnvelopesReleasePoint < 0)
                        //    f_SoundType = (uint)settings.SoundTypeEnvelopesNums.Length;

                        if (f_SoundType >= settings.SoundTypeEnvelopesNums.Length)
                        {
                            if (settings.SoundTypeEnvelopesReleasePoint >= 0 && f_SoundType <= (uint)settings.SoundTypeEnvelopesReleasePoint)
                                f_SoundType = (uint)settings.SoundTypeEnvelopesReleasePoint;
                            else if (settings.SoundTypeEnvelopesReleasePoint < 0)
                                f_SoundType = (uint)settings.SoundTypeEnvelopesNums.Length;
                        }
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
        [InstLock]
        public class AY8910GlobalSettings : ContextBoundObject
        {
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Override global settings")]
            [DefaultValue(false)]
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
            [Category("Chip(Global)")]
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
            [Category("Chip(Global)")]
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
            [Category("Chip(Global)")]
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
            [Category("Chip(Global)")]
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
            [Category("Chip(Global)")]
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