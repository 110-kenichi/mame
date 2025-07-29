using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Util
{
    public class OpnPcmConverter
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly long[] stepSizeTable = new long[16]{
             57,  57,  57,  57,  77, 102, 128, 153,
             57,  57,  57,  57,  77, 102, 128, 153
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcWave"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        public static byte[] EncodeAdpcmB(short[] srcWave, int maxSize)
        {
            List<byte> dest = new List<byte>();
            int lpc, flag;
            long i, dn, xn, stepSize;
            byte adpcm;
            byte adpcmPack = 0;

            xn = 0;
            stepSize = 127;
            flag = 0;

            int srcidx = 0;
            for (lpc = 0; lpc < srcWave.Length; lpc++)
            {
                dn = srcWave[srcidx] - xn;
                srcidx++;

                i = (Math.Abs(dn) << 16) / (stepSize << 14);
                if (i > 7)
                    i = 7;
                adpcm = (byte)i;

                i = (adpcm * 2 + 1) * stepSize / 8;

                if (dn < 0)
                {
                    adpcm |= 0x8;
                    xn -= i;
                }
                else
                {
                    xn += i;
                }

                stepSize = (stepSizeTable[adpcm] * stepSize) / 64;

                if (stepSize < 127)
                    stepSize = 127;
                else if (stepSize > 24576)
                    stepSize = 24576;

                if (flag == 0)
                {
                    adpcmPack = (byte)(adpcm << 4);
                    flag = 1;
                }
                else
                {
                    adpcmPack |= adpcm;
                    dest.Add(adpcmPack);
                    if (dest.Count - 1 == maxSize)
                        break;
                    flag = 0;
                }
            }

            return dest.ToArray();
        }

        /* usual ADPCM table (16 * 1.1^N)
         * (and by "usual" I guess they mean the shortened OKI ADPCM table )*/
        static int[] step_size = new int[49]{
               16,  17,   19,   21,   23,   25,   28,
               31,  34,   37,   41,   45,   50,   55,
               60,  66,   73,   80,   88,   97,  107,
              118, 130,  143,  157,  173,  190,  209,
              230, 253,  279,  307,  337,  371,  408,
              449, 494,  544,  598,  658,  724,  796,
              876, 963, 1060, 1166, 1282, 1411, 1552
        };

        /* different from the usual ADPCM table */
        private static readonly int[] step_adj = new int[] {
                -1, -1, -1, -1, 2, 5, 7, 9,
                -1, -1, -1, -1, 2, 5, 7, 9
        };

        private static int[] f_jedi_table;

        private static int[] jedi_table
        {
            get
            {
                if (f_jedi_table == null)
                {
                    f_jedi_table = new int[49 * 16];
                    int step, nib;

                    for (step = 0; step < 49; step++)
                    {
                        /* loop over all nibbles and compute the difference */
                        for (nib = 0; nib < 16; nib++)
                        {
                            int value = (2 * (nib & 0x07) + 1) * step_size[step] / 8;
                            f_jedi_table[step * 16 + nib] = (nib & 0x08) != 0 ? -value : value;
                        }
                    }
                }
                return f_jedi_table;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] EncodeAdpcmA(short[] data)
        {
            /* decoding */
            int adpcm_accum = 0; /* ADPCM accumulator; initial condition must be 0 */
            int adpcm_decstep = 0;   /* ADPCM decoding step; initial condition must be 0 */

            /* encoding */
            short diff = 0;
            short step = 0;
            int predictsample = 0;
            int index = 0;                   /* Index into step_size table */
            int prevsample = 0;  /* previous sample; initial condition must be 0 */
            int previndex = 0;  /* previous index; initial condition must be 0 */

            /* decode ADPCM-A value */
            short ADPCMA_Decode(byte code)
            {
                adpcm_accum += jedi_table[adpcm_decstep + code];

                /* extend 12-bit signed int */
                if ((adpcm_accum & ~0x7FF) != 0)
                {       /* accum > 2047 */
                    adpcm_accum |= ~0xFFF;
                }
                else
                {
                    adpcm_accum &= 0xFFF;
                }

                adpcm_decstep += step_adj[code & 7] * 16;

                /* perform limit and return */
                if (adpcm_decstep < 0)
                {
                    adpcm_decstep = 0;
                }
                if (adpcm_decstep > (48 * 16))
                {
                    adpcm_decstep = 48 * 16;
                }

                return (short)adpcm_accum;
            }

            /* encode value for ADPCM-A */
            byte ADPCMA_Encode(short sample)
            {
                short tempstep;
                byte code;

                predictsample = prevsample;
                index = previndex;
                step = (short)step_size[index];

                diff = (short)(sample - predictsample);

                if (diff >= 0)
                {
                    code = 0;
                }
                else
                {
                    code = 8;
                    diff = (short)-diff;
                }

                tempstep = step;
                if (diff >= tempstep)
                {
                    code |= 4;
                    diff -= tempstep;
                }

                tempstep >>= 1;
                if (diff >= tempstep)
                {
                    code |= 2;
                    diff -= tempstep;
                }

                tempstep >>= 1;
                if (diff >= tempstep) { code |= 1; }

                predictsample = ADPCMA_Decode(code);

                index += step_adj[code];

                if (index < 0) { index = 0; }
                if (index > 48) { index = 48; }

                prevsample = predictsample;
                previndex = index;

                return code;
            }

            List<byte> ret = new List<byte>();
            /* convert data to 12 bits; similar to MVSTracker code
             */
            for (int i = 0; i < data.Length; i++)
            {
                /* downscale to 12 bits */
                data[i] >>= 4;
            }
            /* Generate ADPCM */
            for (int i = 0; i < data.Length; i += 2)
            {
                byte val = 0;
                if (i + 1 >= data.Length)
                    val = (byte)((ADPCMA_Encode(data[i]) << 4) | ADPCMA_Encode(0));
                else
                    val = (byte)((ADPCMA_Encode(data[i]) << 4) | ADPCMA_Encode(data[i + 1]));
                ret.Add(val);
                if (ret.Count == 0xFFFFF)   // Max size 1MB
                    break;
            }

            return ret.ToArray();
        }

    }
}
