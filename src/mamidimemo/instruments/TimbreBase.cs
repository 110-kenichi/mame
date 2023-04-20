// copyright-holders:K.Ito
using FastColoredTextBoxNS;
using FM_SoundConvertor;
using Kermalis.SoundFont2;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Gui;
using zanac.MAmidiMEmo.Gui.FMEditor;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2612;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TimbrePropertyTabAttribute]
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<TimbreBase>))]
    [DataContract]
    [InstLock]
    public abstract class TimbreBase : ContextBoundObject, ISerializeDataSaveLoad, IDisplayName
    {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="timbre"></param>
        public virtual bool CanOpenTimbreEditor(InstrumentBase inst)
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="timbre"></param>
        public virtual void OpenTimbreEditor(InstrumentBase inst)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="timbre"></param>
        public virtual void OpenPropEditor(InstrumentBase inst)
        {
            var fp = new FormProp(new InstrumentBase[] { inst }, new TimbreBase[] { this });
            {
                fp.Show();
            }
        }

        [Browsable(false)]
        [Obsolete]
        public bool IgnoreKeyOff
        {
            get
            {
                return MDS.IgnoreKeyOff;
            }
            set
            {
                MDS.IgnoreKeyOff = value;
            }
        }

        [Browsable(false)]
        [Obsolete]
        public int KeyShift
        {
            get
            {
                return MDS.KeyShift;
            }
            set
            {
                MDS.KeyShift = value;
            }
        }

        [Browsable(false)]
        [Obsolete]
        public int PitchShift
        {
            get
            {
                return MDS.PitchShift;
            }
            set
            {
                MDS.PitchShift = value;
            }
        }

        [Browsable(false)]
        [Obsolete]
        public int PanShift
        {
            get
            {
                return MDS.PanShift;
            }
            set
            {
                MDS.PanShift = value;
            }
        }

        [Browsable(false)]
        [Obsolete]
        public int KeyOnDelay
        {
            get
            {
                return MDS.KeyOnDelay;
            }
            set
            {
                MDS.KeyOnDelay = value;
            }
        }

        [Browsable(false)]
        [Obsolete]
        public int KeyOffDelay
        {
            get
            {
                return MDS.KeyOffDelay;
            }
            set
            {
                MDS.KeyOffDelay = value;
            }
        }

        [Browsable(false)]
        [Obsolete]
        public string VelocityMap
        {
            get
            {
                return MDS.VelocityMap;
            }
            set
            {
                MDS.VelocityMap = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public virtual bool UseExprForModulator
        {
            get;
            set;
        }

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Assign MIDI channel to hardware channel.\r\n" +
            "Currently supported OPN2, DCSG only.")]
        [DefaultValue(false)]
        public virtual  bool AssignMIDIChtoSlotNum
        {
            get;
            set;
        }

        [DataMember]
        [Category("Chip(Dedicated)")]
        [Description("Assign MIDI channel offset to hardware channel.\r\n" +
    "Currently supported OPN2, DCSG only.")]
        [DefaultValue(0)]
        public virtual int AssignMIDIChtoSlotNumOffset
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public TimbreBase()
        {
            MDS = new MidiDriverSettings();
            SDS = new SoundDriverSettings();
            SCCS = new SoundControlChangeSettings();
        }

        [DataMember]
        [Description("Midi Driver Settings")]
        [DisplayName("Midi Driver Settings[MDS]")]
        [Category("General")]
        public virtual MidiDriverSettings MDS
        {
            get;
            set;
        }

        public virtual bool ShouldSerializeMDS()
        {
            return !string.Equals(MDS.SerializeData, "{}", StringComparison.Ordinal);
        }

        public virtual void ResetMDS()
        {
            MDS.SerializeData = "{}";
        }

        private SoundDriverSettings f_SDS;

        [DataMember]
        [Description("Sound Driver Settings")]
        [DisplayName("Sound Driver Settings[SDS]")]
        [Category("General")]
        public virtual SoundDriverSettings SDS
        {
            get
            {
                return f_SDS;
            }
            set
            {
                f_SDS = value;
                if (f_SDS.FxS == null)
                    InitializeFxS();
            }
        }

        protected virtual void InitializeFxS()
        {
            f_SDS.FxS = new BasicFxSettings();
        }

        public virtual bool ShouldSerializeSDS()
        {
            return !string.Equals(SDS.SerializeData, "{}", StringComparison.Ordinal);
        }

        public virtual void ResetSDS()
        {
            SDS.SerializeData = "{}";
            InitializeFxS();
        }

        [DataMember]
        [Description("Sound Control Change Settings\r\n" +
            "Link Data Entry message value with the Timbre property value (Only the property that has a slider editor)\r\n" +
            "eg) \"DutyCycle,Volume\" ... You can change DutyCycle and Volume property values dynamically via MIDI Control Change No.7x message.")]
        [DisplayName("Sound Control Change Settings[SCCS]")]
        [Category("General")]
        public virtual SoundControlChangeSettings SCCS
        {
            get;
            set;
        }

        public virtual bool ShouldSerializeSCCS()
        {
            return !string.Equals(SCCS.SerializeData, "{}", StringComparison.Ordinal);
        }

        public virtual void ResetSCCS()
        {
            SCCS.SerializeData = "{}";
        }

        private string f_TimbreName;

        [DataMember]
        [Description("Name")]
        [DefaultValue(null)]
        public string TimbreName
        {
            get
            {
                return f_TimbreName;
            }
            set
            {
                f_TimbreName = value;
                if (f_TimbreName != null)
                {
                    f_TimbreName = f_TimbreName.Replace("\0", string.Empty);
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(DisplayName)));
                }
            }
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

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            typeof(UITypeEditor)), Localizable(false)]
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

        private static Regex removeRegex = new Regex(@"[ ()+\-^*/^$]|\bsin\b|\bcos\b|\btg\b|\bctg\b|\bsh\b|\bch\b|\bth\b|\bsqrt\b|\bexp\b|\blog\b|\bln\b|\babs\b|\bpi\b|\be\b", RegexOptions.Compiled);

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
                return TimbreName;
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


    }
}
