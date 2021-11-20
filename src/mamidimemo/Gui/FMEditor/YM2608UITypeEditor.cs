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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2608;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    /// <summary>
    /// 
    /// </summary>
    public class YM2608UITypeEditor : UITypeEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public YM2608UITypeEditor(Type type)
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
            YM2608Timbre tim = context.Instance as YM2608Timbre;
            YM2608Timbre[] tims = value as YM2608Timbre[];
            if (tims != null)
            {
                tim = tims[0];
                singleSel = false;
            }

            YM2608 inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                inst = InstrumentManager.FindParentInstrument(InstrumentType.YM2608, tim) as YM2608;
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
                        nameof(tim.FMS),
                        "GlobalSettings.EN",
                        "GlobalSettings.LFOEN",
                        "GlobalSettings.LFRQ"
                        );
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
                    FormYM2608Editor ed = new FormYM2608Editor(inst, tim, singleSel);
                    {
                        ed.MmlValueGeneral = mmlValueGeneral;

                        ed.FormClosed += (s, e) =>
                        {
                            if (ed.DialogResult == DialogResult.OK)
                            {
                                tim.Detailed = ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1] + "," + ed.MmlValueOps[2] + "," + ed.MmlValueOps[3];
                            }
                            else
                            {
                                tim.ToneType = lastTone;
                                tim.Detailed = mmlValueGeneral + "," + mmlValueOps[0] + "," + mmlValueOps[1] + "," + mmlValueOps[2] + "," + mmlValueOps[3];
                            }
                        };
                        ed.Show();
                    }
                }
                else
                {
                    using (FormYM2608Editor ed = new FormYM2608Editor(inst, tim, singleSel))
                    {
                        string org = JsonConvert.SerializeObject(tims, Formatting.Indented);
                        DialogResult dr = editorService.ShowDialog(ed);
                        if (dr == DialogResult.OK)
                            return value;
                        else
                            return JsonConvert.DeserializeObject<YM2608Timbre[]>(org);
                    }
                }
            }

            return value;                   // エディタ呼び出し直前の設定値をそのまま返す

        }
    }
}
