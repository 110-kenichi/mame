using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.VSIF
{
    /// <summary>
    /// 
    /// </summary>
    public class PortWriteData
    {
        public byte Type;

        public byte Address;

        public byte Data;

        public int Wait;

        public int Command;

        public Dictionary<string, Object> Tag;

        public long Tick;

        public override string ToString()
        {
            return $"tick={Tick} tp={Type} a={Address} d={Data}";
        }
    }
}
