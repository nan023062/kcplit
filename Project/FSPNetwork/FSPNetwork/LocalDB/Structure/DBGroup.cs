namespace Nave.DB
{
    using System;
    using System.Collections;

    public interface IDBGroup : IDBElement
    {
    }

    public abstract class DBGroup : IDBGroup
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

        public void SetDirty()
        {
            LocalDB.SendDBMsg(this);
        }

        public abstract object PackMsg();

        public void UnpackMsg(object msg)
        {
            var elem = UnpackItemMsg(msg);
            if (elem != null) LocalDB.NoticDBChanged(elem);
        }

        protected abstract IDBElement UnpackItemMsg(object msg);
    }
}
