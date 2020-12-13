using MetroFramework.Forms;
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
    public partial class FormTextEditor : FormBase
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
                textBoxWsgDataText.ClearUndo();
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
