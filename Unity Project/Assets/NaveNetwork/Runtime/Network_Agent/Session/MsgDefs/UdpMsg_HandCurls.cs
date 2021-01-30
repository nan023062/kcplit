using System;

namespace Nave.Network
{
    /// <summary>
    /// UDP--角色手势数据
    /// </summary>
    public class UdpMsg_HandCurls : AutoMsg
    {
        public override int fieldCount => 3;

        //测试字段前后向兼容--删除6个字段
        //public float LThumb = 0f;                 //1 左手拇指    NOT
        //public float LIndex = 0f;                 //2 左手食指    NOT
        //public float LMiddle = 0f;                //3 左手中指    NOT
        //public float RThumb = 0f;                 //4 右手拇指    NOT
        //public float RIndex = 0f;                 //5 右手食指    NOT
        //public float RMiddle = 0f;                //6 右手中指    NOT
        public Fingers Lefts = new Fingers();       //7 左手 
        public Fingers Rights = new Fingers();      //8 右手
        public bool fixedGesture = false;           //9 固定手势

        protected override int EncodeUsedFields(Codec codec)
        {
            //codec.In(1); codec.In(LThumb);
            //codec.In(2); codec.In(LIndex);
            //codec.In(3); codec.In(LMiddle);
            //codec.In(4); codec.In(RThumb);
            //codec.In(5); codec.In(RIndex);
            //codec.In(6); codec.In(RMiddle);
            codec.In(7); Lefts.Encode(codec);
            codec.In(8); Rights.Encode(codec);
            codec.In(9); codec.In(fixedGesture);
            return 3;
        }

        protected override void TryDecodeField(int id, Codec codec)
        {
            //if (id == 1) codec.Out(out LThumb);
            //else if (id == 2) codec.Out(out LIndex);
            //else if (id == 3) codec.Out(out LMiddle);
            //else if (id == 4) codec.Out(out RThumb);
            //else if (id == 5) codec.Out(out RIndex);
            //else if (id == 6) codec.Out(out RMiddle);
            if (id == 7) Lefts.Decode(codec);
            else if (id == 8) Rights.Decode(codec);
            else if (id == 9) codec.Out(out fixedGesture);
        }

        public class Fingers : ICodecable
        {
            public float thumb = 0f;
            public float index = 0f;
            public float middle = 0f;
            public float ring = 0f;
            public float pinky = 0f;

            public void Decode(Codec codec)
            {
                codec.Out(out thumb);
                codec.Out(out index);
                codec.Out(out middle);
                codec.Out(out ring);
                codec.Out(out pinky);
            }

            public void Encode(Codec codec)
            {
                codec.In(thumb);
                codec.In(index);
                codec.In(middle);
                codec.In(ring);
                codec.In(pinky);
            }
        }
    }
}
