using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using ProtoBuf;

namespace Nave.Network.Network
{
    public delegate void ConnectEvent(SocketError error);

    public delegate void RecvPacketEvent(NetPacket packet);

    /// <summary>
    /// 与服务器进行网络会话的类
    /// </summary>
    public class ClientSession
    {
        public string ip { private set; get; }
        public ushort port { private set; get; }
        public string Log_Tag { private set; get; }

        private ClientSocket m_clientSocket = null;
        public event ConnectEvent connectEvent = null;
        public event RecvPacketEvent recvPacketEvent = null;

        public ClientSession()
        {
            m_clientSocket = new ClientSocket(this);
            _netpacketPool = new TObjectPool<NetPacket>();
            Log_Tag = "ClientSession";
        }

        public void Update(float deltaTime)
        {
            try
            {
                //更新socket
                m_clientSocket.Update(deltaTime);

                //处理收到的包
                while (_recvPacketQueue.Count > 0)
                {
                    var packet = _recvPacketQueue.Dequeue();
                    recvPacketEvent(packet);
                    UnspawnPacket(packet);
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
            }
        }

        #region 网络连接

        /// <summary>
        /// 连接网络
        /// </summary>
        public void Connect(string ipAdress, ushort port)
        {
            try
            {
                ip = ipAdress;
                this.port = port;
                this.Log("Connect() : {0}:{1}", ipAdress, port);
                m_clientSocket.Connect(ipAdress, port);
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
                OnDisconnect(SocketError.SocketError);
            }   
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            m_clientSocket.Disconnect();
        }

        /// <summary>
        /// 当网络连接时
        /// </summary>
        public void OnConnect()
        {
            if (connectEvent != null) connectEvent(SocketError.Success);
        }

        /// <summary>
        /// 当网络断开时
        /// </summary>
        /// <param name="error_code"></param>
        public void OnDisconnect(SocketError error_code)
        {
            if (connectEvent != null) connectEvent(error_code);
        }

        #endregion

        #region 网络包处理

        private TObjectPool<NetPacket> _netpacketPool = null;
        private Queue<NetPacket> _recvPacketQueue = new Queue<NetPacket>();
        private NetPacket __sendPacketData = new NetPacket();
        private SmartBuffer __sendBuffer = new SmartBuffer();
        private byte[] __sizeCount_buf = new byte[4];

        public NetPacket SpawnPacket()
        {
            return _netpacketPool.Spawn();
        }

        public void UnspawnPacket(NetPacket packet)
        {
            _netpacketPool.Unspawn(packet);
        }

        /// <summary>
        /// 发送网络数据包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        public void SendNetPacket<T>(T instance, UInt16 opcode) 
        {
            __sendBuffer.Reset();
            Serializer.Serialize<T>(__sendBuffer, instance);
            SendNetPacket(__sendBuffer.GetBuffer(), opcode);
        }

        /// <summary>
        /// 发出字节流包
        /// </summary>
        public void SendNetPacket(byte[] bytes, UInt16 opcode)
        {
            __sendPacketData.Reset();
            __sendPacketData.opcode = opcode;

            if (bytes != null && bytes.Length > 0)
            {
                UInt16 sizeCount = (UInt16)bytes.Length;
                NetworkTools.WriteToBuffer(sizeCount, __sizeCount_buf, 0);
                __sendPacketData.In(__sizeCount_buf, 4);
                __sendPacketData.In(bytes, (UInt32)bytes.Length);
            }
            m_clientSocket.SendNetPacket(__sendPacketData);
        }

        /// <summary>
        /// 收到网络包
        /// </summary>
        public void OnRecivePacket(NetPacket packet)
        {
            _recvPacketQueue.Enqueue(packet);
        }

        private const int _headSize = 4;
        private static byte[] _hsize = new byte[_headSize];

        /// <summary>
        /// 反序列化
        /// </summary>
        public void IniGameInf<T>(ref T instance, NetPacket packet)
        {
            uint rPos = packet.ReadPosition;
            packet.Out(ref _hsize, _headSize);
            UInt16 bytesize = NetworkTools.ReadUInt16FromBuffer(_hsize, 0);
            packet.Size = (uint)(bytesize + _headSize + rPos);
            Serializer.Merge<T>(packet, instance);
        }
        #endregion
    }
}
