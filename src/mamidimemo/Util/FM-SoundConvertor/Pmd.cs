


using System;

using static FM_SoundConvertor.Path;
using static FM_SoundConvertor.File;



namespace FM_SoundConvertor
{
	class Pmd
	{
		enum eState
		{
			Entry,
			Op0,
			Op1,
			Op2,
			Op3,
		}



		static void GetOp(string[] aTok, ref Op @Op)
		{
			int.TryParse(aTok[0], out Op.AR);
			int.TryParse(aTok[1], out Op.DR);
			int.TryParse(aTok[2], out Op.SR);
			int.TryParse(aTok[3], out Op.RR);
			int.TryParse(aTok[4], out Op.SL);
			int.TryParse(aTok[5], out Op.TL);
			int.TryParse(aTok[6], out Op.KS);
			int.TryParse(aTok[7], out Op.ML);
			int.TryParse(aTok[8], out Op.DT);
			Op.DT2 = 0;
			int.TryParse(aTok[9], out Op.AM);
		}



		static string Put(int Value)
		{
			return String.Format("{0,4}", Value);
		}

		static string Put(Op @Op)
		{
			return Put(Op.AR) + Put(Op.DR) + Put(Op.SR) + Put(Op.RR) + Put(Op.SL) + Put(Op.TL) + Put(Op.KS) + Put(Op.ML) + Put(Op.DT) + Put(Op.AM);
		}

		public static void Put(Tone @Tone, ref string Buffer)
		{
			if (Tone.IsValid())
			{
				var Header = String.Format("@{0}", Tone.Number) + Put(Tone.AL) + Put(Tone.FB) + String.Format(" = {0}", Tone.Name) + "\n";
				var Op0 = Put(Tone.aOp[0]) + "\n";
				var Op1 = Put(Tone.aOp[1]) + "\n";
				var Op2 = Put(Tone.aOp[2]) + "\n";
				var Op3 = Put(Tone.aOp[3]) + "\n";
				Buffer += Header + Op0 + Op1 + Op2 + Op3 + "\n";
			}
		}



		public static Tone Reader(string Path, Option @Option)
		{
			var vTone = new Tone();
			bool bLineComment = false;

			var BufferMuc = "";
			var BufferDat = Dat.New();
			var BufferFmp = "";
			var BufferVopm = Vopm.New();

			var State = eState.Entry;
			var aLine = ReadLine(Path);
			foreach (var Line in aLine)
			{
				if (String.IsNullOrWhiteSpace(Line)) continue;

				var bLine = (Line[0] == '`');
				if (bLine)
				{
					bLineComment = !bLineComment;
					continue;
				}
				if (bLineComment) continue;

				var bPartCommnet = false;
				var bTailCommnet = false;
				var aChar = Line.ToCharArray();
				var oChar = 0;
				foreach (var Char in aChar)
				{
					if (bTailCommnet) aChar[oChar] = ' ';

					var bPart = (Char == '`');
					if (bPartCommnet) aChar[oChar] = ' ';
					if (bPart) bPartCommnet = !bPartCommnet;
					if (bPartCommnet) aChar[oChar] = ' ';

					if (Char == ';') bTailCommnet = true;
					if (bTailCommnet) aChar[oChar] = ' ';
					++oChar;
				}
				var Text = new string(aChar);

				switch (State)
				{
					case eState.Entry:
						{
							if (Text[0] == ' ' || Text[0] == '\t') continue;

							var bHead = Text.StartsWith("@");
							var oName = Text.IndexOf('=');
							if (bHead)
							{
								var aTok = Text.Split(new char[] { ' ', '\t', '=', }, StringSplitOptions.RemoveEmptyEntries);
								if (aTok.Length >= 3)
								{
									aTok[0] = aTok[0].Substring(1);

									int.TryParse(aTok[0], out vTone.Number);
									int.TryParse(aTok[1], out vTone.AL);
									int.TryParse(aTok[2], out vTone.FB);
									vTone.Name = (aTok.Length >= 4 && oName > 0) ? aTok[3] : "";
									State = eState.Op0;
									break;
								}
							}
							break;
						}
					case eState.Op0:
						{
							var aTok = Text.Split(new char[] { ' ', '\t', }, StringSplitOptions.RemoveEmptyEntries);
							if (aTok.Length >= 10)
							{
								GetOp(aTok, ref vTone.aOp[0]);
								State = eState.Op1;
								break;
							}
							State = eState.Entry;
							break;
						}
					case eState.Op1:
						{
							var aTok = Text.Split(new char[] { ' ', '\t', }, StringSplitOptions.RemoveEmptyEntries);
							if (aTok.Length >= 10)
							{
								GetOp(aTok, ref vTone.aOp[1]);
								State = eState.Op2;
								break;
							}
							State = eState.Entry;
							break;
						}
					case eState.Op2:
						{
							var aTok = Text.Split(new char[] { ' ', '\t', }, StringSplitOptions.RemoveEmptyEntries);
							if (aTok.Length >= 10)
							{
								GetOp(aTok, ref vTone.aOp[2]);
								State = eState.Op3;
								break;
							}
							State = eState.Entry;
							break;
						}
					case eState.Op3:
						{
							var aTok = Text.Split(new char[] { ' ', '\t', }, StringSplitOptions.RemoveEmptyEntries);
							if (aTok.Length >= 10)
							{
								GetOp(aTok, ref vTone.aOp[3]);

								if (Option.bMuc) Muc.Put(vTone, ref BufferMuc);
								if (Option.bDat) Dat.Put(vTone, ref BufferDat);
								if (Option.bFmp) Fmp.Put(vTone, ref BufferFmp);
								if (Option.bVopm) Vopm.Put(vTone, ref BufferVopm);
							}
							State = eState.Entry;
							break;
						}
				}
			}

			if (Option.bMuc) Muc.Writer(Path, BufferMuc);
			if (Option.bDat) Dat.Writer(Path, BufferDat);
			if (Option.bFmp) Fmp.Writer(Path, BufferFmp);
			if (Option.bVopm) Vopm.Writer(Path, BufferVopm);

            return vTone;
		}



		public static Tone Reader(string[] aPath, Option @Option)
		{
			foreach (var Path in aPath)
			{
				if (!String.IsNullOrWhiteSpace(Path) && Extension(Path) == ".mml")
                    return Reader(Path, Option);
			}

            return null;
		}



		public static void Writer(string Path, string Buffer)
		{
			WriteText(ChangeExtension(Path, ".mml"), Buffer);
		}
	}
}
