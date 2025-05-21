using System;
using System.Text;

namespace Praat2Lpc
{

    public class LpcUnvoicedFrame(int energy, long quantizedCoefficients) : LpcEnergyFrame(energy)
    {
        public long k = quantizedCoefficients;

        public override int BitCount
        {
            get
            {
                return 29;
            }
        }

        public override long ToBits()
        {
            return (long)Energy << 25 | k;
        }

        public override string ToString(LpcQuantizer q)
        {
            int[] decodedCoefficients = q.DecodeLpcCoefficients(k, false);
            StringBuilder sb = new();

            for (int i = 0; i < decodedCoefficients.Length; ++i)
            {
                sb.Append(string.Format("{0}", q.lpcCoefficientTable[i][decodedCoefficients[i]]));
                if (i < decodedCoefficients.Length - 1)
                {
                    sb.Append(',');
                }
            }

            return string.Format("Unvoiced(energy={0}, k={1})", q.energyTable[Energy], sb.ToString());
        }

        public override LpcFrame Clone()
        {
            return (LpcUnvoicedFrame)MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format("Unvoiced(energy={0}, k={1})", Energy, k);
        }
    }
}