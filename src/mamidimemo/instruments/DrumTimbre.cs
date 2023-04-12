// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Midi;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(CustomExpandableObjectConverter))]
    [JsonConverter(typeof(NoTypeConverterJsonConverter<DrumTimbre>))]
    [DataContract]
    [InstLock]
    public class DrumTimbre : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        [IgnoreDataMember]
        public InstrumentBase Instrument
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [IgnoreDataMember]
        [ReadOnly(true)]
        public String KeyName
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [IgnoreDataMember]
        [ReadOnly(true)]
        public int NoteNumber
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [TypeConverter(typeof(ProgramAssignmentNumberConverter))]
        [EditorAttribute(typeof(ProgramAssignmentNumberTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public ProgramAssignmentNumber? TimbreNumber
        {
            get;
            set;
        }

        private NoteNames f_BaseNote;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [TypeConverter(typeof(NoteNameConverter))]
        [EditorAttribute(typeof(NoteNumberTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public NoteNames BaseNote
        {
            get
            {
                return f_BaseNote;
            }
            set
            {
                f_BaseNote = value;
            }
        }

        public bool ShouldSerializeBaseNote()
        {
            return BaseNote != NoteNames.C4;
        }

        public void ResetBaseNote()
        {
            BaseNote = NoteNames.C4;
        }

        private uint f_GateTime = 500;

        [DataMember]
        [Description("Gate Time[ms]")]
        [DefaultValue(typeof(uint), "500")]
        [SlideParametersAttribute(1, 10000)]
        [EditorAttribute(typeof(SlideEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public uint GateTime
        {
            get
            {
                return f_GateTime;
            }
            set
            {
                if (value > 10000)
                    value = 10000;
                if (f_GateTime != value && value >= 1)
                {
                    f_GateTime = value;
                }
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

        private String f_TimbreName;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public String TimbreName
        {
            get
            {
                if (f_TimbreName != null)
                    return f_TimbreName;
                else if (Instrument != null)
                {
                    if (this.TimbreNumber != null && (int)(TimbreNumber.Value) < Instrument.BaseTimbres.Length)
                    {
                        return Instrument.BaseTimbres[(int)(TimbreNumber.Value)].TimbreName;
                    }
                }
                return null;
            }
            set
            {
                f_TimbreName = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteNumber"></param>
        public DrumTimbre(int noteNumber, ProgramAssignmentNumber? timbreNumber)
        {
            NoteNumber = noteNumber;
            KeyName = Midi.MidiManager.GetNoteName((SevenBitNumber)noteNumber);

            TimbreNumber = timbreNumber;
            BaseNote = NoteNames.C4;
        }
    }
}