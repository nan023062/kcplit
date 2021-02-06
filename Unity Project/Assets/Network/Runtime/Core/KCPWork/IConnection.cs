
namespace Nave.Network.KCPWork
{
    /// <summary>
    /// 客户端连接
    /// </summary>
    public interface IConnection
    {
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