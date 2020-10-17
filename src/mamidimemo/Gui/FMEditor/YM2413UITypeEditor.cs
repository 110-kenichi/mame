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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2413;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    /// <summary>
    /// 
    /// </summary>
    public class YM2413UITypeEditor : UITypeEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public YM2413UITypeEditor(Type type)
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

            YM2413Timbre tim = context.Instance as YM2413Timbre;
            YM2413 inst = null;
            lock (InstrumentManager.ExclusiveLockObject)
            {
                foreach (var i in InstrumentManager.GetInstruments((int)InstrumentType.YM2413 + 1))
                {
                    Parallel.ForEach(i.BaseTimbres, t =>
                    {
                        if (t == tim)
                            inst = (YM2413)i;
                        if (inst != null)
                            return;
                    });
                }
            }

            if (inst != null)
            {
                using (FormYM2413Editor ed = new FormYM2413Editor(inst, tim))
                {
                    var mmlValueGeneral = SimpleSerializer.SerializeProps(tim,
                        nameof(tim.FB),
                        nameof(tim.SUS));
                    var tt = tim.ToneType;
                    tim.ToneType = ToneType.Custom;
                    ed.MmlValueGeneral = mmlValueGeneral;

                    List<string> mmlValueOps = new List<string>();

                    var mod = tim.Modulator;
                    mmlValueOps.Add(SimpleSerializer.SerializeProps(mod,
                        nameof(mod.AR),
                        nameof(mod.DR),
                        nameof(mod.RR),
                        nameof(mod.SL),
                        nameof(mod.SR),
                        nameof(mod.TL),
                        nameof(mod.KSL),
                        nameof(mod.KSR),
                        nameof(mod.MUL),
                        nameof(mod.AM),
                        nameof(mod.VIB),
                        nameof(mod.EG),
                        nameof(mod.DIST)
                        ));

                    var ca = tim.Career;
                    mmlValueOps.Add(SimpleSerializer.SerializeProps(ca,
                        nameof(ca.AR),
                        nameof(ca.DR),
                        nameof(ca.RR),
                        nameof(ca.SL),
                        nameof(ca.SR),
                        nameof(ca.KSL),
                        nameof(ca.KSR),
                        nameof(ca.MUL),
                        nameof(ca.AM),
                        nameof(ca.VIB),
                        nameof(ca.EG),
                        nameof(ca.DIST)
                        ));

                    DialogResult dr = editorService.ShowDialog(ed);
                    if (dr == DialogResult.OK)
                    {
                        return ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1];
                    }
                    else
                    {
                        tim.ToneType = tt;
                        return mmlValueGeneral + "," + mmlValueOps[0] + "," + mmlValueOps[1];
                    }
                }
            }

            return value;                   // エディタ呼び出し直前の設定値をそのまま返す

        }
    }
}
