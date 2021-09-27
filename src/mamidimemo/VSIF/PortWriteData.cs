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
    }
}
