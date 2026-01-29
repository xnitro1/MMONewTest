using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct UpdateGuildMemberRoleReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            GuildId = reader.GetInt();
            GuildRole = reader.GetByte();
            MemberCharacterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(GuildId);
            writer.Put(GuildRole);
            writer.Put(MemberCharacterId);
        }
    }
}







