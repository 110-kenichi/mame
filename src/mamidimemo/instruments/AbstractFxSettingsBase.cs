﻿using Newtonsoft.Json;
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

namespace zanac.MAmidiMEmo.Instruments
{

    [JsonConverter(typeof(NoTypeConverterJsonConverter<AbstractFxSettingsBase>))]
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [DataContract]
    [MidiHook]
    public abstract class AbstractFxSettingsBase : ContextBoundObject
    {
        private bool f_Enable;

        [DataMember]
        [Description("Whether enable Sound Driver Level Fx")]
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

        private uint f_Interval = 50;

        [DataMember]
        [Description("Set interval of envelope changing [ms]")]
        [DefaultValue((uint)50)]
        public uint EnvelopeInterval
        {
            get
            {
                return f_Interval;
            }
            set
            {
                if (f_Interval != value && value >= InstrumentManager.TIMER_INTERVAL)
                {
                    f_Interval = value;
                }
            }
        }

        [DataMember]
        [Description("Memo")]
        public string Memo
        {
            get;
            set;
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
        [IgnoreDataMember]
        [JsonIgnore]
        [Description("You can copy and paste this text data to other same type timber.")]
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
}
