// copyright-holders:K.Ito
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
    public partial class FormSplash : FormBase
    {
        public FormSplash()
        {
            DoubleBuffered = true;

            InitializeComponent();

            labelVer.Text = string.Format(Program.FILE_COPYRIGHT, Program.FILE_VERSION);

            if (!Program.IsVSTiMode())
                labelWarn.Visible = false;

            ImageUtility.AdjustControlImagesDpiScale(this);
        }

        private void FormSplash_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(SystemPens.WindowFrame, new Rectangle(0, 0, Width - 1, Height - 1));
        }

    }
}
