using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Praat2Lpc
{
    internal class VlmLpcUnvoicedFrame(int energy, long k) : LpcEnergyFrame(energy)
    {
        public long k = k;

        public override int BitCount
        {
            get
            {
                return 48;
            }
        }

        public override long ToBits()
        {
            long bit = (long)this.Energy << 6 | this.k << 11;
            return bit;
        }

        public override string ToString(LpcQuantizer q)
        {
            int[] decodedCoefficients = q.DecodeLpcCoefficients(k, false);
            StringBuilder sb = new ();

            for (int i = 0; i < decodedCoefficients.Length; ++i)
            {
                sb.Append(string.Format("{0}", q.lpcCoefficientTable[i][decodedCoefficients[i]]));
                if (i < decodedCoefficients.Length - 1)
                {
                    sb.Append(',');
                }
            }

            return string.Format("VLM Unvoiced(energy={0}, k={1})", q.energyTable[Energy], sb.ToString());
        }

        public override LpcUnvoicedFrame Clone()
        {
            return (LpcUnvoicedFrame)MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format("VLM Unvoiced(energy={0}, k={1})", Energy, k);
        }
    }
}
