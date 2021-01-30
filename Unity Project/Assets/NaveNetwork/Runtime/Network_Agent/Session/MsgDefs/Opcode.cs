using System.Collections.Generic;

namespace Nave.Network
{
    public class Op_room
    {
        public static readonly string opcode = "room";
    }

    public class Op_action
    {
        public static readonly string opcode = "action";
        public static readonly int testAction = 1000;
    }

    public class Op_roomdata
    {
        public static readonly string opcode = "roomdata";

        public static readonly string room_getgdata = "qdata";
        public static readonly string room_setgdata = "setdata";

        public static readonly string room_getodata = "qshareddata";
        public static readonly string room_setodata = "setshareddata";
    }

    public class Opcode
    {
        //TCP
        public static readonly int TCP_Default = 0;             //原始默认
        public static readonly int TCP_GlobalData = 1;          //全局数据同步消息
        public static readonly int TCP_PhysiclerData = 2;       //物理单位数据同步消息
        public static readonly int TCP_OptionMsg = 3;           //行为操作同步消息
        public static readonly int TCP_Transforms = 4;          //TCP位置同步消息

        //UDP
        public static readonly int UDP_Transforms = 101;        //UDP的Transform数据消息
        public static readonly int UDP_HandCurls = 102;         //UDP手势操作同步消息
    }
}
