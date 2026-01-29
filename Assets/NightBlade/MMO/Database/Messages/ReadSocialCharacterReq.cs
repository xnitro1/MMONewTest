using LiteNetLib.Utils;

namespace NightBlade.MMO
{
#nullable enable
    public partial struct GetSocialCharacterReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);
        }
    }
}







