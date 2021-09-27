using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments.Envelopes
{

    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<AdsrSettings>))]
    [DataContract]
    [InstLock]
    public class AdsrSettings : ContextBoundObject
    {
        #region ADSR 

        private bool f_Enable;

        [DataMember]
        [Description("Whether enable Sound Driver Level ADSR")]
        [DefaultValue(false)]
        public bool Enable
        {
            get
            {
                return f_Enable;
            }
            set
            {
                if (f_Enable != value)
                {
                    f_Enable = value;
                }
            }
        }

        /*  TODO:
        private byte f_KSR;

        /// <summary>
        /// Keyboard scaling rate (0-1)
        /// </summary>
        [DataMember]
        [Description("Sound Driver Level Key Scaling Rate")]
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

        private byte f_KSL;

        /// <summary>
        /// Key Scaling Level(0-3)
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Key Scaling Level (00:No Change 10:1.5dB/8ve 01:3dB/8ve 11:6dB/8ve)")]
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
        */

        private byte f_AR = 110;

        /// <summary>
        /// Attack Rate (0(10s)-127(0s))
        /// 0s - 10s
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Attack Rate (0(max)-127(0s))")]
        [DefaultValue((byte)110)]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte AR
        {
            get
            {
                return f_AR;
            }
            set
            {
                f_AR = (byte)(value & 127);
            }
        }

        private byte f_DR = 64;

        /// <summary>
        /// Decay Rate (0(0s)-127(10s))
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Decay Rate  (0(0s)-127(max))")]
        [DefaultValue((byte)64)]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte DR
        {
            get
            {
                return f_DR;
            }
            set
            {
                f_DR = (byte)(value & 127);
            }
        }

        private byte f_SL = 16;

        /// <summary>
        /// Sustain Level (0(min)-127(max))
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Sustain Level (0(none)-127(max))")]
        [DefaultValue((byte)16)]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte SL
        {
            get
            {
                return f_SL;
            }
            set
            {
                f_SL = (byte)(value & 127);
            }
        }

        private byte f_RR = 48;

        /// <summary>
        /// release rate (0(0s)-127(60s))
        /// </summary>
        [DataMember]
        [Category("Sound")]
        [Description("Sound Driver Level Release Rate (0(0s)-127(max))")]
        [DefaultValue((byte)48)]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte RR
        {
            get
            {
                return f_RR;
            }
            set
            {
                f_RR = (byte)(value & 127);
            }
        }

        #endregion

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
                var obj = JsonConvert.DeserializeObject<AdsrSettings>(serializeData);
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
}
