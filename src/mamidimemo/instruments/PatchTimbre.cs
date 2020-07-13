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
    [JsonConverter(typeof(NoTypeConverterJsonConverter<TimbreBase>))]
    [DataContract]
    [MidiHook]
    public class PatchTimbre : TimbreBase
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

        private string f_BindTimbres;

        [DataMember]
        [Description("Set the timbre numbers to bind this patch by text. Input the timbre numbers(0-127) and split it with space.")]
        public string BindTimbres
        {
            get
            {
                return f_BindTimbres;
            }
            set
            {
                if (f_BindTimbres != value)
                {
                    if (value == null)
                    {
                        BindTimbreNums = new int[] { };
                        f_BindTimbres = string.Empty;
                        return;
                    }
                    f_BindTimbres = value;
                    string[] vals = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> vs = new List<int>();
                    for (int i = 0; i < vals.Length; i++)
                    {
                        string val = vals[i];
                        int v;
                        if (int.TryParse(val, out v))
                        {
                            if (v < 0)
                                v = 0;
                            else if (v >= InstrumentBase.MAX_TIMBRES)   // 0 - 127
                                v = 7;
                            vs.Add(v);
                        }
                    }
                    BindTimbreNums = vs.ToArray();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < BindTimbreNums.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        sb.Append(BindTimbreNums[i].ToString((IFormatProvider)null));
                    }
                    f_BindTimbres = sb.ToString();
                }
            }
        }

        public bool ShouldSerializeBindTimbres()
        {
            return !string.IsNullOrEmpty(BindTimbres);
        }

        public void ResetBindTimbres()
        {
            BindTimbres = null;
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public int[] BindTimbreNums { get; set; } = new int[] { };

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public override SoundDriverSettings SDS
        {
            get
            {
                return SDS2;
            }
            set
            {
            }
        }

        [DataMember]
        [Description("Sound Driver Settings")]
        [DisplayName("Sound Driver Settings(SDS)")]
        public PatchSoundDriverSettings SDS2
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public PatchTimbre()
        {
            SDS2 = new PatchSoundDriverSettings();
        }

        public override void RestoreFrom(string serializeData)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<PatchTimbre>(serializeData);
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
    public class PatchSoundDriverSettings : SoundDriverSettings
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
