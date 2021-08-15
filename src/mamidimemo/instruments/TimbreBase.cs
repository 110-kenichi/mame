// copyright-holders:K.Ito
using Newtonsoft.Json;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
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


        [DataMember]
        [Description("Define custom velocity map\r\n" +
            "Link with Velocity value with the Timbre property value\r\n" +
            "eg 1) \"DutyCycle,Volume\"\r\n" +
            "... You can change DutyCycle and Volume property values dynamically via Velocity value.\r\n" +
            "eg 2) \"16+Ops[2].TL/4, 64-Ops[2].MUL/2, Ops[2].D2R/4\"\r\r" +
            "... You can change Operator TL, MUL, D2R values dynamically via Velocity value.")]
        [DefaultValue(null)]
        public string VelocityMap
        {
            get;
            set;
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


        private static Dictionary<Type, Dictionary<string, InstancePropertyInfo>> propertyInfoTable = new Dictionary<Type, Dictionary<string, InstancePropertyInfo>>();

        private static Regex removeRegex = new Regex(@"[ ()+\-^*/^]|\bsin\b|\bcos\b|\btg\b|\bctg\b|\bsh\b|\bch\b|\bth\b|\bsqrt\b|\bexp\b|\blog\b|\bln\b|\babs\b|\bpi\b|\be\b", RegexOptions.Compiled);

        private static Regex nameRegex = new Regex(@"\ [0-9]*", RegexOptions.Compiled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timbre"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static InstancePropertyInfo[] GetPropertiesInfo(TimbreBase timbre, string propertyNames)
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

                    var pi = GetPropertyInfo(timbre, pn);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timbre"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static InstancePropertyInfo GetPropertyInfo(TimbreBase timbre, string propertyName)
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

    }
}
