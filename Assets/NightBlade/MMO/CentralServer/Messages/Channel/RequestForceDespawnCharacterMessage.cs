using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public struct RequestForceDespawnCharacterMessage : INetSerializable
    {
        public string userId;

        public void Deserialize(NetDataReader reader)
        {
            userId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(userId);
        }
    }
}







