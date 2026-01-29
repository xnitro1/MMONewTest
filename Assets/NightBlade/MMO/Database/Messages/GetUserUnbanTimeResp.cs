using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetUserUnbanTimeResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            UnbanTime = reader.GetPackedLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedLong(UnbanTime);
        }
    }
}







