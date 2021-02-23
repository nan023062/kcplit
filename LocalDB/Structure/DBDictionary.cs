namespace Nave.DB
{
    using System.Collections.Generic;
    
    /// <summary>
    /// DB存词典
    /// </summary>
    public abstract class DBDictionary<T> : Dictionary<string, T>, IDBGroup where T : class, IDBElement
    {
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

        public new T this[string key]
        {
            set
            {
                base[key] = value;
            }
            get
            {
                T result = null;
                TryGetValue(key, out result);
                return result;
            }
        }

        public new void Add(string key, T value)
        {
            this[key] = value;
        }

        public void SetDirty()
        {
            LocalDB.SendDBMsg(this);
        }

        public object PackMsg()
        {
            throw new System.NotImplementedException();
        }

        public void UnpackMsg(object msg)
        {
            var elem = UnpackItemMsg(msg);
            if(elem != null) LocalDB.NoticDBChanged(elem);
        }

        protected abstract T UnpackItemMsg(object msg);
    }
}
