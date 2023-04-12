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

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class DrumTableUITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public DrumTableUITypeEditor(Type type) : base(type)
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

            // CurrencyValueEditorForm を使用したプロパティエディタの表示
            InstrumentBase inst = context.Instance as InstrumentBase;

            using (FormDrumTableEditor frm = new FormDrumTableEditor(inst))
            {
                frm.Instrument = inst;

                if (value != null)
                {
                    var s = JsonConvert.SerializeObject((DrumTimbre[])value, Program.JsonAutoSettings);
                    DrumTimbre[] cd = JsonConvert.DeserializeObject<DrumTimbre[]>(s, Program.JsonAutoSettings);
                    DrumTimbre[] od = (DrumTimbre[])value;
                    for (int i = 0; i < od.Length; i++)
                    {
                        cd[i].NoteNumber = od[i].NoteNumber;
                        cd[i].KeyName = od[i].KeyName;
                    }
                    frm.DrumData = cd;
                }
                else
                {
                    var drumTimbres = new DrumTimbre[128];
                    for (int i = 0; i < drumTimbres.Length; i++)
                        drumTimbres[i] = new DrumTimbre(i, null);

                    frm.DrumData = drumTimbres;
                }
                DialogResult dr = editorService.ShowDialog(frm);
                if (dr == DialogResult.OK)
                {
                    for (int i = 0; i < frm.DrumData.Length; i++)
                        ((DrumTimbre[])value)[i] = frm.DrumData[i];
                    return value;
                }
                else
                {
                    return value;                   // エディタ呼び出し直前の設定値をそのまま返す
                }
            }
        }
    }
}
