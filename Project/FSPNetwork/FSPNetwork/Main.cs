using Nave.Network.IPCLit;
using Nave.Network.KCPLit;
using System;
using System.Threading;
using Nave.Network.Proto;

namespace Nave.Network
{
    class Entry
    {
        static Thread m_ThreadTick;
        static Session m_Session;
        static uint cmd = 023062;

        static void Main(string[] args)
        {
            Debuger.Log("Hello World! 按1登陆客户端，按2登陆服务器：");
            string tag = Console.ReadLine();

            if (tag == "2")
            {
                m_Session = new Server();
            }
            else
            {
                m_Session = new Client();
            }

            m_ThreadTick = new Thread(Thread_Tick) { IsBackground = true };
            m_ThreadTick.Start();

            m_Session.Input();
        }

        static void Thread_Tick()
        {
            while (true)
            {
                try
                {
                    m_Session.Tick();
                }
                catch (Exception e)
                {
                    Debuger.LogWarning(e.Message + "\n" + e.StackTrace);
                }
                Thread.Sleep(33);
            }
        }

        public interface Session
        {
            void Input();


            void Tick();
        }

        public class Server: Session
        {
            Nave.Network.KCPLit.Server kcp = null;

            public Server()
            {
                kcp = new KCPLit.Server();
                kcp.Init(8002);
                kcp.SetAuthCmd(cmd);
                kcp.AddListener<pb.Mail_UserMailInfo>(cmd, HandMsg);
            }

            public void HandMsg(KCPWork.ISession session, uint index, pb.Mail_UserMailInfo msg)
            {
                Debuger.LogWarning($"HandMsg: from={session.id},index={index},mail_id={msg.id}, content={msg.content}");
                msg.id = 5201314;
                msg.content = msg.content + "---回复！";
                kcp.Send(session, 0, cmd, msg);
            }

            public void Input()
            {
                while (true)
                {
                    Thread.Sleep(1);
                }
            }

            public void Tick()
            {
                kcp.Tick();
            }
        }

        public class Client: Session
        {
            Nave.Network.KCPLit.Client kcp = null;

            public Client()
            {
                kcp = new KCPLit.Client(13,8001);
                kcp.Connect("127.0.0.1", 8002);

                kcp.AddListener<pb.Mail_UserMailInfo>(cmd, HandMsgRsp);
            }

            public void HandMsgRsp(pb.Mail_UserMailInfo msg)
            {
                Debuger.LogWarning($"HandMsgRsp: mail_id={msg.id}, content={msg.content}");
            }

            public void Input()
            {
                while (true)
                {
                    string input = Console.ReadLine();
                    pb.Mail_UserMailInfo msg = new pb.Mail_UserMailInfo();
                    msg.id = 5201314;
                    msg.content = input;
                    kcp.Send(cmd, msg);
                }
            }

            public void Tick()
            {
                kcp.Tick();
            }
        }
    }
}
