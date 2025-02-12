﻿using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Midi
{

    /// <summary>
    /// 
    /// </summary>
    public class InternalMidiPlayerDevice : IOutputDevice
    {
        public event EventHandler<MidiEventSentEventArgs> EventSent;

        public void Dispose()
        {
        }

        public void PrepareForEventsSending()
        {
        }

        public void SendEvent(MidiEvent midiEvent)
        {
            MidiManager.SendMidiEvent(MidiPort.PortA, midiEvent);
            EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
        }
    }
}
