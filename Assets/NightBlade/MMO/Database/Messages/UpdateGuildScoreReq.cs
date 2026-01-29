using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct UpdateGuildScoreReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            Score = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(Score);
        }
    }
}







