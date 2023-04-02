// copyright-holders:K.Ito
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
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<CombinedTimbreSettings>))]
    [DataContract]
    [InstLock]
    public class CombinedTimbreSettings : ContextBoundObject
    {

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

        private double f_VolumeOffest;

        [DataMember]
        [Description("Volume offset (-1.0 - 0 - 1.0)")]
        [DefaultValue(0d)]
        [DoubleSlideParameters(-1.0, 1.0, 0.1)]
        [EditorAttribute(typeof(DoubleSlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public double VolumeOffest
        {
            get => f_VolumeOffest;
            set
            {
                f_VolumeOffest = value;
                if (f_VolumeOffest < -1.0)
                    f_VolumeOffest = -1.0;
                else if (f_VolumeOffest > 1.0)
                    f_VolumeOffest = 1.0;
            }
        }

        private int f_KeyOnDelayOffset;

        [DataMember]
        [Description("Key On Delay Offset [ms]")]
        [DefaultValue(0)]
        [SlideParametersAttribute(0, 1000)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int KeyOnDelayOffset
        {
            get => f_KeyOnDelayOffset;
            set
            {
                f_KeyOnDelayOffset = value;
                if (f_KeyOnDelayOffset < 0)
                    f_KeyOnDelayOffset = 0;
            }
        }

        private int f_KeyOffDelayOffset;

        [DataMember]
        [Description("Key Off Delay Offset [ms]")]
        [DefaultValue(0)]
        [SlideParametersAttribute(0, 1000)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int KeyOffDelayOffset
        {
            get => f_KeyOffDelayOffset;
            set
            {
                f_KeyOffDelayOffset = value;
                if (f_KeyOffDelayOffset < 0)
                    f_KeyOffDelayOffset = 0;
            }
        }

        private NoteNames f_KeyRangeLow = NoteNames.C_1;

        [DataMember]
        [Description("Lower key range")]
        [DefaultValue(NoteNames.C_1)]
        public NoteNames KeyRangeLow
        {
            get => f_KeyRangeLow;
            set => f_KeyRangeLow = value;
        }

        private NoteNames f_KeyRangeHigh = NoteNames.G9;

        [DataMember]
        [Description("Higher key range")]
        [DefaultValue(NoteNames.G9)]
        public NoteNames KeyRangeHigh
        {
            get => f_KeyRangeHigh;
            set => f_KeyRangeHigh = value;
        }

        private int f_VelocityRangeLow;

        [DataMember]
        [Description("Lower velocity range")]
        [DefaultValue(0)]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int VelocityRangeLow
        {
            get => f_VelocityRangeLow;
            set
            {
                f_VelocityRangeLow = value;
                if (f_VelocityRangeLow < 0)
                    f_VelocityRangeLow = 0;
                else if (f_VelocityRangeLow > 127)
                    f_VelocityRangeLow = 127;
            }
        }

        private int f_VelocityRangeHigh = 127;

        [DataMember]
        [Description("Higher velocity range")]
        [DefaultValue(127)]
        [SlideParametersAttribute(0, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int VelocityRangeHigh
        {
            get => f_VelocityRangeHigh;
            set
            {
                f_VelocityRangeHigh = value;
                if (f_VelocityRangeHigh < 0)
                    f_VelocityRangeHigh = 0;
                else if (f_VelocityRangeHigh > 127)
                    f_VelocityRangeHigh = 127;
            }
        }

        [DataMember]
        [Description("Set the timbre numbers to bind this Combibed Timbre.")]
        [DefaultValue(ProgramAssignmentTimbreNumber.Timbre0)]
        public ProgramAssignmentTimbreNumber TimbreNumber
        {
            get;
            set;
        } = ProgramAssignmentTimbreNumber.Timbre0;

        [IgnoreDataMember]
        [JsonIgnore]
        public TimbreBase TimberObject
        {
            get
            {
                return findTimbre();
            }
        }

        private TimbreBase findTimbre()
        {
            TimbreBase inst = null;
            foreach (var i in InstrumentManager.GetAllInstruments())
            {
                Parallel.ForEach(i.CombinedTimbres, t =>
                {
                    foreach (var bt in t.Timbres)
                    {
                        if (bt == this && (int)TimbreNumber < i.BaseTimbres.Length)
                        {
                            inst = i.BaseTimbres[(int)TimbreNumber];
                            break;
                        }
                    }
                    if (inst != null)
                        return;
                });
            }
            return inst;
        }


        [DataMember]
        [Description("Memo")]
        [DefaultValue(null)]
        public string Memo
        {
            get;
            set;
        }

        [Editor(typeof(SerializeSaveUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [IgnoreDataMember]
        [JsonIgnore]
        [DisplayName("(Save...)")]
        [Description("Save all parameters as serialize data to the file.")]
        [TypeConverter(typeof(EmptyTypeConverter))]
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
        [DisplayName("(Load...)")]
        [Description("Load all parameters as serialize data from the file.")]
        [TypeConverter(typeof(EmptyTypeConverter))]
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

        /// <summary>
        /// 
        /// </summary>
        public CombinedTimbreSettings()
        {

        }

        public void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<CombinedTimbreSettings>(serializeData);
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
