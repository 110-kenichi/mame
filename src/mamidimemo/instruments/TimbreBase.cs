﻿// copyright-holders:K.Ito
using Newtonsoft.Json;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<TimbreBase>))]
    [DataContract]
    [MidiHook]
    public abstract class TimbreBase : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        public TimbreBase()
        {
            SDS = new SoundDriverSettings();
            SCCS = new SoundControlChangeSettings();
        }

        [DataMember]
        [Description("Sound Driver Settings")]
        [DisplayName("Sound Driver Settings(SDS)")]
        public SoundDriverSettings SDS
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control Settings")]
        [DisplayName("Sound Control Settings(SCCS)")]
        public SoundControlChangeSettings SCCS
        {
            get;
            set;
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
        [Description("You can copy and paste this text data to other same type timber.\r\nNote: Open dropdown editor then copy all text and paste to dropdown editor. Do not copy and paste one liner text.")]
        public string SerializeData
        {
            get
            {
                return SerializeObject();
            }
            set
            {
                RestoreFrom(value);
            }
        }

        protected virtual string SerializeObject()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public abstract void RestoreFrom(string serializeData);

    }
}
