using System;
using time_t = System.UInt64;

namespace Nave.FSPLit
{
    public static class TimeSystem
    {
        private static time_t startServerTimeMilliseconds = 0;
        private static time_t serverGMTOffset = 0; //当地与服务器时差
        private static DateTime timeSinceSync = new DateTime(1970, 1, 1, 8, 0, 0);
        private static DateTime timeSinceStartup = timeSinceSync;

        /// <summary>
        /// 初始化程序
        /// </summary>
        /// <param name="GMTOffset"></param>
        public static void SetLocalTime(DateTime _timeSinceStartup, time_t GMTOffset = 0)
        {
            serverGMTOffset = GMTOffset;
            timeSinceStartup = _timeSinceStartup;
        }

        /// <summary>
        /// 同步服务器时间戳
        /// </summary>
        public static void SetGlobalTime(time_t serverMilliseconds)
        {
            startServerTimeMilliseconds = serverMilliseconds;
            timeSinceSync = DateTime.Now;
        }

        /// <summary>
        /// 同步时间
        /// </summary>
        public static time_t GetSecondsSinceSyncTime()
        {
            DateTime nowtime = DateTime.Now.ToLocalTime();
            return (time_t)(nowtime.Subtract(timeSinceSync).TotalSeconds);
        }

        /// <summary>
        /// 应用程序时间
        /// </summary>
        public static time_t GetSecondsSinceStartup()
        {
            DateTime nowtime = DateTime.Now.ToLocalTime();
            return (time_t)(nowtime.Subtract(timeSinceStartup).TotalSeconds);
        }

        /// <summary>
        /// 当前服务器时间戳
        /// </summary>
        public static time_t curServerTimeMilliseconds
        {
            get
            {
                return startServerTimeMilliseconds + 1000* GetSecondsSinceSyncTime();
            }
        }

        /// <summary>
        /// 当前服务器时间秒
        /// </summary>
        public static float curServerTime
        {
            get { return curServerTimeMilliseconds*0.001f; }
        }

        /// <summary>
        /// 离目标时间剩余秒数
        /// </summary>
        public static ulong GetLeftSeconds(time_t targetMilliseconds)
        {
            return System.Math.Max(0, (targetMilliseconds - curServerTimeMilliseconds)*1000);
        }

        /// <summary>
        /// 获取当地时间
        /// </summary>
        public static time_t LocalTime(time_t time)
        {
            return time - serverGMTOffset;
        }
    }
}

