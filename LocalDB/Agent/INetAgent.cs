namespace Nave.Network
{
    public enum NetworkTag
    {
        Null = 0,
        Sender = 1,
        Receive = 2,
    }

    public interface INetAgent 
    {
        uint id { get; }
    }
}
