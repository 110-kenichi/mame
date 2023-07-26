// copyright-holders:K.Ito
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// 
    /// </summary>
    public class EnvironmentSettings
    {

        [JsonConverter(typeof(NoTypeConverterJsonConverterObject<InstrumentBase>))]
        [DataMember]
        public List<List<InstrumentBase>> Instruments;

        [JsonConverter(typeof(NoTypeConverterJsonConverterObject<InstrumentBase>))]
        [DataMember]
        public List<InstrumentBase> MidiThru;

    }

}
