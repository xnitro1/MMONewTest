using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestOpenStorageMessage : INetSerializable
    {
        public StorageType storageType;

        public void Deserialize(NetDataReader reader)
        {
            storageType = (StorageType)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)storageType);
        }
    }
}







