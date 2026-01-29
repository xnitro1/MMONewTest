using NightBlade.SerializationSurrogates;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NightBlade
{
    public class SummonBuffsSaveDataSerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(
            object obj,
            SerializationInfo info,
            StreamingContext context)
        {
            SummonBuffsSaveData data = (SummonBuffsSaveData)obj;
            info.AddListValue("summonBuffs", data.summonBuffs);
        }

        public object SetObjectData(
            object obj,
            SerializationInfo info,
            StreamingContext context,
            ISurrogateSelector selector)
        {
            SummonBuffsSaveData data = (SummonBuffsSaveData)obj;
            data.summonBuffs = new List<CharacterBuff>(info.GetListValue<CharacterBuff>("summonBuffs"));
            obj = data;
            return obj;
        }
    }
}







