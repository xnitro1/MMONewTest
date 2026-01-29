using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestGetGuildInfoMessage : INetSerializable
    {
        public int guildId;

        public void Deserialize(NetDataReader reader)
        {
            guildId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(guildId);
        }
    }
}







