using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NightBlade.Tools.Editor
{
    /// <summary>
    /// Editor Bridge - Works in EDIT MODE (not just Play mode!)
    /// Same JSON interface as UnityBridge, but for scene editing.
    /// </summary>
    [InitializeOnLoad]
    public static class UnityEditorBridge
    {
        private static string commandsFilePath;
        private static string resultsFilePath;
        private static string logsFilePath;
        private static double lastCheckTime;
        private static readonly double checkInterval = 0.5; // Check every 0.5 seconds
        private static bool isEnabled = true;

        static UnityEditorBridge()
        {
            // Setup file paths
            commandsFilePath = Path.Combine(Application.dataPath, "..", "unity_bridge_commands.json");
            resultsFilePath = Path.Combine(Application.dataPath, "..", "unity_bridge_results.json");
            logsFilePath = Path.Combine(Application.dataPath, "..", "unity_bridge_logs.txt");

            // Hook into Editor update
            EditorApplication.update += OnEditorUpdate;

            // Initialize
            InitializeBridge();
            Log("🌉 Unity Editor Bridge Initialized! (Edit Mode)");
        }

        private static void OnEditorUpdate()
        {
            if (!isEnabled || EditorApplication.isPlaying)
                return; // Let UnityBridge handle Play mode

            // Check for commands at interval
            if (EditorApplication.timeSinceStartup - lastCheckTime > checkInterval)
            {
                lastCheckTime = EditorApplication.timeSinceStartup;
                CheckForCommands();
            }
        }

        private static void InitializeBridge()
        {
            // Create empty command file if it doesn't exist
            if (!File.Exists(commandsFilePath))
            {
                File.WriteAllText(commandsFilePath, "");
            }

            // Clear results file
            File.WriteAllText(resultsFilePath, "{}");

            // Initialize log file
            if (!File.Exists(logsFilePath))
            {
                File.WriteAllText(logsFilePath, "=== Unity Editor Bridge Logs ===\n" + System.DateTime.Now + "\n\n");
            }
        }

        private static void CheckForCommands()
        {
            if (!File.Exists(commandsFilePath))
                return;

            string json = File.ReadAllText(commandsFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return;

            try
            {
                var wrapper = JsonConvert.DeserializeObject<CommandWrapper>(json);
                if (wrapper?.commands != null && wrapper.commands.Count > 0)
                {
                    // Process each command
                    foreach (var cmd in wrapper.commands)
                    {
                        Log($"📥 Executing: {cmd.type} (ID: {cmd.id})");
                        var result = ExecuteCommand(cmd);
                        WriteResult(result);
                    }

                    // Clear command file after processing
                    File.WriteAllText(commandsFilePath, "");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EditorBridge] Error processing commands: {ex.Message}");
            }
        }

        private static BridgeResult ExecuteCommand(BridgeCommand cmd)
        {
            try
            {
                switch (cmd.type)
                {
                    case "Ping":
                        return new BridgeResult { commandId = cmd.id, success = true, message = "Pong! (Edit Mode)" };

                    case "GetSceneInfo":
                        return GetSceneInfoCommand(cmd);

                    case "FindGameObject":
                        return FindGameObjectCommand(cmd);

                    case "MoveGameObject":
                        return MoveGameObjectCommand(cmd);

                    case "SetActive":
                        return SetActiveCommand(cmd);

                    case "GetComponentValue":
                        return GetComponentValueCommand(cmd);

                    case "SetComponentValue":
                        return SetComponentValueCommand(cmd);

                    case "SetPosition":
                        return SetPositionCommand(cmd);

                    case "GetChildren":
                        return GetChildrenCommand(cmd);

                    case "GetHierarchy":
                        return GetHierarchyCommand(cmd);

                    case "Log":
                        return LogCommand(cmd);

                    default:
                        return new BridgeResult
                        {
                            commandId = cmd.id,
                            success = false,
                            message = $"Unknown command: {cmd.type}"
                        };
                }
            }
            catch (System.Exception ex)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"Error: {ex.Message}",
                    error = ex.ToString()
                };
            }
        }

        #region Commands

        private static BridgeResult GetSceneInfoCommand(BridgeCommand cmd)
        {
            // Check if we're in Prefab Mode
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var prefabRoot = prefabStage.prefabContentsRoot;
                List<string> childNames = new List<string>();
                foreach (Transform child in prefabRoot.transform)
                {
                    childNames.Add(child.name);
                }

                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = true,
                    message = $"Prefab Mode: {prefabRoot.name}",
                    data = new Dictionary<string, object>
                    {
                        { "mode", "prefab" },
                        { "prefabName", prefabRoot.name },
                        { "prefabPath", prefabStage.assetPath },
                        { "rootObjectCount", 1 },
                        { "rootObjects", new string[] { prefabRoot.name } },
                        { "childCount", childNames.Count },
                        { "children", childNames.ToArray() }
                    }
                };
            }

            // Regular scene mode
            var scene = EditorSceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();

            return new BridgeResult
            {
                commandId = cmd.id,
                success = true,
                message = $"Scene: {scene.name}",
                data = new Dictionary<string, object>
                {
                    { "mode", "scene" },
                    { "sceneName", scene.name },
                    { "scenePath", scene.path },
                    { "rootObjectCount", rootObjects.Length },
                    { "rootObjects", System.Array.ConvertAll(rootObjects, go => go.name) }
                }
            };
        }

        private static BridgeResult FindGameObjectCommand(BridgeCommand cmd)
        {
            string objectName = cmd.parameters?.GetValueOrDefault("object")?.ToString() 
                ?? cmd.parameters?.GetValueOrDefault("name")?.ToString();

            GameObject go = FindGameObject(objectName);
            if (go == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"GameObject '{objectName}' not found"
                };
            }

            var components = go.GetComponents<Component>();
            return new BridgeResult
            {
                commandId = cmd.id,
                success = true,
                message = $"Found: {objectName}",
                data = new Dictionary<string, object>
                {
                    { "name", go.name },
                    { "active", go.activeSelf },
                    { "position", go.transform.position.ToString("F2") },
                    { "components", System.Array.ConvertAll(components, c => c.GetType().Name) }
                }
            };
        }

        // Helper to find GameObjects in both scene and prefab mode
        private static GameObject FindGameObject(string name)
        {
            // Check Prefab Mode first
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var prefabRoot = prefabStage.prefabContentsRoot;
                
                // Check if searching for root
                if (prefabRoot.name == name)
                    return prefabRoot;
                
                // Search in children
                return FindInChildren(prefabRoot.transform, name);
            }

            // Regular scene search
            return GameObject.Find(name);
        }

        private static GameObject FindInChildren(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child.gameObject;
                
                var found = FindInChildren(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static BridgeResult MoveGameObjectCommand(BridgeCommand cmd)
        {
            string objectName = cmd.parameters?.GetValueOrDefault("object")?.ToString();
            float x = System.Convert.ToSingle(cmd.parameters?.GetValueOrDefault("x") ?? 0);
            float y = System.Convert.ToSingle(cmd.parameters?.GetValueOrDefault("y") ?? 0);
            float z = System.Convert.ToSingle(cmd.parameters?.GetValueOrDefault("z") ?? 0);

            GameObject go = FindGameObject(objectName);
            if (go == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"GameObject '{objectName}' not found"
                };
            }

            Vector3 oldPos = go.transform.position;
            Vector3 newPos = new Vector3(x, y, z);

            // Use Undo system so changes can be undone
            Undo.RecordObject(go.transform, "Move GameObject via AI");
            go.transform.position = newPos;

            // Mark scene dirty
            EditorUtility.SetDirty(go);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);

            return new BridgeResult
            {
                commandId = cmd.id,
                success = true,
                message = $"Moved '{objectName}' from {oldPos} to {newPos} (PERMANENT)",
                data = new Dictionary<string, object>
                {
                    { "oldPosition", oldPos.ToString() },
                    { "newPosition", newPos.ToString() }
                }
            };
        }

        private static BridgeResult SetActiveCommand(BridgeCommand cmd)
        {
            string objectName = cmd.parameters?.GetValueOrDefault("object")?.ToString();
            bool active = cmd.parameters?.GetValueOrDefault("active")?.ToString() == "true";

            GameObject go = FindGameObject(objectName);
            if (go == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"GameObject '{objectName}' not found"
                };
            }

            Undo.RecordObject(go, "Set Active via AI");
            go.SetActive(active);
            EditorUtility.SetDirty(go);

            return new BridgeResult
            {
                commandId = cmd.id,
                success = true,
                message = $"Set '{objectName}' active = {active} (PERMANENT)"
            };
        }

        private static BridgeResult GetComponentValueCommand(BridgeCommand cmd)
        {
            string objectName = cmd.parameters?.GetValueOrDefault("object")?.ToString();
            string componentType = cmd.parameters?.GetValueOrDefault("component")?.ToString();
            
            GameObject go = FindGameObject(objectName);
            if (go == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"GameObject '{objectName}' not found"
                };
            }
            
            Component comp = go.GetComponent(componentType);
            if (comp == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"Component '{componentType}' not found"
                };
            }
            
            // Read all public fields and properties
            var data = new Dictionary<string, object>();
            var type = comp.GetType();
            
            // Get fields
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                try
                {
                    var value = field.GetValue(comp);
                    data[field.Name] = SerializeValue(value);
                }
                catch { }
            }
            
            // Get properties
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.CanRead)
                {
                    try
                    {
                        var value = prop.GetValue(comp);
                        data[prop.Name] = SerializeValue(value);
                    }
                    catch { }
                }
            }
            
            return new BridgeResult
            {
                commandId = cmd.id,
                success = true,
                message = $"Read {data.Count} properties from {componentType}",
                data = data
            };
        }
        
        private static object SerializeValue(object value)
        {
            if (value == null) return "null";
            if (value is string || value is int || value is float || value is bool) return value;
            if (value is Vector3 v3) return $"({v3.x:F2}, {v3.y:F2}, {v3.z:F2})";
            if (value is Vector2 v2) return $"({v2.x:F2}, {v2.y:F2})";
            if (value is Color c) return $"RGBA({c.r:F2}, {c.g:F2}, {c.b:F2}, {c.a:F2})";
            if (value is System.Enum) return value.ToString();
            return value.ToString();
        }
        
        private static BridgeResult SetComponentValueCommand(BridgeCommand cmd)
        {
            string objectName = cmd.parameters?.GetValueOrDefault("object")?.ToString();
            string componentType = cmd.parameters?.GetValueOrDefault("component")?.ToString();
            string fieldName = cmd.parameters?.GetValueOrDefault("field")?.ToString();
            object value = cmd.parameters?.GetValueOrDefault("value");

            GameObject go = FindGameObject(objectName);
            if (go == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"GameObject '{objectName}' not found"
                };
            }

            Component comp = go.GetComponent(componentType);
            if (comp == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"Component '{componentType}' not found"
                };
            }

            Undo.RecordObject(comp, "Set Component Value via AI");

            // Use reflection to set value
            var field = comp.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                object convertedValue = ConvertValueToType(value, field.FieldType);
                field.SetValue(comp, convertedValue);
                EditorUtility.SetDirty(comp);
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = true,
                    message = $"Set {fieldName} = {convertedValue} (PERMANENT)"
                };
            }

            // Try property
            var property = comp.GetType().GetProperty(fieldName);
            if (property != null && property.CanWrite)
            {
                object convertedValue = ConvertValueToType(value, property.PropertyType);
                property.SetValue(comp, convertedValue);
                EditorUtility.SetDirty(comp);
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = true,
                    message = $"Set {fieldName} = {convertedValue} (PERMANENT)"
                };
            }

            return new BridgeResult
            {
                commandId = cmd.id,
                success = false,
                message = $"Field/Property '{fieldName}' not found or not writable"
            };
        }

        private static BridgeResult SetPositionCommand(BridgeCommand cmd)
        {
            string objectName = cmd.parameters?.GetValueOrDefault("object")?.ToString();
            object positionValue = cmd.parameters?.GetValueOrDefault("position");

            GameObject go = FindGameObject(objectName);
            if (go == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"GameObject '{objectName}' not found"
                };
            }

            Vector3 position = (Vector3)ConvertValueToType(positionValue, typeof(Vector3));
            Undo.RecordObject(go.transform, "Set Position via AI");
            go.transform.position = position;
            EditorUtility.SetDirty(go);

            return new BridgeResult
            {
                commandId = cmd.id,
                success = true,
                message = $"Moved '{objectName}' to {position} (PERMANENT)",
                data = new Dictionary<string, object>
                {
                    { "newPosition", position.ToString() }
                }
            };
        }

        private static BridgeResult GetChildrenCommand(BridgeCommand cmd)
        {
            string objectName = cmd.parameters?.GetValueOrDefault("object")?.ToString()
                ?? cmd.parameters?.GetValueOrDefault("name")?.ToString();

            GameObject go = FindGameObject(objectName);
            if (go == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"GameObject '{objectName}' not found"
                };
            }

            List<string> childNames = new List<string>();
            foreach (Transform child in go.transform)
            {
                childNames.Add(child.name);
            }

            return new BridgeResult
            {
                commandId = cmd.id,
                success = true,
                message = $"Found {childNames.Count} children",
                data = new Dictionary<string, object>
                {
                    { "parent", go.name },
                    { "childCount", childNames.Count },
                    { "children", childNames.ToArray() }
                }
            };
        }

        private static BridgeResult GetHierarchyCommand(BridgeCommand cmd)
        {
            string objectName = cmd.parameters?.GetValueOrDefault("object")?.ToString()
                ?? cmd.parameters?.GetValueOrDefault("name")?.ToString();
            int maxDepth = 2; // Default depth
            if (cmd.parameters?.ContainsKey("depth") == true)
            {
                maxDepth = System.Convert.ToInt32(cmd.parameters["depth"]);
            }

            GameObject go = FindGameObject(objectName);
            if (go == null)
            {
                return new BridgeResult
                {
                    commandId = cmd.id,
                    success = false,
                    message = $"GameObject '{objectName}' not found"
                };
            }

            var hierarchy = BuildHierarchy(go.transform, 0, maxDepth);

            return new BridgeResult
            {
                commandId = cmd.id,
                success = true,
                message = $"Hierarchy for '{objectName}'",
                data = hierarchy
            };
        }

        private static Dictionary<string, object> BuildHierarchy(Transform transform, int currentDepth, int maxDepth)
        {
            var data = new Dictionary<string, object>
            {
                { "name", transform.name },
                { "active", transform.gameObject.activeSelf },
                { "childCount", transform.childCount }
            };

            if (currentDepth < maxDepth && transform.childCount > 0)
            {
                List<Dictionary<string, object>> children = new List<Dictionary<string, object>>();
                foreach (Transform child in transform)
                {
                    children.Add(BuildHierarchy(child, currentDepth + 1, maxDepth));
                }
                data["children"] = children.ToArray();
            }

            return data;
        }

        private static BridgeResult LogCommand(BridgeCommand cmd)
        {
            string message = cmd.parameters?.GetValueOrDefault("message")?.ToString() ?? "No message";
            Log($"🔔 External: {message}");

            return new BridgeResult
            {
                commandId = cmd.id,
                success = true,
                message = "Logged"
            };
        }

        #endregion

        #region Helpers

        private static object ConvertValueToType(object value, System.Type targetType)
        {
            if (value == null)
                return null;

            if (targetType.IsInstanceOfType(value))
                return value;

            // Handle Unity types from JSON
            if (value is Newtonsoft.Json.Linq.JObject jobj)
            {
                if (targetType == typeof(Vector3))
                {
                    return new Vector3(
                        jobj["x"]?.ToObject<float>() ?? 0f,
                        jobj["y"]?.ToObject<float>() ?? 0f,
                        jobj["z"]?.ToObject<float>() ?? 0f
                    );
                }

                if (targetType == typeof(Vector2))
                {
                    return new Vector2(
                        jobj["x"]?.ToObject<float>() ?? 0f,
                        jobj["y"]?.ToObject<float>() ?? 0f
                    );
                }

                if (targetType == typeof(Color))
                {
                    return new Color(
                        jobj["r"]?.ToObject<float>() ?? 0f,
                        jobj["g"]?.ToObject<float>() ?? 0f,
                        jobj["b"]?.ToObject<float>() ?? 0f,
                        jobj["a"]?.ToObject<float>() ?? 1f
                    );
                }
            }

            // Handle enum conversions
            if (targetType.IsEnum)
            {
                try
                {
                    if (value is string s)
                        return System.Enum.Parse(targetType, s);
                    if (value is int i)
                        return System.Enum.ToObject(targetType, i);
                    if (value is long l)
                        return System.Enum.ToObject(targetType, (int)l);
                }
                catch { }
            }
            
            // Handle numeric conversions
            try
            {
                if (targetType == typeof(float))
                    return System.Convert.ToSingle(value);
                if (targetType == typeof(int))
                    return System.Convert.ToInt32(value);
                if (targetType == typeof(bool))
                    return System.Convert.ToBoolean(value);
                if (targetType == typeof(string))
                    return value.ToString();
            }
            catch { }

            return System.Convert.ChangeType(value, targetType);
        }

        private static void WriteResult(BridgeResult result)
        {
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(resultsFilePath, json);
        }

        private static void Log(string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            string logMessage = $"[{timestamp}] {message}\n";

            Debug.Log($"[EditorBridge] {message}");

            try
            {
                File.AppendAllText(logsFilePath, logMessage);
            }
            catch { }
        }

        #endregion

        #region Data Classes (matching UnityBridge)

        [System.Serializable]
        private class CommandWrapper
        {
            public List<BridgeCommand> commands;
        }

        [System.Serializable]
        private class BridgeCommand
        {
            public string id;
            public string type;
            public Dictionary<string, object> parameters;
        }

        [System.Serializable]
        private class BridgeResult
        {
            public string commandId;
            public bool success;
            public string message;
            public string timestamp;
            public Dictionary<string, object> data;
            public string error;
        }

        #endregion
    }
}
