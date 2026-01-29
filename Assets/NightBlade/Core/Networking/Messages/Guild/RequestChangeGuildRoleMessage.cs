using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestChangeGuildRoleMessage : INetSerializable
    {
        public byte guildRole;
        public GuildRoleData guildRoleData;

        public void Deserialize(NetDataReader reader)
        {
            guildRole = reader.GetByte();
            guildRoleData = reader.Get<GuildRoleData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(guildRole);
            writer.Put(guildRoleData);
        }
    }
}







