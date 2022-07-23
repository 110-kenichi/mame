// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class YM3806GeneralContainer : RegisterContainerBase
    {
        private YM3806.YM3806Timbre tim;

        /// <summary>
        /// 
        /// </summary>
        public override string SerializeData
        {
            get
            {
                return SerializeProps(this,
                    nameof(tim.ALG),
                    nameof(tim.FB),
                    nameof(tim.REV),
                    nameof(tim.AMS),
                    nameof(tim.PMS),
                    nameof(tim.PitchShift13),
                    nameof(tim.PitchShift24),
                    "GlobalSettings.EN",
                    "GlobalSettings.LFOE",
                    "GlobalSettings.LFRQ"
                    );
            }
            set
            {
                DeserializeProps(this, value,
                   nameof(tim.ALG),
                   nameof(tim.FB),
                   nameof(tim.REV),
                   nameof(tim.AMS),
                   nameof(tim.PMS),
                   nameof(tim.PitchShift13),
                   nameof(tim.PitchShift24),
                    "GlobalSettings.EN",
                    "GlobalSettings.LFOE",
                    "GlobalSettings.LFRQ"
                   );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YM3806GeneralContainer(InstrumentBase inst, YM3806.YM3806Timbre tim, string name) : base(tim, name)
        {
            InitializeComponent();

            this.tim = tim;

            AddControl(new RegisterValue("ALG", tim.ALG, 0, 7));
            AddControl(new RegisterValue("FB", tim.FB, 0, 7));
            AddControl(new RegisterValue("REV", tim.REV, 0, 1));
            AddControl(new RegisterValue("AMS", tim.AMS, 0, 3));
            AddControl(new RegisterValue("PMS", tim.PMS, 0, 7));
            AddControl(new RegisterValue("P13", "PitchShift13", tim.PitchShift13, -1200, 1200, false));
            AddControl(new RegisterValue("P24", "PitchShift24", tim.PitchShift24, -1200, 1200, false));

            AddControl(new RegisterFlag("EN", "GlobalSettings.EN", tim.GlobalSettings.EN != 0 ? true : false));
            AddControl(new RegisterValue("LFOE", "GlobalSettings.LFOE", tim.GlobalSettings.LFOE == null ? -1 : tim.GlobalSettings.LFOE.Value, 0, 1, true));
            AddControl(new RegisterValue("LFRQ", "GlobalSettings.LFRQ", tim.GlobalSettings.LFRQ == null ? -1 : tim.GlobalSettings.LFRQ.Value, 0, 7, true));

            AddControl(new RegisterOpqAlg4OpImg((RegisterValue)GetControl("ALG")));
            AddControl(new RegisterSpace("spc") { Dock = DockStyle.Right });
            AddControl(new RegisterOscViewer(inst));
        }
    }
}
