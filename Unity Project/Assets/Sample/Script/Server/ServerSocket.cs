using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using Nave.Network;

public class ServerSocket
{
    private Socket socket;

    //定义一个集合，存储客户端信息
    private Dictionary<string, Socket> clientSockets = new Dictionary<string, Socket>();

    public void BeginListening()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //服务端发送信息需要一个IP地址和端口号  
        IPEndPoint ipAndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8098);
        socket.Bind(ipAndPoint);

        //将套接字的监听队列长度限制为20  
        socket.Listen(20);

        //负责监听客户端的线程:创建一个监听线程  
        Thread threadListening = new Thread(Watchconnecting);
        threadListening.IsBackground = true;
        threadListening.Start();

        GameLog.Log("开启监听......");
    }

    private SmartBuffer __sendBUffer = new SmartBuffer();

    public void  BroadCastToClients<T>(int opcode, T msg)
    {
        var package = NetPackage.Get();
        package.opcode = opcode;
        NetworkTools.Serialize<T>(msg, package);
        NetworkTools.WriteToBuffer(package, ref __sendBUffer);

        foreach (var _clientSocket in clientSockets.Values)
        {
            IPEndPoint netpoint = _clientSocket.RemoteEndPoint as IPEndPoint;
            GameLog.Log("发送消息给客户端： {0} ！", netpoint);
            _clientSocket.Send(__sendBUffer.Buffer(), 0, __sendBUffer.Size(), SocketFlags.None);
        }
    }

    //监听客户端发来的请求  
    private void Watchconnecting()
    {
        //持续不断监听客户端发来的请求     
        while (true)
        {
            try
            {
                Socket connection = socket.Accept();

                //获取客户端的IP和端口号 
                IPEndPoint netpoint = connection.RemoteEndPoint as IPEndPoint;
                IPAddress clientIP = netpoint.Address;
                int clientPort = netpoint.Port;

                //让客户显示"连接成功的"的信息  
                string sendmsg = "连接服务端成功！\r\n" + "本地IP:" + clientIP + "，本地端口" + clientPort.ToString();
                GameLog.Log(sendmsg);

                //添加客户端信息  
                clientSockets.Add(netpoint.ToString(), connection);

                //创建一个通信线程      
                var pts = new ParameterizedThreadStart(recvThread);

                //设置为后台线程，随着主线程退出而退出 
                Thread thread = new Thread(pts);             
                thread.IsBackground = true;     
                thread.Start(connection);
            }
            catch (Exception ex)
            {
                //提示套接字监听异常     
                GameLog.LogError("提示套接字监听异常 : {0}", ex.Message);
                break;
            }
        }
    }

    /// <summary>
    /// 接收客户端发来的信息，客户端套接字对象
    /// </summary>
    /// <param name="socketclientpara"></param>    
    private void recvThread(object socketclientpara)
    {
        Socket socketServer = socketclientpara as Socket;
  
        byte[] __recvBuffer = new byte[1024 * 256];

        while (true)
        {
            //将接收到的信息存入到内存缓冲区，并返回其字节数组的长度    
            try
            {
                int nRecv = socketServer.Receive(__recvBuffer);
                if (nRecv >= NetPackage.HEAD_SIZE)
                {
                    NetPackage package = NetPackage.Get();
                    package.opcode = BitConverter.ToInt32(__recvBuffer, 0);
                    int bodySize = BitConverter.ToInt32(__recvBuffer, 4);
                    package.In(__recvBuffer, NetPackage.HEAD_SIZE, bodySize);
                    OnHandleNetPackage(package);
                    NetPackage.Put(package);
                }
            }
            catch (Exception ex)
            {
                clientSockets.Remove(socketServer.RemoteEndPoint.ToString());

                GameLog.Log("Client Count:" + clientSockets.Count);

                //提示套接字监听异常  
                GameLog.Log("客户端" + socketServer.RemoteEndPoint + "已经中断连接" + "\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n");

                //关闭之前accept出来的和客户端进行通信的套接字 
                socketServer.Close();
                break;
            }

            Thread.Sleep(100);
        }
    }
    
    static void OnHandleNetPackage(NetPackage package)
    {
        if (package.opcode == 101)
        {
            var msg = NetworkTools.DeSerialize<pb.Mail_UserMailInfo>(package);
            GameLog.Log("接受到客户端101消息, 内容为：mail_id = {0}, content = {1}, send_time = {2}！", msg.mail_id, msg.content, msg.send_time);
        }
    }

}
