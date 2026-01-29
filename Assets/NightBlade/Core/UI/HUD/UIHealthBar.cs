using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NightBlade.UI.HUD
{
    /// <summary>
    /// Modern, animated health bar for NightBlade.
    /// 
    /// Features:
    /// - Smooth lerping (no jarring updates)
    /// - Color gradient (green → yellow → red)
    /// - Damage flash effect
    /// - Heal pulse effect
    /// - Performance optimized (no Update loop!)
    /// 
    /// Example of the new NightBlade UI philosophy:
    /// - Event-driven (UpdateHealth called when HP changes)
    /// - Smooth animations
    /// - Clear, readable code
    /// - Easy for humans AND AI to modify
    /// </summary>
    public class UIHealthBar : UIBase
    {
        #region Serialized Fields
        
        [Header("Health Bar Components")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image flashOverlay;
        
        [Header("Health Bar Settings")]
        [Tooltip("How fast the bar lerps to new value (0 = instant)")]
        [SerializeField] private float lerpSpeed = 5f;
        
        [Tooltip("Color gradient: Green (full HP) → Yellow (mid HP) → Red (low HP)")]
        [SerializeField] private Gradient healthGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.2f, 0.2f), 0f),   // Red at 0%
                new GradientColorKey(new Color(1f, 1f, 0.2f), 0.5f),   // Yellow at 50%
                new GradientColorKey(new Color(0.2f, 1f, 0.2f), 1f)     // Green at 100%
            }
        };
        
        [Tooltip("Duration of damage flash effect")]
        [SerializeField] private float flashDuration = 0.2f;
        
        [Tooltip("Duration of heal pulse effect")]
        [SerializeField] private float pulseDuration = 0.3f;
        
        #endregion
        
        #region State
        
        private float currentHealth = 100f;
        private float maxHealth = 100f;
        private float targetFillAmount = 1f;
        private float displayedFillAmount = 1f;
        
        private Coroutine lerpCoroutine;
        private Coroutine flashCoroutine;
        private Coroutine pulseCoroutine;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            
            // Validate components
            if (fillImage == null)
            {
                Debug.LogError("[UIHealthBar] Fill Image not assigned!", this);
            }
            
            // Initialize fill image
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillAmount = 1f;
                fillImage.color = healthGradient.Evaluate(1f);
            }
            
            // Initialize flash overlay (invisible by default)
            if (flashOverlay != null)
            {
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Update health bar to new value (with smooth animation)
        /// </summary>
        /// <param name="current">Current health</param>
        /// <param name="max">Maximum health</param>
        /// <param name="animate">Should animate the change?</param>
        public void UpdateHealth(float current, float max, bool animate = true)
        {
            float previous = currentHealth;
            
            currentHealth = Mathf.Clamp(current, 0f, max);
            maxHealth = Mathf.Max(max, 1f); // Prevent divide by zero
            
            targetFillAmount = currentHealth / maxHealth;
            
            // Trigger appropriate effect
            if (current < previous)
            {
                // Took damage
                TriggerDamageFlash();
            }
            else if (current > previous)
            {
                // Healed
                TriggerHealPulse();
            }
            
            // Start lerping to new value
            if (animate)
            {
                if (lerpCoroutine != null)
                    StopCoroutine(lerpCoroutine);
                lerpCoroutine = StartCoroutine(LerpToTargetHealth());
            }
            else
            {
                // Update immediately
                displayedFillAmount = targetFillAmount;
                UpdateVisuals();
            }
        }
        
        /// <summary>
        /// Set health immediately without animation
        /// </summary>
        public void SetHealthImmediate(float current, float max)
        {
            UpdateHealth(current, max, animate: false);
        }
        
        /// <summary>
        /// Get current health percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return currentHealth / maxHealth;
        }
        
        #endregion
        
        #region Animation
        
        private IEnumerator LerpToTargetHealth()
        {
            while (Mathf.Abs(displayedFillAmount - targetFillAmount) > 0.001f)
            {
                displayedFillAmount = Mathf.Lerp(displayedFillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);
                UpdateVisuals();
                yield return null;
            }
            
            // Snap to final value
            displayedFillAmount = targetFillAmount;
            UpdateVisuals();
            
            lerpCoroutine = null;
        }
        
        private void UpdateVisuals()
        {
            if (fillImage == null)
                return;
            
            fillImage.fillAmount = displayedFillAmount;
            fillImage.color = healthGradient.Evaluate(displayedFillAmount);
        }
        
        #endregion
        
        #region Effects
        
        private void TriggerDamageFlash()
        {
            if (flashOverlay == null)
                return;
            
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(DamageFlashEffect());
        }
        
        private IEnumerator DamageFlashEffect()
        {
            float elapsed = 0f;
            
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                float alpha = Mathf.Lerp(0.5f, 0f, t);
                flashOverlay.color = new Color(1f, 0f, 0f, alpha);
                yield return null;
            }
            
            flashOverlay.color = new Color(1f, 0f, 0f, 0f);
            flashCoroutine = null;
        }
        
        private void TriggerHealPulse()
        {
            if (flashOverlay == null)
                return;
            
            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(HealPulseEffect());
        }
        
        private IEnumerator HealPulseEffect()
        {
            float elapsed = 0f;
            
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                float alpha = Mathf.Sin(t * Mathf.PI) * 0.3f; // Pulse in and out
                flashOverlay.color = new Color(0f, 1f, 0f, alpha);
                yield return null;
            }
            
            flashOverlay.color = new Color(0f, 1f, 0f, 0f);
            pulseCoroutine = null;
        }
        
        #endregion
        
        #region Cleanup
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // Stop any running animations
            if (lerpCoroutine != null)
                StopCoroutine(lerpCoroutine);
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            if (pulseCoroutine != null)
                StopCoroutine(pulseCoroutine);
        }
        
        protected override void OnReturnToPool()
        {
            base.OnReturnToPool();
            
            // Reset state when returning to pool
            currentHealth = 100f;
            maxHealth = 100f;
            targetFillAmount = 1f;
            displayedFillAmount = 1f;
            
            if (fillImage != null)
            {
                fillImage.fillAmount = 1f;
                fillImage.color = healthGradient.Evaluate(1f);
            }
            
            if (flashOverlay != null)
            {
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
            }
        }
        
        #endregion
    }
}
