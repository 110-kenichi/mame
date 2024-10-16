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

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<CombinedTimbre>))]
    [DataContract]
    [InstLock]
    public class CombinedTimbre : TimbreBase
    {

        [Browsable(false)]
        [DataMember]
        [DefaultValue(null)]
        [Obsolete]
        public ProgramAssignmentTimbreNumber?[] BindTimbres
        {
            get
            {
                return null;
            }
            set
            {
                // for compatibility
                if (value != null)
                {
                    foreach (var i in value)
                    {
                        if (i != null)
                        {
                            var cts = new CombinedTimbreSettings();
                            cts.TimbreNumber = i.Value;
                            Timbres.Add(cts);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("Set the timbre to bind this Combined Timbre.")]
        public CombinedTimbreSettingsCollection Timbres
        {
            get;
            set;
        } = new CombinedTimbreSettingsCollection();

        public bool ShouldSerializeTimbres()
        {
            return Timbres.Count != 0;
        }

        public void ResetTimbres()
        {
            Timbres.Clear();
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public override bool AssignMIDIChtoSlotNum
        {
            get;
            set;
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public override int AssignMIDIChtoSlotNumOffset
        {
            get;
            set;
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public override MidiDriverSettings MDS
        {
            get;
            set;
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public override SoundDriverSettings SDS
        {
            get
            {
                return CSDS;
            }
            set
            {
            }
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        [Description("Sound Driver Settings")]
        [DisplayName("Sound Driver Settings[CSDS]")]
        public CombinedSoundDriverSettings CSDS
        {
            get;
            set;
        }


        public virtual bool ShouldSerializeCSDS()
        {
            return !string.Equals(CSDS.SerializeData, "{}", StringComparison.Ordinal);
        }

        public virtual void ResetCSDS()
        {
            CSDS.SerializeData = "{}";
        }

        /// <summary>
        /// 
        /// </summary>
        public CombinedTimbre()
        {
            CSDS = new CombinedSoundDriverSettings();
        }

        public override void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<CombinedTimbre>(serializeData);
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
        public override string DisplayName
        {
            get
            {
                if (TimbreName != null)
                {
                    return TimbreName;
                }
                else if (Timbres.Count != 0)
                {
                    string name = string.Empty;
                    foreach (var t in Timbres)
                    {
                        if (t.DisplayName != null)
                        {
                            if (name.Length != 0)
                                name += ",";
                            name += t.TimberObject.DisplayName;
                        }
                    }
                    return name;
                }
                return TimbreName;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class CombinedSoundDriverSettings : SoundDriverSettings
    {

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public override AdsrSettings ADSR { get => base.ADSR; set => base.ADSR = value; }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public override AbstractFxSettingsBase FxS { get => base.FxS; set => base.FxS = value; }
    }

}
