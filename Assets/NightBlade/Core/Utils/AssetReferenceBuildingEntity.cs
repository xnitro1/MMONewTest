using LiteNetLibManager;

namespace NightBlade
{
    [System.Serializable]
    public class AssetReferenceBuildingEntity : AssetReferenceLiteNetLibBehaviour<BuildingEntity>
    {
        public AssetReferenceBuildingEntity(string guid) : base(guid)
        {
        }
    }
}







