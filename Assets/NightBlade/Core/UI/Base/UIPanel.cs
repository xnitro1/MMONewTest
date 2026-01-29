using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NightBlade.UI
{
    /// <summary>
    /// Base class for window-style UI panels (Inventory, Character Sheet, etc.)
    /// Extends UIBase with dragging, closing, and window management features.
    /// 
    /// Features:
    /// - Draggable windows
    /// - Close button support
    /// - Window focus management
    /// - ESC key to close
    /// - Automatic cleanup
    /// </summary>
    public class UIPanel : UIBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region Serialized Fields
        
        [Header("Panel Settings")]
        [Tooltip("Can this panel be dragged by the user?")]
        [SerializeField] protected bool isDraggable = true;
        
        [Tooltip("Can this panel be closed with ESC key?")]
        [SerializeField] protected bool closeWithEscape = true;
        
        [Tooltip("Header area for dragging (optional - if null, entire panel is draggable)")]
        [SerializeField] protected RectTransform dragHeader;
        
        [Tooltip("Close button (optional - will auto-wire if found)")]
        [SerializeField] protected Button closeButton;
        
        [Tooltip("Should this panel block player controls when open?")]
        [SerializeField] protected bool blockPlayerControls = true;
        
        [Tooltip("Panel title (for window management)")]
        [SerializeField] protected string panelTitle = "Panel";
        
        #endregion
        
        #region Dragging State
        
        private Vector2 dragOffset;
        private bool isDragging = false;
        private Canvas parentCanvas;
        private RectTransform canvasRectTransform;
        
        #endregion
        
        #region Properties
        
        public string PanelTitle => panelTitle;
        public bool BlockPlayerControls => blockPlayerControls;
        public bool IsDragging => isDragging;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected override void Awake()
        {
            base.Awake();
            
            // Cache parent canvas
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                canvasRectTransform = parentCanvas.GetComponent<RectTransform>();
            
            // Auto-wire close button if we have one
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
            else
            {
                // Try to find close button by name
                Transform closeTransform = transform.Find("CloseButton");
                if (closeTransform == null)
                    closeTransform = transform.Find("btnClose");
                    
                if (closeTransform != null)
                {
                    closeButton = closeTransform.GetComponent<Button>();
                    if (closeButton != null)
                        closeButton.onClick.AddListener(OnCloseButtonClicked);
                }
            }
        }
        
        protected virtual void Update()
        {
            // Handle ESC key to close
            if (closeWithEscape && IsVisible && !IsAnimating)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    OnEscapePressed();
                }
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // Clean up button listener
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }
        
        #endregion
        
        #region Show/Hide Overrides
        
        public override void Show(bool animate = true)
        {
            base.Show(animate);
            
            // Register with UIManager for focus management
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RegisterPanel(this);
            }
            
            // Bring to front
            transform.SetAsLastSibling();
        }
        
        public override void Hide(bool animate = true)
        {
            base.Hide(animate);
            
            // Unregister from UIManager
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UnregisterPanel(this);
            }
        }
        
        #endregion
        
        #region Dragging Implementation
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDraggable || !IsVisible)
                return;
            
            // Check if we're dragging from the header (if specified)
            if (dragHeader != null)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(dragHeader, eventData.position, eventData.pressEventCamera))
                    return;
            }
            
            isDragging = true;
            
            // Calculate drag offset
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out dragOffset
            );
            
            // Bring to front when dragging starts
            transform.SetAsLastSibling();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;
            
            if (rectTransform == null || parentCanvas == null)
                return;
            
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint))
            {
                rectTransform.localPosition = localPoint - dragOffset;
                
                // Clamp to canvas bounds (optional - prevents dragging off-screen)
                ClampToCanvas();
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }
        
        /// <summary>
        /// Clamp panel position to stay within canvas bounds
        /// </summary>
        private void ClampToCanvas()
        {
            if (canvasRectTransform == null)
                return;
            
            Vector3 pos = rectTransform.localPosition;
            
            Vector3 minPosition = canvasRectTransform.rect.min - rectTransform.rect.min;
            Vector3 maxPosition = canvasRectTransform.rect.max - rectTransform.rect.max;
            
            pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);
            
            rectTransform.localPosition = pos;
        }
        
        #endregion
        
        #region Input Handling
        
        /// <summary>
        /// Called when ESC key is pressed while panel is open
        /// </summary>
        protected virtual void OnEscapePressed()
        {
            Hide();
        }
        
        /// <summary>
        /// Called when close button is clicked
        /// </summary>
        protected virtual void OnCloseButtonClicked()
        {
            Hide();
        }
        
        #endregion
        
        #region Window Management
        
        /// <summary>
        /// Bring this panel to the front
        /// </summary>
        public void BringToFront()
        {
            transform.SetAsLastSibling();
        }
        
        /// <summary>
        /// Center this panel on screen
        /// </summary>
        public void CenterOnScreen()
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
        
        /// <summary>
        /// Reset panel to default position
        /// </summary>
        public virtual void ResetPosition()
        {
            CenterOnScreen();
        }
        
        #endregion
    }
}
