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
    public partial class YM3812GeneralContainer : RegisterContainerBase
    {
        private YM3812.YM3812Timbre tim;

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
                    "GlobalSettings.EN",
                    "GlobalSettings.AMD",
                    "GlobalSettings.VIB"
                    );
            }
            set
            {
                DeserializeProps(this, value,
                    nameof(tim.ALG),
                    nameof(tim.FB),
                    "GlobalSettings.EN",
                    "GlobalSettings.AMD",
                    "GlobalSettings.VIB"
                    );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YM3812GeneralContainer(InstrumentBase inst, YM3812.YM3812Timbre tim, string name) : base(tim, name)
        {
            InitializeComponent();

            this.tim = tim;

            AddControl(new RegisterValue("ALG", tim.ALG, 0, 1));
            AddControl(new RegisterValue("FB", tim.FB, 0, 7));

            AddControl(new RegisterFlag("LFO", "GlobalSettings.EN", tim.GlobalSettings.EN != 0 ? true : false));
            AddControl(new RegisterValue("AMD", "GlobalSettings.AMD", tim.GlobalSettings.AMD == null ? -1 : tim.GlobalSettings.AMD.Value, 0, 1, true));
            AddControl(new RegisterValue("VIB", "GlobalSettings.VIB", tim.GlobalSettings.VIB == null ? -1 : tim.GlobalSettings.VIB.Value, 0, 1, true));

            AddControl(new RegisterAlg2OpImg((RegisterValue)GetControl("ALG")));
            AddControl(new RegisterSpace("spc") { Dock = DockStyle.Right });
            AddControl(new RegisterOscViewer(inst));
        }

    }
}
