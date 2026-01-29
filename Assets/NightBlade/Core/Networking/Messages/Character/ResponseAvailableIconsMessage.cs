using LiteNetLib.Utils;

namespace NightBlade
{
    public struct ResponseAvailableIconsMessage : INetSerializable
    {
        public UITextKeys message;
        public int[] iconIds;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            iconIds = reader.GetIntArray();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.PutArray(iconIds);
        }
    }
}







