using UnityEngine;

namespace Nave.Network
{

    /// <summary>
    /// UDP--角色位置数据
    /// </summary>
    public class UdpMsg_Transform : AutoMsg
    {
        public override int fieldCount => 2;

        public Pose root = new Pose();                      // 1 Root      模型的根节点参数
        public IKGoals goals = new IKGoals();               // 2 IKGoals   7个IK目标点数据

        protected override int EncodeUsedFields(Codec codec)
        {
            codec.In(1); root.Encode(codec);
            codec.In(2); goals.Encode(codec);
            return 2;
        }

        protected override void TryDecodeField(int id, Codec codec)
        {
            if (id == 1) root.Decode(codec);
            else if (id == 2) goals.Decode(codec);
        }

        public class Pose
        {
            public float speed;
            public float angularSpeed;
            public Vector3 ppsition;
            public Quaternion rotation;
            public virtual void Encode(Codec codec)
            {
                codec.In(speed);
                codec.In(angularSpeed);
                codec.In(ppsition);
                codec.In(rotation);
            }
            public virtual void Decode(Codec codec)
            {
                codec.Out(out speed);
                codec.Out(out angularSpeed);
                codec.Out(out ppsition);
                codec.Out(out rotation);
            }
        }

        public class IKGoals
        {
            public const int count = 7;
            public IKGoal[] goals = new IKGoal[7];

            public void Encode(Codec codec)
            {
                for (int i = 0; i < count; i++)
                {
                    goals[i].Encode(codec);
                }
            }

            public void Decode(Codec codec)
            {
                for (int i = 0; i < count; i++)
                {
                    goals[i].Decode(codec);
                }
            }

            public class IKGoal : Pose
            {
                public float weight;
                public float retarget;
                public override void Encode(Codec codec)
                {
                    codec.In(weight);
                    codec.In(retarget);
                    if (weight > 0) base.Encode(codec);
                }
                public override void Decode(Codec codec)
                {
                    codec.Out(out weight);
                    codec.Out(out retarget);
                    if (weight > 0) base.Decode(codec);
                }
            }

        }
    }

}
