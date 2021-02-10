using System;
using System.Collections.Generic;


namespace Nave.DB
{
    /// <summary>
    /// 特性：定义协议消息类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DBDefsAttribute : Attribute
    {
        public Type protoMsgType;

        public DBDefsAttribute(Type msgType)
        {
            protoMsgType = msgType;
        }
    }
}
