// copyright-holders:K.Ito
using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zanac.MAmidiMEmo.Instruments
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SoundList<T> : IList<T> where T : SoundBase
    {
        private List<T> list;

        private int maxSlot;

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index].Disposed -= Item_Disposed;
                list[index] = value;
                value.Disposed += Item_Disposed;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="maxSlot"></param>
        public SoundList(int maxSlot)
        {
            list = new List<T>(maxSlot > 0 ? maxSlot : 0);
            this.maxSlot = maxSlot;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_Disposed(object sender, EventArgs e)
        {
            Remove((T)sender);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            item.Disposed -= Item_Disposed;
            list.Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            item.Disposed += Item_Disposed;
            list.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            item.Disposed += Item_Disposed;
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this[index].Disposed -= Item_Disposed;
            list.RemoveAt(index);
        }

        public void Clear()
        {
            foreach (var item in this)
                item.Disposed -= Item_Disposed;
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            item.Disposed -= Item_Disposed;
            return list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
