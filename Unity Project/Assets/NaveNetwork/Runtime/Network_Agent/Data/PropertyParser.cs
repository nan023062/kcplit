using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Nave.Network
{
    public static class PropertyParser
    {
        private static Dictionary<string, FieldParser[]> m_PaserConfigs;

        private static FieldParser[] GetParserConfig(Property property)
        {
            if (m_PaserConfigs == null)
            {
                m_PaserConfigs = new Dictionary<string, FieldParser[]>();
                RegistVarPropertyConfigs();
            }
            return GetCharpPropertyConfig(property.GetType());
        }

        public static void DecodeParser(Property property, Codec codec)
        {
            var parserConfig = GetParserConfig(property);
            if (parserConfig == null)
            {
                UnityEngine.Debug.LogErrorFormat("PropertyParser.DecodeParser 失败！ 没有找到解析配置 {0}！", property.name);
                return;
            }
        }

        public static void EncodeParser(Property property, Codec codec)
        {
            var parserConfig = GetParserConfig(property);
            if (parserConfig == null)
            {
                UnityEngine.Debug.LogErrorFormat("PropertyParser.DecodeParser 失败！ 没有找到解析配置 {0}！", property.name);
                return;
            }


        }

        #region Var Property Configs

        private static FieldParser[] GetVarPropertyConfig(string propertyName)
        {
            FieldParser[] result;
            m_PaserConfigs.TryGetValue(propertyName, out result);
            return result;
        }

        /// <summary>
        /// 加载Lua的动态属性解析格式表
        /// </summary>
        private static void RegistVarPropertyConfigs()
        {
            //TODO:...
            m_PaserConfigs.Add("luaTest1", new FieldParser[] { });
            m_PaserConfigs.Add("luaTest2", new FieldParser[] { });
            m_PaserConfigs.Add("luaTest3", new FieldParser[] { });
            m_PaserConfigs.Add("luaTest4", new FieldParser[] { });
            m_PaserConfigs.Add("luaTest5", new FieldParser[] { });
            m_PaserConfigs.Add("luaTest6", new FieldParser[] { });
        }

        #endregion

        #region C# Property

        private static FieldParser[] GetCharpPropertyConfig(System.Type type)
        {
            var result = type.InvokeMember("GetParserConfig", 
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, 
                        null, null,new object[] { 0});
            if (result == null)
            {
                UnityEngine.Debug.LogErrorFormat("{0}没有定义解析配置！", type.FullName);
                return null;
            }
            return result as FieldParser[];
        }

        #endregion

        public sealed class FieldParser
        {
            public int id;
            public string name;
            public ValueType valueType;
            public bool used;
            public FieldParser(int id, string name, ValueType valueType, bool used)
            {
                this.id = id;
                this.name = name;
                this.valueType = valueType;
                this.used = used;
            }
        }
    }
}
