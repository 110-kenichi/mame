


using System;

using static FM_SoundConvertor.Path;
using static FM_SoundConvertor.File;
using System.Collections.Generic;
using System.Text;

namespace FM_SoundConvertor
{
	class Dat
	{
		enum ePut
		{
			Void,
			DTML0, DTML2, DTML1, DTML3,
			TL0, TL2, TL1, TL3,
			KSAR0, KSAR2, KSAR1, KSAR3,
			DR0, DR2, DR1, DR3,
			SR0, SR2, SR1, SR3,
			SLRR0, SLRR2, SLRR1, SLRR3,
			FBAL,
			Name0, Name1, Name2, Name3, Name4, Name5,
		}



		static int ToneLength()
		{
			return 0x100;
		}

		static int PutLength()
		{
			return System.Enum.GetNames(typeof(ePut)).Length;
		}

		static int DatLength()
		{
			return ToneLength() * PutLength();
		}

		public static byte[] New()
		{
			return new byte[DatLength()];
		}

		static void Get(ref Tone @Tone, byte[] Buffer, int i)
		{
			var o = i * PutLength();
			Tone.aOp[0].AR = Buffer[o + (int)ePut.KSAR0] & 0x1f;
			Tone.aOp[1].AR = Buffer[o + (int)ePut.KSAR1] & 0x1f;
			Tone.aOp[2].AR = Buffer[o + (int)ePut.KSAR2] & 0x1f;
			Tone.aOp[3].AR = Buffer[o + (int)ePut.KSAR3] & 0x1f;
			Tone.aOp[0].DR = Buffer[o + (int)ePut.DR0];
			Tone.aOp[1].DR = Buffer[o + (int)ePut.DR1];
			Tone.aOp[2].DR = Buffer[o + (int)ePut.DR2];
			Tone.aOp[3].DR = Buffer[o + (int)ePut.DR3];
			Tone.aOp[0].SR = Buffer[o + (int)ePut.SR0];
			Tone.aOp[1].SR = Buffer[o + (int)ePut.SR1];
			Tone.aOp[2].SR = Buffer[o + (int)ePut.SR2];
			Tone.aOp[3].SR = Buffer[o + (int)ePut.SR3];
			Tone.aOp[0].RR = Buffer[o + (int)ePut.SLRR0] & 0xf;
			Tone.aOp[1].RR = Buffer[o + (int)ePut.SLRR1] & 0xf;
			Tone.aOp[2].RR = Buffer[o + (int)ePut.SLRR2] & 0xf;
			Tone.aOp[3].RR = Buffer[o + (int)ePut.SLRR3] & 0xf;
			Tone.aOp[0].SL = Buffer[o + (int)ePut.SLRR0] >> 4;
			Tone.aOp[1].SL = Buffer[o + (int)ePut.SLRR1] >> 4;
			Tone.aOp[2].SL = Buffer[o + (int)ePut.SLRR2] >> 4;
			Tone.aOp[3].SL = Buffer[o + (int)ePut.SLRR3] >> 4;
			Tone.aOp[0].TL = Buffer[o + (int)ePut.TL0];
			Tone.aOp[1].TL = Buffer[o + (int)ePut.TL1];
			Tone.aOp[2].TL = Buffer[o + (int)ePut.TL2];
			Tone.aOp[3].TL = Buffer[o + (int)ePut.TL3];
			Tone.aOp[0].KS = Buffer[o + (int)ePut.KSAR0] >> 6;
			Tone.aOp[1].KS = Buffer[o + (int)ePut.KSAR1] >> 6;
			Tone.aOp[2].KS = Buffer[o + (int)ePut.KSAR2] >> 6;
			Tone.aOp[3].KS = Buffer[o + (int)ePut.KSAR3] >> 6;
			Tone.aOp[0].ML = Buffer[o + (int)ePut.DTML0] & 0xf;
			Tone.aOp[1].ML = Buffer[o + (int)ePut.DTML1] & 0xf;
			Tone.aOp[2].ML = Buffer[o + (int)ePut.DTML2] & 0xf;
			Tone.aOp[3].ML = Buffer[o + (int)ePut.DTML3] & 0xf;
			Tone.aOp[0].DT = Buffer[o + (int)ePut.DTML0] >> 4;
			Tone.aOp[1].DT = Buffer[o + (int)ePut.DTML1] >> 4;
			Tone.aOp[2].DT = Buffer[o + (int)ePut.DTML2] >> 4;
			Tone.aOp[3].DT = Buffer[o + (int)ePut.DTML3] >> 4;
			Tone.aOp[0].DT2 = 0;
			Tone.aOp[1].DT2 = 0;
			Tone.aOp[2].DT2 = 0;
			Tone.aOp[3].DT2 = 0;
			Tone.aOp[0].AM = 0;
			Tone.aOp[1].AM = 0;
			Tone.aOp[2].AM = 0;
			Tone.aOp[3].AM = 0;
			Tone.AL = Buffer[o + (int)ePut.FBAL] & 0x7;
			Tone.FB = Buffer[o + (int)ePut.FBAL] >> 3;

			var aChar = new byte[]
			{
				Buffer[o + (int)ePut.Name0],
				Buffer[o + (int)ePut.Name1],
				Buffer[o + (int)ePut.Name2],
				Buffer[o + (int)ePut.Name3],
				Buffer[o + (int)ePut.Name4],
				Buffer[o + (int)ePut.Name5],
			};
            
            Tone.Name = Encoding.GetEncoding("Shift_JIS").GetString(aChar);

			Tone.Number = i;
		}

		public static IEnumerable<Tone> Reader(string Path)
		{
            List<Tone> tones = new List<Tone>();

			var Buffer = ReadByte(Path);
			if (Buffer.Length == DatLength())
			{
				Tone vTone = new Tone();

				for (int i = 0; i < ToneLength(); ++i)
				{
					Get(ref vTone, Buffer, i);

                    //if (vTone.IsValid() &&
                    //    i != 0) //skip index 0 tone is dummy
                    if (vTone.IsValid())
						tones.Add(new Tone(vTone));
                }
			}

            return tones;
		}
	}
}
