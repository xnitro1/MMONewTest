using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct UpdateUserCountReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            UserCount = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserCount);
        }
    }
}







