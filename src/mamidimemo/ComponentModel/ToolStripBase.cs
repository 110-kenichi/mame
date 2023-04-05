using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public partial class ToolStripBase : ToolStrip
    {
        /// <summary>
        /// 
        /// </summary>
        public ToolStripBase()
        {

        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            using (var g = this.CreateGraphics())
            {
                var scale = Math.Max(g.DpiX, g.DpiY) / 96.0;
                var newScale = ((int)Math.Floor(scale * 100) / 50 * 50) / 100.0;
                if (newScale > 1)
                {
                    var newWidth = (int)(ImageScalingSize.Width * newScale);
                    var newHeight = (int)(ImageScalingSize.Height * newScale);
                    ImageScalingSize = new Size(newWidth, newHeight);
                    AutoSize = false; //because sometime it is needed
                }
            }
        }

        private bool clickThrough = true;

        /// <summary>
        /// Gets or sets whether the ToolStripEx honors item clicks when its containing form does
        /// not have input focus.
        /// </summary>
        /// <remarks>
        /// Default value is false, which is the same behavior provided by the base ToolStrip class.
        /// </remarks>
        [DefaultValue(true)]
        public bool ClickThrough
        {
            get
            {
                return this.clickThrough;
            }
            set
            {
                this.clickThrough = value;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (this.clickThrough && m.Msg == NativeConstants.WM_MOUSEACTIVATE && m.Result == (IntPtr)NativeConstants.MA_ACTIVATEANDEAT)
                m.Result = (IntPtr)NativeConstants.MA_ACTIVATE;
        }

        private sealed class NativeConstants
        {
            internal const uint WM_MOUSEACTIVATE = 0x21;
            internal const uint MA_ACTIVATE = 1;
            internal const uint MA_ACTIVATEANDEAT = 2;
            internal const uint MA_NOACTIVATE = 3;
            internal const uint MA_NOACTIVATEANDEAT = 4;
        }
    }


}
