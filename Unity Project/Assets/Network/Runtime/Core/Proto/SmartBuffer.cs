using System;
using System.Text;

namespace ProtoBuf
{
    public class SmartBuffer
    {
        //字段
        public const UInt32 MAXSIZE = 0xffffffff;
        protected const int DEFAULT_BUFF_SIZE = 256 << 3;
        private byte[] __InOut_buf = new byte[128];
        private UInt32 m_rpos = 0;
        private UInt32 m_wpos = 0;
        private UInt32 m_validateSize = 0;
        protected UInt32 m_buffSize = 0;    
        protected Byte[] m_buff = new Byte[DEFAULT_BUFF_SIZE];

        //属性
        public UInt32 Size { get { return m_validateSize; } set { m_validateSize = value; } }
        public UInt32 ReadPosition { get { return m_rpos; } }
        public UInt32 WritePosition { get { return m_wpos; } }

        //构造
        public SmartBuffer()
        {
            m_validateSize = m_rpos = m_wpos = 0;
            _Resize(DEFAULT_BUFF_SIZE);
        }

        public SmartBuffer(SmartBuffer r)
        {
            m_validateSize = m_rpos = m_wpos = 0;
            Resize(r.m_validateSize);
        }

        public SmartBuffer(Byte[] buf, UInt32 size)
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
            m_buffSize = m_rpos = m_wpos = 0;
        }

        private void _Resize(UInt32 newsize)
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

        public void Resize(UInt32 newsize)
        {
            _Resize(newsize);
            if (newsize > m_validateSize)
                m_validateSize = newsize;
        }

        public void DestroyBuff() { m_buff = null; }

        public byte[] GetBuffer() { return m_buff; }

        public UInt32 GetBuffSize() { return m_buffSize; }

        public void SkipRead(UInt32 size) { m_rpos += size; }

        public void ResetRdPos() { m_rpos = 0; }

        public void RdPosSkipToEnd() { m_rpos = m_validateSize; }


        //读写接口
        void _Write(byte[] value, UInt32 size)
        {
            Resize(m_wpos + size);
            Array.Copy(value, 0, m_buff, m_wpos, size);
            m_wpos += size;
        }

        void _Write(byte[] value, UInt32 offset, UInt32 size)
        {
            Resize(m_wpos + size);
            Array.Copy(value, offset, m_buff, m_wpos, size);
            m_wpos += size;
        }

        protected void _Read(byte[] dest, UInt32 size)
        {
            Array.Copy(m_buff, m_rpos, dest, 0, size);
            m_rpos += size;
        }

        public int _Read(byte[] dest, int offest, int size)
        {
            if (offest < 0)
            {
                return 0;
            }
            uint dSize = (uint)dest.Length;
            if (offest > dSize - 1)
            {
                return 0;
            }
            if (Size - 1 < m_rpos)
            {
                return 0;
            }

            UInt32 rSize = (UInt32)size;
            if (dSize - 1 < offest + rSize)
            {
                rSize = (UInt32)(dSize - offest - 1);
            }
            uint lSize = Size - m_rpos;

            if (lSize < rSize)
            {
                rSize = lSize;
            }

            if (rSize == 0)
            {
                return 0;
            }
            Array.Copy(m_buff, m_rpos, dest, offest, rSize);
            m_rpos += rSize;
            return (int)rSize;
        }

        public void Append(Byte[] src, UInt16 size) { _Write(src, size); }


        //字节数据操作接口
        public SmartBuffer In(char value)
        {
		    __InOut_buf [0] = (byte)value;
		    _Write(__InOut_buf, sizeof(char));
            return this;
        }
        public SmartBuffer Out(out char value)
        {
		    _Read(__InOut_buf, sizeof(char));
		    value = (char)__InOut_buf[0];
            return this;
        }

        public SmartBuffer In(bool value)
        {
		    if(value)
			    __InOut_buf[0] = (byte)1;
		    else
			    __InOut_buf [0] = (byte)0;
		    _Write (__InOut_buf, sizeof(bool));
            return this;
        }
        public SmartBuffer Out(out bool value)
        {
		    _Read(__InOut_buf, sizeof(bool));
		    value = BitConverter.ToBoolean(__InOut_buf, 0);
            return this;
        }

        public SmartBuffer In(Byte value)
        {
		    __InOut_buf[0] = value;
		    _Write(__InOut_buf, sizeof(Byte));
            return this;
        }
        public SmartBuffer Out(out Byte value)
        {
		    _Read(__InOut_buf, sizeof(byte));
		    value = __InOut_buf[0];
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
            __InOut_buf[0] = (byte)((value >> 0) & 0xFF);
            __InOut_buf[1] = (byte)((value >> 8) & 0xFF);
            _Write (__InOut_buf, sizeof(Int16));
            return this;
        }
        public SmartBuffer Out(out Int16 value)
        {
		    _Read(__InOut_buf, sizeof(Int16));
            value = (Int16)((int)__InOut_buf[0] | 
                        (int)__InOut_buf[1] << 8);
            return this;
        }

        public SmartBuffer In(UInt16 value)
        {
            __InOut_buf[0] = (byte)((value >> 0) & 0xFF);
            __InOut_buf[1] = (byte)((value >> 8) & 0xFF);
		    _Write (__InOut_buf, sizeof(UInt16));
            return this;
        }
        public SmartBuffer Out(out UInt16 value)
        {
		    _Read(__InOut_buf, sizeof(UInt16));
            value = (UInt16)((uint)__InOut_buf[0] |
                        (uint)__InOut_buf[1] << 8);
            return this;
        }

