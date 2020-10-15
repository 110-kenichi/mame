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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2610B;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM2610BEditor : FormFmEditor
    {
        private YM2610BTimbre timbre;

        /// <summary>
        /// 
        /// </summary>
        public string MmlValueGeneral
        {
            get
            {
                return RegisterContainerBase.SerializeProps(GetControl("General"),
                    nameof(timbre.ALG),
                    nameof(timbre.FB),
                    nameof(timbre.AMS),
                    nameof(timbre.FMS));
            }
            set
            {
                RegisterContainerBase.DeserializeProps(GetControl("General"), value,
                    nameof(timbre.ALG),
                    nameof(timbre.FB),
                    nameof(timbre.AMS),
                    nameof(timbre.FMS));
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
                    string opt = RegisterContainerBase.SerializeProps(GetControl("Operator " + (i + 1)),
                        nameof(op.EN),
                        nameof(op.AR),
                        nameof(op.D1R),
                        nameof(op.D2R),
                        nameof(op.RR),
                        nameof(op.SL),
                        nameof(op.TL),
                        nameof(op.RS),
                        nameof(op.MUL),
                        nameof(op.DT1),
                        nameof(op.AM),
                        nameof(op.SSG));
                    list.Add(opt);
                }
                return list.ToArray();
            }
            set
            {
                for (int i = 0; i < timbre.Ops.Length; i++)
                {
                    var op = timbre.Ops[i];
                    RegisterContainerBase.DeserializeProps(GetControl("Operator " + (i + 1)), value[i],
                        nameof(op.EN),
                        nameof(op.AR),
                        nameof(op.D1R),
                        nameof(op.D2R),
                        nameof(op.RR),
                        nameof(op.SL),
                        nameof(op.TL),
                        nameof(op.RS),
                        nameof(op.MUL),
                        nameof(op.DT1),
                        nameof(op.AM),
                        nameof(op.SSG));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2610BEditor()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2610BEditor(YM2610B inst, YM2610BTimbre timbre) : base(inst, timbre)
        {
            this.timbre = timbre;
            InitializeComponent();

            AddControl(new YM2610BGeneralContainer(timbre, "General"));

            AddControl(new YM2610BOperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM2610BOperatorContainer(timbre.Ops[1], "Operator 2"));
            AddControl(new YM2610BOperatorContainer(timbre.Ops[2], "Operator 3"));
            AddControl(new YM2610BOperatorContainer(timbre.Ops[3], "Operator 4"));
        }

    }

}
