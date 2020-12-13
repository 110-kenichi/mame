// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Midi
{
    public class NoteOnTimbreInfo 
    {
        /// <summary>
        /// 
        /// </summary>
        public TimbreBase Timbre
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int TimbreNo
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="noteOnEvent"></param>
        public NoteOnTimbreInfo(TimbreBase timbre, int timbreNo)
        {
            Timbre = timbre;
            TimbreNo = timbreNo;
        }

    }
}
