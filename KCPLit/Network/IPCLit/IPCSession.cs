using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Nave.Network.Proto;

namespace Nave.Network.IPCLit
{
    using ProtoBuf;

    [ProtoContract]
    public class IPCMessage
    {
        [ProtoMember(1)] public int src;//源服务模块ID
        [ProtoMember(2)] public RPCWork.RPCMessage rpc;
    }

    /// <summary>
    /// 使用UDP实现的RPC会话对象
    /// </summary>
    public class IPCSession : RPCWork.RPCManager
    {
        private int m_id;

        private int m_port;

        private Socket m_SystemSocket;

        private Thread m_ThreadRecv;

        private byte[] m_RecvBufferTemp = new byte[4096];

        private Queue<byte[]> m_RecvBufferQueue = new Queue<byte[]>();

        private bool m_IsRunning = false;

        private SmartBuffer m_SendBuff = new SmartBuffer();

        private SmartBuffer m_RecvBuff = new SmartBuffer();

        public void Init(int id)
        {
            m_id = id;
            m_port = IPCConfig.GetIPCInfo(id).port;

            Init();
        }

        public override void Clean()
        {
            Stop();
            base.Clean();
        }

        public void Start()
        {
            try
            {
                m_SystemSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_SystemSocket.Bind(IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, m_port));

                m_IsRunning = true;

                m_ThreadRecv = new Thread(Thread_Recv) { IsBackground = true };
                m_ThreadRecv.Start();
            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message + e.StackTrace);
                Stop();
            }
        }

        public void Stop()
        {
            m_IsRunning = false;

            if (m_ThreadRecv != null)
            {
                m_ThreadRecv.Interrupt();
                m_ThreadRecv = null;
            }

            if (m_SystemSocket != null)
            {
                try
                {
                    m_SystemSocket.Shutdown(SocketShutdown.Both);

                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + e.StackTrace);
                }

                m_SystemSocket.Close();
                m_SystemSocket = null;
            }
        }

        #region 发送逻辑

        private void SendMessage(int dst, byte[] bytes, int len)
        {
            int dstPort = IPCConfig.GetIPCInfo(dst).port;
            IPEndPoint ep = IPUtils.GetHostEndPoint("127.0.0.1", dstPort);
            m_SystemSocket.SendTo(bytes, 0, len, SocketFlags.None, ep);
        }

        #endregion

        #region 接收逻辑

        private void Thread_Recv()
        {
            while (m_IsRunning)
            {
                try
                {
                    DoReceiveInThread();
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + "\n" + e.StackTrace);
                    Thread.Sleep(1);
                }
            }
        }

        private void DoReceiveInThread()
        {
            EndPoint remotePoint = IPUtils.GetIPEndPointAny(AddressFamily.InterNetwork, 0);
            int cnt = m_SystemSocket.ReceiveFrom(m_RecvBufferTemp, m_RecvBufferTemp.Length, SocketFlags.None, ref remotePoint);

            if (cnt > 0)
            {
                byte[] dst = new byte[cnt];
                Buffer.BlockCopy(m_RecvBufferTemp, 0, dst, 0, cnt);

                lock (m_RecvBufferQueue)
                {
                    m_RecvBufferQueue.Enqueue(dst);
                }                
            }
        }

        private void DoReceiveInMain()
        {
            lock (m_RecvBufferQueue)
            {
                if (m_RecvBufferQueue.Count > 0)
                {
                    byte[] buffer = m_RecvBufferQueue.Dequeue();
                    m_RecvBuff.Reset();
                    m_RecvBuff.In(buffer, 0, (uint)buffer.Length);
                    IPCMessage msg = new IPCMessage();
                    m_RecvBuff.DecodeProtoMsg(msg);
              
                    HandleMessage(msg);
                }
            }
        }

        public void Tick()
        {
            DoReceiveInMain();
        }

        #endregion

        #region RPC的协议处理方式

        private string m_currInvokingName;

        private int m_currInvokingSrc;

        private void HandleMessage(IPCMessage msg)
        {
            RPCWork.RPCMessage rpcmsg = msg.rpc;

            Debuger.Log("[{0}]-> {1}({2})", msg.src, rpcmsg.name, rpcmsg.args.ToListString());

            var helper = GetMethodHelper(rpcmsg.name);
            if (helper != null)
            {
                object[] args  = new object[rpcmsg.args.Length +1];

                List<RPCWork.RPCRawArg> raw_args = rpcmsg.raw_args;

                ParameterInfo[] paramInfo = helper.method.GetParameters();

                if (args.Length == paramInfo.Length)
                {
                    for (int i = 0; i < raw_args.Count; i++)
                    {
                        if (raw_args[i].type == RPCWork.RPCArgType.PBObject)
                        {
                            args[i + 1] = m_RecvBuff.DecodeProtoMsg(raw_args[i].raw_value,null, paramInfo[i + 1].ParameterType);
                        }
                        else
                        {
                            args[i + 1] = raw_args[i].value;
                        }
                    }

                    args[0] = msg.src;

                    m_currInvokingName = rpcmsg.name;
                    m_currInvokingSrc = msg.src;

                    try
                    {
                        helper.method.Invoke(helper.listener, BindingFlags.NonPublic, null, args, null);
                    }
                    catch (Exception e)
                    {
                        Debuger.LogError("RPC调用出错：{0}\n{1}", e.Message, e.StackTrace);
                    }

                    m_currInvokingName = "";
                    m_currInvokingSrc = 0;
                }
                else
                {
                    Debuger.LogWarning("参数数量不一致！");
                }
            }
            else
            {
                Debuger.LogWarning("RPC不存在！");
            }
        }

        public void Return(params object[] args)
        {
            var name = "On" + m_currInvokingName;
            Debuger.Log("->[{0}] {1}({2})", m_currInvokingSrc, name, args.ToListString());

            RPCWork.RPCMessage rpcmsg = new RPCWork.RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;

            IPCMessage msg = new IPCMessage();
            msg.src = m_id;
            msg.rpc = rpcmsg;

            m_SendBuff.Reset();
            m_SendBuff.EncodeProtoMsg(msg);
            SendMessage(m_currInvokingSrc, m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
        }

        public void ReturnError(string errinfo, int errcode = 1)
        {
            var name = "On" + m_currInvokingName + "Error";
            Debuger.LogWarning("->[{0}] {1}({2},{3})", m_currInvokingSrc, name, errinfo, errcode);

            RPCWork.RPCMessage rpcmsg = new RPCWork.RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = new object[] { errinfo, errcode };

            IPCMessage msg = new IPCMessage();
            msg.src = m_id;
            msg.rpc = rpcmsg;

            m_SendBuff.Reset();
            m_SendBuff.EncodeProtoMsg(msg);
            SendMessage(m_currInvokingSrc, m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
        }

        public void Invoke(int dst, string name, params object[] args)
        {
            Debuger.Log("->[{0}] {1}({2})", dst, name, args.ToListString());

            RPCWork.RPCMessage rpcmsg = new RPCWork.RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;

            IPCMessage msg = new IPCMessage();
            msg.src = m_id;
            msg.rpc = rpcmsg;

            m_SendBuff.Reset();
            m_SendBuff.EncodeProtoMsg(msg);
            SendMessage(dst, m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
        }

        #endregion
    }
}