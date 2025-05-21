using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Praat2Lpc
{
    internal class VlmLpcVoicedFrame(int energy, int pitch, long k) : LpcPitchFrame(energy, pitch)
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
            long bit = (long)Energy << 6 | (long)Pitch << 1 | k << 11;
            return bit;
        }

        public override String ToString(LpcQuantizer q)
        {
            int[] var2 = q.DecodeLpcCoefficients(this.k, true);
            StringBuilder sb = new ();

            for (int i = 0; i < var2.Length; ++i)
            {
                sb.Append(string.Format("{0}", q.lpcCoefficientTable[i][var2[i]]));
                if (i < var2.Length - 1)
                {
                    sb.Append(',');
                }
            }

            return string.Format("VLM Voiced(energy={0}, pitch={1}, k={2})", q.energyTable[this.Energy], q.pitchTable[this.Pitch], sb.ToString());
        }

        public override LpcVoicedFrame Clone()
        {
            return (LpcVoicedFrame)MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format("VLM Voiced(energy={0}, pitch={1}, k={2})", this.Energy, this.Pitch, this.k);
        }
    }
}
