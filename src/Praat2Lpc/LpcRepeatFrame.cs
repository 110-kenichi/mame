using System;

namespace Praat2Lpc
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lpcEnergy"></param>
    /// <param name="pitch"></param>
    public class LpcRepeatFrame(int lpcEnergy, int pitch) : LpcPitchFrame(lpcEnergy, pitch)
    {
        public override int BitCount
        {
            get
            {
                return 11;
            }
        }

        public override long ToBits()
        {
            return (long)Energy << 7 | 64L | (long)Pitch;
        }

        public override string ToString(LpcQuantizer var1)
        {
            return string.Format("Repeat(energy={0}, pitch={1})", var1.energyTable[this.Energy], var1.pitchTable[this.Pitch]);
        }

        public override LpcFrame Clone()
        {
            return (LpcRepeatFrame)MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format("Repeat(energy={0}, pitch={1})", this.Energy, this.Pitch);
        }
    }
}