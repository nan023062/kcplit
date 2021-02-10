using Nave.Network.IPCLit;
using System;

namespace Nave.Network
{
    class Entry
    {
        static void Main(string[] args)
        {
            Debuger.Log("Hello World! 按1登陆客户端，按2登陆服务器：");

            string tag = Console.ReadLine();
            var s = new IPCSession();
            Session session;

            if (tag == "2")
            {
                s.Init(2);
                session = new Server();
                s.RegisterListener(session);
            }
            else
            {
                s.Init(1);
                session = new Client();
                s.RegisterListener(session);
            }

            session.Running(s);
        }

        public interface Session
        {
            void Running(IPCSession s);
        }

        public class Server: Session
        {
            public void CallServer(int from, string desc)
            {
                Debuger.LogWarning($"Server.CallServer: from={from},desc={desc}");
            }

            public void Running(IPCSession s)
            {
                s.Start();
                while (true)
                {
                    string input = Console.ReadLine();
                    s.Invoke(1, "CallClient", input);
                }
                s.Stop();
            }
        }

        public class Client: Session
        {
            public void CallClient(int from, string desc)
            {
                Debuger.LogWarning($"Client.CallClient: from={from},desc={desc}");
            }

            public void Running(IPCSession s)
            {
                s.Start();
                while (true)
                {
                    string input = Console.ReadLine();
                    s.Invoke(2, "CallServer", input);
                }
            }
        }
    }
}
