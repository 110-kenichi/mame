// copyright-holders:K.Ito
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    /// <summary>
    /// A custom windows control to display text vertically
    /// </summary>
    public class VerticalLabel : System.Windows.Forms.Control
    {
        private string labelText;
        private DrawMode _dm = DrawMode.BottomUp;
        System.Drawing.Text.TextRenderingHint _renderMode = System.Drawing.Text.TextRenderingHint.SystemDefault;

        private System.ComponentModel.Container components = new System.ComponentModel.Container();

        /// <summary>
        /// VerticalLabel constructor
        /// </summary>
        public VerticalLabel()
        {
            InitializeComponent();

            DoubleBuffered = true;
        }

        /// <summary>
        /// Dispose override method
        /// </summary>
        /// <param name="disposing">boolean parameter</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!((components == null)))
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(24, 100);
        }

        /// <summary>
        /// OnPaint override. This is where the text is rendered vertically.
        /// </summary>
        /// <param name="e">PaintEventArgs</param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            float vlblControlWidth;
            float vlblControlHeight;
            float vlblTransformX;
            float vlblTransformY;

            Color controlBackColor = BackColor;
            Pen labelBorderPen;
            SolidBrush labelBackColorBrush;

            labelBorderPen = new Pen(controlBackColor, 0);
            labelBackColorBrush = new SolidBrush(controlBackColor);

            SolidBrush labelForeColorBrush = new SolidBrush(base.ForeColor);
            base.OnPaint(e);
            vlblControlWidth = this.Size.Width;
            vlblControlHeight = this.Size.Height;
            e.Graphics.DrawRectangle(labelBorderPen, 0, 0, vlblControlWidth, vlblControlHeight);
            e.Graphics.FillRectangle(labelBackColorBrush, 0, 0, vlblControlWidth, vlblControlHeight);
            e.Graphics.TextRenderingHint = this._renderMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            if (this.TextDrawMode == DrawMode.BottomUp)
            {
                vlblTransformX = 0;
                vlblTransformY = vlblControlHeight;
                e.Graphics.TranslateTransform(vlblTransformX, vlblTransformY);
                e.Graphics.RotateTransform(270);
                e.Graphics.DrawString(labelText, Font, labelForeColorBrush, 0, 0);
            }
            else
            {
                vlblTransformX = vlblControlWidth;
                vlblTransformY = vlblControlHeight;
                e.Graphics.TranslateTransform(vlblControlWidth, 0);
                e.Graphics.RotateTransform(90);
                e.Graphics.DrawString(labelText, Font, labelForeColorBrush, 0, 0, StringFormat.GenericTypographic);
            }
        }
      
        /// <summary>
        /// Graphics rendering mode. Supprot for antialiasing.
        /// </summary>
        [Category("Properties"), Description("Rendering mode.")]
        public System.Drawing.Text.TextRenderingHint RenderingMode
        {
            get { return _renderMode; }
            set { _renderMode = value; }
        }

        /// <summary>
        /// The text to be displayed in the control
        /// </summary>
        [Category("VerticalLabel"), Description("Text is displayed vertically in container.")]
        public override string Text
        {
            get
            {
                return labelText;
            }
            set
            {
                labelText = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Properties"), Description("Whether the text will be drawn from Bottom or from Top.")]
        public DrawMode TextDrawMode
        {
            get { return _dm; }
            set { _dm = value; }
        }

    }

    /// <summary>
    /// Text Drawing Mode
    /// </summary>
    public enum DrawMode
    {
        /// <summary>
        /// Text is drawn from bottom - up
        /// </summary>
        BottomUp = 1,
        /// <summary>
        /// Text is drawn from top to bottom
        /// </summary>
        TopBottom
    }
}
