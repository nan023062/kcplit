using ProtoBuf;
using Nave.Network.RPCLite;

namespace Nave.Network.IPCWork
{
    [ProtoContract]
    public class IPCMessage
    {
        [ProtoMember(1)] public int src;//源服务模块ID
        [ProtoMember(2)] public RPCMessage rpc;
    }
}