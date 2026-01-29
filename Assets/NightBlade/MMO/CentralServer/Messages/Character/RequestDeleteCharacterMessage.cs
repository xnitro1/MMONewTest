using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public struct RequestDeleteCharacterMessage : INetSerializable
    {
        public string characterId;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
        }
    }
}







