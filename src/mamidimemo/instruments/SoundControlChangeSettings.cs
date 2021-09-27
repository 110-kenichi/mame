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

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<SoundControlChangeSettings>))]
    [DataContract]
    [MidiHook]
    public class SoundControlChangeSettings : ContextBoundObject
    {

        [DataMember]
        [Description("Sound Control 1(Control Change No.70(0x46))\r\n" +
            "Link Data Entry message value with the Timbre property value\r\n" +
            "eg 1) \"DutyCycle,Volume\"\r\n" +
            "... You can change DutyCycle and Volume property values dynamically via MIDI Control Change No.70 message.\r\n" +
            "eg 2) \"16+Ops[2].TL/4, 64-Ops[2].MUL/2, Ops[2].D2R/4\"\r\r" +
            "... You can change Operator TL, MUL, D2R values dynamically via MIDI Control Change No.70 message.")]
        [DefaultValue(null)]
        public string SoundControl1
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 2(Control Change No.71(0x47))")]
        [DefaultValue(null)]
        public string SoundControl2
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 3(Control Change No.72(0x48))")]
        [DefaultValue(null)]
        public string SoundControl3
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 4(Control Change No.73(0x49))")]
        [DefaultValue(null)]
        public string SoundControl4
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 5(Control Change No.74(0x4A))")]
        [DefaultValue(null)]
        public string SoundControl5
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 6(Control Change No.75(0x4B))")]
        [DefaultValue(null)]
        public string SoundControl6
        {
            get;
            set;
        }

        /*
        [DataMember]
        [Description("Sound Control 7(Control Change No.76(0x4C))")]
        [DefaultValue(null)]
        public string SoundControl7
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 8(Control Change No.77(0x4D))")]
        [DefaultValue(null)]
        public string SoundControl8
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control 9(Control Change No.78(0x4E))")]
        [DefaultValue(null)]
        public string SoundControl9
        {
            get;
            set;
        }
        */

        [DataMember]
        [Description("Sound Control 10(Control Change No.79(0x4F))")]
        [DefaultValue(null)]
        public string SoundControl10
        {
            get;
            set;
        }

        [DataMember]
        [Description("Sound Control Channel After Touch\r\n" +
            "Link Channel After Touch value with the Timbre property value\r\n" +
            "eg 1) \"DutyCycle,Volume\"\r\n" +
            "... You can change DutyCycle and Volume property values dynamically via MIDI Channel After Touch message.\r\n" +
            "eg 2) \"16+Ops[2].TL/4, 64-Ops[2].MUL/2, Ops[2].D2R/4\"\r\r" +
            "... You can change Operator TL, MUL, D2R values dynamically via MIDI Channel After Touch message.")]
        [DefaultValue(null)]
        public string AfterTouchCh
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public SoundControlChangeSettings()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="timbre"></param>
        /// <param name="controlNo">1～6,10</param>
        /// <returns></returns>
        public InstancePropertyInfo[] GetPropertyInfo(TimbreBase timbre, int controlNo)
        {
            switch (controlNo)
            {
                case 1:
                    return TimbreBase.GetPropertiesInfo(timbre, SoundControl1);
                case 2:
                    return TimbreBase.GetPropertiesInfo(timbre, SoundControl2);
                case 3:
                    return TimbreBase.GetPropertiesInfo(timbre, SoundControl3);
                case 4:
                    return TimbreBase.GetPropertiesInfo(timbre, SoundControl4);
                case 5:
                    return TimbreBase.GetPropertiesInfo(timbre, SoundControl5);
                case 6:
                    return TimbreBase.GetPropertiesInfo(timbre, SoundControl6);
                case 10:
                    return TimbreBase.GetPropertiesInfo(timbre, SoundControl10);
                case -1:
                    return TimbreBase.GetPropertiesInfo(timbre, AfterTouchCh);
            }

            return null;
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
                var obj = JsonConvert.DeserializeObject<SoundDriverSettings>(serializeData);
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

