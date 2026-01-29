using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Advanced smart asset unloading system for MMO optimization.
    /// Automatically manages asset memory usage with intelligent unloading strategies.
    /// </summary>
    public class SmartAssetManager : MonoBehaviour
    {
        [Header("Memory Management")]
        [SerializeField] private long targetMemoryUsageMB = 512; // Target memory usage in MB
        [SerializeField] private long criticalMemoryThresholdMB = 1024; // Critical threshold for aggressive unloading
        [SerializeField] private float memoryCheckInterval = 30f; // How often to check memory usage
        [SerializeField] private float aggressiveUnloadInterval = 10f; // Faster checks during high memory

        [Header("Unloading Strategy")]
        [SerializeField] private int maxUnloadBatchSize = 10; // Max assets to unload per batch
        [SerializeField] private float minTimeSinceLastAccess = 300f; // 5 minutes minimum before unloading
        [SerializeField] private float unloadBatchDelay = 0.1f; // Delay between unload operations

        [Header("Priority System")]
        [SerializeField] private float sceneAssetPriority = 10f; // High priority for scene assets
        [SerializeField] private float uiAssetPriority = 8f; // UI assets get high priority
        [SerializeField] private float characterAssetPriority = 6f; // Character models
        [SerializeField] private float effectAssetPriority = 3f; // Effects and particles
        [SerializeField] private float audioAssetPriority = 5f; // Audio assets

        [Header("Debug & Monitoring")]
        [SerializeField] private bool enableDetailedLogging = false;
        [SerializeField] private bool showMemoryStats = false;
        [SerializeField] private float statsUpdateInterval = 5f;

        // Editor foldout states for better organization
        [HideInInspector] public bool memoryManagementFoldout = true;
        [HideInInspector] public bool unloadingStrategyFoldout = true;
        [HideInInspector] public bool prioritySystemFoldout = true;
        [HideInInspector] public bool monitoringFoldout = false;

        // Singleton instance
        private static SmartAssetManager instance;
        public static SmartAssetManager Instance => instance;

        // Asset tracking
        private readonly Dictionary<object, AssetInfo> trackedAssets = new Dictionary<object, AssetInfo>();
        private readonly Dictionary<AssetCategory, List<object>> categoryAssets = new Dictionary<AssetCategory, List<object>>();
        private readonly Queue<UnloadOperation> unloadQueue = new Queue<UnloadOperation>();

        // Memory monitoring
        private long currentMemoryUsage = 0;
        private float lastMemoryCheck = 0f;
        private float lastStatsUpdate = 0f;
        private bool isAggressiveMode = false;

        // Thread safety
        private readonly object lockObject = new object();

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCategories();
                Debug.Log("[SmartAssetManager] Initialized - Target memory: " + targetMemoryUsageMB + "MB");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            float currentTime = Time.time;

            // Regular memory monitoring
            if (currentTime - lastMemoryCheck > (isAggressiveMode ? aggressiveUnloadInterval : memoryCheckInterval))
            {
                CheckMemoryUsage();
                lastMemoryCheck = currentTime;
            }

            // Process unload queue
            ProcessUnloadQueue();

            // Update stats display
            if (showMemoryStats && currentTime - lastStatsUpdate > statsUpdateInterval)
            {
                UpdateMemoryStats();
                lastStatsUpdate = currentTime;
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                // Perform final cleanup
                PerformFinalCleanup();
                instance = null;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Track an Addressables asset for smart unloading.
        /// </summary>
        public void TrackAddressableAsset<T>(AsyncOperationHandle<T> handle, AssetCategory category = AssetCategory.General)
        {
            if (!handle.IsValid()) return;

            UnityEngine.Object asset = handle.Result as UnityEngine.Object;
            if (asset != null)
            {
                long estimatedSize = EstimateAssetSize(asset);
                TrackAsset(asset, category, estimatedSize, AssetType.Addressables, handle);
            }
        }

        /// <summary>
        /// Track an AssetBundle asset for smart unloading.
        /// </summary>
        public void TrackAssetBundleAsset(UnityEngine.Object asset, AssetCategory category = AssetCategory.General, AssetBundle bundle = null)
        {
            long estimatedSize = EstimateAssetSize(asset);
            TrackAsset(asset, category, estimatedSize, AssetType.AssetBundle, bundle);
        }

        /// <summary>
        /// Track a Resources asset for smart unloading.
        /// </summary>
        public void TrackResourcesAsset(UnityEngine.Object asset, AssetCategory category = AssetCategory.General)
        {
            long estimatedSize = EstimateAssetSize(asset);
            TrackAsset(asset, category, estimatedSize, AssetType.Resources, null);
        }

        /// <summary>
        /// Manually untrack an asset (when you know it's no longer needed).
        /// </summary>
        public void UntrackAsset(UnityEngine.Object asset)
        {
            lock (lockObject)
            {
                if (trackedAssets.TryGetValue(asset, out AssetInfo info))
                {
                    currentMemoryUsage -= info.EstimatedSize;
                    trackedAssets.Remove(asset);

                    // Remove from category list
                    if (categoryAssets.ContainsKey(info.Category))
                    {
                        categoryAssets[info.Category].Remove(asset);
                    }

                    if (enableDetailedLogging)
                    {
                        Debug.Log($"[SmartAssetManager] Untracked asset: {asset.name}, freed: {FormatBytes(info.EstimatedSize)}");
                    }
                }
            }
        }

        /// <summary>
        /// Force immediate memory cleanup.
        /// </summary>
        public async Task ForceCleanupAsync(long targetMemoryMB = 0)
        {
            long targetMemory = targetMemoryMB > 0 ? targetMemoryMB * 1024 * 1024 : targetMemoryUsageMB * 1024 * 1024;

            if (enableDetailedLogging)
            {
                Debug.Log($"[SmartAssetManager] Force cleanup initiated. Target: {FormatBytes(targetMemory)}");
            }

            await UnloadToTargetMemory(targetMemory);
        }

        /// <summary>
        /// Get current memory statistics.
        /// </summary>
        public AssetMemoryStats GetMemoryStats()
        {
            lock (lockObject)
            {
                return new AssetMemoryStats
                {
                    CurrentMemoryUsage = currentMemoryUsage,
                    TargetMemoryUsage = targetMemoryUsageMB * 1024 * 1024,
                    TrackedAssetCount = trackedAssets.Count,
                    QueuedUnloadOperations = unloadQueue.Count,
                    IsAggressiveMode = isAggressiveMode,
                    CategoryBreakdown = GetCategoryBreakdown()
                };
            }
        }

        /// <summary>
        /// Preload critical assets to prevent unloading.
        /// </summary>
        public void MarkAssetsCritical(IEnumerable<UnityEngine.Object> assets, float duration = 600f) // 10 minutes default
        {
            float criticalUntil = Time.time + duration;

            lock (lockObject)
            {
                foreach (var asset in assets)
                {
                    if (trackedAssets.TryGetValue(asset, out AssetInfo info))
                    {
                        info.IsCritical = true;
                        info.CriticalUntil = criticalUntil;
                    }
                }
            }
        }

        #endregion

        #region Core Implementation

        private void InitializeCategories()
        {
            foreach (AssetCategory category in System.Enum.GetValues(typeof(AssetCategory)))
            {
                categoryAssets[category] = new List<object>();
            }
        }

        private void TrackAsset(UnityEngine.Object asset, AssetCategory category, long estimatedSize, AssetType assetType, object loaderReference)
        {
            if (asset == null) return;

            lock (lockObject)
            {
                if (!trackedAssets.ContainsKey(asset))
                {
                    var assetInfo = new AssetInfo
                    {
                        Asset = asset,
                        Category = category,
                        EstimatedSize = estimatedSize,
                        AssetType = assetType,
                        LoaderReference = loaderReference,
                        LoadTime = Time.time,
                        LastAccessTime = Time.time,
                        AccessCount = 1
                    };

                    trackedAssets[asset] = assetInfo;
                    categoryAssets[category].Add(asset);
                    currentMemoryUsage += estimatedSize;

                    if (enableDetailedLogging)
                    {
                        Debug.Log($"[SmartAssetManager] Tracked {assetType} asset: {asset.name}, size: {FormatBytes(estimatedSize)}, category: {category}");
                    }
                }
                else
                {
                    // Update existing asset access
                    trackedAssets[asset].LastAccessTime = Time.time;
                    trackedAssets[asset].AccessCount++;
                }
            }
        }

        private void CheckMemoryUsage()
        {
            long currentMemoryBytes = targetMemoryUsageMB * 1024 * 1024;
            long criticalMemoryBytes = criticalMemoryThresholdMB * 1024 * 1024;

            // Check if we're over critical threshold
            if (currentMemoryUsage > criticalMemoryBytes)
            {
                if (!isAggressiveMode)
                {
                    isAggressiveMode = true;
                    Debug.LogWarning($"[SmartAssetManager] CRITICAL MEMORY USAGE: {FormatBytes(currentMemoryUsage)} > {FormatBytes(criticalMemoryBytes)}. Entering aggressive mode.");
                }
            }
            else if (currentMemoryUsage > currentMemoryBytes)
            {
                // Over target but not critical
                if (isAggressiveMode)
                {
                    isAggressiveMode = false;
                    Debug.Log("[SmartAssetManager] Memory usage normalized. Exiting aggressive mode.");
                }

                // Queue unloading operation
                QueueUnloadOperation(currentMemoryBytes);
            }
            else if (isAggressiveMode && currentMemoryUsage < currentMemoryBytes * 0.8f)
            {
                // Memory usage is back to safe levels
                isAggressiveMode = false;
                Debug.Log("[SmartAssetManager] Memory usage safe. Exiting aggressive mode.");
            }
        }

        private void QueueUnloadOperation(long targetMemory)
        {
            var candidates = GetUnloadCandidates();
            if (candidates.Count == 0) return;

            // Prioritize by unload score (lower score = unload first)
            candidates.Sort((a, b) => a.UnloadScore.CompareTo(b.UnloadScore));

            int toUnload = Mathf.Min(maxUnloadBatchSize, candidates.Count);
            long expectedFreedMemory = 0;

            for (int i = 0; i < toUnload; i++)
            {
                expectedFreedMemory += candidates[i].Info.EstimatedSize;
                unloadQueue.Enqueue(new UnloadOperation
                {
                    AssetInfo = candidates[i].Info,
                    ExpectedMemoryFreed = candidates[i].Info.EstimatedSize
                });
            }

            if (enableDetailedLogging)
            {
                Debug.Log($"[SmartAssetManager] Queued {toUnload} assets for unloading, expected to free: {FormatBytes(expectedFreedMemory)}");
            }
        }

        private List<UnloadCandidate> GetUnloadCandidates()
        {
            var candidates = new List<UnloadCandidate>();
            float currentTime = Time.time;

            lock (lockObject)
            {
                foreach (var kvp in trackedAssets)
                {
                    var info = kvp.Value;

                    // Skip critical assets
                    if (info.IsCritical && currentTime < info.CriticalUntil)
                        continue;

                    // Skip recently accessed assets
                    if (currentTime - info.LastAccessTime < minTimeSinceLastAccess)
                        continue;

                    float unloadScore = CalculateUnloadScore(info, currentTime);
                    candidates.Add(new UnloadCandidate { Info = info, UnloadScore = unloadScore });
                }
            }

            return candidates;
        }

        private float CalculateUnloadScore(AssetInfo info, float currentTime)
        {
            // Lower score = higher priority for unloading

            // Base score from category priority (lower priority = higher unload score)
            float categoryScore = 10f - GetCategoryPriority(info.Category);

            // Time-based score (older = lower score = higher priority)
            float timeScore = Mathf.Clamp01((currentTime - info.LastAccessTime) / 3600f); // 1 hour max

            // Access frequency score (less accessed = lower score = higher priority)
            float accessScore = Mathf.Clamp01(1f - (info.AccessCount / 100f)); // Normalize access count

            // Size bonus (larger assets get slight priority)
            float sizeScore = Mathf.Clamp01(info.EstimatedSize / (10f * 1024 * 1024)); // 10MB max

            return categoryScore + timeScore + accessScore + sizeScore;
        }

        private float GetCategoryPriority(AssetCategory category)
        {
            switch (category)
            {
                case AssetCategory.Scene: return sceneAssetPriority;
                case AssetCategory.UI: return uiAssetPriority;
                case AssetCategory.Character: return characterAssetPriority;
                case AssetCategory.Audio: return audioAssetPriority;
                case AssetCategory.Effect: return effectAssetPriority;
                default: return 1f;
            }
        }

        private async void ProcessUnloadQueue()
        {
            if (unloadQueue.Count == 0) return;

            var operation = unloadQueue.Dequeue();
            await UnloadAssetAsync(operation);
        }

        private async Task UnloadAssetAsync(UnloadOperation operation)
        {
            try
            {
                bool success = false;

                switch (operation.AssetInfo.AssetType)
                {
                    case AssetType.Addressables:
                        if (operation.AssetInfo.LoaderReference is AsyncOperationHandle handle && handle.IsValid())
                        {
                            Addressables.Release(handle);
                            success = true;
                        }
                        break;

                    case AssetType.AssetBundle:
                        // For AssetBundles, we need the bundle reference
                        if (operation.AssetInfo.LoaderReference is AssetBundle bundle)
                        {
                            bundle.Unload(false); // Unload assets but keep bundle loaded
                            success = true;
                        }
                        break;

                    case AssetType.Resources:
                        // Resources assets are typically unloaded when scene changes
                        // We just untrack them from our system
                        success = true;
                        break;
                }

                if (success)
                {
                    UntrackAsset(operation.AssetInfo.Asset);

                    if (enableDetailedLogging)
                    {
                        Debug.Log($"[SmartAssetManager] Successfully unloaded asset: {operation.AssetInfo.Asset.name}, freed: {FormatBytes(operation.ExpectedMemoryFreed)}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SmartAssetManager] Failed to unload asset: {operation.AssetInfo.Asset?.name ?? "null"}");
                }

                // Small delay between operations to avoid frame spikes
                await Task.Delay((int)(unloadBatchDelay * 1000));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SmartAssetManager] Error unloading asset: {ex.Message}");
            }
        }

        private async Task UnloadToTargetMemory(long targetMemory)
        {
            while (currentMemoryUsage > targetMemory && trackedAssets.Count > 0)
            {
                var candidates = GetUnloadCandidates();
                if (candidates.Count == 0) break;

                candidates.Sort((a, b) => a.UnloadScore.CompareTo(b.UnloadScore));

                foreach (var candidate in candidates)
                {
                    await UnloadAssetAsync(new UnloadOperation
                    {
                        AssetInfo = candidate.Info,
                        ExpectedMemoryFreed = candidate.Info.EstimatedSize
                    });

                    if (currentMemoryUsage <= targetMemory) break;
                }

                // Yield to avoid blocking the main thread
                await Task.Yield();
            }
        }

        private long EstimateAssetSize(UnityEngine.Object asset)
        {
            if (asset == null) return 0;

            // Rough estimation based on asset type
            if (asset is Texture2D texture)
            {
                // Estimate: width * height * bytes per pixel * mipmap factor
                int mipmaps = texture.mipmapCount > 1 ? 2 : 1; // Rough mipmap estimation
                return (long)texture.width * texture.height * 4 * mipmaps; // Assume RGBA
            }
            else if (asset is AudioClip audio)
            {
                // Estimate: samples * channels * bytes per sample
                return (long)audio.samples * audio.channels * 2; // Assume 16-bit
            }
            else if (asset is Mesh mesh)
            {
                // Estimate: vertices * (position + normal + uv) + triangles
                long vertexData = (long)mesh.vertexCount * (12 + 12 + 8); // pos + normal + uv
                long triangleData = (long)mesh.triangles.Length * 4; // indices
                return vertexData + triangleData;
            }
            else if (asset is GameObject prefab)
            {
                // Rough estimate for prefabs - this is very approximate
                return 50 * 1024; // 50KB base estimate
            }

            // Default estimation for unknown types
            return 10 * 1024; // 10KB default
        }

        private Dictionary<AssetCategory, long> GetCategoryBreakdown()
        {
            var breakdown = new Dictionary<AssetCategory, long>();

            lock (lockObject)
            {
                foreach (var kvp in trackedAssets)
                {
                    var category = kvp.Value.Category;
                    if (!breakdown.ContainsKey(category))
                        breakdown[category] = 0;

                    breakdown[category] += kvp.Value.EstimatedSize;
                }
            }

            return breakdown;
        }

        private void UpdateMemoryStats()
        {
            var stats = GetMemoryStats();

            Debug.Log($"[SmartAssetManager] Memory: {FormatBytes(stats.CurrentMemoryUsage)} / {FormatBytes(stats.TargetMemoryUsage)} " +
                     $"| Assets: {stats.TrackedAssetCount} | Queue: {stats.QueuedUnloadOperations} " +
                     $"| Mode: {(stats.IsAggressiveMode ? "AGGRESSIVE" : "NORMAL")}");
        }

        private void PerformFinalCleanup()
        {
            // Aggressive cleanup on shutdown
            Debug.Log("[SmartAssetManager] Performing final cleanup...");

            lock (lockObject)
            {
                trackedAssets.Clear();
                foreach (var list in categoryAssets.Values)
                {
                    list.Clear();
                }
                unloadQueue.Clear();
                currentMemoryUsage = 0;
            }

            Debug.Log("[SmartAssetManager] Final cleanup complete.");
        }

        private string FormatBytes(long bytes)
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

        #endregion

        #region Data Structures

        public enum AssetCategory
        {
            Scene,
            UI,
            Character,
            Audio,
            Effect,
            General
        }

        public enum AssetType
        {
            Addressables,
            AssetBundle,
            Resources
        }

        public class AssetInfo
        {
            public UnityEngine.Object Asset;
            public AssetCategory Category;
            public long EstimatedSize;
            public AssetType AssetType;
            public object LoaderReference;
            public float LoadTime;
            public float LastAccessTime;
            public int AccessCount;
            public bool IsCritical;
            public float CriticalUntil;
        }

        public class AssetMemoryStats
        {
            public long CurrentMemoryUsage;
            public long TargetMemoryUsage;
            public int TrackedAssetCount;
            public int QueuedUnloadOperations;
            public bool IsAggressiveMode;
            public Dictionary<AssetCategory, long> CategoryBreakdown;
        }

        private class UnloadCandidate
        {
            public AssetInfo Info;
            public float UnloadScore;
        }

        private class UnloadOperation
        {
            public AssetInfo AssetInfo;
            public long ExpectedMemoryFreed;
        }

        #endregion
    }
}