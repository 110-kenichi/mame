


using System;

using static FM_SoundConvertor.Path;
using static FM_SoundConvertor.File;



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

			var aChar = new char[]
			{
				(char)Buffer[o + (int)ePut.Name0],
				(char)Buffer[o + (int)ePut.Name1],
				(char)Buffer[o + (int)ePut.Name2],
				(char)Buffer[o + (int)ePut.Name3],
				(char)Buffer[o + (int)ePut.Name4],
				(char)Buffer[o + (int)ePut.Name5],
			};
			Tone.Name = "";
			foreach (var Char in aChar) if (Char != 0) Tone.Name += Char.ToString();

			Tone.Number = i;
		}



		public static void Put(Tone @Tone, ref byte[] Buffer)
		{
			if (Tone.IsValid())
			{
				int o = Tone.Number * PutLength();
				Buffer[o + (int)ePut.Void] = 0;
				Buffer[o + (int)ePut.DTML0] = (byte)((Tone.aOp[0].DT << 4) | Tone.aOp[0].ML);
				Buffer[o + (int)ePut.DTML1] = (byte)((Tone.aOp[1].DT << 4) | Tone.aOp[1].ML);
				Buffer[o + (int)ePut.DTML2] = (byte)((Tone.aOp[2].DT << 4) | Tone.aOp[2].ML);
				Buffer[o + (int)ePut.DTML3] = (byte)((Tone.aOp[3].DT << 4) | Tone.aOp[3].ML);
				Buffer[o + (int)ePut.TL0] = (byte)Tone.aOp[0].TL;
				Buffer[o + (int)ePut.TL1] = (byte)Tone.aOp[1].TL;
				Buffer[o + (int)ePut.TL2] = (byte)Tone.aOp[2].TL;
				Buffer[o + (int)ePut.TL3] = (byte)Tone.aOp[3].TL;
				Buffer[o + (int)ePut.KSAR0] = (byte)((Tone.aOp[0].KS << 6) | Tone.aOp[0].AR);
				Buffer[o + (int)ePut.KSAR1] = (byte)((Tone.aOp[1].KS << 6) | Tone.aOp[1].AR);
				Buffer[o + (int)ePut.KSAR2] = (byte)((Tone.aOp[2].KS << 6) | Tone.aOp[2].AR);
				Buffer[o + (int)ePut.KSAR3] = (byte)((Tone.aOp[3].KS << 6) | Tone.aOp[3].AR);
				Buffer[o + (int)ePut.DR0] = (byte)Tone.aOp[0].DR;
				Buffer[o + (int)ePut.DR1] = (byte)Tone.aOp[1].DR;
				Buffer[o + (int)ePut.DR2] = (byte)Tone.aOp[2].DR;
				Buffer[o + (int)ePut.DR3] = (byte)Tone.aOp[3].DR;
				Buffer[o + (int)ePut.SR0] = (byte)Tone.aOp[0].SR;
				Buffer[o + (int)ePut.SR1] = (byte)Tone.aOp[1].SR;
				Buffer[o + (int)ePut.SR2] = (byte)Tone.aOp[2].SR;
				Buffer[o + (int)ePut.SR3] = (byte)Tone.aOp[3].SR;
				Buffer[o + (int)ePut.SLRR0] = (byte)((Tone.aOp[0].SL << 4) | Tone.aOp[0].RR);
				Buffer[o + (int)ePut.SLRR1] = (byte)((Tone.aOp[1].SL << 4) | Tone.aOp[1].RR);
				Buffer[o + (int)ePut.SLRR2] = (byte)((Tone.aOp[2].SL << 4) | Tone.aOp[2].RR);
				Buffer[o + (int)ePut.SLRR3] = (byte)((Tone.aOp[3].SL << 4) | Tone.aOp[3].RR);
				Buffer[o + (int)ePut.FBAL] = (byte)((Tone.FB << 3) | Tone.AL);
				Buffer[o + (int)ePut.Name0] = (byte)((Tone.Name.Length > 0) ? Tone.Name[0] : 0);
				Buffer[o + (int)ePut.Name1] = (byte)((Tone.Name.Length > 1) ? Tone.Name[1] : 0);
				Buffer[o + (int)ePut.Name2] = (byte)((Tone.Name.Length > 2) ? Tone.Name[2] : 0);
				Buffer[o + (int)ePut.Name3] = (byte)((Tone.Name.Length > 3) ? Tone.Name[3] : 0);
				Buffer[o + (int)ePut.Name4] = (byte)((Tone.Name.Length > 4) ? Tone.Name[4] : 0);
				Buffer[o + (int)ePut.Name5] = (byte)((Tone.Name.Length > 5) ? Tone.Name[5] : 0);
			}
		}



		public static Tone Reader(string Path, Option @Option)
		{
			var Buffer = ReadByte(Path);
			if (Buffer.Length == DatLength())
			{
				Tone vTone = new Tone();

				var BufferMuc = "";
				var BufferFmp = "";
				var BufferPmd = "";
				var BufferVopm = Vopm.New();

				for (int i = 0; i < ToneLength(); )//++i)
				{
					Get(ref vTone, Buffer, i);

					if (Option.bMuc) Muc.Put(vTone, ref BufferMuc);
					if (Option.bFmp) Fmp.Put(vTone, ref BufferFmp);
					if (Option.bPmd) Pmd.Put(vTone, ref BufferPmd);
					if (Option.bVopm) Vopm.Put(vTone, ref BufferVopm);

                    return vTone;
				}

				if (Option.bMuc) Muc.Writer(Path, BufferMuc);
				if (Option.bFmp) Fmp.Writer(Path, BufferFmp);
				if (Option.bPmd) Pmd.Writer(Path, BufferPmd);
				if (Option.bVopm) Vopm.Writer(Path, BufferVopm);
			}

            return null;
		}



		public static Tone Reader(string[] aPath, Option @Option)
		{
			foreach (var Path in aPath)
			{
				if (!String.IsNullOrWhiteSpace(Path) && Extension(Path) == ".dat")
                    return Reader(Path, Option);
			}

            return null;
		}



		public static void Writer(string Path, byte[] Buffer)
		{
			WriteByte(ChangeExtension(Path, ".dat"), Buffer);
		}
	}
}
