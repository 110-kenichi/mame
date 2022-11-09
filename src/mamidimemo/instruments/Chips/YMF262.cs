// copyright-holders:K.Ito
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
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
using zanac.MAmidiMEmo.Gui.FMEditor;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Mame;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.VSIF;
using zanac.MAmidiMEmo.Util;
using System.Threading;

//http://map.grauw.nl/resources/sound/yamaha_ymf262.pdf
//http://guu.fmp.jp/archives/93#gallery-6
//https://www.alsa-project.org/files/pub/manuals/yamaha/sax-rege.pdf

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [InstLock]
    [DataContract]
    public class YMF262 : InstrumentBase
    {

        public override string Name => "YMF262";

        public override string Group => "FM";

        public override InstrumentType InstrumentType => InstrumentType.YMF262;

        [Browsable(false)]
        public override string ImageKey => "YMF262";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ymf262_";

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
                return 22;
            }
        }


        private PortId portId = PortId.No1;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Set Port No for \"VSIF - MSX\".\r\n" +
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

        private IntPtr opl3PortHandle;

        private object sndEnginePtrLock = new object();

        private SoundEngineType f_SoundEngineType;

        private SoundEngineType f_CurrentSoundEngineType;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Select a sound engine type.\r\n" +
            "Supports Software and VSIF - MSX and CMI8738(x64).")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeYMF262))]
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
                    value == SoundEngineType.VSIF_MSX_FTDI ||
                    value == SoundEngineType.Real_OPL3
                    ))
                {
                    setSoundEngine(value);
                }
            }
        }

        private class EnumConverterSoundEngineTypeYMF262 : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                if (Environment.Is64BitProcess)
                {
                    var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.VSIF_MSX_FTDI,
                    SoundEngineType.Real_OPL3,
                    });
                    return sc;
                }
                else
                {
                    var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.VSIF_MSX_FTDI,
                    });
                    return sc;
                }
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
                if (opl3PortHandle != IntPtr.Zero)
                {
                    CloseOPL3Port(opl3PortHandle);
                    opl3PortHandle = IntPtr.Zero;
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
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
                        }
                        else
                        {
                            f_CurrentSoundEngineType = SoundEngineType.Software;
                            SetDevicePassThru(false);
                        }
                        break;
                    case SoundEngineType.Real_OPL3:
                        IntPtr handle = OpenOPL3Port(CMI8738Index);
                        if (handle != IntPtr.Zero)
                        {
                            opl3PortHandle = handle;
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

        private int f_ftdiClkWidth = 15;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [SlideParametersAttribute(1, 100)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue(15)]
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

        private int f_CMI8738Index;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [SlideParametersAttribute(0, 8)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue(0)]
        [Description("Set CMI8738 PCIe card index number.")]
        public int CMI8738Index
        {
            get
            {
                return f_CMI8738Index;
            }
            set
            {
                f_CMI8738Index = value;
            }
        }

        private byte f_CONSEL;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Max number of 4 op mode sound")]
        [SlideParametersAttribute(0, 6)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte CONSEL
        {
            get
            {
                return f_CONSEL;
            }
            set
            {
                if (value <= 6 && f_CONSEL != value)
                {
                    f_CONSEL = value;
                    var v = 0;
                    for (int i = 0; i < f_CONSEL; i++)
                        v |= 1 << i;
                    YMF262WriteData(UnitNumber, 0x104, 0, 0, 0, 0, (byte)v);
                }
            }
        }

        private byte f_AMD;

        /// <summary>
        /// AM Depth (0-1)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("AM depth (0:1dB 1:4.8dB)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte AMD
        {
            get
            {
                return f_AMD;
            }
            set
            {
                var v = (byte)(value & 1);
                if (f_AMD != v)
                {
                    f_AMD = v;
                    YMF262WriteData(UnitNumber, 0xBD, 0, 0, 0, 0, (byte)(AMD << 7 | VIB << 6));
                }
            }
        }

        private byte f_VIB;

        /// <summary>
        /// Vibrato depth (0:7 cent 1:14 cent)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Vibrato depth (0:7 cent 1:14 cent)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte VIB
        {
            get
            {
                return f_VIB;
            }
            set
            {
                var v = (byte)(value & 1);
                if (f_VIB != v)
                {
                    f_VIB = v;
                    YMF262WriteData(UnitNumber, 0xBD, 0, 0, 0, 0, (byte)(AMD << 7 | VIB << 6));
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
        [EditorAttribute(typeof(YMF262UITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public YMF262Timbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<YMF262>(serializeData))
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
        private delegate void delegate_YMF262_write(uint unitNumber, uint address, byte data);

        /// <summary>
        /// 
        /// </summary>
        private static delegate_YMF262_write YMF262_write
        {
            get;
            set;
        }

        private static byte[] chAddressOffset = new byte[] { 0x00, 0x01, 0x02, 0x08, 0x09, 0x0a, 0x10, 0x11, 0x12 };

        private static byte[,] op2chConselTable = new byte[,] {
            { 00, 01, 02, 03, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16 ,17 },
            { 01, 02, 04, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16 ,17 ,00 ,00 },
            { 02, 05, 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16 ,17 ,00, 00, 00, 00 },
            { 06, 07, 08, 09, 10, 11, 12, 13, 14, 15, 16 ,17 ,00, 00, 00, 00, 00, 00 },
            { 06, 07, 08, 10, 11, 13, 14, 15, 16 ,17 ,00, 00, 00, 00, 00, 00, 00, 00 },
            { 06, 07, 08, 11, 14, 15, 16 ,17 ,00, 00, 00, 00, 00, 00, 00, 00, 00, 00 },
            { 06, 07, 08, 15, 16 ,17 ,00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00 },
        };

        private void YMF262WriteData(uint unitNumber, uint address, int op, int slot, byte opmode, int consel, byte data)
        {
            YMF262WriteData(unitNumber, address, op, slot, opmode, consel, data, true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void YMF262WriteData(uint unitNumber, uint address, int op, int slot, byte opmode, int consel, byte data, bool useCache)
        {
            //useCache = false;
            var adrL = address & 0xff;
            var adrH = (address & 0x100) >> 7;  // 0 or 2
            switch (opmode)
            {
                //Channel        0   1   2   3   4   5   6   7   8
                //Operator 1    00  01  02  06  07  09  0C  0D  0E
                //Operator 2    03  04  05  09  0A  0B  0F  10  11
                case 0:
                case 1:
                    slot = op2chConselTable[consel, slot];
                    if (slot >= 9)
                    {
                        adrH = 2;
                        slot = slot - 9;
                    }
                    break;
                //Channel        0   1   2    
                //Operator 1    00  01  02  
                //Operator 2    03  04  05  
                //Operator 3    07  08  09  
                //Operator 4    0A  0B  0C  
                default:
                    if (slot >= 3)
                    {
                        adrH = 2;
                        slot -= 3;
                    }
                    if (op >= 2)
                    {
                        op -= 2;
                        slot += 3;
                    }
                    break;
            }
#if DEBUG
            try
            {
                Program.SoundUpdating();
#endif
                byte chofst = 0;
                switch (adrL)
                {
                    case 0x20:
                    case 0x40:
                    case 0x60:
                    case 0x80:
                    case 0xe0:
                        chofst = chAddressOffset[slot];
                        break;
                    case 0xa0:
                    case 0xb0:
                    case 0xc0:
                        chofst = (byte)slot;
                        break;
                }

                var adr = (byte)(adrL + (op * 3) + chofst);
                address = (adrH << 8) | adr;

                WriteData(address, data, useCache, new Action(() =>
                {
                    lock (sndEnginePtrLock)
                    {
                        switch (CurrentSoundEngine)
                        {
                            case SoundEngineType.VSIF_MSX_FTDI:
                                switch (adrH)
                                {
                                    //https://w.atwiki.jp/msx-sdcc/pages/65.html
                                    case 0:
                                        vsifClient.WriteData(10, adr, data, f_ftdiClkWidth);
                                        break;
                                    case 2:
                                        vsifClient.WriteData(11, adr, data, f_ftdiClkWidth);
                                        break;
                                }
                                break;
                            case SoundEngineType.Real_OPL3:
                                WriteOPL3PortData(opl3PortHandle, (byte)adrH, adr);
                                while ((ReadOPL3PortData(opl3PortHandle, 0) & 5) != 0)
                                {
                                    Thread.Sleep(0);
                                    break;
                                }
                                WriteOPL3PortData(opl3PortHandle, (byte)(adrH + 1), data);

                                //FormMain.OutputLog(this, adrH.ToString("x") + "," + adr);
                                //FormMain.OutputLog(this, (adrH + 1).ToString("x") + "," + data);
                                break;
                        }
                    }
                }));
#if DEBUG
                YMF262_write(unitNumber, (uint)adrH, adr);
                YMF262_write(unitNumber, (uint)1, data);
#else
            DeferredWriteData(YMF262_write, unitNumber, (uint)adrH, (byte)(adrL + (op * 3) + chofst));
            DeferredWriteData(YMF262_write, unitNumber, (uint)1, data);
#endif

#if DEBUG
            }
            finally
            {
                Program.SoundUpdated();
            }
#endif
        }

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr hModule, String ProcName);

        [DllImport("CMI8738OPL3Library.dll")]
        private extern static IntPtr OpenOPL3Port(int memberIndex);

        [DllImport("CMI8738OPL3Library.dll")]
        private extern static void CloseOPL3Port(IntPtr OPL3PortHandle);

        [DllImport("CMI8738OPL3Library.dll")]
        private extern static void WriteOPL3PortData(IntPtr OPL3PortHandle, byte offset, byte data);

        [DllImport("CMI8738OPL3Library.dll")]
        private extern static byte ReadOPL3PortData(IntPtr OPL3PortHandle, byte offset);

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
        static YMF262()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ymf262_write");
            if (funcPtr != IntPtr.Zero)
                YMF262_write = (delegate_YMF262_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_YMF262_write));
        }

        private YMF262SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YMF262(uint unitNumber) : base(unitNumber)
        {
            YMF262WriteData(UnitNumber, 0x105, 0, 0, 0, 0, (byte)0x1);    //OPL3 mode

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new YMF262Timbre[512];
            for (int i = 0; i < 512; i++)
                Timbres[i] = new YMF262Timbre();

            CombinedTimbres = new CombinedTimbre[512];
            for (int i = 0; i < 512; i++)
                CombinedTimbres[i] = new CombinedTimbre();

            setPresetInstruments();

            this.soundManager = new YMF262SoundManager(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();

            lock (sndEnginePtrLock)
            {
                if (vsifClient != null)
                    vsifClient.Dispose();
                if (opl3PortHandle != IntPtr.Zero)
                    CloseOPL3Port(opl3PortHandle);
            }

            base.Dispose();
        }

        public static short ReadInt16Big(BinaryReader reader)
        {
            ushort valH = reader.ReadByte();
            ushort valL = reader.ReadByte();

            return (short)(valH << 8 | valL);
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].FB = 0;
            Timbres[0].ALG = 1;

            Timbres[0].Ops[0].AM = 0;
            Timbres[0].Ops[0].VIB = 0;
            Timbres[0].Ops[0].EG = 0;
            Timbres[0].Ops[0].KSR = 0;
            Timbres[0].Ops[0].MFM = 1;
            Timbres[0].Ops[0].KSL = 0;
            Timbres[0].Ops[0].TL = 0;
            Timbres[0].Ops[0].AR = 15;
            Timbres[0].Ops[0].DR = 0;
            Timbres[0].Ops[0].SL = 0;
            Timbres[0].Ops[0].RR = 7;
            Timbres[0].Ops[0].WS = 1;

            Timbres[0].Ops[1].AM = 0;
            Timbres[0].Ops[1].VIB = 0;
            Timbres[0].Ops[1].EG = 0;
            Timbres[0].Ops[1].KSR = 0;
            Timbres[0].Ops[1].MFM = 1;
            Timbres[0].Ops[1].KSL = 0;
            Timbres[0].Ops[1].TL = 0;
            Timbres[0].Ops[1].AR = 15;
            Timbres[0].Ops[1].DR = 0;
            Timbres[0].Ops[1].SL = 7;
            Timbres[0].Ops[1].RR = 7;
            Timbres[0].Ops[1].WS = 1;

            Timbres[0].Ops[2].AM = 0;
            Timbres[0].Ops[2].VIB = 0;
            Timbres[0].Ops[2].EG = 0;
            Timbres[0].Ops[2].KSR = 0;
            Timbres[0].Ops[2].MFM = 1;
            Timbres[0].Ops[2].KSL = 0;
            Timbres[0].Ops[2].TL = 0;
            Timbres[0].Ops[2].AR = 15;
            Timbres[0].Ops[2].DR = 0;
            Timbres[0].Ops[2].SL = 0;
            Timbres[0].Ops[2].RR = 7;
            Timbres[0].Ops[2].WS = 1;

            Timbres[0].Ops[3].AM = 0;
            Timbres[0].Ops[3].VIB = 0;
            Timbres[0].Ops[3].EG = 0;
            Timbres[0].Ops[3].KSR = 0;
            Timbres[0].Ops[3].MFM = 1;
            Timbres[0].Ops[3].KSL = 0;
            Timbres[0].Ops[3].TL = 0;
            Timbres[0].Ops[3].AR = 15;
            Timbres[0].Ops[3].DR = 0;
            Timbres[0].Ops[3].SL = 7;
            Timbres[0].Ops[3].RR = 7;
            Timbres[0].Ops[3].WS = 1;

        }

        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        private void initGlobalRegisters()
        {
            //NEW
            YMF262WriteData(UnitNumber, 0x105, 0, 0, 0, 0, (byte)5);
            //CONSEL
            var v = 0;
            for (int i = 0; i < f_CONSEL; i++)
                v |= 1 << i;
            YMF262WriteData(UnitNumber, 0x104, 0, 0, 0, 0, (byte)v);
            //AMD, VIB
            YMF262WriteData(UnitNumber, 0xBD, 0, 0, 0, 0, (byte)(AMD << 7 | VIB << 6));
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
        private class YMF262SoundManager : SoundManagerBase
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

            private static SoundList<YMF262Sound> fm2OpOnSounds = new SoundList<YMF262Sound>(18);

            private static SoundList<YMF262Sound> fm4OpOnSounds = new SoundList<YMF262Sound>(6);

            private YMF262 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YMF262SoundManager(YMF262 parent) : base(parent)
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
                foreach (YMF262Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(timbre, note);
                    if (emptySlot.slot < 0)
                        continue;

                    YMF262Sound snd = new YMF262Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot, parentModule.CONSEL);
                    switch (timbre.ALG)
                    {
                        case 0:
                        case 1:
                            fm2OpOnSounds.Add(snd);
                            break;
                        default:
                            fm4OpOnSounds.Add(snd);
                            break;
                    }

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
            private (YMF262 inst, int slot) searchEmptySlot(YMF262Timbre timbre, TaggedNoteOnEvent note)
            {
                byte CONSEL = parentModule.CONSEL;
                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.CONSEL.HasValue)
                        CONSEL = gs.CONSEL.Value;
                }
                switch (timbre.ALG)
                {
                    case 0:
                    case 1:
                        int max = 18 - (CONSEL * 2);
                        return SearchEmptySlotAndOffForLeader(parentModule, fm2OpOnSounds, note, max);
                    default:
                        return SearchEmptySlotAndOffForLeader(parentModule, fm4OpOnSounds, note, CONSEL);
                }
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 9; i++)
                    parentModule.YMF262WriteData(parentModule.UnitNumber, (byte)(0xB0 + i), 0, 0, 0, 0, (byte)(0));
                for (int i = 0; i < 9; i++)
                    parentModule.YMF262WriteData(parentModule.UnitNumber, (byte)(0x1B0 + i), 0, 0, 0, 0, (byte)(0));
                for (int i = 0; i < 18; i++)
                    for (int op = 0; op < 2; op++)
                        parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, i, 0, 0, 63);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class YMF262Sound : SoundBase
        {

            private YMF262 parentModule;

            private YMF262Timbre timbre;

            private byte lastFreqData;

            private byte lastALG;

            private byte lastConsel;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YMF262Sound(YMF262 parentModule, YMF262SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot, byte consel) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (YMF262Timbre)timbre;
                lastALG = this.timbre.ALG;
                lastConsel = consel;
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
                    if (gs.CONSEL.HasValue)
                        parentModule.CONSEL = gs.CONSEL.Value;
                    if (gs.DAM.HasValue)
                        parentModule.AMD = gs.DAM.Value;
                    if (gs.DVB.HasValue)
                        parentModule.VIB = gs.DVB.Value;
                }

                SetTimbre();
                //Volume
                OnVolumeUpdated();

                //Freq
                OnPitchUpdated();
            }

            public override void OnSoundParamsUpdated()
            {

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.CONSEL.HasValue)
                        parentModule.CONSEL = gs.CONSEL.Value;
                    if (gs.DAM.HasValue)
                        parentModule.AMD = gs.DAM.Value;
                    if (gs.DVB.HasValue)
                        parentModule.VIB = gs.DVB.Value;
                }

                SetTimbre();

                base.OnSoundParamsUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                var v = CalcCurrentVolume();
                for (int op = 0; op < (lastALG <= 1 ? 2 : 4); op++)
                {
                    YMF262Operator o = timbre.Ops[op];
                    //$40+: Scaling level/ total level
                    if (lastALG <= 1)
                    {
                        if (timbre.ALG == 1 || op == 1)
                            parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | ((63 * 2 / 3) - (byte)Math.Round(((63 * 2 / 3) - (o.TL * 2 / 3)) * v))));
                        else
                            parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | o.TL));
                    }
                    else
                    {
                        switch (lastALG)
                        {
                            case 2:
                                if (op == 1)
                                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | ((63 * 2 / 3) - (byte)Math.Round(((63 * 2 / 3) - (o.TL * 2 / 3)) * v))));
                                else
                                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | o.TL));
                                break;
                            case 3:
                                if (op == 1 || op == 3)
                                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | ((63 * 2 / 3) - (byte)Math.Round(((63 * 2 / 3) - (o.TL * 2 / 3)) * v))));
                                else
                                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | o.TL));
                                break;
                            case 4:
                                if (op == 0 || op == 3)
                                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | ((63 * 2 / 3) - (byte)Math.Round(((63 * 2 / 3) - (o.TL * 2 / 3)) * v))));
                                else
                                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | o.TL));
                                break;
                            case 5:
                                if (op == 0 || op == 2 || op == 3)
                                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | ((63 * 2 / 3) - (byte)Math.Round(((63 * 2 / 3) - (o.TL * 2 / 3)) * v))));
                                else
                                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x40, op, Slot, lastALG, lastConsel, (byte)(o.KSL << 6 | o.TL));
                                break;
                        }
                    }
                }
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

                if (lastALG <= 1)
                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0xc0, 0, Slot, lastALG, lastConsel, (byte)(pan << 6 | pan << 4 | timbre.FB << 1 | lastALG));
                else
                {
                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0xc0, 0, Slot, lastALG, lastConsel, (byte)(pan << 6 | pan << 4 | timbre.FB << 1 | ((lastALG - 2) & 1)));
                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0xc0, 1, Slot, lastALG, lastConsel, (byte)(pan << 6 | pan << 4 | timbre.FB2 << 1 | (((lastALG - 2) >> 1) & 1)));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
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
                int freq = convertFmFrequency(nnOn);
                var octave = nnOn.GetNoteOctave();

                if (d != 0)
                    freq += (ushort)(((double)(convertFmFrequency(nnOn, (d < 0) ? false : true) - freq)) * Math.Abs(d - Math.Truncate(d)));

                if (octave < 0)
                {
                    freq /= 2 * -octave;
                    octave = 0;
                }
                else if (octave > 7)
                {
                    freq *= 2 * (octave - 7);
                    if (freq > 0x3ff)
                        freq = 0x3ff;
                    octave = 7;
                }

                octave = octave << 2;

                //keyon
                byte kon = IsKeyOff ? (byte)0 : (byte)0x20;
                lastFreqData = (byte)(kon | octave | ((freq >> 8) & 3));

                parentModule.YMF262WriteData(parentModule.UnitNumber, 0xa0, 0, Slot, lastALG, lastConsel, (byte)(0xff & freq), false);
                parentModule.YMF262WriteData(parentModule.UnitNumber, 0xb0, 0, Slot, lastALG, lastConsel, lastFreqData, false);

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                for (int op = 0; op < (lastALG <= 1 ? 2 : 4); op++)
                {
                    YMF262Operator o = timbre.Ops[op];
                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x20, op, Slot, lastALG, lastConsel, (byte)((o.AM << 7 | o.VIB << 6 | o.EG << 5 | o.KSR << 4 | o.MFM)));
                    //$60+: Attack Rate / Decay Rate
                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0x60, op, Slot, lastALG, lastConsel, (byte)(o.AR << 4 | o.DR));
                    //$80+: Sustain Level / Release Rate
                    if (o.SR.HasValue && o.EG == 0)
                        parentModule.YMF262WriteData(parentModule.UnitNumber, 0x80, op, Slot, lastALG, lastConsel, (byte)(o.SL << 4 | o.SR.Value));
                    else
                        parentModule.YMF262WriteData(parentModule.UnitNumber, 0x80, op, Slot, lastALG, lastConsel, (byte)(o.SL << 4 | o.RR));

                    //$e0+: Waveform Select
                    parentModule.YMF262WriteData(parentModule.UnitNumber, 0xe0, op, Slot, lastALG, lastConsel, (byte)(o.WS));
                }

                //$C0+: algorithm and feedback & PAN
                OnPanpotUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                for (int op = 0; op < (lastALG <= 1 ? 2 : 4); op++)
                {
                    YMF262Operator o = timbre.Ops[op];
                    if (o.SR.HasValue && o.EG == 0)
                    {
                        parentModule.YMF262WriteData(parentModule.UnitNumber, 0x20, op, Slot, lastALG, lastConsel, (byte)((o.AM << 7 | o.VIB << 6 | 1 << 5 | o.KSR << 4 | o.MFM)));
                        parentModule.YMF262WriteData(parentModule.UnitNumber, 0x80, op, Slot, lastALG, lastConsel, (byte)(o.SL << 4 | o.RR));
                    }
                }

                parentModule.YMF262WriteData(parentModule.UnitNumber, 0xB0, 0, Slot, lastALG, lastConsel, (byte)(lastFreqData & 0x1f));
            }

            private ushort[] freqTable = new ushort[] {
                0x287/2,
                0x157,
                0x16B,
                0x181,
                0x198,
                0x1B0,
                0x1CA,
                0x1E5,
                0x202,
                0x220,
                0x241,
                0x263,
                0x287,
                0x157*2,
            };

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(TaggedNoteOnEvent note)
            {
                return freqTable[(int)note.GetNoteName() + 1];
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="note"></param>
            /// <param name="freq"></param>
            /// <returns></returns>
            private ushort convertFmFrequency(TaggedNoteOnEvent note, bool plus)
            {
                if (plus)
                    return freqTable[(int)note.GetNoteName() + 2];
                else
                    return freqTable[(int)note.GetNoteName()];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YMF262Timbre>))]
        [DataContract]
        public class YMF262Timbre : TimbreBase
        {
            #region FM Synth

            [Category("Sound")]
            [Editor(typeof(YMF262UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
                        nameof(FB2),

                        "GlobalSettings.EN",
                        "GlobalSettings.DAM",
                        "GlobalSettings.DVB",

                        "Ops[0].AR",
                        "Ops[0].DR",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].SR",
                        "Ops[0].TL",
                        "Ops[0].KSL",
                        "Ops[0].KSR",
                        "Ops[0].MFM",
                        "Ops[0].AM",
                        "Ops[0].VIB",
                        "Ops[0].EG",
                        "Ops[0].WS",

                        "Ops[1].AR",
                        "Ops[1].DR",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].SR",
                        "Ops[1].TL",
                        "Ops[1].KSL",
                        "Ops[1].KSR",
                        "Ops[1].MFM",
                        "Ops[1].AM",
                        "Ops[1].VIB",
                        "Ops[1].EG",
                        "Ops[1].WS",

                        "Ops[2].AR",
                        "Ops[2].DR",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].SR",
                        "Ops[2].TL",
                        "Ops[2].KSL",
                        "Ops[2].KSR",
                        "Ops[2].MFM",
                        "Ops[2].AM",
                        "Ops[2].VIB",
                        "Ops[2].EG",
                        "Ops[2].WS",

                        "Ops[3].AR",
                        "Ops[3].DR",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].SR",
                        "Ops[3].TL",
                        "Ops[3].KSL",
                        "Ops[3].KSR",
                        "Ops[3].MFM",
                        "Ops[3].AM",
                        "Ops[3].VIB",
                        "Ops[3].EG",
                        "Ops[3].WS"
                        );
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),
                        nameof(FB2),

                        "GlobalSettings.EN",
                        "GlobalSettings.DAM",
                        "GlobalSettings.DVB",

                        "Ops[0].AR",
                        "Ops[0].DR",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].SR",
                        "Ops[0].TL",
                        "Ops[0].KSL",
                        "Ops[0].KSR",
                        "Ops[0].MFM",
                        "Ops[0].AM",
                        "Ops[0].VIB",
                        "Ops[0].EG",
                        "Ops[0].WS",

                        "Ops[1].AR",
                        "Ops[1].DR",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].SR",
                        "Ops[1].TL",
                        "Ops[1].KSL",
                        "Ops[1].KSR",
                        "Ops[1].MFM",
                        "Ops[1].AM",
                        "Ops[1].VIB",
                        "Ops[1].EG",
                        "Ops[1].WS",

                        "Ops[2].AR",
                        "Ops[2].DR",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].SR",
                        "Ops[2].TL",
                        "Ops[2].KSL",
                        "Ops[2].KSR",
                        "Ops[2].MFM",
                        "Ops[2].AM",
                        "Ops[2].VIB",
                        "Ops[2].EG",
                        "Ops[2].WS",

                        "Ops[3].AR",
                        "Ops[3].DR",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].SR",
                        "Ops[3].TL",
                        "Ops[3].KSL",
                        "Ops[3].KSR",
                        "Ops[3].MFM",
                        "Ops[3].AM",
                        "Ops[3].VIB",
                        "Ops[3].EG",
                        "Ops[3].WS");
                }
            }

            private byte f_ALG;

            [DataMember]
            [Category("Sound")]
            [Description("Algorithm (0-1 in 2 Op Mode, 2-5 in 4 Op Mode) \r\n" +
                "2 Op Mode:\r\n" +
                " 0: 1->2 (for Distortion guitar sound)\r\n" +
                " 1: 1+2 (for Pipe organ sound)\r\n" +
                "4 Op Mode:\r\n" +
                " 2: 1->2->3->4\r\n" +
                " 3: (1+2)+(3+4)\r\n" +
                " 4: 1+(2+3+4)\r\n" +
                " 5: 1+(2+3)+4")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 5)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte ALG
            {
                get
                {
                    return f_ALG;
                }
                set
                {
                    if (value < 6)
                        f_ALG = value;
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

            private byte f_FB2;

            [DataMember]
            [Category("Sound")]
            [Description("Alt Feedback for 4Op (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte FB2
            {
                get
                {
                    return f_FB2;
                }
                set
                {
                    f_FB2 = (byte)(value & 7);
                }
            }


            #endregion

            /// <summary>
            /// 
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Operators. Uses only first 2 opperators in 2 op mode.")]
            [TypeConverter(typeof(ExpandableCollectionConverter))]
            [DisplayName("Operators[Ops]")]
            [EditorAttribute(typeof(DummyEditor), typeof(UITypeEditor))]
            public YMF262Operator[] Ops
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
                var ops = new YMF262Operator[] {
                    new YMF262Operator(),
                    new YMF262Operator(),
                    new YMF262Operator(),
                    new YMF262Operator() };
                for (int i = 0; i < Ops.Length; i++)
                    Ops[i].InjectFrom(new LoopInjection(), ops[i]);
            }

            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                typeof(UITypeEditor)), Localizable(false)]
            [IgnoreDataMember]
            [JsonIgnore]
            [Category("Sound")]
            [Description("You can copy and paste this text data to other same type timber.\r\n" +
                "ALG, FB, AR, DR, RR, SL, TL, KSL, KSR, MFM, AM(AMS), VIB, EG(EGT), WS, ...\r\n" +
                "You can use comma or space chars as delimiter.")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(ALG),
                        nameof(FB),
                        nameof(FB2),

                        "Ops[0].AR",
                        "Ops[0].DR",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].SR",
                        "Ops[0].TL",
                        "Ops[0].KSL",
                        "Ops[0].KSR",
                        "Ops[0].MFM",
                        "Ops[0].AM",
                        "Ops[0].VIB",
                        "Ops[0].EG",
                        "Ops[0].WS",

                        "Ops[1].AR",
                        "Ops[1].DR",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].SR",
                        "Ops[1].TL",
                        "Ops[1].KSL",
                        "Ops[1].KSR",
                        "Ops[1].MFM",
                        "Ops[1].AM",
                        "Ops[1].VIB",
                        "Ops[1].EG",
                        "Ops[1].WS",

                        "Ops[2].AR",
                        "Ops[2].DR",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].SR",
                        "Ops[2].TL",
                        "Ops[2].KSL",
                        "Ops[2].KSR",
                        "Ops[2].MFM",
                        "Ops[2].AM",
                        "Ops[2].VIB",
                        "Ops[2].EG",
                        "Ops[2].WS",

                        "Ops[3].AR",
                        "Ops[3].DR",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].SR",
                        "Ops[3].TL",
                        "Ops[3].KSL",
                        "Ops[3].KSR",
                        "Ops[3].MFM",
                        "Ops[3].AM",
                        "Ops[3].VIB",
                        "Ops[3].EG",
                        "Ops[3].WS");
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),
                        nameof(FB2),

                        "Ops[0].AR",
                        "Ops[0].DR",
                        "Ops[0].RR",
                        "Ops[0].SL",
                        "Ops[0].SR",
                        "Ops[0].TL",
                        "Ops[0].KSL",
                        "Ops[0].KSR",
                        "Ops[0].MFM",
                        "Ops[0].AM",
                        "Ops[0].VIB",
                        "Ops[0].EG",
                        "Ops[0].WS",

                        "Ops[1].AR",
                        "Ops[1].DR",
                        "Ops[1].RR",
                        "Ops[1].SL",
                        "Ops[1].SR",
                        "Ops[1].TL",
                        "Ops[1].KSL",
                        "Ops[1].KSR",
                        "Ops[1].MFM",
                        "Ops[1].AM",
                        "Ops[1].VIB",
                        "Ops[1].EG",
                        "Ops[1].WS",

                        "Ops[2].AR",
                        "Ops[2].DR",
                        "Ops[2].RR",
                        "Ops[2].SL",
                        "Ops[2].SR",
                        "Ops[2].TL",
                        "Ops[2].KSL",
                        "Ops[2].KSR",
                        "Ops[2].MFM",
                        "Ops[2].AM",
                        "Ops[2].VIB",
                        "Ops[2].EG",
                        "Ops[2].WS",

                        "Ops[3].AR",
                        "Ops[3].DR",
                        "Ops[3].RR",
                        "Ops[3].SL",
                        "Ops[3].SR",
                        "Ops[3].TL",
                        "Ops[3].KSL",
                        "Ops[3].KSR",
                        "Ops[3].MFM",
                        "Ops[3].AM",
                        "Ops[3].VIB",
                        "Ops[3].EG",
                        "Ops[3].WS");
                }
            }

            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public YMF262GlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new YMF262GlobalSettings());
            }

            /// <summary>
            /// 
            /// </summary>
            public YMF262Timbre()
            {
                Ops = new YMF262Operator[] {
                    new YMF262Operator(),
                    new YMF262Operator(),
                    new YMF262Operator(),
                    new YMF262Operator() };
                GlobalSettings = new YMF262GlobalSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YMF262Timbre>(serializeData);
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
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YMF262Operator>))]
        [DataContract]
        [InstLock]
        public class YMF262Operator : ContextBoundObject
        {

            private byte f_AR;

            /// <summary>
            /// Attack Rate (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Attack Rate (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AR
            {
                get
                {
                    return f_AR;
                }
                set
                {
                    f_AR = (byte)(value & 15);
                }
            }

            private byte f_DR;

            /// <summary>
            /// Decay Rate (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Decay Rate (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte DR
            {
                get
                {
                    return f_DR;
                }
                set
                {
                    f_DR = (byte)(value & 15);
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
            /// Sustain Level (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Sustain Level (0-15)")]
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

            private byte? f_SR;

            /// <summary>
            /// Sustain rate(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("When EG = 0 and value is set, Used as Sustain Rate (0-15) when KOFF")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? SR
            {
                get
                {
                    return f_SR;
                }
                set
                {
                    if (value.HasValue)
                        f_SR = (byte)(value & 15);
                    else
                        f_SR = value;
                }
            }

            private byte f_TL;

            /// <summary>
            /// Total Level(0-63)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Total Level (0-63)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 63)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte TL
            {
                get
                {
                    return f_TL;
                }
                set
                {
                    f_TL = (byte)(value & 63);
                }
            }

            private byte f_KSL;

            /// <summary>
            /// Key Scaling Level(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Key Scaling Level (00:No Change 10:1.5dB/8ve 01:3dB/8ve 11:6dB/8ve)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte KSL
            {
                get
                {
                    return f_KSL;
                }
                set
                {
                    f_KSL = (byte)(value & 3);
                }
            }

            private byte f_KSR;

            /// <summary>
            /// Keyboard scaling rate (0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Keyboard scaling rate (1: the sound's envelope is foreshortened as it rises in pitch.")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte KSR
            {
                get
                {
                    return f_KSR;
                }
                set
                {
                    f_KSR = (byte)(value & 1);
                }
            }

            private byte f_MFM = 1;

            /// <summary>
            /// Modulator Frequency Multiple (0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Modulator Frequency Multiple (0-1-15)")]
            [DefaultValue((byte)1)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte MFM
            {
                get
                {
                    return f_MFM;
                }
                set
                {
                    f_MFM = (byte)(value & 15);
                }
            }

            private byte f_AM;

            /// <summary>
            /// Apply amplitude modulation(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Apply amplitude modulation (0:Off 1:On)")]
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

            private byte f_VR;

            /// <summary>
            /// Apply vibrato(0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Vibrato (0:Off 1:On)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte VIB
            {
                get
                {
                    return f_VR;
                }
                set
                {
                    f_VR = (byte)(value & 1);
                }
            }

            private byte f_EG;

            /// <summary>
            /// EG Type (0-1)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("EG Type (0:the sound begins to decay immediately after hitting the SUSTAIN phase 1:the sustain level of the voice is maintained until released")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EG
            {
                get
                {
                    return f_EG;
                }
                set
                {
                    f_EG = (byte)(value & 1);
                }
            }

            private byte f_WS;

            /// <summary>
            /// Waveform Select (0-7)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Waveform Select (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte WS
            {
                get
                {
                    return f_WS;
                }
                set
                {
                    f_WS = (byte)(value & 7);
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
                "AR, DR, RR, SL, TL, KSL, KSR, MFM, AM(AMS), VR, EG(EGT), WS\r\n" +
                "You can use comma or space chars as delimiter.")]
            public string MmlSerializeData
            {
                get
                {
                    return SimpleSerializer.SerializeProps(this,
                        nameof(AR),
                        nameof(DR),
                        nameof(RR),
                        nameof(SL),
                        nameof(TL),
                        nameof(KSL),
                        nameof(KSR),
                        nameof(MFM),
                        nameof(AM),
                        nameof(VIB),
                        nameof(EG),
                        nameof(WS));
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(AR),
                        nameof(DR),
                        nameof(RR),
                        nameof(SL),
                        nameof(TL),
                        nameof(KSL),
                        nameof(KSR),
                        nameof(MFM),
                        nameof(AM),
                        nameof(VIB),
                        nameof(EG),
                        nameof(WS));
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
                    var obj = JsonConvert.DeserializeObject<YMF262Operator>(serializeData);
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
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YMF262GlobalSettings>))]
        [DataContract]
        [InstLock]
        public class YMF262GlobalSettings : ContextBoundObject
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

            private byte? f_CONSEL;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Max number of 4 op mode sound")]
            [SlideParametersAttribute(0, 6)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? CONSEL
            {
                get
                {
                    return f_CONSEL;
                }
                set
                {
                    byte? v = value;
                    if (!value.HasValue || value <= 6)
                        f_CONSEL = value;
                }
            }

            private byte? f_AMD;

            /// <summary>
            /// AM Depth (0-1)
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("AM depth (0:1dB 1:4.8dB)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? DAM
            {
                get
                {
                    return f_AMD;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_AMD = v;
                }
            }

            private byte? f_VIB;

            /// <summary>
            /// Vibrato depth (0:7 cent 1:14 cent)
            /// </summary>
            [DataMember]
            [Category("Chip")]
            [Description("Vibrato depth (0:7 cent 1:14 cent)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? DVB
            {
                get
                {
                    return f_VIB;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_VIB = v;
                }
            }
        }

    }

}