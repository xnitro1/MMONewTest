using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestChangeGuildMessageMessage : INetSerializable
    {
        public string message;

        public void Deserialize(NetDataReader reader)
        {
            message = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(message);
        }
    }
}







