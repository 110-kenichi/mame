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
        [Description("Real(VSIF SMS(UART 115200bps)")]
        VSIF_SMS,
        [Description("Real(VSIF Genesis(UART 163840bps)")]
        VSIF_Genesis,
        [Description("Real(VSIF Genesis(UART 115200bps)")]
        VSIF_Genesis_Low,
        [Description("Real(VSIF Genesis(FTDI))")]
        VSIF_Genesis_FTDI,
        [Description("Real(VSIF Famicom(FTDI))")]
        VSIF_NES_FTDI,
        [Description("Real(VSIF Famicom VRC6(FTDI))")]
        VSIF_NES_FTDI_VRC6,
        [Description("Real(VSIF Famicom FDS(FTDI))")]
        VSIF_NES_FTDI_FDS,
        [Description("Real(VSIF Famicom MMC5(FTDI))")]
        VSIF_NES_FTDI_MMC5,
        [Description("Real(VSIF MSX(FTDI))")]
        VSIF_MSX_FTDI,
        [Description("Real(CMI8738)")]
        Real_OPL3,
        [Description("Real(VSIF C64(FTDI))")]
        VSIF_C64_FTDI,
    }
}
