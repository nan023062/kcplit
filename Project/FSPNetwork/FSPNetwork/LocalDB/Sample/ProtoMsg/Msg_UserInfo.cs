using System;

namespace pb
{
    public class UserInfoProtoMsg
    {
        public UserBaseInfoProtoMsg baseInfo;
    }

    public class UserBaseInfoProtoMsg
    {
        public int level;
        public ulong exp;
        public int vipLevel;
    }
}
