﻿using Newtonsoft.Json;
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
    public class PcmUITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public PcmUITypeEditor(Type type) : base(type)
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

            PcmEditorAttribute att = (PcmEditorAttribute)context.PropertyDescriptor.Attributes[typeof(PcmEditorAttribute)];

            if (editorService == null)
                return value;

            // CurrencyValueEditorForm を使用したプロパティエディタの表示
            using (FormPcmEditor frm = new FormPcmEditor())
            {
                frm.PcmData = JsonConvert.DeserializeObject<PcmTimbreBase[]>(JsonConvert.SerializeObject(((PcmTimbreTableBase)value).PcmTimbres));
                if (att != null)
                    frm.FileDialogFilter = att.Exts;
                else
                    frm.FileDialogFilter = "All Files(*.*)|*.*";
                //"HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*"
                DialogResult dr = editorService.ShowDialog(frm);
                if (dr == DialogResult.OK)
                {
                    for (int i = 0; i < frm.PcmData.Length; i++)
                        ((PcmTimbreTableBase)value).PcmTimbres[i] = frm.PcmData[i];
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
