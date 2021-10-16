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
using zanac.MAmidiMEmo.Instruments.Chips;
using static zanac.MAmidiMEmo.Instruments.Chips.TMS5220;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class LpcUITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public LpcUITypeEditor(Type type) : base(type)
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

            bool singleSel = true;
            TMS5220Timbre tim = context.Instance as TMS5220Timbre;
            TMS5220Timbre[] tims = value as TMS5220Timbre[];
            if (tims != null)
            {
                tim = tims[0];
                singleSel = false;
            }

            TMS5220 inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                inst = InstrumentManager.FindParentInstrument(InstrumentType.TMS5220, tim) as TMS5220;
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitReadLock();
            }

            using (FormLpcEditor frm = new FormLpcEditor(inst, tim, singleSel))
            {
                frm.Tag = context;

                frm.LpcData = (byte[])value;
                frm.ValueChanged += Frm_ValueChangedByte;

                DialogResult dr = frm.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    value = frm.LpcData;
                }
                else if (value != null)
                {
                    value = ((byte[])value).Clone();
                }
                else
                {
                    value = new byte[] { };
                }
            }
            return value;
        }


        private void Frm_ValueChangedByte(object sender, EventArgs e)
        {
            FormLpcEditor editor = (FormLpcEditor)sender;
            ITypeDescriptorContext ctx = (ITypeDescriptorContext)editor.Tag;
            if (ctx != null)
            {
                var value = editor.LpcData;
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
