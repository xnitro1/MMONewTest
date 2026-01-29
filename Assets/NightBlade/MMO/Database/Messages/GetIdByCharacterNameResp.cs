using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetIdByCharacterNameResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
        }
    }
}







