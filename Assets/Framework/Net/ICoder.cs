using System.Collections.Generic;

namespace Framework.Network
{
    [XLua.CSharpCallLua]
    public interface ICoder
    {
        object Decode(byte[] data, short protoID);
        byte[] Encode(Protobuf protobuf);
        string PrintContent(Protobuf protobuf);
    }

    [XLua.CSharpCallLua]
    public class Protobuf
    {
        public object Proto;
        public short ProtoID;
    }
}
