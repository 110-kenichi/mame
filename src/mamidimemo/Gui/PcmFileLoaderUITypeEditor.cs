// copyright-holders:K.Ito
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.IO;
using System.IO.Packaging;
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
                            using (var reader = new NAudio.Wave.WaveFileReader(fn))
                            {
                                var wf = reader.WaveFormat;

                                byte[] data = null;

                                if (att.Bits != 0 && att.Bits != wf.BitsPerSample ||
                                    att.Rate != 0 && att.Rate != wf.SampleRate ||
                                    att.Channels != 0 && att.Channels != wf.Channels)
                                {
                                    /*
                                    var r = MessageBox.Show(null,
                                        $"Incorrect wave format(Expected Ch={att.Channels} Bit={att.Bits}, Rate={att.Rate})\r\n" +
                                        "Do you want to convert?", "Qeuestion", MessageBoxButtons.OKCancel);
                                    if (r == DialogResult.Cancel)
                                    {
                                        throw new FileLoadException(
                                        string.Format($"Incorrect wave format(Expected Ch={att.Channels} Bit={att.Bits}, Rate={att.Rate}"));
                                    }*/

                                    int bits = att.Bits;
                                    if (bits == 0)
                                        bits = wf.BitsPerSample;
                                    int rate = att.Rate;
                                    if (rate == 0)
                                        rate = wf.SampleRate;
                                    int ch = att.Channels;
                                    if (ch == 0)
                                        ch = wf.Channels;

                                    WaveFormat format = new WaveFormat(rate, bits, ch);
                                    using (WaveFormatConversionStream stream = new WaveFormatConversionStream(format, reader))
                                    {
                                        data = new byte[stream.Length];
                                        stream.Read(data, 0, data.Length);
                                    }
                                }
                                else
                                {
                                    data = new byte[reader.Length];
                                    reader.Read(data, 0, data.Length);
                                }

                                {
                                    object rvalue = convertToRetValue(context, data);
                                    if (rvalue != null)
                                    {
                                        TimbreBase tim = context.Instance as TimbreBase;
                                        if (tim != null)
                                            tim.TimbreName = Path.GetFileNameWithoutExtension(fn);
                                        try
                                        {
                                            dynamic dyn = tim;
                                            dyn.SampleRate = (uint)wf.SampleRate;
                                        }
                                        catch { }
                                        return rvalue;
                                    }
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
                short[] sbuf = new short[buf.Length / 2];
                Buffer.BlockCopy(buf.ToArray(), 0, sbuf, 0, sbuf.Length);

                ushort[] ssbuf = new ushort[sbuf.Length];
                for (int i = 0; i < sbuf.Length; i++)
                    ssbuf[i] = (ushort)((int)sbuf[i] + 0x8000);

                rvalue = sbuf;
            }
            else if (context.PropertyDescriptor.PropertyType == typeof(short[]))
            {
                short[] sbuf = new short[buf.Length / 2];
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
