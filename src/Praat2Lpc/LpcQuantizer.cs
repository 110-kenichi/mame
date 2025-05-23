using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praat2Lpc
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="unvoicedLpcCoefficientCount"></param>
    /// <param name="voicedLpcCoefficientCount"></param>
    /// <param name="energyBitCount"></param>
    /// <param name="pitchBitCount"></param>
    /// <param name="lpcCoefficientBitCounts"></param>
    /// <param name="energyTable"></param>
    /// <param name="pitchTable"></param>
    /// <param name="lpcCoefficientTable"></param>
    /// <param name="chirpTable"></param>
    /// <param name="interpolationTable"></param>
    /// <param name="vlmChip"></param>
    public class LpcQuantizer(int unvoicedLpcCoefficientCount, int voicedLpcCoefficientCount, int energyBitCount, int pitchBitCount,
     int[] lpcCoefficientBitCounts, int[] energyTable, int[] pitchTable, int[][] lpcCoefficientTable, byte[] chirpTable, int[] interpolationTable, Boolean vlmChip)
    {
        private static readonly int[] TI_K_BIT_COUNTS = [5, 5, 4, 4, 4, 4, 4, 3, 3, 3];
        private static readonly int[] VLM_K_BIT_COUNTS = [6, 5, 4, 4, 3, 3, 3, 3, 3, 3];
        private static readonly int[] TI_028X_LATER_ENERGY = [0, 1, 2, 3, 4, 6, 8, 11, 16, 23, 33, 47, 63, 85, 114, 0];
        private static readonly int[] VLM_ENERGY = [0, 1, 2, 3, 5, 6, 7, 9, 11, 13, 15, 17, 19, 22, 24, 27, 31, 34, 38, 42, 47, 51, 57, 62, 68, 75, 82, 89, 98, 107, 116, 127];
        private static readonly int[] TI_5220_PITCH = [0, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 44, 46, 48, 50, 52, 53, 56, 58, 60, 62, 65, 68, 70, 72, 76, 78, 80, 84, 86, 91, 94, 98, 101, 105, 109, 114, 118, 122, 127, 132, 137, 142, 148, 153, 159];
        private static readonly int[] VLM_PITCH = [0, 21, 22, 23, 24, 25, 26, 27, 28, 29, 31, 33, 35, 37, 39, 41, 43, 45, 49, 53, 57, 61, 65, 69, 73, 77, 85, 93, 101, 109, 117, 125];
        private static readonly int[][] TI_5110_5220_LPC = [
            [ -501, -498, -497, -495, -493, -491, -488, -482, -478, -474, -469, -464, -459, -452, -445, -437, -412, -380, -339, -288, -227, -158, -81, -1, 80, 157, 226, 287, 337, 379, 411, 436 ],
            [ -328, -303, -274, -244, -211, -175, -138, -99, -59, -18, 24, 64, 105, 143, 180, 215, 248, 278, 306, 331, 354, 374, 392, 408, 422, 435, 445, 455, 463, 470, 476, 506 ],
            [ -441, -387, -333, -279, -225, -171, -117, -63, -9, 45, 98, 152, 206, 260, 314, 368 ],
            [ -328, -273, -217, -161, -106, -50, 5, 61, 116, 172, 228, 283, 339, 394, 450, 506 ],
            [ -328, -282, -235, -189, -142, -96, -50, -3, 43, 90, 136, 182, 229, 275, 322, 368 ],
            [ -256, -212, -168, -123, -79, -35, 10, 54, 98, 143, 187, 232, 276, 320, 365, 409 ],
            [ -308, -260, -212, -164, -117, -69, -21, 27, 75, 122, 170, 218, 266, 314, 361, 409 ],
            [ -256, -161, -66, 29, 124, 219, 314, 409 ],
            [ -256, -176, -96, -15, 65, 146, 226, 307 ],
            [ -205, -132, -59, 14, 87, 160, 234, 307 ] ];
        private static readonly int[][] VLM_LPC = [
            [ 390, 403, 414, 425, 434, 443, 450, 457, 463, 469, 474, 478, 482, 485, 488, 491, 494, 496, 498, 499, 501, 502, 503, 504, 505, 506, 507, 507, 508, 508, 509, 509, -390,-376,-360,-344,-325,-305,-284,-261, -237,-211,-183,-155,-125, -95, -64, -32, 0, 32, 64, 95, 125, 155, 183, 211, 237, 261, 284, 305, 325, 344, 360, 376],
            [0,  50, 100, 149, 196, 241, 284, 325, 362, 396, 426, 452, 473, 490, 502, 510,
                -512,-510,-502,-490,-473,-452,-426,-396, // Entry 16(0x10) = 0 either has some special function, purpose unknown, or is a manufacturing error and should have been -512
          -362,-325,-284,-241,-196,-149,-100, -50],
         [0, 64, 128, 192, 256, 320, 384, 448, -512,-448,-384,-320,-256,-192,-128, -64],
         [0, 64, 128, 192, 256, 320, 384, 448, -512,-448,-384,-320,-256,-192,-128, -64],
         [0, 128, 256, 384,-512,-384,-256,-128],
         [0, 128, 256, 384,-512,-384,-256,-128],
         [0, 128, 256, 384,-512,-384,-256,-128],
         [0, 128, 256, 384,-512,-384,-256,-128],
         [0, 128, 256, 384,-512,-384,-256,-128],
         [0, 128, 256, 384,-512,-384,-256,-128]];

        private static readonly byte[] TI_LATER_CHIRP = [0, 3, 15, 40, 76, 108, 113, 80, 37, 38, 76, 68, 26, 50, 59, 19, 55, 26, 37, 31, 29, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        private static readonly byte[] VLM_CHIRP = [0, 127, 127, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        private static readonly int[] TI_INTERPOLATION = [0, 3, 3, 3, 2, 2, 1, 1];
        private static readonly int[] VLM_INTERPOLATION = [3, 3, 3, 2, 2, 1, 1, 0];

        private static LpcQuantizer? _TMS5220;

        public static LpcQuantizer TMS5220
        {
            get
            {
                _TMS5220 ??= new LpcQuantizer(4, 10, 4, 6, TI_K_BIT_COUNTS, TI_028X_LATER_ENERGY, TI_5220_PITCH, TI_5110_5220_LPC, TI_LATER_CHIRP, TI_INTERPOLATION, false);
                return _TMS5220;
            }
        }

        private static LpcQuantizer? _VLM5030;

        public static LpcQuantizer VLM5030
        {
            get
            {
                _VLM5030 ??= new LpcQuantizer(10, 10, 5, 5, VLM_K_BIT_COUNTS, VLM_ENERGY, VLM_PITCH, VLM_LPC,
           VLM_CHIRP, VLM_INTERPOLATION, true);
                return _VLM5030;
            }
        }

        public int unvoicedLpcCoefficientCount = unvoicedLpcCoefficientCount;
        public int voicedLpcCoefficientCount = voicedLpcCoefficientCount;
        public int energyBitCount = energyBitCount;
        public int pitchBitCount = pitchBitCount;
        public int[] lpcCoefficientBitCounts = lpcCoefficientBitCounts;
        public int[] energyTable = energyTable;
        public int[] pitchTable = pitchTable;
        public int[][] lpcCoefficientTable = lpcCoefficientTable;
        public byte[] chirpTable = chirpTable;
        public int[] interpolationTable = interpolationTable;
        public Boolean vlmChip = vlmChip;

        public static int SilenceEnergy => 0;

        public static int MinEncodedEnergy => 1;

        public int MaxEncodedEnergy => (1 << energyBitCount) - 2;

        public int StopEnergy => (1 << energyBitCount) - 1;

        public int EncodeEnergy(double energy)
        {
            if (vlmChip)
                return GetNearestValueIndex(energyTable, 0, energyTable.Length - 1, GetIntEnergy(energy), false);
            else
                return GetNearestValueIndex(energyTable, 0, energyTable.Length - 2, GetIntEnergy(energy), false);
        }

        public static int MinEncodedPitch => 1;

        public int MaxEncodedPitch => (1 << this.pitchBitCount) - 1;

        public int EncodePitch(double var1)
        {
            return EncodePitch(GetPitch(var1));
        }

        public int EncodePitch(int pitch)
        {
            return GetNearestValueIndex(pitchTable, 1, pitchTable.Length - 1, pitch, false);
        }

        public int[] DecodeLpcCoefficients(long lpcEncodedCoefficients, Boolean voiced)
        {
            int count = voiced ? voicedLpcCoefficientCount : unvoicedLpcCoefficientCount;
            int[] coeffs = new int[count];

            if (vlmChip)
            {
                for (int idx = count - 1; idx >= 0; --idx)
                {
                    int var7 = lpcCoefficientBitCounts[idx];
                    coeffs[idx] = (int)(lpcEncodedCoefficients & (long)((2 << var7 - 1) - 1));
                    lpcEncodedCoefficients >>>= var7;
                }
            }
            else
            {
                for (int idx = 0; idx < count; ++idx)
                {
                    int var7 = lpcCoefficientBitCounts[idx];
                    coeffs[idx] = (int)(lpcEncodedCoefficients & (long)((2 << var7 - 1) - 1));
                    lpcEncodedCoefficients >>>= var7;
                }
            }
            return coeffs;
        }

        public long EncodeLpcCoefficients(int[] coeffs, Boolean voiced)
        {
            long k = 0L;
            int count = voiced ? voicedLpcCoefficientCount : unvoicedLpcCoefficientCount;

            if (vlmChip)
            {
                for (int idx = 0; idx < count; ++idx)
                {
                    k <<= lpcCoefficientBitCounts[idx];
                    k |= (long)coeffs[idx];
                }
            }
            else
            {
                for (int idx = 0; idx < count; ++idx)
                {
                    k <<= lpcCoefficientBitCounts[idx];
                    k |= (long)coeffs[idx];
                }
            }
            return k;
        }

        public long EncodeLpcCoefficients(double[] coeffs, Boolean voiced)
        {
            long k = 0L;
            int count = voiced ? this.voicedLpcCoefficientCount : this.unvoicedLpcCoefficientCount;

            if (vlmChip)
            {
                for (int idx = 0; idx < count; ++idx)
                {
                    int var7 = idx < coeffs.Length ? GetIntLpcCoefficient(coeffs[idx]) : 0;
                    k <<= lpcCoefficientBitCounts[idx];
                    long tk = GetNearestValueIndex(this.lpcCoefficientTable[idx], var7, true);
                    k |= tk;
                    //System.out.println("K" + idx + ": " + tk);
                }
                //System.out.println(String.format("K=%x", k));
            }
            else
            {
                for (int idx = 0; idx < count; ++idx)
                {
                    int var7 = idx < coeffs.Length ? GetIntLpcCoefficient(coeffs[idx]) : 0;
                    k <<= lpcCoefficientBitCounts[idx];
                    k |= (long)GetNearestValueIndex(this.lpcCoefficientTable[idx], var7, false);
                }
            }

            return k;
        }

        private static int GetIntEnergy(double energy)
        {
            return (int)(energy * 128.0D);
        }

        public static int GetPitch(double pitch)
        {
            return (int)Math.Round(8000.0D / pitch - 1.0D);
        }

        public static double GetFrequency(int frequency)
        {
            return 8000.0D / (double)(frequency + 1);
        }

        private static int GetIntLpcCoefficient(double lpcCoefficient)
        {
            return (int)Math.Round(512.0D * lpcCoefficient);
        }

        private static double GetDoubleLpcCoefficient(int lpcCoefficient)
        {
            return (double)lpcCoefficient / 512.0D;
        }

        private static int GetNearestValueIndex(int[] coeffs, int val, Boolean center)
        {
            return GetNearestValueIndex(coeffs, 0, coeffs.Length - 1, val, center);
        }

        private static int GetNearestValueIndex(int[] coeffs, int start, int end, int val, Boolean center)
        {
            int idx;
            if (center)
            {
                for (int i = start; i < (end / 2); ++i)
                {
                    if (coeffs[i] <= val && val < coeffs[i + 1])
                    {
                        return val - coeffs[i] > coeffs[i + 1] - val ? i : i + 1;
                    }
                }
                if (coeffs[end / 2] <= val)
                {
                    return end / 2;
                }

                for (int i = end / 2 + 1; i < end; ++i)
                {
                    if (coeffs[i] <= val && val < coeffs[i + 1])
                    {
                        return val - coeffs[i] > coeffs[i + 1] - val ? i : i + 1;
                    }
                }
                if (coeffs[end] <= val)
                {
                    return end;
                }
                return 0;
            }
            else
            {
                while (start + 1 < end)
                {
                    idx = (start + end) / 2;
                    if (coeffs[idx] < val)
                    {
                        start = idx;
                    }
                    else
                    {
                        end = idx;
                    }
                }
                idx = 2 * val < coeffs[start] + coeffs[end] ? start : end;
                return idx;
            }
        }
    }
}
