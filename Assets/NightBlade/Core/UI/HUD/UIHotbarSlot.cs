using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace NightBlade.UI.HUD
{
    /// <summary>
    /// Individual hotbar slot for skills/items.
    /// Handles display, cooldowns, input, and drag-and-drop.
    /// 
    /// Features:
    /// - Icon display
    /// - Cooldown visualization
    /// - Charges/stack count
    /// - Key binding display
    /// - Click to use
    /// - Drag to rearrange
    /// - Visual feedback (hover, press, cooldown)
    /// 
    /// Part of the new modular hotbar system! 🎮
    /// </summary>
    public class UIHotbarSlot : UIBase, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region Slot Data
        
        public enum SlotType
        {
            Empty,
            Skill,
            Item
        }
        
        public class SlotData
        {
            public SlotType type;
            public BaseSkill skill;
            public int skillLevel;
            public IUsableItem item;
            public int itemAmount;
            public float cooldownRemaining;
            public float cooldownTotal;
            public int charges;
            public int maxCharges;
        }
        
        #endregion
        
        #region Serialized Fields
        
        [Header("Slot Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Image borderImage;
        [SerializeField] private TextMeshProUGUI keyBindText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private GameObject emptyIndicator;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f);
        [SerializeField] private Color pressedColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        
        [Header("Settings")]
        [SerializeField] private KeyCode keyBinding = KeyCode.None;
        [SerializeField] private bool showKeyBinding = true;
        [SerializeField] private bool showCount = true;
        
        #endregion
        
        #region State
        
        private SlotData currentData;
        private bool isOnCooldown = false;
        private bool isDragging = false;
        private UIHotbar parentHotbar;
        
        public int SlotIndex { get; private set; }
        public bool IsEmpty => currentData == null || currentData.type == SlotType.Empty;
        public SlotData Data => currentData;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            
            // Initialize as empty
            ClearSlot();
            
            // Find parent hotbar
            parentHotbar = GetComponentInParent<UIHotbar>();
            
            // Setup cooldown overlay
            if (cooldownOverlay != null)
            {
                cooldownOverlay.type = Image.Type.Filled;
                cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
                cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.color = cooldownColor;
            }
            
            // Setup key binding display
            if (keyBindText != null)
            {
                keyBindText.gameObject.SetActive(showKeyBinding && keyBinding != KeyCode.None);
                if (keyBinding != KeyCode.None)
                {
                    keyBindText.text = GetKeyDisplayName(keyBinding);
                }
            }
        }
        
        private void Update()
        {
            // Handle input
            if (keyBinding != KeyCode.None && Input.GetKeyDown(keyBinding))
            {
                OnSlotActivated();
            }
            
            // Update cooldown display
            if (isOnCooldown && currentData != null)
            {
                if (currentData.cooldownRemaining > 0f)
                {
                    currentData.cooldownRemaining -= Time.deltaTime;
                    UpdateCooldownDisplay();
                }
                else
                {
                    // Cooldown finished
                    isOnCooldown = false;
                    if (cooldownOverlay != null)
                        cooldownOverlay.fillAmount = 0f;
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Initialize this slot with index and key binding
        /// </summary>
        public void Initialize(int index, KeyCode key, UIHotbar hotbar)
        {
            SlotIndex = index;
            keyBinding = key;
            parentHotbar = hotbar;
            
            if (keyBindText != null && showKeyBinding && keyBinding != KeyCode.None)
            {
                keyBindText.text = GetKeyDisplayName(keyBinding);
                keyBindText.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// Set slot to display a skill
        /// </summary>
        public void SetSkill(BaseSkill skill, int level, Sprite icon = null)
        {
            currentData = new SlotData
            {
                type = SlotType.Skill,
                skill = skill,
                skillLevel = level,
                cooldownRemaining = 0f,
                cooldownTotal = 0f, // Get from skill
                charges = 1,
                maxCharges = 1
            };
            
            UpdateDisplay(icon);
        }
        
        /// <summary>
        /// Set slot to display an item
        /// </summary>
        public void SetItem(IUsableItem item, int amount, Sprite icon = null)
        {
            currentData = new SlotData
            {
                type = SlotType.Item,
                item = item,
                itemAmount = amount,
                cooldownRemaining = 0f,
                cooldownTotal = 0f,
                charges = amount,
                maxCharges = amount
            };
            
            UpdateDisplay(icon);
        }
        
        /// <summary>
        /// Clear this slot
        /// </summary>
        public void ClearSlot()
        {
            currentData = new SlotData { type = SlotType.Empty };
            isOnCooldown = false;
            
            if (iconImage != null)
                iconImage.gameObject.SetActive(false);
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0f;
            if (countText != null)
                countText.gameObject.SetActive(false);
            if (emptyIndicator != null)
                emptyIndicator.SetActive(true);
        }
        
        /// <summary>
        /// Start cooldown on this slot
        /// </summary>
        public void StartCooldown(float duration)
        {
            if (currentData == null)
                return;
            
            currentData.cooldownRemaining = duration;
            currentData.cooldownTotal = duration;
            isOnCooldown = true;
        }
        
        /// <summary>
        /// Update item count (for consumables)
        /// </summary>
        public void UpdateCount(int newCount)
        {
            if (currentData == null || currentData.type != SlotType.Item)
                return;
            
            currentData.itemAmount = newCount;
            currentData.charges = newCount;
            
            if (countText != null && showCount)
            {
                countText.text = newCount.ToString();
            }
            
            if (newCount <= 0)
            {
                ClearSlot();
            }
        }
        
        #endregion
        
        #region Input Handling
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnSlotActivated();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnSlotRightClicked();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isDragging && borderImage != null)
            {
                borderImage.color = hoverColor;
            }
            
            // Show tooltip
            if (!IsEmpty)
            {
                ShowTooltip();
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isDragging && borderImage != null)
            {
                borderImage.color = normalColor;
            }
            
            // Hide tooltip
            HideTooltip();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsEmpty)
                return;
            
            isDragging = true;
            
            if (borderImage != null)
                borderImage.color = pressedColor;
            
            // TODO: Create drag visual
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;
            
            // TODO: Update drag visual position
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;
            
            isDragging = false;
            
            if (borderImage != null)
                borderImage.color = normalColor;
            
            // Check if dropped on another slot
            if (parentHotbar != null)
            {
                UIHotbarSlot targetSlot = eventData.pointerEnter?.GetComponent<UIHotbarSlot>();
                if (targetSlot != null && targetSlot != this)
                {
                    parentHotbar.SwapSlots(this, targetSlot);
                }
            }
            
            // TODO: Destroy drag visual
        }
        
        #endregion
        
        #region Private Methods
        
        private void OnSlotActivated()
        {
            if (IsEmpty || isOnCooldown)
                return;
            
            // Visual feedback
            if (borderImage != null)
            {
                borderImage.color = pressedColor;
                Invoke(nameof(ResetBorderColor), 0.1f);
            }
            
            // Trigger action based on type
            switch (currentData.type)
            {
                case SlotType.Skill:
                    UseSkill();
                    break;
                case SlotType.Item:
                    UseItem();
                    break;
            }
        }
        
        private void OnSlotRightClicked()
        {
            if (IsEmpty)
                return;
            
            // Right-click to remove
            if (parentHotbar != null)
            {
                parentHotbar.ClearSlot(SlotIndex);
            }
        }
        
        private void UseSkill()
        {
            if (currentData?.skill == null)
                return;
            
            // TODO: Integrate with actual skill system
            Debug.Log($"[UIHotbarSlot] Using skill: {currentData.skill.Title}");
            
            // Start cooldown (example)
            // StartCooldown(currentData.skill.GetCooldown());
        }
        
        private void UseItem()
        {
            if (currentData?.item == null)
                return;
            
            // Get item title (cast to BaseItem which has Title from BaseGameData)
            string itemTitle = (currentData.item as BaseItem)?.Title ?? "Unknown Item";
            
            // TODO: Integrate with actual item system
            Debug.Log($"[UIHotbarSlot] Using item: {itemTitle}");
            
            // Decrease count
            UpdateCount(currentData.itemAmount - 1);
        }
        
        private void UpdateDisplay(Sprite icon)
        {
            if (currentData == null || currentData.type == SlotType.Empty)
            {
                ClearSlot();
                return;
            }
            
            // Show icon
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            
            // Show count for items
            if (countText != null && showCount)
            {
                if (currentData.type == SlotType.Item && currentData.itemAmount > 1)
                {
                    countText.text = currentData.itemAmount.ToString();
                    countText.gameObject.SetActive(true);
                }
                else
                {
                    countText.gameObject.SetActive(false);
                }
            }
            
            // Hide empty indicator
            if (emptyIndicator != null)
            {
                emptyIndicator.SetActive(false);
            }
        }
        
        private void UpdateCooldownDisplay()
        {
            if (cooldownOverlay == null || currentData == null)
                return;
            
            float fillAmount = currentData.cooldownTotal > 0f 
                ? currentData.cooldownRemaining / currentData.cooldownTotal 
                : 0f;
            
            cooldownOverlay.fillAmount = fillAmount;
        }
        
        private void ResetBorderColor()
        {
            if (borderImage != null)
                borderImage.color = normalColor;
        }
        
        private void ShowTooltip()
        {
            // TODO: Integrate with tooltip system
            if (currentData == null)
                return;
            
            string tooltipText = "";
            switch (currentData.type)
            {
                case SlotType.Skill:
                    tooltipText = $"{currentData.skill?.Title}\nLevel: {currentData.skillLevel}";
                    break;
                case SlotType.Item:
                    string itemTitle = (currentData.item as BaseItem)?.Title ?? "Unknown Item";
                    tooltipText = $"{itemTitle}\nAmount: {currentData.itemAmount}";
                    break;
            }
            
            Debug.Log($"[Tooltip] {tooltipText}");
        }
        
        private void HideTooltip()
        {
            // TODO: Integrate with tooltip system
        }
        
        private string GetKeyDisplayName(KeyCode key)
        {
            // Convert key code to displayable string
            if (key >= KeyCode.Alpha1 && key <= KeyCode.Alpha9)
            {
                return ((int)key - (int)KeyCode.Alpha0).ToString();
            }
            else if (key == KeyCode.Alpha0)
            {
                return "0";
            }
            
            return key.ToString();
        }
        
        #endregion
    }
}
