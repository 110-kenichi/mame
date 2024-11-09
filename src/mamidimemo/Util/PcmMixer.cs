using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Util
{
    internal class PcmMixer
    {
        /// <summary>
        /// 
        /// </summary>
        public static bool DisableDac
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool DacClipping
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public static double DacVolume
        {
            get;
            set;
        } = 100;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Mix(IList<short> data, bool avoidClipping)
        {
            var en = data.GetEnumerator();
            if (!en.MoveNext())
                return 0;

            double mixdata = avoidClipping ?
                (en.Current * (double)PcmMixer.DacVolume / 100d) + 32768d :
                (en.Current * (double)PcmMixer.DacVolume / 100d);

            while (en.MoveNext())
            {
                if (avoidClipping)
                {
                    double cd = (en.Current * (double)PcmMixer.DacVolume / 100d) + 32768d;
                    if (mixdata <= short.MaxValue && cd <= short.MaxValue)
                    {
                        mixdata = mixdata * cd / 32768;
                    }
                    else
                    {
                        mixdata = 2 * (mixdata + cd) - (mixdata * cd / 32768d) - 65536d;
                    }
                }
                else
                {
                    mixdata += (en.Current * (double)PcmMixer.DacVolume / 100d);
                }
            }

            if (avoidClipping)
                return mixdata - 32768d;
            else
                return mixdata;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[] Mix(IList<short[]> data, bool avoidClipping)
        {
            double[] mixdata = new double[data[0].Length];
            for (int smpl = 0; smpl < mixdata.Length; smpl++)
                mixdata[smpl] = avoidClipping ?
                    (data[0][smpl] * (double)PcmMixer.DacVolume / 100d) + 32768d :
                    (data[0][smpl] * (double)PcmMixer.DacVolume / 100d);

            for (int smpl = 0; smpl < mixdata.Length; smpl++)
            {
                for (int ch = 1; ch < data.Count; ch++)
                {
                    if (avoidClipping)
                    {
                        double cd = (data[ch][smpl] * (double)PcmMixer.DacVolume / 100d) + 32768d;
                        if (mixdata[smpl] <= short.MaxValue && cd <= short.MaxValue)
                        {
                            mixdata[smpl] = mixdata[smpl] * cd / 32768d;
                        }
                        else
                        {
                            mixdata[smpl] = 2d * (mixdata[smpl] + cd) - (mixdata[smpl] * cd / 32768d) - 65536d;
                        }
                    }
                    else
                    {
                        mixdata[smpl] += (data[ch][smpl] * (double)PcmMixer.DacVolume / 100d);
                    }
                }
            }
            if (avoidClipping)
            {
                for (int s = 0; s < mixdata.Length; s++)
                    mixdata[s] = mixdata[s] - 32768d;
            }
            return mixdata;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Mix(IEnumerable<sbyte> data, bool avoidClipping)
        {
            var en = data.GetEnumerator();
            if (!en.MoveNext())
                return 0;

            double mixdata = avoidClipping ?
                (en.Current * (double)PcmMixer.DacVolume / 100d) + 128d :
                (en.Current * (double)PcmMixer.DacVolume / 100d);

            while (en.MoveNext())
            {
                if (avoidClipping)
                {
                    double cd = (en.Current * (double)PcmMixer.DacVolume / 100d) + 128d;
                    if (mixdata <= sbyte.MaxValue && cd <= sbyte.MaxValue)
                    {
                        mixdata = mixdata * cd / 128d;
                    }
                    else
                    {
                        mixdata = 2d * (mixdata + cd) - (mixdata * cd / 128d) - 256d;
                    }
                }
                else
                {
                    mixdata += (en.Current * (double)PcmMixer.DacVolume / 100d);
                }
            }

            if (avoidClipping)
                return mixdata - 128d;
            else
                return mixdata;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[] Mix(IList<byte[]> data, bool avoidClipping)
        {
            double[] mixdata = new double[data[0].Length];
            for (int smpl = 0; smpl < mixdata.Length; smpl++)
                mixdata[smpl] = avoidClipping ? (data[0][smpl] * (double)PcmMixer.DacVolume / 100d) + 128d : data[0][smpl];

            for (int smpl = 0; smpl < mixdata.Length; smpl++)
            {
                for (int ch = 1; ch < data.Count; ch++)
                {
                    if (avoidClipping)
                    {
                        double cd = (data[ch][smpl] * (double)PcmMixer.DacVolume / 100d) + 128d;
                        if (mixdata[smpl] <= short.MaxValue && cd <= short.MaxValue)
                        {
                            mixdata[smpl] = mixdata[smpl] * cd / 128d;
                        }
                        else
                        {
                            mixdata[smpl] = 2d * (mixdata[smpl] + cd) - (mixdata[smpl] * cd / 128d) - 256d;
                        }
                    }
                    else
                    {
                        mixdata[smpl] += (data[ch][smpl] * (double)PcmMixer.DacVolume / 100d);
                    }
                }
            }
            if (avoidClipping)
            {
                for (int s = 0; s < mixdata.Length; s++)
                    mixdata[s] = mixdata[s] - 128d;
            }
            return mixdata;
        }

    }
}
