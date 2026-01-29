#if UNITY_EDITOR
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

#if NET || NETCOREAPP
using Microsoft.Data.Sqlite;
#else
using Mono.Data.Sqlite;
#endif

namespace Synthesis
{
    /// <summary>
    /// SQLite-based knowledge base for Synthesis commands, workflows, examples, and FAQs
    /// </summary>
    public class SynthesisKnowledgeBase
    {
        private SqliteConnection _connection;
        private string _dbPath;
        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Initialize the knowledge base with the specified database path
        /// </summary>
        public void Initialize(string dbPath = null)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[Synthesis KB] Already initialized");
                return;
            }

            // Default path in project root
            if (string.IsNullOrEmpty(dbPath))
            {
                _dbPath = Path.Combine(Application.dataPath, "..", "synthesis_knowledge.db");
            }
            else
            {
                _dbPath = dbPath;
            }

            try
            {
                // Create database file if it doesn't exist
                bool isNewDatabase = !File.Exists(_dbPath);
                
                string connectionString = $"URI=file:{_dbPath}";
                _connection = new SqliteConnection(connectionString);
                _connection.Open();

                if (isNewDatabase)
                {
                    Debug.Log($"[Synthesis KB] Creating new database at: {_dbPath}");
                    CreateSchema();
                    PopulateDefaultData();
                }
                else
                {
                    Debug.Log($"[Synthesis KB] Connected to existing database: {_dbPath}");
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Synthesis KB] Failed to initialize database: {ex.Message}");
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Create the database schema
        /// </summary>
        private void CreateSchema()
        {
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS commands (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE,
                    category TEXT NOT NULL,
                    description TEXT NOT NULL,
                    parameters TEXT,
                    example_request TEXT,
                    example_response TEXT,
                    use_cases TEXT,
                    related_commands TEXT,
                    version_added TEXT DEFAULT '1.0.0',
                    common_errors TEXT
                );
            ");

            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS workflows (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL,
                    description TEXT NOT NULL,
                    difficulty TEXT DEFAULT 'Beginner',
                    steps TEXT NOT NULL,
                    commands_used TEXT,
                    estimated_time INTEGER DEFAULT 5,
                    tags TEXT,
                    times_used INTEGER DEFAULT 0,
                    success_rate REAL DEFAULT 1.0
                );
            ");

            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS examples (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL,
                    description TEXT NOT NULL,
                    code TEXT NOT NULL,
                    language TEXT DEFAULT 'json',
                    category TEXT,
                    tags TEXT,
                    difficulty TEXT DEFAULT 'Beginner',
                    upvotes INTEGER DEFAULT 0
                );
            ");

            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS faq (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    question TEXT NOT NULL,
                    answer TEXT NOT NULL,
                    category TEXT DEFAULT 'General',
                    related_commands TEXT,
                    helpful_count INTEGER DEFAULT 0,
                    keywords TEXT
                );
            ");

            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS errors (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    error_code TEXT NOT NULL UNIQUE,
                    error_message TEXT NOT NULL,
                    cause TEXT NOT NULL,
                    solution TEXT NOT NULL,
                    related_command TEXT,
                    severity TEXT DEFAULT 'Error'
                );
            ");

            // Create search indexes
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_commands_name ON commands(name);");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_commands_category ON commands(category);");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_workflows_tags ON workflows(tags);");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_faq_keywords ON faq(keywords);");

