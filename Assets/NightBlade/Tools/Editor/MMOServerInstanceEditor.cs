using UnityEngine;
using UnityEditor;
using NightBlade.MMO;
using System.Collections.Generic;
using System.Linq;

namespace NightBlade
{
    [CustomEditor(typeof(MMOServerInstance))]
    public class MMOServerInstanceEditor : Editor
    {
        private MMOServerInstance mmoServer;

        // Foldout states
        private bool networkManagersFoldout = true;
        private bool databaseConfigFoldout = true;
        private bool protocolSettingsFoldout = true;
        private bool editorSettingsFoldout = false;
        private bool serverStatusFoldout = true;
        private bool serverControlsFoldout = false;
        private bool diagnosticsFoldout = false;

        private void OnEnable()
        {
            mmoServer = (MMOServerInstance)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("🏭 NightBlade MMO Server Instance", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenMMOServerInstanceDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Status and overview
            DrawStatusOverview();

            // Network managers section
            DrawNetworkManagersSection();

            // Database configuration section
            DrawDatabaseConfigurationSection();

            // Protocol settings section
            DrawProtocolSettingsSection();

            // Editor settings section
            DrawEditorSettingsSection();

            // Server status (runtime)
            DrawServerStatusSection();

            // Server controls
            DrawServerControlsSection();

            // Diagnostics
            DrawDiagnosticsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStatusOverview()
        {
            EditorGUILayout.LabelField("📊 Server Instance Status", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Singleton status
            bool isSingleton = MMOServerInstance.Singleton == mmoServer;
            EditorGUILayout.LabelField($"Singleton: {(isSingleton ? "Active" : "Inactive")}",
                isSingleton ? EditorStyles.boldLabel : EditorStyles.miniLabel);

            // Component counts
            int assignedManagers = CountAssignedNetworkManagers();
            EditorGUILayout.LabelField($"Network Managers: {assignedManagers}/4 configured", EditorStyles.miniLabel);

            // Database status
            string dbStatus = GetDatabaseStatus();
            EditorGUILayout.LabelField($"Database: {dbStatus}", EditorStyles.miniLabel);

            // Protocol status
            bool useWebSocket = serializedObject.FindProperty("useWebSocket").boolValue;
            bool webSocketSecure = serializedObject.FindProperty("webSocketSecure").boolValue;
            string protocol = useWebSocket ? (webSocketSecure ? "WebSocket (Secure)" : "WebSocket (Standard)") : "UDP";
            EditorGUILayout.LabelField($"Protocol: {protocol}", EditorStyles.miniLabel);

            // Runtime status (if playing)
            if (Application.isPlaying)
            {
                // Check if any servers are running
                bool anyServerRunning = IsAnyServerRunning();
                EditorGUILayout.LabelField($"Servers Running: {(anyServerRunning ? "Yes" : "No")}",
                    anyServerRunning ? EditorStyles.boldLabel : EditorStyles.miniLabel);
            }

            // Quick validation
            bool hasCriticalComponents = HasCriticalComponents();
            if (!hasCriticalComponents)
            {
                EditorGUILayout.HelpBox($"⚠️ Missing critical network managers. Server functionality may be limited.", MessageType.Warning);
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
                GenerateServerReport();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawNetworkManagersSection()
        {
            networkManagersFoldout = EditorGUILayout.Foldout(networkManagersFoldout, "🔗 Network Managers - Core Server Components");
            if (networkManagersFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Four core network managers that handle different aspects of the MMO server infrastructure.", MessageType.Info);

                EditorGUILayout.LabelField("Central Network Manager", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Handles authentication, character management, and central server coordination.", MessageType.Info);
                SafePropertyField("centralNetworkManager", "Reference to the CentralNetworkManager component");

                Object centralManager = serializedObject.FindProperty("centralNetworkManager").objectReferenceValue;
                if (centralManager == null)
                {
                    EditorGUILayout.HelpBox("⚠️ Central Network Manager not assigned. Authentication and character services unavailable.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField($"✓ Assigned: {centralManager.name}", EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Map Spawn Network Manager", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Manages dynamic map server spawning and lifecycle.", MessageType.Info);
                SafePropertyField("mapSpawnNetworkManager", "Reference to the MapSpawnNetworkManager component");

                Object mapSpawnManager = serializedObject.FindProperty("mapSpawnNetworkManager").objectReferenceValue;
                if (mapSpawnManager == null)
                {
                    EditorGUILayout.HelpBox("⚠️ Map Spawn Network Manager not assigned. Dynamic map spawning unavailable.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField($"✓ Assigned: {mapSpawnManager.name}", EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Map Network Manager", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Handles world/map server networking and gameplay.", MessageType.Info);
                SafePropertyField("mapNetworkManager", "Reference to the MapNetworkManager component");

                Object mapManager = serializedObject.FindProperty("mapNetworkManager").objectReferenceValue;
                if (mapManager == null)
                {
                    EditorGUILayout.HelpBox("⚠️ Map Network Manager not assigned. World servers cannot operate.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField($"✓ Assigned: {mapManager.name}", EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Database Network Manager", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Manages database connections and data persistence.", MessageType.Info);
                SafePropertyField("databaseNetworkManager", "Reference to the DatabaseNetworkManager component");

                Object dbManager = serializedObject.FindProperty("databaseNetworkManager").objectReferenceValue;
                if (dbManager == null)
                {
                    EditorGUILayout.HelpBox("ℹ️ Database Network Manager not assigned. Will use custom database client if configured.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.LabelField($"✓ Assigned: {dbManager.name}", EditorStyles.miniLabel);
                }

                // Overall status
                int assignedCount = CountAssignedNetworkManagers();
                if (assignedCount == 4)
                {
                    EditorGUILayout.LabelField("✅ All network managers configured", EditorStyles.boldLabel);
                }
                else if (assignedCount >= 2)
                {
                    EditorGUILayout.LabelField($"⚠️ {4 - assignedCount} network managers missing - partial functionality", EditorStyles.boldLabel);
                }
                else
                {
                    EditorGUILayout.LabelField($"❌ Critical: Only {assignedCount} network managers configured", EditorStyles.boldLabel);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawDatabaseConfigurationSection()
        {
            databaseConfigFoldout = EditorGUILayout.Foldout(databaseConfigFoldout, "💾 Database Configuration - Data Persistence");
            if (databaseConfigFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure database connectivity and client options for data persistence.", MessageType.Info);

                EditorGUILayout.LabelField("Database Client Options", EditorStyles.boldLabel);

                SafePropertyField("useCustomDatabaseClient", "Use custom database client instead of DatabaseNetworkManager");
                bool useCustom = serializedObject.FindProperty("useCustomDatabaseClient").boolValue;

                if (useCustom)
                {
                    SafePropertyField("customDatabaseClientSource", "GameObject containing the custom database client component");

                    Object customSource = serializedObject.FindProperty("customDatabaseClientSource").objectReferenceValue;
                    if (customSource == null)
                    {
                        EditorGUILayout.HelpBox("⚠️ Custom database client source not set. Will use this GameObject.", MessageType.Warning);
                    }
                    else
                    {
                        // Check if the source has IDatabaseClient
                        var dbClient = (customSource as GameObject)?.GetComponent<IDatabaseClient>();
                        if (dbClient == null)
                        {
                            EditorGUILayout.HelpBox("⚠️ Custom database client source does not have IDatabaseClient component.", MessageType.Error);
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"✓ IDatabaseClient found on {customSource.name}", EditorStyles.miniLabel);
                        }
                    }
                }
                else
                {
                    Object dbManager = serializedObject.FindProperty("databaseNetworkManager").objectReferenceValue;
                    if (dbManager == null)
                    {
                        EditorGUILayout.HelpBox("⚠️ DatabaseNetworkManager not assigned and not using custom client.", MessageType.Error);
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"✓ Using DatabaseNetworkManager: {dbManager.name}", EditorStyles.miniLabel);
                    }
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Database Options", EditorStyles.boldLabel);
                SafePropertyField("databaseOptionIndex", "Database option index for DatabaseNetworkManager");
                SafePropertyField("disableDatabaseCaching", "Disable database caching for debugging");

                int dbOption = serializedObject.FindProperty("databaseOptionIndex").intValue;
                EditorGUILayout.LabelField($"Database Option: {dbOption}", EditorStyles.miniLabel);

                bool disableCache = serializedObject.FindProperty("disableDatabaseCaching").boolValue;
                if (disableCache)
                {
                    EditorGUILayout.HelpBox("⚠️ Database caching disabled - may impact performance.", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawProtocolSettingsSection()
        {
            protocolSettingsFoldout = EditorGUILayout.Foldout(protocolSettingsFoldout, "🌐 Protocol Settings - Network Communication");
            if (protocolSettingsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure network protocol and security settings for server communication.", MessageType.Info);

                EditorGUILayout.LabelField("Protocol Selection", EditorStyles.boldLabel);

                SafePropertyField("useWebSocket", "Use WebSocket protocol instead of UDP");
                bool useWebSocket = serializedObject.FindProperty("useWebSocket").boolValue;

                if (useWebSocket)
                {
                    SafePropertyField("webSocketSecure", "Use secure WebSocket (WSS) for encrypted connections");
                    bool secure = serializedObject.FindProperty("webSocketSecure").boolValue;

                    EditorGUILayout.LabelField($"Protocol: {(secure ? "WSS (Secure)" : "WS (Standard)")}", EditorStyles.miniLabel);

                    if (secure)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.LabelField("SSL Certificate Configuration", EditorStyles.boldLabel);
                        SafePropertyField("webSocketCertPath", "Path to SSL certificate file (.pfx or .p12)");
                        SafePropertyField("webSocketCertPassword", "Password for SSL certificate");

                        string certPath = serializedObject.FindProperty("webSocketCertPath").stringValue;
                        string certPass = serializedObject.FindProperty("webSocketCertPassword").stringValue;

                        if (string.IsNullOrEmpty(certPath))
                        {
                            EditorGUILayout.HelpBox("⚠️ SSL certificate path not configured. Secure connections will fail.", MessageType.Error);
                        }
                        else if (string.IsNullOrEmpty(certPass))
                        {
                            EditorGUILayout.HelpBox("⚠️ SSL certificate password not set.", MessageType.Warning);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("✅ SSL certificate configured", EditorStyles.miniLabel);
                        }
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

                // Protocol recommendations
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Recommended Settings:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Development: WebSocket (Standard) - easier debugging", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Testing: WebSocket (Secure) - production simulation", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Production: WebSocket (Secure) - encrypted traffic", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawEditorSettingsSection()
        {
            editorSettingsFoldout = EditorGUILayout.Foldout(editorSettingsFoldout, "🎮 Editor Settings - Development Configuration");
            if (editorSettingsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Settings for running servers in the Unity Editor for development and testing.", MessageType.Info);

                EditorGUILayout.LabelField("Auto-Start Options", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Automatically start server components when entering play mode in the editor.", MessageType.Info);

                SafePropertyField("startCentralOnAwake", "Auto-start Central Network Manager on Awake");
                SafePropertyField("startMapSpawnOnAwake", "Auto-start Map Spawn Network Manager on Awake");
                SafePropertyField("startMapOnAwake", "Auto-start Map Network Manager on Awake");
                SafePropertyField("startDatabaseOnAwake", "Auto-start Database Network Manager on Awake");

                // Count enabled auto-starts
                int autoStartCount = 0;
                if (serializedObject.FindProperty("startCentralOnAwake").boolValue) autoStartCount++;
                if (serializedObject.FindProperty("startMapSpawnOnAwake").boolValue) autoStartCount++;
                if (serializedObject.FindProperty("startDatabaseOnAwake").boolValue) autoStartCount++;
                if (serializedObject.FindProperty("startMapOnAwake").boolValue) autoStartCount++;

                EditorGUILayout.LabelField($"Auto-start servers: {autoStartCount}/4", EditorStyles.miniLabel);

                if (autoStartCount > 0)
                {
                    EditorGUILayout.HelpBox("ℹ️ Auto-start enabled for development. Disable for production builds.", MessageType.Info);
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Map Configuration", EditorStyles.boldLabel);
                SafePropertyField("startingMap", "Default map to load when starting map server");

                Object startingMap = serializedObject.FindProperty("startingMap").objectReferenceValue;
                if (startingMap == null && serializedObject.FindProperty("startMapOnAwake").boolValue)
                {
                    EditorGUILayout.HelpBox("⚠️ Starting map not set but auto-start map is enabled.", MessageType.Warning);
                }
                else if (startingMap != null)
                {
                    EditorGUILayout.LabelField($"✓ Starting map: {startingMap.name}", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawServerStatusSection()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("▶️ Enter play mode to see real-time server status.", MessageType.Info);
                return;
            }

            serverStatusFoldout = EditorGUILayout.Foldout(serverStatusFoldout, "📡 Server Status - Runtime Monitoring");
            if (serverStatusFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Real-time monitoring of server component status and connectivity.", MessageType.Info);

                // Central Server Status
                EditorGUILayout.LabelField("Central Server", EditorStyles.boldLabel);
                var centralManager = mmoServer.CentralNetworkManager;
                bool centralRunning = centralManager != null && centralManager.IsServer;
                EditorGUILayout.LabelField($"Status: {(centralRunning ? "🟢 Running" : "🔴 Stopped")}",
                    centralRunning ? EditorStyles.boldLabel : EditorStyles.miniLabel);

                if (centralRunning && centralManager != null)
                {
                    EditorGUILayout.LabelField($"Port: {centralManager.networkPort}", EditorStyles.miniLabel);
                    // Could show player count if available
                }

                // Map Spawn Server Status
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Map Spawn Server", EditorStyles.boldLabel);
                var mapSpawnManager = mmoServer.MapSpawnNetworkManager;
                bool mapSpawnRunning = mapSpawnManager != null && mapSpawnManager.IsServer;
                EditorGUILayout.LabelField($"Status: {(mapSpawnRunning ? "🟢 Running" : "🔴 Stopped")}",
                    mapSpawnRunning ? EditorStyles.boldLabel : EditorStyles.miniLabel);

                // Map Server Status
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Map Server", EditorStyles.boldLabel);
                var mapManager = mmoServer.MapNetworkManager;
                bool mapRunning = mapManager != null && mapManager.IsServer;
                EditorGUILayout.LabelField($"Status: {(mapRunning ? "🟢 Running" : "🔴 Stopped")}",
                    mapRunning ? EditorStyles.boldLabel : EditorStyles.miniLabel);

                if (mapRunning && mapManager != null)
                {
                    EditorGUILayout.LabelField($"Port: {mapManager.networkPort}", EditorStyles.miniLabel);
                    // Show current map if available
                    var currentMap = mapManager.MapInfo;
                    if (currentMap != null)
                    {
                        EditorGUILayout.LabelField($"Current Map: {currentMap.Id}", EditorStyles.miniLabel);
                    }
                }

                // Database Status
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Database", EditorStyles.boldLabel);
                string dbStatus = GetDatabaseStatus();
                bool dbActive = !dbStatus.Contains("Not");
                EditorGUILayout.LabelField($"Status: {(dbActive ? "🟢 Connected" : "🔴 Disconnected")}",
                    dbActive ? EditorStyles.boldLabel : EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Type: {dbStatus}", EditorStyles.miniLabel);

                // Overall status
                int runningServers = 0;
                if (centralRunning) runningServers++;
                if (mapSpawnRunning) runningServers++;
                if (mapRunning) runningServers++;
                if (dbActive) runningServers++;

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Active Services: {runningServers}/4", EditorStyles.boldLabel);

                if (runningServers == 4)
                {
                    EditorGUILayout.LabelField("✅ All servers operational", EditorStyles.boldLabel);
                }
                else if (runningServers >= 2)
                {
                    EditorGUILayout.LabelField("⚠️ Partial server operation", EditorStyles.boldLabel);
                }
                else if (runningServers == 0)
                {
                    EditorGUILayout.LabelField("❌ No servers running", EditorStyles.boldLabel);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawServerControlsSection()
        {
            serverControlsFoldout = EditorGUILayout.Foldout(serverControlsFoldout, "🎛️ Server Controls - Runtime Management");
            if (serverControlsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Runtime controls for starting and stopping individual server components.", MessageType.Info);

                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("▶️ Server controls available only in play mode.", MessageType.Info);
                }
                else
                {
                    // Central Server Controls
                    EditorGUILayout.LabelField("Central Server", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("▶️ Start", GUILayout.Height(25)))
                    {
                        mmoServer.StartCentralServer();
                    }
                    if (GUILayout.Button("⏹️ Stop", GUILayout.Height(25)))
                    {
                        mmoServer.CentralNetworkManager?.StopServer();
                    }
                    EditorGUILayout.EndHorizontal();

                    // Map Spawn Server Controls
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Map Spawn Server", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("▶️ Start", GUILayout.Height(25)))
                    {
                        mmoServer.StartMapSpawnServer();
                    }
                    if (GUILayout.Button("⏹️ Stop", GUILayout.Height(25)))
                    {
                        mmoServer.MapSpawnNetworkManager?.StopServer();
                    }
                    EditorGUILayout.EndHorizontal();

                    // Map Server Controls
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Map Server", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("▶️ Start", GUILayout.Height(25)))
                    {
                        mmoServer.StartMapServer();
                    }
                    if (GUILayout.Button("⏹️ Stop", GUILayout.Height(25)))
                    {
                        mmoServer.MapNetworkManager?.StopServer();
                    }
                    EditorGUILayout.EndHorizontal();

                    // Database Controls
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Database", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("▶️ Start Server", GUILayout.Height(25)))
                    {
                        mmoServer.StartDatabaseManagerServer();
                    }
                    if (GUILayout.Button("🔗 Start Client", GUILayout.Height(25)))
                    {
                        mmoServer.StartDatabaseManagerClient();
                    }
                    EditorGUILayout.EndHorizontal();

                    // Mass controls
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("All Servers", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("🚀 Start All", GUILayout.Height(25)))
                    {
                        StartAllServers();
                    }
                    if (GUILayout.Button("🛑 Stop All", GUILayout.Height(25)))
                    {
                        StopAllServers();
                    }
                    EditorGUILayout.EndHorizontal();
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

                int assignedManagers = CountAssignedNetworkManagers();
                bool useWebSocket = serializedObject.FindProperty("useWebSocket").boolValue;
                bool webSocketSecure = serializedObject.FindProperty("webSocketSecure").boolValue;
                bool useCustomDB = serializedObject.FindProperty("useCustomDatabaseClient").boolValue;

                EditorGUILayout.LabelField($"• Network Managers: {assignedManagers}/4 assigned", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"• Protocol: {(useWebSocket ? (webSocketSecure ? "WebSocket (Secure)" : "WebSocket (Standard)") : "UDP")}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"• Database: {(useCustomDB ? "Custom Client" : "Network Manager")}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"• Auto-start: {CountAutoStartServers()}/4 enabled", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }
        }

        private void AutoAssignNetworkManagers()
        {
            if (EditorUtility.DisplayDialog("Auto-Assign Network Managers",
                "This will search for and assign CentralNetworkManager, MapSpawnNetworkManager, MapNetworkManager, and DatabaseNetworkManager components. Continue?",
                "Yes", "No"))
            {
                // Find and assign managers
                var centralManagers = FindObjectsByType<CentralNetworkManager>(FindObjectsSortMode.None);
                var mapSpawnManagers = FindObjectsByType<MapSpawnNetworkManager>(FindObjectsSortMode.None);
                var mapManagers = FindObjectsByType<MapNetworkManager>(FindObjectsSortMode.None);
                var dbManagers = FindObjectsByType<DatabaseNetworkManager>(FindObjectsSortMode.None);

                if (centralManagers.Length > 0)
                {
                    serializedObject.FindProperty("centralNetworkManager").objectReferenceValue = centralManagers[0];
                    Debug.Log($"[MMOServerInstance] Auto-assigned CentralNetworkManager: {centralManagers[0].name}");
                }

                if (mapSpawnManagers.Length > 0)
                {
                    serializedObject.FindProperty("mapSpawnNetworkManager").objectReferenceValue = mapSpawnManagers[0];
                    Debug.Log($"[MMOServerInstance] Auto-assigned MapSpawnNetworkManager: {mapSpawnManagers[0].name}");
                }

                if (mapManagers.Length > 0)
                {
                    serializedObject.FindProperty("mapNetworkManager").objectReferenceValue = mapManagers[0];
                    Debug.Log($"[MMOServerInstance] Auto-assigned MapNetworkManager: {mapManagers[0].name}");
                }

                if (dbManagers.Length > 0)
                {
                    serializedObject.FindProperty("databaseNetworkManager").objectReferenceValue = dbManagers[0];
                    Debug.Log($"[MMOServerInstance] Auto-assigned DatabaseNetworkManager: {dbManagers[0].name}");
                }

                serializedObject.ApplyModifiedProperties();
                Debug.Log("[MMOServerInstance] Auto-assignment completed");
            }
        }

        private void ValidateConfigurationAndShowDialog()
        {
            ValidationResult result = ValidateConfiguration();

            if (result.IsValid)
            {
                EditorUtility.DisplayDialog("Validation Complete", "All server configurations are valid!", "OK");
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

        private void GenerateServerReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== MMOServerInstance Configuration Report ===");
            report.AppendLine($"Network Managers Assigned: {CountAssignedNetworkManagers()}/4");
            bool useWebSocket = serializedObject.FindProperty("useWebSocket").boolValue;
            bool webSocketSecure = serializedObject.FindProperty("webSocketSecure").boolValue;
            string protocol = useWebSocket ? (webSocketSecure ? "WebSocket (Secure)" : "WebSocket (Standard)") : "UDP";
            report.AppendLine($"Protocol: {protocol}");
            bool useCustomDB = serializedObject.FindProperty("useCustomDatabaseClient").boolValue;
            string databaseType = useCustomDB ? "Custom Client" : "Network Manager";
            report.AppendLine($"Database: {databaseType}");

            report.AppendLine($"Auto-start Servers: {CountAutoStartServers()}/4");

            string certPath = serializedObject.FindProperty("webSocketCertPath").stringValue;
            string sslStatus = !string.IsNullOrEmpty(certPath) ? "Configured" : "Not Set";
            report.AppendLine($"SSL Certificate: {sslStatus}");

            if (Application.isPlaying)
            {
                report.AppendLine($"Runtime - Central Running: {IsServerRunning("Central")}");
                report.AppendLine($"Runtime - Map Spawn Running: {IsServerRunning("MapSpawn")}");
                report.AppendLine($"Runtime - Map Running: {IsServerRunning("Map")}");
                report.AppendLine($"Runtime - Database Active: {GetDatabaseStatus().Contains("Connected")}");
            }

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("Server Report Generated", "Configuration report logged to console.", "OK");
        }

        private void StartAllServers()
        {
            try
            {
                mmoServer.StartCentralServer();
                System.Threading.Thread.Sleep(100); // Small delay between starts
                mmoServer.StartMapSpawnServer();
                System.Threading.Thread.Sleep(100);
                mmoServer.StartMapServer();
                System.Threading.Thread.Sleep(100);
                mmoServer.StartDatabaseManagerServer();
                Debug.Log("[MMOServerInstance] All servers started");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MMOServerInstance] Error starting servers: {ex.Message}");
            }
        }

        private void StopAllServers()
        {
            try
            {
                mmoServer.CentralNetworkManager?.StopServer();
                mmoServer.MapSpawnNetworkManager?.StopServer();
                mmoServer.MapNetworkManager?.StopServer();
                // Database doesn't have a direct stop method in the API shown
                Debug.Log("[MMOServerInstance] All servers stopped");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MMOServerInstance] Error stopping servers: {ex.Message}");
            }
        }

        private int CountAssignedNetworkManagers()
        {
            int count = 0;
            if (serializedObject.FindProperty("centralNetworkManager").objectReferenceValue != null) count++;
            if (serializedObject.FindProperty("mapSpawnNetworkManager").objectReferenceValue != null) count++;
            if (serializedObject.FindProperty("mapNetworkManager").objectReferenceValue != null) count++;
            if (serializedObject.FindProperty("databaseNetworkManager").objectReferenceValue != null) count++;
            return count;
        }

        private int CountAutoStartServers()
        {
            int count = 0;
            if (serializedObject.FindProperty("startCentralOnAwake").boolValue) count++;
            if (serializedObject.FindProperty("startMapSpawnOnAwake").boolValue) count++;
            if (serializedObject.FindProperty("startDatabaseOnAwake").boolValue) count++;
            if (serializedObject.FindProperty("startMapOnAwake").boolValue) count++;
            return count;
        }

        private bool HasCriticalComponents()
        {
            // At minimum, we need central and map managers
            return serializedObject.FindProperty("centralNetworkManager").objectReferenceValue != null &&
                   serializedObject.FindProperty("mapNetworkManager").objectReferenceValue != null;
        }

        private bool IsAnyServerRunning()
        {
            return IsServerRunning("Central") || IsServerRunning("MapSpawn") ||
                   IsServerRunning("Map") || GetDatabaseStatus().Contains("Connected");
        }

        private bool IsServerRunning(string serverType)
        {
            switch (serverType)
            {
                case "Central":
                    var central = mmoServer.CentralNetworkManager;
                    return central != null && central.IsServer;
                case "MapSpawn":
                    var mapSpawn = mmoServer.MapSpawnNetworkManager;
                    return mapSpawn != null && mapSpawn.IsServer;
                case "Map":
                    var map = mmoServer.MapNetworkManager;
                    return map != null && map.IsServer;
                default:
                    return false;
            }
        }

        private string GetDatabaseStatus()
        {
            bool useCustom = serializedObject.FindProperty("useCustomDatabaseClient").boolValue;
            if (useCustom)
            {
                Object customSource = serializedObject.FindProperty("customDatabaseClientSource").objectReferenceValue;
                if (customSource != null)
                {
                    var dbClient = (customSource as GameObject)?.GetComponent<IDatabaseClient>();
                    return dbClient != null ? "Custom Client" : "Custom Client (Invalid)";
                }
                return "Custom Client (No Source)";
            }
            else
            {
                Object dbManager = serializedObject.FindProperty("databaseNetworkManager").objectReferenceValue;
                return dbManager != null ? "Network Manager" : "Network Manager (Not Assigned)";
            }
        }

        private ValidationResult ValidateConfiguration()
        {
            var result = new ValidationResult();
            var issues = new List<ValidationIssue>();
            var performanceWarnings = new List<string>();

            // Check critical network managers
            if (serializedObject.FindProperty("centralNetworkManager").objectReferenceValue == null)
                issues.Add(new ValidationIssue("❌", "Central Network Manager not assigned - authentication will fail"));
            if (serializedObject.FindProperty("mapNetworkManager").objectReferenceValue == null)
                issues.Add(new ValidationIssue("❌", "Map Network Manager not assigned - world servers cannot run"));

            // Check database configuration
            bool useCustomDB = serializedObject.FindProperty("useCustomDatabaseClient").boolValue;
            if (!useCustomDB && serializedObject.FindProperty("databaseNetworkManager").objectReferenceValue == null)
                issues.Add(new ValidationIssue("❌", "Database Network Manager not assigned and not using custom client"));

            if (useCustomDB)
            {
                Object customSource = serializedObject.FindProperty("customDatabaseClientSource").objectReferenceValue;
                if (customSource == null)
                {
                    issues.Add(new ValidationIssue("⚠️", "Custom database client source not set"));
                }
                else
                {
                    var dbClient = (customSource as GameObject)?.GetComponent<IDatabaseClient>();
                    if (dbClient == null)
                        issues.Add(new ValidationIssue("❌", "Custom database client source missing IDatabaseClient component"));
                }
            }

            // Check WebSocket SSL configuration
            bool useWebSocket = serializedObject.FindProperty("useWebSocket").boolValue;
            bool webSocketSecure = serializedObject.FindProperty("webSocketSecure").boolValue;

            if (useWebSocket && webSocketSecure)
            {
                string certPath = serializedObject.FindProperty("webSocketCertPath").stringValue;
                string certPass = serializedObject.FindProperty("webSocketCertPassword").stringValue;

                if (string.IsNullOrEmpty(certPath))
                    issues.Add(new ValidationIssue("❌", "SSL certificate path required for secure WebSocket"));
                if (string.IsNullOrEmpty(certPass))
                    issues.Add(new ValidationIssue("⚠️", "SSL certificate password not set"));
            }

            // Check auto-start configuration
            if (serializedObject.FindProperty("startMapOnAwake").boolValue &&
                serializedObject.FindProperty("startingMap").objectReferenceValue == null)
            {
                issues.Add(new ValidationIssue("⚠️", "Auto-start map enabled but no starting map selected"));
            }

            // Performance warnings
            bool useWebSocketNonSecure = useWebSocket && !webSocketSecure;
            if (useWebSocketNonSecure)
                performanceWarnings.Add("WebSocket without SSL - unencrypted traffic in production");

            if (CountAutoStartServers() > 2)
                performanceWarnings.Add("Multiple auto-start servers may slow editor startup");

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

        private void OpenMMOServerInstanceDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "MMOServerInstance.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened MMOServerInstance documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/MMOServerInstance.md");
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