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
using zanac.MAmidiMEmo.VSIF;
using static zanac.MAmidiMEmo.Instruments.Chips.C140;

//https://www.waitingforfriday.com/?p=661#6581_SID_Block_Diagram
//http://www.bellesondes.fr/wiki/doku.php?id=mos6581#mos6581_sound_interface_device_sid
//https://www.sfpgmr.net/blog/entry/mos-sid-6581を調べた.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public abstract class SIDBase : InstrumentBase
    {

        public override string Group => "PSG";


        private PortId portId = PortId.No1;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set FTDI No for \"VSIF - C64\"\r\n" +
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

        private object sndEnginePtrLock = new object();

        private VsifClient vsifClient;

        private SoundEngineType f_SoundEngineType;

        private SoundEngineType f_CurrentSoundEngineType;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Select a sound engine type.\r\n" +
            "Supports \"Software\" and \"VSIF - C64\"")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeC64))]
        public SoundEngineType SoundEngine
        {
            get
            {
                return f_SoundEngineType;
            }
            set
            {
                if (f_SoundEngineType != value)
                    setSoundEngine(value);
            }
        }

        [Category("Chip(Dedicated)")]
        [Description("Current sound engine type.")]
        [DefaultValue(SoundEngineType.Software)]
        [RefreshProperties(RefreshProperties.All)]
        public SoundEngineType CurrentSoundEngine
        {
            get
            {
                return f_CurrentSoundEngineType;
            }
        }

        private void setSoundEngine(SoundEngineType value)
        {
            AllSoundOff();

            lock (sndEnginePtrLock)
            {
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
                    case SoundEngineType.VSIF_C64_FTDI:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.C64_FTDI, PortId, false);
                        if (vsifClient != null)
                        {
                            if (vsifClient.DataWriter.FtdiDeviceType == FTD2XX_NET.FTDI.FT_DEVICE.FT_DEVICE_232R)
                            {
                                if (FtdiClkWidth < 25)
                                    FtdiClkWidth = 25;
                            }
                            else
                            {
                                if (FtdiClkWidth < 32)
                                    FtdiClkWidth = 32;
                            }

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
            }

            ClearWrittenDataCache();
            PrepareSound();
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        private void initGlobalRegisters()
        {
            SidWriteData(UnitNumber, 0x15, (byte)(f_FC & 0x7), (byte)(f_FC >> 3));
            SidWriteData(UnitNumber, 0x17, (byte)(f_Off3 << 7 | f_RES << 4 | (int)f_FILT));
            SidWriteData(UnitNumber, 0x18, (byte)((int)f_FilterType << 4 | (int)f_Volume));
        }

        private SidBaseAddressType f_SidBaseAddress;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("SID Address offset")]
        [DefaultValue(SidBaseAddressType.D400)]
        public SidBaseAddressType SidBaseAddress
        {
            get => f_SidBaseAddress;
            set
            {
                if (f_SidBaseAddress != value)
                {
                    f_SidBaseAddress = value;
                    setSoundEngine(SoundEngine);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum SidBaseAddressType
        {
            D400 = 0x00,
            D420 = 0x20,
            D440 = 0x40,
            D460 = 0x60,
            D480 = 0x80,
            D4A0 = 0xA0,
            D4C0 = 0xC0,
            D4E0 = 0xE0,
        }

        private int f_ftdiClkWidth = VsifManager.FTDI_BAUDRATE_C64_CLK_WIDTH;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [SlideParametersAttribute(1, 100)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue(VsifManager.FTDI_BAUDRATE_C64_CLK_WIDTH)]
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
            PAL = 985248,
            NTSC = 1022272,
        }

        private uint f_MasterClock;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set Master Clock of this chip")]
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
                }
            }
        }

        public bool ShouldSerializeMasterClock()
        {
            return MasterClock != (uint)MasterClockType.PAL;
        }

        public void ResetMasterClock()
        {
            MasterClock = (uint)MasterClockType.PAL;
        }

        private byte f_RES;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Resonance (0-15)")]
        [SlideParametersAttribute(0, 15)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte RES
        {
            get => f_RES;
            set
            {
                if (f_RES != value)
                {
                    f_RES = (byte)(value & 15);
                    SidWriteData(UnitNumber, 0x17, (byte)(f_RES << 4 | (int)FILT));
                }
            }
        }

        private ushort f_FC;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Cutoff (or Center) Frequency (0-2047)(30Hz - 10KHz)")]
        [SlideParametersAttribute(0, 2047)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public ushort FC
        {
            get => f_FC;
            set
            {
                var v = (ushort)(value & 2047);
                if (f_FC != v)
                {
                    f_FC = v;

                    SidWriteData(UnitNumber, 0x15, (byte)(f_FC & 0x7), (byte)(f_FC >> 3));
                    //SidWriteData(UnitNumber, 0x15, (byte)(f_FC & 0x7));
                    //SidWriteData(UnitNumber, 0x16, (byte)(f_FC >> 3));
                }
            }
        }

        public bool ShouldSerializeFC()
        {
            return FC != 0;
        }

        public void ResetFC()
        {
            FC = 0;
        }

        private FilterChannel f_FILT;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [DefaultValue(FilterChannel.None)]
        [TypeConverter(typeof(FlagsEnumConverter))]
        [Description("Apply Filter Ch")]
        public FilterChannel FILT
        {
            get => f_FILT;
            set
            {
                if (f_FILT != value)
                {
                    f_FILT = value;
                    SidWriteData(UnitNumber, 0x17, (byte)(OFF3 << 7 | RES << 4 | (int)f_FILT));
                }
            }
        }


        private byte f_Off3;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [Description("Disable ch 3 sound (0:Enable 1:Disable)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte OFF3
        {
            get
            {
                return f_Off3;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_Off3 != v)
                {
                    f_Off3 = v;
                    SidWriteData(UnitNumber, 0x18, (byte)(f_Off3 << 7 | (int)FilterType << 4 | Volume));
                }
            }
        }

        private FilterTypes f_FilterType;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip")]
        [DefaultValue(FilterTypes.None)]
        [Description("Filter Type")]
        [TypeConverter(typeof(FlagsEnumConverter))]
        public FilterTypes FilterType
        {
            get => f_FilterType;
            set
            {
                if (f_FilterType != value)
                {
                    f_FilterType = value;
                    SidWriteData(UnitNumber, 0x18, (byte)(OFF3 << 7 | (int)f_FilterType << 4 | Volume));
                }
            }
        }

        private byte f_Volume;

        [IgnoreDataMember]
        [JsonIgnore]
        [Browsable(false)]
        public byte Volume
        {
            get => f_Volume;
            set
            {
                byte v = (byte)(value & 15);
                if (f_Volume != v)
                {
                    f_Volume = v;
                    SidWriteData(UnitNumber, 0x18, (byte)((int)FilterType << 4 | (int)f_Volume));
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("General")]
        [Description("Set LFO and Portament processing interval[ms]. If you use a real hardware, please increase to appropriate value.")]
        [DefaultValue(10d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(1d, 50d, 1d)]
        public override double ProcessingInterval
        {
            get
            {
                return base.ProcessingInterval;
            }
            set
            {
                base.ProcessingInterval = value;
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
            set
            {
                Timbres = (SIDTimbre[])value;
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
        public SIDTimbre[] Timbres
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
                var obj = JsonConvert.DeserializeObject(serializeData, this.GetType());
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
        private delegate void delegate_sid_write(uint unitNumber, int address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private delegate_sid_write Sid_write
        {
            get;
            set;
        }

        [Browsable(false)]
        protected abstract string WriteProcName
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SidWriteData(uint unitNumber, int address, byte data)
        {
            WriteData((uint)address, data, true, new Action(() =>
            {
                lock (sndEnginePtrLock)
                {
                    if (CurrentSoundEngine == SoundEngineType.VSIF_C64_FTDI)
                    {
                        vsifClient?.WriteData(0, (byte)(address + SidBaseAddress), data, f_ftdiClkWidth);
                    }
                }
                DeferredWriteData(Sid_write, unitNumber, address, data);
            }));
            /*
            try
            {
                Program.SoundUpdating();
                Sid_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private void SidWriteData(uint unitNumber, int address, byte data1, byte data2)
        {
            uint adrs = (uint)address;
            if (GetCachedWrittenData(adrs) != data1 &&
                GetCachedWrittenData(adrs + 1) != data2)
            {
                WriteData(adrs, data1, true, null);
                WriteData(adrs + 1, data2, true, null);
                lock (sndEnginePtrLock)
                {
                    if (CurrentSoundEngine == SoundEngineType.VSIF_C64_FTDI)
                    {
                        vsifClient?.WriteData(1, (byte)(address + SidBaseAddress), new byte[] { data2, data1 }, f_ftdiClkWidth);
                    }
                }
                DeferredWriteData(Sid_write, unitNumber, address, data1);
                DeferredWriteData(Sid_write, unitNumber, address + 1, data2);
            }
            else
            {
                SidWriteData(unitNumber, address, data1);
                SidWriteData(unitNumber, address + 1, data2);
            }


            /*
            try
            {
                Program.SoundUpdating();
                Sid_write(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
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
        public override void Dispose()
        {
            if (Sid_write != null)
                InstrumentBase.RemoveCachedDelegate(Sid_write);
            soundManager?.Dispose();

            lock (sndEnginePtrLock)
            {
                if (vsifClient != null)
                    vsifClient.Dispose();
            }

            base.Dispose();
        }

        private SIDSoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public SIDBase(uint unitNumber) : base(unitNumber)
        {
            FilterMode = FilterMode.LowPass;
            FilterCutoff = 0.9d;
            FilterResonance = 0.1d;

            MasterClock = (uint)MasterClockType.PAL;

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            ProcessingInterval = 10;

            Timbres = new SIDTimbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new SIDTimbre();
            setPresetInstruments();

            IntPtr funcPtr = MameIF.GetProcAddress(WriteProcName);
            if (funcPtr != IntPtr.Zero)
                Sid_write = (delegate_sid_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sid_write));

            this.soundManager = new SIDSoundManager(this);
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
            soundManager.ProcessAllSoundOff();
        }

        internal override void ResetAll()
        {
            ClearWrittenDataCache();
            PrepareSound();
        }

        /// <summary>
        /// 
        /// </summary>
        private class SIDSoundManager : SoundManagerBase
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

            private static SoundList<SIDSound> psgOnSounds = new SoundList<SIDSound>(3);

            private SIDBase parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SIDSoundManager(SIDBase parent) : base(parent)
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
                foreach (SIDTimbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    SIDSound snd = new SIDSound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    psgOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn PSG ch" + emptySlot + " " + note.ToString());
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
            private (SIDBase inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, SIDTimbre timbre)
            {
                var emptySlot = (parentModule, -1);

                switch (timbre.PhysicalChannel)
                {
                    case PhysicalChannel.Indeterminatene:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 3);
                            break;
                        }
                    case PhysicalChannel.Ch1:
                    case PhysicalChannel.Ch2:
                    case PhysicalChannel.Ch3:
                        {
                            emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 1, (int)timbre.PhysicalChannel - 1, 0);
                            break;
                        }
                }
                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 3; i++)
                {
                    parentModule.SidWriteData(parentModule.UnitNumber, i * 7 + 4, 0);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class SIDSound : SoundBase
        {

            private SIDBase parentModule;

            private SIDTimbre timbre;

            private Waveforms lastWaveform;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SIDSound(SIDBase parentModule, SIDSoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (SIDTimbre)timbre;

                lastWaveform = this.timbre.Waveform;
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
                    if (gs.FC.HasValue)
                        parentModule.FC = gs.FC.Value;
                    if (gs.RES.HasValue)
                        parentModule.RES = gs.RES.Value;
                    if (gs.OFF3.HasValue)
                        parentModule.OFF3 = gs.OFF3.Value;
                    if (gs.FILT.HasValue)
                    {
                        FilterChannel2 fc = (FilterChannel2)gs.FILT.Value;
                        if(fc == FilterChannel2.AutoChOnOr)
                            parentModule.FILT |= (FilterChannel)(1 << Slot);
                        else if (fc == FilterChannel2.AutoChOnExc)
                            parentModule.FILT = (FilterChannel)(1 << Slot);
                        else if (fc == FilterChannel2.AutoChOff)
                            parentModule.FILT = parentModule.FILT & ~(FilterChannel)(1 << Slot);
                        else if (fc != FilterChannel2.None)
                            parentModule.FILT = (FilterChannel)fc;
                    }
                    if (gs.FilterType.HasValue)
                        parentModule.FilterType = (FilterTypes)gs.FilterType.Value;
                }

                //HACK: workaround of the SID bug
                //parentModule.SidWriteData(parentModule.UnitNumber, Slot * 7 + 4, 0b1000);

                SetTimbre();
                OnVolumeUpdated();
                OnPitchUpdated();
            }


            public override void OnSoundParamsUpdated()
            {
                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.FC.HasValue)
                        parentModule.FC = gs.FC.Value;
                    if (gs.RES.HasValue)
                        parentModule.RES = gs.RES.Value;
                    if (gs.OFF3.HasValue)
                        parentModule.OFF3 = gs.OFF3.Value;
                    if (gs.FILT.HasValue)
                    {
                        FilterChannel2 fc = (FilterChannel2)gs.FILT.Value;
                        if (fc == FilterChannel2.AutoChOnOr)
                            parentModule.FILT |= (FilterChannel)(1 << Slot);
                        else if (fc == FilterChannel2.AutoChOnExc)
                            parentModule.FILT = (FilterChannel)(1 << Slot);
                        else if (fc == FilterChannel2.AutoChOff)
                            parentModule.FILT = parentModule.FILT & ~(FilterChannel)(1 << Slot);
                        else if (fc != FilterChannel2.None)
                            parentModule.FILT = (FilterChannel)fc;
                    }
                    if (gs.FilterType.HasValue)
                        parentModule.FilterType = (FilterTypes)gs.FilterType.Value;
                }

                SetTimbre();

                base.OnSoundParamsUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                var pn = parentModule.ProgramNumbers[NoteOnEvent.Channel];

                parentModule.SidWriteData(parentModule.UnitNumber, Slot * 7 + 5, (byte)(timbre.ATK << 4 | timbre.DCY), (byte)(timbre.STN << 4 | timbre.RIS));
                //parentModule.SidWriteData(parentModule.UnitNumber, Slot * 7 + 5, (byte)(timbre.ATK << 4 | timbre.DCY));
                //parentModule.SidWriteData(parentModule.UnitNumber, Slot * 7 + 6, (byte)(timbre.STN << 4 | timbre.RIS));
            }


            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                var v = CalcCurrentVolume();

                parentModule.Volume = (byte)Math.Round(15 * v);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                double freq = CalcCurrentFrequency();
                int f = (int)Math.Round(16777216d * freq / parentModule.MasterClock);
                if (f > 0xffff)
                    f = 0xffff;

                var pw = timbre.PW;
                var res = parentModule.RES;
                var fc = parentModule.FC;
                var w = timbre.Waveform;
                var sync = timbre.SYNC;
                var ring = timbre.RING;
                var test = timbre.TEST;
                if (FxEngine != null && FxEngine.Active)
                {
                    var eng = (SidFxEngine)FxEngine;
                    if (eng.Settings.Enable)
                    {
                        if (eng.DutyValue != null)
                            pw = eng.DutyValue.Value;
                        if (eng.WaveFormValue != null)
                            w = eng.WaveFormValue.Value;
                        if (eng.ResonanceValue != null)
                            res = eng.ResonanceValue.Value;
                        if (eng.CutOffValue != null)
                            fc = eng.CutOffValue.Value;

                        if (eng.SyncValue != null)
                            sync = eng.SyncValue.Value;
                        if (eng.RingValue != null)
                            ring = eng.RingValue.Value;
                        if (eng.TestValue != null)
                            ring = eng.TestValue.Value;
                    }
                }
                var un = parentModule.UnitNumber;

                parentModule.RES = res;
                parentModule.FC = fc;

                parentModule.SidWriteData(un, Slot * 7 + 2, (byte)(pw & 0xff), (byte)(pw >> 8));
                //parentModule.SidWriteData(un, Slot * 7 + 2, (byte)(pw & 0xff));
                //parentModule.SidWriteData(un, Slot * 7 + 3, (byte)(pw >> 8));

                parentModule.SidWriteData(un, Slot * 7 + 0, (byte)(f & 0xff), (byte)(f >> 8));
                //parentModule.SidWriteData(un, Slot * 7 + 0, (byte)(f & 0xff));
                //parentModule.SidWriteData(un, Slot * 7 + 1, (byte)(f >> 8));

                byte data = (byte)((int)w << 4 | timbre.TEST << 3 | timbre.RING << 2 | timbre.SYNC << 1 | (IsKeyOff ? 0 : 1));
                parentModule.SidWriteData(un, Slot * 7 + 4, data);

                base.OnPitchUpdated();
            }

            public override void KeyOff()
            {
                base.KeyOff();

                byte data = (byte)((int)timbre.Waveform << 4 | timbre.TEST << 3 | timbre.RING << 2 | timbre.SYNC << 1 | 0);
                parentModule.SidWriteData(parentModule.UnitNumber, Slot * 7 + 4, data);
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                byte data = (byte)((int)timbre.Waveform << 4 | timbre.TEST << 3 | timbre.RING << 2 | timbre.SYNC << 1 | 0);
                parentModule.SidWriteData(parentModule.UnitNumber, Slot * 7 + 4, data);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SIDTimbre>))]
        [DataContract]
        [InstLock]
        public class SIDTimbre : TimbreBase
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

            [DataMember]
            [Category("Sound")]
            [Description("Physical Channel")]
            [DefaultValue(PhysicalChannel.Indeterminatene)]
            public PhysicalChannel PhysicalChannel
            {
                get;
                set;
            }

            private Waveforms f_Waveform = Waveforms.Pulse;

            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(Waveforms.Pulse)]
            [TypeConverter(typeof(FlagsEnumConverter))]
            public Waveforms Waveform
            {
                get
                {
                    return f_Waveform;
                }
                set
                {
                    var f = value;

                    if ((f & Waveforms.Noise) == Waveforms.Noise && (f_Waveform & Waveforms.Noise) != Waveforms.Noise)
                        f &= Waveforms.Noise;
                    else if ((f & (Waveforms)7) != 0 && (f_Waveform & Waveforms.Noise) == Waveforms.Noise)
                        f &= (Waveforms)7;

                    if (f_Waveform != f)
                        f_Waveform = f;
                }
            }

            private byte f_ATK;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((byte)0)]
            [Description("Attack (0-15)")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte ATK
            {
                get
                {
                    return f_ATK;
                }
                set
                {
                    f_ATK = (byte)(value & 15);
                }
            }


            private byte f_DCY = 15;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((byte)15)]
            [Description("Decay (0-15)")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DCY
            {
                get
                {
                    return f_DCY;
                }
                set
                {
                    f_DCY = (byte)(value & 15);
                }
            }

            private byte f_STN = 15;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((byte)15)]
            [Description("Sustain (0-15)")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte STN
            {
                get
                {
                    return f_STN;
                }
                set
                {
                    f_STN = (byte)(value & 15);
                }
            }


            private byte f_RIS;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((byte)0)]
            [Description("Release Rate (0-15)")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte RIS
            {
                get
                {
                    return f_RIS;
                }
                set
                {
                    f_RIS = (byte)(value & 15);
                }
            }


            private ushort f_PW = 2047;

            [DataMember]
            [Category("Sound")]
            [Description("Pulse Width (0-4095)(0% - 100%)")]
            [SlideParametersAttribute(0, 4095)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public ushort PW
            {
                get
                {
                    return f_PW;
                }
                set
                {
                    f_PW = (ushort)(value & 4095);
                }
            }

            public bool ShouldSerializePW()
            {
                return PW != 2047;
            }

            public void ResetPW()
            {
                PW = 2047;
            }

            private byte f_RING;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((byte)0)]
            [Description("Ring Modulation (0-1)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte RING
            {
                get
                {
                    return f_RING;
                }
                set
                {
                    f_RING = (byte)(value & 1);
                }
            }

            private byte f_SYNC;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((byte)0)]
            [Description("Synchronize Oscillator (0-1)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte SYNC
            {
                get
                {
                    return f_SYNC;
                }
                set
                {
                    f_SYNC = (byte)(value & 1);
                }
            }

            private byte f_TEST;

            [DataMember]
            [Category("Sound")]
            [DefaultValue((byte)0)]
            [Description("Disable voice, reset noise generator (0-1)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte TEST
            {
                get
                {
                    return f_TEST;
                }
                set
                {
                    f_TEST = (byte)(value & 1);
                }
            }

            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public SIDGlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new SIDGlobalSettings());
            }

            /// <summary>
            /// 
            /// </summary>
            public SIDTimbre()
            {
                GlobalSettings = new SIDGlobalSettings();
            }

            protected override void InitializeFxS()
            {
                this.SDS.FxS = new SidFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SIDTimbre>(serializeData);
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


        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SIDGlobalSettings>))]
        [DataContract]
        [InstLock]
        public class SIDGlobalSettings : ContextBoundObject
        {
            [DataMember]
            [Category("Chip")]
            [Description("Override global settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
            }

            private byte? f_RES;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [DefaultValue(null)]
            [Description("Resonance (0-15)")]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(UITypeEditor))]
            public byte? RES
            {
                get => f_RES;
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 15);
                    f_RES = v;
                }
            }

            private ushort? f_FC;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [DefaultValue(null)]
            [Description("Cutoff (or Center) Frequency (0-2047)(30Hz - 10KHz)")]
            [SlideParametersAttribute(0, 2047)]
            [EditorAttribute(typeof(SlideEditor), typeof(UITypeEditor))]
            public ushort? FC
            {
                get => f_FC;
                set
                {
                    ushort? v = value;
                    if (value.HasValue)
                        v = (ushort)(value & 2047);
                    f_FC = v;
                }
            }

            private byte? f_Off3;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [DefaultValue(null)]
            [Description("Disable ch 3 sound (0:Enable 1:Disable)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(UITypeEditor))]
            public byte? OFF3
            {
                get => f_Off3;
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_Off3 = v;
                }
            }

            private FilterChannel2? f_FILT;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [DefaultValue(null)]
            [Description("Apply Filter Ch")]
            public FilterChannel2? FILT
            {
                get => f_FILT;
                set
                {
                    f_FILT = value;
                }
            }

            private FilterTypes2? f_FilterType;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [DefaultValue(null)]
            [Description("Filter Type")]
            public FilterTypes2? FilterType
            {
                get => f_FilterType;
                set
                {
                    f_FilterType = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum Waveforms
        {
            None = 0,
            Triangle = 1,
            Saw = 2,
            Pulse = 4,
            Noise = 8,
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum FilterTypes
        {
            None = 0,
            LowPass = 1,
            BandPass = 2,
            HighPass = 4,
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum FilterTypes2
        {
            None = 0,
            LowPass = 1,
            BandPass = 2,
            LowBandPass = 3,
            HighPass = 4,
            LowHighPass = 5,
            BandHighPass = 6,
            LowBandHighPass = 7,
        }


        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum FilterChannel
        {
            None = 0,
            Ch1 = 1,
            Ch2 = 2,
            Ch3 = 4,
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum FilterChannel2
        {
            None = 0,
            Ch1 = 1,
            Ch2 = 2,
            Ch12 = 3,
            Ch3 = 4,
            Ch13 = 5,
            Ch23 = 6,
            Ch123 = 7,
            AutoChOnOr = 8,
            AutoChOnExc = 9,
            AutoChOff = 10,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum PhysicalChannel
        {
            Indeterminatene = 0,
            Ch1 = 1,
            Ch2 = 2,
            Ch3 = 3,
        }


        /// <summary>
        /// 
        /// </summary>
        public class SidFxEngine : BasicFxEngine
        {
            private SidFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public SidFxEngine(SidFxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_dutyCounter;

            public ushort? DutyValue
            {
                get;
                private set;
            }

            private uint f_resCounter;

            public byte? ResonanceValue
            {
                get;
                private set;
            }

            private uint f_cutCounter;

            public ushort? CutOffValue
            {
                get;
                private set;
            }

            private uint f_wavCounter;

            public Waveforms? WaveFormValue
            {
                get;
                private set;
            }

            private uint f_syncCounter;

            public byte? SyncValue
            {
                get;
                private set;
            }

            private uint f_ringCounter;

            public byte? RingValue
            {
                get;
                private set;
            }

            private uint f_testCounter;

            public byte? TestValue
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);

                DutyValue = null;
                if (settings.DutyEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.DutyEnvelopesNums.Length;
                        if (settings.DutyEnvelopesReleasePoint >= 0)
                            vm = settings.DutyEnvelopesReleasePoint;
                        if (f_dutyCounter >= vm)
                        {
                            if (settings.DutyEnvelopesRepeatPoint >= 0)
                                f_dutyCounter = (uint)settings.DutyEnvelopesRepeatPoint;
                            else
                                f_dutyCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (f_dutyCounter < settings.DutyEnvelopesNums.Length)
                        {
                            if (settings.DutyEnvelopesReleasePoint >= 0 && f_dutyCounter <= (uint)settings.DutyEnvelopesReleasePoint)
                                f_dutyCounter = (uint)settings.DutyEnvelopesReleasePoint;
                            //else if (settings.DutyEnvelopesReleasePoint < 0)
                                //f_dutyCounter = (uint)settings.DutyEnvelopesNums.Length;
                        }
                    }
                    if (f_dutyCounter < settings.DutyEnvelopesNums.Length)
                    {
                        int vol = settings.DutyEnvelopesNums[f_dutyCounter++];

                        DutyValue = (ushort)vol;
                        process = true;
                    }
                }

                ResonanceValue = null;
                if (settings.ResonanceEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.ResonanceEnvelopesNums.Length;
                        if (settings.ResonanceEnvelopesReleasePoint >= 0)
                            vm = settings.ResonanceEnvelopesReleasePoint;
                        if (f_resCounter >= vm)
                        {
                            if (settings.ResonanceEnvelopesRepeatPoint >= 0)
                                f_resCounter = (uint)settings.ResonanceEnvelopesRepeatPoint;
                            else
                                f_resCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (f_resCounter < settings.ResonanceEnvelopesNums.Length)
                        {
                            if (settings.ResonanceEnvelopesReleasePoint >= 0 && f_resCounter <= (uint)settings.ResonanceEnvelopesReleasePoint)
                                f_resCounter = (uint)settings.ResonanceEnvelopesReleasePoint;
                            //else if (settings.ResonanceEnvelopesReleasePoint < 0)
                                //f_resCounter = (uint)settings.ResonanceEnvelopesNums.Length;

                        }
                    }
                    if (f_resCounter < settings.ResonanceEnvelopesNums.Length)
                    {
                        int vol = settings.ResonanceEnvelopesNums[f_resCounter++];

                        ResonanceValue = (byte)vol;
                        process = true;
                    }
                }

                CutOffValue = null;
                if (settings.CutOffEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.CutOffEnvelopesNums.Length;
                        if (settings.CutOffEnvelopesReleasePoint >= 0)
                            vm = settings.CutOffEnvelopesReleasePoint;
                        if (f_cutCounter >= vm)
                        {
                            if (settings.CutOffEnvelopesRepeatPoint >= 0)
                                f_cutCounter = (uint)settings.CutOffEnvelopesRepeatPoint;
                            else
                                f_cutCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (f_cutCounter < settings.CutOffEnvelopesNums.Length)
                        {
                            if (settings.CutOffEnvelopesReleasePoint >= 0 && f_cutCounter <= (uint)settings.CutOffEnvelopesReleasePoint)
                                f_cutCounter = (uint)settings.CutOffEnvelopesReleasePoint;
                            //else if (settings.CutOffEnvelopesReleasePoint < 0)
                                //f_cutCounter = (uint)settings.CutOffEnvelopesNums.Length;
                        }
                    }

                    if (f_cutCounter < settings.CutOffEnvelopesNums.Length)
                    {
                        int vol = settings.CutOffEnvelopesNums[f_cutCounter++];

                        CutOffValue = (ushort)vol;
                        process = true;
                    }
                }

                WaveFormValue = null;
                if (settings.WaveFormEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.WaveFormEnvelopesNums.Length;
                        if (settings.WaveFormEnvelopesReleasePoint >= 0)
                            vm = settings.WaveFormEnvelopesReleasePoint;
                        if (f_wavCounter >= vm)
                        {
                            if (settings.WaveFormEnvelopesRepeatPoint >= 0)
                                f_wavCounter = (uint)settings.WaveFormEnvelopesRepeatPoint;
                            else
                                f_wavCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (f_wavCounter >= settings.WaveFormEnvelopesNums.Length && f_wavCounter < (uint)settings.WaveFormEnvelopesReleasePoint)
                        {
                            if (settings.WaveFormEnvelopesReleasePoint >= 0 && f_wavCounter <= (uint)settings.WaveFormEnvelopesReleasePoint)
                                f_wavCounter = (uint)settings.WaveFormEnvelopesReleasePoint;
                            //else if (settings.WaveFormEnvelopesReleasePoint < 0)
                                //f_wavCounter = (uint)settings.WaveFormEnvelopesNums.Length;
                        }
                    }

                    if (f_wavCounter < settings.WaveFormEnvelopesNums.Length)
                    {
                        int vol = settings.WaveFormEnvelopesNums[f_wavCounter++];

                        WaveFormValue = (Waveforms)vol;
                        process = true;
                    }
                }

                SyncValue = null;
                if (settings.SyncEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.SyncEnvelopesNums.Length;
                        if (settings.SyncEnvelopesReleasePoint >= 0)
                            vm = settings.SyncEnvelopesReleasePoint;
                        if (f_syncCounter >= vm)
                        {
                            if (settings.WaveFormEnvelopesRepeatPoint >= 0)
                                f_syncCounter = (uint)settings.WaveFormEnvelopesRepeatPoint;
                            else
                                f_syncCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (f_syncCounter >= settings.SyncEnvelopesNums.Length)
                        {
                            if (settings.SyncEnvelopesReleasePoint >= 0 && f_syncCounter <= (uint)settings.SyncEnvelopesReleasePoint)
                                f_syncCounter = (uint)settings.SyncEnvelopesReleasePoint;
                            //else if (settings.SyncEnvelopesReleasePoint < 0)
                                //f_syncCounter = (uint)settings.SyncEnvelopesNums.Length;
                        }
                    }

                    if (f_syncCounter < settings.SyncEnvelopesNums.Length)
                    {
                        int vol = settings.SyncEnvelopesNums[f_syncCounter++];

                        SyncValue = (byte)vol;
                        process = true;
                    }
                }

                RingValue = null;
                if (settings.RingEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.RingEnvelopesNums.Length;
                        if (settings.RingEnvelopesReleasePoint >= 0)
                            vm = settings.RingEnvelopesReleasePoint;
                        if (f_ringCounter >= vm)
                        {
                            if (settings.WaveFormEnvelopesRepeatPoint >= 0)
                                f_ringCounter = (uint)settings.WaveFormEnvelopesRepeatPoint;
                            else
                                f_ringCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (f_ringCounter >= settings.RingEnvelopesNums.Length)
                        {
                            if (settings.RingEnvelopesReleasePoint >= 0 && f_ringCounter <= (uint)settings.RingEnvelopesReleasePoint)
                                f_ringCounter = (uint)settings.RingEnvelopesReleasePoint;
                            //else if (settings.RingEnvelopesReleasePoint < 0)
                                //f_ringCounter = (uint)settings.RingEnvelopesNums.Length;

                        }
                    }

                    if (f_ringCounter < settings.RingEnvelopesNums.Length)
                    {
                        int vol = settings.RingEnvelopesNums[f_ringCounter++];

                        RingValue = (byte)vol;
                        process = true;
                    }
                }

                TestValue = null;
                if (settings.TestEnvelopesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.TestEnvelopesNums.Length;
                        if (settings.TestEnvelopesReleasePoint >= 0)
                            vm = settings.TestEnvelopesReleasePoint;
                        if (f_testCounter >= vm)
                        {
                            if (settings.WaveFormEnvelopesRepeatPoint >= 0)
                                f_testCounter = (uint)settings.WaveFormEnvelopesRepeatPoint;
                            else
                                f_testCounter = (uint)vm;
                        }
                    }
                    else
                    {
                        //if (f_testCounter >= settings.TestEnvelopesNums.Length)
                        {
                            if (settings.TestEnvelopesReleasePoint >= 0 && f_testCounter <= (uint)settings.TestEnvelopesReleasePoint)
                                f_testCounter = (uint)settings.TestEnvelopesReleasePoint;
                            //else if (settings.TestEnvelopesReleasePoint < 0)
                                //f_testCounter = (uint)settings.TestEnvelopesNums.Length;
                        }
                    }

                    if (f_testCounter < settings.TestEnvelopesNums.Length)
                    {
                        int vol = settings.TestEnvelopesNums[f_testCounter++];

                        TestValue = (byte)vol;
                        process = true;
                    }
                }

                return process;
            }

        }

        [JsonConverter(typeof(NoTypeConverterJsonConverter<SidFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class SidFxSettings : BasicFxSettings
        {

            private string f_DutyEnvelopes;

            [DataMember]
            [Description("Set Duty envelop by text. Input duty value and split it with space like the FamiTracker.\r\n" +
                       "0 ～ 4095 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 4095)]
            public string DutyEnvelopes
            {
                get
                {
                    return f_DutyEnvelopes;
                }
                set
                {
                    if (f_DutyEnvelopes != value)
                    {
                        DutyEnvelopesRepeatPoint = -1;
                        DutyEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            DutyEnvelopesNums = new int[] { };
                            f_DutyEnvelopes = string.Empty;
                            return;
                        }
                        f_DutyEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                DutyEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                DutyEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 4095)
                                        v = 4095;
                                    vs.Add(v);
                                }
                            }
                        }
                        DutyEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < DutyEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (DutyEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (DutyEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < DutyEnvelopesNums.Length)
                                sb.Append(DutyEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_DutyEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeDutyEnvelopes()
            {
                return !string.IsNullOrEmpty(DutyEnvelopes);
            }

            public void ResetDutyEnvelopes()
            {
                DutyEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] DutyEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int DutyEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int DutyEnvelopesReleasePoint { get; set; } = -1;


            private string f_ResonanceEnvelopes;

            [DataMember]
            [Description("Set Resonance envelop by text. Input resonance value and split it with space like the FamiTracker.\r\n" +
                       "0 ～ 15 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 15)]
            public string ResonanceEnvelopes
            {
                get
                {
                    return f_ResonanceEnvelopes;
                }
                set
                {
                    if (f_ResonanceEnvelopes != value)
                    {
                        ResonanceEnvelopesRepeatPoint = -1;
                        ResonanceEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            ResonanceEnvelopesNums = new int[] { };
                            f_ResonanceEnvelopes = string.Empty;
                            return;
                        }
                        f_ResonanceEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                ResonanceEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                ResonanceEnvelopesReleasePoint = vs.Count;
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
                        ResonanceEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < ResonanceEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (ResonanceEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (ResonanceEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < ResonanceEnvelopesNums.Length)
                                sb.Append(ResonanceEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_ResonanceEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeResonanceEnvelopes()
            {
                return !string.IsNullOrEmpty(ResonanceEnvelopes);
            }

            public void ResetResonanceEnvelopes()
            {
                ResonanceEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] ResonanceEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int ResonanceEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int ResonanceEnvelopesReleasePoint { get; set; } = -1;


            private string f_CutOffEnvelopes;

            [DataMember]
            [Description("Set Cut Off envelop by text. Input resonance value and split it with space like the FamiTracker.\r\n" +
                       "0 ～ 2047 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 2047)]
            public string CutOffEnvelopes
            {
                get
                {
                    return f_CutOffEnvelopes;
                }
                set
                {
                    if (f_CutOffEnvelopes != value)
                    {
                        CutOffEnvelopesRepeatPoint = -1;
                        CutOffEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            CutOffEnvelopesNums = new int[] { };
                            f_CutOffEnvelopes = string.Empty;
                            return;
                        }
                        f_CutOffEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                CutOffEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                CutOffEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 2047)
                                        v = 2047;
                                    vs.Add(v);
                                }
                            }
                        }
                        CutOffEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < CutOffEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (CutOffEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (CutOffEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < CutOffEnvelopesNums.Length)
                                sb.Append(CutOffEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_CutOffEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeCutOffEnvelopes()
            {
                return !string.IsNullOrEmpty(CutOffEnvelopes);
            }

            public void ResetCutOffEnvelopes()
            {
                CutOffEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] CutOffEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int CutOffEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int CutOffEnvelopesReleasePoint { get; set; } = -1;

            private string f_WaveFormEnvelopes;

            [DataMember]
            [Description("Set WaveForm envelop by text. Input resonance value and split it with space like the FamiTracker.\r\n" +
                       "0 ～ 8(Tri:1 Saw:2 Pulse:4 Noise:8) \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(1, 8)]
            public string WaveFormEnvelopes
            {
                get
                {
                    return f_WaveFormEnvelopes;
                }
                set
                {
                    if (f_WaveFormEnvelopes != value)
                    {
                        WaveFormEnvelopesRepeatPoint = -1;
                        WaveFormEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            WaveFormEnvelopesNums = new int[] { };
                            f_WaveFormEnvelopes = string.Empty;
                            return;
                        }
                        f_WaveFormEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                WaveFormEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                WaveFormEnvelopesReleasePoint = vs.Count;
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
                        WaveFormEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < WaveFormEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (WaveFormEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (WaveFormEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < WaveFormEnvelopesNums.Length)
                                sb.Append(WaveFormEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_WaveFormEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeWaveFormEnvelopes()
            {
                return !string.IsNullOrEmpty(WaveFormEnvelopes);
            }

            public void ResetWaveFormEnvelopes()
            {
                WaveFormEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] WaveFormEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int WaveFormEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int WaveFormEnvelopesReleasePoint { get; set; } = -1;


            private string f_SyncEnvelopes;

            [DataMember]
            [Description("Set SYNC envelop by text. Input resonance value and split it with space like the FamiTracker.\r\n" +
                       "0 or 1 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(1, 8)]
            public string SyncEnvelopes
            {
                get
                {
                    return f_SyncEnvelopes;
                }
                set
                {
                    if (f_SyncEnvelopes != value)
                    {
                        SyncEnvelopesRepeatPoint = -1;
                        SyncEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            SyncEnvelopesNums = new int[] { };
                            f_SyncEnvelopes = string.Empty;
                            return;
                        }
                        f_SyncEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                SyncEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                SyncEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 1;
                                    else if (v > 1)
                                        v = 1;
                                    vs.Add(v);
                                }
                            }
                        }
                        SyncEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < SyncEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (SyncEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (SyncEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < SyncEnvelopesNums.Length)
                                sb.Append(SyncEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_SyncEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeSyncEnvelopes()
            {
                return !string.IsNullOrEmpty(SyncEnvelopes);
            }

            public void ResetSyncEnvelopes()
            {
                SyncEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] SyncEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int SyncEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int SyncEnvelopesReleasePoint { get; set; } = -1;


            private string f_RingEnvelopes;

            [DataMember]
            [Description("Set RING envelop by text. Input resonance value and split it with space like the FamiTracker.\r\n" +
                       "0 or 1 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(1, 8)]
            public string RingEnvelopes
            {
                get
                {
                    return f_RingEnvelopes;
                }
                set
                {
                    if (f_RingEnvelopes != value)
                    {
                        RingEnvelopesRepeatPoint = -1;
                        RingEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            RingEnvelopesNums = new int[] { };
                            f_RingEnvelopes = string.Empty;
                            return;
                        }
                        f_RingEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                RingEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                RingEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 1;
                                    else if (v > 1)
                                        v = 1;
                                    vs.Add(v);
                                }
                            }
                        }
                        RingEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < RingEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (RingEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (RingEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < RingEnvelopesNums.Length)
                                sb.Append(RingEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_RingEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeRingEnvelopes()
            {
                return !string.IsNullOrEmpty(RingEnvelopes);
            }

            public void ResetRingEnvelopes()
            {
                RingEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] RingEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int RingEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int RingEnvelopesReleasePoint { get; set; } = -1;


            private string f_TestEnvelopes;

            [DataMember]
            [Description("Set TEST envelop by text. Input resonance value and split it with space like the FamiTracker.\r\n" +
                       "0 or 1 \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(1, 8)]
            public string TestEnvelopes
            {
                get
                {
                    return f_TestEnvelopes;
                }
                set
                {
                    if (f_TestEnvelopes != value)
                    {
                        TestEnvelopesRepeatPoint = -1;
                        TestEnvelopesReleasePoint = -1;
                        if (value == null)
                        {
                            TestEnvelopesNums = new int[] { };
                            f_TestEnvelopes = string.Empty;
                            return;
                        }
                        f_TestEnvelopes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                TestEnvelopesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                TestEnvelopesReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 1;
                                    else if (v > 1)
                                        v = 1;
                                    vs.Add(v);
                                }
                            }
                        }
                        TestEnvelopesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < TestEnvelopesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (TestEnvelopesRepeatPoint == i)
                                sb.Append("| ");
                            if (TestEnvelopesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < TestEnvelopesNums.Length)
                                sb.Append(TestEnvelopesNums[i].ToString((IFormatProvider)null));
                        }
                        f_TestEnvelopes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeTestEnvelopes()
            {
                return !string.IsNullOrEmpty(TestEnvelopes);
            }

            public void ResetTestEnvelopes()
            {
                TestEnvelopes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] TestEnvelopesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int TestEnvelopesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int TestEnvelopesReleasePoint { get; set; } = -1;


            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new SidFxEngine(this);
            }

        }

        private class EnumConverterSoundEngineTypeC64 : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.VSIF_C64_FTDI});

                return sc;
            }
        }
    }
}