using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetIdByCharacterNameReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            CharacterName = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterName);
        }
    }
}







