using UnityEngine;
using System.Collections.Generic;

namespace NightBlade.UI.HUD
{
    /// <summary>
    /// Modern hotbar system for NightBlade.
    /// Manages multiple slots for skills and items.
    /// 
    /// Features:
    /// - Configurable number of slots
    /// - Auto-assign key bindings (1-9, 0, -, =)
    /// - Swap slots via drag-and-drop
    /// - Save/load hotbar configuration
    /// - Multiple hotbar pages (TODO)
    /// 
    /// Design Philosophy:
    /// - Modular (each slot is independent)
    /// - Clean API for other systems
    /// - Event-driven
    /// - Easy to extend
    /// 
    /// Part of the sexy new UI system! 🎮✨
    /// </summary>
    public class UIHotbar : UIBase
    {
        #region Serialized Fields
        
        [Header("Hotbar Settings")]
        [SerializeField] private UIHotbarSlot[] slots;
        [SerializeField] private int numberOfSlots = 10;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;
        
        [Header("Key Bindings")]
        [Tooltip("Auto-assign keys to slots (1-9, 0, -, =)")]
        [SerializeField] private bool autoAssignKeys = true;
        [SerializeField] private KeyCode[] customKeyBindings;
        
        #endregion
        
        #region Default Key Bindings
        
        private static readonly KeyCode[] DefaultKeys = new KeyCode[]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0,
            KeyCode.Minus,
            KeyCode.Equals
        };
        
        #endregion
        
        #region State
        
        private List<UIHotbarSlot> activeSlots = new List<UIHotbarSlot>();
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            
            // Initialize slots
            if (slots == null || slots.Length == 0)
            {
                // Create slots if not assigned
                if (slotsContainer != null && slotPrefab != null)
                {
                    CreateSlots();
                }
                else
                {
                    Debug.LogError("[UIHotbar] No slots assigned and cannot create them (missing container or prefab)!");
                }
            }
            else
            {
                InitializeExistingSlots();
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void CreateSlots()
        {
            slots = new UIHotbarSlot[numberOfSlots];
            
            for (int i = 0; i < numberOfSlots; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
                slotObj.name = $"HotbarSlot_{i + 1}";
                
                UIHotbarSlot slot = slotObj.GetComponent<UIHotbarSlot>();
                if (slot != null)
                {
                    KeyCode key = GetKeyBindingForSlot(i);
                    slot.Initialize(i, key, this);
                    slots[i] = slot;
                    activeSlots.Add(slot);
                }
                else
                {
                    Debug.LogError($"[UIHotbar] Slot prefab does not have UIHotbarSlot component!");
                }
            }
            
            Debug.Log($"[UIHotbar] Created {numberOfSlots} slots");
        }
        
        private void InitializeExistingSlots()
        {
            activeSlots.Clear();
            
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    KeyCode key = GetKeyBindingForSlot(i);
                    slots[i].Initialize(i, key, this);
                    activeSlots.Add(slots[i]);
                }
            }
            
