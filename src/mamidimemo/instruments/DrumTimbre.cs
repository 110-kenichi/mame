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
    public class DrumTimbre : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        [IgnoreDataMember]
        public String KeyName
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public int NoteNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public ProgramAssignmentNumber TimbreNumber
        {
            get;
            set;
        }

        private byte f_BaseNote;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public byte BaseNote
        {
            get
            {
                return f_BaseNote;
            }
            set
            {
                f_BaseNote = value;
                if (f_BaseNote > 127)
                    f_BaseNote = 127;
            }
        }

        public bool ShouldSerializeBaseNote()
        {
            return BaseNote != 60;
        }

        public void ResetBaseNote()
        {
            BaseNote = 60;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteNumber"></param>
        public DrumTimbre(int noteNumber, ProgramAssignmentNumber timbreNumber)
        {
            NoteNumber = noteNumber;
            var no = new NoteOnEvent((SevenBitNumber)NoteNumber, (SevenBitNumber)0);
            KeyName = no.GetNoteName() + no.GetNoteOctave().ToString();

            TimbreNumber = timbreNumber;
            BaseNote = 60;
        }
    }
}