using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GoldResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Gold = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Gold);
        }
    }
}







