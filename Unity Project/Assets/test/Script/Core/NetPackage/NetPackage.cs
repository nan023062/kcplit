using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class NetPackage : SmartBuffer
{
    public const int MAX_SIZE = 256;
    public const int HEAD_SIZE = 8;  //前4表示ID 后4表示长度

    static Queue<NetPackage> packagePool = null;

    public static NetPackage Get()
    {
        if (packagePool == null)
        {
            packagePool = new Queue<NetPackage>();
        }
        if (packagePool.Count <= 0)
        {
            return new NetPackage();
        }
        else
        {
            return packagePool.Dequeue();
        }
    }

    public static void Put(NetPackage package)
    {
        if (packagePool == null)
        {
            packagePool = new Queue<NetPackage>();
        }
        package.opcode = 0;
        package.Reset();
        packagePool.Enqueue(package);
    }

    public Int32 opcode = 0;

    public static void Copy(NetPackage src, NetPackage dest)
    {
        dest.opcode = src.opcode;
        dest._ReBufferSize(src.Size());
        Array.Copy(src.Buffer(),0, dest.Buffer(),0, src.Size());
    }
}

