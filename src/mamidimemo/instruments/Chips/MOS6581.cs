// copyright-holders:K.Ito
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments.Chips
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [InstLock]
    public class MOS6581 : SIDBase
    {

        public override string Name => "MOS6581";

        public override InstrumentType InstrumentType => InstrumentType.MOS6581;

        [Browsable(false)]
        public override string ImageKey => "MOS6581";

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        protected override string SoundInterfaceTagNamePrefix => "mos6581_";

        [Browsable(false)]
        protected override string WriteProcName => "mos6581_write";

        /// <summary>
        /// 
        /// </summary>
        [Category("MIDI")]
        [Description("MIDI Device ID")]
        [IgnoreDataMember]
        [JsonIgnore]
        public override uint DeviceID
        {
            get
            {
                return 13;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public MOS6581(uint unitNumber) : base(unitNumber)
        {
            
        }

    }
}