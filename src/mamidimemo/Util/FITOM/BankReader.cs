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

namespace zanac.MAmidiMEmo.Util.FITOM
{
    public static class BankReader
    {

        public static Tone[] Read(string fileName)
        {
            var table = loadIniFile(fileName);

            List<Tone> tones = new List<Tone>();

            switch (getValue(table, "Header", "Type"))
            {
                case "OPM":
                    readOpmBank(table, tones);
                    break;
                case "OPN":
                    readOpnBank(table, tones);
                    break;
                case "OPL2":
                case "OPL3":
                case "OPLL":
                    readOplBank(table, tones);
                    break;
            }

            return tones.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="tones"></param>
        private static void readOpmBank(Dictionary<string, Dictionary<string, string>> table, List<Tone> tones)
        {
            for (int progNo = 0; progNo < 128; progNo++)
            {
                Tone tone = new Tone();

                tone.Number = progNo;

                var val = getValue(table, "Prog" + progNo, "Name");
                if (!string.IsNullOrWhiteSpace(val))
                    tone.Name = val;

                val = getValue(table, "Prog" + progNo, "ALFB");
                if (string.IsNullOrWhiteSpace(val))
                    continue;
                var vals = extractParams(val);

                tone.AL = vals[0];
                tone.FB = vals[1];
                tone.NE = vals[2];
                tone.NF = vals[3];

                for (int opNo = 0; opNo < 4; opNo++)
                {
                    val = getValue(table, "Prog" + progNo, "OP" + (opNo + 1));
                    if (string.IsNullOrWhiteSpace(val))
                    {
                        tone = null;
                        break;
                    }
                    vals = extractParams(val);

                    tone.aOp[opNo].AR = vals[0];
                    tone.aOp[opNo].DR = vals[1];
                    tone.aOp[opNo].SR = vals[2];
                    tone.aOp[opNo].RR = vals[3];
                    tone.aOp[opNo].SL = vals[4];
                    tone.aOp[opNo].TL = vals[5];
                    tone.aOp[opNo].KS = vals[6];
                    tone.aOp[opNo].ML = vals[7]; tone.aOp[opNo].FIXF = tone.aOp[opNo].ML;
                    tone.aOp[opNo].DT = vals[8];
                    tone.aOp[opNo].DT2 = vals[9];
                    tone.aOp[opNo].AM = vals[10];
                }
                for (int opNo = 0; opNo < 4; opNo++)
                {
                    val = getValue(table, "Prog" + progNo, "ADD" + (opNo + 1));
                    if (string.IsNullOrWhiteSpace(val))
                    {
                        tone = null;
                        break;
                    }
                    vals = extractParams(val);

                    tone.aOp[opNo].FIX = vals[0];
                    tone.aOp[opNo].FINE = vals[1];
                    tone.aOp[opNo].FIXR = tone.aOp[opNo].DT;
                    tone.aOp[opNo].OSCW = vals[2];
                    tone.aOp[opNo].REV = vals[3];
                    tone.aOp[opNo].EGSF = vals[4];
                }
                if (tone != null)
                    tones.Add(tone);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="tones"></param>
        private static void readOpnBank(Dictionary<string, Dictionary<string, string>> table, List<Tone> tones)
        {
            for (int progNo = 0; progNo < 128; progNo++)
            {
                Tone tone = new Tone();

                tone.Number = progNo;

                var val = getValue(table, "Prog" + progNo, "Name");
                if (!string.IsNullOrWhiteSpace(val))
                    tone.Name = val;

                val = getValue(table, "Prog" + progNo, "ALFB");
                if (string.IsNullOrWhiteSpace(val))
                    continue;
                var vals = extractParams(val);

                tone.AL = vals[0];
                tone.FB = vals[1];

                for (int opNo = 0; opNo < 4; opNo++)
                {
                    val = getValue(table, "Prog" + progNo, "OP" + (opNo + 1));
                    if (string.IsNullOrWhiteSpace(val))
                    {
                        tone = null;
                        break;
                    }
                    vals = extractParams(val);

                    tone.aOp[opNo].AR = vals[0];
                    tone.aOp[opNo].DR = vals[1];
                    tone.aOp[opNo].SR = vals[2];
                    tone.aOp[opNo].RR = vals[3];
                    tone.aOp[opNo].SL = vals[4];
                    tone.aOp[opNo].TL = vals[5];
                    tone.aOp[opNo].KS = vals[6];
                    tone.aOp[opNo].ML = vals[7];
                    tone.aOp[opNo].DT = vals[8];
                    tone.aOp[opNo].EG = vals[9];
                    tone.aOp[opNo].AM = vals[10];
                }
                if (tone != null)
                    tones.Add(tone);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="tones"></param>
        private static void readOplBank(Dictionary<string, Dictionary<string, string>> table, List<Tone> tones)
        {
            for (int progNo = 0; progNo < 128; progNo++)
            {
                Tone tone = new Tone();

                tone.Number = progNo;

                var val = getValue(table, "Prog" + progNo, "Name");
                if (!string.IsNullOrWhiteSpace(val))
                    tone.Name = val;

                val = getValue(table, "Prog" + progNo, "ALFB");
                if (string.IsNullOrWhiteSpace(val))
                    continue;
                var vals = extractParams(val);

                tone.AL = vals[0];
                tone.FB = vals[1];

                for (int opNo = 0; opNo < 4; opNo++)
                {
                    val = getValue(table, "Prog" + progNo, "OP" + (opNo + 1));
                    if (string.IsNullOrWhiteSpace(val))
                    {
                        tone = null;
                        break;
                    }
                    vals = extractParams(val);

                    tone.aOp[opNo].AR = vals[0] * 2;
                    tone.aOp[opNo].DR = vals[1] * 2;
                    tone.aOp[opNo].SR = vals[2] * 2;
                    tone.aOp[opNo].RR = vals[3];
                    tone.aOp[opNo].SL = vals[4];
                    tone.aOp[opNo].TL = vals[5] * 2;
                    tone.aOp[opNo].KS = (vals[6] & 7) >> 1;
                    tone.aOp[opNo].KSR = vals[6] & 1;
                    tone.aOp[opNo].ML = vals[7];

                    tone.aOp[opNo].DT = vals[8];
                    tone.aOp[opNo].WS = vals[9];
                    tone.aOp[opNo].AM = (vals[10] & 0x1) != 0 ? 1 : 0;
                    tone.aOp[opNo].VIB = (vals[10] & 0x2) != 0 ? 1 : 0;
                }
                if (tone != null)
                    tones.Add(tone);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="sectionName"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        private static string getValue(Dictionary<string, Dictionary<string, string>> table, string sectionName, string keyName)
        {
            if (table.ContainsKey(sectionName))
            {
                if (table[sectionName].ContainsKey(keyName))
                    return table[sectionName][keyName];
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static int[] extractParams(string text)
        {
            var vals = text.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);

            return new List<int>(vals.Select<string, int>((t) => { return int.Parse(t); })).ToArray();
        }

        private static readonly Regex regularExpressionSectionName =
            new Regex("^(" + Regex.Escape("[") + ")(.+)(" + Regex.Escape("]") + ")$");
        private static readonly Regex regularExpressionKeyValue = new Regex("^(.+?)(=)(.*)$");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, string>> loadIniFile(string fileName)
        {
            Dictionary<string, Dictionary<string, string>> sectionContents = new Dictionary<string, Dictionary<string, string>>();
            var fileContent = File.ReadLines(fileName);

            Dictionary<string, string> sectionContent = null;
            foreach (string content in fileContent)
            {
                if (string.IsNullOrEmpty(content))
                    continue;

                if (regularExpressionSectionName.Match(content).Success)
                {
                    string section = regularExpressionSectionName.Match(content).Groups[2].Value;
                    if (!sectionContents.TryGetValue(section, out sectionContent))
                    {
                        sectionContent = new Dictionary<string, string>();
                        sectionContents[section] = sectionContent;
                    }
                    continue;
                }

                if (sectionContent == null)
                    continue;

                // キーと値の「～=～」の判定
                if (regularExpressionKeyValue.Match(content).Success)
                {
                    string key = regularExpressionKeyValue.Match(content).Groups[1].Value;
                    string value = regularExpressionKeyValue.Match(content).Groups[3].Value;
                    sectionContent[key] = value;
                }
            }

            return sectionContents;
        }

    }
}
