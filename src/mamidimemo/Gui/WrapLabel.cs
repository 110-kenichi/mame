using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Gui
{
    public class WrapLabel : Label
    {
        private Timer timer;

        /// <summary>
        /// 
        /// </summary>
        public WrapLabel()
        {
            timer = new Timer();
            timer.Interval = 200;
            timer.Tick += Timer_Tick;

            AutoEllipsis = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer?.Dispose();
                timer = null;
            }
            base.Dispose(disposing);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Text.Length > 1)
            {
                string t1 = Text.Substring(1);
                string t0 = Text.Substring(0, 1);
                Text = t1 + t0;
            }
        }


        private string originalText;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void SetText(String text)
        {
            originalText = text;

            updateWrapStat();
        }

        private void updateWrapStat()
        {
            if (Width < TextRenderer.MeasureText(originalText, Font, Size).Width)
            {
                Text = originalText + " *** ";
                timer.Enabled = true;
            }
            else
            {
                Text = originalText;
                timer.Enabled = false;
            }
        }

    }
}
