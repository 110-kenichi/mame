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
    public partial class YMF262GeneralContainer : RegisterContainerBase
    {
        private YMF262.YMF262Timbre tim;

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
                    nameof(tim.FB2)
                    );
            }
            set
            {
                DeserializeProps(this, value,
                    nameof(tim.ALG),
                    nameof(tim.FB),
                    nameof(tim.FB2)
                    );
            }
        }
        private RegisterValue fb2;

        /// <summary>
        /// 
        /// </summary>
        public YMF262GeneralContainer(InstrumentBase inst, YMF262.YMF262Timbre tim, string name) : base(tim, name)
        {
            InitializeComponent();

            this.tim = tim;

            AddControl(new RegisterValue("ALG", tim.ALG, 0, 5));
            AddControl(new RegisterValue("FB", tim.FB, 0, 7));
            fb2 = new RegisterValue("FB2", tim.FB2, 0, 7);
            AddControl(fb2);
            AddControl(new RegisterAlg2OpImg((RegisterValue)GetControl("ALG")));
            AddControl(new RegisterSpace("spc") { Dock = DockStyle.Right });
            AddControl(new RegisterOscViewer(inst));

            fb2.Enabled = (tim.ALG >= 2);
        }

        protected override void OnValueChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ALG":
                    fb2.Enabled = (tim.ALG >= 2);
                    break;
            }

            base.OnValueChanged(sender, e);
        }
    }
}
