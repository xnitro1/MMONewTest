using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NightBlade.Core.Utils.Editor
{
    /// <summary>
    /// AI Assistant Window - Chat with Cline/Claude directly through Unity Bridge! 🤖
    /// 
    /// Communicates via file system - no API keys needed!
    /// Messages are exchanged through ai_chat_messages.json
    /// 
    /// Access via: NightBlade → AI Features → AI Assistant
    /// </summary>
    public class AIAssistantWindow : EditorWindow
    {
        #region Window Setup
        
        [MenuItem("NightBlade/AI Features/AI Assistant")]
        public static void ShowWindow()
        {
            var window = GetWindow<AIAssistantWindow>("AI Assistant");
            window.minSize = new Vector2(400, 500);
        }
        
        #endregion
        
        #region Settings
        
        private string chatFilePath;
        private double lastCheckTime;
        private readonly double checkInterval = 0.5; // Check every 0.5 seconds
        
        #endregion
        
        #region State
        
        private List<ChatMessage> chatHistory = new List<ChatMessage>();
        private string userInput = "";
        private Vector2 scrollPosition;
        private bool isWaitingForResponse = false;
        
        private GUIStyle messageStyle;
        private GUIStyle userMessageStyle;
        private GUIStyle assistantMessageStyle;
        private bool stylesInitialized = false;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void OnEnable()
        {
            chatFilePath = Path.Combine(Application.dataPath, "..", "ai_chat_messages.json");
            
            // Load chat history
            LoadChatHistory();
            
            // Add welcome message if empty
            if (chatHistory.Count == 0)
            {
                chatHistory.Add(new ChatMessage
                {
                    role = "assistant",
                    content = "👋 Hi! I'm Claude (Cline), and I'm connected through the Unity Bridge! Ask me anything about Unity development, and I'll respond directly!"
                });
                SaveChatHistory();
            }
            
            EditorApplication.update += OnEditorUpdate;
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            SaveChatHistory();
        }
        
        private void OnEditorUpdate()
        {
            // Check for new messages from Cline
            if (EditorApplication.timeSinceStartup - lastCheckTime > checkInterval)
            {
                lastCheckTime = EditorApplication.timeSinceStartup;
                CheckForResponses();
            }
        }
        
        #endregion
        
        #region GUI
        
        private void OnGUI()
        {
            InitializeStyles();
            DrawHeader();
            EditorGUILayout.Space(5);
            DrawChatArea();
            EditorGUILayout.Space(5);
            DrawInputArea();
        }
        
        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            messageStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 2, 2),
                wordWrap = true,
                richText = true
            };
            
            userMessageStyle = new GUIStyle(messageStyle);
            userMessageStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.5f, 0.8f, 0.3f));
            
            assistantMessageStyle = new GUIStyle(messageStyle);
            assistantMessageStyle.normal.background = MakeTex(2, 2, new Color(0.3f, 0.8f, 0.5f, 0.3f));
            
            stylesInitialized = true;
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("🤖 AI Assistant (via Unity Bridge)", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("🔄 Check for Response", EditorStyles.toolbarButton, GUILayout.Width(140)))
            {
                LoadChatHistory();
                isWaitingForResponse = false;
                Repaint();
            }
            
            if (GUILayout.Button("Clear Chat", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ClearChat();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawChatArea()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            
            foreach (var message in chatHistory)
            {
                GUIStyle style = message.role == "user" ? userMessageStyle : assistantMessageStyle;
                string prefix = message.role == "user" ? "You: " : "Claude: ";
                
                EditorGUILayout.BeginVertical(style);
                EditorGUILayout.LabelField(prefix + message.content, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();
                GUILayout.Space(2);
            }
            
            if (isWaitingForResponse)
            {
                EditorGUILayout.BeginVertical(assistantMessageStyle);
                EditorGUILayout.LabelField("Claude: Thinking...", EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawInputArea()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Your Message:", EditorStyles.boldLabel);
            userInput = EditorGUILayout.TextArea(userInput, GUILayout.Height(80));
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.enabled = !isWaitingForResponse && !string.IsNullOrWhiteSpace(userInput);
            
            if (GUILayout.Button("Send", GUILayout.Width(100), GUILayout.Height(30)))
            {
                SendMessage();
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            // Show waiting indicator with helpful instructions
            if (isWaitingForResponse)
            {
                EditorGUILayout.HelpBox("⏳ Message sent! Copy and paste this into your main Cline chat window:\n\n\"Check Unity chat messages in ai_chat_messages.json and respond\"", MessageType.Warning);
                
                if (GUILayout.Button("📋 Copy Ping Message to Clipboard", GUILayout.Height(30)))
                {
                    EditorGUIUtility.systemCopyBuffer = "Check Unity chat messages in ai_chat_messages.json and respond";
                    Debug.Log("✅ Copied ping message to clipboard! Paste it in your Cline chat window.");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("💡 Connected to Cline via Unity Bridge. After sending, copy the ping message and paste it in the main Cline window.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        #endregion
        
        #region Chat Logic
        
        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(userInput)) return;
            
            // Add user message to history
            chatHistory.Add(new ChatMessage
            {
                role = "user",
                content = userInput.Trim(),
                timestamp = System.DateTime.Now.ToString("HH:mm:ss")
            });
            
            // Save to file for Cline to read
            SaveChatHistory();
            
            // Clear input
            userInput = "";
            isWaitingForResponse = true;
            
            // Scroll to bottom
            scrollPosition.y = float.MaxValue;
            Repaint();
        }
        
        private void CheckForResponses()
        {
            if (!isWaitingForResponse) return;
            
            LoadChatHistory();
            
            // Check if last message is from assistant
            if (chatHistory.Count > 0 && chatHistory[chatHistory.Count - 1].role == "assistant")
            {
                isWaitingForResponse = false;
                scrollPosition.y = float.MaxValue;
                Repaint();
            }
        }
        
        private void ClearChat()
        {
            if (EditorUtility.DisplayDialog("Clear Chat", "Are you sure you want to clear the chat history?", "Clear", "Cancel"))
            {
                chatHistory.Clear();
                chatHistory.Add(new ChatMessage
                {
                    role = "assistant",
                    content = "👋 Chat cleared! How can I help you?"
                });
                SaveChatHistory();
                Repaint();
            }
        }
        
        #endregion
        
        #region Persistence
        
        private void SaveChatHistory()
        {
            try
            {
                var data = new ChatData { messages = chatHistory };
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(chatFilePath, json);
                
                // Send bridge command to notify Cline of new message
                SendBridgeNotification();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save chat: {ex.Message}");
            }
        }
        
        private void SendBridgeNotification()
        {
            try
            {
                string commandsPath = Path.Combine(Application.dataPath, "..", "unity_bridge_commands.json");
                
                var command = new {
                    commands = new[] {
                        new {
                            id = "chat_notification_" + System.DateTime.Now.Ticks,
                            type = "Log",
                            parameters = new {
                                message = "💬 NEW CHAT MESSAGE! Check ai_chat_messages.json for user's question!"
                            }
                        }
                    }
                };
                
                string json = JsonConvert.SerializeObject(command, Formatting.Indented);
                File.WriteAllText(commandsPath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to send bridge notification: {ex.Message}");
            }
        }
        
        private void LoadChatHistory()
        {
            try
            {
                if (!File.Exists(chatFilePath))
                    return;
                
                string json = File.ReadAllText(chatFilePath);
                if (string.IsNullOrWhiteSpace(json))
                    return;
                
                var data = JsonConvert.DeserializeObject<ChatData>(json);
                if (data?.messages != null)
                {
                    chatHistory = data.messages;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to load chat: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Helpers
        
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class ChatMessage
    {
        public string role; // "user" or "assistant"
        public string content;
        public string timestamp;
    }
    
    [System.Serializable]
    public class ChatData
    {
        public List<ChatMessage> messages;
    }
    
    #endregion
}
