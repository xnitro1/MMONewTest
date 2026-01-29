using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NightBlade.UI.Dialogue
{
    /// <summary>
    /// Specialized dialogue for quest offers and completions.
    /// Extends UIDialogue with quest-specific features.
    /// 
    /// Features:
    /// - Quest title/description
    /// - Objectives list
    /// - Rewards display (gold, items, XP)
    /// - Accept/Decline buttons
    /// - Turn-in validation
    /// 
    /// Usage:
    /// ```csharp
    /// UIQuestDialogue.Instance.ShowQuestOffer(
    ///     questData: myQuest,
    ///     onAccept: () => AcceptQuest(),
    ///     onDecline: () => DeclineQuest()
    /// );
    /// ```
    /// </summary>
    public class UIQuestDialogue : UIPanel
    {
        #region Singleton
        
        private static UIQuestDialogue instance;
        public static UIQuestDialogue Instance => instance;
        
        #endregion
        
        #region Serialized Fields
        
        [Header("Quest Components")]
        [SerializeField] private TextMeshProUGUI questTitleText;
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private Transform objectivesContainer;
        [SerializeField] private GameObject objectivePrefab;
        [SerializeField] private Transform rewardsContainer;
        [SerializeField] private GameObject rewardPrefab;
        
        [Header("NPC Info")]
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private Image npcPortrait;
        
        [Header("Buttons")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button declineButton;
        [SerializeField] private Button turnInButton;
        [SerializeField] private Button closeButton;
        
        [Header("Colors")]
        [SerializeField] private Color completeObjectiveColor = new Color(0.3f, 1f, 0.3f);
        [SerializeField] private Color incompleteObjectiveColor = new Color(1f, 0.3f, 0.3f);
        
        #endregion
        
        #region State
        
        private System.Action onAcceptCallback;
        private System.Action onDeclineCallback;
        private System.Action onTurnInCallback;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            
            // Singleton setup
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[UIQuestDialogue] Multiple instances detected! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            
            // Wire up buttons
            if (acceptButton != null)
                acceptButton.onClick.AddListener(OnAcceptClicked);
            if (declineButton != null)
                declineButton.onClick.AddListener(OnDeclineClicked);
            if (turnInButton != null)
                turnInButton.onClick.AddListener(OnTurnInClicked);
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
            
            // Start hidden
            Hide(animate: false);
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion
        
        #region Public API - Quest Offer
        
        /// <summary>
        /// Show quest offer dialogue
        /// </summary>
        public void ShowQuestOffer(
            string questTitle,
            string questDescription,
            string npcName,
            Sprite npcIcon,
            string[] objectives,
            QuestReward[] rewards,
            System.Action onAccept,
            System.Action onDecline)
        {
            // Store callbacks
            onAcceptCallback = onAccept;
            onDeclineCallback = onDecline;
            onTurnInCallback = null;
            
            // Set NPC info
            if (npcNameText != null)
                npcNameText.text = npcName;
            if (npcPortrait != null && npcIcon != null)
            {
                npcPortrait.sprite = npcIcon;
                npcPortrait.gameObject.SetActive(true);
            }
            
            // Set quest info
            if (questTitleText != null)
                questTitleText.text = questTitle;
            if (questDescriptionText != null)
                questDescriptionText.text = questDescription;
            
            // Show objectives
            DisplayObjectives(objectives, null);
            
            // Show rewards
            DisplayRewards(rewards);
            
            // Show accept/decline buttons
            if (acceptButton != null)
                acceptButton.gameObject.SetActive(true);
            if (declineButton != null)
                declineButton.gameObject.SetActive(true);
            if (turnInButton != null)
                turnInButton.gameObject.SetActive(false);
            
            // Show dialogue
            Show();
        }
        
        #endregion
        
        #region Public API - Quest Turn-In
        
        /// <summary>
        /// Show quest turn-in dialogue
        /// </summary>
        public void ShowQuestTurnIn(
            string questTitle,
            string completionText,
            string npcName,
            Sprite npcIcon,
            string[] objectives,
            bool[] objectiveCompletionStatus,
            QuestReward[] rewards,
            System.Action onTurnIn)
        {
            // Store callbacks
            onTurnInCallback = onTurnIn;
            onAcceptCallback = null;
            onDeclineCallback = null;
            
            // Set NPC info
            if (npcNameText != null)
                npcNameText.text = npcName;
            if (npcPortrait != null && npcIcon != null)
            {
                npcPortrait.sprite = npcIcon;
                npcPortrait.gameObject.SetActive(true);
            }
            
            // Set quest info
            if (questTitleText != null)
                questTitleText.text = questTitle;
            if (questDescriptionText != null)
                questDescriptionText.text = completionText;
            
            // Show objectives with completion status
            DisplayObjectives(objectives, objectiveCompletionStatus);
            
            // Show rewards
            DisplayRewards(rewards);
            
            // Show turn-in button (only if all objectives complete)
            bool allComplete = true;
            if (objectiveCompletionStatus != null)
            {
                foreach (bool status in objectiveCompletionStatus)
                {
                    if (!status)
                    {
                        allComplete = false;
                        break;
                    }
                }
            }
            
            if (acceptButton != null)
                acceptButton.gameObject.SetActive(false);
            if (declineButton != null)
                declineButton.gameObject.SetActive(false);
            if (turnInButton != null)
            {
                turnInButton.gameObject.SetActive(allComplete);
                turnInButton.interactable = allComplete;
            }
            
            // Show dialogue
            Show();
        }
        
        #endregion
        
        #region Private Methods
        
        private void DisplayObjectives(string[] objectives, bool[] completionStatus)
        {
            if (objectivesContainer == null || objectivePrefab == null)
                return;
            
            // Clear old objectives
            foreach (Transform child in objectivesContainer)
            {
                Destroy(child.gameObject);
            }
            
            if (objectives == null)
                return;
            
            // Create objective entries
            for (int i = 0; i < objectives.Length; i++)
            {
                GameObject objGO = Instantiate(objectivePrefab, objectivesContainer);
                TextMeshProUGUI objText = objGO.GetComponentInChildren<TextMeshProUGUI>();
                
                if (objText != null)
                {
                    bool isComplete = completionStatus != null && i < completionStatus.Length && completionStatus[i];
                    string checkmark = isComplete ? "✓ " : "○ ";
                    objText.text = checkmark + objectives[i];
                    objText.color = isComplete ? completeObjectiveColor : incompleteObjectiveColor;
                }
                
                objGO.SetActive(true);
            }
        }
        
        private void DisplayRewards(QuestReward[] rewards)
        {
            if (rewardsContainer == null || rewardPrefab == null)
                return;
            
            // Clear old rewards
            foreach (Transform child in rewardsContainer)
            {
                Destroy(child.gameObject);
            }
            
            if (rewards == null)
                return;
            
            // Create reward entries
            foreach (QuestReward reward in rewards)
            {
                GameObject rewardGO = Instantiate(rewardPrefab, rewardsContainer);
                
                // Set reward display (icon + amount)
                Image icon = rewardGO.GetComponentInChildren<Image>();
                TextMeshProUGUI amountText = rewardGO.GetComponentInChildren<TextMeshProUGUI>();
                
                if (icon != null && reward.icon != null)
                {
                    icon.sprite = reward.icon;
                }
                
                if (amountText != null)
                {
                    amountText.text = reward.GetDisplayText();
                }
                
                rewardGO.SetActive(true);
            }
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnAcceptClicked()
        {
            onAcceptCallback?.Invoke();
            Hide();
        }
        
        private void OnDeclineClicked()
        {
            onDeclineCallback?.Invoke();
            Hide();
        }
        
        private void OnTurnInClicked()
        {
            onTurnInCallback?.Invoke();
            Hide();
        }
        
        private void OnCloseClicked()
        {
            Hide();
        }
        
        #endregion
        
        #region UIPanel Overrides
        
        protected override void OnEscapePressed()
        {
            // ESC acts as decline/close
            if (onDeclineCallback != null)
            {
                onDeclineCallback.Invoke();
            }
            
            Hide();
        }
        
        #endregion
    }
    
    #region Quest Data Structures
    
    /// <summary>
    /// Quest reward data
    /// </summary>
    [System.Serializable]
    public struct QuestReward
    {
        public enum RewardType
        {
            Gold,
            Experience,
            Item,
            Currency
        }
        
        public RewardType type;
        public int amount;
        public Sprite icon;
        public string itemName;
        
        public string GetDisplayText()
        {
            switch (type)
            {
                case RewardType.Gold:
                    return $"{amount} Gold";
                case RewardType.Experience:
                    return $"{amount} XP";
                case RewardType.Item:
                    return $"{amount}x {itemName}";
                case RewardType.Currency:
                    return $"{amount}x {itemName}";
                default:
                    return amount.ToString();
            }
        }
    }
    
    #endregion
}
