
namespace Nave.Network.IPCLit
{
    using ProtoBuf;

    [ProtoContract]
    public class IPCMessage
    {
        [ProtoMember(1)] public int src;//源服务模块ID
        [ProtoMember(2)] public RPCWork.RPCMessage rpc;
    }
}