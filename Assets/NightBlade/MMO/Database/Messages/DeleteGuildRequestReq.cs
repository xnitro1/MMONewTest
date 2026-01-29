using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct DeleteGuildRequestReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetPackedInt();
            RequesterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(GuildId);
            writer.Put(RequesterId);
        }
    }
}







