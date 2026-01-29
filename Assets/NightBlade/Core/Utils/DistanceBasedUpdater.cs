using UnityEngine;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Abstract base class for distance-based update frequency scaling.
    /// Automatically reduces update frequency for distant entities to improve performance.
    /// </summary>
    public abstract class DistanceBasedUpdater : MonoBehaviour
    {
        [Header("Distance Configuration")]
        [SerializeField] protected float[] distanceTiers = { 15f, 35f, 75f, 150f }; // Extended ranges for higher CCU
        [SerializeField] protected float[] updateFrequencies = { 50f, 15f, 3f, 0.5f, 0.1f }; // 5 tiers for better scaling

        [Header("Runtime Monitoring")]
        [SerializeField] protected bool showDebugInfo = false;

        protected float currentDistanceToPlayer = 0f;
        protected float currentUpdateInterval = 0f;
        protected float nextUpdateTime = 0f;
        protected Transform playerTransform;
        protected CombatZoneManager combatZoneManager;

        protected virtual void Start()
        {
            // Find player transform (could be improved with a service locator pattern)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // Fallback: use main camera
                playerTransform = Camera.main?.transform;
            }

            // Find combat zone manager
            combatZoneManager = FindFirstObjectByType<CombatZoneManager>();

            // Initialize timing
            UpdateDistanceAndInterval();
            nextUpdateTime = Time.time;
        }

        protected virtual void OnEnable()
        {
            // Try to find player again if it wasn't found in Start
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
                else
                {
                    playerTransform = Camera.main?.transform;
                }
            }
        }

        protected virtual void Update()
        {
            // Only update when distance-based timer expires
            if (Time.time >= nextUpdateTime)
            {
                UpdateDistanceAndInterval();
                PerformUpdate();
                nextUpdateTime = Time.time + currentUpdateInterval;
            }

            if (showDebugInfo)
            {
                DrawDebugInfo();
            }
        }

        /// <summary>
        /// Updates distance calculation and determines appropriate update interval.
        /// </summary>
        protected void UpdateDistanceAndInterval()
        {
            if (playerTransform == null) return;

            currentDistanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            currentUpdateInterval = GetUpdateIntervalForDistance(currentDistanceToPlayer);
        }

        /// <summary>
        /// Calculates the appropriate update interval based on distance and combat zones.
        /// </summary>
        /// <param name="distance">Distance from player</param>
        /// <returns>Update interval in seconds (1/frequency)</returns>
        protected float GetUpdateIntervalForDistance(float distance)
        {
            float baseInterval = GetBaseUpdateInterval(distance);

            // Apply combat zone boost if available
            if (combatZoneManager != null)
            {
                float combatMultiplier = combatZoneManager.GetPriorityMultiplier(transform.position);
                baseInterval /= combatMultiplier; // Higher priority = shorter interval = more updates
            }

            return baseInterval;
        }

        /// <summary>
        /// Gets the base update interval without combat zone modifications.
        /// </summary>
        protected float GetBaseUpdateInterval(float distance)
        {
            for (int i = 0; i < distanceTiers.Length; i++)
            {
                if (distance <= distanceTiers[i])
                {
                    return 1f / updateFrequencies[i]; // Convert FPS to interval
                }
            }

            // Beyond last tier - use slowest update rate
            return 1f / updateFrequencies[updateFrequencies.Length - 1];
        }

        /// <summary>
        /// Override this method to implement your specific update logic.
        /// This will be called at the distance-appropriate frequency.
        /// </summary>
        protected abstract void PerformUpdate();

        /// <summary>
        /// Draws debug information in the scene view.
        /// </summary>
        protected virtual void DrawDebugInfo()
        {
            // Draw distance indicator
            Debug.DrawLine(transform.position, playerTransform.position, Color.cyan, 0.1f);

#if UNITY_EDITOR
            // Show current tier and combat zone status
            int tier = GetCurrentTier();
            bool inCombatZone = combatZoneManager != null &&
                               combatZoneManager.IsInCombatZone(transform.position);

            GUI.color = GetTierColor(tier);
            string zoneStatus = inCombatZone ? "COMBAT" : "NORMAL";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
                $"Tier: {tier}\nDist: {currentDistanceToPlayer:F1}\nFPS: {1f/currentUpdateInterval:F1}\nZone: {zoneStatus}");
#endif
        }

        /// <summary>
        /// Gets the current distance tier for debugging.
        /// </summary>
        protected int GetCurrentTier()
        {
            for (int i = 0; i < distanceTiers.Length; i++)
            {
                if (currentDistanceToPlayer <= distanceTiers[i])
                {
                    return i;
                }
            }
            return distanceTiers.Length; // Beyond last tier
        }

        /// <summary>
        /// Gets color for debug visualization based on tier.
        /// </summary>
        protected Color GetTierColor(int tier)
        {
            switch (tier)
            {
                case 0: return Color.green;   // Close - full updates (0-15m)
                case 1: return Color.yellow;  // Medium - reduced updates (15-35m)
                case 2: return Color.orange;  // Far - minimal updates (35-75m)
                case 3: return Color.red;     // Very far - ultra minimal (75-150m)
                case 4: return Color.magenta; // Extremely far - minimal updates (150m+)
                default: return Color.gray;   // Unknown tier
            }
        }

        /// <summary>
        /// Forces an immediate update regardless of timing.
        /// Useful for important events like taking damage.
        /// </summary>
        public void ForceUpdate()
        {
            UpdateDistanceAndInterval();
            PerformUpdate();
            nextUpdateTime = Time.time + currentUpdateInterval;
        }

        /// <summary>
        /// Gets current performance statistics.
        /// </summary>
        public virtual DistanceUpdateStats GetStats()
        {
            return new DistanceUpdateStats
            {
                DistanceToPlayer = currentDistanceToPlayer,
                UpdateInterval = currentUpdateInterval,
                UpdateFrequency = 1f / currentUpdateInterval,
                CurrentTier = GetCurrentTier()
            };
        }

        /// <summary>
        /// Statistics structure for monitoring performance.
        /// </summary>
        public struct DistanceUpdateStats
        {
            public float DistanceToPlayer;
            public float UpdateInterval;
            public float UpdateFrequency;
            public int CurrentTier;
        }
    }
}
