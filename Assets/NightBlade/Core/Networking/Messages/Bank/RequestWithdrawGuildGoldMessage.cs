using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestWithdrawGuildGoldMessage : INetSerializable
    {
        public int gold;

        public void Deserialize(NetDataReader reader)
        {
            gold = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(gold);
        }
    }
}







