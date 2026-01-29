using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Example script demonstrating how to use SmartAssetManager in various scenarios.
    /// This script shows integration patterns for different asset loading systems.
    /// </summary>
    public class SmartAssetExample : MonoBehaviour
    {
        [Header("Example Configuration")]
        [SerializeField] private string addressableAddress = "ExampleAsset";
        [SerializeField] private string assetBundlePath = "examplebundle";
        [SerializeField] private string assetBundleAssetName = "ExampleAsset";
        [SerializeField] private string resourcesPath = "ExampleAsset";

        [Header("Demo Settings")]
        [SerializeField] private bool runAddressablesDemo = true;
        [SerializeField] private bool runAssetBundleDemo = false;
        [SerializeField] private bool runResourcesDemo = false;
        [SerializeField] private bool demonstrateMemoryPressure = false;

        private List<Object> loadedAssets = new List<Object>();

        private async void Start()
        {
            Debug.Log("[SmartAssetExample] Starting SmartAssetManager demonstrations...");

            if (runAddressablesDemo)
            {
                await DemonstrateAddressablesLoading();
            }

            if (runAssetBundleDemo)
            {
                await DemonstrateAssetBundleLoading();
            }

            if (runResourcesDemo)
            {
                DemonstrateResourcesLoading();
            }

            if (demonstrateMemoryPressure)
            {
                StartCoroutine(DemonstrateMemoryPressure());
            }

            // Show initial stats
            Debug.Log(SmartAssetIntegration.SmartAssetMonitor.GetDetailedStats());
        }

        /// <summary>
        /// Demonstrate Addressables integration with automatic tracking.
        /// </summary>
        private async System.Threading.Tasks.Task DemonstrateAddressablesLoading()
        {
            Debug.Log("[SmartAssetExample] Demonstrating Addressables integration...");

            try
            {
                // Method 1: Using SmartAddressables helper (recommended)
                var texture = await SmartAssetIntegration.SmartAddressables.LoadAssetAsync<Texture2D>(
                    addressableAddress,
                    SmartAssetManager.AssetCategory.UI);

                if (texture != null)
                {
                    loadedAssets.Add(texture);
                    Debug.Log($"[SmartAssetExample] Loaded Addressables texture: {texture.name}");

                    // Mark as critical if it's important for the current scene
                    texture.MarkCritical(300f); // Critical for 5 minutes
                }

                // Method 2: Manual tracking after loading
                var audioHandle = Addressables.LoadAssetAsync<AudioClip>("ExampleAudio");
                await audioHandle.Task;

                if (audioHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    // Track manually
                    SmartAssetManager.Instance?.TrackAddressableAsset(
                        audioHandle,
                        SmartAssetManager.AssetCategory.Audio);

                    loadedAssets.Add(audioHandle.Result);
                    Debug.Log($"[SmartAssetExample] Manually tracked Addressables audio: {audioHandle.Result.name}");
                }

                // Method 3: Using extension methods
                var prefabHandle = Addressables.LoadAssetAsync<GameObject>("ExamplePrefab");
                await prefabHandle.Task;

                if (prefabHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    // Use extension method for tracking
                    prefabHandle.TrackWithSmartAssetManager(SmartAssetManager.AssetCategory.Character);
                    loadedAssets.Add(prefabHandle.Result);
                    Debug.Log($"[SmartAssetExample] Extension-tracked prefab: {prefabHandle.Result.name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SmartAssetExample] Addressables demo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Demonstrate AssetBundle integration.
        /// </summary>
        private async System.Threading.Tasks.Task DemonstrateAssetBundleLoading()
        {
            Debug.Log("[SmartAssetExample] Demonstrating AssetBundle integration...");

            try
            {
                // Load asset from bundle with automatic tracking
                var asset = await SmartAssetIntegration.SmartAssetBundles.LoadAssetFromBundleAsync<Object>(
                    assetBundlePath,
                    assetBundleAssetName,
                    SmartAssetManager.AssetCategory.Effect);

                if (asset != null)
                {
                    loadedAssets.Add(asset);
                    Debug.Log($"[SmartAssetExample] Loaded bundle asset: {asset.name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SmartAssetExample] AssetBundle demo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Demonstrate Resources integration.
        /// </summary>
        private void DemonstrateResourcesLoading()
        {
            Debug.Log("[SmartAssetExample] Demonstrating Resources integration...");

            try
            {
                // Load resource with automatic tracking
                var asset = SmartAssetIntegration.SmartResources.Load<Object>(
                    resourcesPath,
                    SmartAssetManager.AssetCategory.General);

                if (asset != null)
                {
                    loadedAssets.Add(asset);
                    Debug.Log($"[SmartAssetExample] Loaded Resources asset: {asset.name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SmartAssetExample] Resources demo failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Demonstrate memory pressure handling.
        /// </summary>
        private IEnumerator DemonstrateMemoryPressure()
        {
            Debug.Log("[SmartAssetExample] Demonstrating memory pressure handling...");

            // Wait a bit for initial loading
            yield return new WaitForSeconds(2f);

            Debug.Log("[SmartAssetExample] Simulating memory pressure by loading many assets...");

            // Load many assets to create memory pressure
            var pressureAssets = new List<Object>();
            for (int i = 0; i < 50; i++)
            {
                // Create dummy assets to simulate memory pressure
                var dummyTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
                pressureAssets.Add(dummyTexture);

                SmartAssetManager.Instance?.TrackResourcesAsset(
                    dummyTexture,
                    SmartAssetManager.AssetCategory.General);

                if (i % 10 == 0)
                {
                    Debug.Log($"[SmartAssetExample] Loaded {i + 1} dummy assets...");
                    yield return null; // Allow frame processing
                }
            }

            Debug.Log("[SmartAssetExample] Memory pressure simulation complete. Monitor SmartAssetManager logs for automatic cleanup.");

            // Wait to see automatic cleanup
            yield return new WaitForSeconds(10f);

            // Manual cleanup if needed
            Debug.Log("[SmartAssetExample] Triggering manual cleanup...");
            var cleanupTask = SmartAssetManager.Instance?.ForceCleanupAsync(128); // Target 128MB
            if (cleanupTask != null)
            {
                yield return new WaitUntil(() => cleanupTask.IsCompleted);
                Debug.Log("[SmartAssetExample] Manual cleanup complete.");
            }

            // Cleanup demo assets
            foreach (var asset in pressureAssets)
            {
                SmartAssetManager.Instance?.UntrackAsset(asset);
                Destroy(asset);
            }

            Debug.Log("[SmartAssetExample] Memory pressure demonstration complete.");
        }

        /// <summary>
        /// Demonstrate scene-based asset management.
        /// </summary>
        public async System.Threading.Tasks.Task DemonstrateSceneManagement()
        {
            Debug.Log("[SmartAssetExample] Demonstrating scene-based asset management...");

            // Mark current assets as critical during scene transition
            SmartAssetManager.Instance?.MarkAssetsCritical(loadedAssets, 60f); // 1 minute

            // Load new scene with automatic cleanup
            await SmartAssetIntegration.SmartSceneManager.LoadSceneAsync("ExampleScene");

            Debug.Log("[SmartAssetExample] Scene transition complete with automatic asset cleanup.");
        }

        /// <summary>
        /// Manual cleanup method for demonstration.
        /// </summary>
        public void CleanupExampleAssets()
        {
            Debug.Log("[SmartAssetExample] Cleaning up example assets...");

            foreach (var asset in loadedAssets)
            {
                SmartAssetManager.Instance?.UntrackAsset(asset);
                // Note: In real usage, you'd handle unloading based on asset type
            }

            loadedAssets.Clear();
            Debug.Log("[SmartAssetExample] Example assets cleaned up.");
        }

        /// <summary>
        /// Get performance statistics.
        /// </summary>
        public void LogPerformanceStats()
        {
            string stats = SmartAssetIntegration.SmartAssetMonitor.GetDetailedStats();
            Debug.Log($"[SmartAssetExample] Performance Stats:\n{stats}");
        }

        private void OnDestroy()
        {
            // Cleanup on destroy
            CleanupExampleAssets();
        }

        #region Public Demo Methods (can be called from UI buttons)

        /// <summary>
        /// Public method to trigger Addressables demo (can be called from UI).
        /// </summary>
        public void TriggerAddressablesDemo()
        {
            StartCoroutine(TriggerAddressablesDemoCoroutine());
        }

        private IEnumerator TriggerAddressablesDemoCoroutine()
        {
            var task = DemonstrateAddressablesLoading();
            yield return new WaitUntil(() => task.IsCompleted);
            LogPerformanceStats();
        }

        /// <summary>
        /// Public method to trigger memory pressure demo.
        /// </summary>
        public void TriggerMemoryPressureDemo()
        {
            StartCoroutine(DemonstrateMemoryPressure());
        }

        /// <summary>
        /// Public method to log current stats.
        /// </summary>
        public void LogStats()
        {
            LogPerformanceStats();
        }

        /// <summary>
        /// Public method to trigger manual cleanup.
        /// </summary>
        public async void TriggerManualCleanup()
        {
            Debug.Log("[SmartAssetExample] Triggering manual memory cleanup...");
            await SmartAssetManager.Instance?.ForceCleanupAsync(256);
            LogPerformanceStats();
        }

        #endregion
    }
}