// copyright-holders:K.Ito
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class RegisterEnvForm : RegisterBase
    {
        private RegisterValue ar;

        private RegisterValue tl;

        private RegisterValue dr;

        private bool invertDr;

        private RegisterValue sl;

        private bool invertSl;

        private RegisterValue sr;

        private RegisterValue rr;

        /// <summary>
        /// 
        /// </summary>
        public RegisterEnvForm(RegisterValue ar, RegisterValue tl, RegisterValue dr, bool invertDr, RegisterValue sl, bool invertSl, RegisterValue sr, RegisterValue rr) : base("AlgEnvForm")
        {
            DoubleBuffered = true;

            InitializeComponent();
            Dock = DockStyle.Right;

            this.ar = ar;
            ar.ValueChanged += Ar_ValueChanged;

            if (tl != null)
            {
                this.tl = tl;
                tl.ValueChanged += Ar_ValueChanged;
            }

            this.invertDr = invertDr;
            this.dr = dr;
            dr.ValueChanged += Ar_ValueChanged;

            this.invertSl = invertSl;
            this.sl = sl;
            sl.ValueChanged += Ar_ValueChanged;

            if (sr != null)
            {
                this.sr = sr;
                sr.ValueChanged += Ar_ValueChanged;
            }

            this.rr = rr;
            rr.ValueChanged += Ar_ValueChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ar_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            using (Pen gp = new Pen(Color.Green, 3))
            {
                using (Pen bp = new Pen(Color.LightBlue, 3))
                {
                    var rect = ClientRectangle;

                    Point p1 = new Point(0, rect.Height - 1);

                    //AR
                    Point p2 = new Point((rect.Width / 4) * (ar.Maximum - ar.Value) / ar.Maximum,
                        tl != null ? rect.Height * tl.Value / tl.Maximum : 0);
                    g.DrawLine(gp, p1, p2);

                    //DR
                    Point p3 = new Point(
                        p2.X +
                        (!invertDr ?
                        ((rect.Width / 4) * (dr.Maximum - dr.Value) / dr.Maximum) :
                        ((rect.Width / 4) * dr.Value / dr.Maximum)),
                        p2.Y + (rect.Height - p2.Y) * (invertSl ? sl.Maximum - sl.Value : sl.Value) / sl.Maximum);
                    g.DrawLine(gp, p2, p3);

                    //SR
                    if (sr != null && sr.Value >= 0 )
                    {
                        Point p4 = new Point(p3.X + ((rect.Width / 4) * (sr.Maximum - sr.Value) / sr.Maximum), rect.Height);
                        g.DrawLine(gp, p3, p4);
                    }

                    //RR
                    Point p5 = new Point(p3.X + ((rect.Width / 4) * (rr.Maximum - rr.Value) / rr.Maximum), rect.Height);
                    g.DrawLine(bp, p3, p5);
                }
            }
        }
    }
}
