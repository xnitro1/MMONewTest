using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GuildGoldResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildGold = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildGold);
        }
    }
}







