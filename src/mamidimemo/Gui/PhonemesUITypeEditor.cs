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
using static zanac.MAmidiMEmo.Instruments.Chips.SAM;
using static zanac.MAmidiMEmo.Instruments.Chips.SP0256;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class PhonemesUITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public PhonemesUITypeEditor(Type type) : base(type)
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
            SAMTimbre tim = context.Instance as SAMTimbre;
            SAMTimbre[] tims = value as SAMTimbre[];
            if (tims != null)
            {
                tim = tims[0];
                singleSel = false;
            }

            SAM inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                inst = InstrumentManager.FindParentInstrument(InstrumentType.SAM, tim) as SAM;
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitReadLock();
            }

            using (var frm = new FormPhonemesEditor(inst, tim, singleSel))
            {
                frm.Tag = context;
                frm.Allophones = (string)value;
                frm.ValueChanged += (s, e) =>
                {
                    try
                    {
                        //InstrumentManager.ExclusiveLockObject.EnterWriteLock();

                        context.PropertyDescriptor.SetValue(context.Instance, frm.Allophones);
                    }
                    finally
                    {
                        //InstrumentManager.ExclusiveLockObject.ExitWriteLock();
                    }
                };

                DialogResult dr = frm.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    value = frm.Allophones;
                }
                else
                {
                    value = ((string)value + " ").Clone();
                }
            }
            return value;
        }

        private void Frm_TestPlayed(object sender, EventArgs e)
        {

        }

    }
}
