using System;

namespace Praat2Lpc
{
    public abstract class LpcFrame : ICloneable
    {
        public abstract int BitCount
        {
            get;
        }

        public abstract long ToBits();

        public abstract string ToString(LpcQuantizer var1);

        public abstract object Clone();

        public virtual long ToReversedBits()
        {
            ulong input = (ulong)ToBits();
            ulong result = 0;

            for (int i = 0; i < 64; i++)
            {
                result <<= 1;
                result |= input & 1;
                input >>= 1;
            }

            return ((long)result) >>> 64 - BitCount;
        }

    }
}