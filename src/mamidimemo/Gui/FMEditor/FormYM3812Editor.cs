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
using static zanac.MAmidiMEmo.Instruments.Chips.YM3812;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM3812Editor : FormFmEditor
    {
        private YM3812Timbre timbre;

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
        public FormYM3812Editor()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM3812Editor(YM3812 inst, YM3812Timbre timbre) : base(inst, timbre)
        {
            this.timbre = timbre;
            InitializeComponent();

            AddControl(new YM3812GeneralContainer(timbre, "General"));

            AddControl(new YM3812OperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM3812OperatorContainer(timbre.Ops[1], "Operator 2"));
        }

    }

}
