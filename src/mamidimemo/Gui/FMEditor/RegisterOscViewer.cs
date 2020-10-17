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
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    public partial class RegisterOscViewer : RegisterBase
    {
        private InstrumentBase inst;

        /// <summary>
        /// 
        /// </summary>
        public RegisterOscViewer(InstrumentBase inst) : base("OscViewer")
        {
            DoubleBuffered = true;

            InitializeComponent();
            Dock = DockStyle.Right;

            this.inst = inst;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            timer1.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            OscUtility.DrawOsc(e, inst, this);

            base.OnPaint(e);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
