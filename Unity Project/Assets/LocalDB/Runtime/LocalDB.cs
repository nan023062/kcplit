using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Nave.DB
{
    /// <summary>
    /// 本地缓存数据库对象。
    /// 1 负责管理所有缓存数据
    /// 2 提供数据增删查改功能
    /// 3 提供全局事件通知
    /// </summary>
    public sealed class LocalDB : DBDictionary<IDBElement>
    {
        public delegate void DBEvent(IDBElement element);

        public delegate void DBMsgEvent(string group, object msg);
        private LocalDB() { m_Name = "LocalDB"; }

        private static LocalDB s_Instance;

        public static event DBMsgEvent onSendDBMsg;

        public static event DBEvent onDBChangeEvent;

        private static string s_domain = string.Empty;

        static LocalDB() {  s_Instance = new LocalDB(); }

        /// <summary>
        /// 设置DB类型的程序集名称，用作后期反射调用
        /// </summary>
        /// <param name="assemblyString">程序集</param>
        public static void InitDomain(string assemblyString)
        {
            s_domain = assemblyString;
        }

        /// <summary>
        /// 查找一个数据-T泛型
        /// </summary>
        public static T Get<T>() where T : class, IDBGroup
        {
            Type type = typeof(T);
            var result = s_Instance[type.Name];
            if(result == null) {
                if (!type.IsAbstract) {
                    result = Activator.CreateInstance<T>();
                    s_Instance[type.Name] = result;
                }
            }
            return result as T;
        }

        /// <summary>
        /// 查找一个数据
        /// </summary>
        public static IDBGroup Get(string name)
        {
            var result = s_Instance[name];
            if (result == null) {
                Assembly asb = Assembly.Load(s_domain);
                Type type = asb.GetType(name);
                if(type == null) return null;
                if (!type.IsAbstract && type.IsClass && type.IsSubclassOf(typeof(IDBGroup))) {
                    result = Activator.CreateInstance(type) as IDBGroup;
                    s_Instance[name] = result;
                }
            }
            return result as IDBGroup;
        }

        /// <summary>
        /// DB发生变化时执行
        /// </summary>
        internal static void SendDBMsg(IDBElement element)
        {
            NoticDBChanged(element);

            var group = element.group != null ? element.group.name : element.name;
            onSendDBMsg?.Invoke(group, element.PackMsg());   
        }

        internal static void NoticDBChanged(IDBElement element)
        {
            onDBChangeEvent?.Invoke(element);
        }

        public static void HandleDBMsg(string groupName, object msg)
        {
            var group = Get(groupName);
            group.UnpackMsg(msg);
        }

        protected override IDBElement UnpackItemMsg(object msg)
        {
            throw new NotImplementedException();
        }
    }
}


