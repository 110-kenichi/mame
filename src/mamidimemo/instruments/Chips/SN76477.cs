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


//http://www.kyohritsu.jp/eclib/OTHER/DATASHEET/sn76477n.pdf
//http://www.st.rim.or.jp/~nkomatsu/peripheral/SN76477.html
//http://xyama.sakura.ne.jp/hp/SoundMachine.html

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class SN76477 : InstrumentBase
    {

        public override string Name => "SN76477";

        public override string Group => "DISCRETE";

        public override InstrumentType InstrumentType => InstrumentType.SN76477;

        [Browsable(false)]
        public override string ImageKey => "SN76477";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "sn76477_";

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
                return 27;
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
        public SN76477Timbre[] Timbres
        {
            get;
            set;
        }

        #region General

        private int f_Inhibit = 1;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( General)")]
        [Description("Sound Inhibit & One Shot Trigger")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((int)1)]
        public int Inhibit
        {
            get
            {
                return f_Inhibit;
            }
            set
            {
                var v = (int)(value & 1);
                if (f_Inhibit != v)
                {
                    f_Inhibit = v;

                    SN76477_logic_w(UnitNumber, 0, v);
                }
            }
        }

        /// <summary>
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore]
        [Category("Chip( General)")]
        [Description("Mixer A,B,C")]
        [DefaultValue(MixerSet.VCO)]
        public MixerSet Mixer
        {
            get
            {
                return (MixerSet)(MixerA | MixerB << 1 | MixerC << 2);
            }
            set
            {
                if (Mixer != value)
                {
                    var v = (int)value;
                    MixerA = v & 1;
                    MixerB = (v >> 1) & 1;
                    MixerC = (v >> 2) & 1;
                }
            }
        }

        private int f_MixerA;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( General)")]
        [Description("Mixer A\r\n" +
            "C B A\r\n" +
            "L L L: VCO\r\n" +
            "L L H: SLF\r\n" +
            "L H L: Noise\r\n" +
            "L H H: VCOxNoise\r\n" +
            "H L L: SLFxNoise\r\n" +
            "H L H: SLFxVCOxNoise\r\n" +
            "H H L: SLFxVCO\r\n" +
            "H H H: Inhibit")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((int)0)]
        public int MixerA
        {
            get
            {
                return f_MixerA;
            }
            set
            {
                var v = (int)(value & 1);
                if (f_MixerA != v)
                {
                    f_MixerA = v;

                    SN76477_logic_w(UnitNumber, 1, v);
                }
            }
        }

        private int f_MixerB;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( General)")]
        [Description("Mixer B\r\n" +
            "C B A\r\n" +
            "L L L: VCO\r\n" +
            "L L H: SLF\r\n" +
            "L H L: Noise\r\n" +
            "L H H: VCOxNoise\r\n" +
            "H L L: SLFxNoise\r\n" +
            "H L H: SLFxVCOxNoise\r\n" +
            "H H L: SLFxVCO\r\n" +
            "H H H: Inhibit")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((int)0)]
        public int MixerB
        {
            get
            {
                return f_MixerB;
            }
            set
            {
                var v = (int)(value & 1);
                if (f_MixerB != v)
                {
                    f_MixerB = v;

                    SN76477_logic_w(UnitNumber, 2, v);
                }
            }
        }

        private int f_MixerC;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( General)")]
        [Description("Mixer C\r\n" +
            "C B A\r\n" +
            "L L L: VCO\r\n" +
            "L L H: SLF\r\n" +
            "L H L: Noise\r\n" +
            "L H H: VCOxNoise\r\n" +
            "H L L: SLFxNoise\r\n" +
            "H L H: SLFxVCOxNoise\r\n" +
            "H H L: SLFxVCO\r\n" +
            "H H H: Inhibit")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((int)0)]
        public int MixerC
        {
            get
            {
                return f_MixerC;
            }
            set
            {
                var v = (int)(value & 1);
                if (f_MixerC != v)
                {
                    f_MixerC = v;

                    SN76477_logic_w(UnitNumber, 3, v);
                }
            }
        }


        /// <summary>
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore]
        [Category("Chip( General)")]
        [Description("Envelope 1,2")]
        [DefaultValue(EnvelopeSet.VCO)]
        public EnvelopeSet Envelope
        {
            get
            {
                return (EnvelopeSet)(Envelope2 | Envelope1 << 1);
            }
            set
            {
                if (Envelope != value)
                {
                    var v = (int)value;
                    Envelope2 = v & 1;
                    Envelope1 = (v >> 1) & 1;
                }
            }
        }

        private int f_Envelope1;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( General)")]
        [Description("Envelope 1\r\n" +
            "1 2\r\n" +
            "L L:VCO\r\n" +
            "L H:Only Mixer\r\n" +
            "H L:One Shot\r\n" +
            "H H:1/2 VCO")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((int)0)]
        public int Envelope1
        {
            get
            {
                return f_Envelope1;
            }
            set
            {
                var v = (int)(value & 1);
                if (f_Envelope1 != v)
                {
                    f_Envelope1 = v;

                    SN76477_logic_w(UnitNumber, 4, v);
                }
            }
        }

        private int f_Envelope2;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( General)")]
        [Description("Envelope 2\r\n" +
            "1 2\r\n" +
            "L L: VCO\r\n" +
            "L H: Only Mixer\r\n" +
            "H L: One Shot\r\n" +
            "H H: 1/2 VCO")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((int)0)]
        public int Envelope2
        {
            get
            {
                return f_Envelope2;
            }
            set
            {
                var v = (int)(value & 1);
                if (f_Envelope2 != v)
                {
                    f_Envelope2 = v;

                    SN76477_logic_w(UnitNumber, 5, v);
                }
            }
        }

        #endregion

        #region OneShot

        private double f_OneShot_C = 0.001 * 0.001 * 0.001;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(OneShot)")]
        [Description("OneShot Capacitor")]
        [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)0.001 * 0.001 * 0.001)]
        public double OneShot_C
        {
            get
            {
                return f_OneShot_C;
            }
            set
            {
                if (f_OneShot_C != value)
                {
                    f_OneShot_C = value;

                    SN76477_cap_w(UnitNumber, 0, value);
                }
            }
        }

        private double f_OneShot_C_V = -1;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(OneShot)")]
        [Description("OneShot Capacitor Voltage\r\n" +
            "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
        [DoubleSlideParametersAttribute(-1, 2.5, 0.1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)-1)]
        public double OneShot_C_V
        {
            get
            {
                return f_OneShot_C_V;
            }
            set
            {
                if (f_OneShot_C_V != value)
                {
                    f_OneShot_C_V = value;

                    SN76477_cap_w(UnitNumber, 1, value);
                }
            }
        }

        private double f_OneShot_R = 7.5 * 1000;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(OneShot)")]
        [Description("OneShot Resistor\r\n" +
            "-1:Unconnected\r\n" +
            " 0:Short circuit")]
        [DoubleSlideParametersAttribute(-1, 1000 * 1000, 1000)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)7.5 * 1000)]
        public double OneShot_R
        {
            get
            {
                return f_OneShot_R;
            }
            set
            {
                if (f_OneShot_R != value)
                {
                    f_OneShot_R = value;

                    SN76477_res_w(UnitNumber, 0, value);
                }
            }
        }

        #endregion

        #region SLF

        private double f_SLF_C = 1E-05;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( SLF)")]
        [Description("Super Low Frequency Capacitor")]
        [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)1E-05)]
        public double SLF_C
        {
            get
            {
                return f_SLF_C;
            }
            set
            {
                if (f_SLF_C != value)
                {
                    f_SLF_C = value;

                    SN76477_cap_w(UnitNumber, 2, value);
                }
            }
        }

        private double f_SLF_C_V = -1;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( SLF)")]
        [Description("Super Low Frequency Capacitor Voltage\r\n" +
            "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
        [DoubleSlideParametersAttribute(0, 2.37, 0.1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)-1)]
        public double SLF_C_V
        {
            get
            {
                return f_SLF_C_V;
            }
            set
            {
                if (f_SLF_C_V != value)
                {
                    f_SLF_C_V = value;

                    SN76477_cap_w(UnitNumber, 3, value);
                }
            }
        }

        private double f_SLF_R = 40000;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( SLF)")]
        [Description("Super Low Frequency Resistor\r\n" +
            "-1:Unconnected\r\n" +
            " 0:Short circuit")]
        [DoubleSlideParametersAttribute(-1, 100 * 1000, 1000)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)40000)]
        public double SLF_R
        {
            get
            {
                return f_SLF_R;
            }
            set
            {
                if (f_SLF_R != value)
                {
                    f_SLF_R = value;

                    SN76477_res_w(UnitNumber, 1, value);
                }
            }
        }

        #endregion

        #region VCO

        private int f_VCO = 1;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( VCO)")]
        [Description("VCO On/Off")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((int)1)]
        public int VCO
        {
            get
            {
                return f_VCO;
            }
            set
            {
                var v = (int)(value & 1);
                if (f_VCO != v)
                {
                    f_VCO = v;

                    SN76477_logic_w(UnitNumber, 6, v);
                }
            }
        }

        private double f_VCO_C = 1E-07;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( VCO)")]
        [Description("Voltage Controlled Oscillator Capacitor")]
        [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)1E-07)]
        public double VCO_C
        {
            get
            {
                return f_VCO_C;
            }
            set
            {
                if (f_VCO_C != value)
                {
                    f_VCO_C = value;

                    SN76477_cap_w(UnitNumber, 4, value);
                }
            }
        }

        private double f_VCO_C_V = -1;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( VCO)")]
        [Description("Voltage Controlled Oscillator Capacitor Voltage\r\n" +
            "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
        [DoubleSlideParametersAttribute(-1, 2.37, 0.1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)-1)]
        public double VCO_C_V
        {
            get
            {
                return f_VCO_C_V;
            }
            set
            {
                if (f_VCO_C_V != value)
                {
                    f_VCO_C_V = value;

                    SN76477_cap_w(UnitNumber, 5, value);
                }
            }
        }

        private double f_VCO_V = 5;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( VCO)")]
        [Description("Voltage Controlled Oscillator Voltage")]
        [DoubleSlideParametersAttribute(4, 6, 0.1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)5)]
        public double VCO_V
        {
            get
            {
                return f_VCO_V;
            }
            set
            {
                if (f_VCO_V != value)
                {
                    f_VCO_V = value;

                    SN76477_voltage_w(UnitNumber, 0, value);
                }
            }
        }

        private double f_VCO_R = 40000;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( VCO)")]
        [Description("Voltage Controlled Oscillator Resistor\r\n" +
            "-1:Unconnected\r\n" +
            " 0:Short circuit")]
        [DoubleSlideParametersAttribute(-1, 1 * 1000 * 1000, 1000)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)40000)]
        public double VCO_R
        {
            get
            {
                return f_VCO_R;
            }
            set
            {
                if (f_VCO_R != value)
                {
                    f_VCO_R = value;

                    SN76477_res_w(UnitNumber, 2, value);
                }
            }
        }

        private double f_Pitch_V = 5;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip( VCO)")]
        [Description("Voltage Controlled Oscillator Pitch Voltage")]
        [DoubleSlideParametersAttribute(0, 6, 0.1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)5)]
        public double Pitch_V
        {
            get
            {
                return f_Pitch_V;
            }
            set
            {
                if (f_Pitch_V != value)
                {
                    f_Pitch_V = value;

                    SN76477_voltage_w(UnitNumber, 0, value);
                }
            }
        }

        #endregion

        #region Noise

        private int f_Noise_Clk = 0;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Noise)")]
        [Description("External Noise Clock Input")]
        [SlideParametersAttribute(0, 1)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((int)0)]
        public int NoiseClk
        {
            get
            {
                return f_Noise_Clk;
            }
            set
            {
                var v = (int)(value & 1);
                if (f_Noise_Clk != v)
                {
                    f_Noise_Clk = v;

                    SN76477_logic_w(UnitNumber, 7, v);
                }
            }
        }

        private double f_NoiseClk_R = 47 * 1000;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Noise)")]
        [Description("Noise clock Resistor\r\n" +
            "-1:Unconnected\r\n" +
            " 0:External clock")]
        [DoubleSlideParametersAttribute(-1, 1000 * 1000, 1000)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)47 * 1000)]
        public double NoiseClk_R
        {
            get
            {
                return f_NoiseClk_R;
            }
            set
            {
                if (f_NoiseClk_R != value)
                {
                    f_NoiseClk_R = value;

                    SN76477_res_w(UnitNumber, 3, value);
                }
            }
        }

        private double f_NoiseFilt_C = 1E-07;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Noise)")]
        [Description("Noise filter Capacitor")]
        [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)1E-07)]
        public double NoiseFilt_C
        {
            get
            {
                return f_NoiseFilt_C;
            }
            set
            {
                if (f_NoiseFilt_C != value)
                {
                    f_NoiseFilt_C = value;

                    SN76477_cap_w(UnitNumber, 6, value);
                }
            }
        }

        private double f_NoiseFilt_C_V = -1;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Noise)")]
        [Description("Noise filter Capacitor Voltage\r\n" +
            "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
        [DoubleSlideParametersAttribute(0, 5, 0.1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)-1)]
        public double NoiseFilt_C_V
        {
            get
            {
                return f_NoiseFilt_C_V;
            }
            set
            {
                if (f_NoiseFilt_C_V != value)
                {
                    f_NoiseFilt_C_V = value;

                    SN76477_cap_w(UnitNumber, 7, value);
                }
            }
        }

        private double f_NoiseFilt_R = 4.7 * 1000;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(Noise)")]
        [Description("Noise filter Resistor\r\n" +
            "-1:Unconnected\r\n" +
            " 0:Short circuit")]
        [DoubleSlideParametersAttribute(-1, 10 * 1000, 10)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)4.7 * 1000)]
        public double NoiseFilt_R
        {
            get
            {
                return f_NoiseFilt_R;
            }
            set
            {
                if (f_NoiseFilt_R != value)
                {
                    f_NoiseFilt_R = value;

                    SN76477_res_w(UnitNumber, 4, value);
                }
            }
        }

        #endregion

        #region AtkDcy

        private double f_AtkDcy_C = 0.001 * 0.001 * 0.001;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(AtkDcy)")]
        [Description("Attack Decay Capacitor")]
        [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)0.001 * 0.001 * 0.001)]
        public double AtkDcy_C
        {
            get
            {
                return f_AtkDcy_C;
            }
            set
            {
                if (f_AtkDcy_C != value)
                {
                    f_AtkDcy_C = value;

                    SN76477_cap_w(UnitNumber, 8, value);
                }
            }
        }


        private double f_AtkDcy_C_V = -1;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(AtkDcy)")]
        [Description("Attack Decay Capacitor Voltage\r\n" +
            "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
        [DoubleSlideParametersAttribute(0, 4.44, 0.1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)-1)]
        public double AtkDcy_C_V
        {
            get
            {
                return f_AtkDcy_C_V;
            }
            set
            {
                if (f_AtkDcy_C_V != value)
                {
                    f_AtkDcy_C_V = value;

                    SN76477_cap_w(UnitNumber, 9, value);
                }
            }
        }

        private double f_Atk_R = 5000000;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(AtkDcy)")]
        [Description("Attack Resistor\r\n" +
            "-1:Unconnected\r\n" +
            " 0:Short circuit")]
        [DoubleSlideParametersAttribute(-1, 10 * 1000 * 1000, 1000)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)5000000)]
        public double Atk_R
        {
            get
            {
                return f_Atk_R;
            }
            set
            {
                if (f_Atk_R != value)
                {
                    f_Atk_R = value;

                    SN76477_res_w(UnitNumber, 6, value);
                }
            }
        }

        private double f_Dcy_R = 5000000000;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(AtkDcy)")]
        [Description("Decay Resistor\r\n" +
            "-1:Unconnected\r\n" +
            " 0:Short circuit")]
        [DoubleSlideParametersAttribute(-1, 10 * 1000 * 1000, 1000)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)5000000000)]
        public double Dcy_R
        {
            get
            {
                return f_Dcy_R;
            }
            set
            {
                if (f_Dcy_R != value)
                {
                    f_Dcy_R = value;

                    SN76477_res_w(UnitNumber, 5, value);
                }
            }
        }

        #endregion

        #region ETC

        private double f_Amp_R = 47 * 1000;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(ETC)")]
        [Description("Amplitude Resistor\r\n" +
            "-1:Unconnected\r\n" +
            " 0:Short circuit")]
        [DoubleSlideParametersAttribute(-1, 1 * 1000 * 1000, 1000)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)47 * 1000)]
        public double Amp_R
        {
            get
            {
                return f_Amp_R;
            }
            set
            {
                if (f_Amp_R != value)
                {
                    f_Amp_R = value;

                    SN76477_res_w(UnitNumber, 7, value);
                }
            }
        }

        private double f_Feedback_R = 47 * 1000;

        /// <summary>
        /// </summary>
        [DataMember]
        [Category("Chip(ETC)")]
        [Description("Feedback Resistor\r\n" +
            "-1:Unconnected\r\n" +
            " 0:Short circuit")]
        [DoubleSlideParametersAttribute(-1, 1 * 1000 * 1000, 1000)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DefaultValue((double)47 * 1000)]
        public double Feedback_R
        {
            get
            {
                return f_Feedback_R;
            }
            set
            {
                if (f_Feedback_R != value)
                {
                    f_Feedback_R = value;

                    SN76477_res_w(UnitNumber, 7, value);
                }
            }
        }

        #endregion

        private const float DEFAULT_GAIN = 0.1f;

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
                using (var obj = JsonConvert.DeserializeObject<SN76477>(serializeData))
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
        private delegate void delegate_sn76477_logic_w(uint unitNumber, byte type, int state);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sn76477_logic_w sn76477_logic_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void SN76477_logic_w(uint unitNumber, byte type, int state)
        {
            DeferredWriteData(sn76477_logic_w, unitNumber, type, state);
            /*
            try
            {
                Program.SoundUpdating();
                SN76477_set_clock(unitNumber, state, frequency);
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
        private delegate void delegate_sn76477_res_w(uint unitNumber, byte type, double data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sn76477_res_w sn76477_res_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void SN76477_res_w(uint unitNumber, byte type, double data)
        {
            DeferredWriteData(sn76477_res_w, unitNumber, type, data);
            /*
            try
            {
                Program.SoundUpdating();
                SN76477_set_clock(unitNumber, state, frequency);
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
        private delegate void delegate_sn76477_cap_w(uint unitNumber, byte type, double data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sn76477_cap_w sn76477_cap_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void SN76477_cap_w(uint unitNumber, byte type, double data)
        {
            DeferredWriteData(sn76477_cap_w, unitNumber, type, data);
            /*
            try
            {
                Program.SoundUpdating();
                SN76477_set_clock(unitNumber, state, frequency);
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
        private delegate void delegate_sn76477_voltage_w(uint unitNumber, byte type, double data);


        /// <summary>
        /// 
        /// </summary>
        private static delegate_sn76477_voltage_w sn76477_voltage_w
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        private static void SN76477_voltage_w(uint unitNumber, byte type, double data)
        {
            DeferredWriteData(sn76477_voltage_w, unitNumber, type, data);
            /*
            try
            {
                Program.SoundUpdating();
                SN76477_set_clock(unitNumber, state, frequency);
            }
            finally
            {
                Program.SoundUpdated();
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        static SN76477()
        {
            IntPtr funcPtr = MameIF.GetProcAddress("sn76477_logic_w");
            if (funcPtr != IntPtr.Zero)
            {
                sn76477_logic_w = (delegate_sn76477_logic_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sn76477_logic_w));
            }
            funcPtr = MameIF.GetProcAddress("sn76477_res_w");
            if (funcPtr != IntPtr.Zero)
            {
                sn76477_res_w = (delegate_sn76477_res_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sn76477_res_w));
            }
            funcPtr = MameIF.GetProcAddress("sn76477_cap_w");
            if (funcPtr != IntPtr.Zero)
            {
                sn76477_cap_w = (delegate_sn76477_cap_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sn76477_cap_w));
            }
            funcPtr = MameIF.GetProcAddress("sn76477_voltage_w");
            if (funcPtr != IntPtr.Zero)
            {
                sn76477_voltage_w = (delegate_sn76477_voltage_w)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(delegate_sn76477_voltage_w));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            soundManager?.Dispose();
            base.Dispose();
        }

        private SN76477SoundManager soundManager;

        /// <summary>
        /// 
        /// </summary>
        public SN76477(uint unitNumber) : base(unitNumber)
        {
            GainLeft = DEFAULT_GAIN;
            GainRight = DEFAULT_GAIN;

            Timbres = new SN76477Timbre[InstrumentBase.DEFAULT_MAX_TIMBRES];
            for (int i = 0; i < InstrumentBase.DEFAULT_MAX_TIMBRES; i++)
                Timbres[i] = new SN76477Timbre();
            setPresetInstruments();

            this.soundManager = new SN76477SoundManager(this);
        }

        internal override void PrepareSound()
        {
            SN76477_logic_w(UnitNumber, 0, f_Inhibit);
            SN76477_logic_w(UnitNumber, 1, f_MixerA);
            SN76477_logic_w(UnitNumber, 2, f_MixerB);
            SN76477_logic_w(UnitNumber, 3, f_MixerC);

            SN76477_logic_w(UnitNumber, 6, f_VCO);
            SN76477_logic_w(UnitNumber, 7, f_Noise_Clk);

            SN76477_res_w(UnitNumber, 0, f_OneShot_R);
            SN76477_cap_w(UnitNumber, 0, f_OneShot_C);
            SN76477_cap_w(UnitNumber, 1, f_OneShot_C_V);

            SN76477_res_w(UnitNumber, 1, f_SLF_R);
            SN76477_cap_w(UnitNumber, 2, f_SLF_C);
            SN76477_cap_w(UnitNumber, 3, f_SLF_C_V);

            SN76477_res_w(UnitNumber, 2, f_VCO_R);
            SN76477_cap_w(UnitNumber, 4, f_VCO_C);
            SN76477_cap_w(UnitNumber, 5, f_VCO_C_V);
            SN76477_voltage_w(UnitNumber, 0, f_VCO_V);
            SN76477_voltage_w(UnitNumber, 1, f_Pitch_V);

            SN76477_res_w(UnitNumber, 3, f_NoiseClk_R);
            SN76477_res_w(UnitNumber, 4, f_NoiseFilt_R);
            SN76477_cap_w(UnitNumber, 6, f_NoiseFilt_C);
            SN76477_cap_w(UnitNumber, 7, f_NoiseFilt_C_V);

            SN76477_res_w(UnitNumber, 5, f_Dcy_R);
            SN76477_res_w(UnitNumber, 6, f_Atk_R);
            SN76477_cap_w(UnitNumber, 8, f_AtkDcy_C);
            SN76477_cap_w(UnitNumber, 9, f_AtkDcy_C_V);

            SN76477_res_w(UnitNumber, 7, f_Amp_R);

            SN76477_res_w(UnitNumber, 8, f_Feedback_R);

            base.PrepareSound();
        }

        /// <summary>
        /// 
        /// </summary>
        private void setPresetInstruments()
        {
            Timbres[0].AssignPitch = true;
            Timbres[0].VCO.Enable = true;
            Timbres[0].VCO.VCO_R = 40000;
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

        //
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

        /// <summary>
        /// 
        /// </summary>
        private class SN76477SoundManager : SoundManagerBase
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

            private static SoundList<SN76477Sound> psgOnSounds = new SoundList<SN76477Sound>(1);

            private SN76477 parentModule;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parent"></param>
            public SN76477SoundManager(SN76477 parent) : base(parent)
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
                foreach (SN76477Timbre timbre in parentModule.GetBaseTimbres(note))
                {
                    tindex++;
                    var emptySlot = searchEmptySlot(note);
                    if (emptySlot.slot < 0)
                        continue;

                    SN76477Sound snd = new SN76477Sound(emptySlot.inst, this, timbre, tindex - 1, note, emptySlot.slot);
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
            private (SN76477 inst, int slot) searchEmptySlot(TaggedNoteOnEvent note)
            {
                return SearchEmptySlotAndOffForLeader(parentModule, psgOnSounds, note, 1);
            }

            internal override void ProcessAllSoundOff()
            {
                var me = new ControlChangeEvent((SevenBitNumber)120, (SevenBitNumber)0);
                ProcessControlChange(me);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private class SN76477Sound : SoundBase
        {
            private bool oneshotTrigger;
            private bool amp;
            private bool pitch;

            private SN76477 parentModule;

            private SN76477Timbre timbre;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="parentModule"></param>
            /// <param name="noteOnEvent"></param>
            /// <param name="programNumber"></param>
            /// <param name="slot"></param>
            public SN76477Sound(SN76477 parentModule, SN76477SoundManager manager, TimbreBase timbre, int tindex, TaggedNoteOnEvent noteOnEvent, int slot) : base(parentModule, manager, timbre, tindex, noteOnEvent, slot)
            {
                this.parentModule = parentModule;
                this.timbre = (SN76477Timbre)timbre;

                oneshotTrigger = this.timbre.AssignOneShot;
                amp = this.timbre.AssignAmp;
                pitch = this.timbre.AssignPitch;
            }

            /// <summary>
            /// 
            /// </summary>
            public override void KeyOn()
            {
                base.KeyOn();
                processSoundParams();

                if (oneshotTrigger)
                {
                    parentModule.Inhibit = 1;
                    parentModule.Inhibit = 0;
                }
            }

            private void processSoundParams()
            {
                if (timbre.General.Enable)
                {
                    if (timbre.General.Inhibit.HasValue)
                        parentModule.Inhibit = timbre.General.Inhibit.Value;

                    if (timbre.General.MixerA.HasValue)
                        parentModule.MixerA = timbre.General.MixerA.Value;
                    if (timbre.General.MixerB.HasValue)
                        parentModule.MixerB = timbre.General.MixerB.Value;
                    if (timbre.General.MixerC.HasValue)
                        parentModule.MixerC = timbre.General.MixerC.Value;

                    if (timbre.General.Envelope1.HasValue)
                        parentModule.Envelope1 = timbre.General.Envelope1.Value;
                    if (timbre.General.Envelope2.HasValue)
                        parentModule.Envelope2 = timbre.General.Envelope2.Value;
                }

                if (timbre.VCO.Enable)
                {
                    if (timbre.VCO.VCO.HasValue)
                        parentModule.VCO = timbre.VCO.VCO.Value;

                    if (timbre.VCO.VCO_C.HasValue)
                        parentModule.VCO_C = timbre.VCO.VCO_C.Value;
                    if (timbre.VCO.VCO_C_V.HasValue)
                        parentModule.VCO_C_V = timbre.VCO.VCO_C_V.Value;
                    if (timbre.VCO.VCO_R.HasValue)
                        parentModule.VCO_R = timbre.VCO.VCO_R.Value;
                    if (timbre.VCO.VCO_V.HasValue)
                        parentModule.VCO_V = timbre.VCO.VCO_V.Value;
                    if (timbre.VCO.Pitch_V.HasValue)
                        parentModule.Pitch_V = timbre.VCO.Pitch_V.Value;
                }

                if (timbre.SLF.Enable)
                {
                    if (timbre.SLF.SLF_C.HasValue)
                        parentModule.SLF_C = timbre.SLF.SLF_C.Value;
                    if (timbre.SLF.SLF_C_V.HasValue)
                        parentModule.SLF_C_V = timbre.SLF.SLF_C_V.Value;
                    if (timbre.SLF.SLF_R.HasValue)
                        parentModule.SLF_R = timbre.SLF.SLF_R.Value;
                }

                if (timbre.OneShot.Enable)
                {
                    if (timbre.OneShot.OneShot_C.HasValue)
                        parentModule.OneShot_C = timbre.OneShot.OneShot_C.Value;
                    if (timbre.OneShot.OneShot_C_V.HasValue)
                        parentModule.OneShot_C_V = timbre.OneShot.OneShot_C_V.Value;
                    if (timbre.OneShot.OneShot_R.HasValue)
                        parentModule.OneShot_R = timbre.OneShot.OneShot_R.Value;
                }

                if (timbre.Noise.Enable)
                {
                    if (timbre.Noise.NoiseClk.HasValue)
                        parentModule.NoiseClk = timbre.Noise.NoiseClk.Value;
                    if (timbre.Noise.NoiseClk_R.HasValue)
                        parentModule.NoiseClk_R = timbre.Noise.NoiseClk_R.Value;
                    if (timbre.Noise.NoiseFilt_C.HasValue)
                        parentModule.NoiseFilt_C = timbre.Noise.NoiseFilt_C.Value;
                    if (timbre.Noise.NoiseFilt_C_V.HasValue)
                        parentModule.NoiseFilt_C_V = timbre.Noise.NoiseFilt_C_V.Value;
                    if (timbre.Noise.NoiseFilt_R.HasValue)
                        parentModule.NoiseFilt_R = timbre.Noise.NoiseFilt_R.Value;
                }

                if (timbre.AtkDcy.Enable)
                {
                    if (timbre.AtkDcy.AtkDcy_C.HasValue)
                        parentModule.AtkDcy_C = timbre.AtkDcy.AtkDcy_C.Value;
                    if (timbre.AtkDcy.AtkDcy_C_V.HasValue)
                        parentModule.AtkDcy_C_V = timbre.AtkDcy.AtkDcy_C_V.Value;
                    if (timbre.AtkDcy.Atk_R.HasValue)
                        parentModule.Atk_R = timbre.AtkDcy.Atk_R.Value;
                    if (timbre.AtkDcy.Dcy_R.HasValue)
                        parentModule.Dcy_R = timbre.AtkDcy.Dcy_R.Value;
                }

                if (timbre.Etc.Enable)
                {
                    if (timbre.Etc.Amp_R.HasValue)
                        parentModule.Amp_R = timbre.Etc.Amp_R.Value;
                    if (timbre.Etc.Feedback_R.HasValue)
                        parentModule.Feedback_R = timbre.Etc.Feedback_R.Value;
                }
            }

            public override void OnSoundParamsUpdated()
            {
                processSoundParams();

                base.OnSoundParamsUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void OnPitchUpdated()
            {
                double freq = CalcCurrentFrequency();

                if (pitch && timbre.VCO.Enable && timbre.VCO.VCO_R.HasValue)
                {
                    parentModule.VCO_R = timbre.VCO.VCO_R.Value * (440d / freq);
                }

                base.OnPitchUpdated();
            }

            public override void OnVolumeUpdated()
            {
                if (amp)
                {
                    var vol = CalcCurrentVolume();
                    if (vol == 0)
                        vol = 0;
                    else
                        vol = 1000000 - (vol * 999999);

                    parentModule.Amp_R = vol;
                }

                base.OnVolumeUpdated();
            }

            /// <summary>
            /// 
            /// </summary>
            public override void SoundOff()
            {
                base.SoundOff();

                if (oneshotTrigger)
                {
                    parentModule.Inhibit = 1;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76477Timbre>))]
        [DataContract]
        [InstLock]
        public class SN76477Timbre : TimbreBase
        {

            private bool f_AssignOneShot = true;

            /// <summary>
            /// Assign KeyOn to OneShot trigger.
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Assign KeyOn to OneShot trigger.")]
            [DefaultValue(true)]
            public bool AssignOneShot
            {
                get
                {
                    return f_AssignOneShot;
                }
                set
                {
                    f_AssignOneShot = value;
                }
            }

            private bool f_AssignAmp;

            /// <summary>
            /// Assign Velocity to Amp R.
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Assign Velocity to Amp R. *Oneshot trigger does not work properly.")]
            [DefaultValue(false)]
            public bool AssignAmp
            {
                get
                {
                    return f_AssignAmp;
                }
                set
                {
                    f_AssignAmp = value;
                }
            }

            private bool f_AssignPitch;

            /// <summary>
            /// Assign Pitch to VCO R.
            /// </summary>
            [DataMember]
            [Category("Sound")]
            [Description("Assign Pitch to VCO R when VCO.Enable and VCO.VCO_R is set.")]
            [DefaultValue(false)]
            public bool AssignPitch
            {
                get
                {
                    return f_AssignPitch;
                }
                set
                {
                    f_AssignPitch = value;
                }
            }

            [DataMember]
            [Category("Chip(Genelal)")]
            [Description("Genelal Settings")]
            public SN76477GenalSettings General
            {
                get;
                set;
            }

            [DataMember]
            [Category("Chip(VCO)")]
            [Description("VCO Settings")]
            public SN76477VcoSettings VCO
            {
                get;
                set;
            }

            [DataMember]
            [Category("Chip(SLF)")]
            [Description("SLF Settings")]
            public SN76477SlfSettings SLF
            {
                get;
                set;
            }

            [DataMember]
            [Category("Chip(OneShot)")]
            [Description("OneShot Settings")]
            public SN76477OneShotSettings OneShot
            {
                get;
                set;
            }

            [DataMember]
            [Category("Chip(Noise)")]
            [Description("Noise Settings")]
            public SN76477NoiseSettings Noise
            {
                get;
                set;
            }

            [DataMember]
            [Category("Chip(AtkDcy)")]
            [Description("AtkDcy Settings")]
            public SN76477AtkDcySettings AtkDcy
            {
                get;
                set;
            }

            [DataMember]
            [Category("Chip(ETC)")]
            [Description("AtkDcy Settings")]
            public SN76477EtcSettings Etc
            {
                get;
                set;
            }

            /// <summary>
            /// 
            /// </summary>
            public SN76477Timbre()
            {
                General = new SN76477GenalSettings();
                VCO = new SN76477VcoSettings();
                SLF = new SN76477SlfSettings();
                OneShot = new SN76477OneShotSettings();
                Noise = new SN76477NoiseSettings();
                AtkDcy = new SN76477AtkDcySettings();
                Etc = new SN76477EtcSettings();

                this.SDS.FxS = new BasicFxSettings();
            }

            public override void RestoreFrom(string serializeData)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<SN76477Timbre>(serializeData);
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

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76477GenalSettings>))]
        [DataContract]
        [InstLock]
        public class SN76477GenalSettings : InstLockProxy
        {
            [DataMember]
            [Category("Chip(General)")]
            [Description("Override general settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
            }

            private int? f_Inhibit;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( General)")]
            [Description("Sound Inhibit & One Shot Trigger")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public int? Inhibit
            {
                get
                {
                    return f_Inhibit;
                }
                set
                {
                    var v = (int)(value & 1);
                    if (f_Inhibit != v)
                    {
                        f_Inhibit = v;
                    }
                }
            }

            private int? f_MixerA;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( General)")]
            [Description("Mixer A\r\n" +
                "C B A\r\n" +
                "L L L: VCO\r\n" +
                "L L H: SLF\r\n" +
                "L H L: Noise\r\n" +
                "L H H: VCOxNoise\r\n" +
                "H L L: SLFxNoise\r\n" +
                "H L H: SLFxVCOxNoise\r\n" +
                "H H L: SLFxVCO\r\n" +
                "H H H: Inhibit")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public int? MixerA
            {
                get
                {
                    return f_MixerA;
                }
                set
                {
                    var v = (int)(value & 1);
                    if (f_MixerA != v)
                    {
                        f_MixerA = v;
                    }
                }
            }

            private int? f_MixerB;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( General)")]
            [Description("Mixer B\r\n" +
                "C B A\r\n" +
                "L L L: VCO\r\n" +
                "L L H: SLF\r\n" +
                "L H L: Noise\r\n" +
                "L H H: VCOxNoise\r\n" +
                "H L L: SLFxNoise\r\n" +
                "H L H: SLFxVCOxNoise\r\n" +
                "H H L: SLFxVCO\r\n" +
                "H H H: Inhibit")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public int? MixerB
            {
                get
                {
                    return f_MixerB;
                }
                set
                {
                    var v = (int)(value & 1);
                    if (f_MixerB != v)
                    {
                        f_MixerB = v;
                    }
                }
            }

            private int? f_MixerC;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( General)")]
            [Description("Mixer C\r\n" +
                "C B A\r\n" +
                "L L L: VCO\r\n" +
                "L L H: SLF\r\n" +
                "L H L: Noise\r\n" +
                "L H H: VCOxNoise\r\n" +
                "H L L: SLFxNoise\r\n" +
                "H L H: SLFxVCOxNoise\r\n" +
                "H H L: SLFxVCO\r\n" +
                "H H H: Inhibit")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public int? MixerC
            {
                get
                {
                    return f_MixerC;
                }
                set
                {
                    var v = (int)(value & 1);
                    if (f_MixerC != v)
                    {
                        f_MixerC = v;
                    }
                }
            }

            private int? f_Envelope1;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( General)")]
            [Description("Envelope 1\r\n" +
                "1 2\r\n" +
                "L L:VCO\r\n" +
                "L H:Only Mixer\r\n" +
                "H L:One Shot\r\n" +
                "H H:1/2 VCO")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((int)0)]
            public int? Envelope1
            {
                get
                {
                    return f_Envelope1;
                }
                set
                {
                    var v = (int)(value & 1);
                    if (f_Envelope1 != v)
                    {
                        f_Envelope1 = v;
                    }
                }
            }

            private int? f_Envelope2;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( General)")]
            [Description("Envelope 2\r\n" +
                "1 2\r\n" +
                "L L: VCO\r\n" +
                "L H: Only Mixer\r\n" +
                "H L: One Shot\r\n" +
                "H H: 1/2 VCO")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue((int)0)]
            public int? Envelope2
            {
                get
                {
                    return f_Envelope2;
                }
                set
                {
                    var v = (int)(value & 1);
                    if (f_Envelope2 != v)
                    {
                        f_Envelope2 = v;
                    }
                }
            }

        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76477VcoSettings>))]
        [DataContract]
        [InstLock]
        public class SN76477VcoSettings : InstLockProxy
        {
            [DataMember]
            [Category("Chip(VCO)")]
            [Description("Override VCO settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
            }

            private int? f_VCO;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( VCO)")]
            [Description("VCO On/Off")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public int? VCO
            {
                get
                {
                    return f_VCO;
                }
                set
                {
                    var v = (int)(value & 1);
                    if (f_VCO != v)
                    {
                        f_VCO = v;
                    }
                }
            }

            private double? f_VCO_C;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( VCO)")]
            [Description("Voltage Controlled Oscillator Capacitor")]
            [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? VCO_C
            {
                get
                {
                    return f_VCO_C;
                }
                set
                {
                    if (f_VCO_C != value)
                    {
                        f_VCO_C = value;
                    }
                }
            }


            private double? f_VCO_C_V;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( VCO)")]
            [Description("Voltage Controlled Oscillator Capacitor Voltage\r\n" +
                "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
            [DoubleSlideParametersAttribute(-1, 2.37, 0.1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? VCO_C_V
            {
                get
                {
                    return f_VCO_C_V;
                }
                set
                {
                    if (f_VCO_C_V != value)
                    {
                        f_VCO_C_V = value;
                    }
                }
            }


            private double? f_VCO_V;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( VCO)")]
            [Description("Voltage Controlled Oscillator Voltage")]
            [DoubleSlideParametersAttribute(4, 6, 0.1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? VCO_V
            {
                get
                {
                    return f_VCO_V;
                }
                set
                {
                    if (f_VCO_V != value)
                    {
                        f_VCO_V = value;
                    }
                }
            }

            private double? f_VCO_R;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( VCO)")]
            [Description("Voltage Controlled Oscillator Resistor\r\n" +
                "-1:Unconnected\r\n" +
                " 0:Short circuit")]
            [DoubleSlideParametersAttribute(-1, 1 * 1000 * 1000, 1000)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? VCO_R
            {
                get
                {
                    return f_VCO_R;
                }
                set
                {
                    if (f_VCO_R != value)
                    {
                        f_VCO_R = value;
                    }
                }
            }

            private double? f_Pitch_V;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( VCO)")]
            [Description("Voltage Controlled Oscillator Pitch Voltage")]
            [DoubleSlideParametersAttribute(0, 6, 0.1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? Pitch_V
            {
                get
                {
                    return f_Pitch_V;
                }
                set
                {
                    if (f_Pitch_V != value)
                    {
                        f_Pitch_V = value;
                    }
                }
            }

        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76477SlfSettings>))]
        [DataContract]
        [InstLock]
        public class SN76477SlfSettings : InstLockProxy
        {
            [DataMember]
            [Category("Chip(SLF)")]
            [Description("Override SLF settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
            }


            private double? f_SLF_C;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( SLF)")]
            [Description("Super Low Frequency Capacitor")]
            [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? SLF_C
            {
                get
                {
                    return f_SLF_C;
                }
                set
                {
                    if (f_SLF_C != value)
                    {
                        f_SLF_C = value;
                    }
                }
            }

            private double? f_SLF_C_V;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( SLF)")]
            [Description("Super Low Frequency Capacitor Voltage\r\n" +
                "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
            [DoubleSlideParametersAttribute(0, 2.37, 0.1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? SLF_C_V
            {
                get
                {
                    return f_SLF_C_V;
                }
                set
                {
                    if (f_SLF_C_V != value)
                    {
                        f_SLF_C_V = value;
                    }
                }
            }

            private double? f_SLF_R;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip( SLF)")]
            [Description("Super Low Frequency Resistor\r\n" +
                "-1:Unconnected\r\n" +
                " 0:Short circuit")]
            [DoubleSlideParametersAttribute(-1, 100 * 1000, 1000)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? SLF_R
            {
                get
                {
                    return f_SLF_R;
                }
                set
                {
                    if (f_SLF_R != value)
                    {
                        f_SLF_R = value;
                    }
                }
            }
        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76477OneShotSettings>))]
        [DataContract]
        [InstLock]
        public class SN76477OneShotSettings : InstLockProxy
        {
            [DataMember]
            [Category("Chip(OneShot)")]
            [Description("Override OneShot settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
            }

            private double? f_OneShot_C;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(OneShot)")]
            [Description("OneShot Capacitor")]
            [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? OneShot_C
            {
                get
                {
                    return f_OneShot_C;
                }
                set
                {
                    if (f_OneShot_C != value)
                    {
                        f_OneShot_C = value;
                    }
                }
            }

            private double? f_OneShot_C_V;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(OneShot)")]
            [Description("OneShot Capacitor Voltage\r\n" +
                "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
            [DoubleSlideParametersAttribute(-1, 2.5, 0.1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? OneShot_C_V
            {
                get
                {
                    return f_OneShot_C_V;
                }
                set
                {
                    if (f_OneShot_C_V != value)
                    {
                        f_OneShot_C_V = value;
                    }
                }
            }

            private double? f_OneShot_R;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(OneShot)")]
            [Description("OneShot Resistor\r\n" +
                "-1:Unconnected\r\n" +
                " 0:Short circuit")]
            [DoubleSlideParametersAttribute(-1, 1000 * 1000, 1000)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? OneShot_R
            {
                get
                {
                    return f_OneShot_R;
                }
                set
                {
                    if (f_OneShot_R != value)
                    {
                        f_OneShot_R = value;
                    }
                }
            }

        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76477NoiseSettings>))]
        [DataContract]
        [InstLock]
        public class SN76477NoiseSettings : InstLockProxy
        {
            [DataMember]
            [Category("Chip(Noise)")]
            [Description("Override OneShot settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
            }

            private int? f_Noise_Clk;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(Noise)")]
            [Description("External Noise Clock Input")]
            [SlideParametersAttribute(0, 1)]
            [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public int? NoiseClk
            {
                get
                {
                    return f_Noise_Clk;
                }
                set
                {
                    var v = (int)(value & 1);
                    if (f_Noise_Clk != v)
                    {
                        f_Noise_Clk = v;
                    }
                }
            }

            private double? f_NoiseClk_R;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(Noise)")]
            [Description("Noise clock Resistor\r\n" +
                "-1:Unconnected\r\n" +
                " 0:External clock")]
            [DoubleSlideParametersAttribute(-1, 1000 * 1000, 1000)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? NoiseClk_R
            {
                get
                {
                    return f_NoiseClk_R;
                }
                set
                {
                    if (f_NoiseClk_R != value)
                    {
                        f_NoiseClk_R = value;
                    }
                }
            }

            private double? f_NoiseFilt_C;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(Noise)")]
            [Description("Noise filter Capacitor")]
            [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? NoiseFilt_C
            {
                get
                {
                    return f_NoiseFilt_C;
                }
                set
                {
                    if (f_NoiseFilt_C != value)
                    {
                        f_NoiseFilt_C = value;
                    }
                }
            }

            private double? f_NoiseFilt_C_V;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(Noise)")]
            [Description("Noise filter Capacitor Voltage\r\n" +
                "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
            [DoubleSlideParametersAttribute(0, 5, 0.1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? NoiseFilt_C_V
            {
                get
                {
                    return f_NoiseFilt_C_V;
                }
                set
                {
                    if (f_NoiseFilt_C_V != value)
                    {
                        f_NoiseFilt_C_V = value;
                    }
                }
            }

            private double? f_NoiseFilt_R;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(Noise)")]
            [Description("Noise filter Resistor\r\n" +
                "-1:Unconnected\r\n" +
                " 0:Short circuit")]
            [DoubleSlideParametersAttribute(-1, 10 * 1000, 10)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? NoiseFilt_R
            {
                get
                {
                    return f_NoiseFilt_R;
                }
                set
                {
                    if (f_NoiseFilt_R != value)
                    {
                        f_NoiseFilt_R = value;
                    }
                }
            }
        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76477AtkDcySettings>))]
        [DataContract]
        [InstLock]
        public class SN76477AtkDcySettings : InstLockProxy
        {
            [DataMember]
            [Category("Chip(AtkDcy)")]
            [Description("Override OneShot settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
            }

            private double? f_AtkDcy_C;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(AtkDcy)")]
            [Description("Attack Decay Capacitor")]
            [DoubleSlideParametersAttribute(0.001 * 0.001 * 0.001, 100 * 0.001 * 0.001, 0.001 * 0.001)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? AtkDcy_C
            {
                get
                {
                    return f_AtkDcy_C;
                }
                set
                {
                    if (f_AtkDcy_C != value)
                    {
                        f_AtkDcy_C = value;
                    }
                }
            }

            private double? f_AtkDcy_C_V;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(AtkDcy)")]
            [Description("Attack Decay Capacitor Voltage\r\n" +
                "-1: EXTERNAL_VOLTAGE_DISCONNECT")]
            [DoubleSlideParametersAttribute(0, 4.44, 0.1)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? AtkDcy_C_V
            {
                get
                {
                    return f_AtkDcy_C_V;
                }
                set
                {
                    if (f_AtkDcy_C_V != value)
                    {
                        f_AtkDcy_C_V = value;
                    }
                }
            }

            private double? f_Atk_R;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(AtkDcy)")]
            [Description("Attack Resistor\r\n" +
                "-1:Unconnected\r\n" +
                " 0:Short circuit")]
            [DoubleSlideParametersAttribute(-1, 10 * 1000 * 1000, 1000)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? Atk_R
            {
                get
                {
                    return f_Atk_R;
                }
                set
                {
                    if (f_Atk_R != value)
                    {
                        f_Atk_R = value;
                    }
                }
            }

            private double? f_Dcy_R;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(AtkDcy)")]
            [Description("Decay Resistor\r\n" +
                "-1:Unconnected\r\n" +
                " 0:Short circuit")]
            [DoubleSlideParametersAttribute(-1, 10 * 1000 * 1000, 1000)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? Dcy_R
            {
                get
                {
                    return f_Dcy_R;
                }
                set
                {
                    if (f_Dcy_R != value)
                    {
                        f_Dcy_R = value;
                    }
                }
            }

        }

        [TypeConverter(typeof(CustomExpandableObjectConverter))]
        [JsonConverter(typeof(NoTypeConverterJsonConverter<SN76477EtcSettings>))]
        [DataContract]
        [InstLock]
        public class SN76477EtcSettings : InstLockProxy
        {
            [DataMember]
            [Category("Chip(ETC)")]
            [Description("Override ETC settings")]
            [DefaultValue(false)]
            public bool Enable
            {
                get;
                set;
            }

            private double? f_Amp_R;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(ETC)")]
            [Description("Amplitude Resistor\r\n" +
                "-1:Unconnected\r\n" +
                " 0:Short circuit")]
            [DoubleSlideParametersAttribute(-1, 1 * 1000 * 1000, 1000)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? Amp_R
            {
                get
                {
                    return f_Amp_R;
                }
                set
                {
                    if (f_Amp_R != value)
                    {
                        f_Amp_R = value;
                    }
                }
            }

            private double? f_Feedback_R;

            /// <summary>
            /// </summary>
            [DataMember]
            [Category("Chip(ETC)")]
            [Description("Feedback Resistor\r\n" +
                "-1:Unconnected\r\n" +
                " 0:Short circuit")]
            [DoubleSlideParametersAttribute(-1, 1 * 1000 * 1000, 1000)]
            [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [DefaultValue(null)]
            public double? Feedback_R
            {
                get
                {
                    return f_Feedback_R;
                }
                set
                {
                    if (f_Feedback_R != value)
                    {
                        f_Feedback_R = value;
                    }
                }
            }
        }
    }

    public enum MixerSet
    {
        VCO,
        SLF,
        Noise,
        VCOxNoise,
        SLFxNoise,
        SLFxVCOxNoise,
        SLFxVCO,
        Inhibit,
    }

    public enum EnvelopeSet
    {
        VCO,
        OnlyMixer,
        OneShot,
        HalfVCO,
    }
}