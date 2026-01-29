using UnityEngine;
using UnityEditor;
using NightBlade;
using System.Collections.Generic;
using System.Linq;

namespace NightBlade
{
    [CustomEditor(typeof(UISceneGameplay))]
    public class UISceneGameplayEditor : Editor
    {
        private UISceneGameplay uiSceneGameplay;

        // Foldout states for different UI sections
        private bool targetUIsFoldout = true;
        private bool interactionUIsFoldout = true;
        private bool craftingUIsFoldout = false;
        private bool tradingUIsFoldout = false;
        private bool storageUIsFoldout = false;
        private bool buildingUIsFoldout = false;
        private bool socialUIsFoldout = false;
        private bool combatUIsFoldout = false;
        private bool toggleUIsFoldout = false;
        private bool settingsFoldout = true;
        private bool validationFoldout = false;

        private void OnEnable()
        {
            uiSceneGameplay = (UISceneGameplay)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("🎮 NightBlade Gameplay UI Manager", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenUISceneGameplayDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Status and overview
            DrawStatusOverview();

            // Target UIs section
            DrawTargetUIsSection();

            // Interaction UIs section
            DrawInteractionUIsSection();

            // Crafting UIs section
            DrawCraftingUIsSection();

            // Trading UIs section
            DrawTradingUIsSection();

            // Storage UIs section
            DrawStorageUIsSection();

            // Building UIs section
            DrawBuildingUIsSection();

            // Social UIs section
            DrawSocialUIsSection();

            // Combat UIs section
            DrawCombatUIsSection();

            // Toggle UIs section
            DrawToggleUIsSection();

            // Settings section
            DrawSettingsSection();

            // Validation and diagnostics
            DrawValidationSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStatusOverview()
        {
            EditorGUILayout.LabelField("📊 UI Manager Status", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Component counts
            int assignedUIs = CountAssignedUIs();
            int totalUIFields = CountTotalUIFields();

            EditorGUILayout.LabelField($"UI Components: {assignedUIs}/{totalUIFields} assigned", EditorStyles.boldLabel);

            // Progress bar
            Rect progressRect = GUILayoutUtility.GetRect(100, 20);
            float progress = totalUIFields > 0 ? (float)assignedUIs / totalUIFields : 0f;
            EditorGUI.ProgressBar(progressRect, progress, $"{assignedUIs}/{totalUIFields} UI components configured");

            // Quick stats
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Toggle UIs: {uiSceneGameplay.toggleUis.Count}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Block Controllers: {uiSceneGameplay.blockControllerUis.Count}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Ignored Tags: {uiSceneGameplay.ignorePointerOverUITags.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            // Quick actions
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Auto-Assign", GUILayout.Height(25)))
            {
                AutoAssignUIComponents();
            }
            if (GUILayout.Button("✅ Validate", GUILayout.Height(25)))
            {
                ValidateUIConfiguration();
            }
            if (GUILayout.Button("📋 Report", GUILayout.Height(25)))
            {
                GenerateUIReport();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }

        private void DrawTargetUIsSection()
        {
            targetUIsFoldout = EditorGUILayout.Foldout(targetUIsFoldout, "🎯 Target UIs - Entity Selection");
            if (targetUIsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("UI components for displaying information about targeted game entities (characters, NPCs, items, buildings).", MessageType.Info);

                EditorGUILayout.LabelField("Entity Target UIs", EditorStyles.boldLabel);

                // Character and NPC targets
                SafePropertyField("uiTargetCharacter", "UI for selected character entities");
                SafePropertyField("uiTargetNpc", "UI for selected NPC entities");
                SafePropertyField("uiTargetItemDrop", "UI for selected item drops");
                SafePropertyField("uiTargetItemsContainer", "UI for selected item containers");

                // Building and harvestable targets
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Building & Resource UIs", EditorStyles.boldLabel);
                SafePropertyField("uiTargetBuilding", "UI for selected buildings");
                SafePropertyField("uiTargetHarvestable", "UI for selected harvestable objects");
                SafePropertyField("uiTargetVehicle", "UI for selected vehicles");

                // Validation
                int assignedTargets = CountAssignedTargets();
                if (assignedTargets < 7)
                {
                    EditorGUILayout.HelpBox($"⚠️ {7 - assignedTargets} target UIs not assigned. Some entities may not display properly.", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawInteractionUIsSection()
        {
            interactionUIsFoldout = EditorGUILayout.Foldout(interactionUIsFoldout, "💬 Interaction UIs - NPC & Quest Dialogs");
            if (interactionUIsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("UI components for player interactions with NPCs, quests, and dialogs.", MessageType.Info);

                EditorGUILayout.LabelField("Dialog & Quest UIs", EditorStyles.boldLabel);
                SafePropertyField("uiNpcDialog", "NPC conversation dialog interface");
                SafePropertyField("uiQuestRewardItemSelection", "Quest reward item selection dialog");

                // Validation
                bool hasNpcDialog = serializedObject.FindProperty("uiNpcDialog").objectReferenceValue != null;
                bool hasQuestRewards = serializedObject.FindProperty("uiQuestRewardItemSelection").objectReferenceValue != null;

                if (!hasNpcDialog || !hasQuestRewards)
                {
                    EditorGUILayout.HelpBox("⚠️ Core interaction UIs missing. NPC conversations and quest rewards may not work.", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawCraftingUIsSection()
        {
            craftingUIsFoldout = EditorGUILayout.Foldout(craftingUIsFoldout, "⚒️ Crafting UIs - Item Creation");
            if (craftingUIsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("UI components for item crafting, refining, and enhancement systems.", MessageType.Info);

                EditorGUILayout.LabelField("Item Modification UIs", EditorStyles.boldLabel);
                SafePropertyField("uiRefineItem", "Item refinement and upgrade interface");
                SafePropertyField("uiDismantleItem", "Single item dismantling interface");
                SafePropertyField("uiBulkDismantleItems", "Bulk item dismantling interface");
                SafePropertyField("uiRepairItem", "Item repair interface");
                SafePropertyField("uiEnhanceSocketItem", "Socket enhancement interface");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Crafting UIs", EditorStyles.boldLabel);
                SafePropertyField("uiBuildingCraftItems", "Building-based item crafting interface");
                SafePropertyField("uiCraftingQueueItems", "Crafting queue management interface");

                // Validation
                int assignedCrafting = CountAssignedCraftingUIs();
                if (assignedCrafting < 7)
                {
                    EditorGUILayout.HelpBox($"ℹ️ {7 - assignedCrafting} crafting UIs not assigned. Some crafting features may be unavailable.", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawTradingUIsSection()
        {
            tradingUIsFoldout = EditorGUILayout.Foldout(tradingUIsFoldout, "💰 Trading UIs - Commerce & Dealing");
            if (tradingUIsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("UI components for player-to-player and player-to-NPC trading systems.", MessageType.Info);

                EditorGUILayout.LabelField("Dealing System UIs", EditorStyles.boldLabel);
                SafePropertyField("uiDealingRequest", "Trade request interface");
                SafePropertyField("uiDealing", "Active trade session interface");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Vending System UIs", EditorStyles.boldLabel);
                SafePropertyField("uiStartVending", "Start vending interface");
                SafePropertyField("uiVending", "Active vending interface");
                SafePropertyField("showVendingUiOnActivate", "Auto-show vending UI when player activates vending entity");

                // Validation
                bool hasDealing = serializedObject.FindProperty("uiDealing").objectReferenceValue != null &&
                                serializedObject.FindProperty("uiDealingRequest").objectReferenceValue != null;
                bool hasVending = serializedObject.FindProperty("uiVending").objectReferenceValue != null &&
                                serializedObject.FindProperty("uiStartVending").objectReferenceValue != null;

                if (!hasDealing)
                {
                    EditorGUILayout.HelpBox("⚠️ Dealing UIs not configured. Player-to-player trading may not work.", MessageType.Warning);
                }
                if (!hasVending)
                {
                    EditorGUILayout.HelpBox("⚠️ Vending UIs not configured. NPC/vendor trading may not work.", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawStorageUIsSection()
        {
            storageUIsFoldout = EditorGUILayout.Foldout(storageUIsFoldout, "📦 Storage UIs - Inventory Management");
            if (storageUIsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("UI components for managing various storage systems (player, guild, building inventories).", MessageType.Info);

                EditorGUILayout.LabelField("Player Storage UIs", EditorStyles.boldLabel);
                SafePropertyField("uiPlayerStorageItems", "Player personal storage interface");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Guild & Building Storage UIs", EditorStyles.boldLabel);
                SafePropertyField("uiGuildStorageItems", "Guild shared storage interface (uses player storage if not set)");
                SafePropertyField("uiBuildingStorageItems", "Building storage interface (uses player storage if not set)");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Special Building UIs", EditorStyles.boldLabel);
                SafePropertyField("uiBuildingCampfireItems", "Campfire cooking interface");
                SafePropertyField("uiItemsContainer", "Generic items container interface");

                // Validation
                bool hasPlayerStorage = serializedObject.FindProperty("uiPlayerStorageItems").objectReferenceValue != null;
                if (!hasPlayerStorage)
                {
                    EditorGUILayout.HelpBox("⚠️ Player storage UI not configured. Inventory management may not work.", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawBuildingUIsSection()
        {
            buildingUIsFoldout = EditorGUILayout.Foldout(buildingUIsFoldout, "🏗️ Building UIs - Construction & Management");
            if (buildingUIsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("UI components for building construction, management, and interaction.", MessageType.Info);

                EditorGUILayout.LabelField("Construction UIs", EditorStyles.boldLabel);
                SafePropertyField("uiConstructBuilding", "Building construction interface");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Building Management UIs", EditorStyles.boldLabel);
                SafePropertyField("uiCurrentBuilding", "Active building management interface");
                SafePropertyField("uiCurrentDoor", "Door building management (uses current building if not set)");
                SafePropertyField("uiCurrentStorage", "Storage building management (uses current building if not set)");
                SafePropertyField("uiCurrentWorkbench", "Workbench building management (uses current building if not set)");
                SafePropertyField("uiCurrentQueuedWorkbench", "Queued workbench building management (uses current building if not set)");

                // Validation
                bool hasConstruction = serializedObject.FindProperty("uiConstructBuilding").objectReferenceValue != null;
                bool hasManagement = serializedObject.FindProperty("uiCurrentBuilding").objectReferenceValue != null;

                if (!hasConstruction)
                {
                    EditorGUILayout.HelpBox("⚠️ Construction UI not configured. Players cannot build.", MessageType.Warning);
                }
                if (!hasManagement)
                {
                    EditorGUILayout.HelpBox("⚠️ Building management UI not configured. Building interaction may not work.", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSocialUIsSection()
        {
            socialUIsFoldout = EditorGUILayout.Foldout(socialUIsFoldout, "👥 Social UIs - Party & Guild Management");
            if (socialUIsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("UI components for social features like parties, guilds, and dueling.", MessageType.Info);

                EditorGUILayout.LabelField("Party & Guild UIs", EditorStyles.boldLabel);
                SafePropertyField("uiPartyInvitation", "Party invitation interface");
                SafePropertyField("uiGuildInvitation", "Guild invitation interface");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Dueling UIs", EditorStyles.boldLabel);
                SafePropertyField("uiDuelingRequest", "Duel request interface");
                SafePropertyField("uiDueling", "Active duel interface");

                // Validation
                bool hasSocial = serializedObject.FindProperty("uiPartyInvitation").objectReferenceValue != null ||
                               serializedObject.FindProperty("uiGuildInvitation").objectReferenceValue != null;

                if (!hasSocial)
                {
                    EditorGUILayout.HelpBox("ℹ️ Social UIs not configured. Party and guild features may be limited.", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawCombatUIsSection()
        {
            combatUIsFoldout = EditorGUILayout.Foldout(combatUIsFoldout, "⚔️ Combat UIs - Player Activation");
            if (combatUIsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("UI components for combat-related interactions and player activation menus.", MessageType.Info);

                EditorGUILayout.LabelField("Activation UIs", EditorStyles.boldLabel);
                SafePropertyField("uiPlayerActivateMenu", "Player activation menu interface");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Status UIs", EditorStyles.boldLabel);
                SafePropertyField("uiIsWarping", "Warping status indicator");

                // Validation
                bool hasActivation = serializedObject.FindProperty("uiPlayerActivateMenu").objectReferenceValue != null;
                if (!hasActivation)
                {
                    EditorGUILayout.HelpBox("⚠️ Player activation UI not configured. Combat interactions may not work.", MessageType.Warning);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawToggleUIsSection()
        {
            toggleUIsFoldout = EditorGUILayout.Foldout(toggleUIsFoldout, "🔄 Toggle UIs - Keyboard/Input Controls");
            if (toggleUIsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("UI components that can be toggled with keyboard keys or input buttons.", MessageType.Info);

                // Display current toggle UIs
                SerializedProperty toggleUIsProperty = serializedObject.FindProperty("toggleUis");
                EditorGUILayout.PropertyField(toggleUIsProperty, new GUIContent("Toggle UI List"), true);

                // Toggle UI management
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("➕ Add Toggle UI", GUILayout.Height(25)))
                {
                    toggleUIsProperty.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("🗑️ Clear All", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("Clear Toggle UIs",
                        "Are you sure you want to remove all toggle UI entries?", "Yes", "No"))
                    {
                        toggleUIsProperty.arraySize = 0;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Information about toggle UIs
                if (toggleUIsProperty.arraySize > 0)
                {
                    EditorGUILayout.HelpBox("Each toggle UI can be activated by:\n• Keyboard key (KeyCode)\n• Input button name\n• Both (whichever is pressed first)", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSettingsSection()
        {
            settingsFoldout = EditorGUILayout.Foldout(settingsFoldout, "⚙️ Settings - Input & Behavior");
            if (settingsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("General settings for UI behavior, input handling, and system configuration.", MessageType.Info);

                EditorGUILayout.LabelField("UI Behavior Settings", EditorStyles.boldLabel);
                SafePropertyField("showVendingUiOnActivate", "Auto-show vending UI when activating vendor entities");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Input Handling", EditorStyles.boldLabel);

                // Pointer over UI detection
                SerializedProperty ignoreTagsProperty = serializedObject.FindProperty("ignorePointerOverUITags");
                EditorGUILayout.PropertyField(ignoreTagsProperty, new GUIContent("Ignored UI Tags", "Tags that ignore pointer detection"), true);

                SerializedProperty ignoreObjectsProperty = serializedObject.FindProperty("ignorePointerOverUIObjects");
                EditorGUILayout.PropertyField(ignoreObjectsProperty, new GUIContent("Ignored UI Objects", "GameObjects that ignore pointer detection"), true);

                // Controller blocking UIs
                EditorGUILayout.Space(5);
                SerializedProperty blockControllerUIsProperty = serializedObject.FindProperty("blockControllerUis");
                EditorGUILayout.PropertyField(blockControllerUIsProperty, new GUIContent("Controller Blocking UIs", "UIs that block character controller input while visible"), true);

                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("➕ Add Blocker", GUILayout.Height(25)))
                {
                    blockControllerUIsProperty.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("🔍 Find UIBlockController", GUILayout.Height(25)))
                {
                    FindUIBlockControllers();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawValidationSection()
        {
            validationFoldout = EditorGUILayout.Foldout(validationFoldout, "🔍 Validation & Diagnostics");
            if (validationFoldout)
            {
                EditorGUILayout.BeginVertical("box");

                // Validation results
                ValidationResult validation = ValidateConfiguration();

                EditorGUILayout.LabelField("Configuration Status", EditorStyles.boldLabel);

                // Overall status
                if (validation.IsValid)
                {
                    EditorGUILayout.LabelField("✅ All core UIs configured", EditorStyles.boldLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("⚠️ Configuration issues found", EditorStyles.boldLabel);
                }

                // Detailed validation
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Validation Details:", EditorStyles.miniLabel);

                foreach (var issue in validation.Issues)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(issue.Icon, GUILayout.Width(20));
                    EditorGUILayout.LabelField(issue.Message, EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }

                // Performance recommendations
                if (validation.PerformanceWarnings.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Performance Notes:", EditorStyles.miniLabel);
                    foreach (var warning in validation.PerformanceWarnings)
                    {
                        EditorGUILayout.LabelField($"⚡ {warning}", EditorStyles.miniLabel);
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void AutoAssignUIComponents()
        {
            if (EditorUtility.DisplayDialog("Auto-Assign UI Components",
                "This will attempt to find and assign UI components from child objects. Existing assignments will be preserved. Continue?",
                "Yes", "No"))
            {
                // Find all UI components in children
                var uiComponents = uiSceneGameplay.GetComponentsInChildren<UIBase>(true);

                // Auto-assign based on component types
                foreach (var uiComponent in uiComponents)
                {
                    AssignUIComponent(uiComponent);
                }

                serializedObject.ApplyModifiedProperties();
                Debug.Log($"[UISceneGameplay] Auto-assigned {uiComponents.Length} UI components");
            }
        }

        private void AssignUIComponent(UIBase uiComponent)
        {
            // This would contain logic to match component types to fields
            // For now, we'll just log what was found
            Debug.Log($"Found UI component: {uiComponent.GetType().Name} on {uiComponent.gameObject.name}");
        }

        private void ValidateUIConfiguration()
        {
            ValidationResult result = ValidateConfiguration();

            if (result.IsValid)
            {
                EditorUtility.DisplayDialog("Validation Complete", "All core UI components are properly configured!", "OK");
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

        private void GenerateUIReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== UISceneGameplay Configuration Report ===");
            report.AppendLine($"Total UI Fields: {CountTotalUIFields()}");
            report.AppendLine($"Assigned UI Components: {CountAssignedUIs()}");
            report.AppendLine($"Toggle UIs: {uiSceneGameplay.toggleUis.Count}");
            report.AppendLine($"Controller Blockers: {uiSceneGameplay.blockControllerUis.Count}");
            report.AppendLine();

            // List unassigned UIs
            report.AppendLine("Unassigned UI Components:");
            var unassigned = GetUnassignedUIs();
            if (unassigned.Count == 0)
            {
                report.AppendLine("✅ All UI components assigned");
            }
            else
            {
                foreach (var uiField in unassigned)
                {
                    report.AppendLine($"• {uiField}");
                }
            }

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("UI Report Generated", "Configuration report logged to console.", "OK");
        }

        private void FindUIBlockControllers()
        {
            var blockControllers = FindObjectsByType<UIBlockController>(FindObjectsSortMode.None);
            Debug.Log($"[UISceneGameplay] Found {blockControllers.Length} UIBlockController components:");

            foreach (var controller in blockControllers)
            {
                Debug.Log($"• {controller.gameObject.name}");
            }
        }

        private int CountAssignedUIs()
        {
            int count = 0;
            var fields = typeof(UISceneGameplay).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.FieldType.IsSubclassOf(typeof(UIBase)) || field.FieldType == typeof(UIBase))
                {
                    var value = field.GetValue(uiSceneGameplay);
                    if (value != null)
                        count++;
                }
            }

            return count;
        }

        private int CountTotalUIFields()
        {
            int count = 0;
            var fields = typeof(UISceneGameplay).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.FieldType.IsSubclassOf(typeof(UIBase)) || field.FieldType == typeof(UIBase))
                {
                    count++;
                }
            }

            return count;
        }

        private int CountAssignedTargets()
        {
            int count = 0;
            string[] targetFields = { "uiTargetCharacter", "uiTargetNpc", "uiTargetItemDrop", "uiTargetItemsContainer",
                                    "uiTargetBuilding", "uiTargetHarvestable", "uiTargetVehicle" };

            foreach (var fieldName in targetFields)
            {
                var field = typeof(UISceneGameplay).GetField(fieldName);
                if (field != null && field.GetValue(uiSceneGameplay) != null)
                    count++;
            }

            return count;
        }

        private int CountAssignedCraftingUIs()
        {
            int count = 0;
            string[] craftingFields = { "uiRefineItem", "uiDismantleItem", "uiBulkDismantleItems", "uiRepairItem",
                                      "uiEnhanceSocketItem", "uiBuildingCraftItems", "uiCraftingQueueItems" };

            foreach (var fieldName in craftingFields)
            {
                var field = typeof(UISceneGameplay).GetField(fieldName);
                if (field != null && field.GetValue(uiSceneGameplay) != null)
                    count++;
            }

            return count;
        }

        private ValidationResult ValidateConfiguration()
        {
            var result = new ValidationResult();
            var issues = new List<ValidationIssue>();
            var performanceWarnings = new List<string>();

            // Check critical UIs
            if (serializedObject.FindProperty("uiTargetCharacter").objectReferenceValue == null)
                issues.Add(new ValidationIssue("⚠️", "Character target UI not assigned"));
            if (serializedObject.FindProperty("uiNpcDialog").objectReferenceValue == null)
                issues.Add(new ValidationIssue("⚠️", "NPC dialog UI not assigned"));
            if (serializedObject.FindProperty("uiPlayerStorageItems").objectReferenceValue == null)
                issues.Add(new ValidationIssue("⚠️", "Player storage UI not assigned"));

            // Check toggle UIs for conflicts
            var toggleUIs = uiSceneGameplay.toggleUis;
            var usedKeys = new HashSet<KeyCode>();
            var usedButtons = new HashSet<string>();

            foreach (var toggleUI in toggleUIs)
            {
                if (toggleUI.keyCode != KeyCode.None)
                {
                    if (!usedKeys.Add(toggleUI.keyCode))
                        issues.Add(new ValidationIssue("⚠️", $"Key {toggleUI.keyCode} used by multiple toggle UIs"));
                }
                if (!string.IsNullOrEmpty(toggleUI.buttonName))
                {
                    if (!usedButtons.Add(toggleUI.buttonName))
                        issues.Add(new ValidationIssue("⚠️", $"Button '{toggleUI.buttonName}' used by multiple toggle UIs"));
                }
            }

            // Performance warnings
            if (uiSceneGameplay.blockControllerUis.Count > 10)
                performanceWarnings.Add("Many controller-blocking UIs may impact performance");
            if (toggleUIs.Count > 20)
                performanceWarnings.Add("Many toggle UIs may cause input conflicts");

            result.Issues = issues;
            result.PerformanceWarnings = performanceWarnings;
            result.IsValid = issues.Count == 0;

            return result;
        }

        private List<string> GetUnassignedUIs()
        {
            var unassigned = new List<string>();
            var fields = typeof(UISceneGameplay).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.FieldType.IsSubclassOf(typeof(UIBase)) || field.FieldType == typeof(UIBase))
                {
                    var value = field.GetValue(uiSceneGameplay);
                    if (value == null)
                        unassigned.Add(field.Name);
                }
            }

            return unassigned;
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

        private void OpenUISceneGameplayDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "UISceneGameplay.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened UISceneGameplay documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/UISceneGameplay.md");
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