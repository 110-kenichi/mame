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
using System.Text.RegularExpressions;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<GeneralPurposeControlSettings>))]
    [DataContract]
    [MidiHook]
    public class GeneralPurposeControlSettings : ContextBoundObject
    {

        [DataMember]
        [Description("General Purpose Control 1(Control Change No.16(0x10))\r\n" +
            "Link Data Entry message value with the specified instrument property value (Only the property that has a slider editor)\r\n" +
            "eg 1) \"GainLeft,GainRight\"\r\n" +
            "... You can change Gain property values dynamically via MIDI Control Change No.16 message.\r\n" +
            "eg 2) \"16+Timbres[0].Ops[2].TL/4, 64-Ops[2].MUL/2, Ops[2].D2R/4\"\r\n" +
            "... You can change Timbre 0 FM synth algorithm values dynamically via MIDI Control Change No.16 message.")]
        [DefaultValue(null)]
        public string GeneralPurposeControl1
        {
            get;
            set;
        }

        [DataMember]
        [Description("General Purpose Control 2(Control Change No.17(0x11))")]
        [DefaultValue(null)]
        public string GeneralPurposeControl2
        {
            get;
            set;
        }

        [DataMember]
        [Description("General Purpose Control 3(Control Change No.18(0x12))")]
        [DefaultValue(null)]
        public string GeneralPurposeControl3
        {
            get;
            set;
        }

        [DataMember]
        [Description("General Purpose Control 4(Control Change No.19(0x13))")]
        [DefaultValue(null)]
        public string GeneralPurposeControl4
        {
            get;
            set;
        }

        [DataMember]
        [Description("General Purpose Control 5(Control Change No.80(0x50))")]
        [DefaultValue(null)]
        public string GeneralPurposeControl5
        {
            get;
            set;
        }

        [DataMember]
        [Description("General Purpose Control 6(Control Change No.81(0x51))")]
        [DefaultValue(null)]
        public string GeneralPurposeControl6
        {
            get;
            set;
        }

        [DataMember]
        [Description("General Purpose Control 7(Control Change No.82(0x52))")]
        [DefaultValue(null)]
        public string GeneralPurposeControl7
        {
            get;
            set;
        }

        [DataMember]
        [Description("General Purpose Control 8(Control Change No.83(0x53))")]
        [DefaultValue(null)]
        public string GeneralPurposeControl8
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public GeneralPurposeControlSettings()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="controlNo">1～8</param>
        /// <returns></returns>
        public InstancePropertyInfo[] GetPropertyInfo(InstrumentBase inst, int controlNo)
        {
            switch (controlNo)
            {
                case 1:
                    return getPropertiesInfo(inst, GeneralPurposeControl1);
                case 2:
                    return getPropertiesInfo(inst, GeneralPurposeControl2);
                case 3:
                    return getPropertiesInfo(inst, GeneralPurposeControl3);
                case 4:
                    return getPropertiesInfo(inst, GeneralPurposeControl4);
                case 5:
                    return getPropertiesInfo(inst, GeneralPurposeControl5);
                case 6:
                    return getPropertiesInfo(inst, GeneralPurposeControl6);
                case 7:
                    return getPropertiesInfo(inst, GeneralPurposeControl7);
                case 8:
                    return getPropertiesInfo(inst, GeneralPurposeControl8);
            }

            return null;
        }

        private static Dictionary<Type, Dictionary<string, InstancePropertyInfo>> propertyInfoTable = new Dictionary<Type, Dictionary<string, InstancePropertyInfo>>();

        private static Regex removeRegex = new Regex(@"[ ()+\-^*/^]|\bsin\b|\bcos\b|\btg\b|\bctg\b|\bsh\b|\bch\b|\bth\b|\bsqrt\b|\bexp\b|\blog\b|\bln\b|\babs\b|\bpi\b|\be\b", RegexOptions.Compiled);

        private static Regex nameRegex = new Regex(@"\ [0-9]*", RegexOptions.Compiled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private InstancePropertyInfo[] getPropertiesInfo(InstrumentBase inst, string propertyNames)
        {
            List<InstancePropertyInfo> plist = new List<InstancePropertyInfo>();
            var tt = inst.GetType();

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

                    var pi = getPropertyInfo(inst, pn);
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

        private InstancePropertyInfo getPropertyInfo(InstrumentBase inst, string propertyName)
        {
            object obj = inst;
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

