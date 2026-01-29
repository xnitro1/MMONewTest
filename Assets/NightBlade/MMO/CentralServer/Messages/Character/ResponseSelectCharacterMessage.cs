using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public struct ResponseSelectCharacterMessage : INetSerializable
    {
        public UITextKeys message;
        public string mapName;
        public string networkAddress;
        public int networkPort;
        public string instanceId; // NEW: For multiple map instances

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            mapName = reader.GetString();
            networkAddress = reader.GetString();
            networkPort = reader.GetInt();
            instanceId = reader.GetString(); // NEW: Deserialize instance ID
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.Put(mapName);
            writer.Put(networkAddress);
            writer.Put(networkPort);
            writer.Put(instanceId ?? ""); // NEW: Serialize instance ID (empty string if null)
        }
    }
}







