// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Midi
{
    public class TaggedNoteOnEvent : NoteEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public NoteOnEvent NoteOnEvent
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteOnEvent"></param>
        public TaggedNoteOnEvent(NoteOnEvent noteOnEvent) : base(noteOnEvent.EventType, noteOnEvent.NoteNumber, noteOnEvent.Velocity)
        {
            NoteOnEvent = noteOnEvent;
            Channel = noteOnEvent.Channel;
            DeltaTime = noteOnEvent.DeltaTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteNumber"></param>
        /// <param name="velocity"></param>
        public TaggedNoteOnEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
            : base(MidiEventType.NoteOn, noteNumber, velocity)
        {
            NoteOnEvent = new NoteOnEvent(noteNumber, velocity);
        }

        public override string ToString()
        {
            return NoteOnEvent.ToString();
        }


        protected override MidiEvent CloneEvent()
        {
            var non = new NoteOnEvent(base.NoteNumber, base.Velocity)
            {
                Channel = base.Channel
            };
            return new TaggedNoteOnEvent(non);
        }

        /// <summary>
        /// 
        /// </summary>
        public object Tag
        {
            get;
            set;
        }
    }
}
