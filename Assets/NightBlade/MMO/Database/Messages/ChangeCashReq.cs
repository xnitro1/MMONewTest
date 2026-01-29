using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct ChangeCashReq : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            UserId = reader.GetString();
            ChangeAmount = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(UserId);
            writer.Put(ChangeAmount);
        }
    }
}







