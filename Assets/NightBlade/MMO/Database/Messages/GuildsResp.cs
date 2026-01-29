using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GuildsResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            List = reader.GetList<GuildListEntry>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(List);
        }
    }
}







