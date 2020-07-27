using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.ComponentModel
{
    public class DoubleSlideEditor : System.Drawing.Design.UITypeEditor
    {
        private ToolTip toolTip = new ToolTip();

        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var service = provider.GetService
                (typeof(System.Windows.Forms.Design.IWindowsFormsEditorService))
                    as System.Windows.Forms.Design.IWindowsFormsEditorService;


            DoubleSlideParametersAttribute att = (DoubleSlideParametersAttribute)context.PropertyDescriptor.Attributes[typeof(DoubleSlideParametersAttribute)];

            DoubleTrackBar track = new DoubleTrackBar();
            track.ValueChanged += Track_ValueChanged;
            track.Precision = att.Precision;
            track.Maximum = att.SliderMax;
            track.Minimum = att.SliderMin;
            track.Width = 127;
            track.Height = 15;
            double freq = track.Maximum / 50d;
            if (freq < 1)
                freq = 1;
            track.TickFrequency = (int)freq;

            double result;
            if (double.TryParse(context.PropertyDescriptor.Converter.ConvertToString(value), out result))
                track.Value = result;

            if (att.SliderDynamicSetValue)
                track.Tag = context;
            service.DropDownControl(track);

            return context.PropertyDescriptor.Converter.ConvertFromString(track.Value.ToString());
        }

        private void Track_ValueChanged(object sender, EventArgs e)
        {
            DoubleTrackBar track = (DoubleTrackBar)sender;
            toolTip.SetToolTip(track, track.Value.ToString());
            ITypeDescriptorContext ctx = (ITypeDescriptorContext)track.Tag;
            if (ctx != null)
            {
                var val = ctx.PropertyDescriptor.Converter.ConvertFromString(track.Value.ToString());
                try
                {
                    InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                    ctx.PropertyDescriptor.SetValue(ctx.Instance, val);
                }
                finally
                {
                    InstrumentManager.ExclusiveLockObject.EnterWriteLock();
                }
            }
        }

        private class DoubleTrackBar : TrackBar
        {
            private double precision = 0.01f;

            public double Precision
            {
                get { return precision; }
                set
                {
                    precision = value;
                    // todo: update the 5 properties below
                }
            }
            public new double LargeChange
            { get { return base.LargeChange * precision; } set { base.LargeChange = (int)Math.Round(value / precision); } }
            public new double Maximum
            { get { return base.Maximum * precision; } set { base.Maximum = (int)Math.Round(value / precision); } }
            public new double Minimum
            { get { return base.Minimum * precision; } set { base.Minimum = (int)Math.Round(value / precision); } }
            public new double SmallChange
            { get { return base.SmallChange * precision; } set { base.SmallChange = (int)Math.Round(value / precision); } }
            public new double Value
            { get { return base.Value * precision; } set { base.Value = (int)Math.Round(value / precision); } }
        }
    }

}
