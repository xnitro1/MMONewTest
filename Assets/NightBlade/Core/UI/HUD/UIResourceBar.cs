using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace NightBlade.UI.HUD
{
    /// <summary>
    /// Versatile resource bar for HP, MP, Stamina, Food, Water, etc.
    /// Modern, smooth, and sexy AF. 🔥
    /// 
    /// Features:
    /// - Smooth lerping (buttery animations)
    /// - Color gradients (customizable per resource type)
    /// - Flash effects (damage/heal/depletion)
    /// - Optional text display (current/max or percentage)
    /// - Event-driven (no Update() spam!)
    /// - Pooling-friendly
    /// 
    /// Design Philosophy:
    /// - ONE component for ALL resource types
    /// - Configure in Inspector, not code
    /// - Clear, readable, maintainable
    /// </summary>
    public class UIResourceBar : UIBase
    {
        #region Resource Types
        
        public enum ResourceType
        {
            Health,     // Red gradient
            Mana,       // Blue gradient
            Stamina,    // Yellow gradient
            Food,       // Brown gradient
            Water,      // Cyan gradient
            Experience, // Purple gradient
            Custom      // Use custom gradient
        }
        
        #endregion
        
        #region Serialized Fields
        
        [Header("Resource Bar Components")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image flashOverlay;
        [SerializeField] private TextMeshProUGUI valueText;
        
        [Header("Resource Settings")]
        [SerializeField] private ResourceType resourceType = ResourceType.Health;
        [SerializeField] private Gradient customGradient;
        
        [Header("Display Settings")]
        [Tooltip("How fast the bar lerps to new value (0 = instant)")]
        [SerializeField] private float lerpSpeed = 5f;
        
        [Tooltip("Text format: {0}=current, {1}=max, {2}=percent")]
        [SerializeField] private string textFormat = "{0:F0} / {1:F0}";
        
        [Tooltip("Show text overlay?")]
        [SerializeField] private bool showText = true;
        
        [Header("Effect Settings")]
        [Tooltip("Duration of flash effect")]
        [SerializeField] private float flashDuration = 0.2f;
        
        [Tooltip("Should depletion (hitting 0) trigger special effect?")]
        [SerializeField] private bool flashOnDepletion = true;
        
        [Tooltip("Color for increase flash")]
        [SerializeField] private Color increaseFlashColor = new Color(0f, 1f, 0f, 0.3f);
        
        [Tooltip("Color for decrease flash")]
        [SerializeField] private Color decreaseFlashColor = new Color(1f, 0f, 0f, 0.5f);
        
        [Tooltip("Color for depletion flash")]
        [SerializeField] private Color depletionFlashColor = new Color(1f, 0f, 0f, 0.8f);
        
        #endregion
        
        #region Built-in Gradients
        
        private static readonly Gradient HealthGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.8f, 0.1f, 0.1f), 0f),   // Dark red at 0%
                new GradientColorKey(new Color(1f, 0.3f, 0.1f), 0.3f),   // Orange at 30%
                new GradientColorKey(new Color(0.2f, 0.9f, 0.2f), 1f)     // Bright green at 100%
            }
        };
        
        private static readonly Gradient ManaGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.1f, 0.1f, 0.5f), 0f),   // Dark blue at 0%
                new GradientColorKey(new Color(0.3f, 0.5f, 1f), 0.5f),   // Medium blue at 50%
                new GradientColorKey(new Color(0.5f, 0.8f, 1f), 1f)      // Bright cyan at 100%
            }
        };
        
        private static readonly Gradient StaminaGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.8f, 0.5f, 0.1f), 0f),   // Orange at 0%
                new GradientColorKey(new Color(1f, 0.9f, 0.2f), 1f)      // Yellow at 100%
            }
        };
        
        private static readonly Gradient FoodGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.4f, 0.2f, 0.1f), 0f),   // Dark brown at 0%
                new GradientColorKey(new Color(0.8f, 0.5f, 0.2f), 1f)    // Light brown at 100%
            }
        };
        
        private static readonly Gradient WaterGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.1f, 0.4f, 0.5f), 0f),   // Dark cyan at 0%
                new GradientColorKey(new Color(0.3f, 0.8f, 1f), 1f)      // Bright cyan at 100%
            }
        };
        
        private static readonly Gradient ExperienceGradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.4f, 0.1f, 0.6f), 0f),   // Dark purple at 0%
                new GradientColorKey(new Color(0.8f, 0.3f, 1f), 1f)      // Bright purple at 100%
            }
        };
        
        #endregion
        
        #region State
        
        private float currentValue = 100f;
        private float maxValue = 100f;
        private float targetFillAmount = 1f;
        private float displayedFillAmount = 1f;
        
        private Coroutine lerpCoroutine;
        private Coroutine flashCoroutine;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            
            // Validate components
            if (fillImage == null)
            {
                Debug.LogError($"[UIResourceBar] Fill Image not assigned on {gameObject.name}!", this);
            }
            
            // Initialize fill image
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillAmount = 1f;
                fillImage.color = GetGradient().Evaluate(1f);
            }
            
            // Initialize flash overlay
            if (flashOverlay != null)
            {
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
            }
            
            // Initialize text
            if (valueText != null)
            {
                valueText.gameObject.SetActive(showText);
                UpdateText();
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Update resource bar to new value (with smooth animation)
        /// </summary>
        /// <param name="current">Current resource value</param>
        /// <param name="max">Maximum resource value</param>
        /// <param name="animate">Should animate the change?</param>
        public void UpdateResource(float current, float max, bool animate = true)
        {
            float previous = currentValue;
            bool wasAtZero = currentValue <= 0f;
            
            currentValue = Mathf.Clamp(current, 0f, max);
            maxValue = Mathf.Max(max, 1f); // Prevent divide by zero
            
            targetFillAmount = currentValue / maxValue;
            
            // Trigger appropriate effect
            if (current < previous)
            {
                // Resource decreased
                if (current <= 0f && flashOnDepletion)
                {
                    TriggerFlash(depletionFlashColor, flashDuration * 2f);
                }
                else
                {
                    TriggerFlash(decreaseFlashColor, flashDuration);
                }
            }
            else if (current > previous)
            {
                // Resource increased
                TriggerFlash(increaseFlashColor, flashDuration);
            }
            
            // Update text immediately
            UpdateText();
            
            // Start lerping to new value
            if (animate)
            {
                if (lerpCoroutine != null)
                    StopCoroutine(lerpCoroutine);
                lerpCoroutine = StartCoroutine(LerpToTargetResource());
            }
            else
            {
                // Update immediately
                displayedFillAmount = targetFillAmount;
                UpdateVisuals();
            }
        }
        
        /// <summary>
        /// Set resource immediately without animation
        /// </summary>
        public void SetResourceImmediate(float current, float max)
        {
            UpdateResource(current, max, animate: false);
        }
        
        /// <summary>
        /// Get current resource percentage (0-1)
        /// </summary>
        public float GetPercentage()
        {
            return currentValue / maxValue;
        }
        
        /// <summary>
        /// Get current value
        /// </summary>
        public float GetCurrentValue()
        {
            return currentValue;
        }
        
        /// <summary>
        /// Get max value
        /// </summary>
        public float GetMaxValue()
        {
            return maxValue;
        }
        
        #endregion
        
        #region Animation
        
        private IEnumerator LerpToTargetResource()
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
            fillImage.color = GetGradient().Evaluate(displayedFillAmount);
        }
        
        private void UpdateText()
        {
            if (valueText == null || !showText)
                return;
            
            float percentage = (currentValue / maxValue) * 100f;
            valueText.text = string.Format(textFormat, currentValue, maxValue, percentage);
        }
        
        #endregion
        
        #region Effects
        
        private void TriggerFlash(Color color, float duration)
        {
            if (flashOverlay == null)
                return;
            
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashEffect(color, duration));
        }
        
        private IEnumerator FlashEffect(Color color, float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(color.a, 0f, t);
                flashOverlay.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
            
            flashOverlay.color = new Color(color.r, color.g, color.b, 0f);
            flashCoroutine = null;
        }
        
        #endregion
        
        #region Gradient Management
        
        private Gradient GetGradient()
        {
            switch (resourceType)
            {
                case ResourceType.Health:
                    return HealthGradient;
                case ResourceType.Mana:
                    return ManaGradient;
                case ResourceType.Stamina:
                    return StaminaGradient;
                case ResourceType.Food:
                    return FoodGradient;
                case ResourceType.Water:
                    return WaterGradient;
                case ResourceType.Experience:
                    return ExperienceGradient;
                case ResourceType.Custom:
                    return customGradient ?? HealthGradient;
                default:
                    return HealthGradient;
            }
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
        }
        
        protected override void OnReturnToPool()
        {
            base.OnReturnToPool();
            
            // Reset state when returning to pool
            currentValue = 100f;
            maxValue = 100f;
            targetFillAmount = 1f;
            displayedFillAmount = 1f;
            
            if (fillImage != null)
            {
                fillImage.fillAmount = 1f;
                fillImage.color = GetGradient().Evaluate(1f);
            }
            
            if (flashOverlay != null)
            {
                flashOverlay.color = new Color(1f, 1f, 1f, 0f);
            }
            
            if (valueText != null)
            {
                UpdateText();
            }
        }
        
        #endregion
    }
}
