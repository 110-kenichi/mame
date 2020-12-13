


using System;

using static FM_SoundConvertor.Path;
using static FM_SoundConvertor.File;
using System.Collections.Generic;

namespace FM_SoundConvertor
{
	class Muc
	{
		enum eState
		{
			Entry,
			Header,
			Op0,
			Op1,
			Op2,
			Op3,
		}

		enum eType
		{
			Mucom,
			Mmldrv,
		}



		static void GetOp(string[] aTok, ref Op @Op, eType Type)
		{
			switch (Type)
			{
				case eType.Mucom:
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
						Op.AM = 0;
						break;
					}
				case eType.Mmldrv:
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
			return Put(Op.AR) + Put(Op.DR) + Put(Op.SR) + Put(Op.RR) + Put(Op.SL) + Put(Op.TL) + Put(Op.KS) + Put(Op.ML) + Put(Op.DT).TrimEnd(',');
		}

		public static void Put(Tone @Tone, ref string Buffer)
		{
			if (Tone.IsValid())
			{
				var Entry = "  " + String.Format("@{0}:{{", Tone.Number) + "\n";
				var Header = " " + Put(Tone.FB) + Put(Tone.AL).TrimEnd(',') + "\n";
				var Op0 = " " + Put(Tone.aOp[0]) + "\n";
				var Op1 = " " + Put(Tone.aOp[1]) + "\n";
				var Op2 = " " + Put(Tone.aOp[2]) + "\n";
				var Op3 = " " + Put(Tone.aOp[3]) + String.Format(",\"{0}\"}}", Tone.Name) + "\n";
				Buffer += Entry + Header + Op0 + Op1 + Op2 + Op3 + "\n";
			}
		}



		static IEnumerable<Tone> Reader(string Path, Option @Option)
		{
            List<Tone> tones = new List<Tone>();

            var vTone = new Tone();
			var Type = eType.Mucom;
			var nTok = 0;

			var BufferDat = Dat.New();
			var BufferFmp = "";
			var BufferPmd = "";
			var BufferVopm = Vopm.New();

			var State = eState.Entry;
			var aLine = ReadLine(Path);
			foreach (var Line in aLine)
			{
				if (String.IsNullOrWhiteSpace(Line)) continue;
				if (Line[0] == '\'') continue;

				var oMark = Line.IndexOf(';');
				var Text = (oMark >= 0)? Line.Remove(oMark): Line;

				switch (State)
				{
					case eState.Entry:
						{
							Text = Text.Trim();

							var bHead = Text.StartsWith("@");
							var bTail = Text.EndsWith(":{");
							if (bHead)
							{
								vTone.Number++;

								Type = (bTail) ? eType.Mucom : eType.Mmldrv;
								nTok = (bTail) ? 9 : 11;

								var aTok = Text.Split(new char[] { ' ', '\t', '@', ':', '{', }, StringSplitOptions.RemoveEmptyEntries);
								if (aTok.Length >= 1)
								{
									int Number;
									if (int.TryParse(aTok[0], out Number)) vTone.Number = Number;
								}
								State = eState.Header;
								break;
							}
							break;
						}
					case eState.Header:
						{
							var aTok = Text.Split(new char[] { ' ', '\t', ',', }, StringSplitOptions.RemoveEmptyEntries);
							if (aTok.Length >= 2)
							{
								switch (Type)
								{
									case eType.Mucom:
										{
											int.TryParse(aTok[0], out vTone.FB);
											int.TryParse(aTok[1], out vTone.AL);
											break;
										}
									case eType.Mmldrv:
										{
											int.TryParse(aTok[0], out vTone.AL);
											int.TryParse(aTok[1], out vTone.FB);
											break;
										}
								}
								State = eState.Op0;
								break;
							}
							State = eState.Entry;
							break;
						}
					case eState.Op0:
						{
							var aTok = Text.Split(new char[] { ' ', '\t', ',', }, StringSplitOptions.RemoveEmptyEntries);
							if (aTok.Length >= nTok)
							{
								GetOp(aTok, ref vTone.aOp[0], Type);
								State = eState.Op1;
								break;
							}
							State = eState.Entry;
							break;
						}
					case eState.Op1:
						{
							var aTok = Text.Split(new char[] { ' ', '\t', ',', }, StringSplitOptions.RemoveEmptyEntries);
							if (aTok.Length >= nTok)
							{
								GetOp(aTok, ref vTone.aOp[1], Type);
								State = eState.Op2;
								break;
							}
							State = eState.Entry;
							break;
						}
					case eState.Op2:
						{
							var aTok = Text.Split(new char[] { ' ', '\t', ',', }, StringSplitOptions.RemoveEmptyEntries);
							if (aTok.Length >= nTok)
							{
								GetOp(aTok, ref vTone.aOp[2], Type);
								State = eState.Op3;
								break;
							}
							State = eState.Entry;
							break;
						}
					case eState.Op3:
						{
							var aTok = Text.Split(new char[] { ' ', '\t', ',', }, StringSplitOptions.RemoveEmptyEntries);
							switch (Type)
							{
								case eType.Mucom:
									{
										if (aTok.Length >= (nTok+1))
										{
											GetOp(aTok, ref vTone.aOp[3], Type);

											var Tok = "";
											for (int o = nTok; o < aTok.Length; ++o) Tok += aTok[o];

											var oHead = Tok.IndexOf('\"');
											var oTail = Tok.IndexOf('\"', oHead+1);
											var bTerm = Tok.EndsWith("}");
											if (oHead >= 0 && oTail >= 0 && bTerm)
											{
												vTone.Name = Tok.Substring(oHead+1, oTail-oHead-1);

												if (Option.bDat) Dat.Put(vTone, ref BufferDat);
												if (Option.bFmp) Fmp.Put(vTone, ref BufferFmp);
												if (Option.bPmd) Pmd.Put(vTone, ref BufferPmd);
												if (Option.bVopm) Vopm.Put(vTone, ref BufferVopm);

                                                if (vTone.IsValid())
                                                    tones.Add(new Tone(vTone));
                                            }
										}
										break;
									}
								case eType.Mmldrv:
									{
										if (aTok.Length >= nTok)
										{
											GetOp(aTok, ref vTone.aOp[3], Type);

											vTone.Name = "";

											if (Option.bDat) Dat.Put(vTone, ref BufferDat);
											if (Option.bFmp) Fmp.Put(vTone, ref BufferFmp);
											if (Option.bPmd) Pmd.Put(vTone, ref BufferPmd);
											if (Option.bVopm) Vopm.Put(vTone, ref BufferVopm);

                                            if (vTone.IsValid())
                                                tones.Add(new Tone(vTone));
                                        }
                                        break;
									}
							}
							State = eState.Entry;
							break;
						}
				}
			}

			if (Option.bDat) Dat.Writer(Path, BufferDat);
			if (Option.bFmp) Fmp.Writer(Path, BufferFmp);
			if (Option.bPmd) Pmd.Writer(Path, BufferPmd);
			if (Option.bVopm) Vopm.Writer(Path, BufferVopm);

            return tones;
        }



        public static IEnumerable<Tone> Reader(string[] aPath, Option @Option)
		{
			foreach (var Path in aPath)
			{
				if (!String.IsNullOrWhiteSpace(Path) && Extension(Path) == ".muc")
                    return Reader(Path, Option);
			}

            return null;
		}



		public static void Writer(string Path, string Buffer)
		{
			WriteText(ChangeExtension(Path, ".muc"), Buffer);
		}
	}
}
