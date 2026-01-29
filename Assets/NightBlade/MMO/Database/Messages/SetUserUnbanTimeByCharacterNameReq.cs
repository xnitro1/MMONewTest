using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct SetUserUnbanTimeByCharacterNameReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            CharacterName = reader.GetString();
            UnbanTime = reader.GetPackedLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterName);
            writer.PutPackedLong(UnbanTime);
        }
    }
}







