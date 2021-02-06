using System.Collections.Generic;

namespace Nave.Network.IPCLit
{
    /// <summary>
    /// IPC地址和端口信息
    /// </summary>
    public class IPCInfo
    {
        public int id;
        public int port;
    }

    /// <summary>
    /// IPC配置
    /// </summary>
    public class IPCConfig
    {
        private readonly static string Path = "./IPCConfig.json";

        private readonly static MapList<int, IPCInfo> s_MapIPCInfo = new MapList<int, IPCInfo>();

        /// <summary>
        /// 获取配置的的IPC地址信息
        /// </summary>
        /// <param name="id">远程进程ID</param>
        /// <returns></returns>
        public static IPCInfo GetIPCInfo(int id)
        {
            if (s_MapIPCInfo.Count == 0)
                ReadConfig();
            return s_MapIPCInfo[id];
        }

        private static void ReadConfig()
        {
            Debuger.Log();
            string jsonStr = FileUtils.ReadString(Path);
            var obj = MiniJSON.Json.Deserialize(jsonStr) as List<object>;
            for (int i = 0; i < obj.Count; i++)
            {
                var infoJson = obj[i] as Dictionary<string, object>;
                IPCInfo info = new IPCInfo();
                info.id = (int)(long)infoJson["id"];
                info.port = (int)(long)infoJson["port"];
                s_MapIPCInfo.Add(info.id, info);
            }
        }
    }
}