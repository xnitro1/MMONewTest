using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Integration utilities for SmartAssetManager with various Unity asset loading systems.
    /// Provides extension methods and helper classes for seamless integration.
    /// </summary>
    public static class SmartAssetIntegration
    {
        #region Addressables Integration

        /// <summary>
        /// Enhanced Addressables loading with automatic SmartAssetManager tracking.
        /// </summary>
        public static class SmartAddressables
        {
            /// <summary>
            /// Load an asset with automatic tracking for smart unloading.
            /// </summary>
            public static async Task<T> LoadAssetAsync<T>(string address, SmartAssetManager.AssetCategory category = SmartAssetManager.AssetCategory.General)
            {
                var handle = Addressables.LoadAssetAsync<T>(address);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    SmartAssetManager.Instance?.TrackAddressableAsset(handle, category);
                }

                return handle.Result;
            }

            /// <summary>
            /// Load multiple assets with automatic tracking.
            /// </summary>
            public static async Task<IList<T>> LoadAssetsAsync<T>(IEnumerable<string> addresses, SmartAssetManager.AssetCategory category = SmartAssetManager.AssetCategory.General) where T : UnityEngine.Object
            {
                var handle = Addressables.LoadAssetsAsync<T>(addresses, null);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    foreach (var asset in handle.Result)
                    {
                        SmartAssetManager.Instance?.TrackAddressableAsset(handle, category);
                    }
                }

                return handle.Result;
            }

            /// <summary>
            /// Load a scene with scene asset tracking.
            /// </summary>
            public static async Task<Scene> LoadSceneAsync(string address, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true)
            {
                var handle = Addressables.LoadSceneAsync(address, loadMode, activateOnLoad);
                await handle.Task;

                // Track scene-related assets with high priority
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    // Mark scene assets as critical for a period after loading
                    var sceneAssets = new List<UnityEngine.Object>();
                    var scene = handle.Result.Scene;

                    // Get root objects from the scene (this is a simplified approach)
                    // In practice, you might want to track specific assets loaded for the scene
                    SmartAssetManager.Instance?.MarkAssetsCritical(sceneAssets, 300f); // 5 minutes
                }

                return handle.Result.Scene;
            }
        }

        #endregion

        #region AssetBundle Integration

        /// <summary>
        /// Enhanced AssetBundle loading with automatic SmartAssetManager tracking.
        /// </summary>
        public static class SmartAssetBundles
        {
            private static readonly Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();

            /// <summary>
            /// Load an AssetBundle with automatic tracking.
            /// </summary>
            public static async Task<AssetBundle> LoadBundleAsync(string bundlePath, SmartAssetManager.AssetCategory category = SmartAssetManager.AssetCategory.General)
            {
                var request = AssetBundle.LoadFromFileAsync(bundlePath);
                await Task.Run(() => { while (!request.isDone) Task.Yield(); });

                if (request.assetBundle != null)
                {
                    loadedBundles[bundlePath] = request.assetBundle;

                    // Track the bundle itself
                    SmartAssetManager.Instance?.TrackAssetBundleAsset(request.assetBundle, category, request.assetBundle);
                }

                return request.assetBundle;
            }

            /// <summary>
            /// Load an asset from a bundle with automatic tracking.
            /// </summary>
            public static async Task<T> LoadAssetFromBundleAsync<T>(string bundlePath, string assetName, SmartAssetManager.AssetCategory category = SmartAssetManager.AssetCategory.General) where T : UnityEngine.Object
            {
                AssetBundle bundle;
                if (!loadedBundles.TryGetValue(bundlePath, out bundle))
                {
                    bundle = await LoadBundleAsync(bundlePath, category);
                }

                if (bundle == null) return null;

                var request = bundle.LoadAssetAsync<T>(assetName);
                await Task.Run(() => { while (!request.isDone) Task.Yield(); });

                if (request.asset != null)
                {
                    SmartAssetManager.Instance?.TrackAssetBundleAsset(request.asset, category, bundle);
                }

                return request.asset as T;
            }

            /// <summary>
            /// Unload a bundle and all its tracked assets.
            /// </summary>
            public static void UnloadBundle(string bundlePath, bool unloadAllLoadedObjects = false)
            {
                if (loadedBundles.TryGetValue(bundlePath, out AssetBundle bundle))
                {
                    bundle.Unload(unloadAllLoadedObjects);
                    loadedBundles.Remove(bundlePath);
                }
            }
        }

        #endregion

        #region Resources Integration

        /// <summary>
        /// Enhanced Resources loading with automatic SmartAssetManager tracking.
        /// </summary>
        public static class SmartResources
        {
            /// <summary>
            /// Load a resource with automatic tracking.
            /// </summary>
            public static T Load<T>(string path, SmartAssetManager.AssetCategory category = SmartAssetManager.AssetCategory.General) where T : UnityEngine.Object
            {
                var asset = Resources.Load<T>(path);
                if (asset != null)
                {
                    SmartAssetManager.Instance?.TrackResourcesAsset(asset, category);
                }
                return asset;
            }

            /// <summary>
            /// Load all resources of type with automatic tracking.
            /// </summary>
            public static T[] LoadAll<T>(string path, SmartAssetManager.AssetCategory category = SmartAssetManager.AssetCategory.General) where T : UnityEngine.Object
            {
                var assets = Resources.LoadAll<T>(path);
                foreach (var asset in assets)
                {
                    SmartAssetManager.Instance?.TrackResourcesAsset(asset, category);
                }
                return assets;
            }
        }

        #endregion

        #region Scene Management Integration

        /// <summary>
        /// Scene management helpers that coordinate with SmartAssetManager.
        /// </summary>
        public static class SmartSceneManager
        {
            /// <summary>
            /// Load a scene with automatic asset cleanup.
            /// </summary>
            public static async Task<Scene> LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
            {
                // Perform cleanup before loading new scene
                await SmartAssetManager.Instance?.ForceCleanupAsync(256); // Target 256MB before scene load

                var operation = SceneManager.LoadSceneAsync(sceneName, mode);
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                // Mark scene as loaded - you might want to track scene-specific assets here
                Debug.Log($"[SmartSceneManager] Scene '{sceneName}' loaded with memory cleanup");

                return SceneManager.GetSceneByName(sceneName);
            }

            /// <summary>
            /// Unload a scene with asset cleanup.
            /// </summary>
            public static async Task UnloadSceneAsync(string sceneName)
            {
                var operation = SceneManager.UnloadSceneAsync(sceneName);
                if (operation != null)
                {
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }
                }

                // Aggressive cleanup after scene unload
                await SmartAssetManager.Instance?.ForceCleanupAsync();

                Debug.Log($"[SmartSceneManager] Scene '{sceneName}' unloaded with memory cleanup");
            }
        }

        #endregion

        #region Category Helpers

        /// <summary>
        /// Helper methods for determining asset categories based on common patterns.
        /// </summary>
        public static class AssetCategoryHelper
        {
            /// <summary>
            /// Determine category based on asset path/name patterns.
            /// </summary>
            public static SmartAssetManager.AssetCategory DetermineCategory(string assetPath)
            {
                string lowerPath = assetPath.ToLower();

                if (lowerPath.Contains("scene") || lowerPath.Contains("level"))
                    return SmartAssetManager.AssetCategory.Scene;
                if (lowerPath.Contains("ui") || lowerPath.Contains("interface") || lowerPath.Contains("menu"))
                    return SmartAssetManager.AssetCategory.UI;
                if (lowerPath.Contains("character") || lowerPath.Contains("player") || lowerPath.Contains("npc"))
                    return SmartAssetManager.AssetCategory.Character;
                if (lowerPath.Contains("audio") || lowerPath.Contains("sound") || lowerPath.Contains("music"))
                    return SmartAssetManager.AssetCategory.Audio;
                if (lowerPath.Contains("effect") || lowerPath.Contains("particle") || lowerPath.Contains("vfx"))
                    return SmartAssetManager.AssetCategory.Effect;

                return SmartAssetManager.AssetCategory.General;
            }

            /// <summary>
            /// Determine category based on asset type.
            /// </summary>
            public static SmartAssetManager.AssetCategory DetermineCategory(Type assetType)
            {
                if (assetType == typeof(AudioClip))
                    return SmartAssetManager.AssetCategory.Audio;
                if (assetType == typeof(GameObject))
                    return SmartAssetManager.AssetCategory.General; // Could be more specific based on context
                if (assetType == typeof(Texture2D) || assetType == typeof(Sprite))
                    return SmartAssetManager.AssetCategory.UI; // Often UI-related

                return SmartAssetManager.AssetCategory.General;
            }
        }

        #endregion

        #region Performance Monitoring Integration

        /// <summary>
        /// Integration with the existing PerformanceMonitor system.
        /// </summary>
        public static class SmartAssetMonitor
        {
            private static bool isMonitoringEnabled = false;

            /// <summary>
            /// Enable integration with PerformanceMonitor.
            /// </summary>
            public static void EnablePerformanceMonitoring()
            {
                if (isMonitoringEnabled) return;

                isMonitoringEnabled = true;
                Debug.Log("[SmartAssetMonitor] Performance monitoring integration enabled");

                // Hook into PerformanceMonitor updates if it exists
                var performanceMonitor = UnityEngine.Object.FindFirstObjectByType<NightBlade.Core.Utils.Performance.PerformanceMonitor>();
                if (performanceMonitor != null)
                {
                    // Could add custom profiling markers here
                    Debug.Log("[SmartAssetMonitor] Connected to existing PerformanceMonitor");
                }
            }

            /// <summary>
            /// Get detailed performance statistics.
            /// </summary>
            public static string GetDetailedStats()
            {
                var assetStats = SmartAssetManager.Instance?.GetMemoryStats();
                if (assetStats == null) return "SmartAssetManager not available";

                var stats = $"Smart Asset Manager Stats:\n" +
                           $"- Memory Usage: {FormatBytes(assetStats.CurrentMemoryUsage)} / {FormatBytes(assetStats.TargetMemoryUsage)}\n" +
                           $"- Tracked Assets: {assetStats.TrackedAssetCount}\n" +
                           $"- Unload Queue: {assetStats.QueuedUnloadOperations}\n" +
                           $"- Aggressive Mode: {assetStats.IsAggressiveMode}\n" +
                           $"- Category Breakdown:\n";

                foreach (var kvp in assetStats.CategoryBreakdown)
                {
                    stats += $"  * {kvp.Key}: {FormatBytes(kvp.Value)}\n";
                }

                return stats;
            }

            private static string FormatBytes(long bytes)
            {
                string[] suffixes = { "B", "KB", "MB", "GB" };
                int suffixIndex = 0;
                double value = bytes;

                while (value >= 1024 && suffixIndex < suffixes.Length - 1)
                {
                    value /= 1024;
                    suffixIndex++;
                }

                return $"{value:F1}{suffixes[suffixIndex]}";
            }
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for easier SmartAssetManager integration.
    /// </summary>
    public static class SmartAssetExtensions
    {
        /// <summary>
        /// Track an Addressables handle result with the SmartAssetManager.
        /// </summary>
        public static void TrackWithSmartAssetManager<T>(this AsyncOperationHandle<T> handle, SmartAssetManager.AssetCategory category = SmartAssetManager.AssetCategory.General)
        {
            if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
            {
                SmartAssetManager.Instance?.TrackAddressableAsset(handle, category);
            }
        }

        /// <summary>
        /// Mark assets as critical for a specified duration.
        /// </summary>
        public static void MarkCritical(this IEnumerable<UnityEngine.Object> assets, float durationSeconds = 600f)
        {
            SmartAssetManager.Instance?.MarkAssetsCritical(assets, durationSeconds);
        }

        /// <summary>
        /// Mark a single asset as critical.
        /// </summary>
        public static void MarkCritical(this UnityEngine.Object asset, float durationSeconds = 600f)
        {
            if (asset != null)
            {
                SmartAssetManager.Instance?.MarkAssetsCritical(new[] { asset }, durationSeconds);
            }
        }
    }
}