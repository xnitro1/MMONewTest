using System.Collections.Generic;
using UnityEngine;

namespace NightBlade
{
    /// <summary>
    /// Distance-based optimization of NearbyEntityDetector.
    /// Reduces expensive sorting operations for distant entities while maintaining
    /// full performance for nearby entities where it matters most (combat, interaction).
    /// </summary>
    public class DistanceBasedNearbyEntityDetector : NightBlade.Core.Utils.DistanceBasedUpdater
    {
        [Header("Distance-Based Optimization")]
        [Tooltip("Enable sorting for each distance tier (close players need full sorting, distant players can use cached data)")]
        [SerializeField] private bool[] enableSortingByTier = { true, true, false, false, false }; // Updated for 5 tiers

        [Tooltip("Sorting frequency multiplier for each tier (1.0 = normal frequency, 0.1 = 10x less frequent)")]
        [SerializeField] private float[] sortingFrequencyByTier = { 1f, 0.5f, 0.1f, 0.05f, 0.01f }; // Updated for 5 tiers

        [Tooltip("Reduce trigger radius for distant players to save physics calculations")]
        [SerializeField] private bool optimizeTriggerRadius = true;

        [Tooltip("Trigger radius multipliers for each tier")]
        [SerializeField] private float[] triggerRadiusByTier = { 1f, 0.8f, 0.5f, 0.3f, 0.1f }; // Updated for 5 tiers

        [Header("Performance Monitoring")]
        [SerializeField] private bool logPerformanceStats = false;

        // References to the original NearbyEntityDetector components
        private NearbyEntityDetector originalDetector;
        private SphereCollider triggerCollider;

        // Performance tracking
        private int framesSinceLastSort = 0;
        private int totalSortOperations = 0;
        private float lastSortTime = 0f;

        protected override void Start()
        {
            base.Start();

            // Find or create the original NearbyEntityDetector
            originalDetector = GetComponent<NearbyEntityDetector>();
            if (originalDetector == null)
            {
                Debug.LogError("[DistanceBasedNearbyEntityDetector] No NearbyEntityDetector found! This component requires NearbyEntityDetector.");
                enabled = false;
                return;
            }

            // Get the trigger collider
            triggerCollider = GetComponent<SphereCollider>();
            if (triggerCollider == null)
            {
                // Try to find it
                triggerCollider = originalDetector.GetComponent<SphereCollider>();
            }

            // Disable the original Update() method by taking over the sorting
            // We'll call the sorting logic ourselves at distance-appropriate intervals

            if (logPerformanceStats)
            {
                Debug.Log($"[DistanceBasedNearbyEntityDetector] Initialized for {gameObject.name}. Distance tiers: {distanceTiers.Length}, Sorting enabled: {enableSortingByTier.Length}");
            }
        }

