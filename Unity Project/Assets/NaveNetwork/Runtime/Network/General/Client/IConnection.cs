using System;

namespace Nave.Network.Network.General.Client
{
    public interface IConnection
    {
        /// <summary>
        /// 字节数组，长度
        /// </summary>
        Event<byte[], int> onReceive { get; }

        void Init(int connId, int bindPort);
        void Clean();

        bool Connected { get; }

        int id { get; }

        int bindPort { get; }



        void Connect(string ip, int port);

        void Close();

        bool Send(byte[] bytes, int len);

        void Tick();
    }
}