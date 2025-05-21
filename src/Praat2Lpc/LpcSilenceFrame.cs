using System;
using System.IO;

namespace Praat2Lpc
{
    public class LpcSilenceFrame : LpcFrame
    {
        public override int BitCount
        {
            get
            {
                return 4;
            }
        }

        public override long ToBits()
        {
            return 0L;
        }

        public override String ToString(LpcQuantizer q)
        {
            return "Silence";
        }

        public override LpcFrame Clone()
        {
            return (LpcSilenceFrame)MemberwiseClone();
        }

        public override String ToString()
        {
            return "Silence";
        }
    }
}
