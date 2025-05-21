using System;
using System.Text;

namespace Praat2Lpc
{

    public class LpcVoicedFrame(int lpcEnergy, int pitch, long quantizedCoefficients) : LpcPitchFrame(lpcEnergy, pitch)
    {
        public long k = quantizedCoefficients;

        public override int BitCount
        {
            get
            {
                return 50;
            }
        }

        public override long ToBits()
        {
            return (long)Energy << 46 | (long)Pitch << 39 | k;
        }

        public override String ToString(LpcQuantizer q)
        {
            int[] decodedCoefficients = q.DecodeLpcCoefficients(this.k, true);
            StringBuilder sb = new ();

            for (int i = 0; i < decodedCoefficients.Length; ++i)
            {
                sb.Append(String.Format("{0}", q.lpcCoefficientTable[i][decodedCoefficients[i]]));
                if (i < decodedCoefficients.Length - 1)
                {
                    sb.Append(',');
                }
            }

            return string.Format("Voiced(energy={0}, pitch={1}, k={2})", q.energyTable[this.Energy], q.pitchTable[this.Pitch], sb.ToString());
        }

        public override LpcFrame Clone()
        {
            return (LpcVoicedFrame)MemberwiseClone();
        }

        public override string ToString()
        {
            return String.Format("Voiced(energy={0}, pitch={1}, k={2})", this.Energy, this.Pitch, this.k);
        }
    }
}