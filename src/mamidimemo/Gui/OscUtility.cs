using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    public static class OscUtility
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="inst"></param>
        /// <param name="target"></param>
        public static void DrawOsc(PaintEventArgs e, InstrumentBase inst, Control target)
        {
            int[][] data = InstrumentManager.GetLastOutputBuffer(inst);

            e.Graphics.Clear(target.BackColor);

            int w = target.ClientSize.Width;
            int h = target.ClientSize.Height;
            if (data != null)
            {
                using (Pen p = new Pen(Color.DarkGreen))
                {
                    int max = h * 4;
                    for (int ch = 0; ch < 2; ch++)
                    {
                        if (data[ch] != null)
                        {
                            for (int i = 0; i < data[ch].Length; i++)
                            {
                                int dt = data[ch][i];
                                max = Math.Max(Math.Abs(dt), max);
                            }
                        }
                    }
                    max += max / 10;

                    if (data[0] != null)
                        drawCore(e, p, data[0], w / 2 - 1, h / 2, 0, max);
                    if (data[1] != null)
                        drawCore(e, p, data[1], w / 2 - 1, h / 2, w / 2 + 1, max);
                }
            }
            e.Graphics.DrawLine(SystemPens.Control, w / 2 - 1, 0, w / 2 - 1, h);
            e.Graphics.DrawLine(SystemPens.Control, w / 2, 0, w / 2, h);
        }

        private static void drawCore(PaintEventArgs e, Pen p, int[] data, int w, int h, int ox, int max)
        {
            int len = data.Length;
            List<Point> pts = new List<Point>();
            for (int i = 0; i < w; i++)
            {
                int y = data[(int)(((double)i / w) * len)];
                y = -(int)((y / (double)max) * h);
                y += h;
                int lx = i - 1;
                if (lx < 0)
                    lx = 0;
                pts.Add(new Point(ox + i, y));
            }
            e.Graphics.DrawLines(p, pts.ToArray());
        }
    }
}