        protected override void Update()
        {
            // Handle distance-based timing (inherited from DistanceBasedUpdater)
            base.Update();

            // Update trigger radius for distant optimization
            if (optimizeTriggerRadius && triggerCollider != null)
            {
                UpdateTriggerRadiusForTier();
            }

            // Performance logging
            if (logPerformanceStats && Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
            {
                LogPerformanceStats();
            }
        }

        protected override void PerformUpdate()
        {
            if (originalDetector == null) return;

            int currentTier = GetCurrentTier();
            framesSinceLastSort++;

            // Check if we should sort this frame based on tier settings
            if (ShouldSortThisFrame(currentTier))
            {
                float sortStartTime = Time.realtimeSinceStartup;

                // Perform the expensive sorting operations
                PerformDistanceBasedSorting();

                float sortDuration = Time.realtimeSinceStartup - sortStartTime;
                totalSortOperations++;
                lastSortTime = sortDuration;

                framesSinceLastSort = 0;

                if (logPerformanceStats && sortDuration > 0.001f) // Log slow sorts
                {
                    Debug.Log($"[DistanceBasedNearbyEntityDetector] Sort took {sortDuration:F4}s for tier {currentTier}");
                }
            }
        }

        private bool ShouldSortThisFrame(int currentTier)
        {
            if (currentTier < 0 || currentTier >= enableSortingByTier.Length)
                return true; // Default to sorting if tier is invalid

            // Check if sorting is enabled for this tier
            if (!enableSortingByTier[currentTier])
                return false; // No sorting for this tier

            // Check frequency multiplier
            float frequencyMultiplier = (currentTier < sortingFrequencyByTier.Length) ?
                sortingFrequencyByTier[currentTier] : sortingFrequencyByTier[sortingFrequencyByTier.Length - 1];

            // Convert to frame interval (e.g., 0.1 = sort every 10 frames)
            int frameInterval = Mathf.Max(1, Mathf.RoundToInt(1f / frequencyMultiplier));

            return (framesSinceLastSort >= frameInterval);
        }

        private void PerformDistanceBasedSorting()
        {
            if (originalDetector == null) return;

            int currentTier = GetCurrentTier();

            // Get access to private fields via reflection or move sorting to public methods
            // For now, we'll duplicate the sorting logic with tier-based optimizations

            // Tier 0 & 1 (close/near): Full sorting for all entity types
            if (currentTier <= 1)
            {
                PerformFullSorting();
            }
            // Tier 2 (medium): Reduced sorting - skip less critical entities
            else if (currentTier == 2)
            {
                PerformReducedSorting();
            }
            // Tier 3 (far): Minimal sorting - only essential entities
            else
            {
                PerformMinimalSorting();
            }

            // Notify listeners that lists have been updated
            originalDetector.onUpdateList?.Invoke();
        }

        private void PerformFullSorting()
        {
            // Full sorting for close players - all entity types, full frequency
            SortEntityList(originalDetector.characters);
            SortEntityList(originalDetector.players);
            SortEntityList(originalDetector.monsters);
            SortEntityList(originalDetector.npcs);
            SortEntityList(originalDetector.itemDrops);
            SortEntityList(originalDetector.rewardDrops);
            SortEntityList(originalDetector.buildings);
            SortEntityList(originalDetector.vehicles);
            SortEntityList(originalDetector.warpPortals);
            SortEntityList(originalDetector.itemsContainers);

            SortActivatableList(originalDetector.activatableEntities);
            SortActivatableList(originalDetector.holdActivatableEntities);
            SortActivatableList(originalDetector.pickupActivatableEntities);
        }

        private void PerformReducedSorting()
        {
            // Reduced sorting for medium distance - skip less critical entities
            SortEntityList(originalDetector.characters);
            SortEntityList(originalDetector.players);
            SortEntityList(originalDetector.monsters);
            SortEntityList(originalDetector.npcs);
            // Skip item drops and rewards for medium distance
            SortEntityList(originalDetector.buildings);
            SortEntityList(originalDetector.vehicles);
            SortEntityList(originalDetector.warpPortals);
            SortEntityList(originalDetector.itemsContainers);

            // Keep activatable entities for interaction
            SortActivatableList(originalDetector.activatableEntities);
            SortActivatableList(originalDetector.holdActivatableEntities);
            SortActivatableList(originalDetector.pickupActivatableEntities);
        }

        private void PerformMinimalSorting()
        {
            // Minimal sorting for far distance - only essential entities
            SortEntityList(originalDetector.characters);
            SortEntityList(originalDetector.players);
            // Skip monsters, NPCs, items, buildings for far distance
            SortEntityList(originalDetector.warpPortals); // Keep portals for fast travel

            // Minimal activatable entities
            SortActivatableList(originalDetector.activatableEntities);
        }

        private void SortEntityList<T>(List<T> entities) where T : BaseGameEntity
        {
            if (entities == null || entities.Count <= 1) return;

            // Remove inactive entities first
            for (int i = entities.Count - 1; i >= 0; --i)
            {
                if (entities[i] == null || !entities[i].gameObject.activeInHierarchy)
                {
                    entities.RemoveAt(i);
                }
            }

            // Bubble sort by distance (same algorithm as original)
            T temp;
            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = 0; j < entities.Count - 1; j++)
                {
                    float dist1 = Vector3.Distance(entities[j].transform.position, transform.position);
                    float dist2 = Vector3.Distance(entities[j + 1].transform.position, transform.position);

                    if (dist1 > dist2)
                    {
                        temp = entities[j + 1];
                        entities[j + 1] = entities[j];
                        entities[j] = temp;
                    }
                }
            }
        }

