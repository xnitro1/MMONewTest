using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct ValidateUserLoginResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
        }
    }
}







