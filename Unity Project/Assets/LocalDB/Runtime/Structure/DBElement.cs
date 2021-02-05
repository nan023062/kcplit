namespace Nave.DB
{
    public interface IDBElement
    {
        string name { get; }

        IDBElement group { get; }

        void SetDirty();

        object PackMsg();

        void UnpackMsg(object msg);
    }

    /// <summary>
    /// DB存储元素--最小单元
    /// </summary>
    public abstract class DBElement : IDBElement
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

        public IDBElement group { private set; get; }

        protected DBElement(IDBGroup group) { this.group = group; }

        public void SetDirty()
        {
            LocalDB.SendDBMsg(this);
        }

        public abstract object PackMsg();

        public abstract void UnpackMsg(object msg);
    }
}
