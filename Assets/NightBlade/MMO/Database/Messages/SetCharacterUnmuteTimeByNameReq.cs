using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct SetCharacterUnmuteTimeByNameReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            CharacterName = reader.GetString();
            UnmuteTime = reader.GetPackedLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterName);
            writer.PutPackedLong(UnmuteTime);
        }
    }
}







