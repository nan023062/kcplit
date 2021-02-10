namespace Nave.Network.Proto
{
    public class ProtocolHead
    {
        public const int Length = 16;
        public uint uid = 0;
        public uint cmd = 0;
        public uint index = 0;
        public ushort dataSize = 0;
        public ushort checksum = 0;

        public ProtocolHead Deserialize(ProtoBuf.SmartBuffer buffer)
        {
            buffer.Out(out uid);
            buffer.Out(out cmd);
            buffer.Out(out index);
            buffer.Out(out dataSize);
            buffer.Out(out checksum);
            return this;
        }

        public ProtoBuf.SmartBuffer Serialize(ProtoBuf.SmartBuffer buffer)
        {
            buffer.In(uid);
            buffer.In(cmd);
            buffer.In(index);
            buffer.In(dataSize);
            buffer.In(checksum);
            return buffer;
        }

    }
}