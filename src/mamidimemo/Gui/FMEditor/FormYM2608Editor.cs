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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2608;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM2608Editor : FormFmEditor
    {
        private YM2608Timbre timbre;

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
        public FormYM2608Editor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2608Editor(YM2608 inst, YM2608Timbre timbre) : base(inst, timbre)
        {
            this.timbre = timbre;
            InitializeComponent();

            Size = Settings.Default.YM2608EdSize;

            AddControl(new YM2608GeneralContainer(inst, timbre, "General"));

            AddControl(new YM2608OperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM2608OperatorContainer(timbre.Ops[1], "Operator 2"));
            AddControl(new YM2608OperatorContainer(timbre.Ops[2], "Operator 3"));
            AddControl(new YM2608OperatorContainer(timbre.Ops[3], "Operator 4"));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.YM2608EdSize = Size;
        }

        protected override void ApplyTone(Tone tone)
        {
            ((RegisterValue)this["General"]["ALG"]).Value = tone.AL;
            ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
            ((RegisterValue)this["General"]["AMS"]).Value = 0;
            ((RegisterValue)this["General"]["FMS"]).Value = 0;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = false;
            ((RegisterValue)this["General"]["GlobalSettings.LFOEN"]).NullableValue = null;
            ((RegisterValue)this["General"]["GlobalSettings.LFRQ"]).NullableValue = null;

            for (int i = 0; i < 4; i++)
            {
                ((RegisterFlag)this["Operator " + (i + 1)]["EN"]).Value = true;
                ((RegisterValue)this["Operator " + (i + 1)]["AR"]).Value = tone.aOp[i].AR;
                ((RegisterValue)this["Operator " + (i + 1)]["D1R"]).Value = tone.aOp[i].DR;
                ((RegisterValue)this["Operator " + (i + 1)]["D1R"]).Value = tone.aOp[i].SR;
                ((RegisterValue)this["Operator " + (i + 1)]["RR"]).Value = tone.aOp[i].RR;
                ((RegisterValue)this["Operator " + (i + 1)]["SL"]).Value = tone.aOp[i].SL;
                ((RegisterValue)this["Operator " + (i + 1)]["TL"]).Value = tone.aOp[i].TL;
                ((RegisterValue)this["Operator " + (i + 1)]["RS"]).Value = tone.aOp[i].KS;
                ((RegisterValue)this["Operator " + (i + 1)]["MUL"]).Value = tone.aOp[i].ML;
                ((RegisterValue)this["Operator " + (i + 1)]["DT1"]).Value = tone.aOp[i].DT;
                ((RegisterValue)this["Operator " + (i + 1)]["AM"]).Value = tone.aOp[i].AM;
            }
            if (string.IsNullOrWhiteSpace(tone.Name))
                timbre.Memo = tone.Name;
        }

    }

}
