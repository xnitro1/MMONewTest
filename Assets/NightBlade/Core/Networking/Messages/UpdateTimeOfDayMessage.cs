using LiteNetLib.Utils;

namespace NightBlade
{
    public struct UpdateTimeOfDayMessage : INetSerializable
    {
        public float timeOfDay;

        public void Deserialize(NetDataReader reader)
        {
            timeOfDay = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(timeOfDay);
        }
    }
}







