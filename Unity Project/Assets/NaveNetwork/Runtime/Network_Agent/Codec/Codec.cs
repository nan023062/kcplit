using System;
using System.Text;
using UnityEngine;

namespace Nave.Network
{
    /// <summary>
    /// Varint编解码的字节缓冲区
    /// 这里的float型采用低精度处理
    /// </summary>
    public class Codec
    {
        /// <summary>
        /// 全局共享的缓冲区
        /// </summary>
        public static Codec global { private set; get; } = new Codec();

        public const int IN_OUT_SIZE = 1024;
        public const int DEFAULT_BUFF_SIZE = 256 << 3;

        private int nCapacity = DEFAULT_BUFF_SIZE;
        public int nLength { private set; get; }
        public int nReadPos { private set; get; }

        private byte[] __buffer = new byte[DEFAULT_BUFF_SIZE];
        private byte[] __InOut_buf = new byte[IN_OUT_SIZE];
        public byte[] Buffer { get { return __buffer; } }

        /// <summary>
        /// 重置缓冲区数据
        /// </summary>
        public void Reset()
        {
            nReadPos = 0;
            nLength = 0;
        }

        /// <summary>
        /// 调整缓冲区大小
        /// </summary>
        public void Resize(int length)
        {
            if (length <= nCapacity) return;
            int size = nCapacity *= 2;
            if (size < length) size = length;

            byte[] newBuffer = new byte[size];
            Array.Copy(__buffer, newBuffer, nLength);
            nCapacity = size;
            __buffer = newBuffer;
        }

        public bool IsEmpty() { return nReadPos >= nLength; }

        /// <summary>
        /// 从缓冲区读取一个有效数据
        /// </summary>
        private int __Read()
        {
            int n = 0;
            while (true)
            {
                if (IsEmpty()) throw new Exception("字节溢出！！");
                byte b = __buffer[nReadPos++];
                __InOut_buf[n++] = b;
                if ((b & 0x80) == 0) break;
            }
            return n;
        }

        private int __Write(UInt64 ui64)
        {
            int num = 0;
            while (true)
            {
                byte b = (byte)(ui64 & 0x7F);
                ui64 = ui64 >> 7;
                if (ui64 == 0)
                {
                    __InOut_buf[num++] = b;
                    break;
                }
                else
                {
                    b |= 0x80;
                    __InOut_buf[num++] = b;
                }
            }
            return num;
        }

        public void In(byte[] bytes, int offset, int length)
        {
            Resize(nLength + length + 1);
            Array.Copy(bytes, offset, __buffer, nLength, length);
            nLength += length;
        }

        public void Out(byte[] bytes,int offset, int length)
        {
            if (nReadPos + length > nLength) throw new Exception("读取字节溢出！");
            Array.Copy(__buffer, nReadPos, bytes, offset, length);
            nReadPos += length;
        }

        public void In(UInt64 ui64)
        {
            int num = __Write(ui64);
            In(__InOut_buf, 0, num);
        }
        public void Out(out UInt64 ui64)
        {
            int ui64_temp = 0;
            int length = __Read();
            for (int i = 0; i < length; i++)
            {
                byte b = __InOut_buf[i];
                ui64_temp |= (b & 0x7F) << (i * 7);
            }
            ui64 = (uint)ui64_temp;
        }

        public void In(uint ui)
        {
            In((UInt64)ui);
        }
        public void Out(out uint ui)
        {
            UInt64 ui64_temp = 0;
            Out(out ui64_temp);
            ui = (uint)ui64_temp;
        }

        public void In(int i)
        {
            if (i < 0) i = (-i << 1) - 1;
            else i = i << 1;
            In((uint)i);
        }
        public void Out(out int i)
        {
            uint ui = 0;
            Out(out ui);

            if ((ui & 1) == 1) i = -(int)((ui + 1) >> 1);
            else i = (int)(ui >> 1);
        }
        public void Insert(uint i)
        {
            int num = __Write((UInt64)i);
            Resize(nLength + num);
            Array.Copy(__InOut_buf, 0, __buffer, nLength, num);
            nLength += num;
        }

