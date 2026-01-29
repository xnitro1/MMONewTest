using UnityEngine;
using NightBlade.UI.HUD;
using NightBlade.UI.Dialogue;

namespace NightBlade.UI.Examples
{
    /// <summary>
    /// Example controller to demonstrate the new UI systems.
    /// Press keys to test different UI elements!
    /// 
    /// CONTROLS:
    /// 1 - Test Health Bar (damage)
    /// 2 - Test Health Bar (heal)
    /// 3 - Test Combat Text
    /// 4 - Show Dialogue
    /// 5 - Show Quest Offer
    /// 6 - Test Hotbar
    /// T - Set/Clear Target
    /// 
    /// This is just for testing - delete this script when done! 🎮
    /// </summary>
    public class UIExampleController : MonoBehaviour
    {
        [Header("HUD Components (Assign from scene)")]
        [SerializeField] private UIResourceBar healthBar;
        [SerializeField] private UIResourceBar manaBar;
        [SerializeField] private UIResourceBar staminaBar;
        [SerializeField] private UITargetFrame targetFrame;
        [SerializeField] private UIHotbar hotbar;
        
        [Header("Test Settings")]
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentMana = 50f;
        [SerializeField] private float maxMana = 100f;
        [SerializeField] private float currentStamina = 75f;
        [SerializeField] private float maxStamina = 100f;
        
        [Header("Test Target (Optional)")]
        [SerializeField] private DamageableEntity testTarget;
        
        private bool hasTarget = false;
        
        private void Start()
        {
            Debug.Log("=== UI EXAMPLE CONTROLLER ===");
            Debug.Log("Press 1-6 to test UI elements!");
            Debug.Log("Press T to toggle target frame");
            Debug.Log("============================");
            
            // Initialize bars
            if (healthBar != null)
                healthBar.UpdateResource(currentHealth, maxHealth, animate: false);
            if (manaBar != null)
                manaBar.UpdateResource(currentMana, maxMana, animate: false);
            if (staminaBar != null)
                staminaBar.UpdateResource(currentStamina, maxStamina, animate: false);
        }
        
        private void Update()
        {
            // Test Health Bar - Damage
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestHealthDamage();
            }
            
            // Test Health Bar - Heal
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestHealthHeal();
            }
            
