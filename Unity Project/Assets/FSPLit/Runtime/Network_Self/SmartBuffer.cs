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
            Nave.FSPLit.Network.NetworkTools.WriteToBuffer (value, __InOut_buf, 0);
		    _Write (__InOut_buf, sizeof(Int16));
            return this;
        }
        public SmartBuffer Out(out Int16 value)
        {
		    _Read(__InOut_buf, sizeof(Int16));
		    value = BitConverter.ToInt16(__InOut_buf, 0);
            return this;
        }

        public SmartBuffer In(UInt16 value)
        {
            Nave.FSPLit.Network.NetworkTools.WriteToBuffer (value, __InOut_buf, 0);
		    _Write (__InOut_buf, sizeof(UInt16));
            return this;
        }
        public SmartBuffer Out(out UInt16 value)
        {
		    _Read(__InOut_buf, sizeof(UInt16));
		    value = BitConverter.ToUInt16(__InOut_buf, 0);
            return this;
        }

        public SmartBuffer In(Int32 value)
        {
            Nave.FSPLit.Network.NetworkTools.WriteToBuffer (value, __InOut_buf, 0);
		    _Write (__InOut_buf, sizeof(Int32));
            return this;
        }
        public SmartBuffer Out(out Int32 value)
        {
		    _Read(__InOut_buf, sizeof(Int32));
		    value = BitConverter.ToInt32(__InOut_buf, 0);
            return this;
        }

        public SmartBuffer In(UInt32 value)
        {
            Nave.FSPLit.Network.NetworkTools.WriteToBuffer (value, __InOut_buf, 0);
		    _Write (__InOut_buf, sizeof(UInt32));
            return this;
        }
        public SmartBuffer Out(out UInt32 value)
        {
		    _Read(__InOut_buf, sizeof(UInt32));
		    value = BitConverter.ToUInt32(__InOut_buf, 0);
            return this;
        }

        public SmartBuffer In(Int64 value)
        {
            Nave.FSPLit.Network.NetworkTools.WriteToBuffer (value, __InOut_buf, 0);
		    _Write (__InOut_buf, sizeof(Int64));
            return this;
        }
        public SmartBuffer Out(out Int64 value)
        {
		    _Read(__InOut_buf, sizeof(Int64));
		    value = BitConverter.ToInt64(__InOut_buf, 0);
            return this;
        }

        public SmartBuffer In(UInt64 value)
        {
            Nave.FSPLit.Network.NetworkTools.WriteToBuffer (value, __InOut_buf, 0);
		    _Write (__InOut_buf, sizeof(UInt64));
            return this;
        }
        public SmartBuffer Out(out UInt64 value)
        {
		    _Read(__InOut_buf, sizeof(UInt64));
		    value = BitConverter.ToUInt64(__InOut_buf, 0);
            return this;
        }

        public SmartBuffer In(String value)
        {
            UInt16 size = (UInt16)Encoding.UTF8.GetByteCount(value);
            //write size
            In(size);
            if (size > 0)
            {
                // we use utf8
                _Write(System.Text.Encoding.UTF8.GetBytes(value), size);
            }
            return this;
        }
        public SmartBuffer Out(out String value)
        {
            UInt16 size = 0;
            Out(out size);
            if (size > 0)
            {
                //value.Length = size;
                byte[] temp = new byte[size];
                _Read(temp, size);
                //value = System.Text.Encoding.ASCII.GetString(temp);
                value = System.Text.Encoding.UTF8.GetString(temp);
                //Utility.LogWarning("Out string :" + value);
                temp = null;
            }
            else
            {
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
            Nave.FSPLit.Network.NetworkTools.WriteToBuffer (value, __InOut_buf, 0);
		    _Write (__InOut_buf, sizeof(float));
            return this;
        }
        public SmartBuffer Out(out float value)
        {
		    _Read(__InOut_buf, sizeof(float));
		    value = BitConverter.ToSingle(__InOut_buf, 0);
            return this;
        }

        public SmartBuffer In(char[] value)
        {
            UInt32 size = (UInt32)System.Text.Encoding.ASCII.GetByteCount(value);
            if (size >= MAXSIZE) size = MAXSIZE;
            try
            {
			    In (size);
                if (size > 0) _Write(System.Text.Encoding.ASCII.GetBytes(value), size);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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

        public SmartBuffer In(byte[] value, UInt32 size)
        {
            _Write(value, size);
            return this;
        }
	    public SmartBuffer In(byte[] value, UInt32 offset, UInt32 size)
	    {
		    _Write (value, offset, size);
		    return this;
	    }
        public SmartBuffer Out(ref byte[] value, UInt32 size)
        {
            _Read(value, size);
            return this;
        }

     //   public SmartBuffer In(Vector3 value)
     //   {
     //       In(value.x);
     //       In(value.y);
     //       In(value.z);
     //       return this;
     //   }
     //   public SmartBuffer Out(out Vector3 value)
	    //{
		   // Out (out value.x);
		   // Out (out value.y);
		   // Out (out value.z);
		   // return this;
	    //}
    }
}
