using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetGuildRequestNotificationReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(GuildId);
        }
    }
}







