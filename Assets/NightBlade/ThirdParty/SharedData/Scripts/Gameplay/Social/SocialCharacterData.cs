using LiteNetLib.Utils;

namespace NightBlade
{
    [System.Serializable]
    public partial struct SocialCharacterData : INetSerializable
    {
        public string id;
        public string userId;
        public string characterName;
        public int dataId;
        public int level;
        public int factionId;
        public int partyId;
        public int guildId;
        public byte guildRole;
        public int currentHp;
        public int maxHp;
        public int currentMp;
        public int maxMp;

        public static SocialCharacterData Create(IPlayerCharacterData character)
        {
            return new SocialCharacterData()
            {
                id = character.Id,
                characterName = character.CharacterName,
                dataId = character.DataId,
                level = character.Level,
                factionId = character.FactionId,
                partyId = character.PartyId,
                guildId = character.GuildId,
                guildRole = character.GuildRole,
                currentHp = character.CurrentHp,
                currentMp = character.CurrentMp,
            };
        }

        public void Deserialize(NetDataReader reader)
        {
            DeserializeWithoutHpMp(reader);
            currentHp = reader.GetPackedInt();
            maxHp = reader.GetPackedInt();
            currentMp = reader.GetPackedInt();
            maxMp = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            SerializeWithoutHpMp(writer);
            writer.PutPackedInt(currentHp);
            writer.PutPackedInt(maxHp);
            writer.PutPackedInt(currentMp);
            writer.PutPackedInt(maxMp);
        }

        public void DeserializeWithoutHpMp(NetDataReader reader)
        {
            id = reader.GetString();
            userId = reader.GetString();
            characterName = reader.GetString();
            dataId = reader.GetPackedInt();
            level = reader.GetPackedInt();
            factionId = reader.GetPackedInt();
            partyId = reader.GetPackedInt();
            guildId = reader.GetPackedInt();
            guildRole = reader.GetByte();
        }

        public void SerializeWithoutHpMp(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(userId);
            writer.Put(characterName);
            writer.PutPackedInt(dataId);
            writer.PutPackedInt(level);
            writer.PutPackedInt(factionId);
            writer.PutPackedInt(partyId);
            writer.PutPackedInt(guildId);
            writer.Put(guildRole);
        }
    }
}







