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
        uint uniqueID { get; }

        Data data { get; }

        void HandleDataChanged(Codec codec);

        bool CheckDataChanged(Codec codec);

        void EncodeObjectPose(Codec codec);
    }
}
