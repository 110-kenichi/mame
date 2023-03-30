// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.MusicTheory;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Gui.FMEditor;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using zanac.MAmidiMEmo.Scci;
using zanac.MAmidiMEmo.VSIF;
using static zanac.MAmidiMEmo.Instruments.Chips.RP2A03;

//https://www.plutiedev.com/ym2612-registers
//http://www.smspower.org/maxim/Documents/YM2612#regb4

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class YM2612 : InstrumentBase
    {

        public override string Name => "YM2612";

        public override string Group => "FM";

        public override InstrumentType InstrumentType => InstrumentType.YM2612;

        [Browsable(false)]
        public override string ImageKey => "YM2612";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ym2612_";

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
                return 2;
            }
        }

        private PortId portId = PortId.No1;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set FTDI or COM Port No for \"VSIF - Genesis\".\r\n" +
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

        private VsifClient vsifClient;

        private SoundEngineType f_SoundEngineType;

        private SoundEngineType f_CurrentSoundEngineType;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Select a sound engine type.\r\n" +
            "Supports \"Software\" and \"VSIF/SPFM\"")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeYM2612))]
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
            try
            {
                ignoreUpdatePcmData = true;
                AllSoundOff();
            }
            finally
            {
                ignoreUpdatePcmData = false;
            }

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
                    case SoundEngineType.VSIF_Genesis:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis, PortId, false);
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
                    case SoundEngineType.VSIF_Genesis_Low:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_Low, PortId, false);
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
                    case SoundEngineType.VSIF_Genesis_FTDI:
                        vsifClient = VsifManager.TryToConnectVSIF(VsifSoundModuleType.Genesis_FTDI, PortId, false);
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
                    case SoundEngineType.SPFM:
                        spfmPtr = ScciManager.TryGetSoundChip(SoundChipType.SC_TYPE_YM2612, (SC_CHIP_CLOCK)MasterClock);
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
                }
            }
            PrepareSound();
        }

        private int f_ftdiClkWidth = VsifManager.FTDI_BAUDRATE_GEN_CLK_WIDTH;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [SlideParametersAttribute(1, 100)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
            NTSC = 53693175 / 7,
            PAL = 53203424 / 7,
        }

        private uint f_MasterClock;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
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
            return MasterClock != (uint)MasterClockType.NTSC;
        }

        public void ResetMasterClock()
        {
            MasterClock = (uint)MasterClockType.NTSC;
        }

        private bool f_Mode5ch;

        [DataMember]
        [Category("Chip(Global)")]
        [Description("Use 5ch. And also enable DAC mode.\r\n" +
            "The DAC can only sound with real hardware. Particularly recommend FTDI.")]
        [DefaultValue(false)]
        public bool Mode5ch
        {
            get
            {
                return f_Mode5ch;
            }
            set
            {
                if (f_Mode5ch != value)
                {
                    f_Mode5ch = value;
                    Ym2612WriteData(UnitNumber, 0x2B, 0, 0, (byte)(f_Mode5ch ? 0x80 : 0x00));
                    if (value)
                        pcmEngine.StartEngine();
                    else
                        pcmEngine.StopEngine();
                }
            }
        }

        private byte f_Ch3Mode;

        /// <summary>
        /// LFRQ (0-255)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Channel 3 mode (0:Normal 1:SE mode 2:CSM)")]
        [SlideParametersAttribute(0, 2)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte Ch3Mode
        {
            get
            {
                return f_Ch3Mode;
            }
            set
            {
                byte v = (byte)(value & 3);
                if (f_Ch3Mode != v)
                {
                    f_Ch3ModeKeyOn = 0;
                    f_Ch3Mode = v;
                    Ym2612WriteData(UnitNumber, 0x27, 0, 0, (byte)(f_Ch3Mode << 6));
                }
            }
        }

        private byte f_Ch3ModeKeyOn;


        private byte f_LFOEN;

        /// <summary>
        /// LFRQ (0-255)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO Enable (0:Off 1:Enable)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFOEN
        {
            get
            {
                return f_LFOEN;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_LFOEN != v)
                {
                    f_LFOEN = v;
                    Ym2612WriteData(UnitNumber, 0x22, 0, 0, (byte)(LFOEN << 3 | LFRQ));
                }
            }
        }

        private byte f_LFRQ;

        /// <summary>
        /// LFRQ (0-7)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO Freq (0-7)\r\n" +
            "0:	3.82 Hz\r\n" +
            "1: 5.33 Hz\r\n" +
            "2: 5.77 Hz\r\n" +
            "3: 6.11 Hz\r\n" +
            "4: 6.60 Hz\r\n" +
            "5: 9.23 Hz\r\n" +
            "6: 46.11 Hz\r\n" +
            "7: 69.22 Hz\r\n")]
        [SlideParametersAttribute(0, 7)]
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
                byte v = (byte)(value & 7);
                if (f_LFRQ != v)
                {
                    f_LFRQ = v;
                    Ym2612WriteData(UnitNumber, 0x22, 0, 0, (byte)(LFOEN << 3 | LFRQ));
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

        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(YM2612UITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public YM2612Timbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<YM2612>(serializeData))
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
        private delegate void delegate_ym2612_write(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_ym2612_write Ym2612_write
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
            uint port1 = 1;
            if (address >= 0x100)
                port1 = 2;
            address = address & 0xff;

            WriteData(address, data, address != 0x28, new Action(() =>
            {
                lock (sndEnginePtrLock)
                {
                    switch (CurrentSoundEngine)
                    {
                        case SoundEngineType.VSIF_Genesis:
                        case SoundEngineType.VSIF_Genesis_Low:
                        case SoundEngineType.VSIF_Genesis_FTDI:
                            vsifClient.WriteData(0, (byte)(4 * port1), (byte)address, f_ftdiClkWidth);
                            vsifClient.WriteData(0, (byte)(8 * port1), (byte)data, f_ftdiClkWidth);
                            break;
                        case SoundEngineType.VSIF_MSX_FTDI:
                        case SoundEngineType.VSIF_P6_FTDI:
                            vsifClient.WriteData((byte)(0x10 + (port1 - 1)), (byte)address, (byte)data, f_ftdiClkWidth);
                            break;
                    }
                }
                DeferredWriteData(Ym2612_write, UnitNumber, (port1 - 1) * 2 + 0, address);
                DeferredWriteData(Ym2612_write, UnitNumber, (port1 - 1) * 2 + 1, data);
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitNumber"></param>
        /// <param name="address"></param>
        /// <param name="op"></param>
        /// <param name="slot"></param>
        /// <param name="data"></param>
        private void Ym2612WriteData(uint unitNumber, byte address, int op, int slot, byte data)
        {
            Ym2612WriteData(unitNumber, address, op, slot, data, true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Ym2612WriteData(uint unitNumber, byte address, int op, int slot, byte data, bool useCache)
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

            uint reg = (uint)(slot / 3) << 8;
            uint adrs = (uint)(reg + address + (op * 4) + (slot % 3));

            WriteData(adrs, data, useCache, new Action(() =>
            {
                uint yreg = (uint)(slot / 3) * 2;

                lock (sndEnginePtrLock)
                {
                    switch (CurrentSoundEngine)
                    {
                        case SoundEngineType.VSIF_Genesis:
                        case SoundEngineType.VSIF_Genesis_Low:
                        case SoundEngineType.VSIF_Genesis_FTDI:
                            vsifClient.WriteData(0, (byte)((1 + (yreg + 0)) * 4), (byte)(address + (op * 4) + (slot % 3)), f_ftdiClkWidth);
                            vsifClient.WriteData(0, (byte)((1 + (yreg + 1)) * 4), data, f_ftdiClkWidth);
                            break;
                        case SoundEngineType.VSIF_MSX_FTDI:
                        case SoundEngineType.VSIF_P6_FTDI:
                            vsifClient.WriteData((byte)(0x10 + (slot / 3)), (byte)(address + (op * 4) + (slot % 3)), data, f_ftdiClkWidth);
                            break;
                        case SoundEngineType.SPFM:
                            ScciManager.SetRegister(spfmPtr, adrs, data, false);
                            break;
                    }
                }
                DeferredWriteData(Ym2612_write, unitNumber, yreg + 0, (byte)(address + (op * 4) + (slot % 3)));
                DeferredWriteData(Ym2612_write, unitNumber, yreg + 1, data);
            }));
            //try
            //{
            //    Program.SoundUpdating();
            //    Ym2612_write(unitNumber, yreg + 0, (byte)(address + (op * 4) + (slot % 3)));
            //    Ym2612_write(unitNumber, yreg + 1, data);
            //}
            //finally
            //{
            //    Program.SoundUpdated();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adrs"></param>
        /// <param name="dt"></param>
        protected void DeferredWriteOPN2_DAC(uint unitNumber, byte[] dacData)
        {
            List<PortWriteData> list = new List<PortWriteData>();
            lock (sndEnginePtrLock)
            {
                switch (CurrentSoundEngine)
                {
                    case SoundEngineType.VSIF_Genesis:
                    case SoundEngineType.VSIF_Genesis_Low:
                    case SoundEngineType.VSIF_Genesis_FTDI:
                        foreach (var dd in dacData)
                        {
                            list.Add(new PortWriteData() { Type = 0, Address = 0x04, Data = 0x2a, Wait = f_ftdiClkWidth });
                            list.Add(new PortWriteData() { Type = 0, Address = 0x08, Data = dd, Wait = f_ftdiClkWidth });
                        }
                        vsifClient.WriteData(list.ToArray());
                        break;
                    case SoundEngineType.VSIF_MSX_FTDI:
                    case SoundEngineType.VSIF_P6_FTDI:
                        foreach (var dd in dacData)
                            list.Add(new PortWriteData() { Type = 0x14, Address = 0x2a, Data = dd, Wait = f_ftdiClkWidth });
                        vsifClient.WriteData(list.ToArray());
                        break;
                    case SoundEngineType.SPFM:
                        foreach (var dd in dacData)
                            ScciManager.SetRegister(spfmPtr, 0x2a, dd, false);
                        break;
                }
            }
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
        static YM2612()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ym2612_write");
            if (funcPtr != IntPtr.Zero)
            {
                Ym2612_write = (delegate_ym2612_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_ym2612_write));
            }
        }

        private YM2612SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM2612(uint unitNumber) : base(unitNumber)
        {
            MasterClock = (uint)MasterClockType.NTSC;

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new YM2612Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new YM2612Timbre();

            pcmEngine = new PcmEngine(this);

            setPresetInstruments();

            this.soundManager = new YM2612SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            pcmEngine?.Dispose();

            lock (sndEnginePtrLock)
            {
                if (spfmPtr != IntPtr.Zero)
                {
                    ScciManager.ReleaseSoundChip(spfmPtr);
                    spfmPtr = IntPtr.Zero;
                }
                if (vsifClient != null)
                    vsifClient.Dispose();
            }

            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            //Brass Section.dmp
            Timbres[0].FMS = 1;
            Timbres[0].AMS = 0;
            Timbres[0].FB = 7;
            Timbres[0].ALG = 3;

            Timbres[0].Ops[0].Enable = 1;
            Timbres[0].Ops[0].AR = 31;
            Timbres[0].Ops[0].D1R = 6;
            Timbres[0].Ops[0].SL = 15;
            Timbres[0].Ops[0].D2R = 0;
            Timbres[0].Ops[0].RR = 7;

            Timbres[0].Ops[0].MUL = 1;
            Timbres[0].Ops[0].RS = 0;
            Timbres[0].Ops[0].DT1 = 7;
            Timbres[0].Ops[0].AM = 0;
            Timbres[0].Ops[0].SSG_EG = 0;
            Timbres[0].Ops[0].TL = 20;

            Timbres[0].Ops[1].Enable = 1;
            Timbres[0].Ops[1].AR = 31;
            Timbres[0].Ops[1].D1R = 7;
            Timbres[0].Ops[1].SL = 4;
            Timbres[0].Ops[1].D2R = 0;
            Timbres[0].Ops[1].RR = 15;

            Timbres[0].Ops[1].MUL = 2;
            Timbres[0].Ops[1].RS = 0;
            Timbres[0].Ops[1].DT1 = 6;
            Timbres[0].Ops[1].AM = 0;
            Timbres[0].Ops[1].SSG_EG = 0;
            Timbres[0].Ops[1].TL = 21;

            Timbres[0].Ops[2].Enable = 1;
            Timbres[0].Ops[2].AR = 31;
            Timbres[0].Ops[2].D1R = 7;
            Timbres[0].Ops[2].SL = 4;
            Timbres[0].Ops[2].D2R = 0;
            Timbres[0].Ops[2].RR = 15;

            Timbres[0].Ops[2].MUL = 1;
            Timbres[0].Ops[2].RS = 0;
            Timbres[0].Ops[2].DT1 = 2;
            Timbres[0].Ops[2].AM = 0;
            Timbres[0].Ops[2].SSG_EG = 0;
            Timbres[0].Ops[2].TL = 12;

            Timbres[0].Ops[3].Enable = 1;
            Timbres[0].Ops[3].AR = 31;
            Timbres[0].Ops[3].D1R = 0;
            Timbres[0].Ops[3].SL = 0;
            Timbres[0].Ops[3].D2R = 0;
            Timbres[0].Ops[3].RR = 15;

            Timbres[0].Ops[3].MUL = 1;
            Timbres[0].Ops[3].RS = 0;
            Timbres[0].Ops[3].DT1 = 4;
            Timbres[0].Ops[3].AM = 0;
            Timbres[0].Ops[3].SSG_EG = 0;
            Timbres[0].Ops[3].TL = 12;

            //Additive Chimes A.dmp
            Timbres[2].FMS = 0;
            Timbres[2].AMS = 0;
            Timbres[2].FB = 0;
            Timbres[2].ALG = 7;

            Timbres[2].Ops[0].AR = 31;
            Timbres[2].Ops[0].D1R = 4;
            Timbres[2].Ops[0].SL = 15;
            Timbres[2].Ops[0].D2R = 0;
            Timbres[2].Ops[0].RR = 4;

            Timbres[2].Ops[0].MUL = 1;
            Timbres[2].Ops[0].RS = 0;
            Timbres[2].Ops[0].DT1 = 4;
            Timbres[2].Ops[0].AM = 0;
            Timbres[2].Ops[0].SSG_EG = 0;
            Timbres[2].Ops[0].TL = 20;

            Timbres[2].Ops[1].AR = 31;
            Timbres[2].Ops[1].D1R = 7;
            Timbres[2].Ops[1].SL = 15;
            Timbres[2].Ops[1].D2R = 0;
            Timbres[2].Ops[1].RR = 5;

            Timbres[2].Ops[1].MUL = 4;
            Timbres[2].Ops[1].RS = 0;
            Timbres[2].Ops[1].DT1 = 4;
            Timbres[2].Ops[1].AM = 0;
            Timbres[2].Ops[1].SSG_EG = 0;
            Timbres[2].Ops[1].TL = 20;

            Timbres[2].Ops[2].AR = 31;
            Timbres[2].Ops[2].D1R = 10;
            Timbres[2].Ops[2].SL = 15;
            Timbres[2].Ops[2].D2R = 0;
            Timbres[2].Ops[2].RR = 6;

            Timbres[2].Ops[2].MUL = 7;
            Timbres[2].Ops[2].RS = 0;
            Timbres[2].Ops[2].DT1 = 4;
            Timbres[2].Ops[2].AM = 0;
            Timbres[2].Ops[2].SSG_EG = 0;
            Timbres[2].Ops[2].TL = 20;

            Timbres[2].Ops[3].AR = 31;
            Timbres[2].Ops[3].D1R = 13;
            Timbres[2].Ops[3].SL = 15;
            Timbres[2].Ops[3].D2R = 0;
            Timbres[2].Ops[3].RR = 7;

            Timbres[2].Ops[3].MUL = 10;
            Timbres[2].Ops[3].RS = 0;
            Timbres[2].Ops[3].DT1 = 0;
            Timbres[2].Ops[3].AM = 0;
            Timbres[2].Ops[3].SSG_EG = 0;
            Timbres[2].Ops[3].TL = 20;

            //DX Piano1
            Timbres[1].FMS = 0;
            Timbres[1].AMS = 0;
            Timbres[1].FB = 0;
            Timbres[1].ALG = 1;

            Timbres[1].Ops[0].AR = 31;
            Timbres[1].Ops[0].D1R = 9;
            Timbres[1].Ops[0].SL = 15;
            Timbres[1].Ops[0].D2R = 0;
            Timbres[1].Ops[0].RR = 5;

            Timbres[1].Ops[0].MUL = 9;
            Timbres[1].Ops[0].RS = 2;
            Timbres[1].Ops[0].DT1 = 7;
            Timbres[1].Ops[0].AM = 0;
            Timbres[1].Ops[0].SSG_EG = 0;
            Timbres[1].Ops[0].TL = 60;

            Timbres[1].Ops[1].AR = 31;
            Timbres[1].Ops[1].D1R = 9;
            Timbres[1].Ops[1].SL = 15;
            Timbres[1].Ops[1].D2R = 0;
            Timbres[1].Ops[1].RR = 5;

            Timbres[1].Ops[1].MUL = 9;
            Timbres[1].Ops[1].RS = 2;
            Timbres[1].Ops[1].DT1 = 1;
            Timbres[1].Ops[1].AM = 0;
            Timbres[1].Ops[1].SSG_EG = 0;
            Timbres[1].Ops[1].TL = 60;

            Timbres[1].Ops[2].AR = 31;
            Timbres[1].Ops[2].D1R = 7;
            Timbres[1].Ops[2].SL = 15;
            Timbres[1].Ops[2].D2R = 0;
            Timbres[1].Ops[2].RR = 5;

            Timbres[1].Ops[2].MUL = 0;
            Timbres[1].Ops[2].RS = 2;
            Timbres[1].Ops[2].DT1 = 4;
            Timbres[1].Ops[2].AM = 0;
            Timbres[1].Ops[2].SSG_EG = 0;
            Timbres[1].Ops[2].TL = 28;

            Timbres[1].Ops[3].AR = 31;
            Timbres[1].Ops[3].D1R = 3;
            Timbres[1].Ops[3].SL = 15;
            Timbres[1].Ops[3].D2R = 0;
            Timbres[1].Ops[3].RR = 5;

            Timbres[1].Ops[3].MUL = 0;
            Timbres[1].Ops[3].RS = 2;
            Timbres[1].Ops[3].DT1 = 4;
            Timbres[1].Ops[3].AM = 0;
            Timbres[1].Ops[3].SSG_EG = 0;
            Timbres[1].Ops[3].TL = 10;
        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        private bool ignoreUpdatePcmData;

        /// <summary>
        /// 
        /// </summary>
        private void initGlobalRegisters()
        {
            Ym2612WriteData(UnitNumber, 0x22, 0, 0, (byte)(LFOEN << 3 | LFRQ));
            Ym2612WriteData(UnitNumber, 0x27, 0, 0, (byte)(Ch3Mode << 6));
            Ym2612WriteData(UnitNumber, 0x2B, 0, 0, (byte)(Mode5ch ? 0x80 : 0x00));
            f_Ch3ModeKeyOn = 0;
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
        private class YM2612SoundManager : SoundManagerBase
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

            private static SoundList<YM2612Sound> fmOnSounds = new SoundList<YM2612Sound>(6);

            private static SoundList<YM2612Sound> pcmOnSounds = new SoundList<YM2612Sound>(1);

            private YM2612 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM2612SoundManager(YM2612 parent) : base(parent)
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
                foreach (YM2612Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(timbre, note);
                    if (emptySlot.slot < 0)
                        continue;

                    YM2612Sound snd = new YM2612Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
                    switch (timbre.ToneType)
                    {
                        case ToneType.FM:
                            fmOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn FM ch" + emptySlot + " " + note.ToString());
                            break;
                        case ToneType.PCM:
                            pcmOnSounds.Add(snd);
                            FormMain.OutputDebugLog(parentModule, "KeyOn PCM ch" + emptySlot + " " + note.ToString());
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
            private (YM2612 inst, int slot) searchEmptySlot(YM2612Timbre timbre, TaggedNoteOnEvent note)
            {
                var emptySlot = (parentModule, -1);

                //return (parentModule, 1);
                switch (timbre.ToneType)
                {
                    case ToneType.FM:
                        {
                            if (timbre.SeMode)
                            {
                                if (parentModule.Ch3Mode == 1)
                                {
                                    var slot = timbre.AssignMIDIChtoSlotNum ? note.Channel + timbre.AssignMIDIChtoSlotNumOffset : -1;
                                    if (slot > 3)
                                        slot = -1;
                                    return SearchEmptySlotAndOffForLeader(parentModule, fmOnSounds, note, 4, slot, 0);
                                }
                                return (parentModule, -1);
                            }
                            else
                            {
                                var slot = timbre.AssignMIDIChtoSlotNum ? note.Channel + timbre.AssignMIDIChtoSlotNumOffset : -1;
                                if (slot > (parentModule.Mode5ch ? 5 : 6))
                                    slot = -1;
                                return SearchEmptySlotAndOffForLeader(parentModule, fmOnSounds, note, parentModule.Mode5ch ? 5 : 6, slot, 0);
                            }
                        }
                    case ToneType.PCM:
                        {
                            if (!parentModule.Mode5ch)
                                break;
                            return SearchEmptySlotAndOffForLeader(parentModule, pcmOnSounds, note, PcmEngine.MAX_VOICE);
                        }
                }

                return emptySlot;
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 6; i++)
                {
                    uint reg = (uint)(i / 3) * 2;
                    parentModule.Ym2612WriteData(parentModule.UnitNumber, 0x28, 0, 0, (byte)(0x00 | (reg << 1) | (byte)(i % 3)));

                    for (int op = 0; op < 4; op++)
                        parentModule.Ym2612WriteData(parentModule.UnitNumber, 0x40, op, i, 127);
                }

                for (int i = 0; i < PcmEngine.MAX_VOICE; i++)
                    parentModule.pcmEngine.Stop(i);
            }


        }

        private PcmEngine pcmEngine;


        /// <summary>
        /// 
        /// </summary>
        private class PcmEngine : IDisposable
        {
            /// <summary>
            /// 
            /// </summary>
            public const int MAX_VOICE = 4;

            private object engineLockObject;

            private object dataLockObject;

            private AutoResetEvent autoResetEvent;

            private bool stopEngineFlag;

            private bool disposedValue;


            private YM2612 ym2612;

            private uint unitNumber;

            private List<byte> deferredWriteData;

            private SampleData[] currentSampleData;

            /// <summary>
            /// 
            /// </summary>
            public PcmEngine(YM2612 ym2612)
            {
                this.ym2612 = ym2612;
                unitNumber = ym2612.UnitNumber;
                engineLockObject = new object();
                dataLockObject = new object();
                stopEngineFlag = true;
                deferredWriteData = new List<byte>();
                autoResetEvent = new AutoResetEvent(false);
                currentSampleData = new SampleData[MAX_VOICE];
            }


            /// <summary>
            /// 
            /// </summary>
            public void StartEngine()
            {
                if (stopEngineFlag)
                {
                    stopEngineFlag = false;
                    Thread t = new Thread(processDac);
                    t.Priority = ThreadPriority.AboveNormal;
                    t.Start();

                    t = new Thread(deferredWriteDataTask);
                    t.Priority = ThreadPriority.Highest;
                    t.Start();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void StopEngine()
            {
                stopEngineFlag = true;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pcmData"></param>
            public void Play(TaggedNoteOnEvent note, int slot, YM2612Timbre pcmTimbre)
            {
                lock (engineLockObject)
                {
                    currentSampleData[slot] = new SampleData(note, pcmTimbre.PcmData, pcmTimbre.SampleRate);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pcmData"></param>
            public void Stop(int slot)
            {
                lock (engineLockObject)
                {
                    currentSampleData[slot] = null;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pcmData"></param>
            public void StopAll()
            {
                lock (engineLockObject)
                {
                    for(int i=0; i<currentSampleData.Length; i++)
                        currentSampleData[i] = null;
                }
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool QueryPerformanceFrequency(out long frequency);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="address"></param>
            /// <param name="data"></param>
            private void deferredWriteDacData(byte data)
            {
                lock (dataLockObject)
                {
                    if (disposedValue)
                        return;
                    deferredWriteData.Add(data);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            private void deferredWriteDataTask()
            {
                try
                {
                    while (!stopEngineFlag)
                    {
                        if (disposedValue)
                            break;

                        byte[] dd;
                        lock (dataLockObject)
                        {
                            if (deferredWriteData.Count == 0)
                                continue;
                            dd = deferredWriteData.ToArray();
                            deferredWriteData.Clear();
                        }
                        if (dd.Length != 0)
                            ym2612.DeferredWriteOPN2_DAC(unitNumber, dd);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            private void processDac()
            {
                double wait = 0;
                double streamWaitDelta = 0;
                double lastWaitRemain = 0;
                double lastDiff = 0;
                int overflowed = 0;

                long freq, before, after;
                QueryPerformanceFrequency(out freq);

                QueryPerformanceCounter(out before);
                uint sampleRate = 0;

                while (!stopEngineFlag)
                {
                    if (disposedValue)
                        break;

                    QueryPerformanceCounter(out before);

                    if (streamWaitDelta <= 0)
                    {
                        int dacData = 0;
                        bool playDac = false;
                        lock (engineLockObject)
                        {
                            foreach (var sd in currentSampleData)
                            {
                                if (sd == null)
                                    continue;

                                var d = sd.GetDacData();
                                if (d == null)
                                    continue;

                                dacData += ((int)d.Value - 0x80) * sd.Note.Velocity / 127;
                                sampleRate = sd.SampleRate;
                                playDac = true;
                            }
                        }

                        if (playDac || overflowed != 0)
                        {
                            //dacData += overflowed;
                            overflowed = 0;
                            if (dacData > sbyte.MaxValue)
                            {
                                //overflowed = dacData - sbyte.MaxValue;
                                dacData = byte.MaxValue;
                            }
                            else if (dacData < sbyte.MinValue)
                            {
                                //overflowed = dacData - sbyte.MinValue;
                                dacData = byte.MinValue;
                            }
                            deferredWriteDacData((byte)(dacData + 0x80));

                            /* TODO:
                            try
                            {
                                Program.SoundUpdating();
                                Ym2612_write(unitNumber, (uint)0, (byte)0x2a);
                                Ym2612_write(unitNumber, (uint)1, (byte)dacData);
                            }
                            finally
                            {
                                Program.SoundUpdated();
                            }
                            */

                            streamWaitDelta += 44100d / sampleRate;
                        }
                    }

                    wait += streamWaitDelta;
                    streamWaitDelta = 0;

                    if (wait + lastWaitRemain <= 0)
                        continue;

                    lastWaitRemain = 0;

                    QueryPerformanceCounter(out after);
                    double pwait = wait + lastWaitRemain;
                    if (((double)(after - before) / freq) > (pwait / (44.1 * 1000)))
                    {
                        lastDiff = ((double)(after - before) / freq) - (pwait / (44.1 * 1000));
                        lastWaitRemain = -(lastDiff * 44.1 * 1000);
                        wait = 0;
                    }
                    else
                    {
                        while (((double)(after - before) / freq) <= (pwait / (44.1 * 1000)))
                            QueryPerformanceCounter(out after);
                        lastWaitRemain = 0;
                        wait = 0;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="disposing"></param>
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                        stopEngineFlag = true;

                        autoResetEvent?.Dispose();
                    }

                    // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                    // TODO: 大きなフィールドを null に設定します
                    disposedValue = true;
                }
            }

            // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
            // ~PcmEngine()
            // {
            //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            //     Dispose(disposing: false);
            // }

            public void Dispose()
            {
                // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class SampleData
        {
            public byte[] PcmData
            {
                get;
                private set;
            }

            public uint SampleRate
            {
                get;
                private set;
            }

            public TaggedNoteOnEvent Note
            {
                get;
                private set;
            }
             
            /// <summary>
            /// 
            /// </summary>
            /// <param name="adress"></param>
            /// <param name="size"></param>
            public SampleData(TaggedNoteOnEvent note, byte[] pcmData, uint sampleRate)
            {
                Note = note;
                PcmData = (byte[])pcmData.Clone();
                SampleRate = sampleRate;
            }

            private int index;

            public void Restart()
            {
                index = 0;
            }

            public byte? GetDacData()
            {
                if (index >= PcmData.Length)
                    return null;

                return PcmData[index++];
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class YM2612Sound : SoundBase
        {

            private YM2612 parentModule;

            private uint unitNumber;

            private YM2612Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM2612Sound(YM2612 parentModule, YM2612SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (YM2612Timbre)timbre;
                this.unitNumber = parentModule.UnitNumber;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                switch (timbre.ToneType)
                {
                    case ToneType.FM:
                        {
                            var gs = timbre.GlobalSettings;
                            if (gs.Enable)
                            {
                                if (gs.LFOEN.HasValue)
                                    parentModule.LFOEN = gs.LFOEN.Value;
                                if (gs.LFRQ.HasValue)
                                    parentModule.LFRQ = gs.LFRQ.Value;
                            }

                            //
                            SetFmTimbre();
                            //Freq
                            OnPitchUpdated();
                            //Volume
                            OnVolumeUpdated();
                            //On
                            if (timbre.SeMode)
                            {
                                byte op = (byte)(0x10 << Slot);
                                parentModule.f_Ch3ModeKeyOn |= op;
                                parentModule.Ym2612WriteData(unitNumber, 0x28, 0, 0, (byte)(parentModule.f_Ch3ModeKeyOn | (byte)(2 % 3)));
                            }
                            else
                            {
                                uint reg = (uint)(Slot / 3) * 2;
                                byte op = (byte)(timbre.Ops[0].Enable << 4 | timbre.Ops[1].Enable << 5 | timbre.Ops[2].Enable << 6 | timbre.Ops[3].Enable << 7);
                                parentModule.Ym2612WriteData(unitNumber, 0x28, 0, 0, (byte)(op | (reg << 1) | (byte)(Slot % 3)));
                            }
                            break;
                        }
                    case ToneType.PCM:
                        {
                            if (!parentModule.Mode5ch)
                                break;
                            parentModule.pcmEngine.Play(NoteOnEvent, Slot, timbre);
                            break;
                        }
                }
            }


            public override void OnSoundParamsUpdated()
            {
                switch (timbre.ToneType)
                {
                    case ToneType.FM:
                        {

                            var gs = timbre.GlobalSettings;
                            if (gs.Enable)
                            {
                                if (gs.LFOEN.HasValue)
                                    parentModule.LFOEN = gs.LFOEN.Value;
                                if (gs.LFRQ.HasValue)
                                    parentModule.LFRQ = gs.LFRQ.Value;
                            }
                            if (timbre.SeMode)
                            {
                                int op = Slot;
                                {
                                    //$30+: multiply and detune
                                    parentModule.Ym2612WriteData(unitNumber, 0x30, op, 2, (byte)((timbre.Ops[0].DT1 << 4 | timbre.Ops[0].MUL)));
                                    //$40+: total level
                                    switch (timbre.ALG)
                                    {
                                        case 0:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, 2, (byte)timbre.Ops[0].TL);
                                            break;
                                        case 1:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, 2, (byte)timbre.Ops[0].TL);
                                            break;
                                        case 2:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, 2, (byte)timbre.Ops[0].TL);
                                            break;
                                        case 3:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, 2, (byte)timbre.Ops[0].TL);
                                            break;
                                        case 4:
                                            if (op != 1 && op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, 2, (byte)timbre.Ops[0].TL);
                                            break;
                                        case 5:
                                            if (op == 4)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, 2, (byte)timbre.Ops[0].TL);
                                            break;
                                        case 6:
                                            if (op == 4)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, 2, (byte)timbre.Ops[0].TL);
                                            break;
                                        case 7:
                                            break;
                                    }
                                    //$50+: attack rate and rate scaling
                                    parentModule.Ym2612WriteData(unitNumber, 0x50, op, 2, (byte)((timbre.Ops[0].RS << 6 | timbre.Ops[0].AR)));
                                    //$60+: 1st decay rate and AM enable
                                    parentModule.Ym2612WriteData(unitNumber, 0x60, op, 2, (byte)((timbre.Ops[0].AM << 7 | timbre.Ops[0].D1R)));
                                    //$70+: 2nd decay rate
                                    parentModule.Ym2612WriteData(unitNumber, 0x70, op, 2, (byte)timbre.Ops[0].D2R);
                                    //$80+: release rate and sustain level
                                    parentModule.Ym2612WriteData(unitNumber, 0x80, op, 2, (byte)((timbre.Ops[0].SL << 4 | timbre.Ops[0].RR)));
                                    //$90+: SSG-EG
                                    parentModule.Ym2612WriteData(unitNumber, 0x90, op, 2, (byte)timbre.Ops[0].SSG_EG);
                                }
                            }
                            else
                            {
                                for (int op = 0; op < 4; op++)
                                {
                                    //$30+: multiply and detune
                                    parentModule.Ym2612WriteData(unitNumber, 0x30, op, Slot, (byte)((timbre.Ops[op].DT1 << 4 | timbre.Ops[op].MUL)));
                                    //$40+: total level
                                    switch (timbre.ALG)
                                    {
                                        case 0:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 1:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 2:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 3:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 4:
                                            if (op != 1 && op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 5:
                                            if (op == 4)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 6:
                                            if (op == 4)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 7:
                                            break;
                                    }
                                    //$50+: attack rate and rate scaling
                                    parentModule.Ym2612WriteData(unitNumber, 0x50, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].AR)));
                                    //$60+: 1st decay rate and AM enable
                                    parentModule.Ym2612WriteData(unitNumber, 0x60, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].D1R)));
                                    //$70+: 2nd decay rate
                                    parentModule.Ym2612WriteData(unitNumber, 0x70, op, Slot, (byte)timbre.Ops[op].D2R);
                                    //$80+: release rate and sustain level
                                    parentModule.Ym2612WriteData(unitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].SL << 4 | timbre.Ops[op].RR)));
                                    //$90+: SSG-EG
                                    parentModule.Ym2612WriteData(unitNumber, 0x90, op, Slot, (byte)timbre.Ops[op].SSG_EG);
                                }
                            }

                            //$B0+: algorithm and feedback
                            parentModule.Ym2612WriteData(unitNumber, 0xB0, 0, Slot, (byte)(timbre.FB << 3 | timbre.ALG));

                            //On
                            if (!IsKeyOff)
                            {
                                if (!timbre.SeMode)
                                {
                                    uint reg = (uint)(Slot / 3) * 2;
                                    byte open = (byte)(timbre.Ops[0].Enable << 4 | timbre.Ops[1].Enable << 5 | timbre.Ops[2].Enable << 6 | timbre.Ops[3].Enable << 7);
                                    parentModule.Ym2612WriteData(unitNumber, 0x28, 0, 0, (byte)(open | (reg << 1) | (byte)(Slot % 3)));
                                }
                            }
                            break;
                        }
                    case ToneType.PCM:
                        {
                            break;
                        }
                }


                base.OnSoundParamsUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                switch (timbre.ToneType)
                {
                    case ToneType.FM:
                        {
                            List<int> ops = new List<int>();
                            switch (timbre.ALG)
                            {
                                case 0:
                                    ops.Add(3);
                                    break;
                                case 1:
                                    ops.Add(3);
                                    break;
                                case 2:
                                    ops.Add(3);
                                    break;
                                case 3:
                                    ops.Add(3);
                                    break;
                                case 4:
                                    ops.Add(1);
                                    ops.Add(3);
                                    break;
                                case 5:
                                    ops.Add(1);
                                    ops.Add(2);
                                    ops.Add(3);
                                    break;
                                case 6:
                                    ops.Add(1);
                                    ops.Add(2);
                                    ops.Add(3);
                                    break;
                                case 7:
                                    ops.Add(0);
                                    ops.Add(1);
                                    ops.Add(2);
                                    ops.Add(3);
                                    break;
                            }
                            var v = CalcCurrentVolume();
                            int velo = 1 + timbre.MDS.VelocitySensitivity;
                            if (timbre.SeMode)
                            {
                                foreach (int op in ops)
                                {
                                    if (op == Slot)
                                    {
                                        //$40+: total level
                                        parentModule.Ym2612WriteData(unitNumber, 0x40, op, 2, (byte)((127 / velo) - Math.Round(((127 / velo) - (timbre.Ops[op].TL / velo)) * v)));
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (int op in ops)
                                {
                                    //$40+: total level
                                    parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)((127 / velo) - Math.Round(((127 / velo) - (timbre.Ops[op].TL / velo)) * v)));
                                }
                            }

                            break;
                        }
                    case ToneType.PCM:
                        {
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
                switch (timbre.ToneType)
                {
                    case ToneType.FM:
                        {
                            double d = CalcCurrentPitchDeltaNoteNumber();

                            int nn = NoteOnEvent.NoteNumber;
                            if (ParentModule.ChannelTypes[NoteOnEvent.Channel] == ChannelType.Drum)
                                nn = (int)ParentModule.DrumTimbres[NoteOnEvent.NoteNumber].BaseNote;
                            int noteNum = nn + (int)d;
                            if (noteNum > 127)
                                noteNum = 127;
                            else if (noteNum < 0)
                                noteNum = 0;
                            var nnOn = new TaggedNoteOnEvent((SevenBitNumber)noteNum, (SevenBitNumber)127);
                            var freq = convertFmFrequency(nnOn, 0);
                            var octave = nnOn.GetNoteOctave();

                            if (d != 0)
                                freq += (convertFmFrequency(nnOn, (d < 0) ? -1 : +1) - freq) * Math.Abs(d - Math.Truncate(d));

                            if (octave < 0)
                            {
                                freq /= 2 * -octave;
                                octave = 0;
                            }
                            else if (octave > 7)
                            {
                                freq *= 2 * (octave - 7);
                                if (freq > 0x7ff)
                                    freq = 0x7ff;
                                octave = 7;
                            }

                            ushort dfreq = (ushort)Math.Round(freq);
                            octave = octave << 3;
                            if (timbre.SeMode)
                            {
                                switch (Slot)
                                {
                                    case 0:
                                        parentModule.Ym2612WriteData(unitNumber, 0xad, 0, 0, (byte)(octave | ((dfreq >> 8) & 7)), false);
                                        parentModule.Ym2612WriteData(unitNumber, 0xa9, 0, 0, (byte)(0xff & dfreq), false);
                                        break;
                                    case 1:
                                        parentModule.Ym2612WriteData(unitNumber, 0xae, 0, 0, (byte)(octave | ((dfreq >> 8) & 7)), false);
                                        parentModule.Ym2612WriteData(unitNumber, 0xaa, 0, 0, (byte)(0xff & dfreq), false);
                                        break;
                                    case 2:
                                        parentModule.Ym2612WriteData(unitNumber, 0xac, 0, 0, (byte)(octave | ((dfreq >> 8) & 7)), false);
                                        parentModule.Ym2612WriteData(unitNumber, 0xa8, 0, 0, (byte)(0xff & dfreq), false);
                                        break;
                                    case 3:
                                        parentModule.Ym2612WriteData(unitNumber, 0xa6, 0, 0, (byte)(octave | ((dfreq >> 8) & 7)), false);
                                        parentModule.Ym2612WriteData(unitNumber, 0xa2, 0, 0, (byte)(0xff & dfreq), false);
                                        break;
                                }
                            }
                            else
                            {
                                parentModule.Ym2612WriteData(unitNumber, 0xa4, 0, Slot, (byte)(octave | ((dfreq >> 8) & 7)), false);
                                parentModule.Ym2612WriteData(unitNumber, 0xa0, 0, Slot, (byte)(0xff & dfreq), false);
                            }
                            break;
                        }
                    case ToneType.PCM:
                        {
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
                //$B4+: panning, FMS, AMS
                byte pan = CalcCurrentPanpot();
                if (pan < 32)
                    pan = 0x2;
                else if (pan > 96)
                    pan = 0x1;
                else
                    pan = 0x3;
                switch (timbre.ToneType)
                {
                    case ToneType.FM:
                        {
                            if (timbre.SeMode)
                            {
                                parentModule.Ym2612WriteData(unitNumber, 0xB4, 0, 2, (byte)(pan << 6 | (timbre.AMS << 4) | timbre.FMS));
                            }
                            else
                            {
                                parentModule.Ym2612WriteData(unitNumber, 0xB4, 0, Slot, (byte)(pan << 6 | (timbre.AMS << 4) | timbre.FMS));
                            }
                            break;
                        }
                    case ToneType.PCM:
                        {
                            if (!parentModule.Mode5ch)
                                break;

                            parentModule.Ym2612WriteData(unitNumber, 0xB4, 0, 5, (byte)(pan << 6 | (timbre.AMS << 4) | timbre.FMS));
                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetFmTimbre()
            {
                switch (timbre.ToneType)
                {
                    case ToneType.FM:
                        {
                            if (timbre.SeMode)
                            {
                                int op = Slot;
                                {
                                    //$30+: multiply and detune
                                    parentModule.Ym2612WriteData(unitNumber, 0x30, op, 2, (byte)((timbre.Ops[0].DT1 << 4 | timbre.Ops[0].MUL)));
                                    //$40+: total level
                                    switch (timbre.ALG)
                                    {
                                        case 0:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 1:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 2:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 3:
                                            if (op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 4:
                                            if (op != 1 && op != 3)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 5:
                                            if (op == 0)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 6:
                                            if (op == 0)
                                                parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                            break;
                                        case 7:
                                            break;
                                    }
                                    //$50+: attack rate and rate scaling
                                    parentModule.Ym2612WriteData(unitNumber, 0x50, op, 2, (byte)((timbre.Ops[0].RS << 6 | timbre.Ops[0].AR)));
                                    //$60+: 1st decay rate and AM enable
                                    parentModule.Ym2612WriteData(unitNumber, 0x60, op, 2, (byte)((timbre.Ops[0].AM << 7 | timbre.Ops[0].D1R)));
                                    //$70+: 2nd decay rate
                                    parentModule.Ym2612WriteData(unitNumber, 0x70, op, 2, (byte)timbre.Ops[0].D2R);
                                    //$80+: release rate and sustain level
                                    parentModule.Ym2612WriteData(unitNumber, 0x80, op, 2, (byte)((timbre.Ops[0].SL << 4 | timbre.Ops[0].RR)));
                                    //$90+: SSG-EG
                                    parentModule.Ym2612WriteData(unitNumber, 0x90, op, 2, (byte)timbre.Ops[0].SSG_EG);
                                }

                                //$B0+: algorithm and feedback
                                parentModule.Ym2612WriteData(unitNumber, 0xB0, 0, 2, (byte)(timbre.FB << 3 | timbre.ALG));
                            }
                            else
                            {
                                for (int op = 0; op < 4; op++)
                                {
                                    //$30+: multiply and detune
                                    parentModule.Ym2612WriteData(unitNumber, 0x30, op, Slot, (byte)((timbre.Ops[op].DT1 << 4 | timbre.Ops[op].MUL)));
                                    //$40+: total level
                                    parentModule.Ym2612WriteData(unitNumber, 0x40, op, Slot, (byte)timbre.Ops[op].TL);
                                    //$50+: attack rate and rate scaling
                                    parentModule.Ym2612WriteData(unitNumber, 0x50, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].AR)));
                                    //$60+: 1st decay rate and AM enable
                                    parentModule.Ym2612WriteData(unitNumber, 0x60, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].D1R)));
                                    //$70+: 2nd decay rate
                                    parentModule.Ym2612WriteData(unitNumber, 0x70, op, Slot, (byte)timbre.Ops[op].D2R);
                                    //$80+: release rate and sustain level
                                    parentModule.Ym2612WriteData(unitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].SL << 4 | timbre.Ops[op].RR)));
                                    //$90+: SSG-EG
                                    parentModule.Ym2612WriteData(unitNumber, 0x90, op, Slot, (byte)timbre.Ops[op].SSG_EG);
                                }

                                //$B0+: algorithm and feedback
                                parentModule.Ym2612WriteData(unitNumber, 0xB0, 0, Slot, (byte)(timbre.FB << 3 | timbre.ALG));
                            }
                            break;
                        }
                    case ToneType.PCM:
                        {
                            break;
                        }
                }
                OnPanpotUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                switch (timbre.ToneType)
                {
                    case ToneType.FM:
                        {
                            if (timbre.SeMode)
                            {
                                int reg = 0x10 << Slot;
                                parentModule.f_Ch3ModeKeyOn = (byte)(~reg & parentModule.f_Ch3ModeKeyOn);
                                parentModule.Ym2612WriteData(unitNumber, 0x28, 0, 0, (byte)(parentModule.f_Ch3ModeKeyOn | (byte)(2 % 3)));
                            }
                            else
                            {
                                uint reg = (uint)(Slot / 3) * 2;
                                parentModule.Ym2612WriteData(unitNumber, 0x28, 0, 0, (byte)(0x00 | (reg << 1) | (byte)(Slot % 3)));
                            }
                            break;
                        }
                    case ToneType.PCM:
                        {
                            parentModule.pcmEngine.Stop(Slot);
                            break;
                        }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private double convertFmFrequency(TaggedNoteOnEvent note, int deltaNoteNum)
            {
                int nn = note.NoteNumber + deltaNoteNum;
                int oct = note.GetNoteOctave();

                var freq = MidiManager.CalcCurrentFrequency(nn);

                //https://github.com/jotego/jt12/blob/master/doc/YM2608J.PDF
                var rv = (144 * freq * Math.Pow(2, 20) / parentModule.MasterClock) / Math.Pow(2, oct - 1);

                return rv;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2612Timbre>))]
        [DataContract]
        [InstLock]
        public class YM2612Timbre : TimbreBase
        {
            [DataMember]
            [Category("Sound")]
            [Description("Sound Type")]
            [DefaultValue(ToneType.FM)]
            public ToneType ToneType
            {
                get;
                set;
            }

            #region FM Synth

            [Category("Sound")]
            [Editor(typeof(YM2612UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [IgnoreDataMember]
            [JsonIgnore]
            [DisplayName("(Detailed) - Open FM register editor")]
            [Description("Open FM register editor.")]
            [TypeConverter(typeof(EmptyTypeConverter))]
            public string Detailed
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(ALG),
                        nameof(FB),
                        nameof(AMS),
                        nameof(FMS),

                        "GlobalSettings.EN",
                        "GlobalSettings.LFOEN",
                        "GlobalSettings.LFRQ",

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
                        "Ops[0].SSG_EG",

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
                        "Ops[1].SSG_EG",

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
                        "Ops[2].SSG_EG",

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
                        "Ops[3].SSG_EG");
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),
                        nameof(AMS),
                        nameof(FMS),

                        "GlobalSettings.EN",
                        "GlobalSettings.LFOEN",
                        "GlobalSettings.LFRQ",

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
                        "Ops[0].SSG_EG",

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
                        "Ops[1].SSG_EG",

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
                        "Ops[2].SSG_EG",

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
                        "Ops[3].SSG_EG");
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

            private byte f_FMS;

            [DataMember]
            [Category("Sound")]
            [Description("Frequency Modulation Sensitivity (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte FMS
            {
                get
                {
                    return f_FMS;
                }
                set
                {
                    f_FMS = (byte)(value & 7);
                }
            }

            [DataMember]
            [Category("Chip(Global)")]
            [Description("For SE mode timbre. Enabled only Ch3Mode = 1 and Ops[0]")]
            [DefaultValue(false)]
            public bool SeMode
            {
                get;
                set;
            }

            private byte f_SeModeOperator;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [DefaultValue((byte)0)]
            [Description("Set SE mode operator (0:Free 1～7:Force set operator bit)")]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte SeModeOperator
            {
                get
                {
                    return f_SeModeOperator;
                }
                set
                {
                    f_SeModeOperator = (byte)(value & 7);
                }
            }


            #endregion

            #region Ops

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators")]
            [TypeConverter(typeof(ExpandableCollectionConverter))]
            [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
            [DisplayName("Operators[Ops]")]
            public YM2612Operator[] Ops
            {
                get;
                set;
            }

            public virtual bool ShouldSerializeOps()
            {
                foreach (var op in Ops)
                {
                    if (!string.Equals(JsonConvert.SerializeObject(op, Formatting.Indented), "{}"))
                        return true;
                }
                return false;
            }

            public virtual void ResetOps()
            {
                var ops = new YM2612Operator[] {
                    new YM2612Operator(),
                    new YM2612Operator(),
                    new YM2612Operator(),
                    new YM2612Operator() };
                for (int i = 0; i < Ops.Length; i++)
                    Ops[i].InjectFrom(new LoopInjection(), ops[i]);
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Category("Sound")]
            [Description("You can copy and paste this text data to other same type timber.\r\n" +
                "ALG, FB, AR, D1R(DR), D2R(SR), RR, SL, TL, RS(KS), MUL, DT1, AM(AMS), SSG_EG, ... , AMS, FMS\r\n" +
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
                        "Ops[0].SSG_EG",

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
                        "Ops[1].SSG_EG",

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
                        "Ops[2].SSG_EG",

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
                        "Ops[3].SSG_EG",

                        nameof(AMS),
                        nameof(FMS));
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
                        "Ops[0].SSG_EG",

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
                        "Ops[1].SSG_EG",

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
                        "Ops[2].SSG_EG",

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
                        "Ops[3].SSG_EG",

                        nameof(AMS),
                        nameof(FMS));
                }
            }

            #endregion


            #region PCM

            private byte[] f_PcmData = new byte[0];

            [TypeConverter(typeof(TypeConverter))]
            [Editor(typeof(PcmFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Unigned 8bit PCM Raw Data or WAV Data. (1ch)")]
            [PcmFileLoaderEditor("Audio File(*.raw, *.wav)|*.raw;*.wav", 0, 8, 1, 0)]
            public byte[] PcmData
            {
                get
                {
                    return f_PcmData;
                }
                set
                {
                    f_PcmData = value;
                }
            }

            public bool ShouldSerializePcmData()
            {
                return PcmData.Length != 0;
            }

            public void ResetPcmData()
            {
                PcmData = new byte[0];
            }

            /*
            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Set PCM base frequency [Hz]")]
            [DefaultValue(typeof(double), "440")]
            [DoubleSlideParametersAttribute(100, 2000, 1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double BaseFreqency
            {
                get;
                set;
            } = 440;
            */

            [DataMember]
            [Category("Sound(PCM)")]
            [Description("Set PCM samplerate [Hz]")]
            [DefaultValue(typeof(uint), "14000")]
            [SlideParametersAttribute(4000, 14000)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint SampleRate
            {
                get;
                set;
            } = 14000;

            //[DataMember]
            //[Category("Sound(PCM)")]
            //[Description("Set loop point (0 - 65535")]
            //[DefaultValue(typeof(ushort), "0")]
            //[SlideParametersAttribute(0, 65535)]
            //[EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            //public ushort LoopPoint
            //{
            //    get;
            //    set;
            //}

            //private bool f_LoopEnable;

            //[DataMember]
            //[Category("Sound(PCM)")]
            //[Description("Loop point enable")]
            //[SlideParametersAttribute(0, 1)]
            //[EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            //[DefaultValue(false)]
            //public bool LoopEnable
            //{
            //    get
            //    {
            //        return f_LoopEnable;
            //    }
            //    set
            //    {
            //        f_LoopEnable = value;
            //    }
            //}

            #endregion


            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public YM2612GlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new YM2612GlobalSettings());
            }

            /// <summary>
            /// 
            /// </summary>
            public YM2612Timbre()
            {
                Ops = new YM2612Operator[] {
                    new YM2612Operator(),
                    new YM2612Operator(),
                    new YM2612Operator(),
                    new YM2612Operator() };
                GlobalSettings = new YM2612GlobalSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM2612Timbre>(serializeData);
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
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2612Operator>))]
        [DataContract]
        [InstLock]
        public class YM2612Operator : ContextBoundObject
        {
            private byte f_Enable = 1;

            /// <summary>
            /// Enable(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Whether this operator enable or not")]
            [DefaultValue((byte)1)]
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
            [Category("Sound(FM)")]
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
            /// decay rate(0-31)
            /// </summary>
            [DataMember]
            [Category("Sound(FM)")]
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
            [Category("Sound(FM)")]
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
            [Category("Sound(FM)")]
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
            [Category("Sound(FM)")]
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
            [Category("Sound(FM)")]
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
            [Category("Sound(FM)")]
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
            [Category("Sound(FM)")]
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
            [Category("Sound(FM)")]
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
            /// amplitude modulation sensivity(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound(FM)")]
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

            private byte f_SSG_EG;

            /// <summary>
            /// SSG-EG(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound(FM)")]
            [Description("SSG-EG (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte SSG_EG
            {
                get
                {
                    return f_SSG_EG;
                }
                set
                {
                    f_SSG_EG = (byte)(value & 15);
                }
            }

            [IgnoreDataMember]
            [JsonIgnore]
            [Browsable(false)]
            public byte SSG
            {
                get
                {
                    return SSG_EG;
                }
                set
                {
                    SSG_EG = value;
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
                "AR, D1R(DR), D2R(SR), RR, SL, TL, RS(KS), MUL, DT1, AM(AMS), SSG_EG\r\n" +
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
                        nameof(SSG_EG));
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
                        nameof(SSG_EG));
                }
            }

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
                    var obj = JsonConvert.DeserializeObject<YM2612Operator>(serializeData);
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

            #endregion

        }


        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2612GlobalSettings>))]
        [DataContract]
        [InstLock]
        public class YM2612GlobalSettings : ContextBoundObject
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

            private byte? f_LFOEN;

            /// <summary>
            /// LFRQ (0-255)
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("LFO Enable (0:Off 1:Enable)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFOEN
            {
                get
                {
                    return f_LFOEN;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_LFOEN = v;
                }
            }

            private byte? f_LFRQ;

            /// <summary>
            /// LFRQ (0-7)
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("LFO Freq (0-7)\r\n" +
                "0:	3.82 Hz\r\n" +
                "1: 5.33 Hz\r\n" +
                "2: 5.77 Hz\r\n" +
                "3: 6.11 Hz\r\n" +
                "4: 6.60 Hz\r\n" +
                "5: 9.23 Hz\r\n" +
                "6: 46.11 Hz\r\n" +
                "7: 69.22 Hz\r\n")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFRQ
            {
                get
                {
                    return f_LFRQ;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 7);
                    f_LFRQ = v;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public enum ToneType
        {
            FM,
            PCM,
        }

        private class EnumConverterSoundEngineTypeYM2612 : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.VSIF_Genesis,
                    SoundEngineType.VSIF_Genesis_Low,
                    SoundEngineType.VSIF_Genesis_FTDI,
                    SoundEngineType.VSIF_MSX_FTDI,
                    SoundEngineType.VSIF_P6_FTDI,
                    SoundEngineType.SPFM,
                });

                return sc;
            }
        }
    }
}