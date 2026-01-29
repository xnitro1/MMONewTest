using UnityEngine;
using TMPro;

namespace NightBlade.UI.Utils.Pooling
{
    /// <summary>
    /// Specialized pool for floating text messages (status effects, chat bubbles, etc.).
    /// Optimized for frequent text display scenarios.
    /// </summary>
    public class UIFloatingTextPool : MonoBehaviour
    {
        private static UIFloatingTextPool _instance;
        public static UIFloatingTextPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIFloatingTextPool>();
                }
                return _instance;
            }
        }

        [Header("Pool Configuration")]
        [SerializeField] private GameObject floatingTextTemplate;
        [SerializeField] private int preWarmCount = 15;
        [SerializeField] private Transform poolParent;

        private const string POOL_KEY = "FloatingText";

        // Pre-allocated object arrays to avoid GC pressure
        private static readonly object[] FloatingTextAnimationArgs = new object[2];

        // Pre-allocated strings to avoid GC pressure from string interpolation
        private const string XP_PREFIX = " XP";
        private const string LEVEL_PREFIX = "LEVEL ";
        private const string LEVEL_SUFFIX = "!";

        // Pre-allocated colors to avoid struct allocations
        private static readonly Color BUFF_COLOR = new Color(0.2f, 1f, 0.2f);
        private static readonly Color DEBUFF_COLOR = new Color(1f, 0.2f, 0.2f);
        private static readonly Color XP_COLOR = new Color(0.8f, 0.8f, 1f);
        private static readonly Color LEVEL_COLOR = new Color(1f, 0.8f, 0f);

        // Pre-allocated status effect prefixes
        private const string BUFF_PREFIX = "✓ ";
        private const string DEBUFF_PREFIX = "✗ ";

        private void Awake()
        {
            if (poolParent == null)
            {
                poolParent = transform;
            }

            // Only register if we have a template (will be handled by GameInstance if programmatic)
            if (floatingTextTemplate != null)
            {
                UIPoolManager.Instance.RegisterTemplate(POOL_KEY, floatingTextTemplate);
                // Don't pre-warm here - let GameInstance handle it
                // UIPoolManager.Instance.PreWarmPool(POOL_KEY, preWarmCount);
            }
        }

        /// <summary>
        /// Shows floating text at a world position (static method for easy access).
        /// </summary>
        /// <param name="position">World position to display the text</param>
        /// <param name="message">Text message to display</param>
        /// <param name="textColor">Color of the text</param>
        public static void ShowWorldText(Vector3 position, string message, Color textColor)
        {
            if (Instance != null)
            {
                Instance.ShowFloatingText(position, message, textColor, 2f, 24f);
            }
        }

        /// <summary>
        /// Shows floating text at a world position.
        /// </summary>
        /// <param name="position">World position to display the text</param>
        /// <param name="message">Text message to display</param>
        /// <param name="textColor">Color of the text</param>
        /// <param name="duration">How long to display the text</param>
        /// <param name="fontSize">Size of the text</param>
        private void ShowFloatingText(Vector3 position, string message, Color textColor, float duration = 2f, float fontSize = 24f)
        {
            GameObject textObj = UIPoolManager.Instance.GetObject(POOL_KEY);
            if (textObj == null) return;

            // Position in world space
            textObj.transform.position = position;

            // Set text properties
            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = message;
                textComponent.color = textColor;
                textComponent.fontSize = fontSize;
            }

            // Start animation (pooled coroutine for GC optimization)
            // Check if CoroutinePool is initialized, otherwise use regular coroutine
            if (NightBlade.Core.Utils.CoroutinePool.IsInitialized)
            {
                // Use pre-allocated array to avoid GC pressure
                FloatingTextAnimationArgs[0] = textObj;
                FloatingTextAnimationArgs[1] = duration;
                NightBlade.Core.Utils.CoroutinePool.StartPooledCoroutine("FloatingTextAnimation", FloatingTextAnimationArgs);
            }
            else
            {
                // Fallback to regular coroutine if pool not ready yet
                StartCoroutine(AnimateAndReturn(textObj, duration));
            }
        }

        /// <summary>
        /// Shows floating text above a character's head.
        /// </summary>
        /// <param name="characterTransform">Transform of the character to display text above</param>
        /// <param name="message">Text message to display</param>
        /// <param name="textColor">Color of the text</param>
        /// <param name="heightOffset">Height offset from character position</param>
        /// <param name="duration">How long to display the text</param>
        public void ShowCharacterText(Transform characterTransform, string message, Color textColor,
                                    float heightOffset = 2f, float duration = 2f)
        {
            Vector3 position = characterTransform.position + Vector3.up * heightOffset;
            ShowFloatingText(position, message, textColor, duration);
        }

        /// <summary>
        /// Shows a status effect message (buff/debuff notification).
        /// </summary>
        /// <param name="position">World position to display the text</param>
        /// <param name="effectName">Name of the status effect</param>
        /// <param name="isBuff">Whether this is a beneficial effect</param>
        public void ShowStatusEffect(Vector3 position, string effectName, bool isBuff)
        {
            Color textColor = isBuff ? BUFF_COLOR : DEBUFF_COLOR;
            string prefix = isBuff ? BUFF_PREFIX : DEBUFF_PREFIX;
            ShowFloatingText(position, prefix + effectName, textColor, 3f, 20f);
        }

        /// <summary>
        /// Shows an experience gain notification.
        /// </summary>
        /// <param name="position">World position to display the text</param>
        /// <param name="expAmount">Amount of experience gained</param>
        public void ShowExperienceGain(Vector3 position, int expAmount)
        {
            ShowFloatingText(position, "+" + expAmount.ToString() + XP_PREFIX, XP_COLOR, 2f, 18f);
        }

        /// <summary>
        /// Shows a level up notification.
        /// </summary>
        /// <param name="position">World position to display the text</param>
        /// <param name="newLevel">New level achieved</param>
        public void ShowLevelUp(Vector3 position, int newLevel)
        {
            ShowFloatingText(position, LEVEL_PREFIX + newLevel.ToString() + LEVEL_SUFFIX, LEVEL_COLOR, 4f, 32f);
        }

        private System.Collections.IEnumerator AnimateAndReturn(GameObject textObj, float duration)
        {
            float elapsed = 0f;
            Vector3 startPos = textObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * 1.5f;

            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            float startAlpha = textComponent != null ? textComponent.color.a : 1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Gentle upward movement
                float yOffset = Mathf.Sin(t * Mathf.PI) * 0.3f; // Slight wave motion
                textObj.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * yOffset;

                // Fade out over time
                if (textComponent != null)
                {
                    Color color = textComponent.color;
                    color.a = startAlpha * (1f - t);
                    textComponent.color = color;
                }

                yield return null;
            }

            // Return to pool
            UIPoolManager.Instance.ReturnObject(POOL_KEY, textObj);
        }

        /// <summary>
        /// Gets the current pool size for debugging.
        /// </summary>
        public int GetPoolSize()
        {
            var stats = UIPoolManager.Instance.GetPoolStats();
            return stats.ContainsKey(POOL_KEY) ? stats[POOL_KEY] : 0;
        }
    }
}
