using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class GraphicEqulizer : UserControl
    {
        private GraphicEqualizerSettings f_Settings;

        /// <summary>
        /// 
        /// </summary>
        public GraphicEqualizerSettings Settings
        {
            get
            {
                return f_Settings;
            }
            set
            {
                f_Settings = value;

                try
                {
                    ignoreValueChanged = true;

                    trackBar100.Value = getTrackValue(Settings.Gain100Hz);
                    trackBar200.Value = getTrackValue(Settings.Gain200Hz);
                    trackBar400.Value = getTrackValue(Settings.Gain400Hz);
                    trackBar800.Value = getTrackValue(Settings.Gain800Hz);
                    trackBar1600.Value = getTrackValue(Settings.Gain1600Hz);
                    trackBar3200.Value = getTrackValue(Settings.Gain3200Hz);
                    trackBar6400.Value = getTrackValue(Settings.Gain6400Hz);
                    trackBar12800.Value = getTrackValue(Settings.Gain12800Hz);
                    trackBar25600.Value = getTrackValue(Settings.Gain25600Hz);
                }
                finally
                {
                    ignoreValueChanged = false;
                }
            }
        }

        private int getTrackValue(double value)
        {
            if (value > 30)
                return 30;
            else if (value < -30)
                return -30;
            return (int)Math.Round(value);
        }

        /// <summary>
        /// 
        /// </summary>
        public GraphicEqulizer()
        {
            InitializeComponent();
        }

        private bool ignoreValueChanged;

        private void trackBar100_ValueChanged(object sender, EventArgs e)
        {
            if (ignoreValueChanged)
                return;

            if (sender == trackBar100)
            {
                Settings.Gain100Hz = trackBar100.Value;
            }
            else if (sender == trackBar200)
            {
                Settings.Gain200Hz = trackBar200.Value;
            }
            else if (sender == trackBar400)
            {
                Settings.Gain400Hz = trackBar400.Value;
            }
            else if (sender == trackBar800)
            {
                Settings.Gain800Hz = trackBar800.Value;
            }
            else if (sender == trackBar1600)
            {
                Settings.Gain1600Hz = trackBar1600.Value;
            }
            else if (sender == trackBar3200)
            {
                Settings.Gain3200Hz = trackBar3200.Value;
            }
            else if (sender == trackBar6400)
            {
                Settings.Gain6400Hz = trackBar6400.Value;
            }
            else if (sender == trackBar12800)
            {
                Settings.Gain12800Hz = trackBar12800.Value;
            }
            else if (sender == trackBar25600)
            {
                Settings.Gain25600Hz = trackBar25600.Value;
            }
        }
    }
}
