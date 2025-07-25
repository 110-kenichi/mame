﻿// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.MusicTheory;
using NAudio.Wave;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gimic;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Gui.FMEditor;
using zanac.MAmidiMEmo.Instruments.Chips.YM;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Scci;
using zanac.MAmidiMEmo.VSIF;

//https://www16.atwiki.jp/mxdrv/pages/24.html
//http://map.grauw.nl/resources/sound/yamaha_ym2151_synthesis.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class YM2151 : InstrumentBase
    {

        public override string Name => "YM2151";

        public override string Group => "FM";

        public override InstrumentType InstrumentType => InstrumentType.YM2151;

        [Browsable(false)]
        public override string ImageKey => "YM2151";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ym2151_";

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
                return 1;
            }
        }

        private PortId portId = PortId.No1;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set FTDI Port No for \"VSIF - MSX\"\r\n" +
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

        private IntPtr spfmPtr;

        private int gimicPtr = -1;

        private VsifClient vsifClient;

        private SoundEngineType f_SoundEngineType;

        private SoundEngineType f_CurrentSoundEngineType;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Select a sound engine type.\r\n" +
            "Supports Software and SPFM.")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeYM2151))]
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

        private class EnumConverterSoundEngineTypeYM2151 : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.SPFM,
                    SoundEngineType.VSIF_MSX_FTDI,
                    SoundEngineType.GIMIC,
                    SoundEngineType.VSIF_MSX_Pi,
                    SoundEngineType.VSIF_MSX_PiTr,
                });

                return sc;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
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
                if (gimicPtr != -1)
                {
                    GimicManager.ReleaseModule(gimicPtr);
                    gimicPtr = -1;
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
                        spfmPtr = ScciManager.TryGetSoundChip(SoundChipType.SC_TYPE_YM2151, (SC_CHIP_CLOCK)MasterClock);
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
                            enableOpm(ExtOPMSlot, true);
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.GIMIC:
                        gimicPtr = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_OPM);
                        if (gimicPtr >= 0)
                        {
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            f_MasterClock = GimicManager.SetClock(gimicPtr, f_MasterClock);
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.VSIF_MSX_Pi:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_Pi, PortId, false);
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
                    case SoundEngineType.VSIF_MSX_PiTr:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.MSX_PiTR, PortId, false);
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
            }

            ClearWrittenDataCache();
            PrepareSound();
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
        [Description("Set FTDI Clock Width[%].\r\n" +
            "MSX FT232R:25~\r\n" +
            "MSX FT232H:32~")]
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

        public bool ShouldSerializeFtdiClkWidth()
        {
            switch (f_SoundEngineType)
            {
                case SoundEngineType.VSIF_MSX_FTDI:
                case SoundEngineType.VSIF_P6_FTDI:
                    return f_ftdiClkWidth != VsifManager.FTDI_BAUDRATE_MSX_CLK_WIDTH;
            }
            return f_ftdiClkWidth != VsifManager.FTDI_BAUDRATE_GEN_CLK_WIDTH;
        }

        public void ResetFtdiClkWidth()
        {
            switch (f_SoundEngineType)
            {
                case SoundEngineType.VSIF_MSX_FTDI:
                case SoundEngineType.VSIF_P6_FTDI:
                    f_ftdiClkWidth = VsifManager.FTDI_BAUDRATE_MSX_CLK_WIDTH;
                    return;
            }
            f_ftdiClkWidth = VsifManager.FTDI_BAUDRATE_GEN_CLK_WIDTH;
        }

        private OPMSlotNo f_extOPMSlot = OPMSlotNo.Id0;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [DefaultValue(OPMSlotNo.Id0)]
        [Description("Specify the OPM ID number for VSIF(MSX).")]
        public OPMSlotNo ExtOPMSlot
        {
            get
            {
                return f_extOPMSlot;
            }
            set
            {
                if (f_extOPMSlot != value)
                {
                    switch (value)
                    {
                        case OPMSlotNo.Id0:
                        case OPMSlotNo.Id1:
                            f_extOPMSlot = value;
                            break;
                    }
                    switch (CurrentSoundEngine)
                    {
                        case SoundEngineType.VSIF_MSX_FTDI:
                        case SoundEngineType.VSIF_MSX_Pi:
                        case SoundEngineType.VSIF_MSX_PiTr:
                            enableOpm(value, true);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        private void enableOpm(OPMSlotNo slot, bool clearCache)
        {
            if (slot == OPMSlotNo.Id0 || slot == OPMSlotNo.Id1)
            {
                lock (sndEnginePtrLock)
                {
                    int wait = f_ftdiClkWidth;
                    if (CurrentSoundEngine == SoundEngineType.VSIF_MSX_Pi)
                        wait = 0;
                    else if (CurrentSoundEngine == SoundEngineType.VSIF_MSX_PiTr)
                        wait = -2;
                    vsifClient?.WriteData(0xd, 0, (byte)slot, wait);
                }
            }
            if (clearCache)
                ClearWrittenDataCache();
        }

        /// <summary>
        /// 
        /// </summary>
        public enum MasterClockType : uint
        {
            Default = 3579545,
            X68000 = 4000000,
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
                    if (CurrentSoundEngine == SoundEngineType.GIMIC)
                        value = GimicManager.SetClock(gimicPtr, value);
                    f_MasterClock = value;
                    if (CurrentSoundEngine == SoundEngineType.SPFM)
                        setSoundEngine(f_SoundEngineType);
                    SetClock(UnitNumber, (uint)value);
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

        private byte f_LFRQ;

        /// <summary>
        /// LFRQ (0-255)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO Freq (0-255)")]
        [SlideParametersAttribute(0, 255)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFRQ
        {
            get
            {
                return f_LFRQ;
            }
            set
            {
                if (f_LFRQ != value)
                {
                    f_LFRQ = value;
                    Ym2151WriteData(UnitNumber, 0x18, 0, 0, LFRQ);
                }
            }
        }

        private byte f_LFOF;

        /// <summary>
        /// Select AMD or PMD(0:AMD 1:PMD)
        /// </summary>
        [Browsable(false)]
        [Obsolete]
        public byte LFOF
        {
            get
            {
                if (f_LFOF == 0)
                    return AMD;
                else
                    return PMD;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_LFOF != v)
                {
                    f_LFOF = v;
                }
            }
        }

        private byte f_LFOD;

        /// <summary>
        /// LFO Depth(0-127)
        /// </summary>
        [Browsable(false)]
        [Obsolete]
        public byte LFOD
        {
            get
            {
                return f_LFOD;
            }
            set
            {
                byte v = (byte)(value & 127);
                if (f_LFOD != v)
                {
                    f_LFOD = v;

                    if (f_LFOF == 0)
                        AMD = v;
                    else
                        PMD = v;
                }
            }
        }

        private byte f_AMD;


        /// <summary>
        /// AMD Depth(0-127)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("AMD(0-127)")]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        [DisplayName("AMD")]
        public byte AMD
        {
            get
            {
                return f_AMD;
            }
            set
            {
                byte v = (byte)(value & 127);
                if (f_AMD != v)
                {
                    f_AMD = v;
                    Ym2151WriteData(UnitNumber, 0x19, 0, 0, (byte)(0 << 7 | f_AMD));
                }
            }
        }

        private byte f_PMD;


        /// <summary>
        /// PMD Depth(0-127)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("PMD(0-127)")]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        [DisplayName("PMD")]
        public byte PMD
        {
            get
            {
                return f_PMD;
            }
            set
            {
                byte v = (byte)(value & 127);
                if (f_PMD != v)
                {
                    f_PMD = v;
                    Ym2151WriteData(UnitNumber, 0x19, 0, 0, (byte)(1 << 7 | f_PMD));
                }
            }
        }

        private byte f_LFOW;


        /// <summary>
        /// LFO Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)")]
        [SlideParametersAttribute(0, 3)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFOW
        {
            get
            {
                return f_LFOW;
            }
            set
            {
                byte v = (byte)(value & 3);
                if (f_LFOW != v)
                {
                    f_LFOW = v;
                    Ym2151WriteData(UnitNumber, 0x1B, 0, 0, (byte)LFOW);
                }
            }
        }

        private byte f_NE;

        /// <summary>
        /// Noise Enable (0:Disable 1:Enable)
        /// </summary>
        [Browsable(false)]
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Noise Enable (0:Disable 1:Enable)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte NE
        {
            get
            {
                return f_NE;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_NE != v)
                {
                    f_NE = v;
                    Ym2151WriteData(UnitNumber, 0x0f, 0, 0, (byte)(NE << 7 | NFRQ));
                }
            }
        }

        private byte f_NFRQ;

        /// <summary>
        /// Noise Feequency (0-31)
        /// </summary>
        [Browsable(false)]
        [DataMember]
        [Category("Chip(Global)")]
        [Description(" Noise Feequency (0-31)\r\n" +
            "3'579'545/(32*NFRQ)")]
        [SlideParametersAttribute(0, 31)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte NFRQ
        {
            get
            {
                return f_NFRQ;
            }
            set
            {
                byte v = (byte)(value & 31);
                if (f_NFRQ != v)
                {
                    f_NFRQ = v;
                    Ym2151WriteData(UnitNumber, 0x0f, 0, 0, (byte)(NE << 7 | NFRQ));
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
                Timbres = (YM2151Timbre[])value;
            }
        }

        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(YM2151UITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public YM2151Timbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<YM2151>(serializeData))
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
        private delegate void delegate_ym2151_write(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ym2151_write Ym2151_write
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
            Ym2151WriteData(UnitNumber, (byte)address, 0, 0, (byte)data, address != 0x08);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Ym2151WriteData(uint unitNumber, byte address, int op, int slot, byte data)
        {
            Ym2151WriteData(unitNumber, address, op, slot, data, true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Ym2151WriteData(uint unitNumber, byte address, int op, int slot, byte data, bool useCache)
        {
            switch (op)
            {
                case 0:
                    op = 0;
                    break;
                case 1:
                    op = 2;
                    break;
                case 2:
                    op = 1;
                    break;
                case 3:
                    op = 3;
                    break;
            }
            byte adr = (byte)(address + (op * 8) + slot);

            WriteData(adr, data, useCache, new Action(() =>
            {
                lock (sndEnginePtrLock)
                {
                    switch (CurrentSoundEngine)
                    {
                        case SoundEngineType.SPFM:
                            ScciManager.SetRegister(spfmPtr, adr, data, false);
                            break;
                        case SoundEngineType.VSIF_MSX_FTDI:
                        case SoundEngineType.VSIF_MSX_Pi:
                        case SoundEngineType.VSIF_MSX_PiTr:
                            int wait = f_ftdiClkWidth;
                            byte addr = 0xe;
                            if (CurrentSoundEngine == SoundEngineType.VSIF_MSX_Pi)
                                wait = 0;
                            else if (CurrentSoundEngine == SoundEngineType.VSIF_MSX_PiTr)
                            {
                                wait = -2;
                                addr += 0x20;
                            }
                            enableOpm(f_extOPMSlot, false);
                            vsifClient.WriteData(addr, adr, data, wait);
                            break;
                        case SoundEngineType.GIMIC:
                            GimicManager.SetRegister2(gimicPtr, adr, data);
                            break;
                    }
                }
                DeferredWriteData(Ym2151_write, unitNumber, (uint)0, adr);
                DeferredWriteData(Ym2151_write, unitNumber, (uint)1, data);
            }));
            /*
            try
            {
                Program.SoundUpdating();

                Ym2151_write(unitNumber, 0, (byte)(address + (op * 8) + slot));
                Ym2151_write(unitNumber, 1, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }


        /// <summary>
        /// 
        /// </summary>
        static YM2151()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ym2151_write");
            if (funcPtr != IntPtr.Zero)
            {
                Ym2151_write = (delegate_ym2151_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ym2151_write));
            }
        }

        private YM2151SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM2151(uint unitNumber) : base(unitNumber)
        {
            SetDevicePassThru(false);

            MasterClock = (uint)MasterClockType.Default;

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new YM2151Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new YM2151Timbre();

            setPresetInstruments();

            this.soundManager = new YM2151SoundManager(this);
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
                if (gimicPtr >= 0)
                {
                    GimicManager.ReleaseModule(gimicPtr);
                    gimicPtr = -1;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            //Dist Gt
            Timbres[0].PMS = 0;
            Timbres[0].AMS = 0;
            Timbres[0].FB = 0;
            Timbres[0].ALG = 3;

            Timbres[0].Ops[0].Enable = 1;
            Timbres[0].Ops[0].AR = 31;
            Timbres[0].Ops[0].D1R = 0;
            Timbres[0].Ops[0].SL = 0;
            Timbres[0].Ops[0].D2R = 0;
            Timbres[0].Ops[0].RR = 6;

            Timbres[0].Ops[0].MUL = 8;
            Timbres[0].Ops[0].RS = 0;
            Timbres[0].Ops[0].DT1 = 7;
            Timbres[0].Ops[0].AM = 0;
            Timbres[0].Ops[0].DT2 = 0;
            Timbres[0].Ops[0].TL = 56;

            Timbres[0].Ops[1].Enable = 1;
            Timbres[0].Ops[1].AR = 31;
            Timbres[0].Ops[1].D1R = 18;
            Timbres[0].Ops[1].SL = 0;
            Timbres[0].Ops[1].D2R = 0;
            Timbres[0].Ops[1].RR = 6;

            Timbres[0].Ops[1].MUL = 3;
            Timbres[0].Ops[1].RS = 0;
            Timbres[0].Ops[1].DT1 = 7;
            Timbres[0].Ops[1].AM = 0;
            Timbres[0].Ops[1].DT2 = 0;
            Timbres[0].Ops[1].TL = 19;

            Timbres[0].Ops[2].Enable = 1;
            Timbres[0].Ops[2].AR = 31;
            Timbres[0].Ops[2].D1R = 0;
            Timbres[0].Ops[2].SL = 0;
            Timbres[0].Ops[2].D2R = 0;
            Timbres[0].Ops[2].RR = 6;

            Timbres[0].Ops[2].MUL = 3;
            Timbres[0].Ops[2].RS = 0;
            Timbres[0].Ops[2].DT1 = 4;
            Timbres[0].Ops[2].AM = 0;
            Timbres[0].Ops[2].DT2 = 0;
            Timbres[0].Ops[2].TL = 8;

            Timbres[0].Ops[3].Enable = 1;
            Timbres[0].Ops[3].AR = 20;
            Timbres[0].Ops[3].D1R = 17;
            Timbres[0].Ops[3].SL = 0;
            Timbres[0].Ops[3].D2R = 0;
            Timbres[0].Ops[3].RR = 5;

            Timbres[0].Ops[3].MUL = 2;
            Timbres[0].Ops[3].RS = 0;
            Timbres[0].Ops[3].DT1 = 4;
            Timbres[0].Ops[3].AM = 0;
            Timbres[0].Ops[3].DT2 = 0;
            Timbres[0].Ops[3].TL = 24;
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        /// <summary>
        /// 
        /// </summary>
        private void initGlobalRegisters()
        {
            Ym2151WriteData(UnitNumber, 0x18, 0, 0, LFRQ);
            Ym2151WriteData(UnitNumber, 0x19, 0, 0, (byte)(0 << 7 | AMD));
            Ym2151WriteData(UnitNumber, 0x19, 0, 0, (byte)(0 << 7 | PMD));
            Ym2151WriteData(UnitNumber, 0x1B, 0, 0, (byte)LFOW);
            Ym2151WriteData(UnitNumber, 0x0f, 0, 0, (byte)(NE << 7 | NFRQ));
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
            lock (sndEnginePtrLock)
            {
                //HACK:
                switch (f_SoundEngineType)
                {
                    case SoundEngineType.GIMIC:
                        GimicManager.Reset(gimicPtr);
                        break;
                }
            }
        }

        internal override void ResetAll()
        {
            ClearWrittenDataCache();
            PrepareSound();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ClearWrittenDataCache()
        {
            base.ClearWrittenDataCache();

            enableOpm(f_extOPMSlot, false);
        }

        /// <summary>
        /// 
        /// </summary>
        private class YM2151SoundManager : SoundManagerBase
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

            private static SoundList<YM2151Sound> fmOnSounds = new SoundList<YM2151Sound>(8);

            private YM2151 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM2151SoundManager(YM2151 parent) : base(parent)
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
                foreach (YM2151Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    YM2151Sound snd = new YM2151Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    fmOnSounds.Add(snd);

                    FormMain.OutputDebugLog(parentModule, "KeyOn FM ch" + emptySlot + " " + note.ToString());
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
            private (YM2151 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, fmOnSounds, note, 8);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 8; i++)
                {
                    parentModule.Ym2151WriteData(parentModule.UnitNumber, 0x08, 0, 0, (byte)(0x00 | i));

                    for (int op = 0; op < 4; op++)
                        parentModule.Ym2151WriteData(parentModule.UnitNumber, 0x60, op, i, 127);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class YM2151Sound : SoundBase
        {
            private YM2151 parentModule;

            private uint unitNumber;

            private YM2151Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM2151Sound(YM2151 parentModule, YM2151SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (YM2151Timbre)timbre;

                this.unitNumber = parentModule.UnitNumber;
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
                    if (gs.AMD.HasValue)
                        parentModule.AMD = gs.AMD.Value;
                    if (gs.PMD.HasValue)
                        parentModule.PMD = gs.PMD.Value;
                    if (gs.LFOW.HasValue)
                        parentModule.LFOW = gs.LFOW.Value;
                    if (gs.LFRQ.HasValue)
                        parentModule.LFRQ = gs.LFRQ.Value;
                    if (gs.NE.HasValue)
                        parentModule.NE = gs.NE.Value;
                    if (gs.NFRQ.HasValue)
                        parentModule.NFRQ = gs.NFRQ.Value;
                }

                //
                SetTimbre();
                //Freq
                OnPitchUpdated();
                //Volume
                OnVolumeUpdated();
                //On
                parentModule.Ym2151WriteData(unitNumber, 0x01, 0, 0, (byte)0x2);
                parentModule.Ym2151WriteData(unitNumber, 0x01, 0, 0, (byte)0x0);

                byte op = (byte)(timbre.Ops[0].Enable << 3 | timbre.Ops[2].Enable << 4 | timbre.Ops[1].Enable << 5 | timbre.Ops[3].Enable << 6);
                parentModule.Ym2151WriteData(unitNumber, 0x08, 0, 0, (byte)(op | Slot));
            }


            public override void OnSoundParamsUpdated()
            {
                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.AMD.HasValue)
                        parentModule.AMD = gs.AMD.Value;
                    if (gs.PMD.HasValue)
                        parentModule.PMD = gs.PMD.Value;
                    if (gs.LFOW.HasValue)
                        parentModule.LFOW = gs.LFOW.Value;
                    if (gs.LFRQ.HasValue)
                        parentModule.LFRQ = gs.LFRQ.Value;
                    if (gs.NE.HasValue)
                        parentModule.NE = gs.NE.Value;
                    if (gs.NFRQ.HasValue)
                        parentModule.NFRQ = gs.NFRQ.Value;
                }

                parentModule.Ym2151WriteData(unitNumber, 0x38, 0, Slot, (byte)((timbre.PMS << 4 | timbre.AMS)));
                for (int op = 0; op < 4; op++)
                {
                    parentModule.Ym2151WriteData(unitNumber, 0x40, op, Slot, (byte)((timbre.Ops[op].DT1 << 4 | timbre.Ops[op].MUL)));

                    var tl = timbre.Ops[op].TL + timbre.Ops[op].GetLSAttenuationValue(NoteOnEvent.NoteNumber);
                    int kvs = timbre.Ops[op].GetKvsAttenuationValue(NoteOnEvent.Velocity);
                    if (kvs > 0)
                        tl += kvs;
                    if (tl > 127)
                        tl = 127;

                    switch (timbre.ALG)
                    {
                        case 0:
                            if (op != 3)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 1:
                            if (op != 3)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 2:
                            if (op != 3)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 3:
                            if (op != 3)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 4:
                            if (op != 1 && op != 3)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 5:
                            if (op == 3)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 6:
                            if (op == 3)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 7:
                            break;
                    }
                    parentModule.Ym2151WriteData(unitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].AR)));
                    parentModule.Ym2151WriteData(unitNumber, 0xa0, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].D1R)));
                    parentModule.Ym2151WriteData(unitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].DT2 << 6 | timbre.Ops[op].D2R)));
                    parentModule.Ym2151WriteData(unitNumber, 0xe0, op, Slot, (byte)((timbre.Ops[op].SL << 4 | timbre.Ops[op].RR)));
                }

                if (!IsKeyOff)
                {
                    byte open = (byte)(timbre.Ops[0].Enable << 3 | timbre.Ops[2].Enable << 4 | timbre.Ops[1].Enable << 5 | timbre.Ops[3].Enable << 6);
                    parentModule.Ym2151WriteData(unitNumber, 0x08, 0, 0, (byte)(open | Slot));
                }

                base.OnSoundParamsUpdated();
            }


            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                List<int> ops = new List<int>();
                List<int> exops = new List<int>();
                switch (timbre.ALG)
                {
                    case 0:
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        exops.Add(2);
                        break;
                    case 1:
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        exops.Add(2);
                        break;
                    case 2:
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        exops.Add(2);
                        break;
                    case 3:
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(1);
                        exops.Add(2);
                        break;
                    case 4:
                        ops.Add(1);
                        ops.Add(3);
                        exops.Add(0);
                        exops.Add(2);
                        break;
                    case 5:
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        exops.Add(0);
                        break;
                    case 6:
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        exops.Add(0);
                        break;
                    case 7:
                        ops.Add(0);
                        ops.Add(1);
                        ops.Add(2);
                        ops.Add(3);
                        break;
                }
                int velo = 1 + timbre.MDS.VelocitySensitivity;
                foreach (int op in ops)
                {
                    var tl = timbre.Ops[op].TL + timbre.Ops[op].GetLSAttenuationValue(NoteOnEvent.NoteNumber);
                    int kvs = timbre.Ops[op].GetKvsAttenuationValue(NoteOnEvent.Velocity);
                    if (kvs > 0)
                        tl += kvs;
                    if (tl > 127)
                        tl = 127;
                    double vol = 0;
                    if (kvs < 0)
                        vol = ((127 / velo) - Math.Round(((127 / velo) - (tl / velo)) * CalcCurrentVolume()));
                    else
                        vol = (127 - Math.Round((127 - (tl + kvs)) * CalcCurrentVolume(true)));
                    //$60+: total level
                    parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)vol);
                }
                if (timbre.UseExprForModulator)
                {
                    //$60+: total level
                    var mul = CalcModulatorMultiply();
                    foreach (int op in exops)
                    {
                        if (((1 << op) & (int)timbre.ExprTargetModulators) == 0)
                            continue;

                        double vol = timbre.Ops[op].TL;
                        if (mul > 0)
                            vol = vol + ((127 - vol) * mul);
                        else if (mul < 0)
                            vol = vol + ((vol) * mul);
                        vol = Math.Round(vol);
                        parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)vol);
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                double d = (CalcCurrentPitchDeltaNoteNumber() - (12d * Math.Log(parentModule.MasterClock / 3579545d, 2))) * 63d;

                int kf = 0;
                if (d > 0)
                    kf = (int)d % 63;
                else if (d < 0)
                    kf = 63 + ((int)d % 63);

                int noted = (int)d / 63;
                if (d < 0)
                    noted -= 1;

                int nn = NoteOnEvent.NoteNumber;
                if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum ||
                    ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.DrumGt)
                    nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;
                int noteNum = nn + noted;
                if (noteNum > 127)
                    noteNum = 127;
                else if (noteNum < 0)
                    noteNum = 0;

                var nnOn = new TaggedNoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);

                nn = getNoteNum(nnOn.GetNoteName());
                var octave = nnOn.GetNoteOctave();
                if (nn == 14)
                {
                    octave -= 1;
                }
                if (octave < 0)
                {
                    octave = 0;
                    nn = 0;
                }
                if (octave > 7)
                {
                    octave = 7;
                    nn = 14;
                }

                parentModule.Ym2151WriteData(unitNumber, 0x28, 0, Slot, (byte)((octave << 4) | nn), false);
                parentModule.Ym2151WriteData(unitNumber, 0x30, 0, Slot, (byte)(kf << 2), false);

                base.OnPitchUpdated();
            }

            private byte getNoteNum(NoteName noteName)
            {
                byte nn = 0;
                switch (noteName)
                {
                    case NoteName.C:
                        nn = 14;
                        break;
                    case NoteName.CSharp:
                        nn = 0;
                        break;
                    case NoteName.D:
                        nn = 1;
                        break;
                    case NoteName.DSharp:
                        nn = 2;
                        break;
                    case NoteName.E:
                        nn = 4;
                        break;
                    case NoteName.F:
                        nn = 5;
                        break;
                    case NoteName.FSharp:
                        nn = 6;
                        break;
                    case NoteName.G:
                        nn = 8;
                        break;
                    case NoteName.GSharp:
                        nn = 9;
                        break;
                    case NoteName.A:
                        nn = 10;
                        break;
                    case NoteName.ASharp:
                        nn = 12;
                        break;
                    case NoteName.B:
                        nn = 13;
                        break;
                }

                return nn;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPanpotUpdated()
            {
                byte pan = CalcCurrentPanpot();
                if (pan < 32)
                    pan = 0x1;
                else if (pan > 96)
                    pan = 0x2;
                else
                    pan = 0x3;
                parentModule.Ym2151WriteData(unitNumber, 0x20, 0, Slot, (byte)(pan << 6 | (timbre.FB << 3) | timbre.ALG));
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                parentModule.Ym2151WriteData(unitNumber, 0x38, 0, Slot, (byte)((timbre.PMS << 4 | timbre.AMS)));
                for (int op = 0; op < 4; op++)
                {
                    parentModule.Ym2151WriteData(unitNumber, 0x40, op, Slot, (byte)((timbre.Ops[op].DT1 << 4 | timbre.Ops[op].MUL)));

                    switch (timbre.ALG)
                    {
                        case 0:
                            if (op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 1:
                            if (op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 2:
                            if (op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 3:
                            if (op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 4:
                            if (op != 1 && op != 3 && !timbre.UseExprForModulator)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 5:
                            if (op == 0 && !timbre.UseExprForModulator)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 6:
                            if (op == 0 && !timbre.UseExprForModulator)
                                parentModule.Ym2151WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                            break;
                        case 7:
                            break;
                    }
                    parentModule.Ym2151WriteData(unitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].AR)));
                    parentModule.Ym2151WriteData(unitNumber, 0xa0, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].D1R)));
                    parentModule.Ym2151WriteData(unitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].DT2 << 6 | timbre.Ops[op].D2R)));
                    parentModule.Ym2151WriteData(unitNumber, 0xe0, op, Slot, (byte)((timbre.Ops[op].SL << 4 | timbre.Ops[op].RR)));
                }

                OnPanpotUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                parentModule.Ym2151WriteData(unitNumber, 0x08, 0, 0, (byte)(0x00 | Slot));
            }

        }


        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2151Timbre>))]
        [DataContract]
        [InstLock]
        public class YM2151Timbre : TimbreBase, IFmTimbre
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

            #region FM Synth

            /// <summary>
            /// 
            /// </summary>
            /// <param name="inst"></param>
            /// <param name="timbre"></param>
            public override bool CanOpenTimbreEditor(InstrumentBase inst)
            {
                return true;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="inst"></param>
            /// <param name="timbre"></param>
            public override void OpenTimbreEditor(InstrumentBase inst)
            {
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(this);
                PropertyDescriptor pd = pdc["Detailed"];
                UITypeEditor ed = (UITypeEditor)pd.GetEditor(typeof(UITypeEditor));
                RuntimeServiceProvider serviceProvider = new RuntimeServiceProvider(null, this, pd);
                ed.EditValue(serviceProvider, serviceProvider, Detailed);

                //using (var f = new FormYM2151Editor((YM2151)inst, this, true))
                //{
                //    var sd = SerializeData;
                //    var r = f.ShowDialog();
                //    if (r != System.Windows.Forms.DialogResult.OK)
                //        SerializeData = sd;
                //}
            }

            public virtual bool ShouldSerializeDetailed()
            {
                return false;
            }

            [Category("Sound")]
            [Editor(typeof(YM2151UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [IgnoreDataMember]
            [JsonIgnore]
            [DisplayName("(FM resisters)")]
            [Description("Open FM register editor.")]
            [TypeConverter(typeof(OpenEditorTypeConverter))]
            public string Detailed
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(ALG),
                        nameof(FB),
                        nameof(AMS),
                        nameof(PMS),

                        "GlobalSettings.EN",
                        "GlobalSettings.LFRQ",
                        "GlobalSettings.AMD",
                        "GlobalSettings.PMD",
                        "GlobalSettings.LFOW",
                        "GlobalSettings.NE",
                        "GlobalSettings.NFRQ",

                        "Ops[0].EN",
                        "Ops[0].AR",
                        "Ops[0].D1R",
                        "Ops[0].D2R",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].TL",
                        "Ops[0].RS",
                        "Ops[0].MUL",
                        "Ops[0].DT1",
                        "Ops[0].AM",
                        "Ops[0].DT2",
                        "Ops[0].LS",
                        "Ops[0].KVS",

                        "Ops[1].EN",
                        "Ops[1].AR",
                        "Ops[1].D1R",
                        "Ops[1].D2R",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].TL",
                        "Ops[1].RS",
                        "Ops[1].MUL",
                        "Ops[1].DT1",
                        "Ops[1].AM",
                        "Ops[1].DT2",
                        "Ops[1].LS",
                        "Ops[1].KVS",

                        "Ops[2].EN",
                        "Ops[2].AR",
                        "Ops[2].D1R",
                        "Ops[2].D2R",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].TL",
                        "Ops[2].RS",
                        "Ops[2].MUL",
                        "Ops[2].DT1",
                        "Ops[2].AM",
                        "Ops[2].DT2",
                        "Ops[2].LS",
                        "Ops[2].KVS",

                        "Ops[3].EN",
                        "Ops[3].AR",
                        "Ops[3].D1R",
                        "Ops[3].D2R",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].TL",
                        "Ops[3].RS",
                        "Ops[3].MUL",
                        "Ops[3].DT1",
                        "Ops[3].AM",
                        "Ops[3].DT2",
                        "Ops[3].LS",
                        "Ops[3].KVS"
                        );
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),
                        nameof(AMS),
                        nameof(PMS),

                        "GlobalSettings.EN",
                        "GlobalSettings.LFRQ",
                        "GlobalSettings.AMD",
                        "GlobalSettings.PMD",
                        "GlobalSettings.LFOW",
                        "GlobalSettings.NE",
                        "GlobalSettings.NFRQ",

                        "Ops[0].EN",
                        "Ops[0].AR",
                        "Ops[0].D1R",
                        "Ops[0].D2R",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].TL",
                        "Ops[0].RS",
                        "Ops[0].MUL",
                        "Ops[0].DT1",
                        "Ops[0].AM",
                        "Ops[0].DT2",
                        "Ops[0].LS",
                        "Ops[0].KVS",

                        "Ops[1].EN",
                        "Ops[1].AR",
                        "Ops[1].D1R",
                        "Ops[1].D2R",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].TL",
                        "Ops[1].RS",
                        "Ops[1].MUL",
                        "Ops[1].DT1",
                        "Ops[1].AM",
                        "Ops[1].DT2",
                        "Ops[1].LS",
                        "Ops[1].KVS",

                        "Ops[2].EN",
                        "Ops[2].AR",
                        "Ops[2].D1R",
                        "Ops[2].D2R",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].TL",
                        "Ops[2].RS",
                        "Ops[2].MUL",
                        "Ops[2].DT1",
                        "Ops[2].AM",
                        "Ops[2].DT2",
                        "Ops[2].LS",
                        "Ops[2].KVS",

                        "Ops[3].EN",
                        "Ops[3].AR",
                        "Ops[3].D1R",
                        "Ops[3].D2R",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].TL",
                        "Ops[3].RS",
                        "Ops[3].MUL",
                        "Ops[3].DT1",
                        "Ops[3].AM",
                        "Ops[3].DT2",
                        "Ops[3].LS",
                        "Ops[3].KVS"
                        );
                }
            }

            private String patchFile;

            [DataMember]
            [Category("Sound")]
            [Description("FM Patch file info.\r\n*Warning* May contain privacy information. Check the options dialog.")]
            [ReadOnly(true)]
            public String PatchInfo
            {
                get
                {
                    if (Settings.Default.DoNotUsePrivacySettings)
                        return null;

                    return patchFile;
                }
                set
                {
                    patchFile = value;
                }
            }

            private byte f_ALG;

            [DataMember]
            [Category("Sound")]
            [Description("Algorithm (0-7)\r\n" +
                "0: 1->2->3->4 (for Distortion guitar sound)\r\n" +
                "1: (1+2)->3->4 (for Harp, PSG sound)\r\n" +
                "2: (1+(2->3))->4 (for Bass, electric guitar, brass, piano, woods sound)\r\n" +
                "3: ((1->2)+3)->4 (for Strings, folk guitar, chimes sound)\r\n" +
                "4: (1->2)+(3->4) (for Flute, bells, chorus, bass drum, snare drum, tom-tom sound)\r\n" +
                "5: (1->2)+(1->3)+(1->4) (for Brass, organ sound)\r\n" +
                "6: (1->2)+3+4 (for Xylophone, tom-tom, organ, vibraphone, snare drum, base drum sound)\r\n" +
                "7: 1+2+3+4 (for Pipe organ sound)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte ALG
            {
                get
                {
                    return f_ALG;
                }
                set
                {
                    f_ALG = (byte)(value & 7);
                }
            }

            private byte f_FB;

            [DataMember]
            [Category("Sound")]
            [Description("Feedback (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte FB
            {
                get
                {
                    return f_FB;
                }
                set
                {
                    f_FB = (byte)(value & 7);
                }
            }

            private byte f_AMS;

            [DataMember]
            [Category("Sound")]
            [Description("Amplitude Modulation Sensitivity (0-3)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AMS
            {
                get
                {
                    return f_AMS;
                }
                set
                {
                    f_AMS = (byte)(value & 3);
                }
            }

            private byte f_PMS;

            [DataMember]
            [Category("Sound")]
            [Description("Phase Modulation Sensitivity (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte PMS
            {
                get
                {
                    return f_PMS;
                }
                set
                {
                    f_PMS = (byte)(value & 7);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            [TypeConverter(typeof(ExpandableCollectionConverter))]
            [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
            [DisplayName("Operators[Ops]")]
            public YM2151Operator[] Ops
            {
                get;
                set;
            }

            public virtual bool ShouldSerializeOps()
            {
                bool ret = false;
                Parallel.For(0, Ops.Length, (i, loopState) =>
                {
                    if (loopState.IsStopped)
                        return;
                    if (!string.Equals(JsonConvert.SerializeObject(Ops[i], Formatting.Indented), "{}"))
                    {
                        loopState.Stop();
                        ret = true;
                        return;
                    }
                });
                return ret;
            }

            public virtual void ResetOps()
            {
                var ops = new YM2151Operator[] {
                    new YM2151Operator(),
                    new YM2151Operator(),
                    new YM2151Operator(),
                    new YM2151Operator() };
                for (int i = 0; i < Ops.Length; i++)
                    Ops[i].InjectFrom(new LoopInjection(), ops[i]);
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Description("You can copy and paste this text data to other same type timber.\r\n" +
                "ALG, FB, AR, D1R(DR), D2R(SR), RR, SL, TL, RS(KS), MUL, DT1, AM(AMS), DT2, ... , AMS, PMS\r\n" +
                "You can use comma or space chars as delimiter.")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(ALG),
                        nameof(FB),

                        "Ops[0].AR",
                        "Ops[0].D1R",
                        "Ops[0].D2R",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].TL",
                        "Ops[0].RS",
                        "Ops[0].MUL",
                        "Ops[0].DT1",
                        "Ops[0].AM",
                        "Ops[0].DT2",
                        "Ops[0].LS",
                        "Ops[0].KVS",

                        "Ops[1].AR",
                        "Ops[1].D1R",
                        "Ops[1].D2R",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].TL",
                        "Ops[1].RS",
                        "Ops[1].MUL",
                        "Ops[1].DT1",
                        "Ops[1].AM",
                        "Ops[1].DT2",
                        "Ops[1].LS",
                        "Ops[1].KVS",

                        "Ops[2].AR",
                        "Ops[2].D1R",
                        "Ops[2].D2R",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].TL",
                        "Ops[2].RS",
                        "Ops[2].MUL",
                        "Ops[2].DT1",
                        "Ops[2].AM",
                        "Ops[2].DT2",
                        "Ops[2].LS",
                        "Ops[2].KVS",

                        "Ops[3].AR",
                        "Ops[3].D1R",
                        "Ops[3].D2R",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].TL",
                        "Ops[3].RS",
                        "Ops[3].MUL",
                        "Ops[3].DT1",
                        "Ops[3].AM",
                        "Ops[3].DT2",
                        "Ops[3].LS",
                        "Ops[3].KVS",

                        nameof(AMS),
                        nameof(PMS));
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),

                        "Ops[0].AR",
                        "Ops[0].D1R",
                        "Ops[0].D2R",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].TL",
                        "Ops[0].RS",
                        "Ops[0].MUL",
                        "Ops[0].DT1",
                        "Ops[0].AM",
                        "Ops[0].DT2",
                        "Ops[0].LS",
                        "Ops[0].KVS",

                        "Ops[1].AR",
                        "Ops[1].D1R",
                        "Ops[1].D2R",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].TL",
                        "Ops[1].RS",
                        "Ops[1].MUL",
                        "Ops[1].DT1",
                        "Ops[1].AM",
                        "Ops[1].DT2",
                        "Ops[1].LS",
                        "Ops[1].KVS",

                        "Ops[2].AR",
                        "Ops[2].D1R",
                        "Ops[2].D2R",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].TL",
                        "Ops[2].RS",
                        "Ops[2].MUL",
                        "Ops[2].DT1",
                        "Ops[2].AM",
                        "Ops[2].DT2",
                        "Ops[2].LS",
                        "Ops[2].KVS",

                        "Ops[3].AR",
                        "Ops[3].D1R",
                        "Ops[3].D2R",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].TL",
                        "Ops[3].RS",
                        "Ops[3].MUL",
                        "Ops[3].DT1",
                        "Ops[3].AM",
                        "Ops[3].DT2",
                        "Ops[3].LS",
                        "Ops[3].KVS",

                        nameof(AMS),
                        nameof(PMS));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [DefaultValue(false)]
            [Description("Use MIDI Expresion for Career Total Level.")]
            [Browsable(true)]
            public override bool UseExprForModulator
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Traget modulator for UseExprForModulator.")]
            [Browsable(true)]
            [Editor(typeof(FlagEnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public ExprModulators ExprTargetModulators
            {
                get;
                set;
            } = ExprModulators.All;

            public virtual bool ShouldSerializeExprTargetModulators()
            {
                return ExprTargetModulators != ExprModulators.All;
            }

            public virtual void ResetExprTargetModulators()
            {
                ExprTargetModulators = ExprModulators.All;
            }

            [FlagsAttribute]
            public enum ExprModulators
            {
                [Description("Operator 1[Ops[0]")]
                Ops0 = 1,
                [Description("Operator 2[Ops[1]")]
                Ops1 = 2,
                [Description("Operator 3[Ops[2]")]
                Ops2 = 4,
                [Description("All")]
                All = 7,
            }


            [DataMember]
            [Category("Chip(Global)")]
            [Description("Global Settings")]
            public YM2151GlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new YM2151GlobalSettings());
            }

            /// <summary>
            /// 
            /// </summary>
            public YM2151Timbre()
            {
                Ops = new YM2151Operator[] {
                    new YM2151Operator(),
                    new YM2151Operator(),
                    new YM2151Operator(),
                    new YM2151Operator() };
                GlobalSettings = new YM2151GlobalSettings();
            }

            #endregion

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM2151Timbre>(serializeData);
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

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2151Operator>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class YM2151Operator : YMOperatorBase, ISerializeDataSaveLoad
        {

            private byte f_Enable = 1;

            /// <summary>
            /// Enable(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Whether this operator enable or not")]
            [DefaultValue((byte)1)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte Enable
            {
                get
                {
                    return f_Enable;
                }
                set
                {
                    f_Enable = (byte)(value & 1);
                }
            }

            [IgnoreDataMember]
            [JsonIgnore]
            [Browsable(false)]
            public byte EN
            {
                get
                {
                    return Enable;
                }
                set
                {
                    Enable = value;
                }
            }

            private byte f_AR;

            /// <summary>
            /// Attack Rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Attack Rate (0-31)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AR
            {
                get
                {
                    return f_AR;
                }
                set
                {
                    f_AR = (byte)(value & 31);
                }
            }

            private byte f_D1R;

            /// <summary>
            /// Decay rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Decay Rate (0-31)")]
            [DisplayName("D1R(DR)[D1R]")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte D1R
            {
                get
                {
                    return f_D1R;
                }
                set
                {
                    f_D1R = (byte)(value & 31);
                }
            }

            private byte f_D2R;

            /// <summary>
            /// Sustain Rate(2nd Decay Rate)(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Rate(2nd Decay Rate) (0-31)")]
            [DisplayName("D2R(SR)[D2R]")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte D2R
            {
                get
                {
                    return f_D2R;
                }
                set
                {
                    f_D2R = (byte)(value & 31);
                }
            }

            private byte f_RR;

            /// <summary>
            /// release rate(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Release Rate (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte RR
            {
                get
                {
                    return f_RR;
                }
                set
                {
                    f_RR = (byte)(value & 15);
                }
            }

            private byte f_SL;

            /// <summary>
            /// sustain level(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Level(0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte SL
            {
                get
                {
                    return f_SL;
                }
                set
                {
                    f_SL = (byte)(value & 15);
                }
            }

            private byte f_TL;

            /// <summary>
            /// Total Level(0-127)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Total Level (0-127)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte TL
            {
                get
                {
                    return f_TL;
                }
                set
                {
                    f_TL = (byte)(value & 127);
                }
            }

            private byte f_RS;

            /// <summary>
            /// Rate Scaling(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Rate Scaling (0-3)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte RS
            {
                get
                {
                    return f_RS;
                }
                set
                {
                    f_RS = (byte)(value & 3);
                }
            }

            private byte f_MUL;

            /// <summary>
            /// Multiply(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Multiply (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte MUL
            {
                get
                {
                    return f_MUL;
                }
                set
                {
                    f_MUL = (byte)(value & 15);
                }
            }

            private byte f_DT1 = 4;

            /// <summary>
            /// Detune1(0-7)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("DeTune 1 (7(-3),6(-2),5(-1),4(+0),0(+0),1(+1),2(+2),3(+3))")]
            [DefaultValue((byte)4)]
            [SlideParametersAttribute(1, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DT1
            {
                get
                {
                    return f_DT1;
                }
                set
                {
                    f_DT1 = (byte)(value & 7);
                }
            }

            private byte f_AM;

            /// <summary>
            /// AMS Enable (0:Disable 1:Enable)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Amplitude Modulation Sensivity (0-1)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AM
            {
                get
                {
                    return f_AM;
                }
                set
                {
                    f_AM = (byte)(value & 1);
                }
            }


            private byte f_DT2;

            /// <summary>
            /// DT2(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Detune 2 (0-3)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DT2
            {
                get
                {
                    return f_DT2;
                }
                set
                {
                    f_DT2 = (byte)(value & 3);
                }
            }

            #region Etc

            [DataMember]
            [Description("Memo")]
            [DefaultValue(null)]
            public string Memo
            {
                get;
                set;
            }


            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Description("You can copy and paste this text data to other same type timber.\r\n" +
                "AR, D1R(DR), D2R(SR), RR, SL, TL, RS(KS), MUL, DT1, AM(AMS), DT2\r\n" +
                "You can use comma or space chars as delimiter.")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(AR),
                        nameof(D1R),
                        nameof(D2R),
                        nameof(RR),
                        nameof(SL),
                        nameof(TL),
                        nameof(RS),
                        nameof(MUL),
                        nameof(DT1),
                        nameof(AM),
                        nameof(DT2));
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(AR),
                        nameof(D1R),
                        nameof(D2R),
                        nameof(RR),
                        nameof(SL),
                        nameof(TL),
                        nameof(RS),
                        nameof(MUL),
                        nameof(DT1),
                        nameof(AM),
                        nameof(DT2));
                }
            }

            [Editor(typeof(SerializeSaveUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [IgnoreDataMember]
            [JsonIgnore]
            [DisplayName("(Save settings)")]
            [Description("Save all parameters as serialize data to the file.")]
            [TypeConverter(typeof(OpenFileBrowserTypeConverter))]
            public string SerializeDataSave
            {
                get
                {
                    return SerializeData;
                }
                set
                {
                    SerializeData = value;
                }
            }


            [Editor(typeof(SerializeLoadUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [IgnoreDataMember]
            [JsonIgnore]
            [DisplayName("(Load settings)")]
            [Description("Load all parameters as serialize data from the file.")]
            [TypeConverter(typeof(OpenFileBrowserTypeConverter))]
            public string SerializeDataLoad
            {
                get
                {
                    return SerializeData;
                }
                set
                {
                    SerializeData = value;
                }
            }

            public virtual bool ShouldSerializeSerializeDataSave()
            {
                return false;
            }

            public virtual bool ShouldSerializeSerializeDataLoad()
            {
                return false;
            }

            [Browsable(false)]
            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Description("You can copy and paste this text data to other same type timber.\r\nNote: Open dropdown editor then copy all text and paste to dropdown editor. Do not copy and paste one liner text.")]
            public string SerializeData
            {
                get
                {
                    return JsonConvert.SerializeObject(this, Formatting.Indented);
                }
                set
                {
                    RestoreFrom(value);
                }
            }

            public void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM2151Operator>(serializeData);
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

            #endregion

        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2151GlobalSettings>))]
        [DataContract]
        [InstLock]
        public class YM2151GlobalSettings : ContextBoundObject
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


            [IgnoreDataMember]
            [JsonIgnore]
            [Browsable(false)]
            public byte EN
            {
                get
                {
                    return Enable ? (byte)1 : (byte)0;
                }
                set
                {
                    Enable = value == 0 ? false : true;
                }
            }

            private byte? f_LFRQ;

            /// <summary>
            /// LFRQ (0-255)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("LFO Freq (0-255)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFRQ
            {
                get
                {
                    return f_LFRQ;
                }
                set
                {
                    f_LFRQ = value;
                }
            }

            private byte? f_LFOF;

            /// <summary>
            /// Select AMD or PMD(0:AMD 1:PMD)
            /// </summary>
            [Browsable(false)]
            [Obsolete]
            public byte? LFOF
            {
                get
                {
                    if (!f_LFOF.HasValue)
                        return null;

                    return f_LFOF;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_LFOF = v;
                }
            }

            private byte? f_LFOD;


            /// <summary>
            /// LFO Depth(0-127)
            /// </summary>
            [Obsolete]
            [Browsable(false)]
            public byte? LFOD
            {
                get
                {
                    return f_LFOD;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 127);
                    f_LFOD = v;
                    if (f_LFOF.HasValue)
                    {
                        if (f_LFOF == 0)
                            f_AMD = f_LFOD;
                        else
                            f_PMD = f_LFOD;
                    }
                }
            }

            private byte? f_AMD;

            /// <summary>
            /// AMD Depth(0-127)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("AMD(0-127)")]
            [SlideParametersAttribute(0, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            [DisplayName("AMD")]
            public byte? AMD
            {
                get
                {
                    return f_AMD;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 127);
                    f_AMD = v;
                }
            }

            private byte? f_PMD;

            /// <summary>
            /// PMD Depth(0-127)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("PMD(0-127)")]
            [SlideParametersAttribute(0, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((byte)0)]
            [DisplayName("PMD")]
            public byte? PMD
            {
                get
                {
                    return f_PMD;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 127);
                    f_PMD = v;
                }
            }

            private byte? f_LFOW;


            /// <summary>
            /// LFO Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("LFO Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFOW
            {
                get
                {
                    return f_LFOW;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 3);
                    f_LFOW = v;
                }
            }

            private byte? f_NE;

            /// <summary>
            /// Noise Enable (0:Disable 1:Enable)
            /// </summary>
            [Browsable(false)]
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Noise Enable (0:Disable 1:Enable)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? NE
            {
                get
                {
                    return f_NE;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_NE = v;
                }
            }

            private byte? f_NFRQ;

            /// <summary>
            /// Noise Frequency (0-31)
            /// </summary>
            [Browsable(false)]
            [DataMember]
            [Category("Chip(Global)")]
            [Description(" Noise Feequency (0-31)\r\n" +
                "3'579'545/(32*NFRQ)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? NFRQ
            {
                get
                {
                    return f_NFRQ;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 31);
                    f_NFRQ = v;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum OPMSlotNo
        {
            Id0 = 0,
            Id1 = 1,
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [IgnoreDataMember]
        [JsonIgnore]
        public override bool CanImportToneFile
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timbre"></param>
        /// <param name="tone"></param>
        public override void ImportToneFile(TimbreBase timbre, Tone tone)
        {
            YM2151Timbre tim = (YM2151Timbre)timbre;

            tim.ALG = (byte)tone.AL;
            tim.FB = (byte)tone.FB;
            tim.AMS = (byte)tone.AMS;
            tim.PMS = (byte)tone.PMS;
            tim.GlobalSettings.Enable = false;
            tim.GlobalSettings.LFRQ = (byte?)tone.LFRQ;
            tim.GlobalSettings.AMD = (byte?)tone.AMD;
            tim.GlobalSettings.PMD = (byte?)tone.PMD;
            tim.GlobalSettings.LFOW = (byte?)tone.LFOW;
            tim.GlobalSettings.NE = (byte?)tone.NE;
            tim.GlobalSettings.NFRQ = (byte?)tone.NF;
            if (tim.GlobalSettings.NE > 0 ||
                tim.GlobalSettings.LFRQ > 0 ||
                tim.GlobalSettings.LFOW > 0 ||
                tim.GlobalSettings.AMD > 0 ||
                tim.GlobalSettings.PMD > 0
                )
                tim.GlobalSettings.Enable = true;

            for (int i = 0; i < 4; i++)
            {
                tim.Ops[i].Enable = 1;
                tim.Ops[i].AR = (byte)tone.aOp[i].AR;
                tim.Ops[i].D1R = (byte)tone.aOp[i].DR;
                tim.Ops[i].D2R = tone.aOp[i].SR < 0 ? (byte)0 : (byte)tone.aOp[i].SR;
                tim.Ops[i].RR = (byte)tone.aOp[i].RR;
                tim.Ops[i].SL = (byte)tone.aOp[i].SL;
                tim.Ops[i].TL = (byte)tone.aOp[i].TL;
                tim.Ops[i].RS = (byte)tone.aOp[i].KS;
                tim.Ops[i].MUL = (byte)tone.aOp[i].ML;
                tim.Ops[i].DT1 = (byte)tone.aOp[i].DT;
                tim.Ops[i].AM = (byte)tone.aOp[i].AM;
                tim.Ops[i].DT2 = (byte)tone.aOp[i].DT2;
                tim.Ops[i].LS = (byte)tone.aOp[i].LS;
                tim.Ops[i].KVS = (byte)tone.aOp[i].KVS;
            }
            timbre.TimbreName = tone.Name;
        }


        private YM2151CustomToneImporter importer;

        /// <summary>
        /// 
        /// </summary>
        public override CustomToneImporter CustomToneImporter
        {
            get
            {
                if (importer == null)
                {
                    importer = new YM2151CustomToneImporter();
                }
                return importer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class YM2151CustomToneImporter : FmToneImporter
        {

            /// <summary>
            /// 
            /// </summary>
            public override string ExtensionsFilterExt
            {
                get
                {
                    return "*.mopm;*.mopn";
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            public override IEnumerable<Tone> ImportToneFile(string file)
            {
                IEnumerable<Tone> tones = base.ImportToneFile(file);
                if (tones != null)
                    return tones;

                string ext = System.IO.Path.GetExtension(file);

                if (ext.ToUpper(CultureInfo.InvariantCulture).Equals(".MOPN"))
                {
                    try
                    {
                        string txt = System.IO.File.ReadAllText(file);
                        StringReader rs = new StringReader(txt);

                        string ftname = rs.ReadLine();
                        if ("*.mopn" == ftname)
                        {
                            string ver = rs.ReadLine();
                            if (ver != "1.0")
                                throw new InvalidDataException();
                            int num = int.Parse(rs.ReadLine());
                            List<string> lines = new List<string>();
                            List<Tone> ts = new List<Tone>();
                            int progNo = 0;
                            while (true)
                            {
                                string line = rs.ReadLine();
                                if (line == null || line == "-")
                                {
                                    if (lines.Count == 0)
                                        break;
                                    Tone t = new Tone();
                                    var mml = lines.ToArray();

                                    var general = mml[1].Split(',');
                                    mml[1] = String.Format("{0},{1},{2},0,0,,,,,,", general[0], general[1], general[2]);
                                    for (int i = 2; i < mml.Length; i++)
                                    {
                                        var op = mml[i].Split(',');
                                        mml[i] = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},0,0,-1",
                                            op[0], op[1], op[2], op[3], op[4], op[5], op[6], op[7], op[8], op[9], op[10]);
                                    }
                                    t.MML = mml;

                                    t.Name = t.MML[0];
                                    t.Number = progNo++;
                                    ts.Add(t);
                                    lines.Clear();
                                    if (line == null)
                                        break;
                                    continue;
                                }
                                lines.Add(line);
                            }
                            tones = ts;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(Exception))
                            throw;
                        else if (ex.GetType() == typeof(SystemException))
                            throw;

                        MessageBox.Show(Resources.FailedLoadFile + "\r\n" + ex.Message);
                    }
                }
                return tones;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="tones"></param>
            /// <returns></returns>
            public override IEnumerable<TimbreBase> ImportToneFileAsTimbre(string file)
            {
                IEnumerable<Tone> tones = ImportToneFile(file);
                if (tones != null)
                {
                    List<TimbreBase> rv = new List<TimbreBase>();
                    foreach (var t in tones)
                    {
                        YM2151Timbre tim = new YM2151Timbre();
                        tim.TimbreName = t.MML[0];
                        tim.Detailed = t.MML[1] + "," + t.MML[2] + "," + t.MML[3] + "," + t.MML[4] + "," + t.MML[5];
                        rv.Add(tim);
                    }
                    return rv;
                }
                return null;
            }

        }

    }
}