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
                    nameof(tim.FMS));
            }
            set
            {
                DeserializeProps(this, value,
                    nameof(tim.ALG),
                    nameof(tim.FB),
                    nameof(tim.AMS),
                    nameof(tim.FMS));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YM2610BGeneralContainer(YM2610B.YM2610BTimbre tim, string name) : base(tim, name)
        {
            InitializeComponent();

            this.tim = tim;

            AddControl(new RegisterValue("ALG", tim.ALG, 0, 7));
            AddControl(new RegisterValue("FB", tim.FB, 0, 7));
            AddControl(new RegisterValue("AMS", tim.AMS, 0, 3));
            AddControl(new RegisterValue("FMS", tim.FMS, 0, 7));
            AddControl(new RegisterAlg4OpImg((RegisterValue)GetControl("ALG")));
        }

    }
}
