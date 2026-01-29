using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestSortItemsMessage : INetSerializable
    {
        public bool asc;

        public void Deserialize(NetDataReader reader)
        {
            asc = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(asc);
        }
    }
}







