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
                    updateText();
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

            private void updateText()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < ResultOfWsgData.Length; i++)
                {
                    if (sb.Length != 0)
                        sb.Append(' ');
                    sb.Append(ResultOfWsgData[i].ToString((IFormatProvider)null));
                }
                try
                {
                    ((FormWsgEditor)Parent).suspendWsgDataTextChange = true;
                    ((FormWsgEditor)Parent).textBoxWsgDataText.Text = sb.ToString();
                }
                finally
                {
                    ((FormWsgEditor)Parent).suspendWsgDataTextChange = false;
                }
            }

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

                        updateText();

                        ((FormWsgEditor)Parent).ValueChanged?.Invoke(Parent, EventArgs.Empty);
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
                if (byte.TryParse(val, out v))
                    vs.Add(v);
            }

            for (int i = 0; i < Math.Min(ByteWsgData.Length, vs.Count); i++)
                ByteWsgData[i] = vs[i] > f_WsgMaxValue ? (byte)f_WsgMaxValue : vs[i];

            graphControl.Invalidate();
        }
    }
}
