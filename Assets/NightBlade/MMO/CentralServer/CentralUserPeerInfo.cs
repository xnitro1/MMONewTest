using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public struct CentralUserPeerInfo : INetSerializable
    {
        public long connectionId;
        public string userId;
        public string accessToken;

        public void Deserialize(NetDataReader reader)
        {
            userId = reader.GetString();
            accessToken = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(userId);
            writer.Put(accessToken);
        }
    }
}







