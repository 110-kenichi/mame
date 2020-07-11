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
    public partial class FormTextEditor : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public string TextData
        {
            get
            {
                return textBoxWsgDataText.Text;
            }
            set
            {
                textBoxWsgDataText.Text = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormTextEditor()
        {
            InitializeComponent();
        }

    }
}
