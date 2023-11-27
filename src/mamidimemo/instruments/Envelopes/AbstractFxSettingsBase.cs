using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments.Envelopes
{

    [JsonConverter(typeof(NoTypeConverterJsonConverter<AbstractFxSettingsBase>))]
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [DataContract]
    [InstLock]
    public abstract class AbstractFxSettingsBase : ContextBoundObject, ISerializeDataSaveLoad
    {
        private bool f_Enable;

        [DataMember]
        [Description("Whether enable Sound Driver Level Fx")]
        [DefaultValue(false)]
        public virtual bool Enable
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

        private double f_Interval = 50;

        [DataMember]
        [Description("Set interval of envelope changing [ms]")]
        [DefaultValue(typeof(double), "50")]
        [DoubleSlideParametersAttribute(1, 10000, 1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public double EnvelopeInterval
        {
            get
            {
                return f_Interval;
            }
            set
            {
                if (value > 10000)
                    value = 10000;
                if (f_Interval != value && value >= 1)
                {
                    f_Interval = value;
                }
            }
        }

        private double f_KSL = 0d;

        [DataMember]
        [Description("Set level of key scale rate for EnvelopeInterval. " +
            "Calculating on the basis of the last note on frequency. And base frequency is A4(440Hz).")]
        [DefaultValue(typeof(double), "0")]
        [DoubleSlideParameters(-5d, 5d, 0.1d)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public virtual double KeyScaleLevel
        {
            get
            {
                return f_KSL;
            }
            set
            {
                if (f_KSL != value)
                {
                    f_KSL = value;
                }
            }
        }

        [DataMember]
        [Description("Memo")]
        [DefaultValue(null)]
        public string Memo
        {
            get;
            set;
        }

        public bool ShouldSerializeMemo()
        {
            return !string.IsNullOrEmpty(Memo);
        }

        public void ResetMemo()
        {
            Memo = null;
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
        [DefaultValue("{}")]
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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AbstractFxSettingsBase()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract AbstractFxEngine CreateEngine();

        public virtual void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(serializeData, this.GetType());
                this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad"}), obj);
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
}
