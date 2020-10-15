using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class RegisterFlag : RegisterBase
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Value
        {
            get
            {
                return checkBox.Checked;
            }
            set
            {
                checkBox.Checked = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public RegisterFlag() : base(null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public RegisterFlag(string registerName, bool value) : base(registerName)
        {
            InitializeComponent();
            Dock = DockStyle.Left;

            ItemName = registerName;

            labelName.Text = registerName;

            checkBox.Checked = value;

            checkBox.CheckedChanged += (s, e) =>
            {
                OnValueChanged(new PropertyChangedEventArgs(registerName));
            };
        }
    }
}
