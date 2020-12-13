// copyright-holders:K.Ito
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormAbout : FormBase
    {
        public FormAbout()
        {
            InitializeComponent();

            labelVer.Text = string.Format(Program.FILE_COPYRIGHT, Program.FILE_VERSION);

            ImageUtility.AdjustControlImagesDpiScale(this);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/110-kenichi/mame");
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SNQ9JE3JAQMNQ");
        }
    }
}
