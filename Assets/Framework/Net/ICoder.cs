using System.Collections.Generic;

namespace Framework.Network
{
    [XLua.CSharpCallLua]
    public interface ICoder
    {
        bool Decode(byte[] data, out List<Protobuf> protobufs);
        byte[] Encode(Protobuf protobuf);
    }

    [XLua.CSharpCallLua]
    public class Protobuf
    {
        public object Proto;
        public int ProtoID;
    }
}
