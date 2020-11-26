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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2610B;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    /// <summary>
    /// 
    /// </summary>
    public class YM2610BUITypeEditor : UITypeEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public YM2610BUITypeEditor(Type type)
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

            YM2610BTimbre tim = context.Instance as YM2610BTimbre;
            YM2610B inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                inst = InstrumentManager.FindParentInstrument(InstrumentType.YM2610B, tim) as YM2610B;
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitReadLock();
            }

            if (inst != null)
            {
                using (FormYM2610BEditor ed = new FormYM2610BEditor(inst, tim))
                {
                    var mmlValueGeneral = SimpleSerializer.SerializeProps(tim,
                        nameof(tim.ALG),
                        nameof(tim.FB),
                        nameof(tim.AMS),
                        nameof(tim.FMS),
                        "GlobalSettings.EN",
                        "GlobalSettings.LFOEN",
                        "GlobalSettings.LFRQ"
                        );
                    ed.MmlValueGeneral = mmlValueGeneral;
                    var lastTone = tim.ToneType;
                    tim.ToneType = ToneType.FM;

                    List<string> mmlValueOps = new List<string>();
                    for (int i = 0; i < tim.Ops.Length; i++)
                    {
                        var op = tim.Ops[i];
                        mmlValueOps.Add(SimpleSerializer.SerializeProps(op,
                            nameof(op.EN),
                            nameof(op.AR),
                            nameof(op.D1R),
                            nameof(op.D2R),
                            nameof(op.RR),
                            nameof(op.SL),
                            nameof(op.TL),
                            nameof(op.RS),
                            nameof(op.MUL),
                            nameof(op.DT1),
                            nameof(op.AM),
                            nameof(op.SSG)
                            ));
                    }

                    DialogResult dr = editorService.ShowDialog(ed);
                    if (dr == DialogResult.OK)
                    {
                        return ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1] + "," + ed.MmlValueOps[2] + "," + ed.MmlValueOps[3];
                    }
                    else
                    {
                        tim.ToneType = lastTone;
                        return mmlValueGeneral + "," + mmlValueOps[0] + "," + mmlValueOps[1] + "," + mmlValueOps[2] + "," + mmlValueOps[3];
                    }
                }
            }

            return value;                   // エディタ呼び出し直前の設定値をそのまま返す

        }
    }
}
