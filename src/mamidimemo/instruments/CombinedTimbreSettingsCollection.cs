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

namespace zanac.MAmidiMEmo.Instruments
{
    [Editor(typeof(RefreshingCollectionEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(ExpandableCollectionConverter))]
    [RefreshProperties(RefreshProperties.All)]
    public class CombinedTimbreSettingsCollection : IList<CombinedTimbreSettings>, IList
    {
        private InstrumentBase f_Instrument;

        /// <summary>
        /// 
        /// </summary>
        [Browsable(false)]
        public InstrumentBase Instrument
        {
            get
            {
                return f_Instrument;
            }
            set
            {
                f_Instrument = value;
            }
        }

        private List<CombinedTimbreSettings> f_list = new List<CombinedTimbreSettings>();

        /// <summary>
        /// 
        /// </summary>
        public CombinedTimbreSettingsCollection()
        {
        }


        public int IndexOf(CombinedTimbreSettings item)
        {
            return f_list.IndexOf(item);
        }

        public void Insert(int index, CombinedTimbreSettings item)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.RemoveAt(index);
        }

        public CombinedTimbreSettings this[int index]
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

        public void Add(CombinedTimbreSettings item)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.Add(item);
        }

        public void Clear()
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                f_list.Clear();
        }

        public bool Contains(CombinedTimbreSettings item)
        {
            return f_list.Contains(item);
        }

        public void CopyTo(CombinedTimbreSettings[] array, int arrayIndex)
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

        public bool Remove(CombinedTimbreSettings item)
        {
            lock (InstrumentBase.VstPluginContextLockObject)
                return f_list.Remove(item);
        }

        public IEnumerator<CombinedTimbreSettings> GetEnumerator()
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
            Add((CombinedTimbreSettings)value);
            return index;
        }

        bool IList.Contains(object value)
        {
            return Contains((CombinedTimbreSettings)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((CombinedTimbreSettings)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (CombinedTimbreSettings)value);
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
            Remove((CombinedTimbreSettings)value);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (CombinedTimbreSettings)value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if(array is CombinedTimbreSettings[])
                CopyTo((CombinedTimbreSettings[])array, index);
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
