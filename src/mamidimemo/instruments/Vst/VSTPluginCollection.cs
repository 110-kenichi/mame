// copyright-holders:K.Ito
using Jacobi.Vst.Core;
using Jacobi.Vst.Interop.Host;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
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
    public class VSTPluginCollection : IList<VstPlugin>, IList
    {
        private List<VstPlugin> f_list = new List<VstPlugin>();

        public VSTPluginCollection()
        {
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
    }

}
