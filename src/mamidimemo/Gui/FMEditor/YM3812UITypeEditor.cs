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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.Gui.FMEditor;
using zanac.MAmidiMEmo.Instruments;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Midi;
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

            bool singleSel = true;
            YM3812Timbre tim = context.Instance as YM3812Timbre;
            YM3812Timbre[] tims = value as YM3812Timbre[];
            if (tims != null)
            {
                tim = tims[0];
                singleSel = false;
            }

            YM3812 inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                inst = InstrumentManager.FindParentInstrument(InstrumentType.YM3812, tim) as YM3812;
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitReadLock();
            }

            if (inst != null)
            {
                if (singleSel)
                {
                    var mmlValueGeneral = SimpleSerializer.SerializeProps(tim,
                    nameof(tim.ALG),
                    nameof(tim.FB),
                    "GlobalSettings.EN",
                    "GlobalSettings.AMD",
                    "GlobalSettings.VIB");

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
                    FormYM3812Editor ed = new FormYM3812Editor(inst, tim, singleSel);
                    {
                        ed.MmlValueGeneral = mmlValueGeneral;

                        ed.FormClosed += (s, e) =>
                        {
                            if (ed.DialogResult == DialogResult.OK)
                            {
                                tim.Detailed = ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1];
                            }
                            else
                            {
                                tim.Detailed = mmlValueGeneral + "," + mmlValueOps[0] + "," + mmlValueOps[1];
                            }
                        };
                        ed.Show();
                    }
                }
                else
                {
                    using (FormYM3812Editor ed = new FormYM3812Editor(inst, tim, singleSel))
                    {
                        string org = JsonConvert.SerializeObject(tims, Formatting.Indented);
                        DialogResult dr = editorService.ShowDialog(ed);
                        if (dr == DialogResult.OK)
                            return value;
                        else
                            return JsonConvert.DeserializeObject<YM3812Timbre[]>(org);
                    }
                }
            }

            return value;                   // エディタ呼び出し直前の設定値をそのまま返す

        }
    }
}
