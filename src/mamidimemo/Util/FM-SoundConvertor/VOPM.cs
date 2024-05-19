


using System;

using static FM_SoundConvertor.Path;
using static FM_SoundConvertor.File;
using System.Collections.Generic;

namespace FM_SoundConvertor
{
	class Vopm
	{
		enum ePut
		{
			AL, FB,
			_0, _1, _2, _3, _4, _5, _6, _7,
			Mask,
			TL0, AR0, DR0, SL0, SR0, RR0, KS0, ML0, DT1_0, DT2_0, AM0,
			TL1, AR1, DR1, SL1, SR1, RR1, KS1, ML1, DT1_1, DT2_1, AM1,
			TL2, AR2, DR2, SL2, SR2, RR2, KS2, ML2, DT1_2, DT2_2, AM2,
			TL3, AR3, DR3, SL3, SR3, RR3, KS3, ML3, DT1_3, DT2_3, AM3,
			Name0, Name1, Name2, Name3, Name4, Name5, Name6, Name7, Name8, Name9, Name10, Name11, Name12, Name13, Name14, Name15,
		}



		static int ToneLength()
		{
			return 0x80;
		}

		static int HeadLength()
		{
			return 0xa0;
		}

		static int PutLength()
		{
			return System.Enum.GetNames(typeof(ePut)).Length;
		}

		static int FxbLength()
		{
			return HeadLength() + (ToneLength() * PutLength());
		}



		public static byte[] New()
		{
			return new byte[FxbLength()];
		}



		static void Get(ref Tone @Tone, byte[] Buffer, int i)
		{
			int o = HeadLength() + (i * PutLength());
			Tone.aOp[0].AR = Buffer[o + (int)ePut.AR0];
			Tone.aOp[1].AR = Buffer[o + (int)ePut.AR1];
			Tone.aOp[2].AR = Buffer[o + (int)ePut.AR2];
			Tone.aOp[3].AR = Buffer[o + (int)ePut.AR3];
			Tone.aOp[0].DR = Buffer[o + (int)ePut.DR0];
			Tone.aOp[1].DR = Buffer[o + (int)ePut.DR1];
			Tone.aOp[2].DR = Buffer[o + (int)ePut.DR2];
			Tone.aOp[3].DR = Buffer[o + (int)ePut.DR3];
			Tone.aOp[0].SR = Buffer[o + (int)ePut.SR0];
			Tone.aOp[1].SR = Buffer[o + (int)ePut.SR1];
			Tone.aOp[2].SR = Buffer[o + (int)ePut.SR2];
			Tone.aOp[3].SR = Buffer[o + (int)ePut.SR3];
			Tone.aOp[0].RR = Buffer[o + (int)ePut.RR0];
			Tone.aOp[1].RR = Buffer[o + (int)ePut.RR1];
			Tone.aOp[2].RR = Buffer[o + (int)ePut.RR2];
			Tone.aOp[3].RR = Buffer[o + (int)ePut.RR3];
			Tone.aOp[0].SL = Buffer[o + (int)ePut.SL0];
			Tone.aOp[1].SL = Buffer[o + (int)ePut.SL1];
			Tone.aOp[2].SL = Buffer[o + (int)ePut.SL2];
			Tone.aOp[3].SL = Buffer[o + (int)ePut.SL3];
			Tone.aOp[0].TL = Buffer[o + (int)ePut.TL0];
			Tone.aOp[1].TL = Buffer[o + (int)ePut.TL1];
			Tone.aOp[2].TL = Buffer[o + (int)ePut.TL2];
			Tone.aOp[3].TL = Buffer[o + (int)ePut.TL3];
			Tone.aOp[0].KS = Buffer[o + (int)ePut.KS0];
			Tone.aOp[1].KS = Buffer[o + (int)ePut.KS1];
			Tone.aOp[2].KS = Buffer[o + (int)ePut.KS2];
			Tone.aOp[3].KS = Buffer[o + (int)ePut.KS3];
			Tone.aOp[0].ML = Buffer[o + (int)ePut.ML0];
			Tone.aOp[1].ML = Buffer[o + (int)ePut.ML1];
			Tone.aOp[2].ML = Buffer[o + (int)ePut.ML2];
			Tone.aOp[3].ML = Buffer[o + (int)ePut.ML3];
			Tone.aOp[0].DT = Buffer[o + (int)ePut.DT1_0];
			Tone.aOp[1].DT = Buffer[o + (int)ePut.DT1_1];
			Tone.aOp[2].DT = Buffer[o + (int)ePut.DT1_2];
			Tone.aOp[3].DT = Buffer[o + (int)ePut.DT1_3];
			Tone.aOp[0].DT2 = Buffer[o + (int)ePut.DT2_0];
			Tone.aOp[1].DT2 = Buffer[o + (int)ePut.DT2_1];
			Tone.aOp[2].DT2 = Buffer[o + (int)ePut.DT2_2];
			Tone.aOp[3].DT2 = Buffer[o + (int)ePut.DT2_3];
			Tone.aOp[0].AM = Buffer[o + (int)ePut.AM0];
			Tone.aOp[1].AM = Buffer[o + (int)ePut.AM1];
			Tone.aOp[2].AM = Buffer[o + (int)ePut.AM2];
			Tone.aOp[3].AM = Buffer[o + (int)ePut.AM3];
			Tone.AL = Buffer[o + (int)ePut.AL];
			Tone.FB = Buffer[o + (int)ePut.FB];

			var aChar = new char[]
			{
				(char)Buffer[o + (int)ePut.Name0],
				(char)Buffer[o + (int)ePut.Name1],
				(char)Buffer[o + (int)ePut.Name2],
				(char)Buffer[o + (int)ePut.Name3],
				(char)Buffer[o + (int)ePut.Name4],
				(char)Buffer[o + (int)ePut.Name5],
				(char)Buffer[o + (int)ePut.Name6],
				(char)Buffer[o + (int)ePut.Name7],
				(char)Buffer[o + (int)ePut.Name8],
				(char)Buffer[o + (int)ePut.Name9],
				(char)Buffer[o + (int)ePut.Name10],
				(char)Buffer[o + (int)ePut.Name11],
				(char)Buffer[o + (int)ePut.Name12],
				(char)Buffer[o + (int)ePut.Name13],
				(char)Buffer[o + (int)ePut.Name14],
				(char)Buffer[o + (int)ePut.Name15],
			};
			Tone.Name = "";
			foreach (var Char in aChar) if (Char != 0) Tone.Name += Char.ToString();

			Tone.Number = i;
		}

		public static IEnumerable<Tone> Reader(string Path)
		{
            List<Tone> tones = new List<Tone>();

            var Buffer = ReadByte(Path);
			if (Buffer.Length == FxbLength())
			{
                Tone vTone = new Tone();

				for (int i = 0; i < ToneLength(); ++i)
				{
					Get(ref vTone, Buffer, i);

                    if (vTone.IsValid())
                        tones.Add(new Tone(vTone));
				}
			}

            return tones;
        }

	}
}
