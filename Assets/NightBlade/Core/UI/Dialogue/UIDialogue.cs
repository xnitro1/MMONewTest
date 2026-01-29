using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace NightBlade.UI.Dialogue
{
    /// <summary>
    /// Modern dialogue system for NPC conversations, quests, and interactions.
    /// Clean, modular, and sexy AF. 💬✨
    /// 
    /// Features:
    /// - Typewriter text effect
    /// - Multiple choice buttons
    /// - NPC portrait/icon
    /// - Skip dialogue
    /// - Auto-advance option
    /// - Event callbacks
    /// - Fully animated
    /// 
    /// Design Philosophy:
    /// - Event-driven (ShowDialogue called when needed)
    /// - Clean API for quest/NPC systems
    /// - Smooth animations
    /// - Easy to extend
    /// 
    /// Usage:
    /// ```csharp
    /// UIDialogue.Instance.ShowDialogue(
    ///     npcName: "Mysterious Stranger",
    ///     dialogueText: "Greetings, traveler...",
    ///     choices: new string[] { "Hello", "Who are you?", "Goodbye" },
    ///     onChoiceSelected: (index) => HandleChoice(index)
    /// );
    /// ```
    /// </summary>
    public class UIDialogue : UIPanel
    {
        #region Singleton
        
        private static UIDialogue instance;
        public static UIDialogue Instance => instance;
        
        #endregion
        
        #region Serialized Fields
        
        [Header("Dialogue Components")]
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image npcPortrait;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button skipButton;
        
        [Header("Typewriter Settings")]
        [SerializeField] private bool enableTypewriter = true;
        [SerializeField] private float typewriterSpeed = 0.05f;
        [SerializeField] private bool allowSkipTypewriter = true;
        
        [Header("Audio (Optional)")]
        [SerializeField] private AudioSource dialogueAudioSource;
        [SerializeField] private AudioClip typewriterSound;
        [SerializeField] private AudioClip choiceSound;
        
        #endregion
        
        #region State
        
        private List<Button> activeChoiceButtons = new List<Button>();
        private Coroutine typewriterCoroutine;
        private bool isTyping = false;
        private string currentFullText = "";
        private System.Action<int> currentChoiceCallback;
        
        #endregion
        
        #region Properties
        
        public bool IsTyping => isTyping;
        public bool IsShowingDialogue => IsVisible;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            
            // Singleton setup
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[UIDialogue] Multiple UIDialogue instances detected! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            
            // Wire up buttons
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
            
            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
                skipButton.gameObject.SetActive(false);
            }
            
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
            
            // Clean up button listeners
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(OnContinueClicked);
            }
            
            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(OnSkipClicked);
            }
        }
        
        private void Update()
        {
            // Allow clicking anywhere to skip typewriter
            if (isTyping && allowSkipTypewriter)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    SkipTypewriter();
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Show dialogue with optional choices
        /// </summary>
        public void ShowDialogue(
            string npcName,
            string dialogueText,
            Sprite npcIcon = null,
            string[] choices = null,
            System.Action<int> onChoiceSelected = null)
        {
            // Store data
            currentFullText = dialogueText;
            currentChoiceCallback = onChoiceSelected;
            
            // Set NPC name
            if (npcNameText != null)
            {
                npcNameText.text = npcName;
            }
            
            // Set NPC portrait
            if (npcPortrait != null && npcIcon != null)
            {
                npcPortrait.sprite = npcIcon;
                npcPortrait.gameObject.SetActive(true);
            }
            else if (npcPortrait != null)
            {
                npcPortrait.gameObject.SetActive(false);
            }
            
            // Clear old choices
            ClearChoices();
            
            // Show choices if provided
            if (choices != null && choices.Length > 0)
            {
                CreateChoiceButtons(choices);
                if (continueButton != null)
                    continueButton.gameObject.SetActive(false);
            }
            else
            {
                if (continueButton != null)
                    continueButton.gameObject.SetActive(true);
            }
            
            // Show skip button during typewriter
            if (skipButton != null && enableTypewriter)
            {
                skipButton.gameObject.SetActive(true);
            }
            
            // Start typewriter effect
            if (enableTypewriter)
            {
                StartTypewriter(dialogueText);
            }
            else
            {
                if (this.dialogueText != null)
                {
                    this.dialogueText.text = dialogueText;
                }
                
                if (skipButton != null)
                {
                    skipButton.gameObject.SetActive(false);
                }
            }
            
            // Show the dialogue panel
            Show();
        }
        
        /// <summary>
        /// Close dialogue
        /// </summary>
        public void CloseDialogue()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            
            isTyping = false;
            ClearChoices();
            currentChoiceCallback = null;
            
            Hide();
        }
        
        #endregion
        
        #region Typewriter Effect
        
        private void StartTypewriter(string text)
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            
            typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
        }
        
        private IEnumerator TypewriterEffect(string text)
        {
            isTyping = true;
            
            if (dialogueText != null)
            {
                dialogueText.text = "";
            }
            
            for (int i = 0; i < text.Length; i++)
            {
                if (dialogueText != null)
                {
                    dialogueText.text += text[i];
                }
                
                // Play typewriter sound
                if (typewriterSound != null && dialogueAudioSource != null)
                {
                    // Play every few characters to avoid spam
                    if (i % 3 == 0)
                    {
                        dialogueAudioSource.PlayOneShot(typewriterSound, 0.3f);
                    }
                }
                
                yield return new WaitForSeconds(typewriterSpeed);
            }
            
            isTyping = false;
            typewriterCoroutine = null;
            
            // Hide skip button when done
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
            }
        }
        
        private void SkipTypewriter()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            
            if (dialogueText != null)
            {
                dialogueText.text = currentFullText;
            }
            
            isTyping = false;
            
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
            }
        }
        
        #endregion
        
        #region Choices
        
        private void CreateChoiceButtons(string[] choices)
        {
            if (choicesContainer == null || choiceButtonPrefab == null)
            {
                Debug.LogWarning("[UIDialogue] Cannot create choice buttons - missing container or prefab!");
                return;
            }
            
            for (int i = 0; i < choices.Length; i++)
            {
                int choiceIndex = i; // Capture for lambda
                string choiceText = choices[i];
                
                Button button = Instantiate(choiceButtonPrefab, choicesContainer);
                button.gameObject.SetActive(true);
                
                // Set button text
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = choiceText;
                }
                
                // Wire up click event
                button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
                
                activeChoiceButtons.Add(button);
            }
        }
        
        private void ClearChoices()
        {
            foreach (Button button in activeChoiceButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            
            activeChoiceButtons.Clear();
        }
        
        private void OnChoiceSelected(int choiceIndex)
        {
            // Play choice sound
            if (choiceSound != null && dialogueAudioSource != null)
            {
                dialogueAudioSource.PlayOneShot(choiceSound);
            }
            
            // Invoke callback
            currentChoiceCallback?.Invoke(choiceIndex);
            
            // Close dialogue
            CloseDialogue();
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnContinueClicked()
        {
            if (isTyping)
            {
                SkipTypewriter();
            }
            else
            {
                // No more dialogue, close
                CloseDialogue();
            }
        }
        
        private void OnSkipClicked()
        {
            if (isTyping)
            {
                SkipTypewriter();
            }
        }
        
        #endregion
        
        #region UIPanel Overrides
        
        protected override void OnEscapePressed()
        {
            // ESC closes dialogue
            CloseDialogue();
        }
        
        protected override void OnHideComplete()
        {
            base.OnHideComplete();
            
            // Clean up when hidden
            ClearChoices();
            currentChoiceCallback = null;
            
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            
            isTyping = false;
        }
        
        #endregion
    }
}
