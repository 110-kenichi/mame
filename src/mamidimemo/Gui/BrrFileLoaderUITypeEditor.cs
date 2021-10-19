// copyright-holders:K.Ito
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.ComponentModel;
using zanac.MAmidiMEmo.Instruments;
using static zanac.MAmidiMEmo.Instruments.Chips.SPC700;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class BrrFileLoaderUITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public BrrFileLoaderUITypeEditor(Type type) : base(type)
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

            BrrFileLoaderEditorAttribute att = (BrrFileLoaderEditorAttribute)context.PropertyDescriptor.Attributes[typeof(BrrFileLoaderEditorAttribute)];

            // CurrencyValueEditorForm を使用したプロパティエディタの表示
            using (OpenFileDialog frm = new OpenFileDialog())
            {
                if (att != null)
                    frm.Filter = att.Exts;
                else
                    frm.Filter = "All Files(*.*)|*.*";

                while (true)
                {
                    var result = frm.ShowDialog();
                    if (result != DialogResult.OK)
                        break;

                    var fn = frm.FileName;
                    try
                    {
                        if (Path.GetExtension(fn).Equals(".brr", StringComparison.OrdinalIgnoreCase))
                        {
                            List<byte> buf = new List<byte>(File.ReadAllBytes(fn));
                            var fi = new FileInfo(fn);
                            switch (fi.Length % 9)
                            {
                                case 2:
                                    SPC700Timbre tim = context.Instance as SPC700Timbre;
                                    if (tim != null)
                                        tim.LoopPoint = (ushort)((buf[0] | (buf[1] << 8)) / 9);
                                    buf.RemoveRange(0, 2);
                                    break;
                            }
                            if (att != null && att.MaxSize != 0 && att.MaxSize == buf.Count)
                            {
                                if (buf.Count > att.MaxSize)
                                    buf.RemoveRange(att.MaxSize, buf.Count - att.MaxSize);
                            }
                            return buf.ToArray();
                            //object rvalue = convertRawToRetValue(context, buf.ToArray());
                            //if (rvalue != null)
                            //    return rvalue;
                            //return value;
                        }
                        //else if (Path.GetExtension(fn).Equals(".wav", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    var data = WaveFileReader.ReadWaveFile(fn);

                        //    if (data.Data != null)
                        //    {
                        //        object rvalue = convertToRetValue(context, data.Data);
                        //        if (rvalue != null)
                        //            return rvalue;
                        //    }
                        //    return value;
                        //}
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(Exception))
                            throw;
                        else if (ex.GetType() == typeof(SystemException))
                            throw;

                        MessageBox.Show(ex.ToString());
                    }
                }
                return value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        private static object convertToRetValue(ITypeDescriptorContext context, byte[] buf)
        {
            object rvalue = null;

            if (context.PropertyDescriptor.PropertyType == typeof(byte[]))
            {
                rvalue = buf.ToArray();
            }
            else if (context.PropertyDescriptor.PropertyType == typeof(sbyte[]))
            {
                sbyte[] sbuf = new sbyte[buf.Length];
                for (int i = 0; i < buf.Length; i++)
                    sbuf[i] = (sbyte)(buf[i] - 0x80);
                rvalue = sbuf;
            }
            else if (context.PropertyDescriptor.PropertyType == typeof(ushort[]))
            {
                ushort[] sbuf = new ushort[buf.Length / 2];
                Buffer.BlockCopy(buf.ToArray(), 0, sbuf, 0, sbuf.Length);
                rvalue = sbuf;
            }
            else if (context.PropertyDescriptor.PropertyType == typeof(ushort[]))
            {
                short[] sbuf = new short[buf.Length / 2];
                for (int i = 0; i < buf.Length / 2; i++)
                    sbuf[i] = (short)(((buf[(i * 2) + 1] << 8) + buf[i * 2]) - 0x8000);
                Buffer.BlockCopy(buf.ToArray(), 0, sbuf, 0, sbuf.Length);
                rvalue = sbuf;
            }

            return rvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        private static object convertRawToRetValue(ITypeDescriptorContext context, byte[] buf)
        {
            object rvalue = null;

            if (context.PropertyDescriptor.PropertyType == typeof(byte[]))
            {
                rvalue = buf.ToArray();
            }
            else if (context.PropertyDescriptor.PropertyType == typeof(sbyte[]))
            {
                sbyte[] sbuf = new sbyte[buf.Length];
                Buffer.BlockCopy(buf.ToArray(), 0, sbuf, 0, sbuf.Length);
                rvalue = sbuf;
            }
            else if (context.PropertyDescriptor.PropertyType == typeof(ushort[]))
            {
                ushort[] sbuf = new ushort[buf.Length / 2];
                Buffer.BlockCopy(buf.ToArray(), 0, sbuf, 0, sbuf.Length);
                rvalue = sbuf;
            }
            else if (context.PropertyDescriptor.PropertyType == typeof(ushort[]))
            {
                short[] sbuf = new short[buf.Length / 2];
                Buffer.BlockCopy(buf.ToArray(), 0, sbuf, 0, sbuf.Length);
                rvalue = sbuf;
            }

            return rvalue;
        }
    }
}
