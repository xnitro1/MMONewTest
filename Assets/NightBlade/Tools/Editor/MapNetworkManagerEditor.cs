using UnityEngine;
using UnityEditor;
using NightBlade.MMO;

namespace NightBlade
{
    [CustomEditor(typeof(MapNetworkManager))]
    public class MapNetworkManagerEditor : Editor
    {
        private MapNetworkManager mapManager;

        // Foldout states
        private bool centralConnectionFoldout = true;
        private bool mapSpawnFoldout = true;
        private bool playerDisconnectionFoldout = false;

        private void OnEnable()
        {
            mapManager = (MapNetworkManager)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("🗺️ NightBlade Map Network Manager", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenMapNetworkManagerDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Version info and server type
            EditorGUILayout.HelpBox("Map server orchestration and player management for MMO world instances", MessageType.Info);
            EditorGUILayout.Space(5);

            // Server type information
            DrawServerTypeInfo();

            // Runtime status (if playing)
            DrawRuntimeStatus();

            // Configuration sections
            DrawCentralConnectionSection();
            DrawMapSpawnSection();
            DrawPlayerDisconnectionSection();

            // Server health and diagnostics
            DrawServerDiagnostics();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawServerTypeInfo()
        {
            EditorGUILayout.LabelField("🏷️ Server Configuration", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Show what type of server this is
            string serverType = "Standard Map Server";
            string serverDescription = "Regular world server for persistent game content";

            if (!string.IsNullOrEmpty(mapManager.MapInstanceId))
            {
                serverType = "Instance Map Server";
                serverDescription = "Temporary server instance for events, dungeons, or private sessions";
            }
            else if (mapManager.IsAllocate)
            {
                serverType = "Allocate Map Server";
                serverDescription = "Dynamic server allocation for load balancing";
            }

            EditorGUILayout.LabelField($"Type: {serverType}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Description: {serverDescription}", EditorStyles.miniLabel);

            if (!string.IsNullOrEmpty(mapManager.MapInstanceId))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Instance ID: {mapManager.MapInstanceId}", EditorStyles.miniLabel);
                if (mapManager.MapInstanceWarpToPosition != Vector3.zero)
                {
                    EditorGUILayout.LabelField($"Warp Position: {mapManager.MapInstanceWarpToPosition}", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawRuntimeStatus()
        {
            if (Application.isPlaying && mapManager != null)
            {
                EditorGUILayout.LabelField("🎮 Runtime Status", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                // Connection status
                bool isConnected = mapManager.ClusterClient != null && mapManager.ClusterClient.IsNetworkActive;
                EditorGUILayout.LabelField($"🔗 Cluster Connection: {(isConnected ? "Connected" : "Disconnected")}",
                    isConnected ? EditorStyles.boldLabel : EditorStyles.miniLabel);

                // Server info
                if (isConnected)
                {
                    EditorGUILayout.LabelField($"🌐 Server Address: {mapManager.AppAddress}:{mapManager.AppPort}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"🏷️ Reference ID: {mapManager.RefId}", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField($"👥 Peer Type: {mapManager.PeerType}", EditorStyles.miniLabel);
                }

                // Player count (if available)
                if (Application.isPlaying && mapManager.IsServer)
                {
                    int playerCount = mapManager.PlayersCount;
                    EditorGUILayout.LabelField($"👤 Connected Players: {playerCount}", EditorStyles.miniLabel);
                }

                // Termination status
                if (mapManager.ProceedingBeforeQuit)
                {
                    EditorGUILayout.LabelField("⏳ Server Shutdown: In Progress", EditorStyles.boldLabel);
                    if (mapManager.ReadyToQuit)
                    {
                        EditorGUILayout.LabelField("✅ Ready to Quit", EditorStyles.boldLabel);
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(10);
            }
        }

        private void DrawCentralConnectionSection()
        {
            centralConnectionFoldout = EditorGUILayout.Foldout(centralConnectionFoldout, "🌐 Central Network Connection");
            if (centralConnectionFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure connection to the central cluster server that coordinates all map instances.", MessageType.Info);

                EditorGUILayout.LabelField("Cluster Server Connection", EditorStyles.boldLabel);
                SafePropertyField("clusterServerAddress", "IP address or hostname of the central cluster server");
                SafePropertyField("clusterServerPort", "Port number for cluster server communication");

                // Connection validation
                string address = serializedObject.FindProperty("clusterServerAddress").stringValue;
                int port = serializedObject.FindProperty("clusterServerPort").intValue;

                if (string.IsNullOrEmpty(address) || address == "0.0.0.0")
                {
                    EditorGUILayout.HelpBox("⚠️ Cluster server address is not configured", MessageType.Warning);
                }

                if (port < 1024 || port > 65535)
                {
                    EditorGUILayout.HelpBox("⚠️ Port should be between 1024-65535 for standard applications", MessageType.Warning);
                }

                if (address == "127.0.0.1" && port == 6010)
                {
                    EditorGUILayout.LabelField("ℹ️ Using default localhost cluster configuration", EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Public Server Address", EditorStyles.boldLabel);
                SafePropertyField("publicAddress", "Public IP address or hostname that clients will connect to");

                string publicAddr = serializedObject.FindProperty("publicAddress").stringValue;
                if (string.IsNullOrEmpty(publicAddr) || publicAddr == "127.0.0.1")
                {
                    EditorGUILayout.HelpBox("ℹ️ Using localhost address - suitable for development only", MessageType.Info);
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
                EditorGUILayout.HelpBox("Control spawning behavior for map servers and instance coordination.", MessageType.Info);

                EditorGUILayout.LabelField("Spawn Timeouts", EditorStyles.boldLabel);
                SafePropertyField("mapSpawnMillisecondsTimeout", "Timeout in milliseconds for map server spawning operations");

                int timeout = serializedObject.FindProperty("mapSpawnMillisecondsTimeout").intValue;
                if (timeout == 0)
                {
                    EditorGUILayout.HelpBox("ℹ️ No timeout set - spawn operations will wait indefinitely", MessageType.Info);
                }
                else if (timeout < 10000)
                {
                    EditorGUILayout.HelpBox("⚠️ Very short timeout may cause failed spawns during load", MessageType.Warning);
                }
                else if (timeout > 120000)
                {
                    EditorGUILayout.HelpBox("⚠️ Long timeout may delay error recovery", MessageType.Warning);
                }

                // Instance management info
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Instance Management", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Terminate Delay: {MapNetworkManager.TERMINATE_INSTANCE_DELAY}s", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"User Count Update: {MapNetworkManager.UPDATE_USER_COUNT_DELAY}s", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPlayerDisconnectionSection()
        {
            playerDisconnectionFoldout = EditorGUILayout.Foldout(playerDisconnectionFoldout, "👤 Player Disconnection");
            if (playerDisconnectionFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure how player characters are handled when clients disconnect.", MessageType.Info);

                EditorGUILayout.LabelField("Character Despawning", EditorStyles.boldLabel);
                SafePropertyField("playerCharacterDespawnMillisecondsDelay", "Delay in milliseconds before despawning disconnected player characters");

                int delay = serializedObject.FindProperty("playerCharacterDespawnMillisecondsDelay").intValue;
                float delaySeconds = delay / 1000f;

                EditorGUILayout.LabelField($"Delay: {delaySeconds:F1} seconds", EditorStyles.miniLabel);

                if (delay < 1000)
                {
                    EditorGUILayout.HelpBox("⚠️ Very short delay may cause character loss during temporary disconnects", MessageType.Warning);
                }
                else if (delay > 60000)
                {
                    EditorGUILayout.HelpBox("⚠️ Long delay keeps characters active unnecessarily", MessageType.Warning);
                }

                // Recommended ranges
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Recommended Ranges:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Development: 1000-5000ms (1-5 seconds)", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• Production: 5000-15000ms (5-15 seconds)", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("• High-Reliability: 10000-30000ms (10-30 seconds)", EditorStyles.miniLabel);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawServerDiagnostics()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🔍 Server Diagnostics", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

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

            // Server type summary
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Server Summary:", EditorStyles.miniLabel);

            string serverType = mapManager.IsAllocate ? "Allocate Server" :
                              !string.IsNullOrEmpty(mapManager.MapInstanceId) ? "Instance Server" : "Standard Server";
            EditorGUILayout.LabelField($"• Type: {serverType}", EditorStyles.miniLabel);

            string clusterAddr = serializedObject.FindProperty("clusterServerAddress").stringValue;
            int clusterPort = serializedObject.FindProperty("clusterServerPort").intValue;
            EditorGUILayout.LabelField($"• Cluster: {clusterAddr}:{clusterPort}", EditorStyles.miniLabel);

            string publicAddr = serializedObject.FindProperty("publicAddress").stringValue;
            int networkPort = serializedObject.FindProperty("networkPort").intValue;
            EditorGUILayout.LabelField($"• Public: {publicAddr}:{networkPort}", EditorStyles.miniLabel);

            // Quick actions
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🔍 Validate Config", GUILayout.Height(25)))
            {
                ValidateConfiguration();
                Debug.Log("MapNetworkManager configuration validated");
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

            // Address validation
            string clusterAddr = serializedObject.FindProperty("clusterServerAddress").stringValue;
            string publicAddr = serializedObject.FindProperty("publicAddress").stringValue;

            if (string.IsNullOrEmpty(clusterAddr))
            {
                Debug.LogWarning("Cluster server address is not configured");
                isValid = false;
            }

            if (string.IsNullOrEmpty(publicAddr))
            {
                Debug.LogWarning("Public server address is not configured");
                isValid = false;
            }

            // Port validation
            int clusterPort = serializedObject.FindProperty("clusterServerPort").intValue;
            int networkPort = serializedObject.FindProperty("networkPort").intValue;

            if (clusterPort < 1024 || clusterPort > 65535)
            {
                Debug.LogWarning($"Invalid cluster port: {clusterPort} (should be 1024-65535)");
                isValid = false;
            }

            if (networkPort < 1024 || networkPort > 65535)
            {
                Debug.LogWarning($"Invalid network port: {networkPort} (should be 1024-65535)");
                isValid = false;
            }

            // Timeout validation
            int spawnTimeout = serializedObject.FindProperty("mapSpawnMillisecondsTimeout").intValue;
            int despawnDelay = serializedObject.FindProperty("playerCharacterDespawnMillisecondsDelay").intValue;

            if (spawnTimeout < 0)
            {
                Debug.LogWarning("Spawn timeout cannot be negative");
                isValid = false;
            }

            if (despawnDelay < 0)
            {
                Debug.LogWarning("Despawn delay cannot be negative");
                isValid = false;
            }

            return isValid;
        }

        private void GenerateConfigurationReport()
        {
            Debug.Log("📋 MapNetworkManager Configuration Report");
            Debug.Log("==========================================");

            string clusterAddr = serializedObject.FindProperty("clusterServerAddress").stringValue;
            int clusterPort = serializedObject.FindProperty("clusterServerPort").intValue;
            Debug.Log($"Cluster Server: {clusterAddr}:{clusterPort}");

            string publicAddr = serializedObject.FindProperty("publicAddress").stringValue;
            int networkPort = serializedObject.FindProperty("networkPort").intValue;
            Debug.Log($"Public Address: {publicAddr}:{networkPort}");

            string serverType = mapManager.IsAllocate ? "Allocate Server" :
                              !string.IsNullOrEmpty(mapManager.MapInstanceId) ? "Instance Server" : "Standard Server";
            Debug.Log($"Server Type: {serverType}");

            if (!string.IsNullOrEmpty(mapManager.MapInstanceId))
            {
                Debug.Log($"Instance ID: {mapManager.MapInstanceId}");
                Debug.Log($"Warp Position: {mapManager.MapInstanceWarpToPosition}");
            }

            int spawnTimeout = serializedObject.FindProperty("mapSpawnMillisecondsTimeout").intValue;
            Debug.Log($"Spawn Timeout: {spawnTimeout}ms");

            int despawnDelay = serializedObject.FindProperty("playerCharacterDespawnMillisecondsDelay").intValue;
            Debug.Log($"Player Despawn Delay: {despawnDelay}ms ({despawnDelay/1000f:F1}s)");

            Debug.Log($"Terminate Instance Delay: {MapNetworkManager.TERMINATE_INSTANCE_DELAY}s");
            Debug.Log($"Update User Count Delay: {MapNetworkManager.UPDATE_USER_COUNT_DELAY}s");

            Debug.Log("==========================================");
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

        private void OpenMapNetworkManagerDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "MapNetworkManager.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened MapNetworkManager documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/MapNetworkManager.md");
                Debug.LogWarning($"📖 Local documentation not found, opening web version");
            }
        }
    }
}