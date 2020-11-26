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
        public FormYM2151Editor(YM2151 inst, YM2151Timbre timbre) : base(inst, timbre)
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
            ((RegisterValue)this["General"]["ALG"]).Value = tone.AL;
            ((RegisterValue)this["General"]["FB"]).Value = tone.FB;
            ((RegisterValue)this["General"]["AMS"]).Value = 0;
            ((RegisterValue)this["General"]["PMS"]).Value = 0;
            ((RegisterFlag)this["General"]["GlobalSettings.EN"]).Value = false;
            ((RegisterValue)this["General"]["GlobalSettings.LFRQ"]).NullableValue = null;
            ((RegisterValue)this["General"]["GlobalSettings.LFOF"]).NullableValue = null;
            ((RegisterValue)this["General"]["GlobalSettings.LFOD"]).NullableValue = null;
            ((RegisterValue)this["General"]["GlobalSettings.LFOW"]).NullableValue = null;

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
                ((RegisterValue)this["Operator " + (i + 1)]["DT2"]).Value = tone.aOp[i].DT2;
            }
            if (string.IsNullOrWhiteSpace(tone.Name))
                timbre.Memo = tone.Name;
        }

    }

}
