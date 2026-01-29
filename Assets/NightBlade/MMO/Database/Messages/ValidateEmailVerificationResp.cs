using LiteNetLib.Utils;

namespace NightBlade.MMO
{
    public partial struct ValidateEmailVerificationResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            IsPass = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsPass);
        }
    }
}







