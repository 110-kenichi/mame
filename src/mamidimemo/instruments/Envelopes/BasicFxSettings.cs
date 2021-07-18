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

namespace zanac.MAmidiMEmo.Instruments.Envelopes
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(NoTypeConverterJsonConverter<BasicFxSettings>))]
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [DataContract]
    public class BasicFxSettings : AbstractFxSettingsBase
    {
        private string f_VolumeEnvelopes;

        [DataMember]
        [Description("Set volume envelop by text. Input volume value and split it with space like the Famitracker.\r\n" +
                    "0(0%)-127(100%) \"|\" is repeat point. \"/\" is release point.")]
        [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [EnvelopeEditorAttribute(0,127)]
        public string VolumeEnvelopes
        {
            get
            {
                return f_VolumeEnvelopes;
            }
            set
            {
                if (f_VolumeEnvelopes != value)
                {
                    VolumeEnvelopesRepeatPoint = -1;
                    VolumeEnvelopesReleasePoint = -1;
                    if (value == null)
                    {
                        VolumeEnvelopesNums = new int[] { };
                        f_VolumeEnvelopes = string.Empty;
                        return;
                    }
                    f_VolumeEnvelopes = value;
                    string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> vs = new List<int>();
                    for (int i = 0; i < vals.Length; i++)
                    {
                        string val = vals[i];
                        if (val.Equals("|", StringComparison.Ordinal))
                            VolumeEnvelopesRepeatPoint = vs.Count;
                        else if (val.Equals("/", StringComparison.Ordinal))
                            VolumeEnvelopesReleasePoint = vs.Count;
                        else
                        {
                            int v;
                            if (int.TryParse(val, out v))
                            {
                                if (v < 0)
                                    v = 0;
                                else if (v > 127)
                                    v = 127;
                                vs.Add(v);
                            }
                        }
                    }
                    VolumeEnvelopesNums = vs.ToArray();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < VolumeEnvelopesNums.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        if (VolumeEnvelopesRepeatPoint == i)
                            sb.Append("| ");
                        if (VolumeEnvelopesReleasePoint == i)
                            sb.Append("/ ");
                        sb.Append(VolumeEnvelopesNums[i].ToString((IFormatProvider)null));
                    }
                    f_VolumeEnvelopes = sb.ToString();
                }
            }
        }

        public bool ShouldSerializeVolumeEnvelopes()
        {
            return !string.IsNullOrEmpty(VolumeEnvelopes);
        }

        public void ResetVolumeEnvelopes()
        {
            VolumeEnvelopes = null;
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public int[] VolumeEnvelopesNums { get; set; } = new int[] { };

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        [DefaultValue(-1)]
        public int VolumeEnvelopesRepeatPoint { get; set; } = -1;

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        [DefaultValue(-1)]
        public int VolumeEnvelopesReleasePoint { get; set; } = -1;
        
        private string f_PitchEnvelopes;

        [DataMember]
        [Description("Set pitch envelop by text. Input pitch relative value and split it with space like the Famitracker.\r\n" +
                   "-8193 ～ 0 ～ 8192 \"|\" is repeat point. \"/\" is release point.")]
        [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [EnvelopeEditorAttribute(-8193, 8192)]
        public string PitchEnvelopes
        {
            get
            {
                return f_PitchEnvelopes;
            }
            set
            {
                if (f_PitchEnvelopes != value)
                {
                    PitchEnvelopesRepeatPoint = -1;
                    PitchEnvelopesReleasePoint = -1;
                    if (value == null)
                    {
                        PitchEnvelopesNums = new int[] { };
                        f_PitchEnvelopes = string.Empty;
                        return;
                    }
                    f_PitchEnvelopes = value;
                    string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> vs = new List<int>();
                    for (int i = 0; i < vals.Length; i++)
                    {
                        string val = vals[i];
                        if (val.Equals("|", StringComparison.Ordinal))
                            PitchEnvelopesRepeatPoint = vs.Count;
                        else if (val.Equals("/", StringComparison.Ordinal))
                            PitchEnvelopesReleasePoint = vs.Count;
                        else
                        {
                            int v;
                            if (int.TryParse(val, out v))
                            {
                                if (v < -8193)
                                    v = -8193;
                                else if (v > 8192)
                                    v = 8192;
                                vs.Add(v);
                            }
                        }
                    }
                    PitchEnvelopesNums = vs.ToArray();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < PitchEnvelopesNums.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        if (PitchEnvelopesRepeatPoint == i)
                            sb.Append("| ");
                        if (PitchEnvelopesReleasePoint == i)
                            sb.Append("/ ");
                        sb.Append(PitchEnvelopesNums[i].ToString((IFormatProvider)null));
                    }
                    f_PitchEnvelopes = sb.ToString();
                }
            }
        }

        public bool ShouldSerializePitchEnvelopes()
        {
            return !string.IsNullOrEmpty(PitchEnvelopes);
        }

        public void ResetPitchEnvelopes()
        {
            PitchEnvelopes = null;
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public int[] PitchEnvelopesNums { get; set; } = new int[] { };

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        [DefaultValue(-1)]
        public int PitchEnvelopesRepeatPoint { get; set; } = -1;

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        [DefaultValue(-1)]
        public int PitchEnvelopesReleasePoint { get; set; } = -1;

        private PitchStepType f_PitchStepType;


        [DataMember]
        [Description("Set pitch step type.")]
        [DefaultValue(PitchStepType.Relative)]
        public PitchStepType PitchStepType
        {
            get
            {
                return f_PitchStepType;
            }
            set
            {
                if (f_PitchStepType != value)
                {
                    f_PitchStepType = value;
                }
            }
        }

        private int f_PitchEnvelopeRange = 2;

        [DataMember]
        [Description("Pitch envelope sensitivity 0-127 [half note]")]
        [DefaultValue(2)]
        public int PitchEnvelopeRange
        {
            get
            {
                return f_PitchEnvelopeRange;
            }
            set
            {
                if (f_PitchEnvelopeRange != value && value > 0 && value < 127)
                    f_PitchEnvelopeRange = value;
            }
        }


        private string f_PanShiftEnvelopes;

        [DataMember]
        [Description("Set pan shift envelop by text. Input pan value and split it with space like the Famitracker.\r\n" +
                    "-127(Left)-0-127(Right) \"|\" is repeat point. \"/\" is release point.")]
        [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [EnvelopeEditorAttribute(-127, 127)]
        public string PanShiftEnvelopes
        {
            get
            {
                return f_PanShiftEnvelopes;
            }
            set
            {
                if (f_PanShiftEnvelopes != value)
                {
                    PanShiftEnvelopesRepeatPoint = -1;
                    PanShiftEnvelopesReleasePoint = -1;
                    if (value == null)
                    {
                        PanShiftEnvelopesNums = new int[] { };
                        f_PanShiftEnvelopes = string.Empty;
                        return;
                    }
                    f_PanShiftEnvelopes = value;
                    string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int> vs = new List<int>();
                    for (int i = 0; i < vals.Length; i++)
                    {
                        string val = vals[i];
                        if (val.Equals("|", StringComparison.Ordinal))
                            PanShiftEnvelopesRepeatPoint = vs.Count;
                        else if (val.Equals("/", StringComparison.Ordinal))
                            PanShiftEnvelopesReleasePoint = vs.Count;
                        else
                        {
                            int pan;
                            if (int.TryParse(val, out pan))
                            {
                                if (pan < -127)
                                    pan = -127;
                                else if (pan > 127)
                                    pan = 127;
                                vs.Add(pan);
                            }
                        }
                    }
                    PanShiftEnvelopesNums = vs.ToArray();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < PanShiftEnvelopesNums.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        if (PanShiftEnvelopesRepeatPoint == i)
                            sb.Append("| ");
                        if (PanShiftEnvelopesReleasePoint == i)
                            sb.Append("/ ");
                        sb.Append(PanShiftEnvelopesNums[i].ToString((IFormatProvider)null));
                    }
                    f_PanShiftEnvelopes = sb.ToString();
                }
            }
        }

        public bool ShouldSerializePanShiftEnvelopes()
        {
            return !string.IsNullOrEmpty(PanShiftEnvelopes);
        }

        public void ResetPanShiftEnvelopes()
        {
            PanShiftEnvelopes = null;
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public int[] PanShiftEnvelopesNums { get; set; } = new int[] { };

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        [DefaultValue(-1)]
        public int PanShiftEnvelopesRepeatPoint { get; set; } = -1;

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        [DefaultValue(-1)]
        public int PanShiftEnvelopesReleasePoint { get; set; } = -1;


        private string f_ArpEnvelopes;

        [DataMember]
        [Description("Set static arpeggio envelop by text. Input relative or absolute note number and split it with space like the Famitracker.\r\n" +
                   "-128 ～ 0 ～ 127 \"|\" is repeat point. \"/\" is release point.\r\n" +
            "\"-\" is no-operation.")]
        [Editor(typeof(EnvelopeUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [EnvelopeEditorAttribute(-128, 127)]
        public string ArpEnvelopes
        {
            get
            {
                return f_ArpEnvelopes;
            }
            set
            {
                if (f_ArpEnvelopes != value)
                {
                    ArpEnvelopesRepeatPoint = -1;
                    ArpEnvelopesReleasePoint = -1;
                    if (value == null)
                    {
                        ArpEnvelopesNums = new int?[] { };
                        f_ArpEnvelopes = string.Empty;
                        return;
                    }
                    f_ArpEnvelopes = value;
                    string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    List<int?> vs = new List<int?>();
                    for (int i = 0; i < vals.Length; i++)
                    {
                        string val = vals[i];
                        if (val.Equals("|", StringComparison.Ordinal))
                            ArpEnvelopesRepeatPoint = vs.Count;
                        else if (val.Equals("/", StringComparison.Ordinal))
                            ArpEnvelopesReleasePoint = vs.Count;
                        else if (val.Equals("-", StringComparison.Ordinal))
                            vs.Add(null);
                        else
                        {
                            int v;
                            if (int.TryParse(val, out v))
                            {
                                if (v < -128)
                                    v = -128;
                                else if (v > 127)
                                    v = 127;
                                vs.Add(v);
                            }
                        }
                    }
                    ArpEnvelopesNums = vs.ToArray();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < ArpEnvelopesNums.Length; i++)
                    {
                        if (sb.Length != 0)
                            sb.Append(' ');
                        if (ArpEnvelopesRepeatPoint == i)
                            sb.Append("| ");
                        if (ArpEnvelopesReleasePoint == i)
                            sb.Append("/ ");
                        if(ArpEnvelopesNums[i] == null)
                            sb.Append("-");
                        else
                            sb.Append(ArpEnvelopesNums[i].Value.ToString((IFormatProvider)null));
                    }
                    f_ArpEnvelopes = sb.ToString();
                }
            }
        }

        public bool ShouldSerializeArpEnvelopes()
        {
            return !string.IsNullOrEmpty(ArpEnvelopes);
        }

        public void ResetArpEnvelopes()
        {
            ArpEnvelopes = null;
        }

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        public int?[] ArpEnvelopesNums { get; set; } = new int?[] { };

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        [DefaultValue(-1)]
        public int ArpEnvelopesRepeatPoint { get; set; } = -1;

        [Browsable(false)]
        [JsonIgnore]
        [IgnoreDataMember]
        [DefaultValue(-1)]
        public int ArpEnvelopesReleasePoint { get; set; } = -1;


        private ArpStepType f_ArpStepType;


        [DataMember]
        [Description("Set static arpeggio step type.")]
        [DefaultValue(ArpStepType.Absolute)]
        public ArpStepType ArpStepType
        {
            get
            {
                return f_ArpStepType;
            }
            set
            {
                if (f_ArpStepType != value)
                {
                    f_ArpStepType = value;
                }
            }
        }

        private ArpMethod f_ArpMethod = ArpMethod.PitchChange;

        [DataMember]
        [Description("Set arpeggio method (Note On or Pitch Change)")]
        [DefaultValue(ArpMethod.PitchChange)]
        public ArpMethod ArpMethod
        {
            get
            {
                return f_ArpMethod;
            }
            set
            {
                if (f_ArpMethod != value)
                {
                    f_ArpMethod = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override AbstractFxEngine CreateEngine()
        {
            return new BasicFxEngine(this);
        }

    }

    public enum PitchStepType
    {
        Relative,
        Absolute,
    }

    public enum ArpStepType
    {
        Absolute,
        Relative,
        Fixed,
    }


}
