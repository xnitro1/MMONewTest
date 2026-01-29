using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestChangeGuildOptionsMessage : INetSerializable
    {
        public string options;

        public void Deserialize(NetDataReader reader)
        {
            options = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(options);
        }
    }
}







