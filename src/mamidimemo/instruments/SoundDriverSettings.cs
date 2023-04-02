// copyright-holders:K.Ito
using Newtonsoft.Json;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments.Envelopes;
using static zanac.MAmidiMEmo.Instruments.SoundBase;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<SoundDriverSettings>))]
    [DataContract]
    [InstLock]
    public class SoundDriverSettings : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        public SoundDriverSettings()
        {
            ADSR = new AdsrSettings();
            ARP = new ArpSettings();
        }

        #region ADSR 

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("ADSR Settings")]
        public virtual AdsrSettings ADSR
        {
            get;
            set;
        }

        public virtual bool ShouldSerializeADSR()
        {
            return !string.Equals(ADSR.SerializeData, "{}", StringComparison.Ordinal);
        }

        public virtual void ResetADSR()
        {
            ADSR.SerializeData = "{}";
        }

        #endregion

        #region Arp

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("Dynamic Arpeggio Settings")]
        public virtual ArpSettings ARP
        {
            get;
            set;
        }

        public virtual bool ShouldSerializeARP()
        {
            return !string.Equals(ARP.SerializeData, "{}", StringComparison.Ordinal);
        }

        public virtual void ResetARP()
        {
            ARP.SerializeData = "{}";
        }

        #endregion

        #region Fx

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("Fx Settings")]
        [JsonConverter(typeof(NoTypeConverterJsonConverterObject<AbstractFxSettingsBase>))]
        public virtual AbstractFxSettingsBase FxS
        {
            get;
            set;
        }

        public virtual bool ShouldSerializeFxS()
        {
            return !string.Equals(FxS.SerializeData, "{}", StringComparison.Ordinal);
        }

        public virtual void ResetFxS()
        {
            FxS.SerializeData = "{}";
        }

        #endregion

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

        public virtual void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<SoundDriverSettings>(serializeData);
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

        public override bool Equals(object obj)
        {
            var mdsobj = obj as SoundDriverSettings;
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

