using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.VGMPlayer
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }


        protected override void OnShown(EventArgs e)
        {
            textBox1.Text = string.Format(textBox1.Text, Text);

            base.OnShown(e);
        }
    }
}