        public SmartBuffer In(Int32 value)
        {
            __InOut_buf[0] = (byte)((value >> 0) & 0xFF);
            __InOut_buf[1] = (byte)((value >> 8) & 0xFF);
            __InOut_buf[2] = (byte)((value >> 16) & 0xFF);
            __InOut_buf[3] = (byte)((value >> 24) & 0xFF);
            _Write (__InOut_buf, sizeof(Int32));
            return this;
        }
        public SmartBuffer Out(out Int32 value)
        {
		    _Read(__InOut_buf, sizeof(Int32));
            value = (Int32)__InOut_buf[0] << 0 |
                    (Int32)__InOut_buf[1] << 8 |
                    (Int32)__InOut_buf[2] << 16 | 
                    (Int32)__InOut_buf[3] << 24;
            return this;
        }

        public SmartBuffer In(UInt32 value)
        {
            __InOut_buf[0] = (byte)((value >> 0) & 0xFF);
            __InOut_buf[1] = (byte)((value >> 8) & 0xFF);
            __InOut_buf[2] = (byte)((value >> 16) & 0xFF);
            __InOut_buf[3] = (byte)((value >> 24) & 0xFF);
            _Write (__InOut_buf, sizeof(UInt32));
            return this;
        }
        public SmartBuffer Out(out UInt32 value)
        {
		    _Read(__InOut_buf, sizeof(UInt32));
            value = (UInt32)__InOut_buf[0] << 0 |
                    (UInt32)__InOut_buf[1] << 8 |
                    (UInt32)__InOut_buf[2] << 16 |
                    (UInt32)__InOut_buf[3] << 24;
            return this;
        }

        public SmartBuffer In(Int64 value)
        {
            __InOut_buf[0] = (byte)((value >> 0) & 0xFF);
            __InOut_buf[1] = (byte)((value >> 8) & 0xFF);
            __InOut_buf[2] = (byte)((value >> 16) & 0xFF);
            __InOut_buf[3] = (byte)((value >> 24) & 0xFF);
            __InOut_buf[4] = (byte)((value >> 32) & 0xFF);
            __InOut_buf[5] = (byte)((value >> 40) & 0xFF);
            __InOut_buf[6] = (byte)((value >> 48) & 0xFF);
            __InOut_buf[7] = (byte)((value >> 56) & 0xFF);
            _Write (__InOut_buf, sizeof(Int64));
            return this;
        }
        public SmartBuffer Out(out Int64 value)
        {
		    _Read(__InOut_buf, sizeof(Int64));
            value = (Int64)__InOut_buf[0] << 0 |
                    (Int64)__InOut_buf[1] << 8 |
                    (Int64)__InOut_buf[2] << 16 |
                    (Int64)__InOut_buf[3] << 24 |
                    (Int64)__InOut_buf[4] << 32 |
                    (Int64)__InOut_buf[5] << 40 |
                    (Int64)__InOut_buf[6] << 48 |
                    (Int64)__InOut_buf[7] << 56;
            return this;
        }

        public SmartBuffer In(UInt64 value)
        {
            __InOut_buf[0] = (byte)((value >> 0) & 0xFF);
            __InOut_buf[1] = (byte)((value >> 8) & 0xFF);
            __InOut_buf[2] = (byte)((value >> 16) & 0xFF);
            __InOut_buf[3] = (byte)((value >> 24) & 0xFF);
            __InOut_buf[4] = (byte)((value >> 32) & 0xFF);
            __InOut_buf[5] = (byte)((value >> 40) & 0xFF);
            __InOut_buf[6] = (byte)((value >> 48) & 0xFF);
            __InOut_buf[7] = (byte)((value >> 56) & 0xFF);
            _Write (__InOut_buf, sizeof(UInt64));
            return this;
        }
        public SmartBuffer Out(out UInt64 value)
        {
		    _Read(__InOut_buf, sizeof(UInt64));
            value = (UInt64)__InOut_buf[0] << 0 |
                    (UInt64)__InOut_buf[1] << 8 |
                    (UInt64)__InOut_buf[2] << 16 |
                    (UInt64)__InOut_buf[3] << 24 |
                    (UInt64)__InOut_buf[4] << 32 |
                    (UInt64)__InOut_buf[5] << 40 |
                    (UInt64)__InOut_buf[6] << 48 |
                    (UInt64)__InOut_buf[7] << 56;
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
                    __InOut_buf[i] = *(p + i);
                _Write(__InOut_buf, length);
            }
            return this;
        }
        public SmartBuffer Out(out float value)
        {
            unsafe {
                uint length = sizeof(float);
                _Read(__InOut_buf, length);

                float result = 0f;
                byte* p = (byte*)&result;
                for (int i = 0; i < length; i++)
                    *(p + i) = __InOut_buf[i];
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
                byte[] temp = new byte[size];
                _Read(temp, size);
                value = System.Text.Encoding.ASCII.GetChars(temp);
            }
            else
            {
                value = null;
            }
            return this;
        }

	    public SmartBuffer Write(byte[] value, UInt32 offset, UInt32 size)
	    {
		    _Write (value, offset, size);
		    return this;
	    }
    }
}
