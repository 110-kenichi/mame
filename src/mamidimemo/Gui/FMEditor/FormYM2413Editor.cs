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

                var mod = timbre.Modulator;
                string modt = GetControl("Modulator").SerializeData;
                list.Add(modt);

                var ca = timbre.Career;
                string cat = GetControl("Career").SerializeData;
                list.Add(cat);

                return list.ToArray();
            }
            set
            {
                var mod = timbre.Modulator;
                GetControl("Modulator").SerializeData = value[0];

                var ca = timbre.Career;
                GetControl("Career").SerializeData = value[1];
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
