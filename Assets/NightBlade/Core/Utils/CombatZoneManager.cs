using System.Collections.Generic;
using UnityEngine;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Manages combat zones where high-performance updates are maintained
    /// while reducing performance in non-combat areas for better CCU scaling.
    /// </summary>
    public class CombatZoneManager : MonoBehaviour
    {
        [System.Serializable]
        public class CombatZone
        {
            [Tooltip("Unique identifier for this combat zone")]
            public string zoneId;

            [Tooltip("Center point of the combat zone")]
            public Vector3 center;

            [Tooltip("Radius of the full combat zone (full performance)")]
            public float combatRadius = 25f;

            [Tooltip("Additional transition zone width (partial performance)")]
            public float transitionWidth = 10f;

            [Tooltip("Priority boost for entities in this zone (multiplier)")]
            public float priorityBoost = 2f;

            [Tooltip("Whether this zone is currently active")]
            public bool isActive = true;

            public float TotalRadius => combatRadius + transitionWidth;

            public ZoneType GetZoneType(Vector3 position)
            {
                float distance = Vector3.Distance(position, center);

                if (distance <= combatRadius)
                    return ZoneType.Combat;
                else if (distance <= TotalRadius)
                    return ZoneType.Transition;
                else
                    return ZoneType.Normal;
            }

            public float GetPriorityMultiplier(Vector3 position)
            {
                ZoneType zoneType = GetZoneType(position);

                switch (zoneType)
                {
                    case ZoneType.Combat:
                        return priorityBoost;
                    case ZoneType.Transition:
                        // Linear interpolation between combat and normal zones
                        float distance = Vector3.Distance(position, center);
                        float t = Mathf.InverseLerp(combatRadius, TotalRadius, distance);
                        return Mathf.Lerp(priorityBoost, 1f, t);
                    default:
                        return 1f;
                }
            }
        }

        public enum ZoneType
        {
            Normal,     // Outside combat zones - standard optimization
            Transition, // Edge of combat zones - partial boost
            Combat      // Inside combat zones - full performance boost
        }

        [Header("Combat Zone Configuration")]
        [SerializeField] private List<CombatZone> combatZones = new List<CombatZone>();

        [Header("Performance Settings")]
        [Tooltip("Frequency boost multiplier for combat zones")]
        [SerializeField] private float combatZoneFrequencyMultiplier = 3f;

        [Tooltip("Frequency boost multiplier for transition zones")]
        [SerializeField] private float transitionZoneFrequencyMultiplier = 1.5f;

        [Header("Debug Visualization")]
        [SerializeField] private bool showDebugZones = true;
        [SerializeField] private Color combatZoneColor = new Color(1f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color transitionZoneColor = new Color(1f, 1f, 0.5f, 0.2f);

        // Runtime tracking
        private Dictionary<string, CombatZone> activeCombatZones = new Dictionary<string, CombatZone>();

        private void Start()
        {
            InitializeCombatZones();
        }

        private void Update()
        {
            UpdateActiveZones();
        }

        private void InitializeCombatZones()
        {
            foreach (var zone in combatZones)
            {
                if (zone.isActive)
                {
                    activeCombatZones[zone.zoneId] = zone;
                }
            }
        }

        private void UpdateActiveZones()
        {
            // Update zone active status based on combat detection
            // This could be enhanced with actual combat detection logic
        }

        /// <summary>
        /// Gets the zone type for a given position
        /// </summary>
        public ZoneType GetZoneType(Vector3 position)
        {
            foreach (var zone in activeCombatZones.Values)
            {
                ZoneType zoneType = zone.GetZoneType(position);
                if (zoneType != ZoneType.Normal)
                    return zoneType;
            }
            return ZoneType.Normal;
        }

        /// <summary>
        /// Gets the performance priority multiplier for a given position
        /// </summary>
        public float GetPriorityMultiplier(Vector3 position)
        {
            float maxMultiplier = 1f;

            foreach (var zone in activeCombatZones.Values)
            {
                float multiplier = zone.GetPriorityMultiplier(position);
                maxMultiplier = Mathf.Max(maxMultiplier, multiplier);
            }

            return maxMultiplier;
        }

        /// <summary>
        /// Creates a dynamic combat zone at the specified location
        /// </summary>
        public string CreateCombatZone(Vector3 center, float radius, float priorityBoost = 2f, float transitionWidth = 10f)
        {
            string zoneId = $"DynamicZone_{Time.time}_{Random.Range(0, 1000)}";

            CombatZone newZone = new CombatZone
            {
                zoneId = zoneId,
                center = center,
                combatRadius = radius,
                transitionWidth = transitionWidth,
                priorityBoost = priorityBoost,
                isActive = true
            };

            activeCombatZones[zoneId] = newZone;
            return zoneId;
        }

        /// <summary>
        /// Removes a combat zone
        /// </summary>
        public void RemoveCombatZone(string zoneId)
        {
            activeCombatZones.Remove(zoneId);
        }

        /// <summary>
        /// Activates a combat zone (makes it provide performance boosts)
        /// </summary>
        public void ActivateCombatZone(string zoneId)
        {
            if (activeCombatZones.ContainsKey(zoneId))
            {
                activeCombatZones[zoneId].isActive = true;
            }
        }

        /// <summary>
        /// Deactivates a combat zone (removes performance boosts)
        /// </summary>
        public void DeactivateCombatZone(string zoneId)
        {
            if (activeCombatZones.ContainsKey(zoneId))
            {
                activeCombatZones[zoneId].isActive = false;
            }
        }

        /// <summary>
        /// Gets all active combat zones
        /// </summary>
        public Dictionary<string, CombatZone> GetActiveCombatZones()
        {
            return new Dictionary<string, CombatZone>(activeCombatZones);
        }

        /// <summary>
        /// Checks if a position is in any combat zone
        /// </summary>
        public bool IsInCombatZone(Vector3 position)
        {
            return GetZoneType(position) != ZoneType.Normal;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugZones) return;

            foreach (var zone in combatZones)
            {
                if (!zone.isActive) continue;

                // Draw combat zone
                Gizmos.color = combatZoneColor;
                Gizmos.DrawSphere(zone.center, zone.combatRadius);

                // Draw transition zone (wireframe)
                Gizmos.color = transitionZoneColor;
                Gizmos.DrawWireSphere(zone.center, zone.TotalRadius);
            }
        }

        /// <summary>
        /// Gets performance statistics for monitoring
        /// </summary>
        public CombatZoneStats GetStats()
        {
            return new CombatZoneStats
            {
                TotalZones = activeCombatZones.Count,
                ActiveZones = activeCombatZones.Count,
                AverageZoneRadius = CalculateAverageRadius()
            };
        }

        private float CalculateAverageRadius()
        {
            if (activeCombatZones.Count == 0) return 0f;

            float totalRadius = 0f;
            foreach (var zone in activeCombatZones.Values)
            {
                totalRadius += zone.combatRadius;
            }
            return totalRadius / activeCombatZones.Count;
        }

        /// <summary>
        /// Statistics structure for combat zone monitoring
        /// </summary>
        public struct CombatZoneStats
        {
            public int TotalZones;
            public int ActiveZones;
            public float AverageZoneRadius;
        }
    }
}