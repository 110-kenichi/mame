// copyright-holders:K.Ito
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
using zanac.MAmidiMEmo.Gui;

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

        [DataMember]
        [Description("Whether to ignore the keyoff event")]
        [DefaultValue(false)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public bool IgnoreKeyOff
        {
            get;
            set;
        }

        [DataMember]
        [Description("Base frequency offset [Semitone]")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int KeyShift
        {
            get;
            set;
        }

        private int f_PitchShift;

        [DataMember]
        [Description("Base frequency offset [Cent]")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-1200, 1200)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int PitchShift
        {
            get => f_PitchShift;
            set
            {
                f_PitchShift = value;
                if (f_PitchShift < -1200)
                    f_PitchShift = -1200;
                else if (f_PitchShift > 1200)
                    f_PitchShift = 1200;
            }
        }

        private int f_PanShift;

        [DataMember]
        [Description("Base pan pot offset (-127 - 0 - 127)")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int PanShift
        {
            get => f_PanShift;
            set
            {
                f_PanShift = value;
                if (f_PanShift < -127)
                    f_PanShift = -127;
                else if (f_PanShift > 127)
                    f_PanShift = 127;
            }
        }

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
        public virtual SoundDriverSettings SDS
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control Change Settings\r\n" +
            "Link Data Entry message value with the Timbre property value (Only the property that has a slider editor)\r\n" +
            "eg) \"DutyCycle,Volume\" ... You can change DutyCycle and Volume property values dynamically via MIDI Control Change No.7x message.")]
        [DisplayName("Sound Control Change Settings(SCCS)")]
        public virtual SoundControlChangeSettings SCCS
        {
            get;
            set;
        }

        [DataMember]
        [Description("Name")]
        [DefaultValue(null)]
        public string TimbreName
        {
            get;
            set;
        }

        [DataMember]
        [Description("Memo")]
        [DefaultValue(null)]
        public string Memo
        {
            get;
            set;
        }

        [Editor(typeof(FormTextUITypeEditor), typeof(UITypeEditor)), Localizable(false)]
        [TypeConverter(typeof(CustomExpandableObjectConverter))]
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
            //return JsonHelper.SerializeToMinimalJson(this); NG: cant reset child member value
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public abstract void RestoreFrom(string serializeData);

    }
}
