// copyright-holders:K.Ito
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.Instruments;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class EnvelopeUITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public EnvelopeUITypeEditor(Type type) : base(type)
        {
        }

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

            if (provider != null)
                editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (editorService == null)
                return value;

            EnvelopeEditorAttribute env = (EnvelopeEditorAttribute)context.PropertyDescriptor.Attributes[typeof(EnvelopeEditorAttribute)];

            // CurrencyValueEditorForm を使用したプロパティエディタの表示
            using (FormEnvelopeEditor frm = new FormEnvelopeEditor((string)value, env.Min, env.Max))
            {
                frm.ValueChanged += Frm_ValueChangedByte;
                frm.Tag = context;

                DialogResult dr = editorService.ShowDialog(frm);
                if (dr == DialogResult.OK)
                    value = frm.EnvelopeValuesText;
                else if (value != null)
                    value = String.Copy((string)value);

                return value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frm_ValueChangedByte(object sender, EventArgs e)
        {
            FormEnvelopeEditor editor = (FormEnvelopeEditor)sender;
            ITypeDescriptorContext ctx = (ITypeDescriptorContext)editor.Tag;
            if (ctx != null)
            {
                var value = editor.EnvelopeValuesText;
                try
                {
                    //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                    ctx.PropertyDescriptor.SetValue(ctx.Instance, value);
                }
                finally
                {
                    //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                }
            }

        }
    }
}