        public void In(bool b)
        {
            __InOut_buf[0] = (byte)(b ? 1 : 0);
            In(__InOut_buf, 0, 1);
        }
        public void Out(out bool b)
        {
            Out(__InOut_buf, 0, 1);
            b = (__InOut_buf[0]) > 0;
        }

        public void In(short si)
        {
            In((int)si);
        }
        public void Out(out short si)
        {
            int i = 0;
            Out(out i);
            si = (short)i;
        }

        public void In(string s)
        {
            int length = Encoding.UTF8.GetBytes(s, 0, s.Length, __InOut_buf, 0);
            In(length);
            if (length > 0) In(__InOut_buf, 0, length);
        }
        public void Out(out string s)
        {
            int length = 0;
            Out(out length);
            if (length > 0 && length < IN_OUT_SIZE)
            {
                Out(__InOut_buf, 0, length);
                s = Encoding.UTF8.GetString(__InOut_buf, 0, length);
            }
            else
            {
                if (length > IN_OUT_SIZE) Debug.LogError("读取的String长度超过 IN_OUT_SIZE ！");
                s = string.Empty;
            }
        }

        public void In(ushort si)
        {
            In((uint)si);
        }
        public void Out(out ushort si)
        {
            uint i = 0;
            Out(out i);
            si = (ushort)i;
        }

        public void In(float f)
        {
            In(CodecTool.ToInt(f));
        }
        public void Out(out float f)
        {
            int i = 0;
            Out(out i);
            f = CodecTool.ToFloat(i);
        }

        public void In(Vector2 vec2)
        {
            In(vec2.x);
            In(vec2.y);
        }
        public void Out(out Vector2 vec2)
        {
            float x, y = 0f;
            Out(out x);
            Out(out y);
            vec2 = new Vector2(x, y);
        }

        public void In(Vector3 vec3)
        {
            In(vec3.x);
            In(vec3.y);
            In(vec3.z);
        }
        public void Out(out Vector3 vec3)
        {
            float x, y, z = 0f;
            Out(out x);
            Out(out y);
            Out(out z);
            vec3 = new Vector3(x, y, z);
        }

        public void In(Vector4 vec4)
        {
            In(vec4.x);
            In(vec4.y);
            In(vec4.z);
            In(vec4.w);
        }
        public void Out(out Vector4 vec4)
        {
            float x, y, z, w = 0f;
            Out(out x);
            Out(out y);
            Out(out z);
            Out(out w);
            vec4 = new Vector4(x, y, z, w);
        }

        public void In(Quaternion quat)
        {
            In(quat.x);
            In(quat.y);
            In(quat.z);
            In(quat.w);
        }
        public void Out(out Quaternion quat)
        {
            float x, y, z, w = 0f;
            Out(out x);
            Out(out y);
            Out(out z);
            Out(out w);
            quat = new Quaternion(x, y, z, w);
        }

        public int In(Codec codec)
        {
            In(codec.nLength);
            if(codec.nLength > 0)
            {
                codec.Out(__InOut_buf, 0, codec.nLength);
                In(__InOut_buf, 0, codec.nLength);
            }
            return codec.nLength;
        }
        public int Out(Codec codec)
        {
            int length = 0;
            Out(out length);
            if( length > 0)
            {
                Out(__InOut_buf, 0, length);
                codec.In(__InOut_buf, 0, length);
            }
            return length;
        }

        public void PutBuffer(byte[] bytes, int startIndex, int length)
        {
            Reset();
            Resize(length);
            Array.Copy(bytes, startIndex, __buffer, 0, length);
            nLength = length;
        }
        public int GetBuffer(byte[] bytes, int startIndex)
        {
            Array.Copy(__buffer, 0, bytes, startIndex, nLength);
            return nLength;
        }

        private void Test()
        {
            int i = -100;
            int res = ((-i) << 1) - 1;
            int rep = -(int)((res + 1) >> 1);
            Debug.LogFormat("负数{0}-转成正数{1} ==> 正数{2}-还原负数{3}", i, res, res, rep);

            i = 100;
            res = i << 1;
            rep = (int)(res >> 1);
            Debug.LogFormat("正数{0}-转成正数{1} ==> 正数{2}-还原正数{3}", i, res, res, rep);
        }

        public void TestBeginRead() { nReadPos = 0; }
    }
}
