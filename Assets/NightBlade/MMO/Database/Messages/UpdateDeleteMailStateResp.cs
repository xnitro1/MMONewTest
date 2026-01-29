using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct UpdateDeleteMailStateResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Error = (UITextKeys)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Error);
        }
    }
}







