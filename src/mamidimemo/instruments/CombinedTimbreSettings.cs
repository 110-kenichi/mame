﻿// copyright-holders:K.Ito
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
    public class CombinedTimbreSettings : ContextBoundObject, ISerializeDataSaveLoad, IDisplayName
    {
        [Browsable(false)]
        public CombinedTimbreSettingsCollection Parent
        {
            get;
            set;
        }

        private InstrumentBase f_Instrument;

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public InstrumentBase Instrument
        {
            get
            {
                if (f_Instrument == null)
                {
                    f_Instrument = InstrumentManager.FindParentInstrument(this);
                }
                return f_Instrument;
            }
        }

        [Category("Sound")]
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

        [Category("Sound")]
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

        [Category("Sound")]
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

        [Category("Sound")]
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

        [Category("Sound")]
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
                if(Parent != null && Parent.IndexOf(this) == 0)
                    f_KeyOnDelayOffset = 0;
            }
        }

        private int f_KeyOffDelayOffset;

        [Category("Sound")]
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
                if (Parent != null && Parent.IndexOf(this) == 0)
                    f_KeyOnDelayOffset = 0;
            }
        }

        private NoteNames f_KeyRangeLow = NoteNames.C_1;

        [Category("Sound")]
        [DataMember]
        [Description("Lower key range")]
        [DefaultValue(NoteNames.C_1)]
        [TypeConverter(typeof(NoteNameConverter))]
        [EditorAttribute(typeof(NoteNumberTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public NoteNames KeyRangeLow
        {
            get => f_KeyRangeLow;
            set => f_KeyRangeLow = value;
        }

        private NoteNames f_KeyRangeHigh = NoteNames.G9;

        [Category("Sound")]
        [DataMember]
        [Description("Higher key range")]
        [DefaultValue(NoteNames.G9)]
        [TypeConverter(typeof(NoteNameConverter))]
        [EditorAttribute(typeof(NoteNumberTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public NoteNames KeyRangeHigh
        {
            get => f_KeyRangeHigh;
            set => f_KeyRangeHigh = value;
        }

        private int f_VelocityRangeLow;

        [Category("Sound")]
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

        [Category("Sound")]
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

        private ProgramAssignmentTimbreNumber f_TimbreNumber = ProgramAssignmentTimbreNumber.Timbre0;

        [Category(" Timbre")]
        [DataMember]
        [Description("Set the timbre numbers to bind this Combibed Timbre.")]
        [DefaultValue(ProgramAssignmentTimbreNumber.Timbre0)]
        [TypeConverter(typeof(ProgramAssignmentTimbreNumberConverter))]
        [EditorAttribute(typeof(ProgramAssignmentNumberTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public ProgramAssignmentTimbreNumber TimbreNumber
        {
            get
            {
                return f_TimbreNumber;
            }
            set
            {
                if (f_TimbreNumber != value)
                {
                    f_TimbreNumber = value;
                    f_TimberObject = null;
                }
            }
        }

        private TimbreBase f_TimberObject;

        [Category(" Timbre")]
        [IgnoreDataMember]
        [JsonIgnore]
        public TimbreBase TimberObject
        {
            get
            {
                if (f_TimberObject == null)
                    f_TimberObject = findTimbre();
                return f_TimberObject;
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
                this.InjectFrom(new LoopInjection(new[] { "SerializeData", "SerializeDataSave", "SerializeDataLoad" }), obj);
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
        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public string DisplayName
        {
            get
            {
                return ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayName):
                    FormMain.AppliactionForm.SoftRefreshPropertyGrid();
                    break;
            }
            PropertyChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ProgramAssignmentTimbreNumberConverter converter = new ProgramAssignmentTimbreNumberConverter();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            ProgramAssignmentTimbreNumber nn = (ProgramAssignmentTimbreNumber)TimbreNumber;
            string text = nn.ToString();
            if (TimberObject != null)
            {
                InstrumentBase inst = TimberObject.Instrument;
                if (inst != null)
                {
                    if ((int)nn < inst.BaseTimbres.Length)
                        text += " " + inst.BaseTimbres[(int)nn].TimbreName;
                }
            }
            return text;
            //return (string)converter.ConvertTo(null, null, TimbreNumber, typeof(String));
        }


    }

}
