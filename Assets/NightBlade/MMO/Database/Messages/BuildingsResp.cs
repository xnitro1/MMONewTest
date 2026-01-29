using LiteNetLib.Utils;
using System.Collections.Generic;

namespace NightBlade.MMO
{
    public partial struct BuildingsResp : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            List = reader.GetList<BuildingSaveData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(List);
        }
    }
}







