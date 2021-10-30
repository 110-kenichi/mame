// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
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
using zanac.MAmidiMEmo.Scci;
using zanac.MAmidiMEmo.Util.Syx;

//http://sr4.sakura.ne.jp/fmsound/opz.html
//https://sites.google.com/site/undocumentedsoundchips/yamaha/ym2414

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class YM2414 : InstrumentBase
    {

        public override string Name => "YM2414";

        public override string Group => "FM";

        public override InstrumentType InstrumentType => InstrumentType.YM2414;

        [Browsable(false)]
        public override string ImageKey => "YM2414";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "ymfm_opz_";

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
                return 29;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum MasterClockType : uint
        {
            Default = 3579545,
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
                    YM2414WriteData(UnitNumber, 0x18, 0, 0, LFRQ);
                }
            }
        }

        private byte f_LFOF;

        /// <summary>
        /// Select AMD or PMD(0:AMD 1:PMD)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Select AMD or PMD (0:AMD 1:PMD)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFOF
        {
            get
            {
                return f_LFOF;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_LFOF != v)
                {
                    f_LFOF = v;
                    YM2414WriteData(UnitNumber, 0x19, 0, 0, (byte)(LFOF << 7 | LFOD));
                }
            }
        }

        private byte f_LFOD;

        /// <summary>
        /// LFO Depth(0-127)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO Depth (0-127)")]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
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
                    YM2414WriteData(UnitNumber, 0x19, 0, 0, (byte)(LFOF << 7 | LFOD));
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
                    YM2414WriteData(UnitNumber, 0x1B, 0, 0, (byte)(SYNC2 << 5 | SYNC << 4 | LFOW2 << 2 | LFOW));
                }
            }
        }

        private byte f_SYNC;

        /// <summary>
        /// LFO SYNC Enable (0:Disable 1:Enable)
        /// </summary>
        [Browsable(false)]
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO SYNC Enable (0:Disable 1:Enable)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte SYNC
        {
            get
            {
                return f_SYNC;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_SYNC != v)
                {
                    f_SYNC = v;
                    YM2414WriteData(UnitNumber, 0x1B, 0, 0, (byte)(SYNC2 << 5 | SYNC << 4 | LFOW2 << 2 | LFOW));
                }
            }
        }

        private byte f_LFRQ2;

        /// <summary>
        /// LFRQ2 (0-255)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO2 Freq (0-255)")]
        [SlideParametersAttribute(0, 255)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFRQ2
        {
            get
            {
                return f_LFRQ2;
            }
            set
            {
                if (f_LFRQ2 != value)
                {
                    f_LFRQ2 = value;
                    YM2414WriteData(UnitNumber, 0x16, 0, 0, LFRQ2);
                }
            }
        }

        private byte f_LFOF2;

        /// <summary>
        /// Select AMD2 or PMD2(0:AMD2 1:PMD2)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("Select AMD2 or PMD2 (0:AMD2 1:PMD2)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFOF2
        {
            get
            {
                return f_LFOF2;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_LFOF2 != v)
                {
                    f_LFOF2 = v;
                    YM2414WriteData(UnitNumber, 0x17, 0, 0, (byte)(LFOF2 << 7 | LFOD2));
                }
            }
        }

        private byte f_LFOD2;


        /// <summary>
        /// LFO2 Depth(0-127)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO2 Depth (0-127)")]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFOD2
        {
            get
            {
                return f_LFOD2;
            }
            set
            {
                byte v = (byte)(value & 127);
                if (f_LFOD2 != v)
                {
                    f_LFOD2 = v;
                    YM2414WriteData(UnitNumber, 0x17, 0, 0, (byte)(LFOF2 << 7 | LFOD2));
                }
            }
        }

        private byte f_LFOW2;


        /// <summary>
        /// LFO2 Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)
        /// </summary>
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO2 Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)")]
        [SlideParametersAttribute(0, 3)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte LFOW2
        {
            get
            {
                return f_LFOW2;
            }
            set
            {
                byte v = (byte)(value & 3);
                if (f_LFOW2 != v)
                {
                    f_LFOW2 = v;
                    YM2414WriteData(UnitNumber, 0x1B, 0, 0, (byte)(SYNC2 << 5 | SYNC << 4 | LFOW2 << 2 | LFOW));
                }
            }
        }

        private byte f_SYNC2;

        /// <summary>
        /// Noise Enable (0:Disable 1:Enable)
        /// </summary>
        [Browsable(false)]
        [DataMember]
        [Category("Chip(Global)")]
        [Description("LFO SYNC Enable (0:Disable 1:Enable)")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((byte)0)]
        public byte SYNC2
        {
            get
            {
                return f_SYNC2;
            }
            set
            {
                byte v = (byte)(value & 1);
                if (f_SYNC2 != v)
                {
                    f_SYNC2 = v;
                    YM2414WriteData(UnitNumber, 0x1B, 0, 0, (byte)(SYNC2 << 5 | SYNC << 4 | LFOW2 << 2 | LFOW));
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
                    YM2414WriteData(UnitNumber, 0x0f, 0, 0, (byte)(NE << 7 | NFRQ));
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
                    YM2414WriteData(UnitNumber, 0x0f, 0, 0, (byte)(NE << 7 | NFRQ));
                }
            }
        }

        private object lfdLock = new object();

        private byte f_LFD;

        private double lfdStep;

        private double lfdCurrent;

        private double lfdTarget;

        private YM2414Sound lfdSound;

        /// <summary>
        /// LFO Delay
        /// </summary>
        [DataMember]
        [Category("Chip(Global(Driver))")]
        [Description("LFO Delay (0-99)")]
        [DefaultValue((byte)0)]
        [SlideParametersAttribute(0, 99)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte LFD
        {
            get
            {
                return f_LFD;
            }
            set
            {
                if (0 <= value & value <= 99)
                    f_LFD = value;
            }
        }

        private byte f_LFD2;

        private double lfd2Step;

        private double lfd2Current;

        private double lfd2Target;

        private YM2414Sound lfd2Sound;

        /// <summary>
        /// LFO Delay 2
        /// </summary>
        [DataMember]
        [Category("Chip(Global(Driver))")]
        [Description("LFO Delay 2 (0-99)")]
        [DefaultValue((byte)0)]
        [SlideParametersAttribute(0, 99)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte LFD2
        {
            get
            {
                return f_LFD2;
            }
            set
            {
                if (0 <= value & value <= 99)
                    f_LFD2 = value;
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
        [EditorAttribute(typeof(YM2414UITypeEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableCollectionConverter))]
        public YM2414Timbre[] Timbres
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
                using (var obj = JsonConvert.DeserializeObject<YM2414>(serializeData))
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
        private delegate void delegate_YM2414_write(uint unitNumber, uint address, byte data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_YM2414_write YM2414_write
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        private void YM2414WriteData(uint unitNumber, byte address, int op, int slot, byte data)
        {
            YM2414WriteData(unitNumber, address, op, slot, data, false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void YM2414WriteData(uint unitNumber, byte address, int op, int slot, byte data, bool useCache)
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
                DeferredWriteData(YM2414_write, unitNumber, (uint)0, adr);
                DeferredWriteData(YM2414_write, unitNumber, (uint)1, data);
            }));
            //try
            //{
            //    Program.SoundUpdating();

            //    YM2414_write(unitNumber, 0, adr);
            //    YM2414_write(unitNumber, 1, data);
            //}
            //finally
            //{
            //    Program.SoundUpdated();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        private uint? YM2414ReadData(byte address, int op, int slot)
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

            return GetCachedWrittenData(adr);
        }


        /// <summary>
        /// 
        /// </summary>
        static YM2414()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("ymfm_opz_write");
            if (funcPtr != IntPtr.Zero)
            {
                YM2414_write = (delegate_YM2414_write)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_YM2414_write));
            }
        }

        private YM2414SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public YM2414(uint unitNumber) : base(unitNumber)
        {
            SetDevicePassThru(false);

            MasterClock = (uint)MasterClockType.Default;

            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new YM2414Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new YM2414Timbre();

            setPresetInstruments();

            this.soundManager = new YM2414SoundManager(this);
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

        private static void saveMami(string fileName)
        {
            var es = Program.SaveEnvironmentSettings();
            string data = JsonConvert.SerializeObject(es, Formatting.Indented, Program.JsonAutoSettings);
            File.WriteAllText(fileName, StringCompressionUtility.Compress(data));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected void ApplyTone(int offset, FM_SoundConvertor.Tone[] tones)
        {
            for (int idx = 0; idx < 128; idx++)
            {
                //YM2414Timbre tim = (YM2414Timbre)Timbres[offset + idx];
                YM2414Timbre tim = new YM2414Timbre();
                if (idx < tones.Length)
                {
                    FM_SoundConvertor.Tone tone = tones[idx];

                    tim.ALG = (byte)tone.AL;
                    tim.FB = (byte)tone.FB;
                    tim.AMS = (byte)tone.AMS;
                    tim.PMS = (byte)tone.PMS;
                    tim.AMS2 = (byte)tone.AMS2;
                    tim.PMS2 = (byte)tone.PMS2;

                    tim.GlobalSettings.NE = (byte?)tone.NE;
                    tim.GlobalSettings.NFRQ = (byte?)tone.NF;

                    tim.GlobalSettings.LFRQ = (byte?)tone.LFRQ;
                    tim.GlobalSettings.LFRQ2 = (byte?)tone.LFRQ2;
                    tim.GlobalSettings.LFOF = (byte?)tone.LFOF;
                    tim.GlobalSettings.LFOD = (byte?)tone.LFOD;
                    tim.GlobalSettings.LFOF2 = (byte?)tone.LFOF2;
                    tim.GlobalSettings.LFOD2 = (byte?)tone.LFOD2;
                    tim.GlobalSettings.LFOW = (byte?)tone.LFOW;
                    tim.GlobalSettings.LFOW2 = (byte?)tone.LFOW2;

                    tim.GlobalSettings.LFD = (byte?)tone.LFD;
                    tim.GlobalSettings.LFD2 = (byte?)tone.LFD2;

                    tim.GlobalSettings.SYNC = (byte?)tone.SY;
                    tim.GlobalSettings.SYNC2 = (byte?)tone.SY2;

                    if (tim.GlobalSettings.NE > 0 ||
                        tim.GlobalSettings.LFRQ > 0 ||
                        tim.GlobalSettings.LFRQ2 > 0 ||
                        tim.GlobalSettings.LFOW > 0 ||
                        tim.GlobalSettings.LFOW2 > 0 ||
                        tim.GlobalSettings.LFOD > 0 ||
                        tim.GlobalSettings.LFOD2 > 0
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
                        tim.Ops[i].FINE = (byte)tone.aOp[i].FINE;
                        tim.Ops[i].FIX = (byte)tone.aOp[i].FIX;
                        tim.Ops[i].FIXR = (byte)tone.aOp[i].FIXR;
                        tim.Ops[i].FIXF = (byte)tone.aOp[i].FIXF;
                        tim.Ops[i].OSCW = (byte)tone.aOp[i].OSCW;
                        tim.Ops[i].EGSF = (byte)tone.aOp[i].EGSF;
                        tim.Ops[i].REV = (byte)tone.aOp[i].REV;
                        tim.Ops[i].LS = (byte)tone.aOp[i].LS;
                        tim.Ops[i].KVS = (byte)tone.aOp[i].KVS;
                    }
                    tim.TimbreName = tone.Name;
                }
                Timbres[offset + idx] = tim;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="midiEvent"></param>
        protected override void OnNoteOnEvent(TaggedNoteOnEvent midiEvent)
        {
            soundManager.ProcessKeyOn(midiEvent);
            //createSamples();
        }

        private void createSamples()
        {
            if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
            {
                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z_G-StormPatches_01\TX81Z_G-StormPatches_01.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z_G-StormPatches_02\TX81Z_G-StormPatches_02.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_G-Storm.MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z Presets\tx81z_1.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z Presets\tx81z_2.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z Presets\tx81z_3.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z Presets\tx81z_4.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Preset.MAmi");

                //*
                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z01a.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z01g.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z01m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z01n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(01).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z02m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z02n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z03m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z03n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(02-03).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z04m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z04n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z05m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z05n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(04-05).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z06m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z06n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z07m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z07n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(06-07).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z08m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z08n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z09m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z09n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(08-09).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z10m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z10n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z11m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z11n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(10-11).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z12m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z12n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z13m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z13n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(12-13).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z14m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z14n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z15m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z15n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(14-15).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z16m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z16n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z17m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z17n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(16-17).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z18m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z18n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z19m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z19n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(18-19).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z20m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z20n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z21m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z21n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(20-21).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z22m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z22n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z23m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z23n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(22-23).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z24m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z24n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z25m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z25n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(24-25).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z26m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z26n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z27m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z27n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(26-27).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z28m.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z28n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z29m.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z29n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(28-29).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z30n.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z31n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z32n.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z33n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(30-33).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z34n.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z35n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z36n.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z37n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(34-37).MAmi");

                ApplyTone(0, SyxReaderTX81Z.LoadSyx(0, @"D:\Downloads\TX81Z 41 Banks\Tx81z38n.syx"));
                ApplyTone(32, SyxReaderTX81Z.LoadSyx(32, @"D:\Downloads\TX81Z 41 Banks\Tx81z39n.syx"));
                ApplyTone(64, SyxReaderTX81Z.LoadSyx(64, @"D:\Downloads\TX81Z 41 Banks\Tx81z40n.syx"));
                ApplyTone(96, SyxReaderTX81Z.LoadSyx(96, @"D:\Downloads\TX81Z 41 Banks\Tx81z41n.syx"));
                saveMami(@".\Samples\YM2414_TX81Z_Bank(38-41).MAmi");
                //*/
            }
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
        private class YM2414SoundManager : SoundManagerBase
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

            private static SoundList<YM2414Sound> fmOnSounds = new SoundList<YM2414Sound>(8);

            private YM2414 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public YM2414SoundManager(YM2414 parent) : base(parent)
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
                foreach (YM2414Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    YM2414Sound snd = new YM2414Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
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
            private (YM2414 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, fmOnSounds, note, 8);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);

                for (int i = 0; i < 8; i++)
                {
                    parentModule.YM2414WriteData(parentModule.UnitNumber, 0x08, 0, 0, (byte)(0x00 | i));

                    for (int op = 0; op < 4; op++)
                        parentModule.YM2414WriteData(parentModule.UnitNumber, 0x60, op, i, 127);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class YM2414Sound : SoundBase
        {
            private YM2414 parentModule;

            private uint unitNumber;

            private YM2414Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public YM2414Sound(YM2414 parentModule, YM2414SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (YM2414Timbre)timbre;

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
                    if (gs.LFOD.HasValue)
                        parentModule.LFOD = gs.LFOD.Value;
                    if (gs.LFOF.HasValue)
                        parentModule.LFOF = gs.LFOF.Value;
                    if (gs.LFOD2.HasValue)
                        parentModule.LFOD2 = gs.LFOD2.Value;
                    if (gs.LFOF2.HasValue)
                        parentModule.LFOF2 = gs.LFOF2.Value;

                    if (gs.LFOW.HasValue)
                        parentModule.LFOW = gs.LFOW.Value;
                    if (gs.LFOW2.HasValue)
                        parentModule.LFOW2 = gs.LFOW2.Value;

                    if (gs.LFRQ.HasValue)
                        parentModule.LFRQ = gs.LFRQ.Value;
                    if (gs.LFRQ2.HasValue)
                        parentModule.LFRQ2 = gs.LFRQ2.Value;

                    if (gs.SYNC.HasValue)
                        parentModule.SYNC = gs.SYNC.Value;
                    if (gs.SYNC2.HasValue)
                        parentModule.SYNC2 = gs.SYNC2.Value;

                    if (gs.NE.HasValue)
                        parentModule.NE = gs.NE.Value;
                    if (gs.NFRQ.HasValue)
                        parentModule.NFRQ = gs.NFRQ.Value;

                    if (gs.LFD.HasValue)
                        parentModule.LFD = gs.LFD.Value;
                    if (gs.LFD2.HasValue)
                        parentModule.LFD2 = gs.LFD2.Value;
                }

                //
                SetTimbre();
                //Freq
                OnPitchUpdated();
                //Volume
                OnVolumeUpdated();
                //On
                byte op = (byte)(timbre.Ops[0].Enable << 3 | timbre.Ops[2].Enable << 4 | timbre.Ops[1].Enable << 5 | timbre.Ops[3].Enable << 6);
                parentModule.YM2414WriteData(unitNumber, 0x08, 0, 0, (byte)(op | Slot));

                if (parentModule.LFD != 0 || parentModule.LFD2 != 0)
                {
                    lock (parentModule.lfdLock)
                    {
                        if (parentModule.LFD != 0)
                        {
                            parentModule.lfdStep = parentModule.LFD / lfoDelayTime[parentModule.LFD];
                            parentModule.lfdCurrent = 0;
                            parentModule.lfdTarget = parentModule.LFD;
                            parentModule.lfdSound = this;
                        }
                        if (parentModule.LFD2 != 0)
                        {
                            parentModule.lfd2Step = parentModule.LFD2 / lfoDelayTime[parentModule.LFD2];
                            parentModule.lfd2Current = 0;
                            parentModule.lfd2Target = parentModule.LFD2;
                            parentModule.lfd2Sound = this;
                        }
                    }
                    HighPrecisionTimer.SetPeriodicCallback(new Func<object, double>(processLfoDelay), 20, this, true);
                }
            }

            private double processLfoDelay(object state)
            {
                bool processed = false;

                lock (parentModule.lfdLock)
                {
                    if (IsDisposed || IsSoundOff)
                    {
                        if (parentModule.lfdSound == this)
                            parentModule.lfdSound = null;
                        if (parentModule.lfd2Sound == this)
                            parentModule.lfd2Sound = null;
                        return -1;
                    }

                    if (parentModule.lfdSound == this)
                    {
                        processed = true;

                        parentModule.lfdCurrent += parentModule.lfdStep;
                        if (parentModule.lfdCurrent > 127)
                            parentModule.lfdCurrent = 127;
                        parentModule.LFOD = (byte)parentModule.lfdCurrent;
                        if (parentModule.lfdCurrent >= parentModule.lfdTarget)
                            parentModule.lfdSound = null;
                    }
                    if (parentModule.lfd2Sound == this)
                    {
                        processed = true;

                        parentModule.lfd2Current += parentModule.lfd2Step;
                        if (parentModule.lfd2Current > 127)
                            parentModule.lfd2Current = 127;
                        parentModule.LFOD2 = (byte)parentModule.lfd2Current;
                        if (parentModule.lfd2Current >= parentModule.lfd2Target)
                            parentModule.lfd2Sound = null;
                    }
                }

                if (processed)
                    return 20;
                else
                    return -1;
            }

            private static double[] lfoDelayTime = new double[]{
                 0,    15,    16,    16,    17,    18,    18,    19,    19,    20,
                21,    22,    23,    24,    25,    26,    27,    29,    31,    32,
                34,    35,    36,    37,    38,    40,    41,    43,    45,    47,
                49,    52,    54,    57,    61,    64,    67,    69,    71,    74,
                76,    79,    82,    86,    90,    94,    98,    103,    108,    114,
                121,    128,    133,    137,    142,    147,    152,    158,    164,    171,
                179,    187,    196,    205,    216,    228,    241,    256,    265,    273,
                283,    293,    304,    315,    328,    341,    356,    372,    390,    409,
                431,    455,    481,    511,    528,    546,    564,    585,    606,    630,
                655,    682,    711,    744,    780,    818,    861,    909,    963,    1022
            };

            public override void OnSoundParamsUpdated()
            {
                var gs = timbre.GlobalSettings;
                if (gs.Enable)
                {
                    if (gs.LFRQ.HasValue)
                        parentModule.LFRQ = gs.LFRQ.Value;
                    if (gs.LFRQ2.HasValue)
                        parentModule.LFRQ2 = gs.LFRQ2.Value;

                    if (gs.LFOD.HasValue && parentModule.lfdSound == null)
                        parentModule.LFOD = gs.LFOD.Value;
                    if (gs.LFOF.HasValue)
                        parentModule.LFOF = gs.LFOF.Value;
                    if (gs.LFOD2.HasValue && parentModule.lfd2Sound == null)
                        parentModule.LFOD2 = gs.LFOD2.Value;
                    if (gs.LFOF2.HasValue)
                        parentModule.LFOF2 = gs.LFOF2.Value;

                    if (gs.LFOW.HasValue)
                        parentModule.LFOW = gs.LFOW.Value;
                    if (gs.LFOW2.HasValue)
                        parentModule.LFOW2 = gs.LFOW2.Value;

                    if (gs.SYNC.HasValue)
                        parentModule.SYNC = gs.SYNC.Value;
                    if (gs.SYNC2.HasValue)
                        parentModule.SYNC2 = gs.SYNC2.Value;

                    if (gs.NE.HasValue)
                        parentModule.NE = gs.NE.Value;
                    if (gs.NFRQ.HasValue)
                        parentModule.NFRQ = gs.NFRQ.Value;
                }

                parentModule.YM2414WriteData(unitNumber, 0x38, 0, Slot, (byte)((timbre.PMS << 4 | timbre.AMS)));
                parentModule.YM2414WriteData(unitNumber, 0x38, 0, Slot, (byte)((1 << 7 | timbre.PMS << 4 | 1 << 2 | timbre.AMS)));
                for (int op = 0; op < 4; op++)
                {
                    if (timbre.Ops[op].FIX == 0)
                        parentModule.YM2414WriteData(unitNumber, 0x40, op, Slot, (byte)((timbre.Ops[op].DT1 << 4 | timbre.Ops[op].MUL)));
                    else
                        parentModule.YM2414WriteData(unitNumber, 0x40, op, Slot, (byte)((timbre.Ops[op].FIXR << 4 | timbre.Ops[op].FIXF)));
                    parentModule.YM2414WriteData(unitNumber, 0x40, op, Slot, (byte)((1 << 7 | timbre.Ops[op].OSCW << 4 | timbre.Ops[op].FINE)));

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
                                parentModule.YM2414WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 1:
                            if (op != 3)
                                parentModule.YM2414WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 2:
                            if (op != 3)
                                parentModule.YM2414WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 3:
                            if (op != 3)
                                parentModule.YM2414WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 4:
                            if (op != 1 && op != 3)
                                parentModule.YM2414WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 5:
                            if (op == 4)
                                parentModule.YM2414WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 6:
                            if (op == 4)
                                parentModule.YM2414WriteData(unitNumber, 0x60, op, Slot, (byte)tl);
                            break;
                        case 7:
                            break;
                    }
                    parentModule.YM2414WriteData(unitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].FIX << 5 | timbre.Ops[op].AR)));
                    parentModule.YM2414WriteData(unitNumber, 0xa0, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].D1R)));
                    parentModule.YM2414WriteData(unitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].DT2 << 6 | timbre.Ops[op].D2R)));
                    parentModule.YM2414WriteData(unitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].EGSF << 6 | 1 << 5 | timbre.Ops[op].REV)));
                    parentModule.YM2414WriteData(unitNumber, 0xe0, op, Slot, (byte)((timbre.Ops[op].SL << 4 | timbre.Ops[op].RR)));
                }

                base.OnSoundParamsUpdated();
            }


            /// <summary>
            /// 
            /// </summary>
            public override void OnVolumeUpdated()
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
                foreach (int op in ops)
                {
                    //$60+: total level
                    FMOperatorBase opb = timbre.Ops[op];

                    var tl = timbre.Ops[op].TL + timbre.Ops[op].GetLSAttenuationValue(NoteOnEvent.NoteNumber);
                    int kvs = timbre.Ops[op].GetKvsAttenuationValue(NoteOnEvent.Velocity);
                    if (kvs > 0)
                        tl += kvs;
                    if (tl > 127)
                        tl = 127;
                    double vol = 0;
                    if (kvs < 0)
                        vol = ((127 / 3) - Math.Round(((127 / 3) - (tl / 3)) * CalcCurrentVolume()));
                    else
                        vol = (127 - Math.Round((127 - (tl + kvs)) * CalcCurrentVolume(true)));
                    parentModule.YM2414WriteData(unitNumber, 0x60, op, Slot, (byte)vol);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="slot"></param>
            public override void OnPitchUpdated()
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

                parentModule.YM2414WriteData(unitNumber, 0x28, 0, Slot, (byte)((octave << 4) | nn), false);

                byte mono = CalcCurrentPanpot();
                if (32 <= mono && mono < 96)
                    mono = 0x1;
                else
                    mono = 0x0;
                parentModule.YM2414WriteData(unitNumber, 0x30, 0, Slot, (byte)(kf << 2 | mono), false);

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
                else if (pan >= 96)
                    pan = 0x2;
                else
                    pan = 0x3;
                byte mono = 0;
                if (32 <= pan && pan < 96)
                    mono = 0x1;
                else
                    mono = 0x0;

                parentModule.YM2414WriteData(unitNumber, 0x20, 0, Slot, (byte)(pan << 6 | (timbre.FB << 3) | timbre.ALG));

                uint? wd = parentModule.YM2414ReadData(0x20, 0, Slot);
                if (wd != null)
                {
                    byte data = (byte)(wd.Value & 0xfe);
                    parentModule.YM2414WriteData(unitNumber, 0x30, 0, Slot, (byte)(data | mono), false);
                }
                else
                {
                    parentModule.YM2414WriteData(unitNumber, 0x30, 0, Slot, (byte)mono, false);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void SetTimbre()
            {
                parentModule.YM2414WriteData(unitNumber, 0x38, 0, Slot, (byte)((timbre.PMS << 4 | timbre.AMS)));
                parentModule.YM2414WriteData(unitNumber, 0x38, 0, Slot, (byte)((1 << 7 | timbre.PMS2 << 4 | 1 << 2 | timbre.AMS2)));
                for (int op = 0; op < 4; op++)
                {
                    if (timbre.Ops[op].FIX == 0)
                        parentModule.YM2414WriteData(unitNumber, 0x40, op, Slot, (byte)((timbre.Ops[op].DT1 << 4 | timbre.Ops[op].MUL)));
                    else
                        parentModule.YM2414WriteData(unitNumber, 0x40, op, Slot, (byte)((timbre.Ops[op].FIXR << 4 | timbre.Ops[op].FIXR)));
                    parentModule.YM2414WriteData(unitNumber, 0x40, op, Slot, (byte)((1 << 7 | timbre.Ops[op].OSCW << 4 | timbre.Ops[op].FINE)));

                    parentModule.YM2414WriteData(unitNumber, 0x60, op, Slot, (byte)timbre.Ops[op].TL);
                    parentModule.YM2414WriteData(unitNumber, 0x80, op, Slot, (byte)((timbre.Ops[op].RS << 6 | timbre.Ops[op].FIX << 5 | timbre.Ops[op].AR)));
                    parentModule.YM2414WriteData(unitNumber, 0xa0, op, Slot, (byte)((timbre.Ops[op].AM << 7 | timbre.Ops[op].D1R)));
                    parentModule.YM2414WriteData(unitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].DT2 << 6 | timbre.Ops[op].D2R)));
                    parentModule.YM2414WriteData(unitNumber, 0xc0, op, Slot, (byte)((timbre.Ops[op].EGSF << 6 | 1 << 5 | timbre.Ops[op].REV)));
                    parentModule.YM2414WriteData(unitNumber, 0xe0, op, Slot, (byte)((timbre.Ops[op].SL << 4 | timbre.Ops[op].RR)));
                }

                OnPanpotUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                parentModule.YM2414WriteData(unitNumber, 0x08, 0, 0, (byte)(0x00 | Slot));
            }

        }


        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2414Timbre>))]
        [DataContract]
        [InstLock]
        public class YM2414Timbre : TimbreBase
        {
            #region FM Synth

            [Category("Sound")]
            [Editor(typeof(YM2414UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
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
                        nameof(PMS),
                        nameof(AMS2),
                        nameof(PMS2),

                        "GlobalSettings.EN",
                        "GlobalSettings.LFRQ",
                        "GlobalSettings.LFRQ2",
                        "GlobalSettings.LFOF",
                        "GlobalSettings.LFOD",
                        "GlobalSettings.LFOF2",
                        "GlobalSettings.LFOD2",
                        "GlobalSettings.LFOW",
                        "GlobalSettings.LFOW2",
                        "GlobalSettings.SYNC",
                        "GlobalSettings.SYNC2",
                        "GlobalSettings.LFD",
                        "GlobalSettings.LFD2",

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
                        "Ops[0].FINE",
                        "Ops[0].FIX",
                        "Ops[0].FIXR",
                        "Ops[0].FIXF",
                        "Ops[0].OSCW",
                        "Ops[0].EGSF",
                        "Ops[0].REV",
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
                        "Ops[1].FINE",
                        "Ops[1].FIX",
                        "Ops[1].FIXR",
                        "Ops[1].FIXF",
                        "Ops[1].OSCW",
                        "Ops[1].EGSF",
                        "Ops[1].REV",
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
                        "Ops[2].FINE",
                        "Ops[2].FIX",
                        "Ops[2].FIXR",
                        "Ops[2].FIXF",
                        "Ops[2].OSCW",
                        "Ops[2].EGSF",
                        "Ops[2].REV",
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
                        "Ops[3].FINE",
                        "Ops[3].FIX",
                        "Ops[3].FIXR",
                        "Ops[3].FIXF",
                        "Ops[3].OSCW",
                        "Ops[3].EGSF",
                        "Ops[3].REV",
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
                        nameof(AMS2),
                        nameof(PMS2),

                        "GlobalSettings.EN",
                        "GlobalSettings.LFRQ",
                        "GlobalSettings.LFRQ2",
                        "GlobalSettings.LFOF",
                        "GlobalSettings.LFOD",
                        "GlobalSettings.LFOF2",
                        "GlobalSettings.LFOD2",
                        "GlobalSettings.LFOW",
                        "GlobalSettings.LFOW2",
                        "GlobalSettings.SYNC",
                        "GlobalSettings.SYNC2",

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
                        "Ops[0].FINE",
                        "Ops[0].FIX",
                        "Ops[0].FIXR",
                        "Ops[0].FIXF",
                        "Ops[0].OSCW",
                        "Ops[0].EGSF",
                        "Ops[0].REV",
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
                        "Ops[1].FINE",
                        "Ops[1].FIX",
                        "Ops[1].FIXR",
                        "Ops[1].FIXF",
                        "Ops[1].OSCW",
                        "Ops[1].EGSF",
                        "Ops[1].REV",
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
                        "Ops[2].FINE",
                        "Ops[2].FIX",
                        "Ops[2].FIXR",
                        "Ops[2].FIXF",
                        "Ops[2].OSCW",
                        "Ops[2].EGSF",
                        "Ops[2].REV",
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
                        "Ops[3].FINE",
                        "Ops[3].FIX",
                        "Ops[3].FIXR",
                        "Ops[3].FIXF",
                        "Ops[3].OSCW",
                        "Ops[3].EGSF",
                        "Ops[3].REV",
                        "Ops[3].LS",
                        "Ops[3].KVS"
                        );
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
            [Description("Amplitude Modulation Sensitivity for LFO (0-3)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DisplayName("AMS/AMS2(AMS)")]
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
            [Description("Phase Modulation Sensitivity for LFO (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DisplayName("PMS/PMS2(PMS)")]
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

            private byte f_AMS2;

            [DataMember]
            [Category("Sound")]
            [Description("Amplitude Modulation Sensitivity for LFO2 (0-3)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte AMS2
            {
                get
                {
                    return f_AMS2;
                }
                set
                {
                    f_AMS2 = (byte)(value & 3);
                }
            }

            private byte f_PMS2;

            [DataMember]
            [Category("Sound")]
            [Description("Phase Modulation Sensitivity for LFO2 (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte PMS2
            {
                get
                {
                    return f_PMS2;
                }
                set
                {
                    f_PMS2 = (byte)(value & 7);
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
            public YM2414Operator[] Ops
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
                var ops = new YM2414Operator[] {
                    new YM2414Operator(),
                    new YM2414Operator(),
                    new YM2414Operator(),
                    new YM2414Operator() };
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
                        "Ops[0].FINE",
                        "Ops[0].FIX",
                        "Ops[0].FIXR",
                        "Ops[0].OSCW",
                        "Ops[0].EGSF",
                        "Ops[0].REV",

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
                        "Ops[1].FINE",
                        "Ops[1].FIX",
                        "Ops[1].FIXR",
                        "Ops[1].OSCW",
                        "Ops[1].EGSF",
                        "Ops[1].REV",

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
                        "Ops[2].FINE",
                        "Ops[2].FIX",
                        "Ops[2].FIXR",
                        "Ops[2].OSCW",
                        "Ops[2].EGSF",
                        "Ops[2].REV",

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
                        "Ops[3].FINE",
                        "Ops[3].FIX",
                        "Ops[3].FIXR",
                        "Ops[3].OSCW",
                        "Ops[3].EGSF",
                        "Ops[3].REV",

                        nameof(AMS),
                        nameof(PMS));
                }
                set
                {
                    SimpleSerializer.DeserializeProps(this, value,
                        nameof(ALG),
                        nameof(FB),

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
                        "Ops[0].FINE",
                        "Ops[0].FIX",
                        "Ops[0].FIXR",
                        "Ops[0].OSCW",
                        "Ops[0].EGSF",
                        "Ops[0].REV",

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
                        "Ops[1].FINE",
                        "Ops[1].FIX",
                        "Ops[1].FIXR",
                        "Ops[1].OSCW",
                        "Ops[1].EGSF",
                        "Ops[1].REV",

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
                        "Ops[2].FINE",
                        "Ops[2].FIX",
                        "Ops[2].FIXR",
                        "Ops[2].OSCW",
                        "Ops[2].EGSF",
                        "Ops[2].REV",

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
                        "Ops[3].FINE",
                        "Ops[3].FIX",
                        "Ops[3].FIXR",
                        "Ops[3].OSCW",
                        "Ops[3].EGSF",
                        "Ops[3].REV",

                        nameof(AMS),
                        nameof(PMS));
                }
            }


            [DataMember]
            [Category("Chip(Global)")]
            [Description("Global Settings")]
            public YM2414GlobalSettings GlobalSettings
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
                GlobalSettings.InjectFrom(new LoopInjection(), new YM2414GlobalSettings());
            }

            /// <summary>
            /// 
            /// </summary>
            public YM2414Timbre()
            {
                Ops = new YM2414Operator[] {
                    new YM2414Operator(),
                    new YM2414Operator(),
                    new YM2414Operator(),
                    new YM2414Operator() };
                GlobalSettings = new YM2414GlobalSettings();
            }

            #endregion

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<YM2414Timbre>(serializeData);
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
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2414Operator>))]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [DataContract]
        [InstLock]
        public class YM2414Operator : FMOperatorBase
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
            [Description("Multiply (0-15")]
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

            //https://wave.hatenablog.com/entry/2021/09/20/212800
            //https://www.polynominal.com/yamaha-ym2414/
            /// <summary>
            /// Detune1(0-7)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("DeTune 1\r\n" +
                "1-3: +1 ～ +3 cent\r\n" +
                "5-7: -1 ～ -3 cent")]
            [DefaultValue((byte)4)]
            [SlideParametersAttribute(0, 7)]
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
            [Description("Amplitude Modulation Sensivity (0:Disable 1:Enable)")]
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

            private byte f_FIX;

            /// <summary>
            /// Fix Frequency Mode Enable (0:Disable or 1:Enable)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Fix Frequency Mode Enable (0:Disable or 1)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte FIX
            {
                get
                {
                    return f_FIX;
                }
                set
                {
                    f_FIX = (byte)(value & 1);
                }
            }

            private byte f_FIXR;

            /// <summary>
            /// Fix Frequency Range (0-7)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Fix Frequency Range (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte FIXR
            {
                get
                {
                    return f_FIXR;
                }
                set
                {
                    f_FIXR = (byte)(value & 7);
                }
            }

            private byte f_FIXF;

            /// <summary>
            /// Fix Frequency (0-7)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Fix Frequency (0-15)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte FIXF
            {
                get
                {
                    return f_FIXF;
                }
                set
                {
                    f_FIXF = (byte)(value & 15);
                }
            }

            private byte f_FINE;

            /// <summary>
            /// Fine Frequency(0-15)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Fine Frequency\r\n" +
                "When MUL is 0, there are 8 levels (0-7)." +
                "When MUL is 1 or higher, there are 16 levels (0-15).")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 15)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte FINE
            {
                get
                {
                    return f_FINE;
                }
                set
                {
                    f_FINE = (byte)(value & 15);
                }
            }

            private byte f_OSCW;

            /// <summary>
            /// Oscillator Wave Form (0-7)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Oscillator Wave Form (0-7)" +
                "0: Sine\r\n" +
                "1: Sine2\r\n" +
                "2: Half Sine\r\n" +
                "3: Half Sine2\r\n" +
                "4: 1/2 Sine\r\n" +
                "5: 1/2 Sine2\r\n" +
                "6: Double Half Sine\r\n" +
                "7: Double Half Sine2")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte OSCW
            {
                get
                {
                    return f_OSCW;
                }
                set
                {
                    f_OSCW = (byte)(value & 7);
                }
            }

            private byte f_EGSF;

            /// <summary>
            /// EGSF(0-3)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description(" EG Shift(0-3)\r\n" +
                "Note: Not affect for Carrier")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte EGSF
            {
                get
                {
                    return f_EGSF;
                }
                set
                {
                    f_EGSF = (byte)(value & 3);
                }
            }

            private byte f_REV;

            /// <summary>
            /// Reverberation(0-7)
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Reverberation Rate (0-7)")]
            [DefaultValue((byte)0)]
            [SlideParametersAttribute(0, 7)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte REV
            {
                get
                {
                    return f_REV;
                }
                set
                {
                    f_REV = (byte)(value & 3);
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
                        nameof(DT2),
                        nameof(FINE),
                        nameof(FIX),
                        nameof(FIXR),
                        nameof(FIXF),
                        nameof(OSCW),
                        nameof(EGSF),
                        nameof(REV),
                        nameof(LS),
                        nameof(KVS)
                        );
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
                        nameof(DT2),
                        nameof(FINE),
                        nameof(FIX),
                        nameof(FIXR),
                        nameof(FIXF),
                        nameof(OSCW),
                        nameof(EGSF),
                        nameof(REV),
                        nameof(LS),
                        nameof(KVS)
                        );
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
                    var obj = JsonConvert.DeserializeObject<YM2414Operator>(serializeData);
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
        [JsonConverter(typeof(NoTypeConverterJsonConverter<YM2414GlobalSettings>))]
        [DataContract]
        [InstLock]
        public class YM2414GlobalSettings : ContextBoundObject
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
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Select AMD or PMD (0:AMD 1:PMD)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFOF
            {
                get
                {
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
            [DataMember]
            [Category("Chip(Global)")]
            [Description("AM/PM Depth (0-127)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DisplayName("AMD/PMD(LFOD)")]
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
                }
            }

            private byte? f_LFOD2;


            /// <summary>
            /// LFO2 Depth(0-127)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("LFO2 Depth (0-127)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 127)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DisplayName("AMD/PMD(LFOD2)")]
            public byte? LFOD2
            {
                get
                {
                    return f_LFOD2;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 127);
                    f_LFOD2 = v;
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

            private byte? f_SYNC;

            /// <summary>
            /// LFO SYNC Enable (0:Disable 1:Enable)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("LFO SYNC Enable (0:Disable 1:Enable)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? SYNC
            {
                get
                {
                    return f_SYNC;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 3);
                    f_SYNC = v;
                }
            }

            private byte? f_LFRQ2;

            /// <summary>
            /// LFRQ2 (0-255)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("LFO2 Freq (0-255)")]
            [SlideParametersAttribute(0, 255)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? LFRQ2
            {
                get
                {
                    return f_LFRQ2;
                }
                set
                {
                    f_LFRQ2 = value;
                }
            }

            private byte? f_LFOF2;

            /// <summary>
            /// Select AMD2 or PMD2(0:AMD2 1:PMD2)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("Select AMD2 or PMD2 (0:AMD2 1:PMD2)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFOF2
            {
                get
                {
                    return f_LFOF2;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 1);
                    f_LFOF2 = v;
                }
            }

            private byte? f_LFOW2;


            /// <summary>
            /// LFO2 Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("LFO2 Wave Type (0:Saw 1:SQ 2:Tri 3:Rnd)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 3)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFOW2
            {
                get
                {
                    return f_LFOW2;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 3);
                    f_LFOW2 = v;
                }
            }

            private byte? f_SYNC2;

            /// <summary>
            /// LFO SYNC2 Enable (0:Disable 1:Enable)
            /// </summary>
            [DataMember]
            [Category("Chip(Global)")]
            [Description("LFO SYNC2 Enable (0:Disable 1:Enable)")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public byte? SYNC2
            {
                get
                {
                    return f_SYNC2;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue)
                        v = (byte)(value & 3);
                    f_SYNC2 = v;
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

            private byte? f_LFD;


            /// <summary>
            /// LFO Delay
            /// </summary>
            [DataMember]
            [Category("Chip(Global(Driver))")]
            [Description("LFO Delay (0-99)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 99)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFD
            {
                get
                {
                    return f_LFD;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue & 0 <= value.Value & value.Value <= 99)
                        v = (byte)(value);
                    f_LFD = v;
                }
            }


            private byte? f_LFD2;


            /// <summary>
            /// LFO Delay
            /// </summary>
            [DataMember]
            [Category("Chip(Global(Driver))")]
            [Description("LFO Delay 2(0-99)")]
            [DefaultValue(null)]
            [SlideParametersAttribute(0, 99)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            public byte? LFD2
            {
                get
                {
                    return f_LFD2;
                }
                set
                {
                    byte? v = value;
                    if (value.HasValue & 0 <= value.Value & value.Value <= 99)
                        v = (byte)(value);
                    f_LFD2 = v;
                }
            }
        }
    }


    [JsonConverter(typeof(NoTypeConverterJsonConverter<FMOperatorBase>))]
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [DataContract]
    [InstLock]
    public class FMOperatorBase : ContextBoundObject
    {

        private byte f_LS;

        /// <summary>
        /// Level Scaling
        /// </summary>
        [DataMember]
        [Category("Sound(Driver)")]
        [Description("Level Scaling(0-99")]
        [DefaultValue(0)]
        [SlideParametersAttribute(0, 99)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte LS
        {
            get
            {
                return f_LS;
            }
            set
            {
                if (0 <= value && value <= 99)
                    f_LS = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteNumber"></param>
        /// <returns></returns>
        public int GetLSAttenuationValue(int noteNumber)
        {
            for (int i = 0; i < LevelScalingIndex.Length; i++)
            {
                if (noteNumber <= LevelScalingIndex[i])
                    return LevelScalingAttenuationValue[LS][i];
            }
            return LevelScalingAttenuationValue[LS][LevelScalingIndex.Length - 1];
        }

        private static int[] LevelScalingIndex = new int[] { 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60, 63, 66, 69, 72, 75, 78, 81, 84, 87, 90, 93, 96, 99, 102, 105, 108 };

        private static int[][] LevelScalingAttenuationValue = new int[][] {
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,4},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,5},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,2,2,2,3,3,4,5,6,7,8},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,8,9},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,8,10,11},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,10,12},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,8,10,12,13},
                        new int[] {0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,14},
                        new int[] {0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,8,10,12,14,16},
                        new int[] {0,0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,4,4,5,6,7,9,11,13,15,17},
                        new int[] {0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,2,3,4,4,5,7,8,9,11,14,16,18},
                        new int[] {0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,10,12,15,17,20},
                        new int[] {0,0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,15,18,21},
                        new int[] {0,0,0,0,0,0,0,0,0,1,1,1,2,2,2,3,4,5,6,7,8,10,12,14,17,20,22},
                        new int[] {0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,8,10,12,15,17,21,23},
                        new int[] {0,0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,15,18,22,25},
                        new int[] {0,0,0,0,0,0,0,0,1,1,1,2,2,2,3,4,4,6,7,8,10,11,14,16,20,23,26},
                        new int[] {0,0,0,0,0,0,0,0,1,1,1,2,2,3,3,4,5,6,7,8,10,12,14,17,20,24,27},
                        new int[] {0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,10,13,15,18,21,25,29},
                        new int[] {0,0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,15,19,22,26,30},
                        new int[] {0,0,0,0,0,0,1,1,1,1,2,2,2,3,4,4,5,7,8,10,11,14,16,20,23,28,31},
                        new int[] {0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,8,10,12,14,17,20,24,29,33},
                        new int[] {0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,10,12,15,18,21,25,30,34},
                        new int[] {0,0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,8,9,11,13,15,18,22,26,31,35},
                        new int[] {0,0,0,0,0,1,1,1,1,2,2,2,3,4,4,5,6,8,9,11,13,16,19,23,27,32,36},
                        new int[] {0,0,0,0,0,1,1,1,1,2,2,2,3,4,4,5,7,8,10,12,14,16,20,24,28,33,38},
                        new int[] {0,0,0,0,0,1,1,1,1,2,2,2,3,4,5,6,7,8,10,12,14,17,20,24,29,34,39},
                        new int[] {0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,10,12,15,18,21,25,30,35,40},
                        new int[] {0,0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,15,18,22,26,31,37,42},
                        new int[] {0,0,0,0,1,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,16,19,22,27,32,38,43},
                        new int[] {0,0,0,0,1,1,1,1,1,2,2,3,4,4,5,6,8,10,11,14,16,19,23,28,33,39,44},
                        new int[] {0,0,0,0,1,1,1,1,1,2,2,3,4,5,5,7,8,10,12,14,17,20,24,28,34,40,45},
                        new int[] {0,0,0,0,1,1,1,1,2,2,2,3,4,5,6,7,8,10,12,14,17,20,24,29,35,41,47},
                        new int[] {0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,8,10,12,15,17,21,25,30,35,42,48},
                        new int[] {0,0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,15,18,22,26,31,37,43,49},
                        new int[] {0,0,0,1,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,16,19,22,26,32,38,45,51},
                        new int[] {0,0,0,1,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,16,19,23,27,32,38,45,52},
                        new int[] {0,0,0,1,1,1,1,1,2,2,3,4,4,5,6,8,9,12,14,16,20,23,28,33,40,47,53},
                        new int[] {0,0,0,1,1,1,1,1,2,3,3,4,4,6,7,8,10,12,14,17,20,24,28,34,40,48,54},
                        new int[] {0,0,0,1,1,1,1,1,2,3,3,4,5,6,7,8,10,12,14,17,20,24,29,35,41,49,56},
                        new int[] {0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,8,10,12,15,18,21,25,30,36,43,50,57},
                        new int[] {0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,8,10,13,15,18,21,26,30,36,43,51,58},
                        new int[] {0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,15,18,22,26,31,37,44,52,60},
                        new int[] {0,0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,16,19,22,27,32,38,45,53,61},
                        new int[] {0,0,0,1,1,1,1,2,2,3,3,4,5,6,8,9,11,14,16,19,23,27,32,39,46,55,62},
                        new int[] {0,0,1,1,1,1,2,2,2,3,4,4,5,7,8,9,11,14,16,20,23,28,33,40,47,56,63},
                        new int[] {0,0,1,1,1,1,2,2,2,3,4,4,5,7,8,9,12,14,17,20,24,28,34,40,48,57,65},
                        new int[] {0,0,1,1,1,1,2,2,2,3,4,4,6,7,8,10,12,14,17,20,24,29,35,41,49,58,66},
                        new int[] {0,0,1,1,1,1,2,2,2,3,4,5,6,7,8,10,12,15,17,21,25,30,35,42,50,59,67},
                        new int[] {0,0,1,1,1,1,2,2,2,3,4,5,6,7,8,10,12,15,18,21,25,30,36,43,51,60,69},
                        new int[] {0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,10,12,15,18,22,26,31,36,44,52,61,70},
                        new int[] {0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,10,13,16,18,22,26,31,37,45,53,63,71},
                        new int[] {0,0,1,1,1,1,2,2,3,3,4,5,6,7,9,11,13,16,19,22,27,32,38,45,54,63,72},
                        new int[] {0,0,1,1,1,2,2,2,3,4,4,5,6,8,9,11,13,16,19,23,27,32,38,46,55,65,74},
                        new int[] {0,0,1,1,1,2,2,2,3,4,4,5,6,8,9,11,13,16,19,23,28,33,39,47,56,66,75},
                        new int[] {0,0,1,1,1,2,2,2,3,4,4,5,6,8,9,11,14,17,20,24,28,33,40,48,57,67,76},
                        new int[] {0,0,1,1,1,2,2,2,3,4,4,5,7,8,10,11,14,17,20,24,29,34,41,49,58,68,78},
                        new int[] {0,0,1,1,1,2,2,2,3,4,4,5,7,8,10,12,14,17,20,24,29,35,41,49,59,69,79},
                        new int[] {0,0,1,1,1,2,2,2,3,4,5,6,7,8,10,12,14,18,21,25,30,35,42,50,60,70,80},
                        new int[] {0,0,1,1,1,2,2,2,3,4,5,6,7,9,10,12,15,18,21,25,30,36,43,51,61,72,82},
                        new int[] {0,0,1,1,1,2,2,2,3,4,5,6,7,9,10,12,15,18,21,26,30,36,43,52,61,73,83},
                        new int[] {0,0,1,1,1,2,2,2,3,4,5,6,7,9,10,12,15,18,22,26,31,37,44,53,63,74,84},
                        new int[] {0,1,1,1,2,2,2,3,3,4,5,6,7,9,11,13,15,19,22,26,31,37,45,53,63,75,85},
                        new int[] {0,1,1,1,2,2,2,3,3,4,5,6,7,9,11,13,16,19,22,27,32,38,45,54,64,76,87},
                        new int[] {0,1,1,1,2,2,2,3,3,4,5,6,7,9,11,13,16,19,23,27,32,39,46,55,65,77,88},
                        new int[] {0,1,1,1,2,2,2,3,3,4,5,6,8,9,11,13,16,20,23,28,33,39,47,56,66,78,89},
                        new int[] {0,1,1,1,2,2,2,3,3,5,5,6,8,10,11,13,16,20,23,28,33,40,47,57,67,80,91},
                        new int[] {0,1,1,1,2,2,2,3,3,5,5,6,8,10,11,14,16,20,24,28,34,40,48,57,68,80,92},
                        new int[] {0,1,1,1,2,2,2,3,4,5,5,6,8,10,12,14,17,20,24,29,34,41,49,58,69,82,93},
                        new int[] {0,1,1,1,2,2,2,3,4,5,5,7,8,10,12,14,17,21,24,29,35,41,49,59,70,83,94},
                        new int[] {0,1,1,1,2,2,3,3,4,5,6,7,8,10,12,14,17,21,25,30,35,42,50,60,71,84,96},
                        new int[] {0,1,1,1,2,2,3,3,4,5,6,7,8,10,12,14,17,21,25,30,36,43,51,60,72,85,97},
                        new int[] {0,1,1,1,2,2,3,3,4,5,6,7,8,10,12,15,18,22,25,30,36,43,51,61,73,86,98},
                        new int[] {0,1,1,1,2,2,3,3,4,5,6,7,9,10,12,15,18,22,26,31,37,44,52,62,74,87,100},
                        new int[] {0,1,1,1,2,2,3,3,4,5,6,7,9,11,13,15,18,22,26,31,37,44,53,63,75,88,101},
                        new int[] {0,1,1,2,2,2,3,3,4,5,6,7,9,11,13,15,18,22,26,32,38,45,53,64,76,90,102},
                        new int[] {0,1,1,2,2,2,3,3,4,5,6,7,9,11,13,15,19,23,27,32,38,45,54,65,77,91,103},
                        new int[] {0,1,1,2,2,2,3,3,4,5,6,7,9,11,13,16,19,23,27,32,39,46,55,65,78,92,105},
                        new int[] {0,1,1,2,2,2,3,3,4,5,6,7,9,11,13,16,19,23,27,33,39,47,55,66,79,93,106},
                        new int[] {0,1,1,2,2,2,3,3,4,5,6,8,9,11,13,16,19,24,28,33,40,47,56,67,80,94,107},
                        new int[] {0,1,1,2,2,2,3,3,4,5,6,8,9,11,14,16,20,24,28,34,40,48,57,68,81,95,109},
                        new int[] {0,1,1,2,2,3,3,3,4,6,6,8,9,12,14,16,20,24,28,34,41,48,57,69,82,96,110},
                        new int[] {0,1,1,2,2,3,3,3,4,6,7,8,10,12,14,17,20,24,29,35,41,49,58,70,83,98,111},
                        new int[] {0,1,1,2,2,3,3,3,4,6,7,8,10,12,14,17,20,25,29,35,41,49,59,70,83,98,112},
                        new int[] {0,1,1,2,2,3,3,4,4,6,7,8,10,12,14,17,21,25,29,35,42,50,59,71,84,100,114},
                        new int[] {0,1,1,2,2,3,3,4,4,6,7,8,10,12,14,17,21,25,30,36,43,51,60,72,86,101,115},
                        new int[] {0,1,1,2,2,3,3,4,5,6,7,8,10,12,15,17,21,26,30,36,43,51,61,73,86,102,116},
                        new int[] {0,1,1,2,2,3,3,4,5,6,7,8,10,12,15,18,21,26,31,37,43,52,62,74,87,103,118},
                        new int[] {0,1,1,2,2,3,3,4,5,6,7,8,10,13,15,18,21,26,31,37,44,52,62,74,88,104,119},
                        new int[] {0,1,1,2,2,3,3,4,5,6,7,8,10,13,15,18,22,26,31,37,44,53,63,75,89,105,120},
                        new int[] {0,1,1,2,2,3,3,4,5,6,7,9,10,13,15,18,22,27,31,38,45,53,63,76,90,106,121},
                        new int[] {0,1,1,2,2,3,3,4,5,6,7,9,11,13,15,18,22,27,32,38,45,54,64,77,91,108,123},
                        new int[] {0,1,1,2,2,3,3,4,5,6,7,9,11,13,16,19,22,27,32,39,46,55,65,78,92,109,124},
                        new int[] {0,1,1,2,2,3,3,4,5,6,7,9,11,13,16,19,23,28,32,39,46,55,65,78,93,110,125},
                        new int[] {0,1,1,2,2,3,3,4,5,6,7,9,11,13,16,19,23,28,33,39,47,56,66,79,94,111,127}
            };

        private int f_KVS = -1;

        /// <summary>
        /// Level Scaling
        /// </summary>
        [DataMember]
        [Category("Sound(Driver)")]
        [Description("Key Velocity Sense(-1(MAmi),0(Off),1-7(KVS On)\r\n" +
            "-1: MAmi original algolithm" +
            " 0: Off(Ignore Velocity)")]
        [DefaultValue(-1)]
        [SlideParametersAttribute(-1, 7)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int KVS
        {
            get
            {
                return f_KVS;
            }
            set
            {
                if (-1 <= value && value <= 7)
                    f_KVS = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteNumber"></param>
        /// <returns></returns>
        public int GetKvsAttenuationValue(int velocity)
        {
            if (KVS < 0)
                return -1;
            if (KVS == 0)
                return 0;

            return KeyVelocitySenseValues[velocity][KVS - 1];
        }

        private static int[][] KeyVelocitySenseValues = new int[][]{
                          //kvs=1   2   3   4   5   6   7
                new int[] {     0,  0,  0,  0,  0,  0,  0},
                new int[] {    18, 29, 40, 51, 62, 73, 84},
                new int[] {    17, 28, 38, 48, 58, 69, 79},
                new int[] {    17, 27, 37, 47, 56, 66, 76},
                new int[] {    17, 26, 36, 45, 55, 64, 74},
                new int[] {    16, 25, 34, 44, 53, 62, 71},
                new int[] {    16, 25, 34, 43, 51, 60, 69},
                new int[] {    16, 24, 33, 41, 50, 58, 67},
                new int[] {    15, 24, 32, 40, 48, 57, 65},
                new int[] {    15, 23, 31, 39, 47, 55, 63},
                new int[] {    15, 23, 30, 38, 46, 54, 62},
                new int[] {    15, 22, 30, 38, 45, 53, 60},
                new int[] {    14, 22, 29, 37, 44, 51, 59},
                new int[] {    14, 21, 28, 36, 43, 50, 57},
                new int[] {    14, 21, 28, 35, 42, 49, 56},
                new int[] {    14, 21, 27, 34, 41, 48, 55},
                new int[] {    14, 20, 27, 34, 40, 47, 53},
                new int[] {    13, 20, 26, 33, 39, 46, 52},
                new int[] {    13, 20, 26, 32, 38, 45, 51},
                new int[] {    13, 19, 25, 32, 38, 44, 50},
                new int[] {    13, 19, 25, 31, 37, 43, 49},
                new int[] {    13, 19, 24, 30, 36, 42, 48},
                new int[] {    13, 18, 24, 30, 35, 41, 47},
                new int[] {    13, 18, 24, 29, 35, 40, 46},
                new int[] {    12, 18, 23, 29, 34, 40, 45},
                new int[] {    12, 18, 23, 28, 34, 39, 44},
                new int[] {    12, 17, 23, 28, 33, 38, 43},
                new int[] {    12, 17, 22, 27, 32, 37, 42},
                new int[] {    12, 17, 22, 27, 32, 37, 42},
                new int[] {    12, 17, 21, 26, 31, 36, 41},
                new int[] {    12, 17, 21, 26, 31, 36, 40},
                new int[] {    12, 16, 21, 26, 30, 35, 39},
                new int[] {    12, 16, 21, 25, 30, 34, 39},
                new int[] {    11, 16, 20, 25, 29, 33, 38},
                new int[] {    11, 16, 20, 24, 29, 33, 37},
                new int[] {    11, 15, 20, 24, 28, 32, 36},
                new int[] {    11, 15, 19, 24, 28, 32, 36},
                new int[] {    11, 15, 19, 23, 27, 31, 35},
                new int[] {    11, 15, 19, 23, 27, 31, 35},
                new int[] {    11, 15, 19, 23, 26, 30, 34},
                new int[] {    11, 15, 18, 22, 26, 30, 33},
                new int[] {    11, 14, 18, 22, 25, 29, 33},
                new int[] {    11, 14, 18, 22, 25, 29, 32},
                new int[] {    11, 14, 18, 21, 25, 28, 32},
                new int[] {    10, 14, 17, 21, 24, 28, 31},
                new int[] {    10, 14, 17, 21, 24, 27, 31},
                new int[] {    10, 14, 17, 20, 23, 27, 30},
                new int[] {    10, 13, 17, 20, 23, 26, 29},
                new int[] {    10, 13, 16, 20, 23, 26, 29},
                new int[] {    10, 13, 16, 19, 22, 25, 28},
                new int[] {    10, 13, 16, 19, 22, 25, 28},
                new int[] {    10, 13, 16, 19, 22, 25, 28},
                new int[] {    10, 13, 16, 19, 21, 24, 27},
                new int[] {    10, 13, 15, 18, 21, 24, 26},
                new int[] {    10, 12, 15, 18, 20, 23, 26},
                new int[] {    10, 12, 15, 18, 20, 23, 25},
                new int[] {    10, 12, 15, 17, 20, 22, 25},
                new int[] {    10, 12, 15, 17, 20, 22, 25},
                new int[] {     9, 12, 14, 17, 19, 22, 24},
                new int[] {     9, 12, 14, 17, 19, 21, 24},
                new int[] {     9, 12, 14, 16, 19, 21, 23},
                new int[] {     9, 12, 14, 16, 18, 21, 23},
                new int[] {     9, 11, 13, 16, 18, 20, 22},
                new int[] {     9, 11, 13, 15, 17, 19, 21},
                new int[] {     9, 11, 13, 15, 17, 19, 21},
                new int[] {     9, 11, 13, 15, 17, 19, 21},
                new int[] {     9, 11, 13, 15, 16, 18, 20},
                new int[] {     9, 11, 12, 14, 16, 18, 20},
                new int[] {     9, 11, 12, 14, 16, 18, 19},
                new int[] {     9, 10, 12, 14, 15, 17, 19},
                new int[] {     9, 10, 12, 14, 15, 17, 18},
                new int[] {     9, 10, 12, 13, 15, 16, 18},
                new int[] {     9, 10, 12, 13, 15, 16, 18},
                new int[] {     8, 10, 11, 13, 14, 16, 17},
                new int[] {     8, 10, 11, 13, 14, 15, 17},
                new int[] {     8, 10, 11, 12, 14, 15, 16},
                new int[] {     8, 10, 11, 12, 13, 15, 16},
                new int[] {     8,  9, 11, 12, 13, 14, 15},
                new int[] {     8,  9, 10, 12, 13, 14, 15},
                new int[] {     8,  9, 10, 11, 12, 13, 14},
                new int[] {     8,  9, 10, 11, 12, 13, 14},
                new int[] {     8,  9, 10, 11, 12, 13, 14},
                new int[] {     8,  9, 10, 11, 11, 12, 13},
                new int[] {     8,  9,  9, 10, 11, 12, 13},
                new int[] {     8,  9,  9, 10, 11, 12, 12},
                new int[] {     8,  8,  9, 10, 10, 11, 12},
                new int[] {     8,  8,  9, 10, 10, 11, 11},
                new int[] {     8,  8,  9, 10, 10, 11, 11},
                new int[] {     8,  8,  9,  9, 10, 10, 11},
                new int[] {     8,  8,  9,  9, 10, 10, 11},
                new int[] {     7,  8,  8,  9,  9, 10, 10},
                new int[] {     7,  8,  8,  9,  9,  9, 10},
                new int[] {     7,  8,  8,  9,  9,  9, 10},
                new int[] {     7,  8,  8,  8,  9,  9,  9},
                new int[] {     7,  8,  8,  8,  8,  9,  9},
                new int[] {     7,  7,  8,  8,  8,  8,  8},
                new int[] {     7,  7,  7,  8,  8,  8,  8},
                new int[] {     7,  7,  7,  8,  8,  8,  8},
                new int[] {     7,  7,  7,  7,  7,  7,  7},
                new int[] {     7,  7,  7,  7,  7,  7,  7},
                new int[] {     7,  7,  7,  7,  7,  7,  7},
                new int[] {     7,  7,  7,  7,  7,  7,  7},
                new int[] {     7,  7,  7,  7,  6,  6,  6},
                new int[] {     7,  7,  7,  7,  6,  6,  6},
                new int[] {     7,  7,  6,  6,  6,  6,  6},
                new int[] {     7,  7,  6,  6,  6,  6,  5},
                new int[] {     7,  7,  6,  6,  6,  6,  5},
                new int[] {     7,  6,  6,  6,  5,  5,  5},
                new int[] {     7,  6,  6,  6,  5,  5,  5},
                new int[] {     7,  6,  6,  6,  5,  5,  4},
                new int[] {     7,  6,  6,  5,  5,  4,  4},
                new int[] {     7,  6,  6,  5,  5,  4,  4},
                new int[] {     7,  6,  6,  5,  5,  4,  4},
                new int[] {     6,  6,  5,  5,  4,  4,  3},
                new int[] {     6,  6,  5,  5,  4,  4,  3},
                new int[] {     6,  6,  5,  5,  4,  3,  3},
                new int[] {     6,  6,  5,  5,  4,  3,  3},
                new int[] {     6,  6,  5,  4,  4,  3,  2},
                new int[] {     6,  6,  5,  4,  4,  3,  2},
                new int[] {     6,  6,  5,  4,  3,  3,  2},
                new int[] {     6,  6,  5,  4,  3,  3,  2},
                new int[] {     6,  5,  5,  4,  3,  2,  1},
                new int[] {     6,  5,  5,  4,  3,  2,  1},
                new int[] {     6,  5,  4,  4,  3,  2,  1},
                new int[] {     6,  5,  4,  4,  3,  2,  1},
                new int[] {     6,  5,  4,  3,  2,  1,  0},
                new int[] {     6,  5,  4,  3,  2,  1,  0},
                new int[] {     6,  5,  4,  3,  2,  1,  0}
            };

    }


}