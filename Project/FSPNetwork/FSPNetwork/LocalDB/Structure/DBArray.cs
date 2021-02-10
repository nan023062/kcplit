using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nave.DB
{
    public abstract class DBArray<T> : IDBGroup,ICollection, IList<T> where T : class, IDBElement
    {
        private readonly static int MIN_LENGTH = 5;

        private int m_length = 0;

        private T[] m_array = new T[MIN_LENGTH];

        private object m_root = new object();

        protected string m_Name;

        public string name
        {
            get
            {
                if (string.IsNullOrEmpty(m_Name))
                    m_Name = GetType().FullName;
                return m_Name;
            }
        }
        public IDBElement group => null;

        public object PackMsg()
        {
            throw new NotImplementedException();
        }

        public void SetDirty()
        {
            LocalDB.SendDBMsg(this);
        }

        public void UnpackMsg(object msg)
        {
            var elem = UnpackItemMsg(msg);
            if (elem != null) LocalDB.NoticDBChanged(elem);
        }

        protected abstract T UnpackItemMsg(object msg);

        public int Count { get { return m_length; } }

        public bool IsSynchronized => true;

        public object SyncRoot => m_root;

        public bool IsReadOnly => false;

        private void Resize(bool add)
        {
            if (add)
            {
                if (m_length >= m_array.Length)
                {
                    int capacity = m_array.Length * 2;
                    T[] array = new T[capacity];
                    Array.Copy(m_array, array, m_length);
                    m_array = array;
                }
            }
            else if(m_array.Length > MIN_LENGTH * 2)
            {
                if(m_length < m_array.Length / 2)
                {
                    T[] array = new T[m_array.Length / 2];
                    Array.Copy(m_array, array, m_length);
                    m_array = array;
                }
            }
        }

        public T this[int index] 
        { 
            get
            {
                if (index < 0 || index >= m_length)
                    throw new Exception("index_get: index is outof range!!");
                return m_array[index];
            }
            set
            {
                if (index < 0 || index >= m_length + 1)
                    throw new Exception("index_set: index is outof range!!");
                if (index == m_length) { 
                    Resize(true);
                    m_length++;
                }
                m_array[index] = value;
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (array.Length < index + m_length)
                throw new Exception("CopyTo array capacity is not enough !!");
            m_array.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return m_array.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < m_length; i++)
                if (item == m_array[i]) return i;
            return -1;
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > m_length)
                throw new Exception("Insert index is outof range!!");

            Resize(true);
            
            for (int i = m_length - 1; i >= index; i--) {
                m_array[i + 1] = m_array[i];
            }
            m_array[index] = item;
            m_length++;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= m_length)
                return;

            for (int i = index; i < m_length - 1; i++) {
                m_array[i] = m_array[i + 1];
            }
            m_array[m_length - 1] = null;
            m_length--;
            Resize(false);
        }

        public void Add(T item)
        {
            Resize(true);
            m_array[m_length] = item;
            m_length++;
        }

        public void Clear()
        {
            m_length = 0;
            m_array = new T[MIN_LENGTH];
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array.Length < arrayIndex + m_length)
                throw new Exception("CopyTo array capacity is not enough !!");
            Array.Copy(m_array, 0, array, arrayIndex, m_length);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if(index != -1) {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumerator() as IEnumerator<T>;
        }
    }
}
