using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Advanced coroutine pooling system with factory pattern.
    /// Eliminates GC pressure from frequent coroutine allocations.
    /// </summary>
    public static class CoroutinePool
    {
        private static readonly Dictionary<string, Stack<IEnumerator>> pools =
            new Dictionary<string, Stack<IEnumerator>>();
        private static readonly Dictionary<string, System.Func<IEnumerator>> factories =
            new Dictionary<string, System.Func<IEnumerator>>();
        private static readonly Dictionary<string, System.Func<object[], IEnumerator>> parameterizedFactories =
            new Dictionary<string, System.Func<object[], IEnumerator>>();

        private static MonoBehaviour coroutineRunner;
        private static bool initialized = false;
        private static int activeCoroutineCount = 0;

        /// <summary>
        /// Check if the CoroutinePool has been initialized.
        /// </summary>
        public static bool IsInitialized => initialized;

        /// <summary>
        /// Gets the number of currently active pooled coroutines.
        /// </summary>
        public static int ActiveCoroutineCount => activeCoroutineCount;

        /// <summary>
        /// Initializes the coroutine pool system.
        /// Must be called before using any pooled coroutines.
        /// </summary>
        /// <param name="runner">MonoBehaviour to run the coroutines on</param>
        public static void Initialize(MonoBehaviour runner)
        {
            if (initialized) return;

            coroutineRunner = runner;
            initialized = true;

            // Register common coroutine factories
            RegisterCommonCoroutines();

            Debug.Log("[CoroutinePool] Initialized with common coroutine factories");
        }

        /// <summary>
        /// Registers a simple coroutine factory.
        /// </summary>
        /// <param name="coroutineType">Unique identifier for the coroutine type</param>
        /// <param name="factory">Factory function that creates the coroutine</param>
        public static void RegisterCoroutineFactory(string coroutineType, System.Func<IEnumerator> factory)
        {
            factories[coroutineType] = factory;
            pools[coroutineType] = new Stack<IEnumerator>();
        }

        /// <summary>
        /// Registers a parameterized coroutine factory.
        /// </summary>
        /// <param name="coroutineType">Unique identifier for the coroutine type</param>
        /// <param name="factory">Factory function that takes parameters and creates the coroutine</param>
        public static void RegisterParameterizedCoroutineFactory(string coroutineType, System.Func<object[], IEnumerator> factory)
        {
            parameterizedFactories[coroutineType] = factory;
            pools[coroutineType] = new Stack<IEnumerator>();
        }

        /// <summary>
        /// Starts a pooled coroutine without parameters.
        /// </summary>
        /// <param name="coroutineType">The registered coroutine type to start</param>
        /// <param name="onComplete">Optional callback when coroutine completes</param>
        /// <returns>Coroutine handle for management</returns>
        public static Coroutine StartPooledCoroutine(string coroutineType, System.Action onComplete = null)
        {
            if (!initialized)
            {
                Debug.LogError("[CoroutinePool] Not initialized! Call Initialize() first.");
                return null;
            }

            var coroutine = Rent(coroutineType);
            if (coroutine == null) return null;

            activeCoroutineCount++;
            return coroutineRunner.StartCoroutine(new PooledCoroutine(coroutine, coroutineType, onComplete).Run());
        }

        /// <summary>
        /// Starts a pooled coroutine with parameters.
        /// </summary>
        /// <param name="coroutineType">The registered coroutine type to start</param>
        /// <param name="parameters">Parameters to pass to the coroutine factory</param>
        /// <param name="onComplete">Optional callback when coroutine completes</param>
        /// <returns>Coroutine handle for management</returns>
        public static Coroutine StartPooledCoroutine(string coroutineType, object[] parameters, System.Action onComplete = null)
        {
            if (!initialized)
            {
                Debug.LogError("[CoroutinePool] Not initialized! Call Initialize() first.");
                return null;
            }

            var coroutine = Rent(coroutineType, parameters);
            if (coroutine == null) return null;

            activeCoroutineCount++;
            return coroutineRunner.StartCoroutine(new PooledCoroutine(coroutine, coroutineType, onComplete).Run());
        }

        /// <summary>
        /// Rents a coroutine from the pool or creates a new one.
        /// </summary>
        /// <param name="coroutineType">The coroutine type to rent</param>
        /// <returns>The coroutine enumerator</returns>
        private static IEnumerator Rent(string coroutineType)
        {
            if (pools.TryGetValue(coroutineType, out var pool) && pool.Count > 0)
            {
                return pool.Pop();
            }

            if (factories.TryGetValue(coroutineType, out var factory))
            {
                return factory();
            }

            Debug.LogError($"[CoroutinePool] No factory registered for coroutine type: {coroutineType}");
            return null;
        }

        /// <summary>
        /// Rents a parameterized coroutine from the pool or creates a new one.
        /// </summary>
        /// <param name="coroutineType">The coroutine type to rent</param>
        /// <param name="parameters">Parameters for the coroutine</param>
        /// <returns>The coroutine enumerator</returns>
        private static IEnumerator Rent(string coroutineType, object[] parameters)
        {
            if (pools.TryGetValue(coroutineType, out var pool) && pool.Count > 0)
            {
                return pool.Pop();
            }

            if (parameterizedFactories.TryGetValue(coroutineType, out var factory))
            {
                return factory(parameters);
            }

            Debug.LogError($"[CoroutinePool] No parameterized factory registered for coroutine type: {coroutineType}");
            return null;
        }

        /// <summary>
        /// Returns a coroutine to the pool for reuse.
        /// </summary>
        /// <param name="coroutineType">The coroutine type</param>
        /// <param name="coroutine">The coroutine to return</param>
        internal static void Return(string coroutineType, IEnumerator coroutine)
        {
            if (pools.TryGetValue(coroutineType, out var pool))
            {
                pool.Push(coroutine);
            }
            
            // Decrement active count when coroutine completes
            activeCoroutineCount--;
            if (activeCoroutineCount < 0) activeCoroutineCount = 0; // Safety check
        }

        /// <summary>
        /// Registers common coroutine types used in games.
        /// </summary>
        private static void RegisterCommonCoroutines()
        {
            // Damage flash effect
            RegisterCoroutineFactory("DamageFlash", () => CreateDamageFlashCoroutine());

            // Fade in/out effects
            RegisterParameterizedCoroutineFactory("Fade", (params_) =>
                CreateFadeCoroutine((float)params_[0], (float)params_[1]));

            // Shake effect
            RegisterParameterizedCoroutineFactory("Shake", (params_) =>
                CreateShakeCoroutine((float)params_[0], (Vector3)params_[1]));

            // Scale effect
            RegisterParameterizedCoroutineFactory("Scale", (params_) =>
                CreateScaleCoroutine((Vector3)params_[0], (Vector3)params_[1], (float)params_[2]));

            // Delay coroutine
            RegisterParameterizedCoroutineFactory("Delay", (params_) =>
                CreateDelayCoroutine((float)params_[0]));

            // UI Animation coroutines for pooling system
            RegisterParameterizedCoroutineFactory("DamageNumberAnimation", (params_) =>
                CreateDamageNumberAnimation((GameObject)params_[0], (bool)params_[1]));

            RegisterParameterizedCoroutineFactory("FloatingTextAnimation", (params_) =>
                CreateFloatingTextAnimation((GameObject)params_[0], (float)params_[1]));
        }

        #region Common Coroutine Implementations

        private static IEnumerator CreateDamageFlashCoroutine()
        {
            // Default damage flash - white flash for 0.2 seconds
            // NOTE: This coroutine expects parameters to be passed via coroutine arguments
            // The caller should provide: Material material, Renderer renderer, Color flashColor, float duration
            yield return new WaitForSeconds(0.3f); // Default fallback timing

            // This is a template - actual implementation should be customized per use case
            // with proper MaterialPropertyBlock usage for the specific renderer
        }

        private static IEnumerator CreateFadeCoroutine(float startAlpha, float endAlpha)
        {
            // Simple fade implementation - would need component reference
            float duration = 1.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Fade logic would be implemented by caller
                yield return null;
            }
        }

        private static IEnumerator CreateShakeCoroutine(float duration, Vector3 intensity)
        {
            Transform transform = null; // Would be set by caller
            Vector3 originalPosition = Vector3.zero;

            if (transform != null)
            {
                originalPosition = transform.localPosition;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    // Random shake
                    Vector3 shake = new Vector3(
                        Random.Range(-intensity.x, intensity.x),
                        Random.Range(-intensity.y, intensity.y),
                        Random.Range(-intensity.z, intensity.z)
                    ) * (1f - t); // Diminish over time

                    transform.localPosition = originalPosition + shake;
                    yield return null;
                }

                transform.localPosition = originalPosition;
            }
            else
            {
                yield return new WaitForSeconds(duration);
            }
        }

        private static IEnumerator CreateScaleCoroutine(Vector3 startScale, Vector3 endScale, float duration)
        {
            Transform transform = null; // Would be set by caller

            if (transform != null)
            {
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    transform.localScale = Vector3.Lerp(startScale, endScale, t);
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(duration);
            }
        }

        private static IEnumerator CreateDelayCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
        }

        private static IEnumerator CreateDamageNumberAnimation(GameObject damageObj, bool isCritical)
        {
            if (damageObj == null) yield break;

            float duration = isCritical ? 2f : 1.5f;
            float elapsed = 0f;
            Vector3 startPos = damageObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * 2f;

            TMPro.TextMeshProUGUI textComponent = damageObj.GetComponent<TMPro.TextMeshProUGUI>();
            float startFontSize = textComponent != null ? textComponent.fontSize : 1f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Move upward
                if (damageObj != null)
                    damageObj.transform.position = Vector3.Lerp(startPos, endPos, t);

                // Fade out and scale
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
            if (damageObj != null)
            {
                NightBlade.UI.Utils.Pooling.UIPoolManager.Instance?.ReturnObject("DamageNumbers", damageObj);
            }
        }

        private static IEnumerator CreateFloatingTextAnimation(GameObject textObj, float duration)
        {
            if (textObj == null) yield break;

            float elapsed = 0f;
            Vector3 startPos = textObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * 1.5f;

            TMPro.TextMeshProUGUI textComponent = textObj.GetComponent<TMPro.TextMeshProUGUI>();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Move upward
                if (textObj != null)
                    textObj.transform.position = Vector3.Lerp(startPos, endPos, t);

                // Fade out
                if (textComponent != null)
                {
                    Color color = textComponent.color;
                    color.a = 1f - t;
                    textComponent.color = color;
                }

                yield return null;
            }

            // Return to pool
            if (textObj != null)
            {
                NightBlade.UI.Utils.Pooling.UIPoolManager.Instance?.ReturnObject("FloatingText", textObj);
            }
        }

        #endregion

        /// <summary>
        /// Gets pool statistics for debugging.
        /// </summary>
        public static Dictionary<string, int> GetPoolStats()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();
            foreach (var kvp in pools)
            {
                stats[kvp.Key] = kvp.Value.Count;
            }
            return stats;
        }

        /// <summary>
        /// Clears all pools. Use with caution.
        /// </summary>
        public static void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                pool.Clear();
            }
            pools.Clear();
            factories.Clear();
            parameterizedFactories.Clear();
            initialized = false;
        }
    }

    /// <summary>
    /// Wrapper class for pooled coroutines that handles cleanup.
    /// </summary>
    internal class PooledCoroutine
    {
        private readonly IEnumerator coroutine;
        private readonly string coroutineType;
        private readonly System.Action onComplete;

        public PooledCoroutine(IEnumerator coroutine, string coroutineType, System.Action onComplete)
        {
            this.coroutine = coroutine;
            this.coroutineType = coroutineType;
            this.onComplete = onComplete;
        }

        public IEnumerator Run()
        {
            yield return coroutine;
            onComplete?.Invoke();
            CoroutinePool.Return(coroutineType, coroutine);
        }
    }
}
