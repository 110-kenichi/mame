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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2414;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    /// <summary>
    /// 
    /// </summary>
    public class YM2414UITypeEditor : UITypeEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public YM2414UITypeEditor(Type type)
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
            YM2414Timbre tim = context.Instance as YM2414Timbre;
            YM2414Timbre[] tims = value as YM2414Timbre[];
            if (tims != null)
            {
                tim = tims[0];
                singleSel = false;
            }

            YM2414 inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                inst = InstrumentManager.FindParentInstrument(InstrumentType.YM2414, tim) as YM2414;
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
                    nameof(tim.AMS),
                    nameof(tim.PMS),
                    nameof(tim.AMS2),
                    nameof(tim.PMS2),

                    "GlobalSettings.EN",
                    "GlobalSettings.LFRQ",
                    "GlobalSettings.LFRQ2",
                    "GlobalSettings.LFOF",
                    "GlobalSettings.LFOD",
                    "GlobalSettings.LFOF2",
                    "GlobalSettings.LFOD2",
                    "GlobalSettings.LFOW",
                    "GlobalSettings.LFOW2",
                    "GlobalSettings.SYNC",
                    "GlobalSettings.SYNC2",
                    "GlobalSettings.NE",
                    "GlobalSettings.NFRQ",
                    "GlobalSettings.LFD",
                    "GlobalSettings.LFD2"
                    );

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
                            nameof(op.DT2),
                            nameof(op.FINE),
                            nameof(op.FIX),
                            nameof(op.FIXR),
                            nameof(op.FIXF),
                            nameof(op.OSCW),
                            nameof(op.EGSF),
                            nameof(op.REV),
                            nameof(op.LS),
                            nameof(op.KVS)
                            ));
                    }
                    FormYM2414Editor ed = new FormYM2414Editor(inst, tim, singleSel);
                    {
                        ed.MmlValueGeneral = mmlValueGeneral;

                        ed.FormClosed += (s, e) =>
                        {
                            if (ed.DialogResult == DialogResult.OK)
                            {
                                tim.Detailed = ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1] + "," + ed.MmlValueOps[2] + "," + ed.MmlValueOps[3];
                            }
                            else if (ed.DialogResult == DialogResult.Cancel)
                            {
                                tim.Detailed = mmlValueGeneral + "," + mmlValueOps[0] + "," + mmlValueOps[1] + "," + mmlValueOps[2] + "," + mmlValueOps[3];
                            }
                        };
                        ed.Show();
                        ed.Activated += (s, e) =>
                        {
                            tim.Detailed = ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1] + "," + ed.MmlValueOps[2] + "," + ed.MmlValueOps[3];
                        };
                    }
                }
                else
                {
                    using (FormYM2414Editor ed = new FormYM2414Editor(inst, tim, singleSel))
                    {
                        string org = JsonConvert.SerializeObject(tims, Formatting.Indented);
                        DialogResult dr = editorService.ShowDialog(ed);
                        if (dr == DialogResult.OK || dr == DialogResult.Abort)
                            return value;
                        else
                            return JsonConvert.DeserializeObject<YM2414Timbre[]>(org);
                    }
                }
            }

            return value;                   // エディタ呼び出し直前の設定値をそのまま返す

        }
    }
}
