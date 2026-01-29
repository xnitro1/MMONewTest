using LiteNetLibManager;

namespace NightBlade
{
    [System.Serializable]
    public class AssetReferenceBasePlayerCharacterEntity : AssetReferenceLiteNetLibBehaviour<BasePlayerCharacterEntity>
    {
        public AssetReferenceBasePlayerCharacterEntity(string guid) : base(guid)
        {
        }
    }
}







