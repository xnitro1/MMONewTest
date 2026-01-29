using LiteNetLib.Utils;
using System.Collections.Generic;

namespace NightBlade.MMO
{
    public struct RequestCreateCharacterMessage : INetSerializable
    {
        public string characterName;
        public int entityId;
        public int dataId;
        public int factionId;
        public IList<CharacterDataBoolean> publicBools;
        public IList<CharacterDataInt32> publicInts;
        public IList<CharacterDataFloat32> publicFloats;

        public void Deserialize(NetDataReader reader)
        {
            characterName = reader.GetInternedString();
            entityId = reader.GetInt();
            dataId = reader.GetInt();
            factionId = reader.GetInt();
            publicBools = reader.GetList<CharacterDataBoolean>();
            publicInts = reader.GetList<CharacterDataInt32>();
            publicFloats = reader.GetList<CharacterDataFloat32>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutInternedString(characterName);
            writer.Put(entityId);
            writer.Put(dataId);
            writer.Put(factionId);
            writer.PutList(publicBools);
            writer.PutList(publicInts);
            writer.PutList(publicFloats);
        }
    }
}







