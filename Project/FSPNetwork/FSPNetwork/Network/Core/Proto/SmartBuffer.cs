using System;
using System.Text;

namespace ProtoBuf
{
    public class SmartBuffer
    {
        public static SmartBuffer DefaultWriter = new SmartBuffer();

        public static SmartBuffer DefaultReader = new SmartBuffer();

        //字段
        public const uint MAXSIZE = 0xffffffff;
        protected const int DEFAULT_BUFF_SIZE = 256 << 3;
        private byte[] __InOut_buf = new byte[128];
        private byte[] __InOut_8bit = new byte[8];
        private uint m_rpos = 0;
        private uint m_wpos = 0;
        private uint m_validateSize = 0;
        protected uint m_buffSize = 0;    
        protected byte[] m_buff = new byte[DEFAULT_BUFF_SIZE];

        //属性
        public uint Size { get { return m_wpos; } }
        public uint ReadPosition { get { return m_rpos; } }
        public uint WritePosition { get { return m_wpos; } }

        //构造
        public SmartBuffer()
        {
            m_validateSize = m_rpos = m_wpos = 0;
            _Resize(DEFAULT_BUFF_SIZE);
        }

        public SmartBuffer(byte[] buf, uint size)
        {
            m_buffSize = 0;
            m_validateSize = m_rpos = m_wpos = 0;
            if (buf != null)
            {
                Resize(size);
                Array.Copy(buf, m_buff, size);
            }
            else
            {
                _Resize(size);
            }
        }

        public void Reset()
        {
            m_validateSize = m_rpos = m_wpos = 0;
        }

        private void _Resize(uint newsize)
        {
            if (newsize > m_buffSize)
            {
                newsize = (newsize > m_buffSize * 2) ? newsize : (m_buffSize * 2);
                if ((newsize > m_buff.Length * sizeof(Byte)))// && validateSize > 0)
                {
                    if (m_validateSize > 0)
                    {
                        Byte[] tempbuff = m_buff; // copy the content to the tempbuff
                        m_buff = new Byte[newsize];
                        Array.Copy(tempbuff, m_buff, m_validateSize);
                        tempbuff = null;
                    }
                    else
                    {
                        m_buff = new Byte[newsize];
                    }
                }
                m_buffSize = newsize;
            }
        }

        public void Resize(uint newsize)
        {
            _Resize(newsize);
        }

        public byte[] GetBuffer() { return m_buff; }

        public uint GetBuffSize() { return m_buffSize; }

        public void SkipRead(uint size) { m_rpos += size; }

        #region Read && Write

        //读写接口
        private void _Write(byte[] value, uint size)
        {
            _Write(value, 0, size);
        }
        private void _Write(byte[] value, uint offset, uint size)
        {
            Resize(m_wpos + size);
            Array.Copy(value, offset, m_buff, m_wpos, size);
            m_wpos += size;
        }
        private void _Read(byte[] value, uint size)
        {
            Array.Copy(m_buff, m_rpos, value, 0, size);
            m_rpos += size;
        }

        public int Out(byte[] dest, int offest, int size)
        {
            if (offest < 0) return 0;

            uint dSize = (uint)dest.Length;
            if (offest > dSize - 1) return 0;

            if (Size - 1 < m_rpos) return 0;

            uint rSize = (uint)size;
            //if (dSize - 1 < offest + rSize)
            //    rSize = (uint)(dSize - offest - 1);

            uint lSize = Size - m_rpos;
            if (lSize < rSize) rSize = lSize;

            if (rSize == 0) return 0;

            Array.Copy(m_buff, m_rpos, dest, offest, rSize);
            m_rpos += rSize;
            return (int)rSize;
        }
        public SmartBuffer In(byte[] value, uint offset, uint size)
        {
            _Write(value, offset, size);
            return this;
        }

        public SmartBuffer In(char value)
        {
            __InOut_8bit[0] = (byte)value;
		    _Write(__InOut_8bit, sizeof(char));
            return this;
        }
        public SmartBuffer Out(out char value)
        {
		    _Read(__InOut_8bit, sizeof(char));
		    value = (char)__InOut_8bit[0];
            return this;
        }

        public SmartBuffer In(bool value)
        {
		    if(value)
                __InOut_8bit[0] = (byte)1;
		    else
                __InOut_8bit[0] = (byte)0;
		    _Write (__InOut_8bit, sizeof(bool));
            return this;
        }
        public SmartBuffer Out(out bool value)
        {
		    _Read(__InOut_8bit, sizeof(bool));
		    value = BitConverter.ToBoolean(__InOut_8bit, 0);
            return this;
        }

        public SmartBuffer In(byte value)
        {
		    __InOut_8bit[0] = value;
		    _Write(__InOut_8bit, sizeof(Byte));
            return this;
        }
        public SmartBuffer Out(out byte value)
        {
		    _Read(__InOut_8bit, sizeof(byte));
		    value = __InOut_8bit[0];
            return this;
        }

        public SmartBuffer In(SByte value)
        {
            return (In(value));
        }
        public SmartBuffer Out(out SByte value)
        {
            return (Out(out value));
        }

