using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetUserLevelResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            UserLevel = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserLevel);
        }
    }
}







