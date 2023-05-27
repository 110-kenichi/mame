using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormInputNumber : FormBase
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

        /// <summary>
        /// 
        /// </summary>
        public FormInputNumber()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            numericUpDown.Focus();
            base.OnShown(e);
        }
    }
}
