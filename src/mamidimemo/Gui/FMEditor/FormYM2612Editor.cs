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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2612;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM2612Editor : FormFmEditor
    {
        private YM2612Timbre timbre;

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
        public FormYM2612Editor()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2612Editor(YM2612 inst, YM2612Timbre timbre) : base(inst, timbre)
        {
            this.timbre = timbre;
            InitializeComponent();

            AddControl(new YM2612GeneralContainer(inst, timbre, "General"));

            AddControl(new YM2612OperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM2612OperatorContainer(timbre.Ops[1], "Operator 2"));
            AddControl(new YM2612OperatorContainer(timbre.Ops[2], "Operator 3"));
            AddControl(new YM2612OperatorContainer(timbre.Ops[3], "Operator 4"));
        }

    }

}
