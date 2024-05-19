


using System;
using System.Linq;

using static FM_SoundConvertor.Path;
using static FM_SoundConvertor.File;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//http://5inch.floppy.jp/hoot/MUSICLALF_MMLMEMO.TXT
//https://raw.githubusercontent.com/kuma4649/mml2vgm/master/mml2vgm_MMLCommandMemo.txt

namespace FM_SoundConvertor
{
    class Gwi
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
            FM,
            OPN,
            OPM,
            OPLL,
            OPL3_2,
            OPL3_4,
        }



        static void GetOp(string[] aTok, ref Op @Op, eType Type)
        {
            switch (Type)
            {
                case eType.FM:
                    {
                        int.TryParse(aTok[1], out Op.AR);
                        int.TryParse(aTok[2], out Op.DR);
                        int.TryParse(aTok[3], out Op.SR);
                        int.TryParse(aTok[4], out Op.RR);
                        int.TryParse(aTok[5], out Op.SL);
                        int.TryParse(aTok[6], out Op.TL);
                        int.TryParse(aTok[7], out Op.KS);
                        int.TryParse(aTok[8], out Op.ML);
                        int.TryParse(aTok[9], out Op.DT);
                        break;
                    }
                case eType.OPN:
                    {
                        int.TryParse(aTok[1], out Op.AR);
                        int.TryParse(aTok[2], out Op.DR);
                        int.TryParse(aTok[3], out Op.SR);
                        int.TryParse(aTok[4], out Op.RR);
                        int.TryParse(aTok[5], out Op.SL);
                        int.TryParse(aTok[6], out Op.TL);
                        int.TryParse(aTok[7], out Op.KS);
                        int.TryParse(aTok[8], out Op.ML);
                        int.TryParse(aTok[9], out Op.DT);
                        int.TryParse(aTok[10], out Op.AM);
                        int.TryParse(aTok[11], out Op.SSG);
                        break;
                    }
                case eType.OPM:
                    {
                        int.TryParse(aTok[1], out Op.AR);
                        int.TryParse(aTok[2], out Op.DR);
                        int.TryParse(aTok[3], out Op.SR);
                        int.TryParse(aTok[4], out Op.RR);
                        int.TryParse(aTok[5], out Op.SL);
                        int.TryParse(aTok[6], out Op.TL);
                        int.TryParse(aTok[7], out Op.KS);
                        int.TryParse(aTok[8], out Op.ML);
                        int.TryParse(aTok[9], out Op.DT);
                        int.TryParse(aTok[10], out Op.DT2);
                        int.TryParse(aTok[11], out Op.AM);
                        break;
                    }
                case eType.OPLL:
                    {
                        int.TryParse(aTok[1], out Op.AR);
                        int.TryParse(aTok[2], out Op.DR);
                        int.TryParse(aTok[3], out Op.SL);
                        int.TryParse(aTok[4], out Op.RR);
                        int.TryParse(aTok[5], out Op.KS);
                        int.TryParse(aTok[6], out Op.ML);
                        int.TryParse(aTok[7], out Op.AM);
                        int.TryParse(aTok[8], out Op.VIB);
                        int.TryParse(aTok[9], out Op.EG);
                        int.TryParse(aTok[10], out Op.KSR);
                        int.TryParse(aTok[11], out Op.DT);
                        Op.SR = -2;
                        Op.AR *= 2;
                        Op.DR *= 2;
                        break;
                    }
                case eType.OPL3_2:
                case eType.OPL3_4:
                    {
                        int.TryParse(aTok[1], out Op.AR);
                        int.TryParse(aTok[2], out Op.DR);
                        int.TryParse(aTok[3], out Op.SL);
                        int.TryParse(aTok[4], out Op.RR);
                        int.TryParse(aTok[5], out Op.KS);
                        int.TryParse(aTok[6], out Op.TL);
                        int.TryParse(aTok[7], out Op.ML);
                        int.TryParse(aTok[8], out Op.AM);
                        int.TryParse(aTok[9], out Op.VIB);
                        int.TryParse(aTok[10], out Op.EG);
                        int.TryParse(aTok[11], out Op.KSR);
                        int.TryParse(aTok[12], out Op.WS);
                        Op.SR = -2;
                        Op.AR *= 2;
                        Op.DR *= 2;
                        Op.TL *= 2;
                        break;
                    }
            }
        }

        private static Regex headerSplit = new Regex("(?:^|,|\\s)(\"(?:[^\"]+|\"\")*\"|[^,\\s]*)", RegexOptions.Compiled);

        public static IEnumerable<Tone> Reader(string Path)
        {
            List<Tone> tones = new List<Tone>();

            var vTone = new Tone();
            var Type = eType.FM;
            var nTok = 0;

            var State = eState.Entry;
            var aLine = ReadLine(Path);
            foreach (var Line in aLine)
            {
                if (String.IsNullOrWhiteSpace(Line)) continue;
                if (Line[0] != '\'') continue;

                var oMark = Line.IndexOf(';');
                var Text = (oMark >= 0) ? Line.Remove(oMark) : Line;

                switch (State)
                {
                    case eState.Entry:
                        {
                            Text = Text.Trim();

                            var bHead = Text.StartsWith("'@ ");
                            if (bHead)
                            {
                                List<string> list = new List<string>();
                                foreach (Match match in headerSplit.Matches(Text))
                                {
                                    var curr = match.Value;
                                    if (curr.Length != 0)
                                        list.Add(curr.TrimStart(',').Trim());
                                }
                                var aTok = list.ToArray();

                                if (aTok.Length >= 3)
                                {
                                    switch (aTok[1])
                                    {
                                        case "F":
                                            Type = eType.FM;
                                            break;
                                        case "N":
                                            Type = eType.OPN;
                                            break;
                                        case "M":
                                            Type = eType.OPM;
                                            break;
                                        case "LL":
                                            Type = eType.OPLL;
                                            break;
                                        case "L":
                                            Type = eType.OPL3_2;
                                            break;
                                        case "L4":
                                            Type = eType.OPL3_4;
                                            break;
                                    }

                                    //No
                                    int Number;
                                    if (int.TryParse(aTok[2], out Number)) vTone.Number = Number;

                                    //Name
                                    if (aTok.Length >= 4)
                                        vTone.Name = aTok[3].Trim('"');
                                    else
                                        vTone.Name = null;
                                }
                                State = eState.Header;
                                break;
                            }
                            break;
                        }
                    case eState.Header:
                        {
                            var aTok = Text.Split(new char[] { ' ', '\t', ',', }, StringSplitOptions.RemoveEmptyEntries);
                            if (aTok.Length >= nTok)
                            {
                                GetOp(aTok, ref vTone.aOp[0], Type);
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
                                GetOp(aTok, ref vTone.aOp[1], Type);
                                if (Type == eType.OPLL || Type == eType.OPL3_2)
                                    State = eState.Op3;
                                else
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
                                GetOp(aTok, ref vTone.aOp[2], Type);
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
                                GetOp(aTok, ref vTone.aOp[3], Type);
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
                                case eType.FM:
                                case eType.OPN:
                                case eType.OPM:
                                    {
                                        if (aTok.Length >= 3)
                                        {
                                            int.TryParse(aTok[1], out vTone.AL);
                                            int.TryParse(aTok[2], out vTone.FB);

                                            if (vTone.IsValid())
                                                tones.Add(new Tone(vTone));
                                        }
                                        break;
                                    }
                                case eType.OPLL:
                                    {
                                        if (aTok.Length >= 3)
                                        {
                                            int.TryParse(aTok[1], out vTone.aOp[0].TL);
                                            vTone.aOp[0].TL *= 2;
                                            int.TryParse(aTok[2], out vTone.FB);

                                            if (vTone.IsValid2Op())
                                                tones.Add(new Tone(vTone));
                                        }
                                        break;
                                    }
                                case eType.OPL3_2:
                                    {
                                        if (aTok.Length >= 3)
                                        {
                                            int.TryParse(aTok[1], out vTone.CNT);
                                            int.TryParse(aTok[2], out vTone.FB);

                                            if (vTone.IsValid2Op())
                                                tones.Add(new Tone(vTone));
                                        }
                                        break;
                                    }
                                case eType.OPL3_4:
                                    {
                                        if (aTok.Length >= 3)
                                        {
                                            int cnt1, cnt2;
                                            int.TryParse(aTok[1], out cnt1);
                                            int.TryParse(aTok[2], out cnt2);
                                            vTone.CNT = ((cnt2 << 1) | cnt1) + 2;
                                            int.TryParse(aTok[3], out vTone.FB);

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

            return tones;
        }
    }
}
