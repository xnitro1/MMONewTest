using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct CharactersResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            List = reader.GetList<PlayerCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(List);
        }
    }
}







