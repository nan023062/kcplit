using System.Collections.Generic;

namespace Nave.Network
{
    public abstract class Property : IProperty, ICodecable
    {
        #region Pool

        private static List<Property> s_Pool;

        public static T Spawn<T>() where T : Property, new()
        {
            if (s_Pool == null) s_Pool = new List<Property>();
            int count = s_Pool.Count;
            if (count > 0)
            {
                Property property = s_Pool[count - 1];
                s_Pool.RemoveAt(count - 1);
                return property as T;
            }
            else
            {
                return new T();
            }
        }

        public static void Unspawn<T>(T inst) where T : Property, new()
        {
            if (s_Pool == null) s_Pool = new List<Property>();
            s_Pool.Add(inst);
        }

        #endregion

        protected string m_Name;
        public string name 
        { 
            get {
                if (string.IsNullOrEmpty(m_Name))
                    m_Name = GetType().Name;
                return m_Name;        
            } 
        }

        public void ReadBufferAndChangeValues(Codec c)
        {
            Decode(c);
            OnValueChanged();
        }

        public bool CheckChangedAndWriteToBuffer(Codec c)
        {
            if(IsValueChanged())
            {
                Encode(c);
                return true;
            }
            return false;
        }

        public void Decode(Codec c) { ReadFromBuffer(c); }

        public void Encode(Codec c) { WriteToBuffer(c); }

        public void Decode(byte[] bytes, int start, int length)
        {
            Codec.global.Reset();
            Codec.global.In(bytes, start, bytes.Length);
            ReadFromBuffer(Codec.global);
        }

        public int Encode(byte[] bytes, int start)
        {
            Codec.global.Reset();
            WriteToBuffer(Codec.global);
            return Codec.global.GetBuffer(bytes, start);
        }

        protected abstract void OnValueChanged();

        public abstract bool IsValueChanged();

        protected abstract void WriteToBuffer(Codec c);

        protected abstract void ReadFromBuffer(Codec c);
    }
}
