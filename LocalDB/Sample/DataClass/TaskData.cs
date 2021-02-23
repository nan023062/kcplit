using System;
using Nave.DB;
using pb;

namespace db
{

    public class TaskElementInfo : DBElement
    {
        public int id;
        public int prog;
        public int max;
        public int min;
        public int ok;

        public TaskElementInfo(IDBGroup group) : base(group) { 
            
        
        }

        public override object PackMsg()
        {
            Msg_TaskInfo msg = new Msg_TaskInfo();


            throw new NotImplementedException();
        }

        public override void UnpackMsg(object msg)
        {
            throw new NotImplementedException();
        }
    }

    public class TaskData : DBList<TaskElementInfo>
    {
        protected override TaskElementInfo UnpackItemMsg(object msg)
        {
            throw new NotImplementedException();
        }
    }
}
