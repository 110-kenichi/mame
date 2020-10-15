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
                return RegisterContainerBase.SerializeProps(GetControl("General"),
                    nameof(timbre.ALG),
                    nameof(timbre.FB),
                    nameof(timbre.AMS),
                    nameof(timbre.PMS));
            }
            set
            {
                RegisterContainerBase.DeserializeProps(GetControl("General"), value,
                    nameof(timbre.ALG),
                    nameof(timbre.FB),
                    nameof(timbre.AMS),
                    nameof(timbre.PMS));
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
                        nameof(op.DT2));
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
                        nameof(op.DT2));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2151Editor()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2151Editor(YM2151 inst, YM2151Timbre timbre) : base(inst, timbre)
        {
            this.timbre = timbre;
            InitializeComponent();

            AddControl(new YM2151GeneralContainer(timbre, "General"));

            AddControl(new YM2151OperatorContainer(timbre.Ops[0], "Operator 1"));
            AddControl(new YM2151OperatorContainer(timbre.Ops[1], "Operator 2"));
            AddControl(new YM2151OperatorContainer(timbre.Ops[2], "Operator 3"));
            AddControl(new YM2151OperatorContainer(timbre.Ops[3], "Operator 4"));
        }

    }

}
