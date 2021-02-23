using System;
using System.Collections;
using System.Collections.Generic;

namespace Nave.DB
{
    public abstract class DBList<T> : List<T>, IDBGroup where T :class, IDBElement
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
    }
}
