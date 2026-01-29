using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct FindUsernameReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Username = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Username);
        }
    }
}







