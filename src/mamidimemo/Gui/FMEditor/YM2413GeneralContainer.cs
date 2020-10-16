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
    public partial class YM2413GeneralContainer : RegisterContainerBase
    {
        private YM2413.YM2413Timbre tim;

        /// <summary>
        /// 
        /// </summary>
        public override string SerializeData
        {
            get
            {
                return SerializeProps(this,
                    nameof(tim.FB),
                    nameof(tim.SUS));
            }
            set
            {
                DeserializeProps(this, value,
                    nameof(tim.FB),
                    nameof(tim.SUS));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public YM2413GeneralContainer(InstrumentBase inst, YM2413.YM2413Timbre tim, string name) : base(tim, name)
        {
            InitializeComponent();

            this.tim = tim;

            AddControl(new RegisterValue("FB", tim.FB, 0, 7));
            AddControl(new RegisterValue("SUS", tim.SUS, 0, 1));
            AddControl(new RegisterOscViewer(inst));
        }

    }
}
