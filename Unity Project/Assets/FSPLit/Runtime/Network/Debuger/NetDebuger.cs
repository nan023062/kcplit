using System;
using System.Collections.Generic;
using Engine.Network.protocol;

namespace Engine.Network
{
    public static class NetDebuger
    {
        public const string LOG_TAG = "NetDebuger";

        public static string DbgFileName { get; set; }

        public static string DbgFileDir { get; set; }

        public static bool EnableLog = false;

        public static bool EnableProfiler = false;

        public static bool EnableWeakNet = false;           //弱网络测试

        public static float PacketLossRate = 0;             //丢包率

        public static float JitterRate = 0;                 //抖动率

        public static int JitterDelayMin = 0;               //MS

        public static int JitterDelayMax = 0;

        #region Profiler采样

        private static List<NetSampleItem> m_ListNetSamples = new List<NetSampleItem>();

        public static List<NetSampleItem> SampleList
        {
            get { return m_ListNetSamples; }
        }

        public static void ClearSample()
        {
            m_ListNetSamples.Clear();
        }

        public static void AddSample(string tag, string name, int data1 = 0, int data2 = 0, int data3 = 0)
        {
            if (EnableProfiler)
            {
                NetSampleItem item = new NetSampleItem();
                item.name = tag + ":" + name;
                item.time = DateTime.Now.Ticks;
                item.data1 = data1;
                item.data2 = data2;
                item.data3 = data3;
                m_ListNetSamples.Add(item);
            }
        }

        #endregion

        #region 弱网络测试
        public static int GetJitterDelay()
        {
            if (IsNetJitter())
            {
                return Nave.FSPLit.math.Random.Default.Range(JitterDelayMin, JitterDelayMax);
            }
            return 0;
        }

        public static bool IsNetJitter()
        {
            return EnableWeakNet && Nave.FSPLit.math.Random.Default.Rnd() < JitterRate;
        }

        public static bool IsPacketLoss()
        {
            return EnableWeakNet && Nave.FSPLit.math.Random.Default.Rnd() < PacketLossRate;
        }

        public static void WeakNetSimulate(object target, byte[] buffer, int size, Action<byte[], int> handler)
        {
            
        }

        public static void WeakNetCancel(object target)
        {

        }

        private static void OnDelayInvoke(object[] args)
        {
            Action<byte[], int> handler = (Action<byte[], int>)args[0];

            if (handler != null)
            {
                handler((byte[])args[1], (int)args[2]);
            }
        }

        #endregion
    }
}


