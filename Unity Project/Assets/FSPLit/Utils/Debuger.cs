using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Debuger
{
    public static void Log() { }

    public static void Log(string msg, params object[] args) { }

    public static void LogError(string msg, params object[] args) { }

    public static void LogWarning(string msg, params object[] args) { }
}

public static class DebugExtension
{
    public static void Log(this object obj, string msg, params object[] args) { }

    public static void LogError(this object obj, string msg, params object[] args) { }

    public static void LogWarning(this object obj, string msg, params object[] args) { }

    internal static void Log(string v, int length, string name, object p)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 将容器序列化成字符串
    /// 格式：{a, b, c}
    /// </summary>
    public static string ListToString<T>(this IEnumerable<T> source)
    {
        if (source == null)
        {
            return "null";
        }

        if (source.Count() == 0)
        {
            return "[]";
        }

        if (source.Count() == 1)
        {
            return "[" + source.First() + "]";
        }

        var s = "";

        s += source.ButFirst().Aggregate(s, (res, x) => res + ", " + x.ToListString());
        s = "[" + source.First().ToListString() + s + "]";

        return s;
    }


    /// <summary>
    /// 将容器序列化成字符串
    /// </summary>
    public static string ToListString(this object obj)
    {
        if (obj is string)
        {
            return obj.ToString();
        }
        else
        {
            var objAsList = obj as IEnumerable;
            return objAsList == null ? obj.ToString() : objAsList.Cast<object>().ListToString();
        }
    }

    public static IEnumerable<T> ButFirst<T>(this IEnumerable<T> source)
    {
        return source.Skip(1);
    }
}
