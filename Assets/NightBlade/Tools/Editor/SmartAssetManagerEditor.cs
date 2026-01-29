using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NightBlade.Core.Utils;

namespace NightBlade
{
    [CustomEditor(typeof(SmartAssetManager))]
    public class SmartAssetManagerEditor : Editor
    {
        private SmartAssetManager assetManager;

        // Foldout states
        private bool memoryManagementFoldout = true;
        private bool unloadingStrategyFoldout = true;
        private bool prioritySystemFoldout = true;
        private bool monitoringFoldout = false;

        // Cached properties
        private SerializedProperty targetMemoryUsageMB;
        private SerializedProperty criticalMemoryThresholdMB;
        private SerializedProperty memoryCheckInterval;
        private SerializedProperty aggressiveUnloadInterval;
        private SerializedProperty maxUnloadBatchSize;
        private SerializedProperty minTimeSinceLastAccess;
        private SerializedProperty unloadBatchDelay;
        private SerializedProperty sceneAssetPriority;
        private SerializedProperty uiAssetPriority;
        private SerializedProperty characterAssetPriority;
        private SerializedProperty effectAssetPriority;
        private SerializedProperty audioAssetPriority;
        private SerializedProperty enableDetailedLogging;
        private SerializedProperty showMemoryStats;
        private SerializedProperty statsUpdateInterval;

