using LiteNetLibManager;

namespace NightBlade
{
    [System.Serializable]
    public class AssetReferenceHarvestableEntity : AssetReferenceLiteNetLibBehaviour<HarvestableEntity>
    {
        public AssetReferenceHarvestableEntity(string guid) : base(guid)
        {
        }
    }
}







