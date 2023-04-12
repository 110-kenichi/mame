using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2612;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using zanac.MAmidiMEmo.Gui.FMEditor;
using zanac.MAmidiMEmo.Instruments.Chips;
using zanac.MAmidiMEmo.Instruments;
using System.IO;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using Microsoft.Win32;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using zanac.MAmidiMEmo.Properties;

namespace zanac.MAmidiMEmo.ComponentModel
{
    internal class SerializeLoadUITypeEditor : UITypeEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public SerializeLoadUITypeEditor(Type type)
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

            String serializeData = value as string;
            dynamic tim = context.Instance as dynamic;
            if (serializeData != null)
            {
                try
                {
                    string fullTypeName = tim.GetType().FullName;

                    InstrumentManager.InstExclusiveLockObject.EnterReadLock();

                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.InitialDirectory = Program.GetToneLibLastDir();

                        openFileDialog.DefaultExt = "*.msd";
                        openFileDialog.Filter = "MAmi Serialize Data Files(*.msd)|*.msd";

                        openFileDialog.SupportMultiDottedExtensions = true;

                        DialogResult res = openFileDialog.ShowDialog();
                        if (res == DialogResult.OK)
                        {
                            string txt = File.ReadAllText(openFileDialog.FileName);
                            StringReader rs = new StringReader(txt);

                            string ftname = rs.ReadLine();
                            if (!string.Equals(fullTypeName, ftname, StringComparison.Ordinal))
                                throw new InvalidDataException();
                            string ver = rs.ReadLine();
                            if (ver != "1.0")
                                throw new InvalidDataException();

                            value = rs.ReadToEnd();

                            Settings.Default.ToneLibLastDir = Path.GetDirectoryName(openFileDialog.FileName);
                        }
                    }
                }
                finally
                {
                    InstrumentManager.InstExclusiveLockObject.ExitReadLock();
                }
            }

            return value;                   // エディタ呼び出し直前の設定値をそのまま返す
        }
    }
}
