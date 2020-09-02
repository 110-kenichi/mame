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
    [JsonConverter(typeof(NoTypeConverterJsonConverter<ArpSettings>))]
    [DataContract]
    [MidiHook]
    public class ArpSettings : ContextBoundObject
    {
        private bool f_Enable;

        [DataMember]
        [Description("Whether enable sound driver level Arpeggio.")]
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

        private bool f_Hold;

        [DataMember]
        [Description("Select whether Arpeggio key hold or no.")]
        [DefaultValue(false)]
        public bool Hold
        {
            get
            {
                return f_Hold;
            }
            set
            {
                if (f_Hold != value)
                {
                    f_Hold = value;
                }
            }
        }

        private ArpMethod f_ArpMethod;

        [DataMember]
        [Description("Set arpeggio method (Note On or Pitch Change)")]
        [DefaultValue(ArpMethod.KeyOn)]
        public ArpMethod ArpMethod
        {
            get
            {
                return f_ArpMethod;
            }
            set
            {
                if (f_ArpMethod != value)
                {
                    f_ArpMethod = value;
                }
            }
        }

        private ArpStepStyle f_StepStyle;

        [DataMember]
        [Description("Set arpeggio step type.")]
        [DefaultValue(ArpStepStyle.Up)]
        public ArpStepStyle StepStyle
        {
            get
            {
                return f_StepStyle;
            }
            set
            {
                if (f_StepStyle != value)
                {
                    f_StepStyle = value;
                }
            }
        }

        private int f_OctaveRange = 1;

        [DataMember]
        [Description("Set arpeggio octave range (1-4)")]
        [DefaultValue(1)]
        [SlideParametersAttribute(1, 4)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int OctaveRange
        {
            get
            {
                return f_OctaveRange;
            }
            set
            {
                if (f_OctaveRange != value && value >= 1 && value <= 4)
                {
                    f_OctaveRange = value;
                }
            }
        }

        private int f_Beat = 120;

        [DataMember]
        [Description("Set arpeggio tempo (20-300)")]
        [DefaultValue(120)]
        [SlideParametersAttribute(20, 300)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int Beat
        {
            get
            {
                return f_Beat;
            }
            set
            {
                if (f_Beat != value && value >= 20 && value <= 300)
                {
                    f_Beat = value;
                }
            }
        }

        private ArpResolution f_ArpResolution = ArpResolution.SixteenthNote;

        [DataMember]
        [Description("Set arpeggio resolution")]
        [DefaultValue(ArpResolution.SixteenthNote)]
        public ArpResolution ArpResolution
        {
            get
            {
                return f_ArpResolution;
            }
            set
            {
                if (f_ArpResolution != value)
                {
                    f_ArpResolution = value;
                    ArpStep = (60d * HighPrecisionTimer.TIMER_BASIC_1KHZ / Beat) / (double)ArpResolution;
                }
            }
        }

        [Browsable(false)]
        [IgnoreDataMember]
        public double ArpStep
        {
            get;
            private set;
        }


        private int f_GateTime = 127;

        [DataMember]
        [Description("Arpeggio Gate Time for NoteOn type (0(0%)-127(100%))")]
        [DefaultValue(127)]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int GateTime
        {
            get
            {
                return f_GateTime;
            }
            set
            {
                value = value & 127;
                if (f_GateTime != value)
                {
                    f_GateTime = value;
                    ArpStep = (60d * HighPrecisionTimer.TIMER_BASIC_1KHZ / Beat) / (double)ArpResolution;
                }
            }
        }

        private bool f_KeySync;

        [DataMember]
        [Description("When you press a key, arpeggio restart from first. ")]
        [DefaultValue(false)]
        public bool KeySync
        {
            get
            {
                return f_KeySync;
            }
            set
            {
                if (f_KeySync != value)
                {
                    f_KeySync = value;
                }
            }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ArpSettings()
        {
            ArpStep = (60d * HighPrecisionTimer.TIMER_BASIC_1KHZ / Beat) / (double)ArpResolution;
        }

        #region Etc

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
                var obj = JsonConvert.DeserializeObject<ArpSettings>(serializeData);
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
