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
    public partial class RegisterBase : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public string ItemName
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> ValueChanged;

        /// <summary>
        /// 
        /// </summary>
        public RegisterBase() : this(null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public RegisterBase(string registerName)
        {
            InitializeComponent();

            ItemName = registerName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnValueChanged(PropertyChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

    }
}
