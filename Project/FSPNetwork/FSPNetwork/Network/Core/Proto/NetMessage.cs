namespace Nave.Network.Proto
{
    public class NetMessage
    {
        private static ProtoBuf.SmartBuffer Writer = ProtoBuf.SmartBuffer.DefaultWriter;

        private static ProtoBuf.SmartBuffer Reader = ProtoBuf.SmartBuffer.DefaultReader;

        public ProtocolHead head = new ProtocolHead();

        public byte[] content;

        public uint Pack<T>(T msg, ProtoBuf.SmartBuffer buffer)
        {
            Writer.Reset();
            ProtoBuf.Serializer.Serialize(Writer, msg);
            head.dataSize = (ushort)Writer.Size;

            buffer.Reset();
            head.Serialize(buffer);
            buffer.In(Writer.GetBuffer(), 0, Writer.Size);
            return buffer.Size;
        }

        public uint Pack(ProtoBuf.SmartBuffer buffer)
        {
            buffer.Reset();
            head.Serialize(buffer);
            buffer.In(content, 0, Writer.Size);
            return buffer.Size;
        }

        public void Unpack(ProtoBuf.SmartBuffer buffer)
        {
            head.Deserialize(buffer);
            content = new byte[head.dataSize];
            buffer.Out(content, 0, head.dataSize);
        }

        public void ReadMsg<T>(ref T msg)
        {
            Reader.Reset();
            Reader.In(content, 0, head.dataSize);
            ProtoBuf.Serializer.Merge<T>(Reader, msg);
        }
    }
}