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
    public partial class YM2610BGeneralContainer : RegisterContainerBase
    {
        private YM2610B.YM2610BTimbre tim;

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
                    nameof(tim.FMS),
                    "GlobalSettings.EN",
                    "GlobalSettings.LFOEN",
                    "GlobalSettings.LFRQ");
            }
            set
            {
                DeserializeProps(this, value,
                    nameof(tim.ALG),
                    nameof(tim.FB),
                    nameof(tim.AMS),
                    nameof(tim.FMS),
                    "GlobalSettings.EN",
                    "GlobalSettings.LFOEN",
                    "GlobalSettings.LFRQ");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YM2610BGeneralContainer(InstrumentBase inst, YM2610B.YM2610BTimbre tim, string name) : base(tim, name)
        {
            InitializeComponent();

            this.tim = tim;

            AddControl(new RegisterValue("ALG", tim.ALG, 0, 7));
            AddControl(new RegisterValue("FB", tim.FB, 0, 7));
            AddControl(new RegisterValue("AMS", tim.AMS, 0, 3));
            AddControl(new RegisterValue("FMS", tim.FMS, 0, 7));

            AddControl(new RegisterFlag("LFO", "GlobalSettings.EN", tim.GlobalSettings.EN != 0 ? true : false));
            AddControl(new RegisterValue("LFOEN", "GlobalSettings.LFOEN", tim.GlobalSettings.LFOEN == null ? -1 : tim.GlobalSettings.LFOEN.Value, 0, 255, true));
            AddControl(new RegisterValue("LFRQ", "GlobalSettings.LFRQ", tim.GlobalSettings.LFRQ == null ? -1 : tim.GlobalSettings.LFRQ.Value, 0, 7, true));

            AddControl(new RegisterAlg4OpImg((RegisterValue)GetControl("ALG")));
            AddControl(new RegisterSpace("spc") { Dock = DockStyle.Right });
            AddControl(new RegisterOscViewer(inst));
        }

    }
}
