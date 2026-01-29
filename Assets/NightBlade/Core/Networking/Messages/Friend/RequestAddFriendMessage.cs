using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestAddFriendMessage : INetSerializable
    {
        public string friendId;

        public void Deserialize(NetDataReader reader)
        {
            friendId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(friendId);
        }
    }
}







