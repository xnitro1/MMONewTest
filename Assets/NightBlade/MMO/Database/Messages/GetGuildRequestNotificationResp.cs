using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetGuildRequestNotificationResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            NotificationCount = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(NotificationCount);
        }
    }
}







