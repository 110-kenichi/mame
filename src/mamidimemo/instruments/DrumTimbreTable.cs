// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [MidiHook]
    public class DrumTimbreTable : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public DrumTimbre[] DrumTimbres
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public DrumTimbreTable()
        {
            DrumTimbres = new DrumTimbre[128];
            for (int i = 0; i < DrumTimbres.Length; i++)
                DrumTimbres[i] = new DrumTimbre(i, (ProgramAssignmentNumber)((int)ProgramAssignmentNumber.CombinedTimbre0 + 128 + i));
        }

    }

}
