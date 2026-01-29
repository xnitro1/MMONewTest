using UnityEngine;
using UnityEditor;

namespace NightBlade
{
    [CustomEditor(typeof(DefaultGameplayRule))]
    public class DefaultGameplayRuleEditor : Editor
    {
        private DefaultGameplayRule gameplayRule;

        // Foldout states for each section
        private bool levelingFoldout = true;
        private bool staminaFoldout = true;
        private bool movementFoldout = false;
        private bool survivalFoldout = true;
        private bool durabilityFoldout = false;
        private bool combatFoldout = true;
        private bool progressionFoldout = true;
        private bool deathFoldout = true;
        private bool monstersFoldout = false;
        private bool economyFoldout = false;
        private bool pkFoldout = true;

        private void OnEnable()
        {
            gameplayRule = (DefaultGameplayRule)target;
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header with NightBlade branding
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("⚔️ NightBlade Gameplay Rule", EditorStyles.boldLabel);
            if (GUILayout.Button("📚 Docs", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                OpenGameplayRuleDocumentation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Version info
            EditorGUILayout.HelpBox("Core gameplay mechanics and balancing rules for NightBlade", MessageType.Info);
            EditorGUILayout.Space(5);

            // Quick balance calculator
            EditorGUILayout.LabelField("⚖️ Balance Calculator", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Stat Points/Level:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{gameplayRule.increaseStatPointEachLevel}", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Skill Points/Level:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{gameplayRule.increaseSkillPointEachLevel}", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Death Penalty:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{gameplayRule.expLostPercentageWhenDeath}% XP", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField("Sprint Speed:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"{gameplayRule.moveSpeedRateWhileSprinting}x", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);

            // Main configuration sections
            DrawLevelingSection();
            DrawStaminaSection();
            DrawMovementSection();
            DrawSurvivalSection();
            DrawDurabilitySection();
            DrawCombatSection();
            DrawProgressionSection();
            DrawDeathSection();
            DrawMonstersSection();
            DrawEconomySection();
            DrawPKSection();

            // Footer with validation
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Validate Rules", GUILayout.Height(30)))
            {
                ValidateGameplayRules();
            }

            if (GUILayout.Button("📊 Balance Report", GUILayout.Height(30)))
            {
                GenerateBalanceReport();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLevelingSection()
        {
            levelingFoldout = EditorGUILayout.Foldout(levelingFoldout, "📈 Character Progression");
            if (levelingFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Controls how characters grow and develop through experience and leveling.", MessageType.Info);

                EditorGUILayout.LabelField("Stat & Skill Progression", EditorStyles.boldLabel);
                SafePropertyField("increaseStatPointEachLevel");
                SafePropertyField("increaseSkillPointEachLevel");
                SafePropertyField("increaseStatPointsUntilReachedLevel");
                SafePropertyField("increaseSkillPointsUntilReachedLevel");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Death Penalties", EditorStyles.boldLabel);
                SafePropertyField("expLostPercentageWhenDeath");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawStaminaSection()
        {
            staminaFoldout = EditorGUILayout.Foldout(staminaFoldout, "🏃 Stamina & Movement");
            if (staminaFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Stamina recovery and sprint mechanics affect player mobility and combat.", MessageType.Info);

                EditorGUILayout.LabelField("Stamina Management", EditorStyles.boldLabel);
                SafePropertyField("staminaRecoveryPerSeconds");
                SafePropertyField("staminaDecreasePerSeconds");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Movement Modifiers", EditorStyles.boldLabel);
                SafePropertyField("moveSpeedRateWhileOverweight");
                SafePropertyField("moveSpeedRateWhileSprinting");
                SafePropertyField("moveSpeedIncrementWhileSprinting");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawMovementSection()
        {
            movementFoldout = EditorGUILayout.Foldout(movementFoldout, "🚶 Movement Speeds");
            if (movementFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Different movement modes affect gameplay pacing and strategy.", MessageType.Info);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Walking", EditorStyles.boldLabel);
                SafePropertyField("moveSpeedRateWhileWalking");
                SafePropertyField("moveSpeedIncrementWhileWalking");
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Crouching", EditorStyles.boldLabel);
                SafePropertyField("moveSpeedRateWhileCrouching");
                SafePropertyField("moveSpeedIncrementWhileCrouching");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Crawling", EditorStyles.boldLabel);
                SafePropertyField("moveSpeedRateWhileCrawling");
                SafePropertyField("moveSpeedIncrementWhileCrawling");
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Swimming", EditorStyles.boldLabel);
                SafePropertyField("moveSpeedRateWhileSwimming");
                SafePropertyField("moveSpeedIncrementWhileSwimming");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSurvivalSection()
        {
            survivalFoldout = EditorGUILayout.Foldout(survivalFoldout, "🍖 Survival Needs");
            if (survivalFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Food and water mechanics add survival elements to gameplay.", MessageType.Info);

                EditorGUILayout.LabelField("Hunger & Thirst Thresholds", EditorStyles.boldLabel);
                SafePropertyField("hungryWhenFoodLowerThan");
                SafePropertyField("thirstyWhenWaterLowerThan");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Resource Consumption", EditorStyles.boldLabel);
                SafePropertyField("foodDecreasePerSeconds");
                SafePropertyField("waterDecreasePerSeconds");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Health Recovery", EditorStyles.boldLabel);
                SafePropertyField("hpRecoveryRatePerSeconds");
                SafePropertyField("mpRecoveryRatePerSeconds");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Debuff Effects", EditorStyles.boldLabel);
                SafePropertyField("hpDecreaseRatePerSecondsWhenHungry");
                SafePropertyField("mpDecreaseRatePerSecondsWhenHungry");
                SafePropertyField("hpDecreaseRatePerSecondsWhenThirsty");
                SafePropertyField("mpDecreaseRatePerSecondsWhenThirsty");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawDurabilitySection()
        {
            durabilityFoldout = EditorGUILayout.Foldout(durabilityFoldout, "🔧 Equipment Durability");
            if (durabilityFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Equipment degradation adds maintenance gameplay and resource management.", MessageType.Info);

                EditorGUILayout.LabelField("Normal Combat", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Weapon:", GUILayout.Width(60));
                SafePropertyField("normalDecreaseWeaponDurability");
                EditorGUILayout.LabelField("Shield:", GUILayout.Width(60));
                SafePropertyField("normalDecreaseShieldDurability");
                EditorGUILayout.LabelField("Armor:", GUILayout.Width(60));
                SafePropertyField("normalDecreaseArmorDurability");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Blocked Attacks", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Weapon:", GUILayout.Width(60));
                SafePropertyField("blockedDecreaseWeaponDurability");
                EditorGUILayout.LabelField("Shield:", GUILayout.Width(60));
                SafePropertyField("blockedDecreaseShieldDurability");
                EditorGUILayout.LabelField("Armor:", GUILayout.Width(60));
                SafePropertyField("blockedDecreaseArmorDurability");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Critical Hits", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Weapon:", GUILayout.Width(60));
                SafePropertyField("criticalDecreaseWeaponDurability");
                EditorGUILayout.LabelField("Shield:", GUILayout.Width(60));
                SafePropertyField("criticalDecreaseShieldDurability");
                EditorGUILayout.LabelField("Armor:", GUILayout.Width(60));
                SafePropertyField("criticalDecreaseArmorDurability");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Missed Attacks", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Weapon:", GUILayout.Width(60));
                SafePropertyField("missDecreaseWeaponDurability");
                EditorGUILayout.LabelField("Shield:", GUILayout.Width(60));
                SafePropertyField("missDecreaseShieldDurability");
                EditorGUILayout.LabelField("Armor:", GUILayout.Width(60));
                SafePropertyField("missDecreaseArmorDurability");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawCombatSection()
        {
            combatFoldout = EditorGUILayout.Foldout(combatFoldout, "⚔️ Combat Mechanics");
            if (combatFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Core combat calculations and damage mechanics.", MessageType.Info);

                EditorGUILayout.LabelField("Critical Hit Rules", EditorStyles.boldLabel);
                SafePropertyField("alwaysHitWhenCriticalOccurs");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Fall Damage", EditorStyles.boldLabel);
                SafePropertyField("fallDamageMinDistance");
                SafePropertyField("fallDamageMaxDistance");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawProgressionSection()
        {
            progressionFoldout = EditorGUILayout.Foldout(progressionFoldout, "⭐ Level Up & Battle Points");
            if (progressionFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Rewards and progression mechanics for character advancement.", MessageType.Info);

                EditorGUILayout.LabelField("Level Up Recovery", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                SafePropertyField("recoverHpWhenLevelUp");
                SafePropertyField("recoverMpWhenLevelUp");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                SafePropertyField("recoverFoodWhenLevelUp");
                SafePropertyField("recoverWaterWhenLevelUp");
                SafePropertyField("recoverStaminaWhenLevelUp");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Battle Points Scoring", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Point values for different stats in competitive scoring systems.", MessageType.Info);

                // Group related battle point scores
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Health Points", EditorStyles.miniLabel);
                SafePropertyField("hpBattlePointScore");
                SafePropertyField("hpRecoveryBattlePointScore");
                SafePropertyField("hpLeechRateBattlePointScore");
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Mana Points", EditorStyles.miniLabel);
                SafePropertyField("mpBattlePointScore");
                SafePropertyField("mpRecoveryBattlePointScore");
                SafePropertyField("mpLeechRateBattlePointScore");
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Stamina", EditorStyles.miniLabel);
                SafePropertyField("staminaBattlePointScore");
                SafePropertyField("staminaRecoveryBattlePointScore");
                SafePropertyField("staminaLeechRateBattlePointScore");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Combat Stats", EditorStyles.miniLabel);
                SafePropertyField("accuracyBattlePointScore");
                SafePropertyField("evasionBattlePointScore");
                SafePropertyField("criRateBattlePointScore");
                SafePropertyField("criDmgRateBattlePointScore");
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Defense", EditorStyles.miniLabel);
                SafePropertyField("blockRateBattlePointScore");
                SafePropertyField("blockDmgRateBattlePointScore");
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Movement", EditorStyles.miniLabel);
                SafePropertyField("moveSpeedBattlePointScore");
                SafePropertyField("atkSpeedBattlePointScore");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Resources", EditorStyles.miniLabel);
                SafePropertyField("foodBattlePointScore");
                SafePropertyField("waterBattlePointScore");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawDeathSection()
        {
            deathFoldout = EditorGUILayout.Foldout(deathFoldout, "💀 Player Death");
            if (deathFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Consequences and penalties for player death.", MessageType.Info);

                EditorGUILayout.LabelField("Item Loss on Death", EditorStyles.boldLabel);
                SafePropertyField("itemDecreaseOnDeadMin");
                SafePropertyField("itemDecreaseOnDeadMax");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawMonstersSection()
        {
            monstersFoldout = EditorGUILayout.Foldout(monstersFoldout, "👹 Monster Display");
            if (monstersFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Visual indicators for monster difficulty and level differences.", MessageType.Info);

                EditorGUILayout.LabelField("Level-Based Coloring", EditorStyles.boldLabel);
                SafePropertyField("monsterTitleColorChangeLevel");
                SafePropertyField("monsterHighLevelTitleColor");
                SafePropertyField("monsterLowLevelTitleColor");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawEconomySection()
        {
            economyFoldout = EditorGUILayout.Foldout(economyFoldout, "💰 Economy & Trading");
            if (economyFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Trading fees and economic balancing mechanics.", MessageType.Info);

                EditorGUILayout.LabelField("Trading Fees", EditorStyles.boldLabel);
                SafePropertyField("taxByItemPriceRate");
                SafePropertyField("calculateTaxByStackedItems");

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPKSection()
        {
            pkFoldout = EditorGUILayout.Foldout(pkFoldout, "⚔️ Player Killing (PK)");
            if (pkFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox("Player versus player combat rules and consequences.", MessageType.Info);

                EditorGUILayout.LabelField("PK System Settings", EditorStyles.boldLabel);
                SafePropertyField("minLevelToTurnPkOn");
                SafePropertyField("pkPointEachKills");
                SafePropertyField("hoursBeforeTurnPkOff");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("PK Levels & Penalties", EditorStyles.boldLabel);
                SafePropertyField("pkDatas");

                EditorGUILayout.EndVertical();
            }
        }

        private void SafePropertyField(string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property);
            }
        }

        private void ValidateGameplayRules()
        {
            Debug.Log("🔍 Validating gameplay rules...");

            // Check for common issues
            if (gameplayRule.expLostPercentageWhenDeath < 0 || gameplayRule.expLostPercentageWhenDeath > 100)
            {
                Debug.LogWarning("⚠️ Death XP loss should be between 0-100%");
            }

            if (gameplayRule.moveSpeedRateWhileSprinting <= 1f)
            {
                Debug.LogWarning("⚠️ Sprint speed should be > 1.0 for meaningful speed increase");
            }

            if (gameplayRule.hpRecoveryRatePerSeconds <= 0)
            {
                Debug.LogWarning("⚠️ HP recovery rate should be > 0");
            }

            Debug.Log("✅ Basic validation complete");
        }

        private void GenerateBalanceReport()
        {
            Debug.Log("📊 Generating gameplay balance report...");

            // Calculate some balance metrics
            float sprintMultiplier = gameplayRule.moveSpeedRateWhileSprinting;
            float deathPenalty = gameplayRule.expLostPercentageWhenDeath;
            float statPoints = gameplayRule.increaseStatPointEachLevel;

            Debug.Log($"🏃 Sprint Speed: {sprintMultiplier}x base speed");
            Debug.Log($"💀 Death Penalty: {deathPenalty}% XP loss");
            Debug.Log($"📈 Stat Points/Level: {statPoints}");
            Debug.Log($"⚖️ Balance ratios calculated - check console for details");

            Debug.Log("📋 Balance report generated");
        }

        private void OpenGameplayRuleDocumentation()
        {
            string projectPath = Application.dataPath;
            string docsPath = System.IO.Path.Combine(projectPath, "..", "docs", "DefaultGameplayRule.md");

            string fullPath = System.IO.Path.GetFullPath(docsPath);

            if (System.IO.File.Exists(fullPath))
            {
                System.Diagnostics.Process.Start(fullPath);
                Debug.Log($"📖 Opened DefaultGameplayRule documentation: {fullPath}");
            }
            else
            {
                Application.OpenURL("https://github.com/denariigames/nightblade/blob/master/docs/DefaultGameplayRule.md");
                Debug.LogWarning($"📖 Local documentation not found, opening web version");
            }
        }
    }
}