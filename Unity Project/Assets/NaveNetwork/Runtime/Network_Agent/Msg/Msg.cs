
namespace Nave.Network
{
    public abstract class Msg : ICodecable
    {
        public void Decode(Codec codec)
        {
            DecodeMsgBody(codec);
        }

        public void Encode(Codec codec)
        {
            EncodeMsgBody(codec);
        }

        protected abstract void DecodeMsgBody(Codec codec);

        protected abstract void EncodeMsgBody(Codec codec);

        public void Decode(byte[] bytes, int start, int length)
        {
            Codec.global.Reset();
            Codec.global.In(bytes, start, bytes.Length);
            Decode(Codec.global);
        }

        public int Encode(byte[] bytes, int start)
        {
            Codec.global.Reset();
            Encode(Codec.global);
            return Codec.global.GetBuffer(bytes, start);
        }
    }
}
