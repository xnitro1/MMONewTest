using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct UpdateGuildMessageReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            GuildMessage = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(GuildMessage);
        }
    }
}







