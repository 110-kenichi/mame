


using System;

using static FM_SoundConvertor.Path;
using static FM_SoundConvertor.File;



namespace FM_SoundConvertor
{
	class Fmp
	{
		enum eState
		{
			Entry,
			Op0,
			Op1,
			Op2,
			Op3,
			Header,
		}



		static void GetOp(string[] aTok, ref Op @Op, int nTok)
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

			switch (nTok)
			{
				case 9:
					{
						Op.DT2 = 0;
						Op.AM = 0;
						break;
					}
				case 10:
					{
						Op.DT2 = 0;
						int.TryParse(aTok[9], out Op.AM);
						break;
					}
				case 11:
					{
						int.TryParse(aTok[9], out Op.DT2);
						int.TryParse(aTok[10], out Op.AM);
						break;
					}
			}
		}



		static string Put(int Value)
		{
			return String.Format("{0,3},", Value);
		}

		static string Put(Op @Op)
		{
			return Put(Op.AR) + Put(Op.DR) + Put(Op.SR) + Put(Op.RR) + Put(Op.SL) + Put(Op.TL) + Put(Op.KS) + Put(Op.ML) + Put(Op.DT) + Put(Op.AM).TrimEnd(',');
		}

		public static void Put(Tone @Tone, ref string Buffer)
		{
			if (Tone.IsValid())
			{
				var Name = String.Format("({0})", Tone.Name) + "\n";
				var Header = "'@ " + String.Format("FA {0}", Tone.Number) + "\n";
				var Op0 = "'@ " + Put(Tone.aOp[0]) + "\n";
				var Op1 = "'@ " + Put(Tone.aOp[1]) + "\n";
				var Op2 = "'@ " + Put(Tone.aOp[2]) + "\n";
				var Op3 = "'@ " + Put(Tone.aOp[3]) + "\n";
				var Footer = "'@ " + Put(Tone.AL) + Put(Tone.FB).TrimEnd(',') + "\n";
				Buffer += Name + Header + Op0 + Op1 + Op2 + Op3 + Footer + "\n";
			}
		}



		public static Tone Reader(string Path, Option @Option)
		{
			var vTone = new Tone();
			int nTok = 0;

			var BufferMuc = "";
			var BufferDat = Dat.New();
			var BufferPmd = "";
			var BufferVopm = Vopm.New();

			var State = eState.Entry;
			var aLine = ReadLine(Path);
			foreach (var Line in aLine)
			{
				if (String.IsNullOrWhiteSpace(Line)) continue;
				if (Line[0] != '\'') continue;

				var bPartCommnet = false;
				var aChar = Line.ToCharArray();
				var oChar = 0;
				foreach (var Char in aChar)
				{
					var bPart = (Char == ';');
					if (bPartCommnet) aChar[oChar] = ' ';
					if (bPart) bPartCommnet = !bPartCommnet;
					if (bPartCommnet) aChar[oChar] = ' ';
					++oChar;
				}
				var Text = new string(aChar);

				switch (State)
				{
					case eState.Entry:
						{
							var bHead = Text.StartsWith("'@");
							if (bHead)
							{
								nTok = 9;
								var oSub = 0;
								Text = Text.Substring(2).Trim();
								if (Text.StartsWith("F")) { nTok = 9; oSub = 1; }
								if (Text.StartsWith("FA")) { nTok = 10; oSub = 2; }
								if (Text.StartsWith("FC")) { nTok = 11; oSub = 2; }
								Text = Text.Substring(oSub);

								var aTok = Text.Split(new char[] { ' ', '\t', }, StringSplitOptions.RemoveEmptyEntries);
								if (aTok.Length >= 1)
								{
									int.TryParse(aTok[0], out vTone.Number);
									State = eState.Op0;
									break;
								}
							}
							break;
						}
					case eState.Op0:
						{
							var bHead = Text.StartsWith("'@");
							if (bHead)
							{
								Text = Text.Substring(2);
								var aTok = Text.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
								if (aTok.Length >= nTok)
								{
									GetOp(aTok, ref vTone.aOp[0], nTok);
									State = eState.Op1;
									break;
								}
							}
							State = eState.Entry;
							break;
						}
					case eState.Op1:
						{
							var bHead = Text.StartsWith("'@");
							if (bHead)
							{
								Text = Text.Substring(2);
								var aTok = Text.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
								if (aTok.Length >= nTok)
								{
									GetOp(aTok, ref vTone.aOp[1], nTok);
									State = eState.Op2;
									break;
								}
							}
							State = eState.Entry;
							break;
						}
					case eState.Op2:
						{
							var bHead = Text.StartsWith("'@");
							if (bHead)
							{
								Text = Text.Substring(2);
								var aTok = Text.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
								if (aTok.Length >= nTok)
								{
									GetOp(aTok, ref vTone.aOp[2], nTok);
									State = eState.Op3;
									break;
								}
							}
							State = eState.Entry;
							break;
						}
					case eState.Op3:
						{
							var bHead = Text.StartsWith("'@");
							if (bHead)
							{
								Text = Text.Substring(2);
								var aTok = Text.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
								if (aTok.Length >= nTok)
								{
									GetOp(aTok, ref vTone.aOp[3], nTok);
									State = eState.Header;
									break;
								}
							}
							State = eState.Entry;
							break;
						}
					case eState.Header:
						{
							var bHead = Text.StartsWith("'@");
							if (bHead)
							{
								Text = Text.Substring(2);
								var aTok = Text.Split(new char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
								if (aTok.Length >= 2)
								{
									int.TryParse(aTok[0], out vTone.AL);
									int.TryParse(aTok[1], out vTone.FB);
									vTone.Name = "";

									if (Option.bMuc) Muc.Put(vTone, ref BufferMuc);
									if (Option.bDat) Dat.Put(vTone, ref BufferDat);
									if (Option.bPmd) Pmd.Put(vTone, ref BufferPmd);
									if (Option.bVopm) Vopm.Put(vTone, ref BufferVopm);
								}
							}
							State = eState.Entry;
							break;
						}
				}
			}

			if (Option.bMuc) Muc.Writer(Path, BufferMuc);
			if (Option.bDat) Dat.Writer(Path, BufferDat);
			if (Option.bPmd) Pmd.Writer(Path, BufferPmd);
			if (Option.bVopm) Vopm.Writer(Path, BufferVopm);

            return vTone;
		}



		public static Tone Reader(string[] aPath, Option @Option)
		{
			foreach (var Path in aPath)
			{
				if (!String.IsNullOrWhiteSpace(Path) && Extension(Path) == ".mwi")
                    return Reader(Path, Option);
			}

            return null;
		}



		public static void Writer(string Path, string Buffer)
		{
			WriteText(ChangeExtension(Path, ".mwi"), Buffer);
		}
	}
}

