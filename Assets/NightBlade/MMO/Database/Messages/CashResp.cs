using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct CashResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            Cash = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Cash);
        }
    }
}







