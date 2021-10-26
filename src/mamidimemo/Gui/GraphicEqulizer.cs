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

                    trackBar31.Value = getTrackValue(Settings.Gain31Hz);
                    trackBar62.Value = getTrackValue(Settings.Gain62Hz);
                    trackBar125.Value = getTrackValue(Settings.Gain125Hz);
                    trackBar250.Value = getTrackValue(Settings.Gain250Hz);
                    trackBar500.Value = getTrackValue(Settings.Gain500Hz);
                    trackBar1000.Value = getTrackValue(Settings.Gain1000Hz);
                    trackBar2000.Value = getTrackValue(Settings.Gain2000Hz);
                    trackBar4000.Value = getTrackValue(Settings.Gain4000Hz);
                    trackBar8000.Value = getTrackValue(Settings.Gain8000Hz);
                    trackBar16000.Value = getTrackValue(Settings.Gain16000Hz);

                    trackBar31W.Value = getTrackWidthValue(Settings.BandWidth31Hz);
                    trackBar62W.Value = getTrackWidthValue(Settings.BandWidth62Hz);
                    trackBar125W.Value = getTrackWidthValue(Settings.BandWidth125Hz);
                    trackBar250W.Value = getTrackWidthValue(Settings.BandWidth250Hz);
                    trackBar500W.Value = getTrackWidthValue(Settings.BandWidth500Hz);
                    trackBar1000W.Value = getTrackWidthValue(Settings.BandWidth1000Hz);
                    trackBar2000W.Value = getTrackWidthValue(Settings.BandWidth2000Hz);
                    trackBar4000W.Value = getTrackWidthValue(Settings.BandWidth4000Hz);
                    trackBar8000W.Value = getTrackWidthValue(Settings.BandWidth8000Hz);
                    trackBar16000W.Value = getTrackWidthValue(Settings.BandWidth16000Hz);
                }
                finally
                {
                    ignoreValueChanged = false;
                }
            }
        }

        private int getTrackValue(double value)
        {
            if (value > 15)
                return 15;
            else if (value < -15)
                return -15;
            return (int)Math.Round(value);
        }

        private int getTrackWidthValue(double value)
        {
            if (value > 2000)
                return 2000;
            else if (value < 0.01)
                return 1;
            return (int)Math.Round(value * 100);
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

            Settings.Enable = true;

            if (sender == trackBar31)
            {
                Settings.Gain31Hz = trackBar31.Value;
            }
            else if (sender == trackBar62)
            {
                Settings.Gain62Hz = trackBar62.Value;
            }
            else if (sender == trackBar125)
            {
                Settings.Gain125Hz = trackBar125.Value;
            }
            else if (sender == trackBar250)
            {
                Settings.Gain250Hz = trackBar250.Value;
            }
            else if (sender == trackBar500)
            {
                Settings.Gain500Hz = trackBar500.Value;
            }
            else if (sender == trackBar1000)
            {
                Settings.Gain1000Hz = trackBar1000.Value;
            }
            else if (sender == trackBar2000)
            {
                Settings.Gain2000Hz = trackBar2000.Value;
            }
            else if (sender == trackBar4000)
            {
                Settings.Gain4000Hz = trackBar4000.Value;
            }
            else if (sender == trackBar8000)
            {
                Settings.Gain8000Hz = trackBar8000.Value;
            }
            else if (sender == trackBar16000)
            {
                Settings.Gain16000Hz = trackBar16000.Value;
            }
            else if (sender == trackBar31W)
            {
                Settings.BandWidth31Hz = trackBar31W.Value / 100d;
            }
            else if (sender == trackBar62W)
            {
                Settings.BandWidth62Hz = trackBar62W.Value / 100d;
            }
            else if (sender == trackBar125W)
            {
                Settings.BandWidth125Hz = trackBar125W.Value / 100d;
            }
            else if (sender == trackBar250W)
            {
                Settings.BandWidth250Hz = trackBar250W.Value / 100d;
            }
            else if (sender == trackBar500W)
            {
                Settings.BandWidth500Hz = trackBar500W.Value / 100d;
            }
            else if (sender == trackBar1000W)
            {
                Settings.BandWidth1000Hz = trackBar1000W.Value / 100d;
            }
            else if (sender == trackBar2000W)
            {
                Settings.BandWidth2000Hz = trackBar2000W.Value / 100d;
            }
            else if (sender == trackBar4000W)
            {
                Settings.BandWidth4000Hz = trackBar4000W.Value / 100d;
            }
            else if (sender == trackBar8000W)
            {
                Settings.BandWidth8000Hz = trackBar8000W.Value / 100d;
            }
            else if (sender == trackBar16000W)
            {
                Settings.BandWidth16000Hz = trackBar16000W.Value / 100d;
            }
        }
    }
}
