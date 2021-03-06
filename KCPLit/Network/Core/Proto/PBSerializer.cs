using ProtoBuf;
using ProtoBuf.Meta;
using System.IO;

namespace Nave.Network.Proto
{
    /// <summary>
    /// 序列化帮助类
    /// </summary>
    public class PBSerializer
    {

        /// <summary>
        /// 序列化pb数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static byte[] NSerialize<T>(T t)
        {
            byte[] buffer = null;

            using (MemoryStream m = new MemoryStream())
            {
                Serializer.Serialize<T>(m, t);

                m.Position = 0;
                int length = (int)m.Length;
                buffer = new byte[length];
                m.Read(buffer, 0, length);
            }

            return buffer;
        }

        public static int NSerialize(object t, byte[] buffer)
        {
            using (MemoryStream m = new MemoryStream())
            {
                if (t != null)
                {
                    RuntimeTypeModel.Default.Serialize(m, t);
                }

                m.Position = 0;
                int length = (int)m.Length;
                m.Read(buffer, 0, length);
                return length;
            }
        }



        /// <summary>
        /// 反序列化pb数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T NDeserialize<T>(byte[] buffer)
        {
            T t = default(T);
            using (MemoryStream m = new MemoryStream(buffer))
            {
                t = Serializer.Deserialize<T>(m);
            }
            return t;
        }

        public static object NDeserialize(byte[] buffer, System.Type type)
        {
            object t = null;
            using (MemoryStream m = new MemoryStream(buffer))
            {
                t = RuntimeTypeModel.Default.Deserialize(m, null, type);
            }
            return t;
        }
    }

}