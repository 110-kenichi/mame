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

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class PcmFileLoaderUITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public PcmFileLoaderUITypeEditor(Type type) : base(type)
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

            PcmFileLoaderEditorAttribute att = (PcmFileLoaderEditorAttribute)context.PropertyDescriptor.Attributes[typeof(PcmFileLoaderEditorAttribute)];

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
                        if (!Path.GetExtension(fn).Equals(".wav", StringComparison.OrdinalIgnoreCase))
                        {
                            List<byte> buf = new List<byte>();
                            foreach (byte data in File.ReadAllBytes(fn))
                            {
                                buf.Add(data);
                                if (att != null && att.MaxSize != 0 && att.MaxSize == buf.Count)
                                    break;
                            }
                            object rvalue = convertRawToRetValue(context, buf.ToArray());
                            if (rvalue != null)
                            {
                                TimbreBase tim = context.Instance as TimbreBase;
                                if(tim != null)
                                    tim.TimbreName = Path.GetFileNameWithoutExtension(fn);
                                return rvalue;
                            }
                            return value;
                        }
                        else
                        {
                            var data = WaveFileReader.ReadWaveFile(fn);

                            if (att.Bits != 0 && att.Bits != data.BitPerSample ||
                                att.Rate != 0 && att.Rate != data.SampleRate ||
                                att.Channels != 0 && att.Channels != data.Channel)
                            {
                                throw new FileLoadException(
                                    string.Format($"Incorrect wave format(Expected Ch={att.Channels} Bit={att.Bits}, Rate={att.Rate},{2})"));
                            }

                            if (data.Data != null)
                            {
                                object rvalue = convertToRetValue(context, data.Data);
                                if (rvalue != null)
                                {
                                    TimbreBase tim = context.Instance as TimbreBase;
                                    if (tim != null)
                                        tim.TimbreName = Path.GetFileNameWithoutExtension(fn);
                                    return rvalue;
                                }
                            }
                            return value;
                        }
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
