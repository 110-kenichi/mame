using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.Gui
{
    public partial class FormWsgEditor : FormBase
    {

        public event EventHandler ValueChanged;

        public int WsgBitWide
        {
            get
            {
                return graphControl.WsgBitWide;
            }
            set
            {
                graphControl.WsgBitWide = value;
                f_WsgMaxValue = (1 << WsgBitWide) - 1;
            }
        }

        private int f_WsgMaxValue = 15;

        public byte[] ByteWsgData
        {
            get
            {
                return graphControl.ResultOfWsgData;
            }
            set
            {
                graphControl.ResultOfWsgData = value;
            }
        }

        public sbyte[] SbyteWsgData
        {
            get
            {
                int max = ((1 << WsgBitWide) - 1) / 2;
                sbyte[] data = new sbyte[graphControl.ResultOfWsgData.Length];
                for (int i = 0; i < data.Length; i++)
                    data[i] = (sbyte)(graphControl.ResultOfWsgData[i] - max - 1);
                return data;
            }
            set
            {
                byte[] td = new byte[value.Length];
                for (int i = 0; i < value.Length; i++)
                    td[i] = (byte)((int)value[i] + (f_WsgMaxValue / 2) + 1);

                graphControl.ResultOfWsgData = td;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FormWsgEditor()
        {
            InitializeComponent();

            Size = Settings.Default.WsgEdSize;
            graphControl.Editor = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Settings.Default.WsgEdSize = Size;
        }

        /// <summary>
        /// 
        /// </summary>
        private class GraphControl : UserControl
        {
            public FormWsgEditor Editor
            {
                get;
                set;
            }

            private byte[] f_ResultOfWsgData;

            [Browsable(false)]
            public byte[] ResultOfWsgData
            {
                get
                {
                    return f_ResultOfWsgData;
                }
                set
                {
                    if (value == null)
                        return;

                    f_ResultOfWsgData = new byte[value.Length];
                    Array.Copy(value, f_ResultOfWsgData, value.Length);
                    wsgLen = f_ResultOfWsgData.Length;

                    Editor.updateText();
                }
            }

            private int wsgLen;

            private int f_WsgBitWide = 4;

            public int WsgBitWide
            {
                get
                {
                    return f_WsgBitWide;
                }
                set
                {
                    f_WsgBitWide = value;
                    f_WsgMaxValue = (1 << WsgBitWide) - 1;
                }
            }

            private int f_WsgMaxValue = 15;

            /// <summary>
            /// 
            /// </summary>
            public GraphControl()
            {
                DoubleBuffered = true;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="e"></param>
            protected override void OnPaint(PaintEventArgs e)
            {
                // Call the OnPaint method of the base class.  
                base.OnPaint(e);

                Graphics g = e.Graphics;
                Size sz = this.ClientSize;
                Size dotSz = Size.Empty;
                dotSz = new Size(sz.Width / (wsgLen == 0 ? 1 : wsgLen), sz.Height / (f_WsgMaxValue + 1));

                //fill bg
                using (SolidBrush sb = new SolidBrush(Color.Black))
                    g.FillRectangle(sb, e.ClipRectangle);
                //draw grid
                using (Pen pen = new Pen(Color.DarkGray))
                {
                    using (Pen pen2 = new Pen(Color.White, 3))
                    {
                        for (int x = 0; x < wsgLen; x++)
                        {
                            Pen dp = x + 1 == wsgLen / 2 || x + 1 == wsgLen ? pen2 : pen;
                            g.DrawLine(dp, (x * dotSz.Width) + dotSz.Width - 1, 0, (x * dotSz.Width) + dotSz.Width - 1, sz.Height);
                        }
                        for (int y = 0; y < (f_WsgMaxValue + 1); y++)
                        {
                            Pen dp = y + 1 == (f_WsgMaxValue + 1) / 2 || y == f_WsgMaxValue ? pen2 : pen;
                            g.DrawLine(dp,
                                0, sz.Height - ((y * dotSz.Height) + dotSz.Height),
                                sz.Width, sz.Height - ((y * dotSz.Height) + dotSz.Height));
                        }
                    }
                }
                //draw dot
                using (SolidBrush sb = new SolidBrush(Color.Green))
                {
                    for (int x = 0; x < wsgLen; x++)
                    {
                        if (ResultOfWsgData[x] <= f_WsgMaxValue / 2)
                        {
                            g.FillRectangle(sb,
                                x * dotSz.Width,
                                sz.Height - ((f_WsgMaxValue / 2 + 1) * dotSz.Height) + 1,
                                dotSz.Width - 1, (f_WsgMaxValue / 2 - ResultOfWsgData[x] + 1) * dotSz.Height);
                        }
                        else
                        {
                            g.FillRectangle(sb,
                                x * dotSz.Width,
                                sz.Height - (ResultOfWsgData[x] * dotSz.Height) - dotSz.Height + 1,
                                dotSz.Width - 1, (ResultOfWsgData[x] - f_WsgMaxValue / 2) * dotSz.Height);
                        }
                    }
                }
            }

            protected override void OnClientSizeChanged(EventArgs e)
            {
                base.OnClientSizeChanged(e);
                Invalidate();
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                processMouse(e);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                processMouse(e);
            }

            private void processMouse(MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                    return;

                int len = ResultOfWsgData.Length;

                Size sz = this.ClientSize;
                Point pt = e.Location;
                Size dotSz = new Size(sz.Width / len, sz.Height / (f_WsgMaxValue + 1));

                Point wxv = new Point(pt.X / dotSz.Width, (sz.Height - pt.Y) / dotSz.Height);

                if (0 <= wxv.X & wxv.X < len && 0 <= wxv.Y && wxv.Y <= f_WsgMaxValue)
                {
                    if (ResultOfWsgData[wxv.X] != (byte)wxv.Y)
                    {
                        if (ResultOfWsgData[wxv.X] <= f_WsgMaxValue / 2)
                        {
                            Invalidate(new Rectangle(
                                wxv.X * dotSz.Width,
                                sz.Height - ((f_WsgMaxValue / 2 + 1) * dotSz.Height) + 1,
                                dotSz.Width - 1, (f_WsgMaxValue / 2 - ResultOfWsgData[wxv.X] + 1) * dotSz.Height));
                        }
                        else
                        {
                            Invalidate(new Rectangle(
                                wxv.X * dotSz.Width,
                                sz.Height - (ResultOfWsgData[wxv.X] * dotSz.Height) - dotSz.Height + 1,
                                dotSz.Width - 1, (ResultOfWsgData[wxv.X] - f_WsgMaxValue / 2) * dotSz.Height));
                        }

                        ResultOfWsgData[wxv.X] = (byte)wxv.Y;

                        if (ResultOfWsgData[wxv.X] <= f_WsgMaxValue / 2)
                        {
                            Invalidate(new Rectangle(
                                wxv.X * dotSz.Width,
                                sz.Height - ((f_WsgMaxValue / 2 + 1) * dotSz.Height) + 1,
                                dotSz.Width - 1, (f_WsgMaxValue / 2 - ResultOfWsgData[wxv.X] + 1) * dotSz.Height));
                        }
                        else
                        {
                            Invalidate(new Rectangle(
                                wxv.X * dotSz.Width,
                                sz.Height - (ResultOfWsgData[wxv.X] * dotSz.Height) - dotSz.Height + 1,
                                dotSz.Width - 1, (ResultOfWsgData[wxv.X] - f_WsgMaxValue / 2) * dotSz.Height));
                        }

                        Editor.updateText();

                        Editor.ValueChanged?.Invoke(Editor, EventArgs.Empty);
                    }
                }
            }
        }

        private bool suspendWsgDataTextChange = false;

        private void textBoxWsgDataText_TextChanged(object sender, EventArgs e)
        {
            if (suspendWsgDataTextChange)
                return;

            string[] vals = textBoxWsgDataText.Text.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            List<byte> vs = new List<byte>();
            foreach (var val in vals)
            {
                byte v = 0;
                if (checkBoxHex.Checked)
                {
                    if (byte.TryParse(val, NumberStyles.HexNumber, null, out v))
                        vs.Add(v);
                }
                else
                {
                    if (byte.TryParse(val, out v))
                        vs.Add(v);
                }
            }

            for (int i = 0; i < Math.Min(ByteWsgData.Length, vs.Count); i++)
                ByteWsgData[i] = vs[i] > f_WsgMaxValue ? (byte)f_WsgMaxValue : vs[i];

            graphControl.Invalidate();
        }

        private void updateText()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < graphControl.ResultOfWsgData.Length; i++)
            {
                if (sb.Length != 0)
                    sb.Append(' ');
                if (checkBoxHex.Checked)
                    sb.Append(graphControl.ResultOfWsgData[i].ToString("X2"));
                else
                    sb.Append(graphControl.ResultOfWsgData[i].ToString((IFormatProvider)null));
            }
            try
            {
                suspendWsgDataTextChange = true;
                textBoxWsgDataText.Text = sb.ToString();
            }
            finally
            {
                suspendWsgDataTextChange = false;
            }
        }

        private void metroButtonRand1_Click(object sender, EventArgs e)
        {
            var rand = new Random(DateTime.Now.GetHashCode());
            for (int i = 0; i < ByteWsgData.Length; i++)
                ByteWsgData[i] = (byte)rand.Next(f_WsgMaxValue);

            updateText();

            graphControl.Invalidate();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void metroButtonRandom2_Click(object sender, EventArgs e)
        {
            var rand = new Random(DateTime.Now.GetHashCode());

            ByteWsgData[0] = (byte)rand.Next(f_WsgMaxValue);
            int len = rand.Next(ByteWsgData.Length / 2 - 1) + 1;
            int sign = rand.Next(1);
            if (sign == 0)
                sign = -1;
            int delta = rand.Next(f_WsgMaxValue / 2);
            int sy = ByteWsgData[0];
            int height = 0;
            if (sign > 0)
                height = rand.Next(f_WsgMaxValue - sy) + 1;
            else
                height = rand.Next(sy) + 1;
            for (int i = 1; i < ByteWsgData.Length; i++)
            {
                int data = ByteWsgData[i - 1] + sign * delta;
                if (data > f_WsgMaxValue || (sign > 0 && Math.Abs(data - sy) > height))
                {
                    data = Math.Min(f_WsgMaxValue, sy + height);
                    delta = rand.Next(f_WsgMaxValue / 2);
                    len = rand.Next(ByteWsgData.Length / 2 - 1) + 1;
                    sy = data;
                    sign = -1;
                    height = rand.Next(sy) + 1;
                }
                else if (data < 0 || (sign < 0 && Math.Abs(data - sy) > height))
                {
                    data = Math.Max(0, sy - height);
                    delta = rand.Next(f_WsgMaxValue / 2);
                    len = rand.Next(ByteWsgData.Length / 2 - 1) + 1;
                    sy = data;
                    sign = 1;
                    height = rand.Next(f_WsgMaxValue - sy) + 1;
                }

                ByteWsgData[i] = (byte)data;

                len--;
                if (len == 0)
                    len = rand.Next(ByteWsgData.Length - 1) + 1;
            }

            updateText();

            graphControl.Invalidate();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void metroButtonFir1_Click(object sender, EventArgs e)
        {
            ByteWsgData = applyFIR(ByteWsgData, metroTextBoxFirWeight.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            updateText();

            graphControl.Invalidate();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        //http://home.a00.itscom.net/hatada/asp/fir.html
        private byte[] applyFIR(byte[] wsgData, string[] weights)
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
            List<byte> orgData = new List<byte>(wsgData);
            //orgData.Add(0);
            List<byte> firData = new List<byte>();
            for (int k = 0; k < orgData.Count; k++)
            {
                input[m % wei.Length] = (double)orgData[k] - (f_WsgMaxValue / 2);
                double val = 0.0;
                for (int n = 0; n < wei.Length; n++)
                    val += input[(m - n) % wei.Length] * wei[n];
                byte fdata = (byte)Math.Round(val + (f_WsgMaxValue / 2));
                if (fdata > f_WsgMaxValue)
                    fdata = (byte)f_WsgMaxValue;
                else if (fdata < 0)
                    fdata = 0;
                firData.Add(fdata);
                m++;
            }
            //firData.RemoveAt(0);
            return firData.ToArray();
        }

        //https://mathwords.net/dataseikika
        private void metroButtonMax_Click(object sender, EventArgs e)
        {
            int max = 0;
            int min = f_WsgMaxValue;
            for (int i = 0; i < ByteWsgData.Length; i++)
            {
                max = Math.Max(max, ByteWsgData[i]);
                min = Math.Min(min, ByteWsgData[i]);
            }
            for (int i = 0; i < ByteWsgData.Length; i++)
            {
                ByteWsgData[i] = (byte)Math.Round((((double)ByteWsgData[i] - (double)min) / ((double)max - (double)min)) * (double)f_WsgMaxValue);
            }

            updateText();

            graphControl.Invalidate();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void metroButtonSin_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ByteWsgData.Length; i++)
                ByteWsgData[i] = (byte)((f_WsgMaxValue / 2) + (Math.Sin(((double)i / (double)ByteWsgData.Length) * 2d * Math.PI) * (f_WsgMaxValue / 2d)));

            updateText();

            graphControl.Invalidate();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void metroButtonSaw_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ByteWsgData.Length; i++)
                ByteWsgData[i] = (byte)((f_WsgMaxValue * i) / (ByteWsgData.Length - 1));

            updateText();

            graphControl.Invalidate();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void metroButtonSq_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ByteWsgData.Length / 2; i++)
                ByteWsgData[i] = (byte)0;
            for (int i = ByteWsgData.Length / 2; i < ByteWsgData.Length; i++)
                ByteWsgData[i] = (byte)f_WsgMaxValue;

            updateText();

            graphControl.Invalidate();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void metroButtonTri_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ByteWsgData.Length / 4; i++)
            {
                ByteWsgData[i] = (byte)((f_WsgMaxValue / 2) + ((i * (f_WsgMaxValue / 2)) / (ByteWsgData.Length / 4)));
                ByteWsgData[i + (ByteWsgData.Length / 4)] = (byte)(f_WsgMaxValue - ((i * (f_WsgMaxValue / 2)) / (ByteWsgData.Length / 4)));
                ByteWsgData[i + (2 * (ByteWsgData.Length / 4))] = (byte)((f_WsgMaxValue / 2) - ((i * (f_WsgMaxValue / 2)) / (ByteWsgData.Length / 4)));
                ByteWsgData[i + (3 * (ByteWsgData.Length / 4))] = (byte)(((i * (f_WsgMaxValue / 2)) / (ByteWsgData.Length / 4)));
            }

            updateText();

            graphControl.Invalidate();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private void checkBoxHex_CheckedChanged(object sender, EventArgs e)
        {
            updateText();
        }
    }
}
