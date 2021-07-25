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
                    return getPropertiesInfo(timbre, SoundControl1);
                case 2:
                    return getPropertiesInfo(timbre, SoundControl2);
                case 3:
                    return getPropertiesInfo(timbre, SoundControl3);
                case 4:
                    return getPropertiesInfo(timbre, SoundControl4);
                case 5:
                    return getPropertiesInfo(timbre, SoundControl5);
                case 6:
                    return getPropertiesInfo(timbre, SoundControl6);
                case 10:
                    return getPropertiesInfo(timbre, SoundControl10);
                case -1:
                    return getPropertiesInfo(timbre, AfterTouchCh);
            }

            return null;
        }

        private static Dictionary<Type, Dictionary<string, InstancePropertyInfo>> propertyInfoTable = new Dictionary<Type, Dictionary<string, InstancePropertyInfo>>();

        private static Regex removeRegex = new Regex(@"[ ()+\-^*/^]|\bsin\b|\bcos\b|\btg\b|\bctg\b|\bsh\b|\bch\b|\bth\b|\bsqrt\b|\bexp\b|\blog\b|\bln\b|\babs\b|\bpi\b|\be\b", RegexOptions.Compiled);

        private static Regex nameRegex = new Regex(@"\ [0-9]*", RegexOptions.Compiled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timbre"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private InstancePropertyInfo[] getPropertiesInfo(TimbreBase timbre, string propertyNames)
        {
            List<InstancePropertyInfo> plist = new List<InstancePropertyInfo>();
            var tt = timbre.GetType();

            if (!string.IsNullOrWhiteSpace(propertyNames))
            {
                string[] propNameParts = propertyNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var propertyName in propNameParts)
                {
                    var fm = " " + propertyName.Trim();
                    var pn = nameRegex.Replace(removeRegex.Replace(fm, " "), "");

                    if (propertyInfoTable.ContainsKey(tt))
                    {
                        if (propertyInfoTable[tt].ContainsKey(pn))
                        {
                            var tpi = propertyInfoTable[tt][pn];
                            plist.Add(new InstancePropertyInfo(tpi.Owner, tpi.Property) { Formula = fm, Symbol = pn });
                            continue;
                        }
                    }
                    else
                    {
                        propertyInfoTable.Add(tt, new Dictionary<string, InstancePropertyInfo>());
                    }

                    var pi = getPropertyInfo(timbre, pn);
                    if (pi != null)
                    {
                        pi.Formula = fm;
                        pi.Symbol = pn;

                        SlideParametersAttribute attribute =
                            Attribute.GetCustomAttribute(pi.Property, typeof(SlideParametersAttribute)) as SlideParametersAttribute;
                        if (attribute != null)
                        {
                            plist.Add(pi);
                            propertyInfoTable[tt][pn] = pi;
                        }
                        else
                        {
                            DoubleSlideParametersAttribute dattribute =
                                Attribute.GetCustomAttribute(pi.Property, typeof(DoubleSlideParametersAttribute)) as DoubleSlideParametersAttribute;
                            if (dattribute != null)
                            {
                                plist.Add(pi);
                                propertyInfoTable[tt][pn] = pi;
                            }
                            else
                            {
                                if (pi.Property.PropertyType == typeof(bool))
                                {
                                    plist.Add(pi);
                                    propertyInfoTable[tt][pn] = pi;
                                }
                                else if (pi.Property.PropertyType.IsEnum)
                                {
                                    plist.Add(pi);
                                    propertyInfoTable[tt][pn] = pi;
                                }
                            }
                        }
                    }
                }
            }
            return plist.ToArray();
        }

        private InstancePropertyInfo getPropertyInfo(TimbreBase timbre, string propertyName)
        {
            object obj = timbre;
            object lobj = obj;

            // Split property name to parts (propertyName could be hierarchical, like obj.subobj.subobj.property
            string[] propertyNameParts = propertyName.Split('.');

            PropertyInfo pi = null;
            foreach (string propertyNamePart in propertyNameParts)
            {
                if (obj == null)
                    return null;

                // propertyNamePart could contain reference to specific 
                // element (by index) inside a collection
                if (!propertyNamePart.Contains("["))
                {
                    pi = obj.GetType().GetProperty(propertyNamePart);
                    if (pi == null)
                        return null;
                    lobj = obj;
                    obj = pi.GetValue(obj, null);
                }
                else
                {   // propertyNamePart is areference to specific element 
                    // (by index) inside a collection
                    // like AggregatedCollection[123]
                    //   get collection name and element index
                    int indexStart = propertyNamePart.IndexOf("[") + 1;
                    string collectionPropertyName = propertyNamePart.Substring(0, indexStart - 1);
                    int collectionElementIndex = Int32.Parse(propertyNamePart.Substring(indexStart, propertyNamePart.Length - indexStart - 1));
                    //   get collection object
                    pi = obj.GetType().GetProperty(collectionPropertyName);
                    if (pi == null)
                        return null;
                    object unknownCollection = pi.GetValue(obj, null);
                    //   try to process the collection as array
                    if (unknownCollection.GetType().IsArray)
                    {
                        object[] collectionAsArray = unknownCollection as object[];
                        lobj = obj;
                        obj = collectionAsArray[collectionElementIndex];
                    }
                    else
                    {
                        //   try to process the collection as IList
                        System.Collections.IList collectionAsList = unknownCollection as System.Collections.IList;
                        if (collectionAsList != null)
                        {
                            lobj = obj;
                            obj = collectionAsList[collectionElementIndex];
                        }
                        else
                        {
                            // ??? Unsupported collection type
                        }
                    }
                }
            }

            return new InstancePropertyInfo(lobj, pi);
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

