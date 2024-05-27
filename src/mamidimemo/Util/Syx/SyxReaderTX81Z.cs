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
                    tone.aOp[opidx].SL = 15 - (dat[fileidx++] & 15);    //TODO: need to check
                    tone.aOp[opidx].LS = dat[fileidx++] & 127; //(0-99)
                    {
                        tone.aOp[opidx].KVS = dat[fileidx] & 0x7;
                        //TODO: tone.aOp[opidx].EBS = (dat[fileidx] >> 3) & 0x7;
                        tone.aOp[opidx].AM = (dat[fileidx++] & 0x40) == 0x40 ? 1 : 0;
                    }
                    {
                        //https://nornand.hatenablog.com/entry/2020/11/21/201911
                        tone.aOp[opidx].TL = AttenuationOfOperatorOutputLevels[dat[fileidx++] % 100];
                    }
                    {
                        int F = dat[fileidx++] & 63;
                        tone.aOp[opidx].ML = CoarseToMul[F];
                        tone.aOp[opidx].DT2 = CoarseToDt2[F];
                        tone.aOp[opidx].FIXF = F >> 2;
                    }
                    {
                        tone.aOp[opidx].KS = (dat[fileidx] >> 3) & 3;
                        tone.aOp[opidx].DT = Detune1Table[dat[fileidx++] & 7];
                    }
                }
                {
                    tone.SY = (dat[fileidx] >> 6) & 1; tone.SY2 = tone.SY;
                    tone.FB = (dat[fileidx] >> 3) & 7;
                    tone.AL = dat[fileidx++] & 7;

                    //https://nornand.hatenablog.com/entry/2020/11/21/201911
                    switch (tone.AL)
                    {
                        case 4:
                            tone.aOp[1].TL += 8;
                            tone.aOp[3].TL += 8;
                            break;
                        case 5:
                        case 6:
                            tone.aOp[1].TL += 13;
                            tone.aOp[2].TL += 13;
                            tone.aOp[3].TL += 13;
                            break;
                        case 7:
                            tone.aOp[1].TL += 16;
                            tone.aOp[1].TL += 16;
                            tone.aOp[2].TL += 16;
                            tone.aOp[3].TL += 16;
                            break;
                    }
                }
                tone.LFRQ = LfoSpeeds[dat[fileidx++]]; tone.LFRQ2 = tone.LFRQ;
                tone.LFD = dat[fileidx++]; tone.LFD2 = tone.LFD;
                tone.PMD = PMDepth[dat[fileidx++]];
                tone.AMD2 = AMDepth[dat[fileidx++]];
                {
                    tone.PMS = (dat[fileidx] >> 4) & 7; tone.PMS2 = tone.PMS;
                    tone.AMS = (dat[fileidx] >> 2) & 3; tone.AMS2 = tone.AMS;
                    tone.LFOW = dat[fileidx++] & 3; tone.LFOW2 = tone.LFOW;
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
                        tone.aOp[opidx].EGSF = (dat[fileidx] >> 4) & 3;
                        tone.aOp[0].FIX = (dat[fileidx] >> 3) & 1;
                        tone.aOp[opidx].FIXR = dat[fileidx] & 7;
                        fileidx++;
                    }
                    {
                        tone.aOp[opidx].FINE = dat[fileidx] & 15;
                        tone.aOp[opidx].OSCW = (dat[fileidx] >> 4) & 7;
                        fileidx++;
                    }
                }
                {
                    var rev = dat[fileidx++] & 7;
                    for (int opi = 0; opi < 4; opi++)
                    {
                        int opidx = opidtbl[opi];
                        tone.aOp[opidx].REV = rev;
                    }
                }
                fileidx++; //FOOT CONTROL PITCH RANGE (0-99)
                fileidx++; //FOOT CONTROL AMPLITUDE RANGE (0-99)
                fileidx += 44;

                tones.Add(tone);
            }

            return tones.ToArray();
        }

        //https://docs.google.com/spreadsheets/d/1SSmypnGQ3c4COnKj8OF9WvM3qRMZTe0R-sLnMElGPvI/edit#gid=0

        private static int[] CoarseToMul = new int[64]{
             0, 0, 0, 0, 1, 1, 1, 1,
             2, 2, 3, 2, 2, 4, 3, 3,
             5, 3, 4, 6, 4, 4, 7, 5,
             5, 8, 6, 5, 9, 6, 7,10,
             6, 7,11, 8,12, 7, 8, 9,
            13, 8,14,10, 9,15,11, 9,
            10,12,11,10,13,12,11,14,
            13,12,15,14,13,15,14,15
        };

        private static int[] CoarseToDt2 = new int[64]{
            0,1,2,3,0,1,2,3,
            0,1,0,2,3,0,1,2,
            0,3,1,0,2,3,0,1,
            2,0,1,3,0,2,1,0,
            3,2,0,1,0,3,2,1,
            0,3,0,1,2,0,1,3,
            2,1,2,3,1,2,3,1,
            2,3,1,2,3,2,3,3
        };

        private static int[] LfoSpeeds = new int[100]{
              0, 31, 74, 89,106,114,125,133,138,144,
            148,153,157,161,165,167,171,173,176,178,
            181,184,185,188,189,191,194,195,197,198,
            200,201,203,205,206,207,208,210,211,212,
            213,214,216,217,218,219,220,221,222,223,
            223,225,226,226,227,228,229,230,231,231,
            232,233,234,234,235,236,237,237,238,238,
            239,240,240,241,242,242,243,244,244,245,
            246,246,247,247,248,249,249,250,250,251,
            251,252,252,253,253,253,254,255,255,255
        };

        private static int[] PMDepth = new int[100]{
              0,  0,  2,  3,  4,  5,  7,  8,  9, 11,
             12, 13, 14, 16, 17, 18, 20, 21, 22, 23,
             25, 26, 27, 29, 30, 31, 33, 34, 35, 36,
             38, 39, 40, 42, 43, 44, 45, 47, 48, 49,
             51, 52, 53, 54, 56, 57, 58, 60, 61, 62,
             63, 65, 66, 67, 69, 70, 71, 72, 74, 75,
             76, 78, 79, 80, 82, 83, 84, 85, 87, 88,
             89, 91, 92, 93, 94, 96, 97, 98,100,101,
            102,103,105,106,107,109,110,111,112,114,
            115,116,118,119,120,121,123,124,125,127
        };


        private static int[] AMDepth = new int[100]{
             0, 0, 0, 0, 0, 1, 1, 1, 1, 2,
             2, 2, 2, 3, 3, 3, 3, 4, 4, 4,
             5, 5, 5, 6, 6, 6, 7, 7, 7, 7,
             8, 8, 8, 9, 9, 9,10,10,10,11,
            11,12,12,12,13,13,14,14,15,15,
            16,16,17,17,18,18,19,19,20,20,
            21,21,22,23,23,24,25,25,26,27,
            28,28,29,30,31,32,33,34,35,36,
            37,38,40,41,42,44,45,48,49,51,
            54,56,60,62,67,70,77,86,96,127
        };

        private static int[] AttenuationOfOperatorOutputLevels = new int[100]{
            127,122,118,114,110,107,104,102,100, 98,
             96, 94, 92, 90, 88, 86, 85, 84, 82, 81,
             79, 78, 77, 76, 75, 74, 73, 72, 71, 70,
             69, 68, 67, 66, 65, 64, 63, 62, 61, 60,
             59, 58, 57, 56, 55, 54, 53, 52, 51, 50,
             49, 48, 47, 46, 45, 44, 43, 42, 41, 40,
             39, 38, 37, 36, 35, 34, 33, 32, 31, 30,
             29, 28, 27, 26, 25, 24, 23, 22, 21, 20,
             19, 18, 17, 16, 15, 14, 13, 12, 11, 10,
              9,  8,  7,  6,  5,  4,  3,  2,  1,  0
        };

        private static int[] Detune1Table = new int[8]{
            7,  //-3
            6,  //-2
            5,  //-1
            4,  // 0
            1,  //+1
            2,  //+2
            3,  //+3
            0   // 0
            };

        private static int calcRealValue(int max, int value, int baseValue)
        {
            return (int)Math.Round((double)max * ((double)value / (double)baseValue));
        }


    }
}
