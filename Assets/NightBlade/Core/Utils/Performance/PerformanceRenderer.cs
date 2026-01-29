using UnityEngine;
using NightBlade.CameraAndInput;

namespace NightBlade.Core.Utils.Performance
{
    /// <summary>
    /// Handles GUI rendering for the Performance Monitor.
    /// Separated from main logic for better organization and performance.
    /// Originally 1,200+ lines in PerformanceMonitor.cs, now dedicated component.
    /// </summary>
    public class PerformanceRenderer : MonoBehaviour
    {
        private PerformanceStats.Stats currentStats;
        private bool showGUI = true;
        private bool showSimpleView = true;
        private bool showDetailedStats = false;
        
        // GUI positioning (will be centered at top in OnGUI)
        private Rect windowRect = new Rect(0, 10, 600, 500);
        private Rect minimizedRect = new Rect(0, 10, 150, 30);
        
        // Update timing
        private float lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 1f;
        private const float MEMORY_UPDATE_INTERVAL = 0.5f; // Update memory less frequently to avoid GC pressure
        private float lastMemoryUpdate = 0f;
        
        // Real-time FPS tracking (updates every frame)
        private float currentFPS = 0f;
        private float currentFrameTime = 0f;
        
        // Memory tracking (updates every 0.5s to avoid causing GC itself)
        private long currentMemory = 0L;
        private long currentGCAlloc = 0L;
        private long lastMonoUsed = 0L;
        
        // Color coding
        private Color goodColor = new Color(0.3f, 0.8f, 0.3f);
        private Color warningColor = new Color(0.9f, 0.7f, 0.2f);
        private Color criticalColor = new Color(0.9f, 0.3f, 0.3f);
        
        // Styles (created in OnGUI)
        private GUIStyle headerStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;
        private bool stylesInitialized = false;

        /// <summary>
        /// Updates the stats data to be displayed.
        /// </summary>
        public void UpdateStatsData(PerformanceStats.Stats stats)
        {
            currentStats = stats;
        }

        /// <summary>
        /// Toggles GUI visibility.
        /// </summary>
        public void ToggleGUI()
        {
            showGUI = !showGUI;
            Debug.Log($"[PerformanceRenderer] GUI toggled: {(showGUI ? "Visible" : "Hidden")}");
        }

        /// <summary>
        /// Sets GUI visibility.
        /// </summary>
        public void SetGUIVisible(bool visible)
        {
            showGUI = visible;
        }

        private void Update()
        {
            // Update FPS every frame for smooth tracking (lightweight)
            currentFPS = 1f / Time.deltaTime;
            currentFrameTime = Time.deltaTime;
            
            // Update memory/GC stats less frequently to avoid causing GC pressure from the profiler itself!
            // System.GC.GetTotalMemory() can be expensive and trigger GC checks
            if (Time.time - lastMemoryUpdate >= MEMORY_UPDATE_INTERVAL)
            {
                long newMemory = System.GC.GetTotalMemory(false);
                long newMonoUsed = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
                
                // Calculate GC pressure (difference in mono memory usage)
                if (lastMonoUsed > 0)
                {
                    long memoryDelta = newMonoUsed - lastMonoUsed;
                    // Divide by frames elapsed to get per-frame allocation estimate
                    float framesElapsed = MEMORY_UPDATE_INTERVAL / Time.deltaTime;
                    currentGCAlloc = (long)(memoryDelta / framesElapsed);
                    if (currentGCAlloc < 0) currentGCAlloc = 0;
                }
                
                currentMemory = newMemory;
                lastMonoUsed = newMonoUsed;
                lastMemoryUpdate = Time.time;
            }
            
            // Track update timing for display
            if (Time.time - lastUpdateTime >= UPDATE_INTERVAL)
            {
                lastUpdateTime = Time.time;
            }
        }

