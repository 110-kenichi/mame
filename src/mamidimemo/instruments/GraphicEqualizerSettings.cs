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

        private PeakingFilter gain100HzFilter = new PeakingFilter();

        private double f_Gain100Hz;

        [DataMember]
        [Description("Gain for around 100 Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-30d, 30d, 0.1d)]
        public double Gain100Hz
        {
            get
            {
                return f_Gain100Hz;
            }
            set
            {
                if (f_Gain100Hz != value)
                {
                    f_Gain100Hz = value;
                    gain100HzFilter.SetParameters(100.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                }
            }
        }

        private PeakingFilter gain200HzFilter = new PeakingFilter();

        private double f_Gain200Hz;

        [DataMember]
        [Description("Gain for around 200 Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-30d, 30d, 0.1d)]
        public double Gain200Hz
        {
            get
            {
                return f_Gain200Hz;
            }
            set
            {
                if (f_Gain200Hz != value)
                {
                    f_Gain200Hz = value;
                    gain200HzFilter.SetParameters(200.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                }
            }
        }

        private PeakingFilter gain400HzFilter = new PeakingFilter();

        private double f_Gain400Hz;

        [DataMember]
        [Description("Gain for around 400 Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-30d, 30d, 0.1d)]
        public double Gain400Hz
        {
            get
            {
                return f_Gain400Hz;
            }
            set
            {
                if (f_Gain400Hz != value)
                {
                    f_Gain400Hz = value;
                    gain400HzFilter.SetParameters(400.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                }
            }
        }

        private PeakingFilter gain800HzFilter = new PeakingFilter();

        private double f_Gain800Hz;

        [DataMember]
        [Description("Gain for around 800 Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-30d, 30d, 0.1d)]
        public double Gain800Hz
        {
            get
            {
                return f_Gain800Hz;
            }
            set
            {
                if (f_Gain800Hz != value)
                {
                    f_Gain800Hz = value;
                    gain800HzFilter.SetParameters(800.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                }
            }
        }

        private PeakingFilter gain1600HzFilter = new PeakingFilter();

        private double f_Gain1600Hz;

        [DataMember]
        [Description("Gain for around 1.6k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-30d, 30d, 0.1d)]
        public double Gain1600Hz
        {
            get
            {
                return f_Gain1600Hz;
            }
            set
            {
                if (f_Gain1600Hz != value)
                {
                    f_Gain1600Hz = value;
                    gain1600HzFilter.SetParameters(1600.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                }
            }
        }

        private PeakingFilter gain3200HzFilter = new PeakingFilter();

        private double f_Gain3200Hz;

        [DataMember]
        [Description("Gain for around 3.2k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-30d, 30d, 0.1d)]
        public double Gain3200Hz
        {
            get
            {
                return f_Gain3200Hz;
            }
            set
            {
                if (f_Gain3200Hz != value)
                {
                    f_Gain3200Hz = value;
                    gain3200HzFilter.SetParameters(3200.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                }
            }
        }

        private PeakingFilter gain6400HzFilter = new PeakingFilter();

        private double f_Gain6400Hz;

        [DataMember]
        [Description("Gain for around 6.4k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-30d, 30d, 0.1d)]
        public double Gain6400Hz
        {
            get
            {
                return f_Gain6400Hz;
            }
            set
            {
                if (f_Gain6400Hz != value)
                {
                    f_Gain6400Hz = value;
                    gain6400HzFilter.SetParameters(6400.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                }
            }
        }

        private PeakingFilter gain12800HzFilter = new PeakingFilter();

        private double f_Gain12800Hz;

        [DataMember]
        [Description("Gain for around 12.8k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-30d, 30d, 0.1d)]
        public double Gain12800Hz
        {
            get
            {
                return f_Gain12800Hz;
            }
            set
            {
                if (f_Gain12800Hz != value)
                {
                    f_Gain12800Hz = value;
                    gain12800HzFilter.SetParameters(12800.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                }
            }
        }

        private PeakingFilter gain25600HzFilter = new PeakingFilter();

        private double f_Gain25600Hz;

        [DataMember]
        [Description("Gain for around 25.6k Hz")]
        [DefaultValue(0.0d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(UITypeEditor))]
        [DoubleSlideParameters(-30d, 30d, 0.1d)]
        public double Gain25600Hz
        {
            get
            {
                return f_Gain25600Hz;
            }
            set
            {
                if (f_Gain25600Hz != value)
                {
                    f_Gain25600Hz = value;
                    gain25600HzFilter.SetParameters(25600.0d, 1.0d, value, (double)Program.CurrentSamplingRate);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public GraphicEqualizerSettings()
        {
            gain100HzFilter.SetParameters(100.0d, 1.0d, 0, (double)Program.CurrentSamplingRate);
            gain200HzFilter.SetParameters(200.0d, 1.0d, 0, (double)Program.CurrentSamplingRate);
            gain400HzFilter.SetParameters(400.0d, 1.0d, 0, (double)Program.CurrentSamplingRate);
            gain800HzFilter.SetParameters(800.0d, 1.0d, 0, (double)Program.CurrentSamplingRate);
            gain1600HzFilter.SetParameters(1600.0d, 1.0d, 0, (double)Program.CurrentSamplingRate);
            gain3200HzFilter.SetParameters(3200.0d, 1.0d, 0, (double)Program.CurrentSamplingRate);
            gain6400HzFilter.SetParameters(6400.0d, 1.0d, 0, (double)Program.CurrentSamplingRate);
            gain12800HzFilter.SetParameters(12800.0d, 1.0d, 0, (double)Program.CurrentSamplingRate);
            gain25600HzFilter.SetParameters(25600.0d, 1.0d, 0, (double)Program.CurrentSamplingRate);
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
            // 入力信号と正弦波を掛け合わせる
            for (int i = 0; i < samples; i++)
            {
                double[] tmpBuf = new double[] { buf[0][i], buf[1][i] };

                // 入力信号にフィルタを直列にかける(左分)
                gain100HzFilter.Process(tmpBuf);
                gain200HzFilter.Process(tmpBuf);
                gain400HzFilter.Process(tmpBuf);
                gain800HzFilter.Process(tmpBuf);
                gain1600HzFilter.Process(tmpBuf);
                gain3200HzFilter.Process(tmpBuf);
                gain6400HzFilter.Process(tmpBuf);
                gain12800HzFilter.Process(tmpBuf);
                gain25600HzFilter.Process(tmpBuf);

                // フィルタをかけた信号を出力する
                buf[0][i] = (int)Math.Round(tmpBuf[0]);
                buf[1][i] = (int)Math.Round(tmpBuf[1]);
            }
        }

        //https://www.utsbox.com/?page_id=728

        /// <summary>
        /// 
        /// </summary>
        private class PeakingFilter
        {
            // フィルタの係数
            private double a0, a1, a2, b0, b1, b2;
            // バッファ
            private double out1_R, out2_R;
            private double in1_R, in2_R;
            private double out1_L, out2_L;
            private double in1_L, in2_L;

            /// <summary>
            /// 
            /// </summary>
            public PeakingFilter()
            {

            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="inputData"></param>
            /// <returns></returns>
            public void Process(double[] inputData)
            {
                {
                    // 入力信号にフィルタを適用し、出力信号変数に保存。
                    double outData_R = b0 / a0 * inputData[1] + b1 / a0 * in1_R + b2 / a0 * in2_R
                        - a1 / a0 * out1_R - a2 / a0 * out2_R;

                    in2_R = in1_R; // 2つ前の入力信号を更新
                    in1_R = inputData[1];  // 1つ前の入力信号を更新

                    out2_R = out1_R; // 2つ前の出力信号を更新
                    out1_R = outData_R;  // 1つ前の出力信号を更新

                    // 出力信号を返す
                    inputData[1] = outData_R;
                }
                {
                    // 入力信号にフィルタを適用し、出力信号変数に保存。
                    double outData_L = b0 / a0 * inputData[0] + b1 / a0 * in1_R + b2 / a0 * in2_R
                        - a1 / a0 * out1_R - a2 / a0 * out2_R;

                    in2_L = in1_L; // 2つ前の入力信号を更新
                    in1_L = inputData[0];  // 1つ前の入力信号を更新

                    out2_L = out1_L; // 2つ前の出力信号を更新
                    out1_L = outData_L;  // 1つ前の出力信号を更新

                    // 出力信号を返す
                    inputData[0] = outData_L;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="freq"></param>
            /// <param name="bw"></param>
            /// <param name="gain"></param>
            /// <param name="samplerate"></param>
            public void SetParameters(double freq, double bw, double gain, double samplerate)
            {
                // フィルタ係数計算で使用する中間値を求める。
                double omega = 2.0d * 3.14159265d * freq / samplerate;
                double alpha = Math.Sin(omega) * Math.Sinh(Math.Log(2.0d) / 2.0 * bw * omega / Math.Sin(omega));
                double A = Math.Pow(10.0d, (gain / 40.0d));

                // フィルタ係数を求める。
                a0 = 1.0d + alpha / A;
                a1 = -2.0d * Math.Cos(omega);
                a2 = 1.0d - alpha / A;
                b0 = 1.0d + alpha * A;
                b1 = -2.0d * Math.Cos(omega);
                b2 = 1.0d - alpha * A;
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

    }

}

