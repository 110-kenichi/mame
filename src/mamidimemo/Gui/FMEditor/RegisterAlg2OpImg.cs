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
    public partial class RegisterAlg2OpImg : RegisterBase
    {
        private RegisterValue alg;

        /// <summary>
        /// 
        /// </summary>
        public RegisterAlg2OpImg(RegisterValue alg) : base("Alg2OpImg")
        {
            DoubleBuffered = true;

            InitializeComponent();
            Dock = DockStyle.Right;

            this.alg = alg;
            this.BackgroundImage = Resources.ALG9;
            alg.ValueChanged += Alg_ValueChanged;
            Alg_ValueChanged(null, null);
        }

        private void Alg_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (alg.Value)
            {
                case 0:
                    this.BackgroundImage = Resources.ALG9;
                    break;
                case 1:
                    this.BackgroundImage = Resources.ALG10;
                    break;
            }
        }
    }
}
