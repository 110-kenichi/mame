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
    public partial class YM2414GeneralContainer : RegisterContainerBase
    {
        private YM2414.YM2414Timbre tim;

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
                    nameof(tim.AMS),
                    nameof(tim.PMS),
                    nameof(tim.AMS2),
                    nameof(tim.PMS2),

                    "GlobalSettings.EN",
                    "GlobalSettings.LFRQ",
                    "GlobalSettings.LFRQ2",
                    "GlobalSettings.AMD",
                    "GlobalSettings.PMD",
                    "GlobalSettings.AMD2",
                    "GlobalSettings.PMD2",
                    "GlobalSettings.LFOW",
                    "GlobalSettings.LFOW2",
                    "GlobalSettings.SYNC",
                    "GlobalSettings.SYNC2",
                    "GlobalSettings.NE",
                    "GlobalSettings.NFRQ",
                    "GlobalSettings.LFD",
                    "GlobalSettings.LFD2"
                    );
            }
            set
            {
                DeserializeProps(this, value,
                    nameof(tim.ALG),
                    nameof(tim.FB),
                    nameof(tim.AMS),
                    nameof(tim.PMS),
                    nameof(tim.AMS2),
                    nameof(tim.PMS2),

                    "GlobalSettings.EN",
                    "GlobalSettings.LFRQ",
                    "GlobalSettings.LFRQ2",
                    "GlobalSettings.AMD",
                    "GlobalSettings.PMD",
                    "GlobalSettings.AMD2",
                    "GlobalSettings.PMD2",
                    "GlobalSettings.LFOW",
                    "GlobalSettings.LFOW2",
                    "GlobalSettings.SYNC",
                    "GlobalSettings.SYNC2",
                    "GlobalSettings.NE",
                    "GlobalSettings.NFRQ",
                    "GlobalSettings.LFD",
                    "GlobalSettings.LFD2"
                    );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YM2414GeneralContainer(InstrumentBase inst, YM2414.YM2414Timbre tim, string name) : base(tim, name)
        {
            InitializeComponent();

            this.tim = tim;

            AddControl(new RegisterValue("ALG", tim.ALG, 0, 7));
            AddControl(new RegisterValue("FB", tim.FB, 0, 7));
            AddControl(new RegisterValue("AMS", tim.AMS, 0, 3));
            AddControl(new RegisterValue("PMS", tim.PMS, 0, 7));
            AddControl(new RegisterValue("AMS2", tim.AMS2, 0, 3));
            AddControl(new RegisterValue("PMS2", tim.PMS2, 0, 7));

            AddControl(new RegisterFlag("EN", "GlobalSettings.EN", tim.GlobalSettings.EN != 0 ? true : false));

            AddControl(new RegisterValue("LFRQ", "GlobalSettings.LFRQ", tim.GlobalSettings.LFRQ == null ? -1 : tim.GlobalSettings.LFRQ.Value, 0, 255, true));
            AddControl(new RegisterValue("AMD", "GlobalSettings.AMD", tim.GlobalSettings.AMD == null ? -1 : tim.GlobalSettings.AMD.Value, 0, 127, true));
            AddControl(new RegisterValue("PMD", "GlobalSettings.PMD", tim.GlobalSettings.PMD == null ? -1 : tim.GlobalSettings.PMD.Value, 0, 127, true));
            AddControl(new RegisterValue("LFOW", "GlobalSettings.LFOW", tim.GlobalSettings.LFOW == null ? -1 : tim.GlobalSettings.LFOW.Value, 0, 3, true));
            AddControl(new RegisterValue("SY", "GlobalSettings.SYNC", tim.GlobalSettings.SYNC == null ? -1 : tim.GlobalSettings.SYNC.Value, 0, 1, true));

            AddControl(new RegisterValue("LFRQ2", "GlobalSettings.LFRQ2", tim.GlobalSettings.LFRQ2 == null ? -1 : tim.GlobalSettings.LFRQ2.Value, 0, 255, true));
            AddControl(new RegisterValue("AMD2", "GlobalSettings.AMD2", tim.GlobalSettings.AMD2 == null ? -1 : tim.GlobalSettings.AMD2.Value, 0, 127, true));
            AddControl(new RegisterValue("PMD2", "GlobalSettings.PMD2", tim.GlobalSettings.PMD2 == null ? -1 : tim.GlobalSettings.PMD2.Value, 0, 127, true));
            AddControl(new RegisterValue("LFOW2", "GlobalSettings.LFOW2", tim.GlobalSettings.LFOW2 == null ? -1 : tim.GlobalSettings.LFOW2.Value, 0, 3, true));
            AddControl(new RegisterValue("SY2", "GlobalSettings.SYNC2", tim.GlobalSettings.SYNC2 == null ? -1 : tim.GlobalSettings.SYNC2.Value, 0, 1, true));

            AddControl(new RegisterValue("NE", "GlobalSettings.NE", tim.GlobalSettings.NE == null ? -1 : tim.GlobalSettings.NE.Value, 0, 1, true));
            AddControl(new RegisterValue("NFRQ", "GlobalSettings.NFRQ", tim.GlobalSettings.NFRQ == null ? -1 : tim.GlobalSettings.NFRQ.Value, 0, 31, true));

            AddControl(new RegisterValue("LFD", "GlobalSettings.LFD", tim.GlobalSettings.LFD == null ? -1 : tim.GlobalSettings.LFD.Value, 0, 99, true));
            AddControl(new RegisterValue("LFD2", "GlobalSettings.LFD2", tim.GlobalSettings.LFD2 == null ? -1 : tim.GlobalSettings.LFD2.Value, 0, 99, true));

            AddControl(new RegisterAlg4OpImg((RegisterValue)GetControl("ALG")));
            AddControl(new RegisterSpace("spc") { Dock = DockStyle.Right });
            AddControl(new RegisterOscViewer(inst));
        }

    }
}
