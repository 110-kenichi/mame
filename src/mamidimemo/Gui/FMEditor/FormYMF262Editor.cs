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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;
using static zanac.MAmidiMEmo.Instruments.Chips.YMF262;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYMF262Editor : FormFmEditor
    {
        private YMF262Timbre timbre;

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
                    var op = timbre.Ops[i];
                    string opt = GetControl("Operator " + (i + 1)).SerializeData;
                    list.Add(opt);
                }
                return list.ToArray();
            }
            set
            {
                for (int i = 0; i < timbre.Ops.Length; i++)
                {
                    var op = timbre.Ops[i];
                    GetControl("Operator " + (i + 1)).SerializeData = value[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYMF262Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYMF262Editor(YMF262 inst, YMF262Timbre timbre) : base(inst, timbre)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YMF262EdSize;

            var gen = new YMF262GeneralContainer(inst, timbre, "General");
            AddControl(gen);

            AddControl(new YMF262OperatorContainer(timbre.Ops[0], null, "Operator 1"));
            AddControl(new YMF262OperatorContainer(timbre.Ops[1], null, "Operator 2"));
            AddControl(new YMF262OperatorContainer(timbre.Ops[2], gen, "Operator 3"));
            AddControl(new YMF262OperatorContainer(timbre.Ops[3], gen, "Operator 4"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YMF262EdSize = Size;
        }

        protected override void ApplyTone(Tone tone)
        {
            int alg = 0;
            switch (tone.AL)
            {
                case 0:
                    alg = 2;
                    break;
                case 1:
                    alg = 4;    //?
                    break;
                case 2:
                    alg = 4;    //?
                    break;
                case 3:
                    alg = 3;    //?
                    break;
                case 4:
                    alg = 3;
                    break;
                case 5:
                    alg = 5;    //?
                    break;
                case 6:
                    alg = 5;    //?
                    break;
                case 7:
                    alg = 5;
                    break;
            }
            ((RegisterValue)this["General"]["ALG"]).Value = alg;
            ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
            ((RegisterValue)this["General"]["FB2"]).Value = tone.FB;

            for (int i = 0; i < 4; i++)
            {
                ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tone.aOp[i].AR / 2;
                ((RegisterValue)this["Operator " + (i + 1)]["DR"]).Value = tone.aOp[i].DR / 2;
                ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tone.aOp[i].RR;
                ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tone.aOp[i].SL;
                ((RegisterValue)this["Operator " + (i + 1)]["SR"]).Value = tone.aOp[i].SR / 2;
                ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tone.aOp[i].TL / 2;
                ((RegisterValue)this["Operator " + (i + 1)]["KSL"]).Value = tone.aOp[i].KS;
                ((RegisterValue)this["Operator " + (i + 1)]["KSR"]).Value = 0;
                ((RegisterValue)this["Operator " + (i + 1)]["MFM"]).Value = tone.aOp[i].ML;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tone.aOp[i].AM;
                ((RegisterValue)this["Operator " + (i + 1)]["VIB"]).Value = 0;
                ((RegisterValue)this["Operator " + (i + 1)]["EG"]).Value = 0;
                ((RegisterValue)this["Operator " + (i + 1)]["WS"]).Value = 0;
            }
            if (string.IsNullOrWhiteSpace(tone.Name))
                timbre.Memo = tone.Name;
        }
    }

}
