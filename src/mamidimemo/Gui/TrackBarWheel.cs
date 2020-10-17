// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Gui
{
    public class TrackBarWheel : TrackBar
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int delta = this.SmallChange;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                delta = this.LargeChange;

            int val = Value;
            if (e.Delta < 0)
                val -= delta;
            else if (0 < e.Delta)
                val += delta;

            if (val < Minimum)
                val = Minimum;
            else if (val > Maximum)
                val = Maximum;

            Value = val;

            HandledMouseEventArgs handledEventArgs = (HandledMouseEventArgs)e;
            handledEventArgs.Handled = true;
        }
    }
}
