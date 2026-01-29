using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestDeclineGuildInvitationMessage : INetSerializable
    {
        public int guildId;
        public string inviterId;

        public void Deserialize(NetDataReader reader)
        {
            guildId = reader.GetPackedInt();
            inviterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(guildId);
            writer.Put(inviterId);
        }
    }
}







