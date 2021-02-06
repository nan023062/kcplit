using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using pb;
using LocalDB = Nave.DB.LocalDB;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance;

    public ClientSocket socket;

    private void Awake()
    {
        Instance = this;
        socket = new ClientSocket();
        socket.onHandleNetMsg -= OnHandleNetMsg;
        socket.onHandleNetMsg += OnHandleNetMsg;
        __sendPackage = new NetPackage();

        LocalDB.InitDomain("Assembly-CSharp");
        LocalDB.onSendDBMsg -= TestEventMsg;
        LocalDB.onSendDBMsg += TestEventMsg;
        var a = LocalDB.Get("db.UserInfo_Base");
        var b = LocalDB.Get<db.UserInfo>();
        var c = LocalDB.Get<db.TaskData>();
    }

    private void TestEventMsg(string group, object msg)
    {
        UnityEngine.Debug.Log($" >>>> TestEventMsg : group = {group}, msg = {msg}");
    }

    void Update()
    {
        socket.Update(Time.deltaTime);

        GameLog.UpdateNextLogString();
    }

    string log = "";
    NetPackage __sendPackage = null;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "连接服务器"))
        {
            socket.ConnectTo("127.0.0.1", 8098);
        }

        if (GUI.Button(new Rect(10,70,200,50), "发送消息"))
        {
            pb.Mail_UserMailInfo msg = new Mail_UserMailInfo();
            msg.mail_id = 5201314;
            msg.content = "我爱你一生一世";
            msg.send_time = 15002783291;
            socket.SendNetPackage(101, msg);
        }

        String content = GameLog.GetLogStack();
        if (!String.IsNullOrEmpty(content)) log = content;
        GUI.TextField(new Rect(10, 120, 500, 300), log);
    }

    private void OnHandleNetMsg(NetPackage package)
    {
        var msg = NetworkTools.DeSerialize<pb.MarketBuyContent>(package);
        GameLog.Log("OnHandleNetMsg() opcode = {0}, give_typ = {1}, give_val = {1} !", package.opcode, msg.give_typ, msg.give_val);
    }


}
