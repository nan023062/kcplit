using System;
using System.Collections.Generic;
using Nave.Network.Proto;
using ProtoBuf;

namespace Nave.Network.RPCWork
{
    [ProtoContract]
    public class RPCMessage
    {
        public static RPCMessage Default = new RPCMessage();

        [ProtoMember(1)]
        public string name;

        [ProtoMember(2)]
        public List<RPCRawArg> raw_args = new List<RPCRawArg>();

        public object[] args
        {
            get
            {
                var list = new List<object>();
                for (int i = 0; i < raw_args.Count; i++)
                {
                    list.Add(raw_args[i].value);
                }
                return list.ToArray();
            }

            set
            {
                raw_args = new List<RPCRawArg>();
                object[] list = value;
                for (int i = 0; i < list.Length; i++)
                {
                    var raw_arg = new RPCRawArg();
                    raw_arg.value = list[i];
                    raw_args.Add(raw_arg);
                }
            }
        }
    }


    [ProtoContract]
    public class RPCRawArg
    {
        [ProtoMember(1)]
        public RPCArgType type;
        [ProtoMember(2)]
        public byte[] raw_value;

        public object value
        {
            get
            {
                if (raw_value == null || raw_value.Length == 0)
                {
                    return null;
                }

                switch (type)
                {
                    case RPCArgType.Int: return SmartBuffer.ToInt(raw_value);
                    case RPCArgType.UInt: return SmartBuffer.ToUInt(raw_value);
                    case RPCArgType.Long: return SmartBuffer.ToLong(raw_value);
                    case RPCArgType.ULong: return SmartBuffer.ToULong(raw_value);
                    case RPCArgType.Short: return SmartBuffer.ToShort(raw_value);
                    case RPCArgType.UShort: return SmartBuffer.ToUShort(raw_value);
                    case RPCArgType.Double: return SmartBuffer.ToFloat(raw_value);
                    case RPCArgType.Float: return SmartBuffer.ToFloat(raw_value);
                    case RPCArgType.String: return SmartBuffer.ToString(raw_value);
                    case RPCArgType.Byte: return SmartBuffer.ToByte(raw_value);
                    case RPCArgType.Bool: return SmartBuffer.ToBool(raw_value);
                    case RPCArgType.ByteArray: return raw_value;
                    case RPCArgType.PBObject: return raw_value;//由于数据层是不知道具体类型，由反射层去反序列化
                    default: return raw_value;
                }

            }
            set
            {
                //NetBuffer writer;
                object v = value;
                if (v is int)
                {
                    type = RPCArgType.Int;
                    raw_value = SmartBuffer.ToBytes((int)v);
                }
                else if (v is uint)
                {
                    type = RPCArgType.UInt;
                    raw_value = SmartBuffer.ToBytes((uint)v);
                }
                else if (v is long)
                {
                    type = RPCArgType.Long;
                    raw_value = SmartBuffer.ToBytes((long)v);
                }
                else if (v is ulong)
                {
                    type = RPCArgType.ULong;
                    raw_value = SmartBuffer.ToBytes((ulong)v);
                }
                else if (v is short)
                {
                    type = RPCArgType.Short;
                    raw_value = SmartBuffer.ToBytes((short)v);
                }
                else if (v is ushort)
                {
                    type = RPCArgType.UShort;
                    raw_value = SmartBuffer.ToBytes((ushort)v);
                }
                else if (v is double)
                {
                    type = RPCArgType.Double;
                    raw_value = SmartBuffer.ToBytes((float)v);
                }
                else if (v is float)
                {
                    type = RPCArgType.Float;
                    raw_value = SmartBuffer.ToBytes((float)v);
                }
                else if (v is string)
                {
                    type = RPCArgType.String;
                    raw_value = SmartBuffer.ToBytes((string)v);
                }
                else if (v is byte)
                {
                    type = RPCArgType.Byte;
                    raw_value = SmartBuffer.ToBytes((byte)v);
                }
                else if (v is bool)
                {
                    type = RPCArgType.Bool;
                    raw_value = SmartBuffer.ToBytes((bool)v);
                }
                else if (v is byte[])
                {
                    type = RPCArgType.ByteArray;
                    raw_value = new byte[((byte[])v).Length];
                    Buffer.BlockCopy((byte[])v, 0, raw_value, 0, raw_value.Length);
                }
                else
                {
                    var bytes = SmartBuffer.ToBytes(v);
                    if (bytes != null)
                    {
                        type = RPCArgType.PBObject;
                        raw_value = new byte[bytes.Length];
                        Buffer.BlockCopy(bytes, 0, raw_value, 0, raw_value.Length);
                    }
                    else
                    {
                        type = RPCArgType.Unkown;
                        Debuger.LogError("该参数无法序列化！value:{0}", v);
                    }
                }
            }
        }
    }


    public enum RPCArgType
    {
        Unkown = 0,
        Int = 1,
        UInt = 2,
        Long = 3,
        ULong = 4,
        Short = 5,
        UShort = 6,
        Double = 8,
        Float = 9,
        String = 10,
        Byte = 11,
        Bool = 12,
        ByteArray = 31,
        PBObject = 32
    }
}