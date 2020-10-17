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
    public partial class RegisterAlg4OpImg : RegisterBase
    {
        private RegisterValue alg;

        /// <summary>
        /// 
        /// </summary>
        public RegisterAlg4OpImg(RegisterValue alg) : base("Alg4OpImg")
        {
            DoubleBuffered = true;

            InitializeComponent();
            Dock = DockStyle.Left;

            this.alg = alg;
            this.BackgroundImage = Resources.ALG1;
            alg.ValueChanged += Alg_ValueChanged;
            Alg_ValueChanged(null, null);
        }

        private void Alg_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (alg.Value)
            {
                case 0:
                    this.BackgroundImage = Resources.ALG1;
                    break;
                case 1:
                    this.BackgroundImage = Resources.ALG2;
                    break;
                case 2:
                    this.BackgroundImage = Resources.ALG3;
                    break;
                case 3:
                    this.BackgroundImage = Resources.ALG4;
                    break;
                case 4:
                    this.BackgroundImage = Resources.ALG5;
                    break;
                case 5:
                    this.BackgroundImage = Resources.ALG6;
                    break;
                case 6:
                    this.BackgroundImage = Resources.ALG7;
                    break;
                case 7:
                    this.BackgroundImage = Resources.ALG8;
                    break;
            }
        }
    }
}
