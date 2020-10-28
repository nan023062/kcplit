using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Core.Network
{
    /// <summary>
    /// 循环BUFFER
    /// </summary>
    public class CircularBuffer
    {
        private byte[] m_buffer = null;
        private Int32 m_bufferEnd = int.MinValue;
        private Int32 m_regionAPointer = int.MinValue;
        private UInt32 m_regionASize = 0;
        private Int32 m_regionBPointer = int.MinValue;
        private UInt32 m_regionBSize = 0;

        public CircularBuffer() { }

        public void Reset()
        {
            m_regionBPointer = int.MinValue;
            m_regionASize = m_regionBSize = 0;
        }

        UInt32 GetAFreeSpace()
        {
            return (UInt32)(m_bufferEnd - m_regionAPointer - m_regionASize);
        }

        UInt32 GetSpaceBeforeA()
        {
            return (UInt32)(m_regionAPointer - 0);
        }

        UInt32 GetSpaceAfterA()
        {
            return (UInt32)(m_bufferEnd - m_regionAPointer - m_regionASize);
        }

        UInt32 GetBFreeSpace()
        {
            if (m_regionBPointer == int.MinValue) return 0;
            return (UInt32)(m_regionAPointer - m_regionBPointer - m_regionBSize);
        }

        public bool Read(byte[] dest, UInt32 bytes)
        {
            try
            {
                if (m_buffer == null) return false;
                UInt32 cnt = bytes;
                UInt32 aRead = 0;
                UInt32 bRead = 0;

                if ((m_regionASize + m_regionBSize) < bytes) return false;

                if (m_regionASize > 0)
                {
                    //max bytes is regionAsize
                    aRead = (cnt > m_regionASize) ? m_regionASize : cnt; 
                    Array.Copy(m_buffer, m_regionAPointer, dest, 0, aRead);
                    m_regionASize -= aRead;
                    m_regionAPointer += (Int32)aRead;
                    cnt -= aRead;
                }

                if (cnt > 0 && m_regionBSize > 0)
                {
                    bRead = (cnt > m_regionBSize) ? m_regionBSize : cnt;
                    Array.Copy(m_buffer, m_regionBPointer, dest, aRead, bRead);
                    m_regionBSize -= bRead;
                    m_regionBPointer += (Int32)bRead;
                    cnt -= bRead;
                }

                if (m_regionASize == 0)
                {
                    if (m_regionBSize > 0)
                    {
                        if (m_regionBPointer != 0)
                            Buffer.BlockCopy(m_buffer, (Int32)m_regionBPointer, m_buffer, 0, (Int32)m_regionBSize);
                        m_regionAPointer = 0;
                        m_regionASize = m_regionBSize;
                        m_regionBPointer = int.MinValue;
                        m_regionBSize = 0;
                    }
                    else
                    {
                        m_regionBPointer = int.MinValue;
                        m_regionBSize = 0;
                        m_regionAPointer = 0;
                        m_regionASize = 0;
                    }
                }

            }
            catch (Exception e)
            {
                Debuger.Log("Read Data exception : " + e.ToString());
                return false;
            }
            return true;
        }

        public void AllocateB()
        {
            m_regionBPointer = 0;
        }

        public bool Write(byte[] data, UInt32 bytes)
        {
            if (m_buffer == null) return false;

            if (m_regionBPointer != int.MinValue)
            {
                if (GetBFreeSpace() < bytes) return false;
                Array.Copy(data, 0, m_buffer, m_regionBSize, bytes);
                m_regionBSize += bytes;
                return true;
            }
            if (GetAFreeSpace() < GetSpaceBeforeA())
            {
                AllocateB();
                if (GetBFreeSpace() < bytes) return false;
                Array.Copy(data, 0, m_buffer, m_regionBSize, bytes);
                m_regionBSize += bytes;
                return true;
            }
            else
            {
                if (GetAFreeSpace() < bytes) return false;
                if (m_regionAPointer + m_regionASize > m_bufferEnd)
                {
                    AllocateB();
                }
                Array.Copy(data, 0, m_buffer, m_regionAPointer + m_regionASize, bytes);
                m_regionASize += bytes;
                return true;
            }
        }

        public UInt32 GetSpace()
        {
            if (m_regionBPointer != int.MinValue)
            {
                return GetBFreeSpace();
            }
            else
            {
                if (GetAFreeSpace() < GetSpaceBeforeA())
                {
                    AllocateB();
                    return GetBFreeSpace();
                }
                else
                    return GetAFreeSpace();
            }
        }

        public UInt32 GetSize() { return m_regionASize + m_regionBSize; }

        public UInt32 GetContiguiousBytes()
        {
            if (m_regionASize > 0)
                return m_regionASize;
            else
                return m_regionBSize;
        }

        public void Remove(UInt32 len)
        {
            UInt32 cnt = len;
            UInt32 aRem, bRem;

            if (m_regionASize > 0)
            {
                aRem = (cnt > m_regionASize) ? m_regionASize : cnt;
                m_regionASize -= aRem;
                m_regionAPointer += (Int32)aRem;
                cnt -= aRem;
            }

            if (cnt > 0 && m_regionBSize > 0)
            {
                bRem = (cnt > m_regionBSize) ? m_regionBSize : cnt;
                m_regionBSize -= bRem;
                m_regionBPointer += (Int32)bRem;
                cnt -= bRem;
            }

            if (m_regionASize == 0)
            {
                if (m_regionBSize > 0)
                {
                    if (m_regionBPointer != 0)
                        Buffer.BlockCopy(m_buffer, m_regionBPointer, m_buffer, 0, (Int32)m_regionBSize);

                    m_regionAPointer = 0;
                    m_regionASize = m_regionBSize;
                    m_regionBPointer = int.MinValue;
                    m_regionBSize = 0;
                }
                else
                {
                    m_regionBPointer = int.MinValue;
                    m_regionBSize = 0;
                    m_regionAPointer = 0;
                    m_regionASize = 0;
                }
            }
        }

        public byte[] GetBuffer()
        {
            UInt32 size = 64000;
            if (m_regionBPointer != int.MinValue)
            {

                byte[] temp = new byte[size - m_regionBSize];
                Array.Copy(m_buffer, m_regionBPointer, temp, 0, size - m_regionBSize);
                return temp;
            }
            else
            {
                byte[] temp = new byte[size - m_regionASize];
                Array.Copy(m_buffer, m_regionAPointer, temp, 0, size - m_regionASize);
                return temp;
            }
        }

        public void CopyBack(byte[] src, UInt32 len)
        {
            if (m_regionBPointer != int.MinValue)
            {
                Array.Copy(src, 0, m_buffer, m_regionBPointer + m_regionBSize, len);
                m_regionBSize += len;
            }
            else
            {
                Array.Copy(src, 0, m_buffer, m_regionAPointer + m_regionBSize, len);
                m_regionASize += len;
            }
        }

        public void Allocate(UInt32 size)
        {
            m_buffer = new byte[size];
            m_bufferEnd = (Int32)size;
            m_regionAPointer = 0;
        }

        public Int32 GetBufferStartPos()
        {
            if (m_regionASize > 0) return m_regionAPointer;
            else return m_regionBPointer;
        }

	    public byte[] GetBuffFromAndLen(ref byte[] buff, UInt32 len)
	    {
		    if (m_regionBPointer != int.MinValue)
		    {
			    Array.Copy(m_buffer, m_regionBPointer, buff, 0, len);
		    }
		    else
		    {
			    Array.Copy(m_buffer, m_regionAPointer, buff, 0, len);
		    }
		    return buff;
	    }
    }
}