        private void SortActivatableList<T>(List<T> entities) where T : IBaseActivatableEntity
        {
            if (entities == null || entities.Count <= 1) return;

            // Remove inactive entities
            for (int i = entities.Count - 1; i >= 0; --i)
            {
                var entity = entities[i] as MonoBehaviour;
                if (entity == null || !entity.gameObject.activeInHierarchy)
                {
                    entities.RemoveAt(i);
                }
            }

            // Sort by distance if needed
            if (entities.Count > 1)
            {
                entities.Sort((a, b) =>
                {
                    var entityA = a as MonoBehaviour;
                    var entityB = b as MonoBehaviour;
                    if (entityA == null || entityB == null) return 0;

                    float distA = Vector3.Distance(entityA.transform.position, transform.position);
                    float distB = Vector3.Distance(entityB.transform.position, transform.position);
                    return distA.CompareTo(distB);
                });
            }
        }

        private void UpdateTriggerRadiusForTier()
        {
            if (triggerCollider == null || originalDetector == null) return;

            int currentTier = GetCurrentTier();
            float radiusMultiplier = (currentTier < triggerRadiusByTier.Length) ?
                triggerRadiusByTier[currentTier] : triggerRadiusByTier[triggerRadiusByTier.Length - 1];

            float baseRadius = originalDetector.detectingRadius;
            float optimizedRadius = baseRadius * radiusMultiplier;

            // Only update if there's a meaningful change
            if (Mathf.Abs(triggerCollider.radius - optimizedRadius) > 0.1f)
            {
                triggerCollider.radius = optimizedRadius;

                if (logPerformanceStats)
                {
                    Debug.Log($"[DistanceBasedNearbyEntityDetector] Updated trigger radius to {optimizedRadius:F1}m (tier {currentTier})");
                }
            }
        }

        private void LogPerformanceStats()
        {
            int currentTier = GetCurrentTier();
            float sortFrequency = 1f / (framesSinceLastSort > 0 ? framesSinceLastSort : 1);

            Debug.Log($"[DistanceBasedNearbyEntityDetector] " +
                     $"Tier: {currentTier}, " +
                     $"Distance: {currentDistanceToPlayer:F1}m, " +
                     $"Sort Frequency: {sortFrequency:F2} FPS, " +
                     $"Last Sort: {lastSortTime:F4}s, " +
                     $"Total Sorts: {totalSortOperations}");
        }

        /// <summary>
        /// Force immediate sorting regardless of distance tier.
        /// Useful for critical updates like combat target acquisition.
        /// </summary>
        public void ForceImmediateSort()
        {
            PerformDistanceBasedSorting();
            framesSinceLastSort = 0;
        }

        /// <summary>
        /// Get performance statistics for monitoring.
        /// </summary>
        public new DistanceBasedDetectorStats GetStats()
        {
            return new DistanceBasedDetectorStats
            {
                BaseStats = base.GetStats(),
                CurrentTier = GetCurrentTier(),
                FramesSinceLastSort = framesSinceLastSort,
                TotalSortOperations = totalSortOperations,
                LastSortDuration = lastSortTime,
                SortingEnabledForCurrentTier = GetSortingEnabledForCurrentTier(),
                TriggerRadiusMultiplier = GetCurrentTriggerRadiusMultiplier()
            };
        }

        private bool GetSortingEnabledForCurrentTier()
        {
            int tier = GetCurrentTier();
            return tier >= 0 && tier < enableSortingByTier.Length && enableSortingByTier[tier];
        }

        private float GetCurrentTriggerRadiusMultiplier()
        {
            int tier = GetCurrentTier();
            return tier >= 0 && tier < triggerRadiusByTier.Length ?
                triggerRadiusByTier[tier] : triggerRadiusByTier[triggerRadiusByTier.Length - 1];
        }

        /// <summary>
        /// Statistics for monitoring distance-based detector performance.
        /// </summary>
        public struct DistanceBasedDetectorStats
        {
            public NightBlade.Core.Utils.DistanceBasedUpdater.DistanceUpdateStats BaseStats;
            public int CurrentTier;
            public int FramesSinceLastSort;
            public int TotalSortOperations;
            public float LastSortDuration;
            public bool SortingEnabledForCurrentTier;
            public float TriggerRadiusMultiplier;
        }
    }
}