using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nave.Network
{
    public enum ValueType
    {
        Int32,
        UInt32,
        Bool,
        Float,
        String,
        Vector2,
        Vector3,
        Vector4,
        Quaternion,
    }

    public interface IValue
    {
        bool Equals(IValue value);

        void Copy(IValue value);
    }

    public abstract class Value : IValue
    {
        public abstract bool Equals(IValue value);

        public abstract void Copy(IValue value);

        public virtual int GetInt() { return 0; }

        public virtual float GetFloat() { return 0f; }

        public virtual uint GetUInt() { return 0; }

        public virtual bool GetBool() { return false; }

        public virtual UnityEngine.Vector2 GetVector2() { return UnityEngine.Vector2.zero; }

        public virtual UnityEngine.Vector3 GetVector3() { return UnityEngine.Vector3.zero; }
    }

    public class BoolValue : Value
    {
        private bool m_value;

        public override bool Equals(IValue value)
        {
            if (!(value is BoolValue)) return false;
            return (value as BoolValue).GetBool() == GetBool();
        }

        public override void Copy(IValue value)
        {
            if (!(value is BoolValue)) return;
            m_value = (value as BoolValue).GetBool();
        }

        public override bool GetBool()
        {
            return m_value;
        }
    }

    public class UIntValue : Value
    {
        private uint m_value;

        public override bool Equals(IValue value)
        {
            if (!(value is UIntValue)) return false;
            return (value as UIntValue).GetUInt() == m_value;
        }

        public override void Copy(IValue value)
        {
            if (!(value is UIntValue)) return;
            m_value = (value as UIntValue).GetUInt();
        }

        public override uint GetUInt()
        {
            return m_value;
        }
    }

    public class FloatValue : Value
    {
        private float m_value;

        public override bool Equals(IValue value)
        {
            if (!(value is FloatValue)) return false;
            return (value as FloatValue).GetFloat() == m_value;
        }

        public override void Copy(IValue value)
        {
            if (!(value is FloatValue)) return;
            m_value = (value as FloatValue).GetFloat();
        }

        public override float GetFloat()
        {
            return m_value;
        }
    }
}
