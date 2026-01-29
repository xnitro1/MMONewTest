using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetGuildRequestsReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetPackedInt();
            Skip = reader.GetPackedInt();
            Limit = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(GuildId);
            writer.PutPackedInt(Skip);
            writer.PutPackedInt(Limit);
        }
    }
}