            // Test Combat Text
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestCombatText();
            }
            
            // Test Dialogue
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestDialogue();
            }
            
            // Test Quest Dialogue
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                TestQuestDialogue();
            }
            
            // Test Hotbar
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                TestHotbar();
            }
            
            // Toggle Target
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleTarget();
            }
        }
        
        #region Test Methods
        
        private void TestHealthDamage()
        {
            if (healthBar == null)
            {
                Debug.LogWarning("Health bar not assigned!");
                return;
            }
            
            // Simulate taking damage
            currentHealth = Mathf.Max(0f, currentHealth - 25f);
            healthBar.UpdateResource(currentHealth, maxHealth);
            
            Debug.Log($"💔 Took damage! HP: {currentHealth}/{maxHealth}");
        }
        
        private void TestHealthHeal()
        {
            if (healthBar == null)
            {
                Debug.LogWarning("Health bar not assigned!");
                return;
            }
            
            // Simulate healing
            currentHealth = Mathf.Min(maxHealth, currentHealth + 30f);
            healthBar.UpdateResource(currentHealth, maxHealth);
            
            Debug.Log($"💚 Healed! HP: {currentHealth}/{maxHealth}");
        }
        
        private void TestCombatText()
        {
            Vector3 spawnPos = transform.position + Vector3.up * 2f;
            
            // Show damage number
            UICombatTextCoordinator.ShowDamage(spawnPos, 1250, isCritical: true);
            Debug.Log("⚔️ Spawned critical damage: 1250!");
            
            // Show healing number (offset to the side)
            UICombatTextCoordinator.ShowHealing(spawnPos + Vector3.right * 1f, 500);
            Debug.Log("💚 Spawned healing: 500!");
            
            // Show miss text (offset to other side)
            UICombatTextCoordinator.ShowMiss(spawnPos + Vector3.left * 1f);
            Debug.Log("❌ Spawned MISS text!");
        }
        
        private void TestDialogue()
        {
            if (UIDialogue.Instance == null)
            {
                Debug.LogWarning("UIDialogue not in scene! Add it to your canvas.");
                return;
            }
            
            UIDialogue.Instance.ShowDialogue(
                npcName: "Mysterious Stranger",
                dialogueText: "Greetings, traveler! I have a quest for you... but first, let me tell you about the ancient prophecy of the NightBlade...",
                npcIcon: null, // You can assign a sprite here
                choices: new string[] 
                { 
                    "Tell me more about this prophecy",
                    "Who are you exactly?",
                    "I must go, farewell"
                },
                onChoiceSelected: (index) => HandleDialogueChoice(index)
            );
            
            Debug.Log("💬 Opened dialogue!");
        }
        
        private void HandleDialogueChoice(int index)
        {
            switch (index)
            {
                case 0:
                    Debug.Log("Player: Tell me more");
                    break;
                case 1:
                    Debug.Log("Player: Who are you?");
                    break;
                case 2:
                    Debug.Log("Player: Goodbye");
                    break;
            }
        }
        
        private void TestQuestDialogue()
        {
            if (UIQuestDialogue.Instance == null)
            {
                Debug.LogWarning("UIQuestDialogue not in scene! Add it to your canvas.");
                return;
            }
            
            QuestReward[] rewards = new QuestReward[]
            {
                new QuestReward 
                { 
                    type = QuestReward.RewardType.Gold, 
                    amount = 500
                },
                new QuestReward 
                { 
                    type = QuestReward.RewardType.Experience, 
                    amount = 1000
                }
            };
            
            UIQuestDialogue.Instance.ShowQuestOffer(
                questTitle: "The Lost Sword of NightBlade",
                questDescription: "Venture into the Dark Forest and retrieve the legendary sword!",
                npcName: "Knight Captain",
                npcIcon: null,
                objectives: new string[]
                {
                    "Travel to the Dark Forest",
                    "Defeat the Forest Guardian",
                    "Retrieve the Lost Sword"
                },
                rewards: rewards,
                onAccept: () => Debug.Log("✅ Quest accepted!"),
                onDecline: () => Debug.Log("❌ Quest declined!")
            );
            
            Debug.Log("📜 Opened quest dialogue!");
        }
        
        private void TestHotbar()
        {
            if (hotbar == null)
            {
                Debug.LogWarning("Hotbar not assigned!");
                return;
            }
            
            Debug.Log("🎮 Hotbar test:");
            Debug.Log("- Drag & drop slots to rearrange");
            Debug.Log("- Right-click slot to clear");
            Debug.Log("- Press 1-9 keys to use slots");
            Debug.Log("(Skills/items not implemented in this example)");
        }
        
        private void ToggleTarget()
        {
            if (targetFrame == null)
            {
                Debug.LogWarning("Target frame not assigned!");
                return;
            }
            
            hasTarget = !hasTarget;
            
            if (hasTarget)
            {
                if (testTarget != null)
                {
                    targetFrame.SetTarget(testTarget);
                    Debug.Log($"🎯 Targeted: {testTarget.Title}");
                }
                else
                {
                    Debug.LogWarning("No test target assigned! Assign a DamageableEntity in Inspector.");
                    hasTarget = false;
                }
            }
            else
            {
                targetFrame.ClearTarget();
                Debug.Log("❌ Target cleared");
            }
        }
        
        #endregion
        
        #region Debug GUI
        
        private void OnGUI()
        {
            // Show instructions overlay
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>UI EXAMPLE CONTROLS</b>");
            GUILayout.Space(5);
            GUILayout.Label("1 - Take Damage");
            GUILayout.Label("2 - Heal");
            GUILayout.Label("3 - Combat Text");
            GUILayout.Label("4 - Show Dialogue");
            GUILayout.Label("5 - Show Quest");
            GUILayout.Label("6 - Hotbar Info");
            GUILayout.Label("T - Toggle Target");
            
            GUILayout.Space(10);
            GUILayout.Label($"HP: {currentHealth:F0}/{maxHealth:F0}");
            GUILayout.Label($"Target: {(hasTarget ? "Active" : "None")}");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        #endregion
    }
}
