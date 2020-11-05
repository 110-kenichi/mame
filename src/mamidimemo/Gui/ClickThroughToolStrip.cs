// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Gui
{
    public class ClickThroughToolStrip : ToolStrip
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x21 &&
                m.Result == (IntPtr)0x2)
            {
                m.Result = (IntPtr)0x3;
            }
        }
    }
}
