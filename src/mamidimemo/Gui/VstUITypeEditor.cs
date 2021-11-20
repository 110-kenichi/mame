// copyright-holders:K.Ito
using Jacobi.Vst.Interop.Host;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Vst;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class VstUITypeEditor : UITypeEditor
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="provider"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService = null;
            VstSettings vs = (VstSettings)value;

            if (provider != null)
                editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (editorService == null || vs == null)
                return value;

            VstPlugin plugin = (VstPlugin)context.Instance;

            if (!openedFrame.ContainsKey(plugin))
            {
                FormVstEditorFrame dlg = new FormVstEditorFrame();
                {
                    lock (InstrumentBase.VstPluginContextLockObject)
                    {
                        var ctx = vs.VstPluginContext;
                        if (ctx != null)
                            dlg.PluginCommandStub = ctx.Context.PluginCommandStub;
                    }

                    try
                    {
                        InstrumentManager.InstExclusiveLockObject.EnterReadLock();
                        InstrumentBase targetInst = null;
                        foreach (var inst in InstrumentManager.GetAllInstruments())
                        {
                            foreach (var i in inst.VSTPlugins)
                            {
                                if (i == plugin)
                                {
                                    targetInst = inst;
                                    break;
                                }
                            }
                            if (targetInst != null)
                                break;
                        }
                        dlg.FormClosed += Dlg_FormClosed;
                        plugin.PluginDisposing += Plugin_PluginDisposing;
                        openedFrame.Add(plugin, dlg);
                        dlg.Show(null, targetInst);
                    }
                    finally
                    {
                        InstrumentManager.InstExclusiveLockObject.ExitReadLock();
                    }
                }
            }
            else
            {
                openedFrame[plugin].BringToFront();
            }
            return value;
        }

        private void Plugin_PluginDisposing(object sender, EventArgs e)
        {
            var plugin = (VstPlugin)sender;
            if (openedFrame.ContainsKey(plugin))
            {
                var form = openedFrame[plugin];
                openedFrame.Remove(plugin);
                form.Close();
            }
        }

        private void Dlg_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            foreach (var plugin in openedFrame.Keys)
            {
                if (openedFrame[plugin] == sender)
                {
                    openedFrame.Remove(plugin);
                    break;
                }
            }
        }

        private static Dictionary<VstPlugin, FormVstEditorFrame> openedFrame = new Dictionary<VstPlugin, FormVstEditorFrame>();
    }
}
