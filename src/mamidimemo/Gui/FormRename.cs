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
    public partial class FormRename : FormBase
    {
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
        public string InputText
        {
            get
            {
                return metroTextBoxText.Text;
            }
            set
            {
                metroTextBoxText.Text = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormRename()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            metroTextBoxText.Focus();
            base.OnShown(e);
        }
    }
}
