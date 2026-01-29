using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestSetIconMessage : INetSerializable
    {
        public int dataId;

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(dataId);
        }
    }
}







