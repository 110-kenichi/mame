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
    public partial class ScrollPanel : Panel
    {
        public ScrollPanel()
        {
            InitializeComponent();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {

            if (((Control.ModifierKeys & Keys.Alt) != Keys.Alt) && ((Control.ModifierKeys & Keys.Shift) != Keys.Shift))
            {
                ((HandledMouseEventArgs)e).Handled = true;
            }
            else
            {
                // do other staff
            }
        }
    }
}