            Debug.Log("[Synthesis KB] Schema created successfully");
        }

        /// <summary>
        /// Populate the database with default Synthesis commands and documentation
        /// </summary>
        private void PopulateDefaultData()
        {
            Debug.Log("[Synthesis KB] Populating default data...");

            // Add core commands
            AddCommand("Ping", "System", "Check if bridge is alive and responding",
                "{}",
                @"{""command"":""Ping"",""id"":""health_check""}",
                @"{""id"":""health_check"",""success"":true,""message"":""Pong!""}",
                "Health check, verifying bridge is active, testing connectivity",
                "GetSceneInfo",
                "1.0.0",
                "Bridge not responding - ensure Unity is in Play mode");

            AddCommand("GetSceneInfo", "Scene Management", "Get information about the active scene",
                "{}",
                @"{""command"":""GetSceneInfo"",""id"":""scene_info""}",
                @"{""id"":""scene_info"",""success"":true,""data"":{""sceneName"":""MainScene"",""objectCount"":42}}",
                "Understanding scene structure, getting scene details",
                "GetHierarchy, FindGameObject",
                "1.0.0",
                "No active scene - ensure a scene is loaded");

            AddCommand("FindGameObject", "GameObject Management", "Find a GameObject in the active scene by name",
                @"{""name"":""string (required)""}",
                @"{""command"":""FindGameObject"",""id"":""find_obj"",""parameters"":{""name"":""Player""}}",
                @"{""id"":""find_obj"",""success"":true,""data"":{""objectId"":""12345"",""name"":""Player""}}",
                "Locating objects for manipulation, verifying object exists, getting object references",
                "GetComponent, SetComponentValue, GetChildren",
                "1.0.0",
                "GameObject not found - verify name is exact match including case");

            AddCommand("SetComponentValue", "Component Management", "Modify a field or property on a component",
                @"{""objectId"":""string"",""componentType"":""string"",""field"":""string"",""value"":""any""}",
                @"{""command"":""SetComponentValue"",""parameters"":{""objectId"":""123"",""componentType"":""Transform"",""field"":""position"",""value"":{""x"":10,""y"":5,""z"":0}}}",
                @"{""success"":true,""message"":""Value set successfully""}",
                "Modifying object properties, updating UI elements, changing positions/colors/values",
                "GetComponentValue, FindGameObject",
                "1.0.0",
                "Component not found - verify componentType uses full namespace");

            // Add FAQs
            AddFAQ("Why isn't Synthesis responding to my commands?",
                "Check these common issues:\n1. Is Unity in Play mode? Bridge only works during Play.\n2. Is 'Enable Bridge' checked on the Synthesis component?\n3. Are you waiting at least 0.5 seconds between writing command and checking result?\n4. Check synthesis_logs.txt for error messages.\n5. Verify JSON syntax is correct (use a validator).",
                "Troubleshooting",
                "Ping",
                "not working, not responding, no response, doesn't work, bridge not active");

            AddFAQ("How do I modify UI elements in real-time?",
                "Use FindGameObject to locate the UI element, then SetComponentValue to modify properties like anchoredPosition (RectTransform), color (Image), or text (Text/TextMeshProUGUI). Make sure to use the correct component type with full namespace.",
                "UI Manipulation",
                "FindGameObject, SetComponentValue",
                "ui, modify, change, update, real-time, runtime");

            // Add basic workflow
            AddWorkflow("Move UI Element",
                "Reposition a UI element to new coordinates",
                "Beginner",
                @"[{""step"":1,""description"":""Find the UI element"",""command"":""FindGameObject""},{""step"":2,""description"":""Update position"",""command"":""SetComponentValue""}]",
                "FindGameObject, SetComponentValue",
                2,
                "ui, positioning, rectTransform, beginner");

            Debug.Log("[Synthesis KB] Default data populated successfully");
        }

        #region Public API Methods

        /// <summary>
        /// Search for commands by name or description
        /// </summary>
        public List<CommandEntry> SearchCommands(string searchTerm)
        {
            List<CommandEntry> results = new List<CommandEntry>();
            if (!_isInitialized) return results;

            string query = @"
                SELECT * FROM commands 
                WHERE name LIKE @search 
                   OR description LIKE @search 
                   OR category LIKE @search
                ORDER BY name;
            ";

            using (SqliteCommand cmd = new SqliteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(ReadCommandEntry(reader));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Get all commands in a specific category
        /// </summary>
        public List<CommandEntry> GetCommandsByCategory(string category)
        {
            List<CommandEntry> results = new List<CommandEntry>();
            if (!_isInitialized) return results;

            string query = "SELECT * FROM commands WHERE category = @category ORDER BY name;";

            using (SqliteCommand cmd = new SqliteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@category", category);
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(ReadCommandEntry(reader));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Search workflows by tags
        /// </summary>
        public List<WorkflowEntry> SearchWorkflows(string tag)
        {
            List<WorkflowEntry> results = new List<WorkflowEntry>();
            if (!_isInitialized) return results;

            string query = @"
                SELECT * FROM workflows 
                WHERE tags LIKE @tag 
                   OR title LIKE @tag
                ORDER BY times_used DESC;
            ";

            using (SqliteCommand cmd = new SqliteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@tag", $"%{tag}%");
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(ReadWorkflowEntry(reader));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Search FAQs by keywords
        /// </summary>
        public List<FAQEntry> SearchFAQ(string searchTerm)
        {
            List<FAQEntry> results = new List<FAQEntry>();
            if (!_isInitialized) return results;

            string query = @"
                SELECT * FROM faq 
                WHERE question LIKE @search 
                   OR answer LIKE @search 
                   OR keywords LIKE @search
                ORDER BY helpful_count DESC;
            ";

            using (SqliteCommand cmd = new SqliteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(ReadFAQEntry(reader));
                    }
                }
            }

            return results;
        }

        #endregion

        #region Helper Methods

        private void AddCommand(string name, string category, string description, string parameters,
            string exampleRequest, string exampleResponse, string useCases, string relatedCommands,
            string versionAdded, string commonErrors)
        {
            string query = @"
                INSERT INTO commands (name, category, description, parameters, example_request, 
                    example_response, use_cases, related_commands, version_added, common_errors)
                VALUES (@name, @category, @description, @parameters, @example_request, 
                    @example_response, @use_cases, @related_commands, @version_added, @common_errors);
            ";

            using (SqliteCommand cmd = new SqliteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@category", category);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@parameters", parameters);
                cmd.Parameters.AddWithValue("@example_request", exampleRequest);
                cmd.Parameters.AddWithValue("@example_response", exampleResponse);
                cmd.Parameters.AddWithValue("@use_cases", useCases);
                cmd.Parameters.AddWithValue("@related_commands", relatedCommands);
                cmd.Parameters.AddWithValue("@version_added", versionAdded);
                cmd.Parameters.AddWithValue("@common_errors", commonErrors);
                cmd.ExecuteNonQuery();
            }
        }

        private void AddFAQ(string question, string answer, string category, string relatedCommands, string keywords)
        {
            string query = @"
                INSERT INTO faq (question, answer, category, related_commands, keywords)
                VALUES (@question, @answer, @category, @related_commands, @keywords);
            ";

            using (SqliteCommand cmd = new SqliteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@question", question);
                cmd.Parameters.AddWithValue("@answer", answer);
                cmd.Parameters.AddWithValue("@category", category);
                cmd.Parameters.AddWithValue("@related_commands", relatedCommands);
                cmd.Parameters.AddWithValue("@keywords", keywords);
                cmd.ExecuteNonQuery();
            }
        }

        private void AddWorkflow(string title, string description, string difficulty, string steps,
            string commandsUsed, int estimatedTime, string tags)
        {
            string query = @"
                INSERT INTO workflows (title, description, difficulty, steps, commands_used, estimated_time, tags)
                VALUES (@title, @description, @difficulty, @steps, @commands_used, @estimated_time, @tags);
            ";

            using (SqliteCommand cmd = new SqliteCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@difficulty", difficulty);
                cmd.Parameters.AddWithValue("@steps", steps);
                cmd.Parameters.AddWithValue("@commands_used", commandsUsed);
                cmd.Parameters.AddWithValue("@estimated_time", estimatedTime);
                cmd.Parameters.AddWithValue("@tags", tags);
                cmd.ExecuteNonQuery();
            }
        }

        private void ExecuteNonQuery(string sql)
        {
            using (SqliteCommand cmd = new SqliteCommand(sql, _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private CommandEntry ReadCommandEntry(SqliteDataReader reader)
        {
            return new CommandEntry
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Category = reader.GetString(2),
                Description = reader.GetString(3),
                Parameters = reader.IsDBNull(4) ? null : reader.GetString(4),
                ExampleRequest = reader.IsDBNull(5) ? null : reader.GetString(5),
                ExampleResponse = reader.IsDBNull(6) ? null : reader.GetString(6),
                UseCases = reader.IsDBNull(7) ? null : reader.GetString(7),
                RelatedCommands = reader.IsDBNull(8) ? null : reader.GetString(8),
                VersionAdded = reader.IsDBNull(9) ? "1.0.0" : reader.GetString(9),
                CommonErrors = reader.IsDBNull(10) ? null : reader.GetString(10)
            };
        }

        private WorkflowEntry ReadWorkflowEntry(SqliteDataReader reader)
        {
            return new WorkflowEntry
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.GetString(2),
                Difficulty = reader.GetString(3),
                Steps = reader.GetString(4),
                CommandsUsed = reader.IsDBNull(5) ? null : reader.GetString(5),
                EstimatedTime = reader.GetInt32(6),
                Tags = reader.IsDBNull(7) ? null : reader.GetString(7),
                TimesUsed = reader.GetInt32(8),
                SuccessRate = reader.GetFloat(9)
            };
        }

        private FAQEntry ReadFAQEntry(SqliteDataReader reader)
        {
            return new FAQEntry
            {
                Id = reader.GetInt32(0),
                Question = reader.GetString(1),
                Answer = reader.GetString(2),
                Category = reader.GetString(3),
                RelatedCommands = reader.IsDBNull(4) ? null : reader.GetString(4),
                HelpfulCount = reader.GetInt32(5),
                Keywords = reader.IsDBNull(6) ? null : reader.GetString(6)
            };
        }

        #endregion

        /// <summary>
        /// Close the database connection
        /// </summary>
        public void Close()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _isInitialized = false;
                Debug.Log("[Synthesis KB] Database connection closed");
            }
        }
    }

    #region Data Classes

    [Serializable]
    public class CommandEntry
    {
        public int Id;
        public string Name;
        public string Category;
        public string Description;
        public string Parameters;
        public string ExampleRequest;
        public string ExampleResponse;
        public string UseCases;
        public string RelatedCommands;
        public string VersionAdded;
        public string CommonErrors;
    }

    [Serializable]
    public class WorkflowEntry
    {
        public int Id;
        public string Title;
        public string Description;
        public string Difficulty;
        public string Steps;
        public string CommandsUsed;
        public int EstimatedTime;
        public string Tags;
        public int TimesUsed;
        public float SuccessRate;
    }

    [Serializable]
    public class FAQEntry
    {
        public int Id;
        public string Question;
        public string Answer;
        public string Category;
        public string RelatedCommands;
        public int HelpfulCount;
        public string Keywords;
    }

    #endregion
}
#endif
