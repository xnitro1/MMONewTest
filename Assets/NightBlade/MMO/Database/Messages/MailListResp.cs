using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct MailListResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            List = reader.GetList<MailListEntry>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(List);
        }
    }
}







