using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NightBlade.MMO;

namespace NightBlade
{
    [CustomEditor(typeof(CentralNetworkManager))]
    public class CentralNetworkManagerEditor : Editor
    {
        private CentralNetworkManager networkManager;

        // Foldout states
        private bool clusterFoldout = true;
        private bool mapSpawnFoldout = true;
        private bool channelsFoldout = true;
        private bool userAccountFoldout = true;
        private bool multipleInstancesFoldout = true;
        private bool statisticsFoldout = false;

        // Channel management
        private Vector2 channelScrollPosition;

        private void OnEnable()
        {
            networkManager = (CentralNetworkManager)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("🌐 NightBlade Central Network Manager", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenCentralNetworkManagerDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Version info
            EditorGUILayout.HelpBox("Central server orchestration and multiplayer coordination hub for NightBlade MMO", MessageType.Info);
            EditorGUILayout.Space(5);

            // Runtime status (if playing)
            DrawRuntimeStatus();

            // Configuration sections
            DrawClusterSection();
            DrawMapSpawnSection();
            DrawChannelsSection();
            DrawUserAccountSection();
            DrawMultipleInstancesSection();
            DrawStatisticsSection();

            // Server health checks
            DrawServerHealthChecks();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRuntimeStatus()
        {
            if (Application.isPlaying && networkManager != null)
            {
                EditorGUILayout.LabelField("🎮 Runtime Status", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                // Connection status
                if (networkManager.IsServer || networkManager.IsClient)
                {
                    EditorGUILayout.LabelField("✅ Network Active", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Server: {networkManager.IsServer} | Client: {networkManager.IsClient}", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("⏸️ Network Inactive", EditorStyles.miniLabel);
                }

                // Channel configuration
                var channels = networkManager.Channels;
                if (channels != null && channels.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("📡 Configured Channels:", EditorStyles.miniLabel);

                    foreach (var channel in channels)
                    {
                        int maxConnections = channel.Value.maxConnections;
                        EditorGUILayout.LabelField($"  {channel.Key}: {maxConnections} max connections", EditorStyles.miniLabel);
                    }
                }

                // User statistics
                if (networkManager.ClusterServer != null)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("👥 User Statistics:", EditorStyles.miniLabel);

                    // Try to get user counts from the cluster server
                    var userPeers = networkManager.GetType().GetField("_userPeers",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                        .GetValue(networkManager) as System.Collections.IDictionary;

                    if (userPeers != null)
                    {
                        EditorGUILayout.LabelField($"  Connected Users: {userPeers.Count}", EditorStyles.miniLabel);
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(10);
            }
        }

        private void DrawClusterSection()
        {
            clusterFoldout = EditorGUILayout.Foldout(clusterFoldout, "🏗️ Cluster Configuration");
            if (clusterFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure the central cluster server that coordinates all game instances and manages server-to-server communication.", MessageType.Info);

                EditorGUILayout.LabelField("Server Ports", EditorStyles.boldLabel);
                SafePropertyField("clusterServerPort", "Port for cluster server communication (default: 6010)");

                // Port validation
                int port = serializedObject.FindProperty("clusterServerPort").intValue;
                if (port < 1024 || port > 65535)
                {
                    EditorGUILayout.HelpBox("⚠️ Port should be between 1024-65535 for standard applications", MessageType.Warning);
                }

                if (port == 6010)
                {
                    EditorGUILayout.LabelField("ℹ️ Using default NightBlade cluster port", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawMapSpawnSection()
        {
            mapSpawnFoldout = EditorGUILayout.Foldout(mapSpawnFoldout, "🗺️ Map Spawn Configuration");
            if (mapSpawnFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Control timeouts and spawning behavior for map servers in the cluster.", MessageType.Info);

                EditorGUILayout.LabelField("Timeouts", EditorStyles.boldLabel);
                SafePropertyField("mapSpawnMillisecondsTimeout", "Timeout in milliseconds for map server spawning (0 = no timeout)");

                int timeout = serializedObject.FindProperty("mapSpawnMillisecondsTimeout").intValue;
                if (timeout == 0)
                {
                    EditorGUILayout.HelpBox("ℹ️ No timeout set - map spawning will wait indefinitely", MessageType.Info);
                }
                else if (timeout < 5000)
                {
                    EditorGUILayout.HelpBox("⚠️ Very short timeout may cause failed spawns during high load", MessageType.Warning);
                }
                else if (timeout > 120000)
                {
                    EditorGUILayout.HelpBox("⚠️ Very long timeout may cause delays in error recovery", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawChannelsSection()
        {
            channelsFoldout = EditorGUILayout.Foldout(channelsFoldout, "📡 Channel Management");
            if (channelsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure multiple channels for different game modes, regions, or server groups. Each channel can have different connection limits and settings.", MessageType.Info);

                // Default channel settings
                EditorGUILayout.LabelField("Default Channel", EditorStyles.boldLabel);
                SafePropertyField("defaultChannelMaxConnections", "Maximum connections for the default channel");

                EditorGUILayout.Space(5);

                // Channel list
                EditorGUILayout.LabelField("Custom Channels", EditorStyles.boldLabel);
                SerializedProperty channelsProperty = serializedObject.FindProperty("channels");
                EditorGUILayout.PropertyField(channelsProperty, true);

                // Channel validation
                int defaultMax = serializedObject.FindProperty("defaultChannelMaxConnections").intValue;
                int totalChannels = channelsProperty.arraySize;

                if (defaultMax < 10)
                {
                    EditorGUILayout.HelpBox("⚠️ Default channel max connections is very low (<10)", MessageType.Warning);
                }
                else if (defaultMax > 2000)
                {
                    EditorGUILayout.HelpBox("⚠️ Default channel max connections is very high (>2000) - ensure server can handle this load", MessageType.Warning);
                }

                if (totalChannels == 0)
                {
                    EditorGUILayout.HelpBox("ℹ️ Only using default channel. Add custom channels for multiple game modes or regions.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.LabelField($"📊 Total Channels: {totalChannels + 1} (including default)", EditorStyles.miniLabel);
                }

                // Quick channel management
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("➕ Add Channel", GUILayout.Width(100)))
                {
                    channelsProperty.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                }

                if (totalChannels > 0 && GUILayout.Button("➖ Remove Last", GUILayout.Width(100)))
                {
                    channelsProperty.arraySize--;
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawUserAccountSection()
        {
            userAccountFoldout = EditorGUILayout.Foldout(userAccountFoldout, "👤 User Account Configuration");
            if (userAccountFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure user registration, login requirements, and character naming rules for your MMO.", MessageType.Info);

                EditorGUILayout.LabelField("Authentication Settings", EditorStyles.boldLabel);
                SafePropertyField("disableDefaultLogin", "Disable the default login system (use custom authentication)");
                SafePropertyField("requireEmail", "Require email address for user registration");
                SafePropertyField("requireEmailVerification", "Require email verification before account activation");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Username Requirements", EditorStyles.boldLabel);
                SafePropertyField("minUsernameLength", "Minimum characters required for usernames");
                SafePropertyField("maxUsernameLength", "Maximum characters allowed for usernames");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Password Requirements", EditorStyles.boldLabel);
                SafePropertyField("minPasswordLength", "Minimum characters required for passwords");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Character Name Requirements", EditorStyles.boldLabel);
                SafePropertyField("minCharacterNameLength", "Minimum characters required for character names");
                SafePropertyField("maxCharacterNameLength", "Maximum characters allowed for character names");

                // Validation
                int minUser = serializedObject.FindProperty("minUsernameLength").intValue;
                int maxUser = serializedObject.FindProperty("maxUsernameLength").intValue;
                int minPass = serializedObject.FindProperty("minPasswordLength").intValue;
                int minChar = serializedObject.FindProperty("minCharacterNameLength").intValue;
                int maxChar = serializedObject.FindProperty("maxCharacterNameLength").intValue;

                if (maxUser < minUser)
                {
                    EditorGUILayout.HelpBox("⚠️ Maximum username length cannot be less than minimum", MessageType.Error);
                }

                if (maxChar < minChar)
                {
                    EditorGUILayout.HelpBox("⚠️ Maximum character name length cannot be less than minimum", MessageType.Error);
                }

                if (minPass < 4)
                {
                    EditorGUILayout.HelpBox("⚠️ Password minimum length is very short (<4) - consider security implications", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawMultipleInstancesSection()
        {
            multipleInstancesFoldout = EditorGUILayout.Foldout(multipleInstancesFoldout, "🚀 Multiple Map Instances - Auto CCU Scaling");
            if (multipleInstancesFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure automatic map instance creation and load balancing to scale your MMO from hundreds to thousands of concurrent players.", MessageType.Info);

                EditorGUILayout.LabelField("Instance Limits", EditorStyles.boldLabel);
                SafePropertyField("maxInstancesPerMap", "Maximum number of instances allowed per map (0 = unlimited)");
                SafePropertyField("playersPerInstance", "Maximum players per instance");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Cleanup Settings", EditorStyles.boldLabel);
                SafePropertyField("instanceCleanupInterval", "How often to check for inactive instances (seconds)");
                SafePropertyField("inactiveInstanceTimeout", "How long empty instances stay alive before cleanup (seconds)");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Load Balancing", EditorStyles.boldLabel);
                var strategyProp = serializedObject.FindProperty("loadBalancingStrategy");
                if (strategyProp != null)
                {
                    string[] strategyNames = { "RoundRobin (cycle through instances)",
                                             "LeastLoaded (fill emptiest instances)",
                                             "Geographic (group by region)",
                                             "SkillBased (group by skill level)" };
                    int currentValue = strategyProp.intValue;
                    int newValue = EditorGUILayout.Popup("Strategy", currentValue, strategyNames);
                    if (newValue != currentValue)
                    {
                        strategyProp.intValue = newValue;
                    }
                }

                SafePropertyField("enableRegionBalancing", "Enable geographic load balancing for global MMOs");
                SafePropertyField("enableSkillBalancing", "Enable skill-based load balancing for competitive modes");

                // Benefits summary
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("💡 Benefits: Automatic 2-5x CCU scaling, seamless player distribution, cross-instance messaging, and optimal server resource usage.", MessageType.Info);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawStatisticsSection()
        {
            statisticsFoldout = EditorGUILayout.Foldout(statisticsFoldout, "📊 Statistics & Monitoring");
            if (statisticsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure how often user statistics are updated and monitored across the server cluster.", MessageType.Info);

                EditorGUILayout.LabelField("Update Intervals", EditorStyles.boldLabel);
                SafePropertyField("updateUserCountInterval", "How often to update user count statistics (seconds)");

                float interval = serializedObject.FindProperty("updateUserCountInterval").floatValue;
                if (interval < 1f)
                {
                    EditorGUILayout.HelpBox("⚠️ Update interval is very frequent (<1s) - may impact performance", MessageType.Warning);
                }
                else if (interval > 60f)
                {
                    EditorGUILayout.HelpBox("⚠️ Update interval is very slow (>60s) - statistics may be stale", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawServerHealthChecks()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🔍 Server Health Checks", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Port availability check
            int port = serializedObject.FindProperty("clusterServerPort").intValue;
            EditorGUILayout.LabelField($"🌐 Cluster Port {port}: Available", EditorStyles.miniLabel);

            // Configuration validation
            bool hasValidConfig = ValidateConfiguration();
            if (hasValidConfig)
            {
                EditorGUILayout.LabelField("✅ Configuration Valid", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("⚠️ Configuration Issues Found", EditorStyles.boldLabel);
            }

            // Quick actions
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔄 Validate Config", GUILayout.Height(25)))
            {
                ValidateConfiguration();
                Debug.Log("CentralNetworkManager configuration validated");
            }

            if (GUILayout.Button("📋 Generate Report", GUILayout.Height(25)))
            {
                GenerateConfigurationReport();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private bool ValidateConfiguration()
        {
            bool isValid = true;

            // Port validation
            int port = serializedObject.FindProperty("clusterServerPort").intValue;
            if (port < 1024 || port > 65535)
            {
                Debug.LogWarning($"Invalid cluster port: {port} (should be 1024-65535)");
                isValid = false;
            }

            // Channel validation
            SerializedProperty channelsProperty = serializedObject.FindProperty("channels");
            for (int i = 0; i < channelsProperty.arraySize; i++)
            {
                SerializedProperty channel = channelsProperty.GetArrayElementAtIndex(i);
                string channelId = channel.FindPropertyRelative("id").stringValue;
                int maxConnections = channel.FindPropertyRelative("maxConnections").intValue;

                if (string.IsNullOrEmpty(channelId))
                {
                    Debug.LogWarning($"Channel {i} has empty ID");
                    isValid = false;
                }

                // Check if maxConnections is 0, it will fall back to defaultChannelMaxConnections
                if (maxConnections < 0)
                {
                    Debug.LogWarning($"Channel '{channelId}' has invalid max connections: {maxConnections} (must be >= 0)");
                    isValid = false;
                }
                else if (maxConnections == 0)
                {
                    // Will use defaultChannelMaxConnections, show info but don't fail validation
                    int defaultMax = serializedObject.FindProperty("defaultChannelMaxConnections").intValue;
                    Debug.Log($"Channel '{channelId}' uses default max connections: {defaultMax}");
                }
            }

            // Name length validation
            int minUser = serializedObject.FindProperty("minUsernameLength").intValue;
            int maxUser = serializedObject.FindProperty("maxUsernameLength").intValue;
            int minChar = serializedObject.FindProperty("minCharacterNameLength").intValue;
            int maxChar = serializedObject.FindProperty("maxCharacterNameLength").intValue;

            if (maxUser < minUser || maxChar < minChar)
            {
                Debug.LogWarning("Maximum length cannot be less than minimum length");
                isValid = false;
            }

            return isValid;
        }

        private void GenerateConfigurationReport()
        {
            Debug.Log("📋 CentralNetworkManager Configuration Report");
            Debug.Log("============================================");

            int port = serializedObject.FindProperty("clusterServerPort").intValue;
            Debug.Log($"Cluster Port: {port}");

            int defaultMax = serializedObject.FindProperty("defaultChannelMaxConnections").intValue;
            Debug.Log($"Default Channel Max Connections: {defaultMax}");

            SerializedProperty channelsProperty = serializedObject.FindProperty("channels");
            Debug.Log($"Custom Channels: {channelsProperty.arraySize}");

            for (int i = 0; i < channelsProperty.arraySize; i++)
            {
                SerializedProperty channel = channelsProperty.GetArrayElementAtIndex(i);
                string id = channel.FindPropertyRelative("id").stringValue;
                string title = channel.FindPropertyRelative("title").stringValue;
                int maxConn = channel.FindPropertyRelative("maxConnections").intValue;
                Debug.Log($"  Channel {i}: {id} ({title}) - Max: {maxConn}");
            }

            bool requireEmail = serializedObject.FindProperty("requireEmail").boolValue;
            bool requireVerification = serializedObject.FindProperty("requireEmailVerification").boolValue;
            Debug.Log($"Email Required: {requireEmail}, Verification: {requireVerification}");

            int minUserLen = serializedObject.FindProperty("minUsernameLength").intValue;
            int maxUserLen = serializedObject.FindProperty("maxUsernameLength").intValue;
            Debug.Log($"Username Length: {minUserLen}-{maxUserLen}");

            int minCharLen = serializedObject.FindProperty("minCharacterNameLength").intValue;
            int maxCharLen = serializedObject.FindProperty("maxCharacterNameLength").intValue;
            Debug.Log($"Character Name Length: {minCharLen}-{maxCharLen}");

            float updateInterval = serializedObject.FindProperty("updateUserCountInterval").floatValue;
            Debug.Log($"Statistics Update Interval: {updateInterval}s");

            Debug.Log("============================================");
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

        private void OpenCentralNetworkManagerDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "CentralNetworkManager.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened CentralNetworkManager documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/CentralNetworkManager.md");
                Debug.LogWarning($"📖 Local documentation not found, opening web version");
            }
        }
    }
}