using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine;
public class ServerManager : MonoBehaviour
#else
public class ServerManager
#endif
{
    ServerSocket _serverSocket = null;
    NetPackage __sendPackage = null;

    // Start is called before the first frame update
    public void Start()
    {
        _serverSocket = new ServerSocket();
        _serverSocket.BeginListening();
        __sendPackage = new NetPackage();
    }

    public void SendTestMessage()
    {
        pb.MarketBuyContent msg = new pb.MarketBuyContent();
        msg.give_typ = 315774101;
        msg.give_val = 20130915;
        _serverSocket.BroadCastToClients(101, msg);
    }

#if UNITY_EDITOR

    int count = 0;
    string log = "";

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "发送消息"))
        {
            SendTestMessage();
            count++;
        }

        string content = GameLog.GetLogStack();
        if (!string.IsNullOrEmpty(content)) log = content;
        GUI.TextField(new Rect(10, 100, 500, 300), log);
    }

    private void Update()
    {
        GameLog.UpdateNextLogString();
    }
#endif

}