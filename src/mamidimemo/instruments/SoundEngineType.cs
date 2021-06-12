// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments
{
    [Editor(typeof(EnumTypeEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(EnumConverter<SoundEngineType>))]
    public enum SoundEngineType
    {
        [Description("Software")]
        Software,
        [Description("Real(SPFM)")]
        SPFM,
        [Description("Real(VSIF SMS)")]
        VSIF_SMS,
        [Description("Real(VSIF Genesis(UART 163840bps)")]
        VSIF_Genesis,
        [Description("Real(VSIF Genesis(UART 1152000bps)")]
        VSIF_Genesis_Low,
        [Description("Real(VSIF Genesis(FTDI))")]
        VSIF_Genesis_FTDI,
        [Description("Real(VSIF NES(FTDI))")]
        VSIF_NES_FTDI,
    }
}
