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
using static zanac.MAmidiMEmo.Instruments.Chips.SCC1;
using static zanac.MAmidiMEmo.Instruments.Chips.SP0256;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class SccMorphUITypeEditor : UITypeEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public SccMorphUITypeEditor()
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

            //bool singleSel = true;
            SCC1Timbre tim = context.Instance as SCC1Timbre;
            //SCC1Timbre[] tims = value as SCC1Timbre[];
            //if (tims != null)
            //{
            //    tim = tims[0];
            //    singleSel = false;
            //}

            if (tim == null)
                return value;

            SCC1 inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                inst = tim.Instrument as SCC1;
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitReadLock();
            }

            using (FormSccMorphEditor frm = new FormSccMorphEditor(inst, tim))
            {
                var sd = tim.SerializeData;
                DialogResult dr = frm.ShowDialog();
                if (dr != DialogResult.OK)
                    tim.SerializeData = sd;
            }
            return value;
        }

        private void Frm_TestPlayed(object sender, EventArgs e)
        {

        }

    }
}
