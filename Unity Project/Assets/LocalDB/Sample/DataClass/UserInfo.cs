using System;
using Nave.DB;
using pb;

namespace db
{
    public class UserInfo : DBGroup
    {
        public UInt64 uuid { private set; get; }

        public UserInfo_Base baseInfo;

        public UserInfo()
        {
            baseInfo = new UserInfo_Base(this);
        }


        public override object PackMsg()
        {
            throw new NotImplementedException();
        }

        protected override IDBElement UnpackItemMsg(object msg)
        {
            if(msg is UserBaseInfoProtoMsg)
            {
                baseInfo.UnpackMsg(msg as UserBaseInfoProtoMsg);
                return baseInfo;
            }
            return null;
        }
    }

    #region 玩家基础信息


    public class UserInfo_Base : DBElement
    {
        public int level { private set; get; }
        public ulong exp { private set; get; }
        public int vipLevel { private set; get; }

        public UserInfo_Base(IDBGroup group) : base(group) { }

        public override object PackMsg()
        {
            var info = new UserBaseInfoProtoMsg();
            info.level = level;
            info.exp = exp;
            info.vipLevel = vipLevel;
            return info;
        }

        public override void UnpackMsg(object msg)
        {
            var info = msg as UserBaseInfoProtoMsg;
            level = info.level;
            exp = info.exp;
            vipLevel = info.vipLevel;
        }
    }

    #endregion

}

