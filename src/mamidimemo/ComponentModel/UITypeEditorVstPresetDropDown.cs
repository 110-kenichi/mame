using Jacobi.Vst.Interop.Host;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Vst;

namespace zanac.MAmidiMEmo.ComponentModel
{

    public class UITypeEditorVstPresetDropDown : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private IWindowsFormsEditorService service;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            if (service == null)
                return value;
            var list = new System.Windows.Forms.ListBox();
            list.Click += List_Click;
            lock (InstrumentBase.VstPluginContextLockObject)
            {
                VstPlugin inst = (VstPlugin)context.Instance;
                var ctx = inst.PluginContext.Context;
                if (ctx != null)
                {
                    for (int i = 0; i < ctx.PluginInfo.ProgramCount; i++)
                        list.Items.Add(ctx.PluginCommandStub.GetProgramNameIndexed(i));
                }
            }
            list.Height = 200;
            service.DropDownControl(list);
            return (list.SelectedItem != null) ? list.SelectedItem : value;
        }

        private void List_Click(object sender, EventArgs e)
        {
            service?.CloseDropDown();
        }
    }

}