        public SmartBuffer In(Int16 value)
        {
            __InOut_8bit[0] = (byte)((value >> 0) & 0xFF);
            __InOut_8bit[1] = (byte)((value >> 8) & 0xFF);
            _Write (__InOut_8bit, sizeof(Int16));
            return this;
        }
        public SmartBuffer Out(out Int16 value)
        {
		    _Read(__InOut_8bit, sizeof(Int16));
            value = (Int16)((int)__InOut_8bit[0] | 
                        (int)__InOut_8bit[1] << 8);
            return this;
        }

        public SmartBuffer In(UInt16 value)
        {
            __InOut_8bit[0] = (byte)((value >> 0) & 0xFF);
            __InOut_8bit[1] = (byte)((value >> 8) & 0xFF);
		    _Write (__InOut_8bit, sizeof(UInt16));
            return this;
        }
        public SmartBuffer Out(out UInt16 value)
        {
		    _Read(__InOut_8bit, sizeof(UInt16));
            value = (UInt16)((uint)__InOut_8bit[0] |
                        (uint)__InOut_8bit[1] << 8);
            return this;
        }

        public SmartBuffer In(Int32 value)
        {
            __InOut_8bit[0] = (byte)((value >> 0) & 0xFF);
            __InOut_8bit[1] = (byte)((value >> 8) & 0xFF);
            __InOut_8bit[2] = (byte)((value >> 16) & 0xFF);
            __InOut_8bit[3] = (byte)((value >> 24) & 0xFF);
            _Write (__InOut_8bit, sizeof(Int32));
            return this;
        }
        public SmartBuffer Out(out Int32 value)
        {
		    _Read(__InOut_8bit, sizeof(Int32));
            value = (Int32)__InOut_8bit[0] << 0 |
                    (Int32)__InOut_8bit[1] << 8 |
                    (Int32)__InOut_8bit[2] << 16 | 
                    (Int32)__InOut_8bit[3] << 24;
            return this;
        }

        public SmartBuffer In(UInt32 value)
        {
            __InOut_8bit[0] = (byte)((value >> 0) & 0xFF);
            __InOut_8bit[1] = (byte)((value >> 8) & 0xFF);
            __InOut_8bit[2] = (byte)((value >> 16) & 0xFF);
            __InOut_8bit[3] = (byte)((value >> 24) & 0xFF);
            _Write (__InOut_8bit, sizeof(UInt32));
            return this;
        }
        public SmartBuffer Out(out UInt32 value)
        {
		    _Read(__InOut_8bit, sizeof(UInt32));
            value = (UInt32)__InOut_8bit[0] << 0 |
                    (UInt32)__InOut_8bit[1] << 8 |
                    (UInt32)__InOut_8bit[2] << 16 |
                    (UInt32)__InOut_8bit[3] << 24;
            return this;
        }

        public SmartBuffer In(Int64 value)
        {
            __InOut_8bit[0] = (byte)((value >> 0) & 0xFF);
            __InOut_8bit[1] = (byte)((value >> 8) & 0xFF);
            __InOut_8bit[2] = (byte)((value >> 16) & 0xFF);
            __InOut_8bit[3] = (byte)((value >> 24) & 0xFF);
            __InOut_8bit[4] = (byte)((value >> 32) & 0xFF);
            __InOut_8bit[5] = (byte)((value >> 40) & 0xFF);
            __InOut_8bit[6] = (byte)((value >> 48) & 0xFF);
            __InOut_8bit[7] = (byte)((value >> 56) & 0xFF);
            _Write (__InOut_8bit, sizeof(Int64));
            return this;
        }
        public SmartBuffer Out(out Int64 value)
        {
		    _Read(__InOut_8bit, sizeof(Int64));
            value = (Int64)__InOut_8bit[0] << 0 |
                    (Int64)__InOut_8bit[1] << 8 |
                    (Int64)__InOut_8bit[2] << 16 |
                    (Int64)__InOut_8bit[3] << 24 |
                    (Int64)__InOut_8bit[4] << 32 |
                    (Int64)__InOut_8bit[5] << 40 |
                    (Int64)__InOut_8bit[6] << 48 |
                    (Int64)__InOut_8bit[7] << 56;
            return this;
        }

        public SmartBuffer In(UInt64 value)
        {
            __InOut_8bit[0] = (byte)((value >> 0) & 0xFF);
            __InOut_8bit[1] = (byte)((value >> 8) & 0xFF);
            __InOut_8bit[2] = (byte)((value >> 16) & 0xFF);
            __InOut_8bit[3] = (byte)((value >> 24) & 0xFF);
            __InOut_8bit[4] = (byte)((value >> 32) & 0xFF);
            __InOut_8bit[5] = (byte)((value >> 40) & 0xFF);
            __InOut_8bit[6] = (byte)((value >> 48) & 0xFF);
            __InOut_8bit[7] = (byte)((value >> 56) & 0xFF);
            _Write (__InOut_8bit, sizeof(UInt64));
            return this;
        }
        public SmartBuffer Out(out UInt64 value)
        {
		    _Read(__InOut_8bit, sizeof(UInt64));
            value = (UInt64)__InOut_8bit[0] << 0 |
                    (UInt64)__InOut_8bit[1] << 8 |
                    (UInt64)__InOut_8bit[2] << 16 |
                    (UInt64)__InOut_8bit[3] << 24 |
                    (UInt64)__InOut_8bit[4] << 32 |
                    (UInt64)__InOut_8bit[5] << 40 |
                    (UInt64)__InOut_8bit[6] << 48 |
                    (UInt64)__InOut_8bit[7] << 56;
            return this;
        }

