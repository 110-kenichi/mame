// copyright-holders:K.Ito
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Melanchall.DryWetMidi.Core;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using zanac.MAmidiMEmo.ComponentModel;

namespace zanac.MAmidiMEmo.Instruments.Vst
{
    [Editor(typeof(RefreshingCollectionEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(ExpandableCollectionConverter))]
    [RefreshProperties(RefreshProperties.All)]
    public class VSTPluginCollection : IList<VstPlugin>, IList, IDisposable
    {
        private List<VstPlugin> f_list = new List<VstPlugin>();

        public VSTPluginCollection()
        {
        }

        private bool disposedValue = false; // 重複する呼び出しを検出するには

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                foreach (var vp in this)
                    vp.Dispose();

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~VSTPluginCollectionBase() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }

        public int IndexOf(VstPlugin item)
        {
            return f_list.IndexOf(item);
        }

        public void Insert(int index, VstPlugin item)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.RemoveAt(index);
        }

        public VstPlugin this[int index]
        {
            get
            {
                return f_list[index];
            }
            set
            {
                f_list[index] = value;
            }
        }

        public void Add(VstPlugin item)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.Add(item);
        }

        public void Clear()
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.Clear();
        }

        public bool Contains(VstPlugin item)
        {
            return f_list.Contains(item);
        }

        public void CopyTo(VstPlugin[] array, int arrayIndex)
        {
            f_list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return f_list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList)f_list).IsReadOnly;
            }
        }

        public bool Remove(VstPlugin item)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                return f_list.Remove(item);
        }

        public IEnumerator<VstPlugin> GetEnumerator()
        {
            return f_list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int IList.Add(object value)
        {
            int index = Count;
            Add((VstPlugin)value);
            return index;
        }

        bool IList.Contains(object value)
        {
            return Contains((VstPlugin)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((VstPlugin)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (VstPlugin)value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                return ((IList)f_list).IsFixedSize;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return ((IList)f_list).IsReadOnly;
            }
        }

        void IList.Remove(object value)
        {
            Remove((VstPlugin)value);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (VstPlugin)value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if(array is VstPlugin[])
                CopyTo((VstPlugin[])array, index);
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)f_list).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)f_list).SyncRoot;
            }
        }

        public void ProcessSoundControl(ControlChangeEvent midiEvent)
        {
            foreach (var vp in this)
            {
                foreach (var pn in vp.VECCSS[midiEvent.Channel].GetProperties(vp, midiEvent.ControlNumber - 90))
                {
                    float val = (float)midiEvent.ControlValue / (float)128;
                    vp.Settings.SetPropertyValue(pn, val);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void ProcessCallback(IntPtr buffer, int samples)
        {
            if (samples == 0)
                return;

            int[][] buf = new int[2][] { new int[samples], new int[samples] };
            IntPtr[] pt = new IntPtr[] { Marshal.ReadIntPtr(buffer), Marshal.ReadIntPtr(buffer + IntPtr.Size) };
            Marshal.Copy(pt[0], buf[0], 0, samples);
            Marshal.Copy(pt[1], buf[1], 0, samples);

            using (VstAudioBufferManager bufA = new VstAudioBufferManager(2, samples))
            using (VstAudioBufferManager bufB = new VstAudioBufferManager(2, samples))
            {
                lock (InstrumentBase.VstPluginContextLockObject)
                {
                    bool processed = false;
                    foreach (var vp in this)
                    {
                        var ctx = vp.PluginContext;
                        if (ctx != null)
                        {
                            int idx = 0;
                            foreach (VstAudioBuffer vab in bufA)
                            {
                                Parallel.ForEach(Partitioner.Create(0, samples), range =>
                                {
                                    for (var i = range.Item1; i < range.Item2; i++)
                                        vab[i] = (float)buf[idx][i] / 32767.0f;
                                });
                                //for (int i = 0; i < samples; i++)
                                //    vab[i] = (float)buf[idx][i] / (float)int.MaxValue;
                                idx++;
                            }
                            break;
                        }
                    }

                    VstAudioBufferManager bufa = bufA;
                    VstAudioBufferManager bufb = bufA;
                    foreach (var vp in this)
                    {
                        var ctx = vp.PluginContext;
                        if (ctx != null)
                        {
                            ctx.Context.PluginCommandStub.SetBlockSize(samples);
                            ctx.Context.PluginCommandStub.ProcessReplacing(bufa.ToArray<VstAudioBuffer>(), bufb.ToArray<VstAudioBuffer>());
                            processed = true;
                        }
                        var tmp = bufa;
                        bufa = bufb;
                        bufb = tmp;
                    }

                    if (processed)
                    {
                        int idx = 0;
                        foreach (VstAudioBuffer vab in bufb)
                        {
                            Parallel.ForEach(Partitioner.Create(0, samples), range =>
                            {
                                for (var i = range.Item1; i < range.Item2; i++)
                                    buf[idx][i] = (int)(vab[i] * 32767.0f);
                            });
                            //for (int i = 0; i < samples; i++)
                            //    buf[idx][i] = (int)(vab[i] * int.MaxValue);
                            idx++;
                        }
                        Marshal.Copy(buf[0], 0, pt[0], samples);
                        Marshal.Copy(buf[1], 0, pt[1], samples);
                    }
                }
            }
        }

    }

}
