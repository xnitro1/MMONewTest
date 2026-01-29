using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct GetSummonBuffsResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            SummonBuffs = reader.GetList<CharacterBuff>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(SummonBuffs);
        }
    }
}







