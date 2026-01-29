using LiteNetLib.Utils;

namespace NightBlade
{
    [System.Serializable]
    public struct RequestAvailableContentsMessage : INetSerializable
    {
        public UnlockableContentType type;

        public void Deserialize(NetDataReader reader)
        {
            type = (UnlockableContentType)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
        }
    }
}







