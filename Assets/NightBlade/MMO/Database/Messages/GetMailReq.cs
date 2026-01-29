using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetMailReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            MailId = reader.GetString();
            UserId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MailId);
            writer.Put(UserId);
        }
    }
}







