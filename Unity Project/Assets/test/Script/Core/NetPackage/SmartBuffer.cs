using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

/// <summary>
/// 网络字节包
/// </summary>
public class SmartBuffer
{
    public const int DefaultSize = 256;

    private byte[] __buffer = new byte[DefaultSize];
    private int __capacity = DefaultSize;
    private byte[] __InOut_buf = new byte[256];
    private int __ReadPos = 0;
    private int __WritePos = 0;
    private int __bufferSize = 0;
    private StringBuilder __Builder;

    public SmartBuffer()
    {
        __Builder = new StringBuilder();
    }

    public int Size() { return __bufferSize; }
    public byte[] Buffer() { return __buffer; }

    public void Reset()
    {
        __bufferSize = __WritePos = __ReadPos = 0;
    }

    public override string ToString()
    {
        __Builder.Clear();
        for (int i = 0; i < __capacity; i++)
        {
            __Builder.Append("[").Append(i).Append("]:").Append(__buffer[i]).Append("|");
        }
        return __Builder.ToString();
    }

    //扩大容量,修改有效字节数
    protected void _ReBufferSize(int newSize)
    {
        if (newSize > __capacity)
        {
            __capacity = (newSize > __capacity * 2) ? newSize : (__capacity * 2);

            if (__bufferSize > 0)
            {
                Byte[] tempbuff = __buffer; 
                __buffer = new Byte[__capacity];
                Array.Copy(tempbuff, __buffer, __bufferSize);
                tempbuff = null;
            }
            else
            {
                __buffer = new Byte[__capacity];
            }
        }

        if (__bufferSize < newSize)
            __bufferSize = newSize;
    }

    //写入字节
    protected void Write(byte[] bytes, int size)
    {
        _ReBufferSize(__bufferSize + size);
        Array.Copy(bytes, 0, __buffer, __WritePos, size);
        __WritePos += size;
    }
    protected void Write(byte[] bytes, int offset, int size)
    {
        if (offset < 0) return;
        _ReBufferSize(__bufferSize + size);
        Array.Copy(bytes, offset, __buffer, __WritePos, size);
        __WritePos += size;
    }

    //读取字节
    protected int Read(ref byte[] outBuffer, int size)
    {
        if (__bufferSize - 1 < __ReadPos) return 0;

        int nOutBuffSize = outBuffer.Length;

        size = Math.Min(size, nOutBuffSize);
        size = Math.Min(size, __bufferSize - __ReadPos);

        Array.Copy(__buffer, __ReadPos, outBuffer, 0, size);
        __ReadPos += size;
        return size;
    }
    protected int Read(ref byte[] outBuffer, int offset, int size)
    {
        if (__bufferSize - 1 < __ReadPos) return 0;

        int nOutBuffSize = outBuffer.Length;

        if (offset < 0 || offset > nOutBuffSize - 1) return 0;

        size = Math.Min(size, nOutBuffSize - 1 - offset);
        size = Math.Min(size, __bufferSize - __ReadPos);

        Array.Copy(__buffer, __ReadPos, outBuffer, offset, size);
        __ReadPos += size;
        return size;
    }

    public SmartBuffer In(char value)
    {
        __InOut_buf[0] = (byte)value;
        Write(__InOut_buf, sizeof(char));
        return this;
    }
    public SmartBuffer Out(out char value)
    {
        Read(ref __InOut_buf, sizeof(char));
        value = (char)__InOut_buf[0];
        return this;
    }

    public SmartBuffer In(bool value)
    {
        if (value)
            __InOut_buf[0] = (byte)1;
        else
            __InOut_buf[0] = (byte)0;
        Write(__InOut_buf, sizeof(bool));

        return this;
    }
    public SmartBuffer Out(out bool value)
    {
        Read(ref __InOut_buf, sizeof(bool));
        value = BitConverter.ToBoolean(__InOut_buf, 0);
        return this;
    }

    public SmartBuffer In(Byte value)
    {
        __InOut_buf[0] = value;
        Write(__InOut_buf, sizeof(Byte));
        return this;
    }
    public SmartBuffer Out(out Byte value)
    {
        Read(ref __InOut_buf, sizeof(byte));
        value = __InOut_buf[0];
        return this;
    }

