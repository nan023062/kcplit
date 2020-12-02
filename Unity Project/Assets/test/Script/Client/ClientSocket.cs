using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;

public delegate void OnHandleNetMsg(NetPackage msg);

public class ClientSocket
{
    public ClientSocket()
    {
        connected = false;
        socket = null;
        __sendPackageQueue = new Queue<NetPackage>();
        __receivePackageQueue = new Queue<NetPackage>();
        onHandleNetMsg = null;
    }

    public bool connected { private set; get; }

    public string ip { private set; get; }

    public ushort port { private set; get; }

    public Socket socket { private set; get; }

    public event OnHandleNetMsg onHandleNetMsg = null;

    public Boolean ConnectTo(string ip, ushort port)
    {
        this.ip = ip;
        this.port = port;

        IPAddress[] ips;
        try
        {
            ips = Dns.GetHostAddresses(ip);
        }
        catch (System.Exception e)
        {
            GameLog.LogError("IPAddress fail : " + e.Message);
            return false;
        }

        try
        {
            IPEndPoint ipe = new IPEndPoint(ips[0], port);
            socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.Connect(ipe);
            connected = true;
            socket.ReceiveBufferSize = __RevBufSize;
            return true;
        }
        catch (Exception ex)
        {
            GameLog.LogError("Connect To Server Fail : " + ex.Message);
            return false;
        }
    }

    public void Update(float deltaTime)
    {
        if (socket == null || !connected) return;

        try
        {
            if (socket.Poll(0, SelectMode.SelectError))
            {
                //OnDisconnect(SocketError.Success);
                return;
            }
            else if (socket.Poll(0, SelectMode.SelectRead))
            {
                ReceiveMsg(deltaTime);
            }
            else if (socket.Poll(0, SelectMode.SelectWrite))
            {
                SendAllMsg(deltaTime);
            }
        }
        catch (Exception ex)
        {
            GameLog.LogError("socket is error : {0} !", ex.Message);
        }

        HandleNetPackage();
    }

    #region SendMsg

    private SmartBuffer __sendBuffer = new SmartBuffer();
    private Queue<NetPackage> __sendPackageQueue = null;

    private void SendAllMsg(float deltaTime)
    {
        if (!connected) return;

        while (__sendPackageQueue.Count > 0)
        {
            NetPackage package = __sendPackageQueue.Dequeue();
            NetworkTools.WriteToBuffer(package, ref __sendBuffer);
            socket.Send(__sendBuffer.Buffer(), __sendBuffer.Size(), SocketFlags.None);
            NetPackage.Put(package);
        }
    }

    public void SendNetPackage<T>(int opcode, T msg)
    {
        NetPackage package = NetPackage.Get();
        package.opcode = opcode;
        NetworkTools.Serialize<T>(msg, package);
        __sendPackageQueue.Enqueue(package);
    }

    public void SendNetPackage(int opcode, byte[] msg)
    {
        NetPackage package = NetPackage.Get();
        package.opcode = opcode;
        package.In(msg, msg.Length);
        __sendPackageQueue.Enqueue(package);
    }

    #endregion

    #region ReceiveMsg

    private const int __RevBufSize = 256 * 1024;
    private byte[] __recveBuffer = new byte[__RevBufSize];
    private Queue<NetPackage> __receivePackageQueue = null;

    private void ReceiveMsg(float deltaTime)
    {
        if (socket == null || !connected) return;
        if (socket.Available <= 0) return;

        NetworkTools.CheckBuffer(ref __recveBuffer, socket.Available);

        int nRecv = socket.Receive(__recveBuffer, SocketFlags.None);
        if (nRecv >= NetPackage.HEAD_SIZE)
        {
            NetPackage package = NetPackage.Get();
            package.opcode = BitConverter.ToInt32(__recveBuffer, 0);
            int bodySize = BitConverter.ToInt32(__recveBuffer, 4);
            package.In(__recveBuffer, NetPackage.HEAD_SIZE, bodySize);
            __receivePackageQueue.Enqueue(package);
        }
    }

    private void HandleNetPackage()
    {
        if (__receivePackageQueue.Count > 0)
        {
            NetPackage package = __receivePackageQueue.Dequeue();
            if (onHandleNetMsg != null)
                onHandleNetMsg.Invoke(package);
            NetPackage.Put(package);
        }
    }

    #endregion


}
