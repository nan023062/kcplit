using System;
using System.Text;

namespace ProtoBuf
{
    public static class SmartBufferExtensions
    {
        public static void DecodeProtoMsg<T>(this SmartBuffer buff, T msg)
        {
            Serializer.Merge(buff, msg);
        }

        public static void DecodeProtoMsg<T>(this SmartBuffer buff, byte[] bytes, T msg)
        {
            buff.Reset();
            buff.In(bytes, 0, (uint)bytes.Length);
            Serializer.Merge(buff, msg);
        }

        public static void EncodeProtoMsg<T>(this SmartBuffer buff, T msg)
        {
            Serializer.Serialize(buff, msg);
        }

        public static object DecodeProtoMsg(this SmartBuffer buff, object msg, Type type)
        {
            return Serializer.Merge(buff, msg, type);
        }

        public static object DecodeProtoMsg(this SmartBuffer buff, byte[] bytes, object msg, Type type)
        {
            buff.Reset();
            buff.In(bytes, 0, (uint)bytes.Length);
            return Serializer.Merge(buff, msg, type);
        }
    }
}
