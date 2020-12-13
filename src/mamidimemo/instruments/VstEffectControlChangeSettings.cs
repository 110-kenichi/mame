// copyright-holders:K.Ito
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.Serialization;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments.Vst;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<VstEffectControlChangeSettings>))]
    [DataContract]
    [MidiHook]
    public class VstEffectControlChangeSettings : ContextBoundObject
    {

        [DataMember]
        [Description("Vst Effect Control 1(Control Change No.91(0x5B))\r\n" +
            "Link Data Entry message value with the VST property value\r\n" +
            "eg) \"Reverb,Chorus\" ... You can change Reverb and Chorus depth property values dynamically via MIDI Control Change No.91 message.")]
        [DefaultValue(null)]
        public string VstEffectSoundControl1
        {
            get;
            set;
        }

        [DataMember]
        [Description("Vst Effect Control 1(Control Change No.92(0x5C))")]
        [DefaultValue(null)]
        public string VstEffectSoundControl2
        {
            get;
            set;
        }

        [DataMember]
        [Description("Vst Effect Control 3(Control Change No.93(0x5D))")]
        [DefaultValue(null)]
        public string VstEffectSoundControl3
        {
            get;
            set;
        }

        [DataMember]
        [Description("Vst Effect Control 5(Control Change No.94(0x5E))")]
        [DefaultValue(null)]
        public string VstEffectSoundControl4
        {
            get;
            set;
        }

        [DataMember]
        [Description("Vst Effect Control 5(Control Change No.95(0x5F))")]
        [DefaultValue(null)]
        public string VstEffectSoundControl5
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public VstEffectControlChangeSettings()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vp"></param>
        /// <param name="controlNo">1～5</param>
        /// <returns></returns>
        public string[] GetProperties(VstPlugin vp, int controlNo)
        {
            switch (controlNo)
            {
                case 1:
                    return getPropertiesInfo(vp, VstEffectSoundControl1);
                case 2:
                    return getPropertiesInfo(vp, VstEffectSoundControl2);
                case 3:
                    return getPropertiesInfo(vp, VstEffectSoundControl3);
                case 4:
                    return getPropertiesInfo(vp, VstEffectSoundControl4);
                case 5:
                    return getPropertiesInfo(vp, VstEffectSoundControl5);
            }

            return null;
        }

        private static Dictionary<string, object> propertyInfoTable = new Dictionary<string, object>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vp"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private string[] getPropertiesInfo(VstPlugin vp, string propertyNames)
        {
            List<string> plist = new List<string>();
            var tt = vp.GetType();

            if (!string.IsNullOrWhiteSpace(propertyNames))
            {
                string[] propNameParts = propertyNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var propertyName in propNameParts)
                {
                    string pn = propertyName.Trim();
                    if (propertyInfoTable.ContainsKey(pn))
                    {
                        if(propertyInfoTable[pn] != null)
                            plist.Add(pn);
                        continue;
                    }
                    foreach (var rp in vp.Settings.GetAllRawProperties())
                    {
                        if (pn.Equals(rp.Key.Trim()))
                        {
                            plist.Add(pn);
                            propertyInfoTable.Add(pn, new object());
                            break;
                        }
                    }
                    if(!propertyInfoTable.ContainsKey(pn))
                        propertyInfoTable.Add(pn, null);
                }
            }
            return plist.ToArray();
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

