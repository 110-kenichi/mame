using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praat2Lpc
{
    public class VlmLpcSilenceFrame : LpcFrame
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
            return 1L;
        }

        public override long ToReversedBits()
        {
            return ToBits();
        }

        public override String ToString(LpcQuantizer q)
        {
            return ToString();
        }

        public override LpcSilenceFrame Clone()
        {
            return (LpcSilenceFrame)MemberwiseClone();
        }

        public override string ToString()
        {
            return "VLM Silence";
        }
    }

}