            Debug.Log($"[UIHotbar] Initialized {activeSlots.Count} existing slots");
        }
        
        private KeyCode GetKeyBindingForSlot(int index)
        {
            if (!autoAssignKeys)
            {
                if (customKeyBindings != null && index < customKeyBindings.Length)
                {
                    return customKeyBindings[index];
                }
                return KeyCode.None;
            }
            
            if (index < DefaultKeys.Length)
            {
                return DefaultKeys[index];
            }
            
            return KeyCode.None;
        }
        
        #endregion
        
        #region Public API - Slot Management
        
        /// <summary>
        /// Assign a skill to a hotbar slot
        /// </summary>
        public void SetSkill(int slotIndex, BaseSkill skill, int level, Sprite icon = null)
        {
            if (!IsValidSlotIndex(slotIndex))
                return;
            
            slots[slotIndex].SetSkill(skill, level, icon);
            SaveHotbarConfiguration();
        }
        
        /// <summary>
        /// Assign an item to a hotbar slot
        /// </summary>
        public void SetItem(int slotIndex, IUsableItem item, int amount, Sprite icon = null)
        {
            if (!IsValidSlotIndex(slotIndex))
                return;
            
            slots[slotIndex].SetItem(item, amount, icon);
            SaveHotbarConfiguration();
        }
        
        /// <summary>
        /// Clear a hotbar slot
        /// </summary>
        public void ClearSlot(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
                return;
            
            slots[slotIndex].ClearSlot();
            SaveHotbarConfiguration();
        }
        
        /// <summary>
        /// Swap two hotbar slots
        /// </summary>
        public void SwapSlots(UIHotbarSlot slotA, UIHotbarSlot slotB)
        {
            if (slotA == null || slotB == null || slotA == slotB)
                return;
            
            // Store data
            var dataA = slotA.Data;
            var dataB = slotB.Data;
            
            // Swap
            if (dataA.type == UIHotbarSlot.SlotType.Skill)
            {
                slotB.SetSkill(dataA.skill, dataA.skillLevel);
            }
            else if (dataA.type == UIHotbarSlot.SlotType.Item)
            {
                slotB.SetItem(dataA.item, dataA.itemAmount);
            }
            else
            {
                slotB.ClearSlot();
            }
            
            if (dataB.type == UIHotbarSlot.SlotType.Skill)
            {
                slotA.SetSkill(dataB.skill, dataB.skillLevel);
            }
            else if (dataB.type == UIHotbarSlot.SlotType.Item)
            {
                slotA.SetItem(dataB.item, dataB.itemAmount);
            }
            else
            {
                slotA.ClearSlot();
            }
            
            SaveHotbarConfiguration();
            
            Debug.Log($"[UIHotbar] Swapped slots {slotA.SlotIndex} and {slotB.SlotIndex}");
        }
        
        /// <summary>
        /// Clear all hotbar slots
        /// </summary>
        public void ClearAll()
        {
            foreach (var slot in slots)
            {
                if (slot != null)
                {
                    slot.ClearSlot();
                }
            }
            
            SaveHotbarConfiguration();
        }
        
        /// <summary>
        /// Get a specific slot
        /// </summary>
        public UIHotbarSlot GetSlot(int index)
        {
            if (!IsValidSlotIndex(index))
                return null;
            
            return slots[index];
        }
        
        /// <summary>
        /// Find first empty slot
        /// </summary>
        public int FindEmptySlot()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].IsEmpty)
                {
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Find slot containing a specific skill
        /// </summary>
        public int FindSkillSlot(BaseSkill skill)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && !slots[i].IsEmpty)
                {
                    var data = slots[i].Data;
                    if (data.type == UIHotbarSlot.SlotType.Skill && data.skill == skill)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Find slot containing a specific item
        /// </summary>
        public int FindItemSlot(IUsableItem item)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && !slots[i].IsEmpty)
                {
                    var data = slots[i].Data;
                    if (data.type == UIHotbarSlot.SlotType.Item && data.item == item)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        
        #endregion
        
        #region Public API - Cooldowns
        
        /// <summary>
        /// Start cooldown on a specific slot
        /// </summary>
        public void StartCooldown(int slotIndex, float duration)
        {
            if (!IsValidSlotIndex(slotIndex))
                return;
            
            slots[slotIndex].StartCooldown(duration);
        }
        
        /// <summary>
        /// Start cooldown on all slots containing a specific skill
        /// </summary>
        public void StartSkillCooldown(BaseSkill skill, float duration)
        {
            int slotIndex = FindSkillSlot(skill);
            if (slotIndex >= 0)
            {
                StartCooldown(slotIndex, duration);
            }
        }
        
        #endregion
        
        #region Public API - Item Updates
        
        /// <summary>
        /// Update item count in hotbar (when consuming items)
        /// </summary>
        public void UpdateItemCount(IUsableItem item, int newCount)
        {
            int slotIndex = FindItemSlot(item);
            if (slotIndex >= 0)
            {
                slots[slotIndex].UpdateCount(newCount);
            }
        }
        
        #endregion
        
        #region Save/Load (TODO: Integrate with player save system)
        
        private void SaveHotbarConfiguration()
        {
            // TODO: Save to player preferences/save file
            // For now, just log
#if DEBUG_UI
            Debug.Log("[UIHotbar] Configuration saved");
#endif
        }
        
        public void LoadHotbarConfiguration()
        {
            // TODO: Load from player preferences/save file
            Debug.Log("[UIHotbar] Configuration loaded");
        }
        
        #endregion
        
        #region Utility
        
        private bool IsValidSlotIndex(int index)
        {
            if (index < 0 || index >= slots.Length)
            {
                Debug.LogWarning($"[UIHotbar] Invalid slot index: {index}");
                return false;
            }
            
            if (slots[index] == null)
            {
                Debug.LogWarning($"[UIHotbar] Slot {index} is null!");
                return false;
            }
            
            return true;
        }
        
        #endregion
    }
}
