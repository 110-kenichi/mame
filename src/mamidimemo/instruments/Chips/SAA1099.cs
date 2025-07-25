﻿// copyright-holders:K.Ito
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static zanac.MAmidiMEmo.Instruments.Chips.AY8910;
using static zanac.MAmidiMEmo.Instruments.Chips.YMF262;

//https://datasheetspdf.com/pdf-file/493200/Philips/SAA1099/1
//https://www.vogons.org/viewtopic.php?t=38350

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class SAA1099 : InstrumentBase
    {

        public override string Name => "SAA1099";

        public override string Group => "PSG";

        public override InstrumentType InstrumentType => InstrumentType.SAA1099;

        [Browsable(false)]
        public override string ImageKey => "SAA1099";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "saa1099_";

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
                return 33;
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

        private SoundEngineType f_SoundEngineType;

        private SoundEngineType f_CurrentSoundEngineType;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Select a sound engine type.\r\n" +
            "Supports Software and VSIF - MSX.")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeSAA1099))]
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

        private class EnumConverterSoundEngineTypeSAA1099 : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.VSIF_MSX_FTDI,
                    SoundEngineType.VSIF_P6_FTDI,
                    SoundEngineType.VSIF_MSX_Pi,
                    SoundEngineType.VSIF_MSX_PiTr,
                });

                return sc;
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


        /// <summary>
        /// 
        /// </summary>
        public enum MasterClockType : uint
        {
            Default = 8000000,
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

        /////////////////////

        private byte f_Noise0Rate;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Noise 0 Rate")]
        [SlideParametersAttribute(0, 3)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Noise0Rate
        {
            get => f_Noise0Rate;
            set
            {
                byte v = (byte)(value & 3);
                if (f_Noise0Rate != v)
                {
                    f_Noise0Rate = v;
                    SAA1099WriteData(UnitNumber, 0x16, (byte)((f_Noise1Rate << 4) | f_Noise0Rate));
                }
            }
        }

        private byte f_Noise1Rate;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Noise 1 Rate")]
        [SlideParametersAttribute(0, 3)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Noise1Rate
        {
            get => f_Noise1Rate;
            set
            {
                byte v = (byte)(value & 3);
                if (f_Noise1Rate != v)
                {
                    f_Noise1Rate = v;
                    SAA1099WriteData(UnitNumber, 0x16, (byte)((f_Noise1Rate << 4) | f_Noise0Rate));
                }
            }
        }

        /////////////////////

        private bool f_Envelope0Enabled;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Enabled")]
        [DefaultValue(false)]
        public bool Envelope0Enabled
        {
            get => f_Envelope0Enabled;
            set
            {
                if (f_Envelope0Enabled != value)
                {
                    f_Envelope0Enabled = value;
                    writeEnvelope0();
                }
            }
        }

        private byte f_Envelope0ReverseR;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Reverse Right ch")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Envelope0ReverseR
        {
            get => f_Envelope0ReverseR;
            set
            {
                byte v = (byte)(value & 1);
                if (f_Envelope0ReverseR != v)
                {
                    f_Envelope0ReverseR = v;
                    writeEnvelope0();
                }
            }
        }

        private byte f_Envelope0Type;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Bits")]
        [SlideParametersAttribute(0, 7)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Envelope0Type
        {
            get => f_Envelope0Type;
            set
            {
                byte v = (byte)(value & 7);
                if (f_Envelope0Type != v)
                {
                    f_Envelope0Type = v;
                    writeEnvelope0();
                }
            }
        }

        private byte f_Envelope0Bits;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Bits")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Envelope0Bits
        {
            get => f_Envelope0Bits;
            set
            {
                byte v = (byte)(value & 1);
                if (f_Envelope0Bits != v)
                {
                    f_Envelope0Bits = v;
                    writeEnvelope0();
                }
            }
        }

        private byte f_Envelope0Clock;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Clock")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Envelope0Clock
        {
            get => f_Envelope0Clock;
            set
            {
                byte v = (byte)(value & 1);
                if (f_Envelope0Clock != v)
                {
                    f_Envelope0Clock = v;
                    writeEnvelope0();
                }
            }
        }

        private void writeEnvelope0()
        {
            byte en = f_Envelope0Enabled ? (byte)1 : (byte)0;
            SAA1099WriteData(UnitNumber, 0x18, (byte)((en << 7) | (f_Envelope0Clock << 5) | (f_Envelope0Bits << 4) | (f_Envelope0Type << 1) | f_Envelope0ReverseR), false);
        }

        /////////////////////

        private bool f_Envelope1Enabled;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Enabled")]
        [DefaultValue(false)]
        public bool Envelope1Enabled
        {
            get => f_Envelope1Enabled;
            set
            {
                if (f_Envelope1Enabled != value)
                {
                    f_Envelope1Enabled = value;
                    writeEnvelope1();
                }
            }
        }

        private byte f_Envelope1ReverseR;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Reverse Right ch")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Envelope1ReverseR
        {
            get => f_Envelope1ReverseR;
            set
            {
                byte v = (byte)(value & 1);
                if (f_Envelope1ReverseR != v)
                {
                    f_Envelope1ReverseR = v;
                    writeEnvelope1();
                }
            }
        }

        private byte f_Envelope1Type;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Bits")]
        [SlideParametersAttribute(0, 7)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Envelope1Type
        {
            get => f_Envelope1Type;
            set
            {
                byte v = (byte)(value & 7);
                if (f_Envelope1Type != v)
                {
                    f_Envelope1Type = v;
                    writeEnvelope1();
                }
            }
        }

        private byte f_Envelope1Bits;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Bits")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Envelope1Bits
        {
            get => f_Envelope1Bits;
            set
            {
                byte v = (byte)(value & 1);
                if (f_Envelope1Bits != v)
                {
                    f_Envelope1Bits = v;
                    writeEnvelope1();
                }
            }
        }

        private byte f_Envelope1Clock;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Set Envelope 0 Clock")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Envelope1Clock
        {
            get => f_Envelope1Clock;
            set
            {
                byte v = (byte)(value & 1);
                if (f_Envelope1Clock != v)
                {
                    f_Envelope1Clock = v;
                    writeEnvelope1();
                }
            }
        }

        private void writeEnvelope1()
        {
            byte en = f_Envelope1Enabled ? (byte)1 : (byte)0;
            SAA1099WriteData(UnitNumber, 0x19, (byte)((en << 7) | (f_Envelope1Clock << 5) | (f_Envelope1Bits << 4) | (f_Envelope1Type << 1) | f_Envelope1ReverseR), false);
        }

        /////////////////////

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
                Timbres = (SAA1099Timbre[])value;
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
        public SAA1099Timbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<SAA1099>(serializeData))
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
        private delegate void delegate_saa1099_device_write(uint unitNumber, uint offset, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_saa1099_device_write saa1099_device_write
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
            SAA1099WriteData(UnitNumber, (byte)address, (byte)data, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="offset"></param>
        /// <param name="data"></param>
        private void SAA1099WriteData(uint unitNumber, uint offset, byte data)
        {
            SAA1099WriteData(unitNumber, offset, data, true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SAA1099WriteData(uint unitNumber, uint offset, byte data, bool useCache)
        {
            WriteData(offset, data, useCache, new Action(() =>
            {
                lock (sndEnginePtrLock)
                {
                    switch (CurrentSoundEngine)
                    {
                        case SoundEngineType.VSIF_MSX_FTDI:
                        case SoundEngineType.VSIF_P6_FTDI:
                        case SoundEngineType.VSIF_MSX_Pi:
                        case SoundEngineType.VSIF_MSX_PiTr:
                            vsifClient.WriteData(0x1e, (byte)0x05, (byte)offset, f_ftdiClkWidth);
                            vsifClient.WriteData(0x1e, (byte)0x04, (byte)data, f_ftdiClkWidth);
                            break;
                    }
                }
                DeferredWriteData(saa1099_device_write, unitNumber, (uint)1, (byte)offset);
                DeferredWriteData(saa1099_device_write, unitNumber, (uint)0, (byte)data);
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
        private void SAA1099WriteControlData(uint unitNumber, uint offset, bool useCache)
        {
            WriteData(offset, 0, useCache, new Action(() =>
            {
                DeferredWriteData(saa1099_device_write, unitNumber, (uint)1, (byte)offset);
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
        private byte SAA1099ReadData(uint unitNumber, uint offset)
        {
            try
            {
                Program.SoundUpdating();
                FlushDeferredWriteData();

                var wd = GetCachedWrittenData(offset);
                if (wd != null)
                    return (byte)wd.Value;

                return 0;
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static SAA1099()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("saa1099_device_write");
            if (funcPtr != IntPtr.Zero)
            {
                saa1099_device_write = (delegate_saa1099_device_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_saa1099_device_write));
            }
        }

        private SAA1099SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public SAA1099(uint unitNumber) : base(unitNumber)
        {
            MasterClock = (uint)MasterClockType.Default;

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new SAA1099Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new SAA1099Timbre();
            setPresetInstruments();

            this.soundManager = new SAA1099SoundManager(this);
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
            //0x1c | ---- ---x | All channels enable (0 = off, 1 = on)
            //0x1c | ---- --x- | Synch & Reset generators
            SAA1099WriteData(UnitNumber, 0x1c, (byte)(0x1));

            SAA1099WriteData(UnitNumber, 0x16, (byte)((f_Noise1Rate << 4) | f_Noise0Rate));

            byte en = f_Envelope0Enabled ? (byte)1 : (byte)0;
            SAA1099WriteData(UnitNumber, 0x18, (byte)((en << 7) | (f_Envelope0Clock << 5) | (f_Envelope0Bits << 4) | (f_Envelope0Type << 1) | f_Envelope0ReverseR));

            en = f_Envelope1Enabled ? (byte)1 : (byte)0;
            SAA1099WriteData(UnitNumber, 0x19, (byte)((en << 7) | (f_Envelope1Clock << 5) | (f_Envelope1Bits << 4) | (f_Envelope1Type << 1) | f_Envelope1ReverseR));
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
            /*
            var rp = InstrumentManager.GetInstruments(6).ToArray()[0];
            var ay = InstrumentManager.GetInstruments(11).ToArray()[0];

            for (int i = 0; i < 256; i++)
            {
                RP2A03.RP2A03Timbre rpt = (RP2A03.RP2A03Timbre)rp.BaseTimbres[i];
                SAA1099.SAA1099Timbre ayt = (SAA1099.SAA1099Timbre)ay.BaseTimbres[i];

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
        }

        internal override void ResetAll()
        {
            ClearWrittenDataCache();
            PrepareSound();
        }

        /// <summary>
        /// 
        /// </summary>
        private class SAA1099SoundManager : SoundManagerBase
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

            private static SoundList<SAA1099Sound> psg0OnSounds = new SoundList<SAA1099Sound>(3);

            private static SoundList<SAA1099Sound> psg1OnSounds = new SoundList<SAA1099Sound>(3);

            private SAA1099 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SAA1099SoundManager(SAA1099 parent) : base(parent)
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
                foreach (SAA1099Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note, timbre);
                    if (emptySlot.slot < 0)
                        continue;

                    SAA1099Sound snd = new SAA1099Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                        psg0OnSounds.Add(snd);
                    else
                        psg1OnSounds.Add(snd);

                    if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                        FormMain.OutputDebugLog(parentModule, "KeyOn Lo ch" + emptySlot + " " + note.ToString());
                    else
                        FormMain.OutputDebugLog(parentModule, "KeyOn Hi ch" + emptySlot + " " + note.ToString());
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
            private (SAA1099 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note, SAA1099Timbre timbre)
            {
                var emptySlot = (parentModule, -1);

                if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                    emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psg0OnSounds, note, 3);
                else
                    emptySlot = SearchEmptySlotAndOffForLeader(parentModule, psg1OnSounds, note, 3);

                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (byte ch = 0; ch < 6; ch++)
                    parentModule.SAA1099WriteData(parentModule.UnitNumber, ch, 0);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class SAA1099Sound : SoundBase
        {

            private SAA1099 parentModule;

            private SAA1099Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SAA1099Sound(SAA1099 parentModule, SAA1099SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (SAA1099Timbre)timbre;
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
                    if (gs.SyncGenerator.HasValue)
                        parentModule.SAA1099WriteData(parentModule.UnitNumber, 0x1c, 0x3, false);

                    bool writeEnv0 = false;
                    bool writeEnv1 = false;

                    if (gs.NoiseRate.HasValue)
                    {
                        if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                            parentModule.Noise0Rate = gs.NoiseRate.Value;
                        else
                            parentModule.Noise1Rate = gs.NoiseRate.Value;
                    }
                    if (gs.EnvelopeEnabled.HasValue)
                    {
                        if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                        {
                            parentModule.Envelope0Enabled = gs.EnvelopeEnabled.Value;
                            writeEnv0 = true;
                        }
                        else
                        {
                            parentModule.Envelope1Enabled = gs.EnvelopeEnabled.Value;
                            writeEnv1 = true;
                        }
                    }

                    if (gs.EnvelopeReverseR.HasValue)
                    {
                        if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                        {
                            parentModule.Envelope0ReverseR = gs.EnvelopeReverseR.Value;
                            writeEnv0 = true;
                        }
                        else
                        {
                            parentModule.Envelope1ReverseR = gs.EnvelopeReverseR.Value;
                            writeEnv1 = true;
                        }
                    }
                    if (gs.EnvelopeType.HasValue)
                    {
                        if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                        {
                            parentModule.Envelope0Type = gs.EnvelopeType.Value;
                            writeEnv0 = true;
                        }
                        else
                        {
                            parentModule.Envelope1Type = gs.EnvelopeType.Value;
                            writeEnv1 = true;
                        }
                    }
                    if (gs.EnvelopeBits.HasValue)
                    {
                        if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                        {
                            parentModule.Envelope0Bits = gs.EnvelopeBits.Value;
                            writeEnv0 = true;
                        }
                        else
                        {
                            parentModule.Envelope1Bits = gs.EnvelopeBits.Value;
                            writeEnv1 = true;
                        }
                    }
                    if (gs.EnvelopeClock.HasValue)
                    {
                        if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                        {
                            parentModule.Envelope0Clock = gs.EnvelopeClock.Value;
                            writeEnv0 = true;
                        }
                        else
                        {
                            parentModule.Envelope1Clock = gs.EnvelopeClock.Value;
                            writeEnv1 = true;
                        }
                    }
                    if (writeEnv0)
                        parentModule.writeEnvelope0();
                    if (writeEnv1)
                        parentModule.writeEnvelope1();
                }
                int slot = Slot;
                if (timbre.UseChannel == SAA1099Timbre.ChannelType.HighCh)
                    slot += 3;

                var freq = parentModule.SAA1099ReadData(parentModule.UnitNumber, (byte)0x14);
                freq &= (byte)~(1 << slot);
                if (timbre.FreqEnable)
                    freq |= (byte)(1 << slot);
                parentModule.SAA1099WriteData(parentModule.UnitNumber, (byte)0x14, freq);

                var noise = parentModule.SAA1099ReadData(parentModule.UnitNumber, (byte)0x15);
                if (timbre.NoiseEnable)
                    noise |= (byte)(1 << slot);
                parentModule.SAA1099WriteData(parentModule.UnitNumber, (byte)0x15, noise);

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
                    var eng = (SaaFxEngine)FxEngine;
                    if (eng.EnvelopeClocks != null)
                    {
                        if (eng.EnvelopeClocks != 0)
                        {
                            if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                                parentModule.SAA1099WriteControlData(parentModule.UnitNumber, (uint)0x18, false);
                            else
                                parentModule.SAA1099WriteControlData(parentModule.UnitNumber, (uint)0x19, false);
                        }
                    }

                    var gs = timbre.GlobalSettings;
                    if (gs.Enable)
                    {
                        if (eng.NoiseRates != null)
                        {
                            if (gs.NoiseRate.HasValue)
                            {
                                if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                                    parentModule.Noise0Rate = eng.NoiseRates.Value;
                                else
                                    parentModule.Noise1Rate = eng.NoiseRates.Value;
                            }
                        }

                        if (eng.EnvelopeTypes != null)
                        {
                            if (gs.NoiseRate.HasValue)
                            {
                                if (timbre.UseChannel == SAA1099Timbre.ChannelType.LowCh)
                                    parentModule.Envelope0Type = eng.EnvelopeTypes.Value;
                                else
                                    parentModule.Envelope1Type = eng.EnvelopeTypes.Value;
                            }
                        }
                    }
                }
            }

            public override void OnPanpotUpdated()
            {
                OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                if (IsSoundOff)
                    return;

                int pan = CalcCurrentPanpot();

                double left = 0;
                double right = 0;
                if (pan > 64)
                    left = Math.Cos(Math.PI / 2 * (pan / 127d));
                else
                    left = Math.Cos(Math.PI / 2 * (64 / 127d));
                if (pan < 64)
                    right = Math.Sin(Math.PI / 2 * (pan / 127d));
                else
                    right = Math.Sin(Math.PI / 2 * (64 / 127d));

                byte fvl = (byte)(((byte)Math.Round(15 * CalcCurrentVolume() * left) & 0xf));
                byte fvr = (byte)(((byte)Math.Round(15 * CalcCurrentVolume() * right) & 0xf));

                int slot = Slot;
                if (timbre.UseChannel == SAA1099Timbre.ChannelType.HighCh)
                    slot += 3;

                parentModule.SAA1099WriteData(parentModule.UnitNumber, (byte)slot, (byte)(fvr << 4 | fvl));
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                double d = CalcCurrentPitchDeltaNoteNumber();

                int nn = NoteOnEvent.NoteNumber;
                if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum ||
                    ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.DrumGt)
                    nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;
                int noteNum = nn + (int)d;
                if (noteNum > 127)
                    noteNum = 127;
                else if (noteNum < 0)
                    noteNum = 0;
                var nnOn = new TaggedNoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);

                //https://sam.speccy.cz/systech/sam-coupe_tech-man_v3-0.pdf
                double dfreq = CalcCurrentFrequency();
                int octave = nnOn.GetNoteOctave() - 1;// + convertOct(nnOn);
                if (octave < 0)
                    octave = 0;

                // note = 511 - ((15625 * (2^octave)) / frequency)
                int note;
                do
                {
                    note = (int)Math.Round(511d - (((parentModule.MasterClock / 512d) * Math.Pow(2, octave)) / dfreq));
                    if (note < 0)
                        octave--;
                    else if (note > 255)
                        octave++;
                } while ((note > 255) || note < 0);

                if (octave < 0)
                {
                    octave = 0;
                    note = 0;
                }
                else if (octave > 7)
                {
                    octave = 7;
                    note = 0xff;
                }

                int slot = Slot;
                if (timbre.UseChannel == SAA1099Timbre.ChannelType.HighCh)
                    slot += 3;

                //Freq
                parentModule.SAA1099WriteData(parentModule.UnitNumber, (byte)(0x8 + slot), (byte)note);
                //Oct
                var loct = parentModule.SAA1099ReadData(parentModule.UnitNumber, (byte)(0x10 + (slot >> 1)));
                if ((slot & 1) == 0)
                {
                    loct = (byte)(loct & 0xf0);
                    loct = (byte)(loct | octave);
                    parentModule.SAA1099WriteData(parentModule.UnitNumber, (byte)(0x10 + (slot >> 1)), loct);
                }
                else
                {
                    loct = (byte)(loct & 0x0f);
                    loct = (byte)(loct | (octave << 4));
                    parentModule.SAA1099WriteData(parentModule.UnitNumber, (byte)(0x10 + (slot >> 1)), loct);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                int slot = Slot;
                if (timbre.UseChannel == SAA1099Timbre.ChannelType.HighCh)
                    slot += 3;

                parentModule.SAA1099WriteData(parentModule.UnitNumber, (byte)slot, 0);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SAA1099Timbre>))]
        [DataContract]
        [InstLock]
        public class SAA1099Timbre : TimbreBase
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

            private ChannelType f_UseChannel = 0;

            [DataMember]
            [Category("Sound")]
            [Description("Using channel")]
            [DefaultValue(ChannelType.LowCh)]
            public ChannelType UseChannel
            {
                get
                {
                    return f_UseChannel;
                }
                set
                {
                    f_UseChannel = value;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public enum ChannelType
            {
                LowCh,
                HighCh,
            }

            private bool f_FreqEnable = true;

            [DataMember]
            [Category("Sound")]
            [Description("Frequency enable")]
            [DefaultValue(true)]
            public bool FreqEnable
            {
                get
                {
                    return f_FreqEnable;
                }
                set
                {
                    f_FreqEnable = value;
                }
            }

            private bool f_NoiseEnable;

            [DataMember]
            [Category("Sound")]
            [Description("Noise enable")]
            [DefaultValue(false)]
            public bool NoiseEnable
            {
                get
                {
                    return f_NoiseEnable;
                }
                set
                {
                    f_NoiseEnable = value;
                }
            }

            [DataMember]
            [Category("Chip(Global)")]
            [Description("Global Settings")]
            public SAA1099GlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new SAA1099GlobalSettings());
            }

            public SAA1099Timbre()
            {
                GlobalSettings = new SAA1099GlobalSettings();
            }

            protected override void InitializeFxS()
            {
                this.SDS.FxS = new SaFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SAA1099Timbre>(serializeData);
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

        [JsonConverter(typeof(NoTypeConverterJsonConverter<SaFxSettings>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class SaFxSettings : BasicFxSettings
        {

            private string f_EnvelopeClocks;

            [DataMember]
            [Description("Set Envelope Clock. Input sound type value and split it with space like the FamiTracker.\r\n" +
                       "1:Do clock 0:Not clock \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 1)]
            public string EnvelopeClocks
            {
                get
                {
                    return f_EnvelopeClocks;
                }
                set
                {
                    if (f_EnvelopeClocks != value)
                    {
                        EnvelopeClocksRepeatPoint = -1;
                        EnvelopeClocksReleasePoint = -1;
                        if (value == null)
                        {
                            EnvelopeClocksNums = new int[] { };
                            f_EnvelopeClocks = string.Empty;
                            return;
                        }
                        f_EnvelopeClocks = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                EnvelopeClocksRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                EnvelopeClocksReleasePoint = vs.Count;
                            else
                            {
                                int v;
                                if (int.TryParse(val, out v))
                                {
                                    if (v < 0)
                                        v = 0;
                                    else if (v > 1)
                                        v = 1;
                                    vs.Add(v);
                                }
                            }
                        }
                        EnvelopeClocksNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < EnvelopeClocksNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (EnvelopeClocksRepeatPoint == i)
                                sb.Append("| ");
                            if (EnvelopeClocksReleasePoint == i)
                                sb.Append("/ ");
                            if (i < EnvelopeClocksNums.Length)
                                sb.Append(EnvelopeClocksNums[i].ToString((IFormatProvider)null));
                        }
                        f_EnvelopeClocks = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeEnvelopeClocks()
            {
                return !string.IsNullOrEmpty(EnvelopeClocks);
            }

            public void ResetEnvelopeClocks()
            {
                EnvelopeClocks = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] EnvelopeClocksNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int EnvelopeClocksRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int EnvelopeClocksReleasePoint { get; set; } = -1;



            private string f_NoiseRates;

            [DataMember]
            [Description("Set Noise Rate. Input sound type value and split it with space like the FamiTracker.\r\n" +
                       "1:Do clock 0:Not clock \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 1)]
            public string NoiseRates
            {
                get
                {
                    return f_NoiseRates;
                }
                set
                {
                    if (f_NoiseRates != value)
                    {
                        NoiseRatesRepeatPoint = -1;
                        NoiseRatesReleasePoint = -1;
                        if (value == null)
                        {
                            NoiseRatesNums = new int[] { };
                            f_NoiseRates = string.Empty;
                            return;
                        }
                        f_NoiseRates = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                NoiseRatesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                NoiseRatesReleasePoint = vs.Count;
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
                        NoiseRatesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < NoiseRatesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (NoiseRatesRepeatPoint == i)
                                sb.Append("| ");
                            if (NoiseRatesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < NoiseRatesNums.Length)
                                sb.Append(NoiseRatesNums[i].ToString((IFormatProvider)null));
                        }
                        f_NoiseRates = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeNoiseRates()
            {
                return !string.IsNullOrEmpty(NoiseRates);
            }

            public void ResetNoiseRates()
            {
                NoiseRates = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] NoiseRatesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int NoiseRatesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int NoiseRatesReleasePoint { get; set; } = -1;



            private string f_EnvelopeTypes;

            [DataMember]
            [Description("Set Envelope Types. Input sound type value and split it with space like the FamiTracker.\r\n" +
                       "0-7:Envelope Type \"|\" is repeat point. \"/\" is release point.")]
            [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [EnvelopeEditorAttribute(0, 7)]
            public string EnvelopeTypes
            {
                get
                {
                    return f_EnvelopeTypes;
                }
                set
                {
                    if (f_EnvelopeTypes != value)
                    {
                        EnvelopeTypesRepeatPoint = -1;
                        EnvelopeTypesReleasePoint = -1;
                        if (value == null)
                        {
                            EnvelopeTypesNums = new int[] { };
                            f_EnvelopeTypes = string.Empty;
                            return;
                        }
                        f_EnvelopeTypes = value;
                        string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        List<int> vs = new List<int>();
                        for (int i = 0; i < vals.Length; i++)
                        {
                            string val = vals[i];
                            if (val.Equals("|", StringComparison.Ordinal))
                                EnvelopeTypesRepeatPoint = vs.Count;
                            else if (val.Equals("/", StringComparison.Ordinal))
                                EnvelopeTypesReleasePoint = vs.Count;
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
                        EnvelopeTypesNums = vs.ToArray();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < EnvelopeTypesNums.Length; i++)
                        {
                            if (sb.Length != 0)
                                sb.Append(' ');
                            if (EnvelopeTypesRepeatPoint == i)
                                sb.Append("| ");
                            if (EnvelopeTypesReleasePoint == i)
                                sb.Append("/ ");
                            if (i < EnvelopeTypesNums.Length)
                                sb.Append(EnvelopeTypesNums[i].ToString((IFormatProvider)null));
                        }
                        f_EnvelopeTypes = sb.ToString();
                    }
                }
            }

            public bool ShouldSerializeEnvelopeTypes()
            {
                return !string.IsNullOrEmpty(EnvelopeTypes);
            }

            public void ResetEnvelopeTypes()
            {
                EnvelopeTypes = null;
            }

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            public int[] EnvelopeTypesNums { get; set; } = new int[] { };

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int EnvelopeTypesRepeatPoint { get; set; } = -1;

            [Browsable(false)]
            [JsonIgnore]
            [IgnoreDataMember]
            [DefaultValue(-1)]
            public int EnvelopeTypesReleasePoint { get; set; } = -1;

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override AbstractFxEngine CreateEngine()
            {
                return new SaaFxEngine(this);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class SaaFxEngine : BasicFxEngine
        {
            private SaFxSettings settings;

            /// <summary>
            /// 
            /// </summary>
            public SaaFxEngine(SaFxSettings settings) : base(settings)
            {
                this.settings = settings;
            }

            private uint f_EnvelopeClocksCounter;

            public byte? EnvelopeClocks
            {
                get;
                private set;
            }

            private uint f_NoiseRatesCounter;

            public byte? NoiseRates
            {
                get;
                private set;
            }

            private uint f_EnvelopeTypesCounter;

            public byte? EnvelopeTypes
            {
                get;
                private set;
            }

            protected override bool ProcessCore(SoundBase sound, bool isKeyOff, bool isSoundOff)
            {
                bool process = base.ProcessCore(sound, isKeyOff, isSoundOff);

                EnvelopeClocks = null;
                if (settings.EnvelopeClocksNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.EnvelopeClocksNums.Length;
                        if (settings.EnvelopeClocksReleasePoint >= 0)
                            vm = settings.EnvelopeClocksReleasePoint;
                        if (f_EnvelopeClocksCounter >= vm)
                        {
                            if (settings.EnvelopeClocksRepeatPoint >= 0)
                                f_EnvelopeClocksCounter = (uint)settings.EnvelopeClocksRepeatPoint;
                            else
                                f_EnvelopeClocksCounter = (uint)vm - 1;
                        }
                    }
                    else
                    {
                        if (f_EnvelopeClocksCounter < settings.EnvelopeClocksNums.Length)
                        {
                            if (settings.EnvelopeClocksReleasePoint >= 0 && f_EnvelopeClocksCounter <= (uint)settings.EnvelopeClocksReleasePoint)
                                f_EnvelopeClocksCounter = (uint)settings.EnvelopeClocksReleasePoint;
                            else if (settings.EnvelopeClocksReleasePoint < 0 && settings.KeyOffStop)
                                f_EnvelopeClocksCounter = (uint)settings.EnvelopeClocksNums.Length;
                        }
                    }
                    if (f_EnvelopeClocksCounter < settings.EnvelopeClocksNums.Length)
                    {
                        int vol = settings.EnvelopeClocksNums[f_EnvelopeClocksCounter++];

                        EnvelopeClocks = (byte)vol;
                        process = true;
                    }
                }

                NoiseRates = null;
                if (settings.NoiseRatesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.NoiseRatesNums.Length;
                        if (settings.NoiseRatesReleasePoint >= 0)
                            vm = settings.NoiseRatesReleasePoint;
                        if (f_NoiseRatesCounter >= vm)
                        {
                            if (settings.NoiseRatesRepeatPoint >= 0)
                                f_NoiseRatesCounter = (uint)settings.NoiseRatesRepeatPoint;
                            else
                                f_NoiseRatesCounter = (uint)vm - 1;
                        }
                    }
                    else
                    {
                        if (f_NoiseRatesCounter < settings.NoiseRatesNums.Length)
                        {
                            if (settings.NoiseRatesReleasePoint >= 0 && f_NoiseRatesCounter <= (uint)settings.NoiseRatesReleasePoint)
                                f_NoiseRatesCounter = (uint)settings.NoiseRatesReleasePoint;
                            else if (settings.NoiseRatesReleasePoint < 0 && settings.KeyOffStop)
                                f_NoiseRatesCounter = (uint)settings.NoiseRatesNums.Length;
                        }
                    }
                    if (f_NoiseRatesCounter < settings.NoiseRatesNums.Length)
                    {
                        int vol = settings.NoiseRatesNums[f_NoiseRatesCounter++];

                        NoiseRates = (byte)vol;
                        process = true;
                    }
                }

                EnvelopeTypes = null;
                if (settings.EnvelopeTypesNums.Length > 0)
                {
                    if (!isKeyOff)
                    {
                        var vm = settings.EnvelopeTypesNums.Length;
                        if (settings.EnvelopeTypesReleasePoint >= 0)
                            vm = settings.EnvelopeTypesReleasePoint;
                        if (f_EnvelopeTypesCounter >= vm)
                        {
                            if (settings.EnvelopeTypesRepeatPoint >= 0 && settings.KeyOffStop)
                                f_EnvelopeTypesCounter = (uint)settings.EnvelopeTypesRepeatPoint;
                            else
                                f_EnvelopeTypesCounter = (uint)vm - 1;
                        }
                    }
                    else
                    {
                        if (f_EnvelopeTypesCounter < settings.EnvelopeTypesNums.Length)
                        {
                            if (settings.EnvelopeTypesReleasePoint >= 0 && f_EnvelopeTypesCounter <= (uint)settings.EnvelopeTypesReleasePoint)
                                f_EnvelopeTypesCounter = (uint)settings.EnvelopeTypesReleasePoint;
                            else if (settings.EnvelopeTypesReleasePoint < 0 && settings.KeyOffStop)
                                f_EnvelopeTypesCounter = (uint)settings.EnvelopeTypesNums.Length;
                        }
                    }
                    if (f_EnvelopeTypesCounter < settings.EnvelopeTypesNums.Length)
                    {
                        int vol = settings.EnvelopeTypesNums[f_EnvelopeTypesCounter++];

                        EnvelopeTypes = (byte)vol;
                        process = true;
                    }
                }

                return process;
            }
        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SAA1099GlobalSettings>))]
        [DataContract]
        [InstLock]
        public class SAA1099GlobalSettings : ContextBoundObject
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

            private bool? f_SyncGenerator;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Synch & Reset generators")]
            [DefaultValue(null)]
            public bool? SyncGenerator
            {
                get => f_SyncGenerator;
                set
                {
                    f_SyncGenerator = value;
                }
            }

            /////////////////////

            private byte? f_NoiseRate;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Set Noise Rate")]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? NoiseRate
            {
                get => f_NoiseRate;
                set
                {
                    if (value.HasValue)
                    {
                        byte v = (byte)(value & 3);
                        if (f_NoiseRate != v)
                        {
                            f_NoiseRate = v;
                        }
                    }
                    else
                        f_NoiseRate = value;
                }
            }

            /////////////////////

            private bool? f_EnvelopeEnabled;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Set Envelope Enabled")]
            [DefaultValue(null)]
            public bool? EnvelopeEnabled
            {
                get => f_EnvelopeEnabled;
                set
                {
                    if (f_EnvelopeEnabled != value)
                    {
                        f_EnvelopeEnabled = value;
                    }
                }
            }

            private byte? f_EnvelopeReverseR;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Set Envelope Reverse Right ch")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? EnvelopeReverseR
            {
                get => f_EnvelopeReverseR;
                set
                {
                    if (value.HasValue)
                    {
                        byte v = (byte)(value & 1);
                        if (f_EnvelopeReverseR != v)
                        {
                            f_EnvelopeReverseR = v;
                        }
                    }
                    else
                        f_EnvelopeReverseR = value;
                }
            }

            private byte? f_EnvelopeType;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Set Envelope Bits")]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? EnvelopeType
            {
                get => f_EnvelopeType;
                set
                {
                    if (value.HasValue)
                    {
                        byte v = (byte)(value & 7);
                        if (f_EnvelopeType != v)
                        {
                            f_EnvelopeType = v;
                        }
                    }
                    else
                        f_EnvelopeType = value;
                }
            }

            private byte? f_EnvelopeBits;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Set Envelope Bits")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? EnvelopeBits
            {
                get => f_EnvelopeBits;
                set
                {
                    if (value.HasValue)
                    {
                        byte v = (byte)(value & 1);
                        if (f_EnvelopeBits != v)
                        {
                            f_EnvelopeBits = v;
                        }
                    }
                    else
                        f_EnvelopeBits = value;
                }
            }

            private byte? f_EnvelopeClock;

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Set Envelope Clock")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? EnvelopeClock
            {
                get => f_EnvelopeClock;
                set
                {
                    if (value.HasValue)
                    {
                        byte v = (byte)(value & 1);
                        if (f_EnvelopeClock != v)
                        {
                            f_EnvelopeClock = v;
                        }
                    }
                    else
                        f_EnvelopeClock = value;
                }
            }

        }

    }
}