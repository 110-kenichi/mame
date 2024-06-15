// copyright-holders:K.Ito
using FM_SoundConvertor;
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
using static zanac.MAmidiMEmo.Instruments.Chips.YMF262;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<MidiDriverSettings>))]
    [DataContract]
    [InstLock]
    public class MidiDriverSettings : ContextBoundObject, ISerializeDataSaveLoad
    {
        [DataMember]
        [Description("Whether to ignore the keyoff event")]
        [DefaultValue(false)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public bool IgnoreKeyOff
        {
            get;
            set;
        }

        [DataMember]
        [Description("Base Key offset [Semitone]")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public int KeyShift
        {
            get;
            set;
        }

        private int f_PitchShift;

        [DataMember]
        [Description("Base Pitch offset [Cent]")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-1200, 1200)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
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
        [Description("Base Panpot offset (-127 - 0 - 127)")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
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

        private int f_KeyOnDelay;

        [DataMember]
        [Description("Key On Delay [ms]")]
        [DefaultValue(0)]
        [SlideParametersAttribute(0, 1000)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public int KeyOnDelay
        {
            get => f_KeyOnDelay;
            set
            {
                f_KeyOnDelay = value;
                if (f_KeyOnDelay < 0)
                    f_KeyOnDelay = 0;
            }
        }


        private int f_KeyOffDelay;

        [DataMember]
        [Description("Key Off Delay [ms]")]
        [DefaultValue(0)]
        [SlideParametersAttribute(0, 1000)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public int KeyOffDelay
        {
            get => f_KeyOffDelay;
            set
            {
                f_KeyOffDelay = value;
                if (f_KeyOffDelay < 0)
                    f_KeyOffDelay = 0;
            }
        }

        private byte f_VelocitySensitivity = 2;

        [DataMember]
        [Description("Velocity Sensitivity for FM Career")]
        [SlideParametersAttribute(0, 7)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public byte VelocitySensitivity
        {
            get
            {
                return f_VelocitySensitivity;
            }
            set
            {
                f_VelocitySensitivity = (byte)(value & 7);
            }
        }

        public virtual bool ShouldSerializeVelocitySensitivity()
        {
            return f_VelocitySensitivity != 2;
        }

        public virtual void ResetVelocitySensitivity()
        {
            f_VelocitySensitivity = 2;
        }

        [DataMember]
        [Description("Define custom velocity map\r\n" +
            "Link with Velocity value with the Timbre property value\r\n" +
            "eg 1) \"DutyCycle,Volume\"\r\n" +
            "... You can change DutyCycle and Volume property values dynamically via Velocity value.\r\n" +
            "eg 2) \"16+Ops[2].TL/4, 64-Ops[2].MUL/2, Ops[2].D2R/4\"\r\r" +
            "... You can change Operator 2 TL, MUL, D2R values dynamically via Velocity value.\r\n" +
            "Also the \"$\" keyword indicates its own value.\r\n " +
            "eg 3) \"$ * (Ops[3].AR/31)\"\r\r" +
            "... You can change Operator 3 AR value from the original of AR value.")]
        [DefaultValue(null)]
        [Category("MIDI")]
        public string VelocityMap
        {
            get;
            set;
        }

        [DataMember]
        [Description("Ignore Note On velocity.")]
        [DefaultValue(false)]
        [Category("MIDI")]
        public bool IgnoreVelocity
        {
            get;
            set;
        }

        private int f_ModulationShift;

        [DataMember]
        [Description("Base Modulation offset (-127 - 0 - 127)")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public int ModulationShift
        {
            get => f_ModulationShift;
            set
            {
                f_ModulationShift = value;
                if (f_ModulationShift < -127)
                    f_ModulationShift = -127;
                else if (f_ModulationShift > 127)
                    f_ModulationShift = 127;
            }
        }

        private int f_ModulationRateShift;

        [DataMember]
        [Description("Base Modulation Rate offset (-127 - 0 - 127)")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public int ModulationRateShift
        {
            get => f_ModulationRateShift;
            set
            {
                f_ModulationRateShift = value;
                if (f_ModulationRateShift < -127)
                    f_ModulationRateShift = -127;
                else if (f_ModulationRateShift > 127)
                    f_ModulationRateShift = 127;
            }
        }

        private int f_ModulationDepthShift;

        [DataMember]
        [Description("Base Modulation Depth offset (-127 - 0 - 127)")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public int ModulationDepthShift
        {
            get => f_ModulationDepthShift;
            set
            {
                f_ModulationDepthShift = value;
                if (f_ModulationDepthShift < -127)
                    f_ModulationDepthShift = -127;
                else if (f_ModulationDepthShift > 127)
                    f_ModulationDepthShift = 127;
            }
        }

        private int f_ModulationDelayShift;

        [DataMember]
        [Description("Base Modulation Delay offset (-127 - 0 - 127)")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public int ModulationDelayShift
        {
            get => f_ModulationDelayShift;
            set
            {
                f_ModulationDelayShift = value;
                if (f_ModulationDelayShift < -127)
                    f_ModulationDelayShift = -127;
                else if (f_ModulationDelayShift > 127)
                    f_ModulationDelayShift = 127;
            }
        }

        private int f_ModulationDepthRangeNoteShift;

        [DataMember]
        [Description("Base Modulation Depth Range[Note] offset (-127 - 0 - 127)")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public int ModulationDepthRangeNoteShift
        {
            get => f_ModulationDepthRangeNoteShift;
            set
            {
                f_ModulationDepthRangeNoteShift = value;
                if (f_ModulationDepthRangeNoteShift < -127)
                    f_ModulationDepthRangeNoteShift = -127;
                else if (f_ModulationDepthRangeNoteShift > 127)
                    f_ModulationDepthRangeNoteShift = 127;
            }
        }

        private int f_ModulationDepthRangeCentShift;

        [DataMember]
        [Description("Base Modulation Depth Range[Cent] offset (-127 - 0 - 127)")]
        [DefaultValue(0)]
        [SlideParametersAttribute(-127, 127)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("MIDI")]
        public int ModulationDepthRangeCentShift
        {
            get => f_ModulationDepthRangeCentShift;
            set
            {
                f_ModulationDepthRangeCentShift = value;
                if (f_ModulationDepthRangeCentShift < -127)
                    f_ModulationDepthRangeCentShift = -127;
                else if (f_ModulationDepthRangeCentShift > 127)
                    f_ModulationDepthRangeCentShift = 127;
            }
        }

        [DataMember]
        [Description("Define custom note map\r\n" +
            "Link with Note value with the Timbre property value\r\n" +
            "eg 1) \"DutyCycle,Volume\"\r\n" +
            "... You can change DutyCycle and Volume property values dynamically via Velocity value.\r\n" +
            "eg 2) \"16+Ops[2].TL/4, 64-Ops[2].MUL/2, Ops[2].D2R/4\"\r\r" +
            "... You can change Operator 2 TL, MUL, D2R values dynamically via Velocity value.\r\n" +
            "Also the \"$\" keyword indicates its own value.\r\n " +
            "eg 3) \"$ * (Ops[3].AR/31)\"\r\r" +
            "... You can change Operator 3 AR value the result of VelocityMap value.")]
        [DefaultValue(null)]
        [Category("MIDI")]
        public string NoteMap
        {
            get;
            set;
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
                var obj = JsonConvert.DeserializeObject<MidiDriverSettings>(serializeData);
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

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public MidiDriverSettings()
        {

        }

        public override bool Equals(object obj)
        {
            var mdsobj = obj as MidiDriverSettings;
            if (mdsobj == null)
                return false;

            return string.Equals(SerializeData, mdsobj.SerializeData, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return SerializeData.GetHashCode();
        }

    }

}

