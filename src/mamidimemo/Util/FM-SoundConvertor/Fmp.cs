


using System;

using static FM_SoundConvertor.Path;
using static FM_SoundConvertor.File;
using System.Collections.Generic;

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

		public static IEnumerable<Tone> Reader(string Path)
		{
            List<Tone> tones = new List<Tone>();

            var vTone = new Tone();
			int nTok = 0;

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

                                    if (vTone.IsValid())
                                        tones.Add(new Tone(vTone));
								}
							}
							State = eState.Entry;
							break;
						}
				}
			}

            return tones;
		}
	}
}

