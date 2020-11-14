// copyright-holders:K.Ito
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
using zanac.MAmidiMEmo.Gui.FMEditor;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Chips;
using static zanac.MAmidiMEmo.Instruments.Chips.YM3812;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    /// <summary>
    /// 
    /// </summary>
    public class YM3812UITypeEditor : UITypeEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public YM3812UITypeEditor(Type type)
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

            YM3812Timbre tim = context.Instance as YM3812Timbre;
            YM3812 inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                foreach (var i in InstrumentManager.GetInstruments((int)InstrumentType.YM3812 + 1))
                {
                    Parallel.ForEach(i.BaseTimbres, t =>
                    {
                        if (t == tim)
                            inst = (YM3812)i;
                        if (inst != null)
                            return;
                    });
                }
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitReadLock();
            }

            if (inst != null)
            {
                using (FormYM3812Editor ed = new FormYM3812Editor(inst, tim))
                {
                    var mmlValueGeneral = SimpleSerializer.SerializeProps(tim,
                        nameof(tim.ALG),
                        nameof(tim.FB),
                        "GlobalSettings.EN",
                        "GlobalSettings.AMD",
                        "GlobalSettings.VIB"
                    );
                    ed.MmlValueGeneral = mmlValueGeneral;

                    List<string> mmlValueOps = new List<string>();
                    for (int i = 0; i < tim.Ops.Length; i++)
                    {
                        var op = tim.Ops[i];
                        mmlValueOps.Add(SimpleSerializer.SerializeProps(op,
                            nameof(op.AR),
                            nameof(op.DR),
                            nameof(op.RR),
                            nameof(op.SL),
                            nameof(op.SR),
                            nameof(op.TL),
                            nameof(op.KSL),
                            nameof(op.KSR),
                            nameof(op.MFM),
                            nameof(op.AM),
                            nameof(op.VR),
                            nameof(op.EG),
                            nameof(op.WS)
                            ));
                    }

                    DialogResult dr = editorService.ShowDialog(ed);
                    if (dr == DialogResult.OK)
                    {
                        return ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1];
                    }
                    else
                    {
                        return mmlValueGeneral + "," + mmlValueOps[0] + "," + mmlValueOps[1];
                    }
                }
            }

            return value;                   // エディタ呼び出し直前の設定値をそのまま返す

        }
    }
}
