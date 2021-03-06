using System;
using System.Reflection;
using Nave.Network.KCPWork;
using Nave.Network.Proto;
using Nave.Network.RPCWork;
using ProtoBuf;

namespace Nave.Network.KCPLit
{
    public sealed class Client : RPCWork.RPCManager
    {
        private IConnection m_conn;

        private uint m_uid;

        private ProtoBuf.SmartBuffer m_SendBuff = new ProtoBuf.SmartBuffer();

        private ProtoBuf.SmartBuffer m_RecvBuff = new ProtoBuf.SmartBuffer();

        public Client(int connId, int bindPort)
        {
            Debuger.Log("connId:{0}, bindPort:{1}", connId, bindPort);
            m_conn = new KCPWork.KCPConnection();
            m_conn.Init(connId, bindPort);
            Init();
        }

        public override void Clean()
        {
            Debuger.Log();
            if (m_conn != null) {
                m_conn.Clean();
                m_conn = null;
            }
            base.Clean();
            m_listNtfListener.Clear();
            m_listRspListener.Clear();
        }

        public void SetUserId(uint uid)
        {
            m_uid = uid;
        }

        public void Connect(string ip, int port)
        {
            Debuger.Log("ip:{0}, port:{1}", ip, port);
            if (m_conn.Connected) {
                Debuger.Log("旧的连接还在，先关闭旧的连接");
                m_conn.Close();
            }

            m_conn.Connect(ip, port);
            m_conn.onReceive.AddListener(OnReceive);
        }

        public void Close()
        {
            Debuger.Log();
            m_conn.Close();
        }

        public void Tick()
        {
            m_conn.Tick();
            CheckTimeout();
        }

        private void OnReceive(byte[] bytes, int len)
        {
            m_RecvBuff.Reset();
            m_RecvBuff.In(bytes, 0, (uint)len);

            NetMessage msg = new NetMessage();
            msg.Unpack(m_RecvBuff);
            Debuger.LogError("OnReceive！ uid:{0}", msg.head.uid);

            if (msg.head.cmd == 0)
            {
                msg.ReadMsg(ref RPCMessage.Default);
                HandleRPCMessage(RPCMessage.Default);
            }
            else
            {
                HandlePBMessage(msg);
            }
        }


        #region RPC的协议处理方式

        private string m_currInvokingName;

