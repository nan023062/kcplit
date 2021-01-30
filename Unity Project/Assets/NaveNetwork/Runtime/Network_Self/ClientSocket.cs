using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;

namespace Nave.Network.Network
{
    internal class ClientSocket
    {
        private ClientSession _clientSession = null;

        public ClientSocket(ClientSession session)
        {
            _clientSession = session;
            Log_Tag = "ClientSocket";
            InitNetworkBuff();

            Reset();
        }

        public string Log_Tag { private set; get; }

        public void Reset()
        {
            ResetSocket();
            ResetNetworkBuff();
        }

        #region Socket 连接

        private IPEndPoint _ipEndPoint = null;

        private Socket _socket = null;

        private bool _connected = false;

        private void ResetSocket()
        {
            _ipEndPoint = null;
            _socket = null;
            _connected = false;
        }

        // 连接
        public bool Connect(string ipaddr, ushort port)
        {
            IPAddress ipAddress = null;
            if (IsCorrectIp(ipaddr))
            {
                ipAddress = IPAddress.Parse(ipaddr);
            }
            else
            {
                IPHostEntry host = Dns.GetHostEntry(ipaddr);
                ipAddress = host.AddressList[0];
            }
            _ipEndPoint = new IPEndPoint(ipAddress, port);
            _socket = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Blocking = true;   // set blocking socket
            _socket.NoDelay = true;    // Enable nagle buffering algorithm

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    _socket.Connect(_ipEndPoint);
                }
                catch (System.Security.SecurityException e)
                {
                    OnDisconnect(SocketError.SocketError);
                    this.LogWarning("socket try connect failded : " + e.Message);
                    continue;
                }
                catch (SocketException e)
                {
                    OnDisconnect(SocketError.SocketError);
                    this.LogError("socket connect failded : " + e.Message);
                    return false;
                }
                break;
            }

