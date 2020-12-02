using System;
using System.IO;
using ProtoBuf;

public static class NetworkTools
{
    public static void CheckBuffer(ref byte[] tempBuffer, int minSize)
    {   
        if(tempBuffer == null)
        {
            tempBuffer = new byte[minSize];
        }
        else if(tempBuffer.Length < minSize)
        {
            int length = Math.Max(minSize, tempBuffer.Length * 2);
            tempBuffer = new byte[length];
        }
    }

    public static void Serialize<T>(T instance, NetPackage package)
    {
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, instance);
                package.In(ms.GetBuffer(), (int)ms.Length);
            }
        }
        catch (Exception ex)
        {
            GameLog.LogError("Serialize() 失败! msg = {0} !", ex.Message);
        }
    }

    public static T DeSerialize<T>(NetPackage package) where T : class
    {
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(package.Buffer(), 0, package.Size());
                ms.Position = 0;
                return ProtoBuf.Serializer.Deserialize(typeof(T), ms) as T;
            }
        }
        catch (Exception ex)
        {
            GameLog.LogError("反序列化失败: " + ex.ToString());
            return default(T);
        }
    }

    public static object DeSerialize(NetPackage package)
    {
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(package.Buffer(), 0, package.Size());
                ms.Position = 0;
                return ProtoBuf.Serializer.Deserialize(GetTypeByOpcode(package.opcode), ms);
            }
        }
        catch (Exception ex)
        {
            GameLog.LogError("反序列化失败: " + ex.ToString());
            return null;
        }
    }

    public static Type GetTypeByOpcode(int opcode)
    {
        if (opcode == 101) return typeof(pb.Mail_UserMailInfo);
        return null;
    }

    public static void WriteToBuffer(NetPackage package, ref SmartBuffer buffer)
    {
        buffer.Reset();
        buffer.In(package.opcode);
        buffer.In(package.Size());
        buffer.In(package.Buffer(), package.Size());
    }

    public static int ReadInt32FromBuffer(byte[] buffer, int beginLen)
    {
        return (int)buffer[beginLen]
                | (int)buffer[beginLen + 1] << 8
                | (int)buffer[beginLen + 2] << 16
                | (int)buffer[beginLen + 3] << 24;
    }

    public static uint ReadUInt32FromBuffer(byte[] buffer, int beginLen)
    {
        return (uint)buffer[beginLen]
                | (uint)buffer[beginLen + 1] << 8
                | (uint)buffer[beginLen + 2] << 16
                | (uint)buffer[beginLen + 3] << 24;
    }

    public static short ReadShortFromBuffer(byte[] buffer, int beginLen)
    {
        return (short)((int)buffer[beginLen]
                       | (int)buffer[beginLen + 1] << 8);
    }

    public static UInt16 ReadUInt16FromBuffer(byte[] buffer, int beginLen)
    {
        return (UInt16)((int)buffer[beginLen]
                       | (int)buffer[beginLen + 1] << 8);
    }
}

