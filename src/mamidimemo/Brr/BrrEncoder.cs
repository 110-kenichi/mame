using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Brr
{
    public class BrrEncoder
    {
        // Global variables for prediction of filter
        static short p1, p2;
        // Buffer for a single BRR data block (9 bytes)
        static byte[] BRR;

        const int FIR_ORDER = 15;

        static byte filter_at_loop = 0;
        static int p1_at_loop, p2_at_loop;
        static bool[] FIRen = { true, true, true, true };  // Which BRR filters are enabled
        static uint[] FIRstats = { 0, 0, 0, 0 };   // Statistincs on BRR filter usage
        static bool wrap_en = true;

        static double sinc(double x)
        {
            if (x == 0.0)
                return 1.0;
            else
                return Math.Sin(Math.PI * x) / (Math.PI * x);
        }


        /// <summary>
        /// Convert a block from PCM to BRR
        /// Returns the squared error between original data and encoded data
        /// If "is_end_point" is true, the predictions p1/p2 at loop are also used in caluclating the error (depending on filter at loop)
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        static short CLAMP_16(int n)
        {
            return ((short)n != n) ?
                ((short)(0x7fff - (n >> 24)))
                :
                (short)n;
        }


        static int get_brr_prediction(byte filter, short p1, short p2)
        {
            int p;
            switch (filter)                         //Different formulas for 4 filters
            {
                case 0:
                    return 0;

                case 1:
                    p = p1;
                    p -= p1 >> 4;
                    return p;

                case 2:
                    p = p1 << 1;
                    p += (-(p1 + (p1 << 1))) >> 5;
                    p -= p2;
                    p += p2 >> 4;
                    return p;

                case 3:
                    p = p1 << 1;
                    p += (-(p1 + (p1 << 2) + (p1 << 3))) >> 6;
                    p -= p2;
                    p += (p2 + (p2 << 1)) >> 4;
                    return p;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shiftamount"></param>
        /// <param name="filter"></param>
        /// <param name="PCM_data"></param>
        /// <param name="write"></param>
        /// <param name="is_end_point"></param>
        /// <returns></returns>
        static double ADPCMMash(int shiftamount, byte filter, short[] PCM_data, bool write, bool is_end_point)
        {

            double d2 = 0.0;
            short l1 = p1;
            short l2 = p2;
            int step = 1 << shiftamount;

            int vlin = 0, d, da, dp, c;

            for (int i = 0; i < 16; ++i)
            {
                /* make linear prediction for next int */
                /*      vlin = (v0 * iCoef[0] + v1 * iCoef[1]) >> 8; */
                vlin = get_brr_prediction(filter, l1, l2) >> 1;
                d = (PCM_data[i] >> 1) - vlin;      /* difference between linear prediction and current int */
                da = Math.Abs(d);
                if (wrap_en && da > 16384 && da < 32768)
                {
                    /* Take advantage of wrapping */
                    d = d - 32768 * (d >> 24);
                    //if (write)
                    //    printf("Caution : Wrapping was used.\n");
                }
                dp = d + (step << 2) + (step >> 2);
                c = 0;
                if (dp > 0)
                {
                    if (step > 1)
                        c = dp / (step / 2);
                    else
                        c = dp * 2;
                    if (c > 15)
                        c = 15;
                }
                c -= 8;
                dp = (c << shiftamount) >> 1;       /* quantized estimate of samp - vlin */
                                                    /* edge case, if caller even wants to use it */
                if (shiftamount > 12)
                    dp = (dp >> 14) & ~0x7FF;
                c &= 0x0f;      /* mask to 4 bits */

                l2 = l1;            /* shift history */
                l1 = (short)(CLAMP_16(vlin + dp) * 2);

                d = PCM_data[i] - l1;
                d2 += (double)d * d;        /* update square-error */

                if (write)                  /* if we want output, put it in proper place */
                    BRR[1 + (i >> 1)] |= (i & 1) == 1 ? (byte)c : (byte)(c << 4);
            }

            if (is_end_point)
                switch (filter_at_loop)
                {   /* Also account for history points when looping is enabled & filters used */
                    case 0:
                        d2 /= 16d;
                        break;

                    /* Filter 1 */
                    case 1:
                        d = l1 - p1_at_loop;
                        d2 += (double)d * d;
                        d2 /= 17d;
                        break;

                    /* Filters 2 & 3 */
                    default:
                        d = l1 - p1_at_loop;
                        d2 += (double)d * d;
                        d = l2 - p2_at_loop;
                        d2 += (double)d * d;
                        d2 /= 18d;
                        break;
                }
            else
                d2 /= 16d;

            if (write)
            {   /* when generating real output, we want to return these */
                p1 = (short)l1;
                p2 = (short)l2;

                BRR[0] = (byte)((shiftamount << 4) | (filter << 2));
                if (is_end_point)
                    BRR[0] |= 1;                        //Set the end bit if we're on the last block
            }
            return d2;
        }

        // Encode a ADPCM block using brute force over filters and shift amounts
        static void ADPCMBlockMash(short[] PCM_data, bool is_loop_point, bool is_end_point)
        {
            int smin = 0, kmin = 0;
            double dmin = double.PositiveInfinity;
            for (int s = 0; s < 13; ++s)
                for (int k = 0; k < 4; ++k)
                    if (FIRen[k])
                    {
                        double d = ADPCMMash(s, (byte)k, PCM_data, false, is_end_point);
                        if (d < dmin)
                        {
                            kmin = k;       //Memorize the filter, shift values with smaller error
                            dmin = d;
                            smin = s;
                        }
                    }

            if (is_loop_point)
            {
                filter_at_loop = (byte)kmin;
                p1_at_loop = p1;
                p2_at_loop = p2;
            }
            ADPCMMash(smin, (byte)kmin, PCM_data, true, is_end_point);
            FIRstats[kmin]++;
        }

        static int[] resample(int[] samples, int samples_length, int out_length, char type)
        {
            double ratio = (double)samples_length / (double)out_length;
            int[] outb = new int[out_length];

            //printf("Resampling by effective ratio of %f...\n", ratio);

            switch (type)
            {
                case 'n':                               //No interpolation
                    for (int i = 0; i < out_length; ++i)
                    {
                        outb[i] = samples[(int)Math.Floor(i * ratio)];
                    }
                    break;
                case 'l':                               //Linear interpolation
                    for (int i = 0; i < out_length; ++i)
                    {
                        int a = (int)(i * ratio);       //Whole part of index
                        double b = i * ratio - a;       //Fractional part of index
                        if ((a + 1) == samples_length)
                            outb[i] = samples[a];   //This used only for the last int
                        else
                            outb[i] = (int)Math.Round((1 - b) * samples[a] + b * samples[a + 1]);
                    }
                    break;
                case 's':                               //Sine interpolation
                    for (int i = 0; i < out_length; ++i)
                    {
                        int a = (int)(i * ratio);
                        double b = i * ratio - a;
                        double c = (1.0 - Math.Cos(b * Math.PI)) / 2.0;
                        if ((a + 1) == samples_length)
                            outb[i] = samples[a];   //This used only for the last int
                        else
                            outb[i] = (int)Math.Round((1 - c) * samples[a] + c * samples[a + 1]);
                    }
                    break;
                case 'c':                                       //Cubic interpolation
                    for (int i = 0; i < out_length; ++i)
                    {
                        int a = (int)(i * ratio);

                        short s0 = (a == 0) ? (short)samples[0] : (short)samples[a - 1];
                        short s1 = (short)samples[a];
                        short s2 = (a + 1 >= samples_length) ? (short)samples[samples_length - 1] : (short)samples[a + 1];
                        short s3 = (a + 2 >= samples_length) ? (short)samples[samples_length - 1] : (short)samples[a + 2];

                        double a0 = s3 - s2 - s0 + s1;
                        double a1 = s0 - s1 - a0;
                        double a2 = s2 - s0;
                        double b = i * ratio - a;
                        double b2 = b * b;
                        double b3 = b2 * b;
                        outb[i] = (int)Math.Round(b3 * a0 + b2 * a1 + b * a2 + s1);
                    }
                    break;
                case 'b':                                   // Bandlimited interpolation
                                                            // Antialisaing pre-filtering
                    if (ratio > 1.0)
                    {
                        int[] samples_antialiased = new int[samples_length];

                        double[] fir_coefs = new double[FIR_ORDER + 1];

                        // Compute FIR coefficients
                        for (int k = 0; k <= FIR_ORDER; ++k)
                            fir_coefs[k] = sinc(k / ratio) / ratio;

                        // Apply FIR filter to samples
                        for (int i = 0; i < samples_length; ++i)
                        {
                            double acc = samples[i] * fir_coefs[0];
                            for (int k = FIR_ORDER; k > 0; --k)
                            {
                                acc += fir_coefs[k] * ((i + k < samples_length) ? samples[i + k] : samples[samples_length - 1]);
                                acc += fir_coefs[k] * ((i - k >= 0) ? samples[i - k] : samples[0]);
                            }
                            samples_antialiased[i] = (int)acc;
                        }

                        samples = samples_antialiased;
                    }
                    // Actual resampling using sinc interpolation
                    for (int i = 0; i < out_length; ++i)
                    {
                        double a = i * ratio;
                        double acc = 0.0;
                        for (int j = (int)a - FIR_ORDER; j <= (int)a + FIR_ORDER; ++j)
                        {
                            int intv;
                            if (j >= 0)
                                if (j < samples_length)

                                    intv = samples[j];
                                else
                                    intv = samples[samples_length - 1];
                            else
                                intv = samples[0];

                            acc += intv * sinc(a - j);
                        }
                        outb[i] = (int)acc;
                    }
                    break;

                default:
                    //fprintf(stderr, "\nError : A valid interpolation algorithm must be chosen !\n");
                    //print_instructions();
                    break;
            }
            // No longer need the non-resampled version of the int
            return outb;
        }

        // This function applies a treble boosting filter that compensates the gauss lowpass filter
        static short[] treble_boost_filter(short[] samples, int length)
        {   // Tepples' coefficient multiplied by 0.6 to avoid overflow in most cases
            double[] coefs = { 0.912962, -0.16199, -0.0153283, 0.0426783, -0.0372004, 0.023436, -0.0105816, 0.00250474 };

            short[] outb = new short[length];
            for (int i = 0; i < length; ++i)
            {
                double acc = samples[i] * coefs[0];
                for (int k = 7; k > 0; --k)
                {
                    acc += coefs[k] * ((i + k < length) ? samples[i + k] : samples[length - 1]);
                    acc += coefs[k] * ((i - k >= 0) ? samples[i - k] : samples[0]);
                }
                outb[i] = (short)Math.Round(acc);
            }
            return outb;
        }

        public static byte[] ConvertRawWave(short[] samples, bool treble_boost, bool loop_flag, uint loop_start, out uint brr_loop_start)
        {
            // loopsize is the multiple of 16 that comes after loopsize
            loop_start = (loop_start / 16) * 16;

            // Apply trebble boost filter (gussian lowpass compensation) if requested by user
            if (treble_boost)
                samples = treble_boost_filter(samples, samples.Length);

            List<byte> returnData = new List<byte>();

            bool initial_block = false;
            for (int i = 0; i < 16; ++i)                    //Initialization needed if any of the first 16 samples isn't zero
                initial_block |= samples[i] != 0;
            if (initial_block)
            {
                byte[] iblock = { (loop_flag ? (byte)0x02 : (byte)0x00), 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                returnData.AddRange(iblock);
            }

            p1 = 0;
            p2 = 0;
            brr_loop_start = 0;
            for (uint n = 0; n < samples.Length; n += 16)
            {
                BRR = new byte[9];

                short[] sliced_sample = new short[16];
                Array.Copy(samples, n, sliced_sample, 0, 16);

                //Encode BRR block, tell the encoder if we're at loop point (if loop is enabled), and if we're at end point
                ADPCMBlockMash(sliced_sample, loop_flag && (n == loop_start), n == samples.Length - 16);

                if (loop_flag && (n == loop_start))
                    brr_loop_start = (uint)(returnData.Count);

                //Set the loop flag if needed
                BRR[0] |= (loop_flag ? (byte)0x02 : (byte)0x00);

                returnData.AddRange(BRR);
            }

            return returnData.ToArray();
        }
    }
}
