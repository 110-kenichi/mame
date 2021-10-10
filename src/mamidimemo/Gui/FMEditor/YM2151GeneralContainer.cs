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
    public partial class YM2151GeneralContainer : RegisterContainerBase
    {
        private YM2151.YM2151Timbre tim;

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
                    "GlobalSettings.EN",
                    "GlobalSettings.LFRQ",
                    "GlobalSettings.LFOF",
                    "GlobalSettings.LFOD",
                    "GlobalSettings.LFOW"
                    );
            }
            set
            {
                DeserializeProps(this, value,
                   nameof(tim.ALG),
                   nameof(tim.FB),
                   nameof(tim.AMS),
                   nameof(tim.PMS),
                    "GlobalSettings.EN",
                    "GlobalSettings.LFRQ",
                    "GlobalSettings.LFOF",
                    "GlobalSettings.LFOD",
                    "GlobalSettings.LFOW"
                   );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YM2151GeneralContainer(InstrumentBase inst, YM2151.YM2151Timbre tim, string name) : base(tim, name)
        {
            InitializeComponent();

            this.tim = tim;

            AddControl(new RegisterValue("ALG", tim.ALG, 0, 7));
            AddControl(new RegisterValue("FB", tim.FB, 0, 7));
            AddControl(new RegisterValue("AMS", tim.AMS, 0, 3));
            AddControl(new RegisterValue("PMS", tim.PMS, 0, 7));

            AddControl(new RegisterFlag("LFO", "GlobalSettings.EN", tim.GlobalSettings.EN != 0 ? true : false));
            AddControl(new RegisterValue("LFRQ", "GlobalSettings.LFRQ", tim.GlobalSettings.LFRQ == null ? -1 : tim.GlobalSettings.LFRQ.Value, 0, 255, true));
            AddControl(new RegisterValue("LFOF", "GlobalSettings.LFOF", tim.GlobalSettings.LFOF == null ? -1 : tim.GlobalSettings.LFOF.Value, 0, 1, true)).ValueChanged += LFOF_ValueChanged;
            AddControl(new RegisterValue("AMD/PMD", "GlobalSettings.LFOD", tim.GlobalSettings.LFOD == null ? -1 : tim.GlobalSettings.LFOD.Value, 0, 127, true));
            AddControl(new RegisterValue("LFOW", "GlobalSettings.LFOW", tim.GlobalSettings.LFOW == null ? -1 : tim.GlobalSettings.LFOW.Value, 0, 3, true));

            AddControl(new RegisterAlg4OpImg((RegisterValue)GetControl("ALG")));
            AddControl(new RegisterSpace("spc") { Dock = DockStyle.Right });
            AddControl(new RegisterOscViewer(inst));

            LFOF_ValueChanged(GetControl("GlobalSettings.LFOF"), null);
        }

        private void LFOF_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            RegisterValue amsf = (RegisterValue)sender;
            RegisterValue ams = (RegisterValue)GetControl("GlobalSettings.LFOD");
            if (amsf.Value == 1)
            {
                ams.Label = "PMD";
            }
            else
            {
                ams.Label = "AMD";
            }
        }
    }
}
