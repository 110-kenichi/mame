using System;
using System.IO;

namespace Praat2Lpc
{

    public class LpcStopFrame : LpcFrame
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
            return 15L;
        }

        public override string ToString(LpcQuantizer var1)
        {
            return ToString();
        }

        public override LpcFrame Clone()
        {
            return (LpcStopFrame)MemberwiseClone();
        }

        public override string ToString()
        {
            return "Stop";
        }
    }
}