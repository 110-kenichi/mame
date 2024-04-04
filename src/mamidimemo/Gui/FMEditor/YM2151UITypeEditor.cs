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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2151;

namespace zanac.MAmidiMEmo.Gui.FMEditor
{
    /// <summary>
    /// 
    /// </summary>
    public class YM2151UITypeEditor : UITypeEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public YM2151UITypeEditor(Type type)
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
            YM2151Timbre tim = context.Instance as YM2151Timbre;
            YM2151Timbre[] tims = value as YM2151Timbre[];
            if (tims != null)
            {
                tim = tims[0];
                singleSel = false;
            }

            YM2151 inst = null;
            try
            {
                //InstrumentManager.ExclusiveLockObject.EnterReadLock();

                inst = tim.Instrument as YM2151;
            }
            finally
            {
                //InstrumentManager.ExclusiveLockObject.ExitReadLock();
            }

            if (inst != null)
            {
                if (singleSel)
                {
                    string org = tim.SerializeData;

                    FormYM2151Editor ed = new FormYM2151Editor(inst, tim, singleSel);
                    {
                        ed.FormClosed += (s, e) =>
                        {
                            if (ed.DialogResult == DialogResult.OK)
                            {
                                tim.Detailed = ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1] + "," + ed.MmlValueOps[2] + "," + ed.MmlValueOps[3];
                            }
                            else if (ed.DialogResult == DialogResult.Cancel)
                            {
                                tim.SerializeData = org;
                            }
                        };
                        ed.Show();
                        ed.Activated += (s, e) =>
                        {
                            tim.Detailed = ed.MmlValueGeneral + "," + ed.MmlValueOps[0] + "," + ed.MmlValueOps[1] + "," + ed.MmlValueOps[2] + "," + ed.MmlValueOps[3];
                        };
                        string lastCopiedMmlValueGeneral = null;
                        string[] lastCopiedMmlValueOps = null;
                        string lastPatchInfo = null;
                        ed.CopyRequested += (s, e) =>
                        {
                            lastCopiedMmlValueGeneral = ed.MmlValueGeneral;
                            lastCopiedMmlValueOps = new string[] { ed.MmlValueOps[0], ed.MmlValueOps[1], ed.MmlValueOps[2], ed.MmlValueOps[3] };
                            lastPatchInfo = tim.PatchInfo;
                        };
                        ed.PasteRequested += (s, e) =>
                        {
                            if (lastCopiedMmlValueGeneral != null)
                            {
                                ed.MmlValueGeneral = lastCopiedMmlValueGeneral;
                                ed.MmlValueOps = lastCopiedMmlValueOps;
                                tim.PatchInfo = lastPatchInfo;
                            }
                        };
                    }
                }
                else
                {
                    using (FormYM2151Editor ed = new FormYM2151Editor(inst, tim, singleSel))
                    {
                        string lastCopiedMmlValueGeneral = null;
                        string[] lastCopiedMmlValueOps = null;
                        ed.CopyRequested += (s, e) =>
                        {
                            lastCopiedMmlValueGeneral = ed.MmlValueGeneral;
                            lastCopiedMmlValueOps = new string[] { ed.MmlValueOps[0], ed.MmlValueOps[1], ed.MmlValueOps[2], ed.MmlValueOps[3] };
                        };
                        ed.PasteRequested += (s, e) =>
                        {
                            if (lastCopiedMmlValueGeneral != null)
                            {
                                ed.MmlValueGeneral = lastCopiedMmlValueGeneral;
                                ed.MmlValueOps = lastCopiedMmlValueOps;
                            }
                        };

                        string org = JsonConvert.SerializeObject(tims, Formatting.Indented);
                        DialogResult dr = editorService.ShowDialog(ed);
                        if (dr == DialogResult.OK || dr == DialogResult.Abort)
                            return value;
                        else
                            return JsonConvert.DeserializeObject<YM2151Timbre[]>(org);
                    }
                }
            }

            return value;                   // エディタ呼び出し直前の設定値をそのまま返す

        }
    }
}
