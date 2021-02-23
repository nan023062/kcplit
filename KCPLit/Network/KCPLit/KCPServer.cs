using System;
using System.Text;
using System.Reflection;
using Nave.Network.KCPWork;
using Nave.Network.Proto;
using Nave.Network.RPCWork;
using ProtoBuf;

namespace Nave.Network.KCPLit
{
    public class Server : RPCManager, ISessionListener
    {
        private Gateway m_gateway;

        private uint m_authCmd = 0;

        private ProtoBuf.SmartBuffer m_SendBuff = new ProtoBuf.SmartBuffer();

        private ProtoBuf.SmartBuffer m_RecvBuff = new ProtoBuf.SmartBuffer();

        public void Init(int port)
        {
            Debuger.Log("port:{0}", port);

            m_gateway = new Gateway();
            m_gateway.Init(port, this);

            base.Init();
        }

        public override void Clean()
        {
            Debuger.Log();
            if (m_gateway != null)
            {
                m_gateway.Clean();
                m_gateway = null;
            }

            base.Clean();

            m_listMsgListener.Clear();
        }

        public override void Dump()
        {
            m_gateway.Dump();

            StringBuilder sb = new StringBuilder();

            foreach (var pair in m_listMsgListener)
            {
                ListenerHelper helper = pair.Value;
                sb.AppendFormat("\t<cmd:{0}, msg:{1}, \tlistener:{2}.{3}>\n", pair.Key, helper.TMsg.Name,
                    helper.onMsg.Method.DeclaringType.Name, helper.onMsg.Method.Name);
            }

            Debuger.LogWarning("\nNet Listeners ({0}):\n{1}", m_listMsgListener.Count, sb);

            base.Dump();
        }

        public void SetAuthCmd(uint cmd)
        {
            m_authCmd = cmd;
        }

        public void Tick()
        {
            m_gateway.Tick();
        }

        public void OnReceive(ISession session, byte[] bytes, int len)
        {
            NetMessage msg = new NetMessage();

            m_RecvBuff.Reset();
            m_RecvBuff.In(bytes,0, (uint)len);
            msg.Unpack(m_RecvBuff);

            if (session.IsAuth())
            {
                if (msg.head.cmd == 0)
                {
                    msg.ReadMsg(ref RPCMessage.Default);
                    HandleRPCMessage(session, RPCMessage.Default);
                }
                else
                {
                    HandlePBMessage(session, msg);
                }
            }
            else
            {
                if (msg.head.cmd == m_authCmd)
                {
                    HandlePBMessage(session, msg);
                }
                else
                {
                    Debuger.LogWarning("收到未鉴权的消息! cmd:{0}", msg.head.cmd);
                }
            }
        }


        #region RPC的协议处理方式

        private ISession m_currInvokingSession;

        private string m_currInvokingName;
        private void HandleRPCMessage(ISession session, RPCMessage rpcmsg)
        {
            RPCMethodHelper helper = GetMethodHelper(rpcmsg.name);
            if (helper != null)
            {
                object[] args = new object[rpcmsg.raw_args.Count + 1];
                var raw_args = rpcmsg.raw_args;
                var paramInfo = helper.method.GetParameters();

                args[0] = session;

                if (args.Length == paramInfo.Length)
                {
                    for (int i = 0; i < raw_args.Count; i++)
                    {
                        if (raw_args[i].type == RPCArgType.PBObject)
                        {
                            args[i + 1] = m_RecvBuff.DecodeProtoMsg(raw_args[i].raw_value,null, paramInfo[i + 1].ParameterType);
                        }
                        else
                        {
                            args[i + 1] = raw_args[i].value;
                        }
                    }

                    m_currInvokingName = rpcmsg.name;
                    m_currInvokingSession = session;

                    try
                    {    
                        helper.method.Invoke(helper.listener, BindingFlags.NonPublic, null, args, null);
                    }
                    catch (Exception e)
                    {
                        Debuger.LogError("RPC调用出错：{0}\n{1}", e.Message, e.StackTrace);
                    }
                    m_currInvokingName = null;
                    m_currInvokingSession = null;
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
            
            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.Pack(rpcmsg, m_SendBuff);
            m_currInvokingSession.Send(m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
        }   
        public void ReturnError(params object[] args)
        {
            var name = "On" + m_currInvokingName + "Error";

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.Pack(rpcmsg, m_SendBuff);
            m_currInvokingSession.Send(m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
        }
        public void Invoke(ISession session, string name, params object[] args)
        {
            Debuger.Log("->Session[{0}] {1}({2})", session.id, name, args.ToListString());

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.Pack(rpcmsg, m_SendBuff);
            m_currInvokingSession.Send(m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
        }
        public void Invoke(ISession[] listSession, string name, params object[] args)
        {
            Debuger.Log("->Session<Cnt={0}> {1}({2})", listSession.Length, name, args.ToListString());

            RPCMessage rpcmsg = new RPCMessage();
            rpcmsg.name = name;
            rpcmsg.args = args;

            NetMessage msg = new NetMessage();
            msg.head = new ProtocolHead();
            msg.Pack(rpcmsg, m_SendBuff);

            for (int i = 0; i < listSession.Length; i++) {
                listSession[i].Send(m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
            }
        }

        #endregion


        #region 传统的协议处理方式

        class ListenerHelper
        {
            public Type TMsg;
            public Delegate onMsg;
            public object msg;
        }


        private DictionarySafe<uint, ListenerHelper> m_listMsgListener = new DictionarySafe<uint, ListenerHelper>();

        private void HandlePBMessage(ISession session, NetMessage msg)
        {
            var helper = m_listMsgListener[msg.head.cmd];
            if (helper != null)
            {
                helper.msg = m_RecvBuff.DecodeProtoMsg(msg.content,helper.msg, helper.TMsg);
                if (helper.msg != null)
                    helper.onMsg.DynamicInvoke(session, msg.head.index, helper.msg);
            }
            else
            {
                Debuger.LogWarning("未找到对应的监听者! cmd:{0}", msg.head.cmd);
            }
        }

        public void Send<TMsg>(ISession session, uint index, uint cmd, TMsg msg)
        {
            Debuger.Log("index:{0}, cmd:{1}", index, cmd);

            NetMessage msgobj = new NetMessage();
            msgobj.head.index = index;
            msgobj.head.cmd = cmd;
            msgobj.head.uid = session.uid;
            msgobj.Pack(msg, m_SendBuff);
            session.Send(m_SendBuff.GetBuffer(), (int)m_SendBuff.Size);
        }

        public void AddListener<TMsg>(uint cmd, Action<ISession, uint, TMsg> onMsg)
        {
            Debuger.Log("cmd:{0}, listener:{1}.{2}", cmd, onMsg.Method.DeclaringType.Name, onMsg.Method.Name);

            ListenerHelper helper = new ListenerHelper()
            {
                TMsg = typeof(TMsg),
                onMsg = onMsg
            };

            m_listMsgListener.Add(cmd, helper);
        }

        #endregion
    }
}