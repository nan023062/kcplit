using ProtoBuf;
using Nave.FSPLit.Network.RPCLite;

namespace Nave.FSPLit.IPCWork
{
    [ProtoContract]
    public class IPCMessage
    {
        [ProtoMember(1)] public int src;//源服务模块ID
        [ProtoMember(2)] public RPCMessage rpc;
    }
}