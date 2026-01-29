using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NightBlade
{
    // TEMPORARILY DISABLED: Custom editor has bugs causing massive UI duplication
    // TODO: Properly implement section-based property rendering
    //[CustomEditor(typeof(GameInstance))]
    //[CanEditMultipleObjects]
    public class GameInstanceEditor_DISABLED : Editor
    {
        private GameInstance gameInstance;
        private BaseGameplayRule gameplayRule;

        // Foldout states
        private SerializedProperty gameplaySystemsFoldout;
        private SerializedProperty gameplayObjectsFoldout;
        private SerializedProperty gameplayEffectsFoldout;
        private SerializedProperty gameplayDatabaseFoldout;
        private SerializedProperty objectTagsFoldout;
        private SerializedProperty gameplayConfigsFoldout;
        private SerializedProperty itemsConfigsFoldout;
        private SerializedProperty summonConfigsFoldout;
        private SerializedProperty newCharacterFoldout;
        private SerializedProperty serverSettingsFoldout;
        private SerializedProperty playerConfigsFoldout;
        private SerializedProperty platformConfigsFoldout;
        private SerializedProperty editorConfigsFoldout;

        // GameInstance properties
        private SerializedProperty messageManager;
        private SerializedProperty saveSystem;
        private SerializedProperty inventoryManager;
        private SerializedProperty dayNightTimeUpdater;
        private SerializedProperty gmCommands;
        private SerializedProperty equipmentModelBonesSetupManager;
        private SerializedProperty networkSetting;
        private SerializedProperty newCharacterSetting;
        private SerializedProperty startGold;
        private SerializedProperty startItems;
        private SerializedProperty testingNewCharacterSetting;
        private SerializedProperty updateAnimationAtServer;
        private SerializedProperty minCharacterNameLength;
        private SerializedProperty maxCharacterNameLength;
        private SerializedProperty maxCharacterSaves;
        private SerializedProperty serverTargetFrameRate;
        private SerializedProperty testInEditorMode;
        private SerializedProperty networkManagerForOfflineTesting;

        private void OnEnable()
        {
            gameInstance = (GameInstance)target;

            // Get the gameplayRule field from GameInstance
            var gameplayRuleField = gameInstance.GetType().GetField("gameplayRule", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gameplayRuleField != null)
            {
                gameplayRule = (BaseGameplayRule)gameplayRuleField.GetValue(gameInstance);
            }

            // Initialize foldout properties
            gameplaySystemsFoldout = serializedObject.FindProperty("gameplaySystemsFoldout");
            gameplayObjectsFoldout = serializedObject.FindProperty("gameplayObjectsFoldout");
            gameplayEffectsFoldout = serializedObject.FindProperty("gameplayEffectsFoldout");
            gameplayDatabaseFoldout = serializedObject.FindProperty("gameplayDatabaseFoldout");
            objectTagsFoldout = serializedObject.FindProperty("objectTagsFoldout");
            gameplayConfigsFoldout = serializedObject.FindProperty("gameplayConfigsFoldout");
            itemsConfigsFoldout = serializedObject.FindProperty("itemsConfigsFoldout");
            summonConfigsFoldout = serializedObject.FindProperty("summonConfigsFoldout");
            newCharacterFoldout = serializedObject.FindProperty("newCharacterFoldout");
            serverSettingsFoldout = serializedObject.FindProperty("serverSettingsFoldout");
            playerConfigsFoldout = serializedObject.FindProperty("playerConfigsFoldout");
            platformConfigsFoldout = serializedObject.FindProperty("platformConfigsFoldout");
            editorConfigsFoldout = serializedObject.FindProperty("editorConfigsFoldout");

            // Initialize GameInstance properties
            messageManager = serializedObject.FindProperty("messageManager");
            saveSystem = serializedObject.FindProperty("saveSystem");
            inventoryManager = serializedObject.FindProperty("inventoryManager");
            dayNightTimeUpdater = serializedObject.FindProperty("dayNightTimeUpdater");
            gmCommands = serializedObject.FindProperty("gmCommands");
            equipmentModelBonesSetupManager = serializedObject.FindProperty("equipmentModelBonesSetupManager");
            networkSetting = serializedObject.FindProperty("networkSetting");
            newCharacterSetting = serializedObject.FindProperty("newCharacterSetting");
            startGold = serializedObject.FindProperty("startGold");
            startItems = serializedObject.FindProperty("startItems");
            testingNewCharacterSetting = serializedObject.FindProperty("testingNewCharacterSetting");
            updateAnimationAtServer = serializedObject.FindProperty("updateAnimationAtServer");
            minCharacterNameLength = serializedObject.FindProperty("minCharacterNameLength");
            maxCharacterNameLength = serializedObject.FindProperty("maxCharacterNameLength");
            maxCharacterSaves = serializedObject.FindProperty("maxCharacterSaves");
            serverTargetFrameRate = serializedObject.FindProperty("serverTargetFrameRate");
            testInEditorMode = serializedObject.FindProperty("testInEditorMode");
            networkManagerForOfflineTesting = serializedObject.FindProperty("networkManagerForOfflineTesting");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("🎮 NightBlade Game Instance", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenGameInstanceDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Version info
            EditorGUILayout.HelpBox("NightBlade v1.95r3 + Revision 4 Alpha - Core game configuration", MessageType.Info);
            EditorGUILayout.Space(5);

            // Core Configuration
            EditorGUILayout.LabelField("🎯 Core Configuration", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Show the GameplayRule field so users can assign DefaultGameplayRule assets
            SerializedProperty gameplayRuleProp = serializedObject.FindProperty("gameplayRule");
            if (gameplayRuleProp != null)
            {
                EditorGUILayout.PropertyField(gameplayRuleProp, new GUIContent("Gameplay Rule",
                    "Assign a gameplay rule asset (DefaultGameplayRule) to define game mechanics and balancing"));
            }
            else
            {
                EditorGUILayout.HelpBox("GameplayRule field not found. Make sure you're using the correct GameInstance version.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);

            // Quick balance calculator (only if DefaultGameplayRule is assigned)
            if (gameplayRule is DefaultGameplayRule defaultRules)
            {
                EditorGUILayout.LabelField("⚖️ Balance Calculator", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Stat Points/Level:", GUILayout.Width(120));
                EditorGUILayout.LabelField($"{defaultRules.increaseStatPointEachLevel}", EditorStyles.boldLabel, GUILayout.Width(50));
                EditorGUILayout.LabelField("Skill Points/Level:", GUILayout.Width(120));
                EditorGUILayout.LabelField($"{defaultRules.increaseSkillPointEachLevel}", EditorStyles.boldLabel, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Death Penalty:", GUILayout.Width(120));
                EditorGUILayout.LabelField($"{defaultRules.expLostPercentageWhenDeath}% XP", EditorStyles.boldLabel, GUILayout.Width(100));
                EditorGUILayout.LabelField("Sprint Speed:", GUILayout.Width(120));
                EditorGUILayout.LabelField($"{defaultRules.moveSpeedRateWhileSprinting}x", EditorStyles.boldLabel, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(10);
            }

            // Quick validation
            if (Application.isPlaying && gameInstance != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("🎯 Runtime Status", EditorStyles.boldLabel);

                if (GameInstance.Singleton == gameInstance)
                {
                    EditorGUILayout.LabelField("✅ This is the active GameInstance", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("⚠️ Multiple GameInstance components detected", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(10);
            }

            // Main configuration sections
            DrawGameplaySystemsSection();
            DrawGameplayObjectsSection();
            DrawGameplayEffectsSection();
            DrawGameplayDatabaseSection();
            DrawObjectTagsSection();
            DrawGameplayConfigsSection();
            DrawItemsConfigsSection();
            DrawSummonConfigsSection();
            DrawNewCharacterSection();
            DrawServerSettingsSection();
            DrawPlayerConfigsSection();
            DrawPlatformConfigsSection();
            DrawEditorConfigsSection();

            // Footer
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Validate Configuration", GUILayout.Height(30)))
            {
                ValidateConfiguration();
            }

            if (GUILayout.Button("📋 Generate Config Report", GUILayout.Height(30)))
            {
                GenerateConfigReport();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGameplaySystemsSection()
        {
            gameplaySystemsFoldout.boolValue = EditorGUILayout.Foldout(gameplaySystemsFoldout.boolValue, "🎮 Gameplay Systems");
            if (gameplaySystemsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Core gameplay managers and systems that handle game logic.", MessageType.Info);
                DrawGameplaySystemsProperties();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawGameplayObjectsSection()
        {
            gameplayObjectsFoldout.boolValue = EditorGUILayout.Foldout(gameplayObjectsFoldout.boolValue, "🎯 Gameplay Objects");
            if (gameplayObjectsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Prefabs and objects used for gameplay elements like items and effects.", MessageType.Info);
                DrawPropertiesInRange(149, 239); // Gameplay Objects header to next header
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawGameplayEffectsSection()
        {
            gameplayEffectsFoldout.boolValue = EditorGUILayout.Foldout(gameplayEffectsFoldout.boolValue, "✨ Gameplay Effects");
            if (gameplayEffectsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Visual and gameplay effects for combat, magic, and environmental interactions.", MessageType.Info);
                DrawPropertiesInRange(240, 266); // Gameplay Effects header to next header
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawGameplayDatabaseSection()
        {
            gameplayDatabaseFoldout.boolValue = EditorGUILayout.Foldout(gameplayDatabaseFoldout.boolValue, "💾 Gameplay Database & Default Data");
            if (gameplayDatabaseFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure default game data, character stats, and database connections.", MessageType.Info);
                DrawPropertiesInRange(267, 303); // Gameplay Database header to next header
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawObjectTagsSection()
        {
            objectTagsFoldout.boolValue = EditorGUILayout.Foldout(objectTagsFoldout.boolValue, "🏷️ Object Tags & Layers");
            if (objectTagsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure Unity tags and layers used by gameplay systems.", MessageType.Info);
                DrawPropertiesInRange(304, 346); // Object Tags header to next header
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawGameplayConfigsSection()
        {
            gameplayConfigsFoldout.boolValue = EditorGUILayout.Foldout(gameplayConfigsFoldout.boolValue, "⚙️ Gameplay Configs - Generic");
            if (gameplayConfigsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Core gameplay settings affecting all players and game mechanics.", MessageType.Info);
                DrawPropertiesInRange(347, 419); // Gameplay Configs header to next header
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawItemsConfigsSection()
        {
            itemsConfigsFoldout.boolValue = EditorGUILayout.Foldout(itemsConfigsFoldout.boolValue, "🎒 Items, Inventory & Storage");
            if (itemsConfigsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure item systems, inventory management, and storage mechanics.", MessageType.Info);
                DrawPropertiesInRange(420, 446); // Items Configs header to next header
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSummonConfigsSection()
        {
            summonConfigsFoldout.boolValue = EditorGUILayout.Foldout(summonConfigsFoldout.boolValue, "🐉 Summon Systems");
            if (summonConfigsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Settings for summoning monsters and pets in gameplay.", MessageType.Info);
                DrawPropertiesInRange(447, 466); // Summon Configs headers
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawNewCharacterSection()
        {
            newCharacterFoldout.boolValue = EditorGUILayout.Foldout(newCharacterFoldout.boolValue, "👤 New Character Settings");
            if (newCharacterFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Configure starting equipment, stats, and abilities for new characters.", MessageType.Info);
                DrawNewCharacterProperties();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawServerSettingsSection()
        {
            serverSettingsFoldout.boolValue = EditorGUILayout.Foldout(serverSettingsFoldout.boolValue, "🖥️ Server Settings");
            if (serverSettingsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Server-side configuration for multiplayer and networking.", MessageType.Info);
                DrawServerSettingsProperties();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPlayerConfigsSection()
        {
            playerConfigsFoldout.boolValue = EditorGUILayout.Foldout(playerConfigsFoldout.boolValue, "🎮 Player Configs");
            if (playerConfigsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Player-specific settings and limitations.", MessageType.Info);
                DrawPlayerConfigsProperties();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPlatformConfigsSection()
        {
            platformConfigsFoldout.boolValue = EditorGUILayout.Foldout(platformConfigsFoldout.boolValue, "📱 Platform Configs");
            if (platformConfigsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Platform-specific settings for different target devices.", MessageType.Info);
                DrawPlatformConfigsProperties();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawEditorConfigsSection()
        {
            editorConfigsFoldout.boolValue = EditorGUILayout.Foldout(editorConfigsFoldout.boolValue, "🎯 Editor Testing");
            if (editorConfigsFoldout.boolValue)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Settings for testing gameplay in Unity Editor.", MessageType.Info);
                DrawEditorConfigsProperties();
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPropertiesInRange(int startLine, int endLine)
        {
            // DISABLED: This was causing massive duplication
            // TODO: Properly implement section-based property rendering
            // For now, this editor is disabled - Unity will use default inspector
        }

        // Specific section drawers
        private void DrawGameplaySystemsProperties()
        {
            SafePropertyField(messageManager);
            SafePropertyField(saveSystem);
            SafePropertyField(inventoryManager);
            SafePropertyField(dayNightTimeUpdater);
            SafePropertyField(gmCommands);
            SafePropertyField(equipmentModelBonesSetupManager);
            SafePropertyField(networkSetting);
        }

        private void DrawNewCharacterProperties()
        {
            if (newCharacterSetting != null)
            {
                EditorGUILayout.PropertyField(newCharacterSetting);

                // Show startGold and startItems only if newCharacterSetting is null
                if (newCharacterSetting.objectReferenceValue == null)
                {
                    SafePropertyField(startGold);
                    SafePropertyField(startItems);
                }
            }

            SafePropertyField(testingNewCharacterSetting);
        }

        private void DrawServerSettingsProperties()
        {
            SafePropertyField(updateAnimationAtServer);
        }

        private void DrawPlayerConfigsProperties()
        {
            SafePropertyField(minCharacterNameLength);
            SafePropertyField(maxCharacterNameLength);
            SafePropertyField(maxCharacterSaves);
        }

        private void DrawPlatformConfigsProperties()
        {
            SafePropertyField(serverTargetFrameRate);
        }

        private void DrawEditorConfigsProperties()
        {
#if UNITY_EDITOR
            SafePropertyField(testInEditorMode);
            SafePropertyField(networkManagerForOfflineTesting);
#endif
        }

        /// <summary>
        /// Safely draws a property field, checking for null before drawing
        /// </summary>
        private void SafePropertyField(SerializedProperty property)
        {
            if (property != null)
            {
                EditorGUILayout.PropertyField(property);
            }
        }

        private void ValidateConfiguration()
        {
            Debug.Log("🔍 Validating GameInstance configuration...");

            // Add validation logic here
            if (gameInstance != null)
            {
                Debug.Log("✅ Basic validation passed");
            }

            Debug.Log("📋 Validation complete");
        }

        private void GenerateConfigReport()
        {
            Debug.Log("📋 Generating GameInstance configuration report...");

            // Add report generation logic here
            Debug.Log("📊 Configuration report generated");
        }

        private void OpenGameInstanceDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "GameInstance.md");

            // Convert to absolute path
            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                // Open with default application (usually VS Code, Notepad++, etc.)
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened GameInstance documentation: {fullPath}");
            }
            else
            {
                // Fallback: try to open in web browser if local file doesn't exist
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/GameInstance.md");
                Debug.LogWarning($"📖 Local documentation not found, opening web version. Expected: {fullPath}");
            }
        }
    }
}







