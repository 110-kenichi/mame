// copyright-holders:K.Ito
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
    [MidiHook]
    public class CombinedTimbre : TimbreBase
    {
        [IgnoreDataMember]
        [Browsable(false)]
        public new bool IgnoreKeyOff
        {
            get;
            set;
        }

        [IgnoreDataMember]
        [Browsable(false)]
        public new int KeyShift
        {
            get;
            set;
        }

        [IgnoreDataMember]
        [Browsable(false)]
        public new int PitchShift
        {
            get;
            set;
        }

        [Editor(typeof(DummyEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DataMember]
        [Description("Set the timbre numbers to bind this Combibed Timbre.")]
        public ProgramAssignmentTimbreNumber?[] BindTimbres { get; set; } = new ProgramAssignmentTimbreNumber?[4] {
            null,
            null,
            null,
            null
        };

        public bool ShouldSerializeBindTimbres()
        {
            for (int i = 0; i < BindTimbres.Length; i++)
            {
                if (BindTimbres[i] != null)
                    return true;
            }
            return false;
        }

        public void ResetBindTimbres()
        {
            for (int i = 0; i < BindTimbres.Length; i++)
                BindTimbres[i] = null;
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

        [DataMember]
        [Description("Sound Driver Settings")]
        [DisplayName("Sound Driver Settings(SDS)")]
        public CombinedSoundDriverSettings CSDS
        {
            get;
            set;
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
