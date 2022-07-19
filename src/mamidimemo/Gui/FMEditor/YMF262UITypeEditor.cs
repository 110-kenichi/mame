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
using zanac.MAmidiMEmo.Properties;
using static zanac.MAmidiMEmo.Instruments.Chips.YMF262;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    /// <summary>
    /// 
    /// </summary>
    public class YMF262UITypeEditor : UITypeEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public YMF262UITypeEditor(Type type)
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
            YMF262Timbre tim = context.Instance as YMF262Timbre;
            YMF262Timbre[] tims = value as YMF262Timbre[];
            if (tims != null)
            {
                tim = tims[0];
                singleSel = false;
            }

            YMF262 inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                inst = InstrumentManager.FindParentInstrument(InstrumentType.YMF262, tim) as YMF262;
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
                    nameof(tim.FB2),
                    "GlobalSettings.EN",
                    "GlobalSettings.DAM",
                    "GlobalSettings.DVB"
                    );
                    var consel = inst.CONSEL;
                    inst.CONSEL = 6;

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
                            nameof(op.VIB),
                            nameof(op.EG),
                            nameof(op.WS)
                            ));
                    }
                    bool conselWarned = false;
                    FormYMF262Editor ed = new FormYMF262Editor(inst, tim, singleSel);
                    {
                        ed.MmlValueGeneral = mmlValueGeneral;

                        ed.FormClosed += (s, e) =>
                        {
                            inst.CONSEL = consel;
                            if (ed.DialogResult == DialogResult.OK)
                            {
                                if (consel == 0 && ((YMF262Timbre)ed.Timbre).ALG >= 2)
                                    MessageBox.Show(Resources.CNTWarning, "Warning", MessageBoxButtons.OK);

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
                            if (inst.CONSEL == 0 && ((YMF262Timbre)ed.Timbre).ALG >= 2)
                            {
                                if (!conselWarned)
                                {
                                    MessageBox.Show(Resources.CNTWarning, "Warning", MessageBoxButtons.OK);
                                    conselWarned = true;
                                }
                            }

                            tim.Detailed = ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1] + "," + ed.MmlValueOps[2] + "," + ed.MmlValueOps[3];
                        };
                    }
                }
                else
                {
                    using (FormYMF262Editor ed = new FormYMF262Editor(inst, tim, singleSel))
                    {
                        string org = JsonConvert.SerializeObject(tims, Formatting.Indented);
                        var consel = inst.CONSEL;
                        inst.CONSEL = 6;
                        DialogResult dr = editorService.ShowDialog(ed);
                        inst.CONSEL = consel;
                        if (dr == DialogResult.OK || dr == DialogResult.Abort)
                        {
                            if (consel == 0 && ((YMF262Timbre)ed.Timbre).ALG >= 2)
                                MessageBox.Show(Resources.CNTWarning, "Warning", MessageBoxButtons.OK);
                            return value;
                        }
                        else
                            return JsonConvert.DeserializeObject<YMF262Timbre[]>(org);
                    }
                }
            }

            return value;                   // エディタ呼び出し直前の設定値をそのまま返す

        }

    }
}