        public SmartBuffer In(String value)
        {
            try
            {
                int length = Encoding.UTF8.GetBytes(value, 0, value.Length, __InOut_buf, 0);
                In((UInt16)length);
                if (length > 0) _Write(__InOut_buf, (uint)length);
            }
            catch (Exception e)
            {
                Debuger.LogError($"SmartBuffer.In() msg={e.Message}, stacktrack={e.StackTrace}!");
            }
            return this;
        }
        public SmartBuffer Out(out String value)
        {
            UInt16 length = 0;
            Out(out length);
            if (length > 0) {
                _Read(__InOut_buf, length);
                value = Encoding.UTF8.GetString(__InOut_buf, 0, length);
            }
            else {
                value = String.Empty;
            }
            return this;
        }

        public SmartBuffer In(SmartBuffer value)
        {
            UInt16 size = (UInt16)value.Size;
		    In (size);
            _Write(value.GetBuffer(), value.Size);
            return this;
        }
        public SmartBuffer Out(ref SmartBuffer value)
        {
            UInt16 size = 0;
            Out(out size);
            value.Resize(size + value.m_wpos);
            _Read(__InOut_buf, size);
		    Array.Copy(__InOut_buf, 0, value.m_buff, value.m_wpos, size);
            value.m_wpos += size;
            return this;
        }

        public SmartBuffer In(float value)
        {
            unsafe {
                byte* p = (byte*)&value;
                uint length = sizeof(float);
                for (int i = 0; i < length; i++)
                    __InOut_8bit[i] = *(p + i);
                _Write(__InOut_8bit, length);
            }
            return this;
        }
        public SmartBuffer Out(out float value)
        {
            unsafe {
                uint length = sizeof(float);
                _Read(__InOut_8bit, length);

                float result = 0f;
                byte* p = (byte*)&result;
                for (int i = 0; i < length; i++)
                    *(p + i) = __InOut_8bit[i];
                value = result;
            }
            return this;
        }

        public SmartBuffer In(char[] value)
        {
            try
            {
                int length = Encoding.ASCII.GetBytes(value, 0, value.Length, __InOut_buf, 0);
                In ((UInt16)length);
                if (length > 0) _Write(__InOut_buf, (uint)length);
            }
            catch (Exception e)
            {
                Debuger.LogError($"SmartBuffer.In() msg={e.Message}, stacktrack={e.StackTrace}!");
            }
            return this;
        }
        public SmartBuffer Out(out char[] value)
        {
            UInt16 size = 0;
            Out(out size);
            if (size > 0)
            {
                _Read(__InOut_buf, size);
                value = Encoding.ASCII.GetChars(__InOut_buf, 0, size);
            }
            else
            {
                value = null;
            }
            return this;
        }

        #endregion

        #region Static Tools

        public static uint ToUInt(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, sizeof(uint));
                uint result = 0;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(uint value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static int ToInt(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, sizeof(int));
                int result = 0;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(int value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static UInt64 ToULong(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, sizeof(UInt64));
                UInt64 result = 0;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(UInt64 value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static Int64 ToLong(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, sizeof(Int64));
                Int64 result = 0;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(Int64 value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static UInt16 ToUShort(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, sizeof(UInt16));
                UInt16 result = 0;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(UInt16 value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static Int16 ToShort(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, sizeof(Int16));
                Int16 result = 0;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(Int16 value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static float ToFloat(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, sizeof(float));
                float result = 0;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(float value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static string ToString(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, (uint)bytes.Length);
                string result = string.Empty;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(string value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static byte ToByte(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, sizeof(byte));
                byte result = 0;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(byte value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static bool ToBool(byte[] bytes)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, sizeof(bool));
                bool result = false;
                DefaultReader.Out(out result);
                return result;
            }
        }

        public static byte[] ToBytes(bool value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.In(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        public static void ToObject<T>(byte[] bytes, T obj)
        {
            lock (DefaultReader)
            {
                DefaultReader.Reset();
                DefaultReader._Write(bytes, 0, (uint)bytes.Length);
                DefaultReader.DecodeProtoMsg(obj);
            }
        }

        public static byte[] ToBytes<T>(T value)
        {
            lock (DefaultWriter)
            {
                DefaultWriter.Reset();
                DefaultWriter.EncodeProtoMsg(value);
                byte[] bytes = new byte[DefaultWriter.Size];
                DefaultWriter._Read(bytes, DefaultWriter.Size);
                return bytes;
            }
        }

        #endregion
    }
}
