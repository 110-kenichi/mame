using FM_SoundConvertor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using File = System.IO.File;

namespace zanac.MAmidiMEmo.Util.Syx
{
    public static class SyxReaderTX81Z
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Tone[] Read(string fileName)
        {
            return LoadSyx(0, fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Tone[] LoadSyx(int offset, string fileName)
        {
            List<Tone> tones = new List<Tone>();

            int[] opidtbl = { 0, 2, 1, 3 };
            var dat = System.IO.File.ReadAllBytes(fileName);
            if (dat.Length != 4104)
                return tones.ToArray();
            int fileidx = 0;
            //check VMEM header
            if (dat[fileidx++] != 0xf0)
                return tones.ToArray();
            if (dat[fileidx++] != 0x43)
                return tones.ToArray();
            fileidx++;
            if (dat[fileidx++] != 0x04)
                return tones.ToArray();
            if (dat[fileidx++] != 0x20)
                return tones.ToArray();
            if (dat[fileidx++] != 0x00)
                return tones.ToArray();

            for (int ti = 0; ti < 32; ti++)
            {
                FM_SoundConvertor.Tone tone = new FM_SoundConvertor.Tone();
                tone.Number = ti;

                //Tone
                for (int opi = 0; opi < 4; opi++)
                {
                    int opidx = opidtbl[opi];
                    tone.aOp[opidx].AR = dat[fileidx++] & 31;
                    tone.aOp[opidx].DR = dat[fileidx++] & 31;
                    tone.aOp[opidx].SR = dat[fileidx++] & 31;
                    tone.aOp[opidx].RR = dat[fileidx++] & 15;
                    tone.aOp[opidx].SL = 15 - (dat[fileidx++] & 15);    //TODO:
                    fileidx++;  //KEYBOARD LEVEL SCALING (0-99)
                    tone.aOp[opidx].AM = (dat[fileidx++] & 0x40) == 0x40 ? 1 : 0;   //EBS, KVS
                    tone.aOp[opidx].TL = calcRealValue(127, (100 - dat[fileidx++]), 127);
                    {
                        tone.aOp[opidx].ML = (dat[fileidx] >> 2) & 15;
                        tone.aOp[opidx].DT2 = (dat[fileidx++]) & 3;
                    }
                    {
                        tone.aOp[opidx].KS = (dat[fileidx] >> 3) & 3;
                        tone.aOp[opidx].DT = dat[fileidx++] & 15;
                    }
                }
                {
                    tone.SY = (dat[fileidx] >> 6) & 1; tone.SY2 = tone.SY;
                    tone.FB = (dat[fileidx] >> 3) & 7;
                    tone.AL = dat[fileidx++] & 7;
                }
                tone.LFRQ = calcRealValue(255, dat[fileidx++], 100); tone.LFRQ2 = tone.LFRQ;
                fileidx++;  //LFO DELAY
                tone.LFOD = calcRealValue(127, dat[fileidx++], 100); tone.LFOF = 1;
                tone.LFOD2 = calcRealValue(127, dat[fileidx++], 100); tone.LFOF2 = 0;
                {
                    tone.PMS = (dat[fileidx] >> 4) & 7;
                    tone.AMS = (dat[fileidx] >> 2) & 3;
                    tone.LFOW = dat[fileidx++] & 3; tone.LFOW2 = tone.LFOW;
                    tone.PMSF = 0;
                    tone.AMSF = 1;
                }
                tone.KeyShift = dat[fileidx++] - 24;
                fileidx++;  //Pitch Bend Range
                fileidx++;  //CH
                fileidx++;  //PORT
                fileidx++;  //FC VOL
                fileidx++;  //MW PITCH
                fileidx++;  //MW AMPLI
                fileidx++;  //BC PITCH
                fileidx++;  //BC AMPLI
                fileidx++;  //BC P BIAS
                fileidx++;  //BC E BIAS
                StringBuilder name = new StringBuilder(new String(new char[] { (char)dat[fileidx++] }));
                name.Append((char)dat[fileidx++]);
                name.Append((char)dat[fileidx++]);
                name.Append((char)dat[fileidx++]);
                name.Append((char)dat[fileidx++]);
                name.Append((char)dat[fileidx++]);
                name.Append((char)dat[fileidx++]);
                name.Append((char)dat[fileidx++]);
                name.Append((char)dat[fileidx++]);
                name.Append((char)dat[fileidx++]);
                tone.Name = name.ToString();
                fileidx++;  //PEG PR1
                fileidx++;  //PEG PR2
                fileidx++;  //PEG PR3
                fileidx++;  //PEG PL1
                fileidx++;  //PEG PL2
                fileidx++;  //PEG PL3
                for (int opi = 0; opi < 4; opi++)
                {
                    int opidx = opidtbl[opi];
                    {
                        var egst = (dat[fileidx] >> 4) & 2;
                        if (egst != 0)
                        {
                            tone.aOp[opidx].EGSF = 1;
                            tone.aOp[opidx].DT2 = egst;
                        }
                        tone.aOp[0].FIX = (dat[fileidx] >> 3) & 1;
                        if (tone.aOp[opidx].FIX != 0)
                            tone.aOp[opidx].DT = dat[fileidx] & 7;
                        fileidx++;
                    }
                    {
                        var osw = (dat[fileidx] >> 4) & 7;
                        if (osw != 0)
                        {
                            tone.aOp[opidx].OSCF = 1;
                            tone.aOp[opidx].DT = osw;
                            tone.aOp[opidx].ML = dat[fileidx] & 15;
                        }
                        fileidx++;
                    }
                }
                {
                    var rev = dat[fileidx++] & 7;
                    for (int opi = 0; opi < 4; opi++)
                    {
                        int opidx = opidtbl[opi];
                        if (tone.aOp[opidx].OSCF != 0)
                            tone.aOp[opidx].SR = rev;
                    }
                }
                fileidx++; //FOOT CONTROL PITCH RANGE (0-99)
                fileidx++; //FOOT CONTROL AMPLITUDE RANGE (0-99)
                fileidx += 44;

                tones.Add(tone);
            }

            return tones.ToArray();
        }

        private static int calcRealValue(int max, int value, int baseValue)
        {
            return (int)Math.Round((double)max * ((double)value / (double)baseValue));
        }


    }
}
