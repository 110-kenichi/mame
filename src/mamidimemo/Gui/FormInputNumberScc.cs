using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormInputNumberScc : FormBase
    {
        private static decimal lastValue = 1;

        /// <summary>
        /// 
        /// </summary>
        public string TitleText
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal InputValue
        {
            get
            {
                lastValue = numericUpDown.Value;
                return numericUpDown.Value;
            }
            set
            {
                lastValue = value;
                numericUpDown.Value = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal MinimumValue
        {
            get
            {
                return numericUpDown.Minimum;
            }
            set
            {
                numericUpDown.Minimum = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal MaximumValue
        {
            get
            {
                return numericUpDown.Maximum;
            }
            set
            {
                numericUpDown.Maximum = value;
            }
        }

        public bool MethodSimple
        {
            get
            {
                return radioButtonSimple.Checked;
            }
        }

        public bool MethodFft
        {
            get
            {
                return radioButtonFft.Checked;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public FormInputNumberScc()
        {
            InitializeComponent();

            radioButtonSimple.Checked = Settings.Default.SccInterporateMethodSimple;
            radioButtonFft.Checked = Settings.Default.SccInterporateMethodFft;
        }

        protected override void OnShown(EventArgs e)
        {
            numericUpDown.Focus();
            base.OnShown(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.SccInterporateMethodSimple = radioButtonSimple.Checked;
            Settings.Default.SccInterporateMethodFft = radioButtonFft.Checked;

            base.OnClosing(e);
        }
    }
}
