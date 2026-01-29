using LiteNetLibManager;

namespace NightBlade
{
    [System.Serializable]
    public class AssetReferenceVehicleEntity : AssetReferenceLiteNetLibBehaviour<VehicleEntity>
    {
        public AssetReferenceVehicleEntity(string guid) : base(guid)
        {
        }
    }
}







