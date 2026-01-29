using UnityEngine;
using TMPro;

namespace NightBlade.UI.Utils.Pooling
{
    /// <summary>
    /// Specialized pool for floating damage/healing numbers.
    /// Optimized for combat scenarios with frequent creation/destruction.
    /// </summary>
    public class UIDamageNumberPool : MonoBehaviour
    {
        private static UIDamageNumberPool _instance;
        public static UIDamageNumberPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIDamageNumberPool>();
                }
                return _instance;
            }
        }

        [Header("Pool Configuration")]
        [SerializeField] private GameObject damageNumberTemplate;
        [SerializeField] private int preWarmCount = 20;
        [SerializeField] private Transform poolParent;

        /// <summary>
        /// Public access to the damage number template for runtime pool creation.
        /// </summary>
        public GameObject DamageNumberTemplate
        {
            get { return damageNumberTemplate; }
            set { damageNumberTemplate = value; }
        }

        private const string POOL_KEY = "DamageNumbers";

        // Cache main camera to avoid repeated FindObjectOfType calls
        private static Camera _mainCamera;
        private static Camera MainCamera
        {
            get
            {
                if (_mainCamera == null)
                    _mainCamera = Camera.main;
                return _mainCamera;
            }
        }

        // Pre-allocated object arrays to avoid GC pressure
        private static readonly object[] DamageAnimationArgs = new object[2];
        private static readonly object[] CriticalAnimationArgs = new object[2];

        private void Awake()
        {
            if (poolParent == null)
            {
                poolParent = transform;
            }

            // Only register and pre-warm if we have a template (will be handled by GameInstance if programmatic)
            if (damageNumberTemplate != null)
            {
                UIPoolManager.Instance.RegisterTemplate(POOL_KEY, damageNumberTemplate);
                // Don't pre-warm here - let GameInstance handle it
                // UIPoolManager.Instance.PreWarmPool(POOL_KEY, preWarmCount);
            }
        }

        /// <summary>
        /// Shows a damage number at the specified position (static method for easy access).
        /// </summary>
        /// <param name="position">World position to display the number</param>
        /// <param name="damage">Damage amount to display</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        /// <param name="isHealing">Whether this is healing instead of damage</param>
        public static void ShowDamageNumber(Vector3 position, int damage, bool isCritical = false, bool isHealing = false)
        {
            if (Instance != null)
            {
                Instance.ShowDamageNumberInternal(position, damage, isCritical, isHealing);
            }
        }

        /// <summary>
        /// Shows a damage number at the specified position.
        /// </summary>
        /// <param name="position">World position to display the number</param>
        /// <param name="damage">Damage amount to display</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        /// <param name="isHealing">Whether this is healing instead of damage</param>
        private void ShowDamageNumberInternal(Vector3 position, int damage, bool isCritical = false, bool isHealing = false)
        {
            GameObject damageObj = UIPoolManager.Instance.GetObject(POOL_KEY);
            if (damageObj == null) return;

            // Position in world space
            damageObj.transform.position = position;

            // Set text and color
            TextMeshProUGUI textComponent = damageObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = damage.ToString();

                if (isHealing)
                {
                    textComponent.color = Color.green;
                }
                else if (isCritical)
                {
                    textComponent.color = Color.red;
                    textComponent.fontSize *= 1.5f; // Make critical hits bigger
                }
                else
                {
                    textComponent.color = Color.yellow;
                }
            }

            // Add floating animation (pooled coroutine for GC optimization)
            // Check if CoroutinePool is initialized, otherwise use regular coroutine
            if (NightBlade.Core.Utils.CoroutinePool.IsInitialized)
            {
                // Use pre-allocated array to avoid GC pressure
                DamageAnimationArgs[0] = damageObj;
                DamageAnimationArgs[1] = isCritical;
                NightBlade.Core.Utils.CoroutinePool.StartPooledCoroutine("DamageNumberAnimation", DamageAnimationArgs);
            }
            else
            {
                // Fallback to regular coroutine if pool not ready yet
                StartCoroutine(AnimateAndReturn(damageObj, isCritical));
            }
        }

        /// <summary>
        /// Shows a damage number in screen space (for UI elements).
        /// </summary>
        /// <param name="screenPosition">Screen position to display the number</param>
        /// <param name="damage">Damage amount to display</param>
        /// <param name="isCritical">Whether this is a critical hit</param>
        /// <param name="isHealing">Whether this is healing instead of damage</param>
        public void ShowDamageNumberScreen(Vector2 screenPosition, int damage, bool isCritical = false, bool isHealing = false)
        {
            GameObject damageObj = UIPoolManager.Instance.GetObject(POOL_KEY);
            if (damageObj == null) return;

            // Convert screen position to world space
            damageObj.transform.position = MainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));

            // Set text and color (same as world space version)
            TextMeshProUGUI textComponent = damageObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = damage.ToString();

                if (isHealing)
                {
                    textComponent.color = Color.green;
                }
                else if (isCritical)
                {
                    textComponent.color = Color.red;
                    textComponent.fontSize *= 1.5f;
                }
                else
                {
                    textComponent.color = Color.yellow;
                }
            }

            // Add floating animation (pooled coroutine for GC optimization)
            // Check if CoroutinePool is initialized, otherwise use regular coroutine
            if (NightBlade.Core.Utils.CoroutinePool.IsInitialized)
            {
                // Use pre-allocated array to avoid GC pressure
                DamageAnimationArgs[0] = damageObj;
                DamageAnimationArgs[1] = isCritical;
                NightBlade.Core.Utils.CoroutinePool.StartPooledCoroutine("DamageNumberAnimation", DamageAnimationArgs);
            }
            else
            {
                // Fallback to regular coroutine if pool not ready yet
                StartCoroutine(AnimateAndReturn(damageObj, isCritical));
            }
        }

        private System.Collections.IEnumerator AnimateAndReturn(GameObject damageObj, bool isCritical)
        {
            float duration = isCritical ? 2f : 1.5f;
            float elapsed = 0f;
            Vector3 startPos = damageObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * 2f;

            TextMeshProUGUI textComponent = damageObj.GetComponent<TextMeshProUGUI>();
            float startFontSize = textComponent != null ? textComponent.fontSize : 1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Move upward
                damageObj.transform.position = Vector3.Lerp(startPos, endPos, t);

                // Fade out
                if (textComponent != null)
                {
                    Color color = textComponent.color;
                    color.a = 1f - t;
                    textComponent.color = color;

                    // Scale down for critical hits
                    if (isCritical)
                    {
                        float scale = Mathf.Lerp(1.5f, 0.8f, t);
                        textComponent.fontSize = startFontSize * scale;
                    }
                }

                yield return null;
            }

            // Return to pool
            if (textComponent != null && isCritical)
            {
                textComponent.fontSize = startFontSize / 1.5f; // Reset font size
            }

            UIPoolManager.Instance.ReturnObject(POOL_KEY, damageObj);
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
