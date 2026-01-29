using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NightBlade.UI.Utils.Pooling
{
    /// <summary>
    /// Generic UI object pooling system with automatic state reset.
    /// Eliminates GC pressure from frequent UI element creation/destruction.
    /// </summary>
    public class UIPoolManager : MonoBehaviour
    {
        private static UIPoolManager instance;
        public static UIPoolManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<UIPoolManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("UIPoolManager");
                        instance = go.AddComponent<UIPoolManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        private readonly Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> templates = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Gets the total number of pooled UI objects across all pools.
        /// </summary>
        public static int GetTotalPooledObjects()
        {
            int total = 0;
            foreach (var pool in Instance.pools)
            {
                total += pool.Value.Count;
            }
            return total;
        }

        /// <summary>
        /// Registers a UI template for pooling.
        /// </summary>
        /// <param name="poolKey">Unique identifier for the pool</param>
        /// <param name="template">The prefab template to pool</param>
        public void RegisterTemplate(string poolKey, GameObject template)
        {
            if (template == null)
            {
                Debug.LogWarning($"[UIPoolManager] Attempted to register null template for pool '{poolKey}'");
                return;
            }

            Debug.Log($"[UIPoolManager] Registering template '{template.name}' for pool '{poolKey}'");

            // Parent the template to the pool manager to prevent scene destruction
            template.transform.SetParent(transform);

            if (!templates.ContainsKey(poolKey))
            {
                templates[poolKey] = template;
                pools[poolKey] = new Queue<GameObject>();
                Debug.Log($"[UIPoolManager] Successfully registered template for pool '{poolKey}'");
            }
            else
            {
                Debug.Log($"[UIPoolManager] Template already exists for pool '{poolKey}', replacing");
                // Destroy old template if it exists
                if (templates[poolKey] != null)
                {
                    Destroy(templates[poolKey]);
                }
                templates[poolKey] = template;
            }
        }

        /// <summary>
        /// Gets an object from the specified pool, creating one if necessary.
        /// </summary>
        /// <param name="poolKey">The pool identifier</param>
        /// <returns>A clean, ready-to-use UI object</returns>
        public GameObject GetObject(string poolKey)
        {
            if (!pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[UIPoolManager] Pool '{poolKey}' not registered!");
                return null;
            }

            // Validate template exists and is not destroyed
            EnsureTemplateExists(poolKey);

            GameObject obj;
            if (pools[poolKey].Count > 0)
            {
                obj = pools[poolKey].Dequeue();
            }
            else
            {
                if (!templates.ContainsKey(poolKey) || templates[poolKey] == null)
                {
                    Debug.LogError($"[UIPoolManager] Template for pool '{poolKey}' is not available!");
                    return null;
                }

                obj = Instantiate(templates[poolKey]);
            }

            obj.SetActive(true);
            ResetObjectState(obj);
            return obj;
        }

        /// <summary>
        /// Ensures that a template exists for the specified pool, recreating if necessary
        /// </summary>
        private void EnsureTemplateExists(string poolKey)
        {
            if (!templates.ContainsKey(poolKey) || templates[poolKey] == null)
            {
                if (!templates.ContainsKey(poolKey))
                {
                    Debug.LogWarning($"[UIPoolManager] Template for pool '{poolKey}' was never registered. Attempting to recreate...");
                }
                else
                {
                    Debug.LogWarning($"[UIPoolManager] Template for pool '{poolKey}' was destroyed. Attempting to recreate...");
                }

                TryRecreateTemplate(poolKey);
            }
        }

        /// <summary>
        /// Returns an object to its pool for reuse.
        /// </summary>
        /// <param name="poolKey">The pool identifier</param>
        /// <param name="obj">The object to return</param>
        public void ReturnObject(string poolKey, GameObject obj)
        {
            if (obj == null || !pools.ContainsKey(poolKey))
            {
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            ResetObjectState(obj);
            pools[poolKey].Enqueue(obj);
        }

        /// <summary>
        /// Resets a UI object's state for clean reuse.
        /// </summary>
        /// <param name="obj">The object to reset</param>
        private void ResetObjectState(GameObject obj)
        {
            // Reset transform
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;

            // Reset common UI components
            Text textComponent = obj.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = string.Empty;
                textComponent.color = Color.white;
            }

            TextMeshProUGUI tmpComponent = obj.GetComponent<TextMeshProUGUI>();
            if (tmpComponent != null)
            {
                tmpComponent.text = string.Empty;
                tmpComponent.color = Color.white;
                tmpComponent.fontSize = 24f; // Default size
            }

            Image imageComponent = obj.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.sprite = null;
                imageComponent.color = Color.white;
            }

            // Reset Canvas Group
            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            // Reset any animator
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }

        /// <summary>
        /// Pre-warms a pool with a specified number of objects.
        /// </summary>
        /// <param name="poolKey">The pool to pre-warm</param>
        /// <param name="count">Number of objects to create</param>
        public void PreWarmPool(string poolKey, int count)
        {
            Debug.Log($"[UIPoolManager] Pre-warming pool '{poolKey}' with {count} objects");
            if (!pools.ContainsKey(poolKey))
            {
                Debug.LogWarning($"[UIPoolManager] Pool '{poolKey}' does not exist in pools dictionary");
                return;
            }
            if (!templates.ContainsKey(poolKey))
            {
                Debug.LogWarning($"[UIPoolManager] Template '{poolKey}' does not exist in templates dictionary");
                return;
            }

            GameObject template = templates[poolKey];
            if (template == null)
            {
                Debug.LogWarning($"[UIPoolManager] Cannot pre-warm pool '{poolKey}' - template is null");
                return;
            }

            Debug.Log($"[UIPoolManager] Starting pre-warm loop for pool '{poolKey}'");
            for (int i = 0; i < count; i++)
            {
                try
                {
                    GameObject obj = Instantiate(template);
                    if (obj == null)
                    {
                        Debug.LogWarning($"[UIPoolManager] Instantiate returned null for template '{template.name}' in pool '{poolKey}'");
                        continue;
                    }
                    obj.SetActive(false);
                    obj.transform.SetParent(transform);
                    ResetObjectState(obj);
                    pools[poolKey].Enqueue(obj);
                    Debug.Log($"[UIPoolManager] Successfully added object {i+1}/{count} to pool '{poolKey}'");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[UIPoolManager] Failed to instantiate object for pool '{poolKey}': {ex.Message}");
                }
            }
            Debug.Log($"[UIPoolManager] Pre-warm completed for pool '{poolKey}'. Pool size: {pools[poolKey].Count}");
        }

        /// <summary>
        /// Clears all pools. Use with caution.
        /// </summary>
        public void ClearAllPools()
        {
            Debug.Log($"[UIPoolManager] ClearAllPools called. Clearing {pools.Count} pools and {templates.Count} templates");
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }
            foreach (var template in templates.Values)
            {
                if (template != null)
                {
                    Destroy(template);
                }
            }
            pools.Clear();
            templates.Clear();
            Debug.Log("[UIPoolManager] ClearAllPools completed");
        }

        /// <summary>
        /// Gets pool statistics for debugging.
        /// </summary>
        /// <returns>Dictionary of pool names and their current sizes</returns>
        public Dictionary<string, int> GetPoolStats()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();
            foreach (var kvp in pools)
            {
                stats[kvp.Key] = kvp.Value.Count;
            }
            return stats;
        }

        /// <summary>
        /// Attempts to recreate a template for a known pool type
        /// </summary>
        private bool TryRecreateTemplate(string poolKey)
        {
            try
            {
                switch (poolKey)
                {
                    case "DamageNumbers":
                        return TryRecreateDamageNumberTemplate();
                    case "FloatingText":
                        return TryRecreateFloatingTextTemplate();
                    default:
                        Debug.LogWarning($"[UIPoolManager] Unknown pool type '{poolKey}' - cannot recreate template");
                        return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UIPoolManager] Failed to recreate template for '{poolKey}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to recreate the damage number template
        /// </summary>
        private bool TryRecreateDamageNumberTemplate()
        {
            try
            {
                GameObject template = new GameObject("DamageNumberTemplate");
                template.SetActive(false);

                var textComponent = template.AddComponent<TMPro.TextMeshProUGUI>();

                // Try to use TMP font if available, otherwise let TMP handle the default
                if (TMPro.TMP_Settings.defaultFontAsset != null)
                {
                    textComponent.font = TMPro.TMP_Settings.defaultFontAsset;
                }
                else
                {
                    Debug.LogWarning("[UIPoolManager] TMP font not available yet - using default font handling");
                }

                textComponent.fontSize = 24;
                textComponent.alignment = TMPro.TextAlignmentOptions.Center;
                textComponent.color = Color.white;

                var rectTransform = template.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(200, 50);

                templates["DamageNumbers"] = template;
                Debug.Log("[UIPoolManager] Successfully recreated damage number template");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UIPoolManager] Failed to create damage number template: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to recreate the floating text template
        /// </summary>
        private bool TryRecreateFloatingTextTemplate()
        {
            try
            {
                GameObject template = new GameObject("FloatingTextTemplate");
                template.SetActive(false);

                var textComponent = template.AddComponent<TMPro.TextMeshProUGUI>();

                // Try to use TMP font if available, otherwise let TMP handle the default
                if (TMPro.TMP_Settings.defaultFontAsset != null)
                {
                    textComponent.font = TMPro.TMP_Settings.defaultFontAsset;
                }
                else
                {
                    Debug.LogWarning("[UIPoolManager] TMP font not available yet - using default font handling");
                }

                textComponent.fontSize = 24;
                textComponent.alignment = TMPro.TextAlignmentOptions.Center;
                textComponent.color = Color.white;

                var rectTransform = template.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(300, 60);

                templates["FloatingText"] = template;
                Debug.Log("[UIPoolManager] Successfully recreated floating text template");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[UIPoolManager] Failed to create floating text template: {ex.Message}");
                return false;
            }
        }
    }
}
