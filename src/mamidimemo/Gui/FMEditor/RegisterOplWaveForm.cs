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
    public partial class RegisterOplWaveForm : RegisterBase
    {
        private RegisterValue ws;

        /// <summary>
        /// 
        /// </summary>
        public RegisterOplWaveForm(RegisterValue ws) : base("WSForm")
        {
            DoubleBuffered = true;

            InitializeComponent();
            Dock = DockStyle.Left;

            this.ws = ws;
            this.BackgroundImage = Resources.WS0;
            ws.ValueChanged += Alg_ValueChanged;
            Alg_ValueChanged(null, null);
        }

        private void Alg_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (ws.Value)
            {
                case 0:
                    this.BackgroundImage = Resources.WS0;
                    break;
                case 1:
                    this.BackgroundImage = Resources.WS1;
                    break;
                case 2:
                    this.BackgroundImage = Resources.WS2;
                    break;
                case 3:
                    this.BackgroundImage = Resources.WS3;
                    break;
                case 4:
                    this.BackgroundImage = Resources.WS4;
                    break;
                case 5:
                    this.BackgroundImage = Resources.WS5;
                    break;
                case 6:
                    this.BackgroundImage = Resources.WS6;
                    break;
                case 7:
                    this.BackgroundImage = Resources.WS7;
                    break;
            }
        }
    }
}
