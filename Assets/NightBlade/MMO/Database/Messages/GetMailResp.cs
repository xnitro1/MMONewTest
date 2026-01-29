using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetMailResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Mail = reader.Get(() => new Mail());
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Mail);
        }
    }
}







