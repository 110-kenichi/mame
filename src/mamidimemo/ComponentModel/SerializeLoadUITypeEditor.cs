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

                    OpenFileDialog openFileDialog = new OpenFileDialog();

                    openFileDialog.DefaultExt = "*.msd";
                    openFileDialog.Filter = "MAmi Serialize Data Files(*.msd)|*.msd";
                    string fname = null;
                    try
                    {
                        fname = tim.TimbreName;
                    }
                    catch (Exception ex1)
                    {
                        if (ex1.GetType() == typeof(Exception))
                            throw;
                        else if (ex1.GetType() == typeof(SystemException))
                            throw;

                        try
                        {
                            StringReader rs = new StringReader(tim.Memo);
                            while (rs.Peek() > -1)
                            {
                                fname = rs.ReadLine();
                                break;
                            }
                        }
                        catch (Exception ex2)
                        {
                            if (ex2.GetType() == typeof(Exception))
                                throw;
                            else if (ex2.GetType() == typeof(SystemException))
                                throw;

                            try
                            {
                                fname = fullTypeName;
                            }
                            catch (Exception ex3)
                            {
                                if (ex3.GetType() == typeof(Exception))
                                    throw;
                                else if (ex3.GetType() == typeof(SystemException))
                                    throw;
                            }
                        }
                    }
                    if (string.IsNullOrWhiteSpace(fname))
                        fname = "MyData";
                    Path.ChangeExtension(fname, ".msd");

                    foreach (var invalidChar in Path.GetInvalidFileNameChars())
                        fname = fname.Replace(invalidChar.ToString(), "");

                    openFileDialog.FileName = fname;
                    openFileDialog.SupportMultiDottedExtensions = true;

                    DialogResult res = openFileDialog.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        string txt = File.ReadAllText(openFileDialog.FileName);
                        StringReader rs = new StringReader(txt);

                        string ftname = rs.ReadLine();
                        if (fullTypeName != ftname)
                            throw new InvalidDataException();
                        string ver = rs.ReadLine();
                        if (ver != "1.0")
                            throw new InvalidDataException();

                        value = rs.ReadToEnd();
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
