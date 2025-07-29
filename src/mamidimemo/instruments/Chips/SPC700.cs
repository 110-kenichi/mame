// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FM_SoundConvertor;
using Kermalis.SoundFont2;
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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2608;

//https://wiki.superfamicom.org/spc700-reference

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class SPC700 : InstrumentBase
    {
        public override string Name => "SPC700";

        public override string Group => "PCM";

        public override InstrumentType InstrumentType => InstrumentType.SPC700;

        [Browsable(false)]
        public override string ImageKey => "SPC700";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "snes_sound_";

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
                return 17;
            }
        }

        private object sndEnginePtrLock = new object();

        private int gimicPtr = -1;

        private SoundEngineType f_SoundEngineType;

        private SoundEngineType f_CurrentSoundEngineType;

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Select a sound engine type.\r\n" +
            "Supports Software and SPFM/VSIF/G.I.M.I.C .")]
        [DefaultValue(SoundEngineType.Software)]
        [TypeConverter(typeof(EnumConverterSoundEngineTypeSPFM))]
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
                    value == SoundEngineType.GIMIC))
                {
                    setSoundEngine(value);
                }
            }
        }

        private class EnumConverterSoundEngineTypeSPFM : EnumConverter<SoundEngineType>
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var sc = new StandardValuesCollection(new SoundEngineType[] {
                    SoundEngineType.Software,
                    SoundEngineType.GIMIC,
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
                if (gimicPtr != -1)
                {
                    GimicManager.ReleaseModule(gimicPtr);
                    gimicPtr = -1;
                }

                f_SoundEngineType = value;

                switch (f_SoundEngineType)
                {
                    case SoundEngineType.Software:
                        f_CurrentSoundEngineType = f_SoundEngineType;
                        SetDevicePassThru(false);
                        break;
                    case SoundEngineType.GIMIC:
                        gimicPtr = GimicManager.GetModuleIndex(GimicManager.ChipType.CHIP_SPC);
                        if (gimicPtr >= 0)
                        {
                            //GimicManager.SetClock(gimicPtr, (uint)(2.048*1000*1000));
                            f_CurrentSoundEngineType = f_SoundEngineType;
                            SetDevicePassThru(true);
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

        private byte f_LMVOL;

        [DataMember]
        [Category("Chip")]
        [Description("Set Left Output Main Volume")]
        [DefaultValue((byte)127)]
        [SlideParametersAttribute(0, 255)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte LMVOL
        {
            get
            {
                return f_LMVOL;
            }
            set
            {
                if (f_LMVOL != value)
                {
                    f_LMVOL = value;

                    SPC700RegWriteData(UnitNumber, 0xc, value);
                }
            }
        }

        private byte f_RMVOL;

        [DataMember]
        [Category("Chip")]
        [Description("Set Right Output Main Volume")]
        [DefaultValue((byte)127)]
        [SlideParametersAttribute(0, 255)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte RMVOL
        {
            get
            {
                return f_RMVOL;
            }
            set
            {
                if (f_RMVOL != value)
                {
                    f_RMVOL = value;

                    SPC700RegWriteData(UnitNumber, 0x1c, value);
                }
            }
        }

        private sbyte f_LEVOL;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("Set Left Output Echo Volume")]
        [DefaultValue(typeof(sbyte), "127")]
        [SlideParametersAttribute(-128, 128)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte LEVOL
        {
            get
            {
                return f_LEVOL;
            }
            set
            {
                if (f_LEVOL != value)
                {
                    f_LEVOL = value;

                    SPC700RegWriteData(UnitNumber, 0x2c, (byte)value);
                }
            }
        }

        private sbyte f_REVOL;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("Set Right Output Echo Volume")]
        [DefaultValue(typeof(sbyte), "127")]
        [SlideParametersAttribute(-128, 128)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte REVOL
        {
            get
            {
                return f_REVOL;
            }
            set
            {
                if (f_REVOL != value)
                {
                    f_REVOL = value;

                    SPC700RegWriteData(UnitNumber, 0x3c, (byte)value);
                }
            }
        }

        private byte f_NOISE_CLOCK;

        [DataMember]
        [Category("Chip")]
        [Description("Designates the frequency for the white noise.")]
        [DefaultValue((byte)31)]
        [SlideParametersAttribute(0, 31)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte NOISE_CLOCK
        {
            get
            {
                return f_NOISE_CLOCK;
            }
            set
            {
                if (f_NOISE_CLOCK != value)
                {
                    f_NOISE_CLOCK = (byte)(value & 31);

                    SPC700RegWriteData(UnitNumber, (byte)0x6c, (byte)(((~ECEN & 1) << 5) | f_NOISE_CLOCK));
                }
            }
        }

        private byte f_ECEN;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("Echo enable")]
        [DefaultValue((byte)1)]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte ECEN
        {
            get
            {
                return f_ECEN;
            }
            set
            {
                if (f_ECEN != value)
                {
                    f_ECEN = (byte)(value & 1);

                    SPC700RegWriteData(UnitNumber, 0x6c, (byte)(((~f_ECEN & 1) << 5) | NOISE_CLOCK));
                }
            }
        }


        private sbyte f_EFB;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("Echo Feedback")]
        [DefaultValue(typeof(sbyte), "31")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte EFB
        {
            get
            {
                return f_EFB;
            }
            set
            {
                if (f_EFB != value)
                {
                    f_EFB = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x0d, (byte)f_EFB);
                }
            }
        }

        private byte f_EDL;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("EDL specifies the delay between the main sound and the echoed sound. The delay is calculated as EDL * 16ms.")]
        [DefaultValue((byte)1)]
        [SlideParametersAttribute(0, 15)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte EDL
        {
            get
            {
                return f_EDL;
            }
            set
            {
                if (f_EDL != value)
                {
                    f_EDL = (byte)(value & 15);

                    SPC700RegWriteData(UnitNumber, (byte)0x7d, f_EDL);
                    updatePcmData(null);
                }
            }
        }

        private sbyte f_COEF1;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "127")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF1
        {
            get
            {
                return f_COEF1;
            }
            set
            {
                if (f_COEF1 != value)
                {
                    f_COEF1 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x0f, (byte)f_COEF1);
                }
            }
        }

        private sbyte f_COEF2;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF2
        {
            get
            {
                return f_COEF2;
            }
            set
            {
                if (f_COEF2 != value)
                {
                    f_COEF2 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x1f, (byte)f_COEF2);
                }
            }
        }

        private sbyte f_COEF3;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF3
        {
            get
            {
                return f_COEF3;
            }
            set
            {
                if (f_COEF3 != value)
                {
                    f_COEF3 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x2f, (byte)f_COEF3);
                }
            }
        }

        private sbyte f_COEF4;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF4
        {
            get
            {
                return f_COEF4;
            }
            set
            {
                if (f_COEF4 != value)
                {
                    f_COEF4 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x3f, (byte)f_COEF4);
                }
            }
        }

        private sbyte f_COEF5;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF5
        {
            get
            {
                return f_COEF5;
            }
            set
            {
                if (f_COEF5 != value)
                {
                    f_COEF5 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x4f, (byte)f_COEF5);
                }
            }
        }


        private sbyte f_COEF6;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF6
        {
            get
            {
                return f_COEF6;
            }
            set
            {
                if (f_COEF6 != value)
                {
                    f_COEF6 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x5f, (byte)f_COEF6);
                }
            }
        }

        private sbyte f_COEF7;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF7
        {
            get
            {
                return f_COEF7;
            }
            set
            {
                if (f_COEF7 != value)
                {
                    f_COEF7 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x6f, (byte)f_COEF7);
                }
            }
        }

        private sbyte f_COEF8;

        [DataMember]
        [Category("Chip(Filter)")]
        [Description("COEF are used by the 8-tap FIR filter.")]
        [DefaultValue(typeof(sbyte), "0")]
        [SlideParametersAttribute(-128, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public sbyte COEF8
        {
            get
            {
                return f_COEF8;
            }
            set
            {
                if (f_COEF8 != value)
                {
                    f_COEF8 = value;

                    SPC700RegWriteData(UnitNumber, (byte)0x7f, (byte)f_COEF8);
                }
            }
        }

        /*
        [DataMember]
        [Category("Chip")]
        [Description("Assign PCM data to DRUM soundtype instrument.\r\n" +
            "BRR ADPCM Data (MAX 64KB)")]
        [Editor(typeof(PcmTableUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [PcmTableEditor("Audio File(*.brr)|*.brr")]
        //[PcmTableEditor("Audio File(*.brr, *.wav)|*.brr;*.wav")]
        [TypeConverter(typeof(CustomObjectTypeConverter))]
        public SPC700PcmSoundTable DrumSoundTable
        {
            get;
            private set;
        }
        */

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
                Timbres = (SPC700Timbre[])value;
            }
        }

        private SPC700Timbre[] f_Timbres;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Category(" Timbres")]
        [Description("Timbres")]
        [EditorAttribute(typeof(TimbresArrayUITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public SPC700Timbre[] Timbres
        {
            get
            {
                return f_Timbres;
            }
            set
            {
                f_Timbres = value;
            }
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
                using (var obj = JsonConvert.DeserializeObject<SPC700>(serializeData))
                    this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad" }), obj);
                SPC700SetCallback(UnitNumber, f_read_byte_callback);
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
        private delegate void delegate_spc_ram_w(uint unitNumber, uint address, byte data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delegate_spc_ram_r(uint unitNumber, uint address);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_spc_resample(double org_rate, double target_rate, IntPtr org_buffer, uint org_len, IntPtr target_buffer, uint target_len);

        [DllImport("split700.Dll", EntryPoint = "ExtractSpcFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool ExtractSpcFile(string fileName, IntPtr[] sample_buffers, uint[] sample_sizes);

        [DllImport("split700.Dll", EntryPoint = "ExtractSpcFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool FreeExtractedSpcSampleBuffer(IntPtr sample_buffer_ptr);

        /// <summary>
        /// 
        /// </summary>
        private void SPC700RamWriteData(uint unitNumber, uint address, byte data, byte dest)
        {
            if ((dest & 1) != 0)
            {
                WriteData(address, data, true, new Action(() =>
                {
                    lock (sndEnginePtrLock)
                    {
                        switch (CurrentSoundEngine)
                        {
                            case SoundEngineType.GIMIC:
                                GimicManager.SetRegister2(gimicPtr, address, data, 1);
                                break;
                        }
                    }
                }));
            }
            if ((dest & 2) != 0)
            {
                WriteData(0x10000 + address, data, true, new Action(() =>
                {
                    DeferredWriteData(spc_ram_w, unitNumber, address, data);

                }));
            }
            /*
            try
            {
                Program.SoundUpdating();
                spc_ram_w(unitNumber, address, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        private void SPC700RegWriteData(uint unitNumber, byte reg, byte data)
        {
            SPC700RegWriteData(unitNumber, reg, data, false, false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void SPC700RegWriteData(uint unitNumber, byte reg, byte data, bool useCache, bool internalOnly)
        {
            WriteData(reg, data, useCache, new Action(() =>
            {
                if (!internalOnly)
                {
                    lock (sndEnginePtrLock)
                    {
                        switch (CurrentSoundEngine)
                        {
                            case SoundEngineType.GIMIC:
                                GimicManager.SetRegister2(gimicPtr, reg, data, 0);
                                break;
                        }
                    }
                }

                DeferredWriteData(spc_ram_w, unitNumber, (uint)0xf2, reg);
                DeferredWriteData(spc_ram_w, unitNumber, (uint)0xf3, data);

                //FormMain.OutputDebugLog(this, "adr:" + (byte)(address + (op * 4) + (slot % 3)) + " dat:" + data);
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
            }));

            /*
            try
            {
                Program.SoundUpdating();
                spc_ram_w(unitNumber, 0xf2, reg);
                spc_ram_w(unitNumber, 0xf3, data);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static byte SPC700RegReadData(uint unitNumber, byte reg)
        {
            try
            {
                Program.SoundUpdating();
                FlushDeferredWriteData();

                spc_ram_w(unitNumber, 0xf2, reg);
                return spc_ram_r(unitNumber, 0xf3);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static byte SPC700RamReadData(uint unitNumber, uint address)
        {
            try
            {
                Program.SoundUpdating();
                FlushDeferredWriteData();

                return spc_ram_r(unitNumber, address);
            }
            finally
            {
                Program.SoundUpdated();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static delegate_spc_ram_w spc_ram_w
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_spc_ram_r spc_ram_r
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_spc_resample spc_resample
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte delg_callback(byte pn, int pos);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void delegate_set_callback(uint unitNumber, delg_callback callback);

        /// <summary>
        /// 
        /// </summary>
        private static void SPC700SetCallback(uint unitNumber, delg_callback callback)
        {
            DeferredWriteData(set_callback, unitNumber, callback);
            /*
            try
            {
                Program.SoundUpdating();
                set_callback(unitNumber, callback);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        private static delegate_set_callback set_callback
        {
            get;
            set;
        }

        private Dictionary<int, byte[]> tmpPcmDataTable = new Dictionary<int, byte[]>();

        /// <summary>
        /// 
        /// </summary>
        static SPC700()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("spc_ram_w");
            if (funcPtr != IntPtr.Zero)
                spc_ram_w = (delegate_spc_ram_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_spc_ram_w));

            funcPtr = MameIF.GetProcAddress("spc_ram_r");
            if (funcPtr != IntPtr.Zero)
                spc_ram_r = (delegate_spc_ram_r)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_spc_ram_r));

            funcPtr = MameIF.GetProcAddress("spc700_set_callback");
            if (funcPtr != IntPtr.Zero)
                set_callback = (delegate_set_callback)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_set_callback));

            funcPtr = MameIF.GetProcAddress("spc700_resample");
            if (funcPtr != IntPtr.Zero)
                spc_resample = (delegate_spc_resample)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_spc_resample));
        }

        private SPC700SoundManager soundManager;

        private delg_callback f_read_byte_callback;

        /// <summary>
        /// 
        /// </summary>
        public SPC700(uint unitNumber) : base(unitNumber)
        {
            Timbres = new SPC700Timbre[256];
            for (int i = 0; i < 256; i++)
                Timbres[i] = new SPC700Timbre();

            //DrumSoundTable = new SPC700PcmSoundTable();

            setPresetInstruments();

            this.soundManager = new SPC700SoundManager(this);

            f_read_byte_callback = new delg_callback(read_byte_callback);
            SPC700SetCallback(UnitNumber, f_read_byte_callback);

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            f_EDL = 1;
            f_EFB = 31;
            f_NOISE_CLOCK = 31;
            f_ECEN = 1;
            f_LEVOL = 127;
            f_REVOL = 127;
            f_LMVOL = 127;
            f_RMVOL = 127;
            f_COEF1 = 127;

            readSpcFileForTimbre = new ToolStripMenuItem(Resources.ImportSpcTimbre);
            readSpcFileForTimbre.Click += ReadSpcFileForTimbre_Click;

            readSpcFileForDrumTimbre = new ToolStripMenuItem(Resources.ImportSpcDrum);
            readSpcFileForDrumTimbre.Click += ReadSpcFileForDrumTimbre_Click;

            readSoundFontForTimbre = new ToolStripMenuItem(Resources.ImportSF2Timbre);
            readSoundFontForTimbre.Click += ReadSoundFontForTimbre_Click;

            readSoundFontForDrumTimbre = new ToolStripMenuItem(Resources.ImportSF2Drum);
            readSoundFontForDrumTimbre.Click += ReadSoundFontForDrumTimbre_Click;
        }

        #region IDisposable Support

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //マネージ状態を破棄します (マネージ オブジェクト)。
                    soundManager?.Dispose();
                    soundManager = null;

                    readSpcFileForTimbre?.Dispose();
                    readSpcFileForTimbre = null;

                    readSpcFileForDrumTimbre?.Dispose();
                    readSpcFileForDrumTimbre = null;

                    readSoundFontForTimbre?.Dispose();
                    readSoundFontForTimbre = null;

                    readSoundFontForDrumTimbre?.Dispose();
                    readSoundFontForDrumTimbre = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                SPC700SetCallback(UnitNumber, null);

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        ~SPC700()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public override void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);

            base.Dispose();

            lock (sndEnginePtrLock)
            {
                if (gimicPtr >= 0)
                {
                    GimicManager.ReleaseModule(gimicPtr);
                    gimicPtr = -1;
                }
            }

            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pn"></param>
        /// <param name="pos"></param>
        private byte read_byte_callback(byte pn, int pos)
        {
            lock (tmpPcmDataTable)
            {
                if (tmpPcmDataTable.ContainsKey(pn))
                {
                    //HACK: Thread UNSAFE
                    byte[] pd = tmpPcmDataTable[pn];
                    if (pd != null && pd.Length != 0 && pos < pd.Length)
                        return pd[pos];
                }
            }
            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        internal override void PrepareSound()
        {
            base.PrepareSound();

            initGlobalRegisters();
        }

        private void initGlobalRegisters()
        {
            SPC700RegWriteData(UnitNumber, 0xc, f_LMVOL);
            SPC700RegWriteData(UnitNumber, 0x1c, f_RMVOL);
            SPC700RegWriteData(UnitNumber, 0x2c, (byte)f_LEVOL);
            SPC700RegWriteData(UnitNumber, 0x2c, (byte)f_REVOL);
            SPC700RegWriteData(UnitNumber, (byte)0x6c, (byte)(((~f_ECEN & 1) << 5) | f_NOISE_CLOCK));
            SPC700RegWriteData(UnitNumber, (byte)0x0d, (byte)f_EFB);
            SPC700RegWriteData(UnitNumber, (byte)0x7d, f_EDL);
            SPC700RegWriteData(UnitNumber, (byte)0x0f, (byte)f_COEF1);
            SPC700RegWriteData(UnitNumber, (byte)0x1f, (byte)f_COEF2);
            SPC700RegWriteData(UnitNumber, (byte)0x2f, (byte)f_COEF3);
            SPC700RegWriteData(UnitNumber, (byte)0x3f, (byte)f_COEF4);
            SPC700RegWriteData(UnitNumber, (byte)0x4f, (byte)f_COEF5);
            SPC700RegWriteData(UnitNumber, (byte)0x5f, (byte)f_COEF6);
            SPC700RegWriteData(UnitNumber, (byte)0x6f, (byte)f_COEF7);
            SPC700RegWriteData(UnitNumber, (byte)0x7f, (byte)f_COEF8);

            //DIR 0x200 - 0x5ff
            SPC700RegWriteData(UnitNumber, (byte)0x5d, (byte)2);
            //ESA 0x600 - (MAX: 0x7B00-1)
            SPC700RegWriteData(UnitNumber, (byte)0x6d, (byte)6);

            lock (sndEnginePtrLock)
                lastTransferPcmData = new byte[] { };

            if (!IsDisposing)
                updatePcmData(null);
        }


        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].SoundType = SoundType.INST;
        }


        /// <summary>
        /// 
        /// </summary>
        private void updatePcmData(SPC700Timbre timbre)
        {
            lock (sndEnginePtrLock)
            {
                if (CurrentSoundEngine == SoundEngineType.Software)
                    return;
            }
            List<byte> pcmData = new List<byte>();
            //uint nextStartAddress = (uint)(0x600 + (f_ECEN * f_EDL * 2048));
            uint nextStartAddress = (uint)(0x600 + (f_EDL * 2048));
            if (nextStartAddress == 0)
                nextStartAddress += 4;
            for (int i = 0; i < Timbres.Length; i++)
            {
                var tim = Timbres[i];

                tim.PcmAddressStart = 0;
                tim.PcmAddressEnd = 0;
                if (tim.AdpcmData.Length != 0)
                {
                    int tlen = tim.AdpcmData.Length;
                    int pad = 9 - (tlen % 9);    //9 byte pad

                    if (nextStartAddress + tlen + pad - 1 < 0xFDC0)   //MAX 63KB
                    {
                        tim.PcmAddressStart = nextStartAddress;
                        tim.PcmAddressEnd = (uint)(nextStartAddress + tlen + pad - 1);

                        SPC700RamWriteData(UnitNumber, (uint)(0x200 + (i * 4) + 0), (byte)(nextStartAddress & 0xff), 0x1);
                        SPC700RamWriteData(UnitNumber, (uint)(0x200 + (i * 4) + 1), (byte)(nextStartAddress >> 8), 0x1);

                        //Write PCM data
                        pcmData.AddRange(tim.AdpcmData);
                        //Add 9 byte pad
                        for (int j = 0; j < pad; j++)
                            pcmData.Add(0x80);  //Adds silent data

                        nextStartAddress = Timbres[i].PcmAddressEnd + 1;
                    }
                    else
                    {
                        MessageBox.Show(Resources.AdpcmBufferExceeded, "Warning", MessageBoxButtons.OK);
                        break;
                    }
                }
            }
            FormMain.OutputLog(this, "Remaining ADPCM RAM capacity is " + (0xFDC0 - nextStartAddress) + " bytes");

            if (pcmData.Count != 0 && CurrentSoundEngine != SoundEngineType.Software)
            {
                //transferPcmOnlyDiffData(pcmData.ToArray(), null);

                FormMain.OutputLog(this, Resources.UpdatingADPCM + " (" + timbre.DisplayName + ")");
                //if (Program.IsWriteLockHeld())
                //{
                try
                {
                    FormMain.AppliactionForm.Enabled = false;
                    using (FormProgress f = new FormProgress())
                    {
                        f.StartPosition = FormStartPosition.CenterScreen;
                        f.Message = Resources.UpdatingADPCM + " (" + timbre.DisplayName + ")";
                        f.Show();
                        transferPcmOnlyDiffData(pcmData.ToArray(), f);
                    }
                }
                finally
                {
                    FormMain.AppliactionForm.Enabled = true;
                }
                //}
                //else
                //{
                //    FormProgress.RunDialog(Resources.UpdatingADPCM,
                //            new Action<FormProgress>((f) =>
                //            {
                //                transferPcmOnlyDiffData(pcmData.ToArray(), f);
                //            }));
                //}
                FormMain.OutputLog(this, string.Format(Resources.AdpcmBufferUsedSPC700, pcmData.Count / 1024));
            }
        }

        private byte[] lastTransferPcmData;

        private void transferPcmOnlyDiffData(byte[] transferData, FormProgress fp)
        {
            for (int i = 0; i < transferData.Length; i++)
            {
                if (i >= lastTransferPcmData.Length || transferData[i] != lastTransferPcmData[i])
                {
                    sendPcmData(transferData, i, fp);
                    lastTransferPcmData = transferData;
                    break;
                }
            }
        }

        private void sendPcmData(byte[] transferData, int i, FormProgress fp)
        {
            int endAddress = transferData.Length;
            if (endAddress > 0xFDC0)
                endAddress = 0xFDC0;

            //Transfer
            int startAddress = (i / 9) * 9;
            int len = endAddress - startAddress;
            int index = 0;
            int percentage = 0;
            int lastPercentage = 0;
            //uint baseAddress = (uint)(0x600 + (f_ECEN * f_EDL * 2048));
            uint baseAddress = (uint)(0x600 + (f_EDL * 2048));

            for (int adr = startAddress; adr < endAddress; adr++)
            {
                SPC700RamWriteData(UnitNumber, (uint)(baseAddress + adr), transferData[adr], 0x1);

                percentage = (100 * index) / len;
                if (percentage != lastPercentage)
                {
                    if (fp != null)
                    {
                        fp.Percentage = percentage;
                        Application.DoEvents();
                    }
                }
                lastPercentage = percentage;
                index++;
            }

            //Zero padding
            for (int j = endAddress; j < endAddress + (9 - (endAddress % 9)); j++)
                SPC700RamWriteData(UnitNumber, (uint)j, 0x00, 0x1);

            // Finish
            // Wait
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
                        if (CurrentSoundEngine == SoundEngineType.GIMIC)
                        {
                            SPC700RegWriteData(UnitNumber, (byte)0x6c, (byte)(0xc0 | ((~ECEN & 1) << 5) | f_NOISE_CLOCK));
                            SPC700RegWriteData(UnitNumber, (byte)0x6c, (byte)(((~ECEN & 1) << 5) | f_NOISE_CLOCK));
                            //GimicManager.Reset(gimicPtr);
                        }
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
        private class SPC700SoundManager : SoundManagerBase
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

            private static SoundList<SPC700Sound> instOnSounds = new SoundList<SPC700Sound>(8);

            private SPC700 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SPC700SoundManager(SPC700 parent) : base(parent)
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

                var bts = parentModule.GetBaseTimbres(note);
                var ids = parentModule.GetBaseTimbreIndexes(note);
                int tindex = 0;
                for (int i = 0; i < bts.Length; i++)
                {
                    SPC700Timbre timbre = (SPC700Timbre)bts[i];

                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    SPC700Sound snd = new SPC700Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot, (byte)ids[i]);
                    instOnSounds.Add(snd);

                    //HACK: store pcm data to local buffer to avoid "thread lock"
                    if (timbre.SoundType == SoundType.INST)
                    {
                        lock (parentModule.tmpPcmDataTable)
                            parentModule.tmpPcmDataTable[ids[i]] = timbre.AdpcmData;
                    }
                    /*
                    else if (timbre.SoundType == SoundType.DRUM)
                    {
                        var pct = (SPC700PcmTimbre)parentModule.DrumSoundTable.PcmTimbres[note.NoteNumber];
                        lock (parentModule.tmpPcmDataTable)
                            parentModule.tmpPcmDataTable[note.NoteNumber + 128] = pct.PcmData;
                    }
                    */

                    FormMain.OutputDebugLog(parentModule, "KeyOn INST ch" + emptySlot + " " + note.ToString());
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
            private (SPC700 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, instOnSounds, note, 8);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 8; i++)
                {
                    byte bitPos = (byte)(1 << i);

                    byte kon = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x4c) & ~bitPos);
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x4c, kon);
                    byte koff = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x5c) | bitPos);
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x5c, koff);
                }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private class SPC700Sound : SoundBase
        {

            private SPC700 parentModule;

            private byte timbreIndex;

            private SPC700Timbre timbre;

            private SoundType lastSoundType;

            private double baseFreq;

            private uint sampleRate;

            private ushort loopPoint;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SPC700Sound(SPC700 parentModule, SPC700SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot, byte timbreIndex) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbreIndex = timbreIndex;
                this.timbre = (SPC700Timbre)timbre;

                lastSoundType = this.timbre.SoundType;
                if (lastSoundType == SoundType.INST)
                {
                    baseFreq = this.timbre.BaseFreqency;
                    sampleRate = this.timbre.SampleRate;
                    loopPoint = this.timbre.LoopPoint;
                }
                /*
                else if (lastSoundType == SoundType.DRUM)
                {
                    var pct = (SPC700PcmTimbre)parentModule.DrumSoundTable.PcmTimbres[noteOnEvent.NoteNumber];
                    baseFreq = pct.BaseFreqency;
                    sampleRate = pct.SampleRate;
                    loopPoint = pct.LoopPoint;
                }
                */
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPanpotUpdated()
            {
                OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();

                uint reg = (uint)(Slot * 16);
                byte bitPos = (byte)(1 << Slot);

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.LMVOL.HasValue)
                        parentModule.LMVOL = gs.LMVOL.Value;
                    if (gs.RMVOL.HasValue)
                        parentModule.RMVOL = gs.RMVOL.Value;
                    if (gs.LEVOL.HasValue)
                        parentModule.LEVOL = gs.LEVOL.Value;
                    if (gs.REVOL.HasValue)
                        parentModule.REVOL = gs.REVOL.Value;
                    if (gs.NOISE_CLOCK.HasValue)
                        parentModule.NOISE_CLOCK = gs.NOISE_CLOCK.Value;
                    //if (gs.ECEN.HasValue)
                    //    parentModule.ECEN = gs.ECEN.Value;
                    if (gs.EFB.HasValue)
                        parentModule.EFB = gs.EFB.Value;
                    if (gs.EDL.HasValue)
                        parentModule.EDL = gs.EDL.Value;
                    if (gs.COEF1.HasValue)
                        parentModule.COEF1 = gs.COEF1.Value;
                    if (gs.COEF2.HasValue)
                        parentModule.COEF2 = gs.COEF2.Value;
                    if (gs.COEF3.HasValue)
                        parentModule.COEF3 = gs.COEF3.Value;
                    if (gs.COEF4.HasValue)
                        parentModule.COEF4 = gs.COEF4.Value;
                    if (gs.COEF5.HasValue)
                        parentModule.COEF5 = gs.COEF5.Value;
                    if (gs.COEF6.HasValue)
                        parentModule.COEF6 = gs.COEF6.Value;
                    if (gs.COEF7.HasValue)
                        parentModule.COEF7 = gs.COEF7.Value;
                    if (gs.COEF8.HasValue)
                        parentModule.COEF8 = gs.COEF8.Value;
                }

                OnVolumeUpdated();
                OnPanpotUpdated();
                OnPitchUpdated();

                if (lastSoundType == SoundType.INST)
                {
                    //SRCN
                    //prognum no
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 4), timbreIndex);
                }
                /*
                else if (lastSoundType == SoundType.DRUM)
                {
                    //prognum no
                    int nn = NoteOnEvent.NoteNumber;
                    SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 4), (byte)(nn + 128));
                }
                */
                //DIR loop
                {
                    //for Real
                    ushort lpos = (ushort)(timbre.PcmAddressStart + loopPoint * 9);
                    parentModule.SPC700RamWriteData(parentModule.UnitNumber, (uint)(0x200 + (timbreIndex * 4) + 2), (byte)(lpos & 0xff), 0x1);
                    parentModule.SPC700RamWriteData(parentModule.UnitNumber, (uint)(0x200 + (timbreIndex * 4) + 3), (byte)(lpos >> 8), 0x1);
                }
                {
                    //for Emu
                    ushort lpos = (ushort)(loopPoint * 9);
                    parentModule.SPC700RamWriteData(parentModule.UnitNumber, (uint)(0x200 + (timbreIndex * 4) + 2), (byte)(lpos & 0xff), 0x2);
                    parentModule.SPC700RamWriteData(parentModule.UnitNumber, (uint)(0x200 + (timbreIndex * 4) + 3), (byte)(lpos >> 8), 0x2);
                }

                //ADSR
                if (timbre.AdsrEnable)
                {
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 5), (byte)(0x80 | (timbre.AdsrDR << 4) | timbre.AdsrAR));
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 6), (byte)((timbre.AdsrSL << 5) | timbre.AdsrSR));
                }
                else
                {
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 5), 0);
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 7), 0x7f);
                }
                //PMON
                //byte pmon = SPC700RegReadData(parentModule.UnitNumber, 0x2d);
                //pmon &= (byte)~bitPos;
                //pmon |= (byte)(timbre.PMON << Slot);
                //SPC700RegWriteData(parentModule.UnitNumber, 0x2d, pmon);
                //NON
                byte non = SPC700RegReadData(parentModule.UnitNumber, 0x3d);
                non &= (byte)~bitPos;
                non |= (byte)(timbre.NON << Slot);
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x3d, non);
                //EON
                byte eon = SPC700RegReadData(parentModule.UnitNumber, 0x4d);
                eon &= (byte)~bitPos;
                eon |= (byte)(timbre.EON << Slot);
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x4d, eon);

                //KON
                byte koff = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x5c) & ~bitPos);
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x5c, koff);
                byte kon = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x4c) | bitPos);
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x4c, kon);
            }


            public override void OnSoundParamsUpdated()
            {

                uint reg = (uint)(Slot * 16);
                byte bitPos = (byte)(1 << Slot);

                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.LMVOL.HasValue)
                        parentModule.LMVOL = gs.LMVOL.Value;
                    if (gs.RMVOL.HasValue)
                        parentModule.RMVOL = gs.RMVOL.Value;
                    if (gs.LEVOL.HasValue)
                        parentModule.LEVOL = gs.LEVOL.Value;
                    if (gs.REVOL.HasValue)
                        parentModule.REVOL = gs.REVOL.Value;
                    if (gs.NOISE_CLOCK.HasValue)
                        parentModule.NOISE_CLOCK = gs.NOISE_CLOCK.Value;
                    //if (gs.ECEN.HasValue)
                    //    parentModule.ECEN = gs.ECEN.Value;
                    if (gs.EFB.HasValue)
                        parentModule.EFB = gs.EFB.Value;
                    if (gs.EDL.HasValue)
                        parentModule.EDL = gs.EDL.Value;
                    if (gs.COEF1.HasValue)
                        parentModule.COEF1 = gs.COEF1.Value;
                    if (gs.COEF2.HasValue)
                        parentModule.COEF2 = gs.COEF2.Value;
                    if (gs.COEF3.HasValue)
                        parentModule.COEF3 = gs.COEF3.Value;
                    if (gs.COEF4.HasValue)
                        parentModule.COEF4 = gs.COEF4.Value;
                    if (gs.COEF5.HasValue)
                        parentModule.COEF5 = gs.COEF5.Value;
                    if (gs.COEF6.HasValue)
                        parentModule.COEF6 = gs.COEF6.Value;
                    if (gs.COEF7.HasValue)
                        parentModule.COEF7 = gs.COEF7.Value;
                    if (gs.COEF8.HasValue)
                        parentModule.COEF8 = gs.COEF8.Value;
                }

                //OnVolumeUpdated();
                //OnPanpotUpdated();
                //OnPitchUpdated();

                //ADSR
                if (timbre.AdsrEnable)
                {
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 5), (byte)(0x80 | (timbre.AdsrDR << 4) | timbre.AdsrAR));
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 6), (byte)((timbre.AdsrSL << 5) | timbre.AdsrSR));
                }
                else
                {
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 5), 0);
                    parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 7), 0x7f);
                }
                //PMON
                //byte pmon = SPC700RegReadData(parentModule.UnitNumber, 0x2d);
                //pmon &= (byte)~bitPos;
                //pmon |= (byte)(timbre.PMON << Slot);
                //SPC700RegWriteData(parentModule.UnitNumber, 0x2d, pmon);
                //NON
                byte non = SPC700RegReadData(parentModule.UnitNumber, 0x3d);
                non &= (byte)~bitPos;
                non |= (byte)(timbre.NON << Slot);
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x3d, non);
                //EON
                byte eon = SPC700RegReadData(parentModule.UnitNumber, 0x4d);
                eon &= (byte)~bitPos;
                eon |= (byte)(timbre.EON << Slot);
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x4d, eon);

                base.OnSoundParamsUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
            {
                uint reg = (uint)(Slot * 16);
                var vol = CalcCurrentVolume();

                int pan = CalcCurrentPanpot();
                /*
                if (lastSoundType == SoundType.DRUM)
                {
                    var pct = parentModule.DrumSoundTable.PcmTimbres[NoteOnEvent.NoteNumber];
                    pan += pct.PanShift;
                    if (pan < 0)
                        pan = 0;
                    else if (pan > 127)
                        pan = 127;
                }
                */

                byte left = (byte)Math.Round(127d * vol * Math.Cos(Math.PI / 2 * (pan / 127d)));
                byte right = (byte)Math.Round(127d * vol * Math.Sin(Math.PI / 2 * (pan / 127d)));
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 0), left);
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 1), right);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
            {
                uint reg = (uint)(Slot * 16);

                uint freq = 0;
                if (lastSoundType == SoundType.INST)
                {
                    freq = (uint)Math.Round((CalcCurrentFrequency() / baseFreq) * 0x1000 * sampleRate / (double)32000);
                }
                /*
                else if (lastSoundType == SoundType.DRUM)
                {
                    double f = MidiManager.CalcCurrentFrequency
                        (MidiManager.CalcNoteNumberFromFrequency(baseFreq) + CalcCurrentPitchDeltaNoteNumber());

                    freq = (uint)Math.Round((f / baseFreq) * 0x1000 * sampleRate / (double)32000);
                }*/

                if (freq > 0x3fff)
                    freq = 0x3fff;

                parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 2), (byte)(freq & 0xff));
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, (byte)(reg + 3), (byte)((freq >> 8) & 0xff));

                if (timbre.NON == 1 && timbre.SetNoiseClk)
                    parentModule.NOISE_CLOCK = (byte)(freq >> 9);

                base.OnPitchUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                byte bitPos = (byte)(1 << Slot);

                byte kon = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x4c) & ~bitPos);
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x4c, kon);
                byte koff = (byte)(SPC700RegReadData(parentModule.UnitNumber, 0x5c) | bitPos);
                parentModule.SPC700RegWriteData(parentModule.UnitNumber, 0x5c, koff);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SPC700Timbre>))]
        [DataContract]
        [InstLock]
        public class SPC700Timbre : TimbreBase
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
            [Description("Sound Type")]
            [DefaultValue(SoundType.INST)]
            [Browsable(false)]
            public SoundType SoundType
            {
                get;
                set;
            }

            [DataMember]
            [Category("Sound")]
            [Description("Set ADPCM base frequency @ 32KHz [Hz]")]
            [DefaultValue(typeof(double), "500")]
            [DoubleSlideParametersAttribute(100, 2000, 1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public double BaseFreqency
            {
                get;
                set;
            } = 500;

            [DataMember]
            [Category("Sound")]
            [Description("Set PCM samplerate [Hz]")]
            [DefaultValue(typeof(uint), "22050")]
            [SlideParametersAttribute(4000, 96000)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public uint SampleRate
            {
                get;
                set;
            } = 22050;

            private ushort f_LoopPoint;

            [DataMember]
            [Category("Sound")]
            [Description("Set data block number of loop point (0 - 7281) (Need to set the loop flag in BRR PCM data)")]
            [DefaultValue(typeof(ushort), "0")]
            [SlideParametersAttribute(0, 7281)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public ushort LoopPoint
            {
                get => f_LoopPoint;
                set
                {
                    f_LoopPoint = value;
                    if (f_LoopPoint > 7281)
                        f_LoopPoint = 7281;
                }
            }

            private byte[] f_AdcmData = new byte[0];

            [TypeConverter(typeof(LoadDataTypeConverter))]
            [Editor(typeof(BrrFileLoaderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DataMember]
            [Category("Sound")]
            [Description("BRR ADPCM Data (MAX 63KB)")]
            [BrrFileLoaderEditor("Audio File(*.brr)|*.brr", 0xFDC0)]
            //[BrrFileLoaderEditor("Audio File(*.brr, *.wav)|*.brr;*.wav", 65536)]
            public byte[] AdpcmData
            {
                get
                {
                    return f_AdcmData;
                }
                set
                {
                    f_AdcmData = value;


                    var inst = (SPC700)this.Instrument;
                    if (inst != null)
                        inst.updatePcmData(this);
                }
            }

            public bool ShouldSerializeAdpcmData()
            {
                return AdpcmData.Length != 0;
            }

            public void ResetAdpcmData()
            {
                AdpcmData = new byte[0];
            }

            private String pcmDataInfo;

            [DataMember]
            [Category("Sound")]
            [Description("PcmData information.\r\n*Warning* May contain privacy information. Check the options dialog.")]
            [ReadOnly(true)]
            public String PcmDataInfo
            {
                get
                {
                    if (Settings.Default.DoNotUsePrivacySettings)
                        return null;
                    return pcmDataInfo;
                }
                set
                {
                    pcmDataInfo = value;
                }
            }

            [DataMember]
            [Browsable(false)]
            public uint PcmAddressStart
            {
                get;
                set;
            }

            [DataMember]
            [Browsable(false)]
            public uint PcmAddressEnd
            {
                get;
                set;
            }

            [DataMember]
            [Category("Sound")]
            [Description("Enable ADSR mode")]
            [DefaultValue(false)]
            public bool AdsrEnable
            {
                get;
                set;
            }

            private byte f_AdsrAR = 15;

            [DataMember]
            [Category("Sound")]
            [Description("Attack rate of ADSR")]
            [DefaultValue((byte)15)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AdsrAR
            {
                get
                {
                    return f_AdsrAR;
                }
                set
                {
                    f_AdsrAR = (byte)(value & 0xf);
                }
            }

            private byte f_AdsrDR = 7;

            [DataMember]
            [Category("Sound")]
            [Description("Decay rate of ADSR")]
            [DefaultValue((byte)7)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AdsrDR
            {
                get
                {
                    return f_AdsrDR;
                }
                set
                {
                    f_AdsrDR = (byte)(value & 7);
                }
            }

            private byte f_AdsrSL = 7;

            [DataMember]
            [Category("Sound")]
            [Description("Sustain level of ADSR")]
            [DefaultValue((byte)7)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AdsrSL
            {
                get
                {
                    return f_AdsrSL;
                }
                set
                {
                    f_AdsrSL = (byte)(value & 7);
                }
            }

            private byte f_AdsrSR = 15;

            [DataMember]
            [Category("Sound")]
            [Description("Sustain rate of ADSR")]
            [DefaultValue((byte)15)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AdsrSR
            {
                get
                {
                    return f_AdsrSR;
                }
                set
                {
                    f_AdsrSR = (byte)(value & 31);
                }
            }

            //private byte f_PMON;

            //[DataMember]
            //[Category("Sound")]
            //[Description("Pitch modulation enable")]
            //[DefaultValue((byte)0)]
            //[SlideParametersAttribute(0, 1)]
            //[EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            //public byte PMON
            //{
            //    get
            //    {
            //        return f_PMON;
            //    }
            //    set
            //    {
            //        f_PMON = (byte)(value & 1);
            //    }
            //}

            private byte f_NON;

            [DataMember]
            [Category("Sound")]
            [Description("Noise enable")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte NON
            {
                get
                {
                    return f_NON;
                }
                set
                {
                    f_NON = (byte)(value & 1);
                }
            }

            [DataMember]
            [Category("Sound")]
            [Description("Set NOISE_CLOCK by Note No.")]
            [DefaultValue(false)]
            public bool SetNoiseClk
            {
                get;
                set;
            }

            private byte f_EON;

            [DataMember]
            [Category("Sound")]
            [Description("Echo enable")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EON
            {
                get
                {
                    return f_EON;
                }
                set
                {
                    f_EON = (byte)(value & 1);
                }
            }


            [DataMember]
            [Category("Chip")]
            [Description("Global Settings")]
            public SPC700GlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new SPC700GlobalSettings());
            }

            /// <summary>
            /// 
            /// </summary>
            public SPC700Timbre()
            {
                GlobalSettings = new SPC700GlobalSettings();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="serializeData"></param>
            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SPC700Timbre>(serializeData);
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
        public enum SoundType
        {
            INST,
            DRUM,
        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SPC700GlobalSettings>))]
        [DataContract]
        [InstLock]
        public class SPC700GlobalSettings : ContextBoundObject
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

            private byte? f_LMVOL;

            [DataMember]
            [Category("Chip")]
            [Description("Set Left Output Main Volume")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LMVOL
            {
                get
                {
                    return f_LMVOL;
                }
                set
                {
                    f_LMVOL = value;
                }
            }

            private byte? f_RMVOL;

            [DataMember]
            [Category("Chip")]
            [Description("Set Right Output Main Volume")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? RMVOL
            {
                get
                {
                    return f_RMVOL;
                }
                set
                {
                    f_RMVOL = value;
                }
            }

            private sbyte? f_LEVOL;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("Set Left Output Echo Volume")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 128)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? LEVOL
            {
                get
                {
                    return f_LEVOL;
                }
                set
                {
                    f_LEVOL = value;
                }
            }

            private sbyte? f_REVOL;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("Set Right Output Echo Volume")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 128)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? REVOL
            {
                get
                {
                    return f_REVOL;
                }
                set
                {
                    f_REVOL = value;
                }
            }

            private byte? f_NOISE_CLOCK;

            [DataMember]
            [Category("Chip")]
            [Description("Designates the frequency for the white noise.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 31)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? NOISE_CLOCK
            {
                get
                {
                    return f_NOISE_CLOCK;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 31);
                    f_NOISE_CLOCK = v;
                }
            }

            private byte? f_ECEN;

            [Browsable(false)]
            //[DataMember]
            [Category("Chip(Filter)")]
            [Description("Echo enable")]
            [DefaultValue(null)]
            //[SlideParametersAttribute(0, 1)]
            //[EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? ECEN
            {
                get
                {
                    return f_ECEN;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_ECEN = v;
                }
            }


            private sbyte? f_EFB;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("Echo Feedback")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? EFB
            {
                get
                {
                    return f_EFB;
                }
                set
                {
                    f_EFB = value;
                }
            }

            private byte? f_EDL;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("EDL specifies the delay between the main sound and the echoed sound. The delay is calculated as EDL * 16ms.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? EDL
            {
                get
                {
                    return f_EDL;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 15);
                    f_EDL = v;
                }
            }

            private sbyte? f_COEF1;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF1
            {
                get
                {
                    return f_COEF1;
                }
                set
                {
                    f_COEF1 = value;
                }
            }

            private sbyte? f_COEF2;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF2
            {
                get
                {
                    return f_COEF2;
                }
                set
                {
                    f_COEF2 = value;
                }
            }

            private sbyte? f_COEF3;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF3
            {
                get
                {
                    return f_COEF3;
                }
                set
                {
                    f_COEF3 = value;
                }
            }

            private sbyte? f_COEF4;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF4
            {
                get
                {
                    return f_COEF4;
                }
                set
                {
                    f_COEF4 = value;
                }
            }

            private sbyte? f_COEF5;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF5
            {
                get
                {
                    return f_COEF5;
                }
                set
                {
                    f_COEF5 = value;
                }
            }


            private sbyte? f_COEF6;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF6
            {
                get
                {
                    return f_COEF6;
                }
                set
                {
                    f_COEF6 = value;
                }
            }

            private sbyte? f_COEF7;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF7
            {
                get
                {
                    return f_COEF7;
                }
                set
                {
                    f_COEF7 = value;
                }
            }

            private sbyte? f_COEF8;

            [DataMember]
            [Category("Chip(Filter)")]
            [Description("COEF are used by the 8-tap FIR filter.")]
            [DefaultValue(null)]
            [SlideParametersAttribute(-128, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public sbyte? COEF8
            {
                get
                {
                    return f_COEF8;
                }
                set
                {
                    f_COEF8 = value;
                }
            }


        }

        #region MENU

        private ToolStripMenuItem readSpcFileForTimbre;

        private ToolStripMenuItem readSpcFileForDrumTimbre;

        private ToolStripMenuItem readSoundFontForTimbre;

        private ToolStripMenuItem readSoundFontForDrumTimbre;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override IEnumerable<ToolStripMenuItem> GetInstrumentMenus()
        {
            List<ToolStripMenuItem> menus = new System.Collections.Generic.List<ToolStripMenuItem>(base.GetInstrumentMenus());

            menus.AddRange(new ToolStripMenuItem[] {
                readSpcFileForTimbre,
                readSpcFileForDrumTimbre,
                readSoundFontForTimbre,
                readSoundFontForDrumTimbre
            });

            return menus.ToArray();
        }

        private System.Windows.Forms.OpenFileDialog openFileDialog;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadSpcFileForTimbre_Click(object sender, EventArgs e)
        {
            try
            {
                int offset = 0;
                using (openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.SupportMultiDottedExtensions = true;
                    openFileDialog.Title = "Select a SPC file";
                    openFileDialog.Filter = "SPC File(*.spc)|*.spc";

                    var fr = openFileDialog.ShowDialog(null);
                    if (fr != DialogResult.OK)
                        return;

                    loadSpcFile(openFileDialog.FileName, offset, false);
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadSpcFileForDrumTimbre_Click(object sender, EventArgs e)
        {
            try
            {
                int offset = 128;
                using (openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.SupportMultiDottedExtensions = true;
                    openFileDialog.Title = "Select a SPC file";
                    openFileDialog.Filter = "SPC File(*.spc)|*.spc";

                    var fr = openFileDialog.ShowDialog(null);
                    if (fr != DialogResult.OK)
                        return;

                    loadSpcFile(openFileDialog.FileName, offset, false);
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadSoundFontForTimbre_Click(object sender, EventArgs e)
        {
            try
            {
                int offset = 0;
                using (openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.SupportMultiDottedExtensions = true;
                    openFileDialog.Title = "Select a SoundFont v2.0 file";
                    openFileDialog.Filter = "SoundFont v2.0 File(*.sf2)|*.sf2";

                    var fr = openFileDialog.ShowDialog(null);
                    if (fr != DialogResult.OK)
                        return;

                    loadPcm(openFileDialog.FileName, offset, false);
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadSoundFontForDrumTimbre_Click(object sender, EventArgs e)
        {
            try
            {
                int offset = 128;
                using (openFileDialog = new System.Windows.Forms.OpenFileDialog())
                {
                    openFileDialog.SupportMultiDottedExtensions = true;
                    openFileDialog.Title = "Select a SoundFont v2.0 file";
                    openFileDialog.Filter = "SoundFont v2.0 File(*.sf2)|*.sf2";

                    var fr = openFileDialog.ShowDialog(null);
                    if (fr != DialogResult.OK)
                        return;

                    loadPcm(openFileDialog.FileName, offset, true);
                }
                for (int i = 0; i < 128; i++)
                {
                    var tim = (SPC700Timbre)Timbres[i + 128];

                    DrumTimbres[i].TimbreNumber = (ProgramAssignmentNumber)(i + 128);
                    DrumTimbres[i].BaseNote =
                        (NoteNames)(byte)Math.Round(MidiManager.CalcNoteNumberFromFrequency(tim.BaseFreqency));
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

        private short[] resampleLoop16(SF2SampleHeader s, short[] osample, double obaseFreq, ref double tbaseFreq, ref uint tend, ref uint tloopStart, ref uint tloopLen)
        {
            //uint ostart = 0;
            uint oend = s.End - s.Start;
            if (s.LoopEnd < s.End && s.LoopStart < s.LoopEnd)
                oend = s.LoopEnd - s.Start;

            uint olen = oend + 1;
            uint oloopStart = s.LoopStart - s.Start;
            uint oloopLen = olen - oloopStart;

            uint tlen = olen;
            tloopStart = oloopStart;
            tloopLen = oloopLen;
            tend = oend;
            tbaseFreq = obaseFreq;
            double trate = 1;
            if (oloopLen % 16 != 0)
            {
                tloopLen = ((oloopLen / 16) + 1) * 16;
                trate = (double)oloopLen / (double)tloopLen;
                tbaseFreq = obaseFreq / trate;

                tloopLen = (uint)Math.Ceiling(oloopLen / trate);
                tend = (uint)Math.Ceiling(oend / trate);
                tlen = tend + 1;
                tloopStart = tend - tloopLen + 1;
            }

            if (obaseFreq != tbaseFreq)
            {
                //resample

                IntPtr op = Marshal.AllocHGlobal((int)(olen * sizeof(short)));
                IntPtr tp = Marshal.AllocHGlobal((int)(tlen * sizeof(short)));
                MarshalCopy(osample, op, 0, olen);

                spc_resample(obaseFreq, tbaseFreq, op, olen, tp, tlen);

                short[] tsamples = new short[tlen];
                MarshalCopy(tp, tsamples, 0, tlen);

                Marshal.FreeHGlobal(op);
                Marshal.FreeHGlobal(tp);
                return tsamples;
            }
            else
            {
                return osample;
            }
        }

        public static void MarshalCopy(IntPtr source, short[] destination, uint startIndex, uint length)
        {
            unsafe
            {
                var sourcePtr = (short*)source;
                for (var i = startIndex; i < startIndex + length; i++)
                {
                    destination[i] = *sourcePtr;
                    sourcePtr++;
                }
            }
        }

        public static void MarshalCopy(short[] source, IntPtr destination, uint startIndex, uint length)
        {
            unsafe
            {
                var destPtr = (short*)destination;
                for (var i = startIndex; i < startIndex + length; i++)
                {
                    *destPtr = source[i];
                    destPtr++;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        private void loadSpcFile(string fileName, int offset, bool drum)
        {
            IntPtr[] sampleBuffer = new IntPtr[256];
            uint[] sampleSizes = new uint[256];
            try
            {
                ExtractSpcFile(fileName, sampleBuffer, sampleSizes);

                int dtn = 0;
                int num = 0;
                for (int i = 0; i < 256; i++)
                {
                    if (sampleBuffer[i] != IntPtr.Zero && sampleSizes[i] != 0)
                    {
                        //copy
                        byte[] managedArray = new byte[sampleSizes[i]];
                        Marshal.Copy(sampleBuffer[i], managedArray, 0, managedArray.Length);
                        List<byte> buf = new List<byte>(managedArray);

                        var tim = new SPC700Timbre();
                        //loop
                        tim.LoopPoint = (ushort)((buf[0] | (buf[1] << 8)) / 9);
                        buf.RemoveRange(0, 2);
                        //pcm data
                        tim.AdpcmData = buf.ToArray();
                        Timbres[i] = tim;

                        tim.PcmDataInfo = fileName;
                        tim.TimbreName = System.IO.Path.GetFileNameWithoutExtension(fileName);

                        //Drum
                        if (drum)
                        {
                            DrumTimbres[dtn].TimbreNumber = (ProgramAssignmentNumber)(dtn + offset);
                            DrumTimbres[dtn].TimbreName = System.IO.Path.GetFileNameWithoutExtension(fileName);
                            dtn++;
                            if (dtn == 128)
                                break;
                        }
                        num++;
                    }
                }
                MessageBox.Show(string.Format(Resources.TimbreLoaded, num));
            }
            finally
            {
                //free
                for (int i = 0; i < 256; i++)
                {
                    if (sampleBuffer[i] != IntPtr.Zero)
                        FreeExtractedSpcSampleBuffer(sampleBuffer[i]);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        private void loadPcm(string fileName, int offset, bool drum)
        {
            var sf2 = new SF2(fileName);

            var spl = sf2.SoundChunk.SMPLSubChunk.Samples;
            int tn = 0;
            bool warningAlign = false;
            int num = 0;
            foreach (var s in sf2.HydraChunk.SHDRSubChunk.Samples)
            {
                if (s.SampleType == SF2SampleLink.MonoSample ||
                    s.SampleType == SF2SampleLink.LeftSample)
                {
                    var tim = new SPC700Timbre();
                    uint brrLoopStart;

                    double baseFreq = 440.0 * Math.Pow(2.0, (((double)s.OriginalKey - 69.0) / 12.0) + (double)(s.PitchCorrection / 100));
                    tim.SampleRate = s.SampleRate;

                    uint start = 0;
                    uint end = s.End - s.Start;
                    if (s.LoopEnd < end && s.LoopStart < s.LoopEnd)
                        end = s.LoopEnd;

                    uint olen = end - start + 1;
                    short[] samples = new short[olen];
                    Array.Copy(spl, s.Start, samples, 0, olen);
                    uint loopStart = 0;
                    uint loopLen = 0;

                    if (s.LoopStart < s.LoopEnd)
                    {
                        loopStart = s.LoopStart - s.Start;
                        loopLen = olen - loopStart;

                        if (loopLen % 16 != 0)
                            warningAlign = true;

                        samples = resampleLoop16(s, samples, baseFreq, ref baseFreq, ref end, ref loopStart, ref loopLen);
                    }

                    tim.BaseFreqency = baseFreq;

                    //offset to 16
                    if (samples.Length % 16 != 0)
                    {
                        int sl = samples.Length;
                        List<short> tsmpl = new List<short>();
                        tsmpl.AddRange(samples);
                        tsmpl.InsertRange(0, new short[16 - (sl % 16)]);
                        samples = tsmpl.ToArray();
                        loopStart += 16 - (uint)(sl % 16);
                    }

                    //For calc avarage PCM data on loop point to avoid glitch/noise sounsing
                    if (s.LoopStart < s.LoopEnd)
                    {
                        short[] avgData = new short[16];
                        if (loopStart >= 16)
                        {
                            for (int i = 0; i < 16; i++)
                                avgData[i] = (short)(((int)samples[loopStart - 16 + i] + (int)samples[samples.Length - 16 + i]) / 2);
                            for (int i = 0; i < 16; i++)
                            {
                                samples[loopStart - 16 + i] = avgData[i];
                                samples[samples.Length - 16 + i] = avgData[i];
                            }
                        }
                    }

                    var result = Brr.BrrEncoder.ConvertRawWave(samples, false, s.LoopStart < s.LoopEnd, loopStart, out brrLoopStart);
                    brrLoopStart /= 9;

                    tim.AdpcmData = result;
                    tim.LoopPoint = (ushort)brrLoopStart;

                    var nidx = s.SampleName.IndexOf('\0');
                    if (nidx >= 0)
                        tim.TimbreName = s.SampleName.Substring(0, nidx);
                    else
                        tim.TimbreName = s.SampleName;

                    if (s.LoopStart < s.LoopEnd)
                    {
                        tim.SDS.ADSR.Enable = true;
                        tim.SDS.ADSR.DR = 80;
                        tim.SDS.ADSR.SL = 127;
                    }
                    else
                    {
                        tim.SDS.ADSR.Enable = false;
                    }
                    if (drum)
                    {
                        DrumTimbres[tn].TimbreNumber = (ProgramAssignmentNumber)(tn + offset);
                        DrumTimbres[tn].BaseNote =
                            (NoteNames)(byte)Math.Round(MidiManager.CalcNoteNumberFromFrequency(tim.BaseFreqency));
                    }
                    Timbres[tn + offset] = tim;
                    num++;

                    tn++;
                    if (tn == 128)
                        break;
                }
            }
            if (warningAlign)
            {
                MessageBox.Show(Resources.WanrSPC700SampleLength, "Warning", MessageBoxButtons.OK);
            }
            MessageBox.Show(string.Format(Resources.TimbreLoaded, num));
        }

        #endregion
    }

}
