// copyright-holders:K.Ito
using MathParserTK;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<GraphicEqualizerSettings>))]
    [EditorAttribute(typeof(GraphicEqulizerEditor), typeof(UITypeEditor))]
    [DataContract]
    [InstLock]
    public class GraphicEqualizerSettings : ContextBoundObject
    {
        [DataMember]
        [Description("Whether enable graphic equalizer.")]
        [DefaultValue(false)]
        public bool Enable
        {
            get;
            set;
        }

        private PeakFilter gain31HzFilter;

        private double f_Gain31Hz;

        [DataMember]
        [Description("Gain for around 31 Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-15d, 15d, 0.1d)]
        public double Gain31Hz
        {
            get
            {
                return f_Gain31Hz;
            }
            set
            {
                if (f_Gain31Hz != value)
                {
                    f_Gain31Hz = value;
                    //gain31HzFilter.SetParameters(31.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                    gain31HzFilter.GainDB = value;
                }
            }
        }

        private PeakFilter gain62HzFilter;

        private double f_Gain62Hz;

        [DataMember]
        [Description("Gain for around 62 Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-15d, 15d, 0.1d)]
        public double Gain62Hz
        {
            get
            {
                return f_Gain62Hz;
            }
            set
            {
                if (f_Gain62Hz != value)
                {
                    f_Gain62Hz = value;
                    gain62HzFilter.GainDB = value;
                }
            }
        }

        private PeakFilter gain125HzFilter;

        private double f_Gain125Hz;

        [DataMember]
        [Description("Gain for around 125 Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-15d, 15d, 0.1d)]
        public double Gain125Hz
        {
            get
            {
                return f_Gain125Hz;
            }
            set
            {
                if (f_Gain125Hz != value)
                {
                    f_Gain125Hz = value;
                    gain125HzFilter.GainDB = value;
                }
            }
        }

        private PeakFilter gain250HzFilter;

        private double f_Gain250Hz;

        [DataMember]
        [Description("Gain for around 250 Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-15d, 15d, 0.1d)]
        public double Gain250Hz
        {
            get
            {
                return f_Gain250Hz;
            }
            set
            {
                if (f_Gain250Hz != value)
                {
                    f_Gain250Hz = value;
                    gain250HzFilter.GainDB = value;
                }
            }
        }

        private PeakFilter gain500HzFilter;

        private double f_Gain500Hz;

        [DataMember]
        [Description("Gain for around 500 Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-15d, 15d, 0.1d)]
        public double Gain500Hz
        {
            get
            {
                return f_Gain500Hz;
            }
            set
            {
                if (f_Gain500Hz != value)
                {
                    f_Gain500Hz = value;
                    gain500HzFilter.GainDB = value;
                }
            }
        }

        private PeakFilter gain1000HzFilter;

        private double f_Gain1000Hz;

        [DataMember]
        [Description("Gain for around 1k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-15d, 15d, 0.1d)]
        public double Gain1000Hz
        {
            get
            {
                return f_Gain1000Hz;
            }
            set
            {
                if (f_Gain1000Hz != value)
                {
                    f_Gain1000Hz = value;
                    gain1000HzFilter.GainDB = value;
                }
            }
        }

        private PeakFilter gain2000HzFilter;

        private double f_Gain2000Hz;

        [DataMember]
        [Description("Gain for around 2k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-15d, 15d, 0.1d)]
        public double Gain2000Hz
        {
            get
            {
                return f_Gain2000Hz;
            }
            set
            {
                if (f_Gain2000Hz != value)
                {
                    f_Gain2000Hz = value;
                    gain2000HzFilter.GainDB = value;
                }
            }
        }

        private PeakFilter gain4000HzFilter;

        private double f_Gain4000Hz;

        [DataMember]
        [Description("Gain for around 4k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-15d, 15d, 0.1d)]
        public double Gain4000Hz
        {
            get
            {
                return f_Gain4000Hz;
            }
            set
            {
                if (f_Gain4000Hz != value)
                {
                    f_Gain4000Hz = value;
                    gain4000HzFilter.GainDB = value;
                }
            }
        }

        private PeakFilter gain8000HzFilter;

        private double f_Gain8000Hz;

        [DataMember]
        [Description("Gain for around 8k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-15d, 15d, 0.1d)]
        public double Gain8000Hz
        {
            get
            {
                return f_Gain8000Hz;
            }
            set
            {
                if (f_Gain8000Hz != value)
                {
                    f_Gain8000Hz = value;
                    gain8000HzFilter.GainDB = value;
                }
            }
        }


        private PeakFilter gain16000HzFilter;

        private double f_Gain16000Hz;

        [DataMember]
        [Description("Gain for around 16k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0d, 100d, 0.1d)]
        public double Gain16000Hz
        {
            get
            {
                return f_Gain16000Hz;
            }
            set
            {
                if (f_Gain16000Hz != value)
                {
                    f_Gain16000Hz = value;
                    gain16000HzFilter.GainDB = value;
                }
            }
        }





        private double f_BandWidth31Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 31 Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth31Hz
        {
            get
            {
                return f_BandWidth31Hz;
            }
            set
            {
                if (value <= 0)
                    return;
                if (f_BandWidth31Hz != value)
                {
                    f_BandWidth31Hz = value;
                    gain31HzFilter.BandWidth = value;
                }
            }
        }

        private double f_BandWidth62Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 62 Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth62Hz
        {
            get
            {
                return f_BandWidth62Hz;
            }
            set
            {
                if (value <= 0)
                    return;
                if (f_BandWidth62Hz != value)
                {
                    f_BandWidth62Hz = value;
                    gain62HzFilter.BandWidth = value;
                }
            }
        }

        private double f_BandWidth125Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 125 Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth125Hz
        {
            get
            {
                return f_BandWidth125Hz;
            }
            set
            {
                if (value <= 0)
                    return;
                if (f_BandWidth125Hz != value)
                {
                    f_BandWidth125Hz = value;
                    gain125HzFilter.BandWidth = value;
                }
            }
        }

        private double f_BandWidth250Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 250 Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth250Hz
        {
            get
            {
                return f_BandWidth250Hz;
            }
            set
            {
                if (value <= 0)
                    return;
                if (f_BandWidth250Hz != value)
                {
                    f_BandWidth250Hz = value;
                    gain250HzFilter.BandWidth = value;
                }
            }
        }

        private double f_BandWidth500Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 500 Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth500Hz
        {
            get
            {
                return f_BandWidth500Hz;
            }
            set
            {
                if (value <= 0)
                    return;
                if (f_BandWidth500Hz != value)
                {
                    f_BandWidth500Hz = value;
                    gain500HzFilter.BandWidth = value;
                }
            }
        }

        private double f_BandWidth1000Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 1k Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth1000Hz
        {
            get
            {
                return f_BandWidth1000Hz;
            }
            set
            {
                if (f_BandWidth1000Hz != value)
                {
                    f_BandWidth1000Hz = value;
                    gain1000HzFilter.BandWidth = value;
                }
            }
        }

        private double f_BandWidth2000Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 2k Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth2000Hz
        {
            get
            {
                return f_BandWidth2000Hz;
            }
            set
            {
                if (value <= 0)
                    return;
                if (f_BandWidth2000Hz != value)
                {
                    f_BandWidth2000Hz = value;
                    gain2000HzFilter.BandWidth = value;
                }
            }
        }

        private double f_BandWidth4000Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 4k Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth4000Hz
        {
            get
            {
                return f_BandWidth4000Hz;
            }
            set
            {
                if (value <= 0)
                    return;
                if (f_BandWidth4000Hz != value)
                {
                    f_BandWidth4000Hz = value;
                    gain4000HzFilter.BandWidth = value;
                }
            }
        }

        private double f_BandWidth8000Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 8k Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth8000Hz
        {
            get
            {
                return f_BandWidth8000Hz;
            }
            set
            {
                if (value <= 0)
                    return;
                if (f_BandWidth8000Hz != value)
                {
                    f_BandWidth8000Hz = value;
                    gain8000HzFilter.BandWidth = value;
                }
            }
        }

        private double f_BandWidth16000Hz = 1.41d;

        [DataMember]
        [Description("BandWidth for around 16k Hz")]
        [DefaultValue(1.41d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(0.1d, 100d, 0.1d)]
        public double BandWidth16000Hz
        {
            get
            {
                return f_BandWidth16000Hz;
            }
            set
            {
                if (value <= 0)
                    return;
                if (f_BandWidth16000Hz != value)
                {
                    f_BandWidth16000Hz = value;
                    gain16000HzFilter.BandWidth = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public GraphicEqualizerSettings()
        {
            var sampleRate = Program.CurrentSamplingRate;
            double defaultGain = 0;
            gain31HzFilter = new PeakFilter(sampleRate, 31, f_BandWidth31Hz, defaultGain);
            gain62HzFilter = new PeakFilter(sampleRate, 62, f_BandWidth62Hz, defaultGain);
            gain125HzFilter = new PeakFilter(sampleRate, 125, f_BandWidth125Hz, defaultGain);
            gain250HzFilter = new PeakFilter(sampleRate, 250, f_BandWidth250Hz, defaultGain);
            gain500HzFilter = new PeakFilter(sampleRate, 500, f_BandWidth500Hz, defaultGain);
            gain1000HzFilter = new PeakFilter(sampleRate, 1000, f_BandWidth1000Hz, defaultGain);
            gain2000HzFilter = new PeakFilter(sampleRate, 2000, f_BandWidth2000Hz, defaultGain);
            gain4000HzFilter = new PeakFilter(sampleRate, 4000, f_BandWidth4000Hz, defaultGain);
            gain8000HzFilter = new PeakFilter(sampleRate, 8000, f_BandWidth8000Hz, defaultGain);
            gain16000HzFilter = new PeakFilter(sampleRate, 16000, f_BandWidth16000Hz, defaultGain);
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
        [Description("You can copy and paste this text data to other same type timber.\r\nNote: Open dropdown editor then copy all text and paste to dropdown editor. Do not copy and paste one liner text.")]
        [DefaultValue("{}")]
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
                var obj = JsonConvert.DeserializeObject<GraphicEqualizerSettings>(serializeData);
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

        public override bool Equals(object obj)
        {
            var mdsobj = obj as GraphicEqualizerSettings;
            if (mdsobj == null)
                return false;

            return string.Equals(SerializeData, mdsobj.SerializeData, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return SerializeData.GetHashCode();
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="samples"></param>
        public void ProcessCallback(int[][] buf, int samples)
        {
            for (int i = 0; i < samples; i++)
            {
                gain31HzFilter.Process(ref buf[0][i], ref buf[1][i]);
                gain62HzFilter.Process(ref buf[0][i], ref buf[1][i]);
                gain125HzFilter.Process(ref buf[0][i], ref buf[1][i]);
                gain250HzFilter.Process(ref buf[0][i], ref buf[1][i]);
                gain500HzFilter.Process(ref buf[0][i], ref buf[1][i]);
                gain1000HzFilter.Process(ref buf[0][i], ref buf[1][i]);
                gain2000HzFilter.Process(ref buf[0][i], ref buf[1][i]);
                gain4000HzFilter.Process(ref buf[0][i], ref buf[1][i]);
                gain8000HzFilter.Process(ref buf[0][i], ref buf[1][i]);
                gain16000HzFilter.Process(ref buf[0][i], ref buf[1][i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class GraphicEqulizerEditor : System.Drawing.Design.UITypeEditor
        {

            public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                var service = provider.GetService
                    (typeof(System.Windows.Forms.Design.IWindowsFormsEditorService))
                        as System.Windows.Forms.Design.IWindowsFormsEditorService;

                GraphicEqulizer track = new GraphicEqulizer();
                track.Settings = (GraphicEqualizerSettings)value;

                service.DropDownControl(track);

                return value;
            }
        }

        /// <summary>
        /// Represents a biquad-filter.
        /// </summary>
        public abstract class BiQuad
        {
            /// <summary>
            /// The a0 value.
            /// </summary>
            protected double A0;
            /// <summary>
            /// The a1 value.
            /// </summary>
            protected double A1;
            /// <summary>
            /// The a2 value.
            /// </summary>
            protected double A2;
            /// <summary>
            /// The b1 value.
            /// </summary>
            protected double B1;
            /// <summary>
            /// The b2 value.
            /// </summary>
            protected double B2;
            /// <summary>
            /// The q value.
            /// </summary>
            private double _q;
            /// <summary>
            /// The gain value in dB.
            /// </summary>
            private double _gainDB;

            /// <summary>
            /// The z1 value.
            /// </summary>
            protected double Z1_L;
            /// <summary>
            /// The z2 value.
            /// </summary>
            protected double Z2_L;

            /// <summary>
            /// The z1 value.
            /// </summary>
            protected double Z1_R;
            /// <summary>
            /// The z2 value.
            /// </summary>
            protected double Z2_R;

            private double _frequency;

            /// <summary>
            /// Gets or sets the frequency.
            /// </summary>
            /// <exception cref="System.ArgumentOutOfRangeException">value;The samplerate has to be bigger than 2 * frequency.</exception>
            public double Frequency
            {
                get { return _frequency; }
                set
                {
                    _frequency = value;
                    CalculateBiQuadCoefficients();
                }
            }

            /// <summary>
            /// Gets the sample rate.
            /// </summary>
            public int SampleRate { get; private set; }

            /// <summary>
            /// The q value.
            /// </summary>
            public double Q
            {
                get { return _q; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    _q = value;
                    CalculateBiQuadCoefficients();
                }
            }

            /// <summary>
            /// Gets or sets the gain value in dB.
            /// </summary>
            public double GainDB
            {
                get { return _gainDB; }
                set
                {
                    _gainDB = value;
                    CalculateBiQuadCoefficients();
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BiQuad"/> class.
            /// </summary>
            /// <param name="sampleRate">The sample rate.</param>
            /// <param name="frequency">The frequency.</param>
            /// <exception cref="System.ArgumentOutOfRangeException">
            /// sampleRate
            /// or
            /// frequency
            /// or
            /// q
            /// </exception>
            protected BiQuad(int sampleRate, double frequency)
                : this(sampleRate, frequency, 1.0 / Math.Sqrt(2))
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="BiQuad"/> class.
            /// </summary>
            /// <param name="sampleRate">The sample rate.</param>
            /// <param name="frequency">The frequency.</param>
            /// <param name="q">The q.</param>
            /// <exception cref="System.ArgumentOutOfRangeException">
            /// sampleRate
            /// or
            /// frequency
            /// or
            /// q
            /// </exception>
            protected BiQuad(int sampleRate, double frequency, double q)
            {
                if (sampleRate <= 0)
                    throw new ArgumentOutOfRangeException("sampleRate");
                if (frequency <= 0)
                    throw new ArgumentOutOfRangeException("frequency");
                if (q <= 0)
                    throw new ArgumentOutOfRangeException("q");
                SampleRate = sampleRate;
                Frequency = frequency;
                Q = q;
                GainDB = 6;
            }

            /// <summary>
            /// Processes a single <paramref name="inputL"/> sample and returns the result.
            /// </summary>
            /// <param name="inputL">The input sample to process.</param>
            /// <returns>The result of the processed <paramref name="inputL"/> sample.</returns>
            public void Process(ref int inputL, ref int inputR)
            {
                if (SampleRate < Frequency * 2)
                    return;

                double o = inputL * A0 + Z1_L;
                Z1_L = inputL * A1 + Z2_L - B1 * o;
                Z2_L = inputL * A2 - B2 * o;
                inputL = (int)o;

                o = inputR * A0 + Z1_R;
                Z1_R = inputR * A1 + Z2_R - B1 * o;
                Z2_R = inputR * A2 - B2 * o;
                inputR = (int)o;
            }

            /// <summary>
            /// Calculates all coefficients.
            /// </summary>
            protected abstract void CalculateBiQuadCoefficients();
        }

        //https://github.com/filoe/cscore/blob/master/CSCore/DSP/PeakFilter.cs

        /// <summary>
        /// Used to apply an peak-filter to a signal.
        /// </summary>
        public class PeakFilter : BiQuad
        {
            /// <summary>
            /// Gets or sets the bandwidth.
            /// </summary>
            public double BandWidth
            {
                get { return Q; }
                set
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException("value");
                    Q = value;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PeakFilter"/> class.
            /// </summary>
            /// <param name="sampleRate">The sampleRate of the audio data to process.</param>
            /// <param name="frequency">The center frequency to adjust.</param>
            /// <param name="bandWidth">The bandWidth.</param>
            /// <param name="peakGainDB">The gain value in dB.</param>
            public PeakFilter(int sampleRate, double frequency, double bandWidth, double peakGainDB)
                : base(sampleRate, frequency, bandWidth)
            {
                GainDB = peakGainDB;
            }

            /// <summary>
            /// Calculates all coefficients.
            /// </summary>
            protected override void CalculateBiQuadCoefficients()
            {
                double norm;
                double v = Math.Pow(10, Math.Abs(GainDB) / 20.0);
                double k = Math.Tan(Math.PI * Frequency / SampleRate);
                double q = Q;

                if (GainDB >= 0) //boost
                {
                    norm = 1 / (1 + 1 / q * k + k * k);
                    A0 = (1 + v / q * k + k * k) * norm;
                    A1 = 2 * (k * k - 1) * norm;
                    A2 = (1 - v / q * k + k * k) * norm;
                    B1 = A1;
                    B2 = (1 - 1 / q * k + k * k) * norm;
                }
                else //cut
                {
                    norm = 1 / (1 + v / q * k + k * k);
                    A0 = (1 + 1 / q * k + k * k) * norm;
                    A1 = 2 * (k * k - 1) * norm;
                    A2 = (1 - 1 / q * k + k * k) * norm;
                    B1 = A1;
                    B2 = (1 - v / q * k + k * k) * norm;
                }
            }
        }
    }

}

