// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Midi
{
    [Editor(typeof(EnumTypeEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(EnumConverter<MidiPort>))]
    public enum MidiPort 
    {
        [Description("PORT A and B")]
        PortAB,
        [Description("PORT A")]
        PortA,
        [Description("PORT B")]
        PortB,
    }
}