            OnConnect();
            return true;
        }

        public void Disconnect()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
                OnDisconnect(SocketError.Shutdown);
            }

            Reset();
        }

        private void OnConnect()
        {
            this.Log("socket connect success !");
            _connected = true;
            if (_clientSession != null)
                _clientSession.OnConnect();
        }

        private void OnDisconnect(SocketError error_code)
        {
            _connected = false;
            if (_clientSession != null)
                _clientSession.OnDisconnect(error_code);
        }

        // 验证IP地址格式
        private bool IsCorrectIp(string ipaddr)
        {
            return Regex.IsMatch(ipaddr, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        #endregion

        #region 网络字节流处理

        private void CheckBufferSize(ref byte[] buffer, int len)
        {
            if (len > buffer.Length) buffer = new byte[len];
        }

        private void InitNetworkBuff()
        {
            _writeBuffer = new CircularBuffer();
            _writeBuffer.Allocate(CLIENTSOCKET_SENDBUF_SIZE);

            _readBuffer = new CircularBuffer();
            _readBuffer.Allocate(CLIENTSOCKET_RECVBUF_SIZE);

            m_BytesRecieved = 0;
        }

        private void ResetNetworkBuff()
        {
            m_BytesRecieved = 0;
        }

        public void Update(float deltaTime)
        {
            if (_socket == null || !_connected) return;

            try
            {
                if (_socket.Poll(0, SelectMode.SelectError))
                {
                    OnDisconnect(SocketError.Success);
                    return;
                }
                else if (_socket.Poll(0, SelectMode.SelectRead))
                {
                    if (ReadSocketBuffer()) OnReadToPacket();
                }
                else if (_socket.Poll(0, SelectMode.SelectWrite))
                {
                    SendByteBySocket();
                }
            }
            catch (Exception ex)
            {
                Debuger.LogError("socket is error : {0} !", ex.Message);
            }
        }

        #region ReadSocketBuffer

        private UInt64 m_BytesRecieved = 0;
        const UInt32 CLIENTSOCKET_RECVBUF_SIZE = 64000;
        const int SOCKET_READER_BUFFER_SIZE = 128000;
        private CircularBuffer _readBuffer = null;
        private byte[] __readBuff = new byte[SOCKET_READER_BUFFER_SIZE];

        private bool ReadSocketBuffer()
        {
            Int32 len = (Int32)_readBuffer.GetSpace();

            CheckBufferSize(ref __readBuff, len);

            Int32 nRecv = 0;

            try
            {
                nRecv = _socket.Receive(__readBuff, len, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                this.LogError("SocketException: errorCode = {0}, message = {1} !", ex.ErrorCode, ex.Message);
                OnDisconnect((SocketError)ex.ErrorCode);
                return false;
            }

            if (nRecv == 0)
            {
                this.LogError("socket receive 0 bytes, disconnect !");
                OnDisconnect(SocketError.SocketError);
                return false;
            }
            else
            {
                if (!_readBuffer.Write(__readBuff, (uint)nRecv))
                {
                    this.LogError("write recevie bytes to circular buffer error ! ");
                }
                m_BytesRecieved += (UInt64)nRecv;
                return true;
            }
        }

        NetPacketHeader __OnRead_header = new NetPacketHeader();
        byte[] __headerbuf = new byte[NetPacketHeader.HEAD_SIZE];
        byte[] __OnRead_temp_buf = new byte[1024];
        private int __bodySize = 0;
        private int __opcode = 0;

        private void OnReadToPacket()
        {
            __bodySize = 0;

            while (true)
            {
                if (__bodySize == 0)
                {
                    if (_readBuffer.GetSize() < NetPacketHeader.HEAD_SIZE)
                    {
                        return;
                    }

                    if (!_readBuffer.Read(__headerbuf, NetPacketHeader.HEAD_SIZE))
                    {
                        this.LogError("failed read packet header !");
                        return;
                    }

                    __bodySize = NetworkTools.ReadInt32FromBuffer(__headerbuf, 0);
                    __opcode = NetworkTools.ReadInt32FromBuffer(__headerbuf, 4);
                }

                if (__bodySize > 0 && _readBuffer.GetSize() < __bodySize)
                {
                    this.LogError("body字节数不够，异常！！");
                    return;
                }

                NetPacket packet = _clientSession.SpawnPacket();
                packet.opcode = (ushort)__OnRead_header.cmd;
                if (__bodySize > 0)
                {
                    CheckBufferSize(ref __OnRead_temp_buf, __bodySize);
                    if (!_readBuffer.Read(__OnRead_temp_buf, (uint)__bodySize))
                    {
                        this.LogError("failed read packet body !");
                    }
                    packet.In(__OnRead_temp_buf, (uint)__bodySize);
                }
                __bodySize = 0;               
                _clientSession.OnRecivePacket(packet);
            }
        }

        #endregion

        #region Write Buffer

        const UInt32 CLIENTSOCKET_SENDBUF_SIZE = 64000; //发包字节上限
        private byte[] __OnSend_buffer = new byte[2048];
        private CircularBuffer _writeBuffer = null;
        private UInt64 m_BytesSent = 0;

        private void SendByteBySocket()
        {
            UInt32 len = _writeBuffer.GetContiguiousBytes();
            if (len > 0)
            {
                CheckBufferSize(ref __OnSend_buffer, (int)len);

                _writeBuffer.GetBuffFromAndLen(ref __OnSend_buffer, len);
                int nSend = 0;
                try
                {
                    nSend = _socket.Send(__OnSend_buffer, (int)len, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    OnDisconnect((SocketError)ex.ErrorCode);
                    return;
                }
                _writeBuffer.Remove((UInt32)nSend);
                m_BytesSent += (UInt64)nSend;
            }
        }

        // 发出数据包1
        public void SendNetPacket(NetPacket packet)
        {
            ushort opcode = packet.opcode;
            uint len = packet.WritePosition;

            if ((len + NetPacketHeader.HEAD_SIZE + 2) > CLIENTSOCKET_SENDBUF_SIZE)
            {
                Debuger.LogError(string.Format("WARNING: Tried to send a packet of {0} bytes (which is too large) to a socket. Opcode was: {1}", (uint)len, (uint)opcode));
                return;
            }

            //Buffer装不下下一个字节流，就直接发出去
            if (_writeBuffer.GetSpace() < len + NetPacketHeader.HEAD_SIZE)
            {
                SendByteBySocket();
            }

            NetworkTools.WriteToBuffer(len, __headerbuf, 0);
            NetworkTools.WriteToBuffer((uint)opcode, __headerbuf, 4);
            _writeBuffer.Write(__headerbuf, NetPacketHeader.HEAD_SIZE);
            if(len > 0) _writeBuffer.Write(packet.GetBuffer(), len);
        }

        #endregion

        #endregion
    }
}
