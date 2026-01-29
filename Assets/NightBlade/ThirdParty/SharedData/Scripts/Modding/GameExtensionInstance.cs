using NightBlade.DevExtension;

namespace NightBlade
{
    public static partial class GameExtensionInstance
    {
        public static CharacterItemCloneDelegate onCharacterItemClone;
        public static CharacterItemSerializeDelegate onCharacterItemSerialize;
        public static CharacterItemDeserializeDelegate onCharacterItemDeserialize;

        static GameExtensionInstance()
        {
            DevExtUtils.InvokeStaticDevExtMethods(typeof(GameExtensionInstance), "Init");
        }
    }
}







