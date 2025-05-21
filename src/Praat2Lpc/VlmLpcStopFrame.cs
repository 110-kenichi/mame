using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praat2Lpc
{
    public class VlmLpcStopFrame : LpcFrame
    {
        public override int BitCount
        {
            get
            {
                return 8;
            }
        }

        public override long ToBits()
        {
            return 3L;
        }

        public override long ToReversedBits()
        {
            return ToBits();
        }

        public override String ToString(LpcQuantizer var1)
        {
            return "VLM Stop";
        }

        public override LpcStopFrame Clone()
        {
            return (LpcStopFrame)MemberwiseClone();
        }
    }

}
