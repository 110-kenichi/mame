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
        public YM2413GeneralContainer(YM2413.YM2413Timbre tim, string name) : base(tim, name)
        {
            InitializeComponent();

            this.tim = tim;

            AddControl(new RegisterValue("FB", tim.FB, 0, 7));
            AddControl(new RegisterValue("SUS", tim.SUS, 0, 1));
        }

    }
}
