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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2413;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class FormYM2413Editor : FormFmEditor
    {
        private YM2413Timbre timbre;

        /// <summary>
        /// 
        /// </summary>
        public string MmlValueGeneral
        {
            get
            {
                return RegisterContainerBase.SerializeProps(GetControl("General"),
                    nameof(timbre.FB),
                    nameof(timbre.SUS));
            }
            set
            {
                RegisterContainerBase.DeserializeProps(GetControl("General"), value,
                    nameof(timbre.FB),
                    nameof(timbre.SUS));
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

                var mod = timbre.Modulator;
                string modt = RegisterContainerBase.SerializeProps(GetControl("Modulator"),
                    nameof(mod.AR),
                    nameof(mod.DR),
                    nameof(mod.RR),
                    nameof(mod.SL),
                    nameof(mod.SR),
                    nameof(mod.TL),
                    nameof(mod.KSL),
                    nameof(mod.KSR),
                    nameof(mod.MUL),
                    nameof(mod.AM),
                    nameof(mod.VIB),
                    nameof(mod.EG),
                    nameof(mod.DIST));
                list.Add(modt);

                var ca = timbre.Career;
                string cat = RegisterContainerBase.SerializeProps(GetControl("Career"),
                    nameof(ca.AR),
                    nameof(ca.DR),
                    nameof(ca.RR),
                    nameof(ca.SL),
                    nameof(ca.SR),
                    nameof(ca.KSL),
                    nameof(ca.KSR),
                    nameof(ca.MUL),
                    nameof(ca.AM),
                    nameof(ca.VIB),
                    nameof(ca.EG),
                    nameof(ca.DIST));
                list.Add(cat);

                return list.ToArray();
            }
            set
            {
                var mod = timbre.Modulator;
                RegisterContainerBase.DeserializeProps(GetControl("Modulator"), value[0],
                    nameof(mod.AR),
                    nameof(mod.DR),
                    nameof(mod.RR),
                    nameof(mod.SL),
                    nameof(mod.SR),
                    nameof(mod.TL),
                    nameof(mod.KSL),
                    nameof(mod.KSR),
                    nameof(mod.MUL),
                    nameof(mod.AM),
                    nameof(mod.VIB),
                    nameof(mod.EG),
                    nameof(mod.DIST));

                var ca = timbre.Career;
                RegisterContainerBase.DeserializeProps(GetControl("Career"), value[1],
                    nameof(ca.AR),
                    nameof(ca.DR),
                    nameof(ca.RR),
                    nameof(ca.SL),
                    nameof(ca.SR),
                    nameof(ca.KSL),
                    nameof(ca.KSR),
                    nameof(ca.MUL),
                    nameof(ca.AM),
                    nameof(ca.VIB),
                    nameof(ca.EG),
                    nameof(ca.DIST));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2413Editor()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public FormYM2413Editor(YM2413 inst, YM2413Timbre timbre) : base(inst, timbre)
        {
            this.timbre = timbre;
            InitializeComponent();

            AddControl(new YM2413GeneralContainer(timbre, "General"));

            AddControl(new YM2413OperatorContainer(timbre.Modulator, "Modulator"));
            AddControl(new YM2413OperatorContainer(timbre.Career, "Career"));
        }

    }

}
