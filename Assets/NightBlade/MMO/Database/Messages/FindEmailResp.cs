using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct FindEmailResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            FoundAmount = reader.GetLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(FoundAmount);
        }
    }
}







