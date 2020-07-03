using ProtoBuf;
using Engine.Core.Network.RPCLite;

namespace Engine.Core.IPCWork
{
    [ProtoContract]
    public class IPCMessage
    {
        [ProtoMember(1)] public int src;//源服务模块ID
        [ProtoMember(2)] public RPCMessage rpc;
    }
}