    public SmartBuffer In(Int16 value)
    {
        Write(BitConverter.GetBytes(value), sizeof(Int16));
        return this;
    }
    public SmartBuffer Out(out Int16 value)
    {
        Read(ref __InOut_buf, sizeof(Int16));
        value = BitConverter.ToInt16(__InOut_buf, 0);
        return this;
    }

    public SmartBuffer In(UInt16 value)
    {
        Write(BitConverter.GetBytes(value), sizeof(UInt16));
        return this;
    }
    public SmartBuffer Out(out UInt16 value)
    {
        Read(ref __InOut_buf, sizeof(UInt16));
        value = BitConverter.ToUInt16(__InOut_buf, 0);
        return this;
    }

    public SmartBuffer In(Int32 value)
    {
        Write(BitConverter.GetBytes(value), sizeof(Int32));
        return this;
    }
    public SmartBuffer Out(out Int32 value)
    {
        Read(ref __InOut_buf, sizeof(Int32));
        value = BitConverter.ToInt32(__InOut_buf, 0);
        return this;
    }

    public SmartBuffer In(UInt32 value)
    {
        Write(BitConverter.GetBytes(value), sizeof(UInt32));
        return this;
    }
    public SmartBuffer Out(out UInt32 value)
    {
        Read(ref __InOut_buf, sizeof(UInt32));
        value = BitConverter.ToUInt32(__InOut_buf, 0);
        return this;
    }

    public SmartBuffer In(Int64 value)
    {
        Write(BitConverter.GetBytes(value), sizeof(Int64));
        return this;
    }
    public SmartBuffer Out(out Int64 value)
    {
        Read(ref __InOut_buf, sizeof(Int64));
        value = BitConverter.ToInt64(__InOut_buf, 0);
        return this;
    }

    public SmartBuffer In(UInt64 value)
    {
        Write(BitConverter.GetBytes(value), sizeof(UInt64));
        return this;
    }
    public SmartBuffer Out(out UInt64 value)
    {
        Read(ref __InOut_buf, sizeof(UInt64));
        value = BitConverter.ToUInt64(__InOut_buf, 0);
        return this;
    }

    public SmartBuffer In(String value)
    {
        UInt16 size = (UInt16)Encoding.UTF8.GetByteCount(value);
        In(size);
        if (size > 0) Write(Encoding.UTF8.GetBytes(value), size);
        return this;
    }
    public SmartBuffer Out(out String value)
    {
        value = String.Empty;
        UInt16 size = 0;
        Out(out size);
        if (size > 0)
        {
            byte[] temp = new byte[size];
            Read(ref temp, size);
            value = Encoding.UTF8.GetString(temp);
            temp = null;
        }
        return this;
    }

    public SmartBuffer In(float value)
    {
        Write(BitConverter.GetBytes(value), sizeof(float));
        return this;
    }
    public SmartBuffer Out(out float value)
    {
        Read(ref __InOut_buf, sizeof(float));
        value = BitConverter.ToSingle(__InOut_buf, 0);
        return this;
    }

    public SmartBuffer In(char[] value)
    {
        Int32 size = Encoding.ASCII.GetByteCount(value);
        _ReBufferSize(size + sizeof(Int32));
        try
        {
            In(size);
            if (size > 0) Write(Encoding.ASCII.GetBytes(value), size);
        }
        catch (Exception e)
        {
            GameLog.LogError(e.Message);
        }
        return this;
    }
    public SmartBuffer Out(out char[] value)
    {
        value = null;
        Int32 size = 0;
        Out(out size);
        if (size > 0)
        {
            byte[] temp = new byte[size];
            Read(ref temp, size);
            value = Encoding.ASCII.GetChars(temp);
        }
        return this;
    }

    public SmartBuffer In(byte[] value, Int32 size)
    {
        Write(value, size);
        return this;
    }
    public SmartBuffer In(byte[] value, Int32 offset, Int32 size)
    {
        Write(value, offset, size);
        return this;
    }
    public SmartBuffer Out(ref byte[] value, Int32 size)
    {
        Read(ref value, size);
        return this;
    }
}
