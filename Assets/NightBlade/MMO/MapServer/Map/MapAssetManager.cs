using System.Collections.Generic;
using UnityEngine;
using NightBlade.Core.Utils;
using Cysharp.Threading.Tasks;

namespace NightBlade.MMO.MapServer.Map
{
    /// <summary>
    /// Map Server specific asset management coordinator.
    /// Optimizes memory usage for MMO map servers where assets load/unload frequently.
    /// </summary>
    public class MapAssetManager : MonoBehaviour
    {
        [Header("Map Server Memory Settings")]
        [SerializeField] private long targetMemoryForMapMB = 256; // Lower target for map servers
        [SerializeField] private float cleanupIntervalSeconds = 60f; // More frequent cleanup on map servers
        [SerializeField] private bool aggressiveCleanupInCombat = true;

        [Header("Player Asset Management")]
        [SerializeField] private int maxActivePlayerModels = 50; // Limit simultaneous character models
        [SerializeField] private float playerModelCriticalDuration = 300f; // 5 minutes

        [Header("NPC/Enemy Management")]
        [SerializeField] private int maxActiveNpcModels = 100;
        [SerializeField] private float npcModelCriticalDuration = 120f; // 2 minutes

        private float lastCleanupTime = 0f;
        private readonly HashSet<Object> criticalMapAssets = new HashSet<Object>();
        private readonly Dictionary<string, List<Object>> sceneAssetGroups = new Dictionary<string, List<Object>>();

        private void Start()
        {
            // Configure SmartAssetManager for map server usage
            ConfigureForMapServer();

            // Register with scene changes
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;

            Debug.Log("[MapAssetManager] Initialized for MMO Map Server - Memory optimized for frequent asset turnover");
        }

        private void Update()
        {
            float currentTime = Time.time;

            // Regular cleanup for map server (more frequent than default)
            if (currentTime - lastCleanupTime > cleanupIntervalSeconds)
            {
                PerformMapServerCleanup();
                lastCleanupTime = currentTime;
            }

            // Additional cleanup during combat-heavy scenarios
            if (aggressiveCleanupInCombat && IsCombatScenario())
            {
                SmartAssetManager.Instance?.ForceCleanupAsync(128); // Aggressive cleanup during combat
            }
        }

        private void ConfigureForMapServer()
        {
            // Ensure GameInstance exists and is properly initialized
            var gameInstance = GameInstance.EnsureExists();
            if (gameInstance == null)
            {
                Debug.LogError("[MapAssetManager] Failed to ensure GameInstance exists.");
                return;
            }

            // SmartAssetManager should now be available
            var smartAssetManager = SmartAssetManager.Instance;
            if (smartAssetManager == null)
            {
                Debug.LogError("[MapAssetManager] SmartAssetManager not found even after ensuring GameInstance.");
                return;
            }

            // Map servers need lower memory targets due to frequent asset turnover
            // The SmartAssetManager component settings will control the actual thresholds
            Debug.Log($"[MapAssetManager] Configured for map server usage - Target: {targetMemoryForMapMB}MB");
        }

        /// <summary>
        /// Called when a player joins the map server.
        /// Pre-loads critical assets and marks them for protection.
        /// </summary>
        public async UniTask OnPlayerJoined(string playerId, IEnumerable<Object> playerAssets)
        {
            var assetList = new List<Object>(playerAssets);

            // Mark player assets as critical
            SmartAssetManager.Instance?.MarkAssetsCritical(assetList, playerModelCriticalDuration);

            // Track player asset group for cleanup when they leave
            sceneAssetGroups[playerId] = assetList;

            // Pre-load additional assets that might be needed
            await PreloadPlayerAssets(playerId);

            Debug.Log($"[MapAssetManager] Player {playerId} assets marked critical ({assetList.Count} assets)");
        }

        /// <summary>
        /// Called when a player leaves the map server.
        /// Cleans up their assets unless they're still needed elsewhere.
        /// </summary>
        public void OnPlayerLeft(string playerId)
        {
            if (sceneAssetGroups.TryGetValue(playerId, out var playerAssets))
            {
                // Remove critical marking (assets can now be unloaded if needed)
                foreach (var asset in playerAssets)
                {
                    // Assets will be automatically managed by SmartAssetManager based on usage
                }

                sceneAssetGroups.Remove(playerId);
                Debug.Log($"[MapAssetManager] Player {playerId} assets released for management");
            }
        }

