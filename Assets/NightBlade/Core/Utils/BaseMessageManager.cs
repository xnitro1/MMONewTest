using UnityEngine;

namespace NightBlade
{
    public abstract class BaseMessageManager : ScriptableObject
    {
        public abstract string ReplaceKeysToMessages(string format);
    }
}







