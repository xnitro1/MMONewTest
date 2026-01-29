using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NightBlade.AddressableAssetTools
{
    public class AssetReferenceReleaser : MonoBehaviour
    {
        private void OnDestroy()
        {
            Addressables.ReleaseInstance(gameObject);
        }
    }
}







