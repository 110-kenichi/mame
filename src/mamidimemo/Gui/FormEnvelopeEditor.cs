using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Midi;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormEnvelopeEditor : FormBase
    {

        public event EventHandler ValueChanged;

        /// <summary>
        /// 
        /// </summary>
        public FormEnvelopeEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public FormEnvelopeEditor(string value, int min, int max)
        {
            InitializeComponent();

            //Loop
            chart1.Series["SeriesLoop"].Points.Add(new DataPoint(0, new double[] { min, max }));
            chart1.Series["SeriesLoop"]["PixelPointWidth"] = "10";

            //Release
            chart1.Series["SeriesRelease"].Points.Add(new DataPoint(0, new double[] { min, max }));
            chart1.Series["SeriesRelease"]["PixelPointWidth"] = "10";

            chart1.ChartAreas[0].AxisY.Minimum = min;
            chart1.ChartAreas[0].AxisY.Maximum = max;

            updateValue(value);
            suspendTextChange = true;
            textBoxEnvText.Text = EnvelopeValuesText;
            suspendTextChange = false;

            updateRepeatBarColor();
            updateReleaseBarColor();

            Size = Settings.Default.EnvEdSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.EnvEdSize = Size;
        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!e.Button.HasFlag(MouseButtons.Left))
                return;

            if (Control.ModifierKeys == Keys.None)
                setValuePoint(e);
            else if (Control.ModifierKeys == Keys.Shift)
                setLoopPoint(e);
            else if (Control.ModifierKeys == Keys.Control)
                setReleasePoint(e);
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!e.Button.HasFlag(MouseButtons.Left))
                return;

            if (Control.ModifierKeys == Keys.None)
                setValuePoint(e);
            else if (Control.ModifierKeys == Keys.Shift)
                setLoopPoint(e);
            else if (Control.ModifierKeys == Keys.Control)
                setReleasePoint(e);
        }

        private void setValuePoint(MouseEventArgs e)
        {
            ChartArea ca = chart1.ChartAreas[0];
            Series s = chart1.Series["SeriesValues"];

            try
            {
                var xv = ca.AxisX.PixelPositionToValue(e.X);
                var yv = ca.AxisY.PixelPositionToValue(e.Y);

                DataPoint pPrev = s.Points.Select(x => x)
                         .Where(x => x.XValue <= xv)
                         .DefaultIfEmpty(s.Points.First()).Last();
                DataPoint pNext = s.Points.Select(x => x)
                         .Where(x => xv <= x.XValue)
                         .DefaultIfEmpty(s.Points.Last()).First();

                DataPoint pt;
                if (xv - pPrev.XValue <= pNext.XValue - xv)
                    pt = pPrev;
                else
                    pt = pNext;

                if (chart1.ChartAreas[0].AxisY.Minimum <= yv && yv <= chart1.ChartAreas[0].AxisY.Maximum)
                {
                    pt.YValues[0] = Math.Round(yv);
                    chart1.Refresh();
                    updateText();
                }
                else
                {
                    if (yv < chart1.ChartAreas[0].AxisY.Minimum)
                    {
                        pt.YValues[0] = chart1.ChartAreas[0].AxisY.Minimum;
                        chart1.Refresh();
                        updateText();
                    }
                    else if (yv > chart1.ChartAreas[0].AxisY.Maximum)
                    {
                        pt.YValues[0] = chart1.ChartAreas[0].AxisY.Maximum;
                        chart1.Refresh();
                        updateText();
                    }
                }
            }
            catch
            {

            }
        }


        private void setLoopPoint(MouseEventArgs e)
        {
            ChartArea ca = chart1.ChartAreas[0];
            Series sv = chart1.Series["SeriesValues"];
            Series sl = chart1.Series["SeriesLoop"];
            Series sr = chart1.Series["SeriesRelease"];

            try
            {
                var xv = Math.Round(ca.AxisX.PixelPositionToValue(e.X));

                var lpt = sl.Points[0];
                var rpt = sr.Points[0];

                if (0 <= xv && sv.Points.Count >= 0 && xv < sv.Points[sv.Points.Count - 1].XValue &&
                    (metroToggleRelease.Checked ? xv < rpt.XValue : true))
                    lpt.XValue = Math.Round(xv);

                chart1.Refresh();
                updateText();
            }
            catch
            {

            }
        }


        private void setReleasePoint(MouseEventArgs e)
        {
            ChartArea ca = chart1.ChartAreas[0];
            Series sv = chart1.Series["SeriesValues"];
            Series sl = chart1.Series["SeriesLoop"];
            Series sr = chart1.Series["SeriesRelease"];

            try
            {
                var xv = Math.Round(ca.AxisX.PixelPositionToValue(e.X));

                var lpt = sl.Points[0];
                var rpt = sr.Points[0];

                if (0 <= xv && sv.Points.Count >= 0 && xv <= sv.Points[sv.Points.Count - 1].XValue + 1 &&
                    (metroToggleRelease.Checked ? lpt.XValue < xv : true))
                    rpt.XValue = xv;

                chart1.Refresh();
                updateText();
            }
            catch
            {

            }
        }

        public int[] EnvelopesNums { get; set; } = new int[] { };

        public int EnvelopesRepeatPoint { get; set; } = -1;

        public int EnvelopesReleasePoint { get; set; } = -1;


        private void updateText()
        {
            Series sv = chart1.Series["SeriesValues"];
            Series sl = chart1.Series["SeriesLoop"];
            Series sr = chart1.Series["SeriesRelease"];

            var lpt = sl.Points[0];
            var rpt = sr.Points[0];

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= sv.Points.Count; i++)
            {
                if (sb.Length != 0)
                    sb.Append(' ');

                if ((int)lpt.XValue == i && metroToggleRepeat.Checked)
                    sb.Append("| ");
                if ((int)rpt.XValue == i && metroToggleRelease.Checked)
                    sb.Append("/ ");
                if(i < sv.Points.Count)
                    sb.Append(((int)sv.Points[i].YValues[0]).ToString((IFormatProvider)null));
            }

            if (metroToggleRepeat.Checked)
                EnvelopesRepeatPoint = (int)lpt.XValue;
            else
                EnvelopesRepeatPoint = -1;
            if (metroToggleRelease.Checked)
                EnvelopesReleasePoint = (int)rpt.XValue;
            else
                EnvelopesReleasePoint = -1;

            EnvelopesNums = new int[] { };
            List<int> vs = new List<int>();
            for (int i = 0; i < sv.Points.Count; i++)
                vs.Add((int)sv.Points[i].YValues[0]);
            EnvelopesNums = vs.ToArray();

            suspendTextChange = true;
            f_EnvelopeValuesText = sb.ToString();
            textBoxEnvText.Text = f_EnvelopeValuesText;
            suspendTextChange = false;

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool suspendTextChange = false;

        private string f_EnvelopeValuesText;

        public string EnvelopeValuesText
        {
            get
            {
                return f_EnvelopeValuesText;
            }
        }

        private void textBoxText_TextChanged(object sender, EventArgs e)
        {
            if (suspendTextChange)
                return;

            updateValue(textBoxEnvText.Text);
        }

        private void updateValue(string value)
        {
            Series sv = chart1.Series["SeriesValues"];
            Series sl = chart1.Series["SeriesLoop"];
            Series sr = chart1.Series["SeriesRelease"];
            var lpt = sl.Points[0];
            var rpt = sr.Points[0];

            if (f_EnvelopeValuesText != value)
            {
                EnvelopesRepeatPoint = -1;
                EnvelopesReleasePoint = -1;
                if (value == null)
                {
                    EnvelopesNums = new int[] { };
                    f_EnvelopeValuesText = string.Empty;
                    return;
                }
                f_EnvelopeValuesText = value;
                string[] vals = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> vs = new List<int>();
                for (int i = 0; i < vals.Length; i++)
                {
                    string val = vals[i];
                    if (val.Equals("|", StringComparison.Ordinal))
                        EnvelopesRepeatPoint = vs.Count;
                    else if (val.Equals("/", StringComparison.Ordinal))
                        EnvelopesReleasePoint = vs.Count;
                    else
                    {
                        int v;
                        if (int.TryParse(val, out v))
                        {
                            if (v < (int)chart1.ChartAreas[0].AxisY.Minimum)
                                v = (int)chart1.ChartAreas[0].AxisY.Minimum;
                            else if (v > (int)chart1.ChartAreas[0].AxisY.Maximum)
                                v = (int)chart1.ChartAreas[0].AxisY.Maximum;
                            vs.Add(v);
                        }
                    }
                }
                EnvelopesNums = vs.ToArray();
                sv.Points.Clear();
                for (int i = 0; i < EnvelopesNums.Length; i++)
                    sv.Points.Add(new DataPoint(i, EnvelopesNums[i]));

                suspendMetroToggleEvent = true;
                if (EnvelopesRepeatPoint >= 0)
                {
                    metroToggleRepeat.Checked = true;
                    lpt.XValue = EnvelopesRepeatPoint;
                }
                else
                {
                    metroToggleRepeat.Checked = false;
                }
                if (EnvelopesReleasePoint >= 0)
                {
                    metroToggleRelease.Checked = true;
                    rpt.XValue = EnvelopesReleasePoint;
                }
                else
                {
                    metroToggleRelease.Checked = false;
                }
                suspendMetroToggleEvent = false;

                suspendNumericUpDownNum = true;
                numericUpDownNum.Value = EnvelopesNums.Length;
                suspendNumericUpDownNum = false;

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i <= EnvelopesNums.Length; i++)
                {
                    if (sb.Length != 0)
                        sb.Append(' ');
                    if (EnvelopesRepeatPoint == i)
                        sb.Append("| ");
                    if (EnvelopesReleasePoint == i)
                        sb.Append("/ ");
                    if(i < EnvelopesNums.Length)
                        sb.Append(EnvelopesNums[i].ToString((IFormatProvider)null));
                }
                f_EnvelopeValuesText = sb.ToString();

                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool suspendMetroToggleEvent;

        private void metroToggleRepeat_CheckedChanged(object sender, EventArgs e)
        {
            if (suspendMetroToggleEvent)
                return;

            updateRepeatBarColor();

            Series sv = chart1.Series["SeriesValues"];
            Series sl = chart1.Series["SeriesLoop"];
            Series sr = chart1.Series["SeriesRelease"];

            validateRepeatPoint(sv, sl, sr);
            updateText();
        }

        private void updateRepeatBarColor()
        {
            chart1.Series["SeriesLoop"].Color = metroToggleRepeat.Checked ? Color.CornflowerBlue : Color.LightBlue;
        }

        private void validateRepeatPoint(Series sv, Series sl, Series sr)
        {
            try
            {
                var lpt = sl.Points[0];
                var rpt = sr.Points[0];

                bool update = false;
                if (sv.Points.Count == 0)
                {
                    lpt.XValue = 0;
                    update = true;
                }
                else
                {
                    if (rpt.XValue <= lpt.XValue)
                    {
                        lpt.XValue = rpt.XValue - 1;
                        update = true;
                    }
                    if (lpt.XValue < 0)
                    {
                        lpt.XValue = 0;
                        update = true;
                    }
                    if (lpt.XValue >= sv.Points[sv.Points.Count - 1].XValue || lpt.XValue >= rpt.XValue)
                    {
                        lpt.XValue = Math.Min(sv.Points[sv.Points.Count - 1].XValue, rpt.XValue);
                        update = true;
                    }
                }
                if (update)
                {
                    chart1.Refresh();
                    updateText();
                }
            }
            catch
            {

            }
        }

        private void metroToggleRelease_CheckedChanged(object sender, EventArgs e)
        {
            if (suspendMetroToggleEvent)
                return;

            updateReleaseBarColor();

            Series sv = chart1.Series["SeriesValues"];
            Series sl = chart1.Series["SeriesLoop"];
            Series sr = chart1.Series["SeriesRelease"];

            validateReleasePoint(sv, sl, sr);
            updateText();
        }

        private void updateReleaseBarColor()
        {
            chart1.Series["SeriesRelease"].Color = metroToggleRelease.Checked ? Color.DarkOrange : Color.Linen;
        }

        private void validateReleasePoint(Series sv, Series sl, Series sr)
        {
            try
            {
                var lpt = sl.Points[0];
                var rpt = sr.Points[0];

                bool update = false;
                if (sv.Points.Count == 0)
                {
                    rpt.XValue = 0;
                    update = true;
                }
                else
                {
                    if (rpt.XValue <= lpt.XValue)
                    {
                        rpt.XValue = lpt.XValue + 1;
                        update = true;
                    }
                    if (rpt.XValue >= sv.Points[sv.Points.Count - 1].XValue)
                    {
                        rpt.XValue = sv.Points[sv.Points.Count - 1].XValue;
                        update = true;
                    }
                }
                if (update)
                {
                    chart1.Refresh();
                    updateText();
                }
            }
            catch
            {

            }
        }

        private bool suspendNumericUpDownNum;

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (suspendNumericUpDownNum)
                return;

            ChartArea ca = chart1.ChartAreas[0];
            Series sv = chart1.Series["SeriesValues"];
            Series sl = chart1.Series["SeriesLoop"];
            Series sr = chart1.Series["SeriesRelease"];

            int num = (int)numericUpDownNum.Value;
            int cnt = sv.Points.Count;
            if (num < cnt)
            {
                for (int i = num; i < cnt; i++)
                    sv.Points.RemoveAt(sv.Points.Count - 1);

                validateReleasePoint(sv, sl, sr);
                validateRepeatPoint(sv, sl, sr);
            }
            else if (num > cnt)
            {
                int v = 0;
                if (v < chart1.ChartAreas[0].AxisY.Minimum)
                    v = (int)chart1.ChartAreas[0].AxisY.Minimum;

                for (int i = cnt; i < num; i++)
                    sv.Points.Add(new DataPoint(i, v));

                validateReleasePoint(sv, sl, sr);
                validateRepeatPoint(sv, sl, sr);
            }
            chart1.Refresh();
            updateText();
        }

        private void metroButtonRand1_Click(object sender, EventArgs e)
        {
            ChartArea ca = chart1.ChartAreas[0];
            Series s = chart1.Series["SeriesValues"];

            var rand = new Random(DateTime.Now.GetHashCode());

            try
            {
                foreach (var pt in s.Points)
                {
                    pt.YValues[0] = rand.Next((int)ca.AxisY.Minimum, (int)ca.AxisY.Maximum);
                }
            }
            catch
            {

            }

            chart1.Refresh();

            updateText();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void metroButtonRandom2_Click(object sender, EventArgs e)
        {
            ChartArea ca = chart1.ChartAreas[0];
            Series s = chart1.Series["SeriesValues"];

            var rand = new Random(DateTime.Now.GetHashCode());

            int max = (int)(ca.AxisY.Maximum - ca.AxisY.Minimum);
            int offset = (int)(ca.AxisY.Minimum);

            try
            {
                s.Points[0].YValues[0] = rand.Next(max) + offset;
                int len = rand.Next(s.Points.Count / 2 - 1) + 1;
                int sign = rand.Next(1);
                if (sign == 0)
                    sign = -1;
                int delta = rand.Next(max / 2);
                int sy = (int)s.Points[0].YValues[0] - offset;
                int height = 0;
                if (sign > 0)
                    height = rand.Next(max - sy) + 1;
                else
                    height = rand.Next(sy) + 1;
                for (int i = 1; i < s.Points.Count; i++)
                {
                    int data = ((int)s.Points[i - 1].YValues[0] - offset) + sign * delta;
                    if (data > max || (sign > 0 && Math.Abs(data - sy) > height))
                    {
                        data = Math.Min(max, sy + height);
                        delta = rand.Next(max / 2);
                        len = rand.Next(s.Points.Count / 2 - 1) + 1;
                        sy = data;
                        sign = -1;
                        height = rand.Next(sy) + 1;
                    }
                    else if (data < 0 || (sign < 0 && Math.Abs(data - sy) > height))
                    {
                        data = Math.Max(0, sy - height);
                        delta = rand.Next(max / 2);
                        len = rand.Next(s.Points.Count / 2 - 1) + 1;
                        sy = data;
                        sign = 1;
                        height = rand.Next(max - sy) + 1;
                    }

                    s.Points[i].YValues[0] = data + offset;

                    len--;
                    if (len == 0)
                        len = rand.Next(s.Points.Count - 1) + 1;
                }
            }
            catch
            {

            }

            chart1.Refresh();

            updateText();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        //https://mathwords.net/dataseikika
        private void metroButtonMax_Click(object sender, EventArgs e)
        {
            ChartArea ca = chart1.ChartAreas[0];
            Series s = chart1.Series["SeriesValues"];

            var max = ca.AxisY.Minimum;
            var min = ca.AxisY.Maximum;
            for (int i = 0; i < s.Points.Count; i++)
            {
                max = Math.Max(max, s.Points[i].YValues[0]);
                min = Math.Min(min, s.Points[i].YValues[0]);
            }
            for (int i = 0; i < s.Points.Count; i++)
            {
                s.Points[i].YValues[0] =
                    Math.Round(
                        ((
                        (s.Points[i].YValues[0] - min) /
                        (max - min)
                        ) * (ca.AxisY.Maximum - ca.AxisY.Minimum)) + ca.AxisY.Minimum
                        );
            }

            chart1.Refresh();

            updateText();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void metroButtonFir1_Click(object sender, EventArgs e)
        {
            ChartArea ca = chart1.ChartAreas[0];
            Series s = chart1.Series["SeriesValues"];

            List<double> vals = new List<double>();
            for (int i = 0; i < s.Points.Count; i++)
                vals.Add(s.Points[i].YValues[0]);

            var fvals = applyFIR(vals.ToArray(), metroTextBoxFirWeight.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            for (int i = 0; i < s.Points.Count; i++)
                s.Points[i].YValues[0] = fvals[i];

            chart1.Refresh();

            updateText();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }


        //http://home.a00.itscom.net/hatada/asp/fir.html
        private double[] applyFIR(double[] wsgData, string[] weights)
        {
            double[] wei = new double[weights.Length];
            double[] input = new double[weights.Length];
            double sumWei = 0.0;
            for (int n = 0; n < wei.Length; n++)
            {
                Double.TryParse(weights[n], out wei[n]);
                sumWei += wei[n];
            }
            for (int n = 0; n < wei.Length; n++)
            {
                wei[n] /= sumWei;
            }
            int m = 0;
            for (; m < wei.Length - 1; m++)
            {
                input[m] = 0.0;     // 無音とみなす。
            }
            List<double> orgData = new List<double>(wsgData);
            //orgData.Add(0);
            List<double> firData = new List<double>();
            for (int k = 0; k < orgData.Count; k++)
            {
                input[m % wei.Length] = (double)orgData[k];
                double val = 0.0;
                for (int n = 0; n < wei.Length; n++)
                    val += input[(m - n) % wei.Length] * wei[n];
                var fdata = Math.Round(val);
                firData.Add(fdata);
                m++;
            }
            //firData.RemoveAt(0);
            return firData.ToArray();
        }
    }

}
