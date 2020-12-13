// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [MidiHook]
    public class PcmTimbreBase : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        [IgnoreDataMember]
        [Description("Key name of this timbre.")]
        public String KeyName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("Note number of this timbre.")]
        public int NoteNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Description("Set name of this timbre.")]
        public String TimbreName
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public virtual byte[] PcmData
        {
            get;
            set;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteNumber"></param>
        public PcmTimbreBase(int noteNumber)
        {
            NoteNumber = noteNumber;

            KeyName = Midi.MidiManager.GetNoteName((SevenBitNumber)noteNumber);
        }
    }
}