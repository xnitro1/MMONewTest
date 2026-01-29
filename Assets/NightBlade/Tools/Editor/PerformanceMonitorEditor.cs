using UnityEngine;
using UnityEditor;
using NightBlade.Core.Utils.Performance;

namespace NightBlade
{
    [CustomEditor(typeof(PerformanceMonitor))]
    public class PerformanceMonitorEditor : Editor
    {
        private PerformanceMonitor performanceMonitor;

        // Foldout states
        private bool monitoringConfigFoldout = true;
        private bool realTimeStatsFoldout = true;
        private bool profilerMarkersFoldout = false;
        private bool systemIntegrationFoldout = false;

        private void OnEnable()
        {
            performanceMonitor = (PerformanceMonitor)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("📊 NightBlade Performance Monitor", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenPerformanceMonitorDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Status and controls
            DrawStatusAndControls();

            // Runtime statistics (if playing)
            DrawRealTimeStats();

            // Configuration sections
            DrawMonitoringConfiguration();
            DrawProfilerMarkers();
            DrawSystemIntegration();

            // Diagnostic tools
            DrawDiagnosticTools();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStatusAndControls()
        {
            EditorGUILayout.LabelField("🎮 Monitor Status", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Current status
            bool isActive = performanceMonitor != null && performanceMonitor.gameObject.activeInHierarchy;
            EditorGUILayout.LabelField($"Status: {(isActive ? "Active" : "Inactive")}",
                isActive ? EditorStyles.boldLabel : EditorStyles.miniLabel);

            // Quick toggle buttons
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(performanceMonitor.ShowGUIStats ? "🙈 Hide GUI" : "👁️ Show GUI", GUILayout.Height(25)))
            {
                performanceMonitor.ShowGUIStats = !performanceMonitor.ShowGUIStats;
                EditorUtility.SetDirty(performanceMonitor);
            }

            if (GUILayout.Button("🔄 Force Update", GUILayout.Height(25)))
            {
                if (Application.isPlaying)
                {
                    // Force immediate stats update
                    performanceMonitor.Invoke("UpdateStats", 0f);
                }
                else
                {
                    EditorUtility.DisplayDialog("Not Playing", "Performance stats are only available during play mode.", "OK");
                }
            }

            EditorGUILayout.EndHorizontal();

            // GUI status
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"GUI Overlay: {(performanceMonitor.ShowGUIStats ? "Visible" : "Hidden")}", EditorStyles.miniLabel);

            if (performanceMonitor.ShowGUIStats)
            {
                EditorGUILayout.HelpBox("⚠️ GUI overlay is visible in game view. Disable for production builds.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawRealTimeStats()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("▶️ Enter play mode to see real-time performance statistics.", MessageType.Info);
                return;
            }

            realTimeStatsFoldout = EditorGUILayout.Foldout(realTimeStatsFoldout, "📈 Real-Time Statistics");
            if (realTimeStatsFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                // Get current stats
                var stats = performanceMonitor.GetStats();

                // Performance indicators with color coding
                DrawStatWithIndicator("Frame Time", $"{stats.FrameTime * 1000:F2}ms",
                    stats.FrameTime > 0.016f ? Color.red : stats.FrameTime > 0.008f ? Color.yellow : Color.green);

                DrawStatWithIndicator("FPS", $"{stats.FPS:F1}",
                    stats.FPS < 30f ? Color.red : stats.FPS < 60f ? Color.yellow : Color.green);

                DrawStatWithIndicator("GC Allocations", $"{stats.GCAllocationsThisFrame} bytes",
                    stats.GCAllocationsThisFrame > 1024 ? Color.yellow : Color.green);

                // Memory usage
                float memoryMB = stats.TotalMemoryUsed / 1024f / 1024f;
                DrawStatWithIndicator("Total Memory", $"{memoryMB:F1}MB",
                    memoryMB > 500f ? Color.red : memoryMB > 200f ? Color.yellow : Color.green);

                // System metrics
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("System Metrics:", EditorStyles.boldLabel);

                EditorGUILayout.LabelField($"Network Strings Cached: {stats.NetworkStringsCached}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"UI Objects Pooled: {stats.UIPoolSize}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Distance-Based Entities: {stats.DistanceBasedEntities}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Pooled Coroutines: {stats.PooledCoroutinesActive}", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawMonitoringConfiguration()
        {
            monitoringConfigFoldout = EditorGUILayout.Foldout(monitoringConfigFoldout, "⚙️ Monitoring Configuration");
            if (monitoringConfigFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure how the performance monitor collects and displays data.", MessageType.Info);

                EditorGUILayout.LabelField("Collection Settings", EditorStyles.boldLabel);
                SafePropertyField("enableDetailedLogging", "Enable detailed performance logging to console");

                bool loggingEnabled = serializedObject.FindProperty("enableDetailedLogging").boolValue;
                if (loggingEnabled)
                {
                    EditorGUILayout.HelpBox("⚠️ Detailed logging enabled - may impact performance and fill logs.", MessageType.Warning);
                }

                SafePropertyField("statsUpdateInterval", "How often to update performance statistics (seconds)");

                float updateInterval = serializedObject.FindProperty("statsUpdateInterval").floatValue;
                if (updateInterval < 1f)
                {
                    EditorGUILayout.HelpBox("⚠️ Very frequent updates may impact performance.", MessageType.Warning);
                }
                else if (updateInterval > 10f)
                {
                    EditorGUILayout.HelpBox("ℹ️ Infrequent updates may miss performance spikes.", MessageType.Info);
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Display Settings", EditorStyles.boldLabel);
                SafePropertyField("showGUIStats", "Show performance overlay in game view");

                bool showGUI = serializedObject.FindProperty("showGUIStats").boolValue;
                if (showGUI)
                {
                    EditorGUILayout.HelpBox("GUI overlay is active with interactive controls.\nFPS updates in real-time, other stats update every second.\nUse F11 for mini-benchmarks, F12 to toggle visibility.\nCompatible with both legacy and new Input System.", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawProfilerMarkers()
        {
            profilerMarkersFoldout = EditorGUILayout.Foldout(profilerMarkersFoldout, "🔍 Profiler Markers");
            if (profilerMarkersFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Unity Profiler markers for detailed performance analysis.", MessageType.Info);

                EditorGUILayout.LabelField("Available Markers:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("• Network.Update - Network processing operations", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• UI.Render - User interface rendering", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Distance.Update - Distance-based entity updates", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Coroutine.Pool - Pooled coroutine operations", EditorStyles.miniLabel);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Usage Example:", EditorStyles.boldLabel);
                EditorGUILayout.TextArea(@"// Profile network operations
PerformanceMonitor.ProfileNetworkUpdate(() =>
{
    // Your network code here
    SendPlayerUpdates();
    ProcessIncomingMessages();
});", GUI.skin.textArea);

                if (GUILayout.Button("📖 Open Unity Profiler", GUILayout.Height(25)))
                {
                    UnityEditorInternal.ProfilerDriver.enabled = true;
                    EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSystemIntegration()
        {
            systemIntegrationFoldout = EditorGUILayout.Foldout(systemIntegrationFoldout, "🔗 System Integration");
            if (systemIntegrationFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Integration status with various NightBlade optimization systems.", MessageType.Info);

                EditorGUILayout.LabelField("Integration Status:", EditorStyles.boldLabel);

                // Check for various systems
                CheckSystemIntegration("Network String Cache", "GetNetworkStringCacheSize");
                CheckSystemIntegration("UI Object Pool", "GetUIPoolSize");
                CheckSystemIntegration("Distance-Based Updater", "GetDistanceBasedEntityCount");
                CheckSystemIntegration("Coroutine Pool", "GetPooledCoroutineCount");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Integration Notes:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Some integrations marked as TODO and return placeholder values", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Distance-Based Updater is fully integrated", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Network and UI systems need component references", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawDiagnosticTools()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🔧 Diagnostic Tools", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Performance baseline
            EditorGUILayout.LabelField("Performance Baseline:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Target FPS: 60 FPS (16.67ms frame time)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Memory Budget: < 200MB for client builds", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• GC Budget: < 1KB per frame average", EditorStyles.miniLabel);

            // Quick actions
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("📊 Generate Report", GUILayout.Height(25)))
            {
                GeneratePerformanceReport();
            }

            if (GUILayout.Button("🎯 Run Benchmark", GUILayout.Height(25)))
            {
                RunPerformanceBenchmark();
            }

            EditorGUILayout.EndHorizontal();

            // Preset configurations
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Quick Presets:", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Development", GUILayout.Height(20)))
            {
                SetDevelopmentPreset();
            }

            if (GUILayout.Button("Testing", GUILayout.Height(20)))
            {
                SetTestingPreset();
            }

            if (GUILayout.Button("Production", GUILayout.Height(20)))
            {
                SetProductionPreset();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawStatWithIndicator(string label, string value, Color indicatorColor)
        {
            EditorGUILayout.BeginHorizontal();

            // Color indicator
            GUI.color = indicatorColor;
            GUILayout.Label("●", GUILayout.Width(15));
            GUI.color = Color.white;

            EditorGUILayout.LabelField($"{label}: {value}");

            EditorGUILayout.EndHorizontal();
        }

        private void CheckSystemIntegration(string systemName, string methodName)
        {
            // Simple check - in a real implementation, this would check if the system is actually integrated
            bool isIntegrated = methodName == "GetDistanceBasedEntityCount"; // Only this one is fully implemented

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{systemName}:", EditorStyles.miniLabel, GUILayout.Width(150));
            EditorGUILayout.LabelField(isIntegrated ? "✅ Integrated" : "⏳ TODO", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void GeneratePerformanceReport()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Not Playing", "Performance reports are only available during play mode.", "OK");
                return;
            }

            var stats = performanceMonitor.GetStats();
            string report = $"=== Performance Report ===\n" +
                           $"Frame Time: {stats.FrameTime * 1000:F2}ms\n" +
                           $"FPS: {stats.FPS:F1}\n" +
                           $"Memory: {stats.TotalMemoryUsed / 1024 / 1024:F1}MB\n" +
                           $"GC: {stats.GCAllocationsThisFrame} bytes/frame\n" +
                           $"Network Cache: {stats.NetworkStringsCached}\n" +
                           $"UI Pool: {stats.UIPoolSize}\n" +
                           $"Distance Entities: {stats.DistanceBasedEntities}\n" +
                           $"Pooled Coroutines: {stats.PooledCoroutinesActive}";

            Debug.Log(report);
            EditorUtility.DisplayDialog("Performance Report", "Report generated and logged to console.", "OK");
        }

        private void RunPerformanceBenchmark()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Not Playing", "Performance benchmarks require play mode.", "OK");
                return;
            }

            // Simple synchronous benchmark - measure current performance state
            var stats = performanceMonitor.GetStats();

            // Simulate some work to measure performance impact
            float benchmarkStartTime = Time.realtimeSinceStartup;
            const int iterations = 1000;

            // Perform some computational work
            float result = 0f;
            for (int i = 0; i < iterations; i++)
            {
                result += Mathf.Sin(i * 0.01f) * Mathf.Cos(i * 0.01f);
                result += Mathf.Sqrt(Mathf.Abs(Mathf.Tan(i * 0.001f)));
            }

            float benchmarkEndTime = Time.realtimeSinceStartup;
            float benchmarkDuration = benchmarkEndTime - benchmarkStartTime;

            string benchmarkResult = $"=== Performance Benchmark ===\n" +
                                   $"Current FPS: {stats.FPS:F1}\n" +
                                   $"Current Frame Time: {stats.FrameTime * 1000:F2}ms\n" +
                                   $"Memory Usage: {stats.TotalMemoryUsed / 1024 / 1024:F1}MB\n" +
                                   $"GC This Frame: {stats.GCAllocationsThisFrame} bytes\n" +
                                   $"Benchmark Duration: {benchmarkDuration * 1000:F2}ms ({iterations} iterations)\n" +
                                   $"Computational Result: {result:F2} (verification)";

            Debug.Log(benchmarkResult);
            EditorUtility.DisplayDialog("Benchmark Complete",
                $"Benchmark completed in {benchmarkDuration * 1000:F2}ms\nResults logged to console.", "OK");
        }

        private void SetDevelopmentPreset()
        {
            serializedObject.FindProperty("enableDetailedLogging").boolValue = true;
            serializedObject.FindProperty("statsUpdateInterval").floatValue = 1f;
            serializedObject.FindProperty("showGUIStats").boolValue = true;
            serializedObject.ApplyModifiedProperties();
            Debug.Log("Performance Monitor set to Development preset");
        }

        private void SetTestingPreset()
        {
            serializedObject.FindProperty("enableDetailedLogging").boolValue = false;
            serializedObject.FindProperty("statsUpdateInterval").floatValue = 2f;
            serializedObject.FindProperty("showGUIStats").boolValue = true;
            serializedObject.ApplyModifiedProperties();
            Debug.Log("Performance Monitor set to Testing preset");
        }

        private void SetProductionPreset()
        {
            serializedObject.FindProperty("enableDetailedLogging").boolValue = false;
            serializedObject.FindProperty("statsUpdateInterval").floatValue = 5f;
            serializedObject.FindProperty("showGUIStats").boolValue = false;
            serializedObject.ApplyModifiedProperties();
            Debug.Log("Performance Monitor set to Production preset (GUI disabled for performance)");
        }

        private void SafePropertyField(string propertyName, string tooltip = null)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                if (!string.IsNullOrEmpty(tooltip))
                {
                    EditorGUILayout.PropertyField(property, new GUIContent(property.displayName, tooltip));
                }
                else
                {
                    EditorGUILayout.PropertyField(property);
                }
            }
        }

        private void OpenPerformanceMonitorDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "PerformanceMonitor.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened Performance Monitor documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/PerformanceMonitor.md");
                Debug.LogWarning($"📖 Local documentation not found, opening web version");
            }
        }
    }
}