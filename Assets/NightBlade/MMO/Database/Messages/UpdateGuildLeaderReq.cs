using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct UpdateGuildLeaderReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            LeaderCharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(LeaderCharacterId);
        }
    }
}







