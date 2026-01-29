using LiteNetLibManager;

namespace NightBlade
{
    [System.Serializable]
    public class AssetReferenceBaseGameEntity : AssetReferenceLiteNetLibBehaviour<BaseGameEntity>
    {
        public AssetReferenceBaseGameEntity(string guid) : base(guid)
        {
        }
    }
}







