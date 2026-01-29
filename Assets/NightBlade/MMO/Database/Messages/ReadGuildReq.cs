using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetGuildReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetPackedInt();
            ForceClearCache = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(GuildId);
            writer.Put(ForceClearCache);
        }
    }
}







