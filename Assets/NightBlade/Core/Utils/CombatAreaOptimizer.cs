using System.Collections.Generic;
using UnityEngine;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Automatically detects combat hotspots and temporarily boosts performance
    /// in those areas to maintain gameplay quality during active combat.
    /// </summary>
    public class CombatAreaOptimizer : MonoBehaviour
    {
        [Header("Combat Detection Settings")]
        [Tooltip("Radius around entities to check for combat")]
        [SerializeField] private float combatDetectionRadius = 30f;

        [Tooltip("Minimum number of entities needed to trigger combat zone")]
        [SerializeField] private int combatEntityThreshold = 5;

        [Tooltip("How long combat zones persist after combat ends (seconds)")]
        [SerializeField] private float combatZoneDecayTime = 10f;

        [Tooltip("How often to scan for combat (seconds)")]
        [SerializeField] private float scanInterval = 2f;

        [Header("Zone Settings")]
        [Tooltip("Radius of created combat zones")]
        [SerializeField] private float combatZoneRadius = 25f;

        [Tooltip("Performance boost multiplier for combat zones")]
        [SerializeField] private float combatZonePriorityBoost = 2f;

        [Header("Debug Visualization")]
        [SerializeField] private bool showDebugCombatZones = true;
        [SerializeField] private Color combatDetectionColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private Color activeCombatZoneColor = new Color(1f, 0.5f, 0f, 0.4f);

        // References
        private CombatZoneManager zoneManager;
        private Dictionary<string, CombatHotspot> activeHotspots = new Dictionary<string, CombatHotspot>();

        // Timing
        private float lastScanTime;

        [System.Serializable]
        public class CombatHotspot
        {
            public string hotspotId;
            public Vector3 center;
            public int entityCount;
            public float lastCombatTime;
            public string zoneId; // Associated combat zone
            public List<GameObject> combatEntities = new List<GameObject>();
        }

        private void Start()
        {
            zoneManager = GetComponent<CombatZoneManager>();
            if (zoneManager == null)
            {
                zoneManager = gameObject.AddComponent<CombatZoneManager>();
                UnityEngine.Debug.Log("CombatZoneManager component added automatically");
            }
        }

        private void Update()
        {
            if (Time.time - lastScanTime >= scanInterval)
            {
                ScanForCombat();
                UpdateCombatZones();
                CleanupExpiredZones();
                lastScanTime = Time.time;
            }
        }

        private void ScanForCombat()
        {
            // Find all entities that could be in combat
            // This is a simplified implementation - in practice you'd check for
            // entities with combat components, health changes, etc.

            GameObject[] potentialCombatants = GameObject.FindGameObjectsWithTag("Combatant");

            // Group entities by proximity
            var proximityGroups = GroupEntitiesByProximity(potentialCombatants);

            foreach (var group in proximityGroups)
            {
                string hotspotId = GenerateHotspotId(group.Key);

                if (group.Value.Count >= combatEntityThreshold)
                {
                    // Combat detected - create or update hotspot
                    if (!activeHotspots.ContainsKey(hotspotId))
                    {
                        CreateCombatHotspot(hotspotId, group.Key, group.Value);
                    }
                    else
                    {
                        UpdateCombatHotspot(activeHotspots[hotspotId], group.Value);
                    }
                }
            }
        }

        private Dictionary<Vector3, List<GameObject>> GroupEntitiesByProximity(GameObject[] entities)
        {
            var groups = new Dictionary<Vector3, List<GameObject>>();

            foreach (var entity in entities)
            {
                bool addedToGroup = false;

                foreach (var existingGroup in groups)
                {
                    if (Vector3.Distance(entity.transform.position, existingGroup.Key) <= combatDetectionRadius)
                    {
                        existingGroup.Value.Add(entity);
                        addedToGroup = true;
                        break;
                    }
                }

                if (!addedToGroup)
                {
                    groups[entity.transform.position] = new List<GameObject> { entity };
                }
            }

            return groups;
        }

        private void CreateCombatHotspot(string hotspotId, Vector3 center, List<GameObject> entities)
        {
            // Create combat zone for this hotspot
            string zoneId = zoneManager.CreateCombatZone(center, combatZoneRadius, combatZonePriorityBoost);

            var hotspot = new CombatHotspot
            {
                hotspotId = hotspotId,
                center = center,
                entityCount = entities.Count,
                lastCombatTime = Time.time,
                zoneId = zoneId,
                combatEntities = new List<GameObject>(entities)
            };

            activeHotspots[hotspotId] = hotspot;

            if (showDebugCombatZones)
            {
                UnityEngine.Debug.Log($"Combat hotspot detected at {center} with {entities.Count} entities. Created zone: {zoneId}");
            }
        }

        private void UpdateCombatHotspot(CombatHotspot hotspot, List<GameObject> currentEntities)
        {
            hotspot.entityCount = currentEntities.Count;
            hotspot.lastCombatTime = Time.time;
            hotspot.combatEntities = new List<GameObject>(currentEntities);

            // Update zone center to current combat center
            Vector3 newCenter = CalculateGroupCenter(currentEntities);
            hotspot.center = newCenter;

            // Update the combat zone position
            // Note: This would require additional methods in CombatZoneManager to update existing zones
        }

        private void UpdateCombatZones()
        {
            foreach (var hotspot in activeHotspots.Values)
            {
                // Ensure the combat zone is still active
                zoneManager.ActivateCombatZone(hotspot.zoneId);
            }
        }

        private void CleanupExpiredZones()
        {
            var expiredHotspots = new List<string>();

            foreach (var hotspot in activeHotspots)
            {
                if (Time.time - hotspot.Value.lastCombatTime > combatZoneDecayTime)
                {
                    // Combat has ended - remove the zone
                    zoneManager.RemoveCombatZone(hotspot.Value.zoneId);
                    expiredHotspots.Add(hotspot.Key);

                    if (showDebugCombatZones)
                    {
                        UnityEngine.Debug.Log($"Combat hotspot expired: {hotspot.Key}");
                    }
                }
            }

            foreach (var expiredId in expiredHotspots)
            {
                activeHotspots.Remove(expiredId);
            }
        }

        private Vector3 CalculateGroupCenter(List<GameObject> entities)
        {
            if (entities.Count == 0) return Vector3.zero;

            Vector3 center = Vector3.zero;
            foreach (var entity in entities)
            {
                center += entity.transform.position;
            }
            return center / entities.Count;
        }

        private string GenerateHotspotId(Vector3 center)
        {
            return $"CombatHotspot_{center.x:F0}_{center.y:F0}_{center.z:F0}";
        }

        /// <summary>
        /// Gets the performance priority multiplier for a given position
        /// </summary>
        public float GetPriorityMultiplier(Vector3 position)
        {
            return zoneManager.GetPriorityMultiplier(position);
        }

        /// <summary>
        /// Checks if a position is in an active combat area
        /// </summary>
        public bool IsInCombatArea(Vector3 position)
        {
            return zoneManager.IsInCombatZone(position);
        }

        /// <summary>
        /// Gets statistics about active combat areas
        /// </summary>
        public CombatAreaStats GetStats()
        {
            return new CombatAreaStats
            {
                ActiveHotspots = activeHotspots.Count,
                TotalCombatZones = zoneManager.GetActiveCombatZones().Count,
                ZoneManagerStats = zoneManager.GetStats()
            };
        }

        /// <summary>
        /// Forces cleanup of all combat areas (for testing/debugging)
        /// </summary>
        public void ClearAllCombatAreas()
        {
            foreach (var hotspot in activeHotspots.Values)
            {
                zoneManager.RemoveCombatZone(hotspot.zoneId);
            }
            activeHotspots.Clear();
        }

        private void OnDrawGizmos()
        {
            if (!showDebugCombatZones) return;

            // Draw combat detection areas
            foreach (var hotspot in activeHotspots.Values)
            {
                Gizmos.color = combatDetectionColor;
                Gizmos.DrawSphere(hotspot.center, combatDetectionRadius);

                Gizmos.color = activeCombatZoneColor;
                Gizmos.DrawSphere(hotspot.center, combatZoneRadius);
            }
        }

        /// <summary>
        /// Statistics structure for combat area monitoring
        /// </summary>
        public struct CombatAreaStats
        {
            public int ActiveHotspots;
            public int TotalCombatZones;
            public CombatZoneManager.CombatZoneStats ZoneManagerStats;
        }
    }
}