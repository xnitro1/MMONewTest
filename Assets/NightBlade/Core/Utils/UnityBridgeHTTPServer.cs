using UnityEngine;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// HTTP Server for Unity Bridge MCP Integration
    /// Receives commands from the MCP server and executes them in Unity
    /// THIS IS THE KEY TO INSTANT CLINE-UNITY COMMUNICATION!
    /// </summary>
    public class UnityBridgeHTTPServer : MonoBehaviour
    {
        private static UnityBridgeHTTPServer instance;
        public static UnityBridgeHTTPServer Instance => instance;

        [Header("Server Settings")]
        [SerializeField] private int port = 8765;
        [SerializeField] private bool enableServer = true;
        [SerializeField] private bool logRequests = true;

        private HttpListener httpListener;
        private Thread listenerThread;
        private Queue<Action> mainThreadQueue = new Queue<Action>();
        private bool isRunning = false;

        private void Awake()
        {
            // Singleton - persist across scenes
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (enableServer)
            {
                StartServer();
            }
        }

        private void Update()
        {
            // Execute queued actions on main thread
            lock (mainThreadQueue)
            {
                while (mainThreadQueue.Count > 0)
                {
                    mainThreadQueue.Dequeue()?.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            StopServer();
        }

        private void OnApplicationQuit()
        {
            StopServer();
        }

        private void StartServer()
        {
            try
            {
                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://localhost:{port}/");
                httpListener.Start();
                isRunning = true;

                listenerThread = new Thread(ListenForRequests);
                listenerThread.IsBackground = true;
                listenerThread.Start();

                Debug.Log($"[UnityBridgeHTTPServer] 🚀 HTTP Server started on port {port}");
                Debug.Log($"[UnityBridgeHTTPServer] MCP can now control Unity in real-time!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityBridgeHTTPServer] Failed to start server: {ex.Message}");
            }
        }

        private void StopServer()
        {
            isRunning = false;

            if (httpListener != null && httpListener.IsListening)
            {
                httpListener.Stop();
                httpListener.Close();
            }

            if (listenerThread != null && listenerThread.IsAlive)
            {
                listenerThread.Abort();
            }

            Debug.Log("[UnityBridgeHTTPServer] Server stopped");
        }

        private void ListenForRequests()
        {
            while (isRunning)
            {
                try
                {
                    if (httpListener != null && httpListener.IsListening)
                    {
                        var context = httpListener.GetContext();
                        ProcessRequest(context);
                    }
                }
                catch (ThreadAbortException)
                {
                    // Thread is being stopped, exit gracefully
                    break;
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        Debug.LogWarning($"[UnityBridgeHTTPServer] Request error: {ex.Message}");
                    }
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                // CORS headers
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                // Handle OPTIONS (preflight)
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                // Handle POST /execute
                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/execute")
                {
                    // Read request body
                    string requestBody;
                    using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        requestBody = reader.ReadToEnd();
                    }

                    if (logRequests)
                    {
                        Debug.Log($"[UnityBridgeHTTPServer] 📥 Received: {requestBody}");
                    }

                    // Parse command
                    var command = JsonConvert.DeserializeObject<BridgeCommand>(requestBody);

                    // Execute on main thread and wait for result
                    string resultJson = null;
                    var resetEvent = new ManualResetEvent(false);

                    lock (mainThreadQueue)
                    {
                        mainThreadQueue.Enqueue(() =>
                        {
                            try
                            {
                                var result = ExecuteCommand(command);
                                resultJson = JsonConvert.SerializeObject(result);
                                resetEvent.Set();
                            }
                            catch (Exception ex)
                            {
                                var errorResult = new UnityCommandResult
                                {
                                    success = false,
                                    message = $"Execution error: {ex.Message}",
                                    error = ex.ToString()
                                };
                                resultJson = JsonConvert.SerializeObject(errorResult);
                                resetEvent.Set();
                            }
                        });
                    }

                    // Wait for main thread to complete (with timeout)
                    if (!resetEvent.WaitOne(5000))
                    {
                        resultJson = JsonConvert.SerializeObject(new UnityCommandResult
                        {
                            success = false,
                            message = "Command execution timeout"
                        });
                    }

                    // Send response
                    byte[] buffer = Encoding.UTF8.GetBytes(resultJson);
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = 200;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.Close();

                    if (logRequests)
                    {
                        Debug.Log($"[UnityBridgeHTTPServer] 📤 Sent: {resultJson}");
                    }
                }
                // Handle GET /status (health check)
                else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/status")
                {
                    var status = new
                    {
                        status = "online",
                        unity_version = Application.unityVersion,
                        scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    string statusJson = JsonConvert.SerializeObject(status);
                    byte[] buffer = Encoding.UTF8.GetBytes(statusJson);
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = 200;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.Close();
                }
                else
                {
                    response.StatusCode = 404;
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityBridgeHTTPServer] Error processing request: {ex.Message}");
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch (Exception closeEx)
                {
                    // Ignore errors when closing response - connection may already be closed
                    Debug.LogWarning($"[UnityBridgeHTTPServer] Failed to close response: {closeEx.Message}");
                }
            }
        }

        private UnityCommandResult ExecuteCommand(BridgeCommand cmd)
        {
            try
            {
                // Execute commands directly (UnityBridge.Instance not needed for HTTP server)
                switch (cmd.type)
                {
                    case "Ping":
                        return new UnityCommandResult
                        {
                            success = true,
                            message = "Pong! Unity is connected via MCP!",
                            data = new Dictionary<string, object>
                            {
                                { "unity_version", Application.unityVersion },
                                { "scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name },
                                { "mcp", true }
                            }
                        };

                    case "GetSceneInfo":
                        return GetSceneInfo();

                    case "FindGameObject":
                        return FindGameObject(cmd.parameters);

                    case "GetComponent":
                        return GetComponent(cmd.parameters);

                    case "SetComponentValue":
                        return SetComponentValue(cmd.parameters);

                    case "Log":
                        string message = cmd.parameters?.GetValueOrDefault("message")?.ToString() ?? "No message";
                        Debug.Log($"[MCP] {message}");
                        return new UnityCommandResult
                        {
                            success = true,
                            message = "Logged to Unity Console"
                        };

                    default:
                        return new UnityCommandResult
                        {
                            success = false,
                            message = $"Unknown command type: {cmd.type}"
                        };
                }
            }
            catch (Exception ex)
            {
                return new UnityCommandResult
                {
                    success = false,
                    message = $"Command execution failed: {ex.Message}",
                    error = ex.ToString()
                };
            }
        }

        private UnityCommandResult GetSceneInfo()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();

            return new UnityCommandResult
            {
                success = true,
                message = $"Scene: {scene.name}",
                data = new Dictionary<string, object>
                {
                    { "sceneName", scene.name },
                    { "scenePath", scene.path },
                    { "rootObjectCount", rootObjects.Length },
                    { "rootObjects", Array.ConvertAll(rootObjects, go => go.name) }
                }
            };
        }

        private UnityCommandResult FindGameObject(Dictionary<string, object> parameters)
        {
            string name = parameters?.GetValueOrDefault("name")?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return new UnityCommandResult { success = false, message = "Missing 'name' parameter" };
            }

            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                return new UnityCommandResult { success = false, message = $"GameObject '{name}' not found" };
            }

            return new UnityCommandResult
            {
                success = true,
                message = $"Found: {go.name}",
                data = new Dictionary<string, object>
                {
                    { "name", go.name },
                    { "active", go.activeInHierarchy },
                    { "position", go.transform.position.ToString() },
                    { "components", Array.ConvertAll(go.GetComponents<Component>(), c => c.GetType().Name) }
                }
            };
        }

        private UnityCommandResult GetComponent(Dictionary<string, object> parameters)
        {
            string objectName = parameters?.GetValueOrDefault("object")?.ToString();
            string componentType = parameters?.GetValueOrDefault("component")?.ToString();

            GameObject go = GameObject.Find(objectName);
            if (go == null)
            {
                return new UnityCommandResult { success = false, message = $"GameObject '{objectName}' not found" };
            }

            Component comp = go.GetComponent(componentType);
            if (comp == null)
            {
                return new UnityCommandResult { success = false, message = $"Component '{componentType}' not found" };
            }

            var componentData = GetComponentData(comp);

            return new UnityCommandResult
            {
                success = true,
                message = "Component data retrieved",
                data = componentData
            };
        }

        private UnityCommandResult SetComponentValue(Dictionary<string, object> parameters)
        {
            string objectName = parameters?.GetValueOrDefault("object")?.ToString();
            string componentType = parameters?.GetValueOrDefault("component")?.ToString();
            string fieldName = parameters?.GetValueOrDefault("field")?.ToString();
            object value = parameters?.GetValueOrDefault("value");

            GameObject go = GameObject.Find(objectName);
            if (go == null)
            {
                return new UnityCommandResult { success = false, message = $"GameObject '{objectName}' not found" };
            }

            Component comp = go.GetComponent(componentType);
            if (comp == null)
            {
                return new UnityCommandResult { success = false, message = $"Component '{componentType}' not found" };
            }

            // Use reflection to set value
            var field = comp.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                object convertedValue = ConvertValueToType(value, field.FieldType);
                field.SetValue(comp, convertedValue);
                return new UnityCommandResult
                {
                    success = true,
                    message = $"Set {fieldName} = {convertedValue}"
                };
            }

            // Try property
            var property = comp.GetType().GetProperty(fieldName);
            if (property != null && property.CanWrite)
            {
                object convertedValue = ConvertValueToType(value, property.PropertyType);
                property.SetValue(comp, convertedValue);
                return new UnityCommandResult
                {
                    success = true,
                    message = $"Set {fieldName} = {convertedValue}"
                };
            }

            return new UnityCommandResult
            {
                success = false,
                message = $"Field/Property '{fieldName}' not found or not writable"
            };
        }

        private Dictionary<string, object> GetComponentData(Component comp)
        {
            var data = new Dictionary<string, object>();

            var fields = comp.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                try
                {
                    var value = field.GetValue(comp);
                    data[field.Name] = value?.ToString() ?? "null";
                }
                catch { }
            }

            return data;
        }

        private object ConvertValueToType(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsInstanceOfType(value)) return value;

            // Handle JSON objects
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

            // Numeric conversions
            if (targetType == typeof(float)) return Convert.ToSingle(value);
            if (targetType == typeof(int)) return Convert.ToInt32(value);
            if (targetType == typeof(bool)) return Convert.ToBoolean(value);
            if (targetType == typeof(string)) return value.ToString();

            return Convert.ChangeType(value, targetType);
        }
    }

    [Serializable]
    public class UnityCommandResult
    {
        public bool success;
        public string message;
        public Dictionary<string, object> data;
        public string error;
    }
}
