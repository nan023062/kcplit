namespace Nave.Network
{
    public interface IOption
    {
        void Tick();

        void Excute(Codec codec);

        void PackMsg();
    }

    public abstract class Option : IOption
    {
        public readonly int opcode;
        protected Option(int opcode) { this.opcode = opcode; }
        protected abstract bool IsSatisfy();
        public abstract void Tick();
        public abstract void Excute(Codec codec);
        public abstract void PackMsg();
    }

    public abstract class Option<T> : Option where T : AutoMsg, new()
    {
        private static T s_Msg;
        public static T Msg
        {
            get
            {
                if (s_Msg == null) s_Msg = new T();
                return s_Msg;
            }
        }
        protected Option(int opcode) : base(opcode) { }
        protected abstract void PackMsg(ref T msg);
        protected abstract void Excute(T msg);
        public sealed override void Tick()
        {
            if (IsSatisfy())
            {
                PackMsg();
                //Client.Instance?.SendUdpMsg(opcode, Msg);
                //Excute(Msg);
            }
        }
        public sealed override void Excute(Codec codec)
        {
            Msg.Decode(codec);
            Excute(Msg);
        }
        public sealed override void PackMsg()
        {
            var msg = Msg;
            PackMsg(ref msg);
        }
    }

    public class OptionProcessor
    {
        DictionarySafe<int, Option> m_Options;

        public OptionProcessor()
        {
            m_Options = new DictionarySafe<int, Option>();
        }

        public void Tick()
        {
            var e = m_Options.GetEnumerator();
            while (e.MoveNext()) {
                e.Current.Value.Tick();
            }
        }

        public bool Excute(int opcode, Codec codec)
        {
            Option option;
            if(m_Options.TryGetValue(opcode, out option)) {
                option.Excute(codec);
                return true;
            }
            return false;
        }

        public void Regist(Option op)
        {
            m_Options.Add(op.opcode, op);
        }

        public void Unregist(int opcode)
        {
            m_Options.Remove(opcode);
        }
    }
}