        private void HandleRPCMessage(RPCMessage rpcmsg)
        {
            Debuger.Log("Connection[{0}]-> {1}({2})", m_conn.id, rpcmsg.name, rpcmsg.args);

            var helper = GetMethodHelper(rpcmsg.name);
            if (helper != null)
            {
                object[] args = rpcmsg.args;

                var raw_args = rpcmsg.raw_args;

                var paramInfo = helper.method.GetParameters();

                if (raw_args.Count == paramInfo.Length)
                {
                    for (int i = 0; i < raw_args.Count; i++)
                    {
                        if (raw_args[i].type == RPCArgType.PBObject)
                        {
                            m_RecvBuff.DecodeProtoMsg(raw_args[i].raw_value,null, paramInfo[i].ParameterType);
                        }
                        else
                        {
                            args[i] = raw_args[i].value;
                        }
                    }

                    m_currInvokingName = rpcmsg.name;

                    try
                    {
                        helper.method.Invoke(helper.listener, BindingFlags.NonPublic, null, args, null);
                    }
                    catch (Exception e)
                    {
                        Debuger.LogError("RPC调用出错：{0}\n{1}", e.Message, e.StackTrace);
                    }

                    m_currInvokingName = null;
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

        public void Invoke(string name, params object[] args)
        {
            Debuger.Log("->Connection[{0}] {1}({2})", m_conn.id, name, args);

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.head.uid = m_uid;
            msg.Pack(rpcmsg, m_SendBuff);
            m_conn.Send(m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
        }

        public void Return(params object[] args)
        {
            if (m_conn != null)
            {
                var name = "On" + m_currInvokingName;
                Debuger.Log("->Connection[{0}] {1}({2})", m_conn.id, name, args);

                RPCMessage rpcmsg = new RPCMessage();
                rpcmsg.name = name;
                rpcmsg.args = args;

                NetMessage msg = new NetMessage();
                msg.head = new ProtocolHead();
                msg.head.uid = m_uid;
                msg.Pack(rpcmsg, m_SendBuff);
                m_conn.Send(m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
            }
        }

        #endregion

        #region 传统的协议(Protobuf)处理方式

        class ListenerHelper
        {
            public uint cmd;
            public uint index;
            public Type TMsg;
            public Delegate onMsg;
            public Delegate onErr;
            public float timeout;
            public float timestamp;
            public object msg;
        }

        static class MessageIndexGenerator
        {
            private static uint m_lastIndex;
            public static uint NewIndex()
            {
                return ++m_lastIndex;
            }
        }

        private DictionarySafe<uint, ListenerHelper> m_listNtfListener = new DictionarySafe<uint, ListenerHelper>();

        private MapList<uint, ListenerHelper> m_listRspListener = new MapList<uint, ListenerHelper>();

        public void Send<TReq, TRsp>(uint cmd, TReq req, Action<TRsp> onRsp, float timeout = 30,
            Action<NetErrorCode> onErr = null)
        {
            NetMessage msg = new NetMessage();
            msg.head.index = MessageIndexGenerator.NewIndex();
            msg.head.cmd = cmd;
            msg.head.uid = m_uid;
            msg.Pack(req, m_SendBuff);
            m_conn.Send(m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);

            AddListener(cmd, typeof(TRsp), onRsp, msg.head.index, timeout, onErr);
        }

        private void AddListener(uint cmd, Type TRsp, Delegate onRsp, uint index, float timeout, Action<NetErrorCode> onErr)
        {
            ListenerHelper helper = new ListenerHelper()
            {
                cmd = cmd,
                index = index,
                TMsg = TRsp,
                onErr = onErr,
                onMsg = onRsp,
                timeout = timeout,
                timestamp = TimeSystem.GetSecondsSinceStartup()
            };

            m_listRspListener.Add(index, helper);
        }

        public void Send<TReq>(uint cmd, TReq req)
        {
            NetMessage msg = new NetMessage();
            msg.head.index = 0;
            msg.head.cmd = cmd;
            msg.head.uid = m_uid;
            msg.Pack(req, m_SendBuff);
            m_conn.Send(m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
        }

        public void AddListener<TNtf>(uint cmd, Action<TNtf> onNtf)
        {
            Debuger.Log("cmd:{0}, listener:{1}.{2}", cmd, onNtf.Method.DeclaringType.Name, onNtf.Method.Name);

            ListenerHelper helper = new ListenerHelper()
            {
                TMsg = typeof(TNtf),
                onMsg = onNtf
            };

            m_listNtfListener.Add(cmd, helper);
        }

        private void HandlePBMessage(NetMessage msg)
        {
            if (msg.head.index == 0)
            {
                var helper = m_listNtfListener[msg.head.cmd];
                if (helper != null)
                {
                    helper.msg = m_RecvBuff.DecodeProtoMsg(msg.content, helper.msg, helper.TMsg);
                    if (helper.msg != null)
                    {
                        helper.onMsg.DynamicInvoke(helper.msg);
                    }
                    else
                    {
                        Debuger.LogError("协议格式错误！ cmd:{0}", msg.head.cmd);
                    }
                }
                else
                {
                    Debuger.LogError("未找到对应的监听者! cmd:{0}", msg.head.cmd);
                }
            }
            else
            {
                var helper = m_listRspListener[msg.head.index];
                if (helper != null)
                {
                    m_listRspListener.Remove(msg.head.index);
                    helper.msg = m_RecvBuff.DecodeProtoMsg(msg.content, helper.msg, helper.TMsg);
                    if (helper.msg != null)
                    {
                        helper.onMsg.DynamicInvoke(helper.msg);
                    }
                    else
                    {
                        Debuger.LogError("协议格式错误！ cmd:{0}, index:{0}", msg.head.cmd, msg.head.index);
                    }
                }
                else
                {
                    Debuger.LogError("未找到对应的监听者! cmd:{0}, index:{0}", msg.head.cmd, msg.head.index);
                }
            }
        }


        private float m_lastCheckTimeoutStamp = 0;

        private void CheckTimeout()
        {
            float curTime = TimeSystem.GetSecondsSinceStartup();

            if (curTime - m_lastCheckTimeoutStamp >= 5)
            {
                m_lastCheckTimeoutStamp = curTime;

                var list = m_listRspListener.ToArray();
                for (int i = 0; i < list.Length; i++)
                {
                    var helper = list[i];
                    float dt = curTime - helper.timestamp;
                    if (dt >= helper.timeout)
                    {
                        m_listRspListener.Remove(helper.index);
                        if (helper.onErr != null)
                        {
                            helper.onErr.DynamicInvoke(NetErrorCode.Timeout);
                        }

                        Debuger.LogWarning("cmd:{0} Is Timeout!", helper.cmd);
                    }
                }
            }
            
        }

        #endregion

    }
}