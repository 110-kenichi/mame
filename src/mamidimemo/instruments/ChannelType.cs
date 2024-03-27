// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(EnumConverter<ChannelType>))]
    public enum ChannelType
    {
        Normal,
        Drum,
        [Description("Drum(Ignore GateTime)")]
        DrumGt,
    }
}
