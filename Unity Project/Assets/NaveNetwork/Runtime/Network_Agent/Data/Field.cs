using System;
using UnityEngine;

namespace Nave.Network
{
    public struct Field : IValue, ICodecable
    {
        private ValueType mValueType;

        private dynamic mValue;

        public ValueType type { get { return mValueType; } }
        public dynamic value { get { return mValue; } }

        public static Field New(ValueType type)
        {
            switch (type)
            {
                case ValueType.Int32:
                    return new Field((int)0);
                case ValueType.UInt32:
                    return new Field((UInt32)0);
                case ValueType.Bool:
                    return new Field(false);
                case ValueType.Float:
                    return new Field(0f);
                case ValueType.String:
                    return new Field(string.Empty);
                case ValueType.Vector2:
                    return new Field(Vector2.zero);
                case ValueType.Vector3:
                    return new Field(Vector3.zero);
                case ValueType.Vector4:
                    return new Field(Vector4.zero);
                case ValueType.Quaternion:
                    return new Field(Quaternion.identity);
                default:
                    break;
            }
            return new Field(0);
        }

        public Field(int value)
        {
            mValueType = ValueType.Int32;
            mValue = value;
        }

        public Field(uint value)
        {
            mValueType = ValueType.UInt32;
            mValue = value;
        }

        public Field(bool value)
        {
            mValueType = ValueType.Bool;
            mValue = value;
        }

        public Field(float value)
        {
            mValueType = ValueType.Float;
            mValue = value;
        }

        public Field(string value)
        {
            mValueType = ValueType.Int32;
            mValue = value;
        }

        public Field(Vector2 value)
        {
            mValueType = ValueType.Vector2;
            mValue = value;
        }

        public Field(Vector3 value)
        {
            mValueType = ValueType.Vector3;
            mValue = value;
        }

        public Field(Vector4 value)
        {
            mValueType = ValueType.Vector4;
            mValue = value;
        }

        public Field(Quaternion value)
        {
            mValueType = ValueType.Quaternion;
            mValue = value;
        }

        public int ToInt32() { return (int)mValue; }

        public uint ToUInt32() { return (uint)mValue; }

        public bool ToBool() { return (bool)mValue; }

        public float ToFloat() { return (float)mValue; }

        public Vector2 ToVector2() { return (Vector2)mValue; }

        public Vector3 ToVector3() { return (Vector3)mValue; }

        public Vector4 ToVector4() { return (Vector4)mValue; }

        public Quaternion ToQuaternion() { return (Quaternion)mValue; }

        public void Copy(IValue value)
        {
            Field v = (Field)value;
            if(v.type == type) mValue = v.value;
            else Debug.LogErrorFormat("Value.Copy()，类型不一致！");
        }

        public bool Equals(IValue value)
        {
            Field v = (Field)value;
            return v.type == type && v.mValue == mValue;
        }

        public void Decode(Codec c)
        {
            switch (mValueType)
            {
                case ValueType.Int32:
                    int i;
                    c.Out(out i);
                    mValue = i;
                    break;
                case ValueType.UInt32:
                    uint ui;
                    c.Out(out ui);
                    mValue = ui;
                    break;
                case ValueType.Bool:
                    bool b;
                    c.Out(out b);
                    mValue = b;
                    break;
                case ValueType.Float:
                    float f;
                    c.Out(out f);
                    mValue = f;
                    break;
                case ValueType.String:
                    float s;
                    c.Out(out s);
                    mValue = s;
                    break;
                case ValueType.Vector2:
                    Vector2 vec2;
                    c.Out(out vec2);
                    mValue = vec2;
                    break;
                case ValueType.Vector3:
                    Vector3 vec3;
                    c.Out(out vec3);
                    mValue = vec3;
                    break;
                case ValueType.Vector4:
                    Vector4 vec4;
                    c.Out(out vec4);
                    mValue = vec4;
                    break;
                case ValueType.Quaternion:
                    Quaternion quat;
                    c.Out(out quat);
                    mValue = quat;
                    break;
                default:
                    Debug.LogErrorFormat("Value.Decode()，类型错误！");
                    break;
            }
        }

        public void Encode(Codec c)
        {
            switch (mValueType)
            {
                case ValueType.Int32:
                    c.In((int)mValue);
                    break;
                case ValueType.UInt32:
                    c.In((uint)mValue);
                    break;
                case ValueType.Bool:
                    c.In((bool)mValue);
                    break;
                case ValueType.Float:
                    c.In((float)mValue);
                    break;
                case ValueType.String:
                    c.In((string)mValue);
                    break;
                case ValueType.Vector2:
                    c.In((Vector2)mValue);
                    break;
                case ValueType.Vector3:
                    c.In((Vector3)mValue);
                    break;
                case ValueType.Vector4:
                    c.In((Vector4)mValue);
                    break;
                case ValueType.Quaternion:
                    c.In((Quaternion)mValue);
                    break;
                default:
                    Debug.LogErrorFormat("Value.Decode()，类型错误！");
                    break;
            }
        }

        public void Decode(byte[] bytes, int start, int length)
        {
            Codec.global.Reset();
            Codec.global.In(bytes, start, length);
            Decode(Codec.global);
        }

        public int Encode(byte[] bytes, int start)
        {
            Codec.global.Reset();
            Encode(Codec.global);
            return Codec.global.GetBuffer(bytes, start);
        }
    }
}
