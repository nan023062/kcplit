using System;

namespace Nave.Network
{
    public static class ProtoDefs
    {
        static DictionarySafe<int, Type> s_Op2MsgTypes = new DictionarySafe<int, Type>();


        static DictionarySafe<Type, int> s_MsgType2Ops = new DictionarySafe<Type, int>();

        public static void InitMsgDefs(string assemblyString)
        {
            s_Op2MsgTypes.Clear();
            s_MsgType2Ops.Clear();


        }

        public static Type GetType(int opcode)
        {
            Type result = null;
            if (!s_Op2MsgTypes.TryGetValue(opcode, out result))
                throw new Exception("ProtoDefs.GetType : 请先初始化消息定义文件表！！ opcode = " + opcode);
            return result;
        }

        public static int GetOp(Type type)
        {
            int result = 0;
            if (!s_MsgType2Ops.TryGetValue(type, out result))
                throw new Exception("ProtoDefs.GetType : 请先初始化消息定义文件表！！type = " + type.FullName);
            return result;
        }
    }
}
