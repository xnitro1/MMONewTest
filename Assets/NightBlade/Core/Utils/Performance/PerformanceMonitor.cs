using UnityEngine;
using System.Collections;

namespace NightBlade.Core.Utils.Performance
{
    /// <summary>
    /// Simple performance stats structure for initial compilation.
    /// </summary>
    public struct PerformanceStatsStruct
    {
        public float FrameTime;
        public float FPS;
        public long GCAllocationsThisFrame;
        public long TotalMemoryUsed;
        public int NetworkStringsCached;
        public int UIPoolSize;
        public int DistanceBasedEntities;
        public int PooledCoroutinesActive;
        public int StringBuilderPoolSize;
        public int CollectionPoolSize;
        public int MaterialPropertyBlockPoolSize;
        public int DelegatePoolSize;
        public int NetDataWriterPoolSize;
        public int JsonOperationPoolSize;
        public int FxCollectionPoolSize;
    }

    /// <summary>
    /// Performance Monitor - Coordinator for performance monitoring system.
    /// Refactored from 1,411 lines to focused coordination role.
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Monitoring Configuration")]
        [SerializeField] private bool enableDetailedLogging = false;
        [SerializeField] private float statsUpdateInterval = 1f;
        [Tooltip("Enable performance monitor GUI overlay (disabled by default in production)")]
        [SerializeField] private bool showGUIStats = false;

        // Component references
        private PerformanceRenderer renderer;
        private PerformanceProfiler profiler;

        // Stats tracking
        private PerformanceStatsStruct currentStats;
        private float lastStatsUpdate = 0f;

        /// <summary>
        /// Public access to enable/disable the performance monitor GUI.
        /// </summary>
        public bool ShowGUIStats
        {
            get { return showGUIStats; }
            set 
            { 
                showGUIStats = value;
                if (renderer != null)
                {
                    renderer.SetGUIVisible(value);
                }
            }
        }

        // Singleton instance
        private static PerformanceMonitor instance;
        public static PerformanceMonitor Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                // Initialize components
                InitializeComponents();

                // Initialize stats on startup
                UpdateStats();
                lastStatsUpdate = Time.time;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeComponents()
        {
            // Create renderer component
            var rendererGO = new GameObject("PerformanceRenderer");
            rendererGO.transform.SetParent(transform);
            renderer = rendererGO.AddComponent<PerformanceRenderer>();
            
            // Set initial GUI visibility
            if (renderer != null)
            {
                renderer.SetGUIVisible(showGUIStats);
            }

            // Create profiler component
            profiler = new PerformanceProfiler();
        }

        private void Update()
        {
            // Keyboard shortcuts
            if (NightBlade.CameraAndInput.InputManager.GetKeyDown(KeyCode.F12))
            {
                if (renderer != null)
                {
                    renderer.ToggleGUI();
                    Debug.Log($"[PerformanceMonitor] GUI toggled (F12)");
                }
            }

            if (NightBlade.CameraAndInput.InputManager.GetKeyDown(KeyCode.F11))
            {
                if (profiler != null)
                {
                    profiler.RunMiniBenchmark();
                }
            }

            if (Time.time - lastStatsUpdate >= statsUpdateInterval)
            {
                UpdateStats();
                lastStatsUpdate = Time.time;

                if (enableDetailedLogging)
                {
                    LogPerformanceStats();
                }
            }
        }

        // NOTE: OnGUI method moved to PerformanceRenderer.cs for better separation of concerns
        // The original OnGUI method was 1,200+ lines and is now handled by the dedicated renderer component

        /// <summary>
        /// Updates performance statistics.
        /// </summary>
        public void UpdateStats()
        {
            // Calculate current stats
            currentStats.FrameTime = Time.deltaTime;
            currentStats.FPS = 1f / Time.deltaTime;
            
            // Use full stats calculation from PerformanceStats
            var fullStats = PerformanceStats.CalculateCurrentStats();
            
            // Update components
            if (profiler != null)
            {
                profiler.UpdatePeakStats(fullStats);
            }
            
            if (renderer != null)
            {
                renderer.UpdateStatsData(fullStats);
            }
        }

        /// <summary>
        /// Gets the current performance statistics.
        /// </summary>
        public PerformanceStatsStruct GetStats()
        {
            return currentStats;
        }

        /// <summary>
        /// Tests all optimization systems.
        /// </summary>
        public void TestAllSystems()
        {
            if (profiler != null)
            {
                profiler.TestAllSystems();
            }
            else
            {
                Debug.LogWarning("[PerformanceMonitor] Profiler not initialized");
            }
        }

        /// <summary>
        /// Debug method to check system status.
        /// </summary>
        public void DebugSystemStatus()
        {
            if (profiler != null)
            {
                profiler.DebugSystemStatus();
            }
            else
            {
                Debug.LogWarning("[PerformanceMonitor] Profiler not initialized");
            }
        }

        /// <summary>
        /// Starts a coroutine (used by profiler for delayed operations).
        /// </summary>
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return base.StartCoroutine(routine);
        }

        private void LogPerformanceStats()
        {
            Debug.Log($"[PerformanceMonitor] FPS: {currentStats.FPS:F1}, Memory: {currentStats.TotalMemoryUsed}MB, GC: {currentStats.GCAllocationsThisFrame}MB");
        }
    }
}