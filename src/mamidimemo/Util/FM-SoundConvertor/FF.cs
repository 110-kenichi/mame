using System;

using static FM_SoundConvertor.Path;
using static FM_SoundConvertor.File;
using System.Collections.Generic;
using System.Text;
using System.Security.Policy;

namespace FM_SoundConvertor
{
	class FF
	{
        //https://github.com/osoumen/ff2gtp/blob/main/ff2gtp.cpp

        static void Get(ref Tone @Tone, byte[] Buffer, int i)
        {
            var o = i * 32;
            Tone.aOp[0].DT = Buffer[o] >> 4;
            Tone.aOp[0].ML = Buffer[o++] & 0xf;
            Tone.aOp[2].DT = Buffer[o] >> 4;
            Tone.aOp[2].ML = Buffer[o++] & 0xf;
            Tone.aOp[1].DT = Buffer[o] >> 4;
            Tone.aOp[1].ML = Buffer[o++] & 0xf;
            Tone.aOp[3].DT = Buffer[o] >> 4;
            Tone.aOp[3].ML = Buffer[o++] & 0xf;

            Tone.aOp[0].TL = Buffer[o++];
            Tone.aOp[2].TL = Buffer[o++];
            Tone.aOp[1].TL = Buffer[o++];
            Tone.aOp[3].TL = Buffer[o++];

            Tone.aOp[0].KS = Buffer[o] >> 6;
            Tone.aOp[0].AR = Buffer[o++] & 0x1f;
            Tone.aOp[2].KS = Buffer[o] >> 6;
            Tone.aOp[2].AR = Buffer[o++] & 0x1f;
            Tone.aOp[1].KS = Buffer[o] >> 6;
            Tone.aOp[1].AR = Buffer[o++] & 0x1f;
            Tone.aOp[3].DT = Buffer[o] >> 6;
            Tone.aOp[3].AR = Buffer[o++] & 0x1f;

            Tone.aOp[0].AM = Buffer[o] >> 7;
            Tone.aOp[0].DR = Buffer[o++] & 0x1f;
            Tone.aOp[2].AM = Buffer[o] >> 7;
            Tone.aOp[2].DR = Buffer[o++] & 0x1f;
            Tone.aOp[1].AM = Buffer[o] >> 7;
            Tone.aOp[1].DR = Buffer[o++] & 0x1f;
            Tone.aOp[3].AM = Buffer[o] >> 7;
            Tone.aOp[3].DR = Buffer[o++] & 0x1f;

            Tone.aOp[0].DT2 = Buffer[o] >> 6;
            Tone.aOp[0].SR = Buffer[o++] & 0x1f;
            Tone.aOp[2].DT2 = Buffer[o] >> 6;
            Tone.aOp[2].SR = Buffer[o++] & 0x1f;
            Tone.aOp[1].DT2 = Buffer[o] >> 6;
            Tone.aOp[1].SR = Buffer[o++] & 0x1f;
            Tone.aOp[3].DT2 = Buffer[o] >> 6;
            Tone.aOp[3].SR = Buffer[o++] & 0x1f;

            Tone.aOp[0].SL = Buffer[o] >> 4;
            Tone.aOp[0].RR = Buffer[o++] & 0xf;
            Tone.aOp[2].SL = Buffer[o] >> 4;
            Tone.aOp[2].RR = Buffer[o++] & 0xf;
            Tone.aOp[1].SL = Buffer[o] >> 4;
            Tone.aOp[1].RR = Buffer[o++] & 0xf;
            Tone.aOp[3].SL = Buffer[o] >> 4;
            Tone.aOp[3].RR = Buffer[o++] & 0xf;

            Tone.FB = Buffer[o] >> 3;
            Tone.AL = Buffer[o++] & 7;

            var aChar = new byte[]
            {
                Buffer[o++],
                Buffer[o++],
                Buffer[o++],
                Buffer[o++],
                Buffer[o++],
                Buffer[o++],
                Buffer[o++],
            };

            Tone.Name = Encoding.GetEncoding("Shift_JIS").GetString(aChar);

            Tone.Number = i;
        }


        public static IEnumerable<Tone> Reader(string Path, Option @Option)
		{
            List<Tone> tones = new List<Tone>();


			var buffer = File.ReadByte(Path);
            for (int i = 0; i < File.Size(Path) / 32; ++i)
            {
                var tone = new Tone();
                Get(ref tone, buffer, i);
                tones.Add(new Tone(tone));
            }

            return tones;
		}



		public static IEnumerable<Tone> Reader(string[] aPath, Option @Option)
		{
            return null;
		}



		public static void Writer(string Path, string Buffer)
		{
		}
	}
}

