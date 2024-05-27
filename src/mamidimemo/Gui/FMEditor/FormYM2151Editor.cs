// copyright-holders:K.Ito
using FM_SoundConvertor;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2151;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM2151Editor : FormFmEditor
    {
        private YM2151Timbre timbre;

        /// <summary>
        /// 
        /// </summary>
        public string MmlValueGeneral
        {
            get
            {
                return GetControl("General").SerializeData;
            }
            set
            {
                GetControl("General").SerializeData = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] MmlValueOps
        {
            get
            {
                List<string> list = new List<string>();
                for (int i = 0; i < timbre.Ops.Length; i++)
                {
                    string opt = GetControl("Operator " + (i + 1)).SerializeData;
                    list.Add(opt);
                }
                return list.ToArray();
            }
            set
            {
                for (int i = 0; i < timbre.Ops.Length; i++)
                {
                    GetControl("Operator " + (i + 1)).SerializeData = value[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2151Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2151Editor(YM2151 inst, YM2151Timbre timbre, bool singleSelect) : base(inst, timbre, singleSelect)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YM2151EdSize;

            AddControl(new YM2151GeneralContainer(inst, timbre, "General"));

            AddControl(new YM2151OperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM2151OperatorContainer(timbre.Ops[1], "Operator 2"));
            AddControl(new YM2151OperatorContainer(timbre.Ops[2], "Operator 3"));
            AddControl(new YM2151OperatorContainer(timbre.Ops[3], "Operator 4"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YM2151EdSize = Size;
        }

        protected override void ApplyTone(Tone tone)
        {
            if (tone.MML != null)
            {
                Timbre.TimbreName = tone.MML[0];
                MmlValueGeneral = tone.MML[1];
                MmlValueOps = new string[] { tone.MML[2], tone.MML[3], tone.MML[4], tone.MML[5] };
            }
            else
            {
                ((RegisterValue)this["General"]["ALG"]).Value = tone.AL;
                ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
                ((RegisterValue)this["General"]["AMS"]).Value = tone.AMS;
                ((RegisterValue)this["General"]["PMS"]).Value = tone.PMS;
                ((RegisterValue)this["General"]["GlobalSettings.LFRQ"]).NullableValue = tone.LFRQ;
                ((RegisterValue)this["General"]["GlobalSettings.AMD"]).NullableValue = tone.AMD;
                ((RegisterValue)this["General"]["GlobalSettings.PMD"]).NullableValue = tone.PMS;
                ((RegisterValue)this["General"]["GlobalSettings.LFOW"]).NullableValue = tone.LFOW;

                ((RegisterValue)this["General"]["GlobalSettings.NE"]).NullableValue = tone.NE;
                ((RegisterValue)this["General"]["GlobalSettings.NFRQ"]).NullableValue = tone.NF;

                if (tone.NE > 0 ||
                   tone.LFRQ > 0 ||
                   tone.NF > 0 ||
                   tone.LFOW > 0 ||
                   tone.AMD > 0 ||
                   tone.PMD > 0
                   )
                    ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = true;

                for (int i = 0; i < 4; i++)
                {
                    ((RegisterFlag)this["Operator " + (i + 1)]["EN"]).Value = true;
                    ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tone.aOp[i].AR;
                    ((RegisterValue)this["Operator " + (i + 1)]["D1R"]).Value = tone.aOp[i].DR;
                    ((RegisterValue)this["Operator " + (i + 1)]["D2R"]).Value = tone.aOp[i].SR < 0 ? (byte)0 : (byte)tone.aOp[i].SR;
                    ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tone.aOp[i].RR;
                    ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tone.aOp[i].SL;
                    ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tone.aOp[i].TL;
                    ((RegisterValue)this["Operator " + (i + 1)]["RS"]).Value = tone.aOp[i].KS;
                    ((RegisterValue)this["Operator " + (i + 1)]["MUL"]).Value = tone.aOp[i].ML;
                    ((RegisterValue)this["Operator " + (i + 1)]["DT1"]).Value = tone.aOp[i].DT;
                    ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tone.aOp[i].AM;
                    ((RegisterValue)this["Operator " + (i + 1)]["DT2"]).Value = tone.aOp[i].DT2;
                    ((RegisterValue)this["Operator " + (i + 1)]["LS"]).Value = tone.aOp[i].LS;
                    ((RegisterValue)this["Operator " + (i + 1)]["KVS"]).Value = tone.aOp[i].KVS;
                }
                timbre.TimbreName = tone.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTone(TimbreBase timbre, Tone tone)
        {
            if (tone.MML != null)
            {
                ApplyTimbre(timbre);
                ApplyTone(tone);
            }
            else
            {
                ((YM2151)Instrument).ImportToneFile(timbre, tone);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        protected override void ApplyTimbre(TimbreBase timbre)
        {
            YM2151Timbre tim = (YM2151Timbre)timbre;
            this.timbre = tim;

            this["General"].Target = tim;
            ((RegisterValue)this["General"]["ALG"]).Value = tim.ALG;
            ((RegisterValue)this["General"]["FB"]).Value = tim.FB;
            ((RegisterValue)this["General"]["AMS"]).Value = tim.AMS;
            ((RegisterValue)this["General"]["PMS"]).Value = tim.PMS;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = tim.GlobalSettings.Enable;
            ((RegisterValue)this["General"]["GlobalSettings.LFRQ"]).NullableValue = tim.GlobalSettings.LFRQ;
            ((RegisterValue)this["General"]["GlobalSettings.AMD"]).NullableValue = tim.GlobalSettings.AMD;
            ((RegisterValue)this["General"]["GlobalSettings.PMD"]).NullableValue = tim.GlobalSettings.PMD;
            ((RegisterValue)this["General"]["GlobalSettings.LFOW"]).NullableValue = tim.GlobalSettings.LFOW;
            for (int i = 0; i < 4; i++)
            {
                this["Operator " + (i + 1)].Target = tim.Ops[i];
                ((RegisterFlag)this["Operator " + (i + 1)]["EN"]).Value = tim.Ops[i].Enable == 0 ? false : true;
                ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tim.Ops[i].AR;
                ((RegisterValue)this["Operator " + (i + 1)]["D1R"]).Value = tim.Ops[i].D1R;
                ((RegisterValue)this["Operator " + (i + 1)]["D2R"]).Value = tim.Ops[i].D2R;
                ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tim.Ops[i].RR;
                ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tim.Ops[i].SL;
                ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tim.Ops[i].TL;
                ((RegisterValue)this["Operator " + (i + 1)]["RS"]).Value = tim.Ops[i].RS;
                ((RegisterValue)this["Operator " + (i + 1)]["MUL"]).Value = tim.Ops[i].MUL;
                ((RegisterValue)this["Operator " + (i + 1)]["DT1"]).Value = tim.Ops[i].DT1;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tim.Ops[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["DT2"]).Value = tim.Ops[i].DT2;
                ((RegisterValue)this["Operator " + (i + 1)]["LS"]).Value = tim.Ops[i].LS;
                ((RegisterValue)this["Operator " + (i + 1)]["KVS"]).Value = tim.Ops[i].KVS;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string ExtensionsFilterExt
        {
            get
            {
                return "*.mopm;*.mopn";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override IEnumerable<Tone> ImportToneFile(string file)
        {
            IEnumerable<Tone> tones = base.ImportToneFile(file);
            if (tones != null)
                return tones;

            string ext = System.IO.Path.GetExtension(file);

            if (ext.ToUpper(CultureInfo.InvariantCulture).Equals(".MOPN"))
            {
                try
                {
                    string txt = System.IO.File.ReadAllText(file);
                    StringReader rs = new StringReader(txt);

                    string ftname = rs.ReadLine();
                    if ("*.mopn" == ftname)
                    {
                        string ver = rs.ReadLine();
                        if (ver != "1.0")
                            throw new InvalidDataException();
                        int num = int.Parse(rs.ReadLine());
                        List<string> lines = new List<string>();
                        List<Tone> ts = new List<Tone>();
                        int progNo = 0;
                        while (true)
                        {
                            string line = rs.ReadLine();
                            if (line == null || line == "-")
                            {
                                if (lines.Count == 0)
                                    break;
                                Tone t = new Tone();
                                var mml = lines.ToArray();

                                var general = mml[1].Split(',');
                                mml[1] = String.Format("{0},{1},{2},0,0,,,,,,", general[0], general[1], general[2]);
                                for (int i = 2; i < mml.Length; i++)
                                {
                                    var op = mml[i].Split(',');
                                    mml[i] = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},0,0,-1",
                                        op[0], op[1], op[2], op[3], op[4], op[5], op[6], op[7], op[8], op[9], op[10]);
                                }
                                t.MML = mml;

                                t.Name = t.MML[0];
                                t.Number = progNo++;
                                ts.Add(t);
                                lines.Clear();
                                if (line == null)
                                    break;
                                continue;
                            }
                            lines.Add(line);
                        }
                        tones = ts;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(Exception))
                        throw;
                    else if (ex.GetType() == typeof(SystemException))
                        throw;

                    MessageBox.Show(Resources.FailedLoadFile + "\r\n" + ex.Message);
                }
            }
            return tones;
        }

        protected override string[] GetMMlValues()
        {
            return new string[] { Timbre.TimbreName, MmlValueGeneral, MmlValueOps[0], MmlValueOps[1], MmlValueOps[2], MmlValueOps[3] };

        }

    }

}
