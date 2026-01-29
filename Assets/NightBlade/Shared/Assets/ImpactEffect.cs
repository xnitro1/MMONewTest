using NightBlade.AddressableAssetTools;
using NightBlade.UnityEditorUtils;
using UnityEngine;

namespace NightBlade
{
    [System.Serializable]
    public struct ImpactEffect
    {
        public UnityTag tag;
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [AddressableAssetConversion(nameof(addressableEffect))]
        public GameEffect effect;
#endif
        public AssetReferenceGameEffect addressableEffect;
        
        public async void Play(Vector3 position, Quaternion rotation)
        {
#if !UNITY_SERVER
            GameEffect tempPrefab = null;
#if !EXCLUDE_PREFAB_REFS
            tempPrefab = effect;
#endif
            AssetReferenceGameEffect tempAddressablePrefab = addressableEffect;
            GameEffect loadedPrefab = await tempAddressablePrefab.GetOrLoadAssetAsyncOrUsePrefab(tempPrefab);
            if (loadedPrefab != null)
            {
                // SmartAssetManager: Track effect assets for automatic memory management
                NightBlade.Core.Utils.SmartAssetManager.Instance?.TrackResourcesAsset(
                    loadedPrefab,
                    NightBlade.Core.Utils.SmartAssetManager.AssetCategory.Effect);

                PoolSystem.GetInstance(loadedPrefab, position, rotation);
            }
#endif
        }
    }
}







