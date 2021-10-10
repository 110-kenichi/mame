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
                    nameof(tim.AMSF),
                    nameof(tim.AMS),
                    nameof(tim.PMSF),
                    nameof(tim.PMS),

                    "GlobalSettings.EN",
                    "GlobalSettings.LFRQ",
                    "GlobalSettings.LFRQ2",
                    "GlobalSettings.LFOF",
                    "GlobalSettings.LFOD",
                    "GlobalSettings.LFOF2",
                    "GlobalSettings.LFOD2",
                    "GlobalSettings.LFOW",
                    "GlobalSettings.LFOW2",
                    "GlobalSettings.SYNC",
                    "GlobalSettings.SYNC1"
                    );
            }
            set
            {
                DeserializeProps(this, value,
                    nameof(tim.ALG),
                    nameof(tim.FB),
                    nameof(tim.AMSF),
                    nameof(tim.AMS),
                    nameof(tim.PMSF),
                    nameof(tim.PMS),

                    "GlobalSettings.EN",
                    "GlobalSettings.LFRQ",
                    "GlobalSettings.LFRQ2",
                    "GlobalSettings.LFOF",
                    "GlobalSettings.LFOD",
                    "GlobalSettings.LFOF2",
                    "GlobalSettings.LFOD2",
                    "GlobalSettings.LFOW",
                    "GlobalSettings.LFOW2",
                    "GlobalSettings.SYNC",
                    "GlobalSettings.SYNC1"
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
            AddControl(new RegisterFlag("AMSF", tim.AMSF != 0 ? true : false)).ValueChanged += AMSF_ValueChanged;
            AddControl(new RegisterValue("AMS", tim.AMS, 0, 3));
            AddControl(new RegisterFlag("PMSF", tim.PMSF != 0 ? true : false)).ValueChanged += PMSF_ValueChanged;
            AddControl(new RegisterValue("PMS", tim.PMS, 0, 7));

            AddControl(new RegisterFlag("LFO", "GlobalSettings.EN", tim.GlobalSettings.EN != 0 ? true : false));
            AddControl(new RegisterValue("LFRQ", "GlobalSettings.LFRQ", tim.GlobalSettings.LFRQ == null ? -1 : tim.GlobalSettings.LFRQ.Value, 0, 255, true));
            AddControl(new RegisterValue("LFRQ2", "GlobalSettings.LFRQ2", tim.GlobalSettings.LFRQ2 == null ? -1 : tim.GlobalSettings.LFRQ2.Value, 0, 255, true));
            AddControl(new RegisterValue("LFOF", "GlobalSettings.LFOF", tim.GlobalSettings.LFOF == null ? -1 : tim.GlobalSettings.LFOF.Value, 0, 1, true)).ValueChanged += LFOF_ValueChanged;
            AddControl(new RegisterValue("AMD/PMD", "GlobalSettings.LFOD", tim.GlobalSettings.LFOD == null ? -1 : tim.GlobalSettings.LFOD.Value, 0, 127, true));
            AddControl(new RegisterValue("LFOF2", "GlobalSettings.LFOF2", tim.GlobalSettings.LFOF2 == null ? -1 : tim.GlobalSettings.LFOF2.Value, 0, 1, true)).ValueChanged += LFOF2_ValueChanged;
            AddControl(new RegisterValue("AMD2/PMD2", "GlobalSettings.LFOD2", tim.GlobalSettings.LFOD2 == null ? -1 : tim.GlobalSettings.LFOD2.Value, 0, 127, true));
            AddControl(new RegisterValue("LFOW", "GlobalSettings.LFOW", tim.GlobalSettings.LFOW == null ? -1 : tim.GlobalSettings.LFOW.Value, 0, 3, true));
            AddControl(new RegisterValue("LFOW2", "GlobalSettings.LFOW2", tim.GlobalSettings.LFOW2 == null ? -1 : tim.GlobalSettings.LFOW2.Value, 0, 3, true));
            AddControl(new RegisterValue("SY", "GlobalSettings.SYNC", tim.GlobalSettings.SYNC == null ? -1 : tim.GlobalSettings.LFOF.Value, 0, 1, true));
            AddControl(new RegisterValue("SY2", "GlobalSettings.SYNC2", tim.GlobalSettings.SYNC2 == null ? -1 : tim.GlobalSettings.LFOF.Value, 0, 1, true));

            AddControl(new RegisterAlg4OpImg((RegisterValue)GetControl("ALG")));
            AddControl(new RegisterSpace("spc") { Dock = DockStyle.Right });
            AddControl(new RegisterOscViewer(inst));

            LFOF_ValueChanged(GetControl("GlobalSettings.LFOF"), null);
            LFOF2_ValueChanged(GetControl("GlobalSettings.LFOF2"), null);
            AMSF_ValueChanged(GetControl("AMSF"), null);
            PMSF_ValueChanged(GetControl("PMSF"), null);
        }

        private void LFOF2_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            RegisterValue amsf = (RegisterValue)sender;
            RegisterValue ams = (RegisterValue)GetControl("GlobalSettings.LFOD2");
            if (amsf.Value == 1)
            {
                ams.Label = "PMD2";
            }
            else
            {
                ams.Label = "AMD2";
            }
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

        private void AMSF_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            RegisterFlag amsf = (RegisterFlag)sender;
            RegisterValue ams = (RegisterValue)GetControl("AMS");
            if (amsf.Value == false)
            {
                ams.Label = "AMS";
            }
            else
            {
                ams.Label = "AMS2";
            }
        }

        private void PMSF_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            RegisterFlag amsf = (RegisterFlag)sender;
            RegisterValue ams = (RegisterValue)GetControl("PMS");
            if (amsf.Value == false)
            {
                ams.Label = "PMS";
            }
            else
            {
                ams.Label = "PMS2";
            }
        }
    }
}
