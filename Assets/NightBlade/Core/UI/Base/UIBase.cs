using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NightBlade.UI
{
    /// <summary>
    /// Base class for ALL NightBlade UI elements.
    /// Provides common functionality: visibility, caching, pooling support, and animations.
    /// 
    /// Design Philosophy:
    /// - Event-driven (no Update loops unless absolutely necessary)
    /// - Pooling-friendly (proper cleanup on Hide)
    /// - Performance-first (cached components, minimal allocations)
    /// - Easy for humans AND AI to understand and extend
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIBase : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("UI Base Settings")]
        [Tooltip("Should this UI be visible on startup?")]
        [SerializeField] protected bool startVisible = false;
        
        [Tooltip("Should this UI block raycasts when visible?")]
        [SerializeField] protected bool blockRaycastsWhenVisible = true;
        
        [Tooltip("Fade animation duration (0 = instant)")]
        [SerializeField] protected float fadeDuration = 0.2f;
        
        [Tooltip("Should this UI be pooled? (For frequently created/destroyed UI)")]
        [SerializeField] protected bool usePooling = false;
        
        #endregion
        
        #region Cached Components
        
        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;
        protected Canvas canvas;
        
        // Cache commonly used child components
        protected Image[] cachedImages;
        protected Text[] cachedTexts;
        protected Button[] cachedButtons;
        
        private bool componentsAreCached = false;
        
        #endregion
        
        #region State
        
        private bool isVisible = false;
        private bool isAnimating = false;
        private Coroutine fadeCoroutine;
        
        /// <summary>
        /// Is this UI currently visible?
        /// </summary>
        public bool IsVisible => isVisible;
        
        /// <summary>
        /// Is this UI currently animating?
        /// </summary>
        public bool IsAnimating => isAnimating;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            CacheComponents();
            
            if (!startVisible)
            {
                // Set invisible immediately without animation
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                isVisible = false;
            }
            else
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = blockRaycastsWhenVisible;
                isVisible = true;
            }
        }
        
        protected virtual void OnDestroy()
        {
            // Clean up any running coroutines
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
        }
        
        #endregion
        
        #region Component Caching
        
        /// <summary>
        /// Cache frequently accessed components.
        /// Call this in Awake or when components change.
        /// </summary>
        protected virtual void CacheComponents()
        {
            if (componentsAreCached)
                return;
                
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
                
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponent<Canvas>();
            
            // Cache common child components (optional - override if needed)
            cachedImages = GetComponentsInChildren<Image>(true);
            cachedTexts = GetComponentsInChildren<Text>(true);
            cachedButtons = GetComponentsInChildren<Button>(true);
            
            componentsAreCached = true;
        }
        
        /// <summary>
        /// Force re-cache of components (call if hierarchy changes)
        /// </summary>
        protected void RefreshComponentCache()
        {
            componentsAreCached = false;
            CacheComponents();
        }
        
        #endregion
        
        #region Visibility Control
        
        /// <summary>
        /// Show this UI element (with optional fade animation)
        /// </summary>
        public virtual void Show(bool animate = true)
        {
            if (isVisible && !isAnimating)
                return;
                
            gameObject.SetActive(true);
            
            if (animate && fadeDuration > 0f)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeIn());
            }
            else
            {
                // Show immediately
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = blockRaycastsWhenVisible;
                isVisible = true;
                OnShowComplete();
            }
        }
        
        /// <summary>
        /// Hide this UI element (with optional fade animation)
        /// </summary>
        public virtual void Hide(bool animate = true)
        {
            if (!isVisible && !isAnimating)
                return;
                
            if (animate && fadeDuration > 0f)
            {
                if (fadeCoroutine != null)
                    StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeOut());
            }
            else
            {
                // Hide immediately
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                isVisible = false;
                OnHideComplete();
                
                if (usePooling)
                {
                    // Return to pool instead of destroying
                    ReturnToPool();
                }
            }
        }
        
        /// <summary>
        /// Toggle visibility
        /// </summary>
        public virtual void Toggle(bool animate = true)
        {
            if (isVisible)
                Hide(animate);
            else
                Show(animate);
        }
        
        #endregion
        
        #region Animation
        
        private IEnumerator FadeIn()
        {
            isAnimating = true;
            
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            // Enable interactivity immediately for better UX
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = blockRaycastsWhenVisible;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            isVisible = true;
            isAnimating = false;
            fadeCoroutine = null;
            
            OnShowComplete();
        }
        
        private IEnumerator FadeOut()
        {
            isAnimating = true;
            
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            
            // Disable interactivity immediately
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            isVisible = false;
            isAnimating = false;
            fadeCoroutine = null;
            
            OnHideComplete();
            
            if (usePooling)
            {
                ReturnToPool();
            }
        }
        
        #endregion
        
        #region Lifecycle Hooks (Override in derived classes)
        
        /// <summary>
        /// Called when Show() animation completes (or immediately if no animation)
        /// Override to add custom behavior when UI becomes visible
        /// </summary>
        protected virtual void OnShowComplete()
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Called when Hide() animation completes (or immediately if no animation)
        /// Override to add custom behavior when UI becomes hidden
        /// </summary>
        protected virtual void OnHideComplete()
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Called when this UI is returned to the pool
        /// Override to clean up state before pooling
        /// </summary>
        protected virtual void OnReturnToPool()
        {
            // Override in derived classes to reset state
        }
        
        #endregion
        
        #region Pooling Support
        
        /// <summary>
        /// Return this UI to the pool (if pooling is enabled)
        /// </summary>
        protected virtual void ReturnToPool()
        {
            if (!usePooling)
            {
                gameObject.SetActive(false);
                return;
            }
            
            // Clean up state before returning to pool
            OnReturnToPool();
            
            // TODO: Integrate with UIPoolManager when implemented
            // For now, just deactivate
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Set UI visibility without animation (useful for initialization)
        /// </summary>
        public void SetVisibilityImmediate(bool visible)
        {
            if (visible)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = blockRaycastsWhenVisible;
                gameObject.SetActive(true);
            }
            else
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            }
            
            isVisible = visible;
        }
        
        /// <summary>
        /// Enable/disable raycast blocking (useful for see-through panels)
        /// </summary>
        public void SetRaycastBlocking(bool block)
        {
            blockRaycastsWhenVisible = block;
            if (isVisible)
                canvasGroup.blocksRaycasts = block;
        }
        
        #endregion
    }
}