        private void OnGUI()
        {
            if (!showGUI) return;

            InitializeStyles();

            // Center the GUI at the top of the screen
            if (showSimpleView)
            {
                DrawSimpleOverlay();
            }
            else
            {
                // Center the detailed window
                windowRect.x = (Screen.width - windowRect.width) / 2;
                windowRect = GUI.Window(0, windowRect, DrawDetailedWindow, "🖥️ Performance Monitor - NightBlade");
            }
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.9f, 0.9f, 0.9f) }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10
            };

            stylesInitialized = true;
        }

        private void DrawSimpleOverlay()
        {
            // Calculate centered position at top
            float boxWidth = 380f;
            float boxHeight = 260f;
            float xPos = (Screen.width - boxWidth) / 2;
            float yStart = 10f;

            // Background box (centered at top)
            GUI.Box(new Rect(xPos, yStart, boxWidth, boxHeight), "");

            float yPos = yStart + 10;
            float lineHeight = 20f;
            float leftMargin = xPos + 10;

            // Header
            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), "🖥️ Performance Monitor - NightBlade", headerStyle);
            yPos += lineHeight + 5;

            // FPS (real-time updates every frame)
            Color fpsColor = GetFPSColor(currentFPS);
            GUI.color = fpsColor;
            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                $"⚡ FPS: {currentFPS:F1} ({currentFrameTime * 1000:F1}ms)", labelStyle);
            GUI.color = Color.white;
            yPos += lineHeight;

            // Memory (real-time)
            float memoryMB = currentMemory / 1024f / 1024f;
            Color memoryColor = GetMemoryColor(memoryMB);
            GUI.color = memoryColor;
            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                $"🗂️ Memory: {memoryMB:F1}MB", labelStyle);
            GUI.color = Color.white;
            yPos += lineHeight;

            // GC Allocations (real-time)
            float gcKB = currentGCAlloc / 1024f;
            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                $"♻️ GC Pressure: {gcKB:F2}KB/frame", labelStyle);
            yPos += lineHeight + 5;

            // System stats (update every second)
            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                $"🌐 Network Strings: {GetNetworkStringsDisplay()}", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                $"🎨 UI Objects Pooled: {GetUIPoolDisplay()}", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                $"📏 Distance Entities: {GetDistanceEntitiesDisplay()}", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                $"🔄 Pooled Coroutines: {GetCoroutinesDisplay()}", labelStyle);
            yPos += lineHeight;

            // Pool summary - show total objects ready for reuse
            int totalPooled = GetTotalPooledObjects();
            int activeSystems = GetActivePoolSystemsCount();
            Color poolColor = totalPooled > 50 ? goodColor : (totalPooled > 20 ? warningColor : Color.white);
            GUI.color = poolColor;
            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                $"📈 Pooled Objects: {totalPooled} ({activeSystems}/9 systems)", labelStyle);
            GUI.color = Color.white;
            yPos += lineHeight + 5;

            // Buttons
            float buttonY = yPos;
            float buttonWidth = 85f;
            float buttonSpacing = 5f;
            float buttonX = leftMargin;

            if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, 25), "📊 Detailed", buttonStyle))
            {
                showSimpleView = false;
            }
            buttonX += buttonWidth + buttonSpacing;

            if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, 25), "🎯 Benchmark", buttonStyle))
            {
                RunBenchmark();
            }
            buttonX += buttonWidth + buttonSpacing;

            if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, 25), "🙈 Hide", buttonStyle))
            {
                showGUI = false;
            }
            buttonX += buttonWidth + buttonSpacing;

            if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, 25), "🐛 Debug", buttonStyle))
            {
                DebugSystemStatus();
            }

            // Shortcuts info
            yPos = buttonY + 30;
            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                "💡 Shortcuts: F11 Benchmark | F12 Toggle GUI", labelStyle);
            yPos += lineHeight;

            // Update time
            float timeSinceUpdate = Time.time - lastUpdateTime;
            GUI.Label(new Rect(leftMargin, yPos, 360, lineHeight), 
                $"📊 Updated {timeSinceUpdate:F1}s ago", labelStyle);
        }

        private void DrawDetailedWindow(int windowID)
        {
            float yPos = 25;
            float lineHeight = 18f;
            float leftCol = 10f;
            float rightCol = 310f;
            float colWidth = 280f;

            // ═══════════════════════════════════════════════════════════════
            // SECTION 1: Real-Time Performance
            // ═══════════════════════════════════════════════════════════════
            GUI.Label(new Rect(leftCol, yPos, colWidth, lineHeight), "⚡ Real-Time Performance", headerStyle);
            yPos += lineHeight + 3;

            DrawColoredStat(leftCol, yPos, $"FPS: {currentFPS:F1}", GetFPSColor(currentFPS));
            DrawColoredStat(rightCol, yPos, $"Frame: {currentFrameTime * 1000:F2}ms", GetFrameTimeColor(currentFrameTime));
            yPos += lineHeight;

            float memoryMB = currentMemory / 1024f / 1024f;
            DrawColoredStat(leftCol, yPos, $"Memory: {memoryMB:F1}MB", GetMemoryColor(memoryMB));
            float gcKB = currentGCAlloc / 1024f;
            GUI.Label(new Rect(rightCol, yPos, colWidth, lineHeight), $"GC: {gcKB:F2}KB/f", labelStyle);
            yPos += lineHeight + 10;

            // ═══════════════════════════════════════════════════════════════
            // SECTION 2: Object Pooling Systems
            // ═══════════════════════════════════════════════════════════════
            GUI.Label(new Rect(leftCol, yPos, colWidth * 2, lineHeight), "🔄 Object Pooling Systems", headerStyle);
            yPos += lineHeight + 3;

            GUI.Label(new Rect(leftCol, yPos, colWidth, lineHeight), $"StringBuilder: {currentStats.StringBuilderPoolSize}", labelStyle);
            GUI.Label(new Rect(rightCol, yPos, colWidth, lineHeight), $"NetDataWriter: {currentStats.NetDataWriterPoolSize}", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(leftCol, yPos, colWidth, lineHeight), $"Material Props: {currentStats.MaterialPropertyBlockPoolSize}", labelStyle);
            GUI.Label(new Rect(rightCol, yPos, colWidth, lineHeight), $"Collections: {currentStats.CollectionPoolSize}", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(leftCol, yPos, colWidth, lineHeight), $"Coroutines: {currentStats.PooledCoroutinesActive}", labelStyle);
            GUI.Label(new Rect(rightCol, yPos, colWidth, lineHeight), $"Delegates: {currentStats.DelegatePoolSize}", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(leftCol, yPos, colWidth, lineHeight), $"JSON Ops: {currentStats.JsonOperationPoolSize}", labelStyle);
            GUI.Label(new Rect(rightCol, yPos, colWidth, lineHeight), $"FX Collections: {currentStats.FxCollectionPoolSize}", labelStyle);
            yPos += lineHeight + 10;

            // ═══════════════════════════════════════════════════════════════
            // SECTION 3: Network & String Caching
            // ═══════════════════════════════════════════════════════════════
            GUI.Label(new Rect(leftCol, yPos, colWidth * 2, lineHeight), "🌐 Network & Caching", headerStyle);
            yPos += lineHeight + 3;

            Color netColor = currentStats.NetworkStringsCached > 0 ? goodColor : warningColor;
            DrawColoredStat(leftCol, yPos, $"Network Strings: {currentStats.NetworkStringsCached}", netColor);
            yPos += lineHeight + 10;

            // ═══════════════════════════════════════════════════════════════
            // SECTION 4: System Objects
            // ═══════════════════════════════════════════════════════════════
            GUI.Label(new Rect(leftCol, yPos, colWidth * 2, lineHeight), "📦 System Objects", headerStyle);
            yPos += lineHeight + 3;

            GUI.Label(new Rect(leftCol, yPos, colWidth, lineHeight), $"UI Pooled: {currentStats.UIPoolSize}", labelStyle);
            GUI.Label(new Rect(rightCol, yPos, colWidth, lineHeight), $"Distance Entities: {currentStats.DistanceBasedEntities}", labelStyle);
            yPos += lineHeight + 10;

            // ═══════════════════════════════════════════════════════════════
            // SECTION 5: Pool System Summary
            // ═══════════════════════════════════════════════════════════════
            GUI.Label(new Rect(leftCol, yPos, colWidth * 2, lineHeight), "📈 Pooling Summary", headerStyle);
            yPos += lineHeight + 3;

            int totalPooled = GetTotalPooledObjects();
            int activeSystems = GetActivePoolSystemsCount();
            Color poolColor = totalPooled > 50 ? goodColor : (totalPooled > 20 ? warningColor : Color.white);
            DrawColoredStat(leftCol, yPos, $"Total Pooled Objects: {totalPooled}", poolColor);
            GUI.Label(new Rect(rightCol, yPos, colWidth, lineHeight), $"Active Systems: {activeSystems}/9", labelStyle);
            yPos += lineHeight + 15;

            // ═══════════════════════════════════════════════════════════════
            // BUTTONS
            // ═══════════════════════════════════════════════════════════════
            float buttonWidth = 90f;
            float buttonX = leftCol;

            if (GUI.Button(new Rect(buttonX, yPos, buttonWidth, 25), "📊 Simple"))
            {
                showSimpleView = true;
            }
            buttonX += buttonWidth + 5;

            if (GUI.Button(new Rect(buttonX, yPos, buttonWidth, 25), "🎯 Bench"))
            {
                RunBenchmark();
            }
            buttonX += buttonWidth + 5;

            if (GUI.Button(new Rect(buttonX, yPos, buttonWidth, 25), "🔄 Reset"))
            {
                ResetStats();
            }
            buttonX += buttonWidth + 5;

            if (GUI.Button(new Rect(buttonX, yPos, buttonWidth, 25), "🧪 Test"))
            {
                TestAllSystems();
            }
            buttonX += buttonWidth + 5;

            if (GUI.Button(new Rect(buttonX, yPos, buttonWidth, 25), "🙈 Hide"))
            {
                showGUI = false;
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void DrawColoredStat(float x, float y, string text, Color color)
        {
            GUI.color = color;
            GUI.Label(new Rect(x, y, 380, 18), text, labelStyle);
            GUI.color = Color.white;
        }

        #region Color Coding

        private Color GetFPSColor(float fps)
        {
            if (fps >= 60f) return goodColor;
            if (fps >= 30f) return warningColor;
            return criticalColor;
        }

        private Color GetFrameTimeColor(float frameTime)
        {
            if (frameTime <= 0.008f) return goodColor;
            if (frameTime <= 0.016f) return warningColor;
            return criticalColor;
        }

        private Color GetMemoryColor(float memoryMB)
        {
            if (memoryMB < 200f) return goodColor;
            if (memoryMB < 500f) return warningColor;
            return criticalColor;
        }

        #endregion

        #region Display Helpers

        private string GetNetworkStringsDisplay()
        {
            if (currentStats.NetworkStringsCached > 0)
                return $"{currentStats.NetworkStringsCached} (active)";
            return "0 (not active)";
        }

        private string GetUIPoolDisplay()
        {
            if (currentStats.UIPoolSize == -1)
                return "TMP resources missing";
            if (currentStats.UIPoolSize > 0)
                return $"{currentStats.UIPoolSize} objects";
            return "0 (no pools)";
        }

        private string GetDistanceEntitiesDisplay()
        {
            if (currentStats.DistanceBasedEntities > 0)
                return $"{currentStats.DistanceBasedEntities} active";
            return "0 (none found)";
        }

        private string GetCoroutinesDisplay()
        {
            if (currentStats.PooledCoroutinesActive > 0)
                return $"{currentStats.PooledCoroutinesActive} active";
            return "0 (not used)";
        }

        private int GetTotalPooledObjects()
        {
            // Sum all pooled objects across implemented systems
            int total = 0;
            
            total += currentStats.StringBuilderPoolSize;
            total += currentStats.MaterialPropertyBlockPoolSize;
            total += currentStats.NetDataWriterPoolSize;
            total += currentStats.NetworkStringsCached;
            total += currentStats.DelegatePoolSize;
            total += currentStats.JsonOperationPoolSize;
            total += currentStats.FxCollectionPoolSize;
            
            // UI Pool (skip if TMP not available, indicated by -1)
            if (currentStats.UIPoolSize >= 0)
                total += currentStats.UIPoolSize;
            
            // Note: Coroutines are "active" not "pooled waiting", so we don't add them here
            // Collection pool is generic and can't be easily tracked across all types
            
            return total;
        }

        private int GetActivePoolSystemsCount()
        {
            // Count how many implemented pool systems are currently active (10 total)
            int active = 0;
            
            if (currentStats.StringBuilderPoolSize > 0) active++;
            if (currentStats.MaterialPropertyBlockPoolSize > 0) active++;
            if (currentStats.NetDataWriterPoolSize > 0) active++;
            if (currentStats.NetworkStringsCached > 0) active++;
            if (currentStats.UIPoolSize > 0) active++;
            if (currentStats.PooledCoroutinesActive > 0) active++;
            if (currentStats.DelegatePoolSize > 0) active++;
            if (currentStats.JsonOperationPoolSize > 0) active++;
            if (currentStats.FxCollectionPoolSize > 0) active++;
            // CollectionPoolSize is always 0 (generic, can't track easily)
            
            return active;
        }

        #endregion

        #region Button Actions

        private void RunBenchmark()
        {
            var monitor = PerformanceMonitor.Instance;
            if (monitor != null)
            {
                // Call the profiler benchmark through the monitor
                monitor.TestAllSystems(); // This calls profiler.RunMiniBenchmark()
                Debug.Log("[PerformanceRenderer] Benchmark started via F11 or button");
            }
        }

        private void ResetStats()
        {
            Debug.Log("[PerformanceRenderer] Stats reset requested");
            // Could add peak stat reset here
        }

        private void TestAllSystems()
        {
            var monitor = PerformanceMonitor.Instance;
            if (monitor != null)
            {
                monitor.TestAllSystems();
                Debug.Log("[PerformanceRenderer] Testing all optimization systems");
            }
        }

        private void DebugSystemStatus()
        {
            var monitor = PerformanceMonitor.Instance;
            if (monitor != null)
            {
                monitor.DebugSystemStatus();
                Debug.Log("[PerformanceRenderer] Debug system status logged to console");
            }
        }

        #endregion
    }
}
