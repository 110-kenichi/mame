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
    public class SlideEditor : System.Drawing.Design.UITypeEditor
    {
        private ToolTip toolTip = new ToolTip();

        private int? minValue;
        private int? maxValue;

        public SlideEditor()
        {
        }

        public SlideEditor(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var service = provider.GetService
                (typeof(System.Windows.Forms.Design.IWindowsFormsEditorService))
                    as System.Windows.Forms.Design.IWindowsFormsEditorService;


            SlideParametersAttribute att = (SlideParametersAttribute)context.PropertyDescriptor.Attributes[typeof(SlideParametersAttribute)];

            TrackBar track = new TrackBar();
            if (att != null)
            {
                track.Maximum = att.SliderMax;
                track.Minimum = att.SliderMin;
            }
            if(maxValue.HasValue)
                track.Maximum = maxValue.Value;
            if (minValue.HasValue)
                track.Minimum = minValue.Value;
            track.Width = 127;
            track.Height = 15;
            int freq = track.Maximum / 50;
            if (freq < 1)
                freq = 1;
            track.TickFrequency = freq;

            int result;
            if(int.TryParse(context.PropertyDescriptor.Converter.ConvertToString(value),out result))
                track.Value = result;

            if(att != null && att.SliderDynamicSetValue)
                track.Tag = context;

            track.ValueChanged += Track_ValueChanged;
            service.DropDownControl(track);

            if (valueChanged)
                return context.PropertyDescriptor.Converter.ConvertFromString(track.Value.ToString());
            else
                return value;
        }

        bool valueChanged;

        private void Track_ValueChanged(object sender, EventArgs e)
        {
            valueChanged = true;

            TrackBar track = (TrackBar)sender;
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
                    InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                }
            }

        }
    }
}
