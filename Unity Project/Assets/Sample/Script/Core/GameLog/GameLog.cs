using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if UNITY_EDITOR 
using UnityEngine;
#endif

public static class GameLog
{
    public static string logFilePath = "game_log.txt";
    private static FileStream fileStream = null;

    private static Queue<string> LogQueue = new Queue<string>();

    private static List<string> StackofLogs = new List<string>();

    private static StringBuilder _StringBuilder = new StringBuilder();

    public static string UpdateNextLogString()
    {
        if (LogQueue.Count > 0)
        {
            string log = LogQueue.Dequeue() + "\n";
            if (StackofLogs.Count >= 20)
            {
                StackofLogs.RemoveAt(0);
            }
            StackofLogs.Add(log);

            if( fileStream == null)
            {
                fileStream = File.Create(CurTimeString() + "_" + logFilePath);
            }
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(log);
            fileStream.Write(bytes, 0, bytes.Length);
            return log;
        }
        return null;
    }

    public static string GetLogStack(bool clear = false)
    {
        _StringBuilder.Clear();
        foreach (var log in StackofLogs)
        {
            _StringBuilder.Append(log);
        }
        if(clear)
        {
            StackofLogs.Clear();
        }
        return _StringBuilder.ToString();
    }

    public static void Log(string format, params object[] args)
    {
        LogQueue.Enqueue(string.Format("[I]" + format + " <" + GetCurrentTime() + ">", args));
    }

    public static void LogWarning(string format, params object[] args)
    {
        LogQueue.Enqueue(string.Format("[W]" + format + " <" + GetCurrentTime() + ">", args));
    }

    public static void LogError(string format, params object[] args)
    {
        LogQueue.Enqueue(string.Format("[E]" + format + " <" + GetCurrentTime() + ">", args));
    }

    /// 获取当前系统时间的方法       
    static DateTime GetCurrentTime()
    {
        return DateTime.Now;
    }

    static string CurTimeString()
    {
        return DateTime.Now.ToLongDateString();
    }
}

