//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: fsp_debuger.proto
namespace Nave.Network
{
    [global::System.Serializable, global::ProtoBuf.ProtoContract(Name = @"NetSampleItem")]
    public partial class NetSampleItem : global::ProtoBuf.IExtensible
    {
        public NetSampleItem() { }

        private string _name = "";
        [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name = @"name", DataFormat = global::ProtoBuf.DataFormat.Default)]
        [global::System.ComponentModel.DefaultValue("")]
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }
        private long _time = default(long);
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"time", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(long))]
        public long time
        {
            get { return _time; }
            set { _time = value; }
        }
        private int _data1 = default(int);
        [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name = @"data1", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(int))]
        public int data1
        {
            get { return _data1; }
            set { _data1 = value; }
        }
        private int _data2 = default(int);
        [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name = @"data2", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(int))]
        public int data2
        {
            get { return _data2; }
            set { _data2 = value; }
        }
        private int _data3 = default(int);
        [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name = @"data3", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(int))]
        public int data3
        {
            get { return _data3; }
            set { _data3 = value; }
        }
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }

    [global::System.Serializable, global::ProtoBuf.ProtoContract(Name = @"NetDebugFileData")]
    public partial class NetDebugFileData : global::ProtoBuf.IExtensible
    {
        public NetDebugFileData() { }

        private readonly global::System.Collections.Generic.List<Nave.Network.NetSampleItem> _profiler_samples = new global::System.Collections.Generic.List<Nave.Network.NetSampleItem>();
        [global::ProtoBuf.ProtoMember(1, Name = @"profiler_samples", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public global::System.Collections.Generic.List<Nave.Network.NetSampleItem> profiler_samples
        {
            get { return _profiler_samples; }
        }

        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }

}