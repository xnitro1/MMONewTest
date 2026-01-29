using UnityEngine;
using UnityEditor;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Utility to upgrade existing NearbyEntityDetector components to use distance-based optimization.
    /// Also includes runtime auto-upgrader for network entities.
    /// </summary>
    public static class DistanceBasedNearbyEntityDetectorUpgrader
    {
#if UNITY_EDITOR
        [MenuItem("NightBlade/Performance/Upgrade NearbyEntityDetectors to Distance-Based")]
        public static void UpgradeAllNearbyEntityDetectors()
        {
            var detectors = Object.FindObjectsByType<NightBlade.NearbyEntityDetector>(FindObjectsSortMode.None);

            int upgraded = 0;
            foreach (var detector in detectors)
            {
                if (detector.GetComponent<DistanceBasedNearbyEntityDetector>() == null)
                {
                    detector.gameObject.AddComponent<DistanceBasedNearbyEntityDetector>();
                    upgraded++;
                }
            }

            Debug.Log($"[DistanceBasedNearbyEntityDetectorUpgrader] Upgraded {upgraded} NearbyEntityDetector components to use distance-based optimization");
        }

        [MenuItem("NightBlade/Performance/Remove Distance-Based NearbyEntityDetectors")]
        public static void RemoveAllDistanceBasedDetectors()
        {
            var detectors = Object.FindObjectsByType<DistanceBasedNearbyEntityDetector>(FindObjectsSortMode.None);

            int removed = 0;
            foreach (var detector in detectors)
            {
                Object.DestroyImmediate(detector);
                removed++;
            }

            Debug.Log($"[DistanceBasedNearbyEntityDetectorUpgrader] Removed {removed} DistanceBasedNearbyEntityDetector components");
        }
#endif
    }

    /// <summary>
    /// Runtime component that automatically adds distance-based optimization to NearbyEntityDetector components.
    /// Attach this to any GameObject that spawns entities with NearbyEntityDetector.
    /// </summary>
    public class DistanceBasedEntityAutoUpgrader : MonoBehaviour
    {
        [Tooltip("Automatically upgrade entities with NearbyEntityDetector components")]
        [SerializeField] private bool autoUpgradeOnStart = true;

        [Tooltip("Check for new entities every N seconds")]
        [SerializeField] private float checkInterval = 5f;

        private float lastCheckTime = 0f;

        private void Start()
        {
            if (autoUpgradeOnStart)
            {
                UpgradeNearbyEntityDetectorsInScene();
            }
        }

        private void Update()
        {
            if (Time.time - lastCheckTime >= checkInterval)
            {
                UpgradeNearbyEntityDetectorsInScene();
                lastCheckTime = Time.time;
            }
        }

        /// <summary>
        /// Upgrade all NearbyEntityDetector components in the scene that don't already have distance-based optimization.
        /// </summary>
        public void UpgradeNearbyEntityDetectorsInScene()
        {
            var detectors = FindObjectsOfType<NightBlade.NearbyEntityDetector>(true);
            int upgraded = 0;

            foreach (var detector in detectors)
            {
                if (detector.GetComponent<DistanceBasedNearbyEntityDetector>() == null)
                {
                    detector.gameObject.AddComponent<DistanceBasedNearbyEntityDetector>();
                    upgraded++;
                }
            }

            if (upgraded > 0)
            {
                Debug.Log($"[DistanceBasedEntityAutoUpgrader] Auto-upgraded {upgraded} entities with distance-based optimization");
            }
        }
    }
}