        private void OnEnable()
        {
            assetManager = (SmartAssetManager)target;

            // Cache all properties
            targetMemoryUsageMB = serializedObject.FindProperty("targetMemoryUsageMB");
            criticalMemoryThresholdMB = serializedObject.FindProperty("criticalMemoryThresholdMB");
            memoryCheckInterval = serializedObject.FindProperty("memoryCheckInterval");
            aggressiveUnloadInterval = serializedObject.FindProperty("aggressiveUnloadInterval");
            maxUnloadBatchSize = serializedObject.FindProperty("maxUnloadBatchSize");
            minTimeSinceLastAccess = serializedObject.FindProperty("minTimeSinceLastAccess");
            unloadBatchDelay = serializedObject.FindProperty("unloadBatchDelay");
            sceneAssetPriority = serializedObject.FindProperty("sceneAssetPriority");
            uiAssetPriority = serializedObject.FindProperty("uiAssetPriority");
            characterAssetPriority = serializedObject.FindProperty("characterAssetPriority");
            effectAssetPriority = serializedObject.FindProperty("effectAssetPriority");
            audioAssetPriority = serializedObject.FindProperty("audioAssetPriority");
            enableDetailedLogging = serializedObject.FindProperty("enableDetailedLogging");
            showMemoryStats = serializedObject.FindProperty("showMemoryStats");
            statsUpdateInterval = serializedObject.FindProperty("statsUpdateInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("🧠 NightBlade Smart Asset Manager", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenSmartAssetManagerDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Version info
            EditorGUILayout.HelpBox("Intelligent asset memory management for MMO performance optimization", MessageType.Info);
            EditorGUILayout.Space(5);

            // Memory usage dashboard
            DrawMemoryDashboard();

            // Configuration sections
            DrawMemoryManagementSection();
            DrawUnloadingStrategySection();
            DrawPrioritySystemSection();
            DrawMonitoringSection();

            // Performance recommendations
            DrawOptimizationRecommendations();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMemoryDashboard()
        {
            EditorGUILayout.LabelField("📊 Memory Dashboard", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (Application.isPlaying && assetManager != null && SmartAssetManager.Instance != null)
            {
                var stats = SmartAssetManager.Instance.GetMemoryStats();

                // Current memory usage with visual indicator
                float currentMB = stats.CurrentMemoryUsage / (1024f * 1024f);
                float targetMB = stats.TargetMemoryUsage / (1024f * 1024f);
                float criticalMB = criticalMemoryThresholdMB.floatValue;

                EditorGUILayout.LabelField($"Current Usage: {currentMB:F1} MB", EditorStyles.boldLabel);

                // Memory usage bar
                Rect barRect = EditorGUILayout.GetControlRect(false, 20);
                float fillRatio = Mathf.Clamp01(currentMB / criticalMB);
                Color barColor = currentMB > targetMB ? Color.yellow : (currentMB > criticalMB ? Color.red : Color.green);

                EditorGUI.DrawRect(barRect, Color.gray);
                EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * fillRatio, barRect.height), barColor);

                // Memory stats
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Target: {targetMB:F0} MB", GetMemoryStatusColor(currentMB, targetMB, criticalMB));
                EditorGUILayout.LabelField($"Critical: {criticalMB:F0} MB", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Assets: {stats.TrackedAssetCount}", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();

                // Aggressive mode indicator
                if (stats.IsAggressiveMode)
                {
                    EditorGUILayout.HelpBox("⚡ Aggressive unloading mode active - High memory pressure detected!", MessageType.Warning);
                }

                // Category breakdown
                if (stats.CategoryBreakdown != null && stats.CategoryBreakdown.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Asset Categories:", EditorStyles.miniLabel);

                    foreach (var category in stats.CategoryBreakdown)
                    {
                        float categoryMB = category.Value / (1024f * 1024f);
                        EditorGUILayout.LabelField($"  {category.Key}: {categoryMB:F1} MB", EditorStyles.miniLabel);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Memory dashboard available during play mode", MessageType.Info);
                EditorGUILayout.LabelField("Target Memory: " + targetMemoryUsageMB.floatValue + " MB", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Critical Threshold: " + criticalMemoryThresholdMB.floatValue + " MB", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawMemoryManagementSection()
        {
            memoryManagementFoldout = EditorGUILayout.Foldout(memoryManagementFoldout, "💾 Memory Management");
            if (memoryManagementFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure memory targets and monitoring intervals for optimal performance.", MessageType.Info);

                EditorGUILayout.LabelField("Memory Targets", EditorStyles.boldLabel);
                SafePropertyField(targetMemoryUsageMB, "Normal memory usage target (MB)");
                SafePropertyField(criticalMemoryThresholdMB, "Critical threshold triggering aggressive unloading (MB)");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Monitoring Intervals", EditorStyles.boldLabel);
                SafePropertyField(memoryCheckInterval, "How often to check memory usage (seconds)");
                SafePropertyField(aggressiveUnloadInterval, "Faster checks during high memory pressure (seconds)");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawUnloadingStrategySection()
        {
            unloadingStrategyFoldout = EditorGUILayout.Foldout(unloadingStrategyFoldout, "🔄 Unloading Strategy");
            if (unloadingStrategyFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Control how and when assets are unloaded to balance performance and memory usage.", MessageType.Info);

                EditorGUILayout.LabelField("Batch Processing", EditorStyles.boldLabel);
                SafePropertyField(maxUnloadBatchSize, "Maximum assets to unload per batch");
                SafePropertyField(unloadBatchDelay, "Delay between unload operations (seconds)");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Unload Timing", EditorStyles.boldLabel);
                SafePropertyField(minTimeSinceLastAccess, "Minimum time since last access before unloading (seconds)");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPrioritySystemSection()
        {
            prioritySystemFoldout = EditorGUILayout.Foldout(prioritySystemFoldout, "🎯 Priority System");
            if (prioritySystemFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Higher priority assets are less likely to be unloaded. Adjust based on gameplay needs.", MessageType.Info);

                EditorGUILayout.LabelField("Asset Priorities (Higher = Less Likely to Unload)", EditorStyles.boldLabel);

                // Priority sliders with visual indicators
                DrawPrioritySlider(sceneAssetPriority, "Scene Assets", "Critical for level navigation");
                DrawPrioritySlider(uiAssetPriority, "UI Assets", "Essential for user interface");
                DrawPrioritySlider(characterAssetPriority, "Character Assets", "Player and NPC models");
                DrawPrioritySlider(audioAssetPriority, "Audio Assets", "Sound effects and music");
                DrawPrioritySlider(effectAssetPriority, "Effect Assets", "Particles and visual effects");

                // Priority validation
                float highestPriority = Mathf.Max(
                    sceneAssetPriority.floatValue,
                    uiAssetPriority.floatValue,
                    characterAssetPriority.floatValue,
                    audioAssetPriority.floatValue,
                    effectAssetPriority.floatValue
                );

                if (highestPriority > 15f)
                {
                    EditorGUILayout.HelpBox("⚠️ Very high priority values (>15) may prevent necessary unloading", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPrioritySlider(SerializedProperty property, string label, string tooltip)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"  {label}", GUILayout.Width(120));
            float oldValue = property.floatValue;
            float newValue = EditorGUILayout.Slider(property.floatValue, 0f, 20f);
            if (newValue != oldValue)
            {
                property.floatValue = newValue;
            }
            EditorGUILayout.LabelField($"{property.floatValue:F1}", GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(tooltip))
            {
                EditorGUILayout.LabelField($"    {tooltip}", EditorStyles.miniLabel);
            }
        }

        private void DrawMonitoringSection()
        {
            monitoringFoldout = EditorGUILayout.Foldout(monitoringFoldout, "📈 Debug & Monitoring");
            if (monitoringFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Enable logging and real-time statistics for debugging asset management.", MessageType.Info);

                SafePropertyField(enableDetailedLogging, "Log detailed asset loading/unloading operations");
                SafePropertyField(showMemoryStats, "Display real-time memory statistics overlay");

                if (showMemoryStats.boolValue)
                {
                    SafePropertyField(statsUpdateInterval, "How often to update statistics display (seconds)");
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawOptimizationRecommendations()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("💡 Optimization Recommendations", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Memory target recommendations
            float targetMemory = targetMemoryUsageMB.floatValue;
            float criticalMemory = criticalMemoryThresholdMB.floatValue;

            if (targetMemory < 256)
            {
                EditorGUILayout.HelpBox("⚠️ Target memory is very low (<256MB). Consider increasing for better performance.", MessageType.Warning);
            }
            else if (targetMemory > 2048)
            {
                EditorGUILayout.HelpBox("ℹ️ Target memory is very high (>2GB). Ensure your target platform can handle this.", MessageType.Info);
            }

            if (criticalMemory < targetMemory * 1.5f)
            {
                EditorGUILayout.HelpBox("⚠️ Critical threshold should be at least 50% higher than target memory.", MessageType.Warning);
            }

            // Priority recommendations
            if (sceneAssetPriority.floatValue < uiAssetPriority.floatValue)
            {
                EditorGUILayout.HelpBox("ℹ️ Consider giving scene assets higher priority than UI for better level stability.", MessageType.Info);
            }

            if (maxUnloadBatchSize.intValue > 20)
            {
                EditorGUILayout.HelpBox("⚠️ Large batch sizes may cause frame drops. Consider smaller batches with shorter delays.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void SafePropertyField(SerializedProperty property, string tooltip = null)
        {
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

        private GUIStyle GetMemoryStatusColor(float current, float target, float critical)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            if (current > critical)
            {
                style.normal.textColor = Color.red;
            }
            else if (current > target)
            {
                style.normal.textColor = Color.yellow;
            }
            else
            {
                style.normal.textColor = Color.green;
            }
            return style;
        }

        private void OpenSmartAssetManagerDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "SmartAssetManager.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened SmartAssetManager documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/SmartAssetManager.md");
                Debug.LogWarning($"📖 Local documentation not found, opening web version");
            }
        }
    }
}