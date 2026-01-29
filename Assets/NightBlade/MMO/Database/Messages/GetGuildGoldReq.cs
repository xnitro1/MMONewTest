using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetGuildGoldReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
        }
    }
}