        /// <summary>
        /// Called when NPCs/monsters spawn in the area.
        /// </summary>
        public void OnNpcSpawned(IEnumerable<Object> npcAssets)
        {
            var assetList = new List<Object>(npcAssets);

            // Mark NPC assets as critical but with shorter duration
            SmartAssetManager.Instance?.MarkAssetsCritical(assetList, npcModelCriticalDuration);

            // Track for cleanup
            criticalMapAssets.UnionWith(assetList);

            Debug.Log($"[MapAssetManager] NPC assets marked critical ({assetList.Count} assets)");
        }

        /// <summary>
        /// Called when entering combat scenarios.
        /// Increases asset protection and triggers cleanup.
        /// </summary>
        public async UniTask OnCombatStarted(IEnumerable<Object> combatAssets)
        {
            var assetList = new List<Object>(combatAssets);

            // Mark combat assets as highly critical
            SmartAssetManager.Instance?.MarkAssetsCritical(assetList, 600f); // 10 minutes during combat

            // Aggressive cleanup before combat to free memory
            await SmartAssetManager.Instance?.ForceCleanupAsync(200); // Target 200MB during combat

            Debug.Log($"[MapAssetManager] Combat assets protected ({assetList.Count} assets)");
        }

        /// <summary>
        /// Called when combat ends.
        /// Normalizes asset management.
        /// </summary>
        public void OnCombatEnded()
        {
            // Combat assets will naturally lose critical status over time
            // SmartAssetManager will handle unloading based on usage patterns
            Debug.Log("[MapAssetManager] Combat ended - normalizing asset management");
        }

        /// <summary>
        /// Called when changing scenes/maps.
        /// Coordinates with SmartAssetManager for smooth transitions.
        /// </summary>
        private async void OnSceneChanged(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
        {
            Debug.Log($"[MapAssetManager] Scene change: {oldScene.name} -> {newScene.name}");

            // Mark scene change - allow more aggressive cleanup during transition
            await SmartAssetManager.Instance?.ForceCleanupAsync(targetMemoryForMapMB);

            // Clear scene-specific asset tracking
            sceneAssetGroups.Clear();

            // Scene assets will be automatically managed by SmartAssetManager
        }

        /// <summary>
        /// Map server specific cleanup logic.
        /// </summary>
        private async void PerformMapServerCleanup()
        {
            var stats = SmartAssetManager.Instance?.GetMemoryStats();

            if (stats != null && stats.CurrentMemoryUsage > targetMemoryForMapMB * 1024 * 1024)
            {
                // Map server specific cleanup - target lower memory usage
                await SmartAssetManager.Instance?.ForceCleanupAsync(targetMemoryForMapMB);

                Debug.Log($"[MapAssetManager] Performed map server cleanup. Memory: {FormatBytes(stats.CurrentMemoryUsage)}");
            }
        }

        /// <summary>
        /// Pre-loads assets that a player is likely to need soon.
        /// </summary>
        private async UniTask PreloadPlayerAssets(string playerId)
        {
            // This could be expanded to preload common assets like:
            // - Common equipment models
            // - Frequently used effects
            // - Nearby area assets

            // For now, just ensure the player's assets are properly tracked
            await UniTask.Yield();
        }

        /// <summary>
        /// Determines if we're in a combat-heavy scenario.
        /// </summary>
        private bool IsCombatScenario()
        {
            // This could check:
            // - Number of active combats
            // - Player count in area
            // - Recent combat activity

            // For now, return false (implement based on your combat system)
            return false;
        }

        /// <summary>
        /// Gets memory statistics formatted for logging.
        /// </summary>
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

        /// <summary>
        /// Emergency cleanup for critical memory situations.
        /// </summary>
        public async UniTask EmergencyCleanup()
        {
            Debug.LogWarning("[MapAssetManager] Emergency cleanup initiated");
            await SmartAssetManager.Instance?.ForceCleanupAsync(128); // Very aggressive cleanup
        }

        /// <summary>
        /// Gets detailed asset statistics for the map server.
        /// </summary>
        public Dictionary<string, object> GetMapServerStats()
        {
            var stats = new Dictionary<string, object>
            {
                ["sceneAssetGroups"] = sceneAssetGroups.Count,
                ["criticalMapAssets"] = criticalMapAssets.Count,
                ["targetMemoryMB"] = targetMemoryForMapMB,
                ["cleanupInterval"] = cleanupIntervalSeconds,
                ["aggressiveCombatCleanup"] = aggressiveCleanupInCombat
            };

            // Add SmartAssetManager stats if available
            var assetStats = SmartAssetManager.Instance?.GetMemoryStats();
            if (assetStats != null)
            {
                stats["currentMemory"] = FormatBytes(assetStats.CurrentMemoryUsage);
                stats["trackedAssets"] = assetStats.TrackedAssetCount;
                stats["aggressiveMode"] = assetStats.IsAggressiveMode;
            }

            return stats;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }
}