using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Entry : MonoBehaviour
{
    public static bool isClient = false;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10,10,200,50), "登录客户端"))
        {
            isClient = true;
            SceneManager.LoadScene("Client");
        }
        else if (GUI.Button(new Rect(10, 70, 200, 50), "启动虚拟服务器"))
        {
            isClient = false;
            SceneManager.LoadScene("Server");
        }
    }

}
