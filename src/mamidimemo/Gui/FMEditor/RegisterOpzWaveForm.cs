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
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class RegisterOpzWaveForm : RegisterBase
    {
        private RegisterValue oscw;

        /// <summary>
        /// 
        /// </summary>
        public RegisterOpzWaveForm(RegisterValue oscw) : base("WSForm")
        {
            DoubleBuffered = true;

            InitializeComponent();
            Dock = DockStyle.Left;

            this.oscw = oscw;
            this.BackgroundImage = Resources.WS0;
            this.oscw.ValueChanged += Alg_ValueChanged;
            Alg_ValueChanged(null, null);
        }

        private void Alg_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (oscw.Value)
            {
                case 0:
                    this.BackgroundImage = Resources.WS0;
                    break;
                case 1:
                    this.BackgroundImage = Resources.WS8;
                    break;
                case 2:
                    this.BackgroundImage = Resources.WS1;
                    break;
                case 3:
                    this.BackgroundImage = Resources.WS9;
                    break;
                case 4:
                    this.BackgroundImage = Resources.WS4;
                    break;
                case 5:
                    this.BackgroundImage = Resources.WS10;
                    break;
                case 6:
                    this.BackgroundImage = Resources.WS5;
                    break;
                case 7:
                    this.BackgroundImage = Resources.WS11;
                    break;
            }
        }
    }
}
