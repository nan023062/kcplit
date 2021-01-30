using System;

namespace Nave.Network
{
    /// <summary>
    /// 每个id的字段有且只有一次定义的机会，不可更改（类型/ID/名称）
    /// 1 消息格式化--按照固定格式解析bytes字节流
    /// 2 支持MSG的前后协议兼容
    /// </summary>
    public abstract class AutoMsg : Msg
    {
        public abstract int fieldCount { get; }

        public uint uuid;

        protected override void DecodeMsgBody(Codec codec)
        {
            codec.Out(out uuid);
            int length = 0;
            codec.Out(out length);
            for (int i = 0; i < length; i++)
            {
                try
                {
                    int id = 0;
                    codec.Out(out id);
                    if (id > 0) TryDecodeField(id, codec);
                }
                catch (Exception ex)
                {
                    //OasisLog.I($"{GetType().Name}.Decode() 失败！Msg={ex.Message},StackTrack={ex.StackTrace}!!");
                    return;
                }
            }
        }

        protected override void EncodeMsgBody(Codec codec)
        {
            codec.In(uuid);

            if (fieldCount <= 0) return;
            codec.In(fieldCount);
            try
            {
                int n = EncodeUsedFields(codec);
                if (n != fieldCount)
                {
                    //OasisLog.E($"{GetType().Name}.Decode() 失败！Msg=编码的字段数不对!!");
                    return;
                }
            }
            catch (Exception ex)
            {
                //OasisLog.E($"{GetType().Name}.Decode() 失败！Msg={ex.Message},StackTrack={ex.StackTrace}!!");
                return;
            }
        }

        /// <summary>
        /// 自动解析数据
        /// </summary>
        protected abstract void TryDecodeField(int id, Codec codec);

        /// <summary>
        /// 编码正在使用的数据
        /// </summary>
        protected abstract int EncodeUsedFields(Codec codec);
    }
}
