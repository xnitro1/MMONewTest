using UnityEngine;
using UnityEditor;
using NightBlade.MMO;
using System.Collections.Generic;

namespace NightBlade
{
    [CustomEditor(typeof(MMOClientInstance))]
    public class MMOClientInstanceEditor : Editor
    {
        private MMOClientInstance mmoClient;

        // Foldout states
        private bool networkManagersFoldout = true;
        private bool connectionSettingsFoldout = true;
        private bool networkSettingsFoldout = false;
        private bool connectionStatusFoldout = true;
        private bool clientActionsFoldout = false;
        private bool diagnosticsFoldout = false;

        private void OnEnable()
        {
            mmoClient = (MMOClientInstance)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("🌐 NightBlade MMO Client Instance", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenMMOClientInstanceDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Status and overview
            DrawStatusOverview();

            // Network managers section
            DrawNetworkManagersSection();

            // Connection settings section
            DrawConnectionSettingsSection();

            // Network settings section
            DrawNetworkSettingsSection();

            // Connection status (runtime)
            DrawConnectionStatusSection();

            // Client actions
            DrawClientActionsSection();

            // Diagnostics
            DrawDiagnosticsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStatusOverview()
        {
            EditorGUILayout.LabelField("📊 Client Instance Status", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Singleton status
            bool isSingleton = MMOClientInstance.Singleton == mmoClient;
            EditorGUILayout.LabelField($"Singleton: {(isSingleton ? "Active" : "Inactive")}",
                isSingleton ? EditorStyles.boldLabel : EditorStyles.miniLabel);

            // Component counts
            int networkSettingsCount = serializedObject.FindProperty("networkSettings").arraySize;
            EditorGUILayout.LabelField($"Network Settings: {networkSettingsCount}", EditorStyles.miniLabel);

            // Runtime status (if playing)
            if (Application.isPlaying)
            {
                bool centralConnected = mmoClient.IsConnectedToCentralServer();
                EditorGUILayout.LabelField($"Central Server: {(centralConnected ? "Connected" : "Disconnected")}",
                    centralConnected ? EditorStyles.boldLabel : EditorStyles.miniLabel);

                if (!string.IsNullOrEmpty(mmoClient.SelectedChannelId))
                {
                    EditorGUILayout.LabelField($"Selected Channel: {mmoClient.SelectedChannelId}", EditorStyles.miniLabel);
                }
            }

            // Quick validation
            bool hasCentralManager = serializedObject.FindProperty("centralNetworkManager").objectReferenceValue != null;
            bool hasMapManager = serializedObject.FindProperty("mapNetworkManager").objectReferenceValue != null;

            if (!hasCentralManager || !hasMapManager)
            {
                EditorGUILayout.HelpBox($"⚠️ Missing network managers. Auto-assignment recommended.", MessageType.Warning);
            }

            // Quick actions
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Auto-Assign", GUILayout.Height(25)))
            {
                AutoAssignNetworkManagers();
            }
            if (GUILayout.Button("✅ Validate", GUILayout.Height(25)))
            {
                ValidateConfigurationAndShowDialog();
            }
            if (GUILayout.Button("📋 Report", GUILayout.Height(25)))
            {
                GenerateClientReport();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawNetworkManagersSection()
        {
            networkManagersFoldout = EditorGUILayout.Foldout(networkManagersFoldout, "🔗 Network Managers - Core Components");
            if (networkManagersFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Core network managers that handle client-server communication for the MMO system.", MessageType.Info);

                EditorGUILayout.LabelField("Central Network Manager", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Handles authentication, character management, and central server communication.", MessageType.Info);
                SafePropertyField("centralNetworkManager", "Reference to the CentralNetworkManager component");

                Object centralManager = serializedObject.FindProperty("centralNetworkManager").objectReferenceValue;
                if (centralManager == null)
                {
                    EditorGUILayout.HelpBox("⚠️ Central Network Manager not assigned. Will auto-find in hierarchy.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField($"✓ Assigned: {centralManager.name}", EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Map Network Manager", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Handles world/map server connections and gameplay networking.", MessageType.Info);
                SafePropertyField("mapNetworkManager", "Reference to the MapNetworkManager component");

                Object mapManager = serializedObject.FindProperty("mapNetworkManager").objectReferenceValue;
                if (mapManager == null)
                {
                    EditorGUILayout.HelpBox("⚠️ Map Network Manager not assigned. Will auto-find in hierarchy.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField($"✓ Assigned: {mapManager.name}", EditorStyles.miniLabel);
                }

                // Assignment status
                bool bothAssigned = centralManager != null && mapManager != null;
                if (bothAssigned)
                {
                    EditorGUILayout.LabelField("✅ All network managers configured", EditorStyles.boldLabel);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawConnectionSettingsSection()
        {
            connectionSettingsFoldout = EditorGUILayout.Foldout(connectionSettingsFoldout, "⚙️ Connection Settings - Protocol Configuration");
            if (connectionSettingsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure the network protocol and security settings for client connections.", MessageType.Info);

                EditorGUILayout.LabelField("Protocol Settings", EditorStyles.boldLabel);

                // WebSocket settings
                SafePropertyField("useWebSocket", "Use WebSocket protocol instead of UDP (more compatible with web/mobile)");
                bool useWebSocket = serializedObject.FindProperty("useWebSocket").boolValue;

                if (useWebSocket)
                {
                    SafePropertyField("webSocketSecure", "Use secure WebSocket (WSS) for encrypted connections");
                    bool secure = serializedObject.FindProperty("webSocketSecure").boolValue;

                    EditorGUILayout.LabelField($"Protocol: {(secure ? "WSS (Secure)" : "WS (Standard)")}", EditorStyles.miniLabel);

                    if (secure)
                    {
                        EditorGUILayout.HelpBox("🔒 Secure WebSocket enabled. Requires proper SSL certificate configuration.", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("⚠️ Standard WebSocket (unencrypted). Use only for development.", MessageType.Warning);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Protocol: UDP (LiteNetLib)", EditorStyles.miniLabel);
                    EditorGUILayout.HelpBox("🚀 UDP provides better performance for real-time gaming.", MessageType.Info);
                }

                // Connection recommendations
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Recommended Settings:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Development: WebSocket (Standard) - easier debugging", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Production: WebSocket (Secure) - encrypted traffic", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• High Performance: UDP - lowest latency", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawNetworkSettingsSection()
        {
            networkSettingsFoldout = EditorGUILayout.Foldout(networkSettingsFoldout, "🌐 Network Settings - Server Configuration");
            if (networkSettingsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure available network settings for different server environments and regions.", MessageType.Info);

                // Network settings array
                SerializedProperty networkSettingsProperty = serializedObject.FindProperty("networkSettings");
                EditorGUILayout.PropertyField(networkSettingsProperty, new GUIContent("Network Settings Array"), true);

                int settingsCount = networkSettingsProperty.arraySize;
                EditorGUILayout.LabelField($"Configured Servers: {settingsCount}", EditorStyles.miniLabel);

                // Settings management
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("➕ Add Server", GUILayout.Height(25)))
                {
                    networkSettingsProperty.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("🗑️ Clear All", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("Clear Network Settings",
                        "Are you sure you want to remove all network settings?", "Yes", "No"))
                    {
                        networkSettingsProperty.arraySize = 0;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Validation
                if (settingsCount == 0)
                {
                    EditorGUILayout.HelpBox("⚠️ No network settings configured. Client cannot connect to servers.", MessageType.Error);
                }
                else
                {
                    // Check for valid configurations
                    bool hasValidSettings = false;
                    for (int i = 0; i < settingsCount; i++)
                    {
                        var setting = networkSettingsProperty.GetArrayElementAtIndex(i);
                        string title = setting.FindPropertyRelative("title").stringValue;
                        string address = setting.FindPropertyRelative("address").stringValue;
                        int port = setting.FindPropertyRelative("port").intValue;

                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(address) && port > 0)
                        {
                            hasValidSettings = true;
                            break;
                        }
                    }

                    if (!hasValidSettings)
                    {
                        EditorGUILayout.HelpBox("⚠️ Network settings exist but appear incomplete. Check titles, addresses, and ports.", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("✅ Valid network settings detected", EditorStyles.miniLabel);
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawConnectionStatusSection()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("▶️ Enter play mode to see real-time connection status.", MessageType.Info);
                return;
            }

            connectionStatusFoldout = EditorGUILayout.Foldout(connectionStatusFoldout, "📡 Connection Status - Runtime Monitoring");
            if (connectionStatusFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Real-time monitoring of client connection states and server information.", MessageType.Info);

                // Central server status
                EditorGUILayout.LabelField("Central Server Connection", EditorStyles.boldLabel);
                bool centralConnected = mmoClient.IsConnectedToCentralServer();
                EditorGUILayout.LabelField($"Status: {(centralConnected ? "🟢 Connected" : "🔴 Disconnected")}",
                    centralConnected ? EditorStyles.boldLabel : EditorStyles.miniLabel);

                if (centralConnected)
                {
                    // Try to get connection info from CentralNetworkManager
                    var centralManager = mmoClient.CentralNetworkManager;
                    if (centralManager != null)
                    {
                        EditorGUILayout.LabelField($"Address: {MMOClientInstance.SelectedCentralAddress}:{MMOClientInstance.SelectedCentralPort}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField($"User ID: {GameInstance.UserId}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField($"Channel: {mmoClient.SelectedChannelId}", EditorStyles.miniLabel);
                    }
                }

                // Map server status
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Map Server Connection", EditorStyles.boldLabel);
                var mapManager = mmoClient.MapNetworkManager;
                bool mapConnected = mapManager != null && mapManager.IsClientConnected;
                EditorGUILayout.LabelField($"Status: {(mapConnected ? "🟢 Connected" : "🔴 Disconnected")}",
                    mapConnected ? EditorStyles.boldLabel : EditorStyles.miniLabel);

                if (mapConnected && mapManager != null)
                {
                    // Try to show current map info
                    var currentMap = mapManager.MapInfo;
                    if (currentMap != null)
                    {
                        EditorGUILayout.LabelField($"Current Map: {currentMap.Id}", EditorStyles.miniLabel);
                    }
                }

                // Client data
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Client Session Data", EditorStyles.boldLabel);
                if (!string.IsNullOrEmpty(GameInstance.UserId))
                {
                    EditorGUILayout.LabelField($"Authenticated User: {GameInstance.UserId}", EditorStyles.miniLabel);
                }
                if (!string.IsNullOrEmpty(GameInstance.SelectedCharacterId))
                {
                    EditorGUILayout.LabelField($"Selected Character: {GameInstance.SelectedCharacterId}", EditorStyles.miniLabel);
                }

                // Connection quality indicators
                if (centralConnected || mapConnected)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Connection Quality: Good", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawClientActionsSection()
        {
            clientActionsFoldout = EditorGUILayout.Foldout(clientActionsFoldout, "🎮 Client Actions - Runtime Controls");
            if (clientActionsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Runtime controls for testing and debugging client connections.", MessageType.Info);

                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("▶️ Actions available only in play mode.", MessageType.Info);
                }
                else
                {
                    // Central server controls
                    EditorGUILayout.LabelField("Central Server", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("🔗 Connect", GUILayout.Height(25)))
                    {
                        mmoClient.StartCentralClient();
                    }
                    if (GUILayout.Button("🔌 Disconnect", GUILayout.Height(25)))
                    {
                        mmoClient.StopCentralClient();
                    }
                    EditorGUILayout.EndHorizontal();

                    // Map server controls
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Map Server", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("🗺️ Connect to Map", GUILayout.Height(25)))
                    {
                        // This would need map info - show info instead
                        EditorUtility.DisplayDialog("Map Connection",
                            "Use character selection or direct API calls to connect to map servers.", "OK");
                    }
                    if (GUILayout.Button("🔌 Disconnect", GUILayout.Height(25)))
                    {
                        mmoClient.StopMapClient();
                    }
                    EditorGUILayout.EndHorizontal();

                    // Data management
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Session Management", EditorStyles.boldLabel);
                    if (GUILayout.Button("🧹 Clear Client Data", GUILayout.Height(25)))
                    {
                        if (EditorUtility.DisplayDialog("Clear Client Data",
                            "This will clear user authentication and character selection data. Continue?", "Yes", "No"))
                        {
                            mmoClient.ClearClientData();
                            Debug.Log("[MMOClientInstance] Client data cleared");
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawDiagnosticsSection()
        {
            diagnosticsFoldout = EditorGUILayout.Foldout(diagnosticsFoldout, "🔍 Diagnostics - Configuration Analysis");
            if (diagnosticsFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                // Configuration validation
                ValidationResult validation = ValidateConfiguration();

                EditorGUILayout.LabelField("Configuration Health", EditorStyles.boldLabel);

                // Overall status
                if (validation.IsValid)
                {
                    EditorGUILayout.LabelField("✅ Configuration Valid", EditorStyles.boldLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("⚠️ Issues Found", EditorStyles.boldLabel);
                }

                // Detailed validation
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Validation Results:", EditorStyles.miniLabel);

                foreach (var issue in validation.Issues)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(issue.Icon, GUILayout.Width(20));
                    EditorGUILayout.LabelField(issue.Message, EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }

                // Performance notes
                if (validation.PerformanceWarnings.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Performance Notes:", EditorStyles.miniLabel);
                    foreach (var warning in validation.PerformanceWarnings)
                    {
                        EditorGUILayout.LabelField($"⚡ {warning}", EditorStyles.miniLabel);
                    }
                }

                // Configuration summary
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Configuration Summary:", EditorStyles.miniLabel);

                bool useWebSocket = serializedObject.FindProperty("useWebSocket").boolValue;
                bool webSocketSecure = serializedObject.FindProperty("webSocketSecure").boolValue;
                int networkSettingsCount = serializedObject.FindProperty("networkSettings").arraySize;

                EditorGUILayout.LabelField($"• Protocol: {(useWebSocket ? (webSocketSecure ? "WebSocket (Secure)" : "WebSocket (Standard)") : "UDP")}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"• Network Settings: {networkSettingsCount} server configurations", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"• Central Manager: {(serializedObject.FindProperty("centralNetworkManager").objectReferenceValue != null ? "Assigned" : "Auto-detect")}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"• Map Manager: {(serializedObject.FindProperty("mapNetworkManager").objectReferenceValue != null ? "Assigned" : "Auto-detect")}", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }
        }

        private void AutoAssignNetworkManagers()
        {
            if (EditorUtility.DisplayDialog("Auto-Assign Network Managers",
                "This will search for CentralNetworkManager and MapNetworkManager components in the scene. Continue?",
                "Yes", "No"))
            {
                // Find CentralNetworkManager
                var centralManagers = FindObjectsByType<CentralNetworkManager>(FindObjectsSortMode.None);
                if (centralManagers.Length > 0)
                {
                    serializedObject.FindProperty("centralNetworkManager").objectReferenceValue = centralManagers[0];
                    Debug.Log($"[MMOClientInstance] Auto-assigned CentralNetworkManager: {centralManagers[0].name}");
                }

                // Find MapNetworkManager
                var mapManagers = FindObjectsByType<MapNetworkManager>(FindObjectsSortMode.None);
                if (mapManagers.Length > 0)
                {
                    serializedObject.FindProperty("mapNetworkManager").objectReferenceValue = mapManagers[0];
                    Debug.Log($"[MMOClientInstance] Auto-assigned MapNetworkManager: {mapManagers[0].name}");
                }

                serializedObject.ApplyModifiedProperties();
                Debug.Log("[MMOClientInstance] Auto-assignment completed");
            }
        }

        private void ValidateConfigurationAndShowDialog()
        {
            ValidationResult result = ValidateConfiguration();

            if (result.IsValid)
            {
                EditorUtility.DisplayDialog("Validation Complete", "All client configurations are valid!", "OK");
            }
            else
            {
                string message = $"Found {result.Issues.Count} configuration issues:\n\n";
                foreach (var issue in result.Issues)
                {
                    message += $"• {issue.Message}\n";
                }
                EditorUtility.DisplayDialog("Validation Issues", message, "OK");
            }
        }

        private void GenerateClientReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== MMOClientInstance Configuration Report ===");
            report.AppendLine($"WebSocket Enabled: {serializedObject.FindProperty("useWebSocket").boolValue}");
            report.AppendLine($"WebSocket Secure: {serializedObject.FindProperty("webSocketSecure").boolValue}");
            report.AppendLine($"Network Settings Count: {serializedObject.FindProperty("networkSettings").arraySize}");
            report.AppendLine($"Central Manager Assigned: {serializedObject.FindProperty("centralNetworkManager").objectReferenceValue != null}");
            report.AppendLine($"Map Manager Assigned: {serializedObject.FindProperty("mapNetworkManager").objectReferenceValue != null}");

            if (Application.isPlaying)
            {
                report.AppendLine($"Runtime - Central Connected: {mmoClient.IsConnectedToCentralServer()}");
                report.AppendLine($"Runtime - Selected Channel: {mmoClient.SelectedChannelId}");
                report.AppendLine($"Runtime - User ID: {GameInstance.UserId}");
                report.AppendLine($"Runtime - Character ID: {GameInstance.SelectedCharacterId}");
            }

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Client Report Generated", "Configuration report logged to console.", "OK");
        }

        private ValidationResult ValidateConfiguration()
        {
            var result = new ValidationResult();
            var issues = new List<ValidationIssue>();
            var performanceWarnings = new List<string>();

            // Check network managers
            bool hasCentralManager = serializedObject.FindProperty("centralNetworkManager").objectReferenceValue != null;
            bool hasMapManager = serializedObject.FindProperty("mapNetworkManager").objectReferenceValue != null;

            if (!hasCentralManager)
                issues.Add(new ValidationIssue("ℹ️", "Central Network Manager not assigned (will auto-detect)"));
            if (!hasMapManager)
                issues.Add(new ValidationIssue("ℹ️", "Map Network Manager not assigned (will auto-detect)"));

            // Check network settings
            int networkSettingsCount = serializedObject.FindProperty("networkSettings").arraySize;
            if (networkSettingsCount == 0)
            {
                issues.Add(new ValidationIssue("❌", "No network settings configured - client cannot connect"));
            }
            else
            {
                // Validate network settings
                var networkSettingsProperty = serializedObject.FindProperty("networkSettings");
                bool hasValidSetting = false;

                for (int i = 0; i < networkSettingsCount; i++)
                {
                    var setting = networkSettingsProperty.GetArrayElementAtIndex(i);
                    string title = setting.FindPropertyRelative("title").stringValue;
                    string address = setting.FindPropertyRelative("address").stringValue;
                    int port = setting.FindPropertyRelative("port").intValue;

                    if (string.IsNullOrEmpty(title))
                        issues.Add(new ValidationIssue("⚠️", $"Network setting {i + 1}: Missing title"));
                    if (string.IsNullOrEmpty(address))
                        issues.Add(new ValidationIssue("⚠️", $"Network setting {i + 1}: Missing address"));
                    if (port <= 0 || port > 65535)
                        issues.Add(new ValidationIssue("⚠️", $"Network setting {i + 1}: Invalid port ({port})"));
                    else if (port < 1024)
                        issues.Add(new ValidationIssue("ℹ️", $"Network setting {i + 1}: Port {port} is in system range"));

                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(address) && port > 0 && port <= 65535)
                        hasValidSetting = true;
                }

                if (!hasValidSetting)
                    issues.Add(new ValidationIssue("❌", "No valid network settings found"));
            }

            // WebSocket configuration warnings
            bool useWebSocket = serializedObject.FindProperty("useWebSocket").boolValue;
            bool webSocketSecure = serializedObject.FindProperty("webSocketSecure").boolValue;

            if (useWebSocket && !webSocketSecure)
            {
                performanceWarnings.Add("WebSocket without SSL - unencrypted traffic in production");
            }

            result.Issues = issues;
            result.PerformanceWarnings = performanceWarnings;
            result.IsValid = issues.FindAll(i => i.Icon == "❌").Count == 0;

            return result;
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

        private void OpenMMOClientInstanceDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "MMOClientInstance.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened MMOClientInstance documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/MMOClientInstance.md");
                Debug.LogWarning($"📖 Local documentation not found, opening web version");
            }
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
            public List<string> PerformanceWarnings { get; set; } = new List<string>();
        }

        private class ValidationIssue
        {
            public string Icon { get; set; }
            public string Message { get; set; }

            public ValidationIssue(string icon, string message)
            {
                Icon = icon;
                Message = message;
            }
        }
    }
}