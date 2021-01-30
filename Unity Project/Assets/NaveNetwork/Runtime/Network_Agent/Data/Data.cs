using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nave.Network
{
    public sealed class Data
    {
        private Dictionary<string, Property> mProperties;

        public Data()
        {
            mProperties = new Dictionary<string, Property>();
        }

        public void AddProperty(Property property)
        {
            mProperties.Add(property.name, property);
        }

        public T GetProperty<T>(string propertyName) where T : Property
        {
            Property property = null;
            if (mProperties.TryGetValue(propertyName, out property))
            {
                return property as T;
            }
            return null;
        }

        /// <summary>
        /// 处理属性数据变化
        /// </summary>
        public void HandlePropertyChanged(string propertyName, Codec c)
        {
            try
            {
                Property property;
                if (mProperties.TryGetValue(propertyName, out property))
                {
                    property.ReadBufferAndChangeValues(c);
                }
                else
                {
                    Debug.LogErrorFormat("不存在该属性数据：{0}", propertyName);
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Data.HandlePropertyChanged() Error={0},StackTrace={1}!", e.Message,e.StackTrace);
            }
        }

        /// <summary>
        /// 检查数据变化 并序列化变化的属性数据
        /// </summary>
        public bool CheckPropertiesChanged(Codec c)
        {
            int length = 0;
            var e = mProperties.GetEnumerator();
            while (e.MoveNext()){
                var property = e.Current.Value;
                if (property.CheckChangedAndWriteToBuffer(c)) length++;
            }
            if (length > 0){
                c.Insert((uint)length);
                return true;
            }
            return false;
        }
    }
}
