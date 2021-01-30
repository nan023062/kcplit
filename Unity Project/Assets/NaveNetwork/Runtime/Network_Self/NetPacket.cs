using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Nave.Network.Network
{
    /// <summary>
    /// 包头
    /// </summary>
    public class NetPacketHeader
    {
        public const int HEAD_SIZE = 8;
        public UInt32 size;         //包体大小
        public UInt32 cmd;          //消息号
        //public char keng; 
        //public UInt64 accountId;
    }

    /// <summary>
    /// 网络数据包
    /// </summary>
    public class NetPacket : SmartBuffer
    {
        private ushort m_opcode = 0;  //消息号
        public ushort opcode { set { m_opcode = value; } get { return m_opcode; } }

        public NetPacket() { }
        public NetPacket(ushort opcode) { m_opcode = opcode; }
    }
}
