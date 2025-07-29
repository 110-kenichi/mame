// copyright-holders:K.Ito
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
    [InstLock]
    public class PcmTimbreTableBase : ContextBoundObject
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public PcmTimbreBase[] PcmTimbres
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public PcmTimbreTableBase()
        {
            PcmTimbres = new PcmTimbreBase[128];
        }

        /// <summary>
        /// Submit changes to the PCM timbre table.
        /// </summary>
        public virtual void Submit()
        {
        }
    }
}