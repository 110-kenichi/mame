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
using static zanac.MAmidiMEmo.Instruments.Chips.YM2608;
using static zanac.MAmidiMEmo.Instruments.Chips.YM2610B;

namespace zanac.MAmidiMEmo.Gui
{
    /// <summary>
    /// 
    /// </summary>
    public class OpnAdpcmFileLoaderUITypeEditor : ArrayEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public OpnAdpcmFileLoaderUITypeEditor(Type type) : base(type)
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


        static long[] stepSizeTable = new long[16]
        {
             57,  57,  57,  57,  77, 102, 128, 153,
             57,  57,  57,  57,  77, 102, 128, 153
        };

        private static byte[] encodeAdpcm(short[] srcWave, int maxSize)
        {
            List<byte> dest = new List<byte>();
            int lpc, flag;
            long i, dn, xn, stepSize;
            byte adpcm;
            byte adpcmPack = 0;

            xn = 0;
            stepSize = 127;
            flag = 0;

            int srcidx = 0;
            for (lpc = 0; lpc < srcWave.Length; lpc++)
            {
                dn = srcWave[srcidx] - xn;
                srcidx++;

                i = (Math.Abs(dn) << 16) / (stepSize << 14);
                if (i > 7)
                    i = 7;
                adpcm = (byte)i;

                i = (adpcm * 2 + 1) * stepSize / 8;

                if (dn < 0)
                {
                    adpcm |= 0x8;
                    xn -= i;
                }
                else
                {
                    xn += i;
                }

                stepSize = (stepSizeTable[adpcm] * stepSize) / 64;

                if (stepSize < 127)
                    stepSize = 127;
                else if (stepSize > 24576)
                    stepSize = 24576;

                if (flag == 0)
                {
                    adpcmPack = (byte)(adpcm << 4);
                    flag = 1;
                }
                else
                {
                    adpcmPack |= adpcm;
                    dest.Add(adpcmPack);
                    if (dest.Count - 1 == maxSize)
                        break;
                    flag = 0;
                }
            }

            return dest.ToArray();
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
                                return rvalue;
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
                                // byte[] -> short[]
                                List<short> wav = new List<short>();
                                for (int i = 0; i < data.Data.Length; i+=2)
                                    wav.Add((short)((data.Data[i + 1] << 8) | data.Data[i]));
                                // Encode
                                int max = 0;
                                if (att != null && att.MaxSize != 0)
                                    max = att.MaxSize;
                                byte[] adpcmData = encodeAdpcm(wav.ToArray(), max);

                                switch(context.Instance)
                                {
                                    case YM2608Timbre tim:
                                        tim.BaseFreqency = 440d * 55000d / (double)data.SampleRate;
                                        tim.TimbreName = Path.GetFileNameWithoutExtension(fn);
                                        break;
                                    case YM2610BTimbre tim:
                                        tim.BaseFreqency = 440d * 55000d / (double)data.SampleRate;
                                        tim.TimbreName = Path.GetFileNameWithoutExtension(fn);
                                        break;
                                }

                                // byte[] -> byte[]
                                object rvalue = convertToRetValue(context, adpcmData);
                                if (rvalue != null)
                                    return rvalue;
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
