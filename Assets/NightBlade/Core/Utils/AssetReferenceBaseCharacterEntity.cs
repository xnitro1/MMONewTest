using LiteNetLibManager;

namespace NightBlade
{
    [System.Serializable]
    public class AssetReferenceBaseCharacterEntity : AssetReferenceLiteNetLibBehaviour<BaseCharacterEntity>
    {
        public AssetReferenceBaseCharacterEntity(string guid) : base(guid)
        {
        }
    }
}







