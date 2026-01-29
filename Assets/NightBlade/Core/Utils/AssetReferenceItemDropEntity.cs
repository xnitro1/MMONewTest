using LiteNetLibManager;

namespace NightBlade
{
    [System.Serializable]
    public class AssetReferenceItemDropEntity : AssetReferenceLiteNetLibBehaviour<ItemDropEntity>
    {
        public AssetReferenceItemDropEntity(string guid) : base(guid)
        {
        }
    }
}







