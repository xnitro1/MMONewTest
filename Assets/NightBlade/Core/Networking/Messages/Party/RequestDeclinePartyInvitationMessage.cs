using LiteNetLib.Utils;

namespace NightBlade
{
    public struct RequestDeclinePartyInvitationMessage : INetSerializable
    {
        public int partyId;
        public string inviterId;

        public void Deserialize(NetDataReader reader)
        {
            partyId = reader.GetPackedInt();
            inviterId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(partyId);
            writer.Put(inviterId);
        }
    }
}







