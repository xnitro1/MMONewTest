using LiteNetLibManager;

namespace NightBlade
{
    [System.Serializable]
    public class AssetReferenceNpcEntity : AssetReferenceLiteNetLibBehaviour<NpcEntity>
    {
        public AssetReferenceNpcEntity(string guid) : base(guid)
        {
        }
    }
}







