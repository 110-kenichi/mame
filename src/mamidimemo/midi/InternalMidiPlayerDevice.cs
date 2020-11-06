using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
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

        public void PrepareForEventsSending()
        {
        }

        public void SendEvent(MidiEvent midiEvent)
        {
            MidiManager.SendMidiEvent(midiEvent);
            EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
        }
    }
}
