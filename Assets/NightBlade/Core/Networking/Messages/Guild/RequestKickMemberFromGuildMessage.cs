using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestKickMemberFromGuildMessage : INetSerializable
    {
        public string memberId;

        public void Deserialize(NetDataReader reader)
        {
            memberId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(memberId);
        }
    }
}







