using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct DeleteBuildingReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            ChannelId = reader.GetString();
            MapName = reader.GetString();
            BuildingId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ChannelId);
            writer.Put(MapName);
            writer.Put(BuildingId);
        }
    }